// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.SessionStateScriptEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class SessionStateScriptEntry : SessionStateCommandEntry
  {
    private string _path;

    public SessionStateScriptEntry(string path)
      : base(path, SessionStateEntryVisibility.Public)
    {
      this._path = path;
      this._commandType = CommandTypes.ExternalScript;
    }

    internal SessionStateScriptEntry(string path, SessionStateEntryVisibility visibility)
      : base(path, visibility)
    {
      this._path = path;
      this._commandType = CommandTypes.ExternalScript;
    }

    public override InitialSessionStateEntry Clone()
    {
      SessionStateScriptEntry stateScriptEntry = new SessionStateScriptEntry(this._path, this.Visibility);
      stateScriptEntry.SetModule(this.Module);
      return (InitialSessionStateEntry) stateScriptEntry;
    }

    public string Path => this._path;
  }
}
