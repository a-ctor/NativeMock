namespace NativeMock.IntegrationTests
{
  using System;
  using System.Runtime.InteropServices;
  using Infrastructure;
  using NUnit.Framework;

  [TestFixture]
  public class DummyDllNativeApiTests : NativeMockTestBase<DummyDllNativeApiTests.IDummyDllNativeApi>
  {
    [NativeMockInterface (FakeDllNames.NonExistentDll)]
    public interface IDummyDllNativeApi
    {
      void Test();
    }

    public class DummyDllNativeApi
    {
      [DllImport (FakeDllNames.NonExistentDll)]
      public static extern void Test();
    }

    [Test]
    public void TestDummyDll()
    {
      ApiMock.Setup (e => e.Test());

      Assert.That (() => DummyDllNativeApi.Test(), Throws.TypeOf<DllNotFoundException>());

      NativeLibraryDummy.Load (FakeDllNames.NonExistentDll);
      DummyDllNativeApi.Test();

      ApiMock.VerifyAll();
    }
  }
}
