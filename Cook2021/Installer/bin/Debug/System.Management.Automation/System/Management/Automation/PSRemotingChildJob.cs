// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSRemotingChildJob
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal class PSRemotingChildJob : Job
  {
    [TraceSource("PSJob", "Job APIs")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PSJob", "Job APIs");
    private Runspace remoteRunspace;
    private bool hideComputerName = true;
    private bool doFinishCalled;
    private ErrorRecord failureErrorRecord;
    private bool isDisposed;
    private bool cleanupDone;
    private ExecutionCmdletHelper helper;
    private RemotePipeline remotePipeline;
    protected object syncObject = new object();
    private ThrottleManager throttleManager;
    private bool stopIsCalled;

    internal PSRemotingChildJob(
      string remoteCommand,
      ExecutionCmdletHelper helper,
      ThrottleManager throttleManager)
      : base(remoteCommand)
    {
      using (PSRemotingChildJob.tracer.TraceConstructor((object) this))
      {
        this.helper = helper;
        this.remoteRunspace = helper.Pipeline.Runspace;
        this.remotePipeline = helper.Pipeline as RemotePipeline;
        this.throttleManager = throttleManager;
        if (this.remoteRunspace is RemoteRunspace remoteRunspace && remoteRunspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen)
          remoteRunspace.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
        this.AggregateResultsFromHelper(helper);
        this.RegisterThrottleComplete(throttleManager);
      }
    }

    protected PSRemotingChildJob()
    {
    }

    public override void StopJob()
    {
      using (PSRemotingChildJob.tracer.TraceMethod())
      {
        if (this.isDisposed || this.stopIsCalled || this.IsFinishedState(this.JobStateInfo.State))
          return;
        lock (this.syncObject)
        {
          if (this.isDisposed || this.stopIsCalled || this.IsFinishedState(this.JobStateInfo.State))
            return;
          this.stopIsCalled = true;
        }
        this.throttleManager.StopOperation((IThrottleOperation) this.helper);
        this.Finished.WaitOne();
      }
    }

    public override string StatusMessage => "";

    public override bool HasMoreData => this.Results.IsOpen || this.Results.Count > 0;

    public override string Location
    {
      get
      {
        using (PSRemotingChildJob.tracer.TraceProperty())
          return this.remoteRunspace.ConnectionInfo.ComputerName;
      }
    }

    public Runspace Runspace => this.remoteRunspace;

    internal ExecutionCmdletHelper Helper => this.helper;

    internal bool HideComputerName
    {
      get => this.hideComputerName;
      set
      {
        this.hideComputerName = value;
        foreach (Job childJob in (IEnumerable<Job>) this.ChildJobs)
        {
          if (childJob is PSRemotingChildJob remotingChildJob)
            remotingChildJob.HideComputerName = value;
        }
      }
    }

    private void HandleOutputReady(object sender, EventArgs eventArgs)
    {
      using (PSRemotingChildJob.tracer.TraceMethod())
      {
        PSDataCollectionPipelineReader<PSObject, PSObject> collectionPipelineReader = sender as PSDataCollectionPipelineReader<PSObject, PSObject>;
        foreach (PSObject psObject in collectionPipelineReader.NonBlockingRead())
        {
          if (psObject != null)
          {
            if (psObject.Properties[RemotingConstants.ComputerNameNoteProperty] != null)
              psObject.Properties.Remove(RemotingConstants.ComputerNameNoteProperty);
            if (psObject.Properties[RemotingConstants.RunspaceIdNoteProperty] != null)
              psObject.Properties.Remove(RemotingConstants.RunspaceIdNoteProperty);
            psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty(RemotingConstants.ComputerNameNoteProperty, (object) collectionPipelineReader.ComputerName));
            psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty(RemotingConstants.RunspaceIdNoteProperty, (object) collectionPipelineReader.RunspaceId));
            if (!this.hideComputerName && psObject.Properties[RemotingConstants.ShowComputerNameNoteProperty] == null)
            {
              PSNoteProperty psNoteProperty = new PSNoteProperty(RemotingConstants.ShowComputerNameNoteProperty, (object) true);
              psObject.Properties.Add((PSPropertyInfo) psNoteProperty);
            }
          }
          PSStreamObject psStreamObject = new PSStreamObject(PSStreamObjectType.Output, (object) psObject);
          this.Output.Add(psObject);
          this.Results.Add(psStreamObject);
        }
      }
    }

    private void HandleErrorReady(object sender, EventArgs eventArgs)
    {
      using (PSRemotingChildJob.tracer.TraceMethod())
      {
        PSDataCollectionPipelineReader<ErrorRecord, object> collectionPipelineReader = sender as PSDataCollectionPipelineReader<ErrorRecord, object>;
        foreach (object obj in collectionPipelineReader.NonBlockingRead())
        {
          if (obj is ErrorRecord errorRecord)
          {
            OriginInfo originInfo = new OriginInfo(collectionPipelineReader.ComputerName, collectionPipelineReader.RunspaceId);
            RemotingErrorRecord remotingErrorRecord = new RemotingErrorRecord(errorRecord, originInfo);
            this.Error.Add((ErrorRecord) remotingErrorRecord);
            this.Results.Add(new PSStreamObject(PSStreamObjectType.Error, (object) remotingErrorRecord));
          }
        }
      }
    }

    protected void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs)
    {
      string message = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "URIRedirectWarningToHost", (object) eventArgs.Data.OriginalString);
      PSStreamObject psStreamObject = new PSStreamObject(PSStreamObjectType.Warning, (object) message);
      this.Warning.Add(new WarningRecord(message));
      this.Results.Add(psStreamObject);
    }

    private void HandleHostCalls(object sender, EventArgs eventArgs)
    {
      using (PSRemotingChildJob.tracer.TraceMethod())
      {
        if (!(sender is ObjectStream objectStream))
          return;
        Collection<object> collection = objectStream.NonBlockingRead(objectStream.Count);
        lock (this.syncObject)
        {
          foreach (ClientMethodExecutor clientMethodExecutor in collection)
          {
            this.Results.Add(new PSStreamObject(PSStreamObjectType.MethodExecutor, (object) clientMethodExecutor));
            if (clientMethodExecutor.RemoteHostCall.CallId != -100L)
              this.SetJobState(JobState.Blocked, (Exception) null);
          }
        }
      }
    }

    private void HandlePipelineStateChanged(object sender, PipelineStateEventArgs e)
    {
      using (PSRemotingChildJob.tracer.TraceMethod())
      {
        if (this.remoteRunspace != null && e.PipelineStateInfo.State != PipelineState.Running)
          ((RemoteRunspace) this.remoteRunspace).URIRedirectionReported -= new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
        if (e.PipelineStateInfo.State != PipelineState.Running)
          return;
        this.SetJobState(JobState.Running);
      }
    }

    private void HandleThrottleComplete(object sender, EventArgs eventArgs)
    {
      using (PSRemotingChildJob.tracer.TraceMethod())
        this.DoFinish();
    }

    protected virtual void HandleOperationComplete(
      object sender,
      OperationStateEventArgs stateEventArgs)
    {
      using (PSRemotingChildJob.tracer.TraceMethod())
        this.DeterminedAndSetJobState(sender as ExecutionCmdletHelper);
    }

    protected virtual void DoFinish()
    {
      using (PSRemotingChildJob.tracer.TraceMethod())
      {
        if (this.doFinishCalled)
          return;
        lock (this.syncObject)
        {
          if (this.doFinishCalled)
            return;
          this.doFinishCalled = true;
        }
        this.DeterminedAndSetJobState(this.helper);
        this.DoCleanupOnFinished();
      }
    }

    internal ErrorRecord FailureErrorRecord => this.failureErrorRecord;

    protected void ProcessJobFailure(
      ExecutionCmdletHelper helper,
      out Exception failureException,
      out ErrorRecord failureErrorRecord)
    {
      using (PSRemotingChildJob.tracer.TraceMethod())
      {
        RemotePipeline pipeline = helper.Pipeline as RemotePipeline;
        RemoteRunspace runspace = pipeline.GetRunspace() as RemoteRunspace;
        failureException = (Exception) null;
        failureErrorRecord = (ErrorRecord) null;
        if (helper.InternalException != null)
        {
          failureException = helper.InternalException;
          failureErrorRecord = new ErrorRecord(helper.InternalException, "RemotePipelineExecutionFailed", ErrorCategory.OperationStopped, (object) helper);
        }
        else if (runspace.RunspaceStateInfo.State == RunspaceState.Broken)
        {
          failureException = runspace.RunspaceStateInfo.Reason;
          string errorDetails_Message = (string) null;
          if (failureException is PSRemotingTransportException transportException)
          {
            errorDetails_Message = "[" + runspace.ConnectionInfo.ComputerName + "] ";
            if (transportException.ErrorCode == -2144108135)
            {
              string str = PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.URIRedirectionReported, (object) transportException.Message, (object) "MaximumConnectionRedirectionCount", (object) "PSSessionOption", (object) "AllowRedirection");
              errorDetails_Message += str;
            }
            else if (!string.IsNullOrEmpty(transportException.Message))
              errorDetails_Message += transportException.Message;
            else if (!string.IsNullOrEmpty(transportException.TransportMessage))
              errorDetails_Message += transportException.TransportMessage;
          }
          failureErrorRecord = new ErrorRecord(failureException, (object) null, "PSSessionStateBroken", ErrorCategory.OpenError, (string) null, (string) null, (string) null, (string) null, (string) null, errorDetails_Message, (string) null);
        }
        else
        {
          if (pipeline.PipelineStateInfo.State != PipelineState.Failed)
            return;
          failureException = pipeline.PipelineStateInfo.Reason;
          if (failureException == null)
            return;
          RemoteException remoteException = failureException as RemoteException;
          ErrorRecord errorRecord = remoteException == null ? new ErrorRecord(pipeline.PipelineStateInfo.Reason, "JobFailure", ErrorCategory.OperationStopped, (object) this) : remoteException.ErrorRecord;
          OriginInfo originInfo = new OriginInfo(pipeline.GetRunspace().ConnectionInfo.ComputerName, pipeline.GetRunspace().InstanceId);
          failureErrorRecord = (ErrorRecord) new RemotingErrorRecord(errorRecord, originInfo);
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      using (PSRemotingChildJob.tracer.TraceMethod())
      {
        if (!disposing || this.isDisposed)
          return;
        lock (this.syncObject)
        {
          if (this.isDisposed)
            return;
          this.isDisposed = true;
        }
        try
        {
          this.DoCleanupOnFinished();
        }
        finally
        {
          base.Dispose(disposing);
        }
      }
    }

    protected virtual void DoCleanupOnFinished()
    {
      using (PSRemotingChildJob.tracer.TraceMethod())
      {
        bool flag = false;
        if (!this.cleanupDone)
        {
          lock (this.syncObject)
          {
            if (!this.cleanupDone)
            {
              this.cleanupDone = true;
              flag = true;
            }
          }
        }
        if (!flag)
          return;
        this.StopAggregateResultsFromHelper(this.helper);
        this.UnregisterThrottleComplete(this.throttleManager);
        this.throttleManager = (ThrottleManager) null;
      }
    }

    protected void AggregateResultsFromHelper(ExecutionCmdletHelper helper)
    {
      Pipeline pipeline = helper.Pipeline;
      pipeline.Output.DataReady += new EventHandler(this.HandleOutputReady);
      pipeline.Error.DataReady += new EventHandler(this.HandleErrorReady);
      pipeline.StateChanged += new EventHandler<PipelineStateEventArgs>(this.HandlePipelineStateChanged);
      RemotePipeline remotePipeline = pipeline as RemotePipeline;
      remotePipeline.MethodExecutorStream.DataReady += new EventHandler(this.HandleHostCalls);
      remotePipeline.PowerShell.Streams.Progress.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleProgressAdded);
      remotePipeline.PowerShell.Streams.Warning.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleWarningAdded);
      remotePipeline.PowerShell.Streams.Verbose.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleVerboseAdded);
      remotePipeline.PowerShell.Streams.Debug.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleDebugAdded);
      remotePipeline.IsMethodExecutorStreamEnabled = true;
      helper.OperationComplete += new EventHandler<OperationStateEventArgs>(this.HandleOperationComplete);
    }

    private System.Management.Automation.PowerShell GetPipelinePowerShell(
      RemotePipeline pipeline,
      Guid instanceId)
    {
      return pipeline != null ? pipeline.PowerShell : this.GetPowerShell(instanceId);
    }

    private void HandleDebugAdded(object sender, DataAddedEventArgs eventArgs)
    {
      int index = eventArgs.Index;
      System.Management.Automation.PowerShell pipelinePowerShell = this.GetPipelinePowerShell(this.remotePipeline, eventArgs.PowerShellInstanceId);
      if (pipelinePowerShell == null)
        return;
      this.Debug.Add(pipelinePowerShell.Streams.Debug[index]);
    }

    private void HandleVerboseAdded(object sender, DataAddedEventArgs eventArgs)
    {
      int index = eventArgs.Index;
      System.Management.Automation.PowerShell pipelinePowerShell = this.GetPipelinePowerShell(this.remotePipeline, eventArgs.PowerShellInstanceId);
      if (pipelinePowerShell == null)
        return;
      this.Verbose.Add(pipelinePowerShell.Streams.Verbose[index]);
    }

    private void HandleWarningAdded(object sender, DataAddedEventArgs eventArgs)
    {
      int index = eventArgs.Index;
      System.Management.Automation.PowerShell pipelinePowerShell = this.GetPipelinePowerShell(this.remotePipeline, eventArgs.PowerShellInstanceId);
      if (pipelinePowerShell == null)
        return;
      this.Warning.Add(pipelinePowerShell.Streams.Warning[index]);
    }

    private void HandleProgressAdded(object sender, DataAddedEventArgs eventArgs)
    {
      int index = eventArgs.Index;
      System.Management.Automation.PowerShell pipelinePowerShell = this.GetPipelinePowerShell(this.remotePipeline, eventArgs.PowerShellInstanceId);
      if (pipelinePowerShell == null)
        return;
      this.Progress.Add(pipelinePowerShell.Streams.Progress[index]);
    }

    protected void StopAggregateResultsFromHelper(ExecutionCmdletHelper helper)
    {
      Pipeline pipeline = helper.Pipeline;
      pipeline.Output.DataReady -= new EventHandler(this.HandleOutputReady);
      pipeline.Error.DataReady -= new EventHandler(this.HandleErrorReady);
      pipeline.StateChanged -= new EventHandler<PipelineStateEventArgs>(this.HandlePipelineStateChanged);
      pipeline.Dispose();
    }

    protected void RegisterThrottleComplete(ThrottleManager throttleManager) => throttleManager.ThrottleComplete += new EventHandler<EventArgs>(this.HandleThrottleComplete);

    protected void UnregisterThrottleComplete(ThrottleManager throttleManager) => throttleManager.ThrottleComplete -= new EventHandler<EventArgs>(this.HandleThrottleComplete);

    protected void DeterminedAndSetJobState(ExecutionCmdletHelper helper)
    {
      Exception failureException;
      this.ProcessJobFailure(helper, out failureException, out this.failureErrorRecord);
      if (failureException != null)
      {
        this.SetJobState(JobState.Failed, failureException);
      }
      else
      {
        switch (helper.Pipeline.PipelineStateInfo.State)
        {
          case PipelineState.NotStarted:
            this.SetJobState(JobState.Stopped);
            break;
          case PipelineState.Completed:
            this.SetJobState(JobState.Completed);
            break;
          default:
            this.SetJobState(JobState.Stopped);
            break;
        }
      }
    }

    internal void UnblockJob()
    {
      this.SetJobState(JobState.Running, (Exception) null);
      this.JobUnblocked((object) this, EventArgs.Empty);
    }

    internal virtual System.Management.Automation.PowerShell GetPowerShell(Guid instanceId) => throw PSRemotingChildJob.tracer.NewInvalidOperationException();

    internal event EventHandler JobUnblocked;
  }
}
