// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSRemotingJob
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;
using System.Collections.Generic;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Text;

namespace System.Management.Automation
{
  internal class PSRemotingJob : Job
  {
    [TraceSource("PSJob", "Job APIs")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PSJob", "Job APIs");
    private bool moreData = true;
    private bool _stopIsCalled;
    private string statusMessage;
    private bool hideComputerName = true;
    private bool atleastOneChildJobFailed;
    private int finishedChildJobsCount;
    private int blockedChildJobsCount;
    private bool isDisposed;
    private ThrottleManager throttleManager = new ThrottleManager();
    private object syncObject = new object();

    internal PSRemotingJob(
      string[] computerNames,
      List<IThrottleOperation> computerNameHelpers,
      string remoteCommand,
      string name)
      : this(computerNames, computerNameHelpers, remoteCommand, 0, name)
    {
    }

    internal PSRemotingJob(
      PSSession[] remoteRunspaceInfos,
      List<IThrottleOperation> runspaceHelpers,
      string remoteCommand,
      string name)
      : this(remoteRunspaceInfos, runspaceHelpers, remoteCommand, 0, name)
    {
    }

    internal PSRemotingJob(
      string[] computerNames,
      List<IThrottleOperation> computerNameHelpers,
      string remoteCommand,
      int throttleLimit,
      string name)
      : base(remoteCommand, name)
    {
      using (PSRemotingJob.tracer.TraceConstructor((object) this))
      {
        foreach (ExecutionCmdletHelperComputerName computerNameHelper in computerNameHelpers)
        {
          PSRemotingChildJob remotingChildJob = new PSRemotingChildJob(remoteCommand, (ExecutionCmdletHelper) computerNameHelper, this.throttleManager);
          remotingChildJob.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleChildJobStateChanged);
          remotingChildJob.JobUnblocked += new EventHandler(this.HandleJobUnblocked);
          this.ChildJobs.Add((Job) remotingChildJob);
        }
        this.CommonInit(throttleLimit, computerNameHelpers);
      }
    }

    internal PSRemotingJob(
      PSSession[] remoteRunspaceInfos,
      List<IThrottleOperation> runspaceHelpers,
      string remoteCommand,
      int throttleLimit,
      string name)
      : base(remoteCommand, name)
    {
      using (PSRemotingJob.tracer.TraceConstructor((object) this))
      {
        for (int index = 0; index < remoteRunspaceInfos.Length; ++index)
        {
          ExecutionCmdletHelperRunspace runspaceHelper = (ExecutionCmdletHelperRunspace) runspaceHelpers[index];
          PSRemotingChildJob remotingChildJob = new PSRemotingChildJob(remoteCommand, (ExecutionCmdletHelper) runspaceHelper, this.throttleManager);
          remotingChildJob.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleChildJobStateChanged);
          remotingChildJob.JobUnblocked += new EventHandler(this.HandleJobUnblocked);
          this.ChildJobs.Add((Job) remotingChildJob);
        }
        this.CommonInit(throttleLimit, runspaceHelpers);
      }
    }

    protected PSRemotingJob()
    {
    }

    private void CommonInit(int throttleLimit, List<IThrottleOperation> helpers)
    {
      using (PSRemotingJob.tracer.TraceMethod())
      {
        this.CloseAllStreams();
        this.SetJobState(JobState.Running);
        this.throttleManager.ThrottleLimit = throttleLimit;
        this.throttleManager.SubmitOperations(helpers);
        this.throttleManager.EndSubmitOperations();
      }
    }

    internal List<Job> GetJobsForComputer(string computerName)
    {
      using (PSRemotingJob.tracer.TraceMethod())
      {
        List<Job> jobList = new List<Job>();
        foreach (Job childJob in (IEnumerable<Job>) this.ChildJobs)
        {
          PSRemotingChildJob remotingChildJob = childJob as PSRemotingChildJob;
          if (childJob != null && string.Equals(remotingChildJob.Runspace.ConnectionInfo.ComputerName, computerName, StringComparison.OrdinalIgnoreCase))
            jobList.Add((Job) remotingChildJob);
        }
        return jobList;
      }
    }

    internal List<Job> GetJobsForRunspace(PSSession runspace)
    {
      using (PSRemotingJob.tracer.TraceMethod())
      {
        List<Job> jobList = new List<Job>();
        foreach (Job childJob in (IEnumerable<Job>) this.ChildJobs)
        {
          PSRemotingChildJob remotingChildJob = childJob as PSRemotingChildJob;
          if (childJob != null && remotingChildJob.Runspace.InstanceId.Equals(runspace.InstanceId))
            jobList.Add((Job) remotingChildJob);
        }
        return jobList;
      }
    }

