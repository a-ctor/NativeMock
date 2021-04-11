namespace NativeMock.Emit
{
  using System;
  using System.Reflection;

  public interface INativeFunctionForwardProxyFactory
  {
    Delegate CreateNativeFunctionForwardProxy (MethodInfo method);
  }
}
