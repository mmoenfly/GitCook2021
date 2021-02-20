// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.StopJobCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation;
using System.Management.Automation.Remoting;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Stop", "Job", DefaultParameterSetName = "SessionIdParameterSet", SupportsShouldProcess = true)]
  public class StopJobCommand : JobCmdletBase
  {
    private System.Management.Automation.Job[] jobs;
    private bool passThru;

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, ParameterSetName = "JobParameterSet", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
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
    public SwitchParameter PassThru
    {
      get
      {
        using (JobCmdletBase.tracer.TraceProperty())
          return (SwitchParameter) this.passThru;
      }
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.passThru = (bool) value;
      }
    }

    public override string[] Command => (string[]) null;

    protected override void ProcessRecord()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        foreach (System.Management.Automation.Job job in !this.ParameterSetName.Equals("NameParameterSet") ? (!this.ParameterSetName.Equals("InstanceIdParameterSet") ? (!this.ParameterSetName.Equals("SessionIdParameterSet") ? (!this.ParameterSetName.Equals("StateParameterSet") ? this.CopyJobsToList(this.jobs, false, false) : this.FindJobsMatchingByState(false)) : this.FindJobsMatchingBySessionId(true, false, true, false)) : this.FindJobsMatchingByInstanceId(true, false, true, false)) : this.FindJobsMatchingByName(true, false, true, false))
        {
          if (this.Stopping)
            break;
          if (job.IsFinishedState(job.JobStateInfo.State))
          {
            if (this.passThru)
              this.WriteObject((object) job);
          }
          else if (this.ShouldProcess(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.RemovePSJobWhatIfTarget, (object) job.Command, (object) job.Id), "Stop"))
          {
            job.StopJob();
            if (this.passThru)
              this.WriteObject((object) job);
          }
        }
      }
    }
  }
}
