// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.BaseCommandHelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Management.Automation
{
  internal abstract class BaseCommandHelpInfo : HelpInfo
  {
    private HelpCategory _helpCategory;
    [TraceSource("MamlCommandHelpInfo", "MamlCommandHelpInfo")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("MamlCommandHelpInfo", "MamlCommandHelpInfo");

    internal BaseCommandHelpInfo(HelpCategory helpCategory) => this._helpCategory = helpCategory;

    private PSObject Details
    {
      get
      {
        if (this.FullHelp == null)
          return (PSObject) null;
        return this.FullHelp.Properties[nameof (Details)] == null || this.FullHelp.Properties[nameof (Details)].Value == null ? (PSObject) null : PSObject.AsPSObject(this.FullHelp.Properties[nameof (Details)].Value);
      }
    }

    internal override string Name
    {
      get
      {
        PSObject details = this.Details;
        if (details == null || details.Properties[nameof (Name)] == null || details.Properties[nameof (Name)].Value == null)
          return "";
        string str = details.Properties[nameof (Name)].Value.ToString();
        return str == null ? "" : str.Trim();
      }
    }

    internal override string Synopsis
    {
      get
      {
        PSObject details = this.Details;
        if (details == null || details.Properties["Description"] == null || details.Properties["Description"].Value == null)
          return "";
        object[] objArray = (object[]) LanguagePrimitives.ConvertTo(details.Properties["Description"].Value, typeof (object[]), (IFormatProvider) CultureInfo.InvariantCulture);
        if (objArray == null || objArray.Length == 0)
          return "";
        PSObject psObject = objArray[0] == null ? (PSObject) null : PSObject.AsPSObject(objArray[0]);
        if (psObject == null || psObject.Properties["Text"] == null || psObject.Properties["Text"].Value == null)
          return "";
        string str = psObject.Properties["Text"].Value.ToString();
        return str == null ? "" : str.Trim();
      }
    }

    internal override HelpCategory HelpCategory => this._helpCategory;

    internal override Uri GetUriForOnlineHelp()
    {
      if (this.FullHelp == null || this.FullHelp.Properties["relatedLinks"] == null || this.FullHelp.Properties["relatedLinks"].Value == null)
        return base.GetUriForOnlineHelp();
      PSObject psObject = PSObject.AsPSObject(this.FullHelp.Properties["relatedLinks"].Value);
      if (psObject.Properties["navigationLink"] == null)
        return base.GetUriForOnlineHelp();
      foreach (object obj in (object[]) LanguagePrimitives.ConvertTo(psObject.Properties["navigationLink"].Value, typeof (object[]), (IFormatProvider) CultureInfo.InvariantCulture))
      {
        if (obj != null && PSObject.AsPSObject(obj).Properties["uri"] is PSNoteProperty property)
        {
          string result = string.Empty;
          LanguagePrimitives.TryConvertTo<string>(property.Value, (IFormatProvider) CultureInfo.InvariantCulture, out result);
          if (!string.IsNullOrEmpty(result))
          {
            try
            {
              return new Uri(result);
            }
            catch (UriFormatException ex)
            {
              throw BaseCommandHelpInfo.tracer.NewInvalidOperationException("HelpErrors", "InvalidURI", (object) result);
            }
          }
        }
      }
      return base.GetUriForOnlineHelp();
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

    internal override PSObject[] GetParameter(string pattern)
    {
      if (this.FullHelp == null || this.FullHelp.Properties["parameters"] == null || this.FullHelp.Properties["parameters"].Value == null)
        return base.GetParameter(pattern);
      PSObject psObject1 = PSObject.AsPSObject(this.FullHelp.Properties["parameters"].Value);
      if (psObject1.Properties["parameter"] == null)
        return base.GetParameter(pattern);
      PSObject[] psObjectArray = (PSObject[]) LanguagePrimitives.ConvertTo(psObject1.Properties["parameter"].Value, typeof (PSObject[]), (IFormatProvider) CultureInfo.InvariantCulture);
      if (string.IsNullOrEmpty(pattern))
        return psObjectArray;
      List<PSObject> psObjectList = new List<PSObject>();
      WildcardPattern wildcardPattern = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
      foreach (PSObject psObject2 in psObjectArray)
      {
        if (psObject2.Properties["name"] != null && psObject2.Properties["name"].Value != null)
        {
          string input = psObject2.Properties["name"].Value.ToString();
          if (wildcardPattern.IsMatch(input))
            psObjectList.Add(psObject2);
        }
      }
      return psObjectList.ToArray();
    }

    internal string DetailedDescription
    {
      get
      {
        if (this.FullHelp == null || this.FullHelp.Properties["Description"] == null || this.FullHelp.Properties["Description"].Value == null)
          return "";
        object[] objArray = (object[]) LanguagePrimitives.ConvertTo(this.FullHelp.Properties["Description"].Value, typeof (object[]), (IFormatProvider) CultureInfo.InvariantCulture);
        if (objArray == null || objArray.Length == 0)
          return "";
        StringBuilder stringBuilder = new StringBuilder(400);
        foreach (object obj in objArray)
        {
          if (obj != null)
          {
            PSObject psObject = PSObject.AsPSObject(obj);
            if (psObject != null && psObject.Properties["Text"] != null && psObject.Properties["Text"].Value != null)
            {
              string str = psObject.Properties["Text"].Value.ToString();
              stringBuilder.Append(str);
              stringBuilder.Append(Environment.NewLine);
            }
          }
        }
        return stringBuilder.ToString().Trim();
      }
    }
  }
}
