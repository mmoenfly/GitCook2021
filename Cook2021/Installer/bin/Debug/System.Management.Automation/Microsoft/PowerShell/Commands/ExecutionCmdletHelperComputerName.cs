// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ExecutionCmdletHelperComputerName
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  internal class ExecutionCmdletHelperComputerName : ExecutionCmdletHelper
  {
    private RemoteRunspace remoteRunspace;

    internal RemoteRunspace RemoteRunspace => this.remoteRunspace;

    internal ExecutionCmdletHelperComputerName(RemoteRunspace remoteRunspace, Pipeline pipeline)
    {
      this.remoteRunspace = remoteRunspace;
      remoteRunspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
      this.pipeline = pipeline;
      pipeline.StateChanged += new EventHandler<PipelineStateEventArgs>(this.HandlePipelineStateChanged);
    }

    internal override void StartOperation()
    {
      try
      {
        this.remoteRunspace.OpenAsync();
      }
      catch (PSRemotingTransportException ex)
      {
        this.internalException = (Exception) ex;
        this.RaiseOperationCompleteEvent();
      }
    }

    internal override void StopOperation()
    {
      bool flag = false;
      if (this.pipeline.PipelineStateInfo.State == PipelineState.Running || this.pipeline.PipelineStateInfo.State == PipelineState.NotStarted)
        flag = true;
      if (flag)
        this.pipeline.StopAsync();
      else
        this.RaiseOperationCompleteEvent();
    }

    internal override event EventHandler<OperationStateEventArgs> OperationComplete;

    private void HandleRunspaceStateChanged(object sender, RunspaceStateEventArgs stateEventArgs)
    {
      switch (stateEventArgs.RunspaceStateInfo.State)
      {
        case RunspaceState.Opened:
          try
          {
            this.pipeline.InvokeAsync();
            break;
          }
          catch (InvalidPipelineStateException ex)
          {
            this.remoteRunspace.CloseAsync();
            break;
          }
          catch (InvalidRunspaceStateException ex)
          {
            this.internalException = (Exception) ex;
            this.remoteRunspace.CloseAsync();
            break;
          }
        case RunspaceState.Closed:
          if (stateEventArgs.RunspaceStateInfo.Reason != null)
          {
            this.RaiseOperationCompleteEvent((EventArgs) stateEventArgs);
            break;
          }
          this.RaiseOperationCompleteEvent();
          break;
        case RunspaceState.Broken:
          this.RaiseOperationCompleteEvent((EventArgs) stateEventArgs);
          break;
      }
    }

    private void HandlePipelineStateChanged(object sender, PipelineStateEventArgs stateEventArgs)
    {
      switch (stateEventArgs.PipelineStateInfo.State)
      {
        case PipelineState.Stopped:
        case PipelineState.Completed:
        case PipelineState.Failed:
          this.remoteRunspace.CloseAsync();
          break;
      }
    }

    private void RaiseOperationCompleteEvent() => this.RaiseOperationCompleteEvent((EventArgs) null);

    private void RaiseOperationCompleteEvent(EventArgs baseEventArgs)
    {
      this.pipeline.Dispose();
      this.remoteRunspace.Dispose();
      this.OperationComplete((object) this, new OperationStateEventArgs()
      {
        OperationState = OperationState.StopComplete,
        BaseEvent = baseEventArgs
      });
    }
  }
}
