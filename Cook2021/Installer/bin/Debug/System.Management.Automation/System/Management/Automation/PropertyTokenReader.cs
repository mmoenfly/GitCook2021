// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PropertyTokenReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal sealed class PropertyTokenReader : TokenReader
  {
    private TokenId _id;
    [TraceSource("Tokenizer", "Tokenizer")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");

    public PropertyTokenReader(Tokenizer tokenizer, TokenId id)
      : base(tokenizer)
      => this._id = id;

    internal override TokenClass TokenClass => TokenClass.Property;

    internal override Token GetToken(string input, ref int offset)
    {
      string text = this.Match(input, offset);
      if (text == null)
        return (Token) null;
      offset += text.Length;
      Token token = new Token(text, this._id);
      if (this._id == TokenId.ParameterToken)
        token.Data = text[text.Length - 1] != ':' ? (object) text.Substring(1, text.Length - 1) : (object) text.Substring(1, text.Length - 2);
      return token;
    }

    private string Match(string input, int offset)
    {
      int offset1 = offset;
      if (!this.MatchPrefix(input, ref offset1))
        return (string) null;
      for (; offset1 < input.Length; ++offset1)
      {
        char c = input[offset1];
        if (this._id == TokenId.ParameterToken)
        {
          if (c == ':')
          {
            ++offset1;
            break;
          }
          if (":.[{}();,|&\r\n\t ".IndexOf(c) != -1)
            break;
        }
        else if (!char.IsLetterOrDigit(c) && c != '_')
          break;
      }
      return input.Substring(offset, offset1 - offset);
    }

    private bool MatchPrefix(string input, ref int offset)
    {
      if (offset >= input.Length)
        return false;
      switch (this._id)
      {
        case TokenId.ParameterToken:
          if (!SpecialCharacters.IsDash(input[offset]) || offset + 1 < input.Length && SpecialCharacters.IsDash(input[offset + 1]))
            return false;
          ++offset;
          break;
        case TokenId.LoopLabelToken:
          if (input[offset] != ':')
            return false;
          ++offset;
          break;
      }
      if (offset >= input.Length)
        return false;
      char c = input[offset];
      return c == '?' && this._id == TokenId.ParameterToken || (char.IsLetter(c) || c == '_');
    }
  }
}
