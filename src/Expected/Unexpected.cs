namespace Expected;

public static class UnexpectedFunction {
   [MethodImpl(AggressiveInlining)]
   public static Unexpected<TError> Unexpected<TError>(scoped in TError error) where TError : allows ref struct => new(in error);
   [MethodImpl(AggressiveInlining)]
   public static Unexpected<TError> Unexpected<TError>(TError error) where TError : allows ref struct => new(error);
}

public readonly ref struct Unexpected<TError> where TError : allows ref struct {
   public TError Error { get; }
   [MethodImpl(AggressiveInlining)]
   public Unexpected(scoped in TError error) => Error = error;
   [MethodImpl(AggressiveInlining)]
   public Unexpected(TError error) => Error = error;
}
