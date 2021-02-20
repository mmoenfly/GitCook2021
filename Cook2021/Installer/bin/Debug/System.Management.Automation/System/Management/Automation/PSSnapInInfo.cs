// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSSnapInInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;

namespace System.Management.Automation
{
  public class PSSnapInInfo
  {
    private string _name;
    private bool _isDefault;
    private string _applicationBase;
    private string _assemblyName;
    private string _moduleName;
    private string _customPSSnapInType;
    private Version _psVersion;
    private Version _version;
    private Collection<string> _types;
    private Collection<string> _formats;
    private string _descriptionIndirect;
    private string _descriptionFallback = string.Empty;
    private string _description;
    private string _vendorIndirect;
    private string _vendorFallback = string.Empty;
    private string _vendor;
    private bool _logPipelineExecutionDetails;
    [TraceSource("PSSnapInInfo", "PSSnapInInfo")]
    private static PSTraceSource _tracer = PSTraceSource.GetTracer(nameof (PSSnapInInfo), nameof (PSSnapInInfo));

    internal PSSnapInInfo(
      string name,
      bool isDefault,
      string applicationBase,
      string assemblyName,
      string moduleName,
      Version psVersion,
      Version version,
      Collection<string> types,
      Collection<string> formats,
      string descriptionFallback,
      string vendorFallback,
      string customPSSnapInType)
    {
      using (PSSnapInInfo._tracer.TraceConstructor((object) this))
      {
        if (string.IsNullOrEmpty(name))
          throw PSSnapInInfo._tracer.NewArgumentNullException(nameof (name));
        if (string.IsNullOrEmpty(applicationBase))
          throw PSSnapInInfo._tracer.NewArgumentNullException(nameof (applicationBase));
        if (string.IsNullOrEmpty(assemblyName))
          throw PSSnapInInfo._tracer.NewArgumentNullException(nameof (assemblyName));
        if (string.IsNullOrEmpty(moduleName))
          throw PSSnapInInfo._tracer.NewArgumentNullException(nameof (moduleName));
        if (psVersion == (Version) null)
          throw PSSnapInInfo._tracer.NewArgumentNullException(nameof (psVersion));
        if (version == (Version) null)
          version = new Version("0.0");
        if (types == null)
          types = new Collection<string>();
        if (formats == null)
          formats = new Collection<string>();
        if (descriptionFallback == null)
          descriptionFallback = string.Empty;
        if (vendorFallback == null)
          vendorFallback = string.Empty;
        this._name = name;
        this._isDefault = isDefault;
        this._applicationBase = applicationBase;
        this._assemblyName = assemblyName;
        this._moduleName = moduleName;
        this._psVersion = psVersion;
        this._version = version;
        this._types = types;
        this._formats = formats;
        this._customPSSnapInType = customPSSnapInType;
        this._descriptionFallback = descriptionFallback;
        this._vendorFallback = vendorFallback;
      }
    }

    internal PSSnapInInfo(
      string name,
      bool isDefault,
      string applicationBase,
      string assemblyName,
      string moduleName,
      Version psVersion,
      Version version,
      Collection<string> types,
      Collection<string> formats,
      string description,
      string descriptionFallback,
      string vendor,
      string vendorFallback,
      string customPSSnapInType)
      : this(name, isDefault, applicationBase, assemblyName, moduleName, psVersion, version, types, formats, descriptionFallback, vendorFallback, customPSSnapInType)
    {
      using (PSSnapInInfo._tracer.TraceConstructor((object) this))
      {
        this._description = description;
        this._vendor = vendor;
      }
    }

    internal PSSnapInInfo(
      string name,
      bool isDefault,
      string applicationBase,
      string assemblyName,
      string moduleName,
      Version psVersion,
      Version version,
      Collection<string> types,
      Collection<string> formats,
      string description,
      string descriptionFallback,
      string descriptionIndirect,
      string vendor,
      string vendorFallback,
      string vendorIndirect,
      string customPSSnapInType)
      : this(name, isDefault, applicationBase, assemblyName, moduleName, psVersion, version, types, formats, description, descriptionFallback, vendor, vendorFallback, customPSSnapInType)
    {
      using (PSSnapInInfo._tracer.TraceConstructor((object) this))
      {
        if (!isDefault)
          return;
        this._descriptionIndirect = descriptionIndirect;
        this._vendorIndirect = vendorIndirect;
      }
    }

