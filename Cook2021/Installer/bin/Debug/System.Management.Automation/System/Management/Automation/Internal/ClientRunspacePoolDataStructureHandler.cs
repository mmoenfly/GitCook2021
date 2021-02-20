// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.ClientRunspacePoolDataStructureHandler
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Host;
using System.Management.Automation.Remoting;
using System.Management.Automation.Remoting.Client;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Runspaces.Internal;

namespace System.Management.Automation.Internal
{
  internal class ClientRunspacePoolDataStructureHandler : IDisposable
  {
    [TraceSource("CRPP", "ClientRunspacePoolDataStructureHandler")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CRPP", nameof (ClientRunspacePoolDataStructureHandler));
    private Guid clientRunspacePoolId;
    private ClientRemoteSession remoteSession;
    private object syncObject = new object();
    private bool createRunspaceCalled;
    private Exception closingReason;
    private int minRunspaces;
    private int maxRunspaces;
    private PSHost host;
    private PSPrimitiveDictionary applicationArguments;
    private Dictionary<Guid, ClientPowerShellDataStructureHandler> associatedPowerShellDSHandlers = new Dictionary<Guid, ClientPowerShellDataStructureHandler>();
    private object associationSyncObject = new object();
    private BaseClientSessionTransportManager transportManager;

    internal ClientRunspacePoolDataStructureHandler(
      RemoteRunspacePoolInternal clientRunspacePool,
      TypeTable typeTable)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceConstructor((object) this))
      {
        this.clientRunspacePoolId = clientRunspacePool.InstanceId;
        this.minRunspaces = clientRunspacePool.GetMinRunspaces();
        this.maxRunspaces = clientRunspacePool.GetMaxRunspaces();
        this.host = clientRunspacePool.Host;
        this.applicationArguments = clientRunspacePool.ApplicationArguments;
        this.remoteSession = (ClientRemoteSession) this.CreateClientRemoteSession(clientRunspacePool);
        this.transportManager = this.remoteSession.SessionDataStructureHandler.TransportManager;
        this.transportManager.TypeTable = typeTable;
        this.remoteSession.StateChanged += new EventHandler<RemoteSessionStateEventArgs>(this.HandleClientRemoteSessionStateChanged);
      }
    }

