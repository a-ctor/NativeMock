namespace NativeMock.UnitTests
{
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class DuplicateNativeApiTests
  {
    private Mock<IDuplicateNativeApi1> _duplicateMock1;
    private Mock<IDuplicateNativeApi2> _duplicateMock2;
    private Mock<IDuplicateNativeApi3> _duplicateMock3;

    [SetUp]
    public void Setup()
    {
      _duplicateMock1 = new Mock<IDuplicateNativeApi1> (MockBehavior.Strict);
      _duplicateMock2 = new Mock<IDuplicateNativeApi2> (MockBehavior.Strict);
      _duplicateMock3 = new Mock<IDuplicateNativeApi3> (MockBehavior.Strict);

      NativeMockRegistry.ClearMocks();
      NativeMockRegistry.Mock (_duplicateMock1.Object);
      NativeMockRegistry.Mock (_duplicateMock2.Object);
      NativeMockRegistry.Mock (_duplicateMock3.Object);
    }

    [Test]
    public void Duplicate1Test()
    {
      _duplicateMock1.Setup (e => e.NmDuplicate (0));

      DuplicateNativeApi.NmDuplicate1 (0);
      _duplicateMock1.VerifyAll();
    }

    [Test]
    public void Duplicate2Test()
    {
      _duplicateMock2.Setup (e => e.NmDuplicate (0));

      DuplicateNativeApi.NmDuplicate2 (0);
      _duplicateMock2.VerifyAll();
    }

    [Test]
    public void Duplicate3Test()
    {
      _duplicateMock3.Setup (e => e.NmDuplicate (0));

      DuplicateNativeApi.NmDuplicate3 (0);
      _duplicateMock3.VerifyAll();
    }

    [Test]
    public void DuplicateMultiUseTest()
    {
      _duplicateMock1.Setup (e => e.NmDuplicate (1));
      _duplicateMock2.Setup (e => e.NmDuplicate (2));
      _duplicateMock3.Setup (e => e.NmDuplicate (3));

      DuplicateNativeApi.NmDuplicate1 (1);
      DuplicateNativeApi.NmDuplicate2 (2);
      DuplicateNativeApi.NmDuplicate3 (3);

      _duplicateMock3.VerifyAll();
    }
  }
}
