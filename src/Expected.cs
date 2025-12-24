namespace Expected;

public readonly struct Expected<V, E> {
   [MemberNotNullWhen(true, nameof(Value))]
   [MemberNotNullWhen(false, nameof(Error))]
   public bool HasValue { get; }

   [MemberNotNullWhen(false, nameof(Value))]
   [MemberNotNullWhen(true, nameof(Error))]
   public bool HasError {
      [MethodImpl(AggressiveInlining)]
      get => !HasValue;
   }

   public V? Value { get; }
   public E? Error { get; }

   [MethodImpl(AggressiveInlining)]
   public Expected(in V value) {
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
   public V ValueOr(in V v) => HasValue ? Value : v;
   public E ErrorOr(in E e) => HasError ? Error : e;
   public Expected<R, E> Transform<R>(Func<V, R> tr)
       => HasValue ? tr(Value) : new Unexpected<E>(Error);

   [MethodImpl(AggressiveInlining)]
   public Expected<R, E> Transform<Transformer, R>(in Transformer tr)
   where Transformer: ITransformer<V, R>, allows ref struct
       => HasValue ? tr.Transform(Value) : new Unexpected<E>(Error);

   public Expected<V, R> TransformError<R>(Func<E, R> tr)
       => HasError ? new Unexpected<R>(tr(Error)) : Value;

   [MethodImpl(AggressiveInlining)]
   public Expected<V, R> TransformError<Transformer, R>(in Transformer tr)
   where Transformer : ITransformer<E, R>
       => HasError ? new Unexpected<R>(tr.Transform(Error)) : Value;

   public Expected<V, E> AndThen(Func<V, Expected<V, E>> tr)
       => HasValue ? tr(Value) : this;


   public Expected<R, E> AndThen<R>(Func<V, Expected<R, E>> tr)
       => HasValue ? tr(Value) : new Unexpected<E>(Error);

   [MethodImpl(AggressiveInlining)]
   public Expected<R, E> AndThen<Transformer, R>(in Transformer tr)
   where Transformer: ITransformer<V, Expected<R, E>>
      => HasValue ? tr.Transform(Value) : new Unexpected<E>(Error);

   public Expected<V, E> OrElse(Func<E, Expected<V, E>> tr)
       => HasError ? tr(Error) : this;

   public Expected<V, R> OrElse<R>(Func<E, Expected<V, R>> tr)
       => HasError ? tr(Error) : Value;

   [MethodImpl(AggressiveInlining)]
   public Expected<V, R> OrElse<Transformer, R>(in Transformer tr)
   where Transformer: ITransformer<E, Expected<V, R>>
      => HasError ? tr.Transform(Error) : Value;

   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expected<V, E>(in V v) => new(v);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expected<V, E>(in Unexpected<E> u) => new(u);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(in Expected<V, E> r) => r.HasValue;

   [MethodImpl(AggressiveInlining)]
   public static bool operator false(in Expected<V, E> r) => r.HasError;

   [MethodImpl(AggressiveInlining)]
   public static bool operator !(in Expected<V, E> r) => r.HasError;

   public static explicit operator V(in Expected<V, E> e)
      => e.HasValue ? e.Value : throw new InvalidOperationException();
}

