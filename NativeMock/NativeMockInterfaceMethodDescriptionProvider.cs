namespace NativeMock
{
  using System;
  using System.Linq;
  using System.Reflection;

  public class NativeMockInterfaceMethodDescriptionProvider : INativeMockInterfaceMethodDescriptionProvider
  {
    private readonly IPInvokeMemberProvider _pInvokeMemberProvider;

    public NativeMockInterfaceMethodDescriptionProvider (IPInvokeMemberProvider pInvokeMemberProvider)
    {
      if (pInvokeMemberProvider == null)
        throw new ArgumentNullException (nameof(pInvokeMemberProvider));

      _pInvokeMemberProvider = pInvokeMemberProvider;
    }

    /// <inheritdoc />
    public NativeMockInterfaceMethodDescription GetMockInterfaceDescription (MethodInfo method, Type? defaultDeclaringType)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));

      var nativeMockCallbackAttribute = method.GetCustomAttribute<NativeMockCallbackAttribute>();

      var functionName = nativeMockCallbackAttribute?.Name ?? method.Name;
      var declaringType = nativeMockCallbackAttribute?.DeclaringType ?? defaultDeclaringType;
      return new NativeMockInterfaceMethodDescription (
        functionName,
        method,
        ResolveMethod (method, functionName, declaringType));
    }

    private MethodInfo ResolveMethod (MethodInfo originalMethod, string functionName, Type? declaringType)
    {
      if (declaringType == null)
        return originalMethod;

      var pInvokeMembers = _pInvokeMemberProvider.GetPInvokeMembers (declaringType);
      var pInvokeMember = pInvokeMembers.FirstOrDefault (m => m.Name == functionName);
      if (pInvokeMember == null)
        throw new InvalidOperationException ($"Cannot find the P/Invoke method '{functionName}' on the type '{declaringType}'.");

      return pInvokeMember.Method;
    }
  }
}
