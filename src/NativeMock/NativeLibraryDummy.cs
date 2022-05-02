namespace NativeMock
{
  using System;
  using System.IO;
  using Utilities;
#if NET
  using System.Runtime.InteropServices;
#endif

  /// <summary>
  /// Provides methods for loading dummy DLL files to satisfy P/Invoke dependencies.
  /// </summary>
  public static class NativeLibraryDummy
  {
    private const string c_temporaryDirectory64Name = "NativeMockTemporaryDlls64";
    private const string c_temporaryDirectory86Name = "NativeMockTemporaryDlls86";

    private const string c_dllExtension = ".dll";

    private const string c_integrityExceptionMessage = "The integrity of the dummy DLL file could not be verified.";

    private static bool s_firstCall = true;

    /// <summary>
    /// Creates a temporary dummy dll with the specified <paramref name="dllName" /> and loads it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The dummy dll could not be loaded.</exception>
    public static void Load (string dllName)
    {
      if (dllName == null)
        throw new ArgumentNullException (nameof(dllName));

      PlatformUtility.EnsurePlatformIsWindows();

      if (!dllName.EndsWith (c_dllExtension))
        dllName += c_dllExtension;

      var temporaryDirectory = GetTemporaryDllDirectory();
      var dummyDll = new FileInfo (Path.Combine (temporaryDirectory.FullName, dllName));

      DoCleanUpOnFirstRun(temporaryDirectory);

      try
      {
        CreateDummyDll (dummyDll);
        try
        {
          LoadDummyDll (dummyDll);
          return;
        }
        catch (InvalidOperationException)
        {
          // This can happen if the DLL file was corrupted -> delete and retry
        }

        try
        {
          dummyDll.Delete();
        }
        catch (Exception ex)
        {
          throw new InvalidOperationException (c_integrityExceptionMessage, ex);
        }

        CreateDummyDll (dummyDll);
        LoadDummyDll (dummyDll);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException ("Could not load the dummy DLL file.", ex);
      }
    }

    private static void LoadDummyDll (FileInfo dummyDll)
    {
      using (var stream = dummyDll.OpenRead())
      {
        var hash = HashUtility.CreateSha256Hash (stream);
        if (hash != NativeLibraryDummyProvider.DummyDllHash)
          throw new InvalidOperationException (c_integrityExceptionMessage);

        NativeLibrary.Load (dummyDll.FullName);
      }
    }

    private static void CreateDummyDll (FileInfo dummyDll)
    {
      if (dummyDll.Exists)
        return;

      using (var stream = dummyDll.Create())
      {
        using var resourceStream = NativeLibraryDummyProvider.OpenDummyDllStream();
        resourceStream.CopyTo (stream);
      }
    }

    private static void DoCleanUpOnFirstRun(DirectoryInfo tempDirectory)
    {
      if (!s_firstCall)
        return;

      s_firstCall = true;
      foreach (var tempDllFile in tempDirectory.EnumerateFiles("*.dll"))
      {
        try
        {
          tempDllFile.Delete();
        }
        catch
        {
          // Ignore temp files we cannot delete
        }
      }
    }

    private static DirectoryInfo GetTemporaryDllDirectory()
    {
      var tempPath = Path.GetTempPath();
      var directoryName = IntPtr.Size == 8
        ? c_temporaryDirectory64Name
        : c_temporaryDirectory86Name;

      return Directory.CreateDirectory (Path.Combine (tempPath, directoryName));
    }
  }
}
