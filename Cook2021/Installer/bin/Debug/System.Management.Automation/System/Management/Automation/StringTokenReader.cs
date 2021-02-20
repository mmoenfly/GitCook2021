// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.StringTokenReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
  internal sealed class StringTokenReader : TokenReader
  {
    private TokenClass _tokenClass;
    [TraceSource("Tokenizer", "Tokenizer")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Tokenizer", "Tokenizer");

    internal StringTokenReader(Tokenizer tokenizer, TokenClass tokenClass)
      : base(tokenizer)
      => this._tokenClass = tokenClass;

    internal override TokenClass TokenClass => this._tokenClass;

    internal override Token GetToken(string input, ref int offset)
    {
      string literalString = (string) null;
      switch (this._tokenClass)
      {
        case TokenClass.String:
          string text1 = this.MatchString(input, ref offset, out literalString);
          if (text1 == null)
            return (Token) null;
          return literalString != null ? new Token(literalString, TokenId.LiteralStringToken) : new Token(text1, TokenId.ExpandableStringToken);
        case TokenClass.Argument:
          string text2 = this.MatchArgument(input, ref offset, out literalString);
          if (text2 == null)
            return (Token) null;
          return literalString != null ? new Token(literalString, TokenId.LiteralCommandArgumentToken) : new Token(text2, TokenId.ExpandableCommandArgumentToken);
        case TokenClass.Command:
          string text3 = this.MatchArgument(input, ref offset, out literalString);
          return text3 == null ? (Token) null : new Token(text3, TokenId.CmdletNameToken);
        default:
          return (Token) null;
      }
    }

    private string MatchString(string input, ref int offset, out string literalString)
    {
      int index = offset;
      bool hereString = false;
      literalString = (string) null;
      if (input[index] == '@')
      {
        if (++index >= input.Length)
          return (string) null;
        hereString = true;
      }
      if (SpecialCharacters.IsSingleQuote(input[index]))
        return StringTokenReader.MatchSingleQuotedString(input, ref offset, hereString, out literalString, (IStringTokenReaderHelper) this._tokenizer, (Token) null);
      return SpecialCharacters.IsDoubleQuote(input[index]) ? StringTokenReader.MatchDoubleQuotedString(input, ref offset, hereString, out literalString, (IStringTokenReaderHelper) this._tokenizer, (Token) null) : (string) null;
    }

    private static void CheckForNewline(
      string input,
      char currentChar,
      int offset,
      IStringTokenReaderHelper helper)
    {
      switch (currentChar)
      {
        case '\n':
          helper.AddLineStart(offset);
          break;
        case '\r':
          if (offset >= input.Length || input[offset] == '\n')
            break;
          helper.AddLineStart(offset);
          break;
      }
    }

    internal static string MatchSingleQuotedString(
      string input,
      ref int offset,
      bool hereString,
      out string literalString,
      IStringTokenReaderHelper helper,
      Token token)
    {
      int startOffset = offset;
      literalString = (string) null;
      if (hereString)
      {
        if (!StringTokenReader.IsHereStringHeader(input, ref offset, true, helper))
          return (string) null;
      }
      else
      {
        if (!SpecialCharacters.IsSingleQuote(input[offset]))
          return (string) null;
        ++offset;
      }
      StringBuilder sb1 = new StringBuilder();
      StringBuilder sb2 = new StringBuilder();
      int footerOffset = 0;
      while (offset < input.Length)
      {
        char ch = input[offset++];
        if (SpecialCharacters.IsSingleQuote(ch))
        {
          if (!hereString && offset < input.Length && SpecialCharacters.IsSingleQuote(input[offset]))
          {
            sb2.Append(input[offset]);
            sb1.Append(ch);
            sb1.Append(input[offset]);
            ++offset;
            continue;
          }
          if (!hereString || StringTokenReader.IsHereStringFooter(input, offset - 1, true, out footerOffset))
          {
            if (hereString)
            {
              StringTokenReader.CompleteHereString(sb2, footerOffset);
              StringTokenReader.CompleteHereString(sb1, footerOffset);
              ++offset;
            }
            literalString = sb2.ToString();
            if (literalString == null)
              literalString = "";
            return sb1.ToString() ?? "";
          }
        }
        else
          StringTokenReader.CheckForNewline(input, ch, offset, helper);
        sb2.Append(ch);
        sb1.Append(ch);
      }
      string errorId = "TerminatorExpectedAtEndOfString";
      int errorOffset = offset;
      if (footerOffset != 0)
      {
        errorId = "WhitespaceBeforeHereStringFooter";
        if (!helper.InteractiveInput)
          errorOffset = footerOffset;
      }
      helper.SetToEndOfInput();
      helper.ReportMissingTerminator(sb1.ToString(), startOffset, errorOffset, errorId, token, hereString ? "'@" : "'");
      return (string) null;
    }

    internal static string MatchDoubleQuotedString(
      string input,
      ref int offset,
      bool hereString,
      out string literalString,
      IStringTokenReaderHelper helper,
      Token token)
    {
      int startOffset = offset;
      bool flag = true;
      bool inBraces = false;
      literalString = (string) null;
      if (hereString)
      {
        if (!StringTokenReader.IsHereStringHeader(input, ref offset, false, helper))
          return (string) null;
      }
      else
      {
        if (!SpecialCharacters.IsDoubleQuote(input[offset]))
          return (string) null;
        ++offset;
      }
      StringBuilder sb1 = new StringBuilder();
      StringBuilder sb2 = new StringBuilder();
      int footerOffset = 0;
      while (offset < input.Length)
      {
        char ch = input[offset++];
        if (SpecialCharacters.IsDoubleQuote(ch))
        {
          if (!hereString && offset < input.Length && SpecialCharacters.IsDoubleQuote(input[offset]))
          {
            sb2.Append(input[offset]);
            sb1.Append("`");
            sb1.Append(input[offset]);
            ++offset;
            continue;
          }
          if (!hereString || StringTokenReader.IsHereStringFooter(input, offset - 1, false, out footerOffset))
          {
            if (hereString)
            {
              StringTokenReader.CompleteHereString(sb2, footerOffset);
              StringTokenReader.CompleteHereString(sb1, footerOffset);
              ++offset;
            }
            if (flag)
            {
              literalString = sb2.ToString();
              if (literalString == null)
                literalString = "";
            }
            return sb1.ToString() ?? "";
          }
          sb1.Append("`");
        }
        switch (ch)
        {
          case '$':
            flag = false;
            sb1.Append(ch);
            if (offset < input.Length)
            {
              switch (input[offset])
              {
                case '(':
                  sb1.Append('(');
                  sb1.Append(StringTokenReader.MatchSubExpression(input, ref offset, !hereString, helper, token));
                  sb1.Append(')');
                  continue;
                case '{':
                  sb1.Append('{');
                  sb1.Append(StringTokenReader.MatchBracedVariableName(input, ref offset, false, !hereString, helper, token, out inBraces));
                  sb1.Append('}');
                  continue;
                default:
                  continue;
              }
            }
            else
              continue;
          case '`':
            if (offset < input.Length)
            {
              sb2.Append(StringTokenReader.Backtick(input[offset]));
              sb1.Append('`');
              sb1.Append(input[offset++]);
              continue;
            }
            goto label_30;
          default:
            if (SpecialCharacters.IsSingleQuote(ch))
              sb1.Append("`");
            else
              StringTokenReader.CheckForNewline(input, ch, offset, helper);
            sb2.Append(ch);
            sb1.Append(ch);
            continue;
        }
      }
label_30:
      string errorId = "TerminatorExpectedAtEndOfString";
      int errorOffset = offset;
      if (footerOffset != 0)
      {
        errorId = "WhitespaceBeforeHereStringFooter";
        if (!helper.InteractiveInput)
          errorOffset = footerOffset;
      }
      helper.SetToEndOfInput();
      helper.ReportMissingTerminator(sb1.ToString(), startOffset, errorOffset, errorId, token, hereString ? "\"@" : "\"");
      return (string) null;
    }

    private string MatchArgument(string input, ref int offset, out string literalString) => StringTokenReader.MatchArgumentString(input, ref offset, out literalString, (IStringTokenReaderHelper) this._tokenizer, (Token) null);

    internal static string MatchArgumentString(
      string input,
      ref int offset,
      out string literalString,
      IStringTokenReaderHelper helper,
      Token token)
    {
      char ch = input[offset];
      int num = offset;
      literalString = (string) null;
      if ("{}()@#;,|&\r\n\t ".IndexOf(ch) != -1)
        return (string) null;
      if (ch == '.' && offset + 1 < input.Length)
      {
        char c = input[offset + 1];
        if (SpecialCharacters.IsQuote(c) || c == '$')
        {
          ++offset;
          literalString = ".";
          return ".";
        }
      }
      StringBuilder stringBuilder1 = new StringBuilder();
      StringBuilder stringBuilder2 = new StringBuilder();
      bool flag = true;
      bool inBraces = false;
      while (offset < input.Length)
      {
        char c = input[offset];
        if ("{}();,|&\r\n\t ".IndexOf(c) == -1)
        {
          if (c == '`')
          {
            ++offset;
            if (offset < input.Length)
            {
              stringBuilder2.Append(StringTokenReader.Backtick(input[offset]));
              stringBuilder1.Append('`');
              stringBuilder1.Append(input[offset++]);
            }
            else
              break;
          }
          else if (SpecialCharacters.IsSingleQuote(c))
          {
            string literalString1 = (string) null;
            string str = StringTokenReader.MatchSingleQuotedString(input, ref offset, false, out literalString1, helper, token);
            if (flag)
              stringBuilder2.Append(literalString1);
            stringBuilder1.Append('\'');
            stringBuilder1.Append(str);
            stringBuilder1.Append('\'');
          }
          else if (SpecialCharacters.IsDoubleQuote(c))
          {
            string literalString1 = (string) null;
            string str = StringTokenReader.MatchDoubleQuotedString(input, ref offset, false, out literalString1, helper, token);
            if (literalString1 != null && flag)
              stringBuilder2.Append(literalString1);
            else
              flag = false;
            stringBuilder1.Append('"');
            stringBuilder1.Append(str);
            stringBuilder1.Append('"');
          }
          else if (c == '$')
          {
            flag = false;
            stringBuilder1.Append(c);
            ++offset;
            if (offset < input.Length)
            {
              switch (input[offset])
              {
                case '(':
                  stringBuilder1.Append('(');
                  stringBuilder1.Append(StringTokenReader.MatchSubExpression(input, ref offset, false, helper, token));
                  stringBuilder1.Append(')');
                  continue;
                case '{':
                  stringBuilder1.Append('{');
                  stringBuilder1.Append(StringTokenReader.MatchBracedVariableName(input, ref offset, false, false, helper, token, out inBraces));
                  stringBuilder1.Append('}');
                  continue;
                default:
                  continue;
              }
            }
          }
          else
          {
            stringBuilder1.Append(c);
            stringBuilder2.Append(c);
            ++offset;
          }
        }
        else
          break;
      }
      if (offset == num)
        return (string) null;
      if (flag)
      {
        literalString = stringBuilder2.ToString();
        if (literalString == null)
          literalString = "";
      }
      return stringBuilder1.ToString();
    }

    internal static string MatchVariableName(
      string input,
      ref int offset,
      IStringTokenReaderHelper helper,
      Token token,
      out bool inBraces)
    {
      inBraces = false;
      if (offset >= input.Length)
        return (string) null;
      if (input[offset] == '{')
        return StringTokenReader.MatchBracedVariableName(input, ref offset, true, false, helper, token, out inBraces);
      StringBuilder stringBuilder = new StringBuilder();
      char ch = input[offset];
      switch (ch)
      {
        case '$':
        case '?':
        case '^':
          stringBuilder.Append(ch);
          ++offset;
          return stringBuilder.ToString();
        default:
          while (offset < input.Length)
          {
            char c = input[offset];
            if (!char.IsLetterOrDigit(c))
            {
              switch (c)
              {
                case ':':
                  if (offset + 1 >= input.Length || input[offset + 1] != ':')
                  {
                    ++offset;
                    stringBuilder.Append(c);
                    continue;
                  }
                  goto label_12;
                case '?':
                case '_':
                  break;
                default:
                  goto label_12;
              }
            }
            ++offset;
            stringBuilder.Append(c);
          }
label_12:
          return stringBuilder.ToString();
      }
    }

    internal static string MatchBracedVariableName(
      string input,
      ref int offset,
      bool backticking,
      bool doubleQuoted,
      IStringTokenReaderHelper helper,
      Token token,
      out bool inBraces)
    {
      int startOffset = offset;
      inBraces = false;
      if (input[offset] != '{')
        return (string) null;
      inBraces = true;
      ++offset;
      StringBuilder stringBuilder = new StringBuilder();
      while (offset < input.Length)
      {
        char c = input[offset++];
        switch (c)
        {
          case '`':
            if (offset < input.Length)
            {
              if (backticking)
              {
                stringBuilder.Append(StringTokenReader.Backtick(input[offset++]));
                continue;
              }
              stringBuilder.Append('`');
              stringBuilder.Append(input[offset++]);
              continue;
            }
            goto label_15;
          case '}':
            return stringBuilder.ToString();
          default:
            if (doubleQuoted && SpecialCharacters.IsDoubleQuote(c) && (offset < input.Length && SpecialCharacters.IsDoubleQuote(input[offset])))
              ++offset;
            if (c == '{')
              helper.ReportError(stringBuilder.ToString(), offset, token, "OpenBraceNeedsToBeBackTickedInVariableName");
            stringBuilder.Append(c);
            continue;
        }
      }
label_15:
      helper.SetToEndOfInput();
      helper.ReportMissingTerminator(stringBuilder.ToString(), startOffset, offset, "IncompleteDollarVariableReference", token, "");
      return (string) null;
    }

    internal static string MatchSubExpression(
      string input,
      ref int offset,
      bool doubleQuoted,
      IStringTokenReaderHelper helper,
      Token token)
    {
      int startOffset = offset;
      if (input[offset] != '(')
        return (string) null;
      int num = 1;
      ++offset;
      StringBuilder stringBuilder = new StringBuilder();
      while (offset < input.Length)
      {
        char ch = input[offset++];
        if (ch == ')')
        {
          --num;
          if (num == 0)
            return stringBuilder.ToString() ?? "";
        }
        if (ch == '(')
          ++num;
        if (ch == '`')
        {
          if (offset < input.Length)
          {
            if (!doubleQuoted || !SpecialCharacters.IsDoubleQuote(input[offset]))
              stringBuilder.Append(ch);
            stringBuilder.Append(input[offset++]);
          }
          else
            break;
        }
        else
        {
          if (doubleQuoted && SpecialCharacters.IsDoubleQuote(ch))
          {
            if (offset < input.Length && SpecialCharacters.IsDoubleQuote(input[offset]))
              ++offset;
          }
          else
            StringTokenReader.CheckForNewline(input, ch, offset, helper);
          stringBuilder.Append(ch);
        }
      }
      helper.SetToEndOfInput();
      helper.ReportMissingTerminator(stringBuilder.ToString(), startOffset, offset, "IncompleteDollarSubexpressionReference", token, "");
      return (string) null;
    }

    private static bool IsHereStringHeader(
      string input,
      ref int offset,
      bool singleQuote,
      IStringTokenReaderHelper helper)
    {
      if (offset + 1 >= input.Length || input[offset] != '@' || singleQuote && !SpecialCharacters.IsSingleQuote(input[offset + 1]) || !singleQuote && !SpecialCharacters.IsDoubleQuote(input[offset + 1]))
        return false;
      int num = 0;
      while (offset + 2 + num < input.Length && Tokenizer.IsWhiteSpace(input[offset + 2 + num]))
        ++num;
      if (offset + 2 + num >= input.Length)
      {
        offset += 2;
        return true;
      }
      if (input[offset + 2 + num] == '\n')
      {
        offset += 3 + num;
        helper.AddLineStart(offset);
        return true;
      }
      if (input[offset + 2 + num] != '\r')
        return false;
      if (offset + 3 + num < input.Length && input[offset + 3 + num] == '\n')
        offset += 4 + num;
      else
        offset += 3 + num;
      helper.AddLineStart(offset);
      return true;
    }

    private static bool IsHereStringFooter(
      string input,
      int offset,
      bool singleQuote,
      out int footerOffset)
    {
      footerOffset = 0;
      if (offset + 1 >= input.Length || offset <= 0 || singleQuote && !SpecialCharacters.IsSingleQuote(input[offset]) || (!singleQuote && !SpecialCharacters.IsDoubleQuote(input[offset]) || input[offset + 1] != '@'))
        return false;
      while (Tokenizer.IsWhiteSpace(input[offset - 1 - footerOffset]))
        ++footerOffset;
      if (input[offset - 1 - footerOffset] != '\n' && input[offset - 1 - footerOffset] != '\r')
      {
        footerOffset = 0;
        return false;
      }
      if (footerOffset == 0)
        return true;
      footerOffset = offset;
      return false;
    }

    private static void CompleteHereString(StringBuilder sb, int footerOffset)
    {
      if (sb.Length <= 0)
        return;
      if (sb[sb.Length - 1] == '\n')
      {
        sb.Remove(sb.Length - 1, 1);
        if (sb.Length <= 0 || sb[sb.Length - 1] != '\r')
          return;
        sb.Remove(sb.Length - 1, 1);
      }
      else
      {
        if (sb[sb.Length - 1] != '\r')
          return;
        sb.Remove(sb.Length - 1, 1);
      }
    }

    internal static char Backtick(char input)
    {
      switch (input)
      {
        case '0':
          return char.MinValue;
        case 'a':
          return '\a';
        case 'b':
          return '\b';
        case 'f':
          return '\f';
        case 'n':
          return '\n';
        case 'r':
          return '\r';
        case 't':
          return '\t';
        case 'v':
          return '\v';
        default:
          return input;
      }
    }

    internal static void ReportMissingTerminator(
      Parser parser,
      string str,
      int startOffset,
      int errorOffset,
      string errorId,
      string terminator)
    {
      parser.ReportException((object) str, typeof (IncompleteParseException), parser.Tokenizer.PositionToken(errorOffset), errorId, (object) parser.Tokenizer.PositionToken(startOffset).Position(), (object) terminator);
    }

    internal static void ReportError(Parser parser, string str, int offset, string errorId) => parser.ReportException((object) str, typeof (ParseException), parser.Tokenizer.PositionToken(offset), errorId);

    private static void AppendLiteralCharacter(StringBuilder formatString, char c)
    {
      if (formatString == null)
        throw StringTokenReader.tracer.NewArgumentNullException(nameof (formatString));
      formatString.Append(c);
      if (c != '{' && c != '}')
        return;
      formatString.Append(c);
    }

    private static void AppendLiteralString(StringBuilder formatString, string s)
    {
      if (formatString == null)
        throw StringTokenReader.tracer.NewArgumentNullException(nameof (formatString));
      if (s == null)
        return;
      foreach (char c in s)
        StringTokenReader.AppendLiteralCharacter(formatString, c);
    }

    private static void AppendExpression(
      StringBuilder formatString,
      List<ParseTreeNode> expressionsList,
      ParseTreeNode expression)
    {
      if (formatString == null)
        throw StringTokenReader.tracer.NewArgumentNullException(nameof (formatString));
      if (expressionsList == null)
        throw StringTokenReader.tracer.NewArgumentNullException(nameof (expressionsList));
      if (expression == null)
        throw StringTokenReader.tracer.NewArgumentNullException(nameof (expression));
      formatString.AppendFormat("{{{0}}}", (object) expressionsList.Count);
      Token token = new Token("string", TokenId.TypeToken);
      if (expression.NodeToken != null)
        token.SetPosition(expression.NodeToken);
      ParseTreeNode parseTreeNode = (ParseTreeNode) new UnaryPrefixPostFixNode(expression, new List<Token>(1)
      {
        token
      }, (Token) null);
      expressionsList.Add(parseTreeNode);
    }

    internal static ParseTreeNode ExpandStringToFormatExpression(
      IStringTokenReaderHelper2 helper,
      Token token,
      string inString)
    {
      if (string.IsNullOrEmpty(inString))
        return (ParseTreeNode) new ConstantNode(token, (object) "");
      StringBuilder formatString1 = new StringBuilder();
      List<ParseTreeNode> expressionsList = new List<ParseTreeNode>();
      int offset = 0;
      while (offset < inString.Length)
      {
        char ch = inString[offset];
        switch (ch)
        {
          case '$':
            int num1 = offset;
            ++offset;
            if (offset >= inString.Length)
            {
              StringTokenReader.AppendLiteralCharacter(formatString1, '$');
              goto label_31;
            }
            else
            {
              if (inString[offset] == '(')
              {
                int currentLineNumber = helper.CurrentLineNumber;
                string input = StringTokenReader.MatchSubExpression(inString, ref offset, false, (IStringTokenReaderHelper) helper, token);
                if (string.IsNullOrEmpty(input))
                {
                  StringTokenReader.AppendLiteralCharacter(formatString1, '$');
                  continue;
                }
                Parser parser = new Parser();
                parser.AccumulateErrors = helper.ParentParser.AccumulateErrors;
                parser.Tokenizer.File = helper.ParentParser.Tokenizer.File;
                parser.Tokenizer.StartLineNumber = currentLineNumber;
                ParseTreeNode expression = parser.Parse(input, 0);
                if (parser.AccumulateErrors)
                  helper.ParentParser.Errors.AddRange((IEnumerable<RuntimeException>) parser.Errors);
                StringTokenReader.AppendExpression(formatString1, expressionsList, expression);
                continue;
              }
              bool inBraces = false;
              string text = StringTokenReader.MatchVariableName(inString, ref offset, (IStringTokenReaderHelper) helper, token, out inBraces);
              if (string.IsNullOrEmpty(text))
              {
                if (inBraces)
                {
                  ParseTreeNode expression = (ParseTreeNode) new EmptyBracedVariableNode(token);
                  StringTokenReader.AppendExpression(formatString1, expressionsList, expression);
                  continue;
                }
                StringTokenReader.AppendLiteralCharacter(formatString1, '$');
                continue;
              }
              Token var = new Token(text, TokenId.VariableToken);
              if (token != null)
                var.SetPosition(token.File, token.Script, token.Start + num1, token.Start + offset, helper.ParentParser.Tokenizer);
              ParseTreeNode expression1 = (ParseTreeNode) new VariableDereferenceNode(var, (List<TypeLiteral>) null, true);
              StringTokenReader.AppendExpression(formatString1, expressionsList, expression1);
              continue;
            }
          case '`':
            int num2 = offset + 1;
            if (num2 < inString.Length)
            {
              StringBuilder formatString2 = formatString1;
              string str = inString;
              int index = num2;
              offset = index + 1;
              int num3 = (int) StringTokenReader.Backtick(str[index]);
              StringTokenReader.AppendLiteralCharacter(formatString2, (char) num3);
              continue;
            }
            goto label_31;
          default:
            if (SpecialCharacters.IsSingleQuote(ch))
            {
              string literalString = (string) null;
              StringTokenReader.MatchSingleQuotedString(inString, ref offset, false, out literalString, (IStringTokenReaderHelper) helper, token);
              StringTokenReader.AppendLiteralString(formatString1, literalString);
              continue;
            }
            if (SpecialCharacters.IsDoubleQuote(ch))
            {
              int num3 = offset;
              string literalString = (string) null;
              string str = StringTokenReader.MatchDoubleQuotedString(inString, ref offset, false, out literalString, (IStringTokenReaderHelper) helper, token);
              if (literalString != null)
              {
                StringTokenReader.AppendLiteralString(formatString1, literalString);
                continue;
              }
              Token token1 = new Token(str, TokenId.ExpandableStringToken);
              if (token != null)
                token1.SetPosition(token.File, token.Script, token.Start + num3, token.Start + offset, helper.ParentParser.Tokenizer);
              ParseTreeNode formatExpression = StringTokenReader.ExpandStringToFormatExpression(helper, token1, str);
              StringTokenReader.AppendExpression(formatString1, expressionsList, formatExpression);
              continue;
            }
            ++offset;
            StringTokenReader.CheckForNewline(inString, ch, offset, (IStringTokenReaderHelper) helper);
            StringTokenReader.AppendLiteralCharacter(formatString1, ch);
            continue;
        }
      }
label_31:
      ArrayLiteralNode arrayLiteralNode = new ArrayLiteralNode(token, (IList<ParseTreeNode>) expressionsList);
      Token token2 = new Token(formatString1.ToString(), TokenId.LiteralStringToken);
      if (token != null)
        token2.SetPosition(token);
      LiteralStringNode literalStringNode = new LiteralStringNode(token2);
      OperatorToken operatorToken = OperatorTokenReader.GetOperatorToken("-f");
      if (token != null)
        operatorToken.SetPosition(token);
      ExpressionNode expressionNode = new ExpressionNode();
      expressionNode.Add((ParseTreeNode) literalStringNode);
      expressionNode.Add(operatorToken, (ParseTreeNode) arrayLiteralNode);
      expressionNode.Complete(helper.ParentParser.InDataSection);
      return (ParseTreeNode) expressionNode;
    }
  }
}
