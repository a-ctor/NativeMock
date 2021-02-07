namespace NativeMock.IntegrationTests.Infrastructure
{
  [NativeMockModule (FakeDllNames.Dll1)]
  public interface IDuplicateNativeApi1
  {
    void NmDuplicate (int value);
  }
}
