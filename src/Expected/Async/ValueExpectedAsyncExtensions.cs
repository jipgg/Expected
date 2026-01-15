namespace Expected.Async;

public static class ValueExpectedAsyncExtensions {
   public static async Task<ValueExpected<TResult, TError>> SelectAsync<TValue, TError, TResult>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TValue, Task<TResult>> f) {
      var r = await task;
      return await f(+r);
   }
   public static async Task<ValueExpected<TResult, TError>> SelectErrorAsync<TValue, TError, TResult>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TError, Task<TResult>> f) {
      var r = await task;
      return await f(-r);
   }
   public static async Task<ValueExpected<TValue, TError>> AndThenAsync<TValue, TError>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TValue, Task<ValueExpected<TValue, TError>>> f) {
      var r = await task;
      return await r.AndThen(f);
   }
   public static async Task<ValueExpected<TResult, TError>> AndThenAsync<TValue, TError, TResult>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TValue, Task<ValueExpected<TResult, TError>>> f) {
      var r = await task;
      return await r.AndThen<TResult>(f);
   }
   public static async Task<ValueExpected<TValue, TError>> OrElseAsync<TValue, TError>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TError, Task<ValueExpected<TValue, TError>>> f) {
      var r = await task;
      return await r.OrElse(f);
   }
   public static async Task<ValueExpected<TValue, TResult>> OrElseAsync<TValue, TError, TResult>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TError, Task<ValueExpected<TValue, TResult>>> f) {
      var r = await task;
      return await r.OrElse<TResult>(f);
   }

   public static async Task<ValueExpected<TResult, TError>> Select<TValue, TError, TResult>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TValue, TResult> f) {
      var r = await task;
      return r.Select(f);
   }
   public static async Task<ValueExpected<TValue, TResult>> SelectError<TValue, TError, TResult>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TError, TResult> f) {
      var r = await task;
      return r.SelectError(f);
   }
   public static async Task<ValueExpected<TValue, TError>> AndThen<TValue, TError>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TValue, ValueExpected<TValue, TError>> f) {
      var r = await task;
      return r.AndThen(f);
   }
   public static async Task<ValueExpected<TResult, TError>> AndThen<TValue, TError, TResult>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TValue, ValueExpected<TResult, TError>> f) {
      var r = await task;
      return r.AndThen<TResult>(f);
   }
   public static async Task<ValueExpected<TValue, TError>> OrElse<TValue, TError>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TError, ValueExpected<TValue, TError>> f) {
      var r = await task;
      return r.OrElse(f);
   }
   public static async Task<ValueExpected<TValue, TResult>> OrElse<TValue, TError, TResult>(
       this Task<ValueExpected<TValue, TError>> task,
       Func<TError, ValueExpected<TValue, TResult>> f) {
      var r = await task;
      return r.OrElse<TResult>(f);
   }
   // ValueTask<ValueExpected<TValue, TError>>
   public static async Task<ValueExpected<TResult, TError>> SelectAsync<TValue, TError, TResult>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TValue, Task<TResult>> f) {
      var r = await task;
      return await f(+r);
   }
   public static async Task<ValueExpected<TResult, TError>> SelectErrorAsync<TValue, TError, TResult>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TError, Task<TResult>> f) {
      var r = await task;
      return await f(-r);
   }
   public static async Task<ValueExpected<TValue, TError>> AndThenAsync<TValue, TError>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TValue, Task<ValueExpected<TValue, TError>>> f) {
      var r = await task;
      return await r.AndThen(f);
   }
   public static async Task<ValueExpected<TResult, TError>> AndThenAsync<TValue, TError, TResult>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TValue, Task<ValueExpected<TResult, TError>>> f) {
      var r = await task;
      return await r.AndThen<TResult>(f);
   }
   public static async Task<ValueExpected<TValue, TError>> OrElseAsync<TValue, TError>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TError, Task<ValueExpected<TValue, TError>>> f) {
      var r = await task;
      return await r.OrElse(f);
   }
   public static async Task<ValueExpected<TValue, TResult>> OrElseAsync<TValue, TError, TResult>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TError, Task<ValueExpected<TValue, TResult>>> f) {
      var r = await task;
      return await r.OrElse<TResult>(f);
   }

   public static async Task<ValueExpected<TResult, TError>> Select<TValue, TError, TResult>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TValue, TResult> f) {
      var r = await task;
      return r.Select(f);
   }
   public static async Task<ValueExpected<TValue, TResult>> SelectError<TValue, TError, TResult>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TError, TResult> f) {
      var r = await task;
      return r.SelectError(f);
   }
   public static async Task<ValueExpected<TValue, TError>> AndThen<TValue, TError>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TValue, ValueExpected<TValue, TError>> f) {
      var r = await task;
      return r.AndThen(f);
   }
   public static async Task<ValueExpected<TResult, TError>> AndThen<TValue, TError, TResult>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TValue, ValueExpected<TResult, TError>> f) {
      var r = await task;
      return r.AndThen<TResult>(f);
   }
   public static async Task<ValueExpected<TValue, TError>> OrElse<TValue, TError>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TError, ValueExpected<TValue, TError>> f) {
      var r = await task;
      return r.OrElse(f);
   }
   public static async Task<ValueExpected<TValue, TResult>> OrElse<TValue, TError, TResult>(
       this ValueTask<ValueExpected<TValue, TError>> task,
       Func<TError, ValueExpected<TValue, TResult>> f) {
      var r = await task;
      return r.OrElse<TResult>(f);
   }
   public static async Task<ValueExpected<TResult, TError>> SelectAsync<TValue, TError, TResult>(
      this ValueExpected<TValue, TError> e,
      Func<TValue, Task<TResult>> f
   ) {
      if (e.HasValue) return await f(e._value);
      return new Unexpected<TError>(e._error);
   }
   public static async Task<ValueExpected<TValue, TResult>> SelectErrorAsync<TValue, TError, TResult>(
      this ValueExpected<TValue, TError> e,
      Func<TError, Task<TResult>> f
   ) {
      if (!e.HasValue) return new Unexpected<TResult>(await f(e._error));
      return e._value;
   }
   public static ValueTask<ValueExpected<TValue, TError>> AndThenAsync<TValue, TError>(
      this ValueExpected<TValue, TError> e,
      Func<TValue, Task<ValueExpected<TValue, TError>>> f
   ) {
      if (e.HasValue) return new(f(e._value));
      else return new(new Unexpected<TError>(e._error));
   }
   public static ValueTask<ValueExpected<TResult, TError>> AndThenAsync<TValue, TError, TResult>(
      this ValueExpected<TValue, TError> e,
      Func<TValue, Task<ValueExpected<TResult, TError>>> f
   ) {
      if (e.HasValue) return new(f(e._value));
      else return new(new Unexpected<TError>(e._error));
   }
   public static ValueTask<ValueExpected<TValue, TError>> OrElseAsync<TValue, TError>(
      this ValueExpected<TValue, TError> e,
      Func<TError, Task<ValueExpected<TValue, TError>>> f
   ) {
      if (!e.HasValue) return new(f(e._error));
      else return new(e._value);
   }
   public static ValueTask<ValueExpected<TValue, TResult>> OrElseAsync<TValue, TError, TResult>(
      this ValueExpected<TValue, TError> e,
      Func<TError, Task<ValueExpected<TValue, TResult>>> f
   ) {
      if (!e.HasValue) return new(f(e._error));
      else return new(e._value);
   }
}
