// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CustomPSSnapIn
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public abstract class CustomPSSnapIn : PSSnapInInstaller
  {
    private Collection<CmdletConfigurationEntry> _cmdlets;
    private Collection<ProviderConfigurationEntry> _providers;
    private Collection<TypeConfigurationEntry> _types;
    private Collection<FormatConfigurationEntry> _formats;
    private Dictionary<string, object> _regValues;
    [TraceSource("CustomPSSnapIn", "CustomPSSnapIn")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CustomPSSnapIn), nameof (CustomPSSnapIn));

    internal string CustomPSSnapInType => this.GetType().FullName;

    public virtual Collection<CmdletConfigurationEntry> Cmdlets
    {
      get
      {
        using (CustomPSSnapIn.tracer.TraceProperty())
        {
          if (this._cmdlets == null)
            this._cmdlets = new Collection<CmdletConfigurationEntry>();
          return this._cmdlets;
        }
      }
    }

    public virtual Collection<ProviderConfigurationEntry> Providers
    {
      get
      {
        using (CustomPSSnapIn.tracer.TraceProperty())
        {
          if (this._providers == null)
            this._providers = new Collection<ProviderConfigurationEntry>();
          return this._providers;
        }
      }
    }

    public virtual Collection<TypeConfigurationEntry> Types
    {
      get
      {
        using (CustomPSSnapIn.tracer.TraceProperty())
        {
          if (this._types == null)
            this._types = new Collection<TypeConfigurationEntry>();
          return this._types;
        }
      }
    }

    public virtual Collection<FormatConfigurationEntry> Formats
    {
      get
      {
        using (CustomPSSnapIn.tracer.TraceProperty())
        {
          if (this._formats == null)
            this._formats = new Collection<FormatConfigurationEntry>();
          return this._formats;
        }
      }
    }

    internal override Dictionary<string, object> RegValues
    {
      get
      {
        if (this._regValues == null)
        {
          this._regValues = base.RegValues;
          if (!string.IsNullOrEmpty(this.CustomPSSnapInType))
            this._regValues["CustomPSSnapInType"] = (object) this.CustomPSSnapInType;
        }
        return this._regValues;
      }
    }
  }
}
