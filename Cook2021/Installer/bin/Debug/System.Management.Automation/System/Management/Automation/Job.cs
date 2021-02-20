// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Job
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Remoting;
using System.Threading;

namespace System.Management.Automation
{
  public abstract class Job : IDisposable
  {
    [TraceSource("PSJob", "Job APIs")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PSJob", "Job APIs");
    private string command;
    private JobStateInfo stateInfo = new JobStateInfo(JobState.NotStarted);
    private ManualResetEvent finished = new ManualResetEvent(false);
    private Guid guid = Guid.NewGuid();
    private int sessionId;
    private string name;
    private IList<Job> childJobs;
    private object syncObject = new object();
    private PSDataCollection<PSStreamObject> results = new PSDataCollection<PSStreamObject>();
    private PSDataCollection<ErrorRecord> error = new PSDataCollection<ErrorRecord>();
    private PSDataCollection<ProgressRecord> progress = new PSDataCollection<ProgressRecord>();
    private PSDataCollection<VerboseRecord> verbose = new PSDataCollection<VerboseRecord>();
    private PSDataCollection<WarningRecord> warning = new PSDataCollection<WarningRecord>();
    private PSDataCollection<DebugRecord> debug = new PSDataCollection<DebugRecord>();
    private PSDataCollection<PSObject> output = new PSDataCollection<PSObject>();
    private static int _jobIdSeed = 0;
    private bool isDisposed;

    protected Job()
    {
    }

    protected Job(string command)
    {
      using (Job.tracer.TraceConstructor((object) this))
      {
        this.command = command;
        this.SetJobState(JobState.NotStarted);
        this.sessionId = Interlocked.Increment(ref Job._jobIdSeed);
        this.name = this.AutoGenerateJobName();
      }
    }

    protected Job(string command, string name)
      : this(command)
    {
      using (Job.tracer.TraceConstructor((object) this))
      {
        if (string.IsNullOrEmpty(name))
          return;
        this.name = name;
      }
    }

    protected Job(string command, string name, IList<Job> childJobs)
      : this(command, name)
    {
      using (Job.tracer.TraceConstructor((object) this))
        this.childJobs = childJobs;
    }

    public string Command => this.command;

    public JobStateInfo JobStateInfo => this.stateInfo;

    public WaitHandle Finished => (WaitHandle) this.finished;

    public Guid InstanceId => this.guid;

    public int Id => this.sessionId;

    public string Name
    {
      get => this.name;
      set
      {
        this.AssertNotDisposed();
        this.name = value;
      }
    }

    public IList<Job> ChildJobs
    {
      get
      {
        if (this.childJobs == null)
        {
          lock (this.syncObject)
          {
            if (this.childJobs == null)
              this.childJobs = (IList<Job>) new List<Job>();
          }
        }
        return this.childJobs;
      }
    }

    public abstract string StatusMessage { get; }

    public abstract bool HasMoreData { get; }

    internal PSDataCollection<PSStreamObject> Results
    {
      get => this.results;
      set
      {
        using (Job.tracer.TraceProperty())
        {
          if (value == null)
            throw Job.tracer.NewArgumentNullException(nameof (Results));
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.results = value;
          }
        }
      }
    }

    public PSDataCollection<PSObject> Output
    {
      get => this.output;
      set
      {
        using (Job.tracer.TraceProperty())
        {
          if (value == null)
            throw Job.tracer.NewArgumentNullException(nameof (Output));
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.output = value;
          }
        }
      }
    }

