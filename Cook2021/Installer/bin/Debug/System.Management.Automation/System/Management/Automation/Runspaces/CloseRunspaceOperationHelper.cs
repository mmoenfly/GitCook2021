// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.CloseRunspaceOperationHelper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting;

namespace System.Management.Automation.Runspaces
{
  internal sealed class CloseRunspaceOperationHelper : IThrottleOperation
  {
    private RemoteRunspace remoteRunspace;

    internal CloseRunspaceOperationHelper(RemoteRunspace remoteRunspace)
    {
      this.remoteRunspace = remoteRunspace;
      this.remoteRunspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
    }

    private void HandleRunspaceStateChanged(object sender, RunspaceStateEventArgs eventArgs)
    {
      switch (eventArgs.RunspaceStateInfo.State)
      {
        case RunspaceState.BeforeOpen:
          break;
        case RunspaceState.Opening:
          break;
        case RunspaceState.Opened:
          break;
        case RunspaceState.Closing:
          break;
        default:
          this.RaiseOperationCompleteEvent();
          break;
      }
    }

    internal override void StartOperation()
    {
      if (this.remoteRunspace.RunspaceStateInfo.State == RunspaceState.Closed || this.remoteRunspace.RunspaceStateInfo.State == RunspaceState.Broken)
        this.RaiseOperationCompleteEvent();
      else
        this.remoteRunspace.CloseAsync();
    }

    internal override void StopOperation()
    {
    }

    internal override event EventHandler<OperationStateEventArgs> OperationComplete;

    private void RaiseOperationCompleteEvent() => this.OperationComplete((object) this, new OperationStateEventArgs()
    {
      OperationState = OperationState.StopComplete,
      BaseEvent = EventArgs.Empty
    });
  }
}
