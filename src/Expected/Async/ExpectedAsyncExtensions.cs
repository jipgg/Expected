namespace Expected.Async;

public static class ExpectedAsyncExtensions {
   public static async Task<Expected<TResult, TError>> SelectAsync<TValue, TError, TResult>(
       this Task<Expected<TValue, TError>> task,
       Func<TValue, Task<TResult>> f) {
      var r = await task;
      return await f(+r);
   }
   public static async Task<Expected<TResult, TError>> SelectErrorAsync<TValue, TError, TResult>(
       this Task<Expected<TValue, TError>> task,
       Func<TError, Task<TResult>> f) {
      var r = await task;
      return await f(-r);
   }
   public static async Task<Expected<TValue, TError>> AndThenAsync<TValue, TError>(
       this Task<Expected<TValue, TError>> task,
       Func<TValue, Task<Expected<TValue, TError>>> f) {
      var r = await task;
      return await r.AndThen(f);
   }
   public static async Task<Expected<TResult, TError>> AndThenAsync<TValue, TError, TResult>(
       this Task<Expected<TValue, TError>> task,
       Func<TValue, Task<Expected<TResult, TError>>> f) {
      var r = await task;
      return await r.AndThen<TResult>(f);
   }
   public static async Task<Expected<TValue, TError>> OrElseAsync<TValue, TError>(
       this Task<Expected<TValue, TError>> task,
       Func<TError, Task<Expected<TValue, TError>>> f) {
      var r = await task;
      return await r.OrElse(f);
   }
   public static async Task<Expected<TValue, TResult>> OrElseAsync<TValue, TError, TResult>(
       this Task<Expected<TValue, TError>> task,
       Func<TError, Task<Expected<TValue, TResult>>> f) {
      var r = await task;
      return await r.OrElse<TResult>(f);
   }

