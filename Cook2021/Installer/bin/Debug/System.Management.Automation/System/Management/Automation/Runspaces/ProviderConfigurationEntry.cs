// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.ProviderConfigurationEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class ProviderConfigurationEntry : RunspaceConfigurationEntry
  {
    private Type _type;
    private string _helpFileName;
    [TraceSource("ProviderConfigurationEntry", "ProviderConfigurationEntry")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ProviderConfigurationEntry), nameof (ProviderConfigurationEntry));

    public ProviderConfigurationEntry(string name, Type implementingType, string helpFileName)
      : base(name)
    {
      using (ProviderConfigurationEntry.tracer.TraceConstructor((object) this))
      {
        this._type = implementingType != null ? implementingType : throw ProviderConfigurationEntry.tracer.NewArgumentNullException(nameof (implementingType));
        if (!string.IsNullOrEmpty(helpFileName))
          this._helpFileName = helpFileName.Trim();
        else
          this._helpFileName = helpFileName;
      }
    }

    internal ProviderConfigurationEntry(
      string name,
      Type implementingType,
      string helpFileName,
      PSSnapInInfo psSnapinInfo)
      : base(name, psSnapinInfo)
    {
      using (ProviderConfigurationEntry.tracer.TraceConstructor((object) this))
      {
        this._type = implementingType != null ? implementingType : throw ProviderConfigurationEntry.tracer.NewArgumentNullException(nameof (implementingType));
        if (!string.IsNullOrEmpty(helpFileName))
          this._helpFileName = helpFileName.Trim();
        else
          this._helpFileName = helpFileName;
      }
    }

    public Type ImplementingType => this._type;

    public string HelpFileName => this._helpFileName;
  }
}