    public string Name => this._name;

    public bool IsDefault => this._isDefault;

    public string ApplicationBase => this._applicationBase;

    public string AssemblyName => this._assemblyName;

    public string ModuleName => this._moduleName;

    internal string AbsoluteModulePath => string.IsNullOrEmpty(this._moduleName) || Path.IsPathRooted(this._moduleName) ? this._moduleName : Path.Combine(this._applicationBase, this._moduleName);

    internal string CustomPSSnapInType => this._customPSSnapInType;

    public Version PSVersion => this._psVersion;

    public Version Version => this._version;

    public Collection<string> Types => this._types;

    public Collection<string> Formats => this._formats;

    public string Description
    {
      get
      {
        if (this._description == null)
          this.LoadIndirectResources();
        return this._description;
      }
    }

    public string Vendor
    {
      get
      {
        if (this._vendor == null)
          this.LoadIndirectResources();
        return this._vendor;
      }
    }

    public bool LogPipelineExecutionDetails
    {
      get
      {
        using (PSSnapInInfo._tracer.TraceProperty(nameof (LogPipelineExecutionDetails), new object[0]))
          return this._logPipelineExecutionDetails;
      }
      set
      {
        using (PSSnapInInfo._tracer.TraceProperty(nameof (LogPipelineExecutionDetails), new object[0]))
          this._logPipelineExecutionDetails = value;
      }
    }

    public override string ToString() => this._name;

    internal RegistryKey MshSnapinKey
    {
      get
      {
        RegistryKey registryKey = (RegistryKey) null;
        try
        {
          registryKey = PSSnapInReader.GetMshSnapinKey(this._name, this._psVersion.Major.ToString((IFormatProvider) CultureInfo.InvariantCulture));
        }
        catch (ArgumentException ex)
        {
        }
        catch (SecurityException ex)
        {
        }
        catch (IOException ex)
        {
        }
        return registryKey;
      }
    }

    internal void LoadIndirectResources()
    {
      using (RegistryStringResourceIndirect resourceIndirectReader = RegistryStringResourceIndirect.GetResourceIndirectReader())
        this.LoadIndirectResources(resourceIndirectReader);
    }

    internal void LoadIndirectResources(RegistryStringResourceIndirect resourceReader)
    {
      if (this.IsDefault)
      {
        this._description = resourceReader.GetResourceStringIndirect(this._assemblyName, this._moduleName, this._descriptionIndirect);
        this._vendor = resourceReader.GetResourceStringIndirect(this._assemblyName, this._moduleName, this._vendorIndirect);
      }
      else
      {
        RegistryKey mshSnapinKey = this.MshSnapinKey;
        if (mshSnapinKey != null)
        {
          this._description = resourceReader.GetResourceStringIndirect(mshSnapinKey, "DescriptionIndirect", this._assemblyName, this._moduleName);
          this._vendor = resourceReader.GetResourceStringIndirect(mshSnapinKey, "VendorIndirect", this._assemblyName, this._moduleName);
        }
      }
      if (string.IsNullOrEmpty(this._description))
        this._description = this._descriptionFallback;
      if (!string.IsNullOrEmpty(this._vendor))
        return;
      this._vendor = this._vendorFallback;
    }

    internal PSSnapInInfo Clone()
    {
      using (PSSnapInInfo._tracer.TraceMethod())
        return new PSSnapInInfo(this._name, this._isDefault, this._applicationBase, this._assemblyName, this._moduleName, this._psVersion, this._version, new Collection<string>((IList<string>) this.Types), new Collection<string>((IList<string>) this.Formats), this._description, this._descriptionFallback, this._descriptionIndirect, this._vendor, this._vendorFallback, this._vendorIndirect, this._customPSSnapInType);
    }

    internal static bool IsPSSnapinIdValid(string psSnapinId) => !string.IsNullOrEmpty(psSnapinId) && Regex.IsMatch(psSnapinId, "^[A-Za-z0-9-_.]*$");

    internal static void VerifyPSSnapInFormatThrowIfError(string psSnapinId)
    {
      if (!PSSnapInInfo.IsPSSnapinIdValid(psSnapinId))
        throw PSSnapInInfo._tracer.NewArgumentException("mshSnapInId", "MshSnapInCmdletResources", "InvalidPSSnapInName", (object) psSnapinId);
    }
  }
}
