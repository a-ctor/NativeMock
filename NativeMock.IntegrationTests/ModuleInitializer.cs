namespace NativeMock.IntegrationTests
{
  using System.Runtime.CompilerServices;

  public static class ModuleInitializer
  {
    [ModuleInitializer]
    public static void Initialize()
    {
      NativeMockRepository.Initialize();
      NativeMockRepository.RegisterFromAssembly (typeof(ModuleInitializer).Assembly, RegisterFromAssemblySearchBehavior.IncludeNestedTypes);
    }
  }
}
