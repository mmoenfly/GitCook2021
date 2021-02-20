// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ExpandableStringNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class ExpandableStringNode : ParseTreeNode
  {
    private readonly ParseTreeNode _formatExpression;

    public ExpandableStringNode(Token token, Parser parser)
    {
      this.NodeToken = token;
      this._formatExpression = StringTokenReader.ExpandStringToFormatExpression((IStringTokenReaderHelper2) new StringTokenReaderParseTimeHelper(parser, this.NodeToken.StartLineNumber), this.NodeToken, token.TokenText);
    }

    internal override bool SkipDebuggerStep => true;

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      return this._formatExpression.Execute(context);
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      this._formatExpression.Accept(visitor);
    }
  }
}
