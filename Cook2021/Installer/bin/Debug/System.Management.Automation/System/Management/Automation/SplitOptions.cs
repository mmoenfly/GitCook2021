// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SplitOptions
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  [System.Flags]
  public enum SplitOptions
  {
    SimpleMatch = 1,
    RegexMatch = 2,
    CultureInvariant = 4,
    IgnorePatternWhitespace = 8,
    Multiline = 16, // 0x00000010
    Singleline = 32, // 0x00000020
    IgnoreCase = 64, // 0x00000040
    ExplicitCapture = 128, // 0x00000080
  }
}
