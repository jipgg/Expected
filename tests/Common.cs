using static Expected.UnexpectedFunction;
using System.Runtime.InteropServices;
using Expected;
namespace Tests;

record ObjFoo(int X);
record ObjBar(string? Msg);

readonly record struct Foo(int X);
readonly record struct Bar(string? Msg);

readonly ref struct RefFoo {
   public readonly int X;
   public RefFoo(int x) => X = x;
}

readonly ref struct RefBar {
   public readonly string Msg;
   public RefBar(string msg) => Msg = msg;
}

[ErrorCode(Title = "yo")]
public enum YoError {
   Something,
   SomethingElse,
   Abc,
   Xyz,
}
[Expected]
partial struct Abc<T> : IExpectedTypeArguments<string, T>;

[Expected(TError = nameof(ErrorCode))]
readonly partial struct Result<T>;

public static class X {
   static Expected<int, float> DoSomething() => 10;
   static void E() {
      Abc<int> abc = "eeee";
      Result<int> result = 1;
      YoError.Something.AsCode();
   }
}
