// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.MshExpressionResult
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal class MshExpressionResult
  {
    private object _result;
    private MshExpression _resolvedExpression;
    private Exception _exception;

    internal MshExpressionResult(object res, MshExpression re, Exception e)
    {
      this._result = res;
      this._resolvedExpression = re;
      this._exception = e;
    }

    internal object Result => this._result;

    internal MshExpression ResolvedExpression => this._resolvedExpression;

    internal Exception Exception => this._exception;
  }
}
