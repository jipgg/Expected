namespace Expected;

public ref struct RefExpected<TValue, TError>
where TValue : allows ref struct
where TError : allows ref struct {
   public bool HasValue { readonly get; internal set; }
   internal TValue _value;
   internal TError _error;
   public TValue Value {
      readonly get => HasValue ? _value : throw new BadExpectedAccess();
      set {
         HasValue = true;
         _value = value;
      }
   }
   public TError Error {
      readonly get => HasValue ? throw new BadExpectedAccess() : _error;
      set {
         HasValue = false;
         _error = value;
      }
   }
   public readonly TValue ValueOr(in TValue v) {
      return HasValue ? _value : v;
   }
   public readonly TError ErrorOr(in TError e) => HasValue ? e : _error;

   [MethodImpl(AggressiveInlining)]
   public RefExpected(scoped in TValue value) {
      HasValue = true;
      _error = default!;
      _value = value;
   }
   [MethodImpl(AggressiveInlining)]
   public RefExpected(scoped in Unexpected<TError> u) {
      HasValue = false;
      _value = default!;
      _error = u.Error;
   }
   public readonly RefExpected<TResult, TError> Select<TResult>(Func<TValue, TResult> f) where TResult : allows ref struct {
      return HasValue ? new(f(_value)) : new(Unexpected(_error));
   }

   public readonly RefExpected<TValue, TResult> SelectError<TResult>(Func<TError, TResult> f) where TResult : allows ref struct
       => HasValue ? new(_value) : new(Unexpected(f(_error)));

   public readonly RefExpected<TValue, TError> AndThen(Func<TValue, RefExpected<TValue, TError>> f)
       => HasValue ? f(_value) : this;

   public readonly RefExpected<TResult, TError> AndThen<TResult>(Func<TValue, RefExpected<TResult, TError>> f)
       => HasValue ? f(_value) : new(Unexpected(_error));

   public readonly RefExpected<TValue, TError> OrElse(Func<TError, RefExpected<TValue, TError>> f)
       => HasValue ? this : f(_error);
   public readonly RefExpected<TValue, TResult> OrElse<TResult>(Func<TError, RefExpected<TValue, TResult>> f) where TResult : allows ref struct
       => HasValue ? new(_value) : f(_error);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator RefExpected<TValue, TError>(scoped in TValue v) => new(v);
   [MethodImpl(AggressiveInlining)]
   public static implicit operator RefExpected<TValue, TError>(scoped in Unexpected<TError> u) => new(u);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(in RefExpected<TValue, TError> r) => r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator false(in RefExpected<TValue, TError> r) => !r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator !(in RefExpected<TValue, TError> r) => !r.HasValue;

   public static TValue operator +(scoped in RefExpected<TValue, TError> e) => e.Value;
   public static TError operator -(scoped in RefExpected<TValue, TError> e) => e.Error;

}
