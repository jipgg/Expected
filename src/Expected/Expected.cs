using System.Runtime.InteropServices;
using System.Buffers;
namespace Expected;

/// <summary>
/// <see langword="scoped"/> <see langword="in"/> variant of <see cref="Func{T, TResult}"/>.
/// </summary>
public delegate R ScopedInFunc<T, out R>(scoped in T a)
where T : allows ref struct
where R : allows ref struct;

/// <summary>
/// Gets thrown whenever the data of an <see cref="Expected{V, E}"/>
/// object (or source generated variants) are accessed incorrectly.
/// </summary>
public sealed class BadExpectedAccess : InvalidOperationException;

/// <summary>
/// Tag type marking an object to be an error.
/// Used in <see cref="Expected{V, E}"/> type constructors
/// to disambiguate the overloads in case of generic type collisions
/// or overlap between the error and value type.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0)]
public readonly struct Unexpect;

[MaybeUnexpected]
public readonly ref struct Unexpected<E> where E : allows ref struct {
   public E Error { get; }
   [MethodImpl(AggressiveInlining)]
   public Unexpected(scoped in E error) => Error = error;
   [MethodImpl(AggressiveInlining)]
   public Unexpected(E error) => Error = error;
}

/// <summary>
/// Builtin non-<see langword="ref"/> <see langword="struct"/> counterpart of <see cref="Expected{V, E}"/>.
/// </summary>
public partial struct ValueExpected<V, E> : IExpected<ValueExpected<V, E>, V, E>;

public static class ExpectedExtensions {
   /// <summary>
   /// Converts a <see cref="Expected{V, E}"/> object into a non-<see langword="ref"/> <see langword="struct"/>
   /// representation.
   /// </summary>
   public static ValueExpected<V, E> AsValueExpected<V, E>(this scoped in Expected<V, E> expected)
      => expected._hasValue ? new(expected._value) : new(default, expected._error);
}

public static class ExpectedMarshal {
   /// <summary>
   /// Unlike <see cref="Expected{V, E}.Value"/>,
   /// this does not throw <see cref="BadExpectedAccess"/> in the case
   /// of <see cref="Expected{V, E}.HasValue"/> being <see langword="false"/>.
   /// </summary>
   [MethodImpl(AggressiveInlining)]
   public static V GetValue<V, E>(scoped ref readonly Expected<V, E> expected)
   where V : allows ref struct where E : allows ref struct {
      return expected._value;
   }
   /// <summary>
   /// Unlike <see cref="Expected{V, E}.Error"/>,
   /// this does not throw <see cref="BadExpectedAccess"/> in the case
   /// of <see cref="Expected{V, E}.HasValue"/> being <see langword="true"/>.
   /// </summary>
   [MethodImpl(AggressiveInlining)]
   public static E GetError<V, E>(scoped ref readonly Expected<V, E> expected)
   where V : allows ref struct where E : allows ref struct {
      return expected._error;
   }
   /// <inheritdoc cref="GetValue{V, E}(ref readonly Expected{V, E})"/>
   [MethodImpl(AggressiveInlining)]
   public static V GetValue<V, E>(ref readonly ValueExpected<V, E> expected) {
      return expected._value;
   }
   /// <inheritdoc cref="GetError{V, E}(ref readonly Expected{V, E})"/>
   [MethodImpl(AggressiveInlining)]
   public static E GetError<V, E>(ref readonly ValueExpected<V, E> expected) {
      return expected._error;
   }
}

[MaybeUnexpected]
public readonly ref struct Expected<V, E>
where V : allows ref struct
where E : allows ref struct {
   internal readonly V _value;
   internal readonly E _error;
   internal readonly bool _hasValue;
   /// <inheritdoc cref="ValueExpected{V, E}.Value"/>
   public V Value => _hasValue ? _value : throw new BadExpectedAccess();
   /// <inheritdoc cref="ValueExpected{V, E}.Error"/>
   public E Error => _hasValue ? throw new BadExpectedAccess() : _error;
   /// <inheritdoc cref="ValueExpected{V, E}.HasValue"/>
   public bool HasValue => _hasValue;

   /// <inheritdoc cref="ValueExpected{V, E}.ValueExpected(V)"/>
   [MethodImpl(AggressiveInlining)]
   public Expected(scoped in V value) {
      _value = value;
      _error = default!;
      _hasValue = true;
   }
   /// <summary>
   /// <paramref name="unexpect"/> should be passed as <see langword="default"/>(<see cref="Unexpect"/>).
   /// </summary>
   [MethodImpl(AggressiveInlining)]
   public Expected(Unexpect unexpect, scoped in E error) {
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
   /// <summary>
   /// Gets the Value of.
   ///
   /// throws <see cref="BadExpectedAccess"/>
   /// </summary>
   /// <exception cref="BadExpectedAccess"/>
   V Value { get; }
   /// <summary>
   /// Gets the Error.
   /// </summary>
   /// <exception cref="BadExpectedAccess"/>
   E Error { get; }
   /// <summary>
   /// Gets HasValue
   /// </summary>
   bool HasValue { get; }
   /// <summary>
   /// AJDWJADAWD
   /// </summary>
   V ValueOr(V value) => HasValue ? Value : value;
   /// <summary>
   /// yoyoyoyo
   /// </summary>
   E ErrorOr(E error) => HasValue ? error : Error;
   static abstract implicit operator Expected(scoped in Unexpected<E> error);
   static abstract implicit operator Expected(scoped in Expected<V, E> expected);
   static abstract implicit operator Expected<V, E>(Expected expected);

   /// <summary>
   /// yo
   /// </summary>
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

   static abstract bool operator true(scoped in Expected expected);
   static abstract bool operator false(scoped in Expected expected);
   static abstract bool operator !(scoped in Expected expected);

   /// <summary>
   /// returns the value
   /// </summary>
   static abstract V operator +(scoped in Expected expected);
   /// <summary>
   /// returns the error
   /// </summary>
   static abstract E operator -(scoped in Expected expected);
}
public interface IMutableExpected<Expected, V, E> : IExpected<Expected, V, E>
where Expected : IMutableExpected<Expected, V, E>, allows ref struct
where V : allows ref struct
where E : allows ref struct {
   new V Value { get; set; }
   new E Error { get; set; }
}
