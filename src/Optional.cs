namespace Expected;

public readonly struct Optional<V> {
   public readonly V? Value;
   [MemberNotNullWhen(true, nameof(Value))]
   public bool HasValue { get; }
   public Optional() {
      Value = default!;
      HasValue = false;
   }
   public Optional(in V value) {
      Value = value;
      HasValue = true;
   }
   public V ValueOr(in V v) => HasValue ? Value : v;
   public static implicit operator Optional<V>(in V v) => new(v);
   public Optional<R> Transform<R>(Func<V, R> f)
       => HasValue ? new(f(Value)) : default;

   [MethodImpl(AggressiveInlining)]
   public Optional<R> Transform<Transformer, R>(in Transformer f)
   where Transformer: ITransformer<V, R>, allows ref struct
       => HasValue ? new(f.Transform(Value)) : default;

   public Optional<V> AndThen(Func<V, Optional<V>> f)
       => HasValue ? f(Value) : this;

   public Optional<R> AndThen<R>(Func<V, Optional<R>> f)
       => HasValue ? f(Value) : default;

   [MethodImpl(AggressiveInlining)]
   public Optional<R> AndThen<Transformer, R>(in Transformer f)
   where Transformer: ITransformer<V, Optional<R>>
      => HasValue ? f.Transform(Value) : default;

   public static bool operator true(in Optional<V> o) => o.HasValue;
   public static bool operator false(in Optional<V> o) => !o.HasValue;
   public static bool operator !(in Optional<V> o) => !o.HasValue;

   public static explicit operator V(in Optional<V> n)
      => n.HasValue ? n.Value! : throw new InvalidOperationException();
}
