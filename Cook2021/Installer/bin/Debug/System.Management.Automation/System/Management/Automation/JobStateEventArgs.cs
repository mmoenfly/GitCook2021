// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.JobStateEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public sealed class JobStateEventArgs : EventArgs
  {
    private JobStateInfo _jobStateInfo;
    [TraceSource("PSJob", "Job APIs")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PSJob", "Job APIs");

    public JobStateEventArgs(JobStateInfo jobStateInfo)
    {
      using (JobStateEventArgs.tracer.TraceConstructor((object) this))
        this._jobStateInfo = jobStateInfo != null ? jobStateInfo : throw JobStateEventArgs.tracer.NewArgumentNullException(nameof (jobStateInfo));
    }

    public JobStateInfo JobStateInfo => this._jobStateInfo;
  }
}
