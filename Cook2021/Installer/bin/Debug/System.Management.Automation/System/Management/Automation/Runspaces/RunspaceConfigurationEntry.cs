// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceConfigurationEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public abstract class RunspaceConfigurationEntry
  {
    private string _name;
    private PSSnapInInfo _PSSnapin;
    internal bool _builtIn;
    internal UpdateAction _action = UpdateAction.None;
    [TraceSource("RunspaceConfigurationEntry", "RunspaceConfigurationEntry")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (RunspaceConfigurationEntry), nameof (RunspaceConfigurationEntry));

    protected RunspaceConfigurationEntry(string name)
    {
      using (RunspaceConfigurationEntry.tracer.TraceConstructor((object) this))
        this._name = !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(name.Trim()) ? name.Trim() : throw RunspaceConfigurationEntry.tracer.NewArgumentNullException(nameof (name));
    }

    internal RunspaceConfigurationEntry(string name, PSSnapInInfo psSnapin)
    {
      using (RunspaceConfigurationEntry.tracer.TraceConstructor((object) this))
      {
        this._name = !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(name.Trim()) ? name.Trim() : throw RunspaceConfigurationEntry.tracer.NewArgumentNullException(nameof (name));
        this._PSSnapin = psSnapin != null ? psSnapin : throw RunspaceConfigurationEntry.tracer.NewArgumentException(nameof (psSnapin));
      }
    }

    public string Name => this._name;

    public PSSnapInInfo PSSnapIn => this._PSSnapin;

    public bool BuiltIn => this._builtIn;

    internal UpdateAction Action => this._action;
  }
}
