// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ClientRemoteSessionContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting
{
  internal class ClientRemoteSessionContext
  {
    private Uri _remoteAddress;
    private PSCredential _userCredential;
    private RemoteSessionCapability _clientCapability;
    private RemoteSessionCapability _serverCapability;
    private string _shellName;
    [TraceSource("CRSessionCtxt", "ClientRemoteSessionContext")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("CRSessionCtxt", nameof (ClientRemoteSessionContext));

    internal Uri RemoteAddress
    {
      get
      {
        using (ClientRemoteSessionContext._trace.TraceProperty())
          return this._remoteAddress;
      }
      set
      {
        using (ClientRemoteSessionContext._trace.TraceProperty())
          this._remoteAddress = value;
      }
    }

    internal PSCredential UserCredential
    {
      get
      {
        using (ClientRemoteSessionContext._trace.TraceProperty())
          return this._userCredential;
      }
      set
      {
        using (ClientRemoteSessionContext._trace.TraceProperty())
          this._userCredential = value;
      }
    }

    internal RemoteSessionCapability ClientCapability
    {
      get
      {
        using (ClientRemoteSessionContext._trace.TraceProperty())
          return this._clientCapability;
      }
      set
      {
        using (ClientRemoteSessionContext._trace.TraceProperty())
          this._clientCapability = value;
      }
    }

    internal RemoteSessionCapability ServerCapability
    {
      get
      {
        using (ClientRemoteSessionContext._trace.TraceProperty())
          return this._serverCapability;
      }
      set
      {
        using (ClientRemoteSessionContext._trace.TraceProperty())
          this._serverCapability = value;
      }
    }

    internal string ShellName
    {
      get
      {
        using (ClientRemoteSessionContext._trace.TraceProperty())
          return this._shellName;
      }
      set
      {
        using (ClientRemoteSessionContext._trace.TraceProperty())
          this._shellName = value;
      }
    }
  }
}
