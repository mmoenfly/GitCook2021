// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SpecialCharacters
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal static class SpecialCharacters
  {
    public const char enDash = '–';
    public const char emDash = '—';
    public const char horizontalBar = '―';
    public const char quoteSingleLeft = '‘';
    public const char quoteSingleRight = '’';
    public const char quoteSingleBase = '‚';
    public const char quoteReversed = '‛';
    public const char quoteDoubleLeft = '“';
    public const char quoteDoubleRight = '”';
    public const char quoteLowDoubleLeft = '„';

    public static bool IsDash(char c) => c == '–' || c == '—' || c == '―' || c == '-';

    public static bool IsSingleQuote(char c) => c == '‘' || c == '’' || (c == '‚' || c == '‛') || c == '\'';

    public static bool IsDoubleQuote(char c) => c == '"' || c == '“' || c == '”' || c == '„';

    public static bool IsQuote(char c) => SpecialCharacters.IsSingleQuote(c) | SpecialCharacters.IsDoubleQuote(c);

    public static bool IsDelimiter(char c, char delimiter)
    {
      if (delimiter == '"')
        return SpecialCharacters.IsDoubleQuote(c);
      return delimiter == '\'' ? SpecialCharacters.IsSingleQuote(c) : (int) c == (int) delimiter;
    }

    public static char AsQuote(char c)
    {
      if (SpecialCharacters.IsSingleQuote(c))
        return '\'';
      return SpecialCharacters.IsDoubleQuote(c) ? '"' : c;
    }
  }
}
