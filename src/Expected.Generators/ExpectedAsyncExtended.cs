namespace Expected.Generators;

record ExpectedAsyncExtensionsParams(string Namespace, string Type);

[Generator]
public sealed class ExpectedAsyncExtended : IIncrementalGenerator {
   const string MetadataName = "Expected.Internal.ExpectedAsyncExtendedAttribute";
   public void Initialize(IncrementalGeneratorInitializationContext context) {
      context.RegisterSourceOutput(
         context.SyntaxProvider.ForAttributeWithMetadataName(MetadataName,
            static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax and { AttributeLists.Count: > 0 },
            static (context, _) => {
               var symbol = context.TargetSymbol;
               var attrClass = context.SemanticModel.Compilation.GetTypeByMetadataName(MetadataName);
               var attr = AttributeParser.From(symbol, attrClass);
               return new ExpectedAsyncExtensionsParams(
                  attr.Parse<string>("Namespace") ?? symbol.ContainingNamespace.ToDisplayString(),
                  attr.Parse<string>("Type") ?? symbol.Name
               );
            }),
         static (context, args) => context.AddSource(
            $"{args.Type}.g.cs",
            ExpectedAsyncExtendedTemplate.Apply(args)
         ));

   }
}
