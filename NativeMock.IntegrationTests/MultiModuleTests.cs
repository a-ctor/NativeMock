namespace NativeMock.IntegrationTests
{
  using System.Runtime.InteropServices;
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  public class MultiModuleTests : NativeMockTestBase<MultiModuleTests.IMultiModule>
  {
    [NativeMockModule (FakeDllNames.Dll1)]
    [NativeMockModule (FakeDllNames.Dll2)]
    [NativeMockInterface]
    public interface IMultiModule
    {
      void NmMultiModule();
    }

    public class MultiModule
    {
      [DllImport (FakeDllNames.Dll1, EntryPoint = "NmMultiModule")]
      public static extern void NmMultiModule1();

      [DllImport (FakeDllNames.Dll2, EntryPoint = "NmMultiModule")]
      public static extern void NmMultiModule2();
    }

    [Test]
    public void MultiModuleTest()
    {
      ApiMock.Setup (e => e.NmMultiModule());
      ApiMock.Setup (e => e.NmMultiModule());

      MultiModule.NmMultiModule1();
      MultiModule.NmMultiModule2();

      ApiMock.Verify (e => e.NmMultiModule(), Times.Exactly (2));
      ApiMock.VerifyAll();
    }
  }
}
