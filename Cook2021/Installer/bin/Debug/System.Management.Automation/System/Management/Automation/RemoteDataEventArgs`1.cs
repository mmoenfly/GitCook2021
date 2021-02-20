// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemoteDataEventArgs`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal sealed class RemoteDataEventArgs<T> : EventArgs
  {
    [TraceSource("DataEventArgs", "RemoteDataEventArgs<T>")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("DataEventArgs", "RemoteDataEventArgs<T>");
    private T data;

    internal T Data
    {
      get
      {
        using (RemoteDataEventArgs<T>.tracer.TraceProperty())
          return this.data;
      }
    }

    internal RemoteDataEventArgs(object data)
    {
      using (RemoteDataEventArgs<T>.tracer.TraceConstructor((object) this))
        this.data = (T) data;
    }
  }
}
