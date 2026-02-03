using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Immutable;
namespace Expected.Meta;

using static TypeSpec;
sealed record TypeArguments(string V, string E);

enum TypeSpec {
   Bad,
   RefStruct,
   ReadOnlyRefStruct,
   ReadOnlyStruct,
   Struct,
   RecordStruct,
   Class,
   RecordClass,
}

static class TypeSpecExtensions {
   public static bool IsReadOnly(this TypeSpec t) {
      return t
         is ReadOnlyRefStruct
         or ReadOnlyStruct
         or RecordStruct
         or RecordClass;
   }
   public static bool IsStruct(this TypeSpec t) {
      return t
         is ReadOnlyRefStruct
         or RefStruct
         or ReadOnlyStruct
         or Struct
         or RecordStruct;
   }
   public static bool IsRecord(this TypeSpec t) => t is RecordClass or RecordStruct;
   public static bool IsRefStruct(this TypeSpec t) {
      return t is ReadOnlyRefStruct or RefStruct;
   }
   public static bool IsClass(this TypeSpec type) {
      return type is Class or RecordClass;
   }
   public static string Keyword(this TypeSpec type) {
      return type switch {
         RefStruct or ReadOnlyStruct or ReadOnlyRefStruct or Struct => "struct",
         RecordStruct => "record struct",
         RecordClass => "record class",
         _ => "class",
      };
   }
}
static class NamedSymbolExtensions {
   public static bool InheritsFrom(this INamedTypeSymbol symbol, ITypeSymbol target) {
      INamedTypeSymbol? baseType = symbol.BaseType;
      while (baseType is not null) {
         if (SymbolEqualityComparer.Default.Equals(baseType, target)) {
            return true;
         }
         baseType = baseType.BaseType;
      }
      return false;
   }
}
public enum StorageStrategy {
   Sequential,
   Union,
   Object,
   SameField,
}
sealed record TypeSymbols(ITypeSymbol V, ITypeSymbol E) {
   public static implicit operator TypeArguments(TypeSymbols r)
      => new(Formatting.Format(r.V), Formatting.Format(r.E));
}

static class Formatting {
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

   public static string ToHintName(INamedTypeSymbol symbol, TypeArguments typeArguments) {
      var ns = symbol.ContainingNamespace;
      var nsPrefix = ns.IsGlobalNamespace ? "" : $"{ns.ToDisplayString()}.";
      var typeArgs = ToFilenameString($"{{{typeArguments.V},{typeArguments.E}}}");
      return $"{nsPrefix}{symbol.Name}{typeArgs}.g";
   }
}


class ValueEqualityArray<T>(ImmutableArray<T> data) : IEquatable<ValueEqualityArray<T>>, IEnumerable<T> where T : IEquatable<T> {
   readonly int _cachedHashCode = MakeHashCode(data);
   readonly ImmutableArray<T> _data = data;

   static int MakeHashCode(ImmutableArray<T> data) {
      int hash = 5381;
      foreach (var e in data) {
         hash = ((hash << 5) + hash) ^ (e?.GetHashCode() ?? 0);
      }
      return hash;
   }
   public bool Equals(ValueEqualityArray<T>? other) {
      if (other is null) return false;
      if (_cachedHashCode != other._cachedHashCode) return false;
      return _data.SequenceEqual(other._data);
   }
   public override bool Equals(object? obj) {
      if (obj is not ValueEqualityArray<T> other) return false;
      return Equals(other);
   }
   public ImmutableArray<T>.Enumerator GetEnumerator() => _data.GetEnumerator();
   IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_data).GetEnumerator();
   IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_data).GetEnumerator();

   public override int GetHashCode() => _cachedHashCode;
   public static bool operator ==(ValueEqualityArray<T>? a, ValueEqualityArray<T>? b)
      => object.ReferenceEquals(a, b) || (a?.Equals(b) ?? false);
   public static bool operator !=(ValueEqualityArray<T>? a, ValueEqualityArray<T>? b)
      => !(a == b);

   public int Length => _data.Length;
   public T this[int index] { get => _data[index]; }
}

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
