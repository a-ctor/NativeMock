namespace NativeMock.IntegrationTests
{
  using System.Runtime.InteropServices;
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class GlobalSetupTests : NativeMockTestBase<IFakeNativeApi>
  {
    [NativeMockInterface (FakeDllNames.Dll1, DeclaringType = typeof(GlobalSetup))]
    public interface IGlobalSetup
    {
      int NmGlobalSetup();
    }

    private class GlobalSetup : IGlobalSetup
    {
      [DllImport (FakeDllNames.Dll1)]
      public static extern int NmGlobalSetup();

      /// <inheritdoc />
      int IGlobalSetup.NmGlobalSetup() => 34;
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      NativeMockRegistry.GlobalSetups.Setup<IGlobalSetup> (new GlobalSetup());
    }

    // The following two methods are duplicated to make sure that a global setup is independent from any async flow
    // With only one method the asnyc flow from the OneTimeSetUp might flow into the test and influence it
    [Test]
    public void GlobalSetupIsAlwaysActive1()
    {
      Assert.That (GlobalSetup.NmGlobalSetup(), Is.EqualTo (34));
    }

    [Test]
    public void GlobalSetupIsAlwaysActive2()
    {
      Assert.That (GlobalSetup.NmGlobalSetup(), Is.EqualTo (34));
    }

    [Test]
    public void LocalSetupOverridesGlobalSetup()
    {
      var mock = new Mock<IGlobalSetup>();
      mock.Setup (e => e.NmGlobalSetup()).Returns (13);

      var nativeMock = new NativeMock<IGlobalSetup> (mock.Object);
      Assert.That (GlobalSetup.NmGlobalSetup(), Is.EqualTo (13));
      nativeMock.Dispose();

      mock.VerifyAll();
      Assert.That (GlobalSetup.NmGlobalSetup(), Is.EqualTo (34));
    }
  }
}
