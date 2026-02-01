using Expected;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Numerics;
namespace Tests;

[Expected<int, int>]
readonly partial struct Same<T>;

[Expected]
partial struct Result<T, E> where T : class where E : Exception {
   public static implicit operator Result<T>(Result<T, E> r) => r ? new(+r) : new(default, -r);
   public static implicit operator Result(Result<T, E> r) => r ? new(+r) : new(default, -r);
}
[Unexpected<Exception>]
partial struct Result<T> where T : class {
   public static implicit operator Result(Result<T> r) => r ? new(+r) : new(default, -r);
   public static implicit operator Result<T, Exception>(Result<T> r) => r ? new(+r) : new(default, -r);
}
[Expected<object, Exception>]
partial struct Result {
   public static implicit operator Result<object, Exception>(Result r) => r ? new(+r) : new(default, -r);
   public static implicit operator Result<object>(Result r) => r ? new(+r) : new(default, -r);
}

public static class Abc {
   [Fact]
   static void Xyz() {
      Result<string, Exception> result1 = "eeee";
      Result result = result1;
      result1 = result.Select(static (in v) => (string)v);
      // Same s = 1;
      Same<int> same = 1;
      Console.Write(result1.Value);
   }
}