   public static async Task<Expected<TResult, TError>> Select<TValue, TError, TResult>(
       this Task<Expected<TValue, TError>> task,
       Func<TValue, TResult> f) {
      var r = await task;
      return r.Select(f);
   }
   public static async Task<Expected<TValue, TResult>> SelectError<TValue, TError, TResult>(
       this Task<Expected<TValue, TError>> task,
       Func<TError, TResult> f) {
      var r = await task;
      return r.SelectError(f);
   }
   public static async Task<Expected<TValue, TError>> AndThen<TValue, TError>(
       this Task<Expected<TValue, TError>> task,
       Func<TValue, Expected<TValue, TError>> f) {
      var r = await task;
      return r.AndThen(f);
   }
   public static async Task<Expected<TResult, TError>> AndThen<TValue, TError, TResult>(
       this Task<Expected<TValue, TError>> task,
       Func<TValue, Expected<TResult, TError>> f) {
      var r = await task;
      return r.AndThen<TResult>(f);
   }
   public static async Task<Expected<TValue, TError>> OrElse<TValue, TError>(
       this Task<Expected<TValue, TError>> task,
       Func<TError, Expected<TValue, TError>> f) {
      var r = await task;
      return r.OrElse(f);
   }
   public static async Task<Expected<TValue, TResult>> OrElse<TValue, TError, TResult>(
       this Task<Expected<TValue, TError>> task,
       Func<TError, Expected<TValue, TResult>> f) {
      var r = await task;
      return r.OrElse<TResult>(f);
   }
   // ValueTask<Expected<TValue, TError>>
   public static async Task<Expected<TResult, TError>> SelectAsync<TValue, TError, TResult>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TValue, Task<TResult>> f) {
      var r = await task;
      return await f(+r);
   }
   public static async Task<Expected<TResult, TError>> SelectErrorAsync<TValue, TError, TResult>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TError, Task<TResult>> f) {
      var r = await task;
      return await f(-r);
   }
   public static async Task<Expected<TValue, TError>> AndThenAsync<TValue, TError>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TValue, Task<Expected<TValue, TError>>> f) {
      var r = await task;
      return await r.AndThen(f);
   }
   public static async Task<Expected<TResult, TError>> AndThenAsync<TValue, TError, TResult>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TValue, Task<Expected<TResult, TError>>> f) {
      var r = await task;
      return await r.AndThen<TResult>(f);
   }
   public static async Task<Expected<TValue, TError>> OrElseAsync<TValue, TError>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TError, Task<Expected<TValue, TError>>> f) {
      var r = await task;
      return await r.OrElse(f);
   }
   public static async Task<Expected<TValue, TResult>> OrElseAsync<TValue, TError, TResult>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TError, Task<Expected<TValue, TResult>>> f) {
      var r = await task;
      return await r.OrElse<TResult>(f);
   }

   public static async Task<Expected<TResult, TError>> Select<TValue, TError, TResult>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TValue, TResult> f) {
      var r = await task;
      return r.Select(f);
   }
   public static async Task<Expected<TValue, TResult>> SelectError<TValue, TError, TResult>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TError, TResult> f) {
      var r = await task;
      return r.SelectError(f);
   }
   public static async Task<Expected<TValue, TError>> AndThen<TValue, TError>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TValue, Expected<TValue, TError>> f) {
      var r = await task;
      return r.AndThen(f);
   }
   public static async Task<Expected<TResult, TError>> AndThen<TValue, TError, TResult>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TValue, Expected<TResult, TError>> f) {
      var r = await task;
      return r.AndThen<TResult>(f);
   }
   public static async Task<Expected<TValue, TError>> OrElse<TValue, TError>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TError, Expected<TValue, TError>> f) {
      var r = await task;
      return r.OrElse(f);
   }
   public static async Task<Expected<TValue, TResult>> OrElse<TValue, TError, TResult>(
       this ValueTask<Expected<TValue, TError>> task,
       Func<TError, Expected<TValue, TResult>> f) {
      var r = await task;
      return r.OrElse<TResult>(f);
   }

   public static async Task<Expected<TResult, TError>> SelectAsync<TValue, TError, TResult>(
      this Expected<TValue, TError> e,
      Func<TValue, Task<TResult>> f
   ) {
      if (e.HasValue) return await f(e._value);
      return new Unexpected<TError>(e._error);
   }
   public static async Task<Expected<TValue, TResult>> SelectErrorAsync<TValue, TError, TResult>(
      this Expected<TValue, TError> e,
      Func<TError, Task<TResult>> f
   ) {
      if (!e.HasValue) return new Unexpected<TResult>(await f(e._error));
      return e._value;
   }
   public static ValueTask<Expected<TValue, TError>> AndThenAsync<TValue, TError>(
      this Expected<TValue, TError> e,
      Func<TValue, Task<Expected<TValue, TError>>> f
   ) {
      if (e.HasValue) return new(f(e._value));
      else return new(new Unexpected<TError>(e._error));
   }
   public static ValueTask<Expected<TResult, TError>> AndThenAsync<TValue, TError, TResult>(
      this Expected<TValue, TError> e,
      Func<TValue, Task<Expected<TResult, TError>>> f
   ) {
      if (e.HasValue) return new(f(e._value));
      else return new(new Unexpected<TError>(e._error));
   }
   public static ValueTask<Expected<TValue, TError>> OrElseAsync<TValue, TError>(
      this Expected<TValue, TError> e,
      Func<TError, Task<Expected<TValue, TError>>> f
   ) {
      if (!e.HasValue) return new(f(e._error));
      else return new(e._value);
   }
   public static ValueTask<Expected<TValue, TResult>> OrElseAsync<TValue, TError, TResult>(
      this Expected<TValue, TError> e,
      Func<TError, Task<Expected<TValue, TResult>>> f
   ) {
      if (!e.HasValue) return new(f(e._error));
      else return new(e._value);
   }
}
