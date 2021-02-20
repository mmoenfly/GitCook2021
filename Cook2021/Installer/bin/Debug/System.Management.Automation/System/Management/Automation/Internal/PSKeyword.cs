// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSKeyword
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  internal enum PSKeyword : ulong
  {
    Runspace = 1,
    Pipeline = 2,
    Protocol = 4,
    Transport = 8,
    Host = 16, // 0x0000000000000010
    Cmdlets = 32, // 0x0000000000000020
    Serializer = 64, // 0x0000000000000040
    Session = 128, // 0x0000000000000080
    ManagedPlugin = 256, // 0x0000000000000100
    UseAlwaysAnalytic = 4611686018427387904, // 0x4000000000000000
    UseAlwaysOperational = 9223372036854775808, // 0x8000000000000000
  }
}
