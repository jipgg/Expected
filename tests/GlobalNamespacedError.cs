using Expected;

[ErrorCode(MessageImpl = MessageImplOptions.FullName)]
public enum GlobalNamespacedError { A, B }

readonly struct NullError;

[Expected<NullError>]
sealed partial class Errorable<TError>;

[Expected<int, string>]
partial struct Result;

record DomainSpecificError;

[Unexpected<DomainSpecificError>]
partial struct DomainSpecificResult<T>;

partial struct MyExpected<T> : ISourceGeneratedExpectedMarker<List<T>, Exception>;

namespace Named {
   [Unexpected<string>]
   readonly partial struct ValueExpected<V>;

   [ErrorCode]
   public enum NamedError { A, B }

}

file static class Abc {
   static void Xyz() {
      Errorable<Exception> errorable = default(NullError);
      MyExpected<int> exp = new([]);
      Named.ValueExpected<int> e = 1;
      DomainSpecificResult<float> result = 1;
      e = e.SelectError(e => 1).SelectError(e => e.ToString());
   }
}
