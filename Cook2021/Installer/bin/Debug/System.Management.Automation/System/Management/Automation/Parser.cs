// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Parser
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Management.Automation
{
  internal sealed class Parser
  {
    private Tokenizer _tokenizer;
    private bool _inDataSection;
    private bool _interactiveInput;
    private string _previousFirstToken = "";
    private string _previousLastToken = "";
    private uint _recursionDepth;
    private uint _maxRecursionDepth;
    private bool _accumulateErrors;
    private List<RuntimeException> _errors = new List<RuntimeException>();
    [TraceSource("Parser", "Parser")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (Parser), nameof (Parser));
    private readonly ParseTreeNode EmptyPipelineNode;
    private int _catchBlockDepth;

    public Parser()
    {
      this.EmptyPipelineNode = (ParseTreeNode) new PipelineNode((Token) null);
      this._tokenizer = new Tokenizer(this);
    }

    public Parser(string path)
      : this()
      => this._tokenizer.File = path;

    internal Tokenizer Tokenizer => this._tokenizer;

    private void InitParse(string input, int offset)
    {
      this.Tokenizer.Reset(input, offset);
      this._errors.Clear();
      this._recursionDepth = 0U;
      this._maxRecursionDepth = (uint) ((double) PsUtils.GetStackSize() * 0.00055);
      if (this._maxRecursionDepth > 300U)
        this._maxRecursionDepth -= 200U;
      else
        this._maxRecursionDepth = 50U;
    }

    internal bool InDataSection => this._inDataSection;

    internal bool InteractiveInput => this._interactiveInput;

    internal string PreviousFirstToken => this._previousFirstToken;

    internal string PreviousLastToken => this._previousLastToken;

    internal void SetPreviousFirstLastToken(ExecutionContext context)
    {
      if (this._tokenizer.FirstToken == null)
        return;
      context.SetVariable("global:^", (object) this._previousFirstToken);
      if (!this._tokenizer.FirstToken.IsDollarDollarOrDollarCaret())
        this._previousFirstToken = this._tokenizer.FirstToken.TokenText;
      context.SetVariable("global:$", (object) this._previousLastToken);
      if (this._tokenizer.LastToken.IsDollarDollarOrDollarCaret())
        return;
      this._previousLastToken = this._tokenizer.LastToken.TokenText;
    }

    public ParseTreeNode Parse(string input, int cursorPosition)
    {
      this.InitParse(input, cursorPosition);
      StatementListNode statementListNode = this.StatementListRule(this._tokenizer.PositionToken());
      Token errToken = this._tokenizer.Next();
      if (errToken != null)
        this.ReportException((object) errToken.TokenText, typeof (ParseException), errToken, "UnexpectedToken", (object) errToken.TokenText);
      return (ParseTreeNode) statementListNode;
    }

    private StatementListNode StatementListRule(Token start)
    {
      try
      {
        this.IncrementRecursionDepth();
        List<ParseTreeNode> parseTreeNodeList = new List<ParseTreeNode>();
        List<ExceptionHandlerNode> exceptionHandlerNodeList = (List<ExceptionHandlerNode>) null;
        Token token;
        do
        {
          ParseTreeNode parseTreeNode = this.StatementRule();
          if (parseTreeNode != this.EmptyPipelineNode)
          {
            if (parseTreeNode is ExceptionHandlerNode exceptionHandlerNode)
            {
              if (exceptionHandlerNodeList == null)
                exceptionHandlerNodeList = new List<ExceptionHandlerNode>();
              exceptionHandlerNodeList.Add(exceptionHandlerNode);
            }
            else
              parseTreeNodeList.Add(parseTreeNode);
          }
          this._tokenizer.SkipNewlinesOrSemicolons();
          token = this._tokenizer.Peek();
        }
        while (token != null && !token.Is(TokenId.CloseParenToken) && !token.Is(TokenId.CloseBraceToken));
        Parser.tracer.WriteLine("breaking out of statement list parse", new object[0]);
        return new StatementListNode(start, this._tokenizer.PositionToken(), parseTreeNodeList.ToArray(), exceptionHandlerNodeList == null ? (ExceptionHandlerNode[]) null : exceptionHandlerNodeList.ToArray());
      }
      finally
      {
        this.DecrementRecursionDepth();
      }
    }

    private StatementListNode statementBlockRule()
    {
      if (this._tokenizer.Expect(TokenId.OpenBraceToken) == null)
        return (StatementListNode) null;
      StatementListNode statementListNode = this.StatementListRule(this._tokenizer.PositionToken());
      this._tokenizer.Require(TokenId.CloseBraceToken, "MissingEndCurlyBrace");
      return statementListNode;
    }

    public ScriptBlockNode ParseScriptBlock(string input, bool interactiveInput)
    {
      this.InitParse(input, 0);
      ScriptBlockNode scriptBlockNode = (ScriptBlockNode) null;
      bool interactiveInput1 = this._interactiveInput;
      try
      {
        this._interactiveInput = interactiveInput;
        scriptBlockNode = this.ScriptBlockRule((string) null, false, false, (ParameterDeclarationNode) null, (List<Token>) null, (List<List<Token>>) null);
        Token errToken = this._tokenizer.Next();
        if (errToken != null)
          this.ReportException((object) errToken.TokenText, typeof (ParseException), errToken, "UnexpectedToken", (object) errToken.TokenText);
      }
      finally
      {
        this._interactiveInput = interactiveInput1;
      }
      return scriptBlockNode;
    }

    private ScriptBlockNode ScriptBlockRule(
      string name,
      bool requireBrace,
      bool isFilter,
      ParameterDeclarationNode parameterDeclaration,
      List<Token> functionComments,
      List<List<Token>> parameterComments)
    {
      Token token1 = (Token) null;
      if (requireBrace && (token1 = this._tokenizer.Expect(TokenId.OpenBraceToken)) == null)
        return (ScriptBlockNode) null;
      Token token2 = this._tokenizer.PositionToken();
      this._tokenizer.SkipNewlinesOrSemicolons();
      Token token3 = token1;
      List<Token> tokenList;
      while (true)
      {
        tokenList = this._tokenizer.GetFollowingCommentBlock(token3);
        if (tokenList.Count != 0 && !HelpCommentsParser.IsCommentHelpText(tokenList))
          token3 = tokenList[tokenList.Count - 1];
        else
          break;
      }
      if (!requireBrace && this._tokenizer.FirstToken != null && (this._tokenizer.FirstToken.Is(TokenId.FunctionDeclarationToken) && tokenList.Count > 0) && tokenList[tokenList.Count - 1].LineNumber + 2 >= this._tokenizer.FirstToken.LineNumber)
        tokenList = (List<Token>) null;
      List<AttributeNode> attributes = this.AttributesRule();
      List<List<Token>> parameterComments1;
      ParameterDeclarationNode parameterDeclarationNode = this.ParameterDeclarationRule(true, token2, out parameterComments1);
      if (parameterDeclarationNode != null && parameterDeclaration != null && !parameterDeclaration.IsEmpty)
        this.ReportException((object) null, typeof (ParseException), this._tokenizer.PositionToken(), "OnlyOneParameterListAllowed");
      else if (parameterDeclarationNode != null)
      {
        parameterDeclaration = parameterDeclarationNode;
        parameterComments = parameterComments1;
      }
      if (parameterDeclarationNode == null)
      {
        foreach (AttributeNode attributeNode in attributes)
          this.ReportException((object) null, typeof (IncompleteParseException), attributeNode.NodeToken, "UnexpectedAttribute", (object) attributeNode.Name);
      }
      ParseTreeNode begin = (ParseTreeNode) null;
      ParseTreeNode process = (ParseTreeNode) null;
      ParseTreeNode end = (ParseTreeNode) null;
      ParseTreeNode dynamicParams = (ParseTreeNode) null;
      StatementListNode body = (StatementListNode) null;
      while (true)
      {
        this._tokenizer.SkipNewlinesOrSemicolons();
        Token errToken = this._tokenizer.Peek();
        if (errToken != null && (!requireBrace || !errToken.Is(TokenId.CloseBraceToken)) && errToken.IsCmdletKeyword())
        {
          this._tokenizer.Next();
          this._tokenizer.SkipNewlines();
          ParseTreeNode parseTreeNode = (ParseTreeNode) this.statementBlockRule();
          if (parseTreeNode == null)
            this.ReportException((object) null, typeof (IncompleteParseException), errToken, "MissingStatementBlock", (object) errToken.TokenText);
          if (errToken.Is(TokenId.BeginToken) && begin == null)
            begin = parseTreeNode;
          else if (errToken.Is(TokenId.ProcessToken) && process == null)
            process = parseTreeNode;
          else if (errToken.Is(TokenId.EndToken) && end == null)
            end = parseTreeNode;
          else if (errToken.Is(TokenId.DynamicParamToken) && dynamicParams == null)
            dynamicParams = parseTreeNode;
          else
            this.ReportException((object) null, typeof (ParseException), errToken, "DuplicateScriptCommandClause", (object) errToken.TokenText);
        }
        else
          break;
      }
      if (begin == null && process == null && (end == null && dynamicParams == null))
      {
        if (parameterDeclaration != null)
          token2 = this._tokenizer.PositionToken();
        body = this.StatementListRule(token2);
      }
      List<Token> precedingComments = this._tokenizer.GetPrecedingComments((Token) null);
      if (requireBrace)
        this._tokenizer.Require(TokenId.CloseBraceToken, "MissingEndCurlyBrace");
      ScriptBlockNode scriptBlockNode = (ScriptBlockNode) null;
      if (!this.AccumulateErrors || this.Errors.Count == 0)
      {
        List<Token> helpComments = (List<Token>) null;
        if (HelpCommentsParser.IsCommentHelpText(functionComments))
          helpComments = functionComments;
        else if (HelpCommentsParser.IsCommentHelpText(tokenList) && (parameterDeclaration != null || !Parser.CommentsUsedInBody(tokenList, body)))
          helpComments = tokenList;
        else if (HelpCommentsParser.IsCommentHelpText(precedingComments))
          helpComments = precedingComments;
        if (helpComments == null)
          parameterComments = (List<List<Token>>) null;
        scriptBlockNode = body == null ? new ScriptBlockNode(parameterDeclaration, begin, process, end, dynamicParams, attributes, helpComments, parameterComments) : new ScriptBlockNode(parameterDeclaration, (ParseTreeNode) body, isFilter, attributes, helpComments, parameterComments);
      }
      return scriptBlockNode;
    }

    private static bool CommentsUsedInBody(List<Token> comments, StatementListNode body)
    {
      if (body != null && body.Statements.Length > 0)
      {
        ParseTreeNode statement = body.Statements[0];
        if (statement is FunctionDeclarationNode)
        {
          List<Token> helpComments = ((FunctionDeclarationNode) statement).Body._helpComments;
          if (helpComments != null && Parser.CommentBlocksMatch(helpComments, comments))
            return true;
        }
      }
      return false;
    }

    private static bool CommentBlocksMatch(List<Token> first, List<Token> second)
    {
      if (first.Count != second.Count)
        return false;
      for (int index = 0; index < first.Count; ++index)
      {
        if (first[index] != second[index])
          return false;
      }
      return true;
    }

    internal static object ConvertTo(object obj, Type type, Token token)
    {
      try
      {
        return LanguagePrimitives.ConvertTo(obj, type, (IFormatProvider) CultureInfo.InvariantCulture);
      }
      catch (PSInvalidCastException ex)
      {
        RuntimeException runtimeException = new RuntimeException(ex.Message, (Exception) ex);
        runtimeException.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, token));
        Parser.tracer.TraceException((Exception) runtimeException);
        throw runtimeException;
      }
    }

    internal static T ConvertTo<T>(object obj, Token token)
    {
      try
      {
        return (T) LanguagePrimitives.ConvertTo(obj, typeof (T), (IFormatProvider) CultureInfo.InvariantCulture);
      }
      catch (PSInvalidCastException ex)
      {
        RuntimeException runtimeException = new RuntimeException(ex.Message, (Exception) ex);
        runtimeException.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, token));
        Parser.tracer.TraceException((Exception) runtimeException);
        throw runtimeException;
      }
    }

    private object GetPropertyOrUnaryExpression()
    {
      Token propertyNameToken = this._tokenizer.GetPropertyNameToken();
      return propertyNameToken != null ? (object) propertyNameToken.TokenText : (object) this.UnaryPrefixPostfixExpressionRule();
    }

    private ParseTreeNode GetCommandArgumentOrCompoundValue()
    {
      Token commandArgumentToken = this._tokenizer.GetCommandArgumentToken();
      if (commandArgumentToken == null)
        return this.CompoundValueRule();
      return commandArgumentToken.Is(TokenId.LiteralCommandArgumentToken) || commandArgumentToken.Is(TokenId.ParameterToken) ? (ParseTreeNode) new LiteralStringNode(commandArgumentToken) : (ParseTreeNode) new ExpandableStringNode(commandArgumentToken, this);
    }

    private void IncrementRecursionDepth()
    {
      if (++this._recursionDepth > this._maxRecursionDepth)
        throw InterpreterError.NewInterpreterException((object) null, typeof (ParseException), this._tokenizer.PositionToken(), "ScriptTooComplicated");
    }

    private void DecrementRecursionDepth() => --this._recursionDepth;

    internal bool AccumulateErrors
    {
      get => this._accumulateErrors;
      set => this._accumulateErrors = value;
    }

    internal List<RuntimeException> Errors => this._errors;

    internal void ReportException(
      object targetObject,
      Type exceptionType,
      Token errToken,
      string resourceIdAndErrorId,
      params object[] args)
    {
      if (errToken == null)
        errToken = this._tokenizer.PositionToken();
      RuntimeException runtimeException = InterpreterError.NewInterpreterException(targetObject, exceptionType, errToken, resourceIdAndErrorId, args);
      if (!this._accumulateErrors)
        throw runtimeException;
      runtimeException.ErrorToken = errToken;
      this._errors.Add(runtimeException);
    }

    internal void ReportExceptionWithInnerException(
      object targetObject,
      Type exceptionType,
      Token errToken,
      string resourceIdAndErrorId,
      Exception innerException,
      params object[] args)
    {
      if (errToken == null)
        errToken = this._tokenizer.PositionToken();
      RuntimeException runtimeException = InterpreterError.NewInterpreterExceptionWithInnerException(targetObject, exceptionType, errToken, resourceIdAndErrorId, innerException, args);
      if (!this._accumulateErrors)
        throw runtimeException;
      runtimeException.ErrorToken = errToken;
      this._errors.Add(runtimeException);
    }

    internal void ReportExceptionByMessage(
      Type exceptionType,
      Token errToken,
      string message,
      string errorId,
      Exception innerException)
    {
      if (errToken == null)
        errToken = this._tokenizer.PositionToken();
      RuntimeException runtimeException = InterpreterError.NewInterpreterExceptionByMessage(exceptionType, errToken, message, errorId, innerException);
      if (!this._accumulateErrors)
        throw runtimeException;
      runtimeException.ErrorToken = errToken;
      this._errors.Add(runtimeException);
    }

    private ParseTreeNode StatementRule()
    {
      ParseMode mode = this._tokenizer.Mode;
      this._tokenizer.Mode = ParseMode.General;
      try
      {
        this._tokenizer.SkipNewlines();
        ParseTreeNode parseTreeNode1 = this.IfStatementRule();
        if (parseTreeNode1 != null)
          return parseTreeNode1;
        ParseTreeNode parseTreeNode2 = this.SwitchStatementRule();
        if (parseTreeNode2 != null)
          return parseTreeNode2;
        ParseTreeNode parseTreeNode3 = this.ForeachStatementRule();
        if (parseTreeNode3 != null)
          return parseTreeNode3;
        ParseTreeNode parseTreeNode4 = this.FromStatementRule();
        if (parseTreeNode4 != null)
          return parseTreeNode4;
        ParseTreeNode parseTreeNode5 = this.ForWhileStatementRule();
        if (parseTreeNode5 != null)
          return parseTreeNode5;
        ParseTreeNode parseTreeNode6 = this.DoWhileStatementRule();
        if (parseTreeNode6 != null)
          return parseTreeNode6;
        ParseTreeNode parseTreeNode7 = this.FunctionDeclarationRule();
        if (parseTreeNode7 != null)
          return parseTreeNode7;
        ParseTreeNode parseTreeNode8 = this.FlowControlStatementRule();
        if (parseTreeNode8 != null)
          return parseTreeNode8;
        ParseTreeNode parseTreeNode9 = this.TrapStatementRule();
        if (parseTreeNode9 != null)
          return parseTreeNode9;
        ParseTreeNode parseTreeNode10 = this.TryStatementRule();
        if (parseTreeNode10 != null)
          return parseTreeNode10;
        ParseTreeNode parseTreeNode11 = this.DataSectionStatementRule();
        if (parseTreeNode11 != null)
          return parseTreeNode11;
        Token errToken = this._tokenizer.Peek();
        if (errToken != null && errToken.Is(TokenId.ReservedKeywordToken))
          this.ReportException((object) null, typeof (ParseException), errToken, "ReservedKeywordNotAllowed", (object) errToken);
        return this.PipelineRule();
      }
      finally
      {
        this._tokenizer.Mode = mode;
      }
    }

    private ParseTreeNode PipelineRule()
    {
      try
      {
        this.IncrementRecursionDepth();
        PipelineNode pipelineNode = new PipelineNode(this._tokenizer.PositionToken());
        while (true)
        {
          Token errToken1;
          do
          {
            ParseTreeNode parseTreeNode = this.ExpressionRule();
            CommandNode pElement;
            if (parseTreeNode != null)
            {
              if (pipelineNode.Commands.Count != 0)
                this.ReportException((object) null, typeof (ParseException), this._tokenizer.PositionToken(), "ExpressionsMustBeFirstInPipeline");
              Token token = this._tokenizer.Peek();
              if (token == null)
                return parseTreeNode;
              if (token.Is(TokenId.AssignmentOperatorToken))
                return this.AssignmentRule(parseTreeNode);
              pElement = new CommandNode(parseTreeNode, token);
              while (true)
              {
                RedirectionNode redirection = this.RedirectionRule();
                if (redirection != null)
                  pElement.AddRedirection(this, redirection);
                else
                  break;
              }
            }
            else
              pElement = this.commandRule();
            if (pElement != null)
              pipelineNode.Add(pElement);
            Token errToken2 = this._tokenizer.Peek();
            if (errToken2 != null && !errToken2.IsEndOfStatement() && (!errToken2.Is(TokenId.CloseParenToken) && !errToken2.Is(TokenId.CloseBraceToken)))
            {
              if (!errToken2.Is(TokenId.PipeToken))
              {
                if (this._accumulateErrors)
                {
                  bool flag = false;
                  while (true)
                  {
                    Token token = this._tokenizer.Peek();
                    if (token != null && !token.Is(TokenId.PipeToken) && (!token.IsEndOfStatement() && !token.Is(TokenId.CloseParenToken)) && !token.Is(TokenId.CloseBraceToken))
                    {
                      if (!token.Is(TokenId.PipeToken))
                      {
                        Token errToken3 = this._tokenizer.Next();
                        this.ReportException((object) errToken3.TokenText, typeof (ParseException), errToken3, "UnexpectedToken", (object) errToken3);
                      }
                      else
                        goto label_22;
                    }
                    else
                      break;
                  }
                  flag = true;
label_22:
                  if (flag)
                    goto label_26;
                }
                else
                  throw InterpreterError.NewInterpreterException((object) errToken2.TokenText, typeof (ParseException), errToken2, "UnexpectedToken", (object) errToken2);
              }
              errToken1 = this._tokenizer.Next();
              this._tokenizer.SkipNewlines();
            }
            else
              goto label_26;
          }
          while (!this._tokenizer.EndOfInput());
          this.ReportException((object) null, typeof (IncompleteParseException), errToken1, "EmptyPipeElement");
        }
label_26:
        if (pipelineNode.Commands.Count == 0)
          return this.EmptyPipelineNode;
        return pipelineNode.Commands.Count == 1 && pipelineNode.Commands[0].IsPureExpression ? pipelineNode.Commands[0].Expression : (ParseTreeNode) pipelineNode;
      }
      finally
      {
        this.DecrementRecursionDepth();
      }
    }

    private CommandNode commandRule()
    {
      bool flag1 = true;
      bool flag2 = false;
      CommandNode commandNode = new CommandNode();
      ParseMode mode = this._tokenizer.Mode;
      this._tokenizer.Mode = ParseMode.Command;
      try
      {
        while (true)
        {
          Token token1;
          do
          {
            Token errToken = this._tokenizer.Peek();
            if (errToken == null || errToken.IsEndOfStatement() || (errToken.Is(TokenId.CloseParenToken) || errToken.Is(TokenId.CloseBraceToken)))
            {
              Parser.tracer.WriteLine("Token was '{0}', ungot it, ending pipeline parse", (object) errToken);
              goto label_43;
            }
            else if (errToken.Is(TokenId.PipeToken))
            {
              Parser.tracer.WriteLine("Token was '|', completing pipeline element.", new object[0]);
              if (commandNode.Elements.Count == 0)
              {
                this.ReportException((object) null, typeof (ParseException), errToken, "EmptyPipeElement");
                goto label_43;
              }
              else
                goto label_43;
            }
            else
            {
              Token token2 = this._tokenizer.Next();
              if (flag1)
              {
                Parser.tracer.WriteLine("Start of pipeline element, cmdlet is {0}", (object) token2);
                commandNode.Elements.Add((object) token2);
                if (token2.Is("."))
                {
                  token2.TokenId = TokenId.DotToken;
                  flag2 = true;
                }
                else if (token2.Is(TokenId.LiteralCommandArgumentToken) || token2.Is(TokenId.ExpandableCommandArgumentToken) || token2.Is(TokenId.ParameterToken))
                  token2.TokenId = TokenId.CmdletNameToken;
                else if (token2.Is(TokenId.AmpersandToken))
                  flag2 = true;
                flag1 = false;
              }
              else
              {
                if (flag2)
                {
                  Parser.tracer.WriteLine("adding actual command {0} to pipeline", (object) token2);
                  flag2 = false;
                  if (token2.Is(TokenId.LiteralCommandArgumentToken))
                  {
                    commandNode.Elements.Add((object) new LiteralStringNode(token2));
                    token2.TokenId = TokenId.CmdletNameToken;
                    continue;
                  }
                  if (token2.Is(TokenId.ExpandableCommandArgumentToken))
                  {
                    commandNode.Elements.Add((object) new ExpandableStringNode(token2, this));
                    token2.TokenId = TokenId.CmdletNameToken;
                    continue;
                  }
                  if (token2.Is(TokenId.ParameterToken))
                    token2.TokenId = TokenId.CmdletNameToken;
                }
                if (token2.Is(TokenId.AmpersandToken))
                  this.ReportException((object) null, typeof (ParseException), token2, "AmpersandNotAllowed");
                else if (token2.Is(TokenId.ParameterToken) || token2.Is(TokenId.NumberToken) || token2.Is(TokenId.MinusMinusToken))
                {
                  commandNode.Elements.Add((object) token2);
                  Parser.tracer.WriteLine("Adding token {0} to pipeline.", (object) token2);
                }
                else if (token2.Is(TokenId.CommaToken))
                {
                  commandNode.Elements.Add((object) token2);
                  this._tokenizer.SkipNewlines();
                  Parser.tracer.WriteLine("Adding token {0} to pipeline.", (object) token2);
                }
                else if (token2.Is(TokenId.ExpandableCommandArgumentToken))
                {
                  commandNode.Elements.Add((object) new ExpandableStringNode(token2, this));
                  Parser.tracer.WriteLine("Adding parameter argument {0} to pipeline.", (object) token2);
                }
                else if (token2.Is(TokenId.LiteralCommandArgumentToken))
                {
                  commandNode.Elements.Add((object) new LiteralStringNode(token2));
                  Parser.tracer.WriteLine("Adding parameter argument {0} to pipeline.", (object) token2);
                }
                else if (token2.Is(TokenId.SplattedVariableToken))
                {
                  Token referenceOperatorToken = this._tokenizer.GetReferenceOperatorToken();
                  if (referenceOperatorToken != null && !referenceOperatorToken.PreceedByWhiteSpace())
                  {
                    this.ReportException((object) null, typeof (ParseException), token2, "NoPropertiesInSplatting", (object) token2.ToString());
                  }
                  else
                  {
                    commandNode.Elements.Add((object) token2);
                    Parser.tracer.WriteLine("Adding token {0} to pipeline.", (object) token2);
                  }
                  if (referenceOperatorToken != null)
                    this._tokenizer.UngetToken(referenceOperatorToken);
                }
                else if (token2.Is(TokenId.RedirectionOperatorToken))
                {
                  this._tokenizer.UngetToken(token2);
                  RedirectionNode redirection = this.RedirectionRule();
                  commandNode.AddRedirection(this, redirection);
                }
                else
                {
                  this._tokenizer.UngetToken(token2);
                  ParseTreeNode parseTreeNode = this.CompoundValueRule();
                  if (parseTreeNode != null)
                    commandNode.Elements.Add((object) parseTreeNode);
                  else
                    token1 = this._tokenizer.Next();
                }
              }
            }
          }
          while (token1 == null);
          commandNode.Elements.Add((object) new LiteralStringNode(token1));
        }
label_43:
        return commandNode.IsValid(this) ? commandNode : (CommandNode) null;
      }
      finally
      {
        this._tokenizer.Mode = mode;
      }
    }

    private ParseTreeNode AssignmentRule(ParseTreeNode lvalue)
    {
      Token token = this._tokenizer.Next();
      if (token == null || !token.Is(TokenId.AssignmentOperatorToken))
        return (ParseTreeNode) null;
      if (!(lvalue is IAssignableParseTreeNode left))
        this.ReportException((object) null, typeof (ParseException), token, "InvalidLeftHandSide");
      if (lvalue is ArrayLiteralNode && !token.Is("="))
        this.ReportException((object) null, typeof (ParseException), token, "EqualsNotSupported");
      this._tokenizer.SkipNewlines();
      ParseTreeNode right = this.StatementRule();
      if (right == this.EmptyPipelineNode)
        this.ReportException((object) null, typeof (IncompleteParseException), token, "ExpectedValueExpression", (object) token.TokenText);
      return (ParseTreeNode) new AssignmentStatementNode(token, left, right);
    }

    private RedirectionNode RedirectionRule()
    {
      Token token1 = this._tokenizer.Peek();
      if (token1 == null || !token1.Is(TokenId.RedirectionOperatorToken))
        return (RedirectionNode) null;
      Token token2 = this._tokenizer.Next();
      if (token2.Is("1>&2") || token2.Is("<"))
        this.ReportException((object) token2, typeof (ParseException), token2, "RedirectionNotSupported", (object) token2.TokenText);
      RedirectionNode redirectionNode;
      if (RedirectionNode.IsMerging(token2))
      {
        redirectionNode = new RedirectionNode(token2);
      }
      else
      {
        ParseTreeNode argumentOrCompoundValue = this.GetCommandArgumentOrCompoundValue();
        if (argumentOrCompoundValue == null)
          this.ReportException((object) null, typeof (ParseException), token2, "MissingFileSpecification");
        redirectionNode = new RedirectionNode(token2, argumentOrCompoundValue);
      }
      return redirectionNode;
    }

    private ParseTreeNode IfStatementRule()
    {
      Token nodeToken = this._tokenizer.Peek();
      if (nodeToken == null || !nodeToken.Is(TokenId.IfToken))
        return (ParseTreeNode) null;
      ArrayList arrayList = new ArrayList();
      Token token1 = this._tokenizer.Next();
      do
      {
        ParseTreeNode parseTreeNode1 = (ParseTreeNode) null;
        if (token1.Is(TokenId.IfToken) || token1.Is(TokenId.ElseIfToken))
        {
          Token token2 = this._tokenizer.Require(TokenId.OpenParenToken, "MissingEndParenthesisInIfStatement", (object) token1.TokenText);
          this._tokenizer.SkipNewlines();
          if (token2 != null || this._tokenizer.Peek() != null && !this._tokenizer.Peek().Is(TokenId.OpenBraceToken))
            parseTreeNode1 = this.PipelineRule();
          if (parseTreeNode1 == null || this.EmptyPipelineNode == parseTreeNode1)
            this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "IfStatementMissingCondition", (object) token1.TokenText);
          this._tokenizer.Require(TokenId.CloseParenToken, "MissingEndParenthesisAfterStatement", (object) token1.TokenText);
        }
        arrayList.Add((object) parseTreeNode1);
        this._tokenizer.SkipNewlines();
        ParseTreeNode parseTreeNode2 = (ParseTreeNode) this.statementBlockRule();
        if (parseTreeNode2 == null)
        {
          if (token1.Is(TokenId.ElseToken))
            this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingStatementBlockAfterElse");
          else
            this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingStatementBlock", (object) token1.TokenText);
        }
        arrayList.Add((object) parseTreeNode2);
        if (!token1.Is(TokenId.ElseToken))
        {
          this._tokenizer.SkipNewlines();
          token1 = this._tokenizer.Next();
        }
        else
          goto label_16;
      }
      while (token1 != null && (token1.Is(TokenId.ElseIfToken) || token1.Is(TokenId.ElseToken)));
      this._tokenizer.UngetToken(token1);
