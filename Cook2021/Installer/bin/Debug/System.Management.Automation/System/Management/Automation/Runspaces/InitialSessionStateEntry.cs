// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.InitialSessionStateEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public abstract class InitialSessionStateEntry
  {
    private string _name;
    private PSSnapInInfo _psSnapIn;
    private PSModuleInfo _module;

    protected InitialSessionStateEntry(string name) => this._name = name;

    public string Name
    {
      get => this._name;
      internal set => this._name = value;
    }

    public PSSnapInInfo PSSnapIn => this._psSnapIn;

    internal void SetPSSnapIn(PSSnapInInfo psSnapIn) => this._psSnapIn = psSnapIn;

    public PSModuleInfo Module => this._module;

    internal void SetModule(PSModuleInfo module) => this._module = module;

    public abstract InitialSessionStateEntry Clone();
  }
}
