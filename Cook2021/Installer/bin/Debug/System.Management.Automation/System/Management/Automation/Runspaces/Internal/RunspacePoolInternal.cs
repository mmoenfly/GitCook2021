// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.Internal.RunspacePoolInternal
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Host;
using System.Threading;

namespace System.Management.Automation.Runspaces.Internal
{
  internal class RunspacePoolInternal
  {
    [TraceSource("RunspacePool", "Powershell hosting interfaces")]
    protected static PSTraceSource tracer = PSTraceSource.GetTracer("RunspacePool", "Powershell hosting interfaces");
    protected static string resBaseName = "RunspacePoolStrings";
    protected int maxPoolSz;
    protected int minPoolSz;
    protected int totalRunspaces;
    protected List<Runspace> runspaceList = new List<Runspace>();
    protected Stack<Runspace> pool;
    protected Queue<GetRunspaceAsyncResult> runspaceRequestQueue;
    protected Queue<GetRunspaceAsyncResult> ultimateRequestQueue;
    protected RunspacePoolStateInfo stateInfo;
    protected RunspaceConfiguration rsConfig;
    protected InitialSessionState _initialSessionState;
    protected PSHost host;
    protected Guid instanceId;
    private bool isDisposed;
    protected bool isServicingRequests;
    protected object syncObject = new object();
    private PSPrimitiveDictionary applicationPrivateData;
    private PSThreadOptions threadOptions;
    private ApartmentState apartmentState = ApartmentState.Unknown;

    public RunspacePoolInternal(
      int minRunspaces,
      int maxRunspaces,
      RunspaceConfiguration runspaceConfiguration,
      PSHost host)
      : this(minRunspaces, maxRunspaces)
    {
      using (RunspacePoolInternal.tracer.TraceConstructor((object) this))
      {
        if (runspaceConfiguration == null)
          throw RunspacePoolInternal.tracer.NewArgumentNullException(nameof (runspaceConfiguration));
        if (host == null)
          throw RunspacePoolInternal.tracer.NewArgumentNullException(nameof (host));
        this.rsConfig = runspaceConfiguration;
        this.host = host;
        this.pool = new Stack<Runspace>();
        this.runspaceRequestQueue = new Queue<GetRunspaceAsyncResult>();
        this.ultimateRequestQueue = new Queue<GetRunspaceAsyncResult>();
      }
    }

    public RunspacePoolInternal(
      int minRunspaces,
      int maxRunspaces,
      InitialSessionState initialSessionState,
      PSHost host)
      : this(minRunspaces, maxRunspaces)
    {
      using (RunspacePoolInternal.tracer.TraceConstructor((object) this))
      {
        if (initialSessionState == null)
          throw RunspacePoolInternal.tracer.NewArgumentNullException(nameof (initialSessionState));
        if (host == null)
          throw RunspacePoolInternal.tracer.NewArgumentNullException(nameof (host));
        this._initialSessionState = initialSessionState.Clone();
        this.host = host;
        this.pool = new Stack<Runspace>();
        this.runspaceRequestQueue = new Queue<GetRunspaceAsyncResult>();
        this.ultimateRequestQueue = new Queue<GetRunspaceAsyncResult>();
      }
    }

    protected RunspacePoolInternal(int minRunspaces, int maxRunspaces)
    {
      using (RunspacePoolInternal.tracer.TraceConstructor((object) this))
      {
        if (maxRunspaces < 1)
          throw RunspacePoolInternal.tracer.NewArgumentException(nameof (maxRunspaces), RunspacePoolInternal.resBaseName, "MaxPoolLessThan1");
        if (minRunspaces < 1)
          throw RunspacePoolInternal.tracer.NewArgumentException(nameof (minRunspaces), RunspacePoolInternal.resBaseName, "MinPoolLessThan1");
        this.maxPoolSz = minRunspaces <= maxRunspaces ? maxRunspaces : throw RunspacePoolInternal.tracer.NewArgumentException(nameof (minRunspaces), RunspacePoolInternal.resBaseName, "MinPoolGreaterThanMaxPool");
        this.minPoolSz = minRunspaces;
        this.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.BeforeOpen, (Exception) null);
        this.instanceId = Guid.NewGuid();
      }
    }

    internal RunspacePoolInternal()
    {
    }

