// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ClientMethodExecutor
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting.Client;
using System.Management.Automation.Runspaces.Internal;

namespace System.Management.Automation.Remoting
{
  internal class ClientMethodExecutor
  {
    private BaseClientTransportManager _transportManager;
    private PSHost _clientHost;
    private Guid _clientRunspacePoolId;
    private Guid _clientPowerShellId;
    private RemoteHostCall _remoteHostCall;

    internal RemoteHostCall RemoteHostCall => this._remoteHostCall;

    private ClientMethodExecutor(
      BaseClientTransportManager transportManager,
      PSHost clientHost,
      Guid clientRunspacePoolId,
      Guid clientPowerShellId,
      RemoteHostCall remoteHostCall)
    {
      this._transportManager = transportManager;
      this._remoteHostCall = remoteHostCall;
      this._clientHost = clientHost;
      this._clientRunspacePoolId = clientRunspacePoolId;
      this._clientPowerShellId = clientPowerShellId;
    }

    internal static void Dispatch(
      BaseClientTransportManager transportManager,
      PSHost clientHost,
      PSDataCollectionStream<ErrorRecord> errorStream,
      ObjectStream methodExecutorStream,
      bool isMethodExecutorStreamEnabled,
      RemoteRunspacePoolInternal runspacePool,
      Guid clientPowerShellId,
      RemoteHostCall remoteHostCall)
    {
      ClientMethodExecutor clientMethodExecutor = new ClientMethodExecutor(transportManager, clientHost, runspacePool.InstanceId, clientPowerShellId, remoteHostCall);
      if (clientPowerShellId == Guid.Empty)
        clientMethodExecutor.Execute(errorStream);
      else if (remoteHostCall.IsSetShouldExit && isMethodExecutorStreamEnabled)
        runspacePool.Close();
      else if (isMethodExecutorStreamEnabled)
        methodExecutorStream.Write((object) clientMethodExecutor);
      else
        clientMethodExecutor.Execute(errorStream);
    }

    private bool IsRunspacePushed(PSHost host) => host is IHostSupportsInteractiveSession interactiveSession && interactiveSession.IsRunspacePushed;

    internal void Execute(PSDataCollectionStream<ErrorRecord> errorStream) => this.Execute(errorStream == null || this.IsRunspacePushed(this._clientHost) ? (Action<ErrorRecord>) (errorRecord =>
    {
      try
      {
        if (this._clientHost.UI == null)
          return;
        this._clientHost.UI.WriteErrorLine(errorRecord.ToString());
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
    }) : (Action<ErrorRecord>) (errorRecord => errorStream.Write((object) errorRecord)));

    internal void Execute(Cmdlet cmdlet) => this.Execute(new Action<ErrorRecord>(cmdlet.WriteError));

    internal void Execute(Action<ErrorRecord> writeErrorAction)
    {
      if (this._remoteHostCall.IsVoidMethod)
        this.ExecuteVoid(writeErrorAction);
      else
        this._transportManager.DataToBeSentCollection.Add<PSObject>(RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.Server, this._clientPowerShellId == Guid.Empty ? RemotingDataType.RemoteRunspaceHostResponseData : RemotingDataType.RemotePowerShellHostResponseData, this._clientRunspacePoolId, this._clientPowerShellId, this._remoteHostCall.ExecuteNonVoidMethod(this._clientHost).Encode()), DataPriorityType.PromptResponse);
    }

    internal void ExecuteVoid(Action<ErrorRecord> writeErrorAction)
    {
      try
      {
        this._remoteHostCall.ExecuteVoidMethod(this._clientHost);
      }
      catch (Exception ex)
      {
        Exception exception = ex;
        CommandProcessorBase.CheckForSevereException(exception);
        if (exception.InnerException != null)
          exception = exception.InnerException;
        ErrorRecord errorRecord = new ErrorRecord(exception, PSRemotingErrorId.RemoteHostCallFailed.ToString(), ErrorCategory.InvalidArgument, (object) this._remoteHostCall.MethodName);
        writeErrorAction(errorRecord);
      }
    }
  }
}
