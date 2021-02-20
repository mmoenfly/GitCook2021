// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSParameterizedProperty
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public class PSParameterizedProperty : PSMethodInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    internal Adapter adapter;
    internal object adapterData;
    internal object baseObject;

    public override string ToString() => this.adapter.BaseParameterizedPropertyToString(this);

    internal PSParameterizedProperty(
      string name,
      Adapter adapter,
      object baseObject,
      object adapterData)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSParameterizedProperty.tracer.NewArgumentException(nameof (name));
      this.adapter = adapter;
      this.adapterData = adapterData;
      this.baseObject = baseObject;
    }

    internal PSParameterizedProperty(string name) => this.name = !string.IsNullOrEmpty(name) ? name : throw PSParameterizedProperty.tracer.NewArgumentException(nameof (name));

    public bool IsSettable => this.adapter.BaseParameterizedPropertyIsSettable(this);

    public bool IsGettable => this.adapter.BaseParameterizedPropertyIsGettable(this);

    public override object Invoke(params object[] arguments) => arguments != null ? this.adapter.BaseParameterizedPropertyGet(this, arguments) : throw PSParameterizedProperty.tracer.NewArgumentNullException(nameof (arguments));

    public void InvokeSet(object valueToSet, params object[] arguments)
    {
      if (arguments == null)
        throw PSParameterizedProperty.tracer.NewArgumentNullException(nameof (arguments));
      this.adapter.BaseParameterizedPropertySet(this, valueToSet, arguments);
    }

    public override Collection<string> OverloadDefinitions => this.adapter.BaseParameterizedPropertyDefinitions(this);

    public override string TypeNameOfValue => this.adapter.BaseParameterizedPropertyType(this);

    public override PSMemberInfo Copy()
    {
      PSParameterizedProperty parameterizedProperty = new PSParameterizedProperty(this.name, this.adapter, this.baseObject, this.adapterData);
      this.CloneBaseProperties((PSMemberInfo) parameterizedProperty);
      return (PSMemberInfo) parameterizedProperty;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.ParameterizedProperty;
  }
}