    internal List<Job> GetJobsForOperation(IThrottleOperation operation)
    {
      using (PSRemotingJob.tracer.TraceMethod())
      {
        List<Job> jobList = new List<Job>();
        ExecutionCmdletHelper executionCmdletHelper = operation as ExecutionCmdletHelper;
        foreach (Job childJob in (IEnumerable<Job>) this.ChildJobs)
        {
          PSRemotingChildJob remotingChildJob = childJob as PSRemotingChildJob;
          if (childJob != null && remotingChildJob.Helper.Equals((object) executionCmdletHelper))
            jobList.Add((Job) remotingChildJob);
        }
        return jobList;
      }
    }

    public override bool HasMoreData
    {
      get
      {
        using (PSRemotingJob.tracer.TraceProperty())
        {
          if (this.moreData && this.IsFinishedState(this.JobStateInfo.State))
          {
            bool flag = false;
            for (int index = 0; index < this.ChildJobs.Count; ++index)
            {
              if (this.ChildJobs[index].HasMoreData)
              {
                flag = true;
                break;
              }
            }
            this.moreData = flag;
          }
          return this.moreData;
        }
      }
    }

    public override void StopJob()
    {
      using (PSRemotingJob.tracer.TraceMethod())
      {
        if (this.isDisposed || this._stopIsCalled || this.IsFinishedState(this.JobStateInfo.State))
          return;
        lock (this.syncObject)
        {
          if (this.isDisposed || this._stopIsCalled || this.IsFinishedState(this.JobStateInfo.State))
            return;
          this._stopIsCalled = true;
        }
        this.throttleManager.StopAllOperations();
        this.Finished.WaitOne();
      }
    }

    public override string StatusMessage => this.statusMessage;

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

    private void SetStatusMessage()
    {
      using (PSRemotingJob.tracer.TraceMethod())
        this.statusMessage = "test";
    }

    private void HandleChildJobStateChanged(object sender, JobStateEventArgs e)
    {
      using (PSRemotingJob.tracer.TraceMethod())
      {
        if (e.JobStateInfo.State == JobState.Blocked)
        {
          lock (this.syncObject)
            ++this.blockedChildJobsCount;
          this.SetJobState(JobState.Blocked, (Exception) null);
        }
        else
        {
          if (!this.IsFinishedState(e.JobStateInfo.State))
            return;
          if (e.JobStateInfo.State == JobState.Failed)
            this.atleastOneChildJobFailed = true;
          bool flag = false;
          lock (this.syncObject)
          {
            ++this.finishedChildJobsCount;
            if (this.finishedChildJobsCount == this.ChildJobs.Count)
              flag = true;
          }
          if (!flag)
            return;
          if (this.atleastOneChildJobFailed)
            this.SetJobState(JobState.Failed);
          else if (this._stopIsCalled)
            this.SetJobState(JobState.Stopped);
          else
            this.SetJobState(JobState.Completed);
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      using (PSRemotingJob.tracer.TraceMethod())
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
          if (!this.IsFinishedState(this.JobStateInfo.State))
            this.StopJob();
          foreach (Job childJob in (IEnumerable<Job>) this.ChildJobs)
            childJob.Dispose();
          this.throttleManager.Dispose();
        }
        finally
        {
          base.Dispose(disposing);
        }
      }
    }

    private string ConstructLocation()
    {
      using (PSRemotingJob.tracer.TraceMethod())
      {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (PSRemotingChildJob childJob in (IEnumerable<Job>) this.ChildJobs)
        {
          stringBuilder.Append(childJob.Location);
          stringBuilder.Append(",");
        }
        stringBuilder.Remove(stringBuilder.Length - 1, 1);
        return stringBuilder.ToString();
      }
    }

    public override string Location => this.ConstructLocation();

    private void HandleJobUnblocked(object sender, EventArgs eventArgs)
    {
      bool flag = false;
      lock (this.syncObject)
      {
        --this.blockedChildJobsCount;
        if (this.blockedChildJobsCount == 0)
          flag = true;
      }
      if (!flag)
        return;
      this.SetJobState(JobState.Running, (Exception) null);
    }
  }
}
