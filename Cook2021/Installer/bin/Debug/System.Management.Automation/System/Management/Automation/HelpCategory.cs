// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HelpCategory
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  [System.Flags]
  internal enum HelpCategory
  {
    None = 0,
    Alias = 1,
    Cmdlet = 2,
    Provider = 4,
    General = 16, // 0x00000010
    FAQ = 32, // 0x00000020
    Glossary = 64, // 0x00000040
    HelpFile = 128, // 0x00000080
    ScriptCommand = 256, // 0x00000100
    Function = 512, // 0x00000200
    Filter = 1024, // 0x00000400
    ExternalScript = 2048, // 0x00000800
    All = 4095, // 0x00000FFF
    DefaultHelp = 4096, // 0x00001000
  }
}
