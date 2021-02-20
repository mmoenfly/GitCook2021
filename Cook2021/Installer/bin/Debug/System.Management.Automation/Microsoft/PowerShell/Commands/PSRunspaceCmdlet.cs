// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.PSRunspaceCmdlet
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  public abstract class PSRunspaceCmdlet : PSRemotingCmdlet
  {
    protected const string InstanceIdParameterSet = "InstanceId";
    protected const string IdParameterSet = "Id";
    protected const string NameParameterSet = "Name";
    private Guid[] remoteRunspaceIds;
    private int[] sessionIds;
    private string[] names;
    private string[] computerNames;

    [ValidateNotNull]
    [Parameter(ParameterSetName = "InstanceId", ValueFromPipelineByPropertyName = true)]
    public Guid[] InstanceId
    {
      get => this.remoteRunspaceIds;
      set => this.remoteRunspaceIds = value;
    }

    [Parameter(Mandatory = true, ParameterSetName = "Id", Position = 0, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNull]
    public int[] Id
    {
      get => this.sessionIds;
      set => this.sessionIds = value;
    }

    [Parameter(ParameterSetName = "Name", ValueFromPipelineByPropertyName = true)]
    public string[] Name
    {
      get => this.names;
      set => this.names = value;
    }

    [Alias(new string[] {"Cn"})]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "ComputerName", Position = 0, ValueFromPipelineByPropertyName = true)]
    public string[] ComputerName
    {
      get => this.computerNames;
      set => this.computerNames = value;
    }

    protected Dictionary<Guid, PSSession> GetMatchingRunspaces(
      bool writeobject,
      bool writeErrorOnNoMatch)
    {
      switch (this.ParameterSetName)
      {
        case "ComputerName":
          return this.GetMatchingRunspacesByComputerName(writeobject, writeErrorOnNoMatch);
        case "InstanceId":
          return this.GetMatchingRunspacesByRunspaceId(writeobject, writeErrorOnNoMatch);
        case "Name":
          return this.GetMatchingRunspacesByName(writeobject, writeErrorOnNoMatch);
        case "Id":
          return this.GetMatchingRunspacesBySessionId(writeobject, writeErrorOnNoMatch);
        default:
          return (Dictionary<Guid, PSSession>) null;
      }
    }

    private Dictionary<Guid, PSSession> GetMatchingRunspacesByComputerName(
      bool writeobject,
      bool writeErrorOnNoMatch)
    {
      Dictionary<Guid, PSSession> dictionary = new Dictionary<Guid, PSSession>();
      List<PSSession> runspaces = this.RunspaceRepository.Runspaces;
      if (this.computerNames == null || this.computerNames.Length == 0)
      {
        foreach (PSSession psSession in runspaces)
        {
          if (writeobject)
            this.WriteObject((object) psSession);
          else
            dictionary.Add(psSession.InstanceId, psSession);
        }
        return dictionary;
      }
      foreach (string computerName in this.computerNames)
      {
        WildcardPattern wildcardPattern = new WildcardPattern(computerName, WildcardOptions.IgnoreCase);
        bool flag = false;
        foreach (PSSession psSession in runspaces)
        {
          if (wildcardPattern.IsMatch(psSession.ComputerName))
          {
            flag = true;
            if (writeobject)
            {
              this.WriteObject((object) psSession);
            }
            else
            {
              try
              {
                dictionary.Add(psSession.InstanceId, psSession);
              }
              catch (ArgumentException ex)
              {
              }
            }
          }
        }
        if (!flag && writeErrorOnNoMatch)
          this.WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedComputer, (object) computerName);
      }
      return dictionary;
    }

    private Dictionary<Guid, PSSession> GetMatchingRunspacesByName(
      bool writeobject,
      bool writeErrorOnNoMatch)
    {
      Dictionary<Guid, PSSession> dictionary = new Dictionary<Guid, PSSession>();
      List<PSSession> runspaces = this.RunspaceRepository.Runspaces;
      foreach (string name in this.names)
      {
        WildcardPattern wildcardPattern = new WildcardPattern(name, WildcardOptions.IgnoreCase);
        bool flag = false;
        foreach (PSSession psSession in runspaces)
        {
          if (wildcardPattern.IsMatch(psSession.Name))
          {
            flag = true;
            if (writeobject)
            {
              this.WriteObject((object) psSession);
            }
            else
            {
              try
              {
                dictionary.Add(psSession.InstanceId, psSession);
              }
              catch (ArgumentException ex)
              {
              }
            }
          }
        }
        if (!flag && writeErrorOnNoMatch && !WildcardPattern.ContainsWildcardCharacters(name))
          this.WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedName, (object) name);
      }
      return dictionary;
    }

    private Dictionary<Guid, PSSession> GetMatchingRunspacesByRunspaceId(
      bool writeobject,
      bool writeErrorOnNoMatch)
    {
      Dictionary<Guid, PSSession> dictionary = new Dictionary<Guid, PSSession>();
      List<PSSession> runspaces = this.RunspaceRepository.Runspaces;
      foreach (Guid remoteRunspaceId in this.remoteRunspaceIds)
      {
        bool flag = false;
        foreach (PSSession psSession in runspaces)
        {
          if (remoteRunspaceId.Equals(psSession.InstanceId))
          {
            flag = true;
            if (writeobject)
            {
              this.WriteObject((object) psSession);
            }
            else
            {
              try
              {
                dictionary.Add(psSession.InstanceId, psSession);
              }
              catch (ArgumentException ex)
              {
              }
            }
          }
        }
        if (!flag && writeErrorOnNoMatch)
          this.WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedRunspaceId, (object) remoteRunspaceId);
      }
      return dictionary;
    }

    private Dictionary<Guid, PSSession> GetMatchingRunspacesBySessionId(
      bool writeobject,
      bool writeErrorOnNoMatch)
    {
      Dictionary<Guid, PSSession> dictionary = new Dictionary<Guid, PSSession>();
      List<PSSession> runspaces = this.RunspaceRepository.Runspaces;
      foreach (int sessionId in this.sessionIds)
      {
        bool flag = false;
        foreach (PSSession psSession in runspaces)
        {
          if (sessionId == psSession.Id)
          {
            flag = true;
            if (writeobject)
            {
              this.WriteObject((object) psSession);
            }
            else
            {
              try
              {
                dictionary.Add(psSession.InstanceId, psSession);
              }
              catch (ArgumentException ex)
              {
              }
            }
          }
        }
        if (!flag && writeErrorOnNoMatch)
          this.WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedSessionId, (object) sessionId);
      }
      return dictionary;
    }

    private void WriteInvalidArgumentError(PSRemotingErrorId errorId, object errorArgument) => this.WriteError(new ErrorRecord((Exception) new ArgumentException(this.GetMessage(errorId, errorArgument)), errorId.ToString(), ErrorCategory.InvalidArgument, errorArgument));
  }
}
