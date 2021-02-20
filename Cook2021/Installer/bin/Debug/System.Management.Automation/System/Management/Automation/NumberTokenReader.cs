// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.NumberTokenReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Text;

namespace System.Management.Automation
{
  internal sealed class NumberTokenReader : TokenReader
  {
    public NumberTokenReader(Tokenizer tokenizer)
      : base(tokenizer)
    {
    }

    internal override TokenClass TokenClass => TokenClass.Number;

    internal override Token GetToken(string input, ref int offset)
    {
      object tokenData;
      string text = this.Match(input, offset, out tokenData);
      if (text == null)
        return (Token) null;
      offset += text.Length;
      return new Token(text, TokenId.NumberToken)
      {
        Data = tokenData
      };
    }

    private string Match(string input, int offset, out object tokenData)
    {
      tokenData = (object) null;
      int startIndex = offset;
      if (this.Tokenizer.AllowSignedNumber && offset < input.Length && (input[offset] == '+' || SpecialCharacters.IsDash(input[offset])))
        ++offset;
      string str1 = NumberTokenReader.ScanForNumber(input, offset);
      if (string.IsNullOrEmpty(str1))
        return (string) null;
      offset += str1.Length;
      if (offset < input.Length)
      {
        if (this.Tokenizer.Mode == ParseMode.Command)
        {
          if ("{}();,|&\r\n\t ".IndexOf(input[offset]) == -1)
            return (string) null;
        }
        else if (char.IsLetterOrDigit(input[offset]))
        {
          if (this.Tokenizer.Mode == ParseMode.Expression)
            this.Tokenizer.Parser.ReportException((object) str1, typeof (ParseException), this.Tokenizer.PositionToken(offset), "BadNumericConstant", (object) str1);
          return (string) null;
        }
      }
      try
      {
        string str2 = input.Substring(startIndex, offset - startIndex);
        tokenData = NumberTokenReader.ConvertToNumber(str2, this.Tokenizer);
        return str2;
      }
      catch (RuntimeException ex)
      {
        if (this.Tokenizer.Mode != ParseMode.Expression)
          return (string) null;
        if (ex.ErrorRecord != null)
          ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, this.Tokenizer.PositionToken(offset)));
        throw;
      }
    }

    private static string ScanForNumber(string input, int offset)
    {
      int startIndex = offset;
      if (offset >= input.Length)
        return (string) null;
      char c1 = input[offset];
      if (!char.IsDigit(c1) && c1 != '.')
        return (string) null;
      bool flag1 = true;
      bool flag2 = false;
      bool flag3 = false;
      if (c1 == '.')
      {
        if (offset + 1 >= input.Length || !char.IsDigit(input[offset + 1]))
          return (string) null;
        flag1 = false;
      }
      if (c1 == '0' && offset + 1 < input.Length && char.ToUpperInvariant(input[offset + 1]) == 'X')
      {
        ++offset;
        flag2 = true;
        flag1 = false;
      }
      while (++offset < input.Length)
      {
        char c2 = input[offset];
        if (!char.IsDigit(c2) && (!flag2 || char.ToUpperInvariant(c2) > 'F' || char.ToUpperInvariant(c2) < 'A'))
        {
          if (c2 == '.')
          {
            if (flag1 && (offset + 1 >= input.Length || input[offset + 1] != '.'))
              flag1 = false;
            else
              break;
          }
          else if (char.ToUpperInvariant(c2) == 'E' && !flag3)
          {
            if (offset + 1 < input.Length && (input[offset + 1] == '+' || SpecialCharacters.IsDash(input[offset + 1])))
              ++offset;
            flag3 = true;
            flag1 = false;
          }
          else
            break;
        }
      }
      if (offset < input.Length)
      {
        switch (char.ToUpperInvariant(input[offset]))
        {
          case 'D':
          case 'L':
            ++offset;
            break;
        }
      }
      if (NumberTokenReader.IsMultiplier(input, offset))
        offset += 2;
      return input.Substring(startIndex, offset - startIndex);
    }

    internal static bool IsMultiplier(string input, int offset)
    {
      if (offset < 0 || offset > input.Length - 2)
        return false;
      char upperInvariant1 = char.ToUpperInvariant(input[offset]);
      char upperInvariant2 = char.ToUpperInvariant(input[offset + 1]);
      return (upperInvariant1 == 'K' || upperInvariant1 == 'M' || (upperInvariant1 == 'G' || upperInvariant1 == 'T') || upperInvariant1 == 'P') && upperInvariant2 == 'B';
    }

    private static object ConvertToNumber(string str, Tokenizer tokenizer)
    {
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      bool flag4 = false;
      long num = 1;
      str = NumberTokenReader.ConvertDash(str);
      if (str.IndexOf("0x", 0, StringComparison.OrdinalIgnoreCase) >= 0)
        flag1 = true;
      if (!flag1 && (str.IndexOf('.', 0) >= 0 || str.IndexOf("e", 0, StringComparison.OrdinalIgnoreCase) >= 0))
        flag4 = true;
      if (NumberTokenReader.IsMultiplier(str, str.Length - 2))
      {
        switch (char.ToUpperInvariant(str[str.Length - 2]))
        {
          case 'G':
            num = 1073741824L;
            str = str.Substring(0, str.Length - 2);
            break;
          case 'K':
            num = 1024L;
            str = str.Substring(0, str.Length - 2);
            break;
          case 'M':
            num = 1048576L;
            str = str.Substring(0, str.Length - 2);
            break;
          case 'P':
            num = 1125899906842624L;
            str = str.Substring(0, str.Length - 2);
            break;
          case 'T':
            num = 1099511627776L;
            str = str.Substring(0, str.Length - 2);
            break;
        }
      }
      char upperInvariant = char.ToUpperInvariant(str[str.Length - 1]);
      if (upperInvariant == 'L')
      {
        flag2 = true;
        str = str.Substring(0, str.Length - 1);
      }
      else if (!flag1 && upperInvariant == 'D')
      {
        flag3 = true;
        str = str.Substring(0, str.Length - 1);
      }
      Exception innerException;
      try
      {
        if (flag3)
          return (object) (Decimal.Parse(str, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, (IFormatProvider) NumberFormatInfo.InvariantInfo) * (Decimal) num);
        if (flag2)
          return (object) checked ((long) LanguagePrimitives.ConvertTo((object) str, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo) * num);
        if (flag4)
          return (object) (double.Parse(str, (IFormatProvider) NumberFormatInfo.InvariantInfo) * (double) num);
        try
        {
          return (object) checked ((int) LanguagePrimitives.ConvertTo((object) str, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo) * (int) num);
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
        }
        try
        {
          return (object) checked ((long) LanguagePrimitives.ConvertTo((object) str, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo) * num);
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
        }
        try
        {
          return (object) ((Decimal) LanguagePrimitives.ConvertTo((object) str, typeof (Decimal), (IFormatProvider) NumberFormatInfo.InvariantInfo) * (Decimal) num);
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
        }
        return (object) ((double) LanguagePrimitives.ConvertTo((object) str, typeof (double), (IFormatProvider) NumberFormatInfo.InvariantInfo) * (double) num);
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (PSInvalidCastException ex)
      {
        innerException = (Exception) ex;
      }
      if (innerException != null)
      {
        if (tokenizer != null)
          tokenizer.Parser.ReportExceptionWithInnerException((object) str, typeof (ParseException), tokenizer.PositionToken(), "BadNumericConstant", innerException, (object) innerException.Message);
        else
          throw InterpreterError.NewInterpreterExceptionWithInnerException((object) str, typeof (RuntimeException), (Token) null, "BadNumericConstant", innerException, (object) innerException.Message);
      }
      return (object) null;
    }

    private static string ConvertDash(string str)
    {
      if (string.IsNullOrEmpty(str))
        return str;
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char c in str)
      {
        if (SpecialCharacters.IsDash(c))
          stringBuilder.Append('-');
        else
          stringBuilder.Append(c);
      }
      return stringBuilder.ToString();
    }

    internal static object StringToNumber(string input)
    {
      if (string.IsNullOrEmpty(input))
        return (object) null;
      input = input.Trim();
      if (string.IsNullOrEmpty(input))
        return (object) null;
      string str1 = "";
      if (input[0] == '+' || SpecialCharacters.IsDash(input[0]))
      {
        str1 = input[0].ToString();
        input = input.Substring(1);
      }
      string str2 = NumberTokenReader.ScanForNumber(input, 0);
      if (str2 == null)
        return (object) null;
      if (str2.Length < input.Length)
        throw InterpreterError.NewInterpreterException((object) input, typeof (RuntimeException), (Token) null, "BadNumericConstant", (object) input);
      return NumberTokenReader.ConvertToNumber(str1 + str2, (Tokenizer) null);
    }
  }
}
