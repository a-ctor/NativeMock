namespace NativeMock.UnitTests
{
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class DuplicateNativeApiTests
  {
    private Mock<IDuplicateNativeApi1> DuplicateMock1;
    private Mock<IDuplicateNativeApi2> DuplicateMock2;
    private Mock<IDuplicateNativeApi3> DuplicateMock3;

    [SetUp]
    public void Setup()
    {
      DuplicateMock1 = new Mock<IDuplicateNativeApi1> (MockBehavior.Strict);
      DuplicateMock2 = new Mock<IDuplicateNativeApi2> (MockBehavior.Strict);
      DuplicateMock3 = new Mock<IDuplicateNativeApi3> (MockBehavior.Strict);

      NativeMockRegistry.ClearMocks();
      NativeMockRegistry.Mock (DuplicateMock1.Object);
      NativeMockRegistry.Mock (DuplicateMock2.Object);
      NativeMockRegistry.Mock (DuplicateMock3.Object);
    }

    [Test]
    public void Duplicate1Test()
    {
      DuplicateMock1.Setup (e => e.NmDuplicate (0));

      DuplicateNativeApi.NmDuplicate1 (0);
      DuplicateMock1.VerifyAll();
    }

    [Test]
    public void Duplicate2Test()
    {
      DuplicateMock2.Setup (e => e.NmDuplicate (0));

      DuplicateNativeApi.NmDuplicate2 (0);
      DuplicateMock2.VerifyAll();
    }

    [Test]
    public void Duplicate3Test()
    {
      DuplicateMock3.Setup (e => e.NmDuplicate (0));

      DuplicateNativeApi.NmDuplicate3 (0);
      DuplicateMock3.VerifyAll();
    }

    [Test]
    public void DuplicateMultiUseTest()
    {
      DuplicateMock1.Setup (e => e.NmDuplicate (1));
      DuplicateMock2.Setup (e => e.NmDuplicate (2));
      DuplicateMock3.Setup (e => e.NmDuplicate (3));

      DuplicateNativeApi.NmDuplicate1 (1);
      DuplicateNativeApi.NmDuplicate2 (2);
      DuplicateNativeApi.NmDuplicate3 (3);

      DuplicateMock3.VerifyAll();
    }
  }
}
