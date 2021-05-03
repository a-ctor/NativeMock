namespace NativeMock.IntegrationTests
{
  using Infrastructure;

#if NET461
  [NUnit.Framework.SetUpFixture]
#endif
  public static class ModuleInitializer
  {
#if NET461
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
