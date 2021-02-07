namespace NativeMock.UnitTests
{
  using System;
  using System.Linq.Expressions;
  using System.Reflection;
  using NUnit.Framework;

  public class NativeMockInterfaceMethodDescriptionProviderTests
  {
    private interface ITest
    {
      void Empty();

      [NativeMockCallback]
      void EmptyAnnotated();

      [NativeMockCallback ("Test")]
      void Renamed();
    }

    private NativeMockInterfaceMethodDescriptionProvider _nativeMockInterfaceMethodDescriptionProvider;

    [SetUp]
    public void SetUp()
    {
      _nativeMockInterfaceMethodDescriptionProvider = new NativeMockInterfaceMethodDescriptionProvider();
    }

    [Test]
    public void ThrowsOnNullTest()
    {
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (null!), Throws.ArgumentNullException);
    }

    [Test]
    public void EmptyMethodTest()
    {
      var method = GetMethod (e => e.Empty());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo (method.Name));
      Assert.That (interfaceDescription.MethodInfo, Is.EqualTo (method));
    }

    [Test]
    public void EmptyAnnotatedMethodTest()
    {
      var method = GetMethod (e => e.EmptyAnnotated());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo (method.Name));
      Assert.That (interfaceDescription.MethodInfo, Is.EqualTo (method));
    }

    [Test]
    public void RenamedTest()
    {
      var method = GetMethod (e => e.Renamed());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("Test"));
      Assert.That (interfaceDescription.MethodInfo, Is.EqualTo (method));
    }

    private MethodInfo GetMethod (Expression<Action<ITest>> lambdaExpression) => ((MethodCallExpression) lambdaExpression.Body).Method;
  }
}
