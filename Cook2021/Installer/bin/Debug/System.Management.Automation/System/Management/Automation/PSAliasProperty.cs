// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSAliasProperty
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Specialized;
using System.Globalization;
using System.Text;

namespace System.Management.Automation
{
  public class PSAliasProperty : PSPropertyInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private string referencedMemberName;
    private Type conversionType;

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.Name);
      stringBuilder.Append(" = ");
      if (this.conversionType != null)
      {
        stringBuilder.Append("(");
        stringBuilder.Append((object) this.conversionType);
        stringBuilder.Append(")");
      }
      stringBuilder.Append(this.referencedMemberName);
      return stringBuilder.ToString();
    }

    public PSAliasProperty(string name, string referencedMemberName)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSAliasProperty.tracer.NewArgumentException(nameof (name));
      this.referencedMemberName = !string.IsNullOrEmpty(referencedMemberName) ? referencedMemberName : throw PSAliasProperty.tracer.NewArgumentException(nameof (referencedMemberName));
    }

    public PSAliasProperty(string name, string referencedMemberName, Type conversionType)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSAliasProperty.tracer.NewArgumentException(nameof (name));
      this.referencedMemberName = !string.IsNullOrEmpty(referencedMemberName) ? referencedMemberName : throw PSAliasProperty.tracer.NewArgumentException(nameof (referencedMemberName));
      this.conversionType = conversionType;
    }

    public string ReferencedMemberName => this.referencedMemberName;

    internal PSMemberInfo ReferencedMember => this.LookupMember(this.referencedMemberName);

    public Type ConversionType => this.conversionType;

    public override PSMemberInfo Copy()
    {
      PSAliasProperty psAliasProperty = new PSAliasProperty(this.name, this.referencedMemberName);
      psAliasProperty.conversionType = this.conversionType;
      this.CloneBaseProperties((PSMemberInfo) psAliasProperty);
      return (PSMemberInfo) psAliasProperty;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.AliasProperty;

    public override string TypeNameOfValue => this.conversionType != null ? this.conversionType.FullName : this.ReferencedMember.TypeNameOfValue;

    public override bool IsSettable => this.ReferencedMember is PSPropertyInfo referencedMember && referencedMember.IsSettable;

    public override bool IsGettable => this.ReferencedMember is PSPropertyInfo referencedMember && referencedMember.IsGettable;

    private PSMemberInfo LookupMember(string name)
    {
      PSMemberInfo returnedMember;
      bool hasCycle;
      this.LookupMember(name, new HybridDictionary(), out returnedMember, out hasCycle);
      if (hasCycle)
        throw new ExtendedTypeSystemException("CycleInAliasLookup", (Exception) null, "ExtendedTypeSystem", "CycleInAlias", new object[1]
        {
          (object) this.Name
        });
      return returnedMember;
    }

    private void LookupMember(
      string name,
      HybridDictionary visitedAliases,
      out PSMemberInfo returnedMember,
      out bool hasCycle)
    {
      returnedMember = (PSMemberInfo) null;
      if (this.instance == null)
        throw new ExtendedTypeSystemException("AliasLookupMemberOutsidePSObject", (Exception) null, "ExtendedTypeSystem", "AccessMemberOutsidePSObject", new object[1]
        {
          (object) name
        });
      PSMemberInfo property = (PSMemberInfo) this.instance.Properties[name];
      if (property == null)
        throw new ExtendedTypeSystemException("AliasLookupMemberNotPresent", (Exception) null, "ExtendedTypeSystem", "MemberNotPresent", new object[1]
        {
          (object) name
        });
      if (!(property is PSAliasProperty psAliasProperty))
      {
        hasCycle = false;
        returnedMember = property;
      }
      else if (visitedAliases.Contains((object) name))
      {
        hasCycle = true;
      }
      else
      {
        visitedAliases.Add((object) name, (object) name);
        this.LookupMember(psAliasProperty.ReferencedMemberName, visitedAliases, out returnedMember, out hasCycle);
      }
    }

    public override object Value
    {
      get
      {
        object valueToConvert = this.ReferencedMember.Value;
        if (this.conversionType != null)
          valueToConvert = LanguagePrimitives.ConvertTo(valueToConvert, this.conversionType, (IFormatProvider) CultureInfo.InvariantCulture);
        return valueToConvert;
      }
      set => this.ReferencedMember.Value = value;
    }
  }
}
