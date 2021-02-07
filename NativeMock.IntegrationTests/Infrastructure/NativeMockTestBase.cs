namespace NativeMock.IntegrationTests.Infrastructure
{
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public abstract class NativeMockTestBase<T>
    where T : class
  {
    protected Mock<T> ApiMock;

    [SetUp]
    public void Setup()
    {
      ApiMock = new Mock<T> (MockBehavior.Strict);

      NativeMockRegistry.ClearMocks();
      NativeMockRegistry.Mock (ApiMock.Object);
    }
  }
}
