// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.ChoiceDescription
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Host
{
  public sealed class ChoiceDescription
  {
    private const string StringsBaseName = "DescriptionsStrings";
    private const string NullOrEmptyErrorTemplateResource = "NullOrEmptyErrorTemplate";
    private readonly string label;
    private string helpMessage = "";
    [TraceSource("ChoiceDescription", "Describes a choice when choosing an option.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ChoiceDescription), "Describes a choice when choosing an option.");

    public ChoiceDescription(string label)
    {
      using (ChoiceDescription.tracer.TraceConstructor((object) this))
        this.label = !string.IsNullOrEmpty(label) ? label : throw ChoiceDescription.tracer.NewArgumentException(nameof (label), "DescriptionsStrings", "NullOrEmptyErrorTemplate", (object) nameof (label));
    }

    public ChoiceDescription(string label, string helpMessage)
    {
      using (ChoiceDescription.tracer.TraceConstructor((object) this))
      {
        if (string.IsNullOrEmpty(label))
          throw ChoiceDescription.tracer.NewArgumentException(nameof (label), "DescriptionsStrings", "NullOrEmptyErrorTemplate", (object) nameof (label));
        if (helpMessage == null)
          throw ChoiceDescription.tracer.NewArgumentNullException(nameof (helpMessage));
        this.label = label;
        this.helpMessage = helpMessage;
      }
    }

    internal ChoiceDescription(
      string resStringBaseName,
      string labelResourceId,
      string helpResourceId)
      : this(ResourceManagerCache.GetResourceString(resStringBaseName, labelResourceId), ResourceManagerCache.GetResourceString(resStringBaseName, helpResourceId))
    {
    }

    public string Label
    {
      get
      {
        using (ChoiceDescription.tracer.TraceProperty(this.label, new object[0]))
          return this.label;
      }
    }

    public string HelpMessage
    {
      get
      {
        using (ChoiceDescription.tracer.TraceProperty(this.helpMessage, new object[0]))
          return this.helpMessage;
      }
      set
      {
        using (ChoiceDescription.tracer.TraceProperty(value, new object[0]))
          this.helpMessage = value != null ? value : throw ChoiceDescription.tracer.NewArgumentNullException(nameof (value));
      }
    }
  }
}
