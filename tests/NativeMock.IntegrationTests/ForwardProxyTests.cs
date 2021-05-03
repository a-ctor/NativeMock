namespace NativeMock.IntegrationTests
{
  using Infrastructure;
  using Moq;
  using NativeApis;
  using NUnit.Framework;

  [TestFixture]
  public class ForwardProxyTests
  {
    [TearDown]
    public void TearDown()
    {
      TestDriver.ClearForwardHandler();
    }

    [Test]
    public void CanMockTest()
    {
      var mock = new Mock<IForwardProxy> (MockBehavior.Strict);
      mock.Setup (e => e.NmForward (3)).Returns (5);

      using var nativeMock = new NativeMock<IForwardProxy> (mock.Object);

      Assert.That (TestDriverApi.NmForward (3), Is.EqualTo (5));
      mock.VerifyAll();
    }

    [Test]
    public void CanUseForwardInterface()
    {
      var mock = new Mock<IForwardProxy.NmForwardDelegate>();
      mock.Setup (e => e (3)).Returns (5);
      TestDriver.SetForwardHandler (mock.Object);

      var mockForwardObject = NativeMockRegistry.GetMockForwardProxy<IForwardProxy>();

      Assert.That (mockForwardObject.NmForward (3), Is.EqualTo (5));
      mock.VerifyAll();
    }

    [Test]
    public void ForwardsWhenNoMockIsSetUp()
    {
      var mock = new Mock<IForwardProxy.NmForwardDelegate>();
      mock.Setup (e => e (3)).Returns (5);
      TestDriver.SetForwardHandler (mock.Object);

      Assert.That (TestDriverApi.NmForward (3), Is.EqualTo (5));
      mock.VerifyAll();
    }
  }
}
