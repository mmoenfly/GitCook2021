// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.FaqHelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Globalization;
using System.Text;
using System.Xml;

namespace System.Management.Automation
{
  internal class FaqHelpInfo : HelpInfo
  {
    private PSObject _fullHelpObject;
    [TraceSource("FaqHelpInfo", "FaqHelpInfo")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (FaqHelpInfo), nameof (FaqHelpInfo));

    protected FaqHelpInfo(XmlNode xmlNode)
    {
      using (FaqHelpInfo.tracer.TraceConstructor((object) this))
      {
        MamlNode mamlNode = new MamlNode(xmlNode);
        this._fullHelpObject = mamlNode.PSObject;
        this.Errors = mamlNode.Errors;
        this._fullHelpObject.TypeNames.Clear();
        this._fullHelpObject.TypeNames.Add(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "FaqHelpInfo#{0}", (object) this.Name));
        this._fullHelpObject.TypeNames.Add(nameof (FaqHelpInfo));
        this._fullHelpObject.TypeNames.Add("HelpInfo");
      }
    }

    internal override string Name
    {
      get
      {
        using (FaqHelpInfo.tracer.TraceProperty())
        {
          using (FaqHelpInfo.tracer.TraceProperty())
          {
            if (this._fullHelpObject == null || this._fullHelpObject.Properties["Title"] == null || this._fullHelpObject.Properties["Title"].Value == null)
              return "";
            string str = this._fullHelpObject.Properties["Title"].Value.ToString();
            return str == null ? "" : str.Trim();
          }
        }
      }
    }

    internal override string Synopsis
    {
      get
      {
        using (FaqHelpInfo.tracer.TraceProperty())
        {
          if (this._fullHelpObject == null || this._fullHelpObject.Properties["question"] == null || this._fullHelpObject.Properties["question"].Value == null)
            return "";
          string str = this._fullHelpObject.Properties["question"].Value.ToString();
          return str == null ? "" : str.Trim();
        }
      }
    }

    internal override HelpCategory HelpCategory
    {
      get
      {
        using (FaqHelpInfo.tracer.TraceProperty())
          return HelpCategory.FAQ;
      }
    }

    internal override PSObject FullHelp
    {
      get
      {
        using (FaqHelpInfo.tracer.TraceProperty())
          return this._fullHelpObject;
      }
    }

    internal override bool MatchPatternInContent(WildcardPattern pattern)
    {
      string input1 = this.Synopsis;
      string input2 = this.Answers;
      if (input1 == null)
        input1 = string.Empty;
      if (this.Answers == null)
        input2 = string.Empty;
      return pattern.IsMatch(input1) || pattern.IsMatch(input2);
    }

    private string Answers
    {
      get
      {
        if (this.FullHelp == null || this.FullHelp.Properties["answer"] == null || (this.FullHelp.Properties["answer"].Value == null || !(this.FullHelp.Properties["answer"].Value is IList list)) || list.Count == 0)
          return "";
        StringBuilder stringBuilder = new StringBuilder();
        foreach (object obj in (IEnumerable) list)
        {
          PSObject psObject = PSObject.AsPSObject(obj);
          if (psObject != null && psObject.Properties["Text"] != null && psObject.Properties["Text"].Value != null)
          {
            string str = psObject.Properties["Text"].Value.ToString();
            stringBuilder.Append(str);
            stringBuilder.Append(Environment.NewLine);
          }
        }
        return stringBuilder.ToString().Trim();
      }
    }

    internal static FaqHelpInfo Load(XmlNode xmlNode)
    {
      using (FaqHelpInfo.tracer.TraceMethod())
      {
        FaqHelpInfo faqHelpInfo = new FaqHelpInfo(xmlNode);
        if (string.IsNullOrEmpty(faqHelpInfo.Name))
          return (FaqHelpInfo) null;
        faqHelpInfo.AddCommonHelpProperties();
        return faqHelpInfo;
      }
    }
  }
}
