// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.SessionStateApplicationEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class SessionStateApplicationEntry : SessionStateCommandEntry
  {
    private string _path;

    public SessionStateApplicationEntry(string path)
      : base(path, SessionStateEntryVisibility.Public)
    {
      this._path = path;
      this._commandType = CommandTypes.Application;
    }

    internal SessionStateApplicationEntry(string path, SessionStateEntryVisibility visibility)
      : base(path, visibility)
    {
      this._path = path;
      this._commandType = CommandTypes.Application;
    }

    public override InitialSessionStateEntry Clone()
    {
      SessionStateApplicationEntry applicationEntry = new SessionStateApplicationEntry(this._path, this.Visibility);
      applicationEntry.SetModule(this.Module);
      return (InitialSessionStateEntry) applicationEntry;
    }

    public string Path => this._path;
  }
}
