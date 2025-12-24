namespace Expected;

public sealed class InvalidOptionalAccessException : InvalidOperationException;

public readonly struct Optional<T> {
   public readonly T? Value;
   [MemberNotNullWhen(true, nameof(Value))]
   public bool HasValue { get; }
   public Optional() {
      Value = default!;
      HasValue = false;
   }
   public Optional(in T value) {
      Value = value;
      HasValue = true;
   }
   public static implicit operator Optional<T>(in T v) => new(v);
   public bool TryValue([NotNullWhen(true)] out T? value) {
      if (HasValue) {
         value = Value!;
         return true;
      }
      value = default;
      return false;
   }
   public Optional<X> Map<X>(Func<T, X> f)
       => HasValue ? new(f(Value)) : default;

   public Optional<T> AndThen(Func<T, Optional<T>> f)
       => HasValue ? f(Value) : this;

   public Optional<X> AndThen<X>(Func<T, Optional<X>> f)
       => HasValue ? f(Value) : default;

   public static bool operator true(in Optional<T> o) => o.HasValue;
   public static bool operator false(in Optional<T> o) => !o.HasValue;
   public static bool operator !(in Optional<T> o) => !o.HasValue;

   public static explicit operator T(in Optional<T> n)
      => n.HasValue ? n.Value! : throw new InvalidOptionalAccessException();
}


