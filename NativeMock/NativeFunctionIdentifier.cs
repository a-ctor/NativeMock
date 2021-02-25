namespace NativeMock
{
  using System;

  /// <summary>
  /// Represents a native function using its name and an optional containing module.
  /// </summary>
  internal readonly struct NativeFunctionIdentifier : IEquatable<NativeFunctionIdentifier>
  {
    private const string c_dllExtensions = ".dll";

    public string ModuleName { get; }

    public string FunctionName { get; }

    public bool IsInvalid => FunctionName == null!;

    public NativeFunctionIdentifier (string moduleName, string functionName)
    {
      if (moduleName == null)
        throw new ArgumentNullException (nameof(moduleName));
      if (string.IsNullOrWhiteSpace (moduleName))
        throw new ArgumentException ("Module name cannot be empty.");
      if (functionName == null)
        throw new ArgumentNullException (nameof(functionName));
      if (string.IsNullOrWhiteSpace (functionName))
        throw new ArgumentException ("Function name cannot be empty.");

      // Attach the dll extension to the module name if it was not specified
      ModuleName = moduleName.EndsWith (c_dllExtensions, StringComparison.OrdinalIgnoreCase) ? moduleName : moduleName + c_dllExtensions;
      FunctionName = functionName;
    }

    /// <inheritdoc />
    public bool Equals (NativeFunctionIdentifier other) => StringComparer.OrdinalIgnoreCase.Equals (ModuleName, other.ModuleName) && StringComparer.OrdinalIgnoreCase.Equals (FunctionName, other.FunctionName);

    /// <inheritdoc />
    public override bool Equals (object? obj) => obj is NativeFunctionIdentifier other && Equals (other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
      var moduleNameHashCode = ModuleName == null! ? StringComparer.OrdinalIgnoreCase.GetHashCode() : StringComparer.OrdinalIgnoreCase.GetHashCode (ModuleName);
      var functionNameHashCode = FunctionName == null! ? StringComparer.OrdinalIgnoreCase.GetHashCode() : StringComparer.OrdinalIgnoreCase.GetHashCode (FunctionName);
      return HashCode.Combine (moduleNameHashCode, functionNameHashCode);
    }

    public static bool operator == (NativeFunctionIdentifier left, NativeFunctionIdentifier right) => left.Equals (right);

    public static bool operator != (NativeFunctionIdentifier left, NativeFunctionIdentifier right) => !left.Equals (right);

    /// <inheritdoc />
    public override string ToString()
    {
      return $"{ModuleName}+{FunctionName}";
    }
  }
}
