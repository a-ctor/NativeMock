namespace NativeMock.UnitTests
{
  using System;
  using NUnit.Framework;

  [TestFixture]
  public class UnsafeMockTests
  {
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
      Assert.That (() => new UnsafeMock<string>(), Throws.ArgumentException);
      Assert.That (() => new UnsafeMock<Delegate>(), Throws.ArgumentException);
    }

    [Test]
    public void Constructor_InternalInterfaceThrows()
    {
      Assert.That (() => new UnsafeMock<IInternalTestApi>(), Throws.ArgumentException);
    }

    [Test]
    public void Constructor_ObjectIsSetAfterConstruction()
    {
      var unsafeMock = new UnsafeMock<ITestApi>();
      Assert.That (unsafeMock.Object, Is.Not.Null);
    }
  }
}
