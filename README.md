# Expected
## Introduction
As the repository name implies, this is strongly inspired by C++23's `std::expected` abstraction.
`Expected` is like any other result monad/error-by-value abstraction
that you have probably seen floating around in the C# ecosystem.
The main idea is, with the use of implicit conversions that elegantly translates from C++ to C#,
this result type tries to keep the 'happy path' as non-intrusive as possible, similarly to exceptions,
while keeping the error path branches explicit and non-ambiguous.

A quick, trivial parallel to exceptions:
```cs
// by value 
Expected<int, string> Divide(int a, int b) {
    if (b is 0) return new Unexpected<string>("Cannot divide by zero.");
    return a / b; 
}
// by throwing
int Divide(int a, int b) {
    if (b is 0) throw new Exception("Cannot divide by zero.");
    return a / b;
}
```
This is not meant to replace exceptions, but rather to be a lighter,
error-by-value alternative to exceptions where they deem fit.
## Feature set
- `Expected<V, E>` and `ValueExpected<V, E>` type 
- Source generators for custom expected type variants
- `ErrorCode` as a lightweight polymorphic error type, mainly useful for when working with enums as errors.
- Analyzer for checking whether a potentially unexpected result is unused

## Install
[nuget link](https://www.nuget.org/packages/Expected/)
```sh
dotnet add package Expected
```

## Getting started
Code example:
```cs
using Expected;

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
```
### Expected
The main idea is that you have a 'canonical' `Expected<V, E>` type, a ref struct, to which all variants are implicitly convertible to and from.
```cs

[Unexpected<SomeDomainException>]
partial struct SomeDomainResult<T>;

[Expected<NoError>]
partial struct Errorable<E>;

[Expected<ReadOnlySpan<byte>, ErrorCode>]
readonly ref partial struct MyExpected;

[Expected]
partial class MyExpected<T, E> where E: Exception;

partial struct MarkedExpected<T> : ISourceGeneratedExpectedMarker<List<T>, string>;
// more verbose option for finetuned control over nesting of type parameters

```
How the type gets generated depends on the specified keywords, generic type arguments and generic constraints.
The generated types will be implicitly convertible to and from their corresponding 'canonical' type:
- `ref struct` to `RefExpected<TValue, TError>`
- `struct` to `ValueExpected<TValue, TError>`
- `class` to `Expected<TValue, TError>`
### ErrorCode
The idea of this type is quite simple,
store both an `int` as the error code value and a reference to a polymorphic `ErrorCategory` singleton
to allow for a lightweight error object with some additional information and semantics.
The library ships with a source generator for generating the typical boilerplate that comes with this
type of error handling, but ErrorCategories can be created manually if neccessary:
```cs
using Expected;

public enum MySpecialError { A, B, C }
public sealed class MySpecialErrorCategory : ErrorCategory {
   public override string GetMessage(int errorCode)
      => (MySpecialError)errorCode switch {
         MySpecialError.A => "My special message A",
         MySpecialError.B => "My special message B",
         MySpecialError.C => "My special message C",
         _ => "An unknown error occurred.",
      };
   public override string Title => "My special error";
};
public static class MySpecialErrorExtensions {
   static readonly MySpecialErrorCategory _category = new();
   public static ErrorCode AsCode(this MySpecialError err)
      => new((int)err, _category);
}
```
The typical target for an ErrorCode will be an enum, hence why i've added some
possibilities for source generating this boilerplate:
```cs
using Expected;

[ErrorCode(MessageImpl = MessageImplOptions.FullName)]
public enum FullError { Something, SomethingElse }

[ErrorCode(Title = "I am an error.")]
public enum IAmAnError { Something, SomethingElse }

[ErrorCode(Title = "partial", MessageImpl = MessageImplOptions.Partial)]
public enum PartialError { Something, SomethingElse }
partial class PartialErrorCategory {
   public override string GetMessage(int errorCode)
      => (PartialError)errorCode switch {
         PartialError.Something => "Something happened.",
         PartialError.SomethingElse => "Something else happened.",
         _ => throw new Unreachable(),
      };
}
```
If you are targeting .NET10, static extension properties are automatically added
to `ErrorCode` and `ErrorCategory` for easy discovery:
```cs
[ErrorCode]
public enum MyError { MyErrorValue }

var code = ErrorCode.MyErrorValue;
var category = ErrorCategory.MyError;
```
Switch expressions also remain roughly comparable to plain enums in terms of ergonomics
by switching over the categories first:
```cs
var v = errorCode.Category switch {
    FullErrorCategory => (FullError)errorCode.Value switch {
        FullError.Something => "abc",
        FullError.SomethingElse => "xyz",
        _ => "full error",
    },
    PartialErrorCategory => errorCode.Message,
    IAmAnErrorCategory => throw new(),
    _ => errorCode.ToString(),
};
```
> ErrorCodes are not meant to replace Exceptions or other class based error objects.
> They serve as a means to modularize a multitude of static error values (like enums)
> and provide a lightweight polymorphic interface for them.

