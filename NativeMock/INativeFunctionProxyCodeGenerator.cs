namespace NativeMock
{
  using System;

  /// <summary>
  /// Provides methods for generating proxy functions that redirect its calls to a common handler function.
  /// </summary>
  internal interface INativeFunctionProxyCodeGenerator
  {
    Delegate CreateProxyMethod (NativeMockInterfaceMethodDescription method, Type nativeFunctionDelegateType);
  }
}
