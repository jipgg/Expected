namespace Expected;

public sealed class Unreachable(string message = "This code should be unreachable."): InvalidOperationException(message);

public abstract class ErrorCategory {
   public abstract string Title { get; }
   public abstract string GetMessage(int errorCode);
}
public readonly record struct ErrorCode(int Value, ErrorCategory Category) {
   public string Message => Category.GetMessage(Value);
}
