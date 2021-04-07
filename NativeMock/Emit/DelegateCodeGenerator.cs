namespace NativeMock.Emit
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using System.Reflection;
  using System.Reflection.Emit;

  /// <inheritdoc />
  internal class DelegateCodeGenerator : IDelegateCodeGenerator
  {
    private const TypeAttributes c_classTypeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.AnsiClass;

    private const MethodAttributes c_constructorMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName;

    // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
    private const MethodImplAttributes c_runtimeManagedMethodImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed;

    private const MethodAttributes c_instanceMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;

    private static readonly Type[] s_constructorArgumentTypes = {typeof(object), typeof(IntPtr)};

    private readonly ModuleBuilder _moduleBuilder;

    public DelegateCodeGenerator (ModuleBuilder moduleBuilder)
    {
      if (moduleBuilder == null)
        throw new ArgumentNullException (nameof(moduleBuilder));

      _moduleBuilder = moduleBuilder;
    }

    /// <inheritdoc />
    public Type CreateDelegateType (MethodInfo methodInfo)
    {
      var returnParameter = methodInfo.ReturnParameter;
      var returnType = methodInfo.ReturnType;
      var parameters = methodInfo.GetParameters();
      var parameterTypes = parameters.Select (p => p.ParameterType).ToArray();

      // See also DelegateHelpers.MakeNewCustomDelegate in core clr
      var delegateTypeBuilder = _moduleBuilder.DefineType ($"{methodInfo.Name}_NativeFunctionProxyDelegate_{Guid.NewGuid()}", c_classTypeAttributes, typeof(MulticastDelegate));

      // Create the constructor
      var constructorBuilder = delegateTypeBuilder.DefineConstructor (c_constructorMethodAttributes, CallingConventions.Standard, s_constructorArgumentTypes);
      constructorBuilder.SetImplementationFlags (c_runtimeManagedMethodImplAttributes);

      // Create the Invoke method
      var invokeMethodBuilder = delegateTypeBuilder.DefineMethod ("Invoke", c_instanceMethodAttributes, returnType, parameterTypes);
      invokeMethodBuilder.SetImplementationFlags (c_runtimeManagedMethodImplAttributes);

      var returnBuilder = invokeMethodBuilder.DefineParameter (0, returnParameter.Attributes, null);
      ApplyParameterMetadata (returnBuilder, returnParameter);

      for (var i = 0; i < parameters.Length; i++)
      {
        var parameter = parameters[i];
        var parameterBuilder = invokeMethodBuilder.DefineParameter (i + 1, parameter.Attributes, parameter.Name);
        ApplyParameterMetadata (parameterBuilder, parameter);
      }

      var delegateType = delegateTypeBuilder.CreateType();
      if (delegateType == null)
        throw new InvalidOperationException ("Could not create the requested delegate type.");

      return delegateType;
    }

    private void ApplyParameterMetadata (ParameterBuilder parameterBuilder, ParameterInfo parameterInfo)
    {
      foreach (var customAttribute in parameterInfo.CustomAttributes)
        parameterBuilder.SetCustomAttribute (CreateCustomAttributeBuilder (customAttribute));
    }

    private CustomAttributeBuilder CreateCustomAttributeBuilder (CustomAttributeData customAttributeData)
    {
      var propertyArguments = customAttributeData.NamedArguments.Where (e => !e.IsField).ToArray();
      var fieldArguments = customAttributeData.NamedArguments.Where (e => e.IsField).ToArray();
      Debug.Assert (propertyArguments.Length + fieldArguments.Length == customAttributeData.NamedArguments.Count);

      return new CustomAttributeBuilder (
        customAttributeData.Constructor,
        customAttributeData.ConstructorArguments.Select (e => e.Value).ToArray(),
        propertyArguments.Select (p => (PropertyInfo) p.MemberInfo).ToArray(),
        propertyArguments.Select (p => p.TypedValue.Value).ToArray(),
        fieldArguments.Select (f => (FieldInfo) f.MemberInfo).ToArray(),
        fieldArguments.Select (f => f.TypedValue.Value).ToArray());
    }
  }
}
