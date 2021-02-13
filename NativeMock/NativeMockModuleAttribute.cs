namespace NativeMock
{
  using System;

  /// <summary>
  /// Applies the mock interface only to the specified module.
  /// </summary>
  [AttributeUsage (AttributeTargets.Interface | AttributeTargets.Method)]
  public class NativeMockModuleAttribute : Attribute
  {
    public string ModuleName { get; }

    public NativeMockModuleAttribute (string moduleName)
    {
      if (string.IsNullOrWhiteSpace (moduleName))
        throw new ArgumentNullException (nameof(moduleName));

      ModuleName = moduleName;
    }
  }
}
