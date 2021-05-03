namespace NativeMock.Emit
{
  using System;
  using System.Reflection;

  internal readonly struct NativeMockProxyCodeGeneratedMethod
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
