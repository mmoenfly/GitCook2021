// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.SessionStateCommandEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public abstract class SessionStateCommandEntry : ConstrainedSessionStateEntry
  {
    internal CommandTypes _commandType;

    protected SessionStateCommandEntry(string name)
      : base(name, SessionStateEntryVisibility.Public)
    {
    }

    protected internal SessionStateCommandEntry(string name, SessionStateEntryVisibility visibility)
      : base(name, visibility)
    {
    }

    public CommandTypes CommandType => this._commandType;
  }
}
