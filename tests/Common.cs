namespace Tests;

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
