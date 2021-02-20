// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.WildcardPattern
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Text.RegularExpressions;

namespace System.Management.Automation
{
  public sealed class WildcardPattern
  {
    private const char escapeChar = '`';
    private const string regexChars = "()[].?*{}^$+|\\";
    [TraceSource("WildcardPattern", "WildcardPattern")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (WildcardPattern), nameof (WildcardPattern));
    private Regex patternRegex;
    private string pattern;
    private WildcardOptions options;

    internal string Pattern => this.pattern;

    internal WildcardOptions Options => this.options;

    internal string PatternConvertedToRegex
    {
      get
      {
        this.Init();
        return this.patternRegex.ToString();
      }
    }

    public WildcardPattern(string pattern) => this.pattern = pattern != null ? pattern : throw WildcardPattern.tracer.NewArgumentNullException(nameof (pattern));

    public WildcardPattern(string pattern, WildcardOptions options)
    {
      this.pattern = pattern != null ? pattern : throw WildcardPattern.tracer.NewArgumentNullException(nameof (pattern));
      this.options = options;
    }

    private bool Init()
    {
      if (this.patternRegex == null)
      {
        RegexOptions options = RegexOptions.Singleline;
        if ((this.options & WildcardOptions.Compiled) != WildcardOptions.None)
          options |= RegexOptions.Compiled;
        if ((this.options & WildcardOptions.IgnoreCase) != WildcardOptions.None)
          options |= RegexOptions.IgnoreCase;
        if ((this.options & WildcardOptions.CultureInvariant) == WildcardOptions.CultureInvariant)
          options |= RegexOptions.CultureInvariant;
        try
        {
          this.patternRegex = new Regex(WildcardPattern.ConvertWildcardToRegex(this.pattern), options);
        }
        catch (ArgumentException ex)
        {
          throw WildcardPattern.NewWildcardPatternException(this.pattern);
        }
      }
      return this.patternRegex != null;
    }

    private static WildcardPatternException NewWildcardPatternException(
      string invalidPattern)
    {
      return new WildcardPatternException(new ErrorRecord((Exception) new ParentContainsErrorRecordException(ResourceManagerCache.FormatResourceString(nameof (WildcardPattern), "InvalidPattern", (object) invalidPattern)), "WildcardPattern_Invalid", ErrorCategory.InvalidArgument, (object) null));
    }

    public bool IsMatch(string input)
    {
      bool flag = false;
      if (this.Init())
        flag = this.patternRegex.IsMatch(input);
      return flag;
    }

