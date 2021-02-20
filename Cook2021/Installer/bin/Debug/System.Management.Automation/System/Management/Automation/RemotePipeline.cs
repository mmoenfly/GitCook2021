// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemotePipeline
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Management.Automation.Remoting.Client;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace System.Management.Automation
{
  internal class RemotePipeline : Pipeline
  {
    [TraceSource("RemotePipeline", "RemotePipeline")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (RemotePipeline), nameof (RemotePipeline));
    private PowerShell _powershell;
    private bool _addToHistory;
    private bool _isNested;
    private Runspace _runspace;
    private object _syncRoot = new object();
    private bool _disposed;
    private string _historyString;
    private PipelineStateInfo _pipelineStateInfo = new PipelineStateInfo(PipelineState.NotStarted);
    private CommandCollection _commands = new CommandCollection();
    private string _computerName;
    private Guid _runspaceId;
    private Queue<RemotePipeline.ExecutionEventQueueItem> _executionEventQueue = new Queue<RemotePipeline.ExecutionEventQueueItem>();
    private ManualResetEvent _pipelineFinishedEvent;
    private bool _isMethodExecutorStreamEnabled;
    private ObjectStream _methodExecutorStream;
    private bool _performNestedCheck = true;
    private Thread _nestedPipelineExecutionThread;
    private PSDataCollection<PSObject> outputCollection;
    private PSDataCollectionStream<PSObject> _outputStream;
    private PSDataCollection<ErrorRecord> errorCollection;
    private PSDataCollectionStream<ErrorRecord> _errorStream;
    private PSDataCollection<object> inputCollection;
    private PSDataCollectionStream<object> _inputStream;

    internal RemotePipeline(
      RemoteRunspace runspace,
      string command,
      bool addToHistory,
      bool isNested)
      : base((Runspace) runspace)
    {
      this._addToHistory = addToHistory;
      this._isNested = isNested;
      this._runspace = (Runspace) runspace;
      this._computerName = this._runspace.ConnectionInfo.ComputerName;
      this._runspaceId = this._runspace.InstanceId;
      this.inputCollection = new PSDataCollection<object>();
      this.inputCollection.ReleaseOnEnumeration = true;
      this._inputStream = new PSDataCollectionStream<object>(Guid.Empty, this.inputCollection);
      this.outputCollection = new PSDataCollection<PSObject>();
      this._outputStream = new PSDataCollectionStream<PSObject>(Guid.Empty, this.outputCollection);
      this.errorCollection = new PSDataCollection<ErrorRecord>();
      this._errorStream = new PSDataCollectionStream<ErrorRecord>(Guid.Empty, this.errorCollection);
      this._methodExecutorStream = new ObjectStream();
      this._isMethodExecutorStreamEnabled = false;
      this.SetCommandCollection(this._commands);
      if (command != null)
        this._commands.Add(new Command(command, true));
      this._powershell = new PowerShell(true, (ObjectStreamBase) this._inputStream, (ObjectStreamBase) this._outputStream, (ObjectStreamBase) this._errorStream, ((RemoteRunspace) this._runspace).RunspacePool);
      this._powershell.InvocationStateChanged += new EventHandler<PSInvocationStateChangedEventArgs>(this.HandleInvocationStateChanged);
      this._pipelineFinishedEvent = new ManualResetEvent(false);
    }

    private RemotePipeline(RemotePipeline pipeline)
      : this((RemoteRunspace) pipeline.Runspace, (string) null, false, pipeline.IsNested)
    {
      using (RemotePipeline._trace.TraceConstructor((object) this))
      {
        if (pipeline == null)
          throw RemotePipeline._trace.NewArgumentNullException(nameof (pipeline));
        this._addToHistory = !pipeline._disposed ? pipeline._addToHistory : throw RemotePipeline._trace.NewObjectDisposedException(nameof (pipeline));
        this._historyString = pipeline._historyString;
        foreach (Command command in (Collection<Command>) pipeline.Commands)
          this.Commands.Add(command.Clone());
      }
    }

    public override Pipeline Copy()
    {
      using (RemotePipeline._trace.TraceMethod())
      {
        if (this._disposed)
          throw RemotePipeline._trace.NewObjectDisposedException("pipeline");
        return (Pipeline) new RemotePipeline(this);
      }
    }

    public override Runspace Runspace
    {
      get
      {
        using (RemotePipeline._trace.TraceProperty())
        {
          if (this._disposed)
            throw RemotePipeline._trace.NewObjectDisposedException("pipeline");
          return this._runspace;
        }
      }
    }

    internal Runspace GetRunspace()
    {
      using (RemotePipeline._trace.TraceMethod())
        return this._runspace;
    }

    public override bool IsNested
    {
      get
      {
        using (RemotePipeline._trace.TraceProperty())
          return this._isNested;
      }
    }

    internal void SetIsNested(bool isNested)
    {
      using (RemotePipeline._trace.TraceMethod())
        this._isNested = isNested;
    }

    public override PipelineStateInfo PipelineStateInfo
    {
      get
      {
        using (RemotePipeline._trace.TraceProperty())
        {
          lock (this._syncRoot)
            return this._pipelineStateInfo.Clone();
        }
      }
    }

    public override PipelineWriter Input
    {
      get
      {
        using (RemotePipeline._trace.TraceProperty())
          return this._inputStream.ObjectWriter;
      }
    }

    public override PipelineReader<PSObject> Output
    {
      get
      {
        using (RemotePipeline._trace.TraceProperty())
          return this._outputStream.GetPSObjectReaderForPipeline(this._computerName, this._runspaceId);
      }
    }

    public override PipelineReader<object> Error
    {
      get
      {
        using (RemotePipeline._trace.TraceProperty())
          return this._errorStream.GetObjectReaderForPipeline(this._computerName, this._runspaceId);
      }
    }

    internal string HistoryString
    {
      get
      {
        using (RemotePipeline._trace.TraceProperty())
          return this._historyString;
      }
      set
      {
        using (RemotePipeline._trace.TraceProperty())
          this._historyString = value;
      }
    }

    public bool AddToHistory => this._addToHistory;

    protected PSDataCollectionStream<object> InputStream
    {
      get
      {
        using (RemotePipeline._trace.TraceProperty())
          return this._inputStream;
      }
    }

    public override void InvokeAsync()
    {
      this.InitPowerShell(false);
      try
      {
        this._powershell.BeginInvoke();
      }
      catch (InvalidRunspacePoolStateException ex)
      {
        InvalidRunspaceStateException runspaceStateException = new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString("Runspace", "RunspaceNotOpenForPipeline", (object) this._runspace.RunspaceStateInfo.State.ToString()), this._runspace.RunspaceStateInfo.State, RunspaceState.Opened);
        RemotePipeline._trace.TraceException((Exception) runspaceStateException);
        throw runspaceStateException;
      }
    }

    public override Collection<PSObject> Invoke(IEnumerable input)
    {
      if (input == null)
        this.InputStream.Close();
      this.InitPowerShell(true);
      try
      {
        return this._powershell.Invoke(input);
      }
      catch (InvalidRunspacePoolStateException ex)
      {
        InvalidRunspaceStateException runspaceStateException = new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString("Runspace", "RunspaceNotOpenForPipeline", (object) this._runspace.RunspaceStateInfo.State.ToString()), this._runspace.RunspaceStateInfo.State, RunspaceState.Opened);
        RemotePipeline._trace.TraceException((Exception) runspaceStateException);
        throw runspaceStateException;
      }
    }

    public override void Stop()
    {
      bool isAlreadyStopping = false;
      if (this.CanStopPipeline(out isAlreadyStopping) && this._powershell != null)
      {
        IAsyncResult asyncResult;
        try
        {
          asyncResult = this._powershell.BeginStop((AsyncCallback) null, (object) null);
        }
        catch (ObjectDisposedException ex)
        {
          throw RemotePipeline._trace.NewObjectDisposedException("Pipeline");
        }
        asyncResult.AsyncWaitHandle.WaitOne();
      }
      this.PipelineFinishedEvent.WaitOne();
    }

    public override void StopAsync()
    {
      if (!this.CanStopPipeline(out bool _))
        return;
      try
      {
        this._powershell.BeginStop((AsyncCallback) null, (object) null);
      }
      catch (ObjectDisposedException ex)
      {
        throw RemotePipeline._trace.NewObjectDisposedException("Pipeline");
      }
    }

    private bool CanStopPipeline(out bool isAlreadyStopping)
    {
      bool flag = false;
      isAlreadyStopping = false;
      lock (this._syncRoot)
      {
        switch (this._pipelineStateInfo.State)
        {
          case PipelineState.NotStarted:
            this.SetPipelineState(PipelineState.Stopping, (Exception) null);
            this.SetPipelineState(PipelineState.Stopped, (Exception) null);
            flag = false;
            break;
          case PipelineState.Running:
            this.SetPipelineState(PipelineState.Stopping, (Exception) null);
            flag = true;
            break;
          case PipelineState.Stopping:
            isAlreadyStopping = true;
            return false;
          case PipelineState.Stopped:
          case PipelineState.Completed:
          case PipelineState.Failed:
            return false;
        }
      }
      this.RaisePipelineStateEvents();
      return flag;
    }

    public override event EventHandler<PipelineStateEventArgs> StateChanged;

    protected override void Dispose(bool disposing)
    {
      try
      {
        if (this._disposed)
          return;
        lock (this._syncRoot)
        {
          if (this._disposed)
            return;
          this._disposed = true;
        }
        if (!disposing)
          return;
        this.Stop();
        if (this._powershell != null)
        {
          this._powershell.Dispose();
          this._powershell = (PowerShell) null;
        }
        this.inputCollection.Dispose();
        this._inputStream.Dispose();
        this.outputCollection.Dispose();
        this._outputStream.Dispose();
        this.errorCollection.Dispose();
        this._errorStream.Dispose();
        this._methodExecutorStream.Dispose();
        this._pipelineFinishedEvent.Close();
      }
      finally
      {
        base.Dispose(disposing);
      }
    }

    private void HandleInvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
    {
      this.SetPipelineState((PipelineState) e.InvocationStateInfo.State, e.InvocationStateInfo.Reason);
      this.RaisePipelineStateEvents();
    }

    private void SetPipelineState(PipelineState state, Exception reason)
    {
      using (RemotePipeline._trace.TraceMethod("{0} to {1} {2}", (object) this._pipelineStateInfo.State, (object) state, reason != null ? (object) reason.Message : (object) ""))
      {
        PipelineState state1 = state;
        PipelineStateInfo pipelineStateInfo = (PipelineStateInfo) null;
        lock (this._syncRoot)
        {
          switch (this._pipelineStateInfo.State)
          {
            case PipelineState.Running:
              if (state == PipelineState.Running)
                return;
              break;
            case PipelineState.Stopping:
              if (state == PipelineState.Running || state == PipelineState.Stopping)
                return;
              state1 = PipelineState.Stopped;
              break;
            case PipelineState.Stopped:
              return;
            case PipelineState.Completed:
              return;
            case PipelineState.Failed:
              return;
          }
          this._pipelineStateInfo = new PipelineStateInfo(state1, reason);
          pipelineStateInfo = this._pipelineStateInfo;
          RunspaceAvailability runspaceAvailability = this._runspace.RunspaceAvailability;
          this._runspace.UpdateRunspaceAvailability(this._pipelineStateInfo.State, false);
          this._executionEventQueue.Enqueue(new RemotePipeline.ExecutionEventQueueItem(this._pipelineStateInfo.Clone(), runspaceAvailability, this._runspace.RunspaceAvailability));
        }
        if (pipelineStateInfo.State != PipelineState.Completed && pipelineStateInfo.State != PipelineState.Failed && pipelineStateInfo.State != PipelineState.Stopped)
          return;
        this.Cleanup();
      }
    }

    protected void RaisePipelineStateEvents()
    {
      using (RemotePipeline._trace.TraceMethod())
      {
        Queue<RemotePipeline.ExecutionEventQueueItem> executionEventQueueItemQueue = (Queue<RemotePipeline.ExecutionEventQueueItem>) null;
        EventHandler<PipelineStateEventArgs> eventHandler = (EventHandler<PipelineStateEventArgs>) null;
        bool flag = false;
        lock (this._syncRoot)
        {
          eventHandler = this.StateChanged;
          flag = this._runspace.HasAvailabilityChangedSubscribers;
          if (eventHandler != null || flag)
          {
            executionEventQueueItemQueue = this._executionEventQueue;
            this._executionEventQueue = new Queue<RemotePipeline.ExecutionEventQueueItem>();
          }
          else
            this._executionEventQueue.Clear();
        }
        if (executionEventQueueItemQueue == null)
          return;
        while (executionEventQueueItemQueue.Count > 0)
        {
          RemotePipeline.ExecutionEventQueueItem executionEventQueueItem = executionEventQueueItemQueue.Dequeue();
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
              RemotePipeline._trace.TraceException(ex);
            }
          }
        }
      }
    }

    private void InitPowerShell(bool syncCall)
    {
      if (this._commands == null || this._commands.Count == 0)
        throw RemotePipeline._trace.NewInvalidOperationException("Runspace", "NoCommandInPipeline");
      if (this._pipelineStateInfo.State != PipelineState.NotStarted)
      {
        InvalidPipelineStateException pipelineStateException = new InvalidPipelineStateException(ResourceManagerCache.FormatResourceString("Runspace", "PipelineReInvokeNotAllowed"), this._pipelineStateInfo.State, PipelineState.NotStarted);
        RemotePipeline._trace.TraceException((Exception) pipelineStateException);
        throw pipelineStateException;
      }
      ((RemoteRunspace) this._runspace).DoConcurrentCheckAndAddToRunningPipelines(this, syncCall);
      this._powershell.InitForRemotePipeline(this._commands, (ObjectStreamBase) this._inputStream, (ObjectStreamBase) this._outputStream, (ObjectStreamBase) this._errorStream, new PSInvocationSettings()
      {
        AddToHistory = this._addToHistory
      }, this.RedirectShellErrorOutputPipe);
      this._powershell.RemotePowerShell.HostCallReceived += new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleHostCallReceived);
    }

    private void HandleHostCallReceived(
      object sender,
      RemoteDataEventArgs<RemoteHostCall> eventArgs)
    {
      ClientMethodExecutor.Dispatch((BaseClientTransportManager) this._powershell.RemotePowerShell.DataStructureHandler.TransportManager, ((RemoteRunspace) this._runspace).RunspacePool.RemoteRunspacePoolInternal.Host, this._errorStream, this._methodExecutorStream, this.IsMethodExecutorStreamEnabled, ((RemoteRunspace) this._runspace).RunspacePool.RemoteRunspacePoolInternal, this._powershell.InstanceId, eventArgs.Data);
    }

    private void Cleanup()
    {
      using (RemotePipeline._trace.TraceMethod())
      {
        if (this._outputStream.IsOpen)
        {
          try
          {
            this.outputCollection.Complete();
            this._outputStream.Close();
          }
          catch (ObjectDisposedException ex)
          {
            RemotePipeline._trace.TraceException((Exception) ex);
          }
        }
        if (this._errorStream.IsOpen)
        {
          try
          {
            this.errorCollection.Complete();
            this._errorStream.Close();
          }
          catch (ObjectDisposedException ex)
          {
            RemotePipeline._trace.TraceException((Exception) ex);
          }
        }
        if (this._inputStream.IsOpen)
        {
          try
          {
            this.inputCollection.Complete();
            this._inputStream.Close();
          }
          catch (ObjectDisposedException ex)
          {
            RemotePipeline._trace.TraceException((Exception) ex);
          }
        }
        try
        {
          ((RemoteRunspace) this._runspace).RemoveFromRunningPipelineList(this);
          this._pipelineFinishedEvent.Set();
        }
        catch (ObjectDisposedException ex)
        {
          RemotePipeline._trace.TraceException((Exception) ex);
        }
      }
    }

    internal Thread NestedPipelineExecutionThread
    {
      get
      {
        using (RemotePipeline._trace.TraceProperty())
          return this._nestedPipelineExecutionThread;
      }
      set
      {
        using (RemotePipeline._trace.TraceProperty())
          this._nestedPipelineExecutionThread = value;
      }
    }

    internal ManualResetEvent PipelineFinishedEvent => this._pipelineFinishedEvent;

    internal bool IsMethodExecutorStreamEnabled
    {
      get
      {
        using (RemotePipeline._trace.TraceProperty())
          return this._isMethodExecutorStreamEnabled;
      }
      set
      {
        using (RemotePipeline._trace.TraceProperty())
          this._isMethodExecutorStreamEnabled = value;
      }
    }

    internal ObjectStream MethodExecutorStream
    {
      get
      {
        using (RemotePipeline._trace.TraceProperty())
          return this._methodExecutorStream;
      }
    }

    internal void DoConcurrentCheck(bool syncCall)
    {
      using (RemotePipeline._trace.TraceMethod())
      {
        RemotePipeline currentlyRunningPipeline = (RemotePipeline) this._runspace.GetCurrentlyRunningPipeline();
        if (!this._isNested)
        {
          if (currentlyRunningPipeline != null)
            throw RemotePipeline._trace.NewInvalidOperationException("Runspace", "ConcurrentInvokeNotAllowed");
        }
        else
        {
          if (!this._performNestedCheck)
            return;
          if (!syncCall)
            throw RemotePipeline._trace.NewInvalidOperationException("Runspace", "NestedPipelineInvokeAsync");
          if (currentlyRunningPipeline == null)
            throw RemotePipeline._trace.NewInvalidOperationException("Runspace", "NestedPipelineNoParentPipeline");
          Thread currentThread = Thread.CurrentThread;
          if (!currentlyRunningPipeline.NestedPipelineExecutionThread.Equals((object) currentThread))
            throw RemotePipeline._trace.NewInvalidOperationException("Runspace", "NestedPipelineNoParentPipeline");
        }
      }
    }

    internal PowerShell PowerShell => this._powershell;

    internal override void SetHistoryString(string historyString) => this._powershell.HistoryString = historyString;

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
