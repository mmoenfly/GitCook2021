// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspacePool
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;
using System.Management.Automation.Runspaces.Internal;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
  public sealed class RunspacePool : IDisposable
  {
    private const string ResourceBase = "RunspacePoolStrings";
    [TraceSource("RunspacePool", "Powershell hosting interfaces")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (RunspacePool), "Powershell hosting interfaces");
    private RunspacePoolInternal internalPool;
    private object syncObject = new object();
    private bool isRemote;

    private event EventHandler<RunspacePoolStateChangedEventArgs> InternalStateChanged;

    private event EventHandler<PSEventArgs> InternalForwardEvent;

    private event EventHandler<RunspaceCreatedEventArgs> InternalRunspaceCreated;

    internal RunspacePool(
      int minRunspaces,
      int maxRunspaces,
      RunspaceConfiguration runspaceConfiguration,
      PSHost host)
    {
      using (RunspacePool.tracer.TraceConstructor((object) this))
        this.internalPool = new RunspacePoolInternal(minRunspaces, maxRunspaces, runspaceConfiguration, host);
    }

    internal RunspacePool(
      int minRunspaces,
      int maxRunspaces,
      InitialSessionState initialSessionState,
      PSHost host)
    {
      using (RunspacePool.tracer.TraceConstructor((object) this))
        this.internalPool = new RunspacePoolInternal(minRunspaces, maxRunspaces, initialSessionState, host);
    }

    internal RunspacePool(
      int minRunspaces,
      int maxRunspaces,
      TypeTable typeTable,
      PSHost host,
      PSPrimitiveDictionary applicationArguments,
      RunspaceConnectionInfo connectionInfo)
    {
      using (RunspacePool.tracer.TraceConstructor((object) this))
      {
        switch (connectionInfo)
        {
          case WSManConnectionInfo _:
          case NewProcessConnectionInfo _:
            this.internalPool = (RunspacePoolInternal) new RemoteRunspacePoolInternal(minRunspaces, maxRunspaces, typeTable, host, applicationArguments, connectionInfo);
            this.isRemote = true;
            break;
          default:
            throw new NotSupportedException();
        }
      }
    }

    public Guid InstanceId
    {
      get
      {
        using (RunspacePool.tracer.TraceProperty())
          return this.internalPool.InstanceId;
      }
    }

    public bool IsDisposed
    {
      get
      {
        using (RunspacePool.tracer.TraceProperty())
          return this.internalPool.IsDisposed;
      }
    }

    public RunspacePoolStateInfo RunspacePoolStateInfo
    {
      get
      {
        using (RunspacePool.tracer.TraceProperty())
          return this.internalPool.RunspacePoolStateInfo;
      }
    }

    public InitialSessionState InitialSessionState
    {
      get
      {
        using (RunspacePool.tracer.TraceProperty())
          return this.internalPool.InitialSessionState;
      }
    }

    public RunspaceConnectionInfo ConnectionInfo
    {
      get
      {
        using (RunspacePool.tracer.TraceProperty())
          return this.internalPool.ConnectionInfo;
      }
    }

    public event EventHandler<RunspacePoolStateChangedEventArgs> StateChanged
    {
      add
      {
        lock (this.syncObject)
        {
          bool flag = null == this.InternalStateChanged;
          this.InternalStateChanged += value;
          if (!flag)
            return;
          this.internalPool.StateChanged += new EventHandler<RunspacePoolStateChangedEventArgs>(this.OnStateChanged);
        }
      }
      remove
      {
        lock (this.syncObject)
        {
          this.InternalStateChanged -= value;
          if (this.InternalStateChanged != null)
            return;
          this.internalPool.StateChanged -= new EventHandler<RunspacePoolStateChangedEventArgs>(this.OnStateChanged);
        }
      }
    }

    private void OnStateChanged(object source, RunspacePoolStateChangedEventArgs args)
    {
      if (this.InternalStateChanged == null)
        return;
      this.InternalStateChanged((object) this, args);
    }

    internal event EventHandler<PSEventArgs> ForwardEvent
    {
      add
      {
        lock (this.syncObject)
        {
          bool flag = this.InternalForwardEvent == null;
          this.InternalForwardEvent += value;
          if (!flag)
            return;
          this.internalPool.ForwardEvent += new EventHandler<PSEventArgs>(this.OnInternalPoolForwardEvent);
        }
      }
      remove
      {
        lock (this.syncObject)
        {
          this.InternalForwardEvent -= value;
          if (this.InternalForwardEvent != null)
            return;
          this.internalPool.ForwardEvent -= new EventHandler<PSEventArgs>(this.OnInternalPoolForwardEvent);
        }
      }
    }

    private void OnInternalPoolForwardEvent(object sender, PSEventArgs e) => this.OnEventForwarded(e);

    private void OnEventForwarded(PSEventArgs e)
    {
      EventHandler<PSEventArgs> internalForwardEvent = this.InternalForwardEvent;
      if (internalForwardEvent == null)
        return;
      internalForwardEvent((object) this, e);
    }

    internal event EventHandler<RunspaceCreatedEventArgs> RunspaceCreated
    {
      add
      {
        lock (this.syncObject)
        {
          bool flag = null == this.InternalRunspaceCreated;
          this.InternalRunspaceCreated += value;
          if (!flag)
            return;
          this.internalPool.RunspaceCreated += new EventHandler<RunspaceCreatedEventArgs>(this.OnRunspaceCreated);
        }
      }
      remove
      {
        lock (this.syncObject)
        {
          this.InternalRunspaceCreated -= value;
          if (this.InternalRunspaceCreated != null)
            return;
          this.internalPool.RunspaceCreated -= new EventHandler<RunspaceCreatedEventArgs>(this.OnRunspaceCreated);
        }
      }
    }

    private void OnRunspaceCreated(object source, RunspaceCreatedEventArgs args)
    {
      if (this.InternalRunspaceCreated == null)
        return;
      this.InternalRunspaceCreated((object) this, args);
    }

    public bool SetMaxRunspaces(int maxRunspaces)
    {
      using (RunspacePool.tracer.TraceMethod())
        return this.internalPool.SetMaxRunspaces(maxRunspaces);
    }

    public int GetMaxRunspaces()
    {
      using (RunspacePool.tracer.TraceMethod())
        return this.internalPool.GetMaxRunspaces();
    }

    public bool SetMinRunspaces(int minRunspaces)
    {
      using (RunspacePool.tracer.TraceMethod())
        return this.internalPool.SetMinRunspaces(minRunspaces);
    }

    public int GetMinRunspaces()
    {
      using (RunspacePool.tracer.TraceMethod())
        return this.internalPool.GetMinRunspaces();
    }

    public int GetAvailableRunspaces()
    {
      using (RunspacePool.tracer.TraceMethod())
        return this.internalPool.GetAvailableRunspaces();
    }

    public void Open()
    {
      using (RunspacePool.tracer.TraceMethod())
        this.internalPool.Open();
    }

    public IAsyncResult BeginOpen(AsyncCallback callback, object state)
    {
      using (RunspacePool.tracer.TraceMethod())
        return this.internalPool.BeginOpen(callback, state);
    }

    public void EndOpen(IAsyncResult asyncResult)
    {
      using (RunspacePool.tracer.TraceMethod())
        this.internalPool.EndOpen(asyncResult);
    }

    public void Close()
    {
      using (RunspacePool.tracer.TraceMethod())
        this.internalPool.Close();
    }

    public IAsyncResult BeginClose(AsyncCallback callback, object state)
    {
      using (RunspacePool.tracer.TraceMethod())
        return this.internalPool.BeginClose(callback, state);
    }

    public void EndClose(IAsyncResult asyncResult)
    {
      using (RunspacePool.tracer.TraceMethod())
        this.internalPool.EndClose(asyncResult);
    }

    public void Dispose()
    {
      using (RunspacePool.tracer.TraceMethod())
      {
        this.internalPool.Dispose(true);
        GC.SuppressFinalize((object) this);
      }
    }

    public PSPrimitiveDictionary GetApplicationPrivateData() => this.internalPool.GetApplicationPrivateData();

    public PSThreadOptions ThreadOptions
    {
      get => this.internalPool.ThreadOptions;
      set
      {
        if (this.RunspacePoolStateInfo.State != RunspacePoolState.BeforeOpen)
          throw new InvalidRunspacePoolStateException(ResourceManagerCache.GetResourceString("RunspacePoolStrings", "ChangePropertyAfterOpen"));
        this.internalPool.ThreadOptions = value;
      }
    }

    public ApartmentState ApartmentState
    {
      get => this.internalPool.ApartmentState;
      set
      {
        if (this.RunspacePoolStateInfo.State != RunspacePoolState.BeforeOpen)
          throw new InvalidRunspacePoolStateException(ResourceManagerCache.GetResourceString("RunspacePoolStrings", "ChangePropertyAfterOpen"));
        this.internalPool.ApartmentState = value;
      }
    }

    internal IAsyncResult BeginGetRunspace(AsyncCallback callback, object state) => this.internalPool.BeginGetRunspace(callback, state);

    internal void CancelGetRunspace(IAsyncResult asyncResult) => this.internalPool.CancelGetRunspace(asyncResult);

    internal Runspace EndGetRunspace(IAsyncResult asyncResult) => this.internalPool.EndGetRunspace(asyncResult);

    internal void ReleaseRunspace(Runspace runspace) => this.internalPool.ReleaseRunspace(runspace);

    internal bool IsRemote => this.isRemote;

    internal RemoteRunspacePoolInternal RemoteRunspacePoolInternal => this.internalPool is RemoteRunspacePoolInternal ? (RemoteRunspacePoolInternal) this.internalPool : (RemoteRunspacePoolInternal) null;

    internal void AssertPoolIsOpen()
    {
      using (RunspacePool.tracer.TraceMethod())
        this.internalPool.AssertPoolIsOpen();
    }
  }
}
