namespace NativeMock
{
  using System;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.InteropServices;

  /// <inheritdoc />
  internal class PInvokeMemberProvider : IPInvokeMemberProvider
  {
    private const BindingFlags c_pInvokeBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

    /// <inheritdoc />
    public ImmutableArray<PInvokeMember> GetPInvokeMembers (Type type)
    {
      if (type == null)
        throw new ArgumentNullException (nameof(type));

      return type.GetMethods (c_pInvokeBindingFlags)
        .Where (m => (m.Attributes & MethodAttributes.PinvokeImpl) != 0)
        .Select (ToPInvokeMember)
        .ToImmutableArray();
    }

    private PInvokeMember ToPInvokeMember (MethodInfo method)
    {
      var dllImportAttribute = method.GetCustomAttribute<DllImportAttribute>();
      var name = dllImportAttribute?.EntryPoint ?? method.Name;
      return new PInvokeMember (name, method);
    }
  }
}
