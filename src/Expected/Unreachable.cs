namespace Expected;

public sealed class Unreachable(string message = "This code should be unreachable."): InvalidOperationException(message);
