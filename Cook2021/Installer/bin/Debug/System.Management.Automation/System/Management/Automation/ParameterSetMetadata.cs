// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterSetMetadata
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Text;

namespace System.Management.Automation
{
  public sealed class ParameterSetMetadata
  {
    private const string MandatoryFormat = "{0}Mandatory=$true";
    private const string PositionFormat = "{0}Position={1}";
    private const string ValueFromPipelineFormat = "{0}ValueFromPipeline=$true";
    private const string ValueFromPipelineByPropertyNameFormat = "{0}ValueFromPipelineByPropertyName=$true";
    private const string ValueFromRemainingArgumentsFormat = "{0}ValueFromRemainingArguments=$true";
    private const string HelpMessageFormat = "{0}HelpMessage='{1}'";
    [TraceSource("ParameterMetadata", "The metadata associated with a parameter.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ParameterMetadata", "The metadata associated with a parameter.");
    private bool isMandatory;
    private int position;
    private bool valueFromPipeline;
    private bool valueFromPipelineByPropertyName;
    private bool valueFromRemainingArguments;
    private string helpMessage;
    private string helpMessageBaseName;
    private string helpMessageResourceId;

    internal ParameterSetMetadata(ParameterSetSpecificMetadata psMD) => this.Initialize(psMD);

    internal ParameterSetMetadata(ParameterSetMetadata other)
    {
      this.helpMessage = other != null ? other.helpMessage : throw ParameterSetMetadata.tracer.NewArgumentNullException(nameof (other));
      this.helpMessageBaseName = other.helpMessageBaseName;
      this.helpMessageResourceId = other.helpMessageResourceId;
      this.isMandatory = other.isMandatory;
      this.position = other.position;
      this.valueFromPipeline = other.valueFromPipeline;
      this.valueFromPipelineByPropertyName = other.valueFromPipelineByPropertyName;
      this.valueFromRemainingArguments = other.valueFromRemainingArguments;
    }

    public bool IsMandatory
    {
      get => this.isMandatory;
      set => this.isMandatory = value;
    }

    public int Position
    {
      get => this.position;
      set => this.position = value;
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
      set => this.helpMessage = value;
    }

    public string HelpMessageBaseName
    {
      get => this.helpMessageBaseName;
      set => this.helpMessageBaseName = value;
    }

    public string HelpMessageResourceId
    {
      get => this.helpMessageResourceId;
      set => this.helpMessageResourceId = value;
    }

    internal void Initialize(ParameterSetSpecificMetadata psMD)
    {
      this.isMandatory = psMD.IsMandatory;
      this.position = psMD.Position;
      this.valueFromPipeline = psMD.ValueFromPipeline;
      this.valueFromPipelineByPropertyName = psMD.ValueFromPipelineByPropertyName;
      this.valueFromRemainingArguments = psMD.ValueFromRemainingArguments;
      this.helpMessage = psMD.HelpMessage;
      this.helpMessageBaseName = psMD.HelpMessageBaseName;
      this.helpMessageResourceId = psMD.HelpMessageResourceId;
    }

    internal bool Equals(ParameterSetMetadata second) => this.isMandatory == second.isMandatory && this.position == second.position && (this.valueFromPipeline == second.valueFromPipeline && this.valueFromPipelineByPropertyName == second.valueFromPipelineByPropertyName) && (this.valueFromRemainingArguments == second.valueFromRemainingArguments && !(this.helpMessage != second.helpMessage) && (!(this.helpMessageBaseName != second.helpMessageBaseName) && !(this.helpMessageResourceId != second.helpMessageResourceId)));

    internal ParameterSetMetadata.ParameterFlags Flags
    {
      get
      {
        ParameterSetMetadata.ParameterFlags parameterFlags = (ParameterSetMetadata.ParameterFlags) 0;
        if (this.IsMandatory)
          parameterFlags |= ParameterSetMetadata.ParameterFlags.Mandatory;
        if (this.ValueFromPipeline)
          parameterFlags |= ParameterSetMetadata.ParameterFlags.ValueFromPipeline;
        if (this.ValueFromPipelineByPropertyName)
          parameterFlags |= ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName;
        if (this.ValueFromRemainingArguments)
          parameterFlags |= ParameterSetMetadata.ParameterFlags.ValueFromRemainingArguments;
        return parameterFlags;
      }
      set
      {
        this.IsMandatory = ParameterSetMetadata.ParameterFlags.Mandatory == (value & ParameterSetMetadata.ParameterFlags.Mandatory);
        this.ValueFromPipeline = ParameterSetMetadata.ParameterFlags.ValueFromPipeline == (value & ParameterSetMetadata.ParameterFlags.ValueFromPipeline);
        this.ValueFromPipelineByPropertyName = ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName == (value & ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName);
        this.ValueFromRemainingArguments = ParameterSetMetadata.ParameterFlags.ValueFromRemainingArguments == (value & ParameterSetMetadata.ParameterFlags.ValueFromRemainingArguments);
      }
    }

    internal ParameterSetMetadata(
      int position,
      ParameterSetMetadata.ParameterFlags flags,
      string helpMessage)
    {
      this.Position = position;
      this.Flags = flags;
      this.HelpMessage = helpMessage;
    }

    internal string GetProxyParameterData()
    {
      StringBuilder stringBuilder = new StringBuilder();
      string str = "";
      if (this.isMandatory)
      {
        ParameterSetMetadata.tracer.WriteLine("The parameter is Mandatory. Generating Mandatory attribute.", new object[0]);
        stringBuilder.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}Mandatory=$true", (object) str);
        str = ", ";
      }
      if (this.position != int.MinValue)
      {
        ParameterSetMetadata.tracer.WriteLine("The parameter is Positional. Generating Position attribute.", new object[0]);
        stringBuilder.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}Position={1}", (object) str, (object) this.position);
        str = ", ";
      }
      if (this.valueFromPipeline)
      {
        ParameterSetMetadata.tracer.WriteLine("The parameter accepts ValueFromPipeline. Generating ValueFromPipeline attribute.", new object[0]);
        stringBuilder.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}ValueFromPipeline=$true", (object) str);
        str = ", ";
      }
      if (this.valueFromPipelineByPropertyName)
      {
        ParameterSetMetadata.tracer.WriteLine("The parameter accepts ValueFromPipelineByPropertyName. Generating ValueFromPipelineByPropertyName attribute.", new object[0]);
        stringBuilder.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}ValueFromPipelineByPropertyName=$true", (object) str);
        str = ", ";
      }
      if (this.valueFromRemainingArguments)
      {
        ParameterSetMetadata.tracer.WriteLine("The parameter accepts ValueFromRemainingArguments. Generating ValueFromRemainingArguments attribute.", new object[0]);
        stringBuilder.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}ValueFromRemainingArguments=$true", (object) str);
        str = ", ";
      }
      if (!string.IsNullOrEmpty(this.helpMessage))
      {
        ParameterSetMetadata.tracer.WriteLine("The parameter has a help message. Generating HelpMessage attribute.", new object[0]);
        stringBuilder.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}HelpMessage='{1}'", (object) str, (object) CommandMetadata.EscapeSingleQuotedString(this.helpMessage));
      }
      return stringBuilder.ToString();
    }

    [System.Flags]
    internal enum ParameterFlags : uint
    {
      Mandatory = 1,
      ValueFromPipeline = 2,
      ValueFromPipelineByPropertyName = 4,
      ValueFromRemainingArguments = 8,
    }
  }
}
