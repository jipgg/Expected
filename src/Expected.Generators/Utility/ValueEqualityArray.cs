using System.Collections.Immutable;
using System.Collections;
namespace Expected.Generators.Utility;

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
