namespace NativeMock
{
  using System;

  public readonly struct NativeFunctionIdentifier : IEquatable<NativeFunctionIdentifier>
  {
    public string FunctionName { get; }

    public bool IsInvalid => FunctionName == null!;

    public NativeFunctionIdentifier (string functionName)
    {
      if (functionName == null)
        throw new ArgumentNullException (nameof(functionName));

      FunctionName = functionName;
    }

    /// <inheritdoc />
    public bool Equals (NativeFunctionIdentifier other) => FunctionName == other.FunctionName;

    /// <inheritdoc />
    public override bool Equals (object? obj) => obj is NativeFunctionIdentifier other && Equals (other);

    /// <inheritdoc />
    public override int GetHashCode() => FunctionName.GetHashCode();

    public static bool operator == (NativeFunctionIdentifier left, NativeFunctionIdentifier right) => left.Equals (right);

    public static bool operator != (NativeFunctionIdentifier left, NativeFunctionIdentifier right) => !left.Equals (right);

    /// <inheritdoc />
    public override string ToString()
    {
      return FunctionName ?? string.Empty;
    }
  }
}
