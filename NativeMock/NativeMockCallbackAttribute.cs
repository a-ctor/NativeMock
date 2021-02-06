namespace NativeMock
{
  using System;

  [AttributeUsage (AttributeTargets.Method)]
  public class NativeMockCallbackAttribute : Attribute
  {
    public string? Name { get; }

    public NativeMockCallbackAttribute()
    {
    }

    public NativeMockCallbackAttribute (string name)
    {
      Name = name;
    }
  }
}
