// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ValidateEnumeratedArgumentsAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public abstract class ValidateEnumeratedArgumentsAttribute : ValidateArgumentsAttribute
  {
    protected abstract void ValidateElement(object element);

    protected override sealed void Validate(object arguments, EngineIntrinsics engineIntrinsics)
    {
      IEnumerable enumerable = arguments != null && arguments != AutomationNull.Value ? LanguagePrimitives.GetEnumerable(arguments) : throw new ValidationMetadataException("ArgumentIsEmpty", (Exception) null, "Metadata", "ValidateNotNullOrEmptyCollectionFailure", new object[0]);
      if (enumerable == null)
      {
        this.ValidateElement(arguments);
      }
      else
      {
        foreach (object element in enumerable)
          this.ValidateElement(element);
      }
    }
  }
}
