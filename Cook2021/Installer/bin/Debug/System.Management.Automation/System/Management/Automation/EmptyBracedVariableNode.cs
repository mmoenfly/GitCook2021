// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.EmptyBracedVariableNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class EmptyBracedVariableNode : ParseTreeNode
  {
    internal EmptyBracedVariableNode(Token token) => this.NodeToken = token;

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      if (context.IsStrictVersion(2))
        throw InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), this.NodeToken, "EmptyBracedVariableName");
      return (object) "$";
    }

    internal override void Accept(ParseTreeVisitor visitor) => visitor.Visit(this);
  }
}
