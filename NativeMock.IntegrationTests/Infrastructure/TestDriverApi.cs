namespace NativeMock.IntegrationTests.Infrastructure
{
  using System;
  using System.Runtime.InteropServices;

  public static class TestDriverApi
  {
    private const string c_dllName = TestDriver.DllName;

    [DllImport (c_dllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int NmForward (int i);

    [DllImport (c_dllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void NmForwardSetHandler (IntPtr ptr);
  }
}
