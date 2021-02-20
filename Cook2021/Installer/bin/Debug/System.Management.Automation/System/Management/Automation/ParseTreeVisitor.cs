// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParseTreeVisitor
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal abstract class ParseTreeVisitor
  {
    private ParseTreeVisitorOptions options;

    internal ParseTreeVisitor()
    {
    }

    internal ParseTreeVisitor(ParseTreeVisitorOptions options) => this.options = options;

    internal bool SkipInvokableScriptBlocks => (this.options & ParseTreeVisitorOptions.SkipInvokableScriptBlocks) != ParseTreeVisitorOptions.None;

    internal abstract void Visit(ArrayLiteralNode node);

    internal abstract void Visit(ArrayReferenceNode node);

    internal abstract void Visit(ArrayWrapperNode node);

    internal abstract void Visit(AssignmentStatementNode node);

    internal abstract void Visit(AttributeNode node);

    internal abstract void Visit(CommandNode node);

    internal abstract void Visit(ConstantNode node);

    internal abstract void Visit(DeferredExpressionNode node);

    internal abstract void Visit(DataSectionStatementNode node);

    internal abstract void Visit(DoWhileStatementNode node);

    internal abstract void Visit(EmptyBracedVariableNode node);

    internal abstract void Visit(ExceptionHandlerNode node);

    internal abstract void Visit(ExpandableStringNode node);

    internal abstract void Visit(ExpressionNode node);

    internal abstract void Visit(FlowControlNode node);

    internal abstract void Visit(foreachStatementNode node);

    internal abstract void Visit(ForWhileStatementNode node);

    internal abstract void Visit(FunctionDeclarationNode node);

    internal abstract void Visit(HashLiteralNode node);

    internal abstract void Visit(ifStatementNode node);

    internal abstract void Visit(LiteralStringNode node);

    internal abstract void Visit(MethodCallNode node);

    internal abstract void Visit(NumericConstantNode node);

    internal abstract void Visit(ParameterDeclarationNode node);

    internal abstract void Visit(ParameterNode node);

    internal abstract void Visit(PipelineNode node);

    internal abstract void Visit(PropertyReferenceNode node);

    internal abstract void Visit(RedirectionNode node);

    internal abstract void Visit(ScriptBlockNode node);

    internal abstract void Visit(StatementListNode node);

    internal abstract void Visit(SwitchStatementNode node);

    internal abstract void Visit(TryStatementNode node);

    internal abstract void Visit(TypeNode node);

    internal abstract void Visit(UnaryPrefixPostFixNode node);

    internal abstract void Visit(VariableDereferenceNode node);

    internal abstract void Visit(Token node, int index);
  }
}
