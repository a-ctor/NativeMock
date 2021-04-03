namespace NativeMock.IntegrationTests
{
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockTests
  {
    [NativeMockInterface (FakeDllNames.Dll1)]
    public interface ITest
    {
      void NmNoop();
    }

    [Test]
    public void Constructor_TwoMocksAtTheSameTimeThrows()
    {
      var mock = new Mock<ITest>();
      using var nativeMock = new NativeMock<ITest> (mock.Object);

      Assert.That (() => new NativeMock<ITest> (mock.Object), Throws.InvalidOperationException.With.Message.StartWith ("Cannot have"));
    }

    [Test]
    public void Constructor_DisposeUnRegisteresMock()
    {
      var mock = new Mock<ITest>();
      var nativeMock = new NativeMock<ITest> (mock.Object);
      nativeMock.Dispose();

      using var nativeMock2 = new NativeMock<ITest> (mock.Object);
    }
  }
}
