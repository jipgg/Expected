using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
namespace Expected;

public sealed class BadExpectedAccess : InvalidOperationException;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class ExpectedAttribute : Attribute;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class ExpectedAttribute<V, E> : Attribute
where V : allows ref struct
where E : allows ref struct;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class ExpectsAttribute<V> : Attribute
where V : allows ref struct;


[StructLayout(LayoutKind.Sequential, Size = 0)]
public readonly struct Unexpect;

public interface IExpected<TExpected, V, E>
where TExpected : IExpected<TExpected, V, E>, allows ref struct
where V : allows ref struct
where E : allows ref struct {
   V Value { get; }
   E Error { get; }
   bool HasValue { get; }
   V ValueOr(V value) => HasValue ? Value : value;
   E ErrorOr(E error) => HasValue ? error : Error;
   static abstract implicit operator TExpected(V value);
   static abstract implicit operator TExpected(Unexpected<E> error);
   static abstract implicit operator TExpected(Expect<V, E> expected);
   static abstract implicit operator Expect<V, E>(TExpected expected);

   Expect<R, E> Select<R>(ScopedInFunc<V, R> selector)
   where R : allows ref struct;
   Expect<V, TTo> SelectError<TTo>(ScopedInFunc<E, TTo> selector)
   where TTo : allows ref struct;
   Expect<R, E> AndThen<R>(ScopedInFunc<V, Expect<R, E>> selector)
   where R : allows ref struct;
   Expect<V, R> OrElse<R>(ScopedInFunc<E, Expect<V, R>> selector)
   where R : allows ref struct;
   Expect<R, E> Select<R>(Func<V, R> selector)
   where R : allows ref struct;
   Expect<V, R> SelectError<R>(Func<E, R> selector)
   where R : allows ref struct;
   Expect<R, E> AndThen<R>(Func<V, Expect<R, E>> selector)
   where R : allows ref struct;
   Expect<V, R> OrElse<R>(Func<E, Expect<V, R>> selector)
   where R : allows ref struct;

   Expect<V, E> AndThen(Func<V, Expect<V, E>> selector);
   Expect<V, E> OrElse(Func<E, Expect<V, E>> selector);
   Expect<V, E> AndThen(ScopedInFunc<V, Expect<V, E>> selector);
   Expect<V, E> OrElse(ScopedInFunc<E, Expect<V, E>> selector);

   static abstract bool operator true(TExpected expected);
   static abstract bool operator false(TExpected expected);
   static abstract bool operator !(TExpected expected);

   static abstract V operator +(TExpected expected);
   static abstract E operator -(TExpected expected);
}
public sealed partial class Expected<V, E>
: IEquatable<Expected<V, E>>
, IExpected<Expected<V, E>, V, E> {
   public override int GetHashCode() {
      var hash = new HashCode();
      hash.Add(_hasValue);
      if (_hasValue) hash.Add(_value, EqualityComparer<V>.Default);
      else hash.Add(_error, EqualityComparer<E>.Default);
      return hash.ToHashCode();
   }
   public bool Equals(Expected<V, E>? other) {
      if (other is null || _hasValue != other._hasValue) return false;
      return _hasValue
         ? EqualityComparer<V>.Default.Equals(_value, other._value)
         : EqualityComparer<E>.Default.Equals(_error, other._error);
   }
   public override bool Equals(object? obj) {
      if (obj is not Expected<V, E> other) return false;
      return Equals(other);
   }
   public static bool operator ==(Expected<V, E>? a, Expected<V, E>? b) {
      if (ReferenceEquals(a, b)) return true;
      if (a is null) return false;
      return a.Equals(b);
   }
   public static bool operator !=(Expected<V, E>? a, Expected<V, E>? b) => !(a == b);
}
public partial struct ValueExpected<V, E>
: IEquatable<ValueExpected<V, E>>
, IExpected<ValueExpected<V, E>, V, E> {
   public readonly override int GetHashCode() {
      var hash = new HashCode();
      hash.Add(_hasValue);
      if (_hasValue) hash.Add(_value, EqualityComparer<V>.Default);
      else hash.Add(_error, EqualityComparer<E>.Default);
      return hash.ToHashCode();
   }
   [MethodImpl(AggressiveInlining)]
   public readonly bool Equals(scoped in ValueExpected<V, E> other) {
      if (_hasValue != other._hasValue) return false;
      return _hasValue
         ? EqualityComparer<V>.Default.Equals(_value, other._value)
         : EqualityComparer<E>.Default.Equals(_error, other._error);
   }
   [MethodImpl(AggressiveInlining)]
   readonly bool IEquatable<ValueExpected<V, E>>.Equals(ValueExpected<V, E> other) => Equals(other);
   public readonly override bool Equals(object? obj) {
      if (obj is not ValueExpected<V, E> other) return false;
      return Equals(other);
   }
   [MethodImpl(AggressiveInlining)]
   public static bool operator ==(scoped in ValueExpected<V, E> a, scoped in ValueExpected<V, E> b) => a.Equals(b);
   [MethodImpl(AggressiveInlining)]
   public static bool operator !=(scoped in ValueExpected<V, E> a, scoped in ValueExpected<V, E> b) => !(a == b);

   public readonly Expected<V, E> AsExpected()
      => _hasValue ? new(_value) : new(default, _error);
}
