// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSInvokeExpressionSyncJob
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal class PSInvokeExpressionSyncJob : PSRemotingChildJob
  {
    private List<ExecutionCmdletHelper> helpers = new List<ExecutionCmdletHelper>();
    private ThrottleManager throttleManager;
    private Dictionary<Guid, System.Management.Automation.PowerShell> powershells = new Dictionary<Guid, System.Management.Automation.PowerShell>();
    private bool cleanupDone;
    private bool doFinishCalled;

    internal PSInvokeExpressionSyncJob(
      List<IThrottleOperation> operations,
      ThrottleManager throttleManager)
    {
      this.Results.AddRef();
      this.throttleManager = throttleManager;
      this.RegisterThrottleComplete(this.throttleManager);
      foreach (IThrottleOperation operation in operations)
      {
        ExecutionCmdletHelper helper = operation as ExecutionCmdletHelper;
        if (helper.Pipeline.Runspace is RemoteRunspace runspace && runspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen)
        {
          runspace.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(((PSRemotingChildJob) this).HandleURIDirectionReported);
          runspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
        }
        this.helpers.Add(helper);
        this.AggregateResultsFromHelper(helper);
        RemotePipeline pipeline = helper.Pipeline as RemotePipeline;
        this.powershells.Add(pipeline.PowerShell.InstanceId, pipeline.PowerShell);
      }
      throttleManager.SubmitOperations(operations);
      throttleManager.EndSubmitOperations();
    }

    protected override void DoCleanupOnFinished()
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
      foreach (ExecutionCmdletHelper helper in this.helpers)
        this.StopAggregateResultsFromHelper(helper);
      this.UnregisterThrottleComplete(this.throttleManager);
      this.Results.DecrementRef();
    }

    protected override void Dispose(bool disposing) => base.Dispose(disposing);

    protected override void HandleOperationComplete(
      object sender,
      OperationStateEventArgs stateEventArgs)
    {
      Exception failureException;
      ErrorRecord failureErrorRecord;
      this.ProcessJobFailure(sender as ExecutionCmdletHelper, out failureException, out failureErrorRecord);
      if (failureException == null)
        return;
      this.Results.Add(new PSStreamObject(PSStreamObjectType.Error, (object) failureErrorRecord));
    }

    public override void StopJob() => this.throttleManager.StopAllOperations();

    protected override void DoFinish()
    {
      if (this.doFinishCalled)
        return;
      lock (this.syncObject)
      {
        if (this.doFinishCalled)
          return;
        this.doFinishCalled = true;
      }
      foreach (ExecutionCmdletHelper helper in this.helpers)
        this.DeterminedAndSetJobState(helper);
      if (this.helpers.Count == 0 && this.JobStateInfo.State == JobState.NotStarted)
        this.SetJobState(JobState.Completed);
      this.DoCleanupOnFinished();
    }

    internal override System.Management.Automation.PowerShell GetPowerShell(
      Guid instanceId)
    {
      System.Management.Automation.PowerShell powerShell = (System.Management.Automation.PowerShell) null;
      this.powershells.TryGetValue(instanceId, out powerShell);
      return powerShell;
    }

    private void HandleRunspaceStateChanged(object sender, RunspaceStateEventArgs e)
    {
      if (!(sender is RemoteRunspace remoteRunspace) || e.RunspaceStateInfo.State == RunspaceState.Opening)
        return;
      remoteRunspace.URIRedirectionReported -= new EventHandler<RemoteDataEventArgs<Uri>>(((PSRemotingChildJob) this).HandleURIDirectionReported);
      remoteRunspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
    }

    internal Collection<PSStreamObject> ReadAll()
    {
      this.Output.Clear();
      this.Error.Clear();
      this.Debug.Clear();
      this.Warning.Clear();
      this.Verbose.Clear();
      this.Progress.Clear();
      return this.Results.ReadAll();
    }

    internal bool IsTerminalState() => this.IsFinishedState(this.JobStateInfo.State);
  }
}
