namespace Expected;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
public sealed class CouldBeUnexpectedAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class UnexpectedAttribute<TError>: Attribute
   where TError: allows ref struct;


public static class UnexpectedFunction {
   [MethodImpl(AggressiveInlining)]
   public static Unexpected<TError> Unexpected<TError>(scoped in TError error) where TError : allows ref struct => new(in error);
   [MethodImpl(AggressiveInlining)]
   public static Unexpected<TError> Unexpected<TError>(TError error) where TError : allows ref struct => new(error);
}

[CouldBeUnexpected]
public readonly ref struct Unexpected<TError> where TError : allows ref struct {
   public TError Error { get; }
   [MethodImpl(AggressiveInlining)]
   public Unexpected(scoped in TError error) => Error = error;
   [MethodImpl(AggressiveInlining)]
   public Unexpected(TError error) => Error = error;
}
