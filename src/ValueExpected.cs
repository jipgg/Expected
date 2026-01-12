namespace Expected;

public struct ValueExpected<V, E> where V : notnull where E : notnull {
   public bool HasValue { get; internal set; }

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
   public ValueExpected(in V value) {
      HasValue = true;
      _error = default!;
      _value = value;
   }
   [MethodImpl(AggressiveInlining)]
   public ValueExpected(in Unexpected<E> u) {
      HasValue = false;
      _value = default!;
      _error = u.Error;
   }
   public readonly ValueExpected<R, E> Select<R>(Func<V, R> selector) where R : notnull
       => HasValue ? new(selector(_value)) : new(Unexpected(in _error));
   public readonly ValueExpected<V, R> SelectError<R>(Func<E, R> selector) where R : notnull
       => HasValue ? new(_value) : new(Unexpected(selector(_error)));
   public readonly ValueExpected<V, E> AndThen(Func<V, ValueExpected<V, E>> selector)
       => HasValue ? selector(_value) : this;
   public readonly ValueExpected<R, E> AndThen<R>(Func<V, ValueExpected<R, E>> selector) where R : notnull
       => HasValue ? selector(_value) : new(Unexpected(in _error));
   public readonly ValueExpected<V, E> OrElse(Func<E, ValueExpected<V, E>> selector)
       => HasValue ? this : selector(_error);
   public readonly ValueExpected<V, R> OrElse<R>(Func<E, ValueExpected<V, R>> selector) where R : notnull
       => HasValue ? new(_value) : selector(_error);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator ValueExpected<V, E>(in V v) => new(v);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator ValueExpected<V, E>(in Unexpected<E> u) => new(u);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(in ValueExpected<V, E> r) => r.HasValue;

   [MethodImpl(AggressiveInlining)]
   public static bool operator false(in ValueExpected<V, E> r) => !r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator !(in ValueExpected<V, E> r) => !r.HasValue;

	public static V operator +(in ValueExpected<V, E> r) => r.Value;
	public static E operator -(in ValueExpected<V, E> r) => r.Error;

   public static implicit operator Expected<V, E>(in ValueExpected<V, E> e)
       => e.HasValue ? e._value : Unexpected(in e._error);
   public Expected<V, E> AsExpected() => (Expected<V, E>)this;
}

