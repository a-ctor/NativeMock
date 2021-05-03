namespace NativeMock.IntegrationTests
{
  using System;
  using System.Runtime.InteropServices;
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class OrdinalMethodTests
  {
    [NativeMockInterface (FakeDllNames.Dll1)]
    public interface IOrdinalNativeApi
    {
      [NativeMockCallback ("#1337")]
      void NmOrdinal();
    }

    public class OrdinalNativeApi
    {
      [DllImport (FakeDllNames.Dll1, EntryPoint = "#1")]
      public static extern void Test();

      [DllImport (FakeDllNames.Dll1, EntryPoint = "#1337")]
      public static extern void NmOrdinalOtherName();
    }

    [Test]
    public void UsingOrdinalsDoesNotCrash()
    {
      Assert.That (() => OrdinalNativeApi.Test(), Throws.TypeOf<EntryPointNotFoundException>());
    }

    [Test]
    public void CanMockOrdinalNativeFunction()
    {
      var mock = new Mock<IOrdinalNativeApi> (MockBehavior.Strict);
      mock.Setup (e => e.NmOrdinal());

      using var nativeMock = new NativeMock<IOrdinalNativeApi>(mock.Object);
      
      OrdinalNativeApi.NmOrdinalOtherName();

      mock.VerifyAll();
    }
  }
}
