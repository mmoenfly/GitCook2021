// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandParameterInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public class CommandParameterInfo
  {
    [TraceSource("CmdletInfo", "The command information for MSH cmdlets that are directly executable by MSH.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CmdletInfo", "The command information for MSH cmdlets that are directly executable by MSH.");
    private string name = string.Empty;
    private Type parameterType;
    private bool isMandatory;
    private bool isDynamic;
    private int position = int.MinValue;
    private bool valueFromPipeline;
    private bool valueFromPipelineByPropertyName;
    private bool valueFromRemainingArguments;
    private string helpMessage = string.Empty;
    private ReadOnlyCollection<string> aliases;
    private ReadOnlyCollection<Attribute> attributes;

    internal CommandParameterInfo(CompiledCommandParameter parameter, uint parameterSetFlag)
    {
      using (CommandParameterInfo.tracer.TraceConstructor((object) this))
      {
        this.name = parameter != null ? parameter.Name : throw CommandParameterInfo.tracer.NewArgumentNullException(nameof (parameter));
        this.parameterType = parameter.Type;
        this.isDynamic = parameter.IsDynamic;
        this.aliases = new ReadOnlyCollection<string>((IList<string>) parameter.Aliases);
        this.SetAttributes((IList<CompiledCommandAttribute>) parameter.CompiledAttributes);
        this.SetParameterSetData(parameter.GetParameterSetData(parameterSetFlag));
      }
    }

    public string Name
    {
      get
      {
        using (CommandParameterInfo.tracer.TraceProperty(this.name, new object[0]))
          return this.name;
      }
    }

    public Type ParameterType
    {
      get
      {
        using (CommandParameterInfo.tracer.TraceProperty())
          return this.parameterType;
      }
    }

    public bool IsMandatory
    {
      get
      {
        using (CommandParameterInfo.tracer.TraceProperty((object) this.isMandatory))
          return this.isMandatory;
      }
    }

    public bool IsDynamic
    {
      get
      {
        using (CommandParameterInfo.tracer.TraceProperty((object) this.isDynamic))
          return this.isDynamic;
      }
    }

    public int Position
    {
      get
      {
        using (CommandParameterInfo.tracer.TraceProperty((object) this.position))
          return this.position;
      }
    }

    public bool ValueFromPipeline
    {
      get
      {
        using (CommandParameterInfo.tracer.TraceProperty((object) this.valueFromPipeline))
          return this.valueFromPipeline;
      }
    }

    public bool ValueFromPipelineByPropertyName
    {
      get
      {
        using (CommandParameterInfo.tracer.TraceProperty((object) this.valueFromPipelineByPropertyName))
          return this.valueFromPipelineByPropertyName;
      }
    }

    public bool ValueFromRemainingArguments
    {
      get
      {
        using (CommandParameterInfo.tracer.TraceProperty((object) this.valueFromRemainingArguments))
          return this.valueFromRemainingArguments;
      }
    }

    public string HelpMessage
    {
      get
      {
        using (CommandParameterInfo.tracer.TraceProperty(this.helpMessage, new object[0]))
          return this.helpMessage;
      }
    }

    public ReadOnlyCollection<string> Aliases
    {
      get
      {
        using (CommandParameterInfo.tracer.TraceProperty())
          return this.aliases;
      }
    }

    public ReadOnlyCollection<Attribute> Attributes
    {
      get
      {
        using (CommandParameterInfo.tracer.TraceProperty())
          return this.attributes;
      }
    }

    private void SetAttributes(IList<CompiledCommandAttribute> attributeMetadata)
    {
      using (CommandParameterInfo.tracer.TraceMethod())
      {
        Collection<Attribute> collection = new Collection<Attribute>();
        foreach (CompiledCommandAttribute commandAttribute in (IEnumerable<CompiledCommandAttribute>) attributeMetadata)
          collection.Add(commandAttribute.Instance);
        this.attributes = new ReadOnlyCollection<Attribute>((IList<Attribute>) collection);
      }
    }

    private void SetParameterSetData(ParameterSetSpecificMetadata parameterMetadata)
    {
      using (CommandParameterInfo.tracer.TraceMethod())
      {
        this.isMandatory = parameterMetadata.IsMandatory;
        this.position = parameterMetadata.Position;
        this.valueFromPipeline = parameterMetadata.valueFromPipeline;
        this.valueFromPipelineByPropertyName = parameterMetadata.valueFromPipelineByPropertyName;
        this.valueFromRemainingArguments = parameterMetadata.ValueFromRemainingArguments;
        this.helpMessage = parameterMetadata.HelpMessage;
      }
    }
  }
}
