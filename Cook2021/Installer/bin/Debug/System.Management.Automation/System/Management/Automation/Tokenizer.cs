// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Tokenizer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class Tokenizer : IStringTokenReaderHelper
  {
    internal const int AttributeTokenRule = 0;
    internal const int PropertyNameTokenRule = 3;
    internal const int ParameterTokenRule = 4;
    internal const int OperatorTokenRule = 5;
    internal const int ReferenceOperatorTokenRule = 7;
    internal const int CommandArgumentTokenRule = 8;
    internal const int AttributeStartTokenRule = 12;
    private const string SignatureHeaderLower = "#sig#beginsignatureblock";
    private const string SignatureHeaderUpper = "#SIG#BEGINSIGNATUREBLOCK";
    internal const int CommentBlockProximity = 2;
    internal const string TokenTerminator = "{}();,|&\r\n\t ";
    internal const string VariableParameterTokenTerminator = ":.[{}();,|&\r\n\t ";
    private Parser _parser;
    private int _startLineNumber = 1;
    private string _file;
    private int _offset;
    private string _script;
    private List<int> _lineStartOffsets = new List<int>(512);
    private List<string> _scriptLines = new List<string>(512);
    private Token _firstToken;
    private Token _lastToken;
    private ParseMode _mode;
    private bool _allowRangeOperator;
    private bool _processingCallArguments;
    private bool _allowSignedNumber;
    private TokenReader[] _tokenReaders;
    private bool _endOfValidScriptText;
    private List<Token> _tokens = new List<Token>();
    private List<Token> _ungotTokens = new List<Token>();
    [TraceSource("Tokenizer", "Tokenizer")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (Tokenizer), nameof (Tokenizer));

    public Tokenizer(Parser parser)
    {
      this._parser = parser;
      this.Initialize();
    }

    internal Parser Parser => this._parser;

    public void Reset(string script, int cursorPosition)
    {
      this._firstToken = (Token) null;
      this._lastToken = (Token) null;
      this._offset = 0;
      this._script = script;
      this._lineStartOffsets.Clear();
      this._scriptLines.Clear();
      this._lineStartOffsets.Capacity = 4;
      this._scriptLines.Capacity = 4;
      this.AddLineStartNoCheck(0);
      this._endOfValidScriptText = false;
      this._ungotTokens.Clear();
      this._tokens.Clear();
      this._ungotTokens.Capacity = 4;
      this._tokens.Capacity = 4;
      this.Enable(3, false);
      this.Enable(7, false);
      this.Enable(12, false);
      this.Enable(0, false);
      this.Enable(8, false);
      this.Enable(4, true);
      this._allowRangeOperator = false;
      this._processingCallArguments = false;
    }

    internal int StartLineNumber
    {
      get => this._startLineNumber;
      set => this._startLineNumber = value;
    }

    internal string File
    {
      get => this._file;
      set => this._file = value;
    }

    internal int Offset => this._offset;

    internal string Script => this._script;

    internal Token FirstToken => this._firstToken;

    internal Token LastToken => this._lastToken;

    private void SetFirstLastToken(Token token)
    {
      if (token == null || token.Is(TokenId.NewlineToken))
        return;
      if (this._firstToken == null)
        this._firstToken = token;
      this._lastToken = token;
    }

    internal ParseMode Mode
    {
      get => this._mode;
      set
      {
        if (this._mode == value)
          return;
        this._mode = value;
        switch (this._mode)
        {
          case ParseMode.General:
            this.Enable(8, false);
            this.Enable(4, true);
            this._allowRangeOperator = false;
            break;
          case ParseMode.Command:
            this.Enable(8, true);
            this.Enable(4, true);
            this._allowRangeOperator = false;
            break;
          case ParseMode.Expression:
            this.Enable(8, false);
            this.Enable(4, false);
            this._allowRangeOperator = true;
            break;
        }
        this.Resync();
      }
    }

    internal bool AllowRangeOperator => this._allowRangeOperator;

    internal bool ProcessingCallArguments
    {
      get => this._processingCallArguments;
      set
      {
        if (this._processingCallArguments = value)
          return;
        this._processingCallArguments = value;
        this.Resync();
      }
    }

    internal bool AllowSignedNumber
    {
      get => this._allowSignedNumber;
      set => this._allowSignedNumber = value;
    }

    private void Initialize() => this._tokenReaders = new TokenReader[15]
    {
      (TokenReader) new AttributeTokenReader(this),
      (TokenReader) new StringTokenReader(this, TokenClass.String),
      (TokenReader) new VariableTokenReader(this),
      (TokenReader) new PropertyTokenReader(this, TokenId.PropertyNameToken),
      (TokenReader) new PropertyTokenReader(this, TokenId.ParameterToken),
      (TokenReader) new OperatorTokenReader(this, OperatorTokenReaderType.General),
      (TokenReader) new NumberTokenReader(this),
      (TokenReader) new OperatorTokenReader(this, OperatorTokenReaderType.Reference),
      (TokenReader) new StringTokenReader(this, TokenClass.Argument),
      (TokenReader) new OperatorTokenReader(this, OperatorTokenReaderType.Expression),
      (TokenReader) new KeywordTokenReader(this),
      (TokenReader) new PropertyTokenReader(this, TokenId.LoopLabelToken),
      (TokenReader) new OperatorTokenReader(this, OperatorTokenReaderType.Attribute),
      (TokenReader) new TypeTokenReader(this),
      (TokenReader) new StringTokenReader(this, TokenClass.Command)
    };

    public bool Enable(int rule, bool enable)
    {
      bool enabled = this._tokenReaders[rule].Enabled;
      this._tokenReaders[rule].Enabled = enable;
      return enabled;
    }

    public bool IsEnabled(int rule) => this._tokenReaders[rule].Enabled;

    internal Token Next()
    {
      Token tokenFromBuffer;
      if ((tokenFromBuffer = this.GetTokenFromBuffer()) != null)
      {
        this.SetFirstLastToken(tokenFromBuffer);
        this._tokens.Add(tokenFromBuffer);
        return tokenFromBuffer;
      }
      this.FindNextToken();
      Token newLineToken;
      if ((newLineToken = this.GetNewLineToken()) != null)
      {
        this._tokens.Add(newLineToken);
        return newLineToken;
      }
      if (this._offset >= this._script.Length)
        return (Token) null;
      int offset = this._offset;
      for (int index = 0; index < this._tokenReaders.Length; ++index)
      {
        TokenReader tokenReader = this._tokenReaders[index];
        if (tokenReader.Enabled)
        {
          Token token;
          if ((token = tokenReader.GetToken(this._script, ref this._offset)) != null)
          {
            if (this._endOfValidScriptText)
              this.Parser.ReportException((object) null, typeof (ParseException), this.PositionToken(), "TokenAfterEndOfValidScriptText");
            token.SetPosition(this._file, this._script, offset, this._offset, this);
            this.SetFirstLastToken(token);
            this._tokens.Add(token);
            return token;
          }
          if (this._offset >= this._script.Length)
            return (Token) null;
        }
      }
      this.Parser.ReportException((object) null, typeof (ParseException), this.PositionToken(), "UnrecognizedToken");
      return (Token) null;
    }

    internal Token Peek()
    {
      Token token = this.Next();
      if (token != null)
        this.UngetToken(token);
      return token;
    }

    internal Token Expect(TokenId id)
    {
      Token token = this.Next();
      if (token == null)
        return (Token) null;
      if (token.Is(id))
        return token;
      this.UngetToken(token);
      return (Token) null;
    }

    internal Token Require(TokenId tokenId, string resourceIdAndErrorId, params object[] args)
    {
      this.SkipNewlines();
      Tokenizer.tracer.WriteLine("looking for required token {0}", (object) tokenId);
      Token token;
      if ((token = this.Expect(tokenId)) == null)
        this.Parser.ReportException((object) tokenId, typeof (IncompleteParseException), this.PositionToken(), resourceIdAndErrorId, args);
      return token;
    }

    internal Token GetPropertyNameToken()
    {
      bool enable = this.Enable(3, true);
      this.Resync();
      Token token = (Token) null;
      try
      {
        token = this.Next();
      }
      finally
      {
        this.Enable(3, enable);
      }
      if (token == null)
        return (Token) null;
      if (token.Is(TokenId.PropertyNameToken))
        return token;
      this.UngetToken(token);
      return (Token) null;
    }

    internal Token GetReferenceOperatorToken()
    {
      bool enable = this.Enable(7, true);
      this.Resync();
      Token token = (Token) null;
      try
      {
        token = this.Next();
      }
      finally
      {
        this.Enable(7, enable);
      }
      if (token == null)
        return (Token) null;
      if (token.Is(TokenId.ReferenceOperatorToken))
        return token;
      this.UngetToken(token);
      return (Token) null;
    }

    internal Token GetAttributeStartToken()
    {
      bool enable = this.Enable(12, true);
      this.Resync();
      Token token = (Token) null;
      try
      {
        token = this.Next();
      }
      finally
      {
        this.Enable(12, enable);
      }
      if (token == null)
        return (Token) null;
      if (token.Is(TokenId.AttributeStartToken))
        return token;
      this.UngetToken(token);
      return (Token) null;
    }

    internal Token GetAttributeToken()
    {
      bool enable = this.Enable(0, true);
      this.Resync();
      Token token = (Token) null;
      try
      {
        token = this.Next();
      }
      finally
      {
        this.Enable(0, enable);
      }
      if (token == null)
        return (Token) null;
      if (token.Is(TokenId.AttributeToken))
        return token;
      this.UngetToken(token);
      return (Token) null;
    }

    internal Token GetCommandArgumentToken()
    {
      bool enable = this.Enable(8, true);
      this.Resync();
      Token token = (Token) null;
      try
      {
        token = this.Next();
      }
      finally
      {
        this.Enable(8, enable);
      }
      if (token == null)
        return (Token) null;
      switch (token.TokenId)
      {
        case TokenId.ParameterToken:
        case TokenId.LiteralCommandArgumentToken:
        case TokenId.ExpandableCommandArgumentToken:
          return token;
        default:
          this.UngetToken(token);
          return (Token) null;
      }
    }

    internal void SkipNewlines() => this.SkipNewlinesAndMaybeSemicolons(false);

    internal void SkipNewlinesOrSemicolons() => this.SkipNewlinesAndMaybeSemicolons(true);

    private static bool CheckSignatureHeader(string signature, ref int offset)
    {
      if (offset > 0 && signature[offset - 1] != '\n' || signature[offset] != '#')
        return false;
      int index1 = offset + 1;
      int index2;
      for (index2 = 1; index1 < signature.Length && index2 < "#sig#beginsignatureblock".Length; ++index1)
      {
        char ch = signature[index1];
        if ((int) ch == (int) "#sig#beginsignatureblock"[index2] || (int) ch == (int) "#SIG#BEGINSIGNATUREBLOCK"[index2])
          ++index2;
        else if (ch != ' ' && ch != '\t')
          break;
      }
      offset = index1 - 1;
      return index2 == "#sig#beginsignatureblock".Length;
    }

    public void SetToEndOfInput() => this._offset = this._script.Length;

    internal bool EndOfInput()
    {
      this.Resync();
      this.FindNextToken();
      return this._offset >= this._script.Length;
    }

    private Token GetNewLineToken()
    {
      if (this._offset >= this._script.Length)
        return (Token) null;
      if (this._script[this._offset] == '\n')
      {
        Token token = new Token("\n", TokenId.NewlineToken);
        token.SetPosition(this._file, this._script, this._offset, this._offset + 1, this);
        ++this._offset;
        this.AddLineStart(this._offset);
        return token;
      }
      if (this._script[this._offset] != '\r')
        return (Token) null;
      if (this._offset + 1 < this._script.Length && this._script[this._offset + 1] == '\n')
      {
        Token token = new Token("\r\n", TokenId.NewlineToken);
        token.SetPosition(this._file, this._script, this._offset, this._offset + 2, this);
        this._offset += 2;
        this.AddLineStart(this._offset);
        return token;
      }
      Token token1 = new Token("\r", TokenId.NewlineToken);
      token1.SetPosition(this._file, this._script, this._offset, this._offset + 1, this);
      ++this._offset;
      this.AddLineStart(this._offset);
      return token1;
    }

    private void ReadMultiLineComment()
    {
      bool flag = false;
      int offset = this._offset;
      for (this._offset += 2; this._offset < this._script.Length; ++this._offset)
      {
        char ch = this._script[this._offset];
        if (ch == '#' && this._offset + 1 < this._script.Length && this._script[this._offset + 1] == '>')
        {
          ++this._offset;
          flag = true;
          break;
        }
        switch (ch)
        {
          case '\n':
            this.AddLineStart(this._offset + 1);
            break;
          case '\r':
            if (this._offset + 1 < this._script.Length && this._script[this._offset + 1] == '\n')
              ++this._offset;
            this.AddLineStart(this._offset + 1);
            break;
        }
      }
      if (!flag)
      {
        Token errToken = new Token(this._script.Substring(offset), TokenId.CommaToken);
        errToken.SetPosition(this._file, this._script, offset, this._offset, this);
        this.Parser.ReportException((object) null, typeof (IncompleteParseException), errToken, "MissingEndMultiLineComment", (object) "#>");
      }
      else
        this.CreateAccessoryToken(TokenId.CommentToken, offset, this._offset + 1);
    }

    private void FindNextToken()
    {
      int start = 0;
      bool flag = false;
      for (; this._offset < this._script.Length; ++this._offset)
      {
        char c = this._script[this._offset];
        if (flag)
        {
          if (c == '\n' || c == '\r')
            break;
        }
        else if (c == '#')
        {
          flag = true;
          start = this._offset;
          if (Tokenizer.CheckSignatureHeader(this._script, ref this._offset))
            this._endOfValidScriptText = true;
        }
        else if (c == '<' && this._offset + 1 < this._script.Length && this._script[this._offset + 1] == '#')
        {
          this.ReadMultiLineComment();
        }
        else
        {
          if (c == '`' && this._offset + 1 >= this._script.Length)
          {
            ++this._offset;
            this.Parser.ReportException((object) "`", typeof (IncompleteParseException), this.PositionToken(), "IncompleteString");
          }
          if (c == '`' && this._offset + 1 < this._script.Length && (this._script[this._offset + 1] == '\n' || this._script[this._offset + 1] == '\r'))
          {
            int offset = this._offset;
            if (this._script[++this._offset] == '\r' && this._offset < this._script.Length && this._script[this._offset + 1] == '\n')
              ++this._offset;
            this.CreateAccessoryToken(TokenId.LineContinueToken, offset, this._offset + 1);
            this.AddLineStart(this._offset + 1);
          }
          else if (!char.IsWhiteSpace(c) || c == '\n' || c == '\r')
            break;
        }
      }
      if (!flag)
        return;
      this.CreateAccessoryToken(TokenId.CommentToken, start, this._offset);
    }

    private void CreateAccessoryToken(TokenId id, int start, int end)
    {
      Token token = new Token(this._script.Substring(start, end - start), id);
      token.SetPosition(this._file, this._script, start, end, this);
      this._tokens.Add(token);
    }

    internal static bool IsWhiteSpace(char c) => char.IsWhiteSpace(c) && c != '\n' && c != '\r';

    private int FindToken(Token token)
    {
      int index = this._tokens.Count - 1;
      while (index >= 0 && this._tokens[index] != token)
        --index;
      return index;
    }

    internal List<Token> GetFollowingCommentBlock(Token token)
    {
      List<Token> tokenList = new List<Token>();
      int index = token != null ? this.FindToken(token) + 1 : 0;
      int num = int.MaxValue;
      for (; index < this._tokens.Count; ++index)
      {
        Token token1 = this._tokens[index];
        if (token1.StartLineNumber <= num)
        {
          if (token1.Is(TokenId.CommentToken))
          {
            tokenList.Add(token1);
            num = token1.LineNumber + 1;
          }
          else if (!token1.Is(TokenId.NewlineToken) && !token1.Is(TokenId.PositionToken))
            break;
        }
        else
          break;
      }
      return tokenList;
    }

    internal List<Token> GetPrecedingComments(Token token)
    {
      List<Token> tokenList = new List<Token>();
      int index;
      int num;
      if (token != null)
      {
        index = this.FindToken(token) - 1;
        num = token.StartLineNumber - 2;
      }
      else
      {
        index = this._tokens.Count - 1;
        num = 0;
      }
      for (; index >= 0; --index)
      {
        Token token1 = this._tokens[index];
        if (token1.LineNumber >= num)
        {
          if (token1.Is(TokenId.CommentToken))
          {
            tokenList.Add(token1);
            num = token1.StartLineNumber - 1;
          }
          else if (!token1.Is(TokenId.NewlineToken) && !token1.Is(TokenId.PositionToken))
            break;
        }
        else
          break;
      }
      tokenList.Reverse();
      return tokenList;
    }

    private void SkipNewlinesAndMaybeSemicolons(bool skipSemis)
    {
      while (this._ungotTokens.Count > 0)
      {
        Token ungotToken = this._ungotTokens[this._ungotTokens.Count - 1];
        if (ungotToken.Is(TokenId.NewlineToken) || skipSemis && ungotToken.Is(TokenId.SemicolonToken))
        {
          this._ungotTokens.RemoveAt(this._ungotTokens.Count - 1);
          this._tokens.Add(ungotToken);
        }
        else
          break;
      }
      if (this._ungotTokens.Count != 0)
        return;
      while (true)
      {
        this.FindNextToken();
        Token newLineToken;
        if ((newLineToken = this.GetNewLineToken()) != null)
          this._tokens.Add(newLineToken);
        else if (skipSemis && this._offset < this._script.Length && this._script[this._offset] == ';')
          this.Next();
        else
          break;
      }
    }

    public void AddLineStart(int offset)
    {
      if (this._lineStartOffsets[this._lineStartOffsets.Count - 1] >= offset)
        return;
      this.AddLineStartNoCheck(offset);
    }

    private void AddLineStartNoCheck(int offset)
    {
      int startIndex = offset;
      this._lineStartOffsets.Add(offset);
      while (offset < this._script.Length && this._script[offset] != '\r' && this._script[offset] != '\n')
        ++offset;
      this._scriptLines.Add(this._script.Substring(startIndex, offset - startIndex));
    }

    internal int GetLineNumberForOffset(int offset)
    {
      int num = this._lineStartOffsets.BinarySearch(offset);
      if (num < 0)
        num = ~num - 1;
      return num + this._startLineNumber;
    }

    internal int GetOffsetWithinLine(int line, int scriptOffset) => scriptOffset - this._lineStartOffsets[line - this._startLineNumber];

    internal string GetLine(int line) => this._scriptLines[line - this._startLineNumber];

    bool IStringTokenReaderHelper.InteractiveInput => this._parser.InteractiveInput;

    void IStringTokenReaderHelper.ReportMissingTerminator(
      string str,
      int startOffset,
      int errorOffset,
      string errorId,
      Token token,
      string terminator)
    {
      StringTokenReader.ReportMissingTerminator(this._parser, str, startOffset, errorOffset, errorId, terminator);
    }

    void IStringTokenReaderHelper.ReportError(
      string str,
      int offset,
      Token token,
      string errorId)
    {
      StringTokenReader.ReportError(this._parser, str, offset, errorId);
    }

    internal List<Token> Tokens => this._tokens;

    internal void UngetToken(Token token)
    {
      if (token == null)
        return;
      bool flag = false;
      this._ungotTokens.Add(token);
      while (token != this._tokens[this._tokens.Count - 1])
      {
        flag = true;
        this._tokens.RemoveAt(this._tokens.Count - 1);
      }
      this._tokens.RemoveAt(this._tokens.Count - 1);
      if (!flag)
        return;
      this.Resync();
    }

    private Token GetTokenFromBuffer()
    {
      if (this._ungotTokens.Count <= 0)
        return (Token) null;
      Token ungotToken = this._ungotTokens[this._ungotTokens.Count - 1];
      this._ungotTokens.RemoveAt(this._ungotTokens.Count - 1);
      return ungotToken;
    }

    internal void Resync()
    {
      if (this._ungotTokens.Count <= 0)
        return;
      this._offset = this._ungotTokens[this._ungotTokens.Count - 1].Start;
      this._ungotTokens.Clear();
    }

    internal Token BogusToken(string text, TokenId id)
    {
      Token token = new Token(text, id);
      token.SetPosition(this._file, this._script, this._offset, this._offset, this);
      return token;
    }

    internal Token PositionToken()
    {
      int position = this._offset;
      if (this._ungotTokens.Count > 0)
        position = this._ungotTokens[this._ungotTokens.Count - 1].Start;
      return this.PositionToken(position);
    }

    internal Token PositionToken(int position)
    {
      int offset = this._offset;
      if (position > this._script.Length)
        position = this._script.Length;
      if (position < 0)
        position = 0;
      this._offset = position;
      Token token = this.BogusToken("<position>", TokenId.PositionToken);
      this._offset = offset;
      return token;
    }
  }
}
