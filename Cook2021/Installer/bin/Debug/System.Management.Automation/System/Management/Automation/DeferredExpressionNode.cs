﻿// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DeferredExpressionNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal class DeferredExpressionNode : ParseTreeNode
  {
    private readonly ParseTreeNode _expression;

    internal DeferredExpressionNode(ParseTreeNode expression)
    {
      this.NodeToken = expression.NodeToken;
      this.IsConstant = expression.IsConstant;
      this._expression = expression;
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context) => (object) this._expression;

    internal override object GetConstValue() => this._expression.GetConstValue();

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      this._expression.Accept(visitor);
    }
  }
}
