// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.GeneralHelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Xml;

namespace System.Management.Automation
{
  internal class GeneralHelpInfo : HelpInfo
  {
    private PSObject _fullHelpObject;
    [TraceSource("GeneralHelpInfo", "GeneralHelpInfo")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (GeneralHelpInfo), nameof (GeneralHelpInfo));

    protected GeneralHelpInfo(XmlNode xmlNode)
    {
      using (GeneralHelpInfo.tracer.TraceConstructor((object) this))
      {
        MamlNode mamlNode = new MamlNode(xmlNode);
        this._fullHelpObject = mamlNode.PSObject;
        this.Errors = mamlNode.Errors;
        this._fullHelpObject.TypeNames.Clear();
        this._fullHelpObject.TypeNames.Add(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "GeneralHelpInfo#{0}", (object) this.Name));
        this._fullHelpObject.TypeNames.Add(nameof (GeneralHelpInfo));
        this._fullHelpObject.TypeNames.Add("HelpInfo");
      }
    }

    internal override string Name
    {
      get
      {
        using (GeneralHelpInfo.tracer.TraceProperty())
        {
          using (GeneralHelpInfo.tracer.TraceProperty())
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
        using (GeneralHelpInfo.tracer.TraceProperty())
          return "";
      }
    }

    internal override HelpCategory HelpCategory
    {
      get
      {
        using (GeneralHelpInfo.tracer.TraceProperty())
          return HelpCategory.General;
      }
    }

    internal override PSObject FullHelp
    {
      get
      {
        using (GeneralHelpInfo.tracer.TraceProperty())
          return this._fullHelpObject;
      }
    }

    internal static GeneralHelpInfo Load(XmlNode xmlNode)
    {
      using (GeneralHelpInfo.tracer.TraceMethod())
      {
        GeneralHelpInfo generalHelpInfo = new GeneralHelpInfo(xmlNode);
        if (string.IsNullOrEmpty(generalHelpInfo.Name))
          return (GeneralHelpInfo) null;
        generalHelpInfo.AddCommonHelpProperties();
        return generalHelpInfo;
      }
    }
  }
}
