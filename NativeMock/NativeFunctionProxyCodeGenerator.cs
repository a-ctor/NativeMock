namespace NativeMock
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Reflection.Emit;

  /// <summary>
  /// Provides methods for generating a <see cref="NativeFunctionProxy" />s IL.
  /// </summary>
  internal class NativeFunctionProxyCodeGenerator
  {
    private static readonly MethodInfo s_getMockObjectMethod;
    private static readonly ConstructorInfo s_nativeFunctionNotMockedExceptionConstructor;

    static NativeFunctionProxyCodeGenerator()
    {
      s_getMockObjectMethod = typeof(NativeMockRepository).GetMethod (nameof(NativeMockRepository.GetMockObject), BindingFlags.Static | BindingFlags.NonPublic)!
                              ?? throw new NotSupportedException ("NativeMockRepository.GetMockObject could not be found.");
      s_nativeFunctionNotMockedExceptionConstructor = typeof(NativeFunctionNotMockedException).GetConstructor (new[] {typeof(string)})
                                                      ?? throw new NotSupportedException ("Constructor NativeFunctionNotMockedException(string) could not be found.");
    }

    public NativeFunctionProxyCodeGenerator()
    {
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

      // var mockObject = NativeMockRepository.GetMockObject<T>();
      ilGenerator.Emit (OpCodes.Call, s_getMockObjectMethod.MakeGenericMethod (interfaceType));
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
      else
      {
        throw new InvalidOperationException ($"Invalid native mock behavior '{method.Behavior}' specified.");
      }

      return proxyMethod.CreateDelegate (nativeFunctionDelegateType);
    }
  }
}
