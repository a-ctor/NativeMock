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
      // ReSharper disable once UnusedMember.Global
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

    [Test]
    public void Constructor_LocalByDefault()
    {
      var mock = new Mock<ITest>();
      using var nativeMock = new NativeMock<ITest> (mock.Object);

      Assert.That(NativeMockRegistry.LocalSetups.TrySetup (mock.Object), Is.False);
    }

    [Test]
    public void Constructor_GlobalRegistration()
    {
      var mock = new Mock<ITest>();
      using var nativeMock = new NativeMock<ITest> (mock.Object, NativeMockScope.Global);

      Assert.That (NativeMockRegistry.GlobalSetups.TrySetup (mock.Object), Is.False);
    }
  }
}
