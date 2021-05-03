namespace NativeMock.Analyzer.TestAssembly
{
  using System.Runtime.InteropServices;

  public class UnsafeMethods
  {
    [DllImport ("test.dll")]
    public static extern void Test();
  }
}
