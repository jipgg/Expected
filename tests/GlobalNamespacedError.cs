using Expected;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Numerics;
interface IHasPosition
{
    int X { get; }
    int Y { get; }
    (int X, int Y) Position => (X, Y);
}
static class HasPosition {
   extension<T>(T self) where T: IHasPosition {
      public (int X, int Y) Position => self.Position;
   }
}
interface IVirtual {
   int Value { get; }
   int Virtual => Value;
}
interface IStatic<T> where T : IStatic<T> {
   int Value { get; }
   static virtual int Static(T self) => self.Value;
}
readonly struct WithOverrides(int value) : IStatic<WithOverrides>, IVirtual {
   public int Value => value;
   public int Virtual => Value;
   public static int Static(WithOverrides self) => 123;
}
readonly struct WithDefaults(int value) : IStatic<WithDefaults>, IVirtual {
   public int Value => value;
}

record struct Something(int X, int Y): IHasPosition;
record SomethingClass(int X, int Y): IHasPosition;
// struct SomethingPositionGetterImpl : IHasPosition<Something> {
//    public static (int, int) Position(in Something self) => (self.X, self.Y);
//    public static (int, int) DynPosition(IDynHasPosition self) => Position((Something)self);
//    // public static (int, int) Position(in Something self) => (self.X, self.Y);
//    // public static Func<object, (int X, int Y)> Dyn => static o => {
//    //    return Func
//    // };
// }
// static class Ext {
//    extension(Something self) {
//    }
// }

[ErrorCode(MessageImpl = MessageImplOptions.FullName)]
public enum GlobalNamespacedError { A, B }

readonly struct NullError;

[Expects<NullError>]
sealed partial class Errorable<TError>;

partial struct Result<T, E>: IExpected<Result<T, E>, T, E>
where E: Exception;
partial struct Result<T>: IExpected<Result<T>, T, Exception>;

partial struct Mine: IExpected<Mine, int, float>;

record DomainSpecificError;

[Unexpects<DomainSpecificError>]
partial struct DomainSpecificResult<T>;

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
            .Select(e => 1))
      .SelectError(e => e.InnerException)
      .Select<float>(static (scoped in v) => v);
   static void Xyz() {
      Errorable<Exception> errorable = default(NullError);
      MyExpected<int> exp = new([]);
      Named.ValueExpected<int> e = 1;
      var s = new Something(1, 2);
      var c = new SomethingClass(1, 2);
      DomainSpecificResult<float> result = 1;
      int[] span = [1, 2, 3];
      e = e.SelectError(static (in e) => 1)
         .SelectError((in e) => e.ToString());
   }
}
