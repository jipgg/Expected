namespace Expected;

public ref struct RefExpected<V, E>
where V : allows ref struct
where E : allows ref struct {
   public bool HasValue { readonly get; internal set; }
   internal V _value;
   internal E _error;
   public V Value {
      readonly get => HasValue ? _value : throw new BadExpectedAccess();
      set {
         HasValue = true;
			_value = value;
      }
   }
	public E Error {
		readonly get => HasValue ? throw new BadExpectedAccess() : _error;
		set {
			HasValue = false;
			_error = value;
		}
	}
   public readonly V ValueOr(in V v) => HasValue ? _value : v;
   public readonly E ErrorOr(in E e) => HasValue ? e : _error;

   [MethodImpl(AggressiveInlining)]
   public RefExpected(scoped in V value) {
      HasValue = true;
      _error = default!;
      _value = value;
   }
   [MethodImpl(AggressiveInlining)]
   public RefExpected(scoped in Unexpected<E> u) {
      HasValue = false;
      _value = default!;
      _error = u.Error;
   }
   public readonly RefExpected<R, E> Select<R>(Func<V, R> f) where R : allows ref struct
       => HasValue ? new(f(_value)) : new(Unexpected(_error));

   public readonly RefExpected<V, R> SelectError<R>(Func<E, R> f) where R : allows ref struct
       => HasValue ? new(_value) : new(Unexpected(f(_error)));

   public readonly RefExpected<V, E> AndThen(Func<V, RefExpected<V, E>> f)
       => HasValue ? f(_value) : this;

   public readonly RefExpected<R, E> AndThen<R>(Func<V, RefExpected<R, E>> f)
       => HasValue ? f(_value) : new(Unexpected(_error));

   public readonly RefExpected<V, E> OrElse(Func<E, RefExpected<V, E>> f)
       => HasValue ? this : f(_error);
   public readonly RefExpected<V, R> OrElse<R>(Func<E, RefExpected<V, R>> f) where R : allows ref struct
       => HasValue ? new(_value) : f(_error);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator RefExpected<V, E>(scoped in V v) => new(v);
   [MethodImpl(AggressiveInlining)]
   public static implicit operator RefExpected<V, E>(scoped in Unexpected<E> u) => new(u);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(in RefExpected<V, E> r) => r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator false(in RefExpected<V, E> r) => !r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator !(in RefExpected<V, E> r) => !r.HasValue;

   public static V operator +(scoped in RefExpected<V, E> e) => e.Value;
   public static E operator -(scoped in RefExpected<V, E> e) => e.Error;
}
