// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.ScriptConfigurationEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class ScriptConfigurationEntry : RunspaceConfigurationEntry
  {
    private string _definition;
    [TraceSource("ScriptConfigurationEntry", "ScriptConfigurationEntry")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ScriptConfigurationEntry), nameof (ScriptConfigurationEntry));

    public ScriptConfigurationEntry(string name, string definition)
      : base(name)
    {
      using (ScriptConfigurationEntry.tracer.TraceConstructor((object) this))
        this._definition = !string.IsNullOrEmpty(definition) && !string.IsNullOrEmpty(definition.Trim()) ? definition.Trim() : throw ScriptConfigurationEntry.tracer.NewArgumentNullException(nameof (definition));
    }

    public string Definition => this._definition;
  }
}
