// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ReceiveJobCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Receive", "Job", DefaultParameterSetName = "Location")]
  public class ReceiveJobCommand : JobCmdletBase
  {
    protected const string LocationParameterSet = "Location";
    private System.Management.Automation.Job[] jobs;
    private string[] computerNames;
    private string[] locations;
    private PSSession[] remoteRunspaceInfos;
    private bool flush = true;
    private bool recurse = true;

    [Parameter(Mandatory = true, ParameterSetName = "ComputerName", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [Parameter(Mandatory = true, ParameterSetName = "Location", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [Parameter(Mandatory = true, ParameterSetName = "Session", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public System.Management.Automation.Job[] Job
    {
      get => this.jobs;
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.jobs = value;
      }
    }

    [ValidateNotNullOrEmpty]
    [Alias(new string[] {"Cn"})]
    [Parameter(ParameterSetName = "ComputerName", Position = 1, ValueFromPipelineByPropertyName = true)]
    public string[] ComputerName
    {
      get => this.computerNames;
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.computerNames = value;
      }
    }

    [Parameter(ParameterSetName = "Location", Position = 1, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public string[] Location
    {
      get => this.locations;
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.locations = value;
      }
    }

    [Parameter(ParameterSetName = "Session", Position = 1, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNull]
    public PSSession[] Session
    {
      get => this.remoteRunspaceInfos;
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.remoteRunspaceInfos = value;
      }
    }

    [Parameter]
    public SwitchParameter Keep
    {
      get => (SwitchParameter) !this.flush;
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.flush = !(bool) value;
      }
    }

    [Parameter]
    public SwitchParameter NoRecurse
    {
      get => (SwitchParameter) !this.recurse;
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.recurse = !(bool) value;
      }
    }

    public override JobState State => JobState.NotStarted;

    public override string[] Command => (string[]) null;

    protected override void ProcessRecord()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        switch (this.ParameterSetName)
        {
          case "Session":
            foreach (System.Management.Automation.Job job in this.jobs)
            {
              if (!(job is PSRemotingJob psRemotingJob))
              {
                this.WriteError(new ErrorRecord((Exception) new ArgumentException(this.GetMessage(PSRemotingErrorId.RunspaceParamNotSupported)), "RunspaceParameterNotSupported", ErrorCategory.InvalidArgument, (object) job));
              }
              else
              {
                foreach (PSSession remoteRunspaceInfo in this.remoteRunspaceInfos)
                  this.WriteResultsForJobsInCollection(psRemotingJob.GetJobsForRunspace(remoteRunspaceInfo), false);
              }
            }
            break;
          case "ComputerName":
            foreach (System.Management.Automation.Job job in this.jobs)
            {
              if (!(job is PSRemotingJob psRemotingJob))
              {
                this.WriteError(new ErrorRecord((Exception) new ArgumentException(this.GetMessage(PSRemotingErrorId.ComputerNameParamNotSupported)), "ComputerNameParameterNotSupported", ErrorCategory.InvalidArgument, (object) job));
              }
              else
              {
                string[] resolvedComputerNames = (string[]) null;
                this.ResolveComputerNames(this.computerNames, out resolvedComputerNames);
                foreach (string computerName in resolvedComputerNames)
                  this.WriteResultsForJobsInCollection(psRemotingJob.GetJobsForComputer(computerName), false);
              }
            }
            break;
          case "Location":
            if (this.locations == null)
            {
              this.WriteAll();
              break;
            }
            foreach (System.Management.Automation.Job job in this.jobs)
            {
              foreach (string location in this.locations)
                this.WriteResultsForJobsInCollection(job.GetJobsForLocation(location), false);
            }
            break;
          case "InstanceIdParameterSet":
            this.WriteResultsForJobsInCollection(this.FindJobsMatchingByInstanceId(true, false, true, false), true);
            break;
          case "SessionIdParameterSet":
            this.WriteResultsForJobsInCollection(this.FindJobsMatchingBySessionId(true, false, true, false), true);
            break;
          case "NameParameterSet":
            this.WriteResultsForJobsInCollection(this.FindJobsMatchingByName(true, false, true, false), true);
            break;
        }
      }
    }

    private void WriteJobResults(System.Management.Automation.Job job)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        if (job == null)
          return;
        if (job.JobStateInfo.State == JobState.Blocked)
        {
          if (job is PSRemotingChildJob remotingChildJob)
            remotingChildJob.UnblockJob();
          else
            job.SetJobState(JobState.Running, (Exception) null);
        }
        foreach (PSStreamObject psstreamObject in this.ReadAll(job.Results))
          this.WriteStreamObject(psstreamObject);
        if (job.JobStateInfo.State != JobState.Failed)
          return;
        if (job is PSRemotingChildJob remotingChildJob && remotingChildJob.FailureErrorRecord != null)
        {
          this.WriteError(remotingChildJob.FailureErrorRecord);
        }
        else
        {
          if (job.JobStateInfo.Reason == null)
            return;
          this.WriteError(new ErrorRecord(job.JobStateInfo.Reason, "JobStateFailed", ErrorCategory.InvalidResult, (object) null));
        }
      }
    }

    private Collection<PSStreamObject> ReadAll(
      PSDataCollection<PSStreamObject> psDataCollection)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        if (this.flush)
          return psDataCollection.ReadAll();
        PSStreamObject[] array = new PSStreamObject[psDataCollection.Count];
        psDataCollection.CopyTo(array, 0);
        Collection<PSStreamObject> collection = new Collection<PSStreamObject>();
        for (int index = 0; index < array.Length; ++index)
          collection.Add(array[index]);
        return collection;
      }
    }

    private void WriteJobResultsRecursivelyHelper(Hashtable duplicate, System.Management.Automation.Job job)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        if (duplicate.ContainsKey((object) job))
          return;
        duplicate.Add((object) job, (object) job);
        this.WriteJobResults(job);
        foreach (System.Management.Automation.Job childJob in (IEnumerable<System.Management.Automation.Job>) job.ChildJobs)
          this.WriteJobResultsRecursivelyHelper(duplicate, childJob);
      }
    }

    private void WriteJobResultsRecursively(System.Management.Automation.Job job)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        Hashtable duplicate = new Hashtable();
        this.WriteJobResultsRecursivelyHelper(duplicate, job);
        duplicate.Clear();
      }
    }

    private void WriteAll()
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        foreach (System.Management.Automation.Job job in this.jobs)
        {
          if (this.recurse)
            this.WriteJobResultsRecursively(job);
          else
            this.WriteJobResults(job);
        }
      }
    }

    private void WriteResultsForJobsInCollection(List<System.Management.Automation.Job> jobs, bool checkForRecurse)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        foreach (System.Management.Automation.Job job in jobs)
        {
          if (checkForRecurse && this.recurse)
            this.WriteJobResultsRecursively(job);
          else
            this.WriteJobResults(job);
        }
      }
    }
  }
}
