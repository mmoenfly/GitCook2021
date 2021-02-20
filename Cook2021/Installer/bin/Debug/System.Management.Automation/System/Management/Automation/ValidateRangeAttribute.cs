// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ValidateRangeAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public sealed class ValidateRangeAttribute : ValidateEnumeratedArgumentsAttribute
  {
    private object minRange;
    private IComparable minComparable;
    private object maxRange;
    private IComparable maxComparable;
    private Type promotedType;

    public object MinRange => this.minRange;

    public object MaxRange => this.maxRange;

    protected override void ValidateElement(object element)
    {
      if (element == null)
        throw new ValidationMetadataException("ArgumentIsEmpty", (Exception) null, "Metadata", "ValidateNotNullFailure", new object[0]);
      if (element is PSObject)
        element = ((PSObject) element).BaseObject;
      if (!element.GetType().Equals(this.promotedType))
      {
        object result;
        element = LanguagePrimitives.TryConvertTo(element, this.promotedType, out result) ? result : throw new ValidationMetadataException("ValidationRangeElementType", (Exception) null, "Metadata", "ValidateRangeElementType", new object[2]
        {
          (object) element.GetType().Name,
          (object) this.minRange.GetType().Name
        });
      }
      if (this.minComparable.CompareTo(element) > 0)
        throw new ValidationMetadataException("ValidateRangeTooSmall", (Exception) null, "Metadata", "ValidateRangeSmallerThanMinRangeFailure", new object[2]
        {
          (object) element.ToString(),
          (object) this.minRange.ToString()
        });
      if (this.maxComparable.CompareTo(element) < 0)
        throw new ValidationMetadataException("ValidateRangeTooBig", (Exception) null, "Metadata", "ValidateRangeGreaterThanMaxRangeFailure", new object[2]
        {
          (object) element.ToString(),
          (object) this.maxRange.ToString()
        });
    }

    public ValidateRangeAttribute(object minRange, object maxRange)
    {
      if (minRange == null)
        throw CmdletMetadataAttribute.tracer.NewArgumentNullException(nameof (minRange));
      if (maxRange == null)
        throw CmdletMetadataAttribute.tracer.NewArgumentNullException(nameof (maxRange));
      if (!maxRange.GetType().Equals(minRange.GetType()))
      {
        bool flag = true;
        this.promotedType = ParserOps.figureOutOperationType((Token) null, ref minRange, out Type _, ref maxRange, out Type _);
        object result;
        if (this.promotedType != null && LanguagePrimitives.TryConvertTo(minRange, this.promotedType, out result))
        {
          minRange = result;
          if (LanguagePrimitives.TryConvertTo(maxRange, this.promotedType, out result))
          {
            maxRange = result;
            flag = false;
          }
        }
        if (flag)
          throw new ValidationMetadataException("MinRangeNotTheSameTypeOfMaxRange", (Exception) null, "Metadata", "ValidateRangeMinRangeMaxRangeType", new object[2]
          {
            (object) minRange.GetType().Name,
            (object) maxRange.GetType().Name
          });
      }
      else
        this.promotedType = minRange.GetType();
      this.minComparable = minRange as IComparable;
      if (this.minComparable == null)
        throw new ValidationMetadataException("MinRangeNotIComparable", (Exception) null, "Metadata", "ValidateRangeNotIComparable", new object[0]);
      this.maxComparable = maxRange as IComparable;
      if (this.minComparable.CompareTo(maxRange) > 0)
        throw new ValidationMetadataException("MaxRangeSmallerThanMinRange", (Exception) null, "Metadata", "ValidateRangeMaxRangeSmallerThanMinRange", new object[0]);
      this.minRange = minRange;
      this.maxRange = maxRange;
    }
  }
}
