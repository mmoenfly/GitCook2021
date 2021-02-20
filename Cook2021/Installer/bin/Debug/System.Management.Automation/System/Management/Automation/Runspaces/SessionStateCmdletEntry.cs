﻿// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.SessionStateCmdletEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class SessionStateCmdletEntry : SessionStateCommandEntry
  {
    private Type _implementingType;
    private string _helpFileName;

    public SessionStateCmdletEntry(string name, Type implementingType, string helpFileName)
      : base(name, SessionStateEntryVisibility.Public)
    {
      this._implementingType = implementingType;
      this._helpFileName = helpFileName;
      this._commandType = CommandTypes.Cmdlet;
    }

    internal SessionStateCmdletEntry(
      string name,
      Type implementingType,
      string helpFileName,
      SessionStateEntryVisibility visibility)
      : base(name, visibility)
    {
      this._implementingType = implementingType;
      this._helpFileName = helpFileName;
      this._commandType = CommandTypes.Cmdlet;
    }

    public override InitialSessionStateEntry Clone()
    {
      SessionStateCmdletEntry stateCmdletEntry = new SessionStateCmdletEntry(this.Name, this._implementingType, this._helpFileName, this.Visibility);
      stateCmdletEntry.SetPSSnapIn(this.PSSnapIn);
      stateCmdletEntry.SetModule(this.Module);
      return (InitialSessionStateEntry) stateCmdletEntry;
    }

    public Type ImplementingType => this._implementingType;

    public string HelpFileName => this._helpFileName;
  }
}
