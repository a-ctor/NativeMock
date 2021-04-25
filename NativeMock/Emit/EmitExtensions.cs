namespace NativeMock.Emit
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Reflection.Emit;

  public static class EmitExtensions
  {
    private const TypeAttributes c_classBaseAttributes = TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;

    public static TypeBuilder DefinePublicClass (this ModuleBuilder moduleBuilder, string name, Type parent, params Type[] interfaces)
    {
      return moduleBuilder.DefineType (
        name,
        c_classBaseAttributes | TypeAttributes.Public,
        parent,
        interfaces);
    }

    private const MethodAttributes c_constructorMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.RTSpecialName;

    public static ConstructorBuilder DefinePublicConstructor (this TypeBuilder typeBuilder, params Type[] parameters)
    {
      return typeBuilder.DefineConstructor (
        c_constructorMethodAttributes | MethodAttributes.Public,
        CallingConventions.Standard,
        parameters);
    }

    private const MethodAttributes c_implicitInterfaceMethodImplementationAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
    private const MethodAttributes c_explicitMethodImplementationAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

    public static MethodBuilder DefineImplicitInterfaceMethodImplementation (this TypeBuilder typeBuilder, Type returnType, string name, params Type[] parameters)
    {
      return typeBuilder.DefineMethod (
        name,
        c_implicitInterfaceMethodImplementationAttributes,
        returnType,
        parameters);
    }

    public static MethodBuilder DefineExplicitInterfaceMethodImplementation (this TypeBuilder typeBuilder, MethodInfo interfaceMethod)
    {
      static Type[] GetRequiredCustomModifiers (ParameterInfo? parameterInfo) => parameterInfo?.GetRequiredCustomModifiers() ?? Array.Empty<Type>();
      static Type[] GetOptionalCustomModifiers (ParameterInfo? parameterInfo) => parameterInfo?.GetOptionalCustomModifiers() ?? Array.Empty<Type>();

      var parameters = interfaceMethod.GetParameters();
      var parameterTypes = parameters.Select (e => e.ParameterType).ToArray();

      var returnTypeRequiredCustomModifiers = GetRequiredCustomModifiers (interfaceMethod.ReturnParameter);
      var returnTypeOptionalCustomModifiers = GetOptionalCustomModifiers (interfaceMethod.ReturnParameter);

      var parameterTypeRequiredCustomModifiers = parameters.Select (GetRequiredCustomModifiers).ToArray();
      var parameterTypeOptionalCustomModifiers = parameters.Select (GetOptionalCustomModifiers).ToArray();

      var methodBuilder = typeBuilder.DefineMethod (
        interfaceMethod.Name,
        c_explicitMethodImplementationAttributes,
        interfaceMethod.CallingConvention,
        interfaceMethod.ReturnType,
        returnTypeRequiredCustomModifiers,
        returnTypeOptionalCustomModifiers,
        parameterTypes,
        parameterTypeRequiredCustomModifiers,
        parameterTypeOptionalCustomModifiers);

      if (interfaceMethod.ReturnParameter != null)
        methodBuilder.DefineParameterFromParameterInfo (0, interfaceMethod.ReturnParameter);

      for (var i = 0; i < parameters.Length; i++)
        methodBuilder.DefineParameterFromParameterInfo (i + 1, parameters[i]);

      typeBuilder.DefineMethodOverride (methodBuilder, interfaceMethod);

      return methodBuilder;
    }

    public static void DefineParameterFromParameterInfo (this MethodBuilder methodBuilder, int position, ParameterInfo? parameterInfo)
    {
      if (parameterInfo == null)
        return;

      var parameterBuilder = methodBuilder.DefineParameter (position, parameterInfo.Attributes, parameterInfo.Name);

      // Apply any default value
      if (parameterInfo.IsOptional)
        parameterBuilder.SetConstant (parameterInfo.DefaultValue);
    }

    public static FieldBuilder DefinePrivateField (this TypeBuilder typeBuilder, Type type, string name)
    {
      return typeBuilder.DefineField (name, type, FieldAttributes.Private);
    }
  }
}
