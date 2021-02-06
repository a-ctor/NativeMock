namespace NativeMock.UnitTests.Infrastructure
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
    [return: MarshalAs (UnmanagedType.LPUTF8Str)]
    public static extern string NmUtf8StringReturn();


    [DllImport (FakeDllNames.Dll1)]
    public static extern void NmStructArg (int value);

    [DllImport (FakeDllNames.Dll1)]
    public static extern void NmStringArg (string value);

    [DllImport (FakeDllNames.Dll1)]
    public static extern void NmUtf8StringArg ([MarshalAs (UnmanagedType.LPUTF8Str)] string value);


    [DllImport (FakeDllNames.Dll1)]
    public static extern void NmRenameQ();
  }
}
