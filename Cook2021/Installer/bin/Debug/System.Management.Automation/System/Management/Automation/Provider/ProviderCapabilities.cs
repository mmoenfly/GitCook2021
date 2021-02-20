// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Provider.ProviderCapabilities
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Provider
{
  [System.Flags]
  public enum ProviderCapabilities
  {
    None = 0,
    Include = 1,
    Exclude = 2,
    Filter = 4,
    ExpandWildcards = 8,
    ShouldProcess = 16, // 0x00000010
    Credentials = 32, // 0x00000020
    Transactions = 64, // 0x00000040
  }
}
