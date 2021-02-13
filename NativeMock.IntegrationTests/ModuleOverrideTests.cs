namespace NativeMock.IntegrationTests
{
  using System.Runtime.InteropServices;
  using Infrastructure;
  using NUnit.Framework;

  [TestFixture]
  public class ModuleOverrideTests : NativeMockTestBase<ModuleOverrideTests.IModuleOverride>
  {
    [NativeMockModule (FakeDllNames.Dll1)]
    [NativeMockInterface]
    public interface IModuleOverride
    {
      [NativeMockModule (FakeDllNames.Dll2)]
      void Test();
    }

    public class ModuleOverride
    {
      [DllImport (FakeDllNames.Dll2)]
      public static extern void Test();
    }

    [Test]
    public void ModuleOverrideTest()
    {
      ApiMock.Setup (e => e.Test());

      ModuleOverride.Test();
      ApiMock.VerifyAll();
    }
  }
}
