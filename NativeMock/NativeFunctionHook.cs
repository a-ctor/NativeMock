namespace NativeMock
{
  /// <summary>
  /// Represents a function that is called instead of a hooked native function, receiving the name and arguments of the
  /// original function.
  /// </summary>
  internal delegate object? NativeFunctionHook (NativeFunctionIdentifier nativeFunctionIdentifier, object[] arguments);
}
