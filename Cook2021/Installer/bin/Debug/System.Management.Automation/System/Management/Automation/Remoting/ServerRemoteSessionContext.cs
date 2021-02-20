// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ServerRemoteSessionContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting
{
  internal class ServerRemoteSessionContext
  {
    [TraceSource("ServerRemoteSessionContext", "ServerRemoteSessionContext")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (ServerRemoteSessionContext), nameof (ServerRemoteSessionContext));
    private RemoteSessionCapability _clientCapability;
    private RemoteSessionCapability _serverCapability;
    private bool isNegotiationSucceeded;

    internal ServerRemoteSessionContext()
    {
      using (ServerRemoteSessionContext._trace.TraceConstructor((object) this))
        this._serverCapability = RemoteSessionCapability.CreateServerCapability();
    }

    internal RemoteSessionCapability ClientCapability
    {
      get
      {
        using (ServerRemoteSessionContext._trace.TraceProperty())
          return this._clientCapability;
      }
      set
      {
        using (ServerRemoteSessionContext._trace.TraceProperty())
          this._clientCapability = value;
      }
    }

    internal RemoteSessionCapability ServerCapability
    {
      get
      {
        using (ServerRemoteSessionContext._trace.TraceProperty())
          return this._serverCapability;
      }
      set
      {
        using (ServerRemoteSessionContext._trace.TraceProperty())
          this._serverCapability = value;
      }
    }

    internal bool IsNegotiationSucceeded
    {
      get => this.isNegotiationSucceeded;
      set => this.isNegotiationSucceeded = value;
    }
  }
}
