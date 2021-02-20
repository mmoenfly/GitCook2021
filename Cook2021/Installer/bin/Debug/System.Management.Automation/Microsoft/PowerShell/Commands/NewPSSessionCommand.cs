// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.NewPSSessionCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Runspaces.Internal;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("New", "PSSession", DefaultParameterSetName = "ComputerName")]
  public class NewPSSessionCommand : PSRemotingBaseCmdlet, IDisposable
  {
    [TraceSource("NewPSSession", "NewPSSessionCommand")]
    internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer("NewPSSession", nameof (NewPSSessionCommand));
    private string[] computerNames;
    private PSSession[] remoteRunspaceInfos;
    private string[] names;
    private ThrottleManager throttleManager;
    private ObjectStream stream = new ObjectStream();
    private ManualResetEvent operationsComplete = new ManualResetEvent(true);
    private List<RemoteRunspace> toDispose = new List<RemoteRunspace>();
    private Dictionary<Guid, string> friendlyNames = new Dictionary<Guid, string>();

    [Parameter(ParameterSetName = "ComputerName", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] {"Cn"})]
    public override string[] ComputerName
    {
      get => this.computerNames;
      set => this.computerNames = value;
    }

    [Parameter(ParameterSetName = "Uri", ValueFromPipelineByPropertyName = true)]
    [System.Management.Automation.Credential]
    [Parameter(ParameterSetName = "ComputerName", ValueFromPipelineByPropertyName = true)]
    public override PSCredential Credential
    {
      get => base.Credential;
      set => base.Credential = value;
    }

    [Parameter(ParameterSetName = "Session", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public override PSSession[] Session
    {
      get => this.remoteRunspaceInfos;
      set => this.remoteRunspaceInfos = value;
    }

    [Parameter]
    public string[] Name
    {
      get => this.names;
      set => this.names = value;
    }

    protected override void BeginProcessing()
    {
      using (NewPSSessionCommand.tracer.TraceMethod())
      {
        base.BeginProcessing();
        this.operationsComplete.Reset();
        this.throttleManager = new ThrottleManager();
        this.throttleManager.ThrottleLimit = this.ThrottleLimit;
        this.throttleManager.ThrottleComplete += new EventHandler<EventArgs>(this.HandleThrottleComplete);
      }
    }

    protected override void ProcessRecord()
    {
      using (NewPSSessionCommand.tracer.TraceMethod())
      {
        List<IThrottleOperation> operations = new List<IThrottleOperation>();
        List<RemoteRunspace> remoteRunspaceList;
        switch (this.ParameterSetName)
        {
          case "Session":
            remoteRunspaceList = this.CreateRunspacesWhenRunspaceParameterSpecified();
            break;
          case "Uri":
            remoteRunspaceList = this.CreateRunspacesWhenUriParameterSpecified();
            break;
          case "ComputerName":
            remoteRunspaceList = this.CreateRunspacesWhenComputerNameParameterSpecified();
            break;
          default:
            remoteRunspaceList = new List<RemoteRunspace>();
            break;
        }
        foreach (RemoteRunspace runspace in remoteRunspaceList)
        {
          runspace.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(this.OnRunspacePSEventReceived);
          OpenRunspaceOperation runspaceOperation = new OpenRunspaceOperation(runspace);
          runspaceOperation.OperationComplete += new EventHandler<OperationStateEventArgs>(this.HandleRunspaceStateChanged);
          runspace.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
          operations.Add((IThrottleOperation) runspaceOperation);
        }
        if (this.names != null)
        {
          int num = remoteRunspaceList.Count < this.names.Length ? remoteRunspaceList.Count : this.names.Length;
          for (int index = 0; index < num; ++index)
            this.friendlyNames.Add(remoteRunspaceList[index].InstanceId, this.names[index]);
        }
        this.throttleManager.SubmitOperations(operations);
        foreach (PSStreamObject psstreamObject in this.stream.ObjectReader.NonBlockingRead())
          this.WriteStreamObject(psstreamObject);
      }
    }

    protected override void EndProcessing()
    {
      using (NewPSSessionCommand.tracer.TraceMethod())
      {
        this.throttleManager.EndSubmitOperations();
        while (true)
        {
          this.stream.ObjectReader.WaitHandle.WaitOne();
          if (!this.stream.ObjectReader.EndOfPipeline)
            this.WriteStreamObject((PSStreamObject) this.stream.ObjectReader.Read());
          else
            break;
        }
      }
    }

    protected override void StopProcessing()
    {
      using (NewPSSessionCommand.tracer.TraceMethod())
      {
        this.stream.ObjectWriter.Close();
        this.throttleManager.StopAllOperations();
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void OnRunspacePSEventReceived(object sender, PSEventArgs e)
    {
      if (this.Events == null)
        return;
      this.Events.AddForwardedEvent(e);
    }

    private void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs) => this.stream.Write((object) new PSStreamObject(PSStreamObjectType.Warning, (object) ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "URIRedirectWarningToHost", (object) eventArgs.Data.OriginalString)));

    private void HandleRunspaceStateChanged(object sender, OperationStateEventArgs stateEventArgs)
    {
      using (NewPSSessionCommand.tracer.TraceMethod())
      {
        if (sender == null)
          throw NewPSSessionCommand.tracer.NewArgumentNullException(nameof (sender));
        RunspaceStateEventArgs runspaceStateEventArgs = stateEventArgs != null ? stateEventArgs.BaseEvent as RunspaceStateEventArgs : throw NewPSSessionCommand.tracer.NewArgumentNullException(nameof (stateEventArgs));
        RunspaceState state = runspaceStateEventArgs.RunspaceStateInfo.State;
        RemoteRunspace operatedRunspace = (sender as OpenRunspaceOperation).OperatedRunspace;
        if (operatedRunspace != null)
          operatedRunspace.URIRedirectionReported -= new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
        PipelineWriter objectWriter = this.stream.ObjectWriter;
        Exception reason = runspaceStateEventArgs.RunspaceStateInfo.Reason;
        switch (state)
        {
          case RunspaceState.Opened:
            PSSession psSession = new PSSession(operatedRunspace, string.Empty, this.GetShellForDisplay());
            string str1;
            this.friendlyNames.TryGetValue(operatedRunspace.InstanceId, out str1);
            if (!string.IsNullOrEmpty(str1))
              psSession.Name = str1;
            this.RunspaceRepository.Add(psSession);
            PSStreamObject psStreamObject1 = new PSStreamObject(PSStreamObjectType.Output, (object) psSession);
            if (!objectWriter.IsOpen)
              break;
            objectWriter.Write((object) psStreamObject1);
            break;
          case RunspaceState.Closed:
            Uri manConnectionInfo = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<Uri>(operatedRunspace.ConnectionInfo, "ConnectionUri", (Uri) null);
            PSStreamObject psStreamObject2 = new PSStreamObject(PSStreamObjectType.Verbose, (object) this.GetMessage(PSRemotingErrorId.RemoteRunspaceClosed, manConnectionInfo != (Uri) null ? (object) manConnectionInfo.AbsoluteUri : (object) string.Empty));
            if (objectWriter.IsOpen)
              objectWriter.Write((object) psStreamObject2);
            if (reason == null)
              break;
            PSStreamObject psStreamObject3 = new PSStreamObject(PSStreamObjectType.Error, (object) new ErrorRecord(reason, "PSSessionStateClosed", ErrorCategory.OpenError, (object) operatedRunspace));
            if (!objectWriter.IsOpen)
              break;
            objectWriter.Write((object) psStreamObject3);
            break;
          case RunspaceState.Broken:
            PSRemotingTransportException transportException = reason as PSRemotingTransportException;
            string errorDetails_Message = (string) null;
            if (transportException != null && sender is OpenRunspaceOperation runspaceOperation)
            {
              string computerName = runspaceOperation.OperatedRunspace.ConnectionInfo.ComputerName;
              if (transportException.ErrorCode == -2144108135)
              {
                string str2 = PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.URIRedirectionReported, (object) transportException.Message, (object) "MaximumConnectionRedirectionCount", (object) "PSSessionOption", (object) "AllowRedirection");
                errorDetails_Message = "[" + computerName + "] " + str2;
              }
              else
              {
                errorDetails_Message = "[" + computerName + "] ";
                if (!string.IsNullOrEmpty(transportException.Message))
                  errorDetails_Message += transportException.Message;
                else if (!string.IsNullOrEmpty(transportException.TransportMessage))
                  errorDetails_Message += transportException.TransportMessage;
              }
            }
            if (reason is PSRemotingDataStructureException structureException && sender is OpenRunspaceOperation runspaceOperation)
              errorDetails_Message = "[" + runspaceOperation.OperatedRunspace.ConnectionInfo.ComputerName + "] " + structureException.Message;
            PSStreamObject psStreamObject4 = new PSStreamObject(PSStreamObjectType.Error, (object) new ErrorRecord(reason, (object) operatedRunspace, "PSSessionOpenFailed", ErrorCategory.OpenError, (string) null, (string) null, (string) null, (string) null, (string) null, errorDetails_Message, (string) null));
            if (objectWriter.IsOpen)
              objectWriter.Write((object) psStreamObject4);
            this.toDispose.Add(operatedRunspace);
            break;
        }
      }
    }

    private List<RemoteRunspace> CreateRunspacesWhenRunspaceParameterSpecified()
    {
      using (NewPSSessionCommand.tracer.TraceMethod())
      {
        List<RemoteRunspace> remoteRunspaceList = new List<RemoteRunspace>();
        this.ValidateRemoteRunspacesSpecified();
        foreach (PSSession remoteRunspaceInfo in this.remoteRunspaceInfos)
        {
          if (remoteRunspaceInfo != null)
          {
            if (remoteRunspaceInfo.Runspace != null)
            {
              try
              {
                RemoteRunspace runspace1 = (RemoteRunspace) remoteRunspaceInfo.Runspace;
                WSManConnectionInfo connectionInfo1 = runspace1.ConnectionInfo as WSManConnectionInfo;
                WSManConnectionInfo connectionInfo2;
                if (connectionInfo1 != null)
                {
                  connectionInfo2 = connectionInfo1.Copy();
                }
                else
                {
                  connectionInfo2 = new WSManConnectionInfo(WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<Uri>(runspace1.ConnectionInfo, "ConnectionUri", (Uri) null), WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<string>(runspace1.ConnectionInfo, "ShellUri", string.Empty), runspace1.ConnectionInfo.Credential);
                  this.UpdateConnectionInfo(connectionInfo2);
                }
                RemoteRunspacePoolInternal runspacePoolInternal = runspace1.RunspacePool.RemoteRunspacePoolInternal;
                TypeTable typeTable = (TypeTable) null;
                if (runspacePoolInternal != null && runspacePoolInternal.DataStructureHandler != null && runspacePoolInternal.DataStructureHandler.TransportManager != null)
                  typeTable = runspacePoolInternal.DataStructureHandler.TransportManager.Fragmentor.TypeTable;
                RemoteRunspace runspace2 = (RemoteRunspace) RunspaceFactory.CreateRunspace((RunspaceConnectionInfo) connectionInfo2, this.Host, typeTable, this.SessionOption.ApplicationArguments);
                runspace2.AvailabilityChanged += new EventHandler<RunspaceAvailabilityEventArgs>(this.HandleAvailabilityChanged);
                remoteRunspaceList.Add(runspace2);
                continue;
              }
              catch (UriFormatException ex)
              {
                this.stream.ObjectWriter.Write((object) new PSStreamObject(PSStreamObjectType.Error, (object) new ErrorRecord((Exception) ex, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, (object) remoteRunspaceInfo)));
                continue;
              }
            }
          }
          this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentNullException("PSSession"), "PSSessionArgumentNull", ErrorCategory.InvalidArgument, (object) null));
        }
        return remoteRunspaceList;
      }
    }

    private List<RemoteRunspace> CreateRunspacesWhenUriParameterSpecified()
    {
      using (NewPSSessionCommand.tracer.TraceMethod())
      {
        List<RemoteRunspace> remoteRunspaceList = new List<RemoteRunspace>();
        for (int index = 0; index < this.ConnectionUri.Length; ++index)
        {
          try
          {
            WSManConnectionInfo connectionInfo = this.CertificateThumbprint != null ? new WSManConnectionInfo(this.ConnectionUri[index], this.ConfigurationName, this.CertificateThumbprint) : new WSManConnectionInfo(this.ConnectionUri[index], this.ConfigurationName, this.Credential);
            connectionInfo.AuthenticationMechanism = this.Authentication;
            this.UpdateConnectionInfo(connectionInfo);
            RemoteRunspace runspace = (RemoteRunspace) RunspaceFactory.CreateRunspace((RunspaceConnectionInfo) connectionInfo, this.Host, Utils.GetTypeTableFromExecutionContextTLS(), this.SessionOption.ApplicationArguments);
            runspace.AvailabilityChanged += new EventHandler<RunspaceAvailabilityEventArgs>(this.HandleAvailabilityChanged);
            remoteRunspaceList.Add(runspace);
          }
          catch (UriFormatException ex)
          {
            this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, this.ConnectionUri[index]);
          }
          catch (InvalidOperationException ex)
          {
            this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, this.ConnectionUri[index]);
          }
          catch (ArgumentException ex)
          {
            this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, this.ConnectionUri[index]);
          }
          catch (NotSupportedException ex)
          {
            this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, this.ConnectionUri[index]);
          }
        }
        return remoteRunspaceList;
      }
    }

    private List<RemoteRunspace> CreateRunspacesWhenComputerNameParameterSpecified()
    {
      List<RemoteRunspace> remoteRunspaceList = new List<RemoteRunspace>();
      string[] resolvedComputerNames;
      this.ResolveComputerNames(this.ComputerName, out resolvedComputerNames);
      this.ValidateComputerName(resolvedComputerNames);
      for (int index = 0; index < resolvedComputerNames.Length; ++index)
      {
        try
        {
          WSManConnectionInfo connectionInfo;
          if (this.CertificateThumbprint == null)
          {
            connectionInfo = new WSManConnectionInfo(this.UseSSL.IsPresent, resolvedComputerNames[index], this.Port, this.ApplicationName, this.ConfigurationName, this.Credential);
          }
          else
          {
            connectionInfo = new WSManConnectionInfo(WSManConnectionInfo.ConstructUri(this.UseSSL.IsPresent ? WSManConnectionInfo.DEFAULT_SSL_SCHEME : string.Empty, resolvedComputerNames[index], this.Port, this.ApplicationName), this.ConfigurationName, this.CertificateThumbprint);
            connectionInfo.UseDefaultWSManPort = true;
          }
          connectionInfo.AuthenticationMechanism = this.Authentication;
          this.UpdateConnectionInfo(connectionInfo);
          RemoteRunspace runspace = (RemoteRunspace) RunspaceFactory.CreateRunspace((RunspaceConnectionInfo) connectionInfo, this.Host, Utils.GetTypeTableFromExecutionContextTLS(), this.SessionOption.ApplicationArguments);
          runspace.AvailabilityChanged += new EventHandler<RunspaceAvailabilityEventArgs>(this.HandleAvailabilityChanged);
          remoteRunspaceList.Add(runspace);
        }
        catch (UriFormatException ex)
        {
          this.stream.ObjectWriter.Write((object) new PSStreamObject(PSStreamObjectType.Error, (object) new ErrorRecord((Exception) ex, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, (object) resolvedComputerNames[index])));
        }
      }
      return remoteRunspaceList;
    }

    protected void Dispose(bool disposing)
    {
      using (NewPSSessionCommand.tracer.TraceDispose((object) this))
      {
        if (!disposing)
          return;
        this.throttleManager.Dispose();
        this.operationsComplete.WaitOne();
        this.operationsComplete.Close();
        this.throttleManager.ThrottleComplete -= new EventHandler<EventArgs>(this.HandleThrottleComplete);
        this.throttleManager = (ThrottleManager) null;
        foreach (Runspace runspace in this.toDispose)
          runspace.Dispose();
        this.stream.Dispose();
      }
    }

    private void HandleThrottleComplete(object sender, EventArgs eventArgs)
    {
      using (NewPSSessionCommand.tracer.TraceMethod())
      {
        this.stream.ObjectWriter.Close();
        this.operationsComplete.Set();
      }
    }

    private void WriteErrorCreateRemoteRunspaceFailed(Exception e, Uri uri)
    {
      using (NewPSSessionCommand.tracer.TraceMethod())
        this.stream.ObjectWriter.Write((object) new PSStreamObject(PSStreamObjectType.Error, (object) new ErrorRecord(e, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, (object) uri)));
    }

    private string GetShellForDisplay()
    {
      string str = "http://schemas.microsoft.com/powershell/";
      return this.ConfigurationName.IndexOf(str, StringComparison.OrdinalIgnoreCase) == 0 ? this.ConfigurationName.Substring(str.Length) : this.ConfigurationName;
    }

    private void HandleAvailabilityChanged(object sender, RunspaceAvailabilityEventArgs eventArgs)
    {
    }
  }
}
