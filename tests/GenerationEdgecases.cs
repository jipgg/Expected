using Expected;
using System.Numerics;
namespace Tests.Expected;

[Expected<object, float>]
partial struct ExpectedWithObject;

[Expected<IEquatable<float>, float>]
partial struct ExpectedWithInterface;

partial class Base;

[Expected<Base, float>]
partial class Derived : Base;

// [Expected<object, int>]
// partial struct ShouldError;

[Expected<int, ErrorCode>]
[Unexpected<ErrorCode>]
partial class MyResult<T>;
[ErrorCode]
enum MyError { SomeErrorValue, SomeOtherErrorValue }

[ErrorCode(MessageImplOptions.FullName)]
enum TakeError { AmountIsNegative, SpanTooSmall }

readonly ref struct NotTaken(TakeError error, Span<int> span, int amount) {
   public readonly Span<int> Span = span;
   public readonly TakeError Error = error;
   public readonly int Amount = amount;
}
[Expected<Span<int>, NotTaken>]
readonly ref partial struct Taken;

[ErrorCode]
enum OutOfRange : byte;

[Unexpected<OutOfRange>]
ref partial struct Squared<T> where T : allows ref struct;

[Unexpected<Exception>]
partial record Result<T>;
[Expected]
record Result<T, E> where E: Exception;

static class Abc {
   static Taken Take(Span<int> span, int amount) {
      if (amount < 0) {
         return new(default, new(TakeError.AmountIsNegative, span, amount));
      } else if (span.Length < amount) {
         return new Unexpected<NotTaken>(new(TakeError.SpanTooSmall, span, amount));
      }
      return span[..amount];
   }
   static Squared<Span<T>> Square<T>(Span<T> span, Range range) where T : INumber<T> {
      var start = range.Start.GetOffset(span.Length);
      var end = range.End.GetOffset(span.Length);
      if (start > span.Length || end < 0 || end > span.Length) {
         return new Unexpected<OutOfRange>();
      }
      foreach (ref var e in span[range]) e *= e;
      return span;
   }
   static Expected<ReadOnlySpan<int>, ErrorCode> DoSomeStuff(Span<int> data) {
      return Take(data, 4)
         .AndThen(span => Take(span, 8))
         .OrElse((scoped in err) => err.Error switch {
            TakeError.AmountIsNegative => Take(err.Span, -err.Amount),
            TakeError.SpanTooSmall => err.Span,
            _ => throw new InvalidOperationException(),
         })
         .SelectError(err => err.Error.AsCode())
         .AndThen(span => Square(span, 1..10)
            .SelectError(err => err.AsCode())
         .Select<ReadOnlySpan<int>>(span => span));
   }
   static Result<int[]> DoSomeOtherStuff(ReadOnlySpan<int> data) {
      var result = DoSomeStuff([.. data]);
      if (!result) {
         var message = result.Error.Message;
         return new Unexpected<Exception>(new(message));
      }
      return new([.. +result]);
   }
}