    private static string ConvertWildcardToRegex(string pattern)
    {
      if (pattern == null)
        return (string) null;
      string str1 = pattern;
      if (str1.Length == 0)
        return "^$";
      char[] chArray1 = new char[str1.Length * 2 + 2];
      int length = 0;
      bool flag1 = false;
      bool flag2 = false;
      char ch1 = str1[0];
      if (ch1 != '*')
      {
        chArray1[length++] = '^';
        if (ch1 == '`')
          flag2 = true;
        else if (ch1 == '?')
          chArray1[length++] = '.';
        else if (WildcardPattern.IsNonWildcardRegexChar(ch1))
        {
          char[] chArray2 = chArray1;
          int index1 = length;
          int num1 = index1 + 1;
          chArray2[index1] = '\\';
          char[] chArray3 = chArray1;
          int index2 = num1;
          length = index2 + 1;
          int num2 = (int) ch1;
          chArray3[index2] = (char) num2;
        }
        else
        {
          chArray1[length++] = ch1;
          if (ch1 == '[')
            flag1 = true;
        }
        if (str1.Length == 1)
          chArray1[length++] = '$';
      }
      int index3;
      for (index3 = 1; index3 < str1.Length - 1; ++index3)
      {
        char ch2 = str1[index3];
        if (WildcardPattern.IsNonWildcardRegexChar(ch2))
          chArray1[length++] = '\\';
        switch (ch2)
        {
          case '*':
            if (flag2)
            {
              chArray1[length++] = '\\';
              break;
            }
            if (!flag1)
            {
              chArray1[length++] = '.';
              break;
            }
            break;
          case '?':
            if (flag2)
            {
              chArray1[length++] = '\\';
              break;
            }
            if (!flag1)
            {
              ch2 = '.';
              break;
            }
            break;
          case '[':
          case ']':
            if (flag2)
            {
              chArray1[length++] = '\\';
              break;
            }
            flag1 = ch2 == '[';
            break;
        }
        flag2 = (!flag2 || ch2 != '`') && ch2 == '`';
        if (!flag2)
          chArray1[length++] = ch2;
      }
      if (index3 < str1.Length)
      {
        char ch2 = str1[index3];
        if (flag2)
        {
          if (WildcardPattern.IsWildcardChar(ch2))
            chArray1[length++] = '\\';
        }
        else if (ch2 == '?')
          ch2 = '.';
        else if (WildcardPattern.IsNonWildcardRegexChar(ch2))
          chArray1[length++] = '\\';
        if (ch2 != '*' || flag2)
        {
          char[] chArray2 = chArray1;
          int index1 = length;
          int num1 = index1 + 1;
          int num2 = (int) ch2;
          chArray2[index1] = (char) num2;
          char[] chArray3 = chArray1;
          int index2 = num1;
          length = index2 + 1;
          chArray3[index2] = '$';
        }
      }
      string str2 = new string(chArray1, 0, length);
      WildcardPattern.tracer.WriteLine("Converted Wildcard ({0}) to Regex ({1})", (object) pattern, (object) str2);
      return str2;
    }

    public static string Escape(string pattern)
    {
      char[] chArray = pattern != null ? new char[pattern.Length * 2 + 1] : throw WildcardPattern.tracer.NewArgumentNullException(nameof (pattern));
      int length = 0;
      for (int index = 0; index < pattern.Length; ++index)
      {
        char ch = pattern[index];
        if (WildcardPattern.IsWildcardChar(ch))
          chArray[length++] = '`';
        chArray[length++] = ch;
      }
      return length <= 0 ? string.Empty : new string(chArray, 0, length);
    }

    public static bool ContainsWildcardCharacters(string pattern)
    {
      switch (pattern)
      {
        case "":
        case null:
          return false;
        default:
          bool flag = false;
          for (int index = 0; index < pattern.Length; ++index)
          {
            if (WildcardPattern.IsWildcardChar(pattern[index]))
            {
              flag = true;
              break;
            }
            if (pattern[index] == '`')
              ++index;
          }
          return flag;
      }
    }

    public static string Unescape(string pattern)
    {
      char[] chArray = pattern != null ? new char[pattern.Length] : throw WildcardPattern.tracer.NewArgumentNullException(nameof (pattern));
      int length = 0;
      bool flag = false;
      for (int index = 0; index < pattern.Length; ++index)
      {
        char ch = pattern[index];
        if (ch == '`')
        {
          if (flag)
          {
            chArray[length++] = ch;
            flag = false;
          }
          else
            flag = true;
        }
        else
        {
          if (flag && !WildcardPattern.IsWildcardChar(ch))
            chArray[length++] = '`';
          chArray[length++] = ch;
          flag = false;
        }
      }
      if (flag)
        chArray[length++] = '`';
      string str = length <= 0 ? string.Empty : new string(chArray, 0, length);
      WildcardPattern.tracer.WriteLine("{0} ==> {1}", (object) pattern, (object) str);
      return str;
    }

    private static bool IsWildcardChar(char ch) => ch == '*' || ch == '?' || ch == '[' || ch == ']';

    private static bool IsRegexChar(char ch)
    {
      for (int index = 0; index < "()[].?*{}^$+|\\".Length; ++index)
      {
        if ((int) ch == (int) "()[].?*{}^$+|\\"[index])
          return true;
      }
      return false;
    }

    private static bool IsNonWildcardRegexChar(char ch) => !WildcardPattern.IsWildcardChar(ch) && WildcardPattern.IsRegexChar(ch);
  }
}
