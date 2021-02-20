// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSSnapIn
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  public abstract class PSSnapIn : PSSnapInInstaller
  {
    private Dictionary<string, object> _regValues;
    [TraceSource("PSSnapIn", "PSSnapIn")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PSSnapIn), nameof (PSSnapIn));

    public virtual string[] Formats => (string[]) null;

    public virtual string[] Types => (string[]) null;

    internal override Dictionary<string, object> RegValues
    {
      get
      {
        if (this._regValues == null)
        {
          this._regValues = base.RegValues;
          if (this.Types != null && this.Types.Length > 0)
            this._regValues["Types"] = (object) this.Types;
          if (this.Formats != null && this.Formats.Length > 0)
            this._regValues["Formats"] = (object) this.Formats;
        }
        return this._regValues;
      }
    }
  }
}
