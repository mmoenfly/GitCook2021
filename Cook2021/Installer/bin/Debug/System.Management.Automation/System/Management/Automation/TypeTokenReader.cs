// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TypeTokenReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Text;

namespace System.Management.Automation
{
  internal sealed class TypeTokenReader : TokenReader
  {
    [TraceSource("Tokenizer", "Tokenizer")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Tokenizer", "Tokenizer");

    internal TypeTokenReader(Tokenizer tokenizer)
      : base(tokenizer)
    {
    }

    internal override TokenClass TokenClass => TokenClass.Type;

    internal override Token GetToken(string input, ref int offset)
    {
      if (input[offset] != '[')
        return (Token) null;
      ++offset;
      string text = TypeTokenReader.MatchType(input, ref offset, this.Tokenizer, false);
      return text == null ? (Token) null : new Token(text, TokenId.TypeToken);
    }

    internal static string MatchType(
      string input,
      ref int offset,
      Tokenizer tokenizer,
      bool commaTerminates)
    {
      int num = 1;
      bool flag = false;
      StringBuilder stringBuilder = new StringBuilder();
      while (offset < input.Length)
      {
        char ch = input[offset++];
        if (ch == '\n' || ch == '\r')
        {
          --offset;
          break;
        }
        if (ch == ']')
        {
          --num;
          if (num == 0)
          {
            flag = true;
            break;
          }
        }
        else if (ch == ',' && commaTerminates && num == 1)
        {
          flag = true;
          break;
        }
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
      if (flag)
        return stringBuilder.ToString() ?? "";
      tokenizer?.Parser.ReportException((object) stringBuilder.ToString(), typeof (IncompleteParseException), tokenizer.PositionToken(offset), "EndSquareBracketExpectedAtEndOfType");
      return (string) null;
    }
  }
}
