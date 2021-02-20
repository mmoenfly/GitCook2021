// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ValidationMetadataException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class ValidationMetadataException : MetadataException
  {
    internal const string ValidateRangeElementType = "ValidateRangeElementType";
    internal const string ValidateRangeMinRangeMaxRangeType = "ValidateRangeMinRangeMaxRangeType";
    internal const string ValidateRangeNotIComparable = "ValidateRangeNotIComparable";
    internal const string ValidateRangeMaxRangeSmallerThanMinRange = "ValidateRangeMaxRangeSmallerThanMinRange";
    internal const string ValidateRangeGreaterThanMaxRangeFailure = "ValidateRangeGreaterThanMaxRangeFailure";
    internal const string ValidateRangeSmallerThanMinRangeFailure = "ValidateRangeSmallerThanMinRangeFailure";
    internal const string ValidateFailureResult = "ValidateFailureResult";
    internal const string ValidatePatternFailure = "ValidatePatternFailure";
    internal const string ValidateScriptFailure = "ValidateScriptFailure";
    internal const string ValidateCountNotInArray = "ValidateCountNotInArray";
    internal const string ValidateCountMaxLengthSmallerThanMinLength = "ValidateCountMaxLengthSmallerThanMinLength";
    internal const string ValidateCountMinLengthFailure = "ValidateCountMinLengthFailure";
    internal const string ValidateCountMaxLengthFailure = "ValidateCountMaxLengthFailure";
    internal const string ValidateLengthMaxLengthSmallerThanMinLength = "ValidateLengthMaxLengthSmallerThanMinLength";
    internal const string ValidateLengthNotString = "ValidateLengthNotString";
    internal const string ValidateLengthMinLengthFailure = "ValidateLengthMinLengthFailure";
    internal const string ValidateLengthMaxLengthFailure = "ValidateLengthMaxLengthFailure";
    internal const string ValidateSetSeparator = "ValidateSetSeparator";
    internal const string ValidateSetFailure = "ValidateSetFailure";
    internal const string ValidateVersionFailure = "ValidateVersionFailure";
    internal const string InvalidValueFailure = "InvalidValueFailure";

    protected ValidationMetadataException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public ValidationMetadataException()
      : base(typeof (ValidationMetadataException).FullName)
    {
    }

    public ValidationMetadataException(string message)
      : base(message)
    {
    }

    public ValidationMetadataException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    internal ValidationMetadataException(
      string errorId,
      Exception innerException,
      string baseName,
      string resourceId,
      params object[] arguments)
      : base(errorId, innerException, baseName, resourceId, arguments)
    {
    }
  }
}
