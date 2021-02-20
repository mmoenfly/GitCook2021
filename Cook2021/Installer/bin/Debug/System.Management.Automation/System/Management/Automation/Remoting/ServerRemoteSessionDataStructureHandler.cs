// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ServerRemoteSessionDataStructureHandler
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting.Server;

namespace System.Management.Automation.Remoting
{
  internal abstract class ServerRemoteSessionDataStructureHandler : BaseSessionDataStructureHandler
  {
    [TraceSource("ServerRemoteSessionDataStructureHandler", "ServerRemoteSessionDataStructureHandler")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (ServerRemoteSessionDataStructureHandler), nameof (ServerRemoteSessionDataStructureHandler));

    internal ServerRemoteSessionDataStructureHandler()
    {
      using (ServerRemoteSessionDataStructureHandler._trace.TraceConstructor((object) this))
        ;
    }

    internal abstract void ConnectAsync();

    internal abstract void SendNegotiationAsync();

    internal abstract event EventHandler<RemoteSessionNegotiationEventArgs> NegotiationReceived;

    internal abstract void CloseConnectionAsync(Exception reasonForClose);

    internal abstract event EventHandler<EventArgs> SessionClosing;

    internal abstract event EventHandler<RemoteDataEventArgs> CreateRunspacePoolReceived;

    internal abstract ServerRemoteSessionDSHandlerStateMachine StateMachine { get; }

    internal abstract AbstractServerSessionTransportManager TransportManager { get; }

    internal abstract void RaiseDataReceivedEvent(RemoteDataEventArgs arg);

    internal abstract event EventHandler<RemoteDataEventArgs<string>> PublicKeyReceived;

    internal abstract void SendRequestForPublicKey();

    internal abstract void SendEncryptedSessionKey(string encryptedSessionKey);
  }
}
