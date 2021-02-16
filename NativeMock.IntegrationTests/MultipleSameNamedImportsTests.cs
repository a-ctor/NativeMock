namespace NativeMock.IntegrationTests
{
  using System.Runtime.InteropServices;
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  public class MultipleSameNamedImportsTests
  {
    [NativeMockModule (FakeDllNames.Dll1)]
    [NativeMockInterface (DeclaringType = typeof(MultipleSameNamedImports))]
    public interface IMultipleSameNamedImports1
    {
      void SameNamed();
    }

    [NativeMockModule (FakeDllNames.Dll2)]
    [NativeMockInterface (DeclaringType = typeof(MultipleSameNamedImports))]
    public interface IMultipleSameNamedImports2
    {
      void SameNamed (string value);
    }

    public class MultipleSameNamedImports
    {
      [DllImport (FakeDllNames.Dll1, EntryPoint = "SameNamed")]
      public static extern void SameNamed1();

      [DllImport (FakeDllNames.Dll2, EntryPoint = "SameNamed")]
      public static extern void SameNamed2 (string value);
    }

    [Test]
    public void SameNamed1Test()
    {
      var mock = new Mock<IMultipleSameNamedImports1> (MockBehavior.Strict);
      mock.Setup (e => e.SameNamed());
      NativeMockRegistry.Mock (mock.Object);

      MultipleSameNamedImports.SameNamed1();
      mock.VerifyAll();
    }

    [Test]
    public void SameNamed2Test()
    {
      var mock = new Mock<IMultipleSameNamedImports2> (MockBehavior.Strict);
      mock.Setup (e => e.SameNamed("asd"));
      NativeMockRegistry.Mock (mock.Object);

      MultipleSameNamedImports.SameNamed2("asd");
      mock.VerifyAll();
    }
  }
}
