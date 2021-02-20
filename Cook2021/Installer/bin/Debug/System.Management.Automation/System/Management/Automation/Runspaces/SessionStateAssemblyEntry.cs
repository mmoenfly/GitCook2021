// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.SessionStateAssemblyEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class SessionStateAssemblyEntry : InitialSessionStateEntry
  {
    private string _fileName;

    public SessionStateAssemblyEntry(string name, string fileName)
      : base(name)
      => this._fileName = fileName;

    public SessionStateAssemblyEntry(string name)
      : base(name)
    {
    }

    public override InitialSessionStateEntry Clone()
    {
      SessionStateAssemblyEntry stateAssemblyEntry = new SessionStateAssemblyEntry(this.Name, this._fileName);
      stateAssemblyEntry.SetPSSnapIn(this.PSSnapIn);
      stateAssemblyEntry.SetModule(this.Module);
      return (InitialSessionStateEntry) stateAssemblyEntry;
    }

    public string FileName => this._fileName;
  }
}
