namespace Expected;

public sealed class Expected<TValue, TError> {
   [MemberNotNullWhen(true, nameof(_value))]
   [MemberNotNullWhen(false, nameof(_error))]
   public bool HasValue { get; internal set; }
   internal TValue? _value;
   internal TError? _error;
   public TValue Value {
      get => HasValue ? _value : throw new BadExpectedAccess();
      set {
         HasValue = true;
         _value = value;
      }
   }
   public TError Error {
      get => HasValue ? throw new BadExpectedAccess() : _error;
      set {
         HasValue = false;
         _error = value;
      }
   }
   public TValue ValueOr(TValue v) => HasValue ? _value : v;
   public TError ErrorOr(TError e) => HasValue ? e : _error;

   [MethodImpl(AggressiveInlining)]
   public Expected(TValue value) {
      HasValue = true;
      _value = value;
   }
   [MethodImpl(AggressiveInlining)]
   public Expected(in Unexpected<TError> u) {
      HasValue = false;
      _error = u.Error;
   }
   public Expected<TResult, TError> Select<TResult>(Func<TValue, TResult> selector)
       => HasValue ? new(selector(_value)) : new(Unexpected(_error));

   public Expected<TValue, TResult> SelectError<TResult>(Func<TError, TResult> selector)
        => HasValue ? new(_value) : new(Unexpected(selector(_error)));

   public Expected<TValue, TError> AndThen(Func<TValue, Expected<TValue, TError>> selector)
       => HasValue ? selector(_value) : this;

   public Expected<TResult, TError> AndThen<TResult>(Func<TValue, Expected<TResult, TError>> selector)
       => HasValue ? selector(_value) : new(Unexpected(_error));

   public Expected<TValue, TError> OrElse(Func<TError, Expected<TValue, TError>> selector)
       => HasValue ? this : selector(_error);

   public Expected<TValue, TResult> OrElse<TResult>(Func<TError, Expected<TValue, TResult>> selector)
       => HasValue ? new(_value) : selector(_error);

   public ValueTask<Expected<TValue, TError>> AndThen(Func<TValue, Task<Expected<TValue, TError>>> selector)
       => HasValue ? new(selector(_value)) : new(this);

   public ValueTask<Expected<TResult, TError>> AndThen<TResult>(Func<TValue, Task<Expected<TResult, TError>>> selector)
       => HasValue ? new(selector(_value)) : new(new Unexpected<TError>(_error));

   public ValueTask<Expected<TValue, TError>> OrElse(Func<TError, Task<Expected<TValue, TError>>> selector)
       => HasValue ? new(this) : new(selector(_error));

   public ValueTask<Expected<TValue, TResult>> OrElse<TResult>(Func<TError, Task<Expected<TValue, TResult>>> selector)
       => HasValue ? new(_value) : new(selector(_error));

   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expected<TValue, TError>(TValue v) => new(v);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expected<TValue, TError>(in Unexpected<TError> u) => new(u);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(Expected<TValue, TError> r) => r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator false(Expected<TValue, TError> r) => !r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator !(Expected<TValue, TError> r) => !r.HasValue;

   public static TValue operator +(Expected<TValue, TError> e) => e.Value;
   public static TError operator -(Expected<TValue, TError> e) => e.Error;
}
