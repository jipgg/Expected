namespace Expected;

[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class ErrorCodeEnumAttribute : Attribute {
   public string? Name { get; init; }
   public bool DontGenerateGetMessage { get; init; }
}
