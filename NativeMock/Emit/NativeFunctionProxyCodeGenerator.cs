namespace NativeMock.Emit
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Reflection.Emit;
  using Representation;

  /// <inheritdoc />
  internal class NativeFunctionProxyCodeGenerator : INativeFunctionProxyCodeGenerator
  {
    private static readonly ConstructorInfo s_nativeFunctionNotMockedExceptionConstructor;

    static NativeFunctionProxyCodeGenerator()
    {
      s_nativeFunctionNotMockedExceptionConstructor = typeof(NativeFunctionNotMockedException).GetConstructor (new[] {typeof(string)})
                                                      ?? throw new NotSupportedException ("Constructor NativeFunctionNotMockedException(string) could not be found.");
    }

    private readonly MethodInfo _handlerProviderMethod;
    private readonly MethodInfo _getForwardProxyMethod;

    public NativeFunctionProxyCodeGenerator (MethodInfo handlerProviderMethod, MethodInfo getForwardProxyMethod)
    {
      if (handlerProviderMethod == null)
        throw new ArgumentNullException (nameof(handlerProviderMethod));
      if (getForwardProxyMethod == null)
        throw new ArgumentNullException (nameof(getForwardProxyMethod));
      if (!handlerProviderMethod.IsStatic)
        throw new ArgumentException ("Handler provider method must be static.", nameof(handlerProviderMethod));
      if (!handlerProviderMethod.IsGenericMethodDefinition || handlerProviderMethod.GetGenericArguments().Length != 1)
        throw new ArgumentException ("Handler provider method must be generic method definition with one type parameter.", nameof(handlerProviderMethod));
      if (handlerProviderMethod.GetParameters().Length != 0)
        throw new ArgumentException ("Handler provider method must not have any parameters.", nameof(handlerProviderMethod));
      if (!handlerProviderMethod.ReturnType.IsGenericParameter || handlerProviderMethod.ReturnType.IsByRef)
        throw new ArgumentException ("Handler provider method must have a return type of T.", nameof(handlerProviderMethod));
      if (!getForwardProxyMethod.IsStatic)
        throw new ArgumentException ("Handler provider method must be static.", nameof(getForwardProxyMethod));
      if (!getForwardProxyMethod.IsGenericMethodDefinition || getForwardProxyMethod.GetGenericArguments().Length != 1)
        throw new ArgumentException ("Handler provider method must be generic method definition with one type parameter.", nameof(getForwardProxyMethod));
      if (getForwardProxyMethod.GetParameters().Length != 0)
        throw new ArgumentException ("Handler provider method must not have any parameters.", nameof(getForwardProxyMethod));
      if (!getForwardProxyMethod.ReturnType.IsGenericParameter || getForwardProxyMethod.ReturnType.IsByRef)
        throw new ArgumentException ("Handler provider method must have a return type of T.", nameof(getForwardProxyMethod));

      _handlerProviderMethod = handlerProviderMethod;
      _getForwardProxyMethod = getForwardProxyMethod;
    }

    public Delegate CreateProxyMethod (NativeMockInterfaceMethodDescription method, Type nativeFunctionDelegateType)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));
      if (nativeFunctionDelegateType == null)
        throw new ArgumentNullException (nameof(nativeFunctionDelegateType));
      if (!nativeFunctionDelegateType.IsSubclassOf (typeof(Delegate)))
        throw new ArgumentException ("The specified native function type must be a delegate", nameof(nativeFunctionDelegateType));

      var interfaceMethod = method.InterfaceMethod;
      var interfaceType = interfaceMethod.DeclaringType;
      if (interfaceType == null)
        throw new InvalidOperationException ("The specified method has not declaring type.");
      if (!interfaceType.IsInterface)
        throw new InvalidOperationException ("The specified method's declaring type is not an interface.");

      var returnType = interfaceMethod.ReturnType;
      var parameters = interfaceMethod.GetParameters();
      var parameterTypes = parameters.Select (e => e.ParameterType).ToArray();

      var proxyMethod = new DynamicMethod ($"{method.Name}_NativeFunctionProxy", returnType, parameterTypes);
      var ilGenerator = proxyMethod.GetILGenerator();

      var noMockObjectLabel = ilGenerator.DefineLabel();

      var mockObjectLocal = ilGenerator.DeclareLocal (interfaceType);

      // var mockObject = NativeMockRegistry.GetMockObject<T>();
      ilGenerator.Emit (OpCodes.Call, _handlerProviderMethod.MakeGenericMethod (interfaceType));
      ilGenerator.Emit (OpCodes.Stloc, mockObjectLocal);
      ilGenerator.Emit (OpCodes.Ldloc, mockObjectLocal);

      // if (mockObject != null) {
      ilGenerator.Emit (OpCodes.Brfalse_S, noMockObjectLabel);

      //   return mockObject.XXX(arg1, arg2, ...)
      ilGenerator.Emit (OpCodes.Ldloc, mockObjectLocal);
      for (short i = 0; i < parameters.Length; i++)
        ilGenerator.Emit (OpCodes.Ldarg, i);
      ilGenerator.Emit (OpCodes.Callvirt, interfaceMethod);
      ilGenerator.Emit (OpCodes.Ret);
      // }

      // Throw an exception
      ilGenerator.MarkLabel (noMockObjectLabel);

      // Create the no mock found handling code depending on the behavior
      if (method.Behavior == NativeMockBehavior.Default || method.Behavior == NativeMockBehavior.Strict)
      {
        // throw new NativeFunctionNotMockedException("XXX");
        ilGenerator.Emit (OpCodes.Ldstr, method.Name.ToString());
        ilGenerator.Emit (OpCodes.Newobj, s_nativeFunctionNotMockedExceptionConstructor);
        ilGenerator.Emit (OpCodes.Throw);
      }
      else if (method.Behavior == NativeMockBehavior.Loose)
      {
        if (returnType == typeof(void))
        {
          // return;
          ilGenerator.Emit (OpCodes.Ret);
        }
        else if (returnType.IsValueType)
        {
          // return default;
          var resultLocal = ilGenerator.DeclareLocal (returnType);
          ilGenerator.Emit (OpCodes.Ldloca_S, resultLocal);
          ilGenerator.Emit (OpCodes.Initobj, returnType);
          ilGenerator.Emit (OpCodes.Ldloc, resultLocal);
          ilGenerator.Emit (OpCodes.Ret);
        }
        else
        {
          // return null;
          ilGenerator.Emit (OpCodes.Ldnull);
          ilGenerator.Emit (OpCodes.Ret);
        }
      }
      else if (method.Behavior == NativeMockBehavior.Forward)
      {
        // var temp0 = NativeMockRegistry.GetMockForwardProxy<T>();
        ilGenerator.Emit (OpCodes.Call, _getForwardProxyMethod.MakeGenericMethod (interfaceType));

        // return temp0.XXX(args);
        for (short i = 0; i < parameters.Length; i++)
          ilGenerator.Emit (OpCodes.Ldarg, i);
        ilGenerator.Emit (OpCodes.Callvirt, interfaceMethod);
        ilGenerator.Emit (OpCodes.Ret);
      }
      else
      {
        throw new InvalidOperationException ($"Invalid native mock behavior '{method.Behavior}' specified.");
      }

      return proxyMethod.CreateDelegate (nativeFunctionDelegateType);
    }
  }
}
