// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.GetRunspaceAsyncResult
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  internal sealed class GetRunspaceAsyncResult : AsyncResult
  {
    private Runspace runspace;
    private bool isActive;

    internal GetRunspaceAsyncResult(Guid ownerId, AsyncCallback callback, object state)
      : base(ownerId, callback, state)
    {
      using (AsyncResult.tracer.TraceConstructor((object) this))
        this.isActive = true;
    }

    internal Runspace Runspace
    {
      get
      {
        using (AsyncResult.tracer.TraceProperty())
          return this.runspace;
      }
      set
      {
        using (AsyncResult.tracer.TraceProperty())
          this.runspace = value;
      }
    }

    internal bool IsActive
    {
      get
      {
        using (AsyncResult.tracer.TraceProperty())
        {
          lock (this.SyncObject)
            return this.isActive;
        }
      }
      set
      {
        using (AsyncResult.tracer.TraceProperty())
        {
          lock (this.SyncObject)
            this.isActive = value;
        }
      }
    }

    internal void DoComplete(object state)
    {
      using (AsyncResult.tracer.TraceMethod())
        this.SetAsCompleted((Exception) null);
    }
  }
}
