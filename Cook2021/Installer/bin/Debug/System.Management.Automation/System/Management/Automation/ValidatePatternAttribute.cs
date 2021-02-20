// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ValidatePatternAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;
using System.Text.RegularExpressions;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public sealed class ValidatePatternAttribute : ValidateEnumeratedArgumentsAttribute
  {
    private string regexPattern;
    private RegexOptions options = RegexOptions.IgnoreCase;

    public string RegexPattern => this.regexPattern;

    public RegexOptions Options
    {
      set => this.options = value;
      get => this.options;
    }

    protected override void ValidateElement(object element)
    {
      string input = element != null ? element.ToString() : throw new ValidationMetadataException("ArgumentIsEmpty", (Exception) null, "Metadata", "ValidateNotNullFailure", new object[0]);
      if (!new Regex(this.regexPattern, this.options).Match(input).Success)
        throw new ValidationMetadataException("ValidatePatternFailure", (Exception) null, "Metadata", "ValidatePatternFailure", new object[2]
        {
          (object) input,
          (object) this.regexPattern
        });
    }

    public ValidatePatternAttribute(string regexPattern) => this.regexPattern = !string.IsNullOrEmpty(regexPattern) ? regexPattern : throw CmdletMetadataAttribute.tracer.NewArgumentException(nameof (regexPattern));
  }
}
