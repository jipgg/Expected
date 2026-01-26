namespace Expected.Generators;

record ExpectedParams(
   ClassInfo ClassInfo,
   string TValue,
   string TError,
   bool IsCanonical
);
[Generator]
public sealed class Expected : IIncrementalGenerator {
   public void Initialize(IncrementalGeneratorInitializationContext context) {
      context.RegisterSourceOutput(
         context.SyntaxProvider.ForAttributeWithMetadataName(MetadataName,
            static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax and { AttributeLists.Count: > 0 },
            static (context, _) => {
               if (context.TargetSymbol is not INamedTypeSymbol symbol) return null;
               var attr = AttributeParser.From(symbol, GetAttributeClass(context));
               var tValue = attr.Parse<string>("TValue");
               var tError = attr.Parse<string>("TError");
               if (GetTypeArgumentsInterface(symbol) is { } typeArgs) {
                  tValue = typeArgs.TypeArguments[0].ToDisplayString();
                  tError = typeArgs.TypeArguments[1].ToDisplayString();
               }
               var resolvedTValue = tValue ?? (symbol.Arity >= 1 ? symbol.TypeParameters[0].Name : null);
               if (resolvedTValue is null) return null;
               var resolvedTError = tError
                  ?? (symbol.Arity >= 2
                     ? symbol.TypeParameters[1].Name
                     : (symbol.Arity >= 1 && tValue != symbol.TypeParameters[0].Name
                        ? symbol.TypeParameters[0].Name
                        : null));
               if (resolvedTError is null) return null;
               return new ExpectedParams(
                  ClassInfo: ClassInfo.Create(context.TargetNode, symbol),
                  TValue: resolvedTValue,
                  TError: resolvedTError,
                  IsCanonical: IsCanonicalType(symbol)
               );
            }).Where(static e => e is not null),
         static (context, args) => {
            context.AddSource(
               $"{args!.ClassInfo.HintName}.g.cs",
               ExpectedTemplate.Apply(args)
            );
         });

   }
   static INamedTypeSymbol? GetTypeArgumentsInterface(INamedTypeSymbol symbol) {
      return symbol.Interfaces.SingleOrDefault(static e => e.Name == "IExpectedTypeArguments"
         && e.ContainingNamespace.ToDisplayString() == "Expected");
   }
   static bool IsCanonicalType(INamedTypeSymbol symbol) {
      return symbol.GetAttributes().Any(e => e.AttributeClass?.ToDisplayString() == "Expected.Internal.IsCanonicalAttribute");
   }
   const string MetadataName = "Expected.ExpectedAttribute";
   static INamedTypeSymbol? GetAttributeClass(GeneratorAttributeSyntaxContext context)
      => context.SemanticModel.Compilation.GetTypeByMetadataName(MetadataName);
}
