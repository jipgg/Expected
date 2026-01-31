using System.Runtime.InteropServices;
namespace Expected.Generators;


sealed record ExpectedParams(
   string HintName,
   ClassInfo ClassInfo,
   string TValue,
   string TError
);
[Generator]
public sealed class Expected : IIncrementalGenerator {
   public void Initialize(IncrementalGeneratorInitializationContext context) {
      var provider = context.SyntaxProvider.CreateSyntaxProvider(
         static (node, _) => {
            if (node is not TypeDeclarationSyntax type) return false;
            if (type is RecordDeclarationSyntax or InterfaceDeclarationSyntax) return false;
            foreach (var modifier in type.Modifiers) {
               if (modifier.IsKind(SyntaxKind.PartialKeyword)) return true;
            }
            return false;
         },
         static (context, _) => {
            var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (declaredSymbol is not INamedTypeSymbol symbol) return null;
            if (Local.ResolveTypeArguments(symbol) is not { } typeArgs) return null;
            var ns = declaredSymbol.ContainingNamespace;
            return new ExpectedParams(
               HintName: Common.ToHintName(symbol, typeArgs),
               ClassInfo: ClassInfo.Create(context.Node, symbol),
               TValue: Common.Format(typeArgs.TValue),
               TError: Common.Format(typeArgs.TError)
            );
         }
      ).Where(static t => t is not null);
      context.RegisterSourceOutput(provider, static (context, args) => {
         context.AddSource(args!.HintName, ExpectedTemplate.Apply(args!));
      });
   }
   const string MetadataName = "Expected.ExpectedAttribute";
}

readonly record struct ExpectedTypeArguments(ITypeSymbol TValue, ITypeSymbol TError);
file static class Local {
   public static AttributeData? GetAttributeData(INamedTypeSymbol symbol, string name, string @namespace = "Expected")
      => symbol.GetAttributes()
      .Where(e => e.AttributeClass?.Name == name
         && e.AttributeClass?.ContainingNamespace.ToDisplayString() is "Expected")
      .SingleOrDefault();


   public static ExpectedTypeArguments? ResolveTypeArguments(INamedTypeSymbol symbol) {
      const string expectedAttrName = "ExpectedAttribute";
      var marker = symbol.Interfaces
         .SingleOrDefault(e => e.MetadataName == "IExpected`3");
      if (marker is not null) {
         return new(marker.TypeArguments[1], marker.TypeArguments[2]);
      }
      if (symbol.Arity is 0) {
         var attribute = GetAttributeData(symbol, expectedAttrName);
         if (attribute?.AttributeClass is null or { Arity: not 2 }) return null;
         var typeArgs = attribute.AttributeClass.TypeArguments;
         return new(typeArgs[0], typeArgs[1]);
      } else if (symbol.Arity is 1) {
         var value = GetAttributeData(symbol, "ExpectsAttribute");
         if (value is { AttributeClass.Arity: not 1 }) return null;
         if (value?.AttributeClass is not null) {
            return new(value.AttributeClass.TypeArguments[0], symbol.TypeArguments[0]);
         }
         var error = GetAttributeData(symbol, "UnexpectsAttribute");
         if (error is { AttributeClass.Arity: not 1 }) return null;
         if (error?.AttributeClass is null) return null;
         return new(symbol.TypeArguments[0], error.AttributeClass.TypeArguments[0]);
      } else if (symbol.Arity is 2) {
         var attribute = GetAttributeData(symbol, expectedAttrName);
         if (attribute?.AttributeClass is null or { Arity: not 0 }) return null;
         return new(symbol.TypeArguments[0], symbol.TypeArguments[1]);
      }
      return null;
   }
}

