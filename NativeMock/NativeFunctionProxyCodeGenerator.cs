namespace NativeMock
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Reflection.Emit;

  /// <summary>
  /// Provides methods for generating a <see cref="NativeFunctionProxy" />s IL.
  /// </summary>
  internal class NativeFunctionProxyCodeGenerator
  {
    // IMPORTANT: when changing the following lines the IL generation has to be fixed as well
    private static readonly Expression<Func<NativeFunctionIdentifier>> s_nativeFunctionIdentifierConstructor1Expression =
      () => new NativeFunctionIdentifier ("a");

    private static readonly Expression<Func<NativeFunctionIdentifier>> s_nativeFunctionIdentifierConstructor2Expression =
      () => new NativeFunctionIdentifier ("a", "b");

    private static readonly ConstructorInfo s_nativeFunctionIdentifierConstructor1Info =
      ((NewExpression) s_nativeFunctionIdentifierConstructor1Expression.Body).Constructor!;

    private static readonly ConstructorInfo s_nativeFunctionIdentifierConstructor2Info =
      ((NewExpression) s_nativeFunctionIdentifierConstructor2Expression.Body).Constructor!;

    public NativeFunctionProxyCodeGenerator()
    {
    }

    public Delegate CreateProxyMethod (NativeFunctionIdentifier name, Type nativeFunctionDelegateType, NativeFunctionHook proxyTarget)
    {
      if (name.IsInvalid)
        throw new ArgumentNullException (nameof(name));
      if (nativeFunctionDelegateType == null)
        throw new ArgumentNullException (nameof(nativeFunctionDelegateType));
      if (!nativeFunctionDelegateType.IsSubclassOf (typeof(Delegate)))
        throw new ArgumentException ("The specified native function type must be a delegate", nameof(nativeFunctionDelegateType));
      if (proxyTarget == null)
        throw new ArgumentNullException (nameof(proxyTarget));
      if (!proxyTarget.Method.IsStatic)
        throw new ArgumentException ("The specified native call proxy must have a static target.");

      var invokeMethod = nativeFunctionDelegateType.GetMethod ("Invoke");
      var returnType = invokeMethod!.ReturnType;
      var parameters = invokeMethod.GetParameters();
      var parameterTypes = parameters.Select (e => e.ParameterType).ToArray();

      var proxyMethod = new DynamicMethod ($"{name}_NativeFunctionProxy", returnType, parameterTypes);
      var ilGenerator = proxyMethod.GetILGenerator();

      if (name.ModuleName == null)
      {
        ilGenerator.Emit (OpCodes.Ldstr, name.FunctionName);
        ilGenerator.Emit (OpCodes.Newobj, s_nativeFunctionIdentifierConstructor1Info);
      }
      else
      {
        ilGenerator.Emit (OpCodes.Ldstr, name.ModuleName);
        ilGenerator.Emit (OpCodes.Ldstr, name.FunctionName);
        ilGenerator.Emit (OpCodes.Newobj, s_nativeFunctionIdentifierConstructor2Info);
      }

      // Create an array for the parameters
      ilGenerator.Emit (OpCodes.Ldc_I4, parameters.Length);
      ilGenerator.Emit (OpCodes.Newarr, typeof(object));

      // Put the function arguments into the array
      for (var i = 0; i < parameters.Length; i++)
      {
        ilGenerator.Emit (OpCodes.Dup);
        ilGenerator.Emit (OpCodes.Ldc_I4, i);
        ilGenerator.Emit (OpCodes.Ldarg, i);
        if (parameterTypes[i].IsValueType)
          ilGenerator.Emit (OpCodes.Box, parameterTypes[i]);
        ilGenerator.Emit (OpCodes.Stelem_Ref);
      }

      // Call the OnCall method
      ilGenerator.Emit (OpCodes.Call, proxyTarget.Method);

      // Convert the result into the correct type
      if (returnType == typeof(void))
      {
        ilGenerator.Emit (OpCodes.Pop);
      }
      else if (returnType.IsValueType)
      {
        ilGenerator.Emit (OpCodes.Unbox_Any, returnType);
      }
      else
      {
        ilGenerator.Emit (OpCodes.Castclass, returnType);
      }

      // Return the result
      ilGenerator.Emit (OpCodes.Ret);

      return proxyMethod.CreateDelegate (nativeFunctionDelegateType);
    }

    public Func<object> CreateDefaultStub (NativeFunctionIdentifier name, Type nativeFunctionDelegateType)
    {
      if (name.IsInvalid)
        throw new ArgumentNullException (nameof(name));
      if (nativeFunctionDelegateType == null)
        throw new ArgumentNullException (nameof(nativeFunctionDelegateType));
      if (nativeFunctionDelegateType == null)
        throw new ArgumentNullException (nameof(nativeFunctionDelegateType));
      if (!nativeFunctionDelegateType.IsSubclassOf (typeof(Delegate)))
        throw new ArgumentException ("The specified native function type must be a delegate", nameof(nativeFunctionDelegateType));

      var returnType = nativeFunctionDelegateType.GetMethod ("Invoke")!.ReturnType;

      var stubMethod = new DynamicMethod ($"{name}_Stub", typeof(object), Array.Empty<Type>());
      var ilGenerator = stubMethod.GetILGenerator();

      if (returnType == typeof(void) || !returnType.IsValueType)
      {
        ilGenerator.Emit (OpCodes.Ldnull);
      }
      else
      {
        var local = ilGenerator.DeclareLocal (returnType);
        ilGenerator.Emit (OpCodes.Ldloca_S, local);
        ilGenerator.Emit (OpCodes.Initobj, returnType);
        ilGenerator.Emit (OpCodes.Ldloc, local);
        ilGenerator.Emit (OpCodes.Box, returnType);
      }

      ilGenerator.Emit (OpCodes.Ret);

      return (Func<object>) stubMethod.CreateDelegate (typeof(Func<object>));
    }
  }
}
