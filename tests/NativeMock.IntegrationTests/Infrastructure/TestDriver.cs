namespace NativeMock.IntegrationTests.Infrastructure
{
  using System;
  using System.IO;
  using System.Runtime.InteropServices;
  using NativeApis;

#if NET461
  using Utilities;
#endif

  public static class TestDriver
  {
    public const string DllName = "NativeMock.IntegrationTests.Driver.dll";

    // We need to keep a reference to the forward handler so that it does not get GC'ed while the test driver has a native reference to it
    // ReSharper disable once NotAccessedField.Local
    private static IForwardProxy.NmForwardDelegate s_forwardHandler;

    public static void LoadDriver()
    {
      var baseFolder = Path.GetDirectoryName (typeof(TestDriver).Assembly.Location) ?? Environment.CurrentDirectory;
      var architectureFolder = IntPtr.Size == 8 ? "x64" : "x86";
      NativeLibrary.Load (Path.Combine(baseFolder, architectureFolder, DllName));
    }

    public static void ClearForwardHandler()
    {
      TestDriverApi.NmForwardSetHandler (IntPtr.Zero);
      s_forwardHandler = null;
    }

    public static void SetForwardHandler (IForwardProxy.NmForwardDelegate func)
    {
      var ptr = Marshal.GetFunctionPointerForDelegate (func);
      TestDriverApi.NmForwardSetHandler (ptr);

      s_forwardHandler = func;
    }
  }
}
