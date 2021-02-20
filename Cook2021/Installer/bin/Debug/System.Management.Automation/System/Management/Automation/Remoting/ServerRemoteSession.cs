// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ServerRemoteSession
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting.Server;
using System.Management.Automation.Runspaces;
using System.Security.Principal;
using System.Threading;

namespace System.Management.Automation.Remoting
{
  internal class ServerRemoteSession : RemoteSession
  {
    [TraceSource("ServerRemoteSession", "ServerRemoteSession")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (ServerRemoteSession), nameof (ServerRemoteSession));
    private ServerRemoteSessionContext _context;
    private ServerRemoteSessionDataStructureHandler _sessionDSHandler;
    private PSSenderInfo _senderInfo;
    private string _configProviderId;
    private string _initParameters;
    private string _initScriptForOutOfProcRS;
    private PSSessionConfiguration _sessionConfigProvider;
    private int? maxRecvdObjectSize;
    private int? maxRecvdDataSizeCommand;
    private ServerRunspacePoolDriver _runspacePoolDriver;
    private PSRemotingCryptoHelperServer _cryptoHelper;
    internal EventHandler<RemoteSessionStateMachineEventArgs> Closed;

    internal ServerRemoteSession(
      PSSenderInfo senderInfo,
      string configurationProviderId,
      string initializationParameters,
      AbstractServerSessionTransportManager transportManager)
    {
      NativeCommandProcessor.IsServerSide = true;
      this._senderInfo = senderInfo;
      this._configProviderId = configurationProviderId;
      this._initParameters = initializationParameters;
      this._cryptoHelper = (PSRemotingCryptoHelperServer) transportManager.CryptoHelper;
      this._cryptoHelper.Session = (RemoteSession) this;
      this._context = new ServerRemoteSessionContext();
      this._sessionDSHandler = (ServerRemoteSessionDataStructureHandler) new ServerRemoteSessionDSHandlerlImpl(this, transportManager);
      this.BaseSessionDataStructureHandler = (BaseSessionDataStructureHandler) this._sessionDSHandler;
      this._sessionDSHandler.CreateRunspacePoolReceived += new EventHandler<RemoteDataEventArgs>(this.HandleCreateRunspacePool);
      this._sessionDSHandler.NegotiationReceived += new EventHandler<RemoteSessionNegotiationEventArgs>(this.HandleNegotiationReceived);
      this._sessionDSHandler.SessionClosing += new EventHandler<EventArgs>(this.HandleSessionDSHandlerClosing);
      this._sessionDSHandler.PublicKeyReceived += new EventHandler<RemoteDataEventArgs<string>>(this.HandlePublicKeyReceived);
      transportManager.Closing += new EventHandler(this.HandleResourceClosing);
      transportManager.ReceivedDataCollection.MaximumReceivedObjectSize = new int?(10485760);
      transportManager.ReceivedDataCollection.MaximumReceivedDataSize = new int?();
    }

    internal static ServerRemoteSession CreateServerRemoteSession(
      PSSenderInfo senderInfo,
      string configurationProviderId,
      string initializationParameters,
      AbstractServerSessionTransportManager transportManager)
    {
      ServerRemoteSession._trace.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Finding InitialSessionState provider for id : {0}", (object) configurationProviderId), new object[0]);
      if (string.IsNullOrEmpty(configurationProviderId))
        throw ServerRemoteSession._trace.NewInvalidOperationException("remotingerroridstrings", "NonExistentInitialSessionStateProvider", (object) configurationProviderId);
      ServerRemoteSession serverRemoteSession = new ServerRemoteSession(senderInfo, configurationProviderId, initializationParameters, transportManager);
      RemoteSessionStateMachineEventArgs fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Start);
      serverRemoteSession._sessionDSHandler.StateMachine.RaiseEvent(fsmEventArg);
      return serverRemoteSession;
    }

    internal static ServerRemoteSession CreateServerRemoteSession(
      PSSenderInfo senderInfo,
      string initializationScriptForOutOfProcessRunspace,
      AbstractServerSessionTransportManager transportManager)
    {
      ServerRemoteSession serverRemoteSession = ServerRemoteSession.CreateServerRemoteSession(senderInfo, "Microsoft.PowerShell", "", transportManager);
      serverRemoteSession._initScriptForOutOfProcRS = initializationScriptForOutOfProcessRunspace;
      return serverRemoteSession;
    }