label_16:
      return (ParseTreeNode) new ifStatementNode(nodeToken, (ParseTreeNode[]) arrayList.ToArray(typeof (ParseTreeNode)));
    }

    private ParseTreeNode SwitchStatementRule()
    {
      string label = "";
      Token token1;
      if ((token1 = this._tokenizer.Expect(TokenId.LoopLabelToken)) != null)
      {
        label = token1.TokenText.Substring(1);
        this._tokenizer.SkipNewlines();
      }
      Token token2;
      if ((token2 = this._tokenizer.Expect(TokenId.SwitchToken)) == null)
      {
        this._tokenizer.UngetToken(token1);
        return (ParseTreeNode) null;
      }
      this._tokenizer.SkipNewlines();
      SwitchMode mode = SwitchMode.None;
      ParseTreeNode expression = this.SwitchFlagsRule(out mode);
      if (expression == null && mode != SwitchMode.File)
        this.ReportException((object) token2, typeof (IncompleteParseException), token2, "PipelineValueRequired");
      ParseTreeNode[] array = this.SwitchClausesRule().ToArray(typeof (ParseTreeNode)) as ParseTreeNode[];
      return (ParseTreeNode) new SwitchStatementNode(token2, label, expression, array, mode);
    }

    private ParseTreeNode SwitchFlagsRule(out SwitchMode mode)
    {
      mode = SwitchMode.None;
      ParseTreeNode parseTreeNode1 = (ParseTreeNode) null;
      while (true)
      {
        Token errToken;
        do
        {
          errToken = this._tokenizer.Expect(TokenId.ParameterToken);
          if (errToken != null)
          {
            switch (char.ToLowerInvariant(errToken.TokenText[1]))
            {
              case 'c':
                mode |= SwitchMode.CaseSensitive;
                continue;
              case 'e':
                mode &= ~SwitchMode.Regex;
                mode &= ~SwitchMode.Wildcard;
                continue;
              case 'f':
                mode |= SwitchMode.File;
                this._tokenizer.SkipNewlines();
                parseTreeNode1 = this.GetCommandArgumentOrCompoundValue();
                continue;
              case 'r':
                mode |= SwitchMode.Regex;
                mode &= ~SwitchMode.Wildcard;
                continue;
              case 'w':
                mode |= SwitchMode.Wildcard;
                mode &= ~SwitchMode.Regex;
                continue;
              default:
                goto label_9;
            }
          }
          else
            goto label_10;
        }
        while (parseTreeNode1 != null);
        this.ReportException((object) errToken.TokenText, typeof (IncompleteParseException), errToken, "MissingFilenameOption");
        continue;
label_9:
        this.ReportException((object) errToken.TokenText, typeof (ParseException), errToken, "InvalidSwitchFlag", (object) errToken.TokenText);
      }
label_10:
      Token errToken1 = this._tokenizer.Expect(TokenId.OpenParenToken);
      if (errToken1 == null)
        return parseTreeNode1;
      if (parseTreeNode1 != null)
        this.ReportException((object) parseTreeNode1, typeof (ParseException), errToken1, "PipelineValueRequired");
      this._tokenizer.SkipNewlines();
      ParseTreeNode parseTreeNode2 = this.PipelineRule();
      this._tokenizer.Require(TokenId.CloseParenToken, "MissingEndParenthesisInSwitchStatement");
      return parseTreeNode2;
    }

    private ArrayList SwitchClausesRule()
    {
      ArrayList arrayList = new ArrayList();
      ParseTreeNode parseTreeNode1 = (ParseTreeNode) null;
      this._tokenizer.Require(TokenId.OpenBraceToken, "MissingCurlyBraceInSwitchStatement");
      do
      {
        this._tokenizer.SkipNewlines();
        ParseTreeNode argumentOrCompoundValue = this.GetCommandArgumentOrCompoundValue();
        if (argumentOrCompoundValue == null)
        {
          this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingSwitchConditionExpression");
          break;
        }
        this._tokenizer.SkipNewlines();
        ParseTreeNode parseTreeNode2 = (ParseTreeNode) this.statementBlockRule();
        if (parseTreeNode2 == null)
          this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingSwitchStatementClause");
        if (argumentOrCompoundValue != null && argumentOrCompoundValue.NodeToken != null && (argumentOrCompoundValue.NodeToken.Is(TokenId.LiteralCommandArgumentToken) && argumentOrCompoundValue.NodeToken.Is("default")))
        {
          if (parseTreeNode1 != null)
            this.ReportException((object) null, typeof (ParseException), argumentOrCompoundValue.NodeToken, "MultipleSwitchDefaultClauses");
          parseTreeNode1 = parseTreeNode2;
        }
        else
        {
          arrayList.Add((object) argumentOrCompoundValue);
          arrayList.Add((object) parseTreeNode2);
        }
        this._tokenizer.SkipNewlinesOrSemicolons();
      }
      while (this._tokenizer.Expect(TokenId.CloseBraceToken) == null);
      if (parseTreeNode1 != null)
      {
        arrayList.Add((object) null);
        arrayList.Add((object) parseTreeNode1);
      }
      return arrayList;
    }

    private ParseTreeNode ForeachStatementRule()
    {
      string label = (string) null;
      Token token1;
      if ((token1 = this._tokenizer.Expect(TokenId.LoopLabelToken)) != null)
      {
        label = token1.TokenText.Substring(1);
        this._tokenizer.SkipNewlines();
      }
      Token nodeToken;
      if ((nodeToken = this._tokenizer.Expect(TokenId.ForeachToken)) == null)
      {
        this._tokenizer.UngetToken(token1);
        return (ParseTreeNode) null;
      }
      Token token2 = this._tokenizer.Require(TokenId.OpenParenToken, "MissingOpenParenthesisAfterKeyword", (object) nodeToken);
      this._tokenizer.SkipNewlines();
      Token variable = (Token) null;
      ParseTreeNode expression = (ParseTreeNode) null;
      if (token2 != null || this._tokenizer.Peek() != null && !this._tokenizer.Peek().Is(TokenId.OpenBraceToken))
      {
        variable = this._tokenizer.Require(TokenId.VariableToken, "MissingVariableNameAfterForeach");
        if (variable == null || this._tokenizer.Peek() != null && !this._tokenizer.Peek().Is(TokenId.InToken))
        {
          Token token3 = this._tokenizer.Next();
          if (this._tokenizer.Peek() == null || !this._tokenizer.Peek().Is(TokenId.InToken))
            this._tokenizer.UngetToken(token3);
        }
        this._tokenizer.Require(TokenId.InToken, "MissingInInForeach");
        this._tokenizer.SkipNewlines();
        expression = this.PipelineRule();
        if (expression == null || expression == this.EmptyPipelineNode)
          this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingForeachExpression");
        this._tokenizer.Require(TokenId.CloseParenToken, "MissingEndParenthesisAfterForeach");
      }
      this._tokenizer.SkipNewlines();
      ParseTreeNode body = (ParseTreeNode) this.statementBlockRule();
      if (body == null)
        this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingForeachStatement");
      return (ParseTreeNode) new foreachStatementNode(nodeToken, label, variable, expression, body);
    }

    private ParseTreeNode FromStatementRule()
    {
      Token token1;
      if ((token1 = this._tokenizer.Expect(TokenId.LoopLabelToken)) != null)
        this._tokenizer.SkipNewlines();
      Token errToken;
      if ((errToken = this._tokenizer.Expect(TokenId.FromToken)) == null)
      {
        this._tokenizer.UngetToken(token1);
        return (ParseTreeNode) null;
      }
      Token token2;
      do
      {
        token2 = this._tokenizer.Next();
      }
      while (token2 != null && !token2.IsEndOfStatement());
      this.ReportException((object) null, typeof (ParseException), errToken, "FromKeywordNotAllowed");
      return (ParseTreeNode) null;
    }

    private ParseTreeNode ForWhileStatementRule()
    {
      string label = "";
      Token token1;
      if ((token1 = this._tokenizer.Expect(TokenId.LoopLabelToken)) != null)
      {
        label = token1.TokenText.Substring(1);
        this._tokenizer.SkipNewlines();
      }
      Token token2 = this._tokenizer.Next();
      if (token2 == null || !token2.Is(TokenId.WhileToken) && !token2.Is(TokenId.ForToken))
      {
        this._tokenizer.UngetToken(token2);
        this._tokenizer.UngetToken(token1);
        return (ParseTreeNode) null;
      }
      ParseTreeNode initialExpression = (ParseTreeNode) null;
      ParseTreeNode loopExpression = (ParseTreeNode) null;
      ParseTreeNode incrementExpression = (ParseTreeNode) null;
      if (token2.Is(TokenId.WhileToken))
      {
        Token token3 = this._tokenizer.Require(TokenId.OpenParenToken, "MissingOpenParenthesisAfterKeyword", (object) token2);
        this._tokenizer.SkipNewlines();
        if (token3 != null || this._tokenizer.Peek() != null && !this._tokenizer.Peek().Is(TokenId.OpenBraceToken))
          loopExpression = this.PipelineRule();
        if (loopExpression == null || loopExpression == this.EmptyPipelineNode)
          this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingExpressionAfterKeyword", (object) token2);
        this._tokenizer.Require(TokenId.CloseParenToken, "MissingEndParenthesisAfterStatement", (object) token2.TokenText);
      }
      else if (token2.Is(TokenId.ForToken))
      {
        Token token3 = this._tokenizer.Require(TokenId.OpenParenToken, "MissingOpenParenthesisAfterKeyword", (object) token2);
        this._tokenizer.SkipNewlines();
        if (token3 != null || this._tokenizer.Peek() != null && !this._tokenizer.Peek().Is(TokenId.OpenBraceToken))
        {
          initialExpression = this.PipelineRule();
          if (initialExpression == this.EmptyPipelineNode)
            initialExpression = (ParseTreeNode) null;
          this._tokenizer.Expect(TokenId.SemicolonToken);
          this._tokenizer.SkipNewlines();
          loopExpression = this.PipelineRule();
          if (loopExpression == this.EmptyPipelineNode)
            loopExpression = (ParseTreeNode) null;
          this._tokenizer.Expect(TokenId.SemicolonToken);
          this._tokenizer.SkipNewlines();
          incrementExpression = this.PipelineRule();
          if (incrementExpression == this.EmptyPipelineNode)
            incrementExpression = (ParseTreeNode) null;
          this._tokenizer.Require(TokenId.CloseParenToken, "MissingEndParenthesisAfterStatement", (object) token2.TokenText);
        }
      }
      this._tokenizer.SkipNewlines();
      ParseTreeNode body = (ParseTreeNode) this.statementBlockRule();
      if (body == null)
        this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingLoopStatement", (object) token2.TokenText);
      return (ParseTreeNode) new ForWhileStatementNode(token2, label, initialExpression, loopExpression, incrementExpression, body);
    }

    private ParseTreeNode DoWhileStatementRule()
    {
      string label = "";
      Token token1;
      if ((token1 = this._tokenizer.Expect(TokenId.LoopLabelToken)) != null)
      {
        label = token1.TokenText.Substring(1);
        this._tokenizer.SkipNewlines();
      }
      Token nodeToken;
      if ((nodeToken = this._tokenizer.Expect(TokenId.DoToken)) == null)
      {
        this._tokenizer.UngetToken(token1);
        return (ParseTreeNode) null;
      }
      this._tokenizer.SkipNewlines();
      ParseTreeNode body = (ParseTreeNode) this.statementBlockRule();
      if (body == null)
        this.ReportException((object) nodeToken.TokenText, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingLoopStatement", (object) nodeToken.TokenText);
      this._tokenizer.SkipNewlines();
      Token token2 = this._tokenizer.Next();
      if (token2 == null || !token2.Is(TokenId.WhileToken) && !token2.Is(TokenId.UntilToken))
      {
        this.ReportException((object) token2, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingWhileOrUntilInDoWhile");
        if (token2 != null && token2.Is(TokenId.OpenParenToken))
          this._tokenizer.UngetToken(token2);
      }
      this._tokenizer.Require(TokenId.OpenParenToken, "MissingOpenParenthesisAfterKeyword", (object) token2);
      this._tokenizer.SkipNewlines();
      ParseTreeNode condition = this.PipelineRule();
      if (condition == null || condition == this.EmptyPipelineNode)
        this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingExpressionAfterKeyword", (object) token2);
      this._tokenizer.Require(TokenId.CloseParenToken, "MissingEndParenthesisAfterStatement", (object) nodeToken.TokenText);
      return (ParseTreeNode) new DoWhileStatementNode(nodeToken, label, condition, token2 != null && token2.Is(TokenId.WhileToken), body);
    }

    private ParseTreeNode TrapStatementRule()
    {
      Token token;
      if ((token = this._tokenizer.Expect(TokenId.TrapToken)) == null)
        return (ParseTreeNode) null;
      this._tokenizer.SkipNewlines();
      TypeLiteral exceptionType = (TypeLiteral) null;
      Token typeName;
      if ((typeName = this._tokenizer.Expect(TokenId.TypeToken)) != null)
        exceptionType = new TypeLiteral(typeName);
      this._tokenizer.SkipNewlines();
      ParseTreeNode body = (ParseTreeNode) this.statementBlockRule();
      if (body == null)
        this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingTrapStatement");
      return (ParseTreeNode) new ExceptionHandlerNode(token, exceptionType, body);
    }

    private ParseTreeNode TryStatementRule()
    {
      Token token;
      if ((token = this._tokenizer.Expect(TokenId.TryToken)) == null)
        return (ParseTreeNode) null;
      this._tokenizer.SkipNewlines();
      ParseTreeNode body = (ParseTreeNode) this.statementBlockRule();
      if (body == null)
        this.ReportException((object) null, typeof (IncompleteParseException), token, "MissingTryStatementBlock");
      List<ExceptionHandlerNode> exceptionHandlerNodeList = new List<ExceptionHandlerNode>();
      ExceptionHandlerNode exceptionHandlerNode;
      while ((exceptionHandlerNode = this.CatchBlockRule()) != null)
        exceptionHandlerNodeList.Add(exceptionHandlerNode);
      ParseTreeNode finallyBlock = this.FinallyBlockRule();
      if (exceptionHandlerNodeList.Count == 0 && finallyBlock == null)
        this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingCatchOrFinally");
      TryStatementNode tryStatementNode = new TryStatementNode(token, body, exceptionHandlerNodeList == null ? (ExceptionHandlerNode[]) null : exceptionHandlerNodeList.ToArray(), finallyBlock);
      tryStatementNode.Validate(this);
      return (ParseTreeNode) tryStatementNode;
    }

    private ExceptionHandlerNode CatchBlockRule()
    {
      this._tokenizer.SkipNewlines();
      Token token;
      if ((token = this._tokenizer.Expect(TokenId.CatchToken)) == null)
        return (ExceptionHandlerNode) null;
      ExceptionTypeList exceptionTypeList = (ExceptionTypeList) null;
      Token errToken = (Token) null;
      do
      {
        this._tokenizer.SkipNewlines();
        Token typeName = this._tokenizer.Expect(TokenId.TypeToken);
        if (typeName == null)
        {
          if (errToken != null)
          {
            this.ReportException((object) errToken.TokenText, typeof (IncompleteParseException), errToken, "MissingTypeLiteralToken");
            break;
          }
          break;
        }
        if (exceptionTypeList == null)
          exceptionTypeList = new ExceptionTypeList();
        exceptionTypeList.Add(new TypeLiteral(typeName));
        this._tokenizer.SkipNewlines();
        errToken = this._tokenizer.Expect(TokenId.CommaToken);
      }
      while (errToken != null);
      this._tokenizer.SkipNewlines();
      ParseTreeNode body;
      try
      {
        ++this._catchBlockDepth;
        body = (ParseTreeNode) this.statementBlockRule();
      }
      finally
      {
        --this._catchBlockDepth;
      }
      if (body == null)
        this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingCatchHandlerBlock");
      return new ExceptionHandlerNode(token, exceptionTypeList, body, true);
    }

    private ParseTreeNode FinallyBlockRule()
    {
      this._tokenizer.SkipNewlines();
      Token errToken;
      if ((errToken = this._tokenizer.Expect(TokenId.FinallyToken)) == null)
        return (ParseTreeNode) null;
      this._tokenizer.SkipNewlines();
      ParseTreeNode parseTreeNode = (ParseTreeNode) this.statementBlockRule();
      if (parseTreeNode == null)
        this.ReportException((object) null, typeof (IncompleteParseException), errToken, "MissingFinallyStatementBlock");
      return parseTreeNode;
    }

    private ParseTreeNode DataSectionStatementRule()
    {
      Token token;
      if ((token = this._tokenizer.Expect(TokenId.DataSectionToken)) == null)
        return (ParseTreeNode) null;
      this._tokenizer.SkipNewlines();
      Token propertyNameToken = this._tokenizer.GetPropertyNameToken();
      this._tokenizer.SkipNewlines();
      ParseTreeNode[] commandsAllowed = this.GetCommandsAllowed();
      this._tokenizer.SkipNewlines();
      ParseTreeNode parseTreeNode = (ParseTreeNode) null;
      this._inDataSection = true;
      try
      {
        parseTreeNode = (ParseTreeNode) this.statementBlockRule();
      }
      finally
      {
        this._inDataSection = false;
      }
      if (parseTreeNode == null)
        this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingStatementBlockForDataSection");
      else
        RestrictedLanguageModeChecker.Check(this, parseTreeNode, (IEnumerable<string>) null);
      this._tokenizer.SkipNewlines();
      return (ParseTreeNode) new DataSectionStatementNode(token, propertyNameToken, parseTreeNode, commandsAllowed);
    }

    private ParseTreeNode[] GetCommandsAllowed()
    {
      Token errToken = this._tokenizer.Expect(TokenId.ParameterToken);
      if (errToken == null)
        return (ParseTreeNode[]) null;
      if (!"-SupportedCommand".StartsWith(errToken.TokenText, StringComparison.OrdinalIgnoreCase))
      {
        this.ReportException((object) errToken, typeof (ParseException), errToken, "InvalidParameterForDataSectionStatement", (object) errToken.TokenText);
        return (ParseTreeNode[]) null;
      }
      ArrayList arrayList = new ArrayList();
      do
      {
        this._tokenizer.SkipNewlines();
        ParseTreeNode argumentOrCompoundValue = this.GetCommandArgumentOrCompoundValue();
        if (argumentOrCompoundValue == null)
        {
          this.ReportException((object) errToken, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingValueForSupportedCommandInDataSectionStatement");
          break;
        }
        arrayList.Add((object) argumentOrCompoundValue);
      }
      while (this._tokenizer.Expect(TokenId.CommaToken) != null);
      return (ParseTreeNode[]) arrayList.ToArray(typeof (ParseTreeNode));
    }

    private ParseTreeNode FlowControlStatementRule()
    {
      Token token = this._tokenizer.Next();
      if (token == null || !token.IsFlowControl())
      {
        this._tokenizer.UngetToken(token);
        return (ParseTreeNode) null;
      }
      object obj;
      if (token.Is(TokenId.BreakToken) || token.Is(TokenId.ContinueToken))
      {
        obj = this.GetPropertyOrUnaryExpression();
      }
      else
      {
        obj = (object) this.PipelineRule();
        if (obj == this.EmptyPipelineNode)
          obj = (object) null;
      }
      return (ParseTreeNode) new FlowControlNode(token, obj, this._catchBlockDepth > 0);
    }

    private ParseTreeNode FunctionDeclarationRule()
    {
      Token token;
      if ((token = this._tokenizer.Expect(TokenId.FunctionDeclarationToken)) == null)
        return (ParseTreeNode) null;
      List<Token> precedingComments = this._tokenizer.GetPrecedingComments(token);
      this._tokenizer.SkipNewlines();
      Token commandArgumentToken = this._tokenizer.GetCommandArgumentToken();
      if (commandArgumentToken == null)
        this.ReportException((object) TokenId.LiteralCommandArgumentToken, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingNameAfterFunctionKeyword", (object[]) null);
      this._tokenizer.SkipNewlines();
      List<List<Token>> parameterComments;
      ParameterDeclarationNode parameterDeclaration = this.ParameterDeclarationRule(false, (Token) null, out parameterComments);
      this._tokenizer.SkipNewlines();
      ScriptBlockNode scriptBlockNode = this.ScriptBlockRule(commandArgumentToken == null ? (string) null : commandArgumentToken.TokenText, true, token.Is("filter"), parameterDeclaration, precedingComments, parameterComments);
      if (scriptBlockNode == null)
        this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "MissingFunctionBody");
      return (ParseTreeNode) new FunctionDeclarationNode(commandArgumentToken, scriptBlockNode);
    }

    private ParameterDeclarationNode ParameterDeclarationRule(
      bool isParameterStatement,
      Token startScriptBlock,
      out List<List<Token>> parameterComments)
    {
      parameterComments = new List<List<Token>>();
      Token token1 = (Token) null;
      if (isParameterStatement)
      {
        if ((token1 = this._tokenizer.Expect(TokenId.ParameterDeclarationToken)) == null)
          return (ParameterDeclarationNode) null;
        this._tokenizer.SkipNewlines();
      }
      bool processingCallArguments = this._tokenizer.ProcessingCallArguments;
      this._tokenizer.ProcessingCallArguments = true;
      try
      {
        Token token2 = this._tokenizer.Expect(TokenId.OpenParenToken);
        if (token2 == null)
        {
          if (isParameterStatement)
            this._tokenizer.UngetToken(token1);
          return (ParameterDeclarationNode) null;
        }
        if (!isParameterStatement)
          token1 = token2;
        this._tokenizer.SkipNewlines();
        int start1 = this._tokenizer.PositionToken().Start;
        List<ParameterNode> parameters = new List<ParameterNode>();
        Token errToken = (Token) null;
        do
        {
          this._tokenizer.SkipNewlines();
          ParseTreeNode initializer = (ParseTreeNode) null;
          List<AttributeNode> parameterAttributes = (List<AttributeNode>) null;
          List<Token> comments = (List<Token>) null;
          VariableDereferenceNode parameter = this.ParameterRule(out parameterAttributes, out initializer, out comments);
          if (parameter == null)
          {
            if (errToken != null)
            {
              this.ReportException((object) errToken, typeof (IncompleteParseException), errToken, "MissingExpressionAfterToken", (object) ',');
              break;
            }
            break;
          }
          parameters.Add(new ParameterNode(parameter, parameterAttributes, initializer));
          parameterComments.Add(comments);
          this._tokenizer.SkipNewlines();
          errToken = this._tokenizer.Expect(TokenId.CallArgumentSeparatorToken);
        }
        while (errToken != null);
        int start2 = this._tokenizer.PositionToken().Start;
        this._tokenizer.SkipNewlines();
        this._tokenizer.Require(TokenId.CloseParenToken, "MissingEndParenthesisInFunctionParameterList");
        string source;
        if (isParameterStatement)
        {
          source = this._tokenizer.Offset < this._tokenizer.Script.Length ? this._tokenizer.Script.Substring(startScriptBlock.Start, this._tokenizer.Offset - startScriptBlock.Start) : this._tokenizer.Script.Substring(startScriptBlock.Start);
        }
        else
        {
          StringBuilder stringBuilder = new StringBuilder("param(");
          stringBuilder.Append(this._tokenizer.Script, start1, start2 - start1);
          stringBuilder.Append(')');
          source = stringBuilder.ToString();
        }
        ParameterDeclarationNode.Validate(this, parameters);
        return new ParameterDeclarationNode(token1, parameters, source);
      }
      finally
      {
        this._tokenizer.ProcessingCallArguments = processingCallArguments;
      }
    }

    private VariableDereferenceNode ParameterRule(
      out List<AttributeNode> parameterAttributes,
      out ParseTreeNode initializer,
      out List<Token> comments)
    {
      comments = (List<Token>) null;
      parameterAttributes = new List<AttributeNode>();
      List<TypeLiteral> typeConstraint = new List<TypeLiteral>();
      bool flag = false;
      this._tokenizer.SkipNewlines();
      comments = this._tokenizer.GetPrecedingComments((Token) null);
      while (true)
      {
        this._tokenizer.SkipNewlines();
        AttributeNode attributeNode1 = this.AttributeRule();
        if (attributeNode1 != null)
        {
          parameterAttributes.Add(attributeNode1);
          flag = true;
        }
        else
        {
          Token token = this._tokenizer.Expect(TokenId.TypeToken);
          if (token != null)
          {
            flag = true;
            TypeLiteral typeLiteral = new TypeLiteral(token);
            typeConstraint.Add(typeLiteral);
            object[] arguments = new object[1]
            {
              (object) typeLiteral
            };
            AttributeNode attributeNode2 = new AttributeNode(token, arguments, (HashLiteralNode) null);
            parameterAttributes.Add(attributeNode2);
          }
          else
            break;
        }
      }
      this._tokenizer.SkipNewlines();
      if (comments == null)
        comments = this._tokenizer.GetPrecedingComments((Token) null);
      ParseMode mode = this._tokenizer.Mode;
      this._tokenizer.Mode = ParseMode.Expression;
      Token token1 = (Token) null;
      try
      {
        token1 = this._tokenizer.Expect(TokenId.VariableToken);
        if (flag)
        {
          if (comments.Count != 0)
            goto label_12;
        }
        comments = this._tokenizer.GetPrecedingComments(token1);
      }
      finally
      {
        this._tokenizer.Mode = mode;
      }
label_12:
      initializer = (ParseTreeNode) null;
      if (token1 == null)
      {
        if (flag)
          this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "InvalidFunctionParameter");
        return (VariableDereferenceNode) null;
      }
      parameterAttributes.Reverse();
      typeConstraint.Reverse();
      VariableDereferenceNode variableDereferenceNode = new VariableDereferenceNode(token1, typeConstraint, false);
      variableDereferenceNode.CheckTypeConstraintViolation();
      this._tokenizer.SkipNewlines();
      Token errToken = this._tokenizer.Expect(TokenId.AssignmentOperatorToken);
      if (errToken != null)
      {
        if (!errToken.Is("="))
          this.ReportException((object) null, typeof (ParseException), this._tokenizer.PositionToken(), "InvalidFunctionParameter");
        this._tokenizer.SkipNewlines();
        initializer = this.ExpressionRule();
        if (initializer == null)
          this.ReportException((object) errToken.TokenText, typeof (IncompleteParseException), errToken, "MissingExpressionAfterToken", (object) '=');
      }
      return variableDereferenceNode;
    }

    private bool AttributeArgumentRule(
      HashLiteralNode hashNode,
      Hashtable keys,
      out ParseTreeNode expression)
    {
      expression = (ParseTreeNode) null;
      if (this.GetNameValuePair(hashNode, keys, false))
        return true;
      expression = this.ExpressionRule();
      return expression != null;
    }

    private AttributeNode AttributeRule()
    {
      Token attributeToken = this.GetAttributeToken();
      if (attributeToken == null)
        return (AttributeNode) null;
      Token token = this._tokenizer.Next();
      ArrayList arrayList = new ArrayList();
      HashLiteralNode hashLiteralNode = new HashLiteralNode();
      Hashtable keys = new Hashtable((IEqualityComparer) StringComparer.CurrentCultureIgnoreCase);
      if (token.Is(TokenId.OpenParenToken))
      {
        Token errToken = (Token) null;
        bool processingCallArguments = this._tokenizer.ProcessingCallArguments;
        this._tokenizer.ProcessingCallArguments = true;
        try
        {
          do
          {
            this._tokenizer.SkipNewlines();
            ParseTreeNode expression = (ParseTreeNode) null;
            if (!this.AttributeArgumentRule(hashLiteralNode, keys, out expression))
            {
              if (errToken != null)
              {
                this.ReportException((object) errToken.TokenText, typeof (IncompleteParseException), errToken, "MissingExpressionAfterToken", (object) ',');
                break;
              }
              break;
            }
            if (expression != null)
            {
              object attributeArgument = this.GetAttributeArgument(expression);
              arrayList.Add(attributeArgument);
            }
            this._tokenizer.SkipNewlines();
            errToken = this._tokenizer.Expect(TokenId.CallArgumentSeparatorToken);
          }
          while (errToken != null);
          this._tokenizer.Require(TokenId.CloseParenToken, "MissingEndParenthesisInExpression");
          this._tokenizer.SkipNewlines();
          this._tokenizer.Require(TokenId.CloseSquareBracketToken, "EndSquareBracketExpectedAtEndOfAttribute");
        }
        finally
        {
          this._tokenizer.ProcessingCallArguments = processingCallArguments;
        }
      }
      hashLiteralNode.ValidateForNamedAttributeArguments(this);
      return new AttributeNode(attributeToken, arrayList.ToArray(), hashLiteralNode);
    }

    private List<AttributeNode> AttributesRule()
    {
      List<AttributeNode> attributeNodeList = new List<AttributeNode>();
      while (true)
      {
        AttributeNode attributeNode = this.AttributeRule();
        if (attributeNode != null)
        {
          attributeNodeList.Add(attributeNode);
          this._tokenizer.SkipNewlines();
        }
        else
          break;
      }
      return attributeNodeList;
    }

    private Token GetAttributeToken()
    {
      Token attributeStartToken = this._tokenizer.GetAttributeStartToken();
      if (attributeStartToken == null)
        return (Token) null;
      Token attributeToken = this._tokenizer.GetAttributeToken();
      if (attributeToken == null)
      {
        this._tokenizer.UngetToken(attributeStartToken);
        this._tokenizer.Resync();
        return (Token) null;
      }
      Token token = this._tokenizer.Peek();
      if (token != null && (token.Is(TokenId.CloseSquareBracketToken) || token.Is(TokenId.OpenParenToken)) && !token.Is(TokenId.CloseSquareBracketToken))
        return attributeToken;
      this._tokenizer.UngetToken(attributeToken);
      this._tokenizer.UngetToken(attributeStartToken);
      this._tokenizer.Resync();
      return (Token) null;
    }

    private object GetAttributeArgument(ParseTreeNode expression)
    {
      if (expression.ValidAttributeArgument)
        return expression.GetConstValue();
      this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "ParameterAttributeArgumentNeedsToBeConstantOrScriptBlock");
      return (object) null;
    }

    private ParseTreeNode ExpressionRule() => this.ExpressionRule(0);

    private ParseTreeNode ExpressionRule(int terminalPrecedence)
    {
      ParseMode mode = this._tokenizer.Mode;
      this._tokenizer.Mode = ParseMode.Expression;
      try
      {
        this.IncrementRecursionDepth();
        ParseTreeNode parseTreeNode = this.ArrayLiteralRule();
        if (parseTreeNode == null)
          return (ParseTreeNode) null;
        ExpressionNode expressionNode = new ExpressionNode();
        expressionNode.Add(parseTreeNode);
        bool flag = true;
        Token token;
        while (true)
        {
          token = this._tokenizer.Next();
          if (token is OperatorToken opToken && opToken.Precedence > terminalPrecedence)
          {
            this._tokenizer.SkipNewlines();
            if (opToken.TokenId == TokenId.LogicalOperatorToken && (opToken.Is("-and") || opToken.Is("-or")))
            {
              parseTreeNode = this.ExpressionRule(opToken.Precedence);
              if (parseTreeNode != null && !parseTreeNode.IsConstant)
                parseTreeNode = (ParseTreeNode) new DeferredExpressionNode(parseTreeNode);
            }
            else
              parseTreeNode = this.ArrayLiteralRule();
            if (parseTreeNode == null)
              this.ReportException((object) null, typeof (IncompleteParseException), (Token) opToken, "ExpectedValueExpression", (object) opToken.TokenText);
            expressionNode.Add(opToken, parseTreeNode);
            flag = false;
          }
          else
            break;
        }
        this._tokenizer.UngetToken(token);
        if (flag)
        {
          parseTreeNode.IsExpression = true;
          return parseTreeNode;
        }
        if (!this._accumulateErrors)
          expressionNode.Complete(this.InDataSection);
        return expressionNode.GetConstantWrapperNode() ?? (ParseTreeNode) expressionNode;
      }
      finally
      {
        this._tokenizer.Mode = mode;
        this.DecrementRecursionDepth();
      }
    }

    private ParseTreeNode ArrayLiteralRule()
    {
      bool flag = true;
      Token token = (Token) null;
      List<ParseTreeNode> expressions = new List<ParseTreeNode>();
      Token errToken = (Token) null;
      while (true)
      {
        ParseTreeNode parseTreeNode = this.UnaryPrefixPostfixExpressionRule();
        if (parseTreeNode != null)
        {
          if (!(parseTreeNode is IAssignableParseTreeNode))
            flag = false;
          expressions.Add(parseTreeNode);
          errToken = this._tokenizer.Expect(TokenId.CommaToken);
          if (errToken != null)
          {
            if (token == null)
              token = errToken;
            this._tokenizer.SkipNewlines();
          }
          else
            goto label_10;
        }
        else
          break;
      }
      if (errToken != null)
        this.ReportException((object) errToken.TokenText, typeof (IncompleteParseException), errToken, "MissingExpressionAfterToken", (object) ',');
label_10:
      if (expressions.Count == 0)
        return (ParseTreeNode) null;
      if (expressions.Count == 1)
        return expressions[0];
      return flag ? (ParseTreeNode) new AssignableArrayLiteralNode(token, expressions) : (ParseTreeNode) new ArrayLiteralNode(token, (IList<ParseTreeNode>) expressions);
    }

    private ParseTreeNode UnaryPrefixPostfixExpressionRule()
    {
      List<Token> preOperators = new List<Token>();
      this._tokenizer.AllowSignedNumber = true;
      try
      {
        this._tokenizer.Resync();
        Token token;
        for (token = this._tokenizer.Next(); token != null && (token.IsUnaryOperator() || token.IsPrePostFix()) && (!token.Is(TokenId.TypeToken) || this.IsTypeUnaryOperator(token)); token = this._tokenizer.Next())
        {
          preOperators.Add(token);
          if (token.IsUnaryOperator() && !token.Is(TokenId.TypeToken))
            this._tokenizer.SkipNewlines();
        }
        this._tokenizer.UngetToken(token);
      }
      finally
      {
        this._tokenizer.AllowSignedNumber = false;
      }
      ParseTreeNode expression = this.CompoundValueRule();
      if (expression == null && preOperators.Count > 0 && preOperators[preOperators.Count - 1].Is(TokenId.TypeToken))
      {
        this._tokenizer.UngetToken(preOperators[preOperators.Count - 1]);
        preOperators.RemoveAt(preOperators.Count - 1);
        expression = this.CompoundValueRule();
      }
      if (expression == null)
      {
        if (preOperators.Count > 0)
          this.ReportException((object) preOperators[preOperators.Count - 1].TokenText, typeof (IncompleteParseException), preOperators[preOperators.Count - 1], "MissingExpressionAfterOperator", (object) preOperators[preOperators.Count - 1].TokenText);
        return (ParseTreeNode) null;
      }
      Token token1 = this._tokenizer.Next();
      if (token1 != null && !token1.IsPrePostFix())
      {
        this._tokenizer.UngetToken(token1);
        token1 = (Token) null;
      }
      if (token1 == null && preOperators.Count == 0)
        return expression;
      if (!UnaryPrefixPostFixNode.ValidateOperatorSequence(this, expression, preOperators, token1))
        return (ParseTreeNode) null;
      UnaryPrefixPostFixNode prefixPostFixNode = new UnaryPrefixPostFixNode(expression, preOperators, token1);
      return prefixPostFixNode.Deprecated ? prefixPostFixNode.Expression : (ParseTreeNode) prefixPostFixNode;
    }

    private bool IsTypeUnaryOperator(Token token)
    {
      Token token1 = this._tokenizer.Peek();
      if (token1 == null || token1.Is(TokenId.CommaToken))
        return false;
      if (token1.IsUnaryOperator() || token1.IsPrePostFix())
        return true;
      Token referenceOperatorToken = this._tokenizer.GetReferenceOperatorToken();
      if (referenceOperatorToken == null)
        return true;
      this._tokenizer.UngetToken(referenceOperatorToken);
      this._tokenizer.Resync();
      return false;
    }

    private ParseTreeNode CompoundValueRule()
    {
      ParseTreeNode target = this.ValueRule();
      if (target == null)
        return target;
      while (true)
      {
        Token referenceOperatorToken = this._tokenizer.GetReferenceOperatorToken();
        if (referenceOperatorToken != null)
        {
          if (referenceOperatorToken.Is("["))
          {
            this._tokenizer.SkipNewlines();
            object obj = (object) this.ExpressionRule();
            if (obj == null)
            {
              this.ReportException((object) referenceOperatorToken.TokenText, typeof (IncompleteParseException), referenceOperatorToken, "MissingArrayIndexExpression");
              obj = (object) new ConstantNode((Token) null, (object) 0);
            }
            this._tokenizer.Require(TokenId.CloseSquareBracketToken, "MissingEndSquareBracket");
            target = (ParseTreeNode) new ArrayReferenceNode(referenceOperatorToken, target, (ParseTreeNode) obj);
          }
          else
          {
            object property = this.MethodNameRule(referenceOperatorToken);
            if (property is Token)
            {
              ParseTreeNode arguments = this.MethodArgumentRule();
              if (arguments != null)
              {
                target = (ParseTreeNode) new MethodCallNode(target, (Token) property, arguments, referenceOperatorToken.Is("::"));
                continue;
              }
            }
            target = (ParseTreeNode) new PropertyReferenceNode(referenceOperatorToken, target, property);
          }
        }
        else
          break;
      }
      return target;
    }

    private object MethodNameRule(Token referenceOperator)
    {
      Token propertyNameToken = this._tokenizer.GetPropertyNameToken();
      if (propertyNameToken != null)
        return (object) propertyNameToken;
      Token token1 = this._tokenizer.Expect(TokenId.LiteralStringToken);
      if (token1 != null)
        return (object) token1;
      Token token2 = this._tokenizer.Peek();
      ParseTreeNode parseTreeNode = token2 == null || !token2.IsUnaryOperator() && !token2.IsPrePostFix() ? this.ValueRule() : this.UnaryPrefixPostfixExpressionRule();
      if (parseTreeNode == null)
        this.ReportException((object) referenceOperator.TokenText, typeof (ParseException), referenceOperator, "MissingPropertyName");
      return (object) parseTreeNode;
    }

    private ParseTreeNode MethodArgumentRule()
    {
      bool processingCallArguments = this._tokenizer.ProcessingCallArguments;
      this._tokenizer.ProcessingCallArguments = true;
      try
      {
        Token token1 = this._tokenizer.Expect(TokenId.OpenParenToken);
        if (token1 == null || token1.PreceedByWhiteSpace())
        {
          this._tokenizer.UngetToken(token1);
          return (ParseTreeNode) null;
        }
        Token token2 = (Token) null;
        Token errToken = (Token) null;
        List<ParseTreeNode> parseTreeNodeList = new List<ParseTreeNode>();
        while (true)
        {
          do
          {
            this._tokenizer.SkipNewlines();
            ParseTreeNode parseTreeNode = this.ExpressionRule();
            if (parseTreeNode == null)
            {
              if (errToken != null)
              {
                this.ReportException((object) errToken.TokenText, typeof (IncompleteParseException), errToken, "MissingExpressionAfterToken", (object) ',');
                goto label_10;
              }
              else
                goto label_10;
            }
            else
            {
              parseTreeNodeList.Add(parseTreeNode);
              this._tokenizer.SkipNewlines();
              errToken = this._tokenizer.Expect(TokenId.CallArgumentSeparatorToken);
              if (errToken == null)
                goto label_10;
            }
          }
          while (token2 != null);
          token2 = errToken;
        }
label_10:
        this._tokenizer.Require(TokenId.CloseParenToken, "MissingEndParenthesisInMethodCall");
        return (ParseTreeNode) new ArrayLiteralNode(token2, (IList<ParseTreeNode>) parseTreeNodeList);
      }
      finally
      {
        this._tokenizer.ProcessingCallArguments = processingCallArguments;
      }
    }

    private ParseTreeNode ValueRule()
    {
      Token token1 = this._tokenizer.Peek();
      if (token1 == null)
        return (ParseTreeNode) null;
      try
      {
        this.IncrementRecursionDepth();
        if (token1.Is(TokenId.OpenParenToken))
          return this.ParenExpressionRule();
        if (token1.IsSubexpression())
          return this.SubExpressionRule();
        if (token1.Is(TokenId.OpenBraceToken))
          return this.SubScriptblockRule();
        if (token1.Is(TokenId.HashLiteralStartToken))
          return this.HashLiteralRule();
      }
      finally
      {
        this.DecrementRecursionDepth();
      }
      Token token2 = this._tokenizer.Next();
      if (token2.Is(TokenId.TypeToken))
        return (ParseTreeNode) new TypeNode(token2);
      if (token2.Is(TokenId.NumberToken))
        return (ParseTreeNode) new NumericConstantNode(token2);
      if (token2.Is(TokenId.LiteralStringToken))
        return (ParseTreeNode) new LiteralStringNode(token2);
      if (token2.Is(TokenId.ExpandableStringToken))
        return (ParseTreeNode) new ExpandableStringNode(token2, this);
      if (token2.Is(TokenId.VariableToken))
        return (ParseTreeNode) new VariableDereferenceNode(token2, (List<TypeLiteral>) null, false);
      if (token2.Is(TokenId.SplattedVariableToken))
      {
        this.ReportException((object) token2.TokenText, typeof (ParseException), token2, "SplattingNotPermitted", (object) token2);
        return (ParseTreeNode) null;
      }
      this._tokenizer.UngetToken(token2);
      return (ParseTreeNode) null;
    }

    private ParseTreeNode ParenExpressionRule()
    {
      if (this._tokenizer.Expect(TokenId.OpenParenToken) == null)
        return (ParseTreeNode) null;
      bool processingCallArguments = this._tokenizer.ProcessingCallArguments;
      this._tokenizer.ProcessingCallArguments = false;
      ParseMode mode = this._tokenizer.Mode;
      this._tokenizer.Mode = ParseMode.General;
      try
      {
        this._tokenizer.SkipNewlines();
        ParseTreeNode parseTreeNode = this.PipelineRule();
        if (parseTreeNode == null || parseTreeNode == this.EmptyPipelineNode)
          this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), "ExpectedExpression");
        this._tokenizer.Require(TokenId.CloseParenToken, "MissingEndParenthesisInExpression");
        if (parseTreeNode != null)
          parseTreeNode.IsVoidable = false;
        return parseTreeNode;
      }
      finally
      {
        this._tokenizer.Mode = mode;
        this._tokenizer.ProcessingCallArguments = processingCallArguments;
      }
    }

    private ParseTreeNode SubExpressionRule()
    {
      Token token = this._tokenizer.Next();
      if (!token.IsSubexpression())
        return (ParseTreeNode) null;
      bool processingCallArguments = this._tokenizer.ProcessingCallArguments;
      this._tokenizer.ProcessingCallArguments = false;
      ParseMode mode = this._tokenizer.Mode;
      this._tokenizer.Mode = ParseMode.General;
      StatementListNode statementListNode;
      try
      {
        statementListNode = this.StatementListRule(this._tokenizer.PositionToken());
        this._tokenizer.Require(TokenId.CloseParenToken, "MissingEndParenthesisInSubexpression");
      }
      finally
      {
        this._tokenizer.Mode = mode;
        this._tokenizer.ProcessingCallArguments = processingCallArguments;
      }
      if (statementListNode == null || statementListNode.IsEmpty)
        return token.Is(TokenId.ArraySubexpressionToken) ? (ParseTreeNode) new ConstantNode(token, (object) new object[0]) : (ParseTreeNode) new ConstantNode(token, (object) null);
      if (token.Is(TokenId.ArraySubexpressionToken))
        return (ParseTreeNode) new ArrayWrapperNode(token, (ParseTreeNode) statementListNode);
      statementListNode.IsExpression = false;
      return (ParseTreeNode) statementListNode;
    }

    private ParseTreeNode SubScriptblockRule()
    {
      Token token = this._tokenizer.Peek();
      if (token == null || !token.Is(TokenId.OpenBraceToken))
        return (ParseTreeNode) null;
      Parser.tracer.WriteLine("beginning code block expression parse", new object[0]);
      bool processingCallArguments = this._tokenizer.ProcessingCallArguments;
      this._tokenizer.ProcessingCallArguments = false;
      ParseMode mode = this._tokenizer.Mode;
      this._tokenizer.Mode = ParseMode.General;
      try
      {
        ScriptBlockNode scriptBlockNode = this.ScriptBlockRule((string) null, true, false, (ParameterDeclarationNode) null, (List<Token>) null, (List<List<Token>>) null);
        Parser.tracer.WriteLine("end code block expression parse", new object[0]);
        return (ParseTreeNode) scriptBlockNode;
      }
      finally
      {
        this._tokenizer.Mode = mode;
        this._tokenizer.ProcessingCallArguments = processingCallArguments;
      }
    }

    private ParseTreeNode HashLiteralRule()
    {
      if (this._tokenizer.Expect(TokenId.HashLiteralStartToken) == null)
        return (ParseTreeNode) null;
      this._tokenizer.SkipNewlines();
      bool processingCallArguments = this._tokenizer.ProcessingCallArguments;
      this._tokenizer.ProcessingCallArguments = false;
      ParseMode mode = this._tokenizer.Mode;
      this._tokenizer.Mode = ParseMode.General;
      try
      {
        HashLiteralNode hashNode = new HashLiteralNode();
        Hashtable keys = new Hashtable((IEqualityComparer) StringComparer.CurrentCultureIgnoreCase);
        while (this.GetNameValuePair(hashNode, keys, true))
        {
          Token token = this._tokenizer.Peek();
          if (token != null && (token.Is(TokenId.NewlineToken) || token.Is(TokenId.SemicolonToken)))
            this._tokenizer.SkipNewlinesOrSemicolons();
          else
            break;
        }
        this._tokenizer.Require(TokenId.CloseBraceToken, "IncompleteHashLiteral");
        return (ParseTreeNode) hashNode;
      }
      finally
      {
        this._tokenizer.Mode = mode;
        this._tokenizer.ProcessingCallArguments = processingCallArguments;
      }
    }

    private bool GetNameValuePair(HashLiteralNode hashNode, Hashtable keys, bool hashLiteral)
    {
      Token token1 = this._tokenizer.PositionToken();
      object key = (object) null;
      if (hashLiteral)
      {
        key = this.GetPropertyOrUnaryExpression();
      }
      else
      {
        Token propertyNameToken = this._tokenizer.GetPropertyNameToken();
        if (propertyNameToken != null)
          key = (object) propertyNameToken.TokenText;
      }
      if (key == null)
        return false;
      Token token2 = this._tokenizer.Next();
      if (token2 == null || !token2.Is("="))
      {
        if (token2 != null)
          this._tokenizer.UngetToken(token2);
        this.ReportException((object) null, typeof (ParseException), token2, hashLiteral ? "MissingEqualsInHashLiteral" : "MissingEqualsInNamedArgument");
        return false;
      }
      this._tokenizer.SkipNewlines();
      ParseTreeNode expression = !hashLiteral ? this.ExpressionRule() : this.StatementRule();
      if (expression == null || expression == this.EmptyPipelineNode)
      {
        this.ReportException((object) null, typeof (IncompleteParseException), this._tokenizer.PositionToken(), hashLiteral ? "MissingStatementInHashLiteral" : "MissingExpressionInNamedArgument");
        return false;
      }
      if (key is string str)
      {
        if (keys.ContainsKey((object) str))
        {
          this.ReportException((object) str, typeof (ParseException), token1, hashLiteral ? "DuplicateKeyInHashLiteral" : "DuplicateNamedArgument", (object) str);
          return false;
        }
        keys.Add((object) str, (object) null);
      }
      hashNode.Add(token1, key, expression);
      return true;
    }
  }
}
