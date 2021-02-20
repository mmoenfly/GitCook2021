// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.ObjectReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace System.Management.Automation.Internal
{
  internal class ObjectReader : ObjectReaderBase<object>
  {
    public ObjectReader([In, Out] ObjectStream stream)
      : base((ObjectStreamBase) stream)
    {
    }

    public override Collection<object> Read(int count)
    {
      using (ObjectReaderBase<object>._trace.TraceMethod())
        return this._stream.Read(count);
    }

    public override object Read()
    {
      using (ObjectReaderBase<object>._trace.TraceMethod())
        return this._stream.Read();
    }

    public override Collection<object> ReadToEnd()
    {
      using (ObjectReaderBase<object>._trace.TraceMethod())
        return this._stream.ReadToEnd();
    }

    public override Collection<object> NonBlockingRead()
    {
      using (ObjectReaderBase<object>._trace.TraceMethod())
        return this._stream.NonBlockingRead(int.MaxValue);
    }

    public override Collection<object> NonBlockingRead(int maxRequested)
    {
      using (ObjectReaderBase<object>._trace.TraceMethod())
        return this._stream.NonBlockingRead(maxRequested);
    }

    public override object Peek()
    {
      using (ObjectReaderBase<object>._trace.TraceMethod())
        return this._stream.Peek();
    }

    protected override void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      this._stream.Close();
    }
  }
}
