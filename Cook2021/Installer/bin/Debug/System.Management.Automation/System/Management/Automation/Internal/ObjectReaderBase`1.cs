// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.ObjectReaderBase`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Management.Automation.Internal
{
  internal abstract class ObjectReaderBase<T> : PipelineReader<T>, IDisposable
  {
    protected ObjectStreamBase _stream;
    private object _monitorObject = new object();
    [TraceSource("ObjectWriter", "Writer for ObjectStream")]
    protected static PSTraceSource _trace = PSTraceSource.GetTracer("ObjectWriter", "Writer for ObjectStream");

    public ObjectReaderBase([In, Out] ObjectStreamBase stream)
    {
      using (ObjectReaderBase<T>._trace.TraceConstructor((object) this))
        this._stream = stream != null ? stream : throw new ArgumentNullException(nameof (stream), "stream may not be null");
    }

    public override event EventHandler DataReady
    {
      add
      {
        lock (this._monitorObject)
        {
          bool flag = null == this.InternalDataReady;
          this.InternalDataReady += value;
          if (!flag)
            return;
          this._stream.DataReady += new EventHandler(this.OnDataReady);
        }
      }
      remove
      {
        lock (this._monitorObject)
        {
          this.InternalDataReady -= value;
          if (this.InternalDataReady != null)
            return;
          this._stream.DataReady -= new EventHandler(this.OnDataReady);
        }
      }
    }

    public event EventHandler InternalDataReady;

    public override WaitHandle WaitHandle
    {
      get
      {
        using (ObjectReaderBase<T>._trace.TraceProperty())
          return this._stream.ReadHandle;
      }
    }

    public override bool EndOfPipeline
    {
      get
      {
        using (ObjectReaderBase<T>._trace.TraceProperty())
          return this._stream.EndOfPipeline;
      }
    }

    public override bool IsOpen
    {
      get
      {
        using (ObjectReaderBase<T>._trace.TraceProperty())
          return this._stream.IsOpen;
      }
    }

    public override int Count
    {
      get
      {
        using (ObjectReaderBase<T>._trace.TraceProperty())
          return this._stream.Count;
      }
    }

    public override int MaxCapacity
    {
      get
      {
        using (ObjectReaderBase<T>._trace.TraceProperty())
          return this._stream.MaxCapacity;
      }
    }

    public override void Close()
    {
      using (ObjectReaderBase<T>._trace.TraceMethod())
        this._stream.Close();
    }

    private void OnDataReady(object sender, EventArgs args)
    {
      if (this.InternalDataReady == null)
        return;
      this.InternalDataReady((object) this, args);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected abstract void Dispose(bool disposing);
  }
}
