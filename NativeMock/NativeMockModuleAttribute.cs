namespace NativeMock
{
  using System;

  [AttributeUsage (AttributeTargets.Interface)]
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
