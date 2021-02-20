// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.NewProcessConnectionInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting;

namespace System.Management.Automation.Runspaces
{
  internal sealed class NewProcessConnectionInfo : RunspaceConnectionInfo
  {
    private PSCredential credential;
    private AuthenticationMechanism authMechanism;
    private ScriptBlock initScript;
    private bool shouldRunsAs32;

    public ScriptBlock InitializationScript
    {
      get => this.initScript;
      set => this.initScript = value;
    }

    public bool RunAs32
    {
      get => this.shouldRunsAs32;
      set => this.shouldRunsAs32 = value;
    }

    public override string ComputerName => "localhost";

    public override PSCredential Credential => this.credential;

    public override AuthenticationMechanism AuthenticationMechanism
    {
      get => this.authMechanism;
      set => this.authMechanism = value == AuthenticationMechanism.Default ? value : throw RunspaceConnectionInfo.tracer.NewInvalidOperationException("RemotingErrorIdStrings", PSRemotingErrorId.IPCSupportsOnlyDefaultAuth.ToString(), (object) value.ToString(), (object) AuthenticationMechanism.Default.ToString());
    }

    public override string CertificateThumbprint => string.Empty;

    public NewProcessConnectionInfo Copy()
    {
      NewProcessConnectionInfo processConnectionInfo = new NewProcessConnectionInfo(this.credential);
      processConnectionInfo.AuthenticationMechanism = this.AuthenticationMechanism;
      processConnectionInfo.InitializationScript = this.InitializationScript;
      processConnectionInfo.RunAs32 = this.RunAs32;
      return processConnectionInfo;
    }

    internal NewProcessConnectionInfo(PSCredential credential)
    {
      this.credential = credential;
      this.authMechanism = AuthenticationMechanism.Default;
    }
  }
}
