// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ArrayLiteralNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal class ArrayLiteralNode : ParseTreeNode
  {
    private readonly ReadOnlyCollection<ParseTreeNode> _expressions;

    internal ArrayLiteralNode(Token token, IList<ParseTreeNode> expressions)
    {
      this._expressions = new ReadOnlyCollection<ParseTreeNode>(expressions);
      this.NodeToken = token;
      bool flag = true;
      foreach (ParseTreeNode expression in this._expressions)
      {
        if (!expression.ValidAttributeArgument)
        {
          flag = false;
          break;
        }
      }
      this.ValidAttributeArgument = flag;
    }

    public ReadOnlyCollection<ParseTreeNode> Expressions => this._expressions;

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      if (this._expressions == null || this._expressions.Count == 0)
        return (object) new object[0];
      object[] objArray = new object[this._expressions.Count];
      for (int index = 0; index < this._expressions.Count; ++index)
      {
        ParseTreeNode expression = this._expressions[index];
        objArray[index] = expression == null ? (object) this._expressions[index] : expression.Execute(input, (Pipe) null, context);
        if (objArray[index] == AutomationNull.Value)
          objArray[index] = (object) null;
      }
      return (object) objArray;
    }

    internal override object GetConstValue()
    {
      if (this._expressions == null || this._expressions.Count == 0)
        return (object) new object[0];
      object[] objArray = new object[this._expressions.Count];
      for (int index = 0; index < this._expressions.Count; ++index)
      {
        ParseTreeNode expression = this._expressions[index];
        objArray[index] = expression == null ? (object) this._expressions[index] : expression.GetConstValue();
        if (objArray[index] == AutomationNull.Value)
          objArray[index] = (object) null;
      }
      return (object) objArray;
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      foreach (ParseTreeNode expression in this._expressions)
        expression.Accept(visitor);
    }
  }
}
