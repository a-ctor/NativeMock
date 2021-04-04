namespace NativeMock
{
  using System;
  using System.Reflection;

  public readonly struct NativeMockProxyCodeGeneratedMethod
  {
    public readonly MethodInfo MethodInfo;
    public readonly int MethodHandle;

    public NativeMockProxyCodeGeneratedMethod (MethodInfo methodInfo, int methodHandle)
    {
      if (methodInfo == null)
        throw new ArgumentNullException (nameof(methodInfo));

      MethodInfo = methodInfo;
      MethodHandle = methodHandle;
    }
  }
}
