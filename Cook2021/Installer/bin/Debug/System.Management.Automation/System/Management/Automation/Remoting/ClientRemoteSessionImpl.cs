// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ClientRemoteSessionImpl
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Runspaces.Internal;
using System.Threading;

namespace System.Management.Automation.Remoting
{
  internal class ClientRemoteSessionImpl : ClientRemoteSession, IDisposable
  {
    [TraceSource("CRSessionImpl", "ClientRemoteSessionImpl")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("CRSessionImpl", nameof (ClientRemoteSessionImpl));
    private PSRemotingCryptoHelperClient _cryptoHelper;
    private ManualResetEvent _waitHandleForConfigurationReceived;
    private RemotingDestination _mySelf;

    internal ClientRemoteSessionImpl(
      RemoteRunspacePoolInternal rsPool,
      ClientRemoteSession.URIDirectionReported uriRedirectionHandler)
    {
      using (ClientRemoteSessionImpl._trace.TraceConstructor((object) this))
      {
        this.RemoteRunspacePoolInternal = rsPool;
        this.Context.RemoteAddress = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<Uri>(rsPool.ConnectionInfo, "ConnectionUri", (Uri) null);
        this._cryptoHelper = new PSRemotingCryptoHelperClient();
        this._cryptoHelper.Session = (RemoteSession) this;
        this.Context.ClientCapability = RemoteSessionCapability.CreateClientCapability();
        this.Context.UserCredential = rsPool.ConnectionInfo.Credential;
        this.Context.ShellName = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<string>(rsPool.ConnectionInfo, "ShellUri", string.Empty);
        this._mySelf = RemotingDestination.Client;
        this.SessionDataStructureHandler = (ClientRemoteSessionDataStructureHandler) new ClientRemoteSessionDSHandlerImpl((ClientRemoteSession) this, (PSRemotingCryptoHelper) this._cryptoHelper, rsPool.ConnectionInfo, uriRedirectionHandler);
        this.BaseSessionDataStructureHandler = (BaseSessionDataStructureHandler) this.SessionDataStructureHandler;
        this._waitHandleForConfigurationReceived = new ManualResetEvent(false);
        this.SessionDataStructureHandler.NegotiationReceived += new EventHandler<RemoteSessionNegotiationEventArgs>(this.HandleNegotiationReceived);
        this.SessionDataStructureHandler.ConnectionStateChanged += new EventHandler<RemoteSessionStateEventArgs>(this.HandleConnectionStateChanged);
        this.SessionDataStructureHandler.EncryptedSessionKeyReceived += new EventHandler<RemoteDataEventArgs<string>>(this.HandleEncryptedSessionKeyReceived);
        this.SessionDataStructureHandler.PublicKeyRequestReceived += new EventHandler<RemoteDataEventArgs<string>>(this.HandlePublicKeyRequestReceived);
      }
    }

    public override void ConnectAsync()
    {
      using (ClientRemoteSessionImpl._trace.TraceMethod())
        this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Start));
    }

