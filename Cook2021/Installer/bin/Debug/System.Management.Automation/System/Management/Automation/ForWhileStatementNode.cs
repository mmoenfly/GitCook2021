// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ForWhileStatementNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class ForWhileStatementNode : ParseTreeNode
  {
    private readonly string _label;
    private readonly ParseTreeNode _initialExpression;
    private readonly ParseTreeNode _loopExpression;
    private readonly ParseTreeNode _incrementExpression;
    private readonly ParseTreeNode _body;

    public ForWhileStatementNode(
      Token nodeToken,
      string label,
      ParseTreeNode initialExpression,
      ParseTreeNode loopExpression,
      ParseTreeNode incrementExpression,
      ParseTreeNode body)
    {
      this.NodeToken = nodeToken;
      this._label = label == null ? "" : label;
      this._initialExpression = initialExpression;
      this._loopExpression = loopExpression;
      this._incrementExpression = incrementExpression;
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
      Debugger debugger = context.Debugger;
      if (this._initialExpression != null)
      {
        debugger.PushStatement(this._initialExpression);
        try
        {
          this._initialExpression.Execute(input, (Pipe) null, context);
        }
        finally
        {
          debugger.PopStatement();
        }
      }
      while (!context.CurrentPipelineStopping)
      {
        if (this._loopExpression != null)
        {
          debugger.PushStatement(this._loopExpression);
          object obj;
          try
          {
            obj = this._loopExpression.Execute(input, (Pipe) null, context);
          }
          finally
          {
            debugger.PopStatement();
          }
          if (!LanguagePrimitives.IsTrue(obj))
            return;
        }
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
        if (this._incrementExpression != null)
        {
          debugger.PushStatement(this._incrementExpression);
          try
          {
            this._incrementExpression.Execute(input, (Pipe) null, context);
          }
          finally
          {
            debugger.PopStatement();
          }
        }
      }
      throw new PipelineStoppedException();
    }

    private bool MatchLabel(string label) => string.IsNullOrEmpty(label) || label.Equals(this._label, StringComparison.OrdinalIgnoreCase);

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      if (this._initialExpression != null)
        this._initialExpression.Accept(visitor);
      if (this._loopExpression != null)
        this._loopExpression.Accept(visitor);
      if (this._incrementExpression != null)
        this._incrementExpression.Accept(visitor);
      if (this._body == null)
        return;
      this._body.Accept(visitor);
    }
  }
}
