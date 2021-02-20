// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.NewPSSessionOptionCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("New", "PSSessionOption")]
  public sealed class NewPSSessionOptionCommand : PSCmdlet
  {
    [TraceSource("NewPSSessionOption", "Class that has New-PSSessionOption command implementation")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("NewPSSessionOption", "Class that has New-PSSessionOption command implementation");
    private int? maximumRedirection;
    private SwitchParameter noCompression;
    private SwitchParameter noMachineProfile;
    private CultureInfo culture;
    private CultureInfo uiCulture;
    private int? maxRecvdDataSizePerCommand;
    private int? maxRecvdObjectSize;
    private PSPrimitiveDictionary applicationArguments;
    private int? openTimeout;
    private int? cancelTimeout;
    private int? idleTimeout;
    private ProxyAccessType _proxyacesstype;
    private AuthenticationMechanism proxyauthentication = AuthenticationMechanism.Negotiate;
    private PSCredential _proxycredential;
    private bool skipcacheck;
    private bool skipcncheck;
    private bool skiprevocationcheck;
    private int? operationtimeout;
    private bool noencryption;
    private bool useutf16;

    [Parameter]
    public int MaximumRedirection
    {
      get => this.maximumRedirection.Value;
      set => this.maximumRedirection = new int?(value);
    }

    [Parameter]
    public SwitchParameter NoCompression
    {
      get => this.noCompression;
      set => this.noCompression = value;
    }

    [Parameter]
    public SwitchParameter NoMachineProfile
    {
      get => this.noMachineProfile;
      set => this.noMachineProfile = value;
    }

    [Parameter]
    [ValidateNotNull]
    public CultureInfo Culture
    {
      get => this.culture;
      set => this.culture = value;
    }

    [Parameter]
    [ValidateNotNull]
    public CultureInfo UICulture
    {
      get => this.uiCulture;
      set => this.uiCulture = value;
    }

    [Parameter]
    public int MaximumReceivedDataSizePerCommand
    {
      get => this.maxRecvdDataSizePerCommand.Value;
      set => this.maxRecvdDataSizePerCommand = new int?(value);
    }

    [Parameter]
    public int MaximumReceivedObjectSize
    {
      get => this.maxRecvdObjectSize.Value;
      set => this.maxRecvdObjectSize = new int?(value);
    }

    [Parameter]
    [ValidateNotNull]
    public PSPrimitiveDictionary ApplicationArguments
    {
      get => this.applicationArguments;
      set => this.applicationArguments = value;
    }

    [ValidateRange(0, 2147483647)]
    [Parameter]
    public int OpenTimeout
    {
      get => !this.openTimeout.HasValue ? 180000 : this.openTimeout.Value;
      set => this.openTimeout = new int?(value);
    }

    [Parameter]
    [ValidateRange(0, 2147483647)]
    public int CancelTimeout
    {
      get => !this.cancelTimeout.HasValue ? 60000 : this.cancelTimeout.Value;
      set => this.cancelTimeout = new int?(value);
    }

    [Parameter]
    [ValidateRange(0, 2147483647)]
    public int IdleTimeout
    {
      get => !this.idleTimeout.HasValue ? 240000 : this.idleTimeout.Value;
      set => this.idleTimeout = new int?(value);
    }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public ProxyAccessType ProxyAccessType
    {
      get => this._proxyacesstype;
      set => this._proxyacesstype = value;
    }

    [Parameter]
    public AuthenticationMechanism ProxyAuthentication
    {
      get => this.proxyauthentication;
      set => this.proxyauthentication = value;
    }

    [Parameter]
    [Credential]
    [ValidateNotNullOrEmpty]
    public PSCredential ProxyCredential
    {
      get => this._proxycredential;
      set => this._proxycredential = value;
    }

    [Parameter]
    public SwitchParameter SkipCACheck
    {
      get => (SwitchParameter) this.skipcacheck;
      set => this.skipcacheck = (bool) value;
    }

    [Parameter]
    public SwitchParameter SkipCNCheck
    {
      get => (SwitchParameter) this.skipcncheck;
      set => this.skipcncheck = (bool) value;
    }

    [Parameter]
    public SwitchParameter SkipRevocationCheck
    {
      get => (SwitchParameter) this.skiprevocationcheck;
      set => this.skiprevocationcheck = (bool) value;
    }

    [Parameter]
    [ValidateRange(0, 2147483647)]
    public int OperationTimeout
    {
      get => !this.operationtimeout.HasValue ? 180000 : this.operationtimeout.Value;
      set => this.operationtimeout = new int?(value);
    }

    [Parameter]
    public SwitchParameter NoEncryption
    {
      get => (SwitchParameter) this.noencryption;
      set => this.noencryption = (bool) value;
    }

    [Parameter]
    public SwitchParameter UseUTF16
    {
      get => (SwitchParameter) this.useutf16;
      set => this.useutf16 = (bool) value;
    }

    protected override void BeginProcessing()
    {
      PSSessionOption psSessionOption = new PSSessionOption();
      psSessionOption.ProxyAccessType = this.ProxyAccessType;
      psSessionOption.ProxyAuthentication = this.ProxyAuthentication;
      psSessionOption.ProxyCredential = this.ProxyCredential;
      psSessionOption.SkipCACheck = (bool) this.SkipCACheck;
      psSessionOption.SkipCNCheck = (bool) this.SkipCNCheck;
      psSessionOption.SkipRevocationCheck = (bool) this.SkipRevocationCheck;
      if (this.operationtimeout.HasValue)
        psSessionOption.OperationTimeout = TimeSpan.FromMilliseconds((double) this.operationtimeout.Value);
      psSessionOption.NoEncryption = (bool) this.NoEncryption;
      psSessionOption.UseUTF16 = (bool) this.UseUTF16;
      if (this.maximumRedirection.HasValue)
        psSessionOption.MaximumConnectionRedirectionCount = this.MaximumRedirection;
      psSessionOption.NoCompression = this.NoCompression.IsPresent;
      psSessionOption.NoMachineProfile = this.NoMachineProfile.IsPresent;
      psSessionOption.MaximumReceivedDataSizePerCommand = this.maxRecvdDataSizePerCommand;
      psSessionOption.MaximumReceivedObjectSize = this.maxRecvdObjectSize;
      if (this.Culture != null)
        psSessionOption.Culture = this.Culture;
      if (this.UICulture != null)
        psSessionOption.UICulture = this.UICulture;
      if (this.openTimeout.HasValue)
        psSessionOption.OpenTimeout = TimeSpan.FromMilliseconds((double) this.openTimeout.Value);
      if (this.cancelTimeout.HasValue)
        psSessionOption.CancelTimeout = TimeSpan.FromMilliseconds((double) this.cancelTimeout.Value);
      if (this.idleTimeout.HasValue)
        psSessionOption.IdleTimeout = TimeSpan.FromMilliseconds((double) this.idleTimeout.Value);
      if (this.ApplicationArguments != null)
        psSessionOption.ApplicationArguments = this.ApplicationArguments;
      this.WriteObject((object) psSessionOption);
    }
  }
}
