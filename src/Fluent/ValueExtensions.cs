namespace Expected.Fluent;

public static class ValueExtensions {
   public readonly ref struct WithValue<T>(scoped in T value) {
      internal readonly T _value = value;
      public Expected<T, E> WithErrorType<E>() => new(_value);
   }
   public readonly ref struct WithError<E>(scoped in Unexpected<E> unexpected) {
      internal readonly Unexpected<E> _error = unexpected;
      public Expected<T, E> WithValueType<T>() => _error;
   }
   public static WithValue<T> MakeExpected<T>(this T value) => new(value);
   public static WithError<E> MakeExpected<E>(this in Unexpected<E> u) => new(u);
   public static Unexpected<E> AsUnexpected<E>(this E error) => new(error);
   public static Optional<T> AsOptional<T>(this T? value) => value is null ? default(Optional<T>) : new(value);
   public static Optional<T> AsOptional<T, E>(this Expected<T, E> expected) => expected ? new((T)expected) : default;
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
