using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Expected;
namespace Expected.Benchmarks;

public enum Mode {
   HappyPath,
   ErrorPath,
   Alternative,
}

sealed class AlternativeException<T>(T value) : Exception {
   public T Value => value;
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
[SimpleJob(RuntimeMoniker.NativeAot10_0)]
public class ThrowVersusReturn {

   [Params(Mode.HappyPath, Mode.ErrorPath)]
   public Mode Mode;

   static int ThrowException(Mode v) {
      switch (v) {
         case Mode.Alternative:
            throw new AlternativeException<int>(10);
         case Mode.ErrorPath:
            throw new Exception();
      }
      return 1;
   }

   static Expected<int, Exception> ReturnException(Mode v) {
      switch (v) {
         case Mode.Alternative:
            return new(default, new AlternativeException<int>(10));
         case Mode.ErrorPath:
            return new Unexpected<Exception>(new());
      }
      return 1;
   }

   static ValueExpected<int, int?> ReturnAlternative(Mode v) {
      switch (v) {
         case Mode.Alternative:
            return new Unexpected<int?>(10);
         case Mode.ErrorPath:
            return new Unexpected<int?>(null);
      }
      return 1;
   }
   [Benchmark(Description = "int or throw")]
   public int Throwing() {
      try {
         return ThrowException(Mode);
      } catch (AlternativeException<int> a) {
         return a.Value;
      } catch (Exception) {
         return -1;
      }
   }
   [Benchmark(Description = "Expected<int, Exception>")]
   public int Returning() {
      var result = ReturnException(Mode);
      if (result) return +result;
      return -result switch {
         AlternativeException<int> a => a.Value,
         Exception => -1
      };
   }
   [Benchmark(Description = "Expected<int, int?>")]
   public int Alternative() {
      var r = ReturnAlternative(Mode);
      if (r) return +r;
      return -r switch {
         int value => value,
         null => -1,
      };
   }
}
