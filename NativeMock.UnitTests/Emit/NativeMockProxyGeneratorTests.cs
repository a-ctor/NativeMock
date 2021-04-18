namespace NativeMock.UnitTests.Emit
{
  using System;
  using System.Reflection;
  using System.Reflection.Emit;
  using Moq;
  using NativeMock.Emit;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockProxyGeneratorTests
  {
    public interface ITestInterface
    {
      int Test (int value);
    }

    public delegate int TestDelegate (int value);

    private readonly MethodInfo _testMethod = typeof(ITestInterface).GetMethod (nameof(ITestInterface.Test));

    private ModuleBuilder _moduleBuilder;
    private Mock<IDelegateFactory> _delegateFactoryMock;
    private NativeMockProxyCodeGenerator _nativeMockProxyCodeGenerator;

    public NativeMockProxyGeneratorTests()
    {
      var assemblyName = new AssemblyName (NativeMockRegistry.ProxyAssemblyName);
      var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.Run);
      _moduleBuilder = assemblyBuilder.DefineDynamicModule (NativeMockRegistry.ProxyAssemblyModuleName);
    }

    [SetUp]
    public void SetUp()
    {
      _delegateFactoryMock = new Mock<IDelegateFactory> (MockBehavior.Strict);
      _delegateFactoryMock.Setup (e => e.CreateDelegateType (_testMethod)).Returns (typeof(TestDelegate));

      _nativeMockProxyCodeGenerator = new NativeMockProxyCodeGenerator (_moduleBuilder, _delegateFactoryMock.Object);
    }

    [Test]
    public void GeneratesNativeMockProxyCorrectly()
    {
      Assert.That (() => _nativeMockProxyCodeGenerator.CreateProxy (typeof(ITestInterface)), Throws.Nothing);
    }

    [Test]
    public void InvocationFailsWhenNoHandlerIsSetUp()
    {
      var (testInterface, _) = CreateNativeMockProxy<ITestInterface>();

      Assert.That (() => testInterface.Test (34), Throws.TypeOf<NativeMockException>());
      _delegateFactoryMock.Verify();
    }

    [Test]
    public void CallsUnderlyingImplementationIfAvailable()
    {
      var testInterfaceMock = new Mock<ITestInterface> (MockBehavior.Strict);
      testInterfaceMock.Setup (e => e.Test (3)).Returns (5);

      var (testInterface, proxyController) = CreateNativeMockProxy<ITestInterface>();
      proxyController.SetUnderlyingImplementation (testInterfaceMock.Object);

      Assert.That (testInterface.Test (3), Is.EqualTo (5));
      testInterfaceMock.Verify();
      _delegateFactoryMock.Verify();
    }

    [Test]
    public void CallsHandlerWhenAvailable()
    {
      var testDelegateMock = new Mock<TestDelegate> (MockBehavior.Strict);
      testDelegateMock.Setup (e => e (3)).Returns (5);

      var (testInterface, proxyController) = CreateNativeMockProxy<ITestInterface>();
      proxyController.SetMethodHandler (1, testDelegateMock.Object);

      Assert.That (testInterface.Test (3), Is.EqualTo (5));
      testDelegateMock.Verify();
      _delegateFactoryMock.Verify();
    }

    [Test]
    public void PrefersHandlerOverUnderlyingImplementation()
    {
      var testInterfaceMock = new Mock<ITestInterface> (MockBehavior.Strict);
      var testDelegateMock = new Mock<TestDelegate> (MockBehavior.Strict);
      testDelegateMock.Setup (e => e (3)).Returns (5);

      var (testInterface, proxyController) = CreateNativeMockProxy<ITestInterface>();
      proxyController.SetUnderlyingImplementation (testInterfaceMock.Object);
      proxyController.SetMethodHandler (1, testDelegateMock.Object);

      Assert.That (testInterface.Test (3), Is.EqualTo (5));
      testDelegateMock.Verify();
      _delegateFactoryMock.Verify();
    }

    [Test]
    public void GetMethodCount()
    {
      var (_, proxyController) = CreateNativeMockProxy<ITestInterface>();

      Assert.That (proxyController.GetMethodCount(), Is.EqualTo (1));
    }


    [Test]
    public void CallingUnderlyingImplementationDoesNotAffectCallCount()
    {
      var testInterfaceMock = new Mock<ITestInterface> (MockBehavior.Strict);
      testInterfaceMock.Setup (e => e.Test (3)).Returns (5);

      var (testInterface, proxyController) = CreateNativeMockProxy<ITestInterface>();
      proxyController.SetUnderlyingImplementation (testInterfaceMock.Object);

      Assert.That (proxyController.GetMethodHandlerCallCount (1), Is.EqualTo (0));

      testInterface.Test (3);
      Assert.That (proxyController.GetMethodHandlerCallCount (1), Is.EqualTo (0));

      testInterfaceMock.Verify();
      _delegateFactoryMock.Verify();
    }

    [Test]
    public void GetMethodHandlerCallCount()
    {
      var testDelegateMock = new Mock<TestDelegate> (MockBehavior.Strict);
      testDelegateMock.Setup (e => e (3)).Returns (5);

      var (testInterface, proxyController) = CreateNativeMockProxy<ITestInterface>();
      proxyController.SetMethodHandler (1, testDelegateMock.Object);

      Assert.That (proxyController.GetMethodHandlerCallCount (1), Is.EqualTo (0));

      testInterface.Test (3);
      Assert.That (proxyController.GetMethodHandlerCallCount (1), Is.EqualTo (1));

      testInterface.Test (3);
      Assert.That (proxyController.GetMethodHandlerCallCount (1), Is.EqualTo (2));

      testDelegateMock.Verify();
      _delegateFactoryMock.Verify();
    }

    private (T testInterface, INativeMockProxyController<T> proxyController) CreateNativeMockProxy<T>()
      where T : class
    {
      var proxyObjectResult = _nativeMockProxyCodeGenerator.CreateProxy (typeof(T));

      var proxyObject = Activator.CreateInstance (proxyObjectResult.ProxyType);
      var testInterface = (T) proxyObject;
      var proxyController = (INativeMockProxyController<T>) proxyObject;

      return (testInterface, proxyController);
    }
  }
}
