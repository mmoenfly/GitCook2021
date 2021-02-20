// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.OperatorTokenReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Text;

namespace System.Management.Automation
{
  internal sealed class OperatorTokenReader : TokenReader
  {
    private OperatorTokenReaderType _type;
    private static Hashtable _generalOperatorTokens = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
    private static readonly string[] RedirectionOperators = new string[7]
    {
      "2>&1",
      "1>&2",
      ">>",
      ">",
      "<",
      "2>>",
      "2>"
    };
    private static Hashtable _referenceOperatorTokens = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
    private static Hashtable _attributeOperatorTokens = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
    private static Hashtable _expressionOperatorTokens = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
    private static readonly string[] ComparisonOperators = new string[17]
    {
      "-eq",
      "-ne",
      "-ge",
      "-gt",
      "-lt",
      "-le",
      "-like",
      "-notlike",
      "-match",
      "-notmatch",
      "-replace",
      "-contains",
      "-notcontains",
      "-split",
      "-isnot",
      "-is",
      "-as"
    };
    private static readonly string[] CaseInsensitiveComparisonOperators = new string[14]
    {
      "-ieq",
      "-ine",
      "-ige",
      "-igt",
      "-ilt",
      "-ile",
      "-ilike",
      "-inotlike",
      "-imatch",
      "-inotmatch",
      "-ireplace",
      "-icontains",
      "-inotcontains",
      "-isplit"
    };
    private static readonly string[] CaseSensitiveComparisonOperators = new string[14]
    {
      "-ceq",
      "-cne",
      "-cge",
      "-cgt",
      "-clt",
      "-cle",
      "-clike",
      "-cnotlike",
      "-cmatch",
      "-cnotmatch",
      "-creplace",
      "-ccontains",
      "-cnotcontains",
      "-csplit"
    };
    private static readonly string[] AssignmentOperators = new string[6]
    {
      "=",
      "+=",
      "-=",
      "*=",
      "/=",
      "%="
    };
    private static readonly string[] LogicalOperators = new string[3]
    {
      "-and",
      "-or",
      "-xor"
    };
    private static readonly string[] BitwiseOperators = new string[3]
    {
      "-band",
      "-bor",
      "-bxor"
    };
    [TraceSource("Tokenizer", "Tokenizer")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");

    static OperatorTokenReader()
    {
      OperatorTokenReader.InitializeExpressionOperatorTokens();
      OperatorTokenReader.InitializeReferenceOperatorTokens();
      OperatorTokenReader.InitializeAttributeOperatorTokens();
      OperatorTokenReader.InitializeGeneralOperatorTokens();
    }

    public OperatorTokenReader(Tokenizer tokenizer, OperatorTokenReaderType type)
      : base(tokenizer)
      => this._type = type;

    internal override TokenClass TokenClass => TokenClass.Operator;

    internal static OperatorToken GetOperatorToken(string op)
    {
      Hashtable[] hashtableArray = new Hashtable[4]
      {
        OperatorTokenReader._expressionOperatorTokens,
        OperatorTokenReader._referenceOperatorTokens,
        OperatorTokenReader._attributeOperatorTokens,
        OperatorTokenReader._generalOperatorTokens
      };
      foreach (Hashtable hashtable in hashtableArray)
      {
        object obj = hashtable[(object) op];
        if (obj != null)
          return (OperatorToken) ((Token) obj).Clone();
      }
      return (OperatorToken) null;
    }

    private Hashtable OperatorTokens
    {
      get
      {
        switch (this._type)
        {
          case OperatorTokenReaderType.General:
            return OperatorTokenReader._generalOperatorTokens;
          case OperatorTokenReaderType.Expression:
            return OperatorTokenReader._expressionOperatorTokens;
          case OperatorTokenReaderType.Reference:
            return OperatorTokenReader._referenceOperatorTokens;
          case OperatorTokenReaderType.Attribute:
            return OperatorTokenReader._attributeOperatorTokens;
          default:
            return (Hashtable) null;
        }
      }
    }

    private static void InitializeGeneralOperatorTokens()
    {
      OperatorTokenReader._generalOperatorTokens.Clear();
      OperatorTokenReader._generalOperatorTokens[(object) "@("] = (object) new OperatorToken("@(", TokenId.ArraySubexpressionToken);
      OperatorTokenReader._generalOperatorTokens[(object) "$("] = (object) new OperatorToken("$(", TokenId.SubexpressionToken);
      OperatorTokenReader._generalOperatorTokens[(object) "@{"] = (object) new OperatorToken("@{", TokenId.HashLiteralStartToken);
      OperatorTokenReader._generalOperatorTokens[(object) "("] = (object) new OperatorToken("(", TokenId.OpenParenToken);
      OperatorTokenReader._generalOperatorTokens[(object) ")"] = (object) new OperatorToken(")", TokenId.CloseParenToken);
      OperatorTokenReader._generalOperatorTokens[(object) "{"] = (object) new OperatorToken("{", TokenId.OpenBraceToken);
      OperatorTokenReader._generalOperatorTokens[(object) "}"] = (object) new OperatorToken("}", TokenId.CloseBraceToken);
      OperatorTokenReader._generalOperatorTokens[(object) ";"] = (object) new OperatorToken(";", TokenId.SemicolonToken);
      OperatorTokenReader._generalOperatorTokens[(object) "&&"] = (object) new OperatorToken("&&", TokenId.AndAndToken);
      OperatorTokenReader._generalOperatorTokens[(object) "||"] = (object) new OperatorToken("||", TokenId.OrOrToken);
      OperatorTokenReader._generalOperatorTokens[(object) "&"] = (object) new OperatorToken("&", TokenId.AmpersandToken);
      OperatorTokenReader._generalOperatorTokens[(object) "]"] = (object) new OperatorToken("]", TokenId.CloseSquareBracketToken);
      OperatorTokenReader._generalOperatorTokens[(object) "|"] = (object) new OperatorToken("|", TokenId.PipeToken);
      OperatorTokenReader._generalOperatorTokens[(object) ","] = (object) new OperatorToken(",", TokenId.CommaToken);
      OperatorTokenReader._generalOperatorTokens[(object) "--"] = (object) new OperatorToken("--", TokenId.MinusMinusToken);
      OperatorTokenReader._generalOperatorTokens[(object) ".."] = (object) new OperatorToken("..", TokenId.RangeOperatorToken, 7);
      foreach (string redirectionOperator in OperatorTokenReader.RedirectionOperators)
        OperatorTokenReader._generalOperatorTokens[(object) redirectionOperator] = (object) new OperatorToken(redirectionOperator, TokenId.RedirectionOperatorToken);
    }

    private static void InitializeReferenceOperatorTokens()
    {
      OperatorTokenReader._referenceOperatorTokens.Clear();
      OperatorTokenReader._referenceOperatorTokens[(object) "::"] = (object) new OperatorToken("::", TokenId.ReferenceOperatorToken);
      OperatorTokenReader._referenceOperatorTokens[(object) "."] = (object) new OperatorToken(".", TokenId.ReferenceOperatorToken);
      OperatorTokenReader._referenceOperatorTokens[(object) "["] = (object) new OperatorToken("[", TokenId.ReferenceOperatorToken);
    }

    private static void InitializeAttributeOperatorTokens()
    {
      OperatorTokenReader._attributeOperatorTokens.Clear();
      OperatorTokenReader._attributeOperatorTokens[(object) "["] = (object) new OperatorToken("[", TokenId.AttributeStartToken);
    }

    private static void InitializeExpressionOperatorTokens()
    {
      OperatorTokenReader._expressionOperatorTokens.Clear();
      OperatorTokenReader._expressionOperatorTokens[(object) "!"] = (object) new OperatorToken("!", TokenId.LogicalNotToken);
      OperatorTokenReader._expressionOperatorTokens[(object) "-not"] = (object) new OperatorToken("-not", TokenId.LogicalNotToken);
      OperatorTokenReader._expressionOperatorTokens[(object) "-f"] = (object) new OperatorToken("-f", TokenId.FormatOperatorToken, 6);
      OperatorTokenReader._expressionOperatorTokens[(object) "++"] = (object) new OperatorToken("++", TokenId.PlusPlusToken);
      OperatorTokenReader._expressionOperatorTokens[(object) "*"] = (object) new OperatorToken("*", TokenId.MultiplyOperatorToken, 5);
      OperatorTokenReader._expressionOperatorTokens[(object) "/"] = (object) new OperatorToken("/", TokenId.MultiplyOperatorToken, 5);
      OperatorTokenReader._expressionOperatorTokens[(object) "%"] = (object) new OperatorToken("%", TokenId.MultiplyOperatorToken, 5);
      OperatorTokenReader._expressionOperatorTokens[(object) "-bnot"] = (object) new OperatorToken("-bnot", TokenId.BitwiseNotToken);
      OperatorTokenReader._expressionOperatorTokens[(object) "+"] = (object) new OperatorToken("+", TokenId.AdditionOperatorToken, 4);
      OperatorTokenReader._expressionOperatorTokens[(object) "-"] = (object) new OperatorToken("-", TokenId.AdditionOperatorToken, 4);
      OperatorTokenReader._expressionOperatorTokens[(object) '—'.ToString()] = (object) new OperatorToken("-", TokenId.AdditionOperatorToken, 4);
      OperatorTokenReader._expressionOperatorTokens[(object) '–'.ToString()] = (object) new OperatorToken("-", TokenId.AdditionOperatorToken, 4);
      OperatorTokenReader._expressionOperatorTokens[(object) '―'.ToString()] = (object) new OperatorToken("-", TokenId.AdditionOperatorToken, 4);
      for (int index = 0; index < OperatorTokenReader.ComparisonOperators.Length; ++index)
      {
        string comparisonOperator = OperatorTokenReader.ComparisonOperators[index];
        OperatorTokenReader._expressionOperatorTokens[(object) comparisonOperator] = (object) new ComparisonToken(comparisonOperator, TokenId.ComparisonOperatorToken, 3, comparisonOperator, true);
      }
      for (int index = 0; index < OperatorTokenReader.CaseSensitiveComparisonOperators.Length; ++index)
      {
        string comparisonOperator = OperatorTokenReader.CaseSensitiveComparisonOperators[index];
        OperatorTokenReader._expressionOperatorTokens[(object) comparisonOperator] = (object) new ComparisonToken(comparisonOperator, TokenId.ComparisonOperatorToken, 3, OperatorTokenReader.ComparisonOperators[index], false);
      }
      for (int index = 0; index < OperatorTokenReader.CaseInsensitiveComparisonOperators.Length; ++index)
      {
        string comparisonOperator = OperatorTokenReader.CaseInsensitiveComparisonOperators[index];
        OperatorTokenReader._expressionOperatorTokens[(object) comparisonOperator] = (object) new ComparisonToken(comparisonOperator, TokenId.ComparisonOperatorToken, 3, OperatorTokenReader.ComparisonOperators[index], true);
      }
      OperatorTokenReader._expressionOperatorTokens[(object) "-join"] = (object) new ComparisonToken("-join", TokenId.ComparisonOperatorToken, 3, "-join", true);
      foreach (string logicalOperator in OperatorTokenReader.LogicalOperators)
        OperatorTokenReader._expressionOperatorTokens[(object) logicalOperator] = (object) new OperatorToken(logicalOperator, TokenId.LogicalOperatorToken, 1);
      foreach (string bitwiseOperator in OperatorTokenReader.BitwiseOperators)
        OperatorTokenReader._expressionOperatorTokens[(object) bitwiseOperator] = (object) new OperatorToken(bitwiseOperator, TokenId.BitwiseOperatorToken, 2);
      foreach (string assignmentOperator in OperatorTokenReader.AssignmentOperators)
        OperatorTokenReader._expressionOperatorTokens[(object) assignmentOperator] = (object) new OperatorToken(assignmentOperator, TokenId.AssignmentOperatorToken);
    }

    internal override Token GetToken(string input, ref int offset)
    {
      int start = offset;
      string a = this.Match(input, offset);
      if (string.IsNullOrEmpty(a))
        return (Token) null;
      if (this._type == OperatorTokenReaderType.Reference)
      {
        if (offset > 0 && char.IsWhiteSpace(input[offset - 1]))
          return (Token) null;
        if (!a.Equals("[", StringComparison.OrdinalIgnoreCase) && offset + a.Length < input.Length && char.IsWhiteSpace(input[offset + a.Length]))
          return (Token) null;
      }
      if (!this.Tokenizer.AllowRangeOperator && string.Equals(a, "..", StringComparison.OrdinalIgnoreCase))
        return (Token) null;
      if (this.Tokenizer.Mode == ParseMode.Command && string.Equals(a, "--", StringComparison.OrdinalIgnoreCase) && (offset + a.Length < input.Length && "{}();,|&\r\n\t ".IndexOf(input[offset + a.Length]) == -1))
        return (Token) null;
      offset += a.Length;
      if (this.Tokenizer.ProcessingCallArguments && string.Equals(a, ",", StringComparison.OrdinalIgnoreCase))
        return (Token) new OperatorToken(",", TokenId.CallArgumentSeparatorToken);
      Token errToken = ((Token) this.OperatorTokens[(object) a]).Clone();
      if (errToken.Is(TokenId.AndAndToken) || errToken.Is(TokenId.OrOrToken))
      {
        errToken.SetPosition(this.Tokenizer.File, this.Tokenizer.Script, start, offset, this.Tokenizer);
        this.Tokenizer.Parser.ReportException((object) errToken.TokenText, typeof (ParseException), errToken, "InvalidEndOfLine", (object) errToken);
      }
      return errToken;
    }

    private string Match(string input, int offset)
    {
      if (offset < 0 || offset >= input.Length)
        return (string) null;
      int num = offset;
      if (SpecialCharacters.IsDash(input[num]))
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append('-');
        if (num + 1 < input.Length)
        {
          if (SpecialCharacters.IsDash(input[num + 1]))
            stringBuilder.Append('-');
          else if (input[num + 1] == '=')
            stringBuilder.Append('=');
          else if (char.IsLetter(input[num + 1]))
          {
            for (int index = num + 1; index < input.Length && char.IsLetter(input[index]); ++index)
              stringBuilder.Append(input[index]);
          }
        }
        if (this.OperatorTokens.Contains((object) stringBuilder.ToString()))
          return stringBuilder.ToString();
      }
      if (char.IsDigit(input[num]))
      {
        if (num + 4 <= input.Length)
        {
          string str = input.Substring(num, 4);
          if (this.OperatorTokens.Contains((object) str))
            return str;
        }
        if (num + 3 <= input.Length)
        {
          string str = input.Substring(num, 3);
          if (this.OperatorTokens.Contains((object) str))
            return str;
        }
      }
      if (num + 2 <= input.Length)
      {
        string str = input.Substring(num, 2);
        if (this.OperatorTokens.Contains((object) str))
          return str;
      }
      string str1 = input.Substring(num, 1);
      return this.OperatorTokens.Contains((object) str1) ? str1 : (string) null;
    }
  }
}
