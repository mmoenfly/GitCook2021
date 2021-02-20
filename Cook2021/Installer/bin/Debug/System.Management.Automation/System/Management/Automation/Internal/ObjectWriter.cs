// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.ObjectWriter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Management.Automation.Internal
{
  internal class ObjectWriter : PipelineWriter
  {
    private ObjectStreamBase _stream;
    [TraceSource("ObjectWriter", "Writer for ObjectStream")]
    protected static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (ObjectWriter), "Writer for ObjectStream");

    public ObjectWriter([In, Out] ObjectStreamBase stream)
    {
      using (ObjectWriter._trace.TraceConstructor((object) this))
        this._stream = stream != null ? stream : throw new ArgumentNullException(nameof (stream));
    }

    public override WaitHandle WaitHandle
    {
      get
      {
        using (ObjectWriter._trace.TraceProperty())
          return this._stream.WriteHandle;
      }
    }

    public override bool IsOpen
    {
      get
      {
        using (ObjectWriter._trace.TraceProperty())
          return this._stream.IsOpen;
      }
    }

    public override int Count
    {
      get
      {
        using (ObjectWriter._trace.TraceProperty())
          return this._stream.Count;
      }
    }

    public override int MaxCapacity
    {
      get
      {
        using (ObjectWriter._trace.TraceProperty())
          return this._stream.MaxCapacity;
      }
    }

    public override void Close()
    {
      using (ObjectWriter._trace.TraceMethod())
        this._stream.Close();
    }

    public override void Flush()
    {
      using (ObjectWriter._trace.TraceMethod())
        this._stream.Flush();
    }

    public override int Write(object obj)
    {
      using (ObjectWriter._trace.TraceMethod())
        return this._stream.Write(obj);
    }

    public override int Write(object obj, bool enumerateCollection)
    {
      using (ObjectWriter._trace.TraceMethod())
        return this._stream.Write(obj, enumerateCollection);
    }
  }
}
