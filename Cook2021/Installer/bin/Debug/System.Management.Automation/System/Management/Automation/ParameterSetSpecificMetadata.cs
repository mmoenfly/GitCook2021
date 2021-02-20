// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterSetSpecificMetadata
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class ParameterSetSpecificMetadata
  {
    [TraceSource("ParameterSetSpecificMetadata", "The metadata associated with a parameterset in a bindable object in MSH.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ParameterSetSpecificMetadata), "The metadata associated with a parameterset in a bindable object in MSH.");
    private bool isMandatory;
    private int position = int.MinValue;
    private bool valueFromRemainingArguments;
    internal bool valueFromPipeline;
    internal bool valueFromPipelineByPropertyName;
    private string helpMessage;
    private string helpMessageBaseName;
    private string helpMessageResourceId;
    private bool isInAllSets;
    private uint parameterSetFlag;
    private ParameterAttribute attribute;

    internal ParameterSetSpecificMetadata(ParameterAttribute attribute)
    {
      using (ParameterSetSpecificMetadata.tracer.TraceConstructor((object) this))
      {
        this.attribute = attribute != null ? attribute : throw ParameterSetSpecificMetadata.tracer.NewArgumentNullException(nameof (attribute));
        this.isMandatory = attribute.Mandatory;
        this.position = attribute.Position;
        this.valueFromRemainingArguments = attribute.ValueFromRemainingArguments;
        this.valueFromPipeline = attribute.ValueFromPipeline;
        this.valueFromPipelineByPropertyName = attribute.ValueFromPipelineByPropertyName;
        this.helpMessage = attribute.HelpMessage;
        this.helpMessageBaseName = attribute.HelpMessageBaseName;
        this.helpMessageResourceId = attribute.HelpMessageResourceId;
      }
    }

    internal ParameterSetSpecificMetadata(
      bool isMandatory,
      int position,
      bool valueFromRemainingArguments,
      bool valueFromPipeline,
      bool valueFromPipelineByPropertyName,
      string helpMessageBaseName,
      string helpMessageResourceId,
      string helpMessage)
    {
      using (ParameterSetSpecificMetadata.tracer.TraceConstructor((object) this))
      {
        this.isMandatory = isMandatory;
        this.position = position;
        this.valueFromRemainingArguments = valueFromRemainingArguments;
        this.valueFromPipeline = valueFromPipeline;
        this.valueFromPipelineByPropertyName = valueFromPipelineByPropertyName;
        this.helpMessageBaseName = helpMessageBaseName;
        this.helpMessageResourceId = helpMessageResourceId;
        this.helpMessage = helpMessage;
      }
    }

    internal bool IsMandatory => this.isMandatory;

    internal int Position => this.position;

    internal bool IsPositional => this.position != int.MinValue;

    internal bool ValueFromRemainingArguments => this.valueFromRemainingArguments;

    internal bool ValueFromPipeline => this.valueFromPipeline;

    internal bool ValueFromPipelineByPropertyName => this.valueFromPipelineByPropertyName;

    internal string HelpMessage => this.helpMessage;

    internal string HelpMessageBaseName => this.helpMessageBaseName;

    internal string HelpMessageResourceId => this.helpMessageResourceId;

    internal bool IsInAllSets
    {
      get => this.isInAllSets;
      set => this.isInAllSets = value;
    }

    internal uint ParameterSetFlag
    {
      get => this.parameterSetFlag;
      set => this.parameterSetFlag = value;
    }

    internal string GetHelpMessage(Cmdlet cmdlet)
    {
      string str = (string) null;
      bool flag1 = !string.IsNullOrEmpty(this.HelpMessage);
      bool flag2 = !string.IsNullOrEmpty(this.HelpMessageBaseName);
      bool flag3 = !string.IsNullOrEmpty(this.HelpMessageResourceId);
      if (flag2 ^ flag3)
        throw ParameterSetSpecificMetadata.tracer.NewArgumentException(flag2 ? "HelpMessageResourceId" : "HelpMessageBaseName");
      if (flag2)
      {
        if (flag3)
        {
          try
          {
            str = cmdlet.GetResourceString(this.HelpMessageBaseName, this.HelpMessageResourceId);
            goto label_13;
          }
          catch (ArgumentException ex)
          {
            if (flag1)
            {
              str = this.HelpMessage;
              goto label_13;
            }
            else
            {
              ParameterSetSpecificMetadata.tracer.TraceException((Exception) ex);
              throw;
            }
          }
          catch (InvalidOperationException ex)
          {
            if (flag1)
            {
              str = this.HelpMessage;
              goto label_13;
            }
            else
            {
              ParameterSetSpecificMetadata.tracer.TraceException((Exception) ex);
              throw;
            }
          }
        }
      }
      if (flag1)
        str = this.HelpMessage;
label_13:
      return str;
    }
  }
}
