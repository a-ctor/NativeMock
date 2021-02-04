namespace NativeMock.UnitTests
{
  using System;
  using Infrastructure;
  using NUnit.Framework;

  public class NativeMockTests : NativeMockTestBase
  {
    [Test]
    public void EmptyMethodTest()
    {
      ApiMock.Setup (e => e.NmEmpty());

      FakeNativeApi.NmEmpty();
      ApiMock.VerifyAll();
    }

    [Test]
    public void StructReturnTest()
    {
      ApiMock.Setup (e => e.NmStructReturn()).Returns (34);

      Assert.That (FakeNativeApi.NmStructReturn(), Is.EqualTo (34));
      ApiMock.VerifyAll();
    }

    [Test]
    public void StringReturnTest()
    {
      ApiMock.Setup (e => e.NmStringReturn()).Returns ("abc");

      Assert.That (FakeNativeApi.NmStringReturn(), Is.EqualTo ("abc"));
      ApiMock.VerifyAll();
    }

    [Test]
    public void Utf8StringReturnTest()
    {
      ApiMock.Setup (e => e.NmUtf8StringReturn()).Returns ("😄");

      Assert.That (FakeNativeApi.NmUtf8StringReturn(), Is.EqualTo ("😄"));
      ApiMock.VerifyAll();
    }


    [Test]
    public void StructArgTest()
    {
      ApiMock.Setup (e => e.NmStructArg (34));

      FakeNativeApi.NmStructArg (34);
      ApiMock.VerifyAll();
    }

    [Test]
    public void StringArgTest()
    {
      ApiMock.Setup (e => e.NmStringArg ("abc"));

      FakeNativeApi.NmStringArg ("abc");
      ApiMock.VerifyAll();
    }

    [Test]
    public void Uf8StringArgTest()
    {
      ApiMock.Setup (e => e.NmUtf8StringArg ("😄"));

      FakeNativeApi.NmUtf8StringArg ("😄");
      ApiMock.VerifyAll();
    }


    [Test]
    public void ExceptionsAreBubbledThroughTheNativeProxyTest()
    {
      var exception = new InvalidOperationException();
      ApiMock.Setup (e => e.NmEmpty()).Throws (exception);

      Assert.That (FakeNativeApi.NmEmpty, Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void UnproxiedThrowsNotSupportedExceptionTest()
    {
      NativeMockRegistry.ClearMocks();

      Assert.That (FakeNativeApi.NmEmpty, Throws.TypeOf<NativeFunctionNotMockedException>());
    }
  }
}
