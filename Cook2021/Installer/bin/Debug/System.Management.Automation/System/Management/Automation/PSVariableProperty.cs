// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSVariableProperty
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Text;

namespace System.Management.Automation
{
  public class PSVariableProperty : PSNoteProperty
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    internal PSVariable _variable;

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.TypeNameOfValue);
      stringBuilder.Append(" ");
      stringBuilder.Append(this._variable.Name);
      stringBuilder.Append("=");
      stringBuilder.Append(this._variable.Value == null ? (object) "null" : this._variable.Value);
      return stringBuilder.ToString();
    }

    public PSVariableProperty(PSVariable variable)
      : base(variable?.Name, (object) null)
      => this._variable = variable != null ? variable : throw PSVariableProperty.tracer.NewArgumentException(nameof (variable));

    public override PSMemberInfo Copy()
    {
      PSNoteProperty psNoteProperty = (PSNoteProperty) new PSVariableProperty(this._variable);
      this.CloneBaseProperties((PSMemberInfo) psNoteProperty);
      return (PSMemberInfo) psNoteProperty;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.NoteProperty;

    public override bool IsSettable => (this._variable.Options & (ScopedItemOptions.ReadOnly | ScopedItemOptions.Constant)) == ScopedItemOptions.None;

    public override bool IsGettable => true;

    public override object Value
    {
      get => this._variable.Value;
      set
      {
        if (!this.IsInstance)
          throw new SetValueException("ChangeValueOfStaticNote", (Exception) null, "ExtendedTypeSystem", "ChangeStaticMember", new object[1]
          {
            (object) this.Name
          });
        this._variable.Value = value;
      }
    }

    public override string TypeNameOfValue
    {
      get
      {
        object obj = this._variable.Value;
        if (obj == null)
          return string.Empty;
        return obj is PSObject psObject && psObject.TypeNames != null && psObject.TypeNames.Count >= 1 ? psObject.TypeNames[0] : obj.GetType().FullName;
      }
    }
  }
}
