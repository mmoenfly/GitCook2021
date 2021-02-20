// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.FunctionDeclarationNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class FunctionDeclarationNode : ParseTreeNode
  {
    private readonly ScriptBlockNode _body;

    public FunctionDeclarationNode(Token name, ScriptBlockNode scriptBlockNode)
    {
      this.NodeToken = name;
      this._body = scriptBlockNode;
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      ScriptBlock function = this._body.BuildNewScriptBlock(this);
      function.SessionStateInternal = context.EngineSessionState;
      context.EngineSessionState.SetFunctionRaw(this.NodeToken.TokenText, function, context.EngineSessionState.currentScope.ScopeOrigin);
      return (object) AutomationNull.Value;
    }

    internal ScriptBlockNode Body => this._body;

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      if (this._body == null || visitor.SkipInvokableScriptBlocks)
        return;
      this._body.Accept(visitor);
    }
  }
}
