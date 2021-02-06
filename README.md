# NativeMock

A .NET 5 library that allows you to mock native function calls by creating an interface that represents the native API. The mocking of that interface can then be done with any mocking library.

## Example

Imagine you have some code that depends on an external function:

```c#
[DllImport("test.dll")]
public static extern int MyExternalFunction([MarshalAs (UnmanagedType.LPUTF8Str)] string s);

public static bool MyFunctionThatDependsOnAnExternalFunction(string s)
{
  return MyExternalFunction(s) > 1;
}
```

In order to test this function you would normally need to use the native DLL in your tests. With *NativeMock* you can mock any calls your external library simply by defining an interface for it:

```c#
[NativeMockInterface ("test.dll")]
public interface IExternalDll
{
  int MyExternalFunction([MarshalAs (UnmanagedType.LPUTF8Str)] string s); // Must match signature with the imported function
}

// Global one-time setup - important: must run before any native DLL symbol is resolved
// in this case using ModuleInitializer (preferred)
public static class ModuleInitializer
{
  [ModuleInitializer]
  public static void Initialize()
  {
    NativeMockRegistry.Initialize();
    NativeMockRegistry.Register<IExternalDll>();
  }
}

// Tests (here with NUnit + Moq)
[TestFixture]
public class MyTests
{
  private Mock<IExternalDll> _apiMock;

  [SetUp]
  public void Setup()
  {
    _apiMock = new Mock<IExternalDll> (MockBehavior.Strict);

    NativeMockRegistry.ClearMocks();
    NativeMockRegistry.Mock (_apiMock.Object);
  }
  
  [Test]
  public void MyFunctionThatDependsOnAnExternalFunction_ReturnsTrueTest()
  {
    _apiMock.Setup(m => m.MyExternalFunction("a")).Returns(5);
      
    Assert.That (MyFunctionThatDependsOnAnExternalFunction("a"), Is.True);
    _apiMock.Verify();
  }
    
  [Test]
  public void MyFunctionThatDependsOnAnExternalFunction_ReturnsFalseTest()
  {
    _apiMock.Setup(m => m.MyExternalFunction("b")).Returns(0);
      
    Assert.That (MyFunctionThatDependsOnAnExternalFunction("b"), Is.False);
    _apiMock.Verify();
  }
}

```

## Limitations

- You cannot mock `GetModuleFileNameW` - bad luck if you need to mock it
- A native library must still be loaded although it could be an empty library. The library also need not export any functions. This is true as long as all used functions are added in the mock interface.