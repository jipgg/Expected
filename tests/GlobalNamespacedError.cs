using Expected;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Numerics;

[ErrorCode(MessageImpl = MessageImplOptions.FullName)]
public enum GlobalNamespacedError { A, B }

readonly struct NullError;
record DomainSpecificError;

[Expects<NullError>]
sealed partial class Errorable<TError>;

[Unexpects<DomainSpecificError>]
partial record DomainSpecificResult<T>;

[Expects<int>]
[Unexpects<float>]
sealed partial class Ex;

readonly partial record struct SomeResult : IExpected<SomeResult, int, float>;

sealed partial class Ex<T, E> : IExpected<Ex<T, E>, T, E>;

partial struct Result<T, E> : IExpected<Result<T, E>, T, E> where E : Exception {
   public static implicit operator Result<T>(Result<T, E> r) => r ? new(+r) : new(default, -r);
}
partial struct Result<T> : IExpected<Result<T>, T, Exception>;


partial struct MyExpected<T> : IExpected<MyExpected<T>, List<T>, Exception>;

namespace Named {
   [Unexpects<string>]
   readonly partial struct ValueExpected<V>;

   [ErrorCode]
   public enum NamedError { A, B }

}

file static class Abc {
   static Result<float, InvalidOperationException> DoSomething() => 1;
   static Result<string, InvalidOperationException> DoSomethingString() => "";
   static Result<float> DoSomethingElse() => DoSomething()
      .AndThen<int>(static v => DoSomethingString()
         .Select(static e => 1))
      .SelectError(e => e.InnerException)
      .Select<float>(static (scoped in v) => v);
   static void Xyz() {
      Result<int, InvalidOperationException> res2 = 1;
      Result<int> res = res2;
      res2 = res.SelectError(e => (InvalidOperationException)e);
      Ex<int, Exception> e = 1;
      SomeResult rrr = 1;
      Errorable<Exception> errorable = default(NullError);
      MyExpected<int> exp = new([]);
      Named.ValueExpected<int> exx = 1;
      DomainSpecificResult<float> result = 1;
      int[] span = [1, 2, 3];
      exx = exx.SelectError(static (in e) => 1)
         .SelectError((in e) => e.ToString());
   }
}
