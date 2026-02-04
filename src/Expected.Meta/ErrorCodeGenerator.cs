namespace Expected.Meta;

using static ErrorCodeSourceTemplate;

[Generator]
public sealed class ErrorCodeGenerator : IIncrementalGenerator {
   public void Initialize(IncrementalGeneratorInitializationContext context) {
      const string metadataName = "Expected.ErrorCodeAttribute";
      var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
         metadataName,
         static (node, _) => node is EnumDeclarationSyntax and { AttributeLists.Count: > 0 },
         static (context, _) => {
            if (context.TargetSymbol is not INamedTypeSymbol symbol) return null;
            var attr = symbol.GetAttributes().FirstOrDefault(e => e.AttributeClass?.ToDisplayString() == metadataName);
            // context.Attributes.Where(e => e.AttributeClass?.Name == "ErrorCodeAttribute");

            var node = (EnumDeclarationSyntax)context.TargetNode;
            var visibility = "internal";
            if (node.Modifiers.Any(SyntaxKind.PublicKeyword)) visibility = "public";
            else if (node.Modifiers.Any(SyntaxKind.PrivateKeyword)) visibility = "private";
            var name = symbol.Name;
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
               Visibility: visibility,
               Title: attr?.Get<string>("Title") ?? name,
               MessageImpl: attr?.Get<byte>(0) is byte b ? (MessageImplOptions)b : MessageImplOptions.Name
            );
         }
      ).Where(static e => e is not null);
      context.RegisterSourceOutput(provider, static (context, args) => {
         context.AddSource($"{args!.Enum.Name}.g.cs", ApplySourceTemplate(args));
      });
   }
}
