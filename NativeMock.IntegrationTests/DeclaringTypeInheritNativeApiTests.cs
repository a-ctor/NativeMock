namespace NativeMock.IntegrationTests
{
  using Infrastructure;
  using NUnit.Framework;

  [TestFixture]
  public class DeclaringTypeInheritNativeApiTests : NativeMockTestBase<IDeclaringTypeInheritNativeApi>
  {
    [Test]
    public void DeclaringTypeInherit()
    {
      ApiMock.Setup (e => e.NmDeclaringTypeInherit ("😄")).Returns ("😄");

      Assert.That (DeclaringTypeInheritNativeApi.NmDeclaringTypeInherit ("😄"), Is.EqualTo ("😄"));
      ApiMock.VerifyAll();
    }

    [Test]
    public void DeclaringTypeOverride()
    {
      ApiMock.Setup (e => e.NmDeclaringTypeOverride ("😄")).Returns ("😄");

      Assert.That (DeclaringTypeInheritNativeApi.Inner.NmDeclaringTypeOverride ("😄"), Is.EqualTo ("😄"));
      ApiMock.VerifyAll();
    }
  }
}
