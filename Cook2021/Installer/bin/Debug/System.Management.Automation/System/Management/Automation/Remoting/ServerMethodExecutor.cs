// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ServerMethodExecutor
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting.Server;

namespace System.Management.Automation.Remoting
{
  internal class ServerMethodExecutor
  {
    private const long DefaultClientPipelineId = -1;
    private Guid _clientRunspacePoolId;
    private Guid _clientPowerShellId;
    private ServerDispatchTable _serverDispatchTable;
    private RemotingDataType _remoteHostCallDataType;
    private AbstractServerTransportManager _transportManager;

    internal ServerMethodExecutor(
      Guid clientRunspacePoolId,
      Guid clientPowerShellId,
      AbstractServerTransportManager transportManager)
    {
      this._clientRunspacePoolId = clientRunspacePoolId;
      this._clientPowerShellId = clientPowerShellId;
      this._transportManager = transportManager;
      this._remoteHostCallDataType = clientPowerShellId == Guid.Empty ? RemotingDataType.RemoteHostCallUsingRunspaceHost : RemotingDataType.RemoteHostCallUsingPowerShellHost;
      this._serverDispatchTable = new ServerDispatchTable();
    }

    internal void HandleRemoteHostResponseFromClient(RemoteHostResponse remoteHostResponse) => this._serverDispatchTable.SetResponse(remoteHostResponse.CallId, remoteHostResponse);

    internal void AbortAllCalls() => this._serverDispatchTable.AbortAllCalls();

    internal void ExecuteVoidMethod(RemoteHostMethodId methodId) => this.ExecuteVoidMethod(methodId, new object[0]);

    internal void ExecuteVoidMethod(RemoteHostMethodId methodId, object[] parameters) => this._transportManager.SendDataToClient<PSObject>(RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.Client, this._remoteHostCallDataType, this._clientRunspacePoolId, this._clientPowerShellId, new RemoteHostCall(-100L, methodId, parameters).Encode()), false);

    internal T ExecuteMethod<T>(RemoteHostMethodId methodId) => this.ExecuteMethod<T>(methodId, new object[0]);

    internal T ExecuteMethod<T>(RemoteHostMethodId methodId, object[] parameters)
    {
      long newCallId = this._serverDispatchTable.CreateNewCallId();
      this._transportManager.SendDataToClient<PSObject>(RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.Client, this._remoteHostCallDataType, this._clientRunspacePoolId, this._clientPowerShellId, new RemoteHostCall(newCallId, methodId, parameters).Encode()), false);
      RemoteHostResponse response = this._serverDispatchTable.GetResponse(newCallId, (RemoteHostResponse) null);
      if (response == null)
        throw RemoteHostExceptions.NewRemoteHostCallFailedException(methodId);
      response.SimulateExecution();
      return (T) response.SimulateExecution();
    }
  }
}
