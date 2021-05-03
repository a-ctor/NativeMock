namespace NativeMock.UnitTests
{
  using System;
  using NativeMock.Emit;
  using NUnit.Framework;

  [TestFixture]
  public class NativeFunctionProxyTests
  {
    [Test]
    public void ThrowsOnNullArgumentTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a", "b");
      var delegateType = typeof(Action);
      var @delegate = new Action (() => { });
      var intPtr = new IntPtr (3);

      Assert.That (() => new NativeFunctionProxy (default, delegateType, @delegate, intPtr), Throws.ArgumentNullException);
      Assert.That (() => new NativeFunctionProxy (nativeFunctionIdentifier, null!, @delegate, intPtr), Throws.ArgumentNullException);
      Assert.That (() => new NativeFunctionProxy (nativeFunctionIdentifier, delegateType, null!, intPtr), Throws.ArgumentNullException);
    }

    [Test]
    public void DelegateAndTypeMustMatchTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a", "b");
      var intPtr = new IntPtr (3);

      Assert.That (() => new NativeFunctionProxy (nativeFunctionIdentifier, typeof(Action), new Func<int> (() => 1), intPtr), Throws.ArgumentException);
    }

    [Test]
    public void MustBeDelegateTest()
    {
      var nativeFunctionIdentifier = new NativeFunctionIdentifier ("a", "b");
      var intPtr = new IntPtr (3);

      Assert.That (() => new NativeFunctionProxy (nativeFunctionIdentifier, typeof(int), new Func<int> (() => 1), intPtr), Throws.ArgumentException);
    }
  }
}
