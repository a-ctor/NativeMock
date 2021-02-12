namespace NativeMock.IntegrationTests.Infrastructure
{
  [NativeMockModule (FakeDllNames.Dll1)]
  [NativeMockInterface]
  public interface IDuplicateNativeApi1
  {
    void NmDuplicate (int value);
  }
}
