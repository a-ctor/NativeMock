namespace NativeMock.UnitTests
{
  using System;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockCallbackTests
  {
    [Test]
    public void IsInvalidTest()
    {
      var actionMock = new Mock<Action> (MockBehavior.Strict);

      Assert.That (new NativeMockCallback().IsInvalid, Is.True);
      Assert.That (new NativeMockCallback (null, actionMock.Object.Method).IsInvalid, Is.False);
    }

    [Test]
    public void ThrowsOnNullArgumentTest()
    {
      Assert.That (() => new NativeMockCallback (new object(), null!), Throws.ArgumentNullException);
    }

    [Test]
    public void InvokeThrowsOnNullArgumentTest()
    {
      var actionMock = new Mock<Action> (MockBehavior.Strict);

      var target = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);

      Assert.That (() => target.Invoke (null!), Throws.ArgumentNullException);
    }

    [Test]
    public void InvokeTest()
    {
      var actionMock = new Mock<Action> (MockBehavior.Strict);
      actionMock.Setup (e => e());

      var target = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);
      target.Invoke (Array.Empty<object>());

      actionMock.VerifyAll();
    }

    [Test]
    public void InvokeReturnTest()
    {
      var actionMock = new Mock<Func<string>> (MockBehavior.Strict);
      actionMock.Setup (e => e()).Returns ("abc");

      var target = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);

      Assert.That (target.Invoke (Array.Empty<object>()), Is.EqualTo ("abc"));
      actionMock.VerifyAll();
    }

    [Test]
    public void InvokeArgumentTest()
    {
      var actionMock = new Mock<Action<string>> (MockBehavior.Strict);
      actionMock.Setup (e => e ("abc"));

      var target = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);
      target.Invoke (new object[] {"abc"});

      actionMock.VerifyAll();
    }

    [Test]
    public void InvokeBubblesExceptionTest()
    {
      var actionMock = new Mock<Action> (MockBehavior.Strict);
      actionMock.Setup (e => e()).Throws<InvalidOperationException>();

      var target = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);
      Assert.That (() => target.Invoke (Array.Empty<object>()), Throws.InvalidOperationException);

      actionMock.VerifyAll();
    }

    [Test]
    public void InvokeThrowsInInvalidUsageTest()
    {
      NativeMockCallback target = default;
      Assert.That (() => target.Invoke (Array.Empty<object>()), Throws.InvalidOperationException);
    }
  }
}
