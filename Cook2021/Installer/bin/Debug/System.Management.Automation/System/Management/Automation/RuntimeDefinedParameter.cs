// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RuntimeDefinedParameter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public class RuntimeDefinedParameter
  {
    [TraceSource("RuntimeDefinedParameters", "The classes representing the runtime-defined parameters")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("RuntimeDefinedParameters", "The classes representing the runtime-defined parameters");
    private TypeLiteral typeLiteral;
    private string name = string.Empty;
    private Type parameterType;
    private object _value;
    private bool isSet;
    private Collection<Attribute> attributes = new Collection<Attribute>();

    public RuntimeDefinedParameter()
    {
    }

    public RuntimeDefinedParameter(
      string name,
      Type parameterType,
      Collection<Attribute> attributes)
    {
      if (string.IsNullOrEmpty(name))
        throw RuntimeDefinedParameter.tracer.NewArgumentException(nameof (name));
      if (parameterType == null)
        throw RuntimeDefinedParameter.tracer.NewArgumentNullException(nameof (parameterType));
      this.name = name;
      this.parameterType = parameterType;
      if (attributes == null)
        return;
      this.attributes = attributes;
    }

    internal RuntimeDefinedParameter(
      string name,
      TypeLiteral typeLiteral,
      Collection<Attribute> attributes)
    {
      if (string.IsNullOrEmpty(name))
        throw RuntimeDefinedParameter.tracer.NewArgumentException(nameof (name));
      if (typeLiteral == null)
        throw RuntimeDefinedParameter.tracer.NewArgumentNullException(nameof (typeLiteral));
      this.name = name;
      this.typeLiteral = typeLiteral;
      if (attributes == null)
        return;
      this.attributes = attributes;
    }

    public string Name
    {
      get => this.name;
      set => this.name = !string.IsNullOrEmpty(value) ? value : throw RuntimeDefinedParameter.tracer.NewArgumentException("name");
    }

    public Type ParameterType
    {
      get
      {
        if (this.parameterType == null && this.typeLiteral != null)
          this.parameterType = this.typeLiteral.Type;
        return this.parameterType;
      }
      set => this.parameterType = value != null ? value : throw RuntimeDefinedParameter.tracer.NewArgumentNullException(nameof (value));
    }

    public object Value
    {
      get => this._value;
      set
      {
        this.IsSet = true;
        this._value = value;
      }
    }

    public bool IsSet
    {
      get => this.isSet;
      set => this.isSet = value;
    }

    public Collection<Attribute> Attributes => this.attributes;
  }
}
