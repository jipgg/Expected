namespace Expected.Fluent;

public static class ValueExtensions {
   public readonly ref struct AsExpectedProxy<T>(scoped in T value) {
      internal readonly T _value = value;
      public Expected<T, E> WithErrorBeing<E>() => new(_value);
   }
   public static AsExpectedProxy<T> AsExpected<T>(this T value) => new(value);
   public static Unexpected<E> AsUnexpected<E>(this E error) => new(error);
   public static Expected<T, E> AsExpected<T, E>(this T value) => new(value);
   public static Optional<T> AsOptional<T>(this T value) => new(value);
}

public static class ValueExtensionsWhereStruct {
   public static T? AsNullable<T>(this in Optional<T> o) where T : struct
      => o.HasValue ? o._value : null;
   public static T? AsNullable<T>(this in RefOptional<T> o) where T : struct
      => o.HasValue ? o._value : null;
   public static T? AsNullable<T, E>(this in Expected<T, E> o) where T : struct
      => o.HasValue ? o._value : null;
   public static T? AsNullable<T, E>(this in RefExpected<T, E> o) where T : struct where E : allows ref struct
      => o.HasValue ? o._value : null;
}
public static class ValueExtensionsWhereClass {
   public static T? AsNullable<T>(this in Optional<T> o) where T : class
      => o.HasValue ? o._value : null;
   public static T? AsNullable<T>(this in RefOptional<T> o) where T : class
      => o.HasValue ? o._value : null;
   public static T? AsNullable<T, E>(this in Expected<T, E> o) where T : class
      => o.HasValue ? o._value : null;
   public static T? AsNullable<T, E>(this in RefExpected<T, E> o) where T : class where E : allows ref struct
      => o.HasValue ? o._value : null;
}
