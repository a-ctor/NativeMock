namespace NativeMock.Emit
{
  using System;
  using System.Reflection;

  internal interface INativeFunctionForwardProxyFactory
  {
    Delegate CreateNativeFunctionForwardProxy (MethodInfo method);
  }
}
