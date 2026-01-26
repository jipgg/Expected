namespace Expected.Generators.Templates;

static class ExpectedAsyncExtendedTemplate {
   public static string Apply(ExpectedAsyncExtensionsParams args) {
      var (@namespace, type) = args;
      return $$"""
      using System.Threading.Tasks;
      namespace {{@namespace}};
      public static class {{type}}AsyncExtensions {
         public static async Task<{{type}}<TResult, TError>> SelectAsync<TValue, TError, TResult>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TValue, Task<TResult>> f) {
            var r = await task;
            return await f(+r);
         }
         public static async Task<{{type}}<TResult, TError>> SelectErrorAsync<TValue, TError, TResult>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TError, Task<TResult>> f) {
            var r = await task;
            return await f(-r);
         }
         public static async Task<{{type}}<TValue, TError>> AndThenAsync<TValue, TError>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TValue, Task<{{type}}<TValue, TError>>> f) {
            var r = await task;
            return await r.AndThen(f);
         }
         public static async Task<{{type}}<TResult, TError>> AndThenAsync<TValue, TError, TResult>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TValue, Task<{{type}}<TResult, TError>>> f) {
            var r = await task;
            return await r.AndThen<TResult>(f);
         }
         public static async Task<{{type}}<TValue, TError>> OrElseAsync<TValue, TError>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TError, Task<{{type}}<TValue, TError>>> f) {
            var r = await task;
            return await r.OrElse(f);
         }
         public static async Task<{{type}}<TValue, TResult>> OrElseAsync<TValue, TError, TResult>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TError, Task<{{type}}<TValue, TResult>>> f) {
            var r = await task;
            return await r.OrElse<TResult>(f);
         }

         public static async Task<{{type}}<TResult, TError>> Select<TValue, TError, TResult>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TValue, TResult> f) {
            var r = await task;
            return r.Select(f);
         }
         public static async Task<{{type}}<TValue, TResult>> SelectError<TValue, TError, TResult>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TError, TResult> f) {
            var r = await task;
            return r.SelectError(f);
         }
         public static async Task<{{type}}<TValue, TError>> AndThen<TValue, TError>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TValue, {{type}}<TValue, TError>> f) {
            var r = await task;
            return r.AndThen(f);
         }
         public static async Task<{{type}}<TResult, TError>> AndThen<TValue, TError, TResult>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TValue, {{type}}<TResult, TError>> f) {
            var r = await task;
            return r.AndThen<TResult>(f);
         }
         public static async Task<{{type}}<TValue, TError>> OrElse<TValue, TError>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TError, {{type}}<TValue, TError>> f) {
            var r = await task;
            return r.OrElse(f);
         }
         public static async Task<{{type}}<TValue, TResult>> OrElse<TValue, TError, TResult>(
            this Task<{{type}}<TValue, TError>> task,
            Func<TError, {{type}}<TValue, TResult>> f) {
            var r = await task;
            return r.OrElse<TResult>(f);
         }

         public static async Task<{{type}}<TResult, TError>> SelectAsync<TValue, TError, TResult>(
            this {{type}}<TValue, TError> e,
            Func<TValue, Task<TResult>> f
         ) {
            if (e.HasValue) return await f(e._value);
            return new Unexpected<TError>(e._error);
         }
         public static async Task<{{type}}<TValue, TResult>> SelectErrorAsync<TValue, TError, TResult>(
            this {{type}}<TValue, TError> e,
            Func<TError, Task<TResult>> f
         ) {
            if (!e.HasValue) return new Unexpected<TResult>(await f(e._error));
            return e._value;
         }
         public static ValueTask<{{type}}<TValue, TError>> AndThenAsync<TValue, TError>(
            this {{type}}<TValue, TError> e,
            Func<TValue, Task<{{type}}<TValue, TError>>> f
         ) {
            if (e.HasValue) return new(f(e._value));
            else return new(new Unexpected<TError>(e._error));
         }
         public static ValueTask<{{type}}<TResult, TError>> AndThenAsync<TValue, TError, TResult>(
            this {{type}}<TValue, TError> e,
            Func<TValue, Task<{{type}}<TResult, TError>>> f
         ) {
            if (e.HasValue) return new(f(e._value));
            else return new(new Unexpected<TError>(e._error));
         }
         public static ValueTask<{{type}}<TValue, TError>> OrElseAsync<TValue, TError>(
            this {{type}}<TValue, TError> e,
            Func<TError, Task<{{type}}<TValue, TError>>> f
         ) {
            if (!e.HasValue) return new(f(e._error));
            else return new(e._value);
         }
         public static ValueTask<{{type}}<TValue, TResult>> OrElseAsync<TValue, TError, TResult>(
            this {{type}}<TValue, TError> e,
            Func<TError, Task<{{type}}<TValue, TResult>>> f
         ) {
            if (!e.HasValue) return new(f(e._error));
            else return new(e._value);
         }
      }
      """;
   }
}
