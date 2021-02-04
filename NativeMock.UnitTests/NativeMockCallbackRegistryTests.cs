namespace NativeMock.UnitTests
{
  using System;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockCallbackRegistryTests
  {
    private NativeMockCallbackRegistry _nativeMockCallbackRegistry;

    [SetUp]
    public void SetUp()
    {
      _nativeMockCallbackRegistry = new NativeMockCallbackRegistry();
    }

    [Test]
    public void ClearTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a");
      var actionMock = new Mock<Action> (MockBehavior.Strict);
      var nativeMockCallback = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);

      _nativeMockCallbackRegistry.Register (nativeFunctionIdentifier, nativeMockCallback);
      _nativeMockCallbackRegistry.Clear();
      _nativeMockCallbackRegistry.Register (nativeFunctionIdentifier, nativeMockCallback);
    }

    [Test]
    public void RegisterThrowsOnNullArgumentTest()
    {
      var actionMock = new Mock<Action> (MockBehavior.Strict);
      var nativeMockCallback = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);

      Assert.That (() => _nativeMockCallbackRegistry.Register (default, nativeMockCallback), Throws.ArgumentNullException);
      Assert.That (() => _nativeMockCallbackRegistry.Register (new NativeFunctionIdentifier (string.Empty), default), Throws.ArgumentNullException);
    }

    [Test]
    public void RegisterTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a");
      var actionMock = new Mock<Action> (MockBehavior.Strict);
      var nativeMockCallback = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);

      _nativeMockCallbackRegistry.Register (nativeFunctionIdentifier, nativeMockCallback);
    }

    [Test]
    public void RegisterTwiceThrowsTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a");
      var actionMock = new Mock<Action> (MockBehavior.Strict);
      var nativeMockCallback = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);

      _nativeMockCallbackRegistry.Register (nativeFunctionIdentifier, nativeMockCallback);
      Assert.That (() => _nativeMockCallbackRegistry.Register (nativeFunctionIdentifier, nativeMockCallback), Throws.InvalidOperationException);
    }

    [Test]
    public void UnregisterThrowsOnNullArgumentTest()
    {
      Assert.That (() => _nativeMockCallbackRegistry.Unregister (default), Throws.ArgumentNullException);
    }

    [Test]
    public void UnregisterTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a");
      var actionMock = new Mock<Action> (MockBehavior.Strict);
      var nativeMockCallback = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);

      _nativeMockCallbackRegistry.Register (nativeFunctionIdentifier, nativeMockCallback);
      _nativeMockCallbackRegistry.Unregister (nativeFunctionIdentifier);
      _nativeMockCallbackRegistry.Register (nativeFunctionIdentifier, nativeMockCallback);
    }

    [Test]
    public void UnregisterTwiceTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a");
      var actionMock = new Mock<Action> (MockBehavior.Strict);
      var nativeMockCallback = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);

      _nativeMockCallbackRegistry.Register (nativeFunctionIdentifier, nativeMockCallback);
      _nativeMockCallbackRegistry.Unregister (nativeFunctionIdentifier);
      _nativeMockCallbackRegistry.Unregister (nativeFunctionIdentifier);
    }

    [Test]
    public void InvokeThrowsOnNullArgumentTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a");
      var emptyArguments = Array.Empty<object>();

      Assert.That (() => _nativeMockCallbackRegistry.Invoke (default, emptyArguments), Throws.ArgumentNullException);
      Assert.That (() => _nativeMockCallbackRegistry.Invoke (nativeFunctionIdentifier, null!), Throws.ArgumentNullException);
    }

    [Test]
    public void InvokeTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a");

      var actionMock = new Mock<Func<string, int>> (MockBehavior.Strict);
      actionMock.Setup (e => e ("abc")).Returns (34);
      var nativeMockCallback = new NativeMockCallback (actionMock.Object.Target, actionMock.Object.Method);

      _nativeMockCallbackRegistry.Register (nativeFunctionIdentifier, nativeMockCallback);
      Assert.That (_nativeMockCallbackRegistry.Invoke (nativeFunctionIdentifier, new object[] {"abc"}), Is.EqualTo (34));
      actionMock.VerifyAll();
    }

    [Test]
    public void InvokeThrowsWhenNotMockIsFoundTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a");

      Assert.That (() => _nativeMockCallbackRegistry.Invoke (nativeFunctionIdentifier, Array.Empty<object>()), Throws.TypeOf<NativeFunctionNotMockedException>());
    }
  }
}
