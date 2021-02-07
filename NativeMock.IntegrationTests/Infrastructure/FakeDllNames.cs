namespace NativeMock.IntegrationTests.Infrastructure
{
  public class FakeDllNames
  {
    // We need any DLL that is already loaded, the DLL does not make a difference
    // since we hook the GetProcAddress function and return the correct hook for 
    // all the functions. coreclr.dll/kernel32.dll seem to be good targets since
    // they should always be loaded in the process

    public const string Dll1 = "coreclr.dll";
    public const string Dll2 = "kernel32.dll";
    public const string Dll3 = "user32.dll";
  }
}
