namespace NativeMock.IntegrationTests.Infrastructure
{
  [NativeMockInterface (FakeDllNames.Dll1)]
  public interface IDuplicateNativeApi1
  {
    void NmDuplicate (int value);
  }
}
