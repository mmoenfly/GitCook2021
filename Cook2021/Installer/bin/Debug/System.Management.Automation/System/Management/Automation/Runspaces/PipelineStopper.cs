// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PipelineStopper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation.Runspaces
{
  internal class PipelineStopper
  {
    private Stack<PipelineProcessor> _stack = new Stack<PipelineProcessor>();
    private object _syncRoot = new object();
    private LocalPipeline _localPipeline;
    private bool _stopping;
    [TraceSource("PipelineStopper", "PipelineStopper")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (PipelineStopper), nameof (PipelineStopper));

    internal PipelineStopper(LocalPipeline localPipeline) => this._localPipeline = localPipeline;

    internal bool IsStopping
    {
      get => this._stopping;
      set => this._stopping = value;
    }

    internal void Push(PipelineProcessor item)
    {
      if (item == null)
        throw PipelineStopper._trace.NewArgumentNullException(nameof (item));
      lock (this._syncRoot)
      {
        if (this._stopping)
        {
          PipelineStoppedException stoppedException = new PipelineStoppedException();
          PipelineStopper._trace.TraceException((Exception) stoppedException);
          throw stoppedException;
        }
        this._stack.Push(item);
      }
      item.LocalPipeline = this._localPipeline;
    }

    internal void Pop()
    {
      lock (this._syncRoot)
      {
        if (this._stopping || this._stack.Count <= 0)
          return;
        this._stack.Pop();
      }
    }

    internal void Stop()
    {
      PipelineProcessor[] array;
      lock (this._syncRoot)
      {
        if (this._stopping)
          return;
        this._stopping = true;
        array = this._stack.ToArray();
      }
      foreach (PipelineProcessor pipelineProcessor in array)
        pipelineProcessor.Stop();
    }
  }
}
