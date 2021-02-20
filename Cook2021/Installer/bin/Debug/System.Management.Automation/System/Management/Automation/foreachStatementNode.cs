// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.foreachStatementNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class foreachStatementNode : ParseTreeNode
  {
    private readonly Token _variable;
    private readonly string _label;
    private readonly ParseTreeNode _expression;
    private readonly ExpressionNode _rangeExpression;
    private readonly ParseTreeNode _body;
    private static readonly ScopedItemLookupPath _foreachVariablePath = new ScopedItemLookupPath("local:foreach");
    private readonly ScopedItemLookupPath _loopVariablePath;

    public foreachStatementNode(
      Token nodeToken,
      string label,
      Token variable,
      ParseTreeNode expression,
      ParseTreeNode body)
    {
      this.NodeToken = nodeToken;
      this._variable = variable;
      this._label = label == null ? "" : label;
      if (variable != null)
        this._loopVariablePath = new ScopedItemLookupPath(variable.TokenText);
      this._expression = expression;
      this._rangeExpression = expression as ExpressionNode;
      if (this._rangeExpression != null && !this._rangeExpression.IsRangeExpression())
        this._rangeExpression = (ExpressionNode) null;
      this._body = body;
    }

    internal override bool SkipDebuggerStep => true;

    internal override void Execute(
      Array input,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      if (this._expression == null)
        return;
      context.Debugger.PushStatement(this._expression);
      object obj;
      try
      {
        obj = this._rangeExpression == null ? this._expression.Execute(context) : (object) this._rangeExpression.GetEnumeratorForRangeExpression(context);
      }
      finally
      {
        context.Debugger.PopStatement();
      }
      if (obj == AutomationNull.Value)
        return;
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(obj);
      if (enumerator == null)
        enumerator = new object[1]{ obj }.GetEnumerator();
      object newValue = (object) null;
      SessionStateInternal engineSessionState = context.EngineSessionState;
      PSVariable psVariable1 = engineSessionState.GetVariableItem(foreachStatementNode._foreachVariablePath, out SessionStateScope _);
      if (psVariable1 == null)
      {
        psVariable1 = new PSVariable(foreachStatementNode._foreachVariablePath.LookupPath.NamespaceSpecificString.ToString(), (object) enumerator);
        engineSessionState.SetVariable(foreachStatementNode._foreachVariablePath, (object) psVariable1, false, true, CommandOrigin.Internal);
      }
      else
      {
        newValue = psVariable1.Value;
        psVariable1.SetValueRaw((object) enumerator, false);
      }
      PSVariable psVariable2 = (PSVariable) engineSessionState.SetVariable(this._loopVariablePath, (object) null, true, CommandOrigin.Internal);
      try
      {
        while (ParserOps.MoveNext(context, this.NodeToken, enumerator))
        {
          object current = enumerator.Current;
          if (psVariable2.WasRemoved)
            psVariable2 = (PSVariable) engineSessionState.SetVariable(this._loopVariablePath, current, true, CommandOrigin.Internal);
          else
            psVariable2.SetValueRaw(current, false);
          try
          {
            this._body.Execute(input, outputPipe, ref resultList, context);
          }
          catch (BreakException ex)
          {
            if (this.MatchLabel(ex.Label))
              break;
            throw;
          }
          catch (ContinueException ex)
          {
            if (!this.MatchLabel(ex.Label))
              throw;
          }
        }
      }
      finally
      {
        psVariable1.SetValueRaw(newValue, false);
      }
    }

    private bool MatchLabel(string label) => string.IsNullOrEmpty(label) || label.Equals(this._label, StringComparison.OrdinalIgnoreCase);

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      if (this._expression != null)
        this._expression.Accept(visitor);
      if (this._body == null)
        return;
      this._body.Accept(visitor);
    }
  }
}
