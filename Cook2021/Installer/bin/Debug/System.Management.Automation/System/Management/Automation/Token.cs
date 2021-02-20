// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Token
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;
using System.Text;

namespace System.Management.Automation
{
  internal class Token
  {
    private TokenId _id;
    private string _text;
    private object _data;
    private string _file;
    private string _script;
    private int _start;
    private int _end;
    private int _lineNumber;
    private int _offsetInLine;
    private string _line;
    private int _startLineNumber;
    private int _startOffsetInLine;
    [TraceSource("Tokenizer", "Tokenizer")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("Tokenizer", "Tokenizer");

    internal Token(string text, TokenId id)
    {
      Token.tracer.WriteLine("token<{0}> id<{1}>", (object) text, (object) id);
      this._text = text;
      this._id = id;
    }

    internal TokenId TokenId
    {
      get => this._id;
      set => this._id = value;
    }

    internal string TokenText => this._text;

    internal string FullText => string.IsNullOrEmpty(this._script) ? (string) null : this._script.Substring(this._start, this._end - this._start);

    internal bool FollowedBySpace => this._end < this._script.Length && char.IsWhiteSpace(this._script[this._end]);

    internal object Data
    {
      get => this._data;
      set => this._data = value;
    }

    public override string ToString() => this.TokenText;

    public virtual Token Clone() => new Token(this._text, this._id);

    internal bool Is(string text)
    {
      int length = this._text.Length;
      if (length != this._text.Length)
        return false;
      if (text[0] != '-')
        return string.Equals(text, this._text, StringComparison.OrdinalIgnoreCase);
      return SpecialCharacters.IsDash(this._text[0]) && string.Compare(text, 1, this._text, 1, length - 1, StringComparison.OrdinalIgnoreCase) == 0;
    }

    internal bool Is(TokenId id) => this._id == id;

    internal virtual bool IsUnaryOperator() => this._id == TokenId.CommaToken || this._id == TokenId.LogicalNotToken || (this._id == TokenId.BitwiseNotToken || this._id == TokenId.AdditionOperatorToken) || this._id == TokenId.TypeToken;

    internal bool IsEndOfStatement()
    {
      if (!this.Is(TokenId.AndAndToken))
        this.Is(TokenId.OrOrToken);
      return this._id == TokenId.NewlineToken || this._id == TokenId.SemicolonToken;
    }

    internal bool IsPrePostFix() => this._id == TokenId.PlusPlusToken || this._id == TokenId.MinusMinusToken;

    internal bool IsFlowControl() => this._id == TokenId.ReturnToken || this._id == TokenId.BreakToken || (this._id == TokenId.ContinueToken || this._id == TokenId.ExitToken) || this._id == TokenId.ThrowToken;

    internal bool IsSubexpression() => this._id == TokenId.ArraySubexpressionToken || this._id == TokenId.SubexpressionToken;

    internal bool IsComparison() => this._id == TokenId.ComparisonOperatorToken || this._id == TokenId.CaseSensitiveComparisonOperatorToken || this._id == TokenId.CaseInsensitiveComparisonOperatorToken;

    internal bool IsCmdletKeyword() => this._id == TokenId.BeginToken || this._id == TokenId.EndToken || this._id == TokenId.ProcessToken || this._id == TokenId.DynamicParamToken;

    internal bool IsConstantValue() => this._id == TokenId.LiteralStringToken || this._id == TokenId.LiteralCommandArgumentToken || this._id == TokenId.NumberToken;

    internal bool IsDollarDollarOrDollarCaret()
    {
      if (this._id != TokenId.VariableToken)
        return false;
      return this.TokenText.Equals("^", StringComparison.OrdinalIgnoreCase) || this.TokenText.Equals("$", StringComparison.OrdinalIgnoreCase);
    }

    internal bool PreceedByWhiteSpace() => this._script != null && this._start > 0 && this._start < this._script.Length && char.IsWhiteSpace(this._script[this._start - 1]);

    internal void SetPosition(
      string file,
      string script,
      int start,
      int end,
      Tokenizer tokenizer)
    {
      this._file = file;
      this._script = script;
      this._start = start;
      this._end = end;
      this._line = (string) null;
      if (tokenizer != null)
      {
        this._startLineNumber = tokenizer.GetLineNumberForOffset(start);
        this._startOffsetInLine = tokenizer.GetOffsetWithinLine(this._startLineNumber, start);
        this._lineNumber = tokenizer.GetLineNumberForOffset(end);
        this._offsetInLine = tokenizer.GetOffsetWithinLine(this._lineNumber, end);
        this._line = tokenizer.GetLine(this._startLineNumber);
      }
      else
      {
        this._startLineNumber = 0;
        this._startOffsetInLine = 0;
        this._lineNumber = 0;
        this._offsetInLine = 0;
        this._line = "";
      }
    }

    internal void SetPosition(Token token)
    {
      this._file = token._file;
      this._script = token._script;
      this._start = token._start;
      this._end = token._end;
      this._line = token._line;
      this._startLineNumber = token._startLineNumber;
      this._startOffsetInLine = token._startOffsetInLine;
      this._lineNumber = token._lineNumber;
      this._offsetInLine = token._offsetInLine;
    }

    internal string File => this._file;

    internal string Script => this._script;

    internal int Start => this._start;

    internal int End => this._end;

    internal bool EndOfInput() => this._script.TrimEnd(' ', '\t', '\n', '\r').Length <= this._end;

    internal int LineNumber => this._lineNumber;

    internal int OffsetInLine => this._offsetInLine + 1;

    internal string Line => this._line;

    internal bool IsMultiLineString => (this.Is(TokenId.LiteralStringToken) || this.Is(TokenId.ExpandableStringToken) || this.Is(TokenId.LiteralCommandArgumentToken)) && this._script.IndexOf('\n', this._start, this._end - this._start) >= 0;

    internal bool GetMultiLineStringTokenPosition(int line, out int start, out int end)
    {
      start = 0;
      end = 0;
      if (line < 0)
        return true;
      int startIndex1 = 0;
      if (line == 0)
      {
        if (this._start > 0)
        {
          int num = this._script.LastIndexOf('\n', this._start - 1, this._start - 1);
          if (num > 0)
          {
            startIndex1 = num + 1;
            start = this._start - startIndex1;
          }
        }
      }
      else
      {
        int startIndex2;
        for (startIndex2 = this._start; line > 0 && startIndex2 < this._end; --line)
        {
          int num = this._script.IndexOf('\n', startIndex2, this._end - startIndex2);
          if (num < 0)
            return true;
          startIndex2 = num + 1;
        }
        if (line > 0)
          return true;
        startIndex1 = startIndex2;
      }
      if (startIndex1 >= this._end)
        return true;
      int num1 = this._script.IndexOf('\n', startIndex1, this._end - startIndex1);
      if (num1 < 0)
      {
        end = this._end - startIndex1;
        return true;
      }
      if (num1 > startIndex1 && this._script[num1 - 1] == '\r')
        --num1;
      end = num1 - startIndex1;
      return false;
    }

    internal string Position() => this.Position(Token.PositionMessageText.Verbose);

    internal string Position(bool verbose) => this.Position(verbose ? Token.PositionMessageText.Verbose : Token.PositionMessageText.Basic);

    internal string Position(Token.PositionMessageText mode)
    {
      if (string.IsNullOrEmpty(this._script))
        return (string) null;
      StringBuilder stringBuilder = new StringBuilder();
      if (this.Is(TokenId.NewlineToken))
      {
        stringBuilder.Append(this._line);
        stringBuilder.Append(" <<<< ");
      }
      else
      {
        stringBuilder.Append(this._line.Substring(0, this._offsetInLine));
        stringBuilder.Append(" <<<< ");
        stringBuilder.Append(this._line.Substring(this._offsetInLine));
      }
      switch (mode)
      {
        case Token.PositionMessageText.Verbose:
          string str = this._file;
          if (string.IsNullOrEmpty(str))
            str = ResourceManagerCache.GetResourceString("Parser", "TextForWordLine");
          return StringUtil.Format(ResourceManagerCache.GetResourceString("Parser", "TextForPositionMessage"), (object) str, (object) this._lineNumber, (object) this.OffsetInLine, (object) stringBuilder.ToString());
        case Token.PositionMessageText.Basic:
          return StringUtil.Format(ResourceManagerCache.GetResourceString("Parser", "TraceScriptLineMessage"), (object) this._lineNumber, (object) stringBuilder.ToString());
        case Token.PositionMessageText.LineOnly:
          return stringBuilder.ToString();
        default:
          return (string) null;
      }
    }

    internal int StartLineNumber => this._startLineNumber;

    internal int StartOffsetInLine => this._startOffsetInLine + 1;

    internal enum PositionMessageText
    {
      Verbose,
      Basic,
      LineOnly,
    }
  }
}
