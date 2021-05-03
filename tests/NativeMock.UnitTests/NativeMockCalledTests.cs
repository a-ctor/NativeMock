namespace NativeMock.UnitTests
{
  using System;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockCalledTests
  {
    [Test]
    public void AtLeast()
    {
      var nativeMockCalled = NativeMockCalled.AtLeast (3);
      Assert.That (nativeMockCalled.From, Is.EqualTo (3));
      Assert.That (nativeMockCalled.To, Is.EqualTo (int.MaxValue));
    }

    [Test]
    public void AtLeast_ThrowsOnNegativeNumber()
    {
      Assert.That (() => NativeMockCalled.AtLeast (-1), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void AtLeastOnce()
    {
      var nativeMockCalled = NativeMockCalled.AtLeastOnce;
      Assert.That (nativeMockCalled.From, Is.EqualTo (1));
      Assert.That (nativeMockCalled.To, Is.EqualTo (int.MaxValue));
    }

    [Test]
    public void AtMost()
    {
      var nativeMockCalled = NativeMockCalled.AtMost (3);
      Assert.That (nativeMockCalled.From, Is.EqualTo (0));
      Assert.That (nativeMockCalled.To, Is.EqualTo (3));
    }

    [Test]
    public void AtMost_ThrowsOnNegativeNumber()
    {
      Assert.That (() => NativeMockCalled.AtMost (-1), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void AtMostOnce()
    {
      var nativeMockCalled = NativeMockCalled.AtMostOnce;
      Assert.That (nativeMockCalled.From, Is.EqualTo (0));
      Assert.That (nativeMockCalled.To, Is.EqualTo (1));
    }

    [Test]
    public void Between()
    {
      var nativeMockCalled = NativeMockCalled.Between (3, 7);
      Assert.That (nativeMockCalled.From, Is.EqualTo (3));
      Assert.That (nativeMockCalled.To, Is.EqualTo (7));
    }

    [Test]
    public void Between_ThrowsOnNegativeNumber()
    {
      Assert.That (() => NativeMockCalled.Between (-1, 3), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That (() => NativeMockCalled.Between (3, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That (() => NativeMockCalled.Between (3, 1), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Exactly_ThrowsOnNegativeNumber()
    {
      Assert.That (() => NativeMockCalled.Exactly (-1), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Exactly()
    {
      var nativeMockCalled = NativeMockCalled.Exactly (3);
      Assert.That (nativeMockCalled.From, Is.EqualTo (3));
      Assert.That (nativeMockCalled.To, Is.EqualTo (3));
    }

    [Test]
    public void Never()
    {
      var nativeMockCalled = NativeMockCalled.Never;
      Assert.That (nativeMockCalled.From, Is.EqualTo (0));
      Assert.That (nativeMockCalled.To, Is.EqualTo (0));
    }

    [Test]
    public void Once()
    {
      var nativeMockCalled = NativeMockCalled.Once;
      Assert.That (nativeMockCalled.From, Is.EqualTo (1));
      Assert.That (nativeMockCalled.To, Is.EqualTo (1));
    }

    [Test]
    public void IsValidCallCount()
    {
      var nativeMockCalled = NativeMockCalled.Between (3, 6);
      Assert.That (nativeMockCalled.IsValidCallCount (2), Is.False);
      Assert.That (nativeMockCalled.IsValidCallCount (3), Is.True);
      Assert.That (nativeMockCalled.IsValidCallCount (6), Is.True);
      Assert.That (nativeMockCalled.IsValidCallCount (7), Is.False);
    }
  }
}
