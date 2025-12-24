namespace Expected;

public readonly struct Expected<V, E> {
   [MemberNotNullWhen(true, nameof(_value))]
   [MemberNotNullWhen(false, nameof(_error))]
   public bool HasValue { get; }

   [MemberNotNullWhen(false, nameof(_value))]
   [MemberNotNullWhen(true, nameof(_error))]
   public bool HasError {
      [MethodImpl(AggressiveInlining)]
      get => !HasValue;
   }
   public readonly V? _value;
   public readonly E? _error;

   public V Value => HasValue ? _value : throw new BadExpectedAccess();
   public E Error => HasError ? _error : throw new BadExpectedAccess();

   [MethodImpl(AggressiveInlining)]
   public Expected(in V value) {
      HasValue = true;
      _error = default;
      _value = value;
   }
   [MethodImpl(AggressiveInlining)]
   public Expected(in Unexpected<E> u) {
      HasValue = false;
      _value = default;
      _error = u.Error;
   }
   public V ValueOr(in V v) => HasValue ? _value : v;
   public E ErrorOr(in E e) => HasError ? _error : e;
   public Expected<R, E> Transform<R>(Func<V, R> tr)
       => HasValue ? tr(_value) : new Unexpected<E>(_error);

   [MethodImpl(AggressiveInlining)]
   public Expected<R, E> Transform<Transformer, R>(in Transformer tr)
   where Transformer: ITransformer<V, R>, allows ref struct
       => HasValue ? tr.Transform(_value) : new Unexpected<E>(_error);

   public Expected<V, R> TransformError<R>(Func<E, R> tr)
       => HasError ? new Unexpected<R>(tr(_error)) : _value;

   [MethodImpl(AggressiveInlining)]
   public Expected<V, R> TransformError<Transformer, R>(in Transformer tr)
   where Transformer : ITransformer<E, R>
       => HasError ? new Unexpected<R>(tr.Transform(_error)) : _value;

   public Expected<V, E> AndThen(Func<V, Expected<V, E>> tr)
       => HasValue ? tr(_value) : this;


   public Expected<R, E> AndThen<R>(Func<V, Expected<R, E>> tr)
       => HasValue ? tr(_value) : new Unexpected<E>(_error);

   [MethodImpl(AggressiveInlining)]
   public Expected<R, E> AndThen<Transformer, R>(in Transformer tr)
   where Transformer: ITransformer<V, Expected<R, E>>
      => HasValue ? tr.Transform(_value) : new Unexpected<E>(_error);

   public Expected<V, E> OrElse(Func<E, Expected<V, E>> tr)
       => HasError ? tr(_error) : this;

   public Expected<V, R> OrElse<R>(Func<E, Expected<V, R>> tr)
       => HasError ? tr(_error) : _value;

   [MethodImpl(AggressiveInlining)]
   public Expected<V, R> OrElse<Transformer, R>(in Transformer tr)
   where Transformer: ITransformer<E, Expected<V, R>>
      => HasError ? tr.Transform(_error) : _value;

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
      => e.HasValue ? e._value : throw new BadExpectedAccess();
}

