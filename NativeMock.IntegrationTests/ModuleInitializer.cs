namespace NativeMock.IntegrationTests
{
  using System.Runtime.CompilerServices;
  using Infrastructure;

  public static class ModuleInitializer
  {
    [ModuleInitializer]
    public static void Initialize()
    {
      NativeMockRegistry.Initialize();
      NativeMockRegistry.RegisterFromAssembly (typeof(ModuleInitializer).Assembly, RegisterFromAssemblySearchBehavior.IncludeNestedTypes);

      TestDriver.LoadDriver();
    }
  }
}
