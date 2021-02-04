namespace NativeMock.UnitTests
{
  using System.Collections.Generic;
  using NUnit.Framework;

  [TestFixture]
  public class NativeFunctionIdentifierTests
  {
    [Test]
    public void ThrowsOnNullArgumentTest()
    {
      Assert.That (() => new NativeFunctionIdentifier (null), Throws.ArgumentNullException);
    }

    [Test]
    public void IsInvalidTest()
    {
      Assert.That (new NativeFunctionIdentifier().IsInvalid, Is.True);
      Assert.That (new NativeFunctionIdentifier ("a").IsInvalid, Is.False);
    }

    [Test]
    public void EqualityTest()
    {
      var a = new NativeFunctionIdentifier ("a");
      var b = new NativeFunctionIdentifier ("b");

      Assert.That (a == new NativeFunctionIdentifier ("a"));
      Assert.That (a != b);
      Assert.That (b == b);
      Assert.That (a.Equals (new NativeFunctionIdentifier ("a")));
      Assert.That (!a.Equals (b));
      Assert.That (b.Equals (b));
    }

    [Test]
    public void HashCodeTest()
    {
      var nativeFunctionIdentifiers = new HashSet<NativeFunctionIdentifier>();
      nativeFunctionIdentifiers.Add (new NativeFunctionIdentifier ("a"));

      Assert.That (nativeFunctionIdentifiers.Contains (new NativeFunctionIdentifier ("a")));
    }

    [Test]
    public void ToStringTest()
    {
      var a = new NativeFunctionIdentifier ("a");

      Assert.That (a.ToString(), Is.EqualTo ("a"));
    }
  }
}
