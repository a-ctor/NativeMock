namespace NativeMock.UnitTests
{
  using System;
  using NUnit.Framework;
  using Utilities;

  [TestFixture]
  public class PInvokeUtilityTests
  {
    [Test]
    public void ResolveDllImport_ThrowsOnNullArguments()
    {
      Assert.That (() => PInvokeUtility.ResolveDllImport (null!, ""), Throws.ArgumentNullException);
      Assert.That (() => PInvokeUtility.ResolveDllImport ("", null!), Throws.ArgumentNullException);
    }

    [Test]
    public void ResolveDllImport_TriesToLoadDllIfNotFound()
    {
      Assert.That (() => PInvokeUtility.ResolveDllImport ("asdasdueirsaf", "asd"), Throws.TypeOf<DllNotFoundException>());
    }

    [Test]
    public void ResolveDllImport_Named()
    {
      var testImportNamed = PInvokeUtility.ResolveDllImport ("kernel32.dll", "BaseThreadInitThunk");
      Assert.That (testImportNamed, Is.Not.EqualTo (IntPtr.Zero));
    }

    [Test]
    public void ResolveDllImport_Ordinal()
    {
      var testImportOrdinal = PInvokeUtility.ResolveDllImport ("kernel32.dll", "#1");
      Assert.That (testImportOrdinal, Is.Not.EqualTo (IntPtr.Zero));
    }

    [Test]
    public void ResolveDllImport_NamedThrowsWhenNotFound()
    {
      Assert.That (() => PInvokeUtility.ResolveDllImport ("kernel32.dll", "BaseThreadInitThunk1337"), Throws.TypeOf<EntryPointNotFoundException>());
    }

    [Test]
    public void ResolveDllImport_OrdinalThrowsWhenNotFound()
    {
      Assert.That (() => PInvokeUtility.ResolveDllImport ("kernel32.dll", "#13337"), Throws.TypeOf<EntryPointNotFoundException>());
    }
  }
}
