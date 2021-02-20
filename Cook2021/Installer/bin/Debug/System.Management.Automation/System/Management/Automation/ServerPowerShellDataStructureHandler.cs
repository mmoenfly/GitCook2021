// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ServerPowerShellDataStructureHandler
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting;
using System.Management.Automation.Remoting.Server;

namespace System.Management.Automation
{
  internal class ServerPowerShellDataStructureHandler
  {
    [TraceSource("SRPP", "ServerPowerShellDataStructureHandler")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("SRPP", nameof (ServerPowerShellDataStructureHandler));
    private AbstractServerTransportManager transportManager;
    private Guid clientRunspacePoolId;
    private Guid clientPowerShellId;
    private RemoteStreamOptions streamSerializationOptions;

    internal ServerPowerShellDataStructureHandler(
      ServerPowerShellDriver driver,
      AbstractServerTransportManager transportManager)
    {
      using (ServerPowerShellDataStructureHandler.tracer.TraceConstructor((object) this))
      {
        this.clientPowerShellId = driver.InstanceId;
        this.clientRunspacePoolId = driver.RunspacePoolId;
        this.transportManager = transportManager;
        this.streamSerializationOptions = driver.RemoteStreamOptions;
        transportManager.Closing += new EventHandler(this.HandleTransportClosing);
      }
    }

    internal void Prepare()
    {
      if (!(this.clientPowerShellId != Guid.Empty))
        return;
      this.transportManager.Prepare();
    }

    internal void SendStateChangedInformationToClient(PSInvocationStateInfo stateInfo)
    {
      using (ServerPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        this.SendDataAsync(RemotingEncoder.GeneratePowerShellStateInfo(stateInfo, this.clientPowerShellId, this.clientRunspacePoolId));
        if (!(this.clientPowerShellId != Guid.Empty))
          return;
        this.transportManager.Closing -= new EventHandler(this.HandleTransportClosing);
        this.transportManager.Close((Exception) null);
      }
    }

    internal void SendOutputDataToClient(PSObject data)
    {
      using (ServerPowerShellDataStructureHandler.tracer.TraceMethod())
        this.SendDataAsync(RemotingEncoder.GeneratePowerShellOutput(data, this.clientPowerShellId, this.clientRunspacePoolId));
    }

    internal void SendErrorRecordToClient(ErrorRecord errorRecord)
    {
      using (ServerPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        errorRecord.SerializeExtendedInfo = (this.streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToErrorRecord) != (RemoteStreamOptions) 0;
        this.SendDataAsync(RemotingEncoder.GeneratePowerShellError((object) errorRecord, this.clientRunspacePoolId, this.clientPowerShellId));
      }
    }

    internal void SendWarningRecordToClient(WarningRecord record)
    {
      using (ServerPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        record.SerializeExtendedInfo = (this.streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToWarningRecord) != (RemoteStreamOptions) 0;
        this.SendDataAsync(RemotingEncoder.GeneratePowerShellInformational((object) record, this.clientRunspacePoolId, this.clientPowerShellId, RemotingDataType.PowerShellWarning));
      }
    }

    internal void SendDebugRecordToClient(DebugRecord record)
    {
      using (ServerPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        record.SerializeExtendedInfo = (this.streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToDebugRecord) != (RemoteStreamOptions) 0;
        this.SendDataAsync(RemotingEncoder.GeneratePowerShellInformational((object) record, this.clientRunspacePoolId, this.clientPowerShellId, RemotingDataType.PowerShellDebug));
      }
    }

    internal void SendVerboseRecordToClient(VerboseRecord record)
    {
      using (ServerPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        record.SerializeExtendedInfo = (this.streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToVerboseRecord) != (RemoteStreamOptions) 0;
        this.SendDataAsync(RemotingEncoder.GeneratePowerShellInformational((object) record, this.clientRunspacePoolId, this.clientPowerShellId, RemotingDataType.PowerShellVerbose));
      }
    }

    internal void SendProgressRecordToClient(ProgressRecord record)
    {
      using (ServerPowerShellDataStructureHandler.tracer.TraceMethod())
        this.SendDataAsync(RemotingEncoder.GeneratePowerShellInformational(record, this.clientRunspacePoolId, this.clientPowerShellId));
    }

    internal void ProcessReceivedData(RemoteDataObject<PSObject> receivedData)
    {
      using (ServerPowerShellDataStructureHandler.tracer.TraceMethod())
      {
        if (receivedData == null)
          throw ServerPowerShellDataStructureHandler.tracer.NewArgumentNullException(nameof (receivedData));
        switch (receivedData.DataType)
        {
          case RemotingDataType.PowerShellInput:
            this.InputReceived((object) this, new RemoteDataEventArgs<object>((object) receivedData.Data));
            break;
          case RemotingDataType.PowerShellInputEnd:
            this.InputEndReceived((object) this, new EventArgs());
            break;
          case RemotingDataType.StopPowerShell:
            this.StopPowerShellReceived((object) this, new EventArgs());
            break;
          case RemotingDataType.RemotePowerShellHostResponseData:
            this.HostResponseReceived((object) this, new RemoteDataEventArgs<RemoteHostResponse>((object) RemoteHostResponse.Decode(receivedData.Data)));
            break;
        }
      }
    }

    internal void RaiseRemoveAssociationEvent()
    {
      using (ServerPowerShellDataStructureHandler.tracer.TraceMethod())
        this.RemoveAssociation((object) this, new EventArgs());
    }

    internal ServerRemoteHost GetHostAssociatedWithPowerShell(
      HostInfo powerShellHostInfo,
      ServerRemoteHost runspaceServerRemoteHost)
    {
      return new ServerRemoteHost(this.clientRunspacePoolId, this.clientPowerShellId, !powerShellHostInfo.UseRunspaceHost ? powerShellHostInfo : runspaceServerRemoteHost.HostInfo, this.transportManager);
    }

    internal event EventHandler RemoveAssociation;

    internal event EventHandler StopPowerShellReceived;

    internal event EventHandler<RemoteDataEventArgs<object>> InputReceived;

    internal event EventHandler InputEndReceived;

    internal event EventHandler<RemoteDataEventArgs<RemoteHostResponse>> HostResponseReceived;

    internal Guid PowerShellId
    {
      get
      {
        using (ServerPowerShellDataStructureHandler.tracer.TraceProperty())
          return this.clientPowerShellId;
      }
    }

    private void SendDataAsync(RemoteDataObject data)
    {
      using (ServerPowerShellDataStructureHandler.tracer.TraceMethod())
        this.transportManager.SendDataToClient(data, false);
    }

    private void HandleTransportClosing(object sender, EventArgs args)
    {
      if (this.StopPowerShellReceived == null)
        return;
      this.StopPowerShellReceived((object) this, args);
    }
  }
}
