// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.SessionStateAliasEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class SessionStateAliasEntry : SessionStateCommandEntry
  {
    private string _definition;
    private string _description = string.Empty;
    private ScopedItemOptions _options;

    public SessionStateAliasEntry(string name, string definition)
      : base(name, SessionStateEntryVisibility.Public)
    {
      this._definition = definition;
      this._commandType = CommandTypes.Alias;
    }

    public SessionStateAliasEntry(string name, string definition, string description)
      : base(name, SessionStateEntryVisibility.Public)
    {
      this._definition = definition;
      this._commandType = CommandTypes.Alias;
      this._description = description;
    }

    public SessionStateAliasEntry(
      string name,
      string definition,
      string description,
      ScopedItemOptions options)
      : base(name, SessionStateEntryVisibility.Public)
    {
      this._definition = definition;
      this._commandType = CommandTypes.Alias;
      this._description = description;
      this._options = options;
    }

    internal SessionStateAliasEntry(
      string name,
      string definition,
      string description,
      ScopedItemOptions options,
      SessionStateEntryVisibility visibility)
      : base(name, visibility)
    {
      this._definition = definition;
      this._commandType = CommandTypes.Alias;
      this._description = description;
      this._options = options;
    }

    public override InitialSessionStateEntry Clone()
    {
      SessionStateAliasEntry sessionStateAliasEntry = new SessionStateAliasEntry(this.Name, this._definition, this._description, this._options, this.Visibility);
      sessionStateAliasEntry.SetModule(this.Module);
      return (InitialSessionStateEntry) sessionStateAliasEntry;
    }

    public string Definition => this._definition;

    public string Description => this._description;

    public ScopedItemOptions Options => this._options;
  }
}
