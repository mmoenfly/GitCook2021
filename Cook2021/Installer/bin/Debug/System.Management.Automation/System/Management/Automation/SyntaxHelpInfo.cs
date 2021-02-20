// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SyntaxHelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class SyntaxHelpInfo : BaseCommandHelpInfo
  {
    private string _name = "";
    private string _synopsis = "";
    private PSObject _fullHelpObject;

    private SyntaxHelpInfo(string name, string text, HelpCategory category)
      : base(category)
    {
      this._fullHelpObject = PSObject.AsPSObject((object) text);
      this._name = name;
      this._synopsis = text;
    }

    internal override string Name => this._name;

    internal override string Synopsis => this._synopsis;

    internal override PSObject FullHelp => this._fullHelpObject;

    internal static SyntaxHelpInfo GetHelpInfo(
      string name,
      string text,
      HelpCategory category)
    {
      if (string.IsNullOrEmpty(name))
        return (SyntaxHelpInfo) null;
      SyntaxHelpInfo syntaxHelpInfo = new SyntaxHelpInfo(name, text, category);
      if (string.IsNullOrEmpty(syntaxHelpInfo.Name))
        return (SyntaxHelpInfo) null;
      syntaxHelpInfo.AddCommonHelpProperties();
      return syntaxHelpInfo;
    }
  }
}
