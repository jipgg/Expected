using static Expected.UnexpectedFunction;
using System.Runtime.InteropServices;
using Expected;
namespace Tests;

record ObjFoo(int X);
record ObjBar(string? Msg);

record TestObject(int Value);
readonly record struct TestError(string Message);
readonly record struct TestValue(int Value);
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
