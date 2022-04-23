namespace NativeMock.IntegrationTests
{
  using Infrastructure;

#if NETFRAMEWORK
  [NUnit.Framework.SetUpFixture]
#endif
  public static class ModuleInitializer
  {
#if NETFRAMEWORK
    [NUnit.Framework.OneTimeSetUp]
#else
    [System.Runtime.CompilerServices.ModuleInitializer]
#endif
    public static void Initialize()
    {
      NativeMockRegistry.Initialize();
      NativeMockRegistry.RegisterFromAssembly (typeof(ModuleInitializer).Assembly, RegisterFromAssemblySearchBehavior.IncludeNestedTypes);

      TestDriver.LoadDriver();

      NativeLibraryDummy.Load (FakeDllNames.Dll1);
      NativeLibraryDummy.Load (FakeDllNames.Dll2);
      NativeLibraryDummy.Load (FakeDllNames.Dll3);
    }
  }
}
