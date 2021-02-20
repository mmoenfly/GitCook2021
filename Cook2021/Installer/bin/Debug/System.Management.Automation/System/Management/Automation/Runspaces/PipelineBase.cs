// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PipelineBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
  internal abstract class PipelineBase : Pipeline
  {
    private Runspace _runspace;
    private bool _isNested;
    private PipelineStateInfo _pipelineStateInfo = new PipelineStateInfo(PipelineState.NotStarted);
    private bool _syncInvokeCall;
    private bool _performNestedCheck = true;
    private Thread _nestedPipelineExecutionThread;
    private Queue<PipelineBase.ExecutionEventQueueItem> _executionEventQueue = new Queue<PipelineBase.ExecutionEventQueueItem>();
    private ManualResetEvent _pipelineFinishedEvent;
    private ObjectStreamBase _outputStream;
    private ObjectStreamBase _errorStream;
    private PSInformationalBuffers _informationalBuffers;
    private ObjectStreamBase _inputStream;
    private bool _addToHistory;
    private string _historyString;
    private object _syncRoot = new object();
    private static readonly string[] _emptyStringArray = new string[0];
    [TraceSource("PipelineBase", "PipelineBase")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (PipelineBase), nameof (PipelineBase));
    private bool _disposed;

    protected PipelineBase(Runspace runspace, string command, bool addToHistory, bool isNested)
      : base(runspace)
    {
      using (PipelineBase._trace.TraceConstructor((object) this))
      {
        this.Initialize(runspace, command, addToHistory, isNested);
        this._inputStream = (ObjectStreamBase) new ObjectStream();
        this._outputStream = (ObjectStreamBase) new ObjectStream();
        this._errorStream = (ObjectStreamBase) new ObjectStream();
      }
    }

    protected PipelineBase(
      Runspace runspace,
      CommandCollection command,
      bool addToHistory,
      bool isNested,
      ObjectStreamBase inputStream,
      ObjectStreamBase outputStream,
      ObjectStreamBase errorStream,
      PSInformationalBuffers infoBuffers)
      : base(runspace, command)
    {
      using (PipelineBase._trace.TraceConstructor((object) this))
      {
        this.Initialize(runspace, (string) null, false, isNested);
        if (addToHistory)
        {
          this._historyString = command.GetCommandStringForHistory();
          this._addToHistory = addToHistory;
        }
        this._inputStream = inputStream;
        this._outputStream = outputStream;
        this._errorStream = errorStream;
        this._informationalBuffers = infoBuffers;
      }
    }

    protected PipelineBase(PipelineBase pipeline)
      : this(pipeline.Runspace, (string) null, false, pipeline.IsNested)
    {
      using (PipelineBase._trace.TraceConstructor((object) this))
      {
        if (pipeline == null)
          throw PipelineBase._trace.NewArgumentNullException(nameof (pipeline));
        this._addToHistory = !pipeline._disposed ? pipeline._addToHistory : throw PipelineBase._trace.NewObjectDisposedException(nameof (pipeline));
        this._historyString = pipeline._historyString;
        foreach (Command command in (Collection<Command>) pipeline.Commands)
          this.Commands.Add(command.Clone());
      }
    }

    public override Runspace Runspace
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._runspace;
      }
    }

    internal Runspace GetRunspace()
    {
      using (PipelineBase._trace.TraceMethod())
        return this._runspace;
    }

    public override bool IsNested
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._isNested;
      }
    }

    public override PipelineStateInfo PipelineStateInfo
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
        {
          lock (this.SyncRoot)
            return this._pipelineStateInfo.Clone();
        }
      }
    }

    public override PipelineWriter Input
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._inputStream.ObjectWriter;
      }
    }

    public override PipelineReader<PSObject> Output
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._outputStream.PSObjectReader;
      }
    }

    public override PipelineReader<object> Error
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._errorStream.ObjectReader;
      }
    }

    public override void Stop()
    {
      using (PipelineBase._trace.TraceMethod())
        this.CoreStop(true);
    }

    public override void StopAsync()
    {
      using (PipelineBase._trace.TraceMethod())
        this.CoreStop(false);
    }

    private void CoreStop(bool syncCall)
    {
      using (PipelineBase._trace.TraceMethod())
      {
        bool flag = false;
        lock (this.SyncRoot)
        {
          switch (this.PipelineState)
          {
            case PipelineState.NotStarted:
              this.SetPipelineState(PipelineState.Stopping);
              this.SetPipelineState(PipelineState.Stopped);
              break;
            case PipelineState.Running:
              this.SetPipelineState(PipelineState.Stopping);
              break;
            case PipelineState.Stopping:
              flag = true;
              break;
            case PipelineState.Stopped:
              return;
            case PipelineState.Completed:
              return;
            case PipelineState.Failed:
              return;
          }
        }
        if (flag)
        {
          if (!syncCall)
            return;
          this.PipelineFinishedEvent.WaitOne();
        }
        else
        {
          this.RaisePipelineStateEvents();
          lock (this.SyncRoot)
          {
            if (this.PipelineState == PipelineState.Stopped)
              return;
          }
          this.ImplementStop(syncCall);
        }
      }
    }

    protected abstract void ImplementStop(bool syncCall);

    public override Collection<PSObject> Invoke(IEnumerable input)
    {
      using (PipelineBase._trace.TraceMethod())
      {
        if (this._disposed)
          throw PipelineBase._trace.NewObjectDisposedException("pipeline");
        this.CoreInvoke(input, true);
        this.PipelineFinishedEvent.WaitOne();
        if (this.SyncInvokeCall)
          this.RaisePipelineStateEvents();
        if (this.PipelineStateInfo.State == PipelineState.Stopped)
          return new Collection<PSObject>();
        if (this.PipelineStateInfo.State == PipelineState.Failed && this.PipelineStateInfo.Reason != null)
        {
          PipelineBase._trace.TraceException(this.PipelineStateInfo.Reason);
          RuntimeException.LockStackTrace(this.PipelineStateInfo.Reason);
          throw this.PipelineStateInfo.Reason;
        }
        return this.Output.NonBlockingRead(int.MaxValue);
      }
    }

    public override void InvokeAsync()
    {
      using (PipelineBase._trace.TraceMethod())
        this.CoreInvoke((IEnumerable) null, false);
    }

    protected bool SyncInvokeCall
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._syncInvokeCall;
      }
    }

    private void CoreInvoke(IEnumerable input, bool syncCall)
    {
      using (PipelineBase._trace.TraceMethod())
      {
        lock (this.SyncRoot)
        {
          if (this._disposed)
            throw PipelineBase._trace.NewObjectDisposedException("pipeline");
          if (this.Commands == null || this.Commands.Count == 0)
            throw PipelineBase._trace.NewInvalidOperationException("Runspace", "NoCommandInPipeline");
          if (this.PipelineState != PipelineState.NotStarted)
          {
            InvalidPipelineStateException pipelineStateException = new InvalidPipelineStateException(ResourceManagerCache.FormatResourceString("Runspace", "PipelineReInvokeNotAllowed"), this.PipelineState, PipelineState.NotStarted);
            PipelineBase._trace.TraceException((Exception) pipelineStateException);
            throw pipelineStateException;
          }
          if (syncCall)
          {
            if (input != null)
            {
              foreach (object obj in input)
                this._inputStream.Write(obj);
            }
            this._inputStream.Close();
          }
          this._syncInvokeCall = syncCall;
          this._pipelineFinishedEvent = new ManualResetEvent(false);
          this.RunspaceBase.DoConcurrentCheckAndAddToRunningPipelines(this, syncCall);
          this.SetPipelineState(PipelineState.Running);
        }
        try
        {
          this.StartPipelineExecution();
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          PipelineBase._trace.TraceException(ex);
          this.RunspaceBase.RemoveFromRunningPipelineList(this);
          this.SetPipelineState(PipelineState.Failed, ex);
          throw;
        }
      }
    }

    protected abstract void StartPipelineExecution();

    internal bool PerformNestedCheck
    {
      set
      {
        using (PipelineBase._trace.TraceProperty())
          this._performNestedCheck = value;
      }
    }

    internal Thread NestedPipelineExecutionThread
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._nestedPipelineExecutionThread;
      }
      set
      {
        using (PipelineBase._trace.TraceProperty())
          this._nestedPipelineExecutionThread = value;
      }
    }

    internal void DoConcurrentCheck(bool syncCall)
    {
      using (PipelineBase._trace.TraceMethod())
      {
        PipelineBase currentlyRunningPipeline = (PipelineBase) this.RunspaceBase.GetCurrentlyRunningPipeline();
        if (!this.IsNested)
        {
          if (currentlyRunningPipeline == null)
            return;
          if (currentlyRunningPipeline != this.RunspaceBase.PulsePipeline)
            throw PipelineBase._trace.NewInvalidOperationException("Runspace", "ConcurrentInvokeNotAllowed");
          this.RunspaceBase.WaitForFinishofPipelines();
          this.DoConcurrentCheck(syncCall);
        }
        else
        {
          if (!this._performNestedCheck)
            return;
          if (!syncCall)
            throw PipelineBase._trace.NewInvalidOperationException("Runspace", "NestedPipelineInvokeAsync");
          if (currentlyRunningPipeline == null)
            throw PipelineBase._trace.NewInvalidOperationException("Runspace", "NestedPipelineNoParentPipeline");
          Thread currentThread = Thread.CurrentThread;
          if (!currentlyRunningPipeline.NestedPipelineExecutionThread.Equals((object) currentThread))
            throw PipelineBase._trace.NewInvalidOperationException("Runspace", "NestedPipelineNoParentPipeline");
        }
      }
    }

    public override event EventHandler<PipelineStateEventArgs> StateChanged;

    protected PipelineState PipelineState
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._pipelineStateInfo.State;
      }
    }

    protected bool IsPipelineFinished()
    {
      using (PipelineBase._trace.TraceMethod())
        return this.PipelineState == PipelineState.Completed || this.PipelineState == PipelineState.Failed || this.PipelineState == PipelineState.Stopped;
    }

    protected void SetPipelineState(PipelineState state, Exception reason)
    {
      using (PipelineBase._trace.TraceMethod("{0} to {1} {2}", (object) this.PipelineState, (object) state, reason != null ? (object) reason.Message : (object) ""))
      {
        lock (this.SyncRoot)
        {
          if (state == this.PipelineState)
            return;
          this._pipelineStateInfo = new PipelineStateInfo(state, reason);
          RunspaceAvailability runspaceAvailability = this._runspace.RunspaceAvailability;
          this._runspace.UpdateRunspaceAvailability(this._pipelineStateInfo.State, false);
          this._executionEventQueue.Enqueue(new PipelineBase.ExecutionEventQueueItem(this._pipelineStateInfo.Clone(), runspaceAvailability, this._runspace.RunspaceAvailability));
        }
      }
    }

    protected void SetPipelineState(PipelineState state)
    {
      using (PipelineBase._trace.TraceMethod())
        this.SetPipelineState(state, (Exception) null);
    }

    protected void RaisePipelineStateEvents()
    {
      using (PipelineBase._trace.TraceMethod())
      {
        Queue<PipelineBase.ExecutionEventQueueItem> executionEventQueueItemQueue = (Queue<PipelineBase.ExecutionEventQueueItem>) null;
        EventHandler<PipelineStateEventArgs> eventHandler = (EventHandler<PipelineStateEventArgs>) null;
        bool flag = false;
        lock (this.SyncRoot)
        {
          eventHandler = this.StateChanged;
          flag = this._runspace.HasAvailabilityChangedSubscribers;
          if (eventHandler != null || flag)
          {
            executionEventQueueItemQueue = this._executionEventQueue;
            this._executionEventQueue = new Queue<PipelineBase.ExecutionEventQueueItem>();
          }
          else
            this._executionEventQueue.Clear();
        }
        if (executionEventQueueItemQueue == null)
          return;
        while (executionEventQueueItemQueue.Count > 0)
        {
          PipelineBase.ExecutionEventQueueItem executionEventQueueItem = executionEventQueueItemQueue.Dequeue();
          if (flag && executionEventQueueItem.NewRunspaceAvailability != executionEventQueueItem.CurrentRunspaceAvailability)
            this._runspace.RaiseAvailabilityChangedEvent(executionEventQueueItem.NewRunspaceAvailability);
          if (eventHandler != null)
          {
            try
            {
              eventHandler((object) this, new PipelineStateEventArgs(executionEventQueueItem.PipelineStateInfo));
            }
            catch (Exception ex)
            {
              CommandProcessorBase.CheckForSevereException(ex);
              PipelineBase._trace.TraceException(ex);
            }
          }
        }
      }
    }

    internal ManualResetEvent PipelineFinishedEvent => this._pipelineFinishedEvent;

    protected ObjectStreamBase OutputStream
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._outputStream;
      }
    }

    protected ObjectStreamBase ErrorStream
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._errorStream;
      }
    }

    protected PSInformationalBuffers InformationalBuffers
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._informationalBuffers;
      }
    }

    protected ObjectStreamBase InputStream
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._inputStream;
      }
    }

    internal bool AddToHistory
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._addToHistory;
      }
      set
      {
        using (PipelineBase._trace.TraceProperty())
          this._addToHistory = value;
      }
    }

    internal string HistoryString
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._historyString;
      }
      set
      {
        using (PipelineBase._trace.TraceProperty())
          this._historyString = value;
      }
    }

    private void Initialize(Runspace runspace, string command, bool addToHistory, bool isNested)
    {
      this._runspace = runspace;
      this._isNested = isNested;
      if (addToHistory && command == null)
        throw PipelineBase._trace.NewArgumentNullException(nameof (command));
      if (command != null)
        this.Commands.Add(new Command(command, true, false));
      this._addToHistory = addToHistory;
      if (!this._addToHistory)
        return;
      this._historyString = command;
    }

    private RunspaceBase RunspaceBase
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return (RunspaceBase) this.Runspace;
      }
    }

    protected internal object SyncRoot
    {
      get
      {
        using (PipelineBase._trace.TraceProperty())
          return this._syncRoot;
      }
    }

    protected override void Dispose(bool disposing)
    {
      using (PipelineBase._trace.TraceDispose((object) this))
      {
        try
        {
          if (this._disposed)
            return;
          this._disposed = true;
          if (!disposing)
            return;
          this._inputStream.Close();
          this._outputStream.Close();
          this._errorStream.Close();
        }
        finally
        {
          base.Dispose(disposing);
        }
      }
    }

    private class ExecutionEventQueueItem
    {
      public PipelineStateInfo PipelineStateInfo;
      public RunspaceAvailability CurrentRunspaceAvailability;
      public RunspaceAvailability NewRunspaceAvailability;

      public ExecutionEventQueueItem(
        PipelineStateInfo pipelineStateInfo,
        RunspaceAvailability currentAvailability,
        RunspaceAvailability newAvailability)
      {
        this.PipelineStateInfo = pipelineStateInfo;
        this.CurrentRunspaceAvailability = currentAvailability;
        this.NewRunspaceAvailability = newAvailability;
      }
    }
  }
}
