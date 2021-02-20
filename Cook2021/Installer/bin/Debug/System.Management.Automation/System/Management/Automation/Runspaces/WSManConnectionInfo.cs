// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.WSManConnectionInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Management.Automation.Remoting.Client;
using System.Net;
using System.Net.Sockets;

namespace System.Management.Automation.Runspaces
{
  public sealed class WSManConnectionInfo : RunspaceConnectionInfo
  {
    internal const int defaultMaximumConnectionRedirectionCount = 5;
    private const string RemotingResourceBaseName = "RemotingErrorIdStrings";
    private const int INFINITE_TIMEOUT = 0;
    private const int DEFAULT_TIMEOUT = -1;
    private const int DEFAULT_OPEN_TIMEOUT = 180000;
    private const int DEFAULT_MAX_SERVER_LIFE_TIMEOUT = 0;
    internal const string HTTP_SCHEME = "http";
    internal const string HTTPS_SCHEME = "https";
    private const int DEFAULT_PORT_HTTP = -1;
    private const int DEFAULT_PORT_HTTPS = -1;
    private const int DEFAULT_PORT = 0;
    private const string DEFAULT_COMPUTER_NAME = "localhost";
    private const int PORT_UNRESOLVED_BY_URI = -1;
    private const int MAX_PORT = 65535;
    private const int MIN_PORT = 0;
    private const string LOCAL_HOST_URI_STRING = "http://localhost/wsman";
    private const string DEFAULT_SHELL_URI = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
    private const PSCredential DEFAULT_CREDENTIAL = null;
    [TraceSource("WSManCI", "Connection Info for WSMAN based connections")]
    internal new static readonly PSTraceSource tracer = PSTraceSource.GetTracer("WSManCI", "Connection Info for WSMAN based connections");
    private Uri connectionUri;
    private PSCredential credential;
    private string shellUri;
    private WSManNativeApi.WSManAuthenticationMechanism authMechanism;
    private bool allowImplicitCredForNegotiate;
    private string thumbPrint;
    private int maxUriRedirectionCount;
    private int? maxRecvdDataSizePerCommand = new int?();
    private int? maxRecvdObjectSize = new int?();
    private bool useCompression = true;
    private bool noMachineProfile;
    private ProxyAccessType proxyAcessType;
    private AuthenticationMechanism proxyAuthentication;
    private PSCredential proxyCredential;
    private bool skipCACheck;
    private bool skipCNCheck;
    private bool skipRevocationCheck;
    private bool noEncryption;
    private bool useUtf16;
    private static string DEFAULT_SCHEME = "http";
    internal static string DEFAULT_SSL_SCHEME = "https";
    private static string DEFAULT_APP_NAME = (string) null;
    private bool useDefaultWSManPort;

    public Uri ConnectionUri
    {
      get
      {
        using (WSManConnectionInfo.tracer.TraceProperty())
          return this.connectionUri;
      }
    }

    public override string ComputerName
    {
      get
      {
        using (WSManConnectionInfo.tracer.TraceProperty())
          return this.ConnectionUri.Host;
      }
    }

    public string Scheme
    {
      get
      {
        using (WSManConnectionInfo.tracer.TraceProperty())
          return this.ConnectionUri.Scheme;
      }
    }

    public int Port
    {
      get
      {
        using (WSManConnectionInfo.tracer.TraceProperty())
          return this.ConnectionUri.Port;
      }
    }

    public string AppName
    {
      get
      {
        using (WSManConnectionInfo.tracer.TraceProperty())
          return this.ConnectionUri.AbsolutePath;
      }
    }

    public override PSCredential Credential
    {
      get
      {
        using (WSManConnectionInfo.tracer.TraceProperty())
          return this.credential;
      }
    }

    public string ShellUri => this.shellUri;

    public override AuthenticationMechanism AuthenticationMechanism
    {
      get
      {
        switch (this.authMechanism)
        {
          case WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION:
            return AuthenticationMechanism.Default;
          case WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_DIGEST:
            return AuthenticationMechanism.Digest;
          case WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE:
            return this.allowImplicitCredForNegotiate ? AuthenticationMechanism.NegotiateWithImplicitCredential : AuthenticationMechanism.Negotiate;
          case WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_BASIC:
            return AuthenticationMechanism.Basic;
          case WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_KERBEROS:
            return AuthenticationMechanism.Kerberos;
          case WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_CREDSSP:
            return AuthenticationMechanism.Credssp;
          default:
            return AuthenticationMechanism.Default;
        }
      }
      set
      {
        switch (value)
        {
          case AuthenticationMechanism.Default:
            this.authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION;
            break;
          case AuthenticationMechanism.Basic:
            this.authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_BASIC;
            break;
          case AuthenticationMechanism.Negotiate:
            this.authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE;
            break;
          case AuthenticationMechanism.NegotiateWithImplicitCredential:
            this.authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE;
            this.allowImplicitCredForNegotiate = true;
            break;
          case AuthenticationMechanism.Credssp:
            this.authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_CREDSSP;
            break;
          case AuthenticationMechanism.Digest:
            this.authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_DIGEST;
            break;
          case AuthenticationMechanism.Kerberos:
            this.authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_KERBEROS;
            break;
          default:
            throw new PSNotSupportedException();
        }
        this.ValidateSpecifiedAuthentication();
      }
    }