    public override void CloseAsync()
    {
      using (ClientRemoteSessionImpl._trace.TraceMethod())
        this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close));
    }

    public override event EventHandler<RemoteSessionStateEventArgs> StateChanged;

    private void HandleConnectionStateChanged(object sender, RemoteSessionStateEventArgs arg)
    {
      using (ClientRemoteSessionImpl._trace.TraceEventHandlers())
      {
        if (arg == null)
          throw ClientRemoteSessionImpl._trace.NewArgumentNullException(nameof (arg));
        if (arg.SessionStateInfo.State == RemoteSessionState.EstablishedAndKeyReceived)
          this.StartKeyExchange();
        if (arg.SessionStateInfo.State == RemoteSessionState.ClosingConnection)
          this.CompleteKeyExchange();
        if (this.StateChanged == null)
          return;
        this.StateChanged((object) this, arg);
      }
    }

    internal override void StartKeyExchange()
    {
      if (this.SessionDataStructureHandler.StateMachine.State != RemoteSessionState.Established && this.SessionDataStructureHandler.StateMachine.State != RemoteSessionState.EstablishedAndKeyRequested)
        return;
      string publicKeyAsString = (string) null;
      Exception reason = (Exception) null;
      bool flag;
      try
      {
        flag = this._cryptoHelper.ExportLocalPublicKey(out publicKeyAsString);
      }
      catch (PSCryptoException ex)
      {
        flag = false;
        reason = (Exception) ex;
      }
      if (!flag)
      {
        this.CompleteKeyExchange();
        this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeySendFailed, reason));
      }
      this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeySent));
      this.SessionDataStructureHandler.SendPublicKeyAsync(publicKeyAsString);
    }

    internal override void CompleteKeyExchange() => this._cryptoHelper.CompleteKeyExchange();

    private void HandleEncryptedSessionKeyReceived(
      object sender,
      RemoteDataEventArgs<string> eventArgs)
    {
      if (this.SessionDataStructureHandler.StateMachine.State != RemoteSessionState.EstablishedAndKeySent)
        return;
      if (!this._cryptoHelper.ImportEncryptedSessionKey(eventArgs.Data))
        this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceiveFailed));
      this.CompleteKeyExchange();
      this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceived));
    }

    private void HandlePublicKeyRequestReceived(
      object sender,
      RemoteDataEventArgs<string> eventArgs)
    {
      if (this.SessionDataStructureHandler.StateMachine.State != RemoteSessionState.Established)
        return;
      this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyRequested));
      this.StartKeyExchange();
    }

    private void HandleNegotiationReceived(object sender, RemoteSessionNegotiationEventArgs arg)
    {
      using (ClientRemoteSessionImpl._trace.TraceEventHandlers())
      {
        if (arg == null)
          throw ClientRemoteSessionImpl._trace.NewArgumentNullException(nameof (arg));
        this.Context.ServerCapability = arg.RemoteSessionCapability != null ? arg.RemoteSessionCapability : throw ClientRemoteSessionImpl._trace.NewArgumentException(nameof (arg));
        try
        {
          this.RunClientNegotiationAlgorithm(this.Context.ServerCapability);
          this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationCompleted));
        }
        catch (PSRemotingDataStructureException ex)
        {
          this.SessionDataStructureHandler.StateMachine.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationFailed, (Exception) ex));
        }
      }
    }

    private bool RunClientNegotiationAlgorithm(
      RemoteSessionCapability serverRemoteSessionCapability)
    {
      using (ClientRemoteSessionImpl._trace.TraceMethod())
      {
        Version protocolVersion1 = serverRemoteSessionCapability.ProtocolVersion;
        Version protocolVersion2 = this.Context.ClientCapability.ProtocolVersion;
        if (!protocolVersion2.Equals(protocolVersion1) && (!(protocolVersion2 == RemotingConstants.ProtocolVersionWin7RTM) || !(protocolVersion1 == RemotingConstants.ProtocolVersionWin7RC)))
          throw new PSRemotingDataStructureException(PSRemotingErrorId.ClientNegotiationFailed, new object[4]
          {
            (object) "protocolversion",
            (object) protocolVersion1,
            (object) PSVersionInfo.BuildVersion,
            (object) RemotingConstants.ProtocolVersion
          });
        Version psVersion = serverRemoteSessionCapability.PSVersion;
        if (!this.Context.ClientCapability.PSVersion.Equals(psVersion))
          throw new PSRemotingDataStructureException(PSRemotingErrorId.ClientNegotiationFailed, new object[4]
          {
            (object) "PSVersion",
            (object) psVersion.ToString(),
            (object) PSVersionInfo.BuildVersion,
            (object) RemotingConstants.ProtocolVersion
          });
        Version serializationVersion = serverRemoteSessionCapability.SerializationVersion;
        if (!this.Context.ClientCapability.SerializationVersion.Equals(serializationVersion))
          throw new PSRemotingDataStructureException(PSRemotingErrorId.ClientNegotiationFailed, new object[4]
          {
            (object) "SerializationVersion",
            (object) serializationVersion.ToString(),
            (object) PSVersionInfo.BuildVersion,
            (object) RemotingConstants.ProtocolVersion
          });
        return true;
      }
    }

    internal override RemotingDestination MySelf
    {
      get
      {
        using (ClientRemoteSessionImpl._trace.TraceProperty())
          return this._mySelf;
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      if (this._waitHandleForConfigurationReceived != null)
      {
        this._waitHandleForConfigurationReceived.Close();
        this._waitHandleForConfigurationReceived = (ManualResetEvent) null;
      }
      ((ClientRemoteSessionDSHandlerImpl) this.SessionDataStructureHandler).Dispose();
      this.SessionDataStructureHandler = (ClientRemoteSessionDataStructureHandler) null;
      this._cryptoHelper.Dispose();
      this._cryptoHelper = (PSRemotingCryptoHelperClient) null;
    }
  }
}
