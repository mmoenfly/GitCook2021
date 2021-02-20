// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ValidateNotNullOrEmptyAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public sealed class ValidateNotNullOrEmptyAttribute : ValidateArgumentsAttribute
  {
    protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
    {
      if (arguments == null || arguments == AutomationNull.Value)
        throw new ValidationMetadataException("ArgumentIsNull", (Exception) null, "Metadata", "ValidateNotNullOrEmptyFailure", new object[0]);
      switch (arguments)
      {
        case string str:
          if (!string.IsNullOrEmpty(str))
            break;
          throw new ValidationMetadataException("ArgumentIsEmpty", (Exception) null, "Metadata", "ValidateNotNullOrEmptyFailure", new object[0]);
        case IEnumerable enumerable:
          int num1 = 0;
          foreach (object obj in enumerable)
          {
            ++num1;
            if (obj == null || obj == AutomationNull.Value)
              throw new ValidationMetadataException("ArgumentIsNull", (Exception) null, "Metadata", "ValidateNotNullOrEmptyCollectionFailure", new object[0]);
            if (obj is string str && string.IsNullOrEmpty(str))
              throw new ValidationMetadataException("ArgumentCollectionContainsEmpty", (Exception) null, "Metadata", "ValidateNotNullOrEmptyFailure", new object[0]);
          }
          if (num1 != 0)
            break;
          throw new ValidationMetadataException("ArgumentIsEmpty", (Exception) null, "Metadata", "ValidateNotNullOrEmptyCollectionFailure", new object[0]);
        case IEnumerator enumerator:
          int num2 = 0;
          while (enumerator.MoveNext())
          {
            ++num2;
            if (enumerator.Current == null || enumerator.Current == AutomationNull.Value)
              throw new ValidationMetadataException("ArgumentIsNull", (Exception) null, "Metadata", "ValidateNotNullOrEmptyCollectionFailure", new object[0]);
          }
          if (num2 != 0)
            break;
          throw new ValidationMetadataException("ArgumentIsEmpty", (Exception) null, "Metadata", "ValidateNotNullOrEmptyCollectionFailure", new object[0]);
      }
    }
  }
}
