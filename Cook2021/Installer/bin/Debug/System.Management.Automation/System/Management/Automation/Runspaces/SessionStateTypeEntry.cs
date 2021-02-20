// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.SessionStateTypeEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class SessionStateTypeEntry : InitialSessionStateEntry
  {
    private string _fileName;
    private TypeTable _typeTable;

    public SessionStateTypeEntry(string fileName)
      : base("*")
      => this._fileName = fileName;

    public SessionStateTypeEntry(TypeTable typeTable)
      : base("*")
      => this._typeTable = typeTable;

    public override InitialSessionStateEntry Clone()
    {
      SessionStateTypeEntry sessionStateTypeEntry = new SessionStateTypeEntry(this._fileName);
      sessionStateTypeEntry._typeTable = this.TypeTable;
      sessionStateTypeEntry.SetPSSnapIn(this.PSSnapIn);
      sessionStateTypeEntry.SetModule(this.Module);
      return (InitialSessionStateEntry) sessionStateTypeEntry;
    }

    public string FileName => this._fileName;

    public TypeTable TypeTable => this._typeTable;
  }
}
