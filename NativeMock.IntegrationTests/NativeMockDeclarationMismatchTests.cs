namespace NativeMock.IntegrationTests
{
  using System.Runtime.InteropServices;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockDeclarationMismatchTests
  {
    private const string c_nonExistDllName = "noexist.dll";

    public class ReturnType
    {
      [DllImport (c_nonExistDllName)]
      public static extern int Test();
    }

    [NativeMockModule (c_nonExistDllName)]
    [NativeMockInterface (DeclaringType = typeof(ReturnType))]
    public interface IReturnType
    {
      void Test();
    }

    [Test]
    public void ReturnTypeMismatch()
    {
      Assert.That (NativeMockRegistry.Register<IReturnType>, Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }
  }
}
