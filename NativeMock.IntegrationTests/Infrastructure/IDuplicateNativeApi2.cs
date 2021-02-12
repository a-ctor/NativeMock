namespace NativeMock.IntegrationTests.Infrastructure
{
  [NativeMockModule (FakeDllNames.Dll2)]
  [NativeMockInterface]
  public interface IDuplicateNativeApi2
  {
    void NmDuplicate (int value);
  }
}