    public Guid InstanceId
    {
      get
      {
        using (RunspacePoolInternal.tracer.TraceProperty())
          return this.instanceId;
      }
    }

    public bool IsDisposed
    {
      get
      {
        using (RunspacePoolInternal.tracer.TraceProperty())
          return this.isDisposed;
      }
    }

    public RunspacePoolStateInfo RunspacePoolStateInfo
    {
      get
      {
        using (RunspacePoolInternal.tracer.TraceProperty())
          return this.stateInfo;
      }
    }

    internal virtual PSPrimitiveDictionary GetApplicationPrivateData()
    {
      if (this.applicationPrivateData == null)
      {
        lock (this.syncObject)
        {
          if (this.applicationPrivateData == null)
            this.applicationPrivateData = new PSPrimitiveDictionary();
        }
      }
      return this.applicationPrivateData;
    }

    internal virtual void PropagateApplicationPrivateData(Runspace runspace) => runspace.SetApplicationPrivateData(this.GetApplicationPrivateData());

    public RunspaceConfiguration RunspaceConfiguration
    {
      get
      {
        using (RunspacePoolInternal.tracer.TraceProperty())
          return this.rsConfig;
      }
    }

    public InitialSessionState InitialSessionState
    {
      get
      {
        using (RunspacePoolInternal.tracer.TraceProperty())
          return this._initialSessionState;
      }
    }

    public virtual RunspaceConnectionInfo ConnectionInfo
    {
      get
      {
        using (RunspacePoolInternal.tracer.TraceProperty())
          return (RunspaceConnectionInfo) null;
      }
    }

    public event EventHandler<RunspacePoolStateChangedEventArgs> StateChanged;

    public event EventHandler<PSEventArgs> ForwardEvent;

    internal event EventHandler<RunspaceCreatedEventArgs> RunspaceCreated;

