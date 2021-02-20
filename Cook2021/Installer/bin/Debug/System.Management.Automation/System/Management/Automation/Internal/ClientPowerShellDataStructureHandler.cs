// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.ClientPowerShellDataStructureHandler
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting;
using System.Management.Automation.Remoting.Client;

namespace System.Management.Automation.Internal
{
  internal class ClientPowerShellDataStructureHandler
  {
    [TraceSource("CRPPB", "ClientPowerShellDataStructureHandlerBase")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CRPPB", "ClientPowerShellDataStructureHandlerBase");
    protected Guid clientRunspacePoolId;
    protected Guid clientPowerShellId;
    private BaseClientCommandTransportManager transportManager;
    private object inputSyncObject = new object();

    internal event EventHandler RemoveAssociation;

    internal event EventHandler<RemoteDataEventArgs<PSInvocationStateInfo>> InvocationStateInfoReceived;

    internal event EventHandler<RemoteDataEventArgs<object>> OutputReceived;

    internal event EventHandler<RemoteDataEventArgs<ErrorRecord>> ErrorReceived;

    internal event EventHandler<RemoteDataEventArgs<InformationalMessage>> InformationalMessageReceived;

    internal event EventHandler<RemoteDataEventArgs<RemoteHostCall>> HostCallReceived;

    internal event EventHandler<RemoteDataEventArgs<Exception>> ClosedNotificationFromRunspacePool;

    internal event EventHandler<RemoteDataEventArgs<Exception>> BrokenNotificationFromRunspacePool;

    internal void Start(
      ClientRemoteSessionDSHandlerStateMachine stateMachine)
    {
      this.transportManager.WSManTransportErrorOccured += new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportError);
      this.transportManager.ConnectAsync();
    }

    internal void HandleTransportError(object sender, TransportErrorOccuredEventArgs e) => this.InvocationStateInfoReceived((object) this, new RemoteDataEventArgs<PSInvocationStateInfo>((object) new PSInvocationStateInfo(PSInvocationState.Failed, (Exception) e.Exception)));

    internal void SendStopPowerShellMessage()
    {
      using (ClientPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        this.transportManager.CryptoHelper.CompleteKeyExchange();
        this.transportManager.SendStopSignal();
      }
    }

    private void OnSignalCompleted(object sender, EventArgs e) => this.InvocationStateInfoReceived((object) this, new RemoteDataEventArgs<PSInvocationStateInfo>((object) new PSInvocationStateInfo(PSInvocationState.Stopped, (Exception) new PSRemotingDataStructureException(PSRemotingErrorId.PipelineStopped, new object[0]))));

    internal void SendHostResponseToServer(RemoteHostResponse hostResponse)
    {
      using (ClientPowerShellDataStructureHandler.tracer.TraceMethod())
        this.transportManager.DataToBeSentCollection.Add<PSObject>(RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.Server, RemotingDataType.RemotePowerShellHostResponseData, this.clientRunspacePoolId, this.clientPowerShellId, hostResponse.Encode()), DataPriorityType.PromptResponse);
    }

    internal void SendInput(ObjectStreamBase inputstream)
    {
      using (ClientPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        if (!inputstream.IsOpen && inputstream.Count == 0)
        {
          lock (this.inputSyncObject)
            this.SendDataAsync(RemotingEncoder.GeneratePowerShellInputEnd(this.clientRunspacePoolId, this.clientPowerShellId));
        }
        else
        {
          lock (this.inputSyncObject)
          {
            inputstream.DataReady += new EventHandler(this.HandleInputDataReady);
            this.WriteInput(inputstream);
          }
        }
      }
    }

