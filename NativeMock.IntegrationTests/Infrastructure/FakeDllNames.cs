namespace NativeMock.IntegrationTests.Infrastructure
{
  public class FakeDllNames
  {
    // These DLLs do not really exist but a dummy DLL is loaded for each one in the ModuleInitializer
    public const string Dll1 = "test1.dll";
    public const string Dll2 = "test2.dll";
    public const string Dll3 = "test3.dll";

    public const string NonExistentDll = "nonexistent.dll";
  }
}
