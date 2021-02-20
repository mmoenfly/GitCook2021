// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ConstantNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class ConstantNode : ParseTreeNode
  {
    private readonly object _value;

    public ConstantNode(Token token, object value)
    {
      this.NodeToken = token;
      this._value = value;
      this.IsExpression = true;
      this.IsConstant = true;
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      return this._value;
    }

    internal override object GetConstValue() => this._value;

    internal override void Accept(ParseTreeVisitor visitor) => visitor.Visit(this);
  }
}
