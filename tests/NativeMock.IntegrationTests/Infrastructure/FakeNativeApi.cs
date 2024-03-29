namespace NativeMock.IntegrationTests.Infrastructure
{
  using System.Runtime.InteropServices;

  public static class FakeNativeApi
  {
    [DllImport (FakeDllNames.Dll1)]
    public static extern void NmEmpty();


    [DllImport (FakeDllNames.Dll1)]
    public static extern int NmStructReturn();

    [DllImport (FakeDllNames.Dll1)]
    public static extern string NmStringReturn();

    [DllImport (FakeDllNames.Dll1)]
    [return: MarshalAs (UnmanagedType.LPWStr)]
    public static extern string NmUtf8StringReturn();


    [DllImport (FakeDllNames.Dll1)]
    public static extern void NmStructArg (int value);

    [DllImport (FakeDllNames.Dll1)]
    public static extern void NmStringArg (string value);

    [DllImport (FakeDllNames.Dll1)]
    public static extern void NmUtf8StringArg ([MarshalAs (UnmanagedType.LPWStr)] string value);


    [DllImport (FakeDllNames.Dll1)]
    public static extern void NmRenameQ();


    [DllImport (FakeDllNames.Dll1)]
    [return: MarshalAs (UnmanagedType.LPWStr)]
    public static extern string NmMethodDefinitionReference ([MarshalAs (UnmanagedType.LPWStr)] string value);

    [DllImport (FakeDllNames.Dll1, EntryPoint = "NmMethodDefinitionReference2")]
    [return: MarshalAs (UnmanagedType.LPWStr)]
    public static extern string NmMethodDefinitionReferenceRenamed ([MarshalAs (UnmanagedType.LPWStr)] string value);

    [DllImport (FakeDllNames.Dll1, EntryPoint = "NmPrivateRenamed")]
    private static extern void NmPrivate();

    public static void NmPrivateRenamed() => NmPrivate();
  }
}
