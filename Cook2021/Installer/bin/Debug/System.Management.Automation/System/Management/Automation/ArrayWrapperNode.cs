// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ArrayWrapperNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class ArrayWrapperNode : ParseTreeNode
  {
    private readonly ParseTreeNode _expression;
    private static readonly object[] empty = new object[0];

    public ArrayWrapperNode(Token token, ParseTreeNode expression)
    {
      this.NodeToken = token;
      this._expression = expression;
      this.ValidAttributeArgument = expression.ValidAttributeArgument;
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      ArrayList resultList = (ArrayList) null;
      this._expression.Execute(input, outputPipe, ref resultList, context);
      return resultList == null ? (object) ArrayWrapperNode.empty : (object) resultList.ToArray();
    }

    internal override object GetConstValue() => this._expression.GetConstValue() ?? (object) ArrayWrapperNode.empty;

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      this._expression.Accept(visitor);
    }
  }
}
