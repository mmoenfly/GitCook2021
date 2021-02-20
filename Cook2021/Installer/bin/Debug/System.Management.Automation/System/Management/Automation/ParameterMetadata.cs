// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterMetadata
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace System.Management.Automation
{
  public sealed class ParameterMetadata
  {
    private const string ResBaseName = "ProxyCommandStrings";
    private const string ParameterNameFormat = "{0}${{{1}}}";
    private const string ParameterTypeFormat = "{0}[{1}]";
    private const string ParameterSetNameFormat = "ParameterSetName='{0}'";
    private const string AliasesFormat = "{0}[Alias({1})]";
    private const string ValidateLengthFormat = "{0}[ValidateLength({1}, {2})]";
    private const string ValidateRangeFloatFormat = "{0}[ValidateRange({1:R}, {2:R})]";
    private const string ValidateRangeFormat = "{0}[ValidateRange({1}, {2})]";
    private const string ValidatePatternFormat = "{0}[ValidatePattern('{1}')]";
    private const string ValidateScriptFormat = "{0}[ValidateScript({{ {1} }})]";
    private const string ValidateCountFormat = "{0}[ValidateCount({1}, {2})]";
    private const string ValidateSetFormat = "{0}[ValidateSet({1})]";
    private const string ValidateNotNullFormat = "{0}[ValidateNotNull()]";
    private const string ValidateNotNullOrEmptyFormat = "{0}[ValidateNotNullOrEmpty()]";
    private const string AllowNullFormat = "{0}[AllowNull()]";
    private const string AllowEmptyStringFormat = "{0}[AllowEmptyString()]";
    private const string AllowEmptyCollectionFormat = "{0}[AllowEmptyCollection()]";
    [TraceSource("ParameterMetadata", "The metadata associated with a parameter.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ParameterMetadata), "The metadata associated with a parameter.");
    private string name;
    private Type parameterType;
    private bool isDynamic;
    private Dictionary<string, ParameterSetMetadata> parameterSets;
    private Collection<string> aliases;
    private Collection<Attribute> attributes;

    public ParameterMetadata(string name)
      : this(name, (Type) null)
    {
    }

    public ParameterMetadata(string name, Type parameterType)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw ParameterMetadata.tracer.NewArgumentNullException(nameof (name));
      this.parameterType = parameterType;
      this.attributes = new Collection<Attribute>();
      this.aliases = new Collection<string>();
      this.parameterSets = new Dictionary<string, ParameterSetMetadata>();
    }

    public ParameterMetadata(ParameterMetadata other)
    {
      this.isDynamic = other != null ? other.isDynamic : throw ParameterMetadata.tracer.NewArgumentNullException(nameof (other));
      this.name = other.name;
      this.parameterType = other.parameterType;
      this.aliases = new Collection<string>((IList<string>) new List<string>(other.aliases.Count));
      foreach (string alias in other.aliases)
        this.aliases.Add(alias);
      if (other.attributes == null)
      {
        this.attributes = (Collection<Attribute>) null;
      }
      else
      {
        this.attributes = new Collection<Attribute>((IList<Attribute>) new List<Attribute>(other.attributes.Count));
        foreach (Attribute attribute in other.attributes)
          this.attributes.Add(attribute);
      }
      this.parameterSets = (Dictionary<string, ParameterSetMetadata>) null;
      if (other.parameterSets == null)
      {
        this.parameterSets = (Dictionary<string, ParameterSetMetadata>) null;
      }
      else
      {
        this.parameterSets = new Dictionary<string, ParameterSetMetadata>(other.parameterSets.Count);
        foreach (KeyValuePair<string, ParameterSetMetadata> parameterSet in other.parameterSets)
          this.parameterSets.Add(parameterSet.Key, new ParameterSetMetadata(parameterSet.Value));
      }
    }

    internal ParameterMetadata(CompiledCommandParameter cmdParameterMD) => this.Initialize(cmdParameterMD);

    internal ParameterMetadata(
      Collection<string> aliases,
      bool isDynamic,
      string name,
      Dictionary<string, ParameterSetMetadata> parameterSets,
      Type parameterType)
    {
      this.aliases = aliases;
      this.isDynamic = isDynamic;
      this.name = name;
      this.parameterSets = parameterSets;
      this.parameterType = parameterType;
    }

    public string Name
    {
      get => this.name;
      set => this.name = !string.IsNullOrEmpty(value) ? value : throw ParameterMetadata.tracer.NewArgumentNullException(nameof (Name));
    }

    public Type ParameterType
    {
      get => this.parameterType;
      set => this.parameterType = value;
    }

    public Dictionary<string, ParameterSetMetadata> ParameterSets => this.parameterSets;

    public bool IsDynamic
    {
      get => this.isDynamic;
      set => this.isDynamic = value;
    }

    public Collection<string> Aliases => this.aliases;

    public Collection<Attribute> Attributes => this.attributes;

    public bool SwitchParameter => this.parameterType != null && this.parameterType.Equals(typeof (System.Management.Automation.SwitchParameter));

    public static Dictionary<string, ParameterMetadata> GetParameterMetadata(
      Type type)
    {
      return type != null ? new CommandMetadata(type).Parameters : throw ParameterMetadata.tracer.NewArgumentNullException(nameof (type));
    }

    internal void Initialize(CompiledCommandParameter compiledParameterMD)
    {
      ParameterMetadata.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Constructing metadata for parameter {0}", (object) compiledParameterMD.Name), new object[0]);
      this.name = compiledParameterMD.Name;
      this.parameterType = compiledParameterMD.Type;
      this.isDynamic = compiledParameterMD.IsDynamic;
      ParameterMetadata.tracer.WriteLine("Constructing ParameterSet metadata", new object[0]);
      this.parameterSets = new Dictionary<string, ParameterSetMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (string key in compiledParameterMD.ParameterSetData.Keys)
      {
        ParameterSetSpecificMetadata psMD = compiledParameterMD.ParameterSetData[key];
        this.parameterSets.Add(key, new ParameterSetMetadata(psMD));
      }
      ParameterMetadata.tracer.WriteLine("Constructing Aliases metadata", new object[0]);
      this.aliases = new Collection<string>();
      foreach (string alias in compiledParameterMD.Aliases)
        this.aliases.Add(alias);
      ParameterMetadata.tracer.WriteLine("Constructing Attributes for this parameter", new object[0]);
      this.attributes = new Collection<Attribute>();
      foreach (CompiledCommandAttribute compiledAttribute in compiledParameterMD.CompiledAttributes)
        this.attributes.Add(compiledAttribute.Instance);
    }

    internal static Dictionary<string, ParameterMetadata> GetParameterMetadata(
      MergedCommandParameterMetadata cmdParameterMetadata)
    {
      Dictionary<string, ParameterMetadata> dictionary = new Dictionary<string, ParameterMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (string key in cmdParameterMetadata.BindableParameters.Keys)
      {
        ParameterMetadata parameterMetadata = new ParameterMetadata(cmdParameterMetadata.BindableParameters[key].Parameter);
        dictionary.Add(key, parameterMetadata);
      }
      return dictionary;
    }

    internal string GetProxyParameterData(
      string prefix,
      string paramNameOverride,
      bool isProxyForCmdlet)
    {
      StringBuilder stringBuilder1 = new StringBuilder();
      if (this.parameterSets != null && isProxyForCmdlet)
      {
        foreach (string key in this.parameterSets.Keys)
        {
          string proxyParameterData = this.parameterSets[key].GetProxyParameterData();
          if (!string.IsNullOrEmpty(proxyParameterData) || !key.Equals("__AllParameterSets"))
          {
            string str = "";
            stringBuilder1.Append(prefix);
            stringBuilder1.Append("[Parameter(");
            if (!key.Equals("__AllParameterSets"))
            {
              stringBuilder1.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "ParameterSetName='{0}'", (object) CommandMetadata.EscapeSingleQuotedString(key));
              str = ", ";
            }
            if (!string.IsNullOrEmpty(proxyParameterData))
            {
              stringBuilder1.Append(str);
              stringBuilder1.Append(proxyParameterData);
            }
            stringBuilder1.Append(")]");
          }
        }
      }
      if (this.aliases != null && this.aliases.Count > 0)
      {
        ParameterMetadata.tracer.WriteLine("Generating Aliases attribue.", new object[0]);
        StringBuilder stringBuilder2 = new StringBuilder();
        string str = "";
        foreach (string alias in this.aliases)
        {
          stringBuilder2.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}'{1}'", (object) str, (object) CommandMetadata.EscapeSingleQuotedString(alias));
          str = ",";
        }
        stringBuilder1.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}[Alias({1})]", (object) prefix, (object) stringBuilder2.ToString());
      }
      if (this.attributes != null && this.attributes.Count > 0)
      {
        ParameterMetadata.tracer.WriteLine("Generating parameter validation attributes", new object[0]);
        foreach (Attribute attribute in this.attributes)
        {
          string proxyAttributeData = this.GetProxyAttributeData(attribute, prefix);
          if (!string.IsNullOrEmpty(proxyAttributeData))
            stringBuilder1.Append(proxyAttributeData);
        }
      }
      ParameterMetadata.tracer.WriteLine("Generating ParameterName and ParameterType.", new object[0]);
      if (this.SwitchParameter)
        stringBuilder1.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}[{1}]", (object) prefix, (object) "Switch");
      else if (this.parameterType != null)
      {
        string str = !this.parameterType.IsGenericType ? this.parameterType.FullName : this.parameterType.AssemblyQualifiedName.Replace("`", "``");
        stringBuilder1.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}[{1}]", (object) prefix, (object) str);
      }
      stringBuilder1.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}${{{1}}}", (object) prefix, (object) CommandMetadata.EscapeVariableName(string.IsNullOrEmpty(paramNameOverride) ? this.name : paramNameOverride));
      return stringBuilder1.ToString();
    }

    private string GetProxyAttributeData(Attribute attrib, string prefix)
    {
      switch (attrib)
      {
        case ValidateLengthAttribute validateLengthAttribute:
          ParameterMetadata.tracer.WriteLine("ValidateLengthAttribute found.", new object[0]);
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}[ValidateLength({1}, {2})]", (object) prefix, (object) validateLengthAttribute.MinLength, (object) validateLengthAttribute.MaxLength);
        case ValidateRangeAttribute validateRangeAttribute:
          ParameterMetadata.tracer.WriteLine("ValidateRangeAttribute found.", new object[0]);
          Type type = validateRangeAttribute.MinRange.GetType();
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, type == typeof (float) || type == typeof (double) ? "{0}[ValidateRange({1:R}, {2:R})]" : "{0}[ValidateRange({1}, {2})]", (object) prefix, validateRangeAttribute.MinRange, validateRangeAttribute.MaxRange);
        case AllowNullAttribute _:
          ParameterMetadata.tracer.WriteLine("AllowNullAttribute found.", new object[0]);
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}[AllowNull()]", (object) prefix);
        case AllowEmptyStringAttribute _:
          ParameterMetadata.tracer.WriteLine("AllowEmptyStringAttribute found.", new object[0]);
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}[AllowEmptyString()]", (object) prefix);
        case AllowEmptyCollectionAttribute _:
          ParameterMetadata.tracer.WriteLine("AllowEmptyCollectionAttribute found.", new object[0]);
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}[AllowEmptyCollection()]", (object) prefix);
        case ValidatePatternAttribute patternAttribute:
          ParameterMetadata.tracer.WriteLine("ValidatePatternAttribute found.", new object[0]);
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}[ValidatePattern('{1}')]", (object) prefix, (object) CommandMetadata.EscapeSingleQuotedString(patternAttribute.RegexPattern));
        case ValidateCountAttribute validateCountAttribute:
          ParameterMetadata.tracer.WriteLine("ValidateCountAttribute found.", new object[0]);
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}[ValidateCount({1}, {2})]", (object) prefix, (object) validateCountAttribute.MinLength, (object) validateCountAttribute.MaxLength);
        case ValidateNotNullAttribute _:
          ParameterMetadata.tracer.WriteLine("ValidateNotNullAttribute found.", new object[0]);
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}[ValidateNotNull()]", (object) prefix);
        case ValidateNotNullOrEmptyAttribute _:
          ParameterMetadata.tracer.WriteLine("ValidateNotNullOrEmptyAttribute found.", new object[0]);
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}[ValidateNotNullOrEmpty()]", (object) prefix);
        case ValidateSetAttribute validateSetAttribute:
          ParameterMetadata.tracer.WriteLine("ValidateSetAttribute found.", new object[0]);
          StringBuilder stringBuilder = new StringBuilder();
          string str = "";
          foreach (string validValue in (IEnumerable<string>) validateSetAttribute.ValidValues)
          {
            stringBuilder.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}'{1}'", (object) str, (object) CommandMetadata.EscapeSingleQuotedString(validValue));
            str = ",";
          }
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}[ValidateSet({1})]", (object) prefix, (object) stringBuilder.ToString());
        case ValidateScriptAttribute validateScriptAttribute:
          ParameterMetadata.tracer.WriteLine("ValidateScriptAttribute found.", new object[0]);
          return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}[ValidateScript({{ {1} }})]", (object) prefix, (object) validateScriptAttribute.ScriptBlock.ToString());
        default:
          ParameterMetadata.tracer.WriteLine("Cannot understand attribute {0}", (object) attrib.GetType().ToString());
          return (string) null;
      }
    }
  }
}
