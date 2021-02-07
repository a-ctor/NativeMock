namespace NativeMock
{
  using System;

  [AttributeUsage (AttributeTargets.Interface)]
  public class NativeMockInterfaceAttribute : Attribute
  {
    public Type? DeclaringType { get; set; }

    public NativeMockInterfaceAttribute()
    {
    }
  }
}
