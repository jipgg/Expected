# Expected
## Introduction
As the repository name implies, this is strongly inspired by C++23's `std::expected` abstraction as well as `std::error_code`.
`Expected` is like any other result monad/error-by-value abstraction
that you have probably seen floating around in the C# ecosystem.
The main idea is, with the use of implicit conversions that elegantly translates from C++ to C#,
this result type tries to keep the 'happy path' as non-intrusive as possible, similarly to exceptions,
while keeping the error path branches explicit and non-ambiguous.

A quick, trivial parallel to exceptions:
```cs
// by value 
ValueExpected<int, Exception> Divide(int a, int b) {
    if (b is 0) return new Unexpected<Exception>(new("Cannot divide by zero."));
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
- `Expected` type variants
- `ErrorCode` as a lightweight polymorphic error type, mainly useful for when working with enums as errors
- Analyzer for checking whether a potential unexpected value is unused
- Source generators for common boilerplate code

## Install
[nuget link](https://www.nuget.org/packages/Expected/)
```sh
dotnet package add Expected
```

## Getting started
A trivial code example:
```cs
using Expected;
using static Expected.UnexpectedFunction;

static Expected<int, MyError> DoSometing(bool isExpected) {
    if (isExpected) return 10;
    else return new Unexpected<MyError>(MyError.SomeErrorValue);
}
static Expected<string, MyError> DoOtherThing(int value)
    => $"something else {value}";

static MyResult<string> HandleImperatively() {
    var did = DoSometing(false);
    if (!did) return Unexpected(did.Error.AsCode());
    var didOther = DoOtherThing(+did);
    if (!didOther) return Unexpected(ErrorCode.SomeOtherErrorValue);
    r"eturn +didOther;
}
static MyResult<string> HandleDeclaratively()
    => DoSometing(false)
        .AndThen(DoOtherThing)
        .SelectError(static err => err.AsCode());

[Expected(TError = nameof(ErrorCode))]
partial class MyResult<T>;
[ErrorCode]
enum MyError { SomeErrorValue, SomeOtherErrorValue }
```
### Expected
The library ships with 3 variations of expected types:
- `Expected`: general-purpose class variant
- `ValueExpected`: struct variant
- `RefExpected`: ref struct variant

but you can also generate a new expected type
with your own custom constraints or logic like in the following examples:
```cs
using Expected;

[Expected(TValue = "System.Span<char>")]
ref partial struct MySpanExpected<TError>
    where TError: allows ref struct;

[Expected]
partial class MyExpected<T, E> where E: Exception;

[Expected]
readonly partial struct MyReadonlyExpected<T>
: IExpectedTypeArguments<List<T>, string>;
// serves as a tag for binding generic type arguments
// especially useful for non-trivial type specializations (i.e. nested)

```
How the type gets generated depends on the specified keywords, generic type arguments and generic constraints.
The generated types will be implicitly convertible to and from their corresponding 'canonical' type:
- `ref struct`s to `RefExpected<TValue, TError>`
- `struct`s to `ValueExpected<TValue, TError>`
- `class`es to `Expected<TValue, TError>`
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

