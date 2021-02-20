// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RestrictedLanguageModeChecker
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class RestrictedLanguageModeChecker : ParseTreeVisitor
  {
    private Parser _parser;
    private IEnumerable<string> _allowedCommands;
    private bool _moduleManifest;

    private static void Check(
      Parser parser,
      ParseTreeNode ptn,
      IEnumerable<string> allowedCommands,
      bool moduleManifest)
    {
      RestrictedLanguageModeChecker languageModeChecker = new RestrictedLanguageModeChecker(parser, allowedCommands, moduleManifest);
      ptn.Accept((ParseTreeVisitor) languageModeChecker);
    }

    public static void Check(Parser parser, ParseTreeNode ptn, IEnumerable<string> allowedCommands) => RestrictedLanguageModeChecker.Check(parser, ptn, allowedCommands, false);

    public static void Check(
      Parser parser,
      ScriptBlock scriptBlock,
      IEnumerable<string> allowedCommands,
      bool moduleManifset)
    {
      if (scriptBlock == null)
        return;
      if (scriptBlock.Begin != null || scriptBlock.Process != null || scriptBlock.ParameterDeclaration != null)
        parser.ReportException((object) null, typeof (ParseException), scriptBlock.Token, "InvalidScriptBlockInDataSection");
      if (scriptBlock.End == null)
        return;
      RestrictedLanguageModeChecker.Check(parser, scriptBlock.End, allowedCommands, moduleManifset);
    }

    private RestrictedLanguageModeChecker(
      Parser parser,
      IEnumerable<string> allowedCommands,
      bool moduleManifest)
    {
      this._parser = parser;
      this._allowedCommands = allowedCommands;
      this._moduleManifest = moduleManifest;
    }

    private void ReportError(ParseTreeNode node, string error) => this.ReportError(node.NodeToken, error);

    private void ReportError(Token node, string error) => this._parser.ReportException((object) node, typeof (ParseException), node, error);

    internal override void Visit(ArrayLiteralNode node)
    {
    }

    internal override void Visit(ArrayReferenceNode node) => this.ReportError((ParseTreeNode) node, "ArrayReferenceNotSupportedInDataSection");

    internal override void Visit(ArrayWrapperNode node)
    {
    }

    internal override void Visit(AssignmentStatementNode node) => this.ReportError((ParseTreeNode) node, "AssignmentStatementNotSupportedInDataSection");

    internal override void Visit(AttributeNode node) => this.ReportError((ParseTreeNode) node, "ParameterDeclarationNotSupportedInDataSection");

    internal override void Visit(CommandNode node)
    {
      if (this._allowedCommands != null && node.Expression == null && node.Elements[0] is Token element)
      {
        foreach (string allowedCommand in this._allowedCommands)
        {
          if (allowedCommand.Equals(element.TokenText, StringComparison.OrdinalIgnoreCase))
            return;
        }
        throw InterpreterError.NewInterpreterException((object) element, typeof (RuntimeException), element, "CmdletNotInAllowedListForDataSection", (object) element.TokenText);
      }
    }

    internal override void Visit(ConstantNode node)
    {
    }

    internal override void Visit(DeferredExpressionNode node)
    {
    }

    internal override void Visit(DataSectionStatementNode node) => this.ReportError((ParseTreeNode) node, "DataSectionStatementNotSupportedInDataSection");

    internal override void Visit(DoWhileStatementNode node) => this.ReportError((ParseTreeNode) node, "DoWhileStatementNotSupportedInDataSection");

    internal override void Visit(EmptyBracedVariableNode node)
    {
    }

    internal override void Visit(ExceptionHandlerNode node) => this.ReportError((ParseTreeNode) node, "TrapStatementNotSupportedInDataSection");

    internal override void Visit(ExpandableStringNode node) => this.ReportError((ParseTreeNode) node, "ExpandableStringNotSupportedInDataSection");

    internal override void Visit(ExpressionNode node) => node.RestrictedLanguageCheck(this._parser);

    internal override void Visit(FlowControlNode node) => this.ReportError((ParseTreeNode) node, "FlowControlStatementNotSupportedInDataSection");

    internal override void Visit(foreachStatementNode node) => this.ReportError((ParseTreeNode) node, "ForeachStatementNotSupportedInDataSection");

    internal override void Visit(ForWhileStatementNode node) => this.ReportError((ParseTreeNode) node, "ForWhileStatementNotSupportedInDataSection");

    internal override void Visit(FunctionDeclarationNode node) => this.ReportError((ParseTreeNode) node, "FunctionDeclarationNotSupportedInDataSection");

    internal override void Visit(HashLiteralNode node)
    {
    }

    internal override void Visit(ifStatementNode node)
    {
    }

    internal override void Visit(LiteralStringNode node)
    {
    }

    internal override void Visit(MethodCallNode node) => this.ReportError((ParseTreeNode) node, "MethodCallNotSupportedInDataSection");

    internal override void Visit(NumericConstantNode node)
    {
    }

    internal override void Visit(ParameterDeclarationNode node) => this.ReportError((ParseTreeNode) node, "ParameterDeclarationNotSupportedInDataSection");

    internal override void Visit(ParameterNode node) => this.ReportError((ParseTreeNode) node, "ParameterDeclarationNotSupportedInDataSection");

    internal override void Visit(PipelineNode node)
    {
    }

    internal override void Visit(PropertyReferenceNode node) => this.ReportError((ParseTreeNode) node, "PropertyReferenceNotSupportedInDataSection");

    internal override void Visit(RedirectionNode node) => this.ReportError((ParseTreeNode) node, "RedirectionNotSupportedInDataSection");

    internal override void Visit(ScriptBlockNode node) => this.ReportError((ParseTreeNode) node, "ScriptBlockNotSupportedInDataSection");

    internal override void Visit(StatementListNode node)
    {
    }

    internal override void Visit(SwitchStatementNode node) => this.ReportError((ParseTreeNode) node, "SwitchStatementNotSupportedInDataSection");

    internal override void Visit(TryStatementNode node) => this.ReportError((ParseTreeNode) node, "TryStatementNotSupportedInDataSection");

    internal override void Visit(TypeNode node)
    {
    }

    internal override void Visit(UnaryPrefixPostFixNode node) => node.RestrictedLanguageCheck(this._parser);

    internal override void Visit(VariableDereferenceNode node)
    {
      if (node.VariableName.Equals("PSCulture", StringComparison.OrdinalIgnoreCase) || node.VariableName.Equals("PSUICulture", StringComparison.OrdinalIgnoreCase) || (node.VariableName.Equals("true", StringComparison.OrdinalIgnoreCase) || node.VariableName.Equals("false", StringComparison.OrdinalIgnoreCase)) || node.VariableName.Equals("null", StringComparison.OrdinalIgnoreCase) || this._moduleManifest && (node.VariableName.StartsWith("env:", StringComparison.OrdinalIgnoreCase) || node.VariableName.Equals("PSScriptRoot", StringComparison.OrdinalIgnoreCase)))
        return;
      this.ReportError((ParseTreeNode) node, "VariableReferenceNotSupportedInDataSection");
    }

    internal override void Visit(Token node, int index)
    {
      if (node.Is(TokenId.SplattedVariableToken))
        this.ReportError(node, "VariableReferenceNotSupportedInDataSection");
      if (index != 0 || !node.Is(TokenId.DotToken))
        return;
      this.ReportError(node, "DotSourcingNotSupportedInDataSection");
    }
  }
}
