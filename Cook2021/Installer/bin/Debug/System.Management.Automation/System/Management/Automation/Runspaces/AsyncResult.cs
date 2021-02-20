// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.AsyncResult
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Threading;

namespace System.Management.Automation.Runspaces
{
  internal class AsyncResult : IAsyncResult
  {
    [TraceSource("PowerShellHosting", "Powershell hosting interfaces")]
    protected static PSTraceSource tracer = PSTraceSource.GetTracer("PowerShellHosting", "Powershell hosting interfaces");
    private Guid ownerId;
    private bool isCompleted;
    private ManualResetEvent completedWaitHandle;
    private Exception exception;
    private AsyncCallback callback;
    private object state;
    private object syncObject = new object();

    internal AsyncResult(Guid ownerId, AsyncCallback callback, object state)
    {
      using (AsyncResult.tracer.TraceConstructor((object) this))
      {
        this.ownerId = ownerId;
        this.callback = callback;
        this.state = state;
      }
    }

    public bool CompletedSynchronously
    {
      get
      {
        using (AsyncResult.tracer.TraceProperty())
          return false;
      }
    }

    public bool IsCompleted
    {
      get
      {
        using (AsyncResult.tracer.TraceProperty())
          return this.isCompleted;
      }
    }

    public object AsyncState => this.state;

    public WaitHandle AsyncWaitHandle
    {
      get
      {
        if (this.completedWaitHandle == null)
        {
          lock (this.syncObject)
          {
            if (this.completedWaitHandle == null)
              this.completedWaitHandle = new ManualResetEvent(this.isCompleted);
          }
        }
        return (WaitHandle) this.completedWaitHandle;
      }
    }

    internal Guid OwnerId => this.ownerId;

    internal Exception Exception => this.exception;

    internal AsyncCallback Callback => this.callback;

    internal object SyncObject => this.syncObject;

    internal void SetAsCompleted(Exception exception)
    {
      if (this.isCompleted)
        return;
      lock (this.syncObject)
      {
        if (this.isCompleted)
          return;
        this.exception = exception;
        this.isCompleted = true;
        this.SignalWaitHandle();
      }
      if (this.callback == null)
        return;
      this.callback((IAsyncResult) this);
    }

    internal void Release()
    {
      if (this.isCompleted)
        return;
      this.isCompleted = true;
      this.SignalWaitHandle();
    }

    internal void SignalWaitHandle()
    {
      lock (this.syncObject)
      {
        if (this.completedWaitHandle == null)
          return;
        this.completedWaitHandle.Set();
      }
    }

    internal void EndInvoke()
    {
      this.AsyncWaitHandle.WaitOne();
      this.AsyncWaitHandle.Close();
      this.completedWaitHandle = (ManualResetEvent) null;
      if (this.exception != null)
        throw this.exception;
    }
  }
}
