namespace NativeMock.Emit
{
  using System;
  using System.Reflection;

  public interface INativeFunctionForwardProxyCodeGenerator
  {
    Delegate GenerateNativeFunctionForwardProxy (MethodInfo method);
  }
}
