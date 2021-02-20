// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ClientRemoteSessionDataStructureHandler
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting.Client;
using System.Management.Automation.Runspaces.Internal;

namespace System.Management.Automation.Remoting
{
  internal abstract class ClientRemoteSessionDataStructureHandler : BaseSessionDataStructureHandler
  {
    [TraceSource("SRSessionDSHdler", "ServerRemoteSessionDataStructureHandler")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("SRSessionDSHdler", "ServerRemoteSessionDataStructureHandler");

    internal abstract void ConnectAsync();

    internal abstract event EventHandler<RemoteSessionStateEventArgs> ConnectionStateChanged;

    internal abstract void SendNegotiationAsync();

    internal abstract event EventHandler<RemoteSessionNegotiationEventArgs> NegotiationReceived;

    internal abstract void CloseConnectionAsync();

    internal abstract ClientRemoteSessionDSHandlerStateMachine StateMachine { get; }

    internal abstract BaseClientSessionTransportManager TransportManager { get; }

    internal abstract BaseClientCommandTransportManager CreateClientCommandTransportManager(
      ClientRemotePowerShell cmd,
      bool noInput);

    internal abstract event EventHandler<RemoteDataEventArgs<string>> EncryptedSessionKeyReceived;

    internal abstract event EventHandler<RemoteDataEventArgs<string>> PublicKeyRequestReceived;

    internal abstract void SendPublicKeyAsync(string localPublicKey);
  }
}
