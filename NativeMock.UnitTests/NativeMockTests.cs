namespace NativeMock.UnitTests
{
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockTests
  {
    public interface ITest
    {
    }

    [Test]
    public void Constructor_ThrowsOnNullImplementation()
    {
      Assert.That (() => new NativeMock<ITest> (null!), Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_ThrowsWhenInterfaceIsNotRegistered()
    {
      var mock = new Mock<ITest>();

      Assert.That (() => new NativeMock<ITest> (mock.Object), Throws.InvalidOperationException.With.Message.StartWith ("The specified"));
    }
  }
}
