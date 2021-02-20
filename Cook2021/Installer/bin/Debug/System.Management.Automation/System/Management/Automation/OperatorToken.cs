// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.OperatorToken
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class OperatorToken : Token
  {
    private int _precedence;
    private PowerShellBinaryOperator _operationDelegate;

    internal OperatorToken(
      string text,
      TokenId id,
      int precedence,
      PowerShellBinaryOperator operationDelegate)
      : base(text, id)
    {
      this._precedence = precedence;
      this._operationDelegate = operationDelegate;
    }

    internal OperatorToken(string text, TokenId id, int precedence)
      : base(text, id)
    {
      this._precedence = precedence;
      this.BindDelegates(text, id);
    }

    internal OperatorToken(string text, TokenId id)
      : base(text, id)
      => this.BindDelegates(text, id);

    public override Token Clone() => (Token) new OperatorToken(this.TokenText, this.TokenId, this._precedence, this._operationDelegate);

    internal int Precedence => this._precedence;

    internal virtual bool IsValidInRestrictedLanguage
    {
      get
      {
        switch (this.TokenId)
        {
          case TokenId.FormatOperatorToken:
          case TokenId.RangeOperatorToken:
            return false;
          default:
            return this.Precedence > 0 || this.IsUnaryOperator();
        }
      }
    }

    internal PowerShellBinaryOperator OperationDelegate => this._operationDelegate;

    protected void BindDelegates(string text, TokenId id)
    {
      switch (id)
      {
        case TokenId.AssignmentOperatorToken:
          switch (text)
          {
            case "+=":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.PolyAdd);
              return;
            case "-=":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.PolyMinus);
              return;
            case "*=":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.PolyMultiply);
              return;
            case "/=":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.PolyDiv);
              return;
            case "%=":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.PolyMod);
              return;
            case null:
              return;
            default:
              return;
          }
        case TokenId.MultiplyOperatorToken:
          switch (text)
          {
            case "*":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.PolyMultiply);
              return;
            case "/":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.PolyDiv);
              return;
            case "%":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.PolyMod);
              return;
            case null:
              return;
            default:
              return;
          }
        case TokenId.ComparisonOperatorToken:
        case TokenId.CaseSensitiveComparisonOperatorToken:
        case TokenId.CaseInsensitiveComparisonOperatorToken:
          ComparisonToken comparisonToken = (ComparisonToken) this;
          if (comparisonToken != null)
          {
            text = comparisonToken.ComparisonName;
            if (text == null)
              break;
          }
          switch (text)
          {
            case "-replace":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.ReplaceOperator);
              return;
            case "-split":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.SplitOperator);
              return;
            case "-join":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.JoinOperator);
              return;
            case "-as":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.AsOperator);
              return;
            case "-isnot":
            case "-is":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.IsOperator);
              return;
            case "-like":
            case "-notlike":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.LikeOperator);
              return;
            case "-match":
            case "-notmatch":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.MatchOperator);
              return;
            default:
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.ComparisonOperators);
              return;
          }
        case TokenId.LogicalOperatorToken:
          switch (text)
          {
            case "-and":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.andOperator);
              return;
            case "-or":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.orOperator);
              return;
            case "-xor":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.xorOperator);
              return;
            case null:
              return;
            default:
              return;
          }
        case TokenId.BitwiseOperatorToken:
          switch (text)
          {
            case "-band":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.bandOperator);
              return;
            case "-bor":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.borOperator);
              return;
            case "-bxor":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.bxorOperator);
              return;
            case null:
              return;
            default:
              return;
          }
        case TokenId.AdditionOperatorToken:
          switch (text)
          {
            case "+":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.PolyAdd);
              return;
            case "-":
              this._operationDelegate = new PowerShellBinaryOperator(ParserOps.PolyMinus);
              return;
            case null:
              return;
            default:
              return;
          }
        case TokenId.FormatOperatorToken:
          this._operationDelegate = new PowerShellBinaryOperator(ParserOps.formatOperator);
          break;
        case TokenId.RangeOperatorToken:
          this._operationDelegate = new PowerShellBinaryOperator(ParserOps.rangeOperator);
          break;
      }
    }
  }
}
