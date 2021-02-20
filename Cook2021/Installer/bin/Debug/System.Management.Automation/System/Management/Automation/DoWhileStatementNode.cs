// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DoWhileStatementNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class DoWhileStatementNode : ParseTreeNode
  {
    private readonly string _label;
    private readonly ParseTreeNode _condition;
    private readonly ParseTreeNode _body;
    private readonly bool _doWhile;

    public DoWhileStatementNode(
      Token nodeToken,
      string label,
      ParseTreeNode condition,
      bool doWhile,
      ParseTreeNode body)
    {
      this.NodeToken = nodeToken;
      this._label = label == null ? "" : label;
      this._condition = condition;
      this._body = body;
      this._doWhile = doWhile;
    }

    internal override bool SkipDebuggerStep => true;

    internal override void Execute(
      Array input,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      while (!context.CurrentPipelineStopping)
      {
        try
        {
          this._body.Execute(input, outputPipe, ref resultList, context);
        }
        catch (BreakException ex)
        {
          if (this.MatchLabel(ex.Label))
            return;
          throw;
        }
        catch (ContinueException ex)
        {
          if (!this.MatchLabel(ex.Label))
            throw;
        }
        if (this._condition != null)
        {
          context.Debugger.PushStatement(this._condition);
          object obj;
          try
          {
            obj = this._condition.Execute(input, (Pipe) null, context);
          }
          finally
          {
            context.Debugger.PopStatement();
          }
          if (LanguagePrimitives.IsTrue(obj) != this._doWhile)
            return;
        }
      }
      throw new PipelineStoppedException();
    }

    private bool MatchLabel(string label) => string.IsNullOrEmpty(label) || label.Equals(this._label, StringComparison.OrdinalIgnoreCase);

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      this._body.Accept(visitor);
      this._condition.Accept(visitor);
    }
  }
}
