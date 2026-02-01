using System.Runtime.InteropServices;
namespace Expected;

public delegate R ScopedInFunc<T, out R>(scoped in T a)
   where T : allows ref struct
   where R : allows ref struct;

public sealed class BadExpectedAccess : InvalidOperationException;

[StructLayout(LayoutKind.Sequential, Size = 0)]
public readonly struct Unexpect;

[CouldBeUnexpected]
public readonly ref struct Unexpected<E> where E : allows ref struct {
   public E Error { get; }
   [MethodImpl(AggressiveInlining)]
   public Unexpected(scoped in E error) => Error = error;
   [MethodImpl(AggressiveInlining)]
   public Unexpected(E error) => Error = error;
}


[CouldBeUnexpected]
public readonly ref struct Expected<V, E>
where V : allows ref struct
where E : allows ref struct {
   internal readonly V _value;
   internal readonly E _error;
   internal readonly bool _hasValue;
   public V Value => _hasValue ? _value : throw new BadExpectedAccess();
   public E Error => _hasValue ? throw new BadExpectedAccess() : _error;
   public bool HasValue => _hasValue;

   [MethodImpl(AggressiveInlining)]
   public Expected(scoped in V value) {
      _value = value;
      _error = default!;
      _hasValue = true;
   }
   [MethodImpl(AggressiveInlining)]
   public Expected(Unexpect _, scoped in E error) {
      _value = default!;
      _error = error;
      _hasValue = false;
   }
   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expected<V, E>(scoped in V value)
      => new(value);
   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expected<V, E>(scoped in Unexpected<E> error)
      => new(default, error.Error);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(Expected<V, E> v) => v._hasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator false(Expected<V, E> v) => !v;
   [MethodImpl(AggressiveInlining)]
   public static bool operator !(Expected<V, E> v) => !v;

   public static V operator +(Expected<V, E> v) => v.Value;
   public static E operator -(Expected<V, E> v) => v.Error;

   [MethodImpl(AggressiveInlining)]
   public Expected<R, E> Select<R>(ScopedInFunc<V, R> f) where R : allows ref struct
      => _hasValue ? new(f(_value)) : new(default, _error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expected<V, R> SelectError<R>(ScopedInFunc<E, R> f) where R : allows ref struct
      => _hasValue ? new(_value) : new(default, f(_error));

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expected<R, E> AndThen<R>(ScopedInFunc<V, Expected<R, E>> f) where R : allows ref struct
      => _hasValue ? f(_value) : new(default, _error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expected<V, R> OrElse<R>(ScopedInFunc<E, Expected<V, R>> f) where R : allows ref struct
      => _hasValue ? new(_value) : f(_error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expected<V, E> AndThen(ScopedInFunc<V, Expected<V, E>> f)
      => _hasValue ? f(_value) : new(default, _error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expected<V, E> OrElse(ScopedInFunc<E, Expected<V, E>> f)
      => _hasValue ? new(_value) : f(_error);

   [MethodImpl(AggressiveInlining)]
   public Expected<R, E> Select<R>(Func<V, R> f)
   where R : allows ref struct
      => _hasValue ? new(f(_value)) : new(default, _error);

   [MethodImpl(AggressiveInlining)]
   public Expected<V, R> SelectError<R>(Func<E, R> f)
   where R : allows ref struct
      => _hasValue ? new(_value) : new(default, f(_error));

   [MethodImpl(AggressiveInlining)]
   public Expected<R, E> AndThen<R>(Func<V, Expected<R, E>> f)
   where R : allows ref struct
      => _hasValue ? f(_value) : new(default, _error);

   [MethodImpl(AggressiveInlining)]
   public Expected<V, R> OrElse<R>(Func<E, Expected<V, R>> f)
   where R : allows ref struct
      => _hasValue ? new(_value) : f(_error);

   [MethodImpl(AggressiveInlining)]
   public Expected<V, E> AndThen(Func<V, Expected<V, E>> f)
      => _hasValue ? f(_value) : new(default, _error);

   [MethodImpl(AggressiveInlining)]
   public Expected<V, E> OrElse(Func<E, Expected<V, E>> f)
      => _hasValue ? new(_value) : f(_error);
}

public interface IExpected<Expected, V, E>
where Expected : IExpected<Expected, V, E>, allows ref struct
where V : allows ref struct
where E : allows ref struct {
   V Value { get; }
   E Error { get; }
   bool HasValue { get; }
   V ValueOr(V value) => HasValue ? Value : value;
   E ErrorOr(E error) => HasValue ? error : Error;
   static abstract implicit operator Expected(V value);
   static abstract implicit operator Expected(Unexpected<E> error);
   static abstract implicit operator Expected(Expected<V, E> expected);
   static abstract implicit operator Expected<V, E>(Expected expected);

   Expected<R, E> Select<R>(ScopedInFunc<V, R> selector) where R : allows ref struct;
   Expected<V, R> SelectError<R>(ScopedInFunc<E, R> selector) where R : allows ref struct;
   Expected<R, E> AndThen<R>(ScopedInFunc<V, Expected<R, E>> selector) where R : allows ref struct;
   Expected<V, R> OrElse<R>(ScopedInFunc<E, Expected<V, R>> selector) where R : allows ref struct;
   Expected<R, E> Select<R>(Func<V, R> selector) where R : allows ref struct;
   Expected<V, R> SelectError<R>(Func<E, R> selector) where R : allows ref struct;
   Expected<R, E> AndThen<R>(Func<V, Expected<R, E>> selector) where R : allows ref struct;
   Expected<V, R> OrElse<R>(Func<E, Expected<V, R>> selector) where R : allows ref struct;

   Expected<V, E> AndThen(Func<V, Expected<V, E>> selector);
   Expected<V, E> OrElse(Func<E, Expected<V, E>> selector);
   Expected<V, E> AndThen(ScopedInFunc<V, Expected<V, E>> selector);
   Expected<V, E> OrElse(ScopedInFunc<E, Expected<V, E>> selector);

   Expected<V, E> AsExpected();

   static abstract bool operator true(Expected expected);
   static abstract bool operator false(Expected expected);
   static abstract bool operator !(Expected expected);

   static abstract V operator +(Expected expected);
   static abstract E operator -(Expected expected);

}

