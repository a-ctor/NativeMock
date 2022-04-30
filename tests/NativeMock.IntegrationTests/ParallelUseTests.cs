namespace NativeMock.IntegrationTests
{
  using System;
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  [Parallelizable (ParallelScope.Children)]
  [TestFixture]
  public class ParallelUseTests
  {
    private const int c_repeatCount = 10_000;

    [Test]
    [Repeat (c_repeatCount)]
    public void ATest()
    {
      DoTest();
    }

    [Test]
    [Repeat (c_repeatCount)]
    public void BTest()
    {
      DoTest();
    }

    [Test]
    [Repeat (c_repeatCount)]
    public void CTest()
    {
      DoTest();
    }

    private void DoTest()
    {
      var mock = new Mock<IFakeNativeApi> (MockBehavior.Strict);

      using var nativeMock = new NativeMock<IFakeNativeApi> (mock.Object);

      var result = Guid.NewGuid().ToString();
      mock.Setup (e => e.NmStringReturn()).Returns (result);

      Assert.That (FakeNativeApi.NmStringReturn(), Is.EqualTo (result));

      mock.VerifyAll();
    }
  }
}
