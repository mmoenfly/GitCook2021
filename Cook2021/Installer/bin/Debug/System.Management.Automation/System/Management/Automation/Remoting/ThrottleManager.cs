// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ThrottleManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation.Remoting
{
  internal class ThrottleManager : IDisposable
  {
    private int throttleLimit = ThrottleManager.DEFAULT_THROTTLE_LIMIT;
    private static int DEFAULT_THROTTLE_LIMIT = 32;
    private static int THROTTLE_LIMIT_MAX = int.MaxValue;
    private List<IThrottleOperation> operationsQueue;
    private List<IThrottleOperation> startOperationQueue;
    private List<IThrottleOperation> stopOperationQueue;
    private object syncObject;
    [TraceSource("ThrottleManager", "ThrottleManager")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ThrottleManager), nameof (ThrottleManager));
    private bool submitComplete;
    private bool stopping;

    internal int ThrottleLimit
    {
      set
      {
        if (value <= 0 || value > ThrottleManager.THROTTLE_LIMIT_MAX)
          return;
        this.throttleLimit = value;
      }
      get => this.throttleLimit;
    }

    internal void SubmitOperations(List<IThrottleOperation> operations)
    {
      using (ThrottleManager.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          if (this.submitComplete)
            throw new InvalidOperationException();
          foreach (IThrottleOperation operation in operations)
            this.operationsQueue.Add(operation);
        }
        this.StartOperationsFromQueue();
      }
    }

    internal void AddOperation(IThrottleOperation operation)
    {
      using (ThrottleManager.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          if (this.submitComplete)
            throw new InvalidOperationException();
          this.operationsQueue.Add(operation);
        }
        this.StartOperationsFromQueue();
      }
    }

    internal void StopAllOperations()
    {
      using (ThrottleManager.tracer.TraceMethod())
      {
        bool flag = false;
        lock (this.syncObject)
        {
          if (!this.stopping)
            this.stopping = true;
          else
            flag = true;
        }
        if (flag)
        {
          this.RaiseThrottleManagerEvents();
        }
        else
        {
          IThrottleOperation[] array;
          lock (this.syncObject)
          {
            this.submitComplete = true;
            this.operationsQueue.Clear();
            array = new IThrottleOperation[this.startOperationQueue.Count];
            this.startOperationQueue.CopyTo(array);
            foreach (IThrottleOperation ithrottleOperation in array)
            {
              this.stopOperationQueue.Add(ithrottleOperation);
              ithrottleOperation.IgnoreStop = true;
            }
          }
          foreach (IThrottleOperation ithrottleOperation in array)
            ithrottleOperation.StopOperation();
          this.RaiseThrottleManagerEvents();
        }
      }
    }

    internal void StopOperation(IThrottleOperation operation)
    {
      using (ThrottleManager.tracer.TraceMethod())
      {
        if (operation.IgnoreStop)
          return;
        if (this.operationsQueue.IndexOf(operation) != -1)
        {
          lock (this.syncObject)
          {
            if (this.operationsQueue.IndexOf(operation) != -1)
            {
              this.operationsQueue.Remove(operation);
              this.RaiseThrottleManagerEvents();
              return;
            }
          }
        }
        lock (this.syncObject)
        {
          this.stopOperationQueue.Add(operation);
          operation.IgnoreStop = true;
        }
        operation.StopOperation();
      }
    }

    internal void EndSubmitOperations()
    {
      using (ThrottleManager.tracer.TraceMethod())
      {
        lock (this.syncObject)
          this.submitComplete = true;
        this.RaiseThrottleManagerEvents();
      }
    }

    internal event EventHandler<EventArgs> ThrottleComplete;

    public ThrottleManager()
    {
      this.operationsQueue = new List<IThrottleOperation>();
      this.startOperationQueue = new List<IThrottleOperation>();
      this.stopOperationQueue = new List<IThrottleOperation>();
      this.syncObject = new object();
    }

    private void OperationCompleteHandler(object source, OperationStateEventArgs stateEventArgs)
    {
      using (ThrottleManager.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          IThrottleOperation ithrottleOperation = source as IThrottleOperation;
          if (stateEventArgs.OperationState == OperationState.StartComplete)
          {
            this.startOperationQueue.RemoveAt(this.startOperationQueue.IndexOf(ithrottleOperation));
          }
          else
          {
            int index1 = this.startOperationQueue.IndexOf(ithrottleOperation);
            if (index1 != -1)
              this.startOperationQueue.RemoveAt(index1);
            int index2 = this.stopOperationQueue.IndexOf(ithrottleOperation);
            if (index2 != -1)
              this.stopOperationQueue.RemoveAt(index2);
            ithrottleOperation.IgnoreStop = true;
          }
        }
        this.RaiseThrottleManagerEvents();
        this.StartOneOperationFromQueue();
      }
    }

    private void StartOneOperationFromQueue()
    {
      using (ThrottleManager.tracer.TraceMethod())
      {
        IThrottleOperation ithrottleOperation = (IThrottleOperation) null;
        lock (this.syncObject)
        {
          if (this.operationsQueue.Count > 0)
          {
            ithrottleOperation = this.operationsQueue[0];
            this.operationsQueue.RemoveAt(0);
            ithrottleOperation.OperationComplete += new EventHandler<OperationStateEventArgs>(this.OperationCompleteHandler);
            this.startOperationQueue.Add(ithrottleOperation);
          }
        }
        ithrottleOperation?.StartOperation();
      }
    }

    private void StartOperationsFromQueue()
    {
      int num1 = 0;
      int num2 = 0;
      lock (this.syncObject)
      {
        num1 = this.startOperationQueue.Count;
        num2 = this.operationsQueue.Count;
      }
      int num3 = this.throttleLimit - num1;
      if (num3 <= 0)
        return;
      int num4 = num3 > num2 ? num2 : num3;
      for (int index = 0; index < num4; ++index)
        this.StartOneOperationFromQueue();
    }

    private void RaiseThrottleManagerEvents()
    {
      bool flag = false;
      lock (this.syncObject)
      {
        if (this.submitComplete)
        {
          if (this.startOperationQueue.Count == 0)
          {
            if (this.stopOperationQueue.Count == 0)
            {
              if (this.operationsQueue.Count == 0)
                flag = true;
            }
          }
        }
      }
      if (!flag || this.ThrottleComplete == null)
        return;
      this.ThrottleComplete((object) this, new EventArgs());
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      this.StopAllOperations();
    }
  }
}
