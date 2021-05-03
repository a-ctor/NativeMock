namespace NativeMock
{
  using System;

  /// <summary>
  /// Allows overriding defaults for native mock interface methods when defining a native mock interface.
  /// </summary>
  /// <remarks>
  /// It is not necessary to apply this attribute to all methods of a mock interface.
  /// By default all methods are part of the mock interface and do not need an extra attribute.
  /// This attribute is only necessary if default metadata should be overriden.
  /// </remarks>
  [AttributeUsage (AttributeTargets.Method)]
  public class NativeMockCallbackAttribute : Attribute
  {
    /// <summary>
    /// Indicates the name of the native function that should be mocked.
    /// </summary>
    /// <remarks>
    /// By default the mock interface method name is used to identify the native function to be mocked.
    /// By explicitly setting the name of the native function the interface methods name and the target native function can be
    /// different.
    /// </remarks>
    /// <example>
    /// In this example the default function name "Test" is overriden with "Test2":
    /// <code>
    /// [DllImport("mytest.dll")]
    /// public static int Test2();
    /// 
    /// [NativeMockInterface]
    /// public interface IMyTestMockInterface
    /// {
    ///   [NativeMockCallback ("Test2")]
    ///   int Test();
    /// }
    /// </code>
    /// </example>
    public string? EntryPoint { get; }

    /// <summary>
    /// Determines the behavior of the native function when it is called when no mock is set up.
    /// </summary>
    public NativeMockBehavior Behavior { get; set; } = NativeMockBehavior.Default;

    /// <summary>
    /// The type that declares the specified native function, or <see langword="null" /> if the interface
    /// method's signature and attributes should be used to determine the native function's signature.
    /// </summary>
    /// <remarks>
    /// By default the annotated method's signature and attributes will be used to mock the native function.
    /// By specifying <see cref="DeclaringType" /> an existing native function declaration on the specified type will be used
    /// instead.
    /// This way, it is not necessary to duplicate marshaling attributes in the mock interface method definition.
    /// </remarks>
    /// <example>
    /// In this example a declaring type is specified instead of duplicating the required marshaling attributes on the mock
    /// interface:
    /// <code>
    /// public class MyTestNativeFunctions
    /// {
    ///   [DllImport("mytest.dll")]
    ///   [return: MarshalAs (UnmanagedType.Bool)]
    ///   public static bool Test();
    /// }
    /// 
    /// [NativeMockInterface]
    /// public interface IMyTestMockInterface
    /// {
    ///   [NativeMockCallback (DeclaringType = typeof (MyTestNativeFunctions))]
    ///   bool Test();
    /// }
    /// </code>
    /// </example>
    public Type? DeclaringType { get; set; }

    public NativeMockCallbackAttribute()
    {
    }

    public NativeMockCallbackAttribute (string entryPoint)
    {
      EntryPoint = entryPoint;
    }
  }
}
