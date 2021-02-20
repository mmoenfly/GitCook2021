// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandTypes
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  [System.Flags]
  public enum CommandTypes
  {
    Alias = 1,
    Function = 2,
    Filter = 4,
    Cmdlet = 8,
    ExternalScript = 16, // 0x00000010
    Application = 32, // 0x00000020
    Script = 64, // 0x00000040
    All = Script | Application | ExternalScript | Cmdlet | Filter | Function | Alias, // 0x0000007F
  }
}
