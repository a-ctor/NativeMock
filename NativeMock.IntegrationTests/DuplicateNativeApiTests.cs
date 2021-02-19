namespace NativeMock.IntegrationTests
{
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class DuplicateNativeApiTests
  {
    private Mock<IDuplicateNativeApi1> _duplicateMock1;
    private Mock<IDuplicateNativeApi2> _duplicateMock2;

    [SetUp]
    public void Setup()
    {
      _duplicateMock1 = new Mock<IDuplicateNativeApi1> (MockBehavior.Strict);
      _duplicateMock2 = new Mock<IDuplicateNativeApi2> (MockBehavior.Strict);

      NativeMockRegistry.ClearMocks();
      NativeMockRegistry.Mock (_duplicateMock1.Object);
      NativeMockRegistry.Mock (_duplicateMock2.Object);
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
  }
}
