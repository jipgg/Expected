using System.Runtime.InteropServices;
namespace Expected.Generators;

sealed record ExpectedParams(
   string HintName,
   string? Namespace,
   string Name,
   string TypeParams,
   ResolvedTypeArguments TypeArgs,
   ResolvedType Type,
   bool Sealed
) {
   public string GenericName => $"{Name}{TypeParams}";
}
[Generator]
public sealed class Expected : IIncrementalGenerator {
   public void Initialize(IncrementalGeneratorInitializationContext context) {
      var provider = context.SyntaxProvider.CreateSyntaxProvider(
         static (node, _) => {
            if (node is not TypeDeclarationSyntax type) return false;
            if (type is InterfaceDeclarationSyntax) return false;
            foreach (var modifier in type.Modifiers) {
               if (modifier.IsKind(SyntaxKind.PartialKeyword)) return true;
            }
            return false;
         },
         static (context, _) => {
            var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (declaredSymbol is not INamedTypeSymbol symbol) return null;
            if (context.Node is not TypeDeclarationSyntax node) return null;
            if (Local.ResolveTypeArguments(symbol) is not { } typeArgs) return null;
            var ns = declaredSymbol.ContainingNamespace;
            return Local.MakeExpectedParams(node, symbol);
         }
      ).Where(static t => t is not null);
      context.RegisterSourceOutput(provider, static (context, args) => {
         context.AddSource(args!.HintName, ExpectedTemplate.Apply(args!));
      });
   }
   const string MetadataName = "Expected.ExpectedAttribute";
}

sealed record ResolvedTypeArguments(string V, string E);
// readonly record struct ExpectedTypeArguments(ITypeSymbol TValue, ITypeSymbol TError);
file static class Local {
   public static ExpectedParams? MakeExpectedParams(TypeDeclarationSyntax node, INamedTypeSymbol symbol) {
      ResolvedType type;
      switch (node) {
         case StructDeclarationSyntax:
            var isReadonly = node.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);
            var isRef = node.Modifiers.Any(SyntaxKind.RefKeyword);
            if (isReadonly && isRef) type = ResolvedType.ReadOnlyRefStruct;
            else if (isRef) type = ResolvedType.RefStruct;
            else if (isReadonly) type = ResolvedType.ReadOnlyStruct;
            else type = ResolvedType.Struct;
            break;
         case ClassDeclarationSyntax:
            type = ResolvedType.Class;
            break;
         case RecordDeclarationSyntax r:
            type = r.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
               ? ResolvedType.RecordStruct
               : ResolvedType.RecordClass;
            break;
         default:
            type = default;
            break;
      }
      if (ResolveTypeArguments(symbol) is not { } typeArgs) return null;
      var ns = symbol.ContainingNamespace;
      var typeParams = symbol.TypeParameters.Length is 0
         ? ""
         : $"<{string.Join(",", symbol.TypeParameters.Select(e => e.Name))}>";
      return new(
         HintName: Common.ToHintName(symbol, typeArgs),
         Namespace: ns.IsGlobalNamespace ? null : ns.ToDisplayString(),
         Name: symbol.Name,
         TypeParams: typeParams,
         Type: type,
         TypeArgs: typeArgs,
         Sealed: node.Modifiers.Any(SyntaxKind.SealedKeyword)
      );
   }
   public static ResolvedTypeArguments? ResolveTypeArguments(INamedTypeSymbol symbol) {
      static ResolvedTypeArguments format(ITypeSymbol v, ITypeSymbol e) {
         return new(Common.Format(v), Common.Format(e));
      }
      var marker = symbol.Interfaces
         .SingleOrDefault(e => e.MetadataName == "IExpected`3");
      if (marker is not null) {
         return format(marker.TypeArguments[1], marker.TypeArguments[2]);
      }
      static AttributeData? findAttr(INamedTypeSymbol symbol, string metadataName) {
         return symbol.GetAttributes()
            .Where(e => e.AttributeClass?.MetadataName == metadataName
               && e.AttributeClass?.ContainingNamespace.ToDisplayString() is "Expected")
            .SingleOrDefault();
      }
      var expected = findAttr(symbol, "ExpectedAttribute`2")?.AttributeClass?.TypeArguments;
      if (expected is {} expectedArgs) {
         return format(expectedArgs[0], expectedArgs[1]);
      }
      var expectsT = findAttr(symbol, "ExpectsAttribute`1")?.AttributeClass?.TypeArguments[0];
      var unexpectsT = findAttr(symbol, "UnexpectsAttribute`1")?.AttributeClass?.TypeArguments[0];
      if (symbol.Arity is 0) {
         if (expectsT is null || unexpectsT is null) return null;
         return format(expectsT, unexpectsT);
      } else if (symbol.Arity is 1) {
         if (expectsT is not null) return format(expectsT, symbol.TypeArguments[0]);
         else if (unexpectsT is not null) return format(symbol.TypeArguments[0], unexpectsT);
         else return null;
      } else if (symbol.Arity is 2) {
         string? typeParam(string attr) => findAttr(symbol, attr)?.ConstructorArguments[0].Value as string;
         var expects = typeParam("ExpectsAttribute");
         var unexpects = typeParam("UnexpectsAttribute");
         if (expects is null || unexpects is null) return null;
         return new(expects, unexpects);
      }
      return null;
   }
}

