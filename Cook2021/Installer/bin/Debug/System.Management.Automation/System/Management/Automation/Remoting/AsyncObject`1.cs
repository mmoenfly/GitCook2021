// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.AsyncObject`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Threading;

namespace System.Management.Automation.Remoting
{
  internal class AsyncObject<T> where T : class
  {
    private T _value;
    private ManualResetEvent _valueWasSet;

    internal T Value
    {
      get
      {
        if (!this._valueWasSet.WaitOne())
          this._value = default (T);
        return this._value;
      }
      set
      {
        this._value = value;
        this._valueWasSet.Set();
      }
    }

    internal AsyncObject() => this._valueWasSet = new ManualResetEvent(false);
  }
}
