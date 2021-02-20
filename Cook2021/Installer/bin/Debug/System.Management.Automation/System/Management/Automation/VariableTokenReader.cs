// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.VariableTokenReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal sealed class VariableTokenReader : TokenReader
  {
    internal VariableTokenReader(Tokenizer tokenizer)
      : base(tokenizer)
    {
    }

    internal override TokenClass TokenClass => TokenClass.Variable;

    internal override Token GetToken(string input, ref int offset)
    {
      if (offset >= input.Length)
        return (Token) null;
      char ch = input[offset];
      switch (ch)
      {
        case '$':
        case '@':
          int offset1 = offset + 1;
          if (offset1 >= input.Length)
            return (Token) null;
          if (ch == '@' && input[offset1] == '{' || input[offset1] == '(')
            return (Token) null;
          bool inBraces = false;
          string text = StringTokenReader.MatchVariableName(input, ref offset1, (IStringTokenReaderHelper) this.Tokenizer, (Token) null, out inBraces);
          if (string.IsNullOrEmpty(text))
          {
            if (inBraces)
              this._tokenizer.Parser.ReportException((object) null, typeof (ParseException), this._tokenizer.PositionToken(offset), "EmptyVariableReference");
            return (Token) null;
          }
          if (offset1 < input.Length && this.Tokenizer.IsEnabled(8) && ":.[{}();,|&\r\n\t ".IndexOf(input[offset1]) == -1)
            return (Token) null;
          offset = offset1;
          return new Token(text, ch == '@' ? TokenId.SplattedVariableToken : TokenId.VariableToken);
        default:
          return (Token) null;
      }
    }
  }
}
