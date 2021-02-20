// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Operation
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Threading;

namespace System.Management.Automation.Remoting
{
  internal class Operation : IThrottleOperation
  {
    private ThreadStart workerThreadDelegate;
    private Thread workerThreadStart;
    private Thread workerThreadStop;
    private int sleepTime = 100;
    private bool done;

    public bool Done
    {
      set => this.done = value;
      get => this.done;
    }

    public int SleepTime
    {
      set => this.sleepTime = value;
      get => this.sleepTime;
    }

    private void WorkerThreadMethodStart()
    {
      Thread.Sleep(this.sleepTime);
      this.done = true;
      this.OperationComplete((object) this, new OperationStateEventArgs()
      {
        OperationState = OperationState.StartComplete
      });
    }

    private void WorkerThreadMethodStop()
    {
      this.workerThreadStart.Abort();
      this.OperationComplete((object) this, new OperationStateEventArgs()
      {
        OperationState = OperationState.StopComplete
      });
    }

    internal Operation()
    {
      this.done = false;
      this.workerThreadDelegate = new ThreadStart(this.WorkerThreadMethodStart);
      this.workerThreadStart = new Thread(this.workerThreadDelegate);
      this.workerThreadDelegate = new ThreadStart(this.WorkerThreadMethodStop);
      this.workerThreadStop = new Thread(this.workerThreadDelegate);
    }

    internal override void StartOperation() => this.workerThreadStart.Start();

    internal override void StopOperation() => this.workerThreadStop.Start();

    internal override event System.EventHandler<OperationStateEventArgs> OperationComplete;

    internal event System.EventHandler<EventArgs> InternalEvent;

    internal event System.EventHandler<EventArgs> EventHandler
    {
      add
      {
        bool flag = null == this.InternalEvent;
        this.InternalEvent += value;
        if (!flag)
          return;
        this.OperationComplete += new System.EventHandler<OperationStateEventArgs>(this.Operation_OperationComplete);
      }
      remove => this.InternalEvent -= value;
    }

    private void Operation_OperationComplete(object sender, OperationStateEventArgs e)
    {
      if (this.InternalEvent == null)
        return;
      this.InternalEvent(sender, (EventArgs) e);
    }

    internal static void SubmitOperations(List<object> operations, ThrottleManager throttleManager)
    {
      List<IThrottleOperation> operations1 = new List<IThrottleOperation>();
      foreach (object operation in operations)
        operations1.Add((IThrottleOperation) operation);
      throttleManager.SubmitOperations(operations1);
    }

    internal static void AddOperation(object operation, ThrottleManager throttleManager) => throttleManager.AddOperation((IThrottleOperation) operation);
  }
}
