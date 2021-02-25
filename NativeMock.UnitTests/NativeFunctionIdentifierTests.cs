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
      Assert.That (() => new NativeFunctionIdentifier (null!, "a"), Throws.ArgumentNullException);
      Assert.That (() => new NativeFunctionIdentifier ("a", null!), Throws.ArgumentNullException);
    }

    [Test]
    public void IsInvalidTest()
    {
      Assert.That (new NativeFunctionIdentifier().IsInvalid, Is.True);
      Assert.That (new NativeFunctionIdentifier ("a", "b").IsInvalid, Is.False);
    }

    [Test]
    public void EqualityTest()
    {
      var a = new NativeFunctionIdentifier ("a", "c");
      var b = new NativeFunctionIdentifier ("b", "c");
      var c = new NativeFunctionIdentifier ("a", "a");

      Assert.That (a == new NativeFunctionIdentifier ("a", "c"));
      Assert.That (a != b);
      Assert.That (a != c);
      Assert.That (b != c);
    }

    [Test]
    public void EqualityIgnoresCaseTest()
    {
      Assert.That (new NativeFunctionIdentifier ("A", "b") == new NativeFunctionIdentifier ("a", "b"));
      Assert.That (new NativeFunctionIdentifier ("a", "B") == new NativeFunctionIdentifier ("a", "b"));
    }

    [Test]
    public void HashCodeTest()
    {
      var nativeFunctionIdentifiers = new HashSet<NativeFunctionIdentifier>();
      nativeFunctionIdentifiers.Add (new NativeFunctionIdentifier ("a", "c"));

      Assert.That (nativeFunctionIdentifiers.Contains (new NativeFunctionIdentifier ("a", "c")));
    }

    [Test]
    public void ToStringTest()
    {
      Assert.That (new NativeFunctionIdentifier ("a.dll", "b").ToString(), Is.EqualTo ("a.dll+b"));
      Assert.That (new NativeFunctionIdentifier ("a", "b").ToString(), Is.EqualTo ("a.dll+b"));
    }
  }
}
