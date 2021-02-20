// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.BuildPreOrderNodesListVisitor
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class BuildPreOrderNodesListVisitor : ParseTreeVisitor
  {
    private List<ParseTreeNode> preOrderNodes = new List<ParseTreeNode>();

    internal BuildPreOrderNodesListVisitor()
    {
    }

    internal BuildPreOrderNodesListVisitor(ParseTreeVisitorOptions options)
      : base(options)
    {
    }

    public List<ParseTreeNode> GetPreOrderNodes() => this.preOrderNodes;

    internal override void Visit(ArrayLiteralNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(ArrayReferenceNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(ArrayWrapperNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(AssignmentStatementNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(AttributeNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(CommandNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(ConstantNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(DeferredExpressionNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(DataSectionStatementNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(DoWhileStatementNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(EmptyBracedVariableNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(ExceptionHandlerNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(ExpandableStringNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(ExpressionNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(FlowControlNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(foreachStatementNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(ForWhileStatementNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(FunctionDeclarationNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(HashLiteralNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(ifStatementNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(LiteralStringNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(MethodCallNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(NumericConstantNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(ParameterDeclarationNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(ParameterNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(PipelineNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(PropertyReferenceNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(RedirectionNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(ScriptBlockNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(StatementListNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(SwitchStatementNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(TryStatementNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(TypeNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(UnaryPrefixPostFixNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(VariableDereferenceNode node) => this.preOrderNodes.Add((ParseTreeNode) node);

    internal override void Visit(Token node, int index)
    {
    }
  }
}
