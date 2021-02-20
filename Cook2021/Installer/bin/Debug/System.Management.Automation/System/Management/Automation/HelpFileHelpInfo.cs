// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HelpFileHelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.IO;

namespace System.Management.Automation
{
  internal class HelpFileHelpInfo : HelpInfo
  {
    private string _name = "";
    private string _filename = "";
    private string _synopsis = "";
    private PSObject _fullHelpObject;
    [TraceSource("HelpFileHelpInfo", "HelpFileHelpInfo")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (HelpFileHelpInfo), nameof (HelpFileHelpInfo));

    private HelpFileHelpInfo(string name, string text, string filename)
    {
      using (HelpFileHelpInfo.tracer.TraceConstructor((object) this))
      {
        this._fullHelpObject = PSObject.AsPSObject((object) text);
        this._name = name;
        this._synopsis = HelpFileHelpInfo.GetLine(text, 5);
        this._synopsis = this._synopsis == null ? "" : this._synopsis.Trim();
        this._filename = filename;
      }
    }

    internal override string Name
    {
      get
      {
        using (HelpFileHelpInfo.tracer.TraceProperty())
          return this._name;
      }
    }

    internal override string Synopsis
    {
      get
      {
        using (HelpFileHelpInfo.tracer.TraceProperty())
          return this._synopsis;
      }
    }

    internal override HelpCategory HelpCategory
    {
      get
      {
        using (HelpFileHelpInfo.tracer.TraceProperty())
          return HelpCategory.HelpFile;
      }
    }

    internal override PSObject FullHelp
    {
      get
      {
        using (HelpFileHelpInfo.tracer.TraceProperty())
          return this._fullHelpObject;
      }
    }

    internal static HelpFileHelpInfo GetHelpInfo(
      string name,
      string text,
      string filename)
    {
      using (HelpFileHelpInfo.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(name))
          return (HelpFileHelpInfo) null;
        HelpFileHelpInfo helpFileHelpInfo = new HelpFileHelpInfo(name, text, filename);
        if (string.IsNullOrEmpty(helpFileHelpInfo.Name))
          return (HelpFileHelpInfo) null;
        helpFileHelpInfo.AddCommonHelpProperties();
        return helpFileHelpInfo;
      }
    }

    private static string GetLine(string text, int line)
    {
      using (HelpFileHelpInfo.tracer.TraceMethod())
      {
        StringReader stringReader = new StringReader(text);
        string str = (string) null;
        for (int index = 0; index < line; ++index)
        {
          str = stringReader.ReadLine();
          if (str == null)
            return (string) null;
        }
        return str;
      }
    }

    internal override bool MatchPatternInContent(WildcardPattern pattern)
    {
      string result = string.Empty;
      LanguagePrimitives.TryConvertTo<string>((object) this.FullHelp, out result);
      return pattern.IsMatch(result);
    }
  }
}
