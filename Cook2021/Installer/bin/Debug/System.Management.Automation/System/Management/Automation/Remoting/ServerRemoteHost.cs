// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ServerRemoteHost
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Remoting.Server;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace System.Management.Automation.Remoting
{
  internal class ServerRemoteHost : PSHost, IHostSupportsInteractiveSession
  {
    private Guid _instanceId = Guid.NewGuid();
    private ServerRemoteHostUserInterface _remoteHostUserInterface;
    private ServerMethodExecutor _serverMethodExecutor;
    private Guid _clientRunspacePoolId;
    private Guid _clientPowerShellId;
    private HostInfo _hostInfo;
    private AbstractServerTransportManager _transportManager;

    internal ServerRemoteHost(
      Guid clientRunspacePoolId,
      Guid clientPowerShellId,
      HostInfo hostInfo,
      AbstractServerTransportManager transportManager)
    {
      this._clientRunspacePoolId = clientRunspacePoolId;
      this._clientPowerShellId = clientPowerShellId;
      this._hostInfo = hostInfo;
      this._transportManager = transportManager;
      this._serverMethodExecutor = new ServerMethodExecutor(clientRunspacePoolId, clientPowerShellId, this._transportManager);
      this._remoteHostUserInterface = hostInfo.IsHostUINull ? (ServerRemoteHostUserInterface) null : new ServerRemoteHostUserInterface(this);
    }

    internal ServerMethodExecutor ServerMethodExecutor => this._serverMethodExecutor;

    public override PSHostUserInterface UI => (PSHostUserInterface) this._remoteHostUserInterface;

    public override string Name => nameof (ServerRemoteHost);

    public override Version Version => RemotingConstants.HostVersion;

    public override Guid InstanceId => this._instanceId;

    public override void SetShouldExit(int exitCode) => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetShouldExit, new object[1]
    {
      (object) exitCode
    });

    public override void EnterNestedPrompt() => throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.EnterNestedPrompt);

    public override void ExitNestedPrompt() => throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.ExitNestedPrompt);

    public override void NotifyBeginApplication()
    {
    }

    public override void NotifyEndApplication()
    {
    }

    public override CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;

    public override CultureInfo CurrentUICulture => Thread.CurrentThread.CurrentUICulture;

    public void PushRunspace(Runspace runspace) => throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.PushRunspace);

    public void PopRunspace() => this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.PopRunspace);

    public bool IsRunspacePushed => throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetIsRunspacePushed);

    public Runspace Runspace => throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetRunspace);

    internal HostInfo HostInfo => this._hostInfo;
  }
}
