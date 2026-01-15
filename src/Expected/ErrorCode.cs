namespace Expected;

[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class ErrorCodeEnumAttribute : Attribute {
   public string? Name { get; init; }
   public bool DontGenerateGetMessage { get; init; }
}

public abstract class ErrorCategory {
   public abstract string Name { get; }
   public abstract string GetMessage(int errorCode);
   public virtual bool IsTruthy(int errorCode) => true;
}

public readonly struct ErrorCode : IEquatable<ErrorCode> {
   public ErrorCategory Category { get; }
   public int Value { get; }
   public string Message => Category.GetMessage(Value);

   public ErrorCode(int value, ErrorCategory category) {
      Value = value;
      Category = category;
   }

   public bool Equals(ErrorCode ec) => Category.Equals(ec.Category) && Value.Equals(ec.Value);
   public static bool operator ==(ErrorCode a, ErrorCode b) => a.Equals(b);
   public static bool operator !=(ErrorCode a, ErrorCode b) => !a.Equals(b);
   public override bool Equals(object? obj) => obj is ErrorCode ec && Equals(ec);
   public override int GetHashCode() => HashCode.Combine(Value, Category);
   public override string ToString() => $"ErrorCode: {{Value: {Value}, Message: {Message}}}";

   public static bool operator true(in ErrorCode ec) => ec.Category.IsTruthy(ec.Value);
   public static bool operator false(in ErrorCode ec) => !ec.Category.IsTruthy(ec.Value);
   public static bool operator !(in ErrorCode ec) => !ec.Category.IsTruthy(ec.Value);
}
