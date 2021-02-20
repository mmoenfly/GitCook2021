// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ifStatementNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class ifStatementNode : ParseTreeNode
  {
    private readonly ParseTreeNode[] _clauses;

    public ifStatementNode(Token nodeToken, ParseTreeNode[] clauses)
    {
      this._clauses = clauses;
      this.NodeToken = nodeToken;
    }

    internal override bool SkipDebuggerStep => true;

    internal override void Execute(
      Array input,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      int index1;
      for (int index2 = 0; index2 < this._clauses.Length; index2 = index1 + 1)
      {
        object obj = (object) true;
        if (this._clauses[index2] != null)
        {
          context.Debugger.PushStatement(this._clauses[index2]);
          try
          {
            obj = this._clauses[index2].Execute(input, (Pipe) null, context);
          }
          finally
          {
            context.Debugger.PopStatement();
          }
        }
        index1 = index2 + 1;
        if (LanguagePrimitives.IsTrue(obj))
        {
          this._clauses[index1].Execute(input, outputPipe, ref resultList, context);
          break;
        }
      }
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      foreach (ParseTreeNode clause in this._clauses)
        clause?.Accept(visitor);
    }
  }
}
