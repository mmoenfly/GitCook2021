// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.WaitJobCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using System.Timers;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Wait", "Job", DefaultParameterSetName = "SessionIdParameterSet")]
  public class WaitJobCommand : JobCmdletBase, IDisposable
  {
    private System.Management.Automation.Job[] jobs;
    private bool waitAny;
    private int timeoutInSeconds = -1;
    private bool isDisposed;
    private List<System.Management.Automation.Job> toWaitJobs = new List<System.Management.Automation.Job>();
    private System.Management.Automation.Job[] toWaitJobsArray;
    private RegisteredWaitHandle[] registeredWaitHandles;
    private System.Timers.Timer timer;
    private bool stopped;
    private bool timerExpired;
    private ManualResetEvent waitCompleted = new ManualResetEvent(false);
    private int waitCompletedArrayIndex = -1;
    private int waitInterval = 1000;
    private object syncObject = new object();

    [Parameter(Mandatory = true, ParameterSetName = "JobParameterSet", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public System.Management.Automation.Job[] Job
    {
      get => this.jobs;
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.jobs = value;
      }
    }

    [Parameter]
    public SwitchParameter Any
    {
      get
      {
        using (JobCmdletBase.tracer.TraceProperty())
          return (SwitchParameter) this.waitAny;
      }
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.waitAny = (bool) value;
      }
    }

    [ValidateRange(-1, 2147483647)]
    [Parameter]
    public int Timeout
    {
      get
      {
        using (JobCmdletBase.tracer.TraceProperty())
          return this.timeoutInSeconds;
      }
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.timeoutInSeconds = value;
      }
    }

    public override string[] Command
    {
      get => (string[]) null;
      set
      {
      }
    }

    protected override void BeginProcessing()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          if (this.timeoutInSeconds <= 0)
            return;
          this.timer = new System.Timers.Timer();
          this.timer.Interval = (double) (this.timeoutInSeconds * 1000);
          this.timer.Elapsed += new ElapsedEventHandler(this.HandleTimerElapsed);
          this.timer.AutoReset = false;
          this.timer.Start();
        }
      }
    }

    protected override void ProcessRecord()
    {
      using (JobCmdletBase.tracer.TraceMethod())
        this.toWaitJobs.AddRange(!this.ParameterSetName.Equals("NameParameterSet") ? (!this.ParameterSetName.Equals("InstanceIdParameterSet") ? (!this.ParameterSetName.Equals("SessionIdParameterSet") ? (!this.ParameterSetName.Equals("StateParameterSet") ? (IEnumerable<System.Management.Automation.Job>) this.CopyJobsToList(this.jobs, false, false) : (IEnumerable<System.Management.Automation.Job>) this.FindJobsMatchingByState(false)) : (IEnumerable<System.Management.Automation.Job>) this.FindJobsMatchingBySessionId(true, false, true, false)) : (IEnumerable<System.Management.Automation.Job>) this.FindJobsMatchingByInstanceId(true, false, true, false)) : (IEnumerable<System.Management.Automation.Job>) this.FindJobsMatchingByName(true, false, true, false));
    }

    protected override void EndProcessing()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        try
        {
          if (this.waitAny)
            this.ProcessWaitAny();
          else
            this.ProcessWaitAll();
        }
        finally
        {
          this.CleanUp();
        }
      }
    }

    protected override void StopProcessing()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        if (this.stopped)
          return;
        lock (this.syncObject)
        {
          if (this.stopped)
            return;
          this.stopped = true;
          if (this.timer != null)
            this.timer.Stop();
          this.waitCompleted.Set();
        }
      }
    }

    private void HandleTimerElapsed(object sender, ElapsedEventArgs arg)
    {
      using (JobCmdletBase.tracer.TraceEventHandlers())
      {
        lock (this.syncObject)
        {
          this.timerExpired = true;
          this.waitCompleted.Set();
        }
      }
    }

    private void ProcessWaitAny()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        if (this.toWaitJobs == null || this.toWaitJobs.Count == 0)
          return;
        if (this.timeoutInSeconds == 0)
        {
          foreach (System.Management.Automation.Job toWaitJob in this.toWaitJobs)
          {
            if (!this.stopped && !this.timerExpired && toWaitJob.IsFinishedState(toWaitJob.JobStateInfo.State))
            {
              this.WriteObject((object) toWaitJob);
              break;
            }
          }
        }
        else
        {
          this.toWaitJobsArray = this.toWaitJobs.ToArray();
          this.registeredWaitHandles = new RegisteredWaitHandle[this.toWaitJobsArray.GetLength(0)];
          for (int index = 0; index < this.toWaitJobsArray.GetLength(0); ++index)
            this.registeredWaitHandles[index] = ThreadPool.RegisterWaitForSingleObject(this.toWaitJobsArray[index].Finished, new WaitOrTimerCallback(this.WaitAnyCallback), (object) index, this.waitInterval, true);
          this.waitCompleted.WaitOne();
          if (this.waitCompletedArrayIndex < 0)
            return;
          this.WriteObject((object) this.toWaitJobsArray[this.waitCompletedArrayIndex]);
        }
      }
    }

    private void WaitAnyCallback(object state, bool timedOut)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        int index = (int) state;
        System.Management.Automation.Job toWaitJobs = this.toWaitJobsArray[index];
        lock (this.syncObject)
        {
          if (this.stopped || this.timerExpired || this.waitCompletedArrayIndex != -1)
            return;
          if (!toWaitJobs.IsFinishedState(toWaitJobs.JobStateInfo.State))
          {
            this.registeredWaitHandles[index].Unregister((WaitHandle) null);
            this.registeredWaitHandles[index] = ThreadPool.RegisterWaitForSingleObject(toWaitJobs.Finished, new WaitOrTimerCallback(this.WaitAnyCallback), (object) index, this.waitInterval, true);
          }
          else
          {
            this.waitCompletedArrayIndex = index;
            this.waitCompleted.Set();
          }
        }
      }
    }

    private void ProcessWaitAll()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        if (this.timeoutInSeconds == 0)
        {
          this.ProcessWaitAllNoWait();
        }
        else
        {
          foreach (System.Management.Automation.Job toWaitJob in this.toWaitJobs)
          {
            this.WaitForOneJob(toWaitJob);
            if (this.stopped || this.timerExpired)
              return;
          }
          this.ProcessWaitAllNoWait();
        }
      }
    }

    private void ProcessWaitAllNoWait()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        bool flag = true;
        foreach (System.Management.Automation.Job toWaitJob in this.toWaitJobs)
        {
          if (!toWaitJob.IsFinishedState(toWaitJob.JobStateInfo.State))
          {
            flag = false;
            break;
          }
        }
        if (!flag)
          return;
        foreach (object toWaitJob in this.toWaitJobs)
          this.WriteObject(toWaitJob);
      }
    }

    private void WaitForOneJob(System.Management.Automation.Job job)
    {
      using (JobCmdletBase.tracer.TraceMethod())
        WaitHandle.WaitAny(new WaitHandle[2]
        {
          job.Finished,
          (WaitHandle) this.waitCompleted
        });
    }

    private void CleanUp()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        if (this.registeredWaitHandles == null)
          return;
        for (int index = 0; index < this.registeredWaitHandles.GetLength(0); ++index)
        {
          RegisteredWaitHandle registeredWaitHandle = this.registeredWaitHandles[index];
          if (registeredWaitHandle != null)
          {
            registeredWaitHandle.Unregister((WaitHandle) null);
            this.registeredWaitHandles[index] = (RegisteredWaitHandle) null;
          }
        }
      }
    }

    public void Dispose()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        this.Dispose(true);
        GC.SuppressFinalize((object) this);
      }
    }

    private void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      lock (this.syncObject)
      {
        if (this.isDisposed)
          return;
        this.isDisposed = true;
        if (this.timer != null)
        {
          this.timer.Dispose();
          this.timer = (System.Timers.Timer) null;
        }
        if (this.waitCompleted == null)
          return;
        this.waitCompleted.Close();
        this.waitCompleted = (ManualResetEvent) null;
      }
    }
  }
}
