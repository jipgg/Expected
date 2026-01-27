using Expected;

[ErrorCode(MessageImpl = MessageImplOptions.FullName)]
public enum GlobalNamespacedError { A, B }

[Expected]
public partial struct GlobalExpected<V, E>;

namespace Named {
   [Expected]
   readonly partial struct ValueExpected<V, E>;

   [Expected]
   partial struct Result<V> : IExpectedTypeArguments<V, Exception>;

   [ErrorCode]
   public enum NamedError { A, B }
}

file static class Abc {
   static void Xyz() {
      Named.ValueExpected<int, float> e = 1;
   }
}
