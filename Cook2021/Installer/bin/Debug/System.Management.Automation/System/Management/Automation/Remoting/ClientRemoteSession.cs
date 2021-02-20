// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ClientRemoteSession
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Runspaces.Internal;

namespace System.Management.Automation.Remoting
{
  internal abstract class ClientRemoteSession : RemoteSession
  {
    [TraceSource("CRSession", "ClientRemoteSession")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("CRSession", nameof (ClientRemoteSession));
    private ClientRemoteSessionContext _context = new ClientRemoteSessionContext();
    private ClientRemoteSessionDataStructureHandler _sessionDSHandler;
    private RemoteRunspacePoolInternal remoteRunspacePool;

    public abstract void ConnectAsync();

    public abstract event EventHandler<RemoteSessionStateEventArgs> StateChanged;

    public abstract void CloseAsync();

    internal ClientRemoteSessionContext Context => this._context;

    internal ClientRemoteSessionDataStructureHandler SessionDataStructureHandler
    {
      get
      {
        using (ClientRemoteSession._trace.TraceProperty())
          return this._sessionDSHandler;
      }
      set
      {
        using (ClientRemoteSession._trace.TraceProperty())
          this._sessionDSHandler = value;
      }
    }

    internal RemoteRunspacePoolInternal RemoteRunspacePoolInternal
    {
      get
      {
        using (ClientRemoteSession._trace.TraceProperty())
          return this.remoteRunspacePool;
      }
      set
      {
        using (ClientRemoteSession._trace.TraceProperty())
          this.remoteRunspacePool = value;
      }
    }

    internal RemoteRunspacePoolInternal GetRunspacePool(
      Guid clientRunspacePoolId)
    {
      using (ClientRemoteSession._trace.TraceMethod())
        return this.remoteRunspacePool != null && this.remoteRunspacePool.InstanceId.Equals(clientRunspacePoolId) ? this.remoteRunspacePool : (RemoteRunspacePoolInternal) null;
    }

    internal delegate void URIDirectionReported(Uri newURI);
  }
}
