namespace Expected.Fluent;

public static class RefAllowanceExtensions {
   public static Expected<V, E> DisallowRef<V, E>(this in RefExpected<V, E> e)
      => e.HasValue ? e.Value : new Unexpected<E>(e.Error);
   public static RefExpected<V, E> AllowRef<V, E>(this in Expected<V, E> e)
      => e.HasValue ? e.Value : new Unexpected<E>(e.Error);
   public static Optional<T> DisallowRef<T>(this in RefOptional<T> o) => o.HasValue ? new(o.Value) : default;
   public static RefOptional<T> AllowRef<T>(this in Optional<T> o) => o.HasValue ? new(o._value) : default;
}
