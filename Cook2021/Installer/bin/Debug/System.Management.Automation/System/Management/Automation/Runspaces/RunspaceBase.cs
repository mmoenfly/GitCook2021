// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
  internal abstract class RunspaceBase : Runspace
  {
    private PSHost _host;
    private RunspaceConfiguration _runspaceConfiguration;
    private InitialSessionState _initialSessionState;
    private Version _version = PSVersionInfo.PSVersion;
    private RunspaceStateInfo _runspaceStateInfo = new RunspaceStateInfo(RunspaceState.BeforeOpen);
    private RunspaceAvailability _runspaceAvailability;
    private object _syncRoot = new object();
    private Queue<RunspaceBase.RunspaceEventQueueItem> _runspaceEventQueue = new Queue<RunspaceBase.RunspaceEventQueueItem>();
    private bool _bypassRunspaceStateCheck;
    private ArrayList _runningPipelines = new ArrayList();
    private Pipeline currentlyRunningPipeline;
    private PipelineBase pulsePipeline;
    private bool _bSessionStateProxyCallInProgress;
    private SessionStateProxy _sessionStateProxy;
    [TraceSource("RunspaceBase", "RunspaceBase")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (RunspaceBase), nameof (RunspaceBase));

    protected RunspaceBase(PSHost host, RunspaceConfiguration runspaceConfiguration)
    {
      if (host == null)
        throw RunspaceBase._trace.NewArgumentNullException(nameof (host));
      if (runspaceConfiguration == null)
        throw RunspaceBase._trace.NewArgumentNullException(nameof (runspaceConfiguration));
      this._host = host;
      this._runspaceConfiguration = runspaceConfiguration;
    }

    protected RunspaceBase(PSHost host, InitialSessionState initialSessionState)
    {
      if (host == null)
        throw RunspaceBase._trace.NewArgumentNullException(nameof (host));
      if (initialSessionState == null)
        throw RunspaceBase._trace.NewArgumentNullException(nameof (initialSessionState));
      this._host = host;
      this._initialSessionState = initialSessionState.Clone();
      this.ApartmentState = initialSessionState.ApartmentState;
      this.ThreadOptions = initialSessionState.ThreadOptions;
    }

    protected RunspaceBase(
      PSHost host,
      InitialSessionState initialSessionState,
      bool suppressClone)
    {
      if (host == null)
        throw RunspaceBase._trace.NewArgumentNullException(nameof (host));
      if (initialSessionState == null)
        throw RunspaceBase._trace.NewArgumentNullException(nameof (initialSessionState));
      this._host = host;
      this._initialSessionState = !suppressClone ? initialSessionState.Clone() : initialSessionState;
      this.ApartmentState = initialSessionState.ApartmentState;
      this.ThreadOptions = initialSessionState.ThreadOptions;
    }

    protected PSHost Host => this._host;

    public override RunspaceConfiguration RunspaceConfiguration => this._runspaceConfiguration;

    public override InitialSessionState InitialSessionState => this._initialSessionState;

    public override Version Version => this._version;

    public override RunspaceStateInfo RunspaceStateInfo
    {
      get
      {
        lock (this._syncRoot)
          return this._runspaceStateInfo.Clone();
      }
    }

    public override RunspaceAvailability RunspaceAvailability
    {
      get => this._runspaceAvailability;
      protected set => this._runspaceAvailability = value;
    }

    protected internal object SyncRoot => this._syncRoot;

    public override RunspaceConnectionInfo ConnectionInfo => (RunspaceConnectionInfo) null;

    public override void Open() => this.CoreOpen(true);

    public override void OpenAsync() => this.CoreOpen(false);

    private void CoreOpen(bool syncCall)
    {
      lock (this.SyncRoot)
      {
        if (this.RunspaceState != RunspaceState.BeforeOpen)
        {
          InvalidRunspaceStateException runspaceStateException = new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString("Runspace", "CannotOpenAgain", (object) this.RunspaceState.ToString()), this.RunspaceState, RunspaceState.BeforeOpen);
          RunspaceBase._trace.TraceException((Exception) runspaceStateException);
          throw runspaceStateException;
        }
        this.SetRunspaceState(RunspaceState.Opening);
      }
      this.RaiseRunspaceStateEvents();
      PSSQMAPI.NoteRunspaceStart(this.InstanceId);
      this.OpenHelper(syncCall);
    }

    protected abstract void OpenHelper(bool syncCall);

    public override void Close() => this.CoreClose(true);

    public override void CloseAsync() => this.CoreClose(false);

    private void CoreClose(bool syncCall)
    {
      bool flag = false;
      lock (this.SyncRoot)
      {
        if (this.RunspaceState == RunspaceState.Closed || this.RunspaceState == RunspaceState.Broken)
          return;
        if (this.RunspaceState == RunspaceState.BeforeOpen)
        {
          this.SetRunspaceState(RunspaceState.Closing, (Exception) null);
          this.SetRunspaceState(RunspaceState.Closed, (Exception) null);
          this.RaiseRunspaceStateEvents();
          return;
        }
        if (this._bSessionStateProxyCallInProgress)
          throw RunspaceBase._trace.NewInvalidOperationException("Runspace", "RunspaceCloseInvalidWhileSessionStateProxy");
        if (this.RunspaceState == RunspaceState.Closing)
        {
          flag = true;
        }
        else
        {
          if (this.RunspaceState != RunspaceState.Opened)
          {
            InvalidRunspaceStateException runspaceStateException = new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString("Runspace", "RunspaceNotInOpenedState", (object) this.RunspaceState.ToString()), this.RunspaceState, RunspaceState.Opened);
            RunspaceBase._trace.TraceException((Exception) runspaceStateException);
            throw runspaceStateException;
          }
          this.SetRunspaceState(RunspaceState.Closing);
        }
      }
      if (flag)
      {
        if (!syncCall)
          return;
        this.WaitForFinishofPipelines();
      }
      else
      {
        this.RaiseRunspaceStateEvents();
        PSSQMAPI.NoteRunspaceEnd(this.InstanceId);
        this.CloseHelper(syncCall);
      }
    }

    protected abstract void CloseHelper(bool syncCall);

    public override Pipeline CreatePipeline() => this.CoreCreatePipeline((string) null, false, false);

    public override Pipeline CreatePipeline(string command) => command != null ? this.CoreCreatePipeline(command, false, false) : throw RunspaceBase._trace.NewArgumentNullException(nameof (command));

    public override Pipeline CreatePipeline(string command, bool addToHistory) => command != null ? this.CoreCreatePipeline(command, addToHistory, false) : throw RunspaceBase._trace.NewArgumentNullException(nameof (command));

    public override Pipeline CreateNestedPipeline() => this.CoreCreatePipeline((string) null, false, true);

    public override Pipeline CreateNestedPipeline(string command, bool addToHistory) => command != null ? this.CoreCreatePipeline(command, addToHistory, true) : throw RunspaceBase._trace.NewArgumentNullException(nameof (command));

    protected abstract Pipeline CoreCreatePipeline(
      string command,
      bool addToHistory,
      bool isNested);

    public override event EventHandler<RunspaceStateEventArgs> StateChanged;

    public override event EventHandler<RunspaceAvailabilityEventArgs> AvailabilityChanged;

    internal override bool HasAvailabilityChangedSubscribers => this.AvailabilityChanged != null;

    protected override void OnAvailabilityChanged(RunspaceAvailabilityEventArgs e)
    {
      EventHandler<RunspaceAvailabilityEventArgs> availabilityChanged = this.AvailabilityChanged;
      if (availabilityChanged == null)
        return;
      try
      {
        availabilityChanged((object) this, e);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        RunspaceBase._trace.TraceException(ex);
      }
    }

    protected RunspaceState RunspaceState => this._runspaceStateInfo.State;

    protected void SetRunspaceState(RunspaceState state, Exception reason)
    {
      lock (this.SyncRoot)
      {
        if (state == this.RunspaceState)
          return;
        this._runspaceStateInfo = new RunspaceStateInfo(state, reason);
        RunspaceAvailability runspaceAvailability = this._runspaceAvailability;
        this.UpdateRunspaceAvailability(this._runspaceStateInfo.State, false);
        this._runspaceEventQueue.Enqueue(new RunspaceBase.RunspaceEventQueueItem(this._runspaceStateInfo.Clone(), runspaceAvailability, this._runspaceAvailability));
      }
    }

    protected void SetRunspaceState(RunspaceState state) => this.SetRunspaceState(state, (Exception) null);

    protected void RaiseRunspaceStateEvents()
    {
      Queue<RunspaceBase.RunspaceEventQueueItem> runspaceEventQueueItemQueue = (Queue<RunspaceBase.RunspaceEventQueueItem>) null;
      EventHandler<RunspaceStateEventArgs> eventHandler = (EventHandler<RunspaceStateEventArgs>) null;
      bool flag = false;
      lock (this.SyncRoot)
      {
        eventHandler = this.StateChanged;
        flag = this.HasAvailabilityChangedSubscribers;
        if (eventHandler != null || flag)
        {
          runspaceEventQueueItemQueue = this._runspaceEventQueue;
          this._runspaceEventQueue = new Queue<RunspaceBase.RunspaceEventQueueItem>();
        }
        else
          this._runspaceEventQueue.Clear();
      }
      if (runspaceEventQueueItemQueue == null)
        return;
      while (runspaceEventQueueItemQueue.Count > 0)
      {
        RunspaceBase.RunspaceEventQueueItem runspaceEventQueueItem = runspaceEventQueueItemQueue.Dequeue();
        if (flag && runspaceEventQueueItem.NewRunspaceAvailability != runspaceEventQueueItem.CurrentRunspaceAvailability)
          this.OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(runspaceEventQueueItem.NewRunspaceAvailability));
        if (eventHandler != null)
        {
          try
          {
            eventHandler((object) this, new RunspaceStateEventArgs(runspaceEventQueueItem.RunspaceStateInfo));
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            RunspaceBase._trace.TraceException(ex);
          }
        }
      }
    }

    protected bool ByPassRunspaceStateCheck
    {
      get => this._bypassRunspaceStateCheck;
      set => this._bypassRunspaceStateCheck = value;
    }

    protected ArrayList RunningPipelines => this._runningPipelines;

    internal void AddToRunningPipelineList(PipelineBase pipeline)
    {
      lock (this._runningPipelines.SyncRoot)
      {
        if (!this._bypassRunspaceStateCheck && this.RunspaceState != RunspaceState.Opened)
        {
          InvalidRunspaceStateException runspaceStateException = new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString("Runspace", "RunspaceNotOpenForPipeline", (object) this.RunspaceState.ToString()), this.RunspaceState, RunspaceState.Opened);
          RunspaceBase._trace.TraceException((Exception) runspaceStateException);
          throw runspaceStateException;
        }
        this._runningPipelines.Add((object) pipeline);
        this.currentlyRunningPipeline = (Pipeline) pipeline;
      }
    }

    internal void RemoveFromRunningPipelineList(PipelineBase pipeline)
    {
      lock (this._runningPipelines.SyncRoot)
      {
        this._runningPipelines.Remove((object) pipeline);
        this.currentlyRunningPipeline = this._runningPipelines.Count != 0 ? (Pipeline) this._runningPipelines[this._runningPipelines.Count - 1] : (Pipeline) null;
        pipeline.PipelineFinishedEvent.Set();
      }
    }

    internal bool WaitForFinishofPipelines()
    {
      PipelineBase[] array;
      lock (this._runningPipelines.SyncRoot)
        array = (PipelineBase[]) this.RunningPipelines.ToArray(typeof (PipelineBase));
      if (array.Length <= 0)
        return true;
      WaitHandle[] waitHandles = new WaitHandle[array.Length];
      for (int index = 0; index < array.Length; ++index)
        waitHandles[index] = (WaitHandle) array[index].PipelineFinishedEvent;
      return WaitHandle.WaitAll(waitHandles);
    }

    protected void StopPipelines()
    {
      PipelineBase[] array;
      lock (this._runningPipelines.SyncRoot)
        array = (PipelineBase[]) this.RunningPipelines.ToArray(typeof (PipelineBase));
      if (array.Length <= 0)
        return;
      for (int index = array.Length - 1; index >= 0; --index)
        array[index].Stop();
    }

    internal override Pipeline GetCurrentlyRunningPipeline() => this.currentlyRunningPipeline;

    internal void StopNestedPipelines(Pipeline pipeline)
    {
      List<Pipeline> pipelineList = (List<Pipeline>) null;
      lock (this._runningPipelines.SyncRoot)
      {
        if (!this._runningPipelines.Contains((object) pipeline) || this.GetCurrentlyRunningPipeline() == pipeline)
          return;
        pipelineList = new List<Pipeline>();
        for (int index = this._runningPipelines.Count - 1; index >= 0; ++index)
        {
          if (this._runningPipelines[index] != pipeline)
            pipelineList.Add((Pipeline) this._runningPipelines[index]);
          else
            break;
        }
      }
      if (pipelineList == null)
        return;
      foreach (Pipeline pipeline1 in pipelineList)
      {
        try
        {
          pipeline1.Stop();
        }
        catch (InvalidPipelineStateException ex)
        {
          RunspaceBase._trace.TraceException((Exception) ex);
        }
      }
    }

    internal void DoConcurrentCheckAndAddToRunningPipelines(PipelineBase pipeline, bool syncCall)
    {
      lock (this._syncRoot)
      {
        if (this._bSessionStateProxyCallInProgress)
          throw RunspaceBase._trace.NewInvalidOperationException("Runspace", "NoPipelineWhenSessionStateProxyInProgress");
        pipeline.DoConcurrentCheck(syncCall);
        this.AddToRunningPipelineList(pipeline);
      }
    }

    internal void Pulse()
    {
      if (this.GetCurrentlyRunningPipeline() != null)
        return;
      lock (this.SyncRoot)
      {
        if (this.GetCurrentlyRunningPipeline() != null)
          return;
        try
        {
          this.pulsePipeline = (PipelineBase) this.CreatePipeline("0");
          this.pulsePipeline.Invoke();
        }
        catch (PSInvalidOperationException ex)
        {
        }
        catch (InvalidRunspaceStateException ex)
        {
        }
        catch (ObjectDisposedException ex)
        {
        }
      }
    }

    internal PipelineBase PulsePipeline => this.pulsePipeline;

    private void DoConcurrentCheckAndMarkSessionStateProxyCallInProgress()
    {
      lock (this._syncRoot)
      {
        if (this.RunspaceState != RunspaceState.Opened)
          throw new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString("Runspace", "RunspaceNotInOpenedState", (object) this.RunspaceState.ToString()), this.RunspaceState, RunspaceState.Opened);
        if (this._bSessionStateProxyCallInProgress)
          throw RunspaceBase._trace.NewInvalidOperationException("Runspace", "AnotherSessionStateProxyInProgress");
        Pipeline currentlyRunningPipeline = this.GetCurrentlyRunningPipeline();
        if (currentlyRunningPipeline != null)
        {
          if (currentlyRunningPipeline != this.pulsePipeline)
            throw RunspaceBase._trace.NewInvalidOperationException("Runspace", "NoSessionStateProxyWhenPipelineInProgress");
          this.WaitForFinishofPipelines();
          this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        }
        else
          this._bSessionStateProxyCallInProgress = true;
      }
    }

    internal void SetVariable(string name, object value)
    {
      this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
      try
      {
        this.DoSetVariable(name, value);
      }
      finally
      {
        lock (this.SyncRoot)
          this._bSessionStateProxyCallInProgress = false;
      }
    }

    internal object GetVariable(string name)
    {
      this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
      try
      {
        return this.DoGetVariable(name);
      }
      finally
      {
        lock (this._syncRoot)
          this._bSessionStateProxyCallInProgress = false;
      }
    }

    internal List<string> Applications
    {
      get
      {
        this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        try
        {
          return this.DoApplications;
        }
        finally
        {
          lock (this._syncRoot)
            this._bSessionStateProxyCallInProgress = false;
        }
      }
    }

    internal List<string> Scripts
    {
      get
      {
        this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        try
        {
          return this.DoScripts;
        }
        finally
        {
          lock (this._syncRoot)
            this._bSessionStateProxyCallInProgress = false;
        }
      }
    }

    internal DriveManagementIntrinsics Drive
    {
      get
      {
        this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        try
        {
          return this.DoDrive;
        }
        finally
        {
          lock (this._syncRoot)
            this._bSessionStateProxyCallInProgress = false;
        }
      }
    }

    internal PSLanguageMode LanguageMode
    {
      get
      {
        this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        try
        {
          return this.DoLanguageMode;
        }
        finally
        {
          lock (this._syncRoot)
            this._bSessionStateProxyCallInProgress = false;
        }
      }
      set
      {
        this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        try
        {
          this.DoLanguageMode = value;
        }
        finally
        {
          lock (this._syncRoot)
            this._bSessionStateProxyCallInProgress = false;
        }
      }
    }

    internal PSModuleInfo Module
    {
      get
      {
        this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        try
        {
          return this.DoModule;
        }
        finally
        {
          lock (this._syncRoot)
            this._bSessionStateProxyCallInProgress = false;
        }
      }
    }

    internal PathIntrinsics PathIntrinsics
    {
      get
      {
        this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        try
        {
          return this.DoPath;
        }
        finally
        {
          lock (this._syncRoot)
            this._bSessionStateProxyCallInProgress = false;
        }
      }
    }

    internal CmdletProviderManagementIntrinsics Provider
    {
      get
      {
        this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        try
        {
          return this.DoProvider;
        }
        finally
        {
          lock (this._syncRoot)
            this._bSessionStateProxyCallInProgress = false;
        }
      }
    }

    internal PSVariableIntrinsics PSVariable
    {
      get
      {
        this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        try
        {
          return this.DoPSVariable;
        }
        finally
        {
          lock (this._syncRoot)
            this._bSessionStateProxyCallInProgress = false;
        }
      }
    }

    internal CommandInvocationIntrinsics InvokeCommand
    {
      get
      {
        this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        try
        {
          return this.DoInvokeCommand;
        }
        finally
        {
          lock (this._syncRoot)
            this._bSessionStateProxyCallInProgress = false;
        }
      }
    }

    internal ProviderIntrinsics InvokeProvider
    {
      get
      {
        this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
        try
        {
          return this.DoInvokeProvider;
        }
        finally
        {
          lock (this._syncRoot)
            this._bSessionStateProxyCallInProgress = false;
        }
      }
    }

    protected abstract void DoSetVariable(string name, object value);

    protected abstract object DoGetVariable(string name);

    protected abstract List<string> DoApplications { get; }

    protected abstract List<string> DoScripts { get; }

    protected abstract DriveManagementIntrinsics DoDrive { get; }

    protected abstract PSLanguageMode DoLanguageMode { get; set; }

    protected abstract PSModuleInfo DoModule { get; }

    protected abstract PathIntrinsics DoPath { get; }

    protected abstract CmdletProviderManagementIntrinsics DoProvider { get; }

    protected abstract PSVariableIntrinsics DoPSVariable { get; }

    protected abstract CommandInvocationIntrinsics DoInvokeCommand { get; }

    protected abstract ProviderIntrinsics DoInvokeProvider { get; }

    internal override SessionStateProxy GetSessionStateProxy()
    {
      if (this._sessionStateProxy == null)
        this._sessionStateProxy = new SessionStateProxy(this);
      return this._sessionStateProxy;
    }

    private class RunspaceEventQueueItem
    {
      public RunspaceStateInfo RunspaceStateInfo;
      public RunspaceAvailability CurrentRunspaceAvailability;
      public RunspaceAvailability NewRunspaceAvailability;

      public RunspaceEventQueueItem(
        RunspaceStateInfo runspaceStateInfo,
        RunspaceAvailability currentAvailability,
        RunspaceAvailability newAvailability)
      {
        this.RunspaceStateInfo = runspaceStateInfo;
        this.CurrentRunspaceAvailability = currentAvailability;
        this.NewRunspaceAvailability = newAvailability;
      }
    }
  }
}