    internal WSManNativeApi.WSManAuthenticationMechanism WSManAuthenticationMechanism => this.authMechanism;

    internal bool AllowImplicitCredentialForNegotiate => this.allowImplicitCredForNegotiate;

    public override string CertificateThumbprint => this.thumbPrint;

    public int MaximumConnectionRedirectionCount
    {
      get => this.maxUriRedirectionCount;
      set => this.maxUriRedirectionCount = value;
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

    public bool UseCompression
    {
      get => this.useCompression;
      set => this.useCompression = value;
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
      set
      {
        if (this.proxyAcessType == ProxyAccessType.None)
          throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.ProxyCredentialWithoutAccess, (object) ProxyAccessType.None));
        this.proxyCredential = value;
      }
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

    public WSManConnectionInfo(
      string scheme,
      string computerName,
      int port,
      string appName,
      string shellUri,
      PSCredential credential,
      int openTimeout)
    {
      using (WSManConnectionInfo.tracer.TraceConstructor((object) this))
      {
        if (string.IsNullOrEmpty(scheme) && port == 0)
          this.UseDefaultWSManPort = true;
        this.ConstructWSManConnectionInfo(scheme, computerName, port, appName, shellUri, credential, openTimeout);
      }
    }

    public WSManConnectionInfo(
      string scheme,
      string computerName,
      int port,
      string appName,
      string shellUri,
      PSCredential credential)
      : this(scheme, computerName, port, appName, shellUri, credential, 180000)
    {
    }

    public WSManConnectionInfo(
      bool useSsl,
      string computerName,
      int port,
      string appName,
      string shellUri,
      PSCredential credential)
      : this(useSsl ? WSManConnectionInfo.DEFAULT_SSL_SCHEME : WSManConnectionInfo.DEFAULT_SCHEME, computerName, port, appName, shellUri, credential)
    {
      if (port != 0)
        return;
      this.UseDefaultWSManPort = true;
    }

    public WSManConnectionInfo(
      bool useSsl,
      string computerName,
      int port,
      string appName,
      string shellUri,
      PSCredential credential,
      int openTimeout)
      : this(useSsl ? WSManConnectionInfo.DEFAULT_SSL_SCHEME : WSManConnectionInfo.DEFAULT_SCHEME, computerName, port, appName, shellUri, credential, openTimeout)
    {
      if (port != 0)
        return;
      this.UseDefaultWSManPort = true;
    }

    public WSManConnectionInfo()
      : this(new Uri("http://localhost/wsman"), "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", (PSCredential) null)
      => this.UseDefaultWSManPort = true;

    public WSManConnectionInfo(Uri uri, string shellUri, PSCredential credential)
      : this(uri, shellUri, credential, false)
    {
    }

