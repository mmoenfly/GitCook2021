// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ValidateNotNullAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public sealed class ValidateNotNullAttribute : ValidateArgumentsAttribute
  {
    protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
    {
      if (arguments == null || arguments == AutomationNull.Value)
        throw new ValidationMetadataException("ArgumentIsNull", (Exception) null, "Metadata", "ValidateNotNullFailure", new object[0]);
      switch (arguments)
      {
        case IEnumerable enumerable:
          IEnumerator enumerator1 = enumerable.GetEnumerator();
          try
          {
            while (enumerator1.MoveNext())
            {
              object current = enumerator1.Current;
              if (current == null || current == AutomationNull.Value)
                throw new ValidationMetadataException("ArgumentIsNull", (Exception) null, "Metadata", "ValidateNotNullCollectionFailure", new object[0]);
            }
            break;
          }
          finally
          {
            if (enumerator1 is IDisposable disposable)
              disposable.Dispose();
          }
        case IEnumerator enumerator2:
          while (enumerator2.MoveNext())
          {
            if (enumerator2.Current == null || enumerator2.Current == AutomationNull.Value)
              throw new ValidationMetadataException("ArgumentIsNull", (Exception) null, "Metadata", "ValidateNotNullCollectionFailure", new object[0]);
          }
          break;
      }
    }
  }
}
