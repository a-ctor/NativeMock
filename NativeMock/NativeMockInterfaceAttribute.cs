namespace NativeMock
{
  using System;

  /// <summary>
  /// Marks the specified interface as a native mock interface and optionally sets default settings for containing mock
  /// functions.
  /// </summary>
  /// <remarks>
  /// All methods of an interface marked with the <see cref="NativeMockInterfaceAttribute" /> will be part of the mock
  /// interface.
  /// Their defaults can be overriden using the <see cref="NativeMockCallbackAttribute" />.
  /// Without a <see cref="NativeMockModuleAttribute" /> applied to the interface, the methods will be matched using only
  /// their name.
  /// By applying a <see cref="NativeMockModuleAttribute" /> the mock interface is only used for the specified native module.
  /// </remarks>
  /// <seealso cref="NativeMockCallbackAttribute" />
  [AttributeUsage (AttributeTargets.Interface)]
  public class NativeMockInterfaceAttribute : Attribute
  {
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

    public NativeMockInterfaceAttribute()
    {
    }
  }
}
