namespace NativeMock.UnitTests.Infrastructure
{
  [NativeMockInterface (FakeDllNames.Dll2)]
  public interface IDuplicateNativeApi2
  {
    void NmDuplicate (int value);
  }
}
