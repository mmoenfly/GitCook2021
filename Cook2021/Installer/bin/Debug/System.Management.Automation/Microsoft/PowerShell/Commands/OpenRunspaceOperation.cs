// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.OpenRunspaceOperation
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  internal class OpenRunspaceOperation : IThrottleOperation
  {
    [TraceSource("ORO", "OpenRunspaceOperation")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ORO", nameof (OpenRunspaceOperation));
    private RemoteRunspace runspace;

    internal RemoteRunspace OperatedRunspace => this.runspace;

    internal OpenRunspaceOperation(RemoteRunspace runspace)
    {
      this.runspace = runspace;
      this.runspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
    }

    internal override void StartOperation() => this.runspace.OpenAsync();

    internal override void StopOperation() => this.runspace.CloseAsync();

    internal override event EventHandler<OperationStateEventArgs> OperationComplete;

    private void HandleRunspaceStateChanged(object source, RunspaceStateEventArgs stateEventArgs)
    {
      switch (stateEventArgs.RunspaceStateInfo.State)
      {
        case RunspaceState.BeforeOpen:
          break;
        case RunspaceState.Opening:
          break;
        case RunspaceState.Closing:
          break;
        default:
          OperationStateEventArgs e = new OperationStateEventArgs();
          e.BaseEvent = (EventArgs) stateEventArgs;
          if (stateEventArgs.RunspaceStateInfo.State == RunspaceState.Opened)
          {
            e.OperationState = OperationState.StartComplete;
            OpenRunspaceOperation.tracer.WriteLine("Runspace opened for {0}", (object) this.runspace.InstanceId);
          }
          else
          {
            e.OperationState = OperationState.StopComplete;
            OpenRunspaceOperation.tracer.WriteLine("Runspace closed for {0}", (object) this.runspace.InstanceId);
          }
          this.OperationComplete((object) this, e);
          break;
      }
    }
  }
}
