// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PipelineThread
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Threading;

namespace System.Management.Automation.Runspaces
{
  internal class PipelineThread : IDisposable
  {
    [TraceSource("PipelineThread", "PipelineThread")]
    private static PSTraceSource trace = PSTraceSource.GetTracer(nameof (PipelineThread), nameof (PipelineThread));
    private Thread worker;
    private ThreadStart workItem;
    private AutoResetEvent workItemReady;
    private bool closed;

    internal PipelineThread(ApartmentState apartmentState)
    {
      using (PipelineThread.trace.TraceConstructor((object) this))
      {
        this.worker = new Thread(new ThreadStart(this.WorkerProc), LocalPipeline.MaxStack);
        this.workItem = (ThreadStart) null;
        this.workItemReady = new AutoResetEvent(false);
        this.closed = false;
        if (apartmentState == ApartmentState.Unknown)
          return;
        this.worker.SetApartmentState(apartmentState);
      }
    }

    internal Thread Worker => this.worker;

    internal void Start(ThreadStart workItem)
    {
      using (PipelineThread.trace.TraceMethod((object) this))
      {
        if (this.closed)
          return;
        this.workItem = workItem;
        this.workItemReady.Set();
        if (this.worker.ThreadState != ThreadState.Unstarted)
          return;
        this.worker.Start();
      }
    }

    internal void Close()
    {
      using (PipelineThread.trace.TraceMethod((object) this))
        this.Dispose();
    }

    private void WorkerProc()
    {
      using (PipelineThread.trace.TraceMethod((object) this))
      {
        while (!this.closed)
        {
          this.workItemReady.WaitOne();
          if (!this.closed)
            this.workItem();
        }
      }
    }

    public void Dispose()
    {
      if (this.closed)
        return;
      this.closed = true;
      this.workItemReady.Set();
      if (this.worker.ThreadState != ThreadState.Unstarted && Thread.CurrentThread != this.worker)
        this.worker.Join();
      this.workItemReady.Close();
      GC.SuppressFinalize((object) this);
    }

    ~PipelineThread() => this.Dispose();
  }
}
