namespace Expected;

[CouldBeUnexpected]
public readonly ref struct Expect<V, E>
where V : allows ref struct
where E : allows ref struct {
   internal readonly V _value;
   internal readonly E _error;
   internal readonly bool _hasValue;

   public V Value => _value;
   public E Error => _error;
   public bool HasValue => _hasValue;

   [MethodImpl(AggressiveInlining)]
   public Expect(scoped in V value) {
      _value = value;
      _error = default!;
      _hasValue = true;
   }
   [MethodImpl(AggressiveInlining)]
   public Expect(Unexpect _, scoped in E error) {
      _value = default!;
      _error = error;
      _hasValue = false;
   }
   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expect<V, E>(scoped in V value)
      => new(value);
   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expect<V, E>(scoped in Unexpected<E> error)
      => new(default, error.Error);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(Expect<V, E> v) => v._hasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator false(Expect<V, E> v) => !v;
   [MethodImpl(AggressiveInlining)]
   public static bool operator !(Expect<V, E> v) => !v;

   public static V operator +(Expect<V, E> v) => v.Value;
   public static E operator -(Expect<V, E> v) => v.Error;

   [MethodImpl(AggressiveInlining)]
   public Expect<R, E> Select<R>(ScopedInFunc<V, R> f) where R : allows ref struct
      => _hasValue ? new(f(_value)) : new(default, _error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expect<V, R> SelectError<R>(ScopedInFunc<E, R> f) where R : allows ref struct
      => _hasValue ? new(_value) : new(default, f(_error));

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expect<R, E> AndThen<R>(ScopedInFunc<V, Expect<R, E>> f) where R : allows ref struct
      => _hasValue ? f(_value) : new(default, _error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expect<V, R> OrElse<R>(ScopedInFunc<E, Expect<V, R>> f) where R : allows ref struct
      => _hasValue ? new(_value) : f(_error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expect<V, E> AndThen(ScopedInFunc<V, Expect<V, E>> f)
      => _hasValue ? f(_value) : new(default, _error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expect<V, E> OrElse(ScopedInFunc<E, Expect<V, E>> f)
      => _hasValue ? new(_value) : f(_error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expect<R, E> Select<R>(Func<V, R> f)
   where R : allows ref struct
      => _hasValue ? new(f(_value)) : new(default, _error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expect<V, R> SelectError<R>(Func<E, R> f)
   where R : allows ref struct
      => _hasValue ? new(_value) : new(default, f(_error));

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expect<R, E> AndThen<R>(Func<V, Expect<R, E>> f)
   where R : allows ref struct
      => _hasValue ? f(_value) : new(default, _error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expect<V, R> OrElse<R>(Func<E, Expect<V, R>> f)
   where R : allows ref struct
      => _hasValue ? new(_value) : f(_error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expect<V, E> AndThen(Func<V, Expect<V, E>> f)
      => _hasValue ? f(_value) : new(default, _error);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Expect<V, E> OrElse(Func<E, Expect<V, E>> f)
      => _hasValue ? new(_value) : f(_error);
}
