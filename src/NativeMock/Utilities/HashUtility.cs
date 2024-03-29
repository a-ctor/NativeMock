namespace NativeMock.Utilities
{
  using System;
  using System.IO;
  using System.Security.Cryptography;

  /// <summary>
  /// Provides methods for hashing <see cref="Stream" />s.
  /// </summary>
  internal static class HashUtility
  {
    public static GenericHash CreateSha256Hash (Stream stream)
    {
      using var sha256 = SHA256.Create();
      var hashValue = sha256.ComputeHash (stream);
#if NETFRAMEWORK
      var hashString = BitConverter.ToString (hashValue).Replace ("-", string.Empty);
      return new GenericHash (hashString);
#else
      return new GenericHash (Convert.ToHexString (hashValue));
#endif
    }
  }
}
