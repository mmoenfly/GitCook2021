// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.RemoveJobCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Remoting;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Remove", "Job", DefaultParameterSetName = "SessionIdParameterSet", SupportsShouldProcess = true)]
  public class RemoveJobCommand : JobCmdletBase
  {
    private System.Management.Automation.Job[] jobs;
    private bool force;

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

    [Parameter(ParameterSetName = "InstanceIdParameterSet")]
    [Parameter(ParameterSetName = "JobParameterSet")]
    [Parameter(ParameterSetName = "SessionIdParameterSet")]
    [Parameter(ParameterSetName = "NameParameterSet")]
    public SwitchParameter Force
    {
      get => (SwitchParameter) this.force;
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.force = (bool) value;
      }
    }

    protected override void ProcessRecord()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        List<System.Management.Automation.Job> jobList;
        switch (this.ParameterSetName)
        {
          case "NameParameterSet":
            jobList = this.FindJobsMatchingByName(false, false, true, !this.force);
            break;
          case "InstanceIdParameterSet":
            jobList = this.FindJobsMatchingByInstanceId(false, false, true, !this.force);
            break;
          case "SessionIdParameterSet":
            jobList = this.FindJobsMatchingBySessionId(false, false, true, !this.force);
            break;
          case "StateParameterSet":
            jobList = this.FindJobsMatchingByState(false);
            break;
          default:
            jobList = this.CopyJobsToList(this.jobs, false, !this.force);
            break;
        }
        foreach (System.Management.Automation.Job job in jobList)
        {
          if (this.ShouldProcess(this.GetMessage(PSRemotingErrorId.StopPSJobWhatIfTarget, (object) job.Command, (object) job.Id), "Uninstall"))
          {
            if (!job.IsFinishedState(job.JobStateInfo.State))
              job.StopJob();
            try
            {
              this.JobRepository.Remove(job);
              job.Dispose();
            }
            catch (ArgumentException ex)
            {
              this.WriteError(new ErrorRecord((Exception) new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.CannotRemoveJob), (Exception) ex), "CannotRemoveJob", ErrorCategory.InvalidOperation, (object) job));
            }
          }
        }
      }
    }
  }
}
