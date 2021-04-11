namespace NativeMock.IntegrationTests
{
  using System.Runtime.InteropServices;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class ForwardProxyTests
  {
    [NativeMockInterface ("kernel32.dll")]
    public interface IForwardProxy
    {
      uint GetErrorMode();
    }

    public class ForwardProxy
    {
      [DllImport ("kernel32.dll", SetLastError = true)]
      public static extern uint GetErrorMode();
    }

    [Test]
    public void CanMockTest()
    {
      var mock = new Mock<IForwardProxy>();
      mock.Setup (e => e.GetErrorMode()).Returns (5);

      using var nativeMock = new NativeMock<IForwardProxy> (mock.Object);

      Assert.That (ForwardProxy.GetErrorMode(), Is.EqualTo (5));
      mock.VerifyAll();
    }

    [Test]
    public void CanUseForwardInterface()
    {
      var mockForwardObject = NativeMockRegistry.GetMockForwardObject<IForwardProxy>();

      var errorMode = mockForwardObject.GetErrorMode();
      Assert.That (errorMode, Is.Not.Zero);
    }
  }
}
