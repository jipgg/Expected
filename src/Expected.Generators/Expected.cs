using System.Runtime.InteropServices;
namespace Expected.Generators;

public enum StorageStrategy {
   Sequential,
   Union,
   Object,
   SameField,
}

sealed record ExpectedParams(
   string HintName,
   string? Namespace,
   string Name,
   string TypeParams,
   ResolvedTypeArguments TypeArgs,
   ResolvedType Type,
   bool Sealed,
   StorageStrategy StorageStrategy
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
            return Local.MakeExpectedParams(node, symbol);
         }
      ).Where(static t => t is not null);
      context.RegisterSourceOutput(provider, static (context, args) => {
         context.AddSource(args!.HintName, ExpectedTemplate.Apply(args!));
      });
   }
   const string MetadataName = "Expected.ExpectedAttribute";
}

sealed record ResolvedTypeArgumentSymbols(ITypeSymbol V, ITypeSymbol E) {
   public static implicit operator ResolvedTypeArguments(ResolvedTypeArgumentSymbols r)
      => new(Common.Format(r.V), Common.Format(r.E));
}
sealed record ResolvedTypeArguments(string V, string E);
// readonly record struct ExpectedTypeArguments(ITypeSymbol TValue, ITypeSymbol TError);
file static class Local {
   public static StorageStrategy ResolveStorageStrategy(ResolvedTypeArgumentSymbols types) {
      if (types.V.ToDisplayString().Equals(types.E.ToDisplayString())) return StorageStrategy.SameField;
      var v = (types.V as INamedTypeSymbol);
      var e = (types.E as INamedTypeSymbol);
      var vParam = types.V as ITypeParameterSymbol;
      var eParam = types.V as ITypeParameterSymbol;
      if (v?.TypeKind is TypeKind.Class && e?.TypeKind is TypeKind.Class) {
         return StorageStrategy.Object;
      } else if (v?.TypeKind is TypeKind.Class && eParam?.HasReferenceTypeConstraint is true) {
         return StorageStrategy.Object;
      } else if (e?.TypeKind is TypeKind.Class && vParam?.HasReferenceTypeConstraint is true) {
         return StorageStrategy.Object;
      } else if (vParam?.HasReferenceTypeConstraint is true && eParam?.HasReferenceTypeConstraint is true) {
         return StorageStrategy.Object;
      } else if (v?.IsUnmanagedType is true && e?.IsUnmanagedType is true) {
         return StorageStrategy.Union;
      }
      return StorageStrategy.Sequential;
   }
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
         Sealed: node.Modifiers.Any(SyntaxKind.SealedKeyword),
         StorageStrategy: ResolveStorageStrategy(typeArgs)
      );
   }
   public static ResolvedTypeArgumentSymbols? ResolveTypeArguments(INamedTypeSymbol symbol) {
      var marker = symbol.Interfaces
         .SingleOrDefault(e => e.MetadataName == "IExpected`3");
      if (marker is not null) {
         return new(marker.TypeArguments[1], marker.TypeArguments[2]);
      }
      static AttributeData? findAttr(INamedTypeSymbol symbol, string metadataName) {
         return symbol.GetAttributes()
            .Where(e => e.AttributeClass?.MetadataName == metadataName
               && e.AttributeClass?.ContainingNamespace.ToDisplayString() is "Expected")
            .SingleOrDefault();
      }
      var expected = findAttr(symbol, "ExpectedAttribute`2")?.AttributeClass?.TypeArguments;
      if (expected is { } expectedArgs) {
         return new(expectedArgs[0], expectedArgs[1]);
      }
      var expectsT = findAttr(symbol, "ExpectsAttribute`1")?.AttributeClass?.TypeArguments[0];
      var unexpectsT = findAttr(symbol, "UnexpectsAttribute`1")?.AttributeClass?.TypeArguments[0];
      if (symbol.Arity is 0) {
         if (expectsT is null || unexpectsT is null) return null;
         return new(expectsT, unexpectsT);
      } else if (symbol.Arity is 1) {
         if (expectsT is not null) return new(expectsT, symbol.TypeArguments[0]);
         else if (unexpectsT is not null) return new(symbol.TypeArguments[0], unexpectsT);
         else return null;
      }
      return null;
   }
}

