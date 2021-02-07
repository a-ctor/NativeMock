namespace NativeMock.IntegrationTests.Infrastructure
{
  using System.Runtime.InteropServices;

  public class DeclaringTypeInheritNativeApi
  {
    public const string DllName = "coreclr.dll";

    [DllImport (DllName)]
    [return: MarshalAs (UnmanagedType.LPUTF8Str)]
    public static extern string NmDeclaringTypeInherit ([MarshalAs (UnmanagedType.LPUTF8Str)] string value);

    [DllImport (DllName)]
    // ReSharper disable once UnusedMember.Global
    public static extern string NmDeclaringTypeOverride (string value);

    public class Inner
    {
      [DllImport (DllName)]
      [return: MarshalAs (UnmanagedType.LPUTF8Str)]
      // ReSharper disable once MemberHidesStaticFromOuterClass
      public static extern string NmDeclaringTypeOverride ([MarshalAs (UnmanagedType.LPUTF8Str)] string value);
    }
  }
}
