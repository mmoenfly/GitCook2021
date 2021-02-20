// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ValidateCountAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public sealed class ValidateCountAttribute : ValidateArgumentsAttribute
  {
    private int minLength;
    private int maxLength;

    public int MinLength => this.minLength;

    public int MaxLength => this.maxLength;

    protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
    {
      uint num = 0;
      if (arguments == null || arguments == AutomationNull.Value)
      {
        num = 0U;
      }
      else
      {
        switch (arguments)
        {
          case IList list:
            num = (uint) list.Count;
            break;
          case ICollection collection:
            num = (uint) collection.Count;
            break;
          case IEnumerable enumerable:
            IEnumerator enumerator1 = enumerable.GetEnumerator();
            while (enumerator1.MoveNext())
              ++num;
            break;
          case IEnumerator enumerator2:
            while (enumerator2.MoveNext())
              ++num;
            break;
          default:
            throw new ValidationMetadataException("NotAnArrayParameter", (Exception) null, "Metadata", "ValidateCountNotInArray", new object[0]);
        }
      }
      if ((long) num < (long) this.minLength)
        throw new ValidationMetadataException("ValidateCountSmallerThanMin", (Exception) null, "Metadata", "ValidateCountMinLengthFailure", new object[2]
        {
          (object) this.minLength,
          (object) num
        });
      if ((long) num > (long) this.maxLength)
        throw new ValidationMetadataException("ValidateCountGreaterThanMax", (Exception) null, "Metadata", "ValidateCountMaxLengthFailure", new object[2]
        {
          (object) this.maxLength,
          (object) num
        });
    }

    public ValidateCountAttribute(int minLength, int maxLength)
    {
      if (minLength < 0)
        throw CmdletMetadataAttribute.tracer.NewArgumentOutOfRangeException(nameof (minLength), (object) minLength);
      if (maxLength <= 0)
        throw CmdletMetadataAttribute.tracer.NewArgumentOutOfRangeException(nameof (maxLength), (object) maxLength);
      this.minLength = maxLength >= minLength ? minLength : throw new ValidationMetadataException("ValidateRangeMaxLengthSmallerThanMinLength", (Exception) null, "Metadata", "ValidateCountMaxLengthSmallerThanMinLength", new object[0]);
      this.maxLength = maxLength;
    }
  }
}
