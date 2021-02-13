namespace NativeMock.IntegrationTests
{
  using System.Runtime.CompilerServices;

  public static class ModuleInitializer
  {
    [ModuleInitializer]
    public static void Initialize()
    {
      NativeMockRegistry.Initialize();
      NativeMockRegistry.AutoRegister (typeof(ModuleInitializer).Assembly, AutoRegisterSearchBehavior.IncludeNestedTypes);
    }
  }
}
