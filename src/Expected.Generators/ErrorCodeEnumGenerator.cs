using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
namespace Expected.Generators;

[Generator]
public sealed class ErrorCodeEnumGenerator : IIncrementalGenerator {
   static bool Predicate(SyntaxNode node, CancellationToken token) {
      if (node is not EnumDeclarationSyntax eds) return false;
      return eds.AttributeLists.Count > 0;
   }
   static INamedTypeSymbol? Transform(GeneratorSyntaxContext context, CancellationToken token) {
      var enumDecl = (EnumDeclarationSyntax)context.Node;
      var symbol = context.SemanticModel.GetDeclaredSymbol(enumDecl) as INamedTypeSymbol;
      if (symbol is null) return null;

      // first try semantic check
      var hasAttribute = symbol.GetAttributes()
          .Any(attr =>
              attr.AttributeClass?.Name == "ErrorCodeEnumAttribute" ||
              attr.AttributeClass?.ToDisplayString() == "Expected.ErrorCodeEnumAttribute");

      // fallback syntax-level check if attribute not fully bound
      if (!hasAttribute) {
         hasAttribute = enumDecl.AttributeLists
             .SelectMany(al => al.Attributes)
             .Any(a => a.Name.ToString().Contains("ErrorCodeEnum"));
      }

      return hasAttribute ? symbol : null;
   }
   public void Initialize(IncrementalGeneratorInitializationContext context) {
      var errorEnums = context.SyntaxProvider
         .CreateSyntaxProvider(Predicate, Transform)
         .Where(e => e is not null);
      context.RegisterSourceOutput(errorEnums, Generate);
   }

   const string Namespace = "global::Expected";
   const string ErrorCode = $"{Namespace}.ErrorCode";
   const string Unreachable = $"{Namespace}.Unreachable";
   const string ErrorCategory = $"{Namespace}.ErrorCategory";
   static void Generate(SourceProductionContext context, INamedTypeSymbol? symbol) {
      if (symbol is null) return;
      var @namespace = symbol.ContainingNamespace.IsGlobalNamespace
         ? "GlobalNamespace"
         : symbol.ContainingNamespace.ToDisplayString();
      var @enum = symbol.Name;

      var attribute = symbol.GetAttributes()
         .FirstOrDefault(e => e.AttributeClass?.Name == "ErrorCodeEnumAttribute");
      var categoryName = attribute?.NamedArguments
         .FirstOrDefault(e => e.Key == "Name")
         .Value.Value as string ?? $"{@enum} error";

      object? dontGenerateGetMessageObj = attribute?.NamedArguments
          .FirstOrDefault(e => e.Key == "DontGenerateGetMessage")
          .Value.Value;
      bool dontGenerateGetMessage = false;
      if (dontGenerateGetMessageObj is not null) dontGenerateGetMessage = (bool)dontGenerateGetMessageObj;

      var category = $"{@enum}Category";
      var enumItems = symbol.GetMembers()
         .OfType<IFieldSymbol>()
         .Where(static e => e.ConstantValue != null)
         .Select(static e => e.Name)
         .ToArray();

      string getMessageImpl() {
         return dontGenerateGetMessage ? "" : $$"""
         public override string GetMessage(int errorCode) {
         #pragma warning disable CS8524
               return ({{@enum}})errorCode switch {
                  {{string.Join("\n         ", enumItems.Select(name => $"{@enum}.{name} => \"{name}\","))}}
               };
         #pragma warning restore CS8524
               throw new {{Unreachable}}();
            }
         """;
      }
      string createErrorCodeEntries() {
         var b = new StringBuilder();
         foreach (var item in enumItems) {
            b.AppendLine($"      public static {ErrorCode} {item} => new((int){@enum}.{item}, {category}.Value);");
         }
         if (b.Length > 0) --b.Length;
         return b.ToString();
      }

      var source = $$"""
      namespace {{@namespace}};
      public sealed partial class {{category}}: {{ErrorCategory}} {
         public override string Name => "{{categoryName}}";
         {{getMessageImpl()}}
         static readonly {{category}} _value = new();
         public static {{category}} Value => _value;
      }
      #if NET10_0_OR_GREATER
      public static class ErrorCodeEnumExtensionsFor{{@enum}} {
         extension ({{ErrorCategory}}) {
            public static {{category}} {{@enum}} => {{category}}.Value;
         }
         extension ({{ErrorCode}} errorCode) {
      {{createErrorCodeEntries()}}
         }
         extension({{@enum}} ec) {
            public {{ErrorCode}} ErrorCode => new((int)ec, {{category}}.Value);
            public static {{category}} ErrorCategory => {{category}}.Value;
            public static bool operator ==({{ErrorCode}} a, {{@enum}} v) => a.Equals(v.ErrorCode);
            public static bool operator !=({{ErrorCode}} a, {{@enum}} v) => !a.Equals(v.ErrorCode);
         }
      }
      #endif //NET10_0_OR_GREATER
      """;
      context.AddSource($"ErrorCodeEnumExtensionsFor{@enum}.g.cs", SourceText.From(source, Encoding.UTF8));
   }
}
