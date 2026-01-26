namespace Expected.Generators.Utility;

readonly struct AttributeParser(AttributeData? data) {
   public object? Parse(string argument) => data?.NamedArguments
      .Where(e => e.Key == argument)
      .Select(static e => e.Value.Value)
      .SingleOrDefault();
   public T? Parse<T>(string argument) where T : class => Parse(argument) as T ?? null;
   public static AttributeParser From(ISymbol symbol, string attributeClassName)
      => new(symbol.GetAttributes().FirstOrDefault(e => e.AttributeClass?.Name == attributeClassName));
   public static AttributeParser From(ISymbol symbol, INamedTypeSymbol? attributeClass)
      => new(symbol.GetAttributes().FirstOrDefault(e => SymbolEqualityComparer.Default.Equals(e.AttributeClass, attributeClass)));
}
static class AttributeArgumentParserWhereStruct {
   public static T? Parse<T>(this ref AttributeParser p, string argument) where T : struct => p.Parse(argument) as T? ?? null;
}
