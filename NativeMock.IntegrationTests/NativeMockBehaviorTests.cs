namespace NativeMock.IntegrationTests
{
  using System.Runtime.InteropServices;
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockBehaviorTests
  {
    // Interface definitions are needed for the global registration 
    // ReSharper disable UnusedType.Global
    // ReSharper disable UnusedMember.Global

    [NativeMockModule ("kernel32.dll")]
    [NativeMockInterface (DeclaringType = typeof(NativeMockBehavior))]
    public interface INativeMockBehavior
    {
      [NativeMockCallback (Behavior = NativeMock.NativeMockBehavior.Strict)]
      void NmStrictBehavior();

      [NativeMockCallback (Behavior = NativeMock.NativeMockBehavior.Loose)]
      void NmLooseBehavior();

      [NativeMockCallback (Behavior = NativeMock.NativeMockBehavior.Loose)]
      int NmLooseBehaviorStructReturn();

      [NativeMockCallback (Behavior = NativeMock.NativeMockBehavior.Loose)]
      string NmLooseBehaviorClassReturn();
    }

    public class NativeMockBehavior
    {
      [DllImport ("kernel32.dll")]
      public static extern void NmStrictBehavior();

      [DllImport ("kernel32.dll")]
      public static extern void NmLooseBehavior();

      [DllImport ("kernel32.dll")]
      public static extern int NmLooseBehaviorStructReturn();

      [DllImport ("kernel32.dll")]
      public static extern string NmLooseBehaviorClassReturn();
    }

    [Test]
    public void StrictBehaviorTest()
    {
      Assert.That (NativeMockBehavior.NmStrictBehavior, Throws.TypeOf<NativeFunctionNotMockedException>());
    }

    [Test]
    public void StrictBehaviorWorksWhenSetupTest()
    {
      var mock = new Mock<INativeMockBehavior> (MockBehavior.Strict);
      mock.Setup (e => e.NmStrictBehavior());

      NativeMockRegistry.ClearMocks();
      NativeMockRegistry.Mock (mock.Object);

      NativeMockBehavior.NmStrictBehavior();
      mock.VerifyAll();
    }

    [Test]
    public void LooseBehaviorTest()
    {
      Assert.That (NativeMockBehavior.NmLooseBehavior, Throws.Nothing);
      Assert.That (NativeMockBehavior.NmLooseBehaviorStructReturn(), Is.EqualTo (0));
      Assert.That (NativeMockBehavior.NmLooseBehaviorClassReturn(), Is.Null);
    }

    [Test]
    public void LooseBehaviorWorksWhenSetupTest()
    {
      var mock = new Mock<INativeMockBehavior> (MockBehavior.Strict);
      mock.Setup (e => e.NmLooseBehavior());

      NativeMockRegistry.ClearMocks();
      NativeMockRegistry.Mock (mock.Object);

      NativeMockBehavior.NmLooseBehavior();
      mock.VerifyAll();
    }

    [Test]
    public void LooseBehaviorStructReturnTest()
    {
      Assert.That (NativeMockBehavior.NmLooseBehaviorStructReturn(), Is.EqualTo (0));
    }

    [Test]
    public void LooseBehaviorStructReturnWorksWhenSetupTest()
    {
      var mock = new Mock<INativeMockBehavior> (MockBehavior.Strict);
      mock.Setup (e => e.NmLooseBehaviorStructReturn()).Returns (0);

      NativeMockRegistry.ClearMocks();
      NativeMockRegistry.Mock (mock.Object);

      NativeMockBehavior.NmLooseBehaviorStructReturn();
      mock.VerifyAll();
    }


    [Test]
    public void LooseBehaviorClassReturnTest()
    {
      Assert.That (NativeMockBehavior.NmLooseBehaviorClassReturn(), Is.Null);
    }

    [Test]
    public void LooseBehaviorClassReturnWorksWhenSetupTest()
    {
      var mock = new Mock<INativeMockBehavior> (MockBehavior.Strict);
      mock.Setup (e => e.NmLooseBehaviorClassReturn()).Returns ((string) null);

      NativeMockRegistry.ClearMocks();
      NativeMockRegistry.Mock (mock.Object);

      NativeMockBehavior.NmLooseBehaviorClassReturn();
      mock.VerifyAll();
    }

    [NativeMockModule (FakeDllNames.Dll1)]
    [NativeMockInterface (Behavior = NativeMock.NativeMockBehavior.Loose, DeclaringType = typeof(NativeMockBehaviorOverride))]
    public interface INativeMockBehaviorOverride
    {
      [NativeMockCallback]
      void NmLooseBehaviorInherited();

      [NativeMockCallback (Behavior = NativeMock.NativeMockBehavior.Strict)]
      void NmLooseBehaviorInheritedOverride();
    }

    public class NativeMockBehaviorOverride
    {
      [DllImport (FakeDllNames.Dll1)]
      public static extern void NmLooseBehaviorInherited();

      [DllImport (FakeDllNames.Dll1)]
      public static extern void NmLooseBehaviorInheritedOverride();
    }

    [Test]
    public void LooseBehaviorInheritedTest()
    {
      Assert.That (NativeMockBehaviorOverride.NmLooseBehaviorInherited, Throws.Nothing);
    }

    [Test]
    public void LooseBehaviorInheritedOverride()
    {
      Assert.That (NativeMockBehaviorOverride.NmLooseBehaviorInheritedOverride, Throws.TypeOf<NativeFunctionNotMockedException>());
    }
  }
}
