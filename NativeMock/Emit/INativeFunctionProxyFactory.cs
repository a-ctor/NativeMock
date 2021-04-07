namespace NativeMock.Emit
{
  using Representation;

  /// <summary>
  /// Provides methods for creating <see cref="NativeFunctionProxy" />s.
  /// </summary>
  internal interface INativeFunctionProxyFactory
  {
    NativeFunctionProxy CreateNativeFunctionProxy (NativeMockInterfaceMethodDescription method);
  }
}
