namespace NativeMock
{
  using System;

  [AttributeUsage (AttributeTargets.Interface)]
  public class NativeMockInterfaceAttribute : Attribute
  {
    public string? Module { get; }

    public NativeMockInterfaceAttribute()
    {
    }

    public NativeMockInterfaceAttribute (string module)
    {
      Module = module;
    }
  }
}
