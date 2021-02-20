// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.MshExpressionFactory
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class MshExpressionFactory
  {
    [TraceSource("MshExpressionFactory", "MshExpressionFactory")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (MshExpressionFactory), nameof (MshExpressionFactory));
    private CreateScriptBlockFromString _scriptBlockFactory;
    private Hashtable _expressionCache = new Hashtable();

    internal MshExpressionFactory(CreateScriptBlockFromString scriptBlockFactory) => this._scriptBlockFactory = scriptBlockFactory != null ? scriptBlockFactory : throw MshExpressionFactory.tracer.NewArgumentNullException(nameof (scriptBlockFactory));

    internal void VerifyScriptBlockText(string scriptText)
    {
      ScriptBlock scriptBlock = this._scriptBlockFactory(scriptText);
    }

    internal MshExpression CreateFromExpressionToken(ExpressionToken et)
    {
      if (!et.isScriptBlock)
        return new MshExpression(et.expressionValue);
      if (this._expressionCache.Contains((object) et))
        return this._expressionCache[(object) et] as MshExpression;
      MshExpression mshExpression = new MshExpression(this._scriptBlockFactory(et.expressionValue));
      this._expressionCache.Add((object) et, (object) mshExpression);
      return mshExpression;
    }
  }
}
