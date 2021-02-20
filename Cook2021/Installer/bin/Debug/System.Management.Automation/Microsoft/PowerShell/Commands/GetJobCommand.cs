// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.GetJobCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Get", "Job", DefaultParameterSetName = "SessionIdParameterSet")]
  public class GetJobCommand : JobCmdletBase
  {
    [Parameter(ParameterSetName = "SessionIdParameterSet", Position = 0, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public override int[] Id
    {
      get => base.Id;
      set => base.Id = value;
    }

    protected override void ProcessRecord()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        if (this.ParameterSetName.Equals("NameParameterSet"))
          this.FindJobsMatchingByName(true, true, true, false);
        else if (this.ParameterSetName.Equals("InstanceIdParameterSet"))
          this.FindJobsMatchingByInstanceId(true, true, true, false);
        else if (this.ParameterSetName.Equals("SessionIdParameterSet"))
        {
          if (this.Id != null)
            this.FindJobsMatchingBySessionId(true, true, true, false);
          else
            this.WriteObject((object) this.JobRepository.Jobs, true);
        }
        else if (this.ParameterSetName.Equals("CommandParameterSet"))
        {
          this.FindJobsMatchingByCommand(true);
        }
        else
        {
          if (!this.ParameterSetName.Equals("StateParameterSet"))
            return;
          this.FindJobsMatchingByState(true);
        }
      }
    }
  }
}
