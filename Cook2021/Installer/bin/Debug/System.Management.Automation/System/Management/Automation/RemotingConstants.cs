// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemotingConstants
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal static class RemotingConstants
  {
    internal const string PSSessionConfigurationNoun = "PSSessionConfiguration";
    internal const string PSRemotingNoun = "PSRemoting";
    internal const string PSPluginDLLName = "pwrshplugin.dll";
    internal const string DefaultShellName = "Microsoft.PowerShell";
    internal static readonly Version HostVersion = new Version(1, 0, 0, 0);
    internal static readonly Version ProtocolVersionWin7RC = new Version(2, 0);
    internal static readonly Version ProtocolVersionWin7RTM = new Version(2, 1);
    internal static readonly Version ProtocolVersion = RemotingConstants.ProtocolVersionWin7RTM;
    internal static readonly string ComputerNameNoteProperty = "PSComputerName";
    internal static readonly string RunspaceIdNoteProperty = "RunspaceId";
    internal static readonly string ShowComputerNameNoteProperty = "PSShowComputerName";
  }
}
