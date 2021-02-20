// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SteppablePipeline
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  public sealed class SteppablePipeline : IDisposable
  {
    private PipelineProcessor _pipeline;
    private ExecutionContext _context;
    private bool _expectInput;
    private bool disposed;

    internal SteppablePipeline(ExecutionContext context, PipelineProcessor pipeline)
    {
      if (pipeline == null)
        throw new ArgumentNullException(nameof (pipeline));
      if (context == null)
        throw new ArgumentNullException(nameof (context));
      this._pipeline = pipeline;
      this._context = context;
    }

    public void Begin(bool expectInput) => this.Begin(expectInput, (ICommandRuntime) null);

    public void Begin(bool expectInput, EngineIntrinsics contextToRedirectTo)
    {
      if (contextToRedirectTo == null)
        throw new ArgumentNullException(nameof (contextToRedirectTo));
      CommandProcessorBase commandProcessor = contextToRedirectTo.SessionState.Internal.ExecutionContext.CurrentCommandProcessor;
      ICommandRuntime commandRuntime = commandProcessor == null ? (ICommandRuntime) null : (ICommandRuntime) commandProcessor.CommandRuntime;
      this.Begin(expectInput, commandRuntime);
    }

    public void Begin(InternalCommand command)
    {
      if (command == null || command.MyInvocation == null)
        throw new ArgumentNullException(nameof (command));
      this.Begin(command.MyInvocation.ExpectingInput, command.commandRuntime);
    }

    private void Begin(bool expectInput, ICommandRuntime commandRuntime)
    {
      try
      {
        this._pipeline.ExecutionScope = this._context.EngineSessionState.CurrentScope;
        this._context.PushPipelineProcessor(this._pipeline);
        this._expectInput = expectInput;
        if (commandRuntime is MshCommandRuntime mshCommandRuntime)
        {
          if (mshCommandRuntime.OutputPipe != null)
            this._pipeline.LinkPipelineSuccessOutput(mshCommandRuntime.OutputPipe);
          if (mshCommandRuntime.ErrorOutputPipe != null)
            this._pipeline.LinkPipelineErrorOutput(mshCommandRuntime.ErrorOutputPipe);
        }
        this._pipeline.StartStepping(this._expectInput);
      }
      finally
      {
        this._context.PopPipelineProcessor();
      }
    }

    public Array Process(object input)
    {
      try
      {
        this._context.PushPipelineProcessor(this._pipeline);
        return this._expectInput ? this._pipeline.Step(input) : this._pipeline.Step((object) AutomationNull.Value);
      }
      finally
      {
        this._context.PopPipelineProcessor();
      }
    }

    public Array Process(PSObject input)
    {
      try
      {
        this._context.PushPipelineProcessor(this._pipeline);
        return this._expectInput ? this._pipeline.Step((object) input) : this._pipeline.Step((object) AutomationNull.Value);
      }
      finally
      {
        this._context.PopPipelineProcessor();
      }
    }

    public Array Process()
    {
      try
      {
        this._context.PushPipelineProcessor(this._pipeline);
        return this._pipeline.Step((object) AutomationNull.Value);
      }
      finally
      {
        this._context.PopPipelineProcessor();
      }
    }

    public Array End()
    {
      try
      {
        this._context.PushPipelineProcessor(this._pipeline);
        return this._pipeline.DoComplete();
      }
      finally
      {
        this._context.PopPipelineProcessor();
        this._pipeline.Dispose();
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      if (disposing)
        this._pipeline.Dispose();
      this.disposed = true;
    }

    ~SteppablePipeline() => this.Dispose(false);
  }
}
