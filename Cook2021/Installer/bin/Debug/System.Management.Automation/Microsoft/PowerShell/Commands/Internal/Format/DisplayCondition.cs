// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.DisplayCondition
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal static class DisplayCondition
  {
    internal static bool Evaluate(
      PSObject obj,
      MshExpression ex,
      out MshExpressionResult expressionResult)
    {
      expressionResult = (MshExpressionResult) null;
      List<MshExpressionResult> values = ex.GetValues(obj);
      if (values.Count == 0)
        return false;
      if (values[0].Exception == null)
        return LanguagePrimitives.IsTrue(values[0].Result);
      expressionResult = values[0];
      return false;
    }
  }
}
