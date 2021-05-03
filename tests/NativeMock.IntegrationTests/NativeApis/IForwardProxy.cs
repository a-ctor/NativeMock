namespace NativeMock.IntegrationTests.NativeApis
{
  using System.Runtime.InteropServices;
  using Infrastructure;

  [NativeMockInterface (TestDriver.DllName)]
  public interface IForwardProxy
  {
    [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
    delegate int NmForwardDelegate (int i);

    [NativeMockCallback (Behavior = NativeMockBehavior.Forward)]
    int NmForward (int i);
  }
}
