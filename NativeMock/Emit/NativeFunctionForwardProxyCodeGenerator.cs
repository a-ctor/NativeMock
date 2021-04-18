namespace NativeMock.Emit
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Reflection.Emit;

  /// <inheritdoc />
  internal class NativeFunctionForwardProxyCodeGenerator : INativeFunctionForwardProxyCodeGenerator
  {
    private readonly IDelegateFactory _delegateFactory;
    private readonly MethodInfo _getForwardProxyMethod;

    public NativeFunctionForwardProxyCodeGenerator (IDelegateFactory delegateFactory, MethodInfo getForwardProxyMethod)
    {
      if (getForwardProxyMethod == null)
        throw new ArgumentNullException (nameof(getForwardProxyMethod));
      if (!getForwardProxyMethod.IsStatic)
        throw new ArgumentException ("Handler provider method must be static.", nameof(getForwardProxyMethod));
      if (!getForwardProxyMethod.IsGenericMethodDefinition || getForwardProxyMethod.GetGenericArguments().Length != 1)
        throw new ArgumentException ("Handler provider method must be generic method definition with one type parameter.", nameof(getForwardProxyMethod));
      if (getForwardProxyMethod.GetParameters().Length != 0)
        throw new ArgumentException ("Handler provider method must not have any parameters.", nameof(getForwardProxyMethod));
      if (!getForwardProxyMethod.ReturnType.IsGenericParameter || getForwardProxyMethod.ReturnType.IsByRef)
        throw new ArgumentException ("Handler provider method must have a return type of T.", nameof(getForwardProxyMethod));

      _delegateFactory = delegateFactory;
      _getForwardProxyMethod = getForwardProxyMethod;
    }

    public Delegate GenerateNativeFunctionForwardProxy (MethodInfo method)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));
      if (method.DeclaringType == null || !method.DeclaringType.IsInterface)
        throw new ArgumentException ("The methods declaring type must be an interface.");

      var interfaceMethod = method;
      var interfaceType = interfaceMethod.DeclaringType;
      if (interfaceType == null)
        throw new InvalidOperationException ("The specified method has not declaring type.");
      if (!interfaceType.IsInterface)
        throw new InvalidOperationException ("The specified method's declaring type is not an interface.");

      var returnType = interfaceMethod.ReturnType;
      var parameters = interfaceMethod.GetParameters();
      var parameterTypes = parameters.Select (e => e.ParameterType).ToArray();

      var proxyMethod = new DynamicMethod ($"{method.Name}_NativeFunctionForwardProxy", returnType, parameterTypes);
      var ilGenerator = proxyMethod.GetILGenerator();

      // var temp0 = NativeMockRegistry.GetMockForwardProxy<T>();
      ilGenerator.Emit (OpCodes.Call, _getForwardProxyMethod.MakeGenericMethod (interfaceType));

      // return temp0.XXX(args);
      for (short i = 0; i < parameters.Length; i++)
        ilGenerator.Emit (OpCodes.Ldarg, i);
      ilGenerator.Emit (OpCodes.Callvirt, interfaceMethod);
      ilGenerator.Emit (OpCodes.Ret);

      var delegateType = _delegateFactory.CreateDelegateType (method);
      return proxyMethod.CreateDelegate (delegateType);
    }
  }
}
