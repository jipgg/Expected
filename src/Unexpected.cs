namespace Expected;

public static class UnexpectedFunction {
   [MethodImpl(AggressiveInlining)]
   public static Unexpected<E> Unexpected<E>(scoped in E error) where E : allows ref struct => new(in error);
   [MethodImpl(AggressiveInlining)]
   public static Unexpected<E> Unexpected<E>(E error) where E : allows ref struct => new(error);
}

public readonly ref struct Unexpected<E> where E : allows ref struct {
   public E Error { get; }
   [MethodImpl(AggressiveInlining)]
   public Unexpected(scoped in E error) => Error = error;
   [MethodImpl(AggressiveInlining)]
   public Unexpected(E error) => Error = error;

}
