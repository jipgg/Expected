namespace Expected.Internal;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
sealed class ExpectedAsyncExtendedAttribute : Attribute {
   public string? Namespace { get; init; }
}
[AttributeUsage(AttributeTargets.All)]
sealed class IsCanonicalAttribute : Attribute;

