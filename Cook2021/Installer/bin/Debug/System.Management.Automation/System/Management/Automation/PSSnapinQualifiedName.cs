// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSSnapinQualifiedName
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;

namespace System.Management.Automation
{
  internal class PSSnapinQualifiedName
  {
    private string _fullName;
    private string _psSnapinName;
    private string _shortName;

    private PSSnapinQualifiedName(string[] splitName)
    {
      if (splitName.Length == 1)
      {
        this._shortName = splitName[0];
      }
      else
      {
        if (splitName.Length != 2)
          throw CommandDiscovery.tracer.NewArgumentException("name");
        if (!string.IsNullOrEmpty(splitName[0]))
          this._psSnapinName = splitName[0];
        this._shortName = splitName[1];
      }
      if (!string.IsNullOrEmpty(this._psSnapinName))
        this._fullName = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}\\{1}", (object) this._psSnapinName, (object) this._shortName);
      else
        this._fullName = this._shortName;
    }

    internal static PSSnapinQualifiedName GetInstance(string name)
    {
      if (name == null)
        return (PSSnapinQualifiedName) null;
      string[] splitName = name.Split('\\');
      if (splitName.Length < 0 || splitName.Length > 2)
        return (PSSnapinQualifiedName) null;
      PSSnapinQualifiedName snapinQualifiedName = new PSSnapinQualifiedName(splitName);
      return string.IsNullOrEmpty(snapinQualifiedName.ShortName) ? (PSSnapinQualifiedName) null : snapinQualifiedName;
    }

    internal string FullName => this._fullName;

    internal string PSSnapInName => this._psSnapinName;

    internal string ShortName => this._shortName;

    public override string ToString() => this._fullName;
  }
}
