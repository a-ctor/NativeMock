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
using TestAssembly = NativeMock.Analyzer.TestAssembly;
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
