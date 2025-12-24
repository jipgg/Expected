namespace Expected;

public readonly ref struct RefOptional<T>
where T : allows ref struct {
   public readonly T? Value { get; }

   [MemberNotNullWhen(true, nameof(Value))]
   public bool HasValue { get; }

   public RefOptional() {
      Value = default!;
      HasValue = false;
   }
   public RefOptional(scoped in T value) {
      Value = value;
      HasValue = true;
   }
   public static implicit operator RefOptional<T>(scoped in T v) => new(v);
   public bool TryValue([NotNullWhen(true)] out T? value) {
      if (HasValue) {
         value = Value!;
         return true;
      }
      value = default;
      return false;
   }
   public RefOptional<X> Map<X>(Func<T, X> f) where X : allows ref struct
       => HasValue ? new(f(Value)) : default;

   public RefOptional<T> AndThen(Func<T, RefOptional<T>> f)
       => HasValue ? f(Value) : this;

   public RefOptional<X> AndThen<X>(Func<T, RefOptional<X>> f) where X : allows ref struct
       => HasValue ? f(Value) : default;

   public static bool operator true(in RefOptional<T> o) => o.HasValue;
   public static bool operator false(in RefOptional<T> o) => !o.HasValue;
   public static bool operator !(in RefOptional<T> o) => !o.HasValue;

   public static explicit operator T(scoped in RefOptional<T> o)
      => o.HasValue ? o.Value! : throw new InvalidOptionalAccessException();
}
