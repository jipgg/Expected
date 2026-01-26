namespace Expected.Generators.Utility;

record TypeParam(
   string Name,
   string Constraints
) {
   public string WhereClause(string typeParam) => string.IsNullOrWhiteSpace(Constraints) ? "" : $"where {typeParam}: {Constraints}";
   public static TypeParam From(ITypeParameterSymbol symbol) {
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

      var baseType = symbol.ConstraintTypes
         .FirstOrDefault(t => t.TypeKind == TypeKind.Class);

      if (baseType is not null) {
         parts.Add(baseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
      }

      foreach (var @interface in symbol.ConstraintTypes.Where(t => t.TypeKind == TypeKind.Interface)) {
         parts.Add(@interface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
      }

      if (symbol.HasConstructorConstraint) {
         parts.Add("new()");
      }
      if (symbol.AllowsRefLikeType) {
         parts.Add("allows ref struct");
      }

      return new TypeParam(
         symbol.Name,
         string.Join(", ", parts)
      );
   }
}

record ClassInfo(
   string? Namespace,
   TypeParam[] TypeParams,
   string Name,
   bool IsStruct,
   bool StructIsReadOnly,
   bool StructIsRef
) {
   public string HintName => TypeParams.Length > 0
      ? $"{Name}{{{string.Join(",", TypeParams.Select(static e => e.Name))}}}" : Name;

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
      var @namespace = symbol.ContainingNamespace.ToDisplayString();
      if (@namespace == "<global namespace>") @namespace = null;
      return new(
         symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString(),
         [.. symbol.TypeParameters.Select(static e => TypeParam.From(e))],
         symbol.Name,
         isStruct,
         isReadOnly,
         isRef
      );
   }
}
