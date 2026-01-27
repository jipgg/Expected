namespace Expected.Generators;

static class Common {
   public static readonly SymbolDisplayFormat DisplayFormat
      = SymbolDisplayFormat.FullyQualifiedFormat
         .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
}
