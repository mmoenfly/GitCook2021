// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MamlCommandHelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Text;
using System.Xml;

namespace System.Management.Automation
{
  internal class MamlCommandHelpInfo : BaseCommandHelpInfo
  {
    private PSObject _fullHelpObject;
    private string _component;
    private string _role;
    private string _functionality;
    [TraceSource("MamlCommandHelpInfo", "MamlCommandHelpInfo")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (MamlCommandHelpInfo), nameof (MamlCommandHelpInfo));

    private MamlCommandHelpInfo(XmlNode xmlNode, HelpCategory helpCategory)
      : base(helpCategory)
    {
      MamlNode mamlNode = new MamlNode(xmlNode);
      this._fullHelpObject = mamlNode.PSObject;
      this.Errors = mamlNode.Errors;
      this._fullHelpObject.TypeNames.Clear();
      this._fullHelpObject.TypeNames.Add(nameof (MamlCommandHelpInfo));
      this._fullHelpObject.TypeNames.Add("HelpInfo");
      this.ForwardHelpCategory = HelpCategory.Provider;
    }

    internal void OverrideProviderSpecificHelpWithGenericHelp(HelpInfo genericHelpInfo)
    {
      PSObject fullHelp = genericHelpInfo.FullHelp;
      MamlUtil.OverrideName(this._fullHelpObject, fullHelp);
      MamlUtil.PrependSyntax(this._fullHelpObject, fullHelp);
      MamlUtil.PrependDetailedDescription(this._fullHelpObject, fullHelp);
      MamlUtil.OverrideParameters(this._fullHelpObject, fullHelp);
      MamlUtil.PrependNotes(this._fullHelpObject, fullHelp);
    }

    internal override PSObject FullHelp => this._fullHelpObject;

    private string Examples => this.FullHelp == null || this.FullHelp.Properties[nameof (Examples)] == null || this.FullHelp.Properties[nameof (Examples)].Value == null ? string.Empty : this.ExtractText(PSObject.AsPSObject(this.FullHelp.Properties[nameof (Examples)].Value));

    private string Notes => this.FullHelp == null || this.FullHelp.Properties["alertset"] == null || this.FullHelp.Properties["alertset"].Value == null ? string.Empty : this.ExtractText(PSObject.AsPSObject(this.FullHelp.Properties["alertset"].Value));

    internal override string Component => this._component;

    internal override string Role => this._role;

    internal override string Functionality => this._functionality;

    internal void SetAdditionalDataFromHelpComment(
      string component,
      string functionality,
      string role)
    {
      this._component = component;
      this._functionality = functionality;
      this._role = role;
      this.UpdateUserDefinedDataProperties();
    }

    internal void AddUserDefinedData(UserDefinedHelpData userDefinedData)
    {
      if (userDefinedData == null)
        return;
      if (userDefinedData.Properties.ContainsKey("component"))
        this._component = userDefinedData.Properties["component"];
      if (userDefinedData.Properties.ContainsKey("role"))
        this._role = userDefinedData.Properties["role"];
      if (userDefinedData.Properties.ContainsKey("functionality"))
        this._functionality = userDefinedData.Properties["functionality"];
      this.UpdateUserDefinedDataProperties();
    }

    internal static MamlCommandHelpInfo Load(
      XmlNode xmlNode,
      HelpCategory helpCategory)
    {
      MamlCommandHelpInfo mamlCommandHelpInfo = new MamlCommandHelpInfo(xmlNode, helpCategory);
      if (string.IsNullOrEmpty(mamlCommandHelpInfo.Name))
        return (MamlCommandHelpInfo) null;
      mamlCommandHelpInfo.AddCommonHelpProperties();
      return mamlCommandHelpInfo;
    }

    private string ExtractText(PSObject psObject)
    {
      if (psObject == null)
        return string.Empty;
      StringBuilder stringBuilder = new StringBuilder(400);
      foreach (PSPropertyInfo property in psObject.Properties)
      {
        switch (property.TypeNameOfValue.ToLowerInvariant())
        {
          case "system.boolean":
          case "system.int32":
            continue;
          case "system.string":
            stringBuilder.Append((string) LanguagePrimitives.ConvertTo(property.Value, typeof (string), (IFormatProvider) CultureInfo.InvariantCulture));
            continue;
          case "system.management.automation.psobject[]":
            foreach (PSObject psObject1 in (PSObject[]) LanguagePrimitives.ConvertTo(property.Value, typeof (PSObject[]), (IFormatProvider) CultureInfo.InvariantCulture))
              stringBuilder.Append(this.ExtractText(psObject1));
            continue;
          case "system.management.automation.psobject":
            stringBuilder.Append(this.ExtractText(PSObject.AsPSObject(property.Value)));
            continue;
          default:
            stringBuilder.Append(this.ExtractText(PSObject.AsPSObject(property.Value)));
            continue;
        }
      }
      return stringBuilder.ToString();
    }

    internal override bool MatchPatternInContent(WildcardPattern pattern)
    {
      string synopsis = this.Synopsis;
      if (!string.IsNullOrEmpty(synopsis) && pattern.IsMatch(synopsis))
        return true;
      string detailedDescription = this.DetailedDescription;
      if (!string.IsNullOrEmpty(detailedDescription) && pattern.IsMatch(detailedDescription))
        return true;
      string examples = this.Examples;
      if (!string.IsNullOrEmpty(examples) && pattern.IsMatch(examples))
        return true;
      string notes = this.Notes;
      return !string.IsNullOrEmpty(notes) && pattern.IsMatch(notes);
    }
  }
}