    public PSDataCollection<ErrorRecord> Error
    {
      get => this.error;
      set
      {
        using (Job.tracer.TraceProperty())
        {
          if (value == null)
            throw Job.tracer.NewArgumentNullException(nameof (Error));
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.error = value;
          }
        }
      }
    }

    public PSDataCollection<ProgressRecord> Progress
    {
      get => this.progress;
      set
      {
        using (Job.tracer.TraceProperty())
        {
          if (value == null)
            throw Job.tracer.NewArgumentNullException(nameof (Progress));
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.progress = value;
          }
        }
      }
    }

    public PSDataCollection<VerboseRecord> Verbose
    {
      get => this.verbose;
      set
      {
        using (Job.tracer.TraceProperty())
        {
          if (value == null)
            throw Job.tracer.NewArgumentNullException(nameof (Verbose));
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.verbose = value;
          }
        }
      }
    }

    public PSDataCollection<DebugRecord> Debug
    {
      get => this.debug;
      set
      {
        using (Job.tracer.TraceProperty())
        {
          if (value == null)
            throw Job.tracer.NewArgumentNullException(nameof (Debug));
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.debug = value;
          }
        }
      }
    }

    public PSDataCollection<WarningRecord> Warning
    {
      get => this.warning;
      set
      {
        using (Job.tracer.TraceProperty())
        {
          if (value == null)
            throw Job.tracer.NewArgumentNullException(nameof (Warning));
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.warning = value;
          }
        }
      }
    }

    public abstract string Location { get; }

    public event EventHandler<JobStateEventArgs> StateChanged;

    protected void SetJobState(JobState state)
    {
      using (Job.tracer.TraceMethod((object) this))
      {
        this.AssertNotDisposed();
        this.SetJobState(state, (Exception) null);
      }
    }

    internal void SetJobState(JobState state, Exception reason)
    {
      using (Job.tracer.TraceMethod((object) this))
      {
        this.AssertNotDisposed();
        bool flag = false;
        lock (this.syncObject)
        {
          this.stateInfo = new JobStateInfo(state, reason);
          if (this.IsFinishedState(state))
          {
            flag = true;
            this.finished.Set();
          }
        }
        if (flag)
          this.CloseAllStreams();
        try
        {
          if (this.StateChanged == null)
            return;
          this.StateChanged((object) this, new JobStateEventArgs(this.stateInfo.Clone()));
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          Job.tracer.TraceException(ex);
        }
      }
    }

    public abstract void StopJob();

    internal bool IsFinishedState(JobState state) => state == JobState.Completed || state == JobState.Failed || state == JobState.Stopped;

    private void AssertChangesAreAccepted()
    {
      this.AssertNotDisposed();
      if (this.JobStateInfo.State == JobState.Running)
        throw new InvalidJobStateException(JobState.Running);
    }

    protected string AutoGenerateJobName() => nameof (Job) + this.sessionId.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo);

    internal void AssertNotDisposed()
    {
      if (this.isDisposed)
        throw Job.tracer.NewObjectDisposedException("PSJob");
    }

    internal void CloseAllStreams()
    {
      using (Job.tracer.TraceMethod())
      {
        this.results.Complete();
        this.output.Complete();
        this.error.Complete();
        this.progress.Complete();
        this.verbose.Complete();
        this.warning.Complete();
      }
    }

    internal List<Job> GetJobsForLocation(string location)
    {
      List<Job> jobList = new List<Job>();
      foreach (Job childJob in (IEnumerable<Job>) this.ChildJobs)
      {
        if (string.Equals(childJob.Location, location, StringComparison.OrdinalIgnoreCase))
          jobList.Add(childJob);
      }
      return jobList;
    }

    public void Dispose()
    {
      using (Job.tracer.TraceMethod())
      {
        this.Dispose(true);
        GC.SuppressFinalize((object) this);
      }
    }

    protected virtual void Dispose(bool disposing)
    {
      using (Job.tracer.TraceMethod())
      {
        if (!disposing || this.isDisposed)
          return;
        this.CloseAllStreams();
        if (this.finished != null)
        {
          this.finished.Close();
          this.finished = (ManualResetEvent) null;
        }
        this.results.Dispose();
        this.output.Dispose();
        this.error.Dispose();
        this.debug.Dispose();
        this.verbose.Dispose();
        this.warning.Dispose();
        this.progress.Dispose();
        this.isDisposed = true;
      }
    }
  }
}
