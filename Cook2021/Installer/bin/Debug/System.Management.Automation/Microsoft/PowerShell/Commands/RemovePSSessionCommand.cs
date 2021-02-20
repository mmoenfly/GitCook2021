// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.RemovePSSessionCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Remove", "PSSession", DefaultParameterSetName = "Id", SupportsShouldProcess = true)]
  public class RemovePSSessionCommand : PSRunspaceCmdlet
  {
    [TraceSource("RemovePSSessionCommand", "RemovePSSessionCommand")]
    internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer(nameof (RemovePSSessionCommand), nameof (RemovePSSessionCommand));
    private PSSession[] remoteRunspaceInfos;

    [Parameter(Mandatory = true, ParameterSetName = "Session", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public PSSession[] Session
    {
      get => this.remoteRunspaceInfos;
      set
      {
        using (RemovePSSessionCommand.tracer.TraceProperty((object) value))
          this.remoteRunspaceInfos = value;
      }
    }

    protected override void ProcessRecord()
    {
      using (RemovePSSessionCommand.tracer.TraceMethod())
      {
        ICollection<PSSession> psSessions;
        switch (this.ParameterSetName)
        {
          case "ComputerName":
          case "Name":
          case "InstanceId":
          case "Id":
            psSessions = (ICollection<PSSession>) this.GetMatchingRunspaces(false, true).Values;
            break;
          case "Session":
            psSessions = (ICollection<PSSession>) this.remoteRunspaceInfos;
            break;
          default:
            psSessions = (ICollection<PSSession>) new Collection<PSSession>();
            break;
        }
        foreach (PSSession psSession in (IEnumerable<PSSession>) psSessions)
        {
          RemoteRunspace runspace = (RemoteRunspace) psSession.Runspace;
          if (this.ShouldProcess(runspace.ConnectionInfo.ComputerName, "Remove"))
          {
            try
            {
              runspace.Dispose();
            }
            catch (PSRemotingTransportException ex)
            {
            }
            try
            {
              this.RunspaceRepository.Remove(psSession);
            }
            catch (ArgumentException ex)
            {
            }
          }
        }
      }
    }
  }
}
