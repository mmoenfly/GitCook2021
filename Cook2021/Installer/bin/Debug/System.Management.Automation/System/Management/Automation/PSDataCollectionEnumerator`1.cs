// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSDataCollectionEnumerator`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace System.Management.Automation
{
  internal sealed class PSDataCollectionEnumerator<W> : IEnumerator<W>, IDisposable, IEnumerator
  {
    [TraceSource("PowerShellHosting", "Powershell hosting interfaces")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PowerShellHosting", "Powershell hosting interfaces");
    private W currentElement;
    private int index;
    private PSDataCollection<W> collToEnumerate;

    internal PSDataCollectionEnumerator(PSDataCollection<W> collection)
    {
      this.collToEnumerate = collection;
      this.index = 0;
      this.currentElement = default (W);
      this.collToEnumerate.IsEnumerated = true;
    }

    W IEnumerator<W>.Current => this.currentElement;

    public object Current => (object) this.currentElement;

    public bool MoveNext() => this.MoveNext(true);

    internal bool MoveNext(bool block)
    {
      lock (this.collToEnumerate.SyncObject)
      {
        while (this.index >= this.collToEnumerate.Count)
        {
          if (this.collToEnumerate.RefCount == 0 || !this.collToEnumerate.IsOpen || !block)
            return false;
          PSDataCollectionEnumerator<W>.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Waiting for data at index {0}", (object) this.index), new object[0]);
          Monitor.Wait(this.collToEnumerate.SyncObject);
        }
        this.currentElement = this.collToEnumerate[this.index];
        if (this.collToEnumerate.ReleaseOnEnumeration)
          this.collToEnumerate[this.index] = default (W);
        ++this.index;
        return true;
      }
    }

    public void Reset()
    {
      this.currentElement = default (W);
      this.index = 0;
    }

    void IDisposable.Dispose()
    {
    }
  }
}
