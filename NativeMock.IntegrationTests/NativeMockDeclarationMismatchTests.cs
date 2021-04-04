namespace NativeMock.IntegrationTests
{
  using System.Runtime.InteropServices;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockDeclarationMismatchTests
  {
    private const string c_nonExistDllName = "noexist.dll";

    private class ReturnType
    {
      [DllImport (c_nonExistDllName)]
      // ReSharper disable once UnusedMember.Local
      public static extern int Test();
    }

    [NativeMockInterface (c_nonExistDllName)]
    private interface IReturnType
    {
      [NativeMockCallback (DeclaringType = typeof(ReturnType))]
      void Test();
    }

    [Test]
    public void ReturnTypeMismatch()
    {
      Assert.That (NativeMockRegistry.Register<IReturnType>, Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }
  }
}
