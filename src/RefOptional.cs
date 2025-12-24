namespace Expected;

public sealed class BadOptionalAccess: InvalidOperationException;

public readonly ref struct RefOptional<V>
where V : allows ref struct {
   internal readonly V? _value;
   public V Value => HasValue ? _value : throw new BadOptionalAccess();

   [MemberNotNullWhen(true, nameof(_value))]
   public bool HasValue { get; }

   public RefOptional() {
      _value = default!;
      HasValue = false;
   }
   public RefOptional(scoped in V value) {
      _value = value;
      HasValue = true;
   }
   public V ValueOr(in V v) => HasValue ? _value : v;
   public static implicit operator RefOptional<V>(scoped in V v) => new(v);
   public RefOptional<R> Transform<R>(Func<V, R> f) where R : allows ref struct
       => HasValue ? new(f(_value)) : default;

   [MethodImpl(AggressiveInlining)]
   public RefOptional<R> Transform<Transformer, R>(in Transformer f)
   where R : allows ref struct
   where Transformer: ITransformer<V, R>, allows ref struct
       => HasValue ? new(f.Transform(_value)) : default;

   public RefOptional<V> AndThen(Func<V, RefOptional<V>> f)
       => HasValue ? f(_value) : this;

   public RefOptional<R> AndThen<R>(Func<V, RefOptional<R>> f) where R : allows ref struct
       => HasValue ? f(_value) : default;

   [MethodImpl(AggressiveInlining)]
   public RefOptional<R> AndThen<Transform, R>(in Transform f)
   where R : allows ref struct
   where Transform: ITransformer<V, RefOptional<R>>
      => HasValue ? f.Transform(_value) : default;

   public static bool operator true(in RefOptional<V> o) => o.HasValue;
   public static bool operator false(in RefOptional<V> o) => !o.HasValue;
   public static bool operator !(in RefOptional<V> o) => !o.HasValue;

   public static explicit operator V(scoped in RefOptional<V> o)
      => o.HasValue ? o._value : throw new BadOptionalAccess();
}
