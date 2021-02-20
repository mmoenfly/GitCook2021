// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.PSRemotingBaseCmdlet
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  public abstract class PSRemotingBaseCmdlet : PSRemotingCmdlet
  {
    internal const string DEFAULT_SESSION_OPTION = "PSSessionOption";
    protected const string DefaultPowerShellRemoteShellName = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
    protected const string DefaultPowerShellRemoteShellNameEnvVariable = "global:PSSessionConfigurationName";
    protected const string DefaultPowerShellRemoteShellAppName = "WSMan";
    protected const string DefaultPowerShellRemoteShellAppNameEnvVariable = "global:PSSessionApplicationName";
    protected const string UriParameterSet = "Uri";
    private PSSession[] remoteRunspaceInfos;
    private string[] computerNames;
    private string[] resolvedComputerNames;
    private PSCredential pscredential;
    private int port;
    private SwitchParameter useSSL;
    private string shell;
    private string appName;
    private int throttleLimit;
    private Uri[] uris;
    private bool allowRedirection;
    private PSSessionOption sessionOption;
    private AuthenticationMechanism authMechanism;
    private string thumbPrint;

    [Parameter(ParameterSetName = "Session", Position = 0, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public virtual PSSession[] Session
    {
      get => this.remoteRunspaceInfos;
      set => this.remoteRunspaceInfos = value;
    }

    [Alias(new string[] {"Cn"})]
    [Parameter(ParameterSetName = "ComputerName", Position = 0, ValueFromPipelineByPropertyName = true)]
    public virtual string[] ComputerName
    {
      get => this.computerNames;
      set => this.computerNames = value;
    }

    protected string[] ResolvedComputerNames
    {
      get => this.resolvedComputerNames;
      set => this.resolvedComputerNames = value;
    }

    [Parameter(ParameterSetName = "ComputerName", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "Uri", ValueFromPipelineByPropertyName = true)]
    [System.Management.Automation.Credential]
    public virtual PSCredential Credential
    {
      get => this.pscredential;
      set
      {
        this.pscredential = value;
        this.ValidateSpecifiedAuthentication();
      }
    }

    [Parameter(ParameterSetName = "ComputerName")]
    [ValidateRange(1, 65535)]
    public virtual int Port
    {
      get => this.port;
      set => this.port = value;
    }

    [Parameter(ParameterSetName = "ComputerName")]
    public virtual SwitchParameter UseSSL
    {
      get => this.useSSL;
      set => this.useSSL = value;
    }

    [Parameter(ParameterSetName = "ComputerName", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "Uri", ValueFromPipelineByPropertyName = true)]
    public virtual string ConfigurationName
    {
      get => this.shell;
      set => this.shell = this.ResolveShell(value);
    }

    [Parameter(ParameterSetName = "ComputerName", ValueFromPipelineByPropertyName = true)]
    public virtual string ApplicationName
    {
      get => this.appName;
      set => this.appName = this.ResolveAppName(value);
    }

    [Parameter(ParameterSetName = "ComputerName")]
    [Parameter(ParameterSetName = "Session")]
    [Parameter(ParameterSetName = "Uri")]
    public virtual int ThrottleLimit
    {
      set => this.throttleLimit = value;
      get => this.throttleLimit;
    }

    [Parameter(Mandatory = true, ParameterSetName = "Uri", Position = 0, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] {"URI", "CU"})]
    public virtual Uri[] ConnectionUri
    {
      get => this.uris;
      set => this.uris = value;
    }

    [Parameter(ParameterSetName = "Uri")]
    public virtual SwitchParameter AllowRedirection
    {
      get => (SwitchParameter) this.allowRedirection;
      set => this.allowRedirection = (bool) value;
    }

    [Parameter(ParameterSetName = "ComputerName")]
    [Parameter(ParameterSetName = "Uri")]
    [ValidateNotNull]
    public virtual PSSessionOption SessionOption
    {
      get
      {
        if (this.sessionOption == null)
        {
          object valueToConvert = this.SessionState.PSVariable.GetValue("PSSessionOption");
          if (valueToConvert == null || !LanguagePrimitives.TryConvertTo<PSSessionOption>(valueToConvert, out this.sessionOption))
            this.sessionOption = new PSSessionOption();
        }
        return this.sessionOption;
      }
      set => this.sessionOption = value;
    }

    [Parameter(ParameterSetName = "ComputerName")]
    [Parameter(ParameterSetName = "Uri")]
    public virtual AuthenticationMechanism Authentication
    {
      get => this.authMechanism;
      set
      {
        this.authMechanism = value;
        this.ValidateSpecifiedAuthentication();
      }
    }

    [Parameter(ParameterSetName = "Uri")]
    [Parameter(ParameterSetName = "ComputerName")]
    public virtual string CertificateThumbprint
    {
      get => this.thumbPrint;
      set
      {
        this.thumbPrint = value;
        this.ValidateSpecifiedAuthentication();
      }
    }

    protected void ValidateSpecifiedAuthentication()
    {
      if (this.Credential != null && this.thumbPrint != null)
        throw new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.NewRunspaceAmbiguosAuthentication, (object) "CertificateThumbPrint", (object) "Credential"));
      if (this.Authentication != AuthenticationMechanism.Default && this.thumbPrint != null)
        throw new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.NewRunspaceAmbiguosAuthentication, (object) "CertificateThumbPrint", (object) this.Authentication.ToString()));
      if (this.Authentication == AuthenticationMechanism.NegotiateWithImplicitCredential && this.Credential != null)
        throw new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.NewRunspaceAmbiguosAuthentication, (object) "Credential", (object) this.Authentication.ToString()));
    }

    protected void ValidateRemoteRunspacesSpecified()
    {
      if (RemotingCommandUtil.HasRepeatingRunspaces(this.Session))
        this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(this.GetMessage(PSRemotingErrorId.RemoteRunspaceInfoHasDuplicates)), PSRemotingErrorId.RemoteRunspaceInfoHasDuplicates.ToString(), ErrorCategory.InvalidArgument, (object) this.Session));
      if (!RemotingCommandUtil.ExceedMaximumAllowableRunspaces(this.Session))
        return;
      this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(this.GetMessage(PSRemotingErrorId.RemoteRunspaceInfoLimitExceeded)), PSRemotingErrorId.RemoteRunspaceInfoLimitExceeded.ToString(), ErrorCategory.InvalidArgument, (object) this.Session));
    }

    protected string ResolveShell(string shell)
    {
      string str = string.IsNullOrEmpty(shell) ? (string) this.GetVariableValue("global:PSSessionConfigurationName", (object) "http://schemas.microsoft.com/powershell/Microsoft.PowerShell") : shell;
      if (str.IndexOf("http://schemas.microsoft.com/powershell/", StringComparison.OrdinalIgnoreCase) == -1)
        str = "http://schemas.microsoft.com/powershell/" + str;
      return str;
    }

    protected string ResolveAppName(string appName) => string.IsNullOrEmpty(appName) ? (string) this.GetVariableValue("global:PSSessionApplicationName", (object) "WSMan") : appName;

    internal void UpdateConnectionInfo(WSManConnectionInfo connectionInfo)
    {
      connectionInfo.SetSessionOptions(this.SessionOption);
      if (!this.ParameterSetName.Equals("Uri", StringComparison.OrdinalIgnoreCase))
        connectionInfo.MaximumConnectionRedirectionCount = 0;
      if (this.allowRedirection)
        return;
      connectionInfo.MaximumConnectionRedirectionCount = 0;
    }

    protected void ValidateComputerName(string[] computerNames)
    {
      foreach (string computerName in computerNames)
      {
        switch (Uri.CheckHostName(computerName))
        {
          case UriHostNameType.Dns:
          case UriHostNameType.IPv4:
          case UriHostNameType.IPv6:
            continue;
          default:
            this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.InvalidComputerName)), "PSSessionInvalidComputerName", ErrorCategory.InvalidArgument, (object) computerNames));
            continue;
        }
      }
    }

    protected override void BeginProcessing()
    {
      base.BeginProcessing();
      if (string.IsNullOrEmpty(this.shell))
        this.shell = this.ResolveShell((string) null);
      if (!string.IsNullOrEmpty(this.appName))
        return;
      this.appName = this.ResolveAppName((string) null);
    }
  }
}