    internal void CreateRunspacePoolAndOpenAsync()
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.remoteSession.ConnectAsync();
    }

    internal void CloseRunspacePoolAsync()
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.remoteSession.CloseAsync();
    }

    internal void ProcessReceivedData(RemoteDataObject<PSObject> receivedData)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
      {
        if (receivedData.RunspacePoolId != this.clientRunspacePoolId)
          throw new PSRemotingDataStructureException(PSRemotingErrorId.RunspaceIdsDoNotMatch, new object[2]
          {
            (object) receivedData.RunspacePoolId,
            (object) this.clientRunspacePoolId
          });
        switch (receivedData.DataType)
        {
          case RemotingDataType.RunspacePoolOperationResponse:
            this.SetMaxMinRunspacesResponseRecieved((object) this, new RemoteDataEventArgs<PSObject>((object) receivedData.Data));
            break;
          case RemotingDataType.RunspacePoolStateInfo:
            RunspacePoolStateInfo runspacePoolStateInfo = RemotingDecoder.GetRunspacePoolStateInfo(receivedData.Data);
            this.StateInfoReceived((object) this, new RemoteDataEventArgs<RunspacePoolStateInfo>((object) runspacePoolStateInfo));
            this.NotifyAssociatedPowerShells(runspacePoolStateInfo);
            break;
          case RemotingDataType.PSEventArgs:
            this.PSEventArgsReceived((object) this, new RemoteDataEventArgs<PSEventArgs>((object) RemotingDecoder.GetPSEventArgs(receivedData.Data)));
            break;
          case RemotingDataType.ApplicationPrivateData:
            this.ApplicationPrivateDataReceived((object) this, new RemoteDataEventArgs<PSPrimitiveDictionary>((object) RemotingDecoder.GetApplicationPrivateData(receivedData.Data)));
            break;
          case RemotingDataType.RemoteHostCallUsingRunspaceHost:
            this.RemoteHostCallReceived((object) this, new RemoteDataEventArgs<RemoteHostCall>((object) RemoteHostCall.Decode(receivedData.Data)));
            break;
        }
      }
    }

    internal ClientPowerShellDataStructureHandler CreatePowerShellDataStructureHandler(
      ClientRemotePowerShell shell)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
        return new ClientPowerShellDataStructureHandler(this.remoteSession.SessionDataStructureHandler.CreateClientCommandTransportManager(shell, shell.NoInput), this.clientRunspacePoolId, shell.InstanceId);
    }

    internal void CreatePowerShellOnServerAndInvoke(ClientRemotePowerShell shell)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
      {
        lock (this.associationSyncObject)
          this.associatedPowerShellDSHandlers.Add(shell.InstanceId, shell.DataStructureHandler);
        shell.DataStructureHandler.RemoveAssociation += new EventHandler(this.HandleRemoveAssociation);
        shell.DataStructureHandler.Start(this.remoteSession.SessionDataStructureHandler.StateMachine);
      }
    }

    internal void DispatchMessageToPowerShell(RemoteDataObject<PSObject> rcvdData)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.GetAssociatedPowerShellDataStructureHandler(rcvdData.PowerShellId)?.ProcessReceivedData(rcvdData);
    }

    internal void SendHostResponseToServer(RemoteHostResponse hostResponse)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.SendDataAsync(hostResponse.Encode(), DataPriorityType.PromptResponse);
    }

    internal void SendSetMaxRunspacesToServer(int maxRunspaces, long callId)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.SendDataAsync(RemotingEncoder.GenerateSetMaxRunspaces(this.clientRunspacePoolId, maxRunspaces, callId));
    }

    internal void SendSetMinRunspacesToServer(int minRunspaces, long callId)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.SendDataAsync(RemotingEncoder.GenerateSetMinRunspaces(this.clientRunspacePoolId, minRunspaces, callId));
    }

    internal void SendGetAvailableRunspacesToServer(long callId) => this.SendDataAsync(RemotingEncoder.GenerateGetAvailableRunspaces(this.clientRunspacePoolId, callId));

    internal event EventHandler<RemoteDataEventArgs<RemoteHostCall>> RemoteHostCallReceived;

    internal event EventHandler<RemoteDataEventArgs<RunspacePoolStateInfo>> StateInfoReceived;

    internal event EventHandler<RemoteDataEventArgs<PSPrimitiveDictionary>> ApplicationPrivateDataReceived;

    internal event EventHandler<RemoteDataEventArgs<PSEventArgs>> PSEventArgsReceived;

    internal event EventHandler<RemoteDataEventArgs<Exception>> SessionClosed;

    internal event EventHandler<RemoteDataEventArgs<Exception>> SessionClosing;

    internal event EventHandler<RemoteDataEventArgs<PSObject>> SetMaxMinRunspacesResponseRecieved;

    internal event EventHandler<RemoteDataEventArgs<Uri>> URIRedirectionReported;

    private void SendDataAsync(RemoteDataObject data)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.transportManager.DataToBeSentCollection.Add<object>((RemoteDataObject<object>) data);
    }

    internal void SendDataAsync<T>(RemoteDataObject<T> data, DataPriorityType priority)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.transportManager.DataToBeSentCollection.Add<T>(data, priority);
    }

    internal void SendDataAsync(PSObject data, DataPriorityType priority)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.transportManager.DataToBeSentCollection.Add<PSObject>(RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.Server, RemotingDataType.InvalidDataType, this.clientRunspacePoolId, Guid.Empty, data));
    }

    private ClientRemoteSessionImpl CreateClientRemoteSession(
      RemoteRunspacePoolInternal rsPoolInternal)
    {
      ClientRemoteSession.URIDirectionReported uriRedirectionHandler = new ClientRemoteSession.URIDirectionReported(this.HandleURIDirectionReported);
      return new ClientRemoteSessionImpl(rsPoolInternal, uriRedirectionHandler);
    }

    private void HandleClientRemoteSessionStateChanged(object sender, RemoteSessionStateEventArgs e)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
      {
        if (e.SessionStateInfo.State == RemoteSessionState.NegotiationSending)
        {
          if (this.createRunspaceCalled)
            return;
          lock (this.syncObject)
          {
            if (this.createRunspaceCalled)
              return;
            this.createRunspaceCalled = true;
          }
          this.SendDataAsync(RemotingEncoder.GenerateCreateRunspacePool(this.clientRunspacePoolId, this.minRunspaces, this.maxRunspaces, this.remoteSession.RemoteRunspacePoolInternal, this.host, PSPrimitiveDictionary.CloneAndAddPSVersionTable(this.applicationArguments)));
        }
        else if (e.SessionStateInfo.State == RemoteSessionState.ClosingConnection)
        {
          Exception exception = this.closingReason;
          if (exception == null)
          {
            exception = e.SessionStateInfo.Reason;
            this.closingReason = exception;
          }
          lock (this.associationSyncObject)
          {
            foreach (ClientPowerShellDataStructureHandler structureHandler in this.associatedPowerShellDSHandlers.Values)
              structureHandler.CloseConnection();
          }
          if (this.SessionClosing == null)
            return;
          this.SessionClosing((object) this, new RemoteDataEventArgs<Exception>((object) exception));
        }
        else if (e.SessionStateInfo.State == RemoteSessionState.Closed)
        {
          Exception reason = this.closingReason;
          if (reason == null)
          {
            reason = e.SessionStateInfo.Reason;
            this.closingReason = reason;
          }
          if (reason != null)
            this.NotifyAssociatedPowerShells(new RunspacePoolStateInfo(RunspacePoolState.Broken, reason));
          else
            this.NotifyAssociatedPowerShells(new RunspacePoolStateInfo(RunspacePoolState.Closed, reason));
          if (this.SessionClosed == null)
            return;
          this.SessionClosed((object) this, new RemoteDataEventArgs<Exception>((object) reason));
        }
        else if (e.SessionStateInfo.State == RemoteSessionState.Connected)
        {
          using (IETWTracer etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Runspace))
            etwTracer.ReplaceActivityIdForCurrentThread(this.clientRunspacePoolId, PSEventId.OperationalTransferEventRunspacePool, PSEventId.AnalyticTransferEventRunspacePool, PSKeyword.Runspace, PSTask.CreateRunspace);
        }
        else
        {
          if (e.SessionStateInfo.Reason == null)
            return;
          this.closingReason = e.SessionStateInfo.Reason;
        }
      }
    }

    private void HandleURIDirectionReported(Uri newURI)
    {
      if (this.URIRedirectionReported == null)
        return;
      this.URIRedirectionReported((object) this, new RemoteDataEventArgs<Uri>((object) newURI));
    }

    private void NotifyAssociatedPowerShells(RunspacePoolStateInfo stateInfo)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
      {
        Collection<ClientPowerShellDataStructureHandler> collection = new Collection<ClientPowerShellDataStructureHandler>();
        if (stateInfo.State == RunspacePoolState.Broken || stateInfo.State == RunspacePoolState.Closed)
        {
          lock (this.associationSyncObject)
          {
            foreach (ClientPowerShellDataStructureHandler structureHandler in this.associatedPowerShellDSHandlers.Values)
              collection.Add(structureHandler);
            this.associatedPowerShellDSHandlers.Clear();
          }
        }
        if (stateInfo.State == RunspacePoolState.Broken)
        {
          foreach (ClientPowerShellDataStructureHandler structureHandler in collection)
            structureHandler.SetStateToFailed(stateInfo.Reason);
        }
        else
        {
          if (stateInfo.State != RunspacePoolState.Closed)
            return;
          foreach (ClientPowerShellDataStructureHandler structureHandler in collection)
            structureHandler.SetStateToStopped(stateInfo.Reason);
        }
      }
    }

    private ClientPowerShellDataStructureHandler GetAssociatedPowerShellDataStructureHandler(
      Guid clientPowerShellId)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
      {
        ClientPowerShellDataStructureHandler structureHandler = (ClientPowerShellDataStructureHandler) null;
        lock (this.associationSyncObject)
        {
          if (!this.associatedPowerShellDSHandlers.TryGetValue(clientPowerShellId, out structureHandler))
            structureHandler = (ClientPowerShellDataStructureHandler) null;
        }
        return structureHandler;
      }
    }

    private void HandleRemoveAssociation(object sender, EventArgs e)
    {
      using (ClientRunspacePoolDataStructureHandler.tracer.TraceMethod())
      {
        ClientPowerShellDataStructureHandler structureHandler = sender as ClientPowerShellDataStructureHandler;
        lock (this.associationSyncObject)
        {
          this.associatedPowerShellDSHandlers.Remove(structureHandler.PowerShellId);
          this.transportManager.RemoveCommandTransportManager(structureHandler.PowerShellId);
        }
      }
    }

    internal ClientRemoteSession RemoteSession => this.remoteSession;

    internal BaseClientSessionTransportManager TransportManager => this.remoteSession != null ? this.remoteSession.SessionDataStructureHandler.TransportManager : (BaseClientSessionTransportManager) null;

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public void Dispose(bool disposing)
    {
      if (!disposing || this.remoteSession == null)
        return;
      ((ClientRemoteSessionImpl) this.remoteSession).Dispose();
      this.remoteSession = (ClientRemoteSession) null;
    }
  }
}
