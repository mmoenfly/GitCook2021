// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.SessionStateFormatEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class SessionStateFormatEntry : InitialSessionStateEntry
  {
    private string _fileName;
    private FormatTable _formattable;

    public SessionStateFormatEntry(string fileName)
      : base("*")
      => this._fileName = fileName;

    public SessionStateFormatEntry(FormatTable formattable)
      : base("*")
      => this._formattable = formattable;

    public override InitialSessionStateEntry Clone()
    {
      SessionStateFormatEntry stateFormatEntry = new SessionStateFormatEntry(this._fileName);
      stateFormatEntry._formattable = this.Formattable;
      stateFormatEntry.SetPSSnapIn(this.PSSnapIn);
      stateFormatEntry.SetModule(this.Module);
      return (InitialSessionStateEntry) stateFormatEntry;
    }

    public string FileName => this._fileName;

    public FormatTable Formattable => this._formattable;
  }
}
