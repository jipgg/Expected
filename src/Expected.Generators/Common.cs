using System.Collections.Immutable;
namespace Expected.Generators;
using static ResolvedType;

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

   public static string ToHintName(INamedTypeSymbol symbol, ResolvedTypeArguments typeArguments) {
      var ns = symbol.ContainingNamespace;
      var nsPrefix = ns.IsGlobalNamespace ? "" : $"{ns.ToDisplayString()}.";
      var typeArgs = ToFilenameString($"{{{typeArguments.V},{typeArguments.E}}}");
      return $"{nsPrefix}{symbol.Name}{typeArgs}.g";
   }
}
enum ResolvedType {
   Bad,
   RefStruct,
   ReadOnlyRefStruct,
   ReadOnlyStruct,
   Struct,
   RecordStruct,
   Class,
   RecordClass,
}

static class Extensions {
   public static bool IsReadOnly(this ResolvedType t) {
      return t
         is ReadOnlyRefStruct
         or ReadOnlyStruct
         or RecordStruct
         or RecordClass;
   }
   public static bool IsStruct(this ResolvedType t) {
      return t
         is ReadOnlyRefStruct
         or RefStruct
         or ReadOnlyStruct
         or Struct
         or RecordStruct;
   }
   public static bool IsRecord(this ResolvedType t) => t is RecordClass or RecordStruct;
   public static bool IsRefStruct(this ResolvedType t) {
      return t is ReadOnlyRefStruct or RefStruct;
   }
   public static bool IsClass(this ResolvedType type) {
      return type is Class or RecordClass;
   }
   public static string Keyword(this ResolvedType type) {
      return type switch {
         RefStruct or ReadOnlyStruct or ReadOnlyRefStruct or Struct => "struct",
         RecordStruct => "record struct",
         RecordClass => "record class",
         _ => "class",
      };
   }
}
