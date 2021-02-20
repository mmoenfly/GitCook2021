// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.JobStateInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public sealed class JobStateInfo
  {
    private JobState _state;
    private Exception _reason;
    [TraceSource("PSJob", "Job APIs")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PSJob", "Job APIs");

    public JobStateInfo(JobState state)
      : this(state, (Exception) null)
    {
    }

    public JobStateInfo(JobState state, Exception reason)
    {
      using (JobStateInfo.tracer.TraceConstructor((object) this))
      {
        this._state = state;
        this._reason = reason;
      }
    }

    internal JobStateInfo(JobStateInfo jobStateInfo)
    {
      using (JobStateInfo.tracer.TraceConstructor((object) this))
      {
        this._state = jobStateInfo.State;
        this._reason = jobStateInfo.Reason;
      }
    }

    public JobState State
    {
      get
      {
        using (JobStateInfo.tracer.TraceProperty())
          return this._state;
      }
    }

    public Exception Reason
    {
      get
      {
        using (JobStateInfo.tracer.TraceProperty())
          return this._reason;
      }
    }

    public override string ToString()
    {
      using (JobStateInfo.tracer.TraceMethod())
        return this._state.ToString();
    }

    internal JobStateInfo Clone()
    {
      using (JobStateInfo.tracer.TraceMethod())
        return new JobStateInfo(this);
    }
  }
}
