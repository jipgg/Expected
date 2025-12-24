namespace Expected;

public readonly ref struct RefExpected<V, E>
where V : allows ref struct
where E : allows ref struct {
   public readonly V? Value { get; }
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
   public RefExpected(scoped in V value) {
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
   public RefExpected<R, E> Transform<R>(Func<V, R> f)
   where R : allows ref struct
       => HasValue ? f(Value) : new Unexpected<E>(Error);

   [MethodImpl(AggressiveInlining)]
   public RefExpected<R, E> Transform<Transformer, R>(in Transformer f)
   where R : allows ref struct
   where Transformer: ITransformer<V, R>, allows ref struct
       => HasValue ? f.Transform(Value) : new Unexpected<E>(Error);

   public RefExpected<V, R> TransformError<R>(Func<E, R> f)
   where R : allows ref struct
       => HasError ? new Unexpected<R>(f(Error)) : Value;

   [MethodImpl(AggressiveInlining)]
   public RefExpected<V, R> TransformError<Transformer, R>(in Transformer f)
   where R : allows ref struct
   where Transformer : ITransformer<E, R>
       => HasError ? new Unexpected<R>(f.Transform(Error)) : Value;

   public RefExpected<V, E> AndThen(Func<V, RefExpected<V, E>> f)
       => HasValue ? f(Value) : this;

   public RefExpected<R, E> AndThen<R>(Func<V, RefExpected<R, E>> f)
       => HasValue ? f(Value) : new Unexpected<E>(Error);

   [MethodImpl(AggressiveInlining)]
   public RefExpected<R, E> AndThen<Transformer, R>(in Transformer f)
   where R : allows ref struct
   where Transformer: ITransformer<V, RefExpected<R, E>>
      => HasValue ? f.Transform(Value) : new Unexpected<E>(Error);

   public RefExpected<V, E> OrElse(Func<E, RefExpected<V, E>> f)
       => HasError ? f(Error) : this;
   public RefExpected<V, R> OrElse<R>(Func<E, RefExpected<V, R>> f) where R : allows ref struct
       => HasError ? f(Error) : Value;

   [MethodImpl(AggressiveInlining)]
   public RefExpected<V, R> OrElse<Transformer, R>(in Transformer f)
   where R : allows ref struct
   where Transformer: ITransformer<E, RefExpected<V, R>>
      => HasError ? f.Transform(Error) : Value;

   [MethodImpl(AggressiveInlining)]
   public static implicit operator RefExpected<V, E>(scoped in V v) => new(v);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator RefExpected<V, E>(scoped in Unexpected<E> u) => new(u);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(in RefExpected<V, E> r) => r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator false(in RefExpected<V, E> r) => r.HasError;
   [MethodImpl(AggressiveInlining)]
   public static bool operator !(in RefExpected<V, E> r) => r.HasError;

   public static explicit operator V(scoped in RefExpected<V, E> e)
      => e.HasValue ? e.Value : throw new InvalidOperationException();
}
