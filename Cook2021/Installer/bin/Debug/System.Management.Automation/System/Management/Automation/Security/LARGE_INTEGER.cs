// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Security.LARGE_INTEGER
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.InteropServices;

namespace System.Management.Automation.Security
{
  [StructLayout(LayoutKind.Explicit)]
  internal struct LARGE_INTEGER
  {
    [FieldOffset(0)]
    public Anonymous_9320654f_2227_43bf_a385_74cc8c562686 Struct1;
    [FieldOffset(0)]
    public Anonymous_947eb392_1446_4e25_bbd4_10e98165f3a9 u;
    [FieldOffset(0)]
    public long QuadPart;
  }
}
