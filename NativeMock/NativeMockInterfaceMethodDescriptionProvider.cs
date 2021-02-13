namespace NativeMock
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Text;

  /// <inheritdoc />
  internal class NativeMockInterfaceMethodDescriptionProvider : INativeMockInterfaceMethodDescriptionProvider
  {
    private readonly IPInvokeMemberProvider _pInvokeMemberProvider;
    private readonly INativeMockModuleDescriptionProvider _mockModuleDescriptionProvider;

    public NativeMockInterfaceMethodDescriptionProvider (IPInvokeMemberProvider pInvokeMemberProvider, INativeMockModuleDescriptionProvider mockModuleDescriptionProvider)
    {
      if (pInvokeMemberProvider == null)
        throw new ArgumentNullException (nameof(pInvokeMemberProvider));

      _pInvokeMemberProvider = pInvokeMemberProvider;
      _mockModuleDescriptionProvider = mockModuleDescriptionProvider;
    }

    /// <inheritdoc />
    public NativeMockInterfaceMethodDescription GetMockInterfaceDescription (MethodInfo method, Type? defaultDeclaringType, NativeMockModuleDescription? defaultModuleDescription)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));

      var nativeMockCallbackAttribute = method.GetCustomAttribute<NativeMockCallbackAttribute>();
      var nativeMockModuleDescription = _mockModuleDescriptionProvider.GetMockModuleDescriptionForMethod (method);

      var functionName = nativeMockCallbackAttribute?.Name ?? method.Name;
      var declaringType = nativeMockCallbackAttribute?.DeclaringType ?? defaultDeclaringType;
      var moduleDescription = nativeMockModuleDescription ?? defaultModuleDescription;
      return new NativeMockInterfaceMethodDescription (
        functionName,
        moduleDescription,
        method,
        ResolveMethod (method, functionName, declaringType));
    }

    private MethodInfo ResolveMethod (MethodInfo originalMethod, string functionName, Type? declaringType)
    {
      if (declaringType == null)
        return originalMethod;

      var pInvokeMembers = _pInvokeMemberProvider.GetPInvokeMembers (declaringType);
      var pInvokeMember = pInvokeMembers.FirstOrDefault (m => m.Name.FunctionName == functionName);
      if (pInvokeMember == null)
        throw new InvalidOperationException ($"Cannot find the P/Invoke method '{functionName}' on the type '{declaringType}'.");

      var resolvedMethod = pInvokeMember.Method;
      EnsureMethodIsCompatible (originalMethod, resolvedMethod);

      return resolvedMethod;
    }

    private void EnsureMethodIsCompatible (MethodInfo original, MethodInfo declaration)
    {
      var originalParameters = original.GetParameters();
      var declarationParameters = declaration.GetParameters();
      if (originalParameters.Length != declarationParameters.Length)
        throw new NativeMockDeclarationMismatchException (original, declaration, "The parameter count differs.");

      var originalReturnParameter = original.ReturnParameter;
      var declarationReturnParameter = declaration.ReturnParameter;
      if (!IsParameterEqual (originalReturnParameter, declarationReturnParameter))
      {
        throw new NativeMockDeclarationMismatchException (
          original,
          declaration,
          $"The return type differs ({FormatComparison (originalReturnParameter, declarationReturnParameter)}).");
      }

      for (var i = 0; i < originalParameters.Length; i++)
      {
        var originalParameter = originalParameters[i];
        var declarationParameter = declarationParameters[i];
        if (!IsParameterEqual (originalParameter, declarationParameter))
        {
          throw new NativeMockDeclarationMismatchException (
            original,
            declaration,
            $"The return type differs ({FormatComparison (originalParameter, declarationParameter)}).");
        }
      }
    }

    private bool IsParameterEqual (ParameterInfo original, ParameterInfo declaration)
    {
      return original.ParameterType == declaration.ParameterType
             && original.IsIn == declaration.IsIn
             && original.IsOut == declaration.IsOut;
    }

    private string FormatComparison (ParameterInfo original, ParameterInfo declaration)
    {
      return $"'{FormatParameter (original)}' vs '{FormatParameter (declaration)}'";
    }

    private string FormatParameter (ParameterInfo parameter)
    {
      var stringBuilder = new StringBuilder();

      if (parameter.IsIn)
        stringBuilder.Append ("in ");
      if (parameter.IsOut)
        stringBuilder.Append ("out ");
      stringBuilder.Append (parameter.ParameterType);

      return stringBuilder.ToString();
    }
  }
}
