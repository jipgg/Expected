namespace Expected;

public sealed class InvalidExpectedAccessException : InvalidOperationException;

public readonly struct Expected<T, E> {
   [MemberNotNullWhen(true, nameof(Value))]
   [MemberNotNullWhen(false, nameof(Error))]
   public bool HasValue {get;}
   [MemberNotNullWhen(false, nameof(Value))]
   [MemberNotNullWhen(true, nameof(Error))]
   public bool HasError {
      [MethodImpl(AggressiveInlining)]
      get => !HasValue;
   }

   public T? Value { get; }
   public E? Error { get; }

   [MethodImpl(AggressiveInlining)]
   [OverloadResolutionPriority(1)]
   public Expected(in T value) {
      HasValue = true;
      Error = default;
      Value = value;
   }
   [MethodImpl(AggressiveInlining)]
   public Expected(in Unexpected<E> u) {
      HasValue = false;
      Value = default;
      Error = u.Error;
   }

   public Expected<X, E> Map<X>(Func<T, X> f)
       => HasValue ? f(Value) : new Unexpected<E>(Error);

   public Expected<T, X> MapError<X>(Func<E, X> f)
       => HasError ? new Unexpected<X>(f(Error)) : Value;

   public Expected<T, E> AndThen(Func<T, Expected<T, E>> f)
       => HasValue ? f(Value) : this;

   public Expected<X, E> AndThen<X>(Func<T, Expected<X, E>> f)
       => HasValue ? f(Value) : new Unexpected<E>(Error);

   public Expected<T, E> OrElse(Func<E, Expected<T, E>> f)
       => HasError ? f(Error) : this;

   public Expected<T, X> OrElse<X>(Func<E, Expected<T, X>> f)
       => HasError ? f(Error) : Value;

   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expected<T, E>(in T v) => new(v);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expected<T, E>(in Unexpected<E> u) => new(u);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(in Expected<T, E> r) => r.HasValue;

   [MethodImpl(AggressiveInlining)]
   public static bool operator false(in Expected<T, E> r) => r.HasError;

   [MethodImpl(AggressiveInlining)]
   public static bool operator !(in Expected<T, E> r) => r.HasError;

   public static explicit operator T(in Expected<T, E> e)
      => e.HasValue ? e.Value : throw new InvalidExpectedAccessException();
}

