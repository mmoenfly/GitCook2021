// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProviderHelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Text;
using System.Xml;

namespace System.Management.Automation
{
  internal class ProviderHelpInfo : HelpInfo
  {
    private PSObject _fullHelpObject;
    [TraceSource("ProviderHelpInfo", "ProviderHelpInfo")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ProviderHelpInfo), nameof (ProviderHelpInfo));

    private ProviderHelpInfo(XmlNode xmlNode)
    {
      using (ProviderHelpInfo.tracer.TraceConstructor((object) this))
      {
        MamlNode mamlNode = new MamlNode(xmlNode);
        this._fullHelpObject = mamlNode.PSObject;
        this.Errors = mamlNode.Errors;
        this._fullHelpObject.TypeNames.Clear();
        this._fullHelpObject.TypeNames.Add(nameof (ProviderHelpInfo));
        this._fullHelpObject.TypeNames.Add("HelpInfo");
      }
    }

    internal override string Name
    {
      get
      {
        using (ProviderHelpInfo.tracer.TraceProperty())
        {
          if (this._fullHelpObject == null || this._fullHelpObject.Properties[nameof (Name)] == null || this._fullHelpObject.Properties[nameof (Name)].Value == null)
            return "";
          string str = this._fullHelpObject.Properties[nameof (Name)].Value.ToString();
          return str == null ? "" : str.Trim();
        }
      }
    }

    internal override string Synopsis
    {
      get
      {
        using (ProviderHelpInfo.tracer.TraceProperty())
        {
          if (this._fullHelpObject == null || this._fullHelpObject.Properties[nameof (Synopsis)] == null || this._fullHelpObject.Properties[nameof (Synopsis)].Value == null)
            return "";
          string str = this._fullHelpObject.Properties[nameof (Synopsis)].Value.ToString();
          return str == null ? "" : str.Trim();
        }
      }
    }

    internal string DetailedDescription
    {
      get
      {
        if (this.FullHelp == null || this.FullHelp.Properties[nameof (DetailedDescription)] == null || (this.FullHelp.Properties[nameof (DetailedDescription)].Value == null || !(this.FullHelp.Properties[nameof (DetailedDescription)].Value is IList list)) || list.Count == 0)
          return "";
        StringBuilder stringBuilder = new StringBuilder(400);
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

    internal override HelpCategory HelpCategory
    {
      get
      {
        using (ProviderHelpInfo.tracer.TraceProperty())
          return HelpCategory.Provider;
      }
    }

    internal override PSObject FullHelp
    {
      get
      {
        using (ProviderHelpInfo.tracer.TraceProperty())
          return this._fullHelpObject;
      }
    }

    internal override bool MatchPatternInContent(WildcardPattern pattern)
    {
      string input1 = this.Synopsis;
      string input2 = this.DetailedDescription;
      if (input1 == null)
        input1 = string.Empty;
      if (input2 == null)
        input2 = string.Empty;
      return pattern.IsMatch(input1) || pattern.IsMatch(input2);
    }

    internal static ProviderHelpInfo Load(XmlNode xmlNode)
    {
      using (ProviderHelpInfo.tracer.TraceMethod())
      {
        ProviderHelpInfo providerHelpInfo = new ProviderHelpInfo(xmlNode);
        if (string.IsNullOrEmpty(providerHelpInfo.Name))
          return (ProviderHelpInfo) null;
        providerHelpInfo.AddCommonHelpProperties();
        return providerHelpInfo;
      }
    }
  }
}
