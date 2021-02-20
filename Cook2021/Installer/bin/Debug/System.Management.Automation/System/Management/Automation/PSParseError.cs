// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSParseError
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public sealed class PSParseError
  {
    private PSToken _psToken;
    private string _message;

    internal PSParseError(RuntimeException rte)
    {
      this._message = rte.Message;
      this._psToken = new PSToken(rte.ErrorToken);
    }

    public PSToken Token => this._psToken;

    public string Message => this._message;
  }
}
