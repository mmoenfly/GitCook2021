// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSSnapInInstaller
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.IO;

namespace System.Management.Automation
{
  public abstract class PSSnapInInstaller : PSInstaller
  {
    private string _psVersion;
    private Dictionary<string, object> _regValues;

    public abstract string Name { get; }

    public abstract string Vendor { get; }

    public virtual string VendorResource => (string) null;

    public abstract string Description { get; }

    public virtual string DescriptionResource => (string) null;

    private string MshSnapinVersion => this.GetType().Assembly.GetName().Version.ToString();

    private string PSVersion
    {
      get
      {
        if (this._psVersion == null)
          this._psVersion = PSVersionInfo.FeatureVersionString;
        return this._psVersion;
      }
    }

    internal override sealed string RegKey
    {
      get
      {
        PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(this.Name);
        return "PowerShellSnapIns\\" + this.Name;
      }
    }

    internal override Dictionary<string, object> RegValues
    {
      get
      {
        if (this._regValues == null)
        {
          this._regValues = new Dictionary<string, object>();
          this._regValues["PowerShellVersion"] = (object) this.PSVersion;
          if (!string.IsNullOrEmpty(this.Vendor))
            this._regValues["Vendor"] = (object) this.Vendor;
          if (!string.IsNullOrEmpty(this.Description))
            this._regValues["Description"] = (object) this.Description;
          if (!string.IsNullOrEmpty(this.VendorResource))
            this._regValues["VendorIndirect"] = (object) this.VendorResource;
          if (!string.IsNullOrEmpty(this.DescriptionResource))
            this._regValues["DescriptionIndirect"] = (object) this.DescriptionResource;
          this._regValues["Version"] = (object) this.MshSnapinVersion;
          this._regValues["ApplicationBase"] = (object) Path.GetDirectoryName(this.GetType().Assembly.Location);
          this._regValues["AssemblyName"] = (object) this.GetType().Assembly.FullName;
          this._regValues["ModuleName"] = (object) this.GetType().Assembly.Location;
        }
        return this._regValues;
      }
    }
  }
}
