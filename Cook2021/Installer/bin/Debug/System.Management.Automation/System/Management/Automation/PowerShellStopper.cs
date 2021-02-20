// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PowerShellStopper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal class PowerShellStopper : IDisposable
  {
    private PipelineBase pipeline;
    private PowerShell powerShell;
    private EventHandler<PipelineStateEventArgs> eventHandler;
    private bool isDisposed;

    internal PowerShellStopper(ExecutionContext context, PowerShell powerShell)
    {
      if (context == null)
        throw new ArgumentNullException(nameof (context));
      this.powerShell = powerShell != null ? powerShell : throw new ArgumentNullException(nameof (powerShell));
      if (context.CurrentCommandProcessor == null || context.CurrentCommandProcessor.CommandRuntime == null || (context.CurrentCommandProcessor.CommandRuntime.PipelineProcessor == null || context.CurrentCommandProcessor.CommandRuntime.PipelineProcessor.LocalPipeline == null))
        return;
      this.eventHandler = new EventHandler<PipelineStateEventArgs>(this.LocalPipeline_StateChanged);
      this.pipeline = (PipelineBase) context.CurrentCommandProcessor.CommandRuntime.PipelineProcessor.LocalPipeline;
      this.pipeline.StateChanged += this.eventHandler;
    }

    private void LocalPipeline_StateChanged(object sender, PipelineStateEventArgs e)
    {
      if (e.PipelineStateInfo.State != PipelineState.Stopping || this.powerShell.InvocationStateInfo.State != PSInvocationState.Running)
        return;
      this.powerShell.Stop();
    }

    public void Dispose()
    {
      if (this.isDisposed)
        return;
      if (this.eventHandler != null)
      {
        this.pipeline.StateChanged -= this.eventHandler;
        this.eventHandler = (EventHandler<PipelineStateEventArgs>) null;
      }
      GC.SuppressFinalize((object) this);
      this.isDisposed = true;
    }
  }
}
