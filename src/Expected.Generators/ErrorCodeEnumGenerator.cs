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
      const string sourceNamespace = "global::Expected";

      string getMessageImpl() {
         return dontGenerateGetMessage ? "" : $$"""
         public override string GetMessage(int errorCode) {
         #pragma warning disable CS8524
               return ({{@enum}})errorCode switch {
                  {{string.Join("\n         ", enumItems.Select(name => $"{@enum}.{name} => \"{name}\","))}}
               };
         #pragma warning restore CS8524
               throw new {{sourceNamespace}}.Unreachable();
            }
         """;
      }

      var source = $$"""
      namespace {{@namespace}};
      public sealed partial class {{category}}: {{sourceNamespace}}.ErrorCategory {
         public override string Name => "{{categoryName}}";
         {{getMessageImpl()}}
         internal {{category}}() { }
      }
      public static class {{@enum}}_ErrorCodeEnumExtensions {
         static readonly {{category}} _category = new();
         public static {{sourceNamespace}}.ErrorCode AsErrorCode(this {{@enum}} ec) => new((int)ec, _category);
      #if NET10_0_OR_GREATER
         extension({{@enum}} error) {
            public static bool operator ==(in ErrorCode a, {{@enum}} v) => a.Equals(v.AsErrorCode());
            public static bool operator !=(in ErrorCode a, {{@enum}} v) => !a.Equals(v.AsErrorCode());
         }
      #endif //NET10_0_OR_GREATER
      }
      """;
      context.AddSource($"{@enum}_ErrorCodeEnum.g.cs", SourceText.From(source, Encoding.UTF8));
   }
}
