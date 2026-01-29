namespace Expected;

public sealed class BadExpectedAccess : InvalidOperationException;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class ExpectedAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class ExpectedAttribute<TValue>: Attribute
   where TValue: allows ref struct;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class ExpectedAttribute<TValue, TError>: Attribute
   where TValue: allows ref struct
   where TError: allows ref struct;

public interface ISourceGeneratedExpectedMarker<TValue, TError>
   where TValue : allows ref struct
   where TError : allows ref struct;

[Expected, IsCanonical, ExpectedAsyncExtended]
public sealed partial class Expected<TValue, TError>;

[Expected, IsCanonical, ExpectedAsyncExtended]
public partial struct ValueExpected<TValue, TError> {
   public readonly Expected<TValue, TError> AsExpected()
      => _hasValue ? new(_value) : new(new Unexpected<TError>(_error));
}
[Expected, IsCanonical]
public ref partial struct RefExpected<TValue, TError>
   where TValue : allows ref struct
   where TError : allows ref struct;
