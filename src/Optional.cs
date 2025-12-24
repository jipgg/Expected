namespace Expected;

public readonly struct Optional<V> {
   internal readonly V? _value;
   public V Value => HasValue ? _value : throw new BadOptionalAccess();
   [MemberNotNullWhen(true, nameof(_value))]
   public bool HasValue { get; }
   public Optional() {
      _value = default!;
      HasValue = false;
   }
   public Optional(in V value) {
      _value = value;
      HasValue = true;
   }
   public V ValueOr(in V v) => HasValue ? _value : v;
   public static implicit operator Optional<V>(in V v) => new(v);
   public Optional<R> Transform<R>(Func<V, R> f)
       => HasValue ? new(f(_value)) : default;

   [MethodImpl(AggressiveInlining)]
   public Optional<R> Transform<Transformer, R>(in Transformer f)
   where Transformer: ITransformer<V, R>, allows ref struct
       => HasValue ? new(f.Transform(_value)) : default;

   public Optional<V> AndThen(Func<V, Optional<V>> f)
       => HasValue ? f(_value) : this;

   public Optional<R> AndThen<R>(Func<V, Optional<R>> f)
       => HasValue ? f(_value) : default;

   [MethodImpl(AggressiveInlining)]
   public Optional<R> AndThen<Transformer, R>(in Transformer f)
   where Transformer: ITransformer<V, Optional<R>>
      => HasValue ? f.Transform(_value) : default;

   public static bool operator true(in Optional<V> o) => o.HasValue;
   public static bool operator false(in Optional<V> o) => !o.HasValue;
   public static bool operator !(in Optional<V> o) => !o.HasValue;

   public static explicit operator V(in Optional<V> n)
      => n.HasValue ? n._value! : throw new BadOptionalAccess();
}