    internal WSManConnectionInfo(
      Uri uri,
      string shellUri,
      PSCredential credential,
      bool useDefaultPort)
    {
      using (WSManConnectionInfo.tracer.TraceConstructor((object) this))
      {
        string shell = shellUri;
        if (string.IsNullOrEmpty(shell))
          shell = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
        if (uri == (Uri) null)
        {
          this.UseDefaultWSManPort = true;
          this.ConstructWSManConnectionInfo(WSManConnectionInfo.DEFAULT_SCHEME, "localhost", 0, WSManConnectionInfo.DEFAULT_APP_NAME, shell, credential, 180000);
        }
        else
        {
          if (!uri.IsAbsoluteUri)
            throw new NotSupportedException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.RelativeUriForRunspacePathNotSupported));
          this.connectionUri = !uri.AbsolutePath.Equals("/", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) ? uri : WSManConnectionInfo.ConstructUri(uri.Scheme, uri.Host, useDefaultPort ? 0 : uri.Port, WSManConnectionInfo.DEFAULT_APP_NAME);
          this.shellUri = shell;
          this.credential = credential;
          this.OpenTimeout = 180000;
        }
      }
    }

    public WSManConnectionInfo(Uri uri, string shellUri, string certificateThumbprint)
      : this(uri, shellUri, (PSCredential) null)
      => this.thumbPrint = certificateThumbprint;

    public WSManConnectionInfo(Uri uri)
      : this(uri, "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", (PSCredential) null)
    {
    }

    public WSManConnectionInfo Copy()
    {
      WSManConnectionInfo manConnectionInfo = new WSManConnectionInfo(this.connectionUri, this.shellUri, this.credential, this.UseDefaultWSManPort);
      manConnectionInfo.authMechanism = this.authMechanism;
      manConnectionInfo.maxUriRedirectionCount = this.maxUriRedirectionCount;
      manConnectionInfo.maxRecvdDataSizePerCommand = this.maxRecvdDataSizePerCommand;
      manConnectionInfo.maxRecvdObjectSize = this.maxRecvdObjectSize;
      manConnectionInfo.OpenTimeout = this.OpenTimeout;
      manConnectionInfo.IdleTimeout = this.IdleTimeout;
      manConnectionInfo.CancelTimeout = this.CancelTimeout;
      manConnectionInfo.OperationTimeout = this.OperationTimeout;
      manConnectionInfo.Culture = this.Culture;
      manConnectionInfo.UICulture = this.UICulture;
      manConnectionInfo.thumbPrint = this.thumbPrint;
      manConnectionInfo.allowImplicitCredForNegotiate = this.allowImplicitCredForNegotiate;
      manConnectionInfo.UseCompression = this.useCompression;
      manConnectionInfo.NoMachineProfile = this.noMachineProfile;
      manConnectionInfo.proxyAcessType = this.ProxyAccessType;
      manConnectionInfo.proxyAuthentication = this.ProxyAuthentication;
      manConnectionInfo.proxyCredential = this.ProxyCredential;
      manConnectionInfo.skipCACheck = this.SkipCACheck;
      manConnectionInfo.skipCNCheck = this.SkipCNCheck;
      manConnectionInfo.skipRevocationCheck = this.SkipRevocationCheck;
      manConnectionInfo.noEncryption = this.NoEncryption;
      manConnectionInfo.useUtf16 = this.UseUTF16;
      manConnectionInfo.UseDefaultWSManPort = this.UseDefaultWSManPort;
      return manConnectionInfo;
    }

    internal static T ExtractPropertyAsWsManConnectionInfo<T>(
      RunspaceConnectionInfo rsCI,
      string property,
      T defaultValue)
    {
      return !(rsCI is WSManConnectionInfo manConnectionInfo) ? defaultValue : (T) typeof (WSManConnectionInfo).GetProperty(property, typeof (T)).GetValue((object) manConnectionInfo, (object[]) null);
    }

    internal void SetConnectionUri(Uri newUri) => this.connectionUri = newUri;

    private void ConstructWSManConnectionInfo(
      string scheme,
      string computerName,
      int port,
      string appName,
      string shell,
      PSCredential credential,
      int openTimeout)
    {
      using (WSManConnectionInfo.tracer.TraceMethod())
      {
        int num = openTimeout;
        switch (openTimeout)
        {
          case -1:
            num = 180000;
            break;
          case 0:
            num = int.MaxValue;
            break;
        }
        string str1 = shell;
        if (string.IsNullOrEmpty(str1))
          str1 = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
        this.connectionUri = WSManConnectionInfo.ConstructUri(scheme, computerName, port, appName);
        this.shellUri = str1;
        this.credential = credential;
        this.OpenTimeout = num;
        IETWTracer etwTracer;
        using (etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Runspace))
        {
          string str2 = "current user";
          if (credential != null)
            str2 = credential.UserName;
          string str3 = "no thumb print";
          if (!string.IsNullOrEmpty(this.thumbPrint))
            str3 = this.thumbPrint;
          etwTracer.AnalyticChannel.WriteVerbose(PSEventId.WSManConnectionInfoDump, PSOpcode.Method, PSTask.CreateRunspace, (object) this.connectionUri, (object) this.shellUri, (object) str2, (object) this.OpenTimeout.ToString((IFormatProvider) CultureInfo.InvariantCulture), (object) this.IdleTimeout.ToString((IFormatProvider) CultureInfo.InvariantCulture), (object) this.CancelTimeout.ToString((IFormatProvider) CultureInfo.InvariantCulture), (object) (uint) this.authMechanism, (object) str3, (object) this.maxUriRedirectionCount.ToString((IFormatProvider) CultureInfo.InvariantCulture), (object) (this.maxRecvdDataSizePerCommand.HasValue ? this.maxRecvdDataSizePerCommand : new int?(-1)).ToString(), (object) (this.maxRecvdObjectSize.HasValue ? this.maxRecvdObjectSize : new int?(-1)).ToString());
        }
      }
    }

    internal static Uri ConstructUri(
      string scheme,
      string computerName,
      int port,
      string appName)
    {
      IETWTracer etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Runspace);
      string scheme1 = scheme;
      if (string.IsNullOrEmpty(scheme1))
        scheme1 = WSManConnectionInfo.DEFAULT_SCHEME;
      if (!scheme1.Equals("http", StringComparison.OrdinalIgnoreCase) && !scheme1.Equals("https", StringComparison.OrdinalIgnoreCase) && !scheme1.Equals(WSManConnectionInfo.DEFAULT_SCHEME, StringComparison.OrdinalIgnoreCase))
      {
        string str = PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.InvalidSchemeValue, (object) scheme1);
        WSManConnectionInfo.tracer.WriteLine(str, new object[0]);
        throw new ArgumentException(str);
      }
      string str1;
      if (string.IsNullOrEmpty(computerName))
      {
        str1 = "localhost";
      }
      else
      {
        str1 = computerName.Trim();
        IPAddress address = (IPAddress) null;
        if (IPAddress.TryParse(str1, out address) && address.AddressFamily == AddressFamily.InterNetworkV6 && (str1.Length == 0 || str1[0] != '['))
          str1 = "[" + str1 + "]";
      }
      etwTracer.AnalyticChannel.WriteVerbose(PSEventId.ComputerName, PSOpcode.Method, PSTask.CreateRunspace, (object) str1);
      if (port < 0 || port > (int) ushort.MaxValue)
      {
        string str2 = PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.PortIsOutOfRange, (object) port);
        WSManConnectionInfo.tracer.WriteLine(str2, new object[0]);
        throw new ArgumentException(str2);
      }
      int port1 = port;
      if (port1 == 0)
        port1 = !scheme1.Equals("http", StringComparison.OrdinalIgnoreCase) ? -1 : -1;
      string pathValue = appName;
      if (string.IsNullOrEmpty(pathValue))
        pathValue = WSManConnectionInfo.DEFAULT_APP_NAME;
      return new UriBuilder(scheme1, str1, port1, pathValue).Uri;
    }

    internal static string GetConnectionString(Uri connectionUri, out bool isSSLSpecified)
    {
      isSSLSpecified = connectionUri.Scheme.Equals("https");
      string str = connectionUri.OriginalString.TrimStart();
      return isSSLSpecified ? str.Substring("https".Length + 3) : str.Substring("http".Length + 3);
    }

    private void ValidateSpecifiedAuthentication()
    {
      if (this.authMechanism != WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION && this.thumbPrint != null)
        throw WSManConnectionInfo.tracer.NewInvalidOperationException("RemotingErrorIdStrings", PSRemotingErrorId.NewRunspaceAmbiguosAuthentication.ToString(), (object) "CertificateThumbPrint", (object) this.AuthenticationMechanism.ToString());
    }

    internal bool UseDefaultWSManPort
    {
      get => this.useDefaultWSManPort;
      set => this.useDefaultWSManPort = value;
    }

    internal override void SetSessionOptions(PSSessionOption options)
    {
      if (options.ProxyAccessType == ProxyAccessType.None && options.ProxyCredential != null)
        throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.ProxyCredentialWithoutAccess, (object) ProxyAccessType.None));
      base.SetSessionOptions(options);
      this.MaximumConnectionRedirectionCount = options.MaximumConnectionRedirectionCount >= 0 ? options.MaximumConnectionRedirectionCount : int.MaxValue;
      this.MaximumReceivedDataSizePerCommand = options.MaximumReceivedDataSizePerCommand;
      this.MaximumReceivedObjectSize = options.MaximumReceivedObjectSize;
      this.UseCompression = !options.NoCompression;
      this.NoMachineProfile = options.NoMachineProfile;
      this.proxyAcessType = options.ProxyAccessType;
      this.proxyAuthentication = options.ProxyAuthentication;
      this.proxyCredential = options.ProxyCredential;
      this.skipCACheck = options.SkipCACheck;
      this.skipCNCheck = options.SkipCNCheck;
      this.skipRevocationCheck = options.SkipRevocationCheck;
      this.noEncryption = options.NoEncryption;
      this.useUtf16 = options.UseUTF16;
    }
  }
}