    internal virtual bool SetMaxRunspaces(int maxRunspaces)
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        bool flag = false;
        lock (this.pool)
        {
          if (maxRunspaces < this.minPoolSz)
            return false;
          if (maxRunspaces > this.maxPoolSz)
          {
            flag = true;
          }
          else
          {
            while (this.pool.Count > maxRunspaces)
              this.DestroyRunspace(this.pool.Pop());
          }
          this.maxPoolSz = maxRunspaces;
        }
        if (flag)
          this.EnqueueCheckAndStartRequestServicingThread((GetRunspaceAsyncResult) null, false);
        return true;
      }
    }

    public int GetMaxRunspaces()
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
        return this.maxPoolSz;
    }

    internal virtual bool SetMinRunspaces(int minRunspaces)
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        lock (this.pool)
        {
          if (minRunspaces < 1 || minRunspaces > this.maxPoolSz)
            return false;
          this.minPoolSz = minRunspaces;
        }
        return true;
      }
    }

    public int GetMinRunspaces()
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
        return this.minPoolSz;
    }

    internal virtual int GetAvailableRunspaces()
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          if (this.stateInfo.State == RunspacePoolState.Opened)
            return this.pool.Count + (this.maxPoolSz - this.totalRunspaces);
          if (this.stateInfo.State != RunspacePoolState.BeforeOpen && this.stateInfo.State != RunspacePoolState.Opening)
            throw new InvalidOperationException(ResourceManagerCache.GetResourceString("HostInterfaceExceptionsStrings", "RunspacePoolNotOpened"));
          return this.maxPoolSz;
        }
      }
    }

    public virtual void Open()
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
        this.CoreOpen(false, (AsyncCallback) null, (object) null);
    }

    public IAsyncResult BeginOpen(AsyncCallback callback, object state)
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
        return this.CoreOpen(true, callback, state);
    }

    public void EndOpen(IAsyncResult asyncResult)
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        if (asyncResult == null)
          throw RunspacePoolInternal.tracer.NewArgumentNullException(nameof (asyncResult));
        if (!(asyncResult is RunspacePoolAsyncResult runspacePoolAsyncResult) || runspacePoolAsyncResult.OwnerId != this.instanceId || !runspacePoolAsyncResult.IsAssociatedWithAsyncOpen)
          throw RunspacePoolInternal.tracer.NewArgumentException(nameof (asyncResult), RunspacePoolInternal.resBaseName, "AsyncResultNotOwned", (object) "IAsyncResult", (object) "BeginOpen");
        runspacePoolAsyncResult.EndInvoke();
      }
    }

    public virtual void Close()
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
        this.CoreClose(false, (AsyncCallback) null, (object) null);
    }

    public virtual IAsyncResult BeginClose(AsyncCallback callback, object state)
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
        return this.CoreClose(true, callback, state);
    }

    public virtual void EndClose(IAsyncResult asyncResult)
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        if (asyncResult == null)
          throw RunspacePoolInternal.tracer.NewArgumentNullException(nameof (asyncResult));
        if (!(asyncResult is RunspacePoolAsyncResult runspacePoolAsyncResult) || runspacePoolAsyncResult.OwnerId != this.instanceId || runspacePoolAsyncResult.IsAssociatedWithAsyncOpen)
          throw RunspacePoolInternal.tracer.NewArgumentException(nameof (asyncResult), RunspacePoolInternal.resBaseName, "AsyncResultNotOwned", (object) "IAsyncResult", (object) "BeginClose");
        runspacePoolAsyncResult.EndInvoke();
      }
    }

    public Runspace GetRunspace()
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        this.AssertPoolIsOpen();
        GetRunspaceAsyncResult runspace = (GetRunspaceAsyncResult) this.BeginGetRunspace((AsyncCallback) null, (object) null);
        runspace.AsyncWaitHandle.WaitOne();
        return runspace.Exception == null ? runspace.Runspace : throw runspace.Exception;
      }
    }

    public void ReleaseRunspace(Runspace runspace)
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        if (runspace == null)
          throw RunspacePoolInternal.tracer.NewArgumentNullException(nameof (runspace));
        this.AssertPoolIsOpen();
        bool flag1 = false;
        bool flag2 = false;
        lock (this.runspaceList)
        {
          if (!this.runspaceList.Contains(runspace))
            throw RunspacePoolInternal.tracer.NewInvalidOperationException(RunspacePoolInternal.resBaseName, "RunspaceNotBelongsToPool");
        }
        if (runspace.RunspaceStateInfo.State == RunspaceState.Opened)
        {
          lock (this.pool)
          {
            if (this.pool.Count < this.maxPoolSz)
            {
              flag1 = true;
              this.pool.Push(runspace);
            }
            else
            {
              flag1 = true;
              flag2 = true;
            }
          }
        }
        else
        {
          flag2 = true;
          flag1 = true;
        }
        if (flag2)
          this.DestroyRunspace(runspace);
        if (!flag1)
          return;
        this.EnqueueCheckAndStartRequestServicingThread((GetRunspaceAsyncResult) null, false);
      }
    }

    public virtual void Dispose(bool disposing)
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        if (this.isDisposed)
          return;
        if (disposing)
          this.Close();
        this.isDisposed = true;
      }
    }

    internal PSThreadOptions ThreadOptions
    {
      get => this.threadOptions;
      set => this.threadOptions = value;
    }

    internal ApartmentState ApartmentState
    {
      get => this.apartmentState;
      set => this.apartmentState = value;
    }

    internal IAsyncResult BeginGetRunspace(AsyncCallback callback, object state)
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        this.AssertPoolIsOpen();
        GetRunspaceAsyncResult requestToEnqueue = new GetRunspaceAsyncResult(this.InstanceId, callback, state);
        this.EnqueueCheckAndStartRequestServicingThread(requestToEnqueue, true);
        return (IAsyncResult) requestToEnqueue;
      }
    }

    internal void CancelGetRunspace(IAsyncResult asyncResult)
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        if (asyncResult == null)
          throw RunspacePoolInternal.tracer.NewArgumentNullException(nameof (asyncResult));
        if (!(asyncResult is GetRunspaceAsyncResult runspaceAsyncResult) || runspaceAsyncResult.OwnerId != this.instanceId)
          throw RunspacePoolInternal.tracer.NewArgumentException(nameof (asyncResult), RunspacePoolInternal.resBaseName, "AsyncResultNotOwned", (object) "IAsyncResult", (object) "BeginGetRunspace");
        runspaceAsyncResult.IsActive = false;
      }
    }

    internal Runspace EndGetRunspace(IAsyncResult asyncResult)
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        if (asyncResult == null)
          throw RunspacePoolInternal.tracer.NewArgumentNullException(nameof (asyncResult));
        if (!(asyncResult is GetRunspaceAsyncResult runspaceAsyncResult) || runspaceAsyncResult.OwnerId != this.instanceId)
          throw RunspacePoolInternal.tracer.NewArgumentException(nameof (asyncResult), RunspacePoolInternal.resBaseName, "AsyncResultNotOwned", (object) "IAsyncResult", (object) "BeginGetRunspace");
        runspaceAsyncResult.EndInvoke();
        return runspaceAsyncResult.Runspace;
      }
    }

    protected virtual IAsyncResult CoreOpen(
      bool isAsync,
      AsyncCallback callback,
      object asyncState)
    {
      lock (this.syncObject)
      {
        this.AssertIfStateIsBeforeOpen();
        this.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Opening, (Exception) null);
      }
      this.RaiseStateChangeEvent(this.stateInfo);
      if (isAsync)
      {
        AsyncResult asyncResult = (AsyncResult) new RunspacePoolAsyncResult(this.instanceId, callback, asyncState, true);
        ThreadPool.QueueUserWorkItem(new WaitCallback(this.OpenThreadProc), (object) asyncResult);
        return (IAsyncResult) asyncResult;
      }
      this.OpenHelper();
      return (IAsyncResult) null;
    }

    protected void OpenHelper()
    {
      try
      {
        this.pool.Push(this.CreateRunspace());
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        this.SetStateToBroken(ex);
        throw;
      }
      bool flag = false;
      lock (this.syncObject)
      {
        if (this.stateInfo.State == RunspacePoolState.Opening)
        {
          this.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Opened, (Exception) null);
          flag = true;
        }
      }
      if (!flag)
        return;
      this.RaiseStateChangeEvent(this.stateInfo);
    }

    private void SetStateToBroken(Exception reason)
    {
      bool flag = false;
      lock (this.syncObject)
      {
        if (this.stateInfo.State != RunspacePoolState.Opening)
        {
          if (this.stateInfo.State != RunspacePoolState.Opened)
            goto label_6;
        }
        this.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Broken, (Exception) null);
        flag = true;
      }
