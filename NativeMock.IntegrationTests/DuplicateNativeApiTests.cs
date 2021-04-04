namespace NativeMock.IntegrationTests
{
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class DuplicateNativeApiTests
  {
    private Mock<IDuplicateNativeApi1> _duplicateMock1;
    private NativeMock<IDuplicateNativeApi1> _duplicateNativeMock1;
    private Mock<IDuplicateNativeApi2> _duplicateMock2;
    private NativeMock<IDuplicateNativeApi2> _duplicateNativeMock2;

    [SetUp]
    public void Setup()
    {
      _duplicateMock1 = new Mock<IDuplicateNativeApi1> (MockBehavior.Strict);
      _duplicateNativeMock1 = new NativeMock<IDuplicateNativeApi1> (_duplicateMock1.Object);

      _duplicateMock2 = new Mock<IDuplicateNativeApi2> (MockBehavior.Strict);
      _duplicateNativeMock2 = new NativeMock<IDuplicateNativeApi2> (_duplicateMock2.Object);
    }

    [TearDown]
    public void TearDown()
    {
      _duplicateNativeMock1.Dispose();
      _duplicateNativeMock2.Dispose();
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
