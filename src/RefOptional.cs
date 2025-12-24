namespace Expected;

public readonly ref struct RefOptional<V>
where V : allows ref struct {
   public readonly V? Value { get; }

   [MemberNotNullWhen(true, nameof(Value))]
   public bool HasValue { get; }

   public RefOptional() {
      Value = default!;
      HasValue = false;
   }
   public RefOptional(scoped in V value) {
      Value = value;
      HasValue = true;
   }
   public static implicit operator RefOptional<V>(scoped in V v) => new(v);
   public V ValueOr(in V v) => HasValue ? Value : v;
   public RefOptional<R> Transform<R>(Func<V, R> f) where R : allows ref struct
       => HasValue ? new(f(Value)) : default;

   [MethodImpl(AggressiveInlining)]
   public RefOptional<R> Transform<Transformer, R>(in Transformer f)
   where R : allows ref struct
   where Transformer: ITransformer<V, R>, allows ref struct
       => HasValue ? new(f.Transform(Value)) : default;

   public RefOptional<V> AndThen(Func<V, RefOptional<V>> f)
       => HasValue ? f(Value) : this;

   public RefOptional<R> AndThen<R>(Func<V, RefOptional<R>> f) where R : allows ref struct
       => HasValue ? f(Value) : default;

   [MethodImpl(AggressiveInlining)]
   public RefOptional<R> AndThen<Transform, R>(in Transform f)
   where R : allows ref struct
   where Transform: ITransformer<V, RefOptional<R>>
      => HasValue ? f.Transform(Value) : default;

   public static bool operator true(in RefOptional<V> o) => o.HasValue;
   public static bool operator false(in RefOptional<V> o) => !o.HasValue;
   public static bool operator !(in RefOptional<V> o) => !o.HasValue;

   public static explicit operator V(scoped in RefOptional<V> o)
      => o.HasValue ? o.Value! : throw new InvalidOperationException();
}
