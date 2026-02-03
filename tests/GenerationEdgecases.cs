using Expected;
namespace Tests.Expected;

[Expected<object, float>]
partial struct ExpectedWithObject;

[Expected<IEquatable<float>, float>]
partial struct ExpectedWithInterface;

partial class Base;

[Expected<Base, float>]
partial class Derived : Base;

// [Expected<ShouldError, int>]
// partial struct ShouldError;

// static class Abc {
//    static ShouldError Err() => new ShouldError(new ShouldError(new Unexpected<int>(1)));
//    static ValueTask<ShouldError> ErrTask() => new(Err());
//    static async Task Xyz() {
//       await ErrTask();
//    }
// }
