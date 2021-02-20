// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ClientRemoteSessionDSHandlerImpl
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;
using System.Management.Automation.Remoting.Client;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Runspaces.Internal;

namespace System.Management.Automation.Remoting
{
  internal class ClientRemoteSessionDSHandlerImpl : 
    ClientRemoteSessionDataStructureHandler,
    IDisposable
  {
    private const string resBaseName = "remotingerroridstrings";
    [TraceSource("CRSDSHdlerImpl", "ClientRemoteSessionDSHandlerImpl")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("CRSDSHdlerImpl", nameof (ClientRemoteSessionDSHandlerImpl));
    private BaseClientSessionTransportManager _transportManager;
    private ClientRemoteSessionDSHandlerStateMachine _stateMachine;
    private ClientRemoteSession _session;
    private RunspaceConnectionInfo _connectionInfo;
    private Uri _redirectUri;
    private int maxUriRedirectionCount;
    private bool isCloseCalled;
    private object syncObject = new object();
    private PSRemotingCryptoHelper _cryptoHelper;
    private ClientRemoteSession.URIDirectionReported uriRedirectionHandler;

    internal override BaseClientSessionTransportManager TransportManager
    {
      get
      {
        using (ClientRemoteSessionDSHandlerImpl._trace.TraceProperty())
          return this._transportManager;
      }
    }

    internal override BaseClientCommandTransportManager CreateClientCommandTransportManager(
      ClientRemotePowerShell cmd,
      bool noInput)
    {
      BaseClientCommandTransportManager transportManager = this._transportManager.CreateClientCommandTransportManager(this._connectionInfo, cmd, noInput);
      transportManager.DataReceived += new EventHandler<RemoteDataEventArgs>(this.DispatchInputQueueData);
      return transportManager;
    }

    internal ClientRemoteSessionDSHandlerImpl(
      ClientRemoteSession session,
      PSRemotingCryptoHelper cryptoHelper,
      RunspaceConnectionInfo connectionInfo,
      ClientRemoteSession.URIDirectionReported uriRedirectionHandler)
    {
      using (ClientRemoteSessionDSHandlerImpl._trace.TraceConstructor((object) this))
      {
        this._session = session != null ? session : throw ClientRemoteSessionDSHandlerImpl._trace.NewArgumentNullException(nameof (session));
        this._stateMachine = new ClientRemoteSessionDSHandlerStateMachine();
        this._stateMachine.StateChanged += new EventHandler<RemoteSessionStateEventArgs>(this.HandleStateChanged);
        this._connectionInfo = connectionInfo;
        this._cryptoHelper = cryptoHelper;
        this._transportManager = !(this._connectionInfo is NewProcessConnectionInfo) ? (BaseClientSessionTransportManager) new WSManClientSessionTransportManager(this._session.RemoteRunspacePoolInternal.InstanceId, (WSManConnectionInfo) this._connectionInfo, cryptoHelper) : (BaseClientSessionTransportManager) new OutOfProcessClientSessionTransportManager(this._session.RemoteRunspacePoolInternal.InstanceId, (NewProcessConnectionInfo) this._connectionInfo, cryptoHelper);
        this._transportManager.DataReceived += new EventHandler<RemoteDataEventArgs>(this.DispatchInputQueueData);
        this._transportManager.WSManTransportErrorOccured += new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportError);
        this._transportManager.CloseCompleted += new EventHandler<EventArgs>(this.HandleCloseComplete);
        if (!(connectionInfo is WSManConnectionInfo manConnectionInfo))
          return;
        this.uriRedirectionHandler = uriRedirectionHandler;
        this.maxUriRedirectionCount = manConnectionInfo.MaximumConnectionRedirectionCount;
      }
    }

    internal override void ConnectAsync()
    {
      using (ClientRemoteSessionDSHandlerImpl._trace.TraceMethod())
      {
        this._transportManager.ConnectCompleted += new EventHandler<EventArgs>(this.HandleConnectComplete);
        this._transportManager.ConnectAsync();
      }
    }

    private void HandleConnectComplete(object sender, EventArgs args)
    {
    }

    internal override void CloseConnectionAsync()
    {
      using (ClientRemoteSessionDSHandlerImpl._trace.TraceMethod())
      {
        lock (this.syncObject)
        {
          if (this.isCloseCalled)
            return;
          this._transportManager.CloseAsync();
          this.isCloseCalled = true;
        }
      }
    }

    private void HandleCloseComplete(object sender, EventArgs args)
    {
      using (ClientRemoteSessionDSHandlerImpl._trace.TraceMethod())
        this._stateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.CloseCompleted));
    }

    internal override void SendNegotiationAsync()
    {
      this._transportManager.ConnectAsync();
      this._stateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSendCompleted));
    }

    internal override event EventHandler<RemoteSessionNegotiationEventArgs> NegotiationReceived;

    internal override event EventHandler<RemoteSessionStateEventArgs> ConnectionStateChanged;

    private void HandleStateChanged(object sender, RemoteSessionStateEventArgs arg)
    {
      using (ClientRemoteSessionDSHandlerImpl._trace.TraceMethod())
      {
        if (arg == null)
          throw ClientRemoteSessionDSHandlerImpl._trace.NewArgumentNullException(nameof (arg));
        if (arg.SessionStateInfo.State == RemoteSessionState.NegotiationSending)
          this.HandleNegotiationSendingStateChange();
        if (this.ConnectionStateChanged != null)
          this.ConnectionStateChanged((object) this, arg);
        if (arg.SessionStateInfo.State == RemoteSessionState.NegotiationSending)
          this.SendNegotiationAsync();
        if (arg.SessionStateInfo.State != RemoteSessionState.ClosingConnection)
          return;
        this.CloseConnectionAsync();
      }
    }

    private void HandleNegotiationSendingStateChange()
    {
      RemoteDataObject sessionCapability = RemotingEncoder.GenerateClientSessionCapability(this._session.Context.ClientCapability, this._session.RemoteRunspacePoolInternal.InstanceId);
      this._transportManager.DataToBeSentCollection.Add<PSObject>(RemoteDataObject<PSObject>.CreateFrom(sessionCapability.Destination, sessionCapability.DataType, sessionCapability.RunspacePoolId, sessionCapability.PowerShellId, (PSObject) sessionCapability.Data));
    }

    internal override ClientRemoteSessionDSHandlerStateMachine StateMachine
    {
      get
      {
        using (ClientRemoteSessionDSHandlerImpl._trace.TraceProperty())
          return this._stateMachine;
      }
    }

    private void PerformURIRedirection(string newURIString)
    {
      this._redirectUri = new Uri(newURIString);
      lock (this.syncObject)
      {
        if (this.isCloseCalled)
          return;
        this._transportManager.CloseCompleted -= new EventHandler<EventArgs>(this.HandleCloseComplete);
        this._transportManager.WSManTransportErrorOccured -= new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportError);
        this._transportManager.CloseCompleted += new EventHandler<EventArgs>(this.HandleTransportCloseCompleteForRedirection);
        this._transportManager.WSManTransportErrorOccured += new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportErrorForRedirection);
        this._transportManager.PrepareForRedirection();
      }
    }

    private void HandleTransportCloseCompleteForRedirection(object source, EventArgs args)
    {
      this._transportManager.CloseCompleted -= new EventHandler<EventArgs>(this.HandleTransportCloseCompleteForRedirection);
      this._transportManager.WSManTransportErrorOccured -= new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportErrorForRedirection);
      this._transportManager.CloseCompleted += new EventHandler<EventArgs>(this.HandleCloseComplete);
      this._transportManager.WSManTransportErrorOccured += new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportError);
      this.PerformURIRedirectionStep2(this._redirectUri);
    }

    private void HandleTransportErrorForRedirection(object sender, TransportErrorOccuredEventArgs e)
    {
      this._transportManager.CloseCompleted -= new EventHandler<EventArgs>(this.HandleTransportCloseCompleteForRedirection);
      this._transportManager.WSManTransportErrorOccured -= new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportErrorForRedirection);
      this._transportManager.CloseCompleted += new EventHandler<EventArgs>(this.HandleCloseComplete);
      this._transportManager.WSManTransportErrorOccured += new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportError);
      this.HandleTransportError(sender, e);
    }

    private void PerformURIRedirectionStep2(Uri newURI)
    {
      lock (this.syncObject)
      {
        if (this.isCloseCalled)
          return;
        if (this.uriRedirectionHandler != null)
          this.uriRedirectionHandler(newURI);
        this._transportManager.Redirect(newURI, this._connectionInfo);
      }
    }

    internal void HandleTransportError(object sender, TransportErrorOccuredEventArgs e)
    {
      if (e.Exception is PSRemotingTransportRedirectException exception && this.maxUriRedirectionCount > 0)
      {
        Exception exception;
        try
        {
          --this.maxUriRedirectionCount;
          this.PerformURIRedirection(exception.RedirectLocation);
          return;
        }
        catch (ArgumentNullException ex)
        {
          exception = (Exception) ex;
        }
        catch (UriFormatException ex)
        {
          exception = (Exception) ex;
        }
        if (exception != null)
          e.Exception = new PSRemotingTransportException(PSRemotingErrorId.RedirectedURINotWellFormatted, new object[2]
          {
            (object) this._session.Context.RemoteAddress.OriginalString,
            (object) exception.RedirectLocation
          })
          {
            TransportMessage = e.Exception.TransportMessage
          };
      }
      RemoteSessionEvent stateEvent = RemoteSessionEvent.ConnectFailed;
      switch (e.ReportingTransportMethod)
      {
        case TransportMethodEnum.CreateShellEx:
          stateEvent = RemoteSessionEvent.ConnectFailed;
          break;
        case TransportMethodEnum.SendShellInputEx:
        case TransportMethodEnum.CommandInputEx:
          stateEvent = RemoteSessionEvent.SendFailed;
          break;
        case TransportMethodEnum.ReceiveShellOutputEx:
        case TransportMethodEnum.ReceiveCommandOutputEx:
          stateEvent = RemoteSessionEvent.ReceiveFailed;
          break;
        case TransportMethodEnum.CloseShellOperationEx:
          stateEvent = RemoteSessionEvent.CloseFailed;
          break;
      }
      this._stateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(stateEvent, (Exception) e.Exception));
    }

    internal void DispatchInputQueueData(object sender, RemoteDataEventArgs dataArg)
    {
      using (ClientRemoteSessionDSHandlerImpl._trace.TraceMethod())
      {
        RemoteDataObject<PSObject> remoteDataObject = dataArg != null ? dataArg.ReceivedData : throw ClientRemoteSessionDSHandlerImpl._trace.NewArgumentNullException(nameof (dataArg));
        RemotingDestination remotingDestination = remoteDataObject != null ? remoteDataObject.Destination : throw ClientRemoteSessionDSHandlerImpl._trace.NewArgumentException(nameof (dataArg));
        if ((remotingDestination & RemotingDestination.Client) != RemotingDestination.Client)
          throw new PSRemotingDataStructureException(PSRemotingErrorId.RemotingDestinationNotForMe, new object[2]
          {
            (object) RemotingDestination.Client,
            (object) remotingDestination
          });
        switch (remoteDataObject.TargetInterface)
        {
          case RemotingTargetInterface.Session:
            this.ProcessSessionMessages(dataArg);
            break;
          case RemotingTargetInterface.RunspacePool:
          case RemotingTargetInterface.PowerShell:
            RemoteSessionStateMachineEventArgs machineEventArgs = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.MessageReceived, (Exception) null);
            if (this.StateMachine.CanByPassRaiseEvent(machineEventArgs))
            {
              this.ProcessNonSessionMessages(dataArg.ReceivedData);
              break;
            }
            this.StateMachine.RaiseEvent(machineEventArgs);
            break;
        }
      }
    }

    private void ProcessSessionMessages(RemoteDataEventArgs arg)
    {
      using (ClientRemoteSessionDSHandlerImpl._trace.TraceMethod())
      {
        RemoteDataObject<PSObject> remoteDataObject = arg != null && arg.ReceivedData != null ? arg.ReceivedData : throw ClientRemoteSessionDSHandlerImpl._trace.NewArgumentNullException(nameof (arg));
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
              throw new PSRemotingDataStructureException(PSRemotingErrorId.ClientNotFoundCapabilityProperties, new object[3]
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
            this.NegotiationReceived((object) this, new RemoteSessionNegotiationEventArgs(sessionCapability));
            break;
          case RemotingDataType.CloseSession:
            this._stateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, (Exception) new PSRemotingDataStructureException(PSRemotingErrorId.ServerRequestedToCloseSession, new object[0])));
            break;
          case RemotingDataType.EncryptedSessionKey:
            this.EncryptedSessionKeyReceived((object) this, new RemoteDataEventArgs<string>((object) RemotingDecoder.GetEncryptedSessionKey(remoteDataObject.Data)));
            break;
          case RemotingDataType.PublicKeyRequest:
            this.PublicKeyRequestReceived((object) this, new RemoteDataEventArgs<string>((object) string.Empty));
            break;
          default:
            throw new PSRemotingDataStructureException(PSRemotingErrorId.ReceivedUnsupportedAction, new object[1]
            {
              (object) dataType
            });
        }
      }
    }

    internal void ProcessNonSessionMessages(RemoteDataObject<PSObject> rcvdData)
    {
      using (ClientRemoteSessionDSHandlerImpl._trace.TraceMethod())
      {
        if (rcvdData == null)
          throw ClientRemoteSessionDSHandlerImpl._trace.NewArgumentNullException(nameof (rcvdData));
        switch (rcvdData.TargetInterface)
        {
          case RemotingTargetInterface.RunspacePool:
            Guid runspacePoolId = rcvdData.RunspacePoolId;
            RemoteRunspacePoolInternal runspacePool = this._session.GetRunspacePool(runspacePoolId);
            if (runspacePool != null)
            {
              runspacePool.DataStructureHandler.ProcessReceivedData(rcvdData);
              break;
            }
            ClientRemoteSessionDSHandlerImpl._trace.WriteLine("Client received data for Runspace (id: {0}), \r\n                                but the Runspace cannot be found", (object) runspacePoolId);
            break;
          case RemotingTargetInterface.PowerShell:
            this._session.GetRunspacePool(rcvdData.RunspacePoolId).DataStructureHandler.DispatchMessageToPowerShell(rcvdData);
            break;
        }
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      this._transportManager.Dispose();
    }

    internal override event EventHandler<RemoteDataEventArgs<string>> EncryptedSessionKeyReceived;

    internal override event EventHandler<RemoteDataEventArgs<string>> PublicKeyRequestReceived;

    internal override void SendPublicKeyAsync(string localPublicKey) => this._transportManager.DataToBeSentCollection.Add<object>((RemoteDataObject<object>) RemotingEncoder.GenerateMyPublicKey(this._session.RemoteRunspacePoolInternal.InstanceId, localPublicKey, RemotingDestination.Server));

    internal override void RaiseKeyExchangeMessageReceived(RemoteDataObject<PSObject> receivedData) => this.ProcessSessionMessages(new RemoteDataEventArgs(receivedData));
  }
}
