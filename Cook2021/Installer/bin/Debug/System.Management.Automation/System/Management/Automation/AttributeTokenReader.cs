// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AttributeTokenReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Text;

namespace System.Management.Automation
{
  internal sealed class AttributeTokenReader : TokenReader
  {
    [TraceSource("Tokenizer", "Tokenizer")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Tokenizer", "Tokenizer");

    internal AttributeTokenReader(Tokenizer tokenizer)
      : base(tokenizer)
    {
    }

    internal override TokenClass TokenClass => TokenClass.Attribute;

    internal override Token GetToken(string input, ref int offset)
    {
      string text = this.MatchAttribute(input, ref offset);
      return text == null ? (Token) null : new Token(text, TokenId.AttributeToken);
    }

    private string MatchAttribute(string input, ref int offset)
    {
      int num = 1;
      StringBuilder stringBuilder = new StringBuilder();
      while (offset < input.Length)
      {
        char ch = input[offset++];
        if (ch == '\n' || ch == '\r')
        {
          --offset;
          break;
        }
        if ((ch == ']' || ch == '(') && num == 1)
        {
          --offset;
          return stringBuilder.ToString() ?? "";
        }
        if (ch == ']')
          --num;
        if (ch == '[')
          ++num;
        if (ch == '`')
        {
          if (offset < input.Length)
            stringBuilder.Append(StringTokenReader.Backtick(input[offset++]));
          else
            break;
        }
        else
          stringBuilder.Append(ch);
      }
      this.Tokenizer.Parser.ReportException((object) stringBuilder.ToString(), typeof (IncompleteParseException), this.Tokenizer.PositionToken(offset), "EndSquareBracketExpectedAtEndOfAttribute");
      return (string) null;
    }
  }
}
