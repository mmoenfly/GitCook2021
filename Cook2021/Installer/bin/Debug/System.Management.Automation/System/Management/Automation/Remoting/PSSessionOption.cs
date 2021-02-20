// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.PSSessionOption
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation.Remoting
{
  public sealed class PSSessionOption
  {
    private int maximumConnectionRedirectionCount = 5;
    private bool noCompression;
    private bool noMachineProfile;
    private ProxyAccessType proxyAcessType;
    private AuthenticationMechanism proxyAuthentication = AuthenticationMechanism.Negotiate;
    private PSCredential proxyCredential;
    private bool skipCACheck;
    private bool skipCNCheck;
    private bool skipRevocationCheck;
    private TimeSpan operationTimeout = TimeSpan.FromMilliseconds(180000.0);
    private bool noEncryption;
    private bool useUtf16;
    private CultureInfo culture;
    private CultureInfo uiCulture;
    private int? maxRecvdDataSizePerCommand;
    private int? maxRecvdObjectSize;
    private PSPrimitiveDictionary applicationArguments;
    private TimeSpan openTimeout = TimeSpan.FromMilliseconds(180000.0);
    private TimeSpan cancelTimeout = TimeSpan.FromMilliseconds(60000.0);
    private TimeSpan idleTimeout = TimeSpan.FromMilliseconds(240000.0);

    public int MaximumConnectionRedirectionCount
    {
      get => this.maximumConnectionRedirectionCount;
      set => this.maximumConnectionRedirectionCount = value;
    }

    public bool NoCompression
    {
      get => this.noCompression;
      set => this.noCompression = value;
    }

    public bool NoMachineProfile
    {
      get => this.noMachineProfile;
      set => this.noMachineProfile = value;
    }

    public ProxyAccessType ProxyAccessType
    {
      get => this.proxyAcessType;
      set => this.proxyAcessType = value;
    }

    public AuthenticationMechanism ProxyAuthentication
    {
      get => this.proxyAuthentication;
      set
      {
        switch (value)
        {
          case AuthenticationMechanism.Basic:
          case AuthenticationMechanism.Negotiate:
          case AuthenticationMechanism.Digest:
            this.proxyAuthentication = value;
            break;
          default:
            throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.ProxyAmbiguosAuthentication, (object) value, (object) AuthenticationMechanism.Basic.ToString(), (object) AuthenticationMechanism.Negotiate.ToString(), (object) AuthenticationMechanism.Digest.ToString()));
        }
      }
    }

    public PSCredential ProxyCredential
    {
      get => this.proxyCredential;
      set => this.proxyCredential = value;
    }

    public bool SkipCACheck
    {
      get => this.skipCACheck;
      set => this.skipCACheck = value;
    }

    public bool SkipCNCheck
    {
      get => this.skipCNCheck;
      set => this.skipCNCheck = value;
    }

    public bool SkipRevocationCheck
    {
      get => this.skipRevocationCheck;
      set => this.skipRevocationCheck = value;
    }

    public TimeSpan OperationTimeout
    {
      get => this.operationTimeout;
      set => this.operationTimeout = value;
    }

    public bool NoEncryption
    {
      get => this.noEncryption;
      set => this.noEncryption = value;
    }

    public bool UseUTF16
    {
      get => this.useUtf16;
      set => this.useUtf16 = value;
    }

    public CultureInfo Culture
    {
      get => this.culture;
      set => this.culture = value;
    }

    public CultureInfo UICulture
    {
      get => this.uiCulture;
      set => this.uiCulture = value;
    }

    public int? MaximumReceivedDataSizePerCommand
    {
      get => this.maxRecvdDataSizePerCommand;
      set => this.maxRecvdDataSizePerCommand = value;
    }

    public int? MaximumReceivedObjectSize
    {
      get => this.maxRecvdObjectSize;
      set => this.maxRecvdObjectSize = value;
    }

    public PSPrimitiveDictionary ApplicationArguments
    {
      get => this.applicationArguments;
      set => this.applicationArguments = value;
    }

    public TimeSpan OpenTimeout
    {
      get => this.openTimeout;
      set => this.openTimeout = value;
    }

    public TimeSpan CancelTimeout
    {
      get => this.cancelTimeout;
      set => this.cancelTimeout = value;
    }

    public TimeSpan IdleTimeout
    {
      get => this.idleTimeout;
      set => this.idleTimeout = value;
    }
  }
}
