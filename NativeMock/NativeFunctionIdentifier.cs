namespace NativeMock
{
  using System;

  /// <summary>
  /// Represents a native function using its name and an optional containing module.
  /// </summary>
  internal readonly struct NativeFunctionIdentifier : IEquatable<NativeFunctionIdentifier>
  {
    public string? ModuleName { get; }

    public string FunctionName { get; }

    public bool IsInvalid => FunctionName == null!;

    public NativeFunctionIdentifier (string functionName)
    {
      if (functionName == null)
        throw new ArgumentNullException (nameof(functionName));

      ModuleName = null;
      FunctionName = functionName;
    }

    public NativeFunctionIdentifier (string moduleName, string functionName)
    {
      if (moduleName == null)
        throw new ArgumentNullException (nameof(moduleName));
      if (functionName == null)
        throw new ArgumentNullException (nameof(functionName));

      ModuleName = moduleName;
      FunctionName = functionName;
    }

    /// <inheritdoc />
    public bool Equals (NativeFunctionIdentifier other) => StringComparer.OrdinalIgnoreCase.Equals (ModuleName, other.ModuleName) && StringComparer.OrdinalIgnoreCase.Equals (FunctionName, other.FunctionName);

    /// <inheritdoc />
    public override bool Equals (object? obj) => obj is NativeFunctionIdentifier other && Equals (other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
      var moduleNameHashCode = ModuleName == null ? StringComparer.OrdinalIgnoreCase.GetHashCode() : StringComparer.OrdinalIgnoreCase.GetHashCode (ModuleName);
      var functionNameHashCode = FunctionName == null! ? StringComparer.OrdinalIgnoreCase.GetHashCode() : StringComparer.OrdinalIgnoreCase.GetHashCode (FunctionName);
      return HashCode.Combine (moduleNameHashCode, functionNameHashCode);
    }

    public static bool operator == (NativeFunctionIdentifier left, NativeFunctionIdentifier right) => left.Equals (right);

    public static bool operator != (NativeFunctionIdentifier left, NativeFunctionIdentifier right) => !left.Equals (right);

    /// <inheritdoc />
    public override string ToString()
    {
      return ModuleName != null
        ? $"{ModuleName}+{FunctionName}"
        : FunctionName;
    }
  }
}
