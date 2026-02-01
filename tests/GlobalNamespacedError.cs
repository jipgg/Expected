using Expected;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Numerics;
namespace Tests;

partial struct Result: IExpected<Result, float, int>;

[Expected<int, int>]
readonly partial struct Same;

partial struct Same<T>: IExpected<Same<T>, T, T>;
readonly partial struct Result<T, E>: IExpected<Result<T, E>, T, E>
where T: class
where E: class;


public static class Abc {
   [Fact]
   static void Xyz() {
      Result result = 0;

      Result<string, Exception> result1 = "eeee";
      Same s = 1;
      Same<int> same = 1;
      Console.Write(result1.Value);
   }
}
