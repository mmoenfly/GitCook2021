// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.ConstrainedSessionStateEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public abstract class ConstrainedSessionStateEntry : InitialSessionStateEntry
  {
    private SessionStateEntryVisibility _visibility;

    protected ConstrainedSessionStateEntry(string name, SessionStateEntryVisibility visibility)
      : base(name)
      => this._visibility = visibility;

    public SessionStateEntryVisibility Visibility
    {
      get => this._visibility;
      set => this._visibility = value;
    }
  }
}
