namespace Expected;

public static class ExpectedExtensions {
   public static Expected<V, E> AsExpected<V, E>(this RefExpected<V, E> e)
      => e.HasValue ? e.Value : new Unexpected<E>(e.Error);
   public static RefExpected<V, E> AsRefExpected<V, E>(this Expected<V, E> e)
      => e.HasValue ? e.Value : new Unexpected<E>(e.Error);
}

public static class OptionalExtensions {
   public static Optional<T> AsOptional<T>(this in RefOptional<T> o) => o.HasValue ? new(o.Value) : default;
   public static Optional<T> AsRefOptional<T>(this in Optional<T> o) => o.HasValue ? new(o.Value) : default;
}

public static class OptionalStructExtensions {
   public static T? AsNullable<T>(this in Optional<T> o) where T : struct
      => o.HasValue ? o.Value : null;
   public static T? AsNullable<T>(this in RefOptional<T> o) where T : struct
      => o.HasValue ? o.Value : null;
}
public static class OptionalClassExtensions {
   public static T? AsNullable<T>(this in Optional<T> o) where T : class
      => o.HasValue ? o.Value : null;
   public static T? AsNullable<T>(this in RefOptional<T> o) where T : class
      => o.HasValue ? o.Value : null;
}
