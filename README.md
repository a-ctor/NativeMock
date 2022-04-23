# NativeMock

A .NET 5/.NET Framework 4.6.2 library that allows you to mock native function using custom interfaces.

Lets say you have a dependency on the following native function:

```c#
[DllImport("test.dll")]
public static extern int MyExternalFunction([MarshalAs (UnmanagedType.LPUTF8Str)] string s);
```

To test your code you have to deal with this dependency. Common solutions are loading the actual DLL in your tests or creating a custom DLL just for testing purposes. Both solutions are tedious and make testing harder.

*NativeMock* simplifies such scenarios by allowing you to mock native function by intercepting calls to the actual DLL. It also supports loading dummy DLLs that can be used instead of the actual DLL.

## Example

The following example shows how you can test the previous native function. To simplify testing a mocking framework can be used. The following example assumes the *NUnit* unit test framework together with *Moq* as mocking framework, but neither are required.

```c#
[NativeMockInterface ("test.dll")]
public interface INativeLibraryApi
{
  int MyExternalFunction ([MarshalAs (UnmanagedType.LPUTF8Str)] string s);
}

[Test]
public void MyExternalFunctionTest()
{
  var mock = new Mock<T> (MockBehavior.Strict);
  mock.Setup(e => e.MyExternalFunction("Test")).Return(5);
  NativeMockRepository.Setup (mock.Object);

  Assert.That(MyExternalFunction("Test"), Is.EqualTo(5));
  mock.VerifyAll();
}
```

## Limitations

- **Windows only**: Currently, the library only supports windows as the import address table (IAT) hook is only implemented for windows.
- **Initialization necessary:** To intercept calls using IAT hooks the initialization must be done **before** any of the native methods are called. For the same reason dynamically generated interfaces cannot be used.

## Installation

1. Install the Nuget package or manually reference the binaries
2. Initialize the library and register your native mock interfaces. **Do this as early as possible** (preferably in a module initializer). Any native function that is called before the initialization of *NativeMock* is completed and the corresponding interface is registered **will no longer by mockable**. This is a technical limitation imposed by the way native functions are mocked.

Example initialization setup with auto-registration:

```c#
public static class ModuleInitializer
{
  [ModuleInitializer]
  public static void Initialize()
  {
    NativeMockRepository.Initialize();
    
    // auto-detect suitable interfaces
    NativeMockRepository.RegisterFromAssembly (typeof(ModuleInitializer).Assembly);
      
    // or register them manually
    // NativeMockRepository.Register<INativeLibraryApi>();
  }
}
```

## Usage

> Hint: take a look at the `NativeMockRepository` and `NativeLibraryDummy` classes

To mock any native function call create an interface for it an apply the `NativeMockInterface` attribute to it. The DLL name provided with the attribute must match the native library used in the P/Invoke method declaration. The method signature should match up with its native counterpart. This includes any marshalling attribute applied to the parameters or the return value. Otherwise bad things are going to happen - see *declaring type* examples below on a good way to minimize the risk of non-matching signatures.

```c#
[DllImport("test.dll")]
public static extern int MyExternalFunction ([MarshalAs (UnmanagedType.LPUTF8Str)] string s);

[NativeMockInterface ("test.dll")]
public interface INativeLibraryApi
{
  int MyExternalFunction ([MarshalAs (UnmanagedType.LPUTF8Str)] string s);
}
```

It is possible to override settings for a specific method by using the `NativeMockCallback` attribute. This allows you to use different names for the native function and interface method. This is especially useful if the native function contains a prefix/suffix which would unnecessarily pollute the method name.

```c#
[DllImport("test.dll")]
public static extern int MTL_InitializeUtf8 ([MarshalAs (UnmanagedType.LPUTF8Str)] string s);

[NativeMockInterface ("test.dll")]
public interface INativeLibraryApi
{
  [NativeMockCallback ("MTL_InitializeUtf8")]
  int MyExternalFunction ([MarshalAs (UnmanagedType.LPUTF8Str)] string s);
}
```

Instead of copying marshaling attributes onto the method declaration it is possible to let *NativeMock* detect them from the native function declaration. This can be done by specifying a `DeclaringType` on either the `NativeMockInterface` attribute or the `NativeMockCallback` attribute. Settings from the interface attribute are inherited to each member which can override them with their own settings if needed. 

To provide a level of safety, the method signatures must match the signatures of their native counterparts. If any of the parameter or return types does not match up, the registration will fail. If possible **always** provide a declaring type since it makes sure that declared native functions exist and that the marshaling works correctly. Differing marshaling attributes can cause hard to diagnose problems.

```c#
public class MyExternalFunctions
{
  [DllImport("test.dll")]
  public static extern int MyExternalFunction ([MarshalAs (UnmanagedType.LPUTF8Str)] string s);
}

[NativeMockInterface ("test.dll", DeclaringType = typeof(MyExternalFunctions))]
public interface INativeLibraryApi
{
  [NativeMockCallback(DeclaringType = ...)] // we can override the inherited value
  int MyExternalFunction (string s);
}
```

Native function mocks are created for the lifetime of the program and are always active. This is a limitation imposed by the static nature of native functions. If no native mock interface is registered it will behave according to its `Behavior` set using the either the `NativeMockInterface` attribute or the `NativeMockCallback` attribute.

The following behaviors are available:

- **Strict** (default): The native function call will throw a `NativeFunctionNotMockedException` if no mock was set up.
- **Loose**: The native function call will do nothing and return a default value if required.

That's all that is needed to define a native mock interface. Register it as described in the Installation section or auto-detect your interfaces. Use the `Setup` on the `NativeMockRepository` to setup an implementation for your native mock. Pro tip: Use a mocking library to mock your native mock interfaces. You can remove any registered native mock implementation by calling `Reset` or `ResetAll`.