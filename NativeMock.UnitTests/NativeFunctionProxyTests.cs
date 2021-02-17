namespace NativeMock.UnitTests
{
  using System;
  using NUnit.Framework;

  [TestFixture]
  public class NativeFunctionProxyTests
  {
    [Test]
    public void ThrowsOnNullArgumentTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a");
      var delegateType = typeof(Action);
      var @delegate = new Action (() => { });
      var intPtr = new IntPtr (3);
      var defaultStub = new Func<object> (() => null);

      Assert.That (() => new NativeFunctionProxy (default, delegateType, @delegate, intPtr, defaultStub), Throws.ArgumentNullException);
      Assert.That (() => new NativeFunctionProxy (nativeFunctionIdentifier, null!, @delegate, intPtr, defaultStub), Throws.ArgumentNullException);
      Assert.That (() => new NativeFunctionProxy (nativeFunctionIdentifier, delegateType, null!, intPtr, defaultStub), Throws.ArgumentNullException);
      Assert.That (() => new NativeFunctionProxy (nativeFunctionIdentifier, delegateType, @delegate, intPtr, null!), Throws.ArgumentNullException);
    }

    [Test]
    public void DelegateAndTypeMustMatchTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a");
      var intPtr = new IntPtr (3);

      Assert.That (() => new NativeFunctionProxy (nativeFunctionIdentifier, typeof(Action), new Func<int> (() => 1), intPtr, () => null), Throws.ArgumentException);
    }

    [Test]
    public void MustBeDelegateTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a");
      var intPtr = new IntPtr (3);

      Assert.That (() => new NativeFunctionProxy (nativeFunctionIdentifier, typeof(int), new Func<int> (() => 1), intPtr, () => null), Throws.ArgumentException);
    }
  }
}
