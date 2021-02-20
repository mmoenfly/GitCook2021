// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSTraceSourceOptions
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  [System.Flags]
  public enum PSTraceSourceOptions
  {
    None = 0,
    Constructor = 1,
    Dispose = 2,
    Finalizer = 4,
    Method = 8,
    Property = 16, // 0x00000010
    Delegates = 32, // 0x00000020
    Events = 64, // 0x00000040
    Exception = 128, // 0x00000080
    Lock = 256, // 0x00000100
    Error = 512, // 0x00000200
    Warning = 1024, // 0x00000400
    Verbose = 2048, // 0x00000800
    WriteLine = 4096, // 0x00001000
    Scope = 8192, // 0x00002000
    Assert = 16384, // 0x00004000
    ExecutionFlow = Scope | Events | Delegates | Method | Finalizer | Dispose | Constructor, // 0x0000206F
    Data = WriteLine | Verbose | Property | Finalizer | Dispose | Constructor, // 0x00001817
    Errors = Error | Exception, // 0x00000280
    All = Errors | Data | Assert | Scope | Warning | Lock | Events | Delegates | Method, // 0x00007FFF
  }
}
