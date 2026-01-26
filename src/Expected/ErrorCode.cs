namespace Expected;

public sealed class Unreachable(string message = "This code should be unreachable."): InvalidOperationException(message);

public enum MessageImplOptions : byte {
   Partial = 0,
   FullName = 1,
   Name = 2,
}
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
public sealed class ErrorCodeAttribute : Attribute {
   public string? Title { get; init; }
   public MessageImplOptions MessageImpl { get; init; }
   public string? CategoryClassName { get; init; }
   public bool GenerateCodesClass {get; init;}
   public string? CodesClassName { get; init; }
}
public abstract class ErrorCategory {
   public abstract string Title { get; }
   public abstract string GetMessage(int errorCode);
}
public readonly record struct ErrorCode(int Value, ErrorCategory Category) {
   public string Message => Category.GetMessage(Value);
}
