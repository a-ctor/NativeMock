namespace NativeMock.UnitTests
{
  using System.IO;
  using System.Text;
  using NUnit.Framework;
  using Utilities;

  [TestFixture]
  public class HashUtilityTests
  {
    [Test]
    public void CreateSha256HashTest()
    {
      var bytes = Encoding.UTF8.GetBytes ("The quick brown fox jumps over the lazy dog");

      GenericHash hash;
      using (var stream = new MemoryStream (bytes, false))
        hash = HashUtility.CreateSha256Hash (stream);

      var expectedHash = new GenericHash ("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592");
      Assert.That (hash, Is.EqualTo (expectedHash));
    }
  }
}
