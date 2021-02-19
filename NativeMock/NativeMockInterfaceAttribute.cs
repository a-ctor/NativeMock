namespace NativeMock
{
  using System;
  using System.Runtime.InteropServices;

  /// <summary>
  /// Marks the specified interface as a native mock interface and optionally sets default settings for containing native
  /// function mocks.
  /// </summary>
  /// <remarks>
  /// All methods of an interface marked with the <see cref="NativeMockInterfaceAttribute" /> will be part of the mock
  /// interface.
  /// Their defaults can be overriden using the <see cref="NativeMockCallbackAttribute" />.
  /// </remarks>
  /// <seealso cref="NativeMockCallbackAttribute" />
  [AttributeUsage (AttributeTargets.Interface)]
  public class NativeMockInterfaceAttribute : Attribute
  {
    /// <summary>
    /// Determines the default <see cref="NativeMockBehavior" /> of the containing members.
    /// </summary>
    /// <remarks>
    /// This setting can be overriden on a per-method basis using <see cref="NativeMockCallbackAttribute" />.
    /// <see cref="NativeMockCallbackAttribute.Behavior" />.
    /// </remarks>
    /// <seealso cref="NativeMockCallbackAttribute.Behavior" />
    public NativeMockBehavior Behavior { get; set; } = NativeMockBehavior.Default;

    /// <summary>
    /// The type that declares that declares the native functions specified by this interface, or <see langword="null" /> if
    /// the mock interface's methods should be used to determine the signature of the native functions.
    /// </summary>
    /// <remarks>
    /// This setting can be overriden on a per-method basis using <see cref="NativeMockCallbackAttribute" />.
    /// <see cref="NativeMockCallbackAttribute.DeclaringType" />.
    /// </remarks>
    /// <seealso cref="NativeMockCallbackAttribute.DeclaringType" />
    public Type? DeclaringType { get; set; }

    /// <summary>
    /// The name of the module that should be mocked using this interface.
    /// </summary>
    /// <remarks>
    /// The specified module must match the name specified in the <see cref="DllImportAttribute" />.
    /// </remarks>
    public string Module { get; }

    public NativeMockInterfaceAttribute (string module)
    {
      if (module == null)
        throw new ArgumentNullException (nameof(module));
      if (string.IsNullOrWhiteSpace (module))
        throw new ArgumentException ("Module must not be empty.");

      Module = module;
    }
  }
}
