// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSNoteProperty
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Text;

namespace System.Management.Automation
{
  public class PSNoteProperty : PSPropertyInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    internal object noteValue;

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.TypeNameOfValue);
      stringBuilder.Append(" ");
      stringBuilder.Append(this.Name);
      stringBuilder.Append("=");
      stringBuilder.Append(this.noteValue == null ? "null" : this.noteValue.ToString());
      return stringBuilder.ToString();
    }

    public PSNoteProperty(string name, object value)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSNoteProperty.tracer.NewArgumentException(nameof (name));
      this.noteValue = value;
    }

    public override PSMemberInfo Copy()
    {
      PSNoteProperty psNoteProperty = new PSNoteProperty(this.name, this.noteValue);
      this.CloneBaseProperties((PSMemberInfo) psNoteProperty);
      return (PSMemberInfo) psNoteProperty;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.NoteProperty;

    public override bool IsSettable => this.IsInstance;

    public override bool IsGettable => true;

    public override object Value
    {
      get => this.noteValue;
      set
      {
        if (!this.IsInstance)
          throw new SetValueException("ChangeValueOfStaticNote", (Exception) null, "ExtendedTypeSystem", "ChangeStaticMember", new object[1]
          {
            (object) this.Name
          });
        this.noteValue = value;
      }
    }

    public override string TypeNameOfValue
    {
      get
      {
        object obj = this.Value;
        if (obj == null)
          return string.Empty;
        return obj is PSObject psObject && psObject.TypeNames != null && psObject.TypeNames.Count >= 1 ? psObject.TypeNames[0] : obj.GetType().FullName;
      }
    }
  }
}
