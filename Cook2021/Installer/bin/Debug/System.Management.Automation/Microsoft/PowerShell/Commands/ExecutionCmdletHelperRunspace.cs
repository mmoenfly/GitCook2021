// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ExecutionCmdletHelperRunspace
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  internal class ExecutionCmdletHelperRunspace : ExecutionCmdletHelper
  {
    internal ExecutionCmdletHelperRunspace(Pipeline pipeline)
    {
      this.pipeline = pipeline;
      this.pipeline.StateChanged += new EventHandler<PipelineStateEventArgs>(this.HandlePipelineStateChanged);
    }

    internal override void StartOperation()
    {
      try
      {
        this.pipeline.InvokeAsync();
      }
      catch (InvalidRunspaceStateException ex)
      {
        this.internalException = (Exception) ex;
        this.RaiseOperationCompleteEvent();
      }
      catch (InvalidPipelineStateException ex)
      {
        this.internalException = (Exception) ex;
        this.RaiseOperationCompleteEvent();
      }
      catch (InvalidOperationException ex)
      {
        this.internalException = (Exception) ex;
        this.RaiseOperationCompleteEvent();
      }
    }

    internal override void StopOperation()
    {
      if (this.pipeline.PipelineStateInfo.State == PipelineState.Running || this.pipeline.PipelineStateInfo.State == PipelineState.NotStarted)
        this.pipeline.StopAsync();
      else
        this.RaiseOperationCompleteEvent();
    }

    internal override event EventHandler<OperationStateEventArgs> OperationComplete;

    private void HandlePipelineStateChanged(object sender, PipelineStateEventArgs stateEventArgs)
    {
      switch (stateEventArgs.PipelineStateInfo.State)
      {
        case PipelineState.NotStarted:
          break;
        case PipelineState.Running:
          break;
        case PipelineState.Stopping:
          break;
        default:
          this.RaiseOperationCompleteEvent((EventArgs) stateEventArgs);
          break;
      }
    }

    private void RaiseOperationCompleteEvent() => this.RaiseOperationCompleteEvent((EventArgs) null);

    private void RaiseOperationCompleteEvent(EventArgs baseEventArgs)
    {
      this.pipeline.Dispose();
      OperationStateEventArgs e = new OperationStateEventArgs();
      e.OperationState = OperationState.StopComplete;
      e.BaseEvent = baseEventArgs;
      if (this.OperationComplete == null)
        return;
      this.OperationComplete((object) this, e);
    }
  }
}
