// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemoteRunspace
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Management.Automation.Remoting.Client;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace System.Management.Automation
{
  internal class RemoteRunspace : Runspace, IDisposable
  {
    [TraceSource("RemoteRunspace", "Runspace on a remote computer")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (RemoteRunspace), "Runspace on a remote computer");
    private IETWTracer tracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Runspace);
    private RunspacePool _runspacePool;
    private ArrayList _runningPipelines = new ArrayList();
    private object _syncRoot = new object();
    private RunspaceStateInfo _runspaceStateInfo = new RunspaceStateInfo(RunspaceState.BeforeOpen);
    private Version _version = PSVersionInfo.PSVersion;
    private bool _bSessionStateProxyCallInProgress;
    private RunspaceConnectionInfo _connectionInfo;
    private bool _disposed;
    private InvokeCommandCommand currentInvokeCommand;
    private long currentLocalPipelineId;
    private Queue<RemoteRunspace.RunspaceEventQueueItem> _runspaceEventQueue = new Queue<RemoteRunspace.RunspaceEventQueueItem>();
    private bool _bypassRunspaceStateCheck;
    private bool _shouldCloseOnPop;
    private PSThreadOptions createThreadOptions;
    private RunspaceAvailability _runspaceAvailability;
    private PSRemoteEventManager _eventManager;

    protected bool ByPassRunspaceStateCheck
    {
      get
      {
        using (RemoteRunspace._trace.TraceProperty())
          return this._bypassRunspaceStateCheck;
      }
      set
      {
        using (RemoteRunspace._trace.TraceProperty())
          this._bypassRunspaceStateCheck = value;
      }
    }

    internal bool ShouldCloseOnPop
    {
      get => this._shouldCloseOnPop;
      set => this._shouldCloseOnPop = value;
    }

    internal RemoteRunspace(
      TypeTable typeTable,
      RunspaceConnectionInfo connectionInfo,
      PSHost host,
      PSPrimitiveDictionary applicationArguments)
    {
      using (RemoteRunspace._trace.TraceConstructor((object) this))
      {
        this.tracer.SetActivityIdForCurrentThread(this.InstanceId);
        this.tracer.OperationalChannel.WriteVerbose(PSEventId.RunspaceConstructor, PSOpcode.Constructor, PSTask.CreateRunspace, (object) this.InstanceId);
        switch (connectionInfo)
        {
          case WSManConnectionInfo _:
            this._connectionInfo = (RunspaceConnectionInfo) ((WSManConnectionInfo) connectionInfo).Copy();
            break;
          case NewProcessConnectionInfo _:
            this._connectionInfo = (RunspaceConnectionInfo) ((NewProcessConnectionInfo) connectionInfo).Copy();
            break;
        }
        this._runspacePool = RunspaceFactory.CreateRunspacePool(1, 1, connectionInfo, host, typeTable, applicationArguments);
        this._eventManager = new PSRemoteEventManager(connectionInfo.ComputerName, this.InstanceId);
        this._runspacePool.StateChanged += new EventHandler<RunspacePoolStateChangedEventArgs>(this.HandleRunspacePoolStateChanged);
        this._runspacePool.RemoteRunspacePoolInternal.HostCallReceived += new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleHostCallReceived);
        this._runspacePool.RemoteRunspacePoolInternal.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
        this._runspacePool.ForwardEvent += new EventHandler<PSEventArgs>(this.HandleRunspacePoolForwardEvent);
      }
    }

    public override RunspaceConfiguration RunspaceConfiguration
    {
      get
      {
        using (RemoteRunspace._trace.TraceProperty())
          throw RemoteRunspace._trace.NewNotImplementedException();
      }
    }

    public override InitialSessionState InitialSessionState
    {
      get
      {
        using (RemoteRunspace._trace.TraceProperty())
          throw RemoteRunspace._trace.NewNotImplementedException();
      }
    }

    public override Version Version
    {
      get
      {
        using (RemoteRunspace._trace.TraceProperty())
          return this._version;
      }
    }

    public override RunspaceStateInfo RunspaceStateInfo
    {
      get
      {
        using (RemoteRunspace._trace.TraceProperty())
        {
          lock (this._syncRoot)
            return this._runspaceStateInfo.Clone();
        }
      }
    }

    public override PSThreadOptions ThreadOptions
    {
      get => this.createThreadOptions;
      set
      {
        lock (this._syncRoot)
        {
          if (value == this.createThreadOptions)
            return;
          if (this.RunspaceStateInfo.State != RunspaceState.BeforeOpen)
            throw new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString("Runspace", "ChangePropertyAfterOpen"));
          this.createThreadOptions = value;
        }
      }
    }

    public override RunspaceAvailability RunspaceAvailability
    {
      get => this._runspaceAvailability;
      protected set => this._runspaceAvailability = value;
    }

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
        RemoteRunspace._trace.TraceException(ex);
      }
    }

    public override RunspaceConnectionInfo ConnectionInfo => this._connectionInfo;

    public override PSEventManager Events => (PSEventManager) this._eventManager;

    internal override ExecutionContext GetExecutionContext => throw RemoteRunspace._trace.NewNotImplementedException();

    internal override bool InNestedPrompt => false;

    internal ClientRemoteSession ClientRemoteSession
    {
      get
      {
        try
        {
          return this._runspacePool.RemoteRunspacePoolInternal.DataStructureHandler.RemoteSession;
        }
        catch (InvalidRunspacePoolStateException ex)
        {
          throw ex.ToInvalidRunspaceStateException();
        }
      }
    }

    public override void OpenAsync()
    {
      using (RemoteRunspace._trace.TraceMethod())
      {
        this.AssertIfStateIsBeforeOpen();
        try
        {
          this._runspacePool.BeginOpen((AsyncCallback) null, (object) null);
        }
        catch (InvalidRunspacePoolStateException ex)
        {
          throw ex.ToInvalidRunspaceStateException();
        }
      }
    }

    public override void Open()
    {
      using (RemoteRunspace._trace.TraceMethod())
      {
        this.AssertIfStateIsBeforeOpen();
        try
        {
          this._runspacePool.ThreadOptions = this.ThreadOptions;
          this._runspacePool.ApartmentState = this.ApartmentState;
          this._runspacePool.Open();
        }
        catch (InvalidRunspacePoolStateException ex)
        {
          throw ex.ToInvalidRunspaceStateException();
        }
      }
    }

    public override void CloseAsync()
    {
      using (RemoteRunspace._trace.TraceMethod())
      {
        try
        {
          this._runspacePool.BeginClose((AsyncCallback) null, (object) null);
        }
        catch (InvalidRunspacePoolStateException ex)
        {
          throw ex.ToInvalidRunspaceStateException();
        }
      }
    }

    public override void Close()
    {
      using (RemoteRunspace._trace.TraceMethod())
      {
        try
        {
          IAsyncResult asyncResult = this._runspacePool.BeginClose((AsyncCallback) null, (object) null);
          this.WaitForFinishofPipelines();
          this._runspacePool.EndClose(asyncResult);
        }
        catch (InvalidRunspacePoolStateException ex)
        {
          throw ex.ToInvalidRunspaceStateException();
        }
      }
    }

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
        this.Close();
        try
        {
          this._runspacePool.Dispose();
        }
        catch (InvalidRunspacePoolStateException ex)
        {
          throw ex.ToInvalidRunspaceStateException();
        }
        this.tracer.Dispose();
      }
      finally
      {
        base.Dispose(disposing);
      }
    }

    public override Pipeline CreatePipeline()
    {
      using (RemoteRunspace._trace.TraceMethod())
        return this.CoreCreatePipeline((string) null, false, false);
    }

    public override Pipeline CreatePipeline(string command)
    {
      using (RemoteRunspace._trace.TraceMethod())
        return command != null ? this.CoreCreatePipeline(command, false, false) : throw RemoteRunspace._trace.NewArgumentNullException(nameof (command));
    }

    public override Pipeline CreatePipeline(string command, bool addToHistory)
    {
      using (RemoteRunspace._trace.TraceMethod())
        return command != null ? this.CoreCreatePipeline(command, addToHistory, false) : throw RemoteRunspace._trace.NewArgumentNullException(nameof (command));
    }

    public override Pipeline CreateNestedPipeline() => throw RemoteRunspace._trace.NewNotSupportedException("RemotingErrorIdStrings", PSRemotingErrorId.NestedPipelineNotSupported.ToString());

    public override Pipeline CreateNestedPipeline(string command, bool addToHistory) => throw RemoteRunspace._trace.NewNotSupportedException("RemotingErrorIdStrings", PSRemotingErrorId.NestedPipelineNotSupported.ToString());

    internal void AddToRunningPipelineList(RemotePipeline pipeline)
    {
      using (RemoteRunspace._trace.TraceMethod())
      {
        lock (this._syncRoot)
        {
          if (!this._bypassRunspaceStateCheck && this._runspaceStateInfo.State != RunspaceState.Opened)
          {
            InvalidRunspaceStateException runspaceStateException = new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString("Runspace", "RunspaceNotOpenForPipeline", (object) this._runspaceStateInfo.State.ToString()), this._runspaceStateInfo.State, RunspaceState.Opened);
            RemoteRunspace._trace.TraceException((Exception) runspaceStateException);
            throw runspaceStateException;
          }
          this._runningPipelines.Add((object) pipeline);
        }
      }
    }

    internal void RemoveFromRunningPipelineList(RemotePipeline pipeline)
    {
      using (RemoteRunspace._trace.TraceMethod())
      {
        lock (this._syncRoot)
        {
          this._runningPipelines.Remove((object) pipeline);
          pipeline.PipelineFinishedEvent.Set();
        }
      }
    }

    internal void DoConcurrentCheckAndAddToRunningPipelines(RemotePipeline pipeline, bool syncCall)
    {
      using (RemoteRunspace._trace.TraceMethod())
      {
        lock (this._syncRoot)
        {
          if (this._bSessionStateProxyCallInProgress)
            throw RemoteRunspace._trace.NewInvalidOperationException("Runspace", "NoPipelineWhenSessionStateProxyInProgress");
          pipeline.DoConcurrentCheck(syncCall);
          this.AddToRunningPipelineList(pipeline);
        }
      }
    }

    internal override SessionStateProxy GetSessionStateProxy()
    {
      using (RemoteRunspace._trace.TraceMethod())
        throw RemoteRunspace._trace.NewNotImplementedException();
    }

    private void HandleRunspacePoolStateChanged(object sender, RunspacePoolStateChangedEventArgs e)
    {
      using (RemoteRunspace._trace.TraceEventHandlers())
      {
        this.SetRunspaceState((RunspaceState) e.RunspacePoolStateInfo.State, e.RunspacePoolStateInfo.Reason);
        this.RaiseRunspaceStateEvents();
      }
    }

    private void AssertIfStateIsBeforeOpen()
    {
      using (RemoteRunspace._trace.TraceMethod())
      {
        lock (this._syncRoot)
        {
          if (this._runspaceStateInfo.State != RunspaceState.BeforeOpen)
          {
            InvalidRunspaceStateException runspaceStateException = new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString("Runspace", "CannotOpenAgain", (object) this._runspaceStateInfo.State.ToString()), this._runspaceStateInfo.State, RunspaceState.BeforeOpen);
            RemoteRunspace._trace.TraceException((Exception) runspaceStateException);
            throw runspaceStateException;
          }
        }
      }
    }

    private void SetRunspaceState(RunspaceState state, Exception reason)
    {
      using (RemoteRunspace._trace.TraceMethod("{0} to {1} {2}", (object) this._runspaceStateInfo.State, (object) state, reason != null ? (object) reason.Message : (object) ""))
      {
        lock (this._syncRoot)
        {
          if (state == this._runspaceStateInfo.State)
            return;
          this._runspaceStateInfo = new RunspaceStateInfo(state, reason);
          RunspaceAvailability runspaceAvailability = this._runspaceAvailability;
          this.UpdateRunspaceAvailability(this._runspaceStateInfo.State, false);
          this._runspaceEventQueue.Enqueue(new RemoteRunspace.RunspaceEventQueueItem(this._runspaceStateInfo.Clone(), runspaceAvailability, this._runspaceAvailability));
        }
      }
    }

    private void RaiseRunspaceStateEvents()
    {
      using (RemoteRunspace._trace.TraceMethod())
      {
        Queue<RemoteRunspace.RunspaceEventQueueItem> runspaceEventQueueItemQueue = (Queue<RemoteRunspace.RunspaceEventQueueItem>) null;
        EventHandler<RunspaceStateEventArgs> eventHandler = (EventHandler<RunspaceStateEventArgs>) null;
        bool flag = false;
        lock (this._syncRoot)
        {
          eventHandler = this.StateChanged;
          flag = this.HasAvailabilityChangedSubscribers;
          if (eventHandler != null || flag)
          {
            runspaceEventQueueItemQueue = this._runspaceEventQueue;
            this._runspaceEventQueue = new Queue<RemoteRunspace.RunspaceEventQueueItem>();
          }
          else
            this._runspaceEventQueue.Clear();
        }
        if (runspaceEventQueueItemQueue == null)
          return;
        while (runspaceEventQueueItemQueue.Count > 0)
        {
          RemoteRunspace.RunspaceEventQueueItem runspaceEventQueueItem = runspaceEventQueueItemQueue.Dequeue();
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
              RemoteRunspace._trace.TraceException(ex);
            }
          }
        }
      }
    }

    private Pipeline CoreCreatePipeline(string command, bool addToHistory, bool isNested)
    {
      using (RemoteRunspace._trace.TraceMethod())
        return (Pipeline) new RemotePipeline(this, command, addToHistory, isNested);
    }

    private bool WaitForFinishofPipelines()
    {
      using (RemoteRunspace._trace.TraceMethod())
      {
        RemotePipeline[] array;
        lock (this._syncRoot)
          array = (RemotePipeline[]) this._runningPipelines.ToArray(typeof (RemotePipeline));
        if (array.Length <= 0)
          return true;
        WaitHandle[] waitHandles = new WaitHandle[array.Length];
        for (int index = 0; index < array.Length; ++index)
          waitHandles[index] = (WaitHandle) array[index].PipelineFinishedEvent;
        return WaitHandle.WaitAll(waitHandles);
      }
    }

    internal override Pipeline GetCurrentlyRunningPipeline()
    {
      using (RemoteRunspace._trace.TraceMethod())
      {
        lock (this._syncRoot)
          return this._runningPipelines.Count != 0 ? (Pipeline) this._runningPipelines[this._runningPipelines.Count - 1] : (Pipeline) null;
      }
    }

    private void HandleHostCallReceived(
      object sender,
      RemoteDataEventArgs<RemoteHostCall> eventArgs)
    {
      ClientMethodExecutor.Dispatch((BaseClientTransportManager) this._runspacePool.RemoteRunspacePoolInternal.DataStructureHandler.TransportManager, this._runspacePool.RemoteRunspacePoolInternal.Host, (PSDataCollectionStream<ErrorRecord>) null, (ObjectStream) null, false, this._runspacePool.RemoteRunspacePoolInternal, Guid.Empty, eventArgs.Data);
    }

    private void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs)
    {
      if (!(this._connectionInfo is WSManConnectionInfo connectionInfo))
        return;
      connectionInfo.SetConnectionUri(eventArgs.Data);
      if (this.URIRedirectionReported == null)
        return;
      this.URIRedirectionReported((object) this, eventArgs);
    }

    private void HandleRunspacePoolForwardEvent(object sender, PSEventArgs e) => this._eventManager.AddForwardedEvent(e);

    internal bool IsAnotherInvokeCommandExecuting(
      InvokeCommandCommand invokeCommand,
      long localPipelineId)
    {
      if (this.currentLocalPipelineId != localPipelineId && this.currentLocalPipelineId != 0L)
        return false;
      if (this.currentInvokeCommand == null)
      {
        this.SetCurrentInvokeCommand(invokeCommand, localPipelineId);
        return false;
      }
      return !this.currentInvokeCommand.Equals((object) invokeCommand);
    }

    internal void SetCurrentInvokeCommand(InvokeCommandCommand invokeCommand, long localPipelineId)
    {
      this.currentInvokeCommand = invokeCommand;
      this.currentLocalPipelineId = localPipelineId;
    }

    internal void ClearInvokeCommand()
    {
      this.currentLocalPipelineId = 0L;
      this.currentInvokeCommand = (InvokeCommandCommand) null;
    }

    internal RunspacePool RunspacePool => this._runspacePool;

    internal event EventHandler<RemoteDataEventArgs<Uri>> URIRedirectionReported;

    public override PSPrimitiveDictionary GetApplicationPrivateData()
    {
      try
      {
        return this._runspacePool.GetApplicationPrivateData();
      }
      catch (InvalidRunspacePoolStateException ex)
      {
        throw ex.ToInvalidRunspaceStateException();
      }
    }

    internal override void SetApplicationPrivateData(PSPrimitiveDictionary applicationPrivateData)
    {
    }

    protected class RunspaceEventQueueItem
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
