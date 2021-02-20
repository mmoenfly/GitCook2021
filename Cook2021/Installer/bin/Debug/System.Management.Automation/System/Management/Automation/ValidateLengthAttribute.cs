// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ValidateLengthAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public sealed class ValidateLengthAttribute : ValidateEnumeratedArgumentsAttribute
  {
    private int minLength;
    private int maxLength;

    public int MinLength => this.minLength;

    public int MaxLength => this.maxLength;

    protected override void ValidateElement(object element)
    {
      int num = element is string str ? str.Length : throw new ValidationMetadataException("ValidateLengthNotString", (Exception) null, "Metadata", "ValidateLengthNotString", new object[0]);
      if (num < this.minLength)
        throw new ValidationMetadataException("ValidateLengthMinLengthFailure", (Exception) null, "Metadata", "ValidateLengthMinLengthFailure", new object[2]
        {
          (object) this.minLength,
          (object) num
        });
      if (num > this.maxLength)
        throw new ValidationMetadataException("ValidateLengthMaxLengthFailure", (Exception) null, "Metadata", "ValidateLengthMaxLengthFailure", new object[2]
        {
          (object) this.maxLength,
          (object) num
        });
    }

    public ValidateLengthAttribute(int minLength, int maxLength)
    {
      if (minLength < 0)
        throw CmdletMetadataAttribute.tracer.NewArgumentOutOfRangeException(nameof (minLength), (object) minLength);
      if (maxLength <= 0)
        throw CmdletMetadataAttribute.tracer.NewArgumentOutOfRangeException(nameof (maxLength), (object) maxLength);
      this.minLength = maxLength >= minLength ? minLength : throw new ValidationMetadataException("ValidateLengthMaxLengthSmallerThanMinLength", (Exception) null, "Metadata", "ValidateLengthMaxLengthSmallerThanMinLength", new object[0]);
      this.maxLength = maxLength;
    }
  }
}
