namespace Expected;

public struct ValueExpected<TValue, TError> {
   public bool HasValue { get; internal set; }

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
   public readonly TValue ValueOr(in TValue v) => HasValue ? _value : v;
   public readonly TError ErrorOr(in TError e) => HasValue ? e : _error;

   [MethodImpl(AggressiveInlining)]
   public ValueExpected(in TValue value) {
      HasValue = true;
      _error = default!;
      _value = value;
   }
   [MethodImpl(AggressiveInlining)]
   public ValueExpected(in Unexpected<TError> u) {
      HasValue = false;
      _value = default!;
      _error = u.Error;
   }
   public readonly ValueExpected<TResult, TError> Select<TResult>(Func<TValue, TResult> selector)
       => HasValue ? new(selector(_value)) : new(Unexpected(in _error));

   public readonly ValueExpected<TValue, TResult> SelectError<TResult>(Func<TError, TResult> selector)
       => HasValue ? new(_value) : new(Unexpected(selector(_error)));
   public readonly ValueExpected<TValue, TError> AndThen(Func<TValue, ValueExpected<TValue, TError>> selector)
       => HasValue ? selector(_value) : this;
   public readonly ValueExpected<TResult, TError> AndThen<TResult>(Func<TValue, ValueExpected<TResult, TError>> selector)
       => HasValue ? selector(_value) : new(Unexpected(in _error));
   public readonly ValueExpected<TValue, TError> OrElse(Func<TError, ValueExpected<TValue, TError>> selector)
       => HasValue ? this : selector(_error);
   public readonly ValueExpected<TValue, TResult> OrElse<TResult>(Func<TError, ValueExpected<TValue, TResult>> selector)
       => HasValue ? new(_value) : selector(_error);

   public readonly ValueTask<ValueExpected<TValue, TError>> AndThen(Func<TValue, Task<ValueExpected<TValue, TError>>> selector)
       => HasValue ? new(selector(_value)) : new(this);

   public readonly ValueTask<ValueExpected<TResult, TError>> AndThen<TResult>(Func<TValue, Task<ValueExpected<TResult, TError>>> selector)
       => HasValue ? new(selector(_value)) : new(new Unexpected<TError>(_error));

   public readonly ValueTask<ValueExpected<TValue, TError>> OrElse(Func<TError, Task<ValueExpected<TValue, TError>>> selector)
       => HasValue ? new(this) : new(selector(_error));

   public readonly ValueTask<ValueExpected<TValue, TResult>> OrElse<TResult>(Func<TError, Task<ValueExpected<TValue, TResult>>> selector)
       => HasValue ? new(_value) : new(selector(_error));

   [MethodImpl(AggressiveInlining)]
   public static implicit operator ValueExpected<TValue, TError>(in TValue v) => new(v);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator ValueExpected<TValue, TError>(in Unexpected<TError> u) => new(u);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(in ValueExpected<TValue, TError> r) => r.HasValue;

   [MethodImpl(AggressiveInlining)]
   public static bool operator false(in ValueExpected<TValue, TError> r) => !r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator !(in ValueExpected<TValue, TError> r) => !r.HasValue;

   public static TValue operator +(in ValueExpected<TValue, TError> r) => r.Value;
   public static TError operator -(in ValueExpected<TValue, TError> r) => r.Error;

   public static implicit operator Expected<TValue, TError>(in ValueExpected<TValue, TError> e)
       => e.HasValue ? e._value : Unexpected(in e._error);
   public readonly Expected<TValue, TError> AsExpected() => (Expected<TValue, TError>)this;
}

