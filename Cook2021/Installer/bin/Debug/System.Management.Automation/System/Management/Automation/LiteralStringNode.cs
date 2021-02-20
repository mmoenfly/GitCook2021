// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.LiteralStringNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class LiteralStringNode : ParseTreeNode
  {
    private readonly string _string;

    public LiteralStringNode(Token token)
    {
      this.NodeToken = token;
      this._string = token.TokenText;
      this.IsExpression = true;
      this.IsConstant = true;
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      return (object) this._string;
    }

    internal override object GetConstValue() => (object) this._string;

    internal override void Accept(ParseTreeVisitor visitor) => visitor.Visit(this);
  }
}
