namespace NativeMock.IntegrationTests
{
  using System.Runtime.CompilerServices;

  public static class ModuleInitializer
  {
    [ModuleInitializer]
    public static void Initialize()
    {
      NativeMockRegistry.Initialize();
      NativeMockRegistry.RegisterFromAssembly (typeof(ModuleInitializer).Assembly, RegisterFromAssemblySearchBehavior.IncludeNestedTypes);
    }
  }
}
