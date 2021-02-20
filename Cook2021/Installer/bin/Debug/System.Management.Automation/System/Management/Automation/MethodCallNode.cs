// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MethodCallNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class MethodCallNode : ParseTreeNode, IAssignableParseTreeNode
  {
    private readonly ParseTreeNode _target;
    private readonly ParseTreeNode _arguments;
    private readonly bool _isStatic;
    private List<TypeLiteral> _typeConstraint;

    public MethodCallNode(
      ParseTreeNode target,
      Token methodToken,
      ParseTreeNode arguments,
      bool isStatic)
    {
      this._target = target;
      this.NodeToken = methodToken;
      this._arguments = arguments;
      this._isStatic = isStatic;
    }

    public List<TypeLiteral> TypeConstraint
    {
      get
      {
        if (this._typeConstraint == null)
          this._typeConstraint = new List<TypeLiteral>();
        return this._typeConstraint;
      }
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      object obj = this.InvokeMethod(this._target.Execute(input, (Pipe) null, context), this._arguments.Execute((Array) null, (Pipe) null, context) as object[], (object) AutomationNull.Value);
      if (this._typeConstraint != null)
      {
        foreach (TypeLiteral typeLiteral in this._typeConstraint)
          obj = Parser.ConvertTo(obj, typeLiteral.Type, this.NodeToken);
      }
      return obj;
    }

    internal object InvokeMethod(object target, object[] arguments, object value) => ParserOps.CallMethod(this.NodeToken, target, this.NodeToken.TokenText, arguments, this._isStatic, value);

    public IAssignableValue GetAssignableValue(
      Array input,
      ExecutionContext context)
    {
      return (IAssignableValue) new AssignableMethodCall(this, this._target.Execute(input, (Pipe) null, context), this._arguments.Execute((Array) null, (Pipe) null, context) as object[]);
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      this._target.Accept(visitor);
      this._arguments.Accept(visitor);
    }
  }
}