    internal void ProcessReceivedData(RemoteDataObject<PSObject> receivedData)
    {
      using (ClientPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        if (receivedData.PowerShellId != this.clientPowerShellId)
          throw new PSRemotingDataStructureException(PSRemotingErrorId.PipelineIdsDoNotMatch, new object[2]
          {
            (object) receivedData.PowerShellId,
            (object) this.clientPowerShellId
          });
        switch (receivedData.DataType)
        {
          case RemotingDataType.PowerShellOutput:
            this.OutputReceived((object) this, new RemoteDataEventArgs<object>(RemotingDecoder.GetPowerShellOutput((object) receivedData.Data)));
            break;
          case RemotingDataType.PowerShellErrorRecord:
            this.ErrorReceived((object) this, new RemoteDataEventArgs<ErrorRecord>((object) RemotingDecoder.GetPowerShellError((object) receivedData.Data)));
            break;
          case RemotingDataType.PowerShellStateInfo:
            this.InvocationStateInfoReceived((object) this, new RemoteDataEventArgs<PSInvocationStateInfo>((object) RemotingDecoder.GetPowerShellStateInfo((object) receivedData.Data)));
            break;
          case RemotingDataType.PowerShellDebug:
            this.InformationalMessageReceived((object) this, new RemoteDataEventArgs<InformationalMessage>((object) new InformationalMessage((object) RemotingDecoder.GetPowerShellDebug((object) receivedData.Data), RemotingDataType.PowerShellDebug)));
            break;
          case RemotingDataType.PowerShellVerbose:
            this.InformationalMessageReceived((object) this, new RemoteDataEventArgs<InformationalMessage>((object) new InformationalMessage((object) RemotingDecoder.GetPowerShellVerbose((object) receivedData.Data), RemotingDataType.PowerShellVerbose)));
            break;
          case RemotingDataType.PowerShellWarning:
            this.InformationalMessageReceived((object) this, new RemoteDataEventArgs<InformationalMessage>((object) new InformationalMessage((object) RemotingDecoder.GetPowerShellWarning((object) receivedData.Data), RemotingDataType.PowerShellWarning)));
            break;
          case RemotingDataType.PowerShellProgress:
            this.InformationalMessageReceived((object) this, new RemoteDataEventArgs<InformationalMessage>((object) new InformationalMessage((object) RemotingDecoder.GetPowerShellProgress((object) receivedData.Data), RemotingDataType.PowerShellProgress)));
            break;
          case RemotingDataType.RemoteHostCallUsingPowerShellHost:
            this.HostCallReceived((object) this, new RemoteDataEventArgs<RemoteHostCall>((object) RemoteHostCall.Decode(receivedData.Data)));
            break;
        }
      }
    }

    internal void SetStateToFailed(Exception reason)
    {
      using (ClientPowerShellDataStructureHandler.tracer.TraceMethod())
        this.BrokenNotificationFromRunspacePool((object) this, new RemoteDataEventArgs<Exception>((object) reason));
    }

    internal void SetStateToStopped(Exception reason) => this.ClosedNotificationFromRunspacePool((object) this, new RemoteDataEventArgs<Exception>((object) reason));

    internal void CloseConnection()
    {
      this.transportManager.CloseCompleted += (EventHandler<EventArgs>) ((source, args) => this.transportManager.Dispose());
      this.transportManager.CloseAsync();
    }

    internal void RaiseRemoveAssociationEvent()
    {
      using (ClientPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        if (this.RemoveAssociation == null)
          return;
        this.RemoveAssociation((object) this, new EventArgs());
      }
    }

    internal ClientPowerShellDataStructureHandler(
      BaseClientCommandTransportManager transportManager,
      Guid clientRunspacePoolId,
      Guid clientPowerShellId)
    {
      using (ClientPowerShellDataStructureHandler.tracer.TraceConstructor((object) this))
      {
        this.transportManager = transportManager;
        this.clientRunspacePoolId = clientRunspacePoolId;
        this.clientPowerShellId = clientPowerShellId;
        transportManager.SignalCompleted += new EventHandler<EventArgs>(this.OnSignalCompleted);
      }
    }

    internal Guid PowerShellId
    {
      get
      {
        using (ClientPowerShellDataStructureHandler.tracer.TraceProperty())
          return this.clientPowerShellId;
      }
    }

    internal BaseClientCommandTransportManager TransportManager => this.transportManager;

    private void SendDataAsync(RemoteDataObject data)
    {
      using (ClientPowerShellDataStructureHandler.tracer.TraceMethod())
        this.transportManager.DataToBeSentCollection.Add<object>((RemoteDataObject<object>) data);
    }

    private void HandleInputDataReady(object sender, EventArgs e)
    {
      using (ClientPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        lock (this.inputSyncObject)
          this.WriteInput(sender as ObjectStreamBase);
      }
    }

    private void WriteInput(ObjectStreamBase inputstream)
    {
      using (ClientPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        foreach (object data in inputstream.ObjectReader.NonBlockingRead(int.MaxValue))
          this.SendDataAsync(RemotingEncoder.GeneratePowerShellInput(data, this.clientRunspacePoolId, this.clientPowerShellId));
        if (inputstream.IsOpen)
          return;
        foreach (object data in inputstream.ObjectReader.NonBlockingRead(int.MaxValue))
          this.SendDataAsync(RemotingEncoder.GeneratePowerShellInput(data, this.clientRunspacePoolId, this.clientPowerShellId));
        inputstream.DataReady -= new EventHandler(this.HandleInputDataReady);
        this.SendDataAsync(RemotingEncoder.GeneratePowerShellInputEnd(this.clientRunspacePoolId, this.clientPowerShellId));
      }
    }
  }
}
