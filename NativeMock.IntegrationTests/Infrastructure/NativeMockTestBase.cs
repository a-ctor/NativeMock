namespace NativeMock.IntegrationTests.Infrastructure
{
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public abstract class NativeMockTestBase<T>
    where T : class
  {
    protected Mock<T> ApiMock;
    protected NativeMock<T> ApiNativeMock;

    [SetUp]
    public void Setup()
    {
      ApiMock = new Mock<T> (MockBehavior.Strict);
      ApiNativeMock = new NativeMock<T> (ApiMock.Object);
    }
  }
}
