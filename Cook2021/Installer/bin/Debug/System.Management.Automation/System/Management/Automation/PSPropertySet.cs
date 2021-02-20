// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSPropertySet
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Management.Automation
{
  public class PSPropertySet : PSMemberInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private Collection<string> referencedPropertyNames;

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.Name);
      stringBuilder.Append(" {");
      if (this.referencedPropertyNames.Count != 0)
      {
        foreach (string referencedPropertyName in this.referencedPropertyNames)
        {
          stringBuilder.Append(referencedPropertyName);
          stringBuilder.Append(", ");
        }
        stringBuilder.Remove(stringBuilder.Length - 2, 2);
      }
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }

    public PSPropertySet(string name, IEnumerable<string> referencedPropertyNames)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSPropertySet.tracer.NewArgumentException(nameof (name));
      if (referencedPropertyNames == null)
        throw PSPropertySet.tracer.NewArgumentNullException(nameof (referencedPropertyNames));
      this.referencedPropertyNames = new Collection<string>();
      foreach (string referencedPropertyName in referencedPropertyNames)
      {
        if (string.IsNullOrEmpty(referencedPropertyName))
          throw PSPropertySet.tracer.NewArgumentException(nameof (referencedPropertyNames));
        this.referencedPropertyNames.Add(referencedPropertyName);
      }
    }

    public Collection<string> ReferencedPropertyNames => this.referencedPropertyNames;

    public override PSMemberInfo Copy()
    {
      PSPropertySet psPropertySet = new PSPropertySet(this.name, (IEnumerable<string>) this.referencedPropertyNames);
      this.CloneBaseProperties((PSMemberInfo) psPropertySet);
      return (PSMemberInfo) psPropertySet;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.PropertySet;

    public override object Value
    {
      get => (object) this;
      set => throw new ExtendedTypeSystemException("CannotChangePSPropertySetValue", (Exception) null, "ExtendedTypeSystem", "CannotSetValueForMemberType", new object[1]
      {
        (object) this.GetType().FullName
      });
    }

    public override string TypeNameOfValue => typeof (PSPropertySet).FullName;
  }
}
