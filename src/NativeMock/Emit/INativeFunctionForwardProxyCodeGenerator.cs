namespace NativeMock.Emit
{
  using System;
  using System.Reflection;

  internal interface INativeFunctionForwardProxyCodeGenerator
  {
    Delegate GenerateNativeFunctionForwardProxy (MethodInfo method);
  }
}
