namespace NativeMock.IntegrationTests.Infrastructure
{
  using System.Runtime.InteropServices;

  public class DeclaringTypeInheritNativeApi
  {
    [DllImport (FakeDllNames.Dll1)]
    [return: MarshalAs (UnmanagedType.LPWStr)]
    public static extern string NmDeclaringTypeInherit ([MarshalAs (UnmanagedType.LPWStr)] string value);

    [DllImport (FakeDllNames.Dll1)]
    // ReSharper disable once UnusedMember.Global
    public static extern string NmDeclaringTypeOverride (string value);

    public class Inner
    {
      [DllImport (FakeDllNames.Dll1)]
      [return: MarshalAs (UnmanagedType.LPWStr)]
      // ReSharper disable once MemberHidesStaticFromOuterClass
      public static extern string NmDeclaringTypeOverride ([MarshalAs (UnmanagedType.LPWStr)] string value);
    }
  }
}
