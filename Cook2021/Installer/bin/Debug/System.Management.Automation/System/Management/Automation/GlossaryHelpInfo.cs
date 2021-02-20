// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.GlossaryHelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Text;
using System.Xml;

namespace System.Management.Automation
{
  internal class GlossaryHelpInfo : HelpInfo
  {
    private string _name = "";
    private PSObject _fullHelpObject;
    [TraceSource("GlossaryHelpInfo", "GlossaryHelpInfo")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (GlossaryHelpInfo), nameof (GlossaryHelpInfo));

    protected GlossaryHelpInfo(XmlNode xmlNode)
    {
      using (GlossaryHelpInfo.tracer.TraceConstructor((object) this))
      {
        MamlNode mamlNode = new MamlNode(xmlNode);
        this._fullHelpObject = mamlNode.PSObject;
        this.Errors = mamlNode.Errors;
        this._name = this.GetTerm();
        this._fullHelpObject.TypeNames.Clear();
        this._fullHelpObject.TypeNames.Add(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "GlossaryHelpInfo#{0}", (object) this.Name));
        this._fullHelpObject.TypeNames.Add(nameof (GlossaryHelpInfo));
        this._fullHelpObject.TypeNames.Add("HelpInfo");
      }
    }

    internal override string Name
    {
      get
      {
        using (GlossaryHelpInfo.tracer.TraceProperty())
        {
          using (GlossaryHelpInfo.tracer.TraceProperty())
            return this._name;
        }
      }
    }

    private string GetTerm()
    {
      if (this._fullHelpObject == null || this._fullHelpObject.Properties["Terms"] == null || this._fullHelpObject.Properties["Terms"].Value == null)
        return "";
      PSObject psObject = (PSObject) this._fullHelpObject.Properties["Terms"].Value;
      if (psObject.Properties["Term"] == null || psObject.Properties["Term"].Value == null)
        return "";
      if (psObject.Properties["Term"].Value.GetType().Equals(typeof (PSObject)))
        return ((PSObject) psObject.Properties["Term"].Value).ToString();
      if (!psObject.Properties["Term"].Value.GetType().Equals(typeof (PSObject[])))
        return "";
      PSObject[] psObjectArray = (PSObject[]) psObject.Properties["Term"].Value;
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < psObjectArray.Length; ++index)
      {
        string str1 = psObjectArray[index].ToString();
        if (str1 != null)
        {
          string str2 = str1.Trim();
          if (!string.IsNullOrEmpty(str2))
          {
            if (stringBuilder.Length > 0)
              stringBuilder.Append(", ");
            stringBuilder.Append(str2);
          }
        }
      }
      return stringBuilder.ToString();
    }

    internal override string Synopsis
    {
      get
      {
        using (GlossaryHelpInfo.tracer.TraceProperty())
          return "";
      }
    }

    internal override HelpCategory HelpCategory
    {
      get
      {
        using (GlossaryHelpInfo.tracer.TraceProperty())
          return HelpCategory.Glossary;
      }
    }

    internal override PSObject FullHelp
    {
      get
      {
        using (GlossaryHelpInfo.tracer.TraceProperty())
          return this._fullHelpObject;
      }
    }

    internal static GlossaryHelpInfo Load(XmlNode xmlNode)
    {
      using (GlossaryHelpInfo.tracer.TraceMethod())
      {
        GlossaryHelpInfo glossaryHelpInfo = new GlossaryHelpInfo(xmlNode);
        if (string.IsNullOrEmpty(glossaryHelpInfo.Name))
          return (GlossaryHelpInfo) null;
        glossaryHelpInfo.AddCommonHelpProperties();
        return glossaryHelpInfo;
      }
    }
  }
}
