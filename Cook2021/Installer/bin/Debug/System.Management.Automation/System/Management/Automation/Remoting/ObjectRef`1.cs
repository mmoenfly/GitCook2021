// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ObjectRef`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting
{
  internal class ObjectRef<T> where T : class
  {
    private T _newValue;
    private T _oldValue;

    internal T OldValue => this._oldValue;

    internal T Value => (object) this._newValue == null ? this._oldValue : this._newValue;

    internal bool IsOverridden => (object) this._newValue != null;

    internal ObjectRef(T oldValue) => this._oldValue = oldValue;

    internal void Override(T newValue) => this._newValue = newValue;

    internal void Revert() => this._newValue = default (T);
  }
}
