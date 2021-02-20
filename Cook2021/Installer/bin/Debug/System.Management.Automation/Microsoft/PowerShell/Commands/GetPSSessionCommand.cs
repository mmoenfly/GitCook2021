// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.GetPSSessionCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Get", "PSSession", DefaultParameterSetName = "ComputerName")]
  public class GetPSSessionCommand : PSRunspaceCmdlet
  {
    [TraceSource("GetPSSession", "GetPSSessionCommand")]
    internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer("GetPSSession", nameof (GetPSSessionCommand));

    protected override void ProcessRecord()
    {
      using (GetPSSessionCommand.tracer.TraceMethod())
        this.GetMatchingRunspaces(true, true);
    }
  }
}
