namespace Expected.Meta;

using static ErrorCodeSourceTemplate;

[Generator]
public sealed class ErrorCodeGenerator : IIncrementalGenerator {
   public void Initialize(IncrementalGeneratorInitializationContext context) {
      const string metadataName = "Expected.ErrorCodeAttribute";
      var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
         metadataName,
         static (node, _) => node is EnumDeclarationSyntax {AttributeLists.Count: > 0},
         static (context, _) => {
            if (context.TargetSymbol is not INamedTypeSymbol symbol) return null;
            var attrClass = context.SemanticModel.Compilation.GetTypeByMetadataName(metadataName);
            var attr = AttributeParser.From(symbol, attrClass);

            var name = symbol.Name;
            var codes = attr.Parse<bool>("GenerateCodesClass") is true
               ? (attr.Parse<string>("CodesClassName") ?? $"{name}Codes")
               : null;
            var ns = symbol.ContainingNamespace;
            return new Arguments(
               Namespace: ns.IsGlobalNamespace ? null : ns.ToDisplayString(),
               Enum: new(
                  Name: name,
                  Fields: new([..symbol.GetMembers()
                     .OfType<IFieldSymbol>()
                     .Where(e => e.IsConst)
                     .Select(static e => e.Name)
                  ])),
               Category: attr.Parse<string>("CategoryClassName") ?? $"{name}Category",
               Codes: codes,
               Title: attr.Parse<string>("Title") ?? name,
               MessageImpl: attr.Parse<byte>("MessageImpl") is byte b ? (MessageImplOptions)b : MessageImplOptions.Name
            );
         }
      ).Where(static e => e is not null);
      context.RegisterSourceOutput(provider, static (context, args) => {
         context.AddSource($"{args!.Enum.Name}.g.cs", ApplySourceTemplate(args));
      });
   }
}
