// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.PSIdentity
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Security.Principal;

namespace System.Management.Automation.Remoting
{
  public sealed class PSIdentity : IIdentity
  {
    private string authenticationType;
    private bool isAuthenticated;
    private string userName;
    private PSCertificateDetails certDetails;

    public string AuthenticationType => this.authenticationType;

    public bool IsAuthenticated => this.isAuthenticated;

    public string Name => this.userName;

    public PSCertificateDetails CertificateDetails => this.certDetails;

    internal PSIdentity(
      string authType,
      bool isAuthenticated,
      string userName,
      PSCertificateDetails cert)
    {
      this.authenticationType = authType;
      this.isAuthenticated = isAuthenticated;
      this.userName = userName;
      this.certDetails = cert;
    }
  }
}
