// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.ObjectStreamBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace System.Management.Automation.Internal
{
  internal abstract class ObjectStreamBase : IDisposable
  {
    [TraceSource("ObjectStream", "Read/Write memory-based object stream")]
    protected static PSTraceSource _trace = PSTraceSource.GetTracer("ObjectStream", "Read/Write memory-based object stream");

    internal event EventHandler DataReady;

    internal void FireDataReadyEvent(object source, EventArgs args)
    {
      if (this.DataReady == null)
        return;
      this.DataReady(source, args);
    }

    internal abstract int MaxCapacity { get; }

    internal virtual WaitHandle ReadHandle => throw ObjectStreamBase._trace.NewNotSupportedException();

    internal virtual WaitHandle WriteHandle => throw ObjectStreamBase._trace.NewNotSupportedException();

    internal abstract bool EndOfPipeline { get; }

    internal abstract bool IsOpen { get; }

    internal abstract int Count { get; }

    internal abstract PipelineReader<object> ObjectReader { get; }

    internal abstract PipelineReader<PSObject> PSObjectReader { get; }

    internal abstract PipelineWriter ObjectWriter { get; }

    internal virtual object Read() => throw ObjectStreamBase._trace.NewNotSupportedException();

    internal virtual Collection<object> Read(int count) => throw ObjectStreamBase._trace.NewNotSupportedException();

    internal virtual Collection<object> ReadToEnd() => throw ObjectStreamBase._trace.NewNotSupportedException();

    internal virtual Collection<object> NonBlockingRead(int maxRequested) => throw ObjectStreamBase._trace.NewNotSupportedException();

    internal virtual object Peek() => throw ObjectStreamBase._trace.NewNotSupportedException();

    internal virtual int Write(object value)
    {
      using (ObjectStreamBase._trace.TraceMethod())
        return this.Write(value, false);
    }

    internal virtual int Write(object obj, bool enumerateCollection) => throw ObjectStreamBase._trace.NewNotSupportedException();

    internal virtual void Close() => throw ObjectStreamBase._trace.NewNotSupportedException();

    internal virtual void Flush() => throw ObjectStreamBase._trace.NewNotSupportedException();

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected abstract void Dispose(bool disposing);
  }
}
