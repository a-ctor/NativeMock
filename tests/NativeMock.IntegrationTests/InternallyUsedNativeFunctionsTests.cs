namespace NativeMock.IntegrationTests
{
  using System;
  using System.Runtime.InteropServices;
  using Infrastructure;
  using NUnit.Framework;

  public class InternallyUsedNativeFunctionsTests : NativeMockTestBase<InternallyUsedNativeFunctionsTests.IInternallyUsedNativeFunctions>
  {
    [NativeMockInterface ("kernel32.dll", DeclaringType = typeof(InternallyUsedNativeFunctions))]
    public unsafe interface IInternallyUsedNativeFunctions
    {
      IntPtr GetProcAddressDelegate (IntPtr hModule, string procName);

      [NativeMockCallback(Behavior = NativeMockBehavior.Forward)]
      int GetModuleFileNameW (IntPtr handle, IntPtr fileName, int capacity);
    }

    public class InternallyUsedNativeFunctions
    {
      [DllImport ("Kernel32.dll", SetLastError = true)]
      public static extern IntPtr GetProcAddressDelegate (IntPtr hModule, string procName);

      [DllImport ("Kernel32.dll", SetLastError = true)]
      public static extern unsafe int GetModuleFileNameW (IntPtr moduleHandle, IntPtr fileName, int capacity);
    }

    [Test]
    public unsafe void GetProcAddressDelegateTest()
    {
      ApiMock.Setup (e => e.GetProcAddressDelegate (IntPtr.Zero, null)).Returns (IntPtr.Zero);

      InternallyUsedNativeFunctions.GetProcAddressDelegate (IntPtr.Zero, null);
      ApiMock.VerifyAll();
    }

    [Test]
    public void GetModuleFileNameWTest()
    {
      ApiMock.Setup (e => e.GetModuleFileNameW (IntPtr.Zero, IntPtr.Zero, 0)).Returns (0);

      InternallyUsedNativeFunctions.GetModuleFileNameW (IntPtr.Zero, IntPtr.Zero, 0);
      ApiMock.VerifyAll();
    }
  }
}
