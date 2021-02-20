// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
  public sealed class ParameterAttribute : ParsingBaseAttribute
  {
    public const string AllParameterSets = "__AllParameterSets";
    private int position = int.MinValue;
    private string parameterSetName = "__AllParameterSets";
    private bool mandatory;
    private bool valueFromRemainingArguments;
    private string helpMessage;
    private string helpMessageBaseName;
    private string helpMessageResourceId;
    private bool valueFromPipeline;
    private bool valueFromPipelineByPropertyName;

    public int Position
    {
      get => this.position;
      set => this.position = value;
    }

    public string ParameterSetName
    {
      get => this.parameterSetName;
      set
      {
        this.parameterSetName = value;
        if (!string.IsNullOrEmpty(this.parameterSetName))
          return;
        this.parameterSetName = "__AllParameterSets";
      }
    }

    public bool Mandatory
    {
      get => this.mandatory;
      set => this.mandatory = value;
    }

    public bool ValueFromPipeline
    {
      get => this.valueFromPipeline;
      set => this.valueFromPipeline = value;
    }

    public bool ValueFromPipelineByPropertyName
    {
      get => this.valueFromPipelineByPropertyName;
      set => this.valueFromPipelineByPropertyName = value;
    }

    public bool ValueFromRemainingArguments
    {
      get => this.valueFromRemainingArguments;
      set => this.valueFromRemainingArguments = value;
    }

    public string HelpMessage
    {
      get => this.helpMessage;
      set => this.helpMessage = !string.IsNullOrEmpty(value) ? value : throw CmdletMetadataAttribute.tracer.NewArgumentException(nameof (value));
    }

    public string HelpMessageBaseName
    {
      get => this.helpMessageBaseName;
      set => this.helpMessageBaseName = !string.IsNullOrEmpty(value) ? value : throw CmdletMetadataAttribute.tracer.NewArgumentException(nameof (value));
    }

    public string HelpMessageResourceId
    {
      get => this.helpMessageResourceId;
      set => this.helpMessageResourceId = !string.IsNullOrEmpty(value) ? value : throw CmdletMetadataAttribute.tracer.NewArgumentException(nameof (value));
    }
  }
}