label_6:
      if (!flag)
        return;
      this.RaiseStateChangeEvent(new RunspacePoolStateInfo(this.stateInfo.State, reason));
    }

    protected void OpenThreadProc(object o)
    {
      AsyncResult asyncResult = (AsyncResult) o;
      Exception exception = (Exception) null;
      try
      {
        this.OpenHelper();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        exception = ex;
      }
      finally
      {
        asyncResult.SetAsCompleted(exception);
      }
    }

    private IAsyncResult CoreClose(
      bool isAsync,
      AsyncCallback callback,
      object asyncState)
    {
      lock (this.syncObject)
      {
        if (this.stateInfo.State == RunspacePoolState.Closed || this.stateInfo.State == RunspacePoolState.Broken || this.stateInfo.State == RunspacePoolState.Closing)
        {
          if (!isAsync)
            return (IAsyncResult) null;
          RunspacePoolAsyncResult runspacePoolAsyncResult = new RunspacePoolAsyncResult(this.instanceId, callback, asyncState, false);
          runspacePoolAsyncResult.SetAsCompleted((Exception) null);
          return (IAsyncResult) runspacePoolAsyncResult;
        }
        this.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Closing, (Exception) null);
      }
      this.RaiseStateChangeEvent(this.stateInfo);
      if (isAsync)
      {
        RunspacePoolAsyncResult runspacePoolAsyncResult = new RunspacePoolAsyncResult(this.instanceId, callback, asyncState, false);
        ThreadPool.QueueUserWorkItem(new WaitCallback(this.CloseThreadProc), (object) runspacePoolAsyncResult);
        return (IAsyncResult) runspacePoolAsyncResult;
      }
      this.CloseHelper();
      return (IAsyncResult) null;
    }

    private void CloseHelper()
    {
      try
      {
        this.InternalClearAllResources();
      }
      finally
      {
        this.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Closed, (Exception) null);
        this.RaiseStateChangeEvent(this.stateInfo);
      }
    }

    private void CloseThreadProc(object o)
    {
      AsyncResult asyncResult = (AsyncResult) o;
      Exception exception = (Exception) null;
      try
      {
        this.CloseHelper();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        exception = ex;
      }
      finally
      {
        asyncResult.SetAsCompleted(exception);
      }
    }

    protected void RaiseStateChangeEvent(RunspacePoolStateInfo stateInfo)
    {
      if (this.StateChanged == null)
        return;
      this.StateChanged((object) this, new RunspacePoolStateChangedEventArgs(stateInfo));
    }

    internal void AssertPoolIsOpen()
    {
      lock (this.syncObject)
      {
        if (this.stateInfo.State != RunspacePoolState.Opened)
          throw new InvalidRunspacePoolStateException(ResourceManagerCache.FormatResourceString(RunspacePoolInternal.resBaseName, "InvalidRunspacePoolState", (object) RunspacePoolState.Opened, (object) this.stateInfo.State), this.stateInfo.State, RunspacePoolState.Opened);
      }
    }

    protected Runspace CreateRunspace()
    {
      Runspace runspace = this.rsConfig == null ? RunspaceFactory.CreateRunspaceFromSessionStateNoClone(this.host, this._initialSessionState) : RunspaceFactory.CreateRunspace(this.host, this.rsConfig);
      runspace.ThreadOptions = this.ThreadOptions == PSThreadOptions.Default ? PSThreadOptions.ReuseThread : this.ThreadOptions;
      runspace.ApartmentState = this.ApartmentState;
      this.PropagateApplicationPrivateData(runspace);
      runspace.Open();
      runspace.Events.ForwardEvent += new EventHandler<PSEventArgs>(this.OnRunspaceForwardEvent);
      lock (this.runspaceList)
      {
        this.runspaceList.Add(runspace);
        this.totalRunspaces = this.runspaceList.Count;
      }
      if (this.RunspaceCreated != null)
        this.RunspaceCreated((object) this, new RunspaceCreatedEventArgs(runspace));
      return runspace;
    }

    protected void DestroyRunspace(Runspace runspace)
    {
      runspace.Dispose();
      lock (this.runspaceList)
      {
        this.runspaceList.Remove(runspace);
        this.totalRunspaces = this.runspaceList.Count;
      }
    }

    protected void CleanupCallback(object state)
    {
      while (this.totalRunspaces > this.minPoolSz && this.stateInfo.State != RunspacePoolState.Closing)
      {
        Runspace runspace = (Runspace) null;
        lock (this.pool)
        {
          if (this.pool.Count <= 0)
            break;
          runspace = this.pool.Pop();
        }
        this.DestroyRunspace(runspace);
      }
    }

    private void InternalClearAllResources()
    {
      Exception exception = (Exception) new InvalidRunspacePoolStateException(ResourceManagerCache.FormatResourceString(RunspacePoolInternal.resBaseName, "InvalidRunspacePoolState", (object) RunspacePoolState.Opened, (object) this.stateInfo.State), this.stateInfo.State, RunspacePoolState.Opened);
      lock (this.runspaceRequestQueue)
      {
        while (this.runspaceRequestQueue.Count > 0)
          this.runspaceRequestQueue.Dequeue().SetAsCompleted(exception);
      }
      lock (this.ultimateRequestQueue)
      {
        while (this.ultimateRequestQueue.Count > 0)
          this.ultimateRequestQueue.Dequeue().SetAsCompleted(exception);
      }
      List<Runspace> runspaceList = new List<Runspace>();
      lock (this.runspaceList)
      {
        runspaceList.AddRange((IEnumerable<Runspace>) this.runspaceList);
        this.runspaceList.Clear();
      }
      for (int index = runspaceList.Count - 1; index >= 0; --index)
      {
        try
        {
          runspaceList[index].Close();
        }
        catch (InvalidRunspaceStateException ex)
        {
          CommandProcessorBase.CheckForSevereException((Exception) ex);
          RunspacePoolInternal.tracer.TraceException((Exception) ex);
        }
      }
      lock (this.pool)
        this.pool.Clear();
    }

    protected void EnqueueCheckAndStartRequestServicingThread(
      GetRunspaceAsyncResult requestToEnqueue,
      bool useCallingThread)
    {
      bool flag = false;
      lock (this.runspaceRequestQueue)
      {
        if (requestToEnqueue != null)
          this.runspaceRequestQueue.Enqueue(requestToEnqueue);
        if (this.isServicingRequests)
          return;
        if (this.runspaceRequestQueue.Count + this.ultimateRequestQueue.Count > 0)
        {
          lock (this.pool)
          {
            if (this.pool.Count <= 0)
            {
              if (this.totalRunspaces >= this.maxPoolSz)
                goto label_15;
            }
            this.isServicingRequests = true;
            if (useCallingThread && this.ultimateRequestQueue.Count == 0)
              flag = true;
            else
              ThreadPool.QueueUserWorkItem(new WaitCallback(this.ServicePendingRequests), (object) false);
          }
        }
      }
label_15:
      if (!flag)
        return;
      this.ServicePendingRequests((object) true);
    }

    protected void ServicePendingRequests(object useCallingThreadState)
    {
      if (this.stateInfo.State == RunspacePoolState.Closed || this.stateInfo.State == RunspacePoolState.Closing)
        return;
      bool flag = (bool) useCallingThreadState;
      GetRunspaceAsyncResult runspaceAsyncResult = (GetRunspaceAsyncResult) null;
      try
      {
        RunspacePoolInternal.tracer.WriteLine("Servicing thread is starting", new object[0]);
label_4:
        lock (this.ultimateRequestQueue)
        {
          while (this.ultimateRequestQueue.Count > 0)
          {
            if (this.stateInfo.State == RunspacePoolState.Closing)
              return;
            Runspace runspace;
            lock (this.pool)
            {
              if (this.pool.Count > 0)
              {
                runspace = this.pool.Pop();
              }
              else
              {
                if (this.totalRunspaces >= this.maxPoolSz)
                  return;
                runspace = this.CreateRunspace();
              }
            }
            runspaceAsyncResult = this.ultimateRequestQueue.Dequeue();
            if (!runspaceAsyncResult.IsActive)
            {
              lock (this.pool)
                this.pool.Push(runspace);
              runspaceAsyncResult.Release();
            }
            else
            {
              runspaceAsyncResult.Runspace = runspace;
              RunspacePoolInternal.tracer.WriteLine("Found a runspace to return", new object[0]);
              if (!flag)
                ThreadPool.QueueUserWorkItem(new WaitCallback(runspaceAsyncResult.DoComplete));
              else
                goto label_34;
            }
          }
        }
        lock (this.runspaceRequestQueue)
        {
          if (this.runspaceRequestQueue.Count != 0)
          {
            while (this.runspaceRequestQueue.Count > 0)
              this.ultimateRequestQueue.Enqueue(this.runspaceRequestQueue.Dequeue());
            goto label_4;
          }
        }
      }
      finally
      {
        lock (this.runspaceRequestQueue)
        {
          RunspacePoolInternal.tracer.WriteLine("Servicing thread is exiting", new object[0]);
          this.isServicingRequests = false;
          this.EnqueueCheckAndStartRequestServicingThread((GetRunspaceAsyncResult) null, false);
        }
      }
label_34:
      if (!flag || runspaceAsyncResult == null)
        return;
      runspaceAsyncResult.DoComplete((object) null);
    }

    protected void AssertIfStateIsBeforeOpen()
    {
      using (RunspacePoolInternal.tracer.TraceMethod())
      {
        if (this.stateInfo.State != RunspacePoolState.BeforeOpen)
        {
          InvalidRunspacePoolStateException poolStateException = new InvalidRunspacePoolStateException(ResourceManagerCache.FormatResourceString(RunspacePoolInternal.resBaseName, "CannotOpenAgain", (object) this.stateInfo.State.ToString()), this.stateInfo.State, RunspacePoolState.BeforeOpen);
          RunspacePoolInternal.tracer.TraceException((Exception) poolStateException);
          throw poolStateException;
        }
      }
    }

    protected virtual void OnForwardEvent(PSEventArgs e)
    {
      EventHandler<PSEventArgs> forwardEvent = this.ForwardEvent;
      if (forwardEvent == null)
        return;
      forwardEvent((object) this, e);
    }

    private void OnRunspaceForwardEvent(object sender, PSEventArgs e)
    {
      if (!e.ForwardEvent)
        return;
      this.OnForwardEvent(e);
    }
  }
}