    internal override RemotingDestination MySelf => RemotingDestination.Server;

    internal void DispatchInputQueueData(object sender, RemoteDataEventArgs dataEventArg)
    {
      using (ServerRemoteSession._trace.TraceMethod())
      {
        RemoteDataObject<PSObject> remoteDataObject = dataEventArg != null ? dataEventArg.ReceivedData : throw ServerRemoteSession._trace.NewArgumentNullException(nameof (dataEventArg));
        RemotingDestination remotingDestination = remoteDataObject != null ? remoteDataObject.Destination : throw ServerRemoteSession._trace.NewArgumentException(nameof (dataEventArg));
        if ((remotingDestination & this.MySelf) != this.MySelf)
          throw new PSRemotingDataStructureException(PSRemotingErrorId.RemotingDestinationNotForMe, new object[2]
          {
            (object) this.MySelf,
            (object) remotingDestination
          });
        RemotingTargetInterface targetInterface = remoteDataObject.TargetInterface;
        RemotingDataType dataType = remoteDataObject.DataType;
        switch (targetInterface)
        {
          case RemotingTargetInterface.Session:
            switch (dataType)
            {
              case RemotingDataType.SessionCapability:
                this._sessionDSHandler.RaiseDataReceivedEvent(dataEventArg);
                return;
              case RemotingDataType.CloseSession:
                this._sessionDSHandler.RaiseDataReceivedEvent(dataEventArg);
                return;
              case RemotingDataType.CreateRunspacePool:
                RemoteSessionStateMachineEventArgs fsmEventArg1 = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.MessageReceived);
                if (this.SessionDataStructureHandler.StateMachine.CanByPassRaiseEvent(fsmEventArg1))
                {
                  fsmEventArg1.RemoteData = remoteDataObject;
                  this.SessionDataStructureHandler.StateMachine.DoMessageReceived((object) this, fsmEventArg1);
                  return;
                }
                this.SessionDataStructureHandler.StateMachine.RaiseEvent(fsmEventArg1);
                return;
              case RemotingDataType.PublicKey:
                this._sessionDSHandler.RaiseDataReceivedEvent(dataEventArg);
                return;
              default:
                return;
            }
          case RemotingTargetInterface.RunspacePool:
          case RemotingTargetInterface.PowerShell:
            RemoteSessionStateMachineEventArgs fsmEventArg2 = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.MessageReceived);
            if (this.SessionDataStructureHandler.StateMachine.CanByPassRaiseEvent(fsmEventArg2))
            {
              fsmEventArg2.RemoteData = remoteDataObject;
              this.SessionDataStructureHandler.StateMachine.DoMessageReceived((object) this, fsmEventArg2);
              break;
            }
            this.SessionDataStructureHandler.StateMachine.RaiseEvent(fsmEventArg2);
            break;
        }
      }
    }

    private void HandlePublicKeyReceived(object sender, RemoteDataEventArgs<string> eventArgs)
    {
      if (this.SessionDataStructureHandler.StateMachine.State != RemoteSessionState.Established && this.SessionDataStructureHandler.StateMachine.State != RemoteSessionState.EstablishedAndKeyRequested)
        return;
      if (!this._cryptoHelper.ImportRemotePublicKey(eventArgs.Data))
        this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceiveFailed));
      this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceived));
    }

    internal override void StartKeyExchange()
    {
      if (this.SessionDataStructureHandler.StateMachine.State != RemoteSessionState.Established)
        return;
      this.SessionDataStructureHandler.SendRequestForPublicKey();
      this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyRequested));
    }

    internal override void CompleteKeyExchange() => this._cryptoHelper.CompleteKeyExchange();

    internal void SendEncryptedSessionKey()
    {
      string encryptedSessionKey = (string) null;
      if (!this._cryptoHelper.ExportEncryptedSessionKey(out encryptedSessionKey))
        this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeySendFailed));
      this.SessionDataStructureHandler.SendEncryptedSessionKey(encryptedSessionKey);
      this.CompleteKeyExchange();
      this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeySent));
    }

    internal ServerRemoteSessionContext Context
    {
      get
      {
        using (ServerRemoteSession._trace.TraceProperty())
          return this._context;
      }
    }

    internal ServerRemoteSessionDataStructureHandler SessionDataStructureHandler
    {
      get
      {
        using (ServerRemoteSession._trace.TraceProperty())
          return this._sessionDSHandler;
      }
    }

    internal void Close(RemoteSessionStateMachineEventArgs reasonForClose)
    {
      if (this.Closed == null)
        return;
      this.Closed((object) this, reasonForClose);
    }

    private void HandleCreateRunspacePool(object sender, RemoteDataEventArgs createRunspaceEventArg)
    {
      using (ServerRemoteSession._trace.TraceMethod())
      {
        RemoteDataObject<PSObject> remoteDataObject = createRunspaceEventArg != null ? createRunspaceEventArg.ReceivedData : throw ServerRemoteSession._trace.NewArgumentNullException(nameof (createRunspaceEventArg));
        if (this._context != null)
          this._senderInfo.ClientTimeZone = this._context.ClientCapability.TimeZone;
        this._senderInfo.ApplicationArguments = RemotingDecoder.GetApplicationArguments(remoteDataObject.Data);
        ConfigurationDataFromXML configData = PSSessionConfiguration.LoadEndPointConfiguration(this._configProviderId, this._initParameters);
        configData.InitializationScriptForOutOfProcessRunspace = this._initScriptForOutOfProcRS;
        this.maxRecvdObjectSize = configData.MaxReceivedObjectSizeMB;
        this.maxRecvdDataSizeCommand = configData.MaxReceivedCommandSizeMB;
        this._sessionConfigProvider = configData.CreateEndPointConfigurationInstance();
        PSPrimitiveDictionary applicationPrivateData = this._sessionConfigProvider.GetApplicationPrivateData(this._senderInfo);
        InitialSessionState initialSessionState = this._sessionConfigProvider.GetInitialSessionState(this._senderInfo);
        if (initialSessionState == null)
          throw ServerRemoteSession._trace.NewInvalidOperationException("RemotingErrorIdStrings", "InitialSessionStateNull", (object) this._configProviderId);
        initialSessionState.ThrowOnRunspaceOpenError = true;
        initialSessionState.Variables.Add(new SessionStateVariableEntry("PSSenderInfo", (object) this._senderInfo, PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.PSSenderInfoDescription), ScopedItemOptions.ReadOnly));
        if (!string.IsNullOrEmpty(configData.EndPointConfigurationTypeName))
        {
          this.maxRecvdObjectSize = this._sessionConfigProvider.GetMaximumReceivedObjectSize(this._senderInfo);
          this.maxRecvdDataSizeCommand = this._sessionConfigProvider.GetMaximumReceivedDataSizePerCommand(this._senderInfo);
        }
        this._sessionDSHandler.TransportManager.ReceivedDataCollection.MaximumReceivedObjectSize = this.maxRecvdObjectSize;
        Guid runspacePoolId = remoteDataObject.RunspacePoolId;
        int minRunspaces = RemotingDecoder.GetMinRunspaces(remoteDataObject.Data);
        int maxRunspaces = RemotingDecoder.GetMaxRunspaces(remoteDataObject.Data);
        PSThreadOptions threadOptions = RemotingDecoder.GetThreadOptions(remoteDataObject.Data);
        ApartmentState apartmentState = RemotingDecoder.GetApartmentState((object) remoteDataObject.Data);
        HostInfo hostInfo = RemotingDecoder.GetHostInfo(remoteDataObject.Data);
        if (this._runspacePoolDriver != null)
          throw new PSRemotingDataStructureException(PSRemotingErrorId.RunspaceAlreadyExists, new object[1]
          {
            (object) this._runspacePoolDriver.InstanceId
          });
        bool isAdministrator = this._senderInfo.UserInfo.IsInRole(WindowsBuiltInRole.Administrator);
        this._runspacePoolDriver = new ServerRunspacePoolDriver(runspacePoolId, minRunspaces, maxRunspaces, threadOptions, apartmentState, hostInfo, initialSessionState, applicationPrivateData, configData, this.SessionDataStructureHandler.TransportManager, isAdministrator, this._context.ServerCapability);
        this._runspacePoolDriver.Closed += new EventHandler<EventArgs>(this.HandleResourceClosing);
        this._runspacePoolDriver.Start();
      }
    }

    private void HandleNegotiationReceived(
      object sender,
      RemoteSessionNegotiationEventArgs negotiationEventArg)
    {
      using (ServerRemoteSession._trace.TraceMethod())
      {
        if (negotiationEventArg == null)
          throw ServerRemoteSession._trace.NewArgumentNullException(nameof (negotiationEventArg));
        try
        {
          this._context.ClientCapability = negotiationEventArg.RemoteSessionCapability;
          this.RunServerNegotiationAlgorithm(negotiationEventArg.RemoteSessionCapability);
          this._sessionDSHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSending));
          this._sessionDSHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationCompleted));
        }
        catch (PSRemotingDataStructureException ex)
        {
          this._sessionDSHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSending));
          this._sessionDSHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationFailed, (Exception) ex));
        }
      }
    }

    private void HandleSessionDSHandlerClosing(object sender, EventArgs eventArgs)
    {
      if (this._runspacePoolDriver != null)
        this._runspacePoolDriver.Close();
      if (this._sessionConfigProvider == null)
        return;
      this._sessionConfigProvider.Dispose();
      this._sessionConfigProvider = (PSSessionConfiguration) null;
    }

    private void HandleResourceClosing(object sender, EventArgs args) => this._sessionDSHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close)
    {
      RemoteData = (RemoteDataObject<PSObject>) null
    });

    private bool RunServerNegotiationAlgorithm(RemoteSessionCapability clientCapability)
    {
      Version protocolVersion = clientCapability.ProtocolVersion;
      Version version = this._context.ServerCapability.ProtocolVersion;
      if (protocolVersion == RemotingConstants.ProtocolVersionWin7RC && version == RemotingConstants.ProtocolVersionWin7RTM)
      {
        version = RemotingConstants.ProtocolVersionWin7RC;
        this._context.ServerCapability.ProtocolVersion = version;
      }
      if (protocolVersion.Major != version.Major || protocolVersion.Minor < version.Minor)
        throw new PSRemotingDataStructureException(PSRemotingErrorId.ServerNegotiationFailed, new object[4]
        {
          (object) "protocolversion",
          (object) protocolVersion,
          (object) PSVersionInfo.BuildVersion,
          (object) RemotingConstants.ProtocolVersion
        });
      Version psVersion1 = clientCapability.PSVersion;
      Version psVersion2 = this._context.ServerCapability.PSVersion;
      if (psVersion1.Major != psVersion2.Major || psVersion1.Minor < psVersion2.Minor)
        throw new PSRemotingDataStructureException(PSRemotingErrorId.ServerNegotiationFailed, new object[4]
        {
          (object) "PSVersion",
          (object) psVersion1,
          (object) PSVersionInfo.BuildVersion,
          (object) RemotingConstants.ProtocolVersion
        });
      Version serializationVersion1 = clientCapability.SerializationVersion;
      Version serializationVersion2 = this._context.ServerCapability.SerializationVersion;
      if (serializationVersion1.Major != serializationVersion2.Major || serializationVersion1.Minor < serializationVersion2.Minor)
        throw new PSRemotingDataStructureException(PSRemotingErrorId.ServerNegotiationFailed, new object[4]
        {
          (object) "SerializationVersion",
          (object) serializationVersion1,
          (object) PSVersionInfo.BuildVersion,
          (object) RemotingConstants.ProtocolVersion
        });
      return true;
    }

    internal ServerRunspacePoolDriver GetRunspacePoolDriver(
      Guid clientRunspacePoolId)
    {
      using (ServerRemoteSession._trace.TraceMethod())
        return this._runspacePoolDriver == null || !(this._runspacePoolDriver.InstanceId == clientRunspacePoolId) ? (ServerRunspacePoolDriver) null : this._runspacePoolDriver;
    }

    internal void ApplyQuotaOnCommandTransportManager(
      AbstractServerTransportManager cmdTransportManager)
    {
      cmdTransportManager.ReceivedDataCollection.MaximumReceivedDataSize = this.maxRecvdDataSizeCommand;
      cmdTransportManager.ReceivedDataCollection.MaximumReceivedObjectSize = this.maxRecvdObjectSize;
    }
  }
}
