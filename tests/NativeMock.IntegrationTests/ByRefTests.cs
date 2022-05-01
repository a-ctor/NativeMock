namespace NativeMock.IntegrationTests
{
  using System.Runtime.InteropServices;
  using Infrastructure;
  using NUnit.Framework;

  [TestFixture]
  public class ByRefTests
  {
    [NativeMockInterface (FakeDllNames.Dll1, DeclaringType = typeof(SpecialMethods), Behavior = NativeMockBehavior.Loose)]
    public interface ISpecialMethods
    {
      void NmInParameter (in string test);

      delegate void NmInParameterDelegate (in string test);

      void NmRefParameter (ref string test);

      delegate void NmRefParameterDelegate (ref string test);

      void NmOutParameter (out string test);

      delegate void NmOutParameterDelegate (out string test);
    }

    public class SpecialMethods
    {
      [DllImport (FakeDllNames.Dll1)]
      public static extern void NmInParameter (in string test);

      [DllImport (FakeDllNames.Dll1)]
      public static extern void NmRefParameter (ref string test);

      [DllImport (FakeDllNames.Dll1)]
      public static extern void NmOutParameter (out string test);
    }

    [Test]
    public void NmInParameter()
    {
      using var nativeMock = new NativeMock<ISpecialMethods>();
      nativeMock.Setup<ISpecialMethods.NmInParameterDelegate> (e => e.NmInParameter, (in string value) => { Assert.That (value, Is.EqualTo ("Test")); });

      var str = "Test";
      SpecialMethods.NmInParameter (in str);

      nativeMock.Verify();
    }

    [Test]
    public void NmRefParameter()
    {
      using var nativeMock = new NativeMock<ISpecialMethods>();
      nativeMock.Setup<ISpecialMethods.NmRefParameterDelegate> (e => e.NmRefParameter, (ref string value) =>
      {
        Assert.That (value, Is.EqualTo ("Test"));
        value = "Test2";
      });

      var str = "Test";
      SpecialMethods.NmRefParameter (ref str);

      Assert.That (str, Is.EqualTo ("Test2"));
      nativeMock.Verify();
    }

    [Test]
    public void NmOutParameter()
    {
      using var nativeMock = new NativeMock<ISpecialMethods>();
      nativeMock.Setup<ISpecialMethods.NmOutParameterDelegate> (e => e.NmOutParameter, (out string value) => { value = "Test2"; });

      string str;
      SpecialMethods.NmOutParameter (out str);

      Assert.That (str, Is.EqualTo ("Test2"));
      nativeMock.Verify();
    }
  }
}
