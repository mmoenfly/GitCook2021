// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSToken
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public sealed class PSToken
  {
    private string _content = "";
    private PSTokenType _type;
    private int _start;
    private int _length;
    private int _startLine;
    private int _startColumn;
    private int _endLine;
    private int _endColumn;

    internal PSToken(Token token)
    {
      this._type = PSToken.GetTokenType(token.TokenId);
      this._content = token.TokenText;
      this._start = token.Start;
      this._length = token.End - token.Start;
      this._startLine = token.StartLineNumber;
      this._startColumn = token.StartOffsetInLine;
      this._endLine = token.LineNumber;
      this._endColumn = token.OffsetInLine;
    }

    public string Content => this._content;

    public PSTokenType Type => this._type;

    private static PSTokenType GetTokenType(TokenId tokenId)
    {
      switch (tokenId)
      {
        case TokenId.NumberToken:
          return PSTokenType.Number;
        case TokenId.VariableToken:
        case TokenId.SplattedVariableToken:
          return PSTokenType.Variable;
        case TokenId.ParameterToken:
          return PSTokenType.CommandParameter;
        case TokenId.MinusMinusToken:
        case TokenId.PlusPlusToken:
        case TokenId.CommaToken:
        case TokenId.CallArgumentSeparatorToken:
        case TokenId.AssignmentOperatorToken:
        case TokenId.RedirectionOperatorToken:
        case TokenId.MultiplyOperatorToken:
        case TokenId.ComparisonOperatorToken:
        case TokenId.CaseSensitiveComparisonOperatorToken:
        case TokenId.CaseInsensitiveComparisonOperatorToken:
        case TokenId.LogicalOperatorToken:
        case TokenId.LogicalNotToken:
        case TokenId.BitwiseOperatorToken:
        case TokenId.BitwiseNotToken:
        case TokenId.AdditionOperatorToken:
        case TokenId.ReferenceOperatorToken:
        case TokenId.AttributeStartToken:
        case TokenId.AndAndToken:
        case TokenId.OrOrToken:
        case TokenId.PipeToken:
        case TokenId.AmpersandToken:
        case TokenId.DotToken:
        case TokenId.CloseSquareBracketToken:
        case TokenId.FormatOperatorToken:
        case TokenId.RangeOperatorToken:
          return PSTokenType.Operator;
        case TokenId.LiteralCommandArgumentToken:
        case TokenId.ExpandableCommandArgumentToken:
          return PSTokenType.CommandArgument;
        case TokenId.NewlineToken:
          return PSTokenType.NewLine;
        case TokenId.PropertyNameToken:
          return PSTokenType.Member;
        case TokenId.LoopLabelToken:
          return PSTokenType.LoopLabel;
        case TokenId.IfToken:
        case TokenId.ElseToken:
        case TokenId.ElseIfToken:
        case TokenId.ForeachToken:
        case TokenId.FromToken:
        case TokenId.ForToken:
        case TokenId.InToken:
        case TokenId.WhileToken:
        case TokenId.UntilToken:
        case TokenId.DoToken:
        case TokenId.TryToken:
        case TokenId.CatchToken:
        case TokenId.FinallyToken:
        case TokenId.ExitToken:
        case TokenId.ReturnToken:
        case TokenId.BreakToken:
        case TokenId.ContinueToken:
        case TokenId.ThrowToken:
        case TokenId.SwitchToken:
        case TokenId.TrapToken:
        case TokenId.DataSectionToken:
        case TokenId.FunctionDeclarationToken:
        case TokenId.ParameterDeclarationToken:
        case TokenId.BeginToken:
        case TokenId.ProcessToken:
        case TokenId.EndToken:
        case TokenId.DynamicParamToken:
          return PSTokenType.Keyword;
        case TokenId.AttributeToken:
          return PSTokenType.Attribute;
        case TokenId.TypeToken:
          return PSTokenType.Type;
        case TokenId.CmdletNameToken:
          return PSTokenType.Command;
        case TokenId.ArraySubexpressionToken:
        case TokenId.SubexpressionToken:
        case TokenId.HashLiteralStartToken:
        case TokenId.OpenParenToken:
        case TokenId.OpenBraceToken:
          return PSTokenType.GroupStart;
        case TokenId.CloseParenToken:
        case TokenId.CloseBraceToken:
          return PSTokenType.GroupEnd;
        case TokenId.SemicolonToken:
          return PSTokenType.StatementSeparator;
        case TokenId.ExpandableStringToken:
        case TokenId.LiteralStringToken:
          return PSTokenType.String;
        case TokenId.PositionToken:
          return PSTokenType.Position;
        case TokenId.CommentToken:
          return PSTokenType.Comment;
        case TokenId.LineContinueToken:
          return PSTokenType.LineContinuation;
        default:
          return PSTokenType.Unknown;
      }
    }

    public int Start => this._start;

    public int Length => this._length;

    public int StartLine => this._startLine;

    public int StartColumn => this._startColumn;

    public int EndLine => this._endLine;

    public int EndColumn => this._endColumn;
  }
}
