namespace NativeMock.UnitTests.Infrastructure
{
  using System.Runtime.CompilerServices;

  public static class ModuleInitializer
  {
    [ModuleInitializer]
    public static void Initialize()
    {
      NativeMockRegistry.Initialize();

      NativeMockRegistry.Register<IFakeNativeApi>();
      NativeMockRegistry.Register<IDuplicateNativeApi1>();
      NativeMockRegistry.Register<IDuplicateNativeApi2>();
      NativeMockRegistry.Register<IDuplicateNativeApi3>();
    }
  }
}
