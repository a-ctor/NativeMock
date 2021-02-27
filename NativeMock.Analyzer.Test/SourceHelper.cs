namespace NativeMock.Analyzer.Test
{
  public static class SourceHelper
  {
    public static string Create (string code)
    {
      return @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NativeMock
{
  [AttributeUsage (AttributeTargets.Interface)]
  public class NativeMockInterfaceAttribute : Attribute
  {
    public Type DeclaringType { get; set; }

    public NativeMockInterfaceAttribute(string name)
    {
    }
  }

  [AttributeUsage (AttributeTargets.Method)]
  public class NativeMockCallbackAttribute : Attribute
  {
    public Type DeclaringType { get; set; }

    public NativeMockCallbackAttribute()
    {
    }

    public NativeMockCallbackAttribute(string name)
    {
    }
  }
}
" + code;
    }

    public static (string before, string after) CreateForCodeFix (string pInvokeMethod, string beforeMethod, string afterMethod)
    {
      return (Create (
        $@"
class NativeFunctions
{{
  [DllImport(""test.dll"")]
  public static extern {pInvokeMethod};
}}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(NativeFunctions))]
  {beforeMethod}; 
}}"), Create (
        $@"
class NativeFunctions
{{
  [DllImport(""test.dll"")]
  public static extern {pInvokeMethod};
}}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(NativeFunctions))]
  {afterMethod}; 
}}"));
    }
  }
}
