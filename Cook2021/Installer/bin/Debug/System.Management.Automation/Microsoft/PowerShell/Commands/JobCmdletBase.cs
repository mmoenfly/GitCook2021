// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.JobCmdletBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Remoting;

namespace Microsoft.PowerShell.Commands
{
  public class JobCmdletBase : PSRemotingCmdlet
  {
    internal const string JobParameterSet = "JobParameterSet";
    internal const string InstanceIdParameterSet = "InstanceIdParameterSet";
    internal const string SessionIdParameterSet = "SessionIdParameterSet";
    internal const string NameParameterSet = "NameParameterSet";
    internal const string StateParameterSet = "StateParameterSet";
    internal const string CommandParameterSet = "CommandParameterSet";
    internal const string JobParameter = "Job";
    internal const string InstanceIdParameter = "InstanceId";
    internal const string SessionIdParameter = "SessionId";
    internal const string NameParameter = "Name";
    internal const string StateParameter = "State";
    internal const string CommandParameter = "Command";
    [TraceSource("JobCmdlets", "Job related cmdlets")]
    internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer("JobCmdlets", "Job related cmdlets");
    private string[] names;
    private Guid[] instanceIds;
    private int[] sessionIds;
    private JobState jobstate;
    private string[] commands;

    internal List<Job> FindJobsMatchingByName(
      bool recurse,
      bool writeobject,
      bool writeErrorOnNoMatch,
      bool checkIfJobCanBeRemoved)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        List<Job> matches = new List<Job>();
        Hashtable duplicateDetector = new Hashtable();
        if (this.names == null)
          return matches;
        List<Job> jobs = this.JobRepository.Jobs;
        foreach (string name in this.names)
        {
          if (!string.IsNullOrEmpty(name))
          {
            duplicateDetector.Clear();
            if (!this.FindJobsMatchingByNameHelper(matches, (IList<Job>) this.JobRepository.Jobs, name, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved) && writeErrorOnNoMatch && !WildcardPattern.ContainsWildcardCharacters(name))
              this.WriteError(new ErrorRecord((Exception) JobCmdletBase.tracer.NewArgumentException("Name", "RemotingErrorIdStrings", PSRemotingErrorId.JobWithSpecifiedNameNotFound.ToString(), (object) name), "JobWithSpecifiedNameNotFound", ErrorCategory.ObjectNotFound, (object) name));
          }
        }
        return matches;
      }
    }

    private bool FindJobsMatchingByNameHelper(
      List<Job> matches,
      IList<Job> jobsToSearch,
      string name,
      Hashtable duplicateDetector,
      bool recurse,
      bool writeobject,
      bool checkIfJobCanBeRemoved)
    {
      bool flag = false;
      WildcardPattern wildcardPattern = new WildcardPattern(name, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
      foreach (Job job in (IEnumerable<Job>) jobsToSearch)
      {
        if (!duplicateDetector.ContainsKey((object) job.Id))
        {
          duplicateDetector.Add((object) job.Id, (object) job.Id);
          if (wildcardPattern.IsMatch(job.Name))
          {
            flag = true;
            if (checkIfJobCanBeRemoved)
            {
              if (!this.CheckJobCanBeRemoved(job, "Name", PSRemotingErrorId.JobWithSpecifiedNameNotCompleted, (object) job.Id, (object) job.Name))
                goto label_9;
            }
            if (writeobject)
              this.WriteObject((object) job);
            else
              matches.Add(job);
          }
label_9:
          if (job.ChildJobs != null && job.ChildJobs.Count > 0 && (recurse && this.FindJobsMatchingByNameHelper(matches, job.ChildJobs, name, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved)))
            flag = true;
        }
      }
      return flag;
    }

    internal List<Job> FindJobsMatchingByInstanceId(
      bool recurse,
      bool writeobject,
      bool writeErrorOnNoMatch,
      bool checkIfJobCanBeRemoved)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        List<Job> matches = new List<Job>();
        Hashtable duplicateDetector = new Hashtable();
        if (this.instanceIds == null)
          return matches;
        foreach (Guid instanceId in this.instanceIds)
        {
          if (!this.FindJobsMatchingByInstanceIdHelper(matches, (IList<Job>) this.JobRepository.Jobs, instanceId, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved) && writeErrorOnNoMatch)
            this.WriteError(new ErrorRecord((Exception) JobCmdletBase.tracer.NewArgumentException("InstanceId", "RemotingErrorIdStrings", PSRemotingErrorId.JobWithSpecifiedInstanceIdNotFound.ToString(), (object) instanceId), "JobWithSpecifiedInstanceIdNotFound", ErrorCategory.ObjectNotFound, (object) instanceId));
        }
        return matches;
      }
    }

    private bool FindJobsMatchingByInstanceIdHelper(
      List<Job> matches,
      IList<Job> jobsToSearch,
      Guid instanceId,
      Hashtable duplicateDetector,
      bool recurse,
      bool writeobject,
      bool checkIfJobCanBeRemoved)
    {
      bool flag = false;
      foreach (Job job in (IEnumerable<Job>) jobsToSearch)
      {
        if (!duplicateDetector.ContainsKey((object) job.Id))
        {
          duplicateDetector.Add((object) job.Id, (object) job.Id);
          if (job.InstanceId == instanceId)
          {
            flag = true;
            if (checkIfJobCanBeRemoved)
            {
              if (!this.CheckJobCanBeRemoved(job, "InstanceId", PSRemotingErrorId.JobWithSpecifiedInstanceIdNotCompleted, (object) job.Id, (object) job.InstanceId))
                continue;
            }
            if (writeobject)
            {
              this.WriteObject((object) job);
              break;
            }
            matches.Add(job);
            break;
          }
        }
      }
      if (!flag && recurse)
      {
        foreach (Job job in (IEnumerable<Job>) jobsToSearch)
        {
          if (job.ChildJobs != null && job.ChildJobs.Count > 0)
          {
            flag = this.FindJobsMatchingByInstanceIdHelper(matches, job.ChildJobs, instanceId, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved);
            if (flag)
              break;
          }
        }
      }
      return flag;
    }

    internal List<Job> FindJobsMatchingBySessionId(
      bool recurse,
      bool writeobject,
      bool writeErrorOnNoMatch,
      bool checkIfJobCanBeRemoved)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        List<Job> matches = new List<Job>();
        if (this.sessionIds == null)
          return matches;
        Hashtable duplicateDetector = new Hashtable();
        foreach (int sessionId in this.sessionIds)
        {
          if (!this.FindJobsMatchingBySessionIdHelper(matches, (IList<Job>) this.JobRepository.Jobs, sessionId, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved) && writeErrorOnNoMatch)
            this.WriteError(new ErrorRecord((Exception) JobCmdletBase.tracer.NewArgumentException("SessionId", "RemotingErrorIdStrings", PSRemotingErrorId.JobWithSpecifiedSessionIdNotFound.ToString(), (object) sessionId), "JobWithSpecifiedSessionNotFound", ErrorCategory.ObjectNotFound, (object) sessionId));
        }
        return matches;
      }
    }

    private bool FindJobsMatchingBySessionIdHelper(
      List<Job> matches,
      IList<Job> jobsToSearch,
      int sessionId,
      Hashtable duplicateDetector,
      bool recurse,
      bool writeobject,
      bool checkIfJobCanBeRemoved)
    {
      bool flag = false;
      foreach (Job job in (IEnumerable<Job>) jobsToSearch)
      {
        if (job.Id == sessionId)
        {
          flag = true;
          if (checkIfJobCanBeRemoved)
          {
            if (!this.CheckJobCanBeRemoved(job, "SessionId", PSRemotingErrorId.JobWithSpecifiedSessionIdNotCompleted, (object) job.Id))
              continue;
          }
          if (writeobject)
          {
            this.WriteObject((object) job);
            break;
          }
          matches.Add(job);
          break;
        }
      }
      if (!flag && recurse)
      {
        foreach (Job job in (IEnumerable<Job>) jobsToSearch)
        {
          if (job.ChildJobs != null && job.ChildJobs.Count > 0)
          {
            flag = this.FindJobsMatchingBySessionIdHelper(matches, job.ChildJobs, sessionId, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved);
            if (flag)
              break;
          }
        }
      }
      return flag;
    }

    internal List<Job> FindJobsMatchingByCommand(bool writeobject)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        List<Job> jobList = new List<Job>();
        if (this.commands == null)
          return jobList;
        List<Job> jobs = this.JobRepository.Jobs;
        foreach (string command in this.commands)
        {
          foreach (Job job in jobs)
          {
            if (job.Command.Equals(command, StringComparison.OrdinalIgnoreCase))
            {
              if (writeobject)
                this.WriteObject((object) job);
              else
                jobList.Add(job);
            }
          }
        }
        return jobList;
      }
    }

    internal List<Job> FindJobsMatchingByState(bool writeobject)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        List<Job> jobList = new List<Job>();
        foreach (Job job in this.JobRepository.Jobs)
        {
          if (job.JobStateInfo.State == this.jobstate)
          {
            if (writeobject)
              this.WriteObject((object) job);
            else
              jobList.Add(job);
          }
        }
        return jobList;
      }
    }

    internal List<Job> CopyJobsToList(
      Job[] jobs,
      bool writeobject,
      bool checkIfJobCanBeRemoved)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        List<Job> jobList = new List<Job>();
        if (jobs == null)
          return jobList;
        foreach (Job job in jobs)
        {
          if (checkIfJobCanBeRemoved)
          {
            if (!this.CheckJobCanBeRemoved(job, "Job", PSRemotingErrorId.JobWithSpecifiedSessionIdNotCompleted, (object) job.Id))
              continue;
          }
          if (writeobject)
            this.WriteObject((object) job);
          else
            jobList.Add(job);
        }
        return jobList;
      }
    }

    private bool CheckJobCanBeRemoved(
      Job job,
      string parameterName,
      PSRemotingErrorId id,
      params object[] list)
    {
      using (JobCmdletBase.tracer.TraceMethod())
      {
        if (job.IsFinishedState(job.JobStateInfo.State))
          return true;
        this.WriteError(new ErrorRecord((Exception) new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(id, list), parameterName), "JobObjectNotFinishedCannotBeRemoved", ErrorCategory.InvalidOperation, (object) job));
        return false;
      }
    }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "NameParameterSet", Position = 0, ValueFromPipelineByPropertyName = true)]
    public string[] Name
    {
      get => this.names;
      set => this.names = value;
    }

    [Parameter(ParameterSetName = "InstanceIdParameterSet", Position = 0, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public Guid[] InstanceId
    {
      get => this.instanceIds;
      set => this.instanceIds = value;
    }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, ParameterSetName = "SessionIdParameterSet", Position = 0, ValueFromPipelineByPropertyName = true)]
    public virtual int[] Id
    {
      get => this.sessionIds;
      set => this.sessionIds = value;
    }

    [Parameter(ParameterSetName = "StateParameterSet", ValueFromPipelineByPropertyName = true)]
    public virtual JobState State
    {
      get => this.jobstate;
      set => this.jobstate = value;
    }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "CommandParameterSet", ValueFromPipelineByPropertyName = true)]
    public virtual string[] Command
    {
      get => this.commands;
      set
      {
        using (JobCmdletBase.tracer.TraceProperty((object) value))
          this.commands = value;
      }
    }

    protected override void BeginProcessing()
    {
    }
  }
}
