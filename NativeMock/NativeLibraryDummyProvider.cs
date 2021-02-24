namespace NativeMock
{
  using System;
  using System.IO;

  /// <summary>
  /// Provides methods for reading embedded native library dummies.
  /// </summary>
  internal static class NativeLibraryDummyProvider
  {
    private const string c_dummyDll64Name = "NativeMock.DummyDll.x64.dll";
    private const string c_dummyDll86Name = "NativeMock.DummyDll.x86.dll";

    private static GenericHash? s_dummyDllHash;

    public static GenericHash DummyDllHash
    {
      get
      {
        if (s_dummyDllHash.HasValue)
          return s_dummyDllHash.Value;

        GenericHash hash;
        using (var stream = OpenDummyDllStream())
          hash = HashUtility.CreateSha256Hash (stream);

        s_dummyDllHash = hash;
        return hash;
      }
    }

    public static Stream OpenDummyDllStream()
    {
      var resourceName = IntPtr.Size == 8
        ? c_dummyDll64Name
        : c_dummyDll86Name;

      var stream = typeof(NativeLibraryDummy).Assembly.GetManifestResourceStream (resourceName);
      if (stream == null)
        throw new NotSupportedException ("Dummy dll is not embedded.");

      if (!s_dummyDllHash.HasValue)
      {
        s_dummyDllHash = HashUtility.CreateSha256Hash (stream);
        stream.Position = 0;
      }

      return stream;
    }
  }
}
