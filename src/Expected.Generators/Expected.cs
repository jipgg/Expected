namespace Expected.Generators;

record ExpectedParams(
   ClassInfo ClassInfo,
   string TValue,
   string TError,
   bool IsCanonical
);
[Generator]
public sealed class Expected : IIncrementalGenerator {
   static string MakeHintName(INamedTypeSymbol symbol, string tValue, string tError) {
      var ns = symbol.ContainingNamespace;
      var str = $"{(ns.IsGlobalNamespace ? "" : $"{ns.ToDisplayString()}.")}";
      str += symbol.Name;
      str += $"{{{tValue.Replace("global::", "")},";
      str += $"{tError.Replace("global::", "")}}}";

      return str;
   }
   public void Initialize(IncrementalGeneratorInitializationContext context) {

      context.RegisterSourceOutput(
         context.SyntaxProvider.ForAttributeWithMetadataName(MetadataName,
            static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax and { AttributeLists.Count: > 0 },
            static (string, ExpectedParams?) (context, _) => {
               if (context.TargetSymbol is not INamedTypeSymbol symbol) return ("", null);
               var attr = AttributeParser.From(symbol, GetAttributeClass(context));
               var tValue = attr.Parse<string>("TValue");
               var tError = attr.Parse<string>("TError");
               if (GetTypeArgumentsInterface(symbol) is { } typeArgs) {
                  var fmt = SymbolDisplayFormat.FullyQualifiedFormat
                     .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
                  tValue = typeArgs.TypeArguments[0].ToDisplayString(fmt);
                  tError = typeArgs.TypeArguments[1].ToDisplayString(fmt);
               }
               var resolvedTValue = tValue
                  ?? (symbol.Arity >= 1
                     ? symbol.TypeParameters[0].Name
                     : null);
               if (resolvedTValue is null) return ("", null);
               var resolvedTError = tError
                  ?? (symbol.Arity >= 2
                     ? symbol.TypeParameters[1].Name
                     : (symbol.Arity >= 1 && tValue != symbol.TypeParameters[0].Name
                        ? symbol.TypeParameters[0].Name
                        : null));
               if (resolvedTError is null) return ("", null);
               return (
                  MakeHintName(symbol, resolvedTValue, resolvedTError),
                  new ExpectedParams(
                     ClassInfo: ClassInfo.Create(context.TargetNode, symbol),
                     TValue: resolvedTValue ?? "TValue",
                     TError: resolvedTError ?? "TError",
                     IsCanonical: IsCanonicalType(symbol)
               ));
            }).Where(static e => e.Item2 is not null),
         static (context, e) => {
            var (hintName, args) = e;
            context.AddSource(
               $"{hintName}.g.cs",
               ExpectedTemplate.Apply(args!)
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
