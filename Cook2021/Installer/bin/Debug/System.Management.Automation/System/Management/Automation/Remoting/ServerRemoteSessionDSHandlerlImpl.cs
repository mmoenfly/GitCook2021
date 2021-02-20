// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ServerRemoteSessionDSHandlerlImpl
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting.Server;

namespace System.Management.Automation.Remoting
{
  internal class ServerRemoteSessionDSHandlerlImpl : ServerRemoteSessionDataStructureHandler
  {
    [TraceSource("ServerRemoteSessionDSHandlerlImpl", "ServerRemoteSessionDSHandlerlImpl")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (ServerRemoteSessionDSHandlerlImpl), nameof (ServerRemoteSessionDSHandlerlImpl));
    private AbstractServerSessionTransportManager _transportManager;
    private ServerRemoteSessionDSHandlerStateMachine _stateMachine;
    private ServerRemoteSession _session;

    internal override AbstractServerSessionTransportManager TransportManager => this._transportManager;

    internal ServerRemoteSessionDSHandlerlImpl(
      ServerRemoteSession session,
      AbstractServerSessionTransportManager transportManager)
    {
      this._session = session;
      this._stateMachine = new ServerRemoteSessionDSHandlerStateMachine(session);
      this._transportManager = transportManager;
      this._transportManager.DataReceived += new EventHandler<RemoteDataEventArgs>(session.DispatchInputQueueData);
    }

    internal override void ConnectAsync()
    {
      using (ServerRemoteSessionDSHandlerlImpl._trace.TraceMethod())
        ;
    }

    internal override void SendNegotiationAsync()
    {
      using (ServerRemoteSessionDSHandlerlImpl._trace.TraceMethod())
      {
        RemoteDataObject sessionCapability = RemotingEncoder.GenerateServerSessionCapability(this._session.Context.ServerCapability, Guid.Empty);
        this._stateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSendCompleted));
        this._transportManager.SendDataToClient<PSObject>(RemoteDataObject<PSObject>.CreateFrom(sessionCapability.Destination, sessionCapability.DataType, sessionCapability.RunspacePoolId, sessionCapability.PowerShellId, (PSObject) sessionCapability.Data), false);
      }
    }

    internal override event EventHandler<RemoteSessionNegotiationEventArgs> NegotiationReceived;

    internal override event EventHandler<EventArgs> SessionClosing;

    internal override event EventHandler<RemoteDataEventArgs<string>> PublicKeyReceived;

    internal override void SendEncryptedSessionKey(string encryptedSessionKey) => this._transportManager.SendDataToClient<object>((RemoteDataObject<object>) RemotingEncoder.GenerateEncryptedSessionKeyResponse(Guid.Empty, encryptedSessionKey), true);

    internal override void SendRequestForPublicKey() => this._transportManager.SendDataToClient<object>((RemoteDataObject<object>) RemotingEncoder.GeneratePublicKeyRequest(Guid.Empty), true);

    internal override void RaiseKeyExchangeMessageReceived(RemoteDataObject<PSObject> receivedData) => this.RaiseDataReceivedEvent(new RemoteDataEventArgs(receivedData));

    internal override void CloseConnectionAsync(Exception reasonForClose)
    {
      using (ServerRemoteSessionDSHandlerlImpl._trace.TraceMethod())
      {
        if (this.SessionClosing != null)
          this.SessionClosing((object) this, EventArgs.Empty);
        this._transportManager.Close(reasonForClose);
        this._stateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.CloseCompleted));
      }
    }

    internal override event EventHandler<RemoteDataEventArgs> CreateRunspacePoolReceived;

    internal override ServerRemoteSessionDSHandlerStateMachine StateMachine
    {
      get
      {
        using (ServerRemoteSessionDSHandlerlImpl._trace.TraceProperty())
          return this._stateMachine;
      }
    }

    internal override void RaiseDataReceivedEvent(RemoteDataEventArgs dataArg)
    {
      using (ServerRemoteSessionDSHandlerlImpl._trace.TraceMethod())
      {
        RemoteDataObject<PSObject> remoteDataObject = dataArg != null ? dataArg.ReceivedData : throw ServerRemoteSessionDSHandlerlImpl._trace.NewArgumentNullException(nameof (dataArg));
        int targetInterface = (int) remoteDataObject.TargetInterface;
        RemotingDataType dataType = remoteDataObject.DataType;
        switch (dataType)
        {
          case RemotingDataType.SessionCapability:
            RemoteSessionCapability sessionCapability;
            try
            {
              sessionCapability = RemotingDecoder.GetSessionCapability((object) remoteDataObject.Data);
            }
            catch (PSRemotingDataStructureException ex)
            {
              throw new PSRemotingDataStructureException(PSRemotingErrorId.ServerNotFoundCapabilityProperties, new object[3]
              {
                (object) ex.Message,
                (object) PSVersionInfo.BuildVersion,
                (object) RemotingConstants.ProtocolVersion
              });
            }
            this._stateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationReceived)
            {
              RemoteSessionCapability = sessionCapability
            });
            if (this.NegotiationReceived == null)
              break;
            this.NegotiationReceived((object) this, new RemoteSessionNegotiationEventArgs(sessionCapability)
            {
              RemoteData = remoteDataObject
            });
            break;
          case RemotingDataType.CloseSession:
            this._stateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, (Exception) new PSRemotingDataStructureException(PSRemotingErrorId.ClientRequestedToCloseSession, new object[0])));
            break;
          case RemotingDataType.CreateRunspacePool:
            if (this.CreateRunspacePoolReceived == null)
              break;
            this.CreateRunspacePoolReceived((object) this, dataArg);
            break;
          case RemotingDataType.PublicKey:
            this.PublicKeyReceived((object) this, new RemoteDataEventArgs<string>((object) RemotingDecoder.GetPublicKey(remoteDataObject.Data)));
            break;
          default:
            throw new PSRemotingDataStructureException(PSRemotingErrorId.ReceivedUnsupportedAction, new object[1]
            {
              (object) dataType
            });
        }
      }
    }
  }
}
