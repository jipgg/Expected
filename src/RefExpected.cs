namespace Expected;

public sealed class BadExpectedAccess: InvalidOperationException;

public readonly ref struct RefExpected<V, E>
where V : allows ref struct
where E : allows ref struct {
   internal readonly V? _value;
   internal readonly E? _error;
   public readonly V Value => HasValue ? _value : throw new BadExpectedAccess();
   public readonly E Error => HasError ? _error : throw new BadExpectedAccess();

   [MemberNotNullWhen(true, nameof(_value))]
   [MemberNotNullWhen(false, nameof(_error))]
   public bool HasValue { get; }

   [MemberNotNullWhen(false, nameof(_value))]
   [MemberNotNullWhen(true, nameof(_error))]
   public bool HasError {
      [MethodImpl(AggressiveInlining)]
      get => !HasValue;
   }

   [MethodImpl(AggressiveInlining)]
   public RefExpected(scoped in V value) {
      HasValue = true;
      _error = default;
      _value = value;
   }
   [MethodImpl(AggressiveInlining)]
   public RefExpected(scoped in Unexpected<E> u) {
      HasValue = false;
      _value = default;
      _error = u.Error;
   }
   public V ValueOr(in V v) => HasValue ? _value : v;
   public E ErrorOr(in E e) => HasError ? _error : e;
   public RefExpected<R, E> Transform<R>(Func<V, R> f)
   where R : allows ref struct
       => HasValue ? f(_value) : new Unexpected<E>(_error);

   [MethodImpl(AggressiveInlining)]
   public RefExpected<R, E> Transform<Transformer, R>(in Transformer f)
   where R : allows ref struct
   where Transformer: ITransformer<V, R>, allows ref struct
       => HasValue ? f.Transform(_value) : new Unexpected<E>(_error);

   public RefExpected<V, R> TransformError<R>(Func<E, R> f)
   where R : allows ref struct
       => HasError ? new Unexpected<R>(f(_error)) : _value;

   [MethodImpl(AggressiveInlining)]
   public RefExpected<V, R> TransformError<Transformer, R>(in Transformer f)
   where R : allows ref struct
   where Transformer : ITransformer<E, R>
       => HasError ? new Unexpected<R>(f.Transform(_error)) : _value;

   public RefExpected<V, E> AndThen(Func<V, RefExpected<V, E>> f)
       => HasValue ? f(_value) : this;

   public RefExpected<R, E> AndThen<R>(Func<V, RefExpected<R, E>> f)
       => HasValue ? f(_value) : new Unexpected<E>(_error);

   [MethodImpl(AggressiveInlining)]
   public RefExpected<R, E> AndThen<Transformer, R>(in Transformer f)
   where R : allows ref struct
   where Transformer: ITransformer<V, RefExpected<R, E>>
      => HasValue ? f.Transform(_value) : new Unexpected<E>(_error);

   public RefExpected<V, E> OrElse(Func<E, RefExpected<V, E>> f)
       => HasError ? f(_error) : this;
   public RefExpected<V, R> OrElse<R>(Func<E, RefExpected<V, R>> f) where R : allows ref struct
       => HasError ? f(_error) : _value;

   [MethodImpl(AggressiveInlining)]
   public RefExpected<V, R> OrElse<Transformer, R>(in Transformer f)
   where R : allows ref struct
   where Transformer: ITransformer<E, RefExpected<V, R>>
      => HasError ? f.Transform(_error) : _value;

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
      => e.HasValue ? e._value : throw new BadExpectedAccess();
}
