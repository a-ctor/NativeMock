namespace NativeMock
{
  using System;
  using System.Reflection;

  public class NativeMockInterfaceMethodDescriptionProvider : INativeMockInterfaceMethodDescriptionProvider
  {
    /// <inheritdoc />
    public NativeMockInterfaceMethodDescription GetMockInterfaceDescription (MethodInfo method)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));

      var nativeMockCallbackAttribute = method.GetCustomAttribute<NativeMockCallbackAttribute>();

      var name = nativeMockCallbackAttribute?.Name ?? method.Name;
      return new (name, method);
    }
  }
}
