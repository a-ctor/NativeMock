namespace NativeMock.IntegrationTests.Infrastructure
{
  using System.Runtime.InteropServices;

  public static class DuplicateNativeApi
  {
    [DllImport (FakeDllNames.Dll1, EntryPoint = "NmDuplicate")]
    public static extern void NmDuplicate1 (int value);

    [DllImport (FakeDllNames.Dll2, EntryPoint = "NmDuplicate")]
    public static extern void NmDuplicate2 (int value);

    [DllImport (FakeDllNames.Dll3, EntryPoint = "NmDuplicate")]
    public static extern void NmDuplicate3 (int value);
  }
}
