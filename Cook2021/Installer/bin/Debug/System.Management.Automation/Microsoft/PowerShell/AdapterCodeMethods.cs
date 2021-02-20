// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.AdapterCodeMethods
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Globalization;
using System.Management.Automation;
using System.Reflection;

namespace Microsoft.PowerShell
{
  public static class AdapterCodeMethods
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");

    public static long ConvertLargeIntegerToInt64(
      PSObject deInstance,
      PSObject largeIntegerInstance)
    {
      object target = largeIntegerInstance != null ? largeIntegerInstance.BaseObject : throw AdapterCodeMethods.tracer.NewArgumentException(nameof (largeIntegerInstance));
      Type type = target.GetType();
      int num1 = (int) type.InvokeMember("HighPart", BindingFlags.Public | BindingFlags.GetProperty, (Binder) null, target, (object[]) null, CultureInfo.InvariantCulture);
      int num2 = (int) type.InvokeMember("LowPart", BindingFlags.Public | BindingFlags.GetProperty, (Binder) null, target, (object[]) null, CultureInfo.InvariantCulture);
      byte[] numArray = new byte[8];
      BitConverter.GetBytes(num2).CopyTo((Array) numArray, 0);
      BitConverter.GetBytes(num1).CopyTo((Array) numArray, 4);
      return BitConverter.ToInt64(numArray, 0);
    }

    public static string ConvertDNWithBinaryToString(
      PSObject deInstance,
      PSObject dnWithBinaryInstance)
    {
      object target = dnWithBinaryInstance != null ? dnWithBinaryInstance.BaseObject : throw AdapterCodeMethods.tracer.NewArgumentException(nameof (dnWithBinaryInstance));
      return (string) target.GetType().InvokeMember("DNString", BindingFlags.Public | BindingFlags.GetProperty, (Binder) null, target, (object[]) null, CultureInfo.InvariantCulture);
    }
  }
}
