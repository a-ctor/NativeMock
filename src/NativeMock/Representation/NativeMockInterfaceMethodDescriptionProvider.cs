namespace NativeMock.Representation
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Text;

  /// <inheritdoc />
  internal class NativeMockInterfaceMethodDescriptionProvider : INativeMockInterfaceMethodDescriptionProvider
  {
    private readonly IPInvokeMemberProvider _pInvokeMemberProvider;

    public NativeMockInterfaceMethodDescriptionProvider (IPInvokeMemberProvider pInvokeMemberProvider)
    {
      _pInvokeMemberProvider = pInvokeMemberProvider ?? throw new ArgumentNullException (nameof(pInvokeMemberProvider));
    }

    /// <inheritdoc />
    public NativeMockInterfaceMethodDescription GetMockInterfaceDescription (
      string moduleName,
      MethodInfo method,
      Type? defaultDeclaringType,
      NativeMockBehavior defaultMockBehavior)
    {
      if (moduleName == null)
        throw new ArgumentNullException (nameof(moduleName));
      if (string.IsNullOrWhiteSpace (moduleName))
        throw new ArgumentException ("Module name cannot be empty.");
      if (method == null)
        throw new ArgumentNullException (nameof(method));

      var nativeMockCallbackAttribute = method.GetCustomAttribute<NativeMockCallbackAttribute>();

      var functionName = nativeMockCallbackAttribute?.EntryPoint ?? method.Name;
      var name = new NativeFunctionIdentifier (moduleName, functionName);
      var nativeMockBehavior = nativeMockCallbackAttribute?.Behavior ?? NativeMockBehavior.Default;
      if (nativeMockBehavior == NativeMockBehavior.Default)
        nativeMockBehavior = defaultMockBehavior;

      var declaringType = nativeMockCallbackAttribute?.DeclaringType ?? defaultDeclaringType;
      return new NativeMockInterfaceMethodDescription (
        method,
        ResolveMethod (method, name, declaringType),
        nativeMockBehavior,
        name);
    }

    private MethodInfo ResolveMethod (MethodInfo originalMethod, NativeFunctionIdentifier name, Type? declaringType)
    {
      if (declaringType == null)
        return originalMethod;

      var pInvokeMembers = _pInvokeMemberProvider.GetPInvokeMembers (declaringType);

      var targetPInvokeMemberMatches = pInvokeMembers.Where (e => e.Name == name).ToArray();
      if (targetPInvokeMemberMatches.Length == 0)
        throw new InvalidOperationException ($"Cannot find the P/Invoke method '{name}' on the type '{declaringType}'.");
      if (targetPInvokeMemberMatches.Length > 1)
        throw new InvalidOperationException ($"Multiple P/Invoke methods for '{name}' found on the type '{declaringType}'.");

      var resolvedMethod = targetPInvokeMemberMatches[0].Method;
      EnsureMethodIsCompatible (originalMethod, resolvedMethod);

      return resolvedMethod;
    }

    private static void EnsureMethodIsCompatible (MethodInfo original, MethodInfo declaration)
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

    private static bool IsParameterEqual (ParameterInfo? original, ParameterInfo? declaration)
    {
      return original != null && declaration != null
             && original.ParameterType == declaration.ParameterType
             && original.IsIn == declaration.IsIn
             && original.IsOut == declaration.IsOut;
    }

    private static string FormatComparison (ParameterInfo? original, ParameterInfo? declaration)
    {
      return $"'{FormatParameter (original)}' vs '{FormatParameter (declaration)}'";
    }

    private static string FormatParameter (ParameterInfo? parameter)
    {
      if (parameter == null)
        return string.Empty;

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
