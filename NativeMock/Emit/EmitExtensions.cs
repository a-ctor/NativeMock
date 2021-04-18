namespace NativeMock.Emit
{
  using System;
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

    public static MethodBuilder DefineExplicitInterfaceMethodImplementation (this TypeBuilder typeBuilder, Type returnType, string name, params Type[] parameters)
    {
      return typeBuilder.DefineMethod (
        name,
        c_explicitMethodImplementationAttributes,
        returnType,
        parameters);
    }

    public static FieldBuilder DefinePrivateField (this TypeBuilder typeBuilder, Type type, string name)
    {
      return typeBuilder.DefineField (name, type, FieldAttributes.Private);
    }
  }
}
