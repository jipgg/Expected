using System.Collections.Immutable;
using System.Collections;
namespace Expected.Generators.Utility;

sealed record TypeParam(
   string Name,
   string Constraints
) {
   public string WhereClause(string typeParam) => string.IsNullOrWhiteSpace(Constraints) ? "" : $"where {typeParam}: {Constraints}";
   public static TypeParam Create(ITypeParameterSymbol symbol) {
      var parts = new List<string>();

      if (symbol.HasUnmanagedTypeConstraint) {
         parts.Add("unmanaged");
      } else if (symbol.HasValueTypeConstraint) {
         parts.Add("struct");
      } else if (symbol.HasReferenceTypeConstraint) {
         var nullable = symbol.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
            ? "class?"
            : "class";
         parts.Add(nullable);
      } else if (symbol.HasNotNullConstraint) {
         parts.Add("notnull");
      }

      var @base = symbol.ConstraintTypes
         .FirstOrDefault(t => t.TypeKind == TypeKind.Class);

      var displayFormat = SymbolDisplayFormat.FullyQualifiedFormat
         .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
      if (@base is not null) parts.Add(@base.ToDisplayString(displayFormat));

      foreach (var @interface in symbol.ConstraintTypes.Where(t => t.TypeKind == TypeKind.Interface)) {
         parts.Add(@interface.ToDisplayString(displayFormat));
      }

      if (symbol.HasConstructorConstraint) parts.Add("new()");
      if (symbol.AllowsRefLikeType) parts.Add("allows ref struct");

      return new(symbol.Name, string.Join(", ", parts));
   }
}

sealed record ClassInfo(
   string? Namespace,
   ValueEqualityArray<TypeParam> TypeParams,
   string Name,
   bool IsStruct,
   bool StructIsReadOnly,
   bool StructIsRef
) {
   public string GenericName => TypeParams.Length > 0
      ? $"{Name}<{string.Join(", ", TypeParams.Select(static e => e.Name))}>" : Name;

   public string TypeMod => IsStruct ? "struct" : "class";

   public bool IsClass => !IsStruct;

   public static ClassInfo Create(SyntaxNode node, INamedTypeSymbol symbol) {
      var isReadOnly = false;
      var isRef = false;
      var isStruct = false;
      if (node is StructDeclarationSyntax structSyntax) {
         isStruct = true;
         isReadOnly = structSyntax.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);
         isRef = structSyntax.Modifiers.Any(SyntaxKind.RefKeyword);
      }
      var @namespace = symbol.ContainingNamespace.IsGlobalNamespace
         ? null : symbol.ContainingNamespace.ToDisplayString();
      return new(
         @namespace,
         new([.. symbol.TypeParameters.Select(static e => TypeParam.Create(e))]),
         symbol.Name,
         isStruct,
         isReadOnly,
         isRef
      );
   }
}
