namespace NativeMock.UnitTests.Infrastructure
{
  using System.Runtime.InteropServices;

  public static class FakeNativeApi
  {
    // We need any DLL that is already loaded, the DLL does not make a difference
    // since we hook the GetProcAddress function and return the correct hook for 
    // all the functions. coreclr.dll seems a good target since that should always
    // be loaded in the process
    private const string c_dllName = "coreclr.dll";

    [DllImport (c_dllName)]
    public static extern void NmEmpty();


    [DllImport (c_dllName)]
    public static extern int NmStructReturn();

    [DllImport (c_dllName)]
    public static extern string NmStringReturn();

    [DllImport (c_dllName)]
    [return: MarshalAs (UnmanagedType.LPUTF8Str)]
    public static extern string NmUtf8StringReturn();


    [DllImport (c_dllName)]
    public static extern void NmStructArg (int value);

    [DllImport (c_dllName)]
    public static extern void NmStringArg (string value);

    [DllImport (c_dllName)]
    public static extern void NmUtf8StringArg ([MarshalAs (UnmanagedType.LPUTF8Str)] string value);


    [DllImport (c_dllName)]
    public static extern void NmRenameQ ();
  }
}
