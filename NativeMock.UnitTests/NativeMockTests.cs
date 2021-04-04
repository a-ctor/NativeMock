namespace NativeMock.UnitTests
{
  using System;
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

    internal interface IInternalTestApi
    {
      void Write (Span<byte> test);
    }

    public interface ITestApi
    {
      void Write (Span<byte> test);
    }

    [Test]
    public void Constructor_TMustBeAnInterface()
    {
      Assert.That (() => new NativeMock<string>(), Throws.ArgumentException);
      Assert.That (() => new NativeMock<Delegate>(), Throws.ArgumentException);
    }

    [Test]
    public void Constructor_ObjectIsSetAfterConstruction()
    {
      var unsafeMock = new NativeMock<ITestApi>();
      Assert.That (unsafeMock.Object, Is.Not.Null);
    }
  }
}
