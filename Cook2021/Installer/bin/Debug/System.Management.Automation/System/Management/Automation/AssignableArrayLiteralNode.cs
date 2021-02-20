// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AssignableArrayLiteralNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class AssignableArrayLiteralNode : 
    ArrayLiteralNode,
    IAssignableParseTreeNode,
    IAssignableValue
  {
    private List<TypeLiteral> _typeConstraint = new List<TypeLiteral>();

    public AssignableArrayLiteralNode(Token token, List<ParseTreeNode> expressions)
      : base(token, (IList<ParseTreeNode>) expressions)
    {
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      if (this._typeConstraint.Count == 0)
        return base.Execute(input, outputPipe, context);
      object obj = base.Execute(input, (Pipe) null, context);
      foreach (TypeLiteral typeLiteral in this._typeConstraint)
        obj = Parser.ConvertTo(obj, typeLiteral.Type, this.NodeToken);
      return obj;
    }

    public IAssignableValue GetAssignableValue(
      Array input,
      ExecutionContext context)
    {
      return (IAssignableValue) this;
    }

    public List<TypeLiteral> TypeConstraint => this._typeConstraint;

    public object GetValue(ExecutionContext context) => (object) null;

    public void SetValue(object value, ExecutionContext context)
    {
      foreach (TypeLiteral typeLiteral in this._typeConstraint)
        value = Parser.ConvertTo(value, typeLiteral.Type, this.NodeToken);
      value = PSObject.Base(value);
      if (!(value is IList right))
        right = (IList) new object[1]{ value };
      for (int index = 0; index < this.Expressions.Count; ++index)
        ((IAssignableParseTreeNode) this.Expressions[index]).GetAssignableValue((Array) null, context).SetValue(this.GetValueToAssign((IList) this.Expressions, right, index), context);
    }

    private object GetValueToAssign(IList left, IList right, int index)
    {
      if (index >= right.Count)
        return (object) null;
      if (index < left.Count - 1 || index == right.Count - 1)
        return right[index];
      object[] objArray = new object[right.Count - index];
      int num = index;
      for (; index < right.Count; ++index)
        objArray[index - num] = right[index];
      return (object) objArray;
    }
  }
}
