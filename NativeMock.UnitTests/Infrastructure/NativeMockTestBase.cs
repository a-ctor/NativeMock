namespace NativeMock.UnitTests.Infrastructure
{
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public abstract class NativeMockTestBase
  {
    protected Mock<IFakeNativeApi> ApiMock;

    [SetUp]
    public void Setup()
    {
      ApiMock = new Mock<IFakeNativeApi> (MockBehavior.Strict);

      NativeMockRegistry.ClearMocks();
      NativeMockRegistry.Mock (ApiMock.Object);
    }
  }
}
