// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ServerRunspacePoolDataStructureHandler
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Remoting;
using System.Management.Automation.Remoting.Server;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal class ServerRunspacePoolDataStructureHandler
  {
    [TraceSource("SRPP", "ServerRunspacePoolDataStructureHandler")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("SRPP", nameof (ServerRunspacePoolDataStructureHandler));
    private Guid clientRunspacePoolId;
    private AbstractServerSessionTransportManager transportManager;
    private Dictionary<Guid, ServerPowerShellDataStructureHandler> associatedShells = new Dictionary<Guid, ServerPowerShellDataStructureHandler>();
    private object associationSyncObject = new object();

    internal ServerRunspacePoolDataStructureHandler(
      ServerRunspacePoolDriver driver,
      AbstractServerSessionTransportManager transportManager)
    {
      using (ServerRunspacePoolDataStructureHandler.tracer.TraceConstructor((object) this))
      {
        this.clientRunspacePoolId = driver.InstanceId;
        this.transportManager = transportManager;
      }
    }

    internal void SendApplicationPrivateDataToClient(
      PSPrimitiveDictionary applicationPrivateData,
      RemoteSessionCapability serverCapability)
    {
      using (ServerRunspacePoolDataStructureHandler.tracer.TraceMethod())
      {
        PSPrimitiveDictionary applicationPrivateData1 = PSPrimitiveDictionary.CloneAndAddPSVersionTable(applicationPrivateData);
        PSPrimitiveDictionary primitiveDictionary = (PSPrimitiveDictionary) applicationPrivateData1["PSVersionTable"];
        primitiveDictionary["PSRemotingProtocolVersion"] = (object) serverCapability.ProtocolVersion;
        primitiveDictionary["SerializationVersion"] = (object) serverCapability.SerializationVersion;
        primitiveDictionary["PSVersion"] = (object) serverCapability.PSVersion;
        this.SendDataAsync(RemotingEncoder.GenerateApplicationPrivateData(this.clientRunspacePoolId, applicationPrivateData1));
      }
    }

    internal void SendStateInfoToClient(RunspacePoolStateInfo stateInfo)
    {
      using (ServerRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.SendDataAsync(RemotingEncoder.GenerateRunspacePoolStateInfo(this.clientRunspacePoolId, stateInfo));
    }

    internal void SendPSEventArgsToClient(PSEventArgs e)
    {
      using (ServerRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.SendDataAsync(RemotingEncoder.GeneratePSEventArgs(this.clientRunspacePoolId, e));
    }

    internal void ProcessReceivedData(RemoteDataObject<PSObject> receivedData)
    {
      using (ServerRunspacePoolDataStructureHandler.tracer.TraceMethod())
      {
        if (receivedData == null)
          throw ServerRunspacePoolDataStructureHandler.tracer.NewArgumentNullException(nameof (receivedData));
        switch (receivedData.DataType)
        {
          case RemotingDataType.SetMaxRunspaces:
            this.SetMaxRunspacesReceived((object) this, new RemoteDataEventArgs<PSObject>((object) receivedData.Data));
            break;
          case RemotingDataType.SetMinRunspaces:
            this.SetMinRunspacesReceived((object) this, new RemoteDataEventArgs<PSObject>((object) receivedData.Data));
            break;
          case RemotingDataType.CreatePowerShell:
            this.CreateAndInvokePowerShell((object) this, new RemoteDataEventArgs<RemoteDataObject<PSObject>>((object) receivedData));
            break;
          case RemotingDataType.AvailableRunspaces:
            this.GetAvailableRunspacesReceived((object) this, new RemoteDataEventArgs<PSObject>((object) receivedData.Data));
            break;
          case RemotingDataType.GetCommandMetadata:
            this.GetCommandMetadata((object) this, new RemoteDataEventArgs<RemoteDataObject<PSObject>>((object) receivedData));
            break;
          case RemotingDataType.RemoteRunspaceHostResponseData:
            this.HostResponseReceived((object) this, new RemoteDataEventArgs<RemoteHostResponse>((object) RemoteHostResponse.Decode(receivedData.Data)));
            break;
        }
      }
    }

    internal ServerPowerShellDataStructureHandler CreatePowerShellDataStructureHandler(
      ServerPowerShellDriver driver)
    {
      using (ServerRunspacePoolDataStructureHandler.tracer.TraceMethod())
      {
        AbstractServerTransportManager transportManager = (AbstractServerTransportManager) this.transportManager;
        if (driver.InstanceId != Guid.Empty)
          transportManager = this.transportManager.GetCommandTransportManager(driver.InstanceId);
        ServerPowerShellDataStructureHandler structureHandler = new ServerPowerShellDataStructureHandler(driver, transportManager);
        lock (this.associationSyncObject)
          this.associatedShells.Add(structureHandler.PowerShellId, structureHandler);
        structureHandler.RemoveAssociation += new EventHandler(this.HandleRemoveAssociation);
        return structureHandler;
      }
    }

    internal void DispatchMessageToPowerShell(RemoteDataObject<PSObject> rcvdData)
    {
      using (ServerRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.GetAssociatedPowerShellDataStructureHandler(rcvdData.PowerShellId)?.ProcessReceivedData(rcvdData);
    }

    internal void SendResponseToClient(long callId, object response) => this.SendDataAsync(RemotingEncoder.GenerateRunspacePoolOperationResponse(this.clientRunspacePoolId, response, callId));

    internal TypeTable TypeTable
    {
      get => this.transportManager.TypeTable;
      set => this.transportManager.TypeTable = value;
    }

    internal event EventHandler<RemoteDataEventArgs<RemoteDataObject<PSObject>>> CreateAndInvokePowerShell;

    internal event EventHandler<RemoteDataEventArgs<RemoteDataObject<PSObject>>> GetCommandMetadata;

    internal event EventHandler<RemoteDataEventArgs<RemoteHostResponse>> HostResponseReceived;

    internal event EventHandler<RemoteDataEventArgs<PSObject>> SetMaxRunspacesReceived;

    internal event EventHandler<RemoteDataEventArgs<PSObject>> SetMinRunspacesReceived;

    internal event EventHandler<RemoteDataEventArgs<PSObject>> GetAvailableRunspacesReceived;

    private void SendDataAsync(RemoteDataObject data)
    {
      using (ServerRunspacePoolDataStructureHandler.tracer.TraceMethod())
        this.transportManager.SendDataToClient(data, true);
    }

    private ServerPowerShellDataStructureHandler GetAssociatedPowerShellDataStructureHandler(
      Guid clientPowerShellId)
    {
      using (ServerRunspacePoolDataStructureHandler.tracer.TraceMethod())
      {
        ServerPowerShellDataStructureHandler structureHandler = (ServerPowerShellDataStructureHandler) null;
        lock (this.associationSyncObject)
        {
          if (!this.associatedShells.TryGetValue(clientPowerShellId, out structureHandler))
            structureHandler = (ServerPowerShellDataStructureHandler) null;
        }
        return structureHandler;
      }
    }

    private void HandleRemoveAssociation(object sender, EventArgs e)
    {
      using (ServerRunspacePoolDataStructureHandler.tracer.TraceMethod())
      {
        ServerPowerShellDataStructureHandler structureHandler = sender as ServerPowerShellDataStructureHandler;
        lock (this.associationSyncObject)
          this.associatedShells.Remove(structureHandler.PowerShellId);
        this.transportManager.RemoveCommandTransportManager(structureHandler.PowerShellId);
      }
    }
  }
}
