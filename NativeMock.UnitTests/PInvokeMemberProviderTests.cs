namespace NativeMock.UnitTests
{
  using System;
  using System.Reflection;
  using System.Runtime.InteropServices;
  using Infrastructure;
  using NUnit.Framework;

  [TestFixture]
  public class PInvokeMemberProviderTests
  {
    private PInvokeMemberProvider _pInvokeMemberProvider;

    [SetUp]
    public void SetUp()
    {
      _pInvokeMemberProvider = new PInvokeMemberProvider();
    }

    [Test]
    public void ThrowsOnNullTest()
    {
      Assert.That (() => _pInvokeMemberProvider.GetPInvokeMembers (null!), Throws.ArgumentNullException);
    }

    private class NoPInvokeMembers
    {
      // ReSharper disable once UnusedMember.Local
      public static void Test()
      {
      }
    }

    [Test]
    public void NoPInvokeMembersTest()
    {
      var pInvokeMembers = _pInvokeMemberProvider.GetPInvokeMembers (typeof(NoPInvokeMembers));

      Assert.That (pInvokeMembers.IsEmpty);
    }

    private class NormalPInvokeMember
    {
      [DllImport (FakeDllNames.Dll1)]
      public static extern void Test();
    }

    [Test]
    public void NormalPInvokeMemberTest()
    {
      var pInvokeMembers = _pInvokeMemberProvider.GetPInvokeMembers (typeof(NormalPInvokeMember));

      var nativeFunctionIdentifier = new NativeFunctionIdentifier (FakeDllNames.Dll1, "Test");
      var expectedPInvokeMember = new PInvokeMember (nativeFunctionIdentifier, GetClassMethod (NormalPInvokeMember.Test));

      Assert.That (pInvokeMembers.Length, Is.EqualTo (1));
      Assert.That (pInvokeMembers[0], Is.EqualTo (expectedPInvokeMember));
    }

    private class RenamedPInvokeMember
    {
      [DllImport (FakeDllNames.Dll1, EntryPoint = "Renamed")]
      public static extern void Test();
    }

    [Test]
    public void RenamedPInvokeMemberTest()
    {
      var pInvokeMembers = _pInvokeMemberProvider.GetPInvokeMembers (typeof(RenamedPInvokeMember));

      var nativeFunctionIdentifier = new NativeFunctionIdentifier (FakeDllNames.Dll1, "Renamed");
      var expectedPInvokeMember = new PInvokeMember (nativeFunctionIdentifier, GetClassMethod (RenamedPInvokeMember.Test));

      Assert.That (pInvokeMembers.Length, Is.EqualTo (1));
      Assert.That (pInvokeMembers[0], Is.EqualTo (expectedPInvokeMember));
    }

    private MethodInfo GetClassMethod (Action action) => action.Method;
  }
}
