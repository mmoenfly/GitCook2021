// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ExtendedTypeDefinition
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Globalization;

namespace System.Management.Automation
{
  public sealed class ExtendedTypeDefinition
  {
    private List<System.Management.Automation.FormatViewDefinition> _viewdefinitions;
    private string _typename;

    public string TypeName => this._typename;

    public List<System.Management.Automation.FormatViewDefinition> FormatViewDefinition => this._viewdefinitions;

    public override string ToString() => string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}", (object) this._typename);

    internal ExtendedTypeDefinition(string typename, List<System.Management.Automation.FormatViewDefinition> viewdefinitions)
    {
      this._typename = typename;
      this._viewdefinitions = viewdefinitions;
    }
  }
}
