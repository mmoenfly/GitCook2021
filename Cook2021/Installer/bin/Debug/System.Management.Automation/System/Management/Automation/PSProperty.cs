// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSProperty
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Text;

namespace System.Management.Automation
{
  public class PSProperty : PSPropertyInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    internal string typeOfValue;
    internal object serializedValue;
    internal bool isDeserialized;
    internal Adapter adapter;
    internal object adapterData;
    internal object baseObject;

    public override string ToString()
    {
      if (!this.isDeserialized)
        return this.adapter.BasePropertyToString(this);
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.TypeNameOfValue);
      stringBuilder.Append(" {get;set;}");
      return stringBuilder.ToString();
    }

    internal PSProperty(string name, object serializedValue)
    {
      this.isDeserialized = true;
      this.serializedValue = serializedValue;
      this.name = name;
    }

    internal PSProperty(string name, Adapter adapter, object baseObject, object adapterData)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSProperty.tracer.NewArgumentException(nameof (name));
      this.adapter = adapter;
      this.adapterData = adapterData;
      this.baseObject = baseObject;
    }

    public override PSMemberInfo Copy()
    {
      PSProperty psProperty = new PSProperty(this.name, this.adapter, this.baseObject, this.adapterData);
      this.CloneBaseProperties((PSMemberInfo) psProperty);
      psProperty.typeOfValue = this.typeOfValue;
      psProperty.serializedValue = this.serializedValue;
      psProperty.isDeserialized = this.isDeserialized;
      return (PSMemberInfo) psProperty;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.Property;

    private object GetAdaptedValue()
    {
      if (this.isDeserialized)
        return this.serializedValue;
      object obj = this.adapter.BasePropertyGet(this);
      PSProperty.tracer.WriteLine(obj as string, new object[0]);
      return obj;
    }

    internal void SetAdaptedValue(object setValue, bool shouldConvert)
    {
      if (this.isDeserialized)
        this.serializedValue = setValue;
      else
        this.adapter.BasePropertySet(this, setValue, shouldConvert);
    }

    public override object Value
    {
      get => this.GetAdaptedValue();
      set => this.SetAdaptedValue(value, true);
    }

    public override bool IsSettable => this.isDeserialized || this.adapter.BasePropertyIsSettable(this);

    public override bool IsGettable => this.isDeserialized || this.adapter.BasePropertyIsGettable(this);

    public override string TypeNameOfValue
    {
      get
      {
        if (!this.isDeserialized)
          return this.adapter.BasePropertyType(this);
        if (this.serializedValue == null)
          return string.Empty;
        return this.serializedValue is PSObject serializedValue && serializedValue.TypeNames != null && serializedValue.TypeNames.Count >= 1 ? serializedValue.TypeNames[0] : this.serializedValue.GetType().FullName;
      }
    }
  }
}
