namespace NativeMock
{
  /// <summary>
  /// Provides methods for looking up <see cref="NativeFunctionProxy" /> by their <see cref="NativeFunctionIdentifier" />s.
  /// </summary>
  internal interface INativeFunctionProxyLookup
  {
    NativeFunctionProxy? GetNativeFunctionProxy (NativeFunctionIdentifier nativeFunctionIdentifier);
  }
}
