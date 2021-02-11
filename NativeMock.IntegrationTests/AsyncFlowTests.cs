namespace NativeMock.IntegrationTests
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using Infrastructure;
  using NUnit.Framework;

  [TestFixture]
  public class AsyncFlowTests : NativeMockTestBase<IFakeNativeApi>
  {
    [Test]
    public void RegisteredApiFlowsToNewThreadTest()
    {
      ApiMock.Setup (e => e.NmStringReturn()).Returns ("test");

      string result = null;
      Exception ex = null;
      var thread = new Thread (
        () =>
        {
          try
          {
            result = FakeNativeApi.NmStringReturn();
          }
          catch (Exception e)
          {
            ex = e;
          }
        });
      thread.Start();
      thread.Join();

      Assert.That (result, Is.EqualTo ("test"));
      Assert.That (ex, Is.Null);
      ApiMock.VerifyAll();
    }

    [Test]
    public void RegisteredApiFlowsToNewTaskTest()
    {
      ApiMock.Setup (e => e.NmStringReturn()).Returns ("test");

      var result = Task.Run (FakeNativeApi.NmStringReturn).GetAwaiter().GetResult();

      Assert.That (result, Is.EqualTo ("test"));
      ApiMock.VerifyAll();
    }
  }
}
