// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.KeywordTokenReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;

namespace System.Management.Automation
{
  internal sealed class KeywordTokenReader : TokenReader
  {
    private static Hashtable _keywordTokens = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
    [TraceSource("Tokenizer", "Tokenizer")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");

    static KeywordTokenReader() => KeywordTokenReader.InitializeKeywordTokens();

    public KeywordTokenReader(Tokenizer tokenizer)
      : base(tokenizer)
    {
    }

    private static void InitializeKeywordTokens()
    {
      KeywordTokenReader._keywordTokens.Clear();
      KeywordTokenReader._keywordTokens[(object) "elseif"] = (object) new Token("elseif", TokenId.ElseIfToken);
      KeywordTokenReader._keywordTokens[(object) "if"] = (object) new Token("if", TokenId.IfToken);
      KeywordTokenReader._keywordTokens[(object) "else"] = (object) new Token("else", TokenId.ElseToken);
      KeywordTokenReader._keywordTokens[(object) "switch"] = (object) new Token("switch", TokenId.SwitchToken);
      KeywordTokenReader._keywordTokens[(object) "foreach"] = (object) new Token("foreach", TokenId.ForeachToken);
      KeywordTokenReader._keywordTokens[(object) "from"] = (object) new Token("from", TokenId.FromToken);
      KeywordTokenReader._keywordTokens[(object) "in"] = (object) new Token("in", TokenId.InToken);
      KeywordTokenReader._keywordTokens[(object) "for"] = (object) new Token("for", TokenId.ForToken);
      KeywordTokenReader._keywordTokens[(object) "while"] = (object) new Token("while", TokenId.WhileToken);
      KeywordTokenReader._keywordTokens[(object) "until"] = (object) new Token("until", TokenId.UntilToken);
      KeywordTokenReader._keywordTokens[(object) "do"] = (object) new Token("do", TokenId.DoToken);
      KeywordTokenReader._keywordTokens[(object) "try"] = (object) new Token("try", TokenId.TryToken);
      KeywordTokenReader._keywordTokens[(object) "catch"] = (object) new Token("catch", TokenId.CatchToken);
      KeywordTokenReader._keywordTokens[(object) "finally"] = (object) new Token("finally", TokenId.FinallyToken);
      KeywordTokenReader._keywordTokens[(object) "trap"] = (object) new Token("trap", TokenId.TrapToken);
      KeywordTokenReader._keywordTokens[(object) "data"] = (object) new Token("data", TokenId.DataSectionToken);
      KeywordTokenReader._keywordTokens[(object) "return"] = (object) new Token("return", TokenId.ReturnToken);
      KeywordTokenReader._keywordTokens[(object) "continue"] = (object) new Token("continue", TokenId.ContinueToken);
      KeywordTokenReader._keywordTokens[(object) "break"] = (object) new Token("break", TokenId.BreakToken);
      KeywordTokenReader._keywordTokens[(object) "exit"] = (object) new Token("exit", TokenId.ExitToken);
      KeywordTokenReader._keywordTokens[(object) "throw"] = (object) new Token("throw", TokenId.ThrowToken);
      KeywordTokenReader._keywordTokens[(object) "begin"] = (object) new Token("begin", TokenId.BeginToken);
      KeywordTokenReader._keywordTokens[(object) "process"] = (object) new Token("process", TokenId.ProcessToken);
      KeywordTokenReader._keywordTokens[(object) "end"] = (object) new Token("end", TokenId.EndToken);
      KeywordTokenReader._keywordTokens[(object) "dynamicparam"] = (object) new Token("dynamicparam", TokenId.DynamicParamToken);
      KeywordTokenReader._keywordTokens[(object) "function"] = (object) new Token("function", TokenId.FunctionDeclarationToken);
      KeywordTokenReader._keywordTokens[(object) "filter"] = (object) new Token("filter", TokenId.FunctionDeclarationToken);
      KeywordTokenReader._keywordTokens[(object) "param"] = (object) new Token("param", TokenId.ParameterDeclarationToken);
      KeywordTokenReader._keywordTokens[(object) "class"] = (object) new Token("class", TokenId.ReservedKeywordToken);
      KeywordTokenReader._keywordTokens[(object) "define"] = (object) new Token("define", TokenId.ReservedKeywordToken);
      KeywordTokenReader._keywordTokens[(object) "var"] = (object) new Token("var", TokenId.ReservedKeywordToken);
      KeywordTokenReader._keywordTokens[(object) "using"] = (object) new Token("using", TokenId.ReservedKeywordToken);
    }

    internal override TokenClass TokenClass => TokenClass.Keyword;

    internal override Token GetToken(string input, ref int offset)
    {
      string str = this.MatchKeywordString(input, offset);
      if (string.IsNullOrEmpty(str))
        return (Token) null;
      if (!KeywordTokenReader._keywordTokens.Contains((object) str))
        return (Token) null;
      offset += str.Length;
      return ((Token) KeywordTokenReader._keywordTokens[(object) str]).Clone();
    }

    private string MatchKeywordString(string input, int offset)
    {
      if (offset < 0)
        return (string) null;
      int index = offset;
      while (index < input.Length && char.IsLetter(input[index]))
        ++index;
      return index >= input.Length || "{}();,|&\r\n\t ".IndexOf(input[index]) >= 0 ? input.Substring(offset, index - offset) : (string) null;
    }
  }
}
