using System.Collections.Immutable;
namespace Expected.Generators;

static class Common {
   public static readonly SymbolDisplayFormat DisplayFormat
      = SymbolDisplayFormat.FullyQualifiedFormat
         .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

   public static string Format(ISymbol symbol)
      => symbol.ToDisplayString(DisplayFormat);

   public static string ToFilenameString(string displayString)
      => displayString
         .Replace("global::", "")
         .Replace('<', '{')
         .Replace('>', '}');

   public static string ToHintName(INamedTypeSymbol symbol, ExpectedTypeArguments typeArguments) {
      var ns = symbol.ContainingNamespace;
      var nsPrefix = ns.IsGlobalNamespace ? "" : $"{ns.ToDisplayString()}.";
      var typeArgs = ToFilenameString($"{{{typeArguments.TValue.ToDisplayString()},{typeArguments.TError.ToDisplayString()}}}");
      return $"{nsPrefix}{symbol.Name}{typeArgs}.g";
   }
}
