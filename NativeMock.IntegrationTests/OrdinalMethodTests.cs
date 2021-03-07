namespace NativeMock.IntegrationTests
{
  using System;
  using System.Runtime.InteropServices;
  using Infrastructure;
  using NUnit.Framework;

  [TestFixture]
  public class OrdinalMethodTests
  {
    public class OrdinalNativeApi
    {
      [DllImport (FakeDllNames.Dll1, EntryPoint = "#1")]
      public static extern void Test();
    }

    [Test]
    public void UsingOrdinalsDoesNotCrash()
    {
      Assert.That (() => OrdinalNativeApi.Test(), Throws.TypeOf<EntryPointNotFoundException>());
    }
  }
}
