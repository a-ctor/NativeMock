namespace NativeMock
{
  /// <summary>
  /// Provides methods for creating <see cref="NativeFunctionProxy" />s.
  /// </summary>
  internal interface INativeFunctionProxyFactory
  {
    NativeFunctionProxy CreateNativeFunctionProxy (NativeMockInterfaceMethodDescription method);
  }
}
