namespace Expected;

public readonly ref struct RefExpected<T, E>
where T : allows ref struct
where E : allows ref struct {
   public readonly T? Value { get; }
   public readonly E? Error { get; }

   [MemberNotNullWhen(true, nameof(Value))]
   [MemberNotNullWhen(false, nameof(Error))]
   public bool HasValue { get; }

   [MemberNotNullWhen(false, nameof(Value))]
   [MemberNotNullWhen(true, nameof(Error))]
   public bool HasError {
      [MethodImpl(AggressiveInlining)]
      get => !HasValue;
   }

   [MethodImpl(AggressiveInlining)]
   [OverloadResolutionPriority(1)]
   public RefExpected(scoped in T value) {
      HasValue = true;
      Error = default;
      Value = value;
   }
   [MethodImpl(AggressiveInlining)]
   public RefExpected(scoped in Unexpected<E> u) {
      HasValue = false;
      Value = default;
      Error = u.Error;
   }
   public RefExpected<X, E> Map<X>(Func<T, X> f)
   where X : allows ref struct
       => HasValue ? f(Value) : new Unexpected<E>(Error);

   public RefExpected<T, X> MapError<X>(Func<E, X> f)
   where X : allows ref struct
       => HasError ? new Unexpected<X>(f(Error)) : Value;

   public RefExpected<T, E> AndThen(Func<T, RefExpected<T, E>> f)
       => HasValue ? f(Value) : this;
   public RefExpected<X, E> AndThen<X>(Func<T, RefExpected<X, E>> f)
       => HasValue ? f(Value) : new Unexpected<E>(Error);

   public RefExpected<T, E> OrElse(Func<E, RefExpected<T, E>> f)
       => HasError ? f(Error) : this;
   public RefExpected<T, X> OrElse<X>(Func<E, RefExpected<T, X>> f) where X : allows ref struct
       => HasError ? f(Error) : Value;

   [MethodImpl(AggressiveInlining)]
   public static implicit operator RefExpected<T, E>(scoped in T v) => new(v);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator RefExpected<T, E>(scoped in Unexpected<E> u) => new(u);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(in RefExpected<T, E> r) => r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator false(in RefExpected<T, E> r) => r.HasError;
   [MethodImpl(AggressiveInlining)]
   public static bool operator !(in RefExpected<T, E> r) => r.HasError;

   public static explicit operator T(scoped in RefExpected<T, E> e)
      => e.HasValue ? e.Value : throw new InvalidExpectedAccessException();
}
