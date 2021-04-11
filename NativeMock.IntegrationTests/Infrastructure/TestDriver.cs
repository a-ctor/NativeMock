namespace NativeMock.IntegrationTests.Infrastructure
{
  using System;
  using System.Runtime.InteropServices;

  public static class TestDriver
  {
    public const string DllName = "NativeMock.IntegrationTests.Driver.dll";

    [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
    public delegate int ForwardHandlerDelegate (int i);


    // We need to keep a reference to the forward handler so that it does not get GC'ed while the test driver has a native reference to it
    // ReSharper disable once NotAccessedField.Local
    private static ForwardHandlerDelegate s_forwardHandler;

    public static void LoadDriver()
    {
      NativeLibrary.Load (DllName);
    }

    public static void ClearForwardHandler()
    {
      TestDriverApi.NmForwardSetHandler (IntPtr.Zero);
      s_forwardHandler = null;
    }

    public static void SetForwardHandler (ForwardHandlerDelegate func)
    {
      var ptr = Marshal.GetFunctionPointerForDelegate (func);
      TestDriverApi.NmForwardSetHandler (ptr);

      s_forwardHandler = func;
    }
  }
}
