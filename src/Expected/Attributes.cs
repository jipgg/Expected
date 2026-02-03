namespace Expected;

using static Local;

[AttributeUsage(ExpectedTargets, AllowMultiple = false)]
public sealed class UnexpectedAttribute<E> : Attribute where E : allows ref struct;
[AttributeUsage(ExpectedTargets, AllowMultiple = false)]
public sealed class ExpectedAttribute<V> : Attribute where V : allows ref struct;
[AttributeUsage(ExpectedTargets, AllowMultiple = false)]
public sealed class ExpectedAttribute<V, E>: Attribute where V: allows ref struct where E: allows ref struct;
[AttributeUsage(ExpectedTargets, AllowMultiple = false)]
public sealed class ExpectedAttribute: Attribute;

[AttributeUsage(ExpectedTargets, AllowMultiple = false)]
public sealed class MaybeUnexpectedAttribute : Attribute;

public enum MessageImplOptions : byte { Partial, FullName, Name }
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
public sealed class ErrorCodeAttribute : Attribute {
   public string? Title { get; init; }
   public MessageImplOptions MessageImpl { get; init; }
   public string? CategoryClassName { get; init; }
   public bool GenerateCodesClass { get; init; }
   public string? CodesClassName { get; init; }
}

file static class Local {
   public const AttributeTargets ExpectedTargets = AttributeTargets.Struct | AttributeTargets.Class;
}
