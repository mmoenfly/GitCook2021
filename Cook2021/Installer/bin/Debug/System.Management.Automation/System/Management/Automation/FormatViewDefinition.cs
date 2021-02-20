// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.FormatViewDefinition
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;

namespace System.Management.Automation
{
  public sealed class FormatViewDefinition
  {
    private string _name;
    private PSControl _control;
    private Guid _instanceId;

    public string Name => this._name;

    public PSControl Control => this._control;

    internal Guid InstanceId => this._instanceId;

    public override string ToString() => string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0} , {1}", (object) this._name, (object) this._control.ToString());

    internal FormatViewDefinition(string name, PSControl control, Guid instanceid)
    {
      this._name = name;
      this._control = control;
      this._instanceId = instanceid;
    }
  }
}
