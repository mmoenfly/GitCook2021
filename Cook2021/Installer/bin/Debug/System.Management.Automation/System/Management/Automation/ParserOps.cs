// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParserOps
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Management.Automation
{
  internal static class ParserOps
  {
    internal const string MethodNotFoundErrorId = "MethodNotFound";
    private const int _MinCache = -100;
    private const int _MaxCache = 1000;
    private const int MaxRegexCache = 1000;
    private static readonly object[] _integerCache = new object[1100];
    private static readonly string[] _chars = new string[(int) byte.MaxValue];
    internal static readonly object _TrueObject = (object) true;
    internal static readonly object _FalseObject = (object) false;
    [TraceSource("Parser", "Parser")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");
    private static Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();

    static ParserOps()
    {
      for (int index = 0; index < 1100; ++index)
        ParserOps._integerCache[index] = (object) (index - 100);
      for (char minValue = char.MinValue; minValue < 'ÿ'; ++minValue)
        ParserOps._chars[(int) minValue] = new string(minValue, 1);
    }

    internal static string CharToString(char ch) => ch < 'ÿ' ? ParserOps._chars[(int) ch] : new string(ch, 1);

    internal static object BoolToObject(bool value) => !value ? ParserOps._FalseObject : ParserOps._TrueObject;

    internal static object IntToObject(int value) => value < 1000 && value >= -100 ? ParserOps._integerCache[value - -100] : (object) value;

    internal static PSObject WrappedNumber(object data, string text) => new PSObject(data)
    {
      TokenText = text
    };

    internal static int FixNum(object obj, Token token)
    {
      obj = PSObject.Base(obj);
      if (obj == null)
        return 0;
      return obj is int num ? num : Parser.ConvertTo<int>(obj, token);
    }

    internal static object StringToNumber(Token opToken, string s)
    {
      try
      {
        return NumberTokenReader.StringToNumber(s);
      }
      catch (RuntimeException ex)
      {
        if (ex.ErrorRecord != null)
          ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, opToken));
        throw;
      }
    }

    private static object ImplicitOp(Token opToken, object lval, object rval, string op)
    {
      lval = PSObject.Base(lval);
      rval = PSObject.Base(rval);
      if (lval == null)
      {
        string str = rval == null ? "$null" : rval.GetType().FullName;
        throw InterpreterError.NewInterpreterException(lval, typeof (RuntimeException), opToken, "NotAdefinedOperationForTypeType", (object) "$null", (object) opToken.TokenText, (object) str);
      }
      Type type = lval.GetType();
      if (type.IsPrimitive)
      {
        string str = rval == null ? "$null" : rval.GetType().FullName;
        throw InterpreterError.NewInterpreterException(lval, typeof (RuntimeException), opToken, "NotAdefinedOperationForTypeType", (object) type.FullName, (object) opToken.TokenText, (object) str);
      }
      object[] paramArray = new object[2]{ lval, rval };
      return ParserOps.CallMethod(opToken, (object) lval.GetType(), op, paramArray, true, (object) AutomationNull.Value);
    }

    internal static object PolyAdd(ExecutionContext context, object lval, object rval) => ParserOps.PolyAdd(context, (Token) null, lval, rval);

    internal static object PolyAdd(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      object lval1 = lval;
      lval = PSObject.Base(lval);
      if (lval == null)
        return rval;
      Type lvalType = lval.GetType();
      if (LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(lvalType)))
      {
        object lval2 = lval;
        object rval1 = rval;
        Type rvalType;
        Type type = ParserOps.figureOutOperationType(opToken, ref lval, out lvalType, ref rval, out rvalType);
        if (type == null)
          return ParserOps.ImplicitOp(opToken, lval2, rval1, "op_Addition");
        if (type.Equals(typeof (Decimal)))
        {
          try
          {
            return (object) (Parser.ConvertTo<Decimal>(lval, opToken) + Parser.ConvertTo<Decimal>(rval, opToken));
          }
          catch (OverflowException ex)
          {
            RuntimeException runtimeException = new RuntimeException(ex.Message, (Exception) ex);
            runtimeException.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, opToken, context));
            ParserOps.tracer.TraceException((Exception) runtimeException);
            throw runtimeException;
          }
        }
        else
        {
          if (!type.Equals(typeof (float)))
          {
            if (!type.Equals(typeof (double)))
            {
              try
              {
                if (type.Equals(typeof (long)))
                  return (object) checked (Parser.ConvertTo<long>(lval, opToken) + Parser.ConvertTo<long>(rval, opToken));
                return type.Equals(typeof (ulong)) ? (object) checked (Parser.ConvertTo<ulong>(lval, opToken) + Parser.ConvertTo<ulong>(rval, opToken)) : ParserOps.IntToObject(checked (lvalType != typeof (int) ? Parser.ConvertTo<int>(lval, opToken) : (int) lval + rvalType != typeof (int) ? Parser.ConvertTo<int>(rval, opToken) : (int) rval));
              }
              catch (OverflowException ex1)
              {
                try
                {
                  return (object) (Parser.ConvertTo<double>(lval, opToken) + Parser.ConvertTo<double>(rval, opToken));
                }
                catch (OverflowException ex2)
                {
                  RuntimeException runtimeException = new RuntimeException(ex2.Message, (Exception) ex2);
                  ParserOps.tracer.TraceException((Exception) runtimeException);
                  throw runtimeException;
                }
              }
            }
          }
          return (object) ((lvalType != typeof (double) ? Parser.ConvertTo<double>(lval, opToken) : (double) lval) + (rvalType != typeof (double) ? Parser.ConvertTo<double>(rval, opToken) : (double) rval));
        }
      }
      else
      {
        if (lvalType.Equals(typeof (string)))
          return (object) ((string) lval + PSObject.ToStringParser(context, rval));
        if (lvalType.Equals(typeof (char)))
          return (object) (lval.ToString() + PSObject.ToStringParser(context, rval));
        IEnumerator enumerator1 = LanguagePrimitives.GetEnumerator(lval);
        if (enumerator1 != null)
        {
          ArrayList arrayList = new ArrayList();
          while (ParserOps.MoveNext(context, opToken, enumerator1))
          {
            object obj = ParserOps.Current(opToken, enumerator1);
            arrayList.Add(obj);
          }
          IEnumerator enumerator2 = LanguagePrimitives.GetEnumerator(rval);
          if (enumerator2 != null)
          {
            while (ParserOps.MoveNext(context, opToken, enumerator2))
            {
              object obj = ParserOps.Current(opToken, enumerator2);
              arrayList.Add(obj);
            }
          }
          else
            arrayList.Add(rval);
          return (object) arrayList.ToArray();
        }
        if (!(lval is IDictionary dictionary))
          return ParserOps.ImplicitOp(opToken, lval1, rval, "op_Addition");
        rval = PSObject.Base(rval);
        if (!(rval is IDictionary dictionary))
          throw InterpreterError.NewInterpreterException(rval, typeof (RuntimeException), opToken, "AddHashTableToNonHashTable");
        Hashtable hashtable = new Hashtable((IEqualityComparer) StringComparer.CurrentCultureIgnoreCase);
        foreach (object key in (IEnumerable) dictionary.Keys)
          hashtable.Add(key, dictionary[key]);
        foreach (object key in (IEnumerable) dictionary.Keys)
          hashtable.Add(key, dictionary[key]);
        return (object) hashtable;
      }
    }

    private static string PolyMultiplyHelper(string s, int times)
    {
      uint num = (uint) times;
      StringBuilder stringBuilder = new StringBuilder(s.Length * times);
      for (int index = 0; index < 32; ++index)
      {
        if (stringBuilder.Length != 0)
          stringBuilder.Append(stringBuilder.ToString());
        if (((int) num & int.MinValue) != 0)
          stringBuilder.Append(s);
        num <<= 1;
      }
      return stringBuilder.ToString();
    }

    private static object[] PolyMultiplyHelper(object[] array, int times)
    {
      uint num1 = (uint) times;
      object[] objArray = new object[array.Length * times];
      uint num2 = 0;
      for (int index = 0; index < 32; ++index)
      {
        if (num2 != 0U)
        {
          Array.Copy((Array) objArray, 0L, (Array) objArray, (long) num2, (long) num2);
          num2 *= 2U;
        }
        if (((int) num1 & int.MinValue) != 0)
        {
          Array.Copy((Array) array, 0L, (Array) objArray, (long) num2, (long) array.Length);
          num2 += (uint) array.Length;
        }
        num1 <<= 1;
      }
      return objArray;
    }

    internal static object PolyMultiply(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      if (lval == null)
        return (object) null;
      lval = PSObject.Base(lval);
      rval = PSObject.Base(rval);
      if (rval == null)
        rval = (object) 0;
      Type lvalType = lval.GetType();
      if (LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(lvalType)))
      {
        object lval1 = lval;
        object rval1 = rval;
        Type rvalType;
        Type type = ParserOps.figureOutOperationType(opToken, ref lval, out lvalType, ref rval, out rvalType);
        if (type == null)
          return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Multiply");
        if (type.Equals(typeof (Decimal)))
          return (object) (Parser.ConvertTo<Decimal>(lval, opToken) * Parser.ConvertTo<Decimal>(rval, opToken));
        if (!type.Equals(typeof (float)))
        {
          if (!type.Equals(typeof (double)))
          {
            try
            {
              if (type.Equals(typeof (long)))
                return (object) checked (Parser.ConvertTo<long>(lval, opToken) * Parser.ConvertTo<long>(rval, opToken));
              return type.Equals(typeof (ulong)) ? (object) checked (Parser.ConvertTo<ulong>(lval, opToken) * Parser.ConvertTo<ulong>(rval, opToken)) : ParserOps.IntToObject(checked (lvalType != typeof (int) ? Parser.ConvertTo<int>(lval, opToken) : (int) lval * rvalType != typeof (int) ? (int) LanguagePrimitives.ConvertTo(rval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo) : (int) rval));
            }
            catch (OverflowException ex)
            {
              return (object) (Parser.ConvertTo<double>(lval, opToken) * Parser.ConvertTo<double>(rval, opToken));
            }
          }
        }
        return (object) ((lvalType != typeof (double) ? Parser.ConvertTo<double>(lval, opToken) : (double) lval) * (rvalType != typeof (double) ? Parser.ConvertTo<double>(rval, opToken) : (double) rval));
      }
      string s1 = lval as string;
      object obj1 = (object) null;
      if (s1 != null)
      {
        if (s1.Length == 0)
          return (object) string.Empty;
        if (rval is string s2)
          obj1 = ParserOps.StringToNumber(opToken, s2);
        if (obj1 != null)
          rval = obj1;
        int times = Parser.ConvertTo<int>(rval, opToken);
        switch (times)
        {
          case 0:
            return (object) string.Empty;
          case 1:
            return (object) s1;
          default:
            if (times < 0)
              throw new ArgumentOutOfRangeException(nameof (rval));
            if (context.LanguageMode == PSLanguageMode.RestrictedLanguage && s1.Length * times > 1024)
              throw InterpreterError.NewInterpreterException((object) times, typeof (RuntimeException), opToken, "StringMultiplyToolongInDataSection", (object) 1024);
            return (object) ParserOps.PolyMultiplyHelper(s1, times);
        }
      }
      else
      {
        IEnumerator enumerator = LanguagePrimitives.GetEnumerator(lval);
        if (enumerator == null)
          return ParserOps.ImplicitOp(opToken, lval, rval, "op_Multiply");
        if (rval is string)
          obj1 = ParserOps.StringToNumber(opToken, (string) rval);
        if (obj1 != null)
          rval = obj1;
        int times = Parser.ConvertTo<int>(rval, opToken);
        switch (times)
        {
          case 0:
            return (object) new object[0];
          case 1:
            return lval;
          default:
            if (times < 0)
              throw new ArgumentOutOfRangeException(nameof (rval));
            ArrayList arrayList = new ArrayList();
            while (ParserOps.MoveNext(context, opToken, enumerator))
            {
              object obj2 = ParserOps.Current(opToken, enumerator);
              arrayList.Add(obj2);
            }
            object[] array = new object[arrayList.Count];
            arrayList.CopyTo((Array) array);
            return array.Length == 0 ? (object) array : (object) ParserOps.PolyMultiplyHelper(array, times);
        }
      }
    }

    internal static object PolyMinus(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      object lval1 = lval;
      object rval1 = rval;
      lval = PSObject.Base(lval);
      rval = PSObject.Base(rval);
      if (rval == null)
        rval = (object) 0;
      if (lval == null)
        lval = (object) 0;
      Type lvalType = lval.GetType();
      if (!LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(lvalType)) && !lvalType.Equals(typeof (string)) && !lvalType.Equals(typeof (char)))
        return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Subtraction");
      Type rvalType;
      Type type = ParserOps.figureOutOperationType(opToken, ref lval, out lvalType, ref rval, out rvalType);
      if (type == null)
      {
        object obj1 = (object) null;
        object obj2 = (object) null;
        if (lvalType.Equals(typeof (string)))
          obj1 = ParserOps.StringToNumber(opToken, (string) lval);
        else if (lvalType.Equals(typeof (char)))
          obj1 = (object) Convert.ToInt32((char) lval);
        if (rvalType.Equals(typeof (string)))
          obj2 = ParserOps.StringToNumber(opToken, (string) rval);
        else if (rvalType.Equals(typeof (char)))
          obj2 = (object) Convert.ToInt32((char) rval);
        if (obj1 == null || obj2 == null)
          return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Subtraction");
        lval = obj1;
        rval = obj2;
        type = ParserOps.figureOutOperationType(opToken, ref lval, out lvalType, ref rval, out rvalType);
        if (type == null)
          return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Subtraction");
      }
      if (type.Equals(typeof (Decimal)))
        return (object) (Parser.ConvertTo<Decimal>(lval, opToken) - Parser.ConvertTo<Decimal>(rval, opToken));
      if (!type.Equals(typeof (float)))
      {
        if (!type.Equals(typeof (double)))
        {
          try
          {
            if (type.Equals(typeof (long)))
              return (object) checked ((long) LanguagePrimitives.ConvertTo(lval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo) - (long) LanguagePrimitives.ConvertTo(rval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo));
            return type.Equals(typeof (ulong)) ? (object) checked ((ulong) LanguagePrimitives.ConvertTo(lval, typeof (ulong), (IFormatProvider) NumberFormatInfo.InvariantInfo) - (ulong) LanguagePrimitives.ConvertTo(rval, typeof (ulong), (IFormatProvider) NumberFormatInfo.InvariantInfo)) : ParserOps.IntToObject(checked (lvalType != typeof (int) ? Parser.ConvertTo<int>(lval, opToken) : (int) lval - rvalType != typeof (int) ? (int) LanguagePrimitives.ConvertTo(rval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo) : (int) rval));
          }
          catch (OverflowException ex)
          {
            return (object) (Parser.ConvertTo<double>(lval, opToken) - Parser.ConvertTo<double>(rval, opToken));
          }
          catch (PSInvalidCastException ex)
          {
            return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Subtraction");
          }
        }
      }
      return (object) ((lvalType != typeof (double) ? Parser.ConvertTo<double>(lval, opToken) : (double) lval) - (rvalType != typeof (double) ? Parser.ConvertTo<double>(rval, opToken) : (double) rval));
    }

    internal static Type figureOutOperationType(
      Token opToken,
      ref object lval,
      out Type lvalType,
      ref object rval,
      out Type rvalType)
    {
      if (lval == null)
      {
        lval = (object) 0;
        lvalType = typeof (int);
      }
      else
        lvalType = lval.GetType();
      if (rval == null)
      {
        rval = (object) 0;
        rvalType = typeof (int);
      }
      else
        rvalType = rval.GetType();
      int index1 = LanguagePrimitives.TypeTableIndex(lvalType);
      int index2 = LanguagePrimitives.TypeTableIndex(rvalType);
      Type type = (Type) null;
      if (index1 == -1 && index2 == -1)
        return (Type) null;
      if (index1 == -1 && lvalType.Equals(typeof (string)))
      {
        object number = ParserOps.StringToNumber(opToken, (string) lval);
        if (number != null)
        {
          lval = number;
          lvalType = lval.GetType();
          index1 = LanguagePrimitives.TypeTableIndex(lvalType);
        }
        else
          type = rvalType;
      }
      if (index2 == -1 && rvalType.Equals(typeof (string)))
      {
        object number = ParserOps.StringToNumber(opToken, (string) rval);
        if (number != null)
        {
          rval = number;
          rvalType = rval.GetType();
          index2 = LanguagePrimitives.TypeTableIndex(rvalType);
        }
        else
          type = lvalType;
      }
      if (type == null)
      {
        if (index1 != -1 && index2 != -1)
          type = LanguagePrimitives.LargestTypeTable[index1][index2] ?? typeof (Decimal);
        else if (index1 != -1)
          type = lvalType;
        else if (index2 != -1)
          type = rvalType;
      }
      return type;
    }

    internal static object PolyDiv(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      lval = PSObject.Base(lval);
      rval = PSObject.Base(rval);
      object lval1 = lval;
      object rval1 = rval;
      if (lval == null)
        lval = (object) 0;
      Type lvalType = lval.GetType();
      if (!LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(lvalType)) && !lvalType.Equals(typeof (string)) && !lvalType.Equals(typeof (char)))
        return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Division");
      Type rvalType;
      Type type = ParserOps.figureOutOperationType(opToken, ref lval, out lvalType, ref rval, out rvalType);
      if (type == null)
      {
        object obj1 = (object) null;
        object obj2 = (object) null;
        if (lvalType.Equals(typeof (string)))
          obj1 = ParserOps.StringToNumber(opToken, (string) lval);
        else if (lvalType.Equals(typeof (char)))
          obj1 = (object) Convert.ToInt32((char) lval);
        if (rvalType.Equals(typeof (string)))
          obj2 = ParserOps.StringToNumber(opToken, (string) rval);
        else if (rvalType.Equals(typeof (char)))
          obj2 = (object) Convert.ToInt32((char) rval);
        if (obj1 == null || obj2 == null)
          return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Division");
        lval = obj1;
        rval = obj2;
        type = ParserOps.figureOutOperationType(opToken, ref lval, out lvalType, ref rval, out rvalType);
        if (type == null)
          return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Division");
      }
      try
      {
        if (type.Equals(typeof (Decimal)))
          return (object) (Parser.ConvertTo<Decimal>(lval, opToken) / Parser.ConvertTo<Decimal>(rval, opToken));
        if (!type.Equals(typeof (float)))
        {
          if (!type.Equals(typeof (double)))
          {
            try
            {
              object obj1;
              object obj2;
              if (type.Equals(typeof (long)))
              {
                long a = (long) LanguagePrimitives.ConvertTo(lval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo);
                long b = (long) LanguagePrimitives.ConvertTo(rval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo);
                long result;
                long num = Math.DivRem(a, b, out result);
                if (result == 0L)
                  return (object) num;
                obj1 = (object) a;
                obj2 = (object) b;
              }
              else if (type.Equals(typeof (ulong)))
              {
                ulong num1 = (ulong) LanguagePrimitives.ConvertTo(lval, typeof (ulong), (IFormatProvider) NumberFormatInfo.InvariantInfo);
                ulong num2 = (ulong) LanguagePrimitives.ConvertTo(rval, typeof (ulong), (IFormatProvider) NumberFormatInfo.InvariantInfo);
                if (num1 % num2 == 0UL)
                  return (object) (num1 / num2);
                obj1 = (object) num1;
                obj2 = (object) num2;
              }
              else
              {
                int a = (int) LanguagePrimitives.ConvertTo(lval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo);
                int b = (int) LanguagePrimitives.ConvertTo(rval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo);
                int result;
                int num = Math.DivRem(a, b, out result);
                if (result == 0)
                  return ParserOps.IntToObject(num);
                obj1 = (object) a;
                obj2 = (object) b;
              }
              return (object) (Parser.ConvertTo<double>(obj1, opToken) / Parser.ConvertTo<double>(obj2, opToken));
            }
            catch (OverflowException ex)
            {
              return (object) (Parser.ConvertTo<double>(lval, opToken) / Parser.ConvertTo<double>(rval, opToken));
            }
            catch (PSInvalidCastException ex)
            {
              return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Division");
            }
          }
        }
        return (object) (Parser.ConvertTo<double>(lval, opToken) / Parser.ConvertTo<double>(rval, opToken));
      }
      catch (DivideByZeroException ex)
      {
        RuntimeException runtimeException = new RuntimeException(ex.Message, (Exception) ex);
        runtimeException.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, opToken, context));
        ParserOps.tracer.TraceException((Exception) runtimeException);
        throw runtimeException;
      }
    }

    internal static object PolyMod(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      lval = PSObject.Base(lval);
      rval = PSObject.Base(rval);
      object lval1 = lval;
      object rval1 = rval;
      if (lval == null)
        lval = (object) 0;
      Type lvalType = lval.GetType();
      if (!LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(lvalType)) && !lvalType.Equals(typeof (string)) && !lvalType.Equals(typeof (char)))
        return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Modulus");
      Type rvalType;
      Type type = ParserOps.figureOutOperationType(opToken, ref lval, out lvalType, ref rval, out rvalType);
      if (type == null)
      {
        object obj1 = (object) null;
        object obj2 = (object) null;
        if (lvalType.Equals(typeof (string)))
          obj1 = ParserOps.StringToNumber(opToken, (string) lval);
        else if (lvalType.Equals(typeof (char)))
          obj1 = (object) Convert.ToInt32((char) lval);
        if (rvalType.Equals(typeof (string)))
          obj2 = ParserOps.StringToNumber(opToken, (string) rval);
        else if (rvalType.Equals(typeof (char)))
          obj2 = (object) Convert.ToInt32((char) rval);
        if (obj1 == null || obj2 == null)
          return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Modulus");
        lval = obj1;
        rval = obj2;
        type = ParserOps.figureOutOperationType(opToken, ref lval, out lvalType, ref rval, out rvalType);
        if (type == null)
          return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Modulus");
      }
      try
      {
        if (type.Equals(typeof (Decimal)))
          return (object) (Parser.ConvertTo<Decimal>(lval, opToken) % Parser.ConvertTo<Decimal>(rval, opToken));
        if (!type.Equals(typeof (float)))
        {
          if (!type.Equals(typeof (double)))
          {
            try
            {
              if (type.Equals(typeof (long)))
                return (object) ((long) LanguagePrimitives.ConvertTo(lval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo) % (long) LanguagePrimitives.ConvertTo(rval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo));
              return type.Equals(typeof (ulong)) ? (object) ((ulong) LanguagePrimitives.ConvertTo(lval, typeof (ulong), (IFormatProvider) NumberFormatInfo.InvariantInfo) % (ulong) LanguagePrimitives.ConvertTo(rval, typeof (ulong), (IFormatProvider) NumberFormatInfo.InvariantInfo)) : ParserOps.IntToObject((int) LanguagePrimitives.ConvertTo(lval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo) % (int) LanguagePrimitives.ConvertTo(rval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo));
            }
            catch (OverflowException ex)
            {
              return (object) (Parser.ConvertTo<double>(lval, opToken) % Parser.ConvertTo<double>(rval, opToken));
            }
            catch (PSInvalidCastException ex)
            {
              return ParserOps.ImplicitOp(opToken, lval1, rval1, "op_Modulus");
            }
          }
        }
        return (object) (Parser.ConvertTo<double>(lval, opToken) % Parser.ConvertTo<double>(rval, opToken));
      }
      catch (DivideByZeroException ex)
      {
        throw InterpreterError.NewInterpreterExceptionByMessage(typeof (RuntimeException), opToken, ex.Message, "DivideByZeroException", (Exception) ex);
      }
    }

    internal static object AsOperator(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      if (!(rval is Type resultType))
        resultType = Parser.ConvertTo<Type>(rval, opToken);
      if (resultType == null)
        throw InterpreterError.NewInterpreterException(rval, typeof (RuntimeException), opToken, "AsOperatorRequiresType");
      try
      {
        return LanguagePrimitives.ConvertTo(lval, resultType, (IFormatProvider) NumberFormatInfo.InvariantInfo);
      }
      catch (PSInvalidCastException ex)
      {
        return (object) null;
      }
    }

    internal static object formatOperator(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      string stringParser = PSObject.ToStringParser(context, lval);
      string str = "";
      if (stringParser != null)
      {
        object[] objArray = rval as object[];
        try
        {
          str = objArray == null ? StringUtil.Format(stringParser, rval) : StringUtil.Format(stringParser, objArray);
        }
        catch (FormatException ex)
        {
          RuntimeException runtimeException = InterpreterError.NewInterpreterException(rval, typeof (RuntimeException), opToken, "FormatError", (object) ex.Message);
          ParserOps.tracer.TraceException((Exception) runtimeException);
          throw runtimeException;
        }
      }
      return (object) str;
    }

    internal static object bandOperator(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      lval = PSObject.Base(lval);
      rval = PSObject.Base(rval);
      Type type = ParserOps.figureOutOperationType(opToken, ref lval, out Type _, ref rval, out Type _);
      return type != null && type.Equals(typeof (int)) ? ParserOps.IntToObject((int) LanguagePrimitives.ConvertTo(lval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo) & (int) LanguagePrimitives.ConvertTo(rval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo)) : (object) ((long) LanguagePrimitives.ConvertTo(lval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo) & (long) LanguagePrimitives.ConvertTo(rval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo));
    }

    internal static object borOperator(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      lval = PSObject.Base(lval);
      rval = PSObject.Base(rval);
      Type type = ParserOps.figureOutOperationType(opToken, ref lval, out Type _, ref rval, out Type _);
      return type != null && type.Equals(typeof (int)) ? ParserOps.IntToObject((int) LanguagePrimitives.ConvertTo(lval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo) | (int) LanguagePrimitives.ConvertTo(rval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo)) : (object) ((long) LanguagePrimitives.ConvertTo(lval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo) | (long) LanguagePrimitives.ConvertTo(rval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo));
    }

    internal static object bxorOperator(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      lval = PSObject.Base(lval);
      rval = PSObject.Base(rval);
      Type type = ParserOps.figureOutOperationType(opToken, ref lval, out Type _, ref rval, out Type _);
      return type != null && type.Equals(typeof (int)) ? ParserOps.IntToObject((int) LanguagePrimitives.ConvertTo(lval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo) ^ (int) LanguagePrimitives.ConvertTo(rval, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo)) : (object) ((long) LanguagePrimitives.ConvertTo(lval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo) ^ (long) LanguagePrimitives.ConvertTo(rval, typeof (long), (IFormatProvider) NumberFormatInfo.InvariantInfo));
    }

    internal static object andOperator(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      if (!LanguagePrimitives.IsTrue(lval))
        return ParserOps._FalseObject;
      if (rval is ParseTreeNode parseTreeNode)
      {
        if (!LanguagePrimitives.IsTrue(parseTreeNode.Execute((Array) null, (Pipe) null, context)))
          return ParserOps._FalseObject;
      }
      else if (!LanguagePrimitives.IsTrue(rval))
        return ParserOps._FalseObject;
      return ParserOps._TrueObject;
    }

    internal static object orOperator(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      if (LanguagePrimitives.IsTrue(lval))
        return ParserOps._TrueObject;
      if (rval is ParseTreeNode parseTreeNode)
      {
        if (LanguagePrimitives.IsTrue(parseTreeNode.Execute((Array) null, (Pipe) null, context)))
          return ParserOps._TrueObject;
      }
      else if (LanguagePrimitives.IsTrue(rval))
        return ParserOps._TrueObject;
      return ParserOps._FalseObject;
    }

    internal static object xorOperator(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      return ParserOps.BoolToObject(LanguagePrimitives.IsTrue(lval) != LanguagePrimitives.IsTrue(rval));
    }

    internal static object rangeOperator(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      int num1 = ParserOps.FixNum(lval, opToken);
      int num2 = ParserOps.FixNum(rval, opToken);
      if (num1 == num2)
        return (object) new object[1]{ (object) num1 };
      object[] objArray = new object[Math.Abs(num2 - num1) + 1];
      if (num1 > num2)
      {
        for (int index = 0; index < objArray.Length; ++index)
          objArray[index] = (object) num1--;
      }
      else
      {
        for (int index = 0; index < objArray.Length; ++index)
          objArray[index] = (object) num1++;
      }
      return (object) objArray;
    }

    private static object[] unfoldTuple(ExecutionContext context, Token opToken, object tuple)
    {
      List<object> objectList = new List<object>();
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(tuple);
      if (enumerator != null)
      {
        while (ParserOps.MoveNext(context, opToken, enumerator))
        {
          object obj = ParserOps.Current(opToken, enumerator);
          objectList.Add(obj);
        }
      }
      else
        objectList.Add(tuple);
      return objectList.ToArray();
    }

    private static IEnumerable<string> enumerateContent(
      ExecutionContext context,
      Token opToken,
      ParserOps.SplitImplOptions implOptions,
      object tuple)
    {
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(tuple);
      if (enumerator == null)
        enumerator = new object[1]{ tuple }.GetEnumerator();
      while (ParserOps.MoveNext(context, opToken, enumerator))
      {
        string strValue = PSObject.ToStringParser(context, enumerator.Current);
        if ((implOptions & ParserOps.SplitImplOptions.TrimContent) != ParserOps.SplitImplOptions.None)
          strValue = strValue.Trim();
        yield return strValue;
      }
    }

    private static RegexOptions parseRegexOptions(SplitOptions options)
    {
      int[][] numArray1 = new int[6][]
      {
        new int[2]{ 4, 512 },
        new int[2]{ 8, 32 },
        new int[2]{ 16, 2 },
        new int[2]{ 32, 16 },
        new int[2]{ 64, 1 },
        new int[2]{ 128, 4 }
      };
      RegexOptions regexOptions = RegexOptions.None;
      foreach (int[] numArray2 in numArray1)
      {
        if ((options & (SplitOptions) numArray2[0]) != (SplitOptions) 0)
          regexOptions |= (RegexOptions) numArray2[1];
      }
      return regexOptions;
    }

    internal static object SplitOperator(ExecutionContext context, Token opToken, object lval) => ParserOps.SplitOperator(context, opToken, lval, (object) new object[1]
    {
      (object) "\\s+"
    }, ParserOps.SplitImplOptions.TrimContent);

    internal static object SplitOperator(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval)
    {
      return ParserOps.SplitOperator(context, opToken, lval, rval, ParserOps.SplitImplOptions.None);
    }

    private static void ExtendList<T>(IList<T> list, IList<T> items)
    {
      foreach (T obj in (IEnumerable<T>) items)
        list.Add(obj);
    }

    private static object SplitOperator(
      ExecutionContext context,
      Token opToken,
      object lval,
      object rval,
      ParserOps.SplitImplOptions implOptions)
    {
      IEnumerable<string> content = ParserOps.enumerateContent(context, opToken, implOptions, lval);
      string separatorPattern = (string) null;
      int limit = 0;
      SplitOptions options = (SplitOptions) 0;
      object[] objArray = ParserOps.unfoldTuple(context, opToken, rval);
      object obj = objArray.Length >= 1 ? objArray[0] : throw InterpreterError.NewInterpreterException(rval, typeof (RuntimeException), opToken, "BadOperatorArgument", (object) "-split", rval);
      if (!(objArray[0] is ScriptBlock predicate))
        separatorPattern = PSObject.ToStringParser(context, objArray[0]);
      if (objArray.Length >= 2)
        limit = ParserOps.FixNum(objArray[1], opToken);
      if (objArray.Length >= 3 && objArray[2] != null && (!(objArray[2] is string str) || !string.IsNullOrEmpty(str)))
      {
        options = Parser.ConvertTo<SplitOptions>(objArray[2], opToken);
        if (predicate != null)
          throw InterpreterError.NewInterpreterException((object) null, typeof (ParseException), opToken, "InvalidSplitOptionWithPredicate");
      }
      return predicate != null ? ParserOps.SplitWithPredicate(context, opToken, content, predicate, limit) : ParserOps.SplitWithPattern(context, opToken, content, separatorPattern, limit, options);
    }

    private static object SplitWithPredicate(
      ExecutionContext context,
      Token opToken,
      IEnumerable<string> content,
      ScriptBlock predicate,
      int limit)
    {
      List<string> stringList1 = new List<string>();
      foreach (string str in content)
      {
        List<string> stringList2 = new List<string>();
        if (limit == 1)
        {
          stringList1.Add(str);
        }
        else
        {
          StringBuilder stringBuilder = new StringBuilder();
          for (int index = 0; index < str.Length; ++index)
          {
            if (LanguagePrimitives.IsTrue(predicate.DoInvokeReturnAsIs((object) ParserOps.CharToString(str[index]), (object) AutomationNull.Value, (object) str, (object) index)))
            {
              stringList2.Add(stringBuilder.ToString());
              stringBuilder = new StringBuilder();
              if (limit > 0 && stringList2.Count >= limit - 1)
              {
                if (index + 1 < str.Length)
                {
                  stringList2.Add(str.Substring(index + 1));
                  break;
                }
                stringList2.Add("");
                break;
              }
              if (index == str.Length - 1)
                stringList2.Add("");
            }
            else
              stringBuilder.Append(str[index]);
          }
          if (stringBuilder.Length > 0 && (limit <= 0 || stringList2.Count < limit))
            stringList2.Add(stringBuilder.ToString());
          ParserOps.ExtendList<string>((IList<string>) stringList1, (IList<string>) stringList2);
        }
      }
      return (object) stringList1.ToArray();
    }

    private static object SplitWithPattern(
      ExecutionContext context,
      Token opToken,
      IEnumerable<string> content,
      string separatorPattern,
      int limit,
      SplitOptions options)
    {
      if ((options & SplitOptions.SimpleMatch) == (SplitOptions) 0 && (options & SplitOptions.RegexMatch) == (SplitOptions) 0)
        options |= SplitOptions.RegexMatch;
      if ((options & SplitOptions.SimpleMatch) != (SplitOptions) 0 && (options & ~(SplitOptions.SimpleMatch | SplitOptions.IgnoreCase)) != (SplitOptions) 0)
        throw InterpreterError.NewInterpreterException((object) null, typeof (ParseException), opToken, "InvalidSplitOptionCombination");
      if ((options & (SplitOptions.Multiline | SplitOptions.Singleline)) == (SplitOptions.Multiline | SplitOptions.Singleline))
        throw InterpreterError.NewInterpreterException((object) null, typeof (ParseException), opToken, "InvalidSplitOptionCombination");
      if ((options & SplitOptions.SimpleMatch) != (SplitOptions) 0)
        separatorPattern = Regex.Escape(separatorPattern);
      if ((options & SplitOptions.IgnoreCase) == (SplitOptions) 0 && opToken is ComparisonToken comparisonToken && comparisonToken.IgnoreCase)
        options |= SplitOptions.IgnoreCase;
      if (limit < 0)
        limit = 0;
      RegexOptions regexOptions = ParserOps.parseRegexOptions(options);
      Regex regex = ParserOps.NewRegex(separatorPattern, regexOptions);
      List<string> stringList = new List<string>();
      foreach (string input in content)
      {
        string[] strArray = regex.Split(input, limit, 0);
        ParserOps.ExtendList<string>((IList<string>) stringList, (IList<string>) strArray);
      }
      return (object) stringList.ToArray();
    }

    internal static object JoinOperator(ExecutionContext context, Token op, object lval) => ParserOps.JoinOperator(context, op, lval, (object) "");

    internal static object JoinOperator(
      ExecutionContext context,
      Token op,
      object lval,
      object rval)
    {
      string stringParser = PSObject.ToStringParser(context, rval);
      IEnumerable enumerable = LanguagePrimitives.GetEnumerable(lval);
      return enumerable != null ? (object) PSObject.ToStringEnumerable(context, enumerable, stringParser, (string) null, (IFormatProvider) null) : (object) PSObject.ToStringParser(context, lval);
    }

    internal static object ReplaceOperator(
      ExecutionContext context,
      Token op,
      object lval,
      object rval)
    {
      ComparisonToken comparisonToken = op as ComparisonToken;
      string replacement = "";
      object targetObject = (object) "";
      rval = PSObject.Base(rval);
      if (rval is IList list)
      {
        if (list.Count > 2)
          throw InterpreterError.NewInterpreterException(rval, typeof (RuntimeException), op, "BadReplaceArgument", (object) op.TokenText, (object) list.Count);
        if (list.Count > 0)
        {
          targetObject = list[0];
          if (list.Count > 1)
            replacement = PSObject.ToStringParser(context, list[1]);
        }
      }
      else
        targetObject = rval;
      RegexOptions options = RegexOptions.None;
      if (comparisonToken != null && comparisonToken.IgnoreCase)
        options = RegexOptions.IgnoreCase;
      if (!(targetObject is Regex regex))
      {
        try
        {
          regex = ParserOps.NewRegex(PSObject.ToStringParser(context, targetObject), options);
        }
        catch (ArgumentException ex)
        {
          throw InterpreterError.NewInterpreterExceptionWithInnerException(targetObject, typeof (RuntimeException), (Token) null, "InvalidRegularExpression", (Exception) ex, targetObject);
        }
      }
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(lval);
      if (enumerator == null)
      {
        string input = (lval == null ? (object) string.Empty : lval).ToString();
        return (object) regex.Replace(input, replacement);
      }
      ArrayList arrayList = new ArrayList();
      while (ParserOps.MoveNext(context, op, enumerator))
      {
        string stringParser = PSObject.ToStringParser(context, ParserOps.Current(op, enumerator));
        arrayList.Add((object) regex.Replace(stringParser, replacement));
      }
      return (object) arrayList.ToArray();
    }

    internal static object IsOperator(
      ExecutionContext context,
      Token op,
      object left,
      object right)
    {
      object o = PSObject.Base(left);
      object targetObject = PSObject.Base(right);
      if (!(targetObject is Type type))
      {
        type = Parser.ConvertTo<Type>(targetObject, op);
        if (type == null)
          throw InterpreterError.NewInterpreterException(targetObject, typeof (RuntimeException), op, "IsOperatorRequiresType");
      }
      ComparisonToken comparisonToken = op as ComparisonToken;
      bool flag = true;
      if (comparisonToken == null || comparisonToken.ComparisonName.Equals("-is", StringComparison.OrdinalIgnoreCase))
        flag = false;
      return type == typeof (PSCustomObject) && o is PSObject || type.Equals(typeof (PSObject)) && left is PSObject ? ParserOps.BoolToObject(!flag) : ParserOps.BoolToObject(type.IsInstanceOfType(o) ^ flag);
    }

    internal static object LikeOperator(
      ExecutionContext context,
      Token op,
      object lval,
      object rval)
    {
      WildcardOptions options = WildcardOptions.IgnoreCase;
      ComparisonToken comparisonToken = op as ComparisonToken;
      bool flag = false;
      if (comparisonToken != null)
      {
        if (!comparisonToken.ComparisonName.Equals("-like", StringComparison.OrdinalIgnoreCase))
          flag = true;
        if (!comparisonToken.IgnoreCase)
          options = WildcardOptions.None;
      }
      WildcardPattern wildcardPattern = new WildcardPattern(PSObject.ToStringParser(context, rval), options);
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(lval);
      if (enumerator == null)
      {
        string input = lval == null ? string.Empty : PSObject.ToStringParser(context, lval);
        return ParserOps.BoolToObject(wildcardPattern.IsMatch(input) ^ flag);
      }
      ArrayList arrayList = new ArrayList();
      while (ParserOps.MoveNext(context, op, enumerator))
      {
        object obj = ParserOps.Current(op, enumerator);
        string input = obj == null ? string.Empty : PSObject.ToStringParser(context, obj);
        if (wildcardPattern.IsMatch(input) ^ flag)
          arrayList.Add((object) input);
      }
      return (object) arrayList.ToArray();
    }

    internal static object MatchOperator(
      ExecutionContext context,
      Token op,
      object lval,
      object rval)
    {
      RegexOptions options = RegexOptions.IgnoreCase;
      ComparisonToken comparisonToken = op as ComparisonToken;
      bool flag = false;
      if (comparisonToken != null)
      {
        if (!comparisonToken.ComparisonName.Equals("-match", StringComparison.OrdinalIgnoreCase))
          flag = true;
        if (!comparisonToken.IgnoreCase)
          options = RegexOptions.None;
      }
      if (!(PSObject.Base(rval) is Regex regex))
        regex = ParserOps.NewRegex(PSObject.ToStringParser(context, rval), options);
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(lval);
      if (enumerator == null)
      {
        string input = lval == null ? string.Empty : PSObject.ToStringParser(context, lval);
        Match match = regex.Match(input);
        if (match.Success)
        {
          GroupCollection groups = match.Groups;
          if (groups.Count > 0)
          {
            Hashtable hashtable = new Hashtable((IEqualityComparer) StringComparer.CurrentCultureIgnoreCase);
            foreach (string groupName in regex.GetGroupNames())
            {
              Group group = groups[groupName];
              if (group.Success)
              {
                int result;
                if (int.TryParse(groupName, out result))
                  hashtable.Add((object) result, (object) group.ToString());
                else
                  hashtable.Add((object) groupName, (object) group.ToString());
              }
            }
            context.SetVariable("Matches", (object) hashtable);
          }
        }
        return ParserOps.BoolToObject(match.Success ^ flag);
      }
      ArrayList arrayList = new ArrayList();
      int num = 0;
      try
      {
        while (enumerator.MoveNext())
        {
          object current = enumerator.Current;
          string input = current == null ? string.Empty : PSObject.ToStringParser(context, current);
          if (regex.Match(input).Success ^ flag)
            arrayList.Add(current);
          if (num++ > 1000)
          {
            if (context != null && context.CurrentPipelineStopping)
              throw new PipelineStoppedException();
            num = 0;
          }
        }
        return (object) arrayList.ToArray();
      }
      catch (RuntimeException ex)
      {
        throw;
      }
      catch (FlowControlException ex)
      {
        throw;
      }
      catch (ScriptCallDepthException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw InterpreterError.NewInterpreterExceptionWithInnerException((object) enumerator, typeof (RuntimeException), op, "BadEnumeration", ex, (object) ex.Message);
      }
    }

    internal static object ComparisonOperators(
      ExecutionContext context,
      Token op,
      object lval,
      object rval)
    {
      ComparisonToken op1 = op as ComparisonToken;
      bool flag = op1.ComparisonName.Equals("-contains", StringComparison.OrdinalIgnoreCase);
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(lval);
      if (enumerator == null)
        return (object) ParserOps.CompareScalar(op1, lval, rval);
      if (string.Equals(op1.ComparisonName, "-contains", StringComparison.Ordinal) || string.Equals(op1.ComparisonName, "-notcontains", StringComparison.Ordinal))
      {
        while (ParserOps.MoveNext(context, (Token) op1, enumerator))
        {
          if (LanguagePrimitives.Equals(ParserOps.Current((Token) op1, enumerator), rval, op1.IgnoreCase, (IFormatProvider) CultureInfo.InvariantCulture))
            return ParserOps.BoolToObject(flag);
        }
        return ParserOps.BoolToObject(!flag);
      }
      ArrayList arrayList = new ArrayList();
      while (ParserOps.MoveNext(context, (Token) op1, enumerator))
      {
        object leftOperand = ParserOps.Current((Token) op1, enumerator);
        if (ParserOps.CompareScalar(op1, leftOperand, rval))
          arrayList.Add(leftOperand);
      }
      return (object) arrayList.ToArray();
    }

    private static bool CompareScalar(ComparisonToken op, object leftOperand, object rightOperand)
    {
      switch (op.ComparisonName)
      {
        case "-eq":
        case "-contains":
          return LanguagePrimitives.Equals(leftOperand, rightOperand, op.IgnoreCase, (IFormatProvider) CultureInfo.InvariantCulture);
        case "-ne":
        case "-notcontains":
          return !LanguagePrimitives.Equals(leftOperand, rightOperand, op.IgnoreCase, (IFormatProvider) CultureInfo.InvariantCulture);
        case "-gt":
          return LanguagePrimitives.Compare(leftOperand, rightOperand, op.IgnoreCase, (IFormatProvider) CultureInfo.InvariantCulture) > 0;
        case "-ge":
          return LanguagePrimitives.Compare(leftOperand, rightOperand, op.IgnoreCase, (IFormatProvider) CultureInfo.InvariantCulture) >= 0;
        case "-lt":
          return LanguagePrimitives.Compare(leftOperand, rightOperand, op.IgnoreCase, (IFormatProvider) CultureInfo.InvariantCulture) < 0;
        case "-le":
          return LanguagePrimitives.Compare(leftOperand, rightOperand, op.IgnoreCase, (IFormatProvider) CultureInfo.InvariantCulture) <= 0;
        default:
          return false;
      }
    }

    internal static Regex NewRegex(string patternString, RegexOptions options)
    {
      if (options != RegexOptions.IgnoreCase)
        return new Regex(patternString, options);
      lock (ParserOps._regexCache)
      {
        if (ParserOps._regexCache.ContainsKey(patternString))
          return ParserOps._regexCache[patternString];
        if (ParserOps._regexCache.Count > 1000)
          ParserOps._regexCache.Clear();
        Regex regex = new Regex(patternString, RegexOptions.IgnoreCase);
        ParserOps._regexCache.Add(patternString, regex);
        return regex;
      }
    }

    internal static bool MoveNext(ExecutionContext context, Token token, IEnumerator enumerator)
    {
      try
      {
        if (context != null && context.CurrentPipelineStopping)
          throw new PipelineStoppedException();
        return enumerator.MoveNext();
      }
      catch (RuntimeException ex)
      {
        throw;
      }
      catch (FlowControlException ex)
      {
        throw;
      }
      catch (ScriptCallDepthException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw InterpreterError.NewInterpreterExceptionWithInnerException((object) enumerator, typeof (RuntimeException), token, "BadEnumeration", ex, (object) ex.Message);
      }
    }

    internal static object Current(Token token, IEnumerator enumerator)
    {
      try
      {
        return enumerator.Current;
      }
      catch (RuntimeException ex)
      {
        throw;
      }
      catch (ScriptCallDepthException ex)
      {
        throw;
      }
      catch (FlowControlException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw InterpreterError.NewInterpreterExceptionWithInnerException((object) enumerator, typeof (RuntimeException), token, "BadEnumeration", ex, (object) ex.Message);
      }
    }

    internal static string GetTypeFullName(object obj)
    {
      if (obj == null)
        return string.Empty;
      if (!(obj is PSObject psObject))
        return obj.GetType().FullName;
      return psObject.TypeNames.Count == 0 ? typeof (PSObject).FullName : psObject.TypeNames[0];
    }

    internal static object CallMethod(
      Token token,
      object target,
      string methodName,
      object[] paramArray,
      bool callStatic,
      object valueToSet)
    {
      PSMethodInfo psMethodInfo = (PSMethodInfo) null;
      object target1 = !LanguagePrimitives.IsNull(target) ? PSObject.Base(target) : throw InterpreterError.NewInterpreterException((object) methodName, typeof (RuntimeException), token, "InvokeMethodOnNull");
      CallsiteCacheEntryFlags flags = CallsiteCacheEntryFlags.None;
      Type targetType;
      if (callStatic)
      {
        flags |= CallsiteCacheEntryFlags.Static;
        targetType = (Type) target1;
      }
      else
        targetType = target1.GetType();
      if (valueToSet != AutomationNull.Value)
        flags |= CallsiteCacheEntryFlags.ParameterizedSetter;
      MethodInformation cachedMethod = Adapter.FindCachedMethod(targetType, methodName, paramArray, flags);
      if (cachedMethod == null)
      {
        psMethodInfo = !callStatic ? PSObject.AsPSObject(target).Members[methodName] as PSMethodInfo : (PSMethodInfo) (PSObject.GetStaticCLRMember(target, methodName) as PSMethod);
        if (psMethodInfo == null)
        {
          string str = !callStatic ? ParserOps.GetTypeFullName(target) : targetType.FullName;
          if (valueToSet == AutomationNull.Value)
            throw InterpreterError.NewInterpreterException((object) methodName, typeof (RuntimeException), token, "MethodNotFound", (object) str, (object) methodName);
          throw InterpreterError.NewInterpreterException((object) methodName, typeof (RuntimeException), token, "ParameterizedPropertyAssignmentFailed", (object) str, (object) methodName);
        }
      }
      try
      {
        if (cachedMethod != null)
        {
          PSObject.memberResolution.WriteLine("cache hit, Calling Method: {0}", (object) cachedMethod.methodDefinition);
          if (valueToSet != AutomationNull.Value)
          {
            DotNetAdapter.ParameterizedPropertyInvokeSet(methodName, target1, valueToSet, new MethodInformation[1]
            {
              cachedMethod
            }, paramArray, false);
            return valueToSet;
          }
          MethodInformation[] methods = new MethodInformation[1]
          {
            cachedMethod
          };
          object[] newArguments;
          Adapter.GetBestMethodAndArguments(methodName, methods, paramArray, out newArguments);
          return DotNetAdapter.AuxiliaryMethodInvoke(target1, newArguments, cachedMethod, paramArray);
        }
        if (valueToSet == AutomationNull.Value)
          return psMethodInfo.Invoke(paramArray);
        if (!(psMethodInfo is PSParameterizedProperty parameterizedProperty))
          throw InterpreterError.NewInterpreterException((object) methodName, typeof (RuntimeException), token, "ParameterizedPropertyAssignmentFailed", (object) ParserOps.GetTypeFullName(target), (object) methodName);
        parameterizedProperty.InvokeSet(valueToSet, paramArray);
        return valueToSet;
      }
      catch (MethodInvocationException ex)
      {
        if (ex.ErrorRecord.InvocationInfo == null)
          ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, token));
        throw;
      }
      catch (RuntimeException ex)
      {
        if (ex.ErrorRecord.InvocationInfo == null)
          ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, token));
        throw;
      }
      catch (FlowControlException ex)
      {
        throw;
      }
      catch (ScriptCallDepthException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw InterpreterError.NewInterpreterExceptionByMessage(typeof (RuntimeException), token, ex.Message, "MethodInvocationException", ex);
      }
    }

    [System.Flags]
    private enum SplitImplOptions
    {
      None = 0,
      TrimContent = 1,
    }
  }
}
