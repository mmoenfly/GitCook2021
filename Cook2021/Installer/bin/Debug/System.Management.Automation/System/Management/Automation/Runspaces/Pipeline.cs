// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.Pipeline
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
  public abstract class Pipeline : IDisposable
  {
    private long _pipelineId;
    private CommandCollection _commands;
    private bool _setPipelineSessionState = true;
    private PSInvocationSettings _invocationSettings;
    private bool _redirectShellErrorOutputPipe;
    [TraceSource("Pipeline", "Pipeline")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (Pipeline), nameof (Pipeline));

    internal Pipeline(Runspace runspace)
      : this(runspace, new CommandCollection())
    {
      using (Pipeline._trace.TraceConstructor((object) this))
        ;
    }

    internal Pipeline(Runspace runspace, CommandCollection command)
    {
      using (Pipeline._trace.TraceConstructor((object) this))
      {
        if (runspace == null)
          Pipeline._trace.NewArgumentNullException(nameof (runspace));
        this._pipelineId = runspace.GeneratePipelineId();
        this._commands = command;
      }
    }

    public abstract Runspace Runspace { get; }

    public abstract bool IsNested { get; }

    public abstract PipelineWriter Input { get; }

    public abstract PipelineReader<PSObject> Output { get; }

    public abstract PipelineReader<object> Error { get; }

    public abstract PipelineStateInfo PipelineStateInfo { get; }

    public long InstanceId
    {
      get
      {
        using (Pipeline._trace.TraceProperty())
          return this._pipelineId;
      }
    }

    public CommandCollection Commands => this._commands;

    public bool SetPipelineSessionState
    {
      get
      {
        using (Pipeline._trace.TraceProperty())
          return this._setPipelineSessionState;
      }
      set
      {
        using (Pipeline._trace.TraceProperty())
          this._setPipelineSessionState = value;
      }
    }

    internal PSInvocationSettings InvocationSettings
    {
      get
      {
        using (Pipeline._trace.TraceProperty())
          return this._invocationSettings;
      }
      set
      {
        using (Pipeline._trace.TraceProperty())
          this._invocationSettings = value;
      }
    }

    internal bool RedirectShellErrorOutputPipe
    {
      get => this._redirectShellErrorOutputPipe;
      set => this._redirectShellErrorOutputPipe = value;
    }

    public abstract event EventHandler<PipelineStateEventArgs> StateChanged;

    public Collection<PSObject> Invoke()
    {
      using (Pipeline._trace.TraceMethod())
        return this.Invoke((IEnumerable) null);
    }

    public abstract Collection<PSObject> Invoke(IEnumerable input);

    public abstract void InvokeAsync();

    public abstract void Stop();

    public abstract void StopAsync();

    public abstract Pipeline Copy();

    internal void SetCommandCollection(CommandCollection commands) => this._commands = commands;

    internal abstract void SetHistoryString(string historyString);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      using (Pipeline._trace.TraceDispose((object) this))
        ;
    }
  }
}
