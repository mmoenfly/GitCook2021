// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.LanguagePrimitives
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace System.Management.Automation
{
  public static class LanguagePrimitives
  {
    private const string NotIcomparable = "NotIcomparable";
    private const string ComparisonFailure = "ComparisonFailure";
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private static Dictionary<Type, LanguagePrimitives.GetEnumerableDelegate> getEnumerableCache = new Dictionary<Type, LanguagePrimitives.GetEnumerableDelegate>(32);
    internal static Type[][] LargestTypeTable = new Type[11][]
    {
      new Type[11]
      {
        typeof (short),
        typeof (int),
        typeof (long),
        typeof (int),
        typeof (long),
        typeof (double),
        typeof (short),
        typeof (short),
        typeof (float),
        typeof (double),
        typeof (Decimal)
      },
      new Type[11]
      {
        typeof (int),
        typeof (int),
        typeof (long),
        typeof (int),
        typeof (long),
        typeof (double),
        typeof (int),
        typeof (int),
        typeof (double),
        typeof (double),
        typeof (Decimal)
      },
      new Type[11]
      {
        typeof (long),
        typeof (long),
        typeof (long),
        typeof (long),
        typeof (long),
        typeof (Decimal),
        typeof (long),
        typeof (long),
        typeof (double),
        typeof (double),
        typeof (Decimal)
      },
      new Type[11]
      {
        typeof (int),
        typeof (int),
        typeof (long),
        typeof (ushort),
        typeof (uint),
        typeof (ulong),
        typeof (int),
        typeof (ushort),
        typeof (float),
        typeof (double),
        typeof (Decimal)
      },
      new Type[11]
      {
        typeof (long),
        typeof (long),
        typeof (long),
        typeof (uint),
        typeof (uint),
        typeof (ulong),
        typeof (long),
        typeof (uint),
        typeof (double),
        typeof (double),
        typeof (Decimal)
      },
      new Type[11]
      {
        typeof (double),
        typeof (double),
        typeof (Decimal),
        typeof (ulong),
        typeof (ulong),
        typeof (ulong),
        typeof (double),
        typeof (ulong),
        typeof (double),
        typeof (double),
        typeof (Decimal)
      },
      new Type[11]
      {
        typeof (short),
        typeof (int),
        typeof (long),
        typeof (int),
        typeof (long),
        typeof (double),
        typeof (sbyte),
        typeof (short),
        typeof (float),
        typeof (double),
        typeof (Decimal)
      },
      new Type[11]
      {
        typeof (short),
        typeof (int),
        typeof (long),
        typeof (ushort),
        typeof (uint),
        typeof (ulong),
        typeof (short),
        typeof (byte),
        typeof (float),
        typeof (double),
        typeof (Decimal)
      },
      new Type[11]
      {
        typeof (float),
        typeof (double),
        typeof (double),
        typeof (float),
        typeof (double),
        typeof (double),
        typeof (float),
        typeof (float),
        typeof (float),
        typeof (double),
        null
      },
      new Type[11]
      {
        typeof (double),
        typeof (double),
        typeof (double),
        typeof (double),
        typeof (double),
        typeof (double),
        typeof (double),
        typeof (double),
        typeof (double),
        typeof (double),
        null
      },
      new Type[11]
      {
        typeof (Decimal),
        typeof (Decimal),
        typeof (Decimal),
        typeof (Decimal),
        typeof (Decimal),
        typeof (Decimal),
        typeof (Decimal),
        typeof (Decimal),
        null,
        null,
        typeof (Decimal)
      }
    };
    internal static PSTraceSource typeConversion = PSTraceSource.GetTracer("TypeConversion", "Traces the type conversion algorithm", false);
    internal static StringToTypeCache stringToTypeCache = new StringToTypeCache();
    internal static StringToAttributeCache stringToAttributeCache = new StringToAttributeCache();
    private static Dictionary<LanguagePrimitives.ConversionTypePair, LanguagePrimitives.ConversionData> converterCache = new Dictionary<LanguagePrimitives.ConversionTypePair, LanguagePrimitives.ConversionData>(256);
    private static Type[] NumericTypes = new Type[11]
    {
      typeof (short),
      typeof (int),
      typeof (long),
      typeof (ushort),
      typeof (uint),
      typeof (ulong),
      typeof (sbyte),
      typeof (byte),
      typeof (float),
      typeof (double),
      typeof (Decimal)
    };
    private static Type[] IntegerTypes = new Type[8]
    {
      typeof (short),
      typeof (int),
      typeof (long),
      typeof (ushort),
      typeof (uint),
      typeof (ulong),
      typeof (sbyte),
      typeof (byte)
    };
    private static Type[] SignedIntegerTypes = new Type[4]
    {
      typeof (sbyte),
      typeof (short),
      typeof (int),
      typeof (long)
    };
    private static Type[] UnsignedIntegerTypes = new Type[4]
    {
      typeof (byte),
      typeof (ushort),
      typeof (uint),
      typeof (ulong)
    };
    private static Type[] RealTypes = new Type[3]
    {
      typeof (float),
      typeof (double),
      typeof (Decimal)
    };
    private static Dictionary<string, bool> possibleTypeConverter = new Dictionary<string, bool>(16);

    static LanguagePrimitives()
    {
      LanguagePrimitives.ResetCaches((TypeTable) null);
      LanguagePrimitives.InitializeGetEnumerableCache();
    }

    internal static void ResetCaches(TypeTable typeTable)
    {
      LanguagePrimitives.RebuildConversionCache();
      if (typeTable == null)
        return;
      lock (LanguagePrimitives.possibleTypeConverter)
        typeTable.ForEachTypeConverter((Action<string>) (x => LanguagePrimitives.possibleTypeConverter[x] = true));
    }

    private static IEnumerable GetEnumerableFromIEnumerableT(object obj)
    {
      foreach (Type enumerableType in obj.GetType().GetInterfaces())
      {
        if (enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
          return (IEnumerable) new LanguagePrimitives.EnumerableTWrapper(obj, enumerableType);
      }
      return (IEnumerable) null;
    }

    private static LanguagePrimitives.GetEnumerableDelegate GetOrCalculateEnumerable(
      Type type)
    {
      LanguagePrimitives.GetEnumerableDelegate enumerableDelegate = (LanguagePrimitives.GetEnumerableDelegate) null;
      lock (LanguagePrimitives.getEnumerableCache)
      {
        if (!LanguagePrimitives.getEnumerableCache.TryGetValue(type, out enumerableDelegate))
        {
          enumerableDelegate = LanguagePrimitives.CalculateGetEnumerable(type);
          LanguagePrimitives.getEnumerableCache.Add(type, enumerableDelegate);
        }
      }
      return enumerableDelegate;
    }

    private static void InitializeGetEnumerableCache()
    {
      lock (LanguagePrimitives.getEnumerableCache)
      {
        LanguagePrimitives.getEnumerableCache.Clear();
        LanguagePrimitives.getEnumerableCache.Add(typeof (string), new LanguagePrimitives.GetEnumerableDelegate(LanguagePrimitives.ReturnNullEnumerable));
        LanguagePrimitives.getEnumerableCache.Add(typeof (int), new LanguagePrimitives.GetEnumerableDelegate(LanguagePrimitives.ReturnNullEnumerable));
        LanguagePrimitives.getEnumerableCache.Add(typeof (double), new LanguagePrimitives.GetEnumerableDelegate(LanguagePrimitives.ReturnNullEnumerable));
      }
    }

    internal static bool IsTypeEnumerable(Type type) => LanguagePrimitives.GetOrCalculateEnumerable(type) != new LanguagePrimitives.GetEnumerableDelegate(LanguagePrimitives.ReturnNullEnumerable);

    public static IEnumerable GetEnumerable(object obj)
    {
      if (obj == null)
        return (IEnumerable) null;
      Type type = obj.GetType();
      if (type == typeof (PSObject))
      {
        obj = ((PSObject) obj).BaseObject;
        type = obj.GetType();
      }
      return LanguagePrimitives.GetOrCalculateEnumerable(type)(obj);
    }

    private static IEnumerable ReturnNullEnumerable(object obj) => (IEnumerable) null;

    private static IEnumerable DataTableEnumerable(object obj) => (IEnumerable) ((DataTable) obj).Rows;

    private static IEnumerable TypicalEnumerable(object obj)
    {
      IEnumerable enumerable = (IEnumerable) obj;
      try
      {
        return enumerable.GetEnumerator() == null ? LanguagePrimitives.GetEnumerableFromIEnumerableT(obj) : enumerable;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        return LanguagePrimitives.GetEnumerableFromIEnumerableT(obj) ?? throw new ExtendedTypeSystemException("ExceptionInGetEnumerator", ex, "ExtendedTypeSystem", "EnumerationException", new object[1]
        {
          (object) ex.Message
        });
      }
    }

    private static LanguagePrimitives.GetEnumerableDelegate CalculateGetEnumerable(
      Type objectType)
    {
      if (typeof (DataTable).IsAssignableFrom(objectType))
        return new LanguagePrimitives.GetEnumerableDelegate(LanguagePrimitives.DataTableEnumerable);
      return typeof (IEnumerable).IsAssignableFrom(objectType) && !typeof (IDictionary).IsAssignableFrom(objectType) && !typeof (XmlNode).IsAssignableFrom(objectType) ? new LanguagePrimitives.GetEnumerableDelegate(LanguagePrimitives.TypicalEnumerable) : new LanguagePrimitives.GetEnumerableDelegate(LanguagePrimitives.ReturnNullEnumerable);
    }

    private static IEnumerator GetEnumeratorFromIEnumeratorT(object obj)
    {
      foreach (Type enumerableType in obj.GetType().GetInterfaces())
      {
        if (enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
        {
          LanguagePrimitives.EnumerableTWrapper enumerableTwrapper = new LanguagePrimitives.EnumerableTWrapper(obj, enumerableType);
          return (IEnumerator) new LanguagePrimitives.EnumeratorTWrapper((object) enumerableTwrapper.GetEnumerator(), enumerableTwrapper.EnumeratorType);
        }
      }
      return (IEnumerator) null;
    }

    public static IEnumerator GetEnumerator(object obj)
    {
      if (obj == null)
        return (IEnumerator) null;
      Type type = obj.GetType();
      if (type == typeof (PSObject))
      {
        obj = ((PSObject) obj).BaseObject;
        type = obj.GetType();
      }
      if (type == typeof (string) || type == typeof (int) || type == typeof (double))
        return (IEnumerator) null;
      if (type.IsArray)
        return ((Array) obj).GetEnumerator();
      if (typeof (IDictionary).IsAssignableFrom(type))
        return (IEnumerator) null;
      if (typeof (DataTable).IsAssignableFrom(type))
      {
        DataTable dataTable = (DataTable) obj;
        return dataTable.Rows != null ? dataTable.Rows.GetEnumerator() : (IEnumerator) null;
      }
      if (typeof (XmlNode).IsAssignableFrom(type))
        return (IEnumerator) null;
      IEnumerable enumerable = obj as IEnumerable;
      if (enumerable != null)
      {
        try
        {
          return enumerable.GetEnumerator() ?? LanguagePrimitives.GetEnumeratorFromIEnumeratorT(obj);
        }
        catch (RuntimeException ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          return LanguagePrimitives.GetEnumeratorFromIEnumeratorT(obj) ?? throw new ExtendedTypeSystemException("ExceptionInGetEnumerator", ex, "ExtendedTypeSystem", "EnumerationException", new object[1]
          {
            (object) ex.Message
          });
        }
      }
      else
        return obj is IEnumerator enumerator ? enumerator : (IEnumerator) null;
    }

    public new static bool Equals(object first, object second) => LanguagePrimitives.Equals(first, second, false, (IFormatProvider) CultureInfo.InvariantCulture);

    public static bool Equals(object first, object second, bool ignoreCase) => LanguagePrimitives.Equals(first, second, ignoreCase, (IFormatProvider) CultureInfo.InvariantCulture);

    public static bool Equals(
      object first,
      object second,
      bool ignoreCase,
      IFormatProvider formatProvider)
    {
      if (formatProvider == null)
        formatProvider = (IFormatProvider) CultureInfo.InvariantCulture;
      first = PSObject.Base(first);
      second = PSObject.Base(second);
      if (first == null)
        return second == null;
      if (second == null)
        return false;
      if (first is string strA)
      {
        if (!(second is string strB))
          strB = (string) LanguagePrimitives.ConvertTo(second, typeof (string), formatProvider);
        return string.Compare(strA, strB, ignoreCase, formatProvider as CultureInfo) == 0;
      }
      if (first.Equals(second))
        return true;
      Type type1 = first.GetType();
      Type type2 = second.GetType();
      int index1 = LanguagePrimitives.TypeTableIndex(type1);
      int index2 = LanguagePrimitives.TypeTableIndex(type2);
      if (index1 != -1 && index2 != -1)
        return LanguagePrimitives.NumericCompare(first, second, index1, index2) == 0;
      if (type1.Equals(typeof (char)))
      {
        if (ignoreCase)
        {
          if (second is string str && str.Length == 1)
            return char.ToUpper((char) first, formatProvider as CultureInfo).Equals(char.ToUpper(str[0], formatProvider as CultureInfo));
          if (type2.Equals(typeof (char)))
            return char.ToUpper((char) first, formatProvider as CultureInfo).Equals(char.ToUpper((char) second, formatProvider as CultureInfo));
        }
      }
      try
      {
        object obj = LanguagePrimitives.ConvertTo(second, type1, formatProvider);
        return first.Equals(obj);
      }
      catch (InvalidCastException ex)
      {
      }
      return false;
    }

    public static int Compare(object first, object second) => LanguagePrimitives.Compare(first, second, false, (IFormatProvider) CultureInfo.InvariantCulture);

    public static int Compare(object first, object second, bool ignoreCase) => LanguagePrimitives.Compare(first, second, ignoreCase, (IFormatProvider) CultureInfo.InvariantCulture);

    public static int Compare(
      object first,
      object second,
      bool ignoreCase,
      IFormatProvider formatProvider)
    {
      first = PSObject.Base(first);
      second = PSObject.Base(second);
      if (first == null)
      {
        if (second == null)
          return 0;
        switch (LanguagePrimitives.GetTypeCode(second.GetType()))
        {
          case TypeCode.SByte:
            return Math.Sign((sbyte) second) >= 0 ? -1 : 1;
          case TypeCode.Int16:
            return Math.Sign((short) second) >= 0 ? -1 : 1;
          case TypeCode.Int32:
            return Math.Sign((int) second) >= 0 ? -1 : 1;
          case TypeCode.Int64:
            return Math.Sign((long) second) >= 0 ? -1 : 1;
          case TypeCode.Single:
            return Math.Sign((float) second) >= 0 ? -1 : 1;
          case TypeCode.Double:
            return Math.Sign((double) second) >= 0 ? -1 : 1;
          case TypeCode.Decimal:
            return Math.Sign((Decimal) second) >= 0 ? -1 : 1;
          default:
            return -1;
        }
      }
      else if (second == null)
      {
        switch (LanguagePrimitives.GetTypeCode(first.GetType()))
        {
          case TypeCode.SByte:
            return Math.Sign((sbyte) first) >= 0 ? 1 : -1;
          case TypeCode.Int16:
            return Math.Sign((short) first) >= 0 ? 1 : -1;
          case TypeCode.Int32:
            return Math.Sign((int) first) >= 0 ? 1 : -1;
          case TypeCode.Int64:
            return Math.Sign((long) first) >= 0 ? 1 : -1;
          case TypeCode.Single:
            return Math.Sign((float) first) >= 0 ? 1 : -1;
          case TypeCode.Double:
            return Math.Sign((double) first) >= 0 ? 1 : -1;
          case TypeCode.Decimal:
            return Math.Sign((Decimal) first) >= 0 ? 1 : -1;
          default:
            return 1;
        }
      }
      else
      {
        if (first is string strA)
        {
          if (!(second is string strB))
          {
            try
            {
              strB = (string) LanguagePrimitives.ConvertTo(second, typeof (string), formatProvider);
            }
            catch (PSInvalidCastException ex)
            {
              throw LanguagePrimitives.tracer.NewArgumentException(nameof (second), "ExtendedTypeSystem", "ComparisonFailure", (object) first.ToString(), (object) second.ToString(), (object) ex.Message);
            }
          }
          return string.Compare(strA, strB, ignoreCase, formatProvider as CultureInfo);
        }
        Type type1 = first.GetType();
        Type type2 = second.GetType();
        int index1 = LanguagePrimitives.TypeTableIndex(type1);
        int index2 = LanguagePrimitives.TypeTableIndex(type2);
        if (index1 != -1)
        {
          if (index2 != -1)
            return LanguagePrimitives.NumericCompare(first, second, index1, index2);
        }
        object obj;
        try
        {
          obj = LanguagePrimitives.ConvertTo(second, type1, formatProvider);
        }
        catch (PSInvalidCastException ex)
        {
          throw LanguagePrimitives.tracer.NewArgumentException(nameof (second), "ExtendedTypeSystem", "ComparisonFailure", (object) first.ToString(), (object) second.ToString(), (object) ex.Message);
        }
        if (first is IComparable comparable)
          return comparable.CompareTo(obj);
        if (first.Equals(second))
          return 0;
        throw LanguagePrimitives.tracer.NewArgumentException(nameof (first), "ExtendedTypeSystem", "NotIcomparable", (object) first.ToString());
      }
    }

    public static bool IsTrue(object obj)
    {
      if (obj == null || obj == AutomationNull.Value)
        return false;
      obj = PSObject.Base(obj);
      Type type = obj.GetType();
      if (type.Equals(typeof (bool)))
        return (bool) obj;
      if (type.Equals(typeof (string)))
        return ((string) obj).Length != 0;
      TypeCode typeCode = LanguagePrimitives.GetTypeCode(type);
      if (LanguagePrimitives.IsInteger(typeCode) || LanguagePrimitives.IsFloating(typeCode))
        return !obj.Equals(Convert.ChangeType((object) 0, type, (IFormatProvider) CultureInfo.InvariantCulture));
      if (type.Equals(typeof (SwitchParameter)))
        return ((SwitchParameter) obj).ToBool();
      if (!(obj is IList list))
        return true;
      switch (list.Count)
      {
        case 0:
          return false;
        case 1:
          if (!(list[0] is IList list))
            return LanguagePrimitives.IsTrue(list[0]);
          return list.Count >= 1;
        default:
          return true;
      }
    }

    internal static bool IsNull(object obj) => obj == null || obj == AutomationNull.Value;

    internal static PSObject AsPSObjectOrNull(object obj) => obj == null ? (PSObject) null : PSObject.AsPSObject(obj);

    internal static int TypeTableIndex(Type type)
    {
      switch (LanguagePrimitives.GetTypeCode(type))
      {
        case TypeCode.SByte:
          return 6;
        case TypeCode.Byte:
          return 7;
        case TypeCode.Int16:
          return 0;
        case TypeCode.UInt16:
          return 3;
        case TypeCode.Int32:
          return 1;
        case TypeCode.UInt32:
          return 4;
        case TypeCode.Int64:
          return 2;
        case TypeCode.UInt64:
          return 5;
        case TypeCode.Single:
          return 8;
        case TypeCode.Double:
          return 9;
        case TypeCode.Decimal:
          return 10;
        default:
          return -1;
      }
    }

    private static int NumericCompareDecimal(Decimal decimalNumber, object otherNumber)
    {
      object obj;
      try
      {
        obj = Convert.ChangeType(otherNumber, typeof (Decimal), (IFormatProvider) CultureInfo.InvariantCulture);
      }
      catch (OverflowException ex1)
      {
        try
        {
          return ((IComparable) (double) Convert.ChangeType((object) decimalNumber, typeof (double), (IFormatProvider) CultureInfo.InvariantCulture)).CompareTo((object) (double) Convert.ChangeType(otherNumber, typeof (double), (IFormatProvider) CultureInfo.InvariantCulture));
        }
        catch (Exception ex2)
        {
          CommandProcessorBase.CheckForSevereException(ex2);
          return -1;
        }
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        return -1;
      }
      return ((IComparable) decimalNumber).CompareTo(obj);
    }

    private static int NumericCompare(object number1, object number2, int index1, int index2)
    {
      if (index1 == 10 && (index2 == 8 || index2 == 9))
        return LanguagePrimitives.NumericCompareDecimal((Decimal) number1, number2);
      if (index2 == 10 && (index1 == 8 || index1 == 9))
        return -LanguagePrimitives.NumericCompareDecimal((Decimal) number2, number1);
      Type conversionType = LanguagePrimitives.LargestTypeTable[index1][index2];
      return ((IComparable) Convert.ChangeType(number1, conversionType, (IFormatProvider) CultureInfo.InvariantCulture)).CompareTo(Convert.ChangeType(number2, conversionType, (IFormatProvider) CultureInfo.InvariantCulture));
    }

    internal static TypeCode GetTypeCode(Type type) => type.IsEnum ? TypeCode.Object : Type.GetTypeCode(type);

    internal static bool IsNumeric(TypeCode typeCode) => LanguagePrimitives.IsInteger(typeCode) || LanguagePrimitives.IsFloating(typeCode);

    internal static T FromObjectAs<T>(object castObject)
    {
      T obj1 = default (T);
      T obj2;
      if (!(castObject is PSObject psObject))
      {
        try
        {
          obj2 = (T) castObject;
        }
        catch (InvalidCastException ex)
        {
          obj2 = default (T);
        }
      }
      else
      {
        try
        {
          obj2 = (T) psObject.BaseObject;
        }
        catch (InvalidCastException ex)
        {
          obj2 = default (T);
        }
      }
      return obj2;
    }

    internal static bool IsSignedInteger(TypeCode typeCode) => typeCode == TypeCode.Int64 || typeCode == TypeCode.Int32 || (typeCode == TypeCode.Int16 || typeCode == TypeCode.SByte);

    internal static bool IsUnsignedInteger(TypeCode typeCode) => typeCode == TypeCode.UInt64 || typeCode == TypeCode.UInt32 || (typeCode == TypeCode.UInt16 || typeCode == TypeCode.Byte);

    internal static bool IsInteger(TypeCode typeCode) => LanguagePrimitives.IsSignedInteger(typeCode) || LanguagePrimitives.IsUnsignedInteger(typeCode);

    internal static bool IsFloating(TypeCode typeCode) => typeCode == TypeCode.Single || typeCode == TypeCode.Double || typeCode == TypeCode.Decimal;

    internal static bool IsBooleanType(Type type) => type == typeof (bool) || type == typeof (bool?);

    internal static bool IsSwitchParameterType(Type type) => type == typeof (SwitchParameter) || type == typeof (SwitchParameter?);

    internal static bool IsBoolOrSwitchParameterType(Type type) => LanguagePrimitives.IsBooleanType(type) || LanguagePrimitives.IsSwitchParameterType(type);

    private static TypeConverter GetIntegerSystemConverter(Type type)
    {
      if (type == typeof (short))
        return (TypeConverter) new Int16Converter();
      if (type == typeof (int))
        return (TypeConverter) new Int32Converter();
      if (type == typeof (long))
        return (TypeConverter) new Int64Converter();
      if (type == typeof (ushort))
        return (TypeConverter) new UInt16Converter();
      if (type == typeof (uint))
        return (TypeConverter) new UInt32Converter();
      if (type == typeof (ulong))
        return (TypeConverter) new UInt64Converter();
      if (type == typeof (byte))
        return (TypeConverter) new ByteConverter();
      return type == typeof (sbyte) ? (TypeConverter) new SByteConverter() : (TypeConverter) null;
    }

    internal static object GetConverter(Type type, TypeTable backupTypeTable)
    {
      object obj = (object) null;
      ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
      if (executionContextFromTls != null)
      {
        LanguagePrimitives.tracer.WriteLine("ecFromTLS != null", new object[0]);
        obj = executionContextFromTls.TypeTable.GetTypeConverter(type.FullName);
      }
      if (obj == null && backupTypeTable != null)
      {
        LanguagePrimitives.tracer.WriteLine("Using provided TypeTable to get the type converter", new object[0]);
        obj = backupTypeTable.GetTypeConverter(type.FullName);
      }
      if (obj != null)
      {
        LanguagePrimitives.tracer.WriteLine("typesXmlConverter != null", new object[0]);
        return obj;
      }
      object[] customAttributes = type.GetCustomAttributes(typeof (TypeConverterAttribute), false);
      if (customAttributes.Length == 0)
        return (object) null;
      string converterTypeName = ((TypeConverterAttribute) customAttributes[0]).ConverterTypeName;
      LanguagePrimitives.typeConversion.WriteLine("{0}'s TypeConverterAttribute points to {1}.", (object) type, (object) converterTypeName);
      return LanguagePrimitives.NewConverterInstance(converterTypeName);
    }

    private static object NewConverterInstance(string assemblyQualifiedTypeName)
    {
      int length = assemblyQualifiedTypeName.IndexOf(",", StringComparison.Ordinal);
      if (length == -1)
      {
        LanguagePrimitives.typeConversion.WriteLine("Type name \"{0}\" should be assembly qualified.", (object) assemblyQualifiedTypeName);
        return (object) null;
      }
      string str1 = assemblyQualifiedTypeName.Substring(length + 2);
      string name = assemblyQualifiedTypeName.Substring(0, length);
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        if (assembly.FullName == str1)
        {
          Type type;
          try
          {
            type = assembly.GetType(name, false);
          }
          catch (ArgumentException ex)
          {
            LanguagePrimitives.typeConversion.WriteLine("Assembly \"{0}\" threw an exception when retrieving the type \"{1}\": \"{2}\".", (object) str1, (object) name, (object) ex.Message);
            return (object) null;
          }
          try
          {
            return Activator.CreateInstance(type);
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            string str2 = !(ex is TargetInvocationException invocationException) || invocationException.InnerException == null ? ex.Message : invocationException.InnerException.Message;
            LanguagePrimitives.typeConversion.WriteLine("Creating an instance of type \"{0}\" caused an exception to be thrown: \"{1}\"", (object) assemblyQualifiedTypeName, (object) str2);
            return (object) null;
          }
        }
      }
      LanguagePrimitives.typeConversion.WriteLine("Could not create an instance of type \"{0}\".", (object) assemblyQualifiedTypeName);
      return (object) null;
    }

    private static Type LookForTypeInAssemblies(
      string typeName,
      IEnumerable<Assembly> assemblies)
    {
      foreach (Assembly assembly in assemblies)
      {
        try
        {
          Type type = assembly.GetType(typeName, false, true);
          if (type != null)
          {
            if (LanguagePrimitives.IsPublic(type))
              return type;
            LanguagePrimitives.typeConversion.WriteLine("\"{0}\" is not public, so it will not be returned.", (object) type);
          }
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          LanguagePrimitives.typeConversion.WriteLine("System.Reflection.Assembly's GetType threw an exception for \"{0}\": \"{1}\".", (object) typeName, (object) ex.Message);
          break;
        }
      }
      return (Type) null;
    }

    private static bool IsPublic(Type type)
    {
      if (type == null)
        throw LanguagePrimitives.tracer.NewArgumentNullException(nameof (type));
      if (type.IsPublic)
        return true;
      if (!type.IsNestedPublic)
        return false;
      for (type = type.DeclaringType; type != null; type = type.DeclaringType)
      {
        if (!type.IsPublic && !type.IsNestedPublic)
          return false;
      }
      return true;
    }

    private static Type GetTypeArgument(
      string typeName,
      ref int index,
      ref Exception exception)
    {
      bool commaTerminates;
      if (typeName[index] == '[')
      {
        ++index;
        commaTerminates = false;
      }
      else
        commaTerminates = true;
      string typeName1 = TypeTokenReader.MatchType(typeName, ref index, (Tokenizer) null, commaTerminates);
      if (typeName1 == null)
        return (Type) null;
      if (commaTerminates)
        --index;
      return LanguagePrimitives.ConvertStringToType(typeName1, out exception);
    }

    private static Collection<Type> GetTypeArguments(
      string typeName,
      int index,
      ref Exception exception)
    {
      Collection<Type> collection = new Collection<Type>();
      bool flag = false;
      while (index < typeName.Length)
      {
        char c = typeName[index];
        if (char.IsWhiteSpace(c))
          ++index;
        else if (flag)
        {
          switch (c)
          {
            case ',':
              ++index;
              flag = false;
              continue;
            case ']':
              goto label_11;
            default:
              return (Collection<Type>) null;
          }
        }
        else
        {
          Type typeArgument = LanguagePrimitives.GetTypeArgument(typeName, ref index, ref exception);
          if (typeArgument == null)
            return (Collection<Type>) null;
          collection.Add(typeArgument);
          flag = true;
        }
      }
label_11:
      return index == typeName.Length - 1 && typeName[index] == ']' ? collection : (Collection<Type>) null;
    }

    internal static Type ConvertStringToType(string typeName, out Exception exception)
    {
      using (LanguagePrimitives.typeConversion.TraceScope("Conversion to System.Type"))
      {
        bool flag1 = false;
        exception = (Exception) null;
        if (string.IsNullOrEmpty(typeName))
          return (Type) null;
        Type type1 = LanguagePrimitives.stringToTypeCache.Get(typeName);
        if (type1 == null)
        {
          flag1 = true;
          if (typeName.Length > 400)
          {
            int num = 0;
            foreach (char ch in typeName)
            {
              if (ch == '[')
              {
                ++num;
                if (num > 200)
                  return (Type) null;
              }
            }
          }
          ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
          if (executionContextFromTls != null)
          {
            type1 = LanguagePrimitives.LookForTypeInAssemblies(typeName, (IEnumerable<Assembly>) executionContextFromTls.AssemblyCache.Values);
            if (type1 != null)
            {
              LanguagePrimitives.typeConversion.WriteLine("Found \"{0}\" in the make kit assemblies.", (object) type1);
              goto label_21;
            }
          }
          Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
          type1 = LanguagePrimitives.LookForTypeInAssemblies(typeName, (IEnumerable<Assembly>) assemblies);
          if (type1 != null)
          {
            LanguagePrimitives.typeConversion.WriteLine("Found \"{0}\" in the loaded assemblies.", (object) type1);
          }
          else
          {
            try
            {
              Type type2 = Type.GetType(typeName, false, true);
              if (type2 != null)
              {
                if (LanguagePrimitives.IsPublic(type2))
                {
                  type1 = type2;
                  LanguagePrimitives.typeConversion.WriteLine("Found \"{0}\" via Type.GetType(typeName).", (object) type1);
                }
                else
                  LanguagePrimitives.typeConversion.WriteLine("\"{0}\" is not public, so it will not be returned.", (object) type2);
              }
            }
            catch (Exception ex)
            {
              CommandProcessorBase.CheckForSevereException(ex);
              LanguagePrimitives.typeConversion.WriteLine("System.Type's GetType threw an exception for \"{0}\": \"{1}\".", (object) typeName, (object) ex.Message);
              exception = ex;
            }
          }
        }
label_21:
        int num1;
        if (type1 == null && (num1 = typeName.IndexOf('[')) >= 0 && !typeName.Contains("`"))
        {
          Type type2 = LanguagePrimitives.stringToTypeCache.Get(typeName.Substring(0, num1));
          Collection<Type> typeArguments = LanguagePrimitives.GetTypeArguments(typeName, num1 + 1, ref exception);
          if (typeArguments != null)
          {
            StringBuilder stringBuilder;
            if (type2 == null || !type2.IsGenericTypeDefinition)
            {
              stringBuilder = new StringBuilder(typeName, 0, num1, typeName.Length + 2);
              stringBuilder.Append('`');
              stringBuilder.Append(typeArguments.Count);
            }
            else
              stringBuilder = new StringBuilder(type2.FullName);
            stringBuilder.Append('[');
            bool flag2 = true;
            foreach (Type type3 in typeArguments)
            {
              if (!flag2)
                stringBuilder.Append(',');
              flag2 = false;
              stringBuilder.Append('[');
              stringBuilder.Append(type3.AssemblyQualifiedName);
              stringBuilder.Append(']');
            }
            stringBuilder.Append(']');
            type1 = LanguagePrimitives.ConvertStringToType(stringBuilder.ToString(), out exception);
          }
          else if (type2 != null)
          {
            StringBuilder stringBuilder = new StringBuilder(type2.FullName);
            stringBuilder.Append(typeName, num1, typeName.Length - num1);
            if (!stringBuilder.ToString().Equals(typeName, StringComparison.OrdinalIgnoreCase))
              type1 = LanguagePrimitives.ConvertStringToType(stringBuilder.ToString(), out exception);
          }
        }
        if (type1 == null && !typeName.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
        {
          type1 = LanguagePrimitives.ConvertStringToType("System." + typeName, out exception);
          if (type1 != null)
            LanguagePrimitives.typeConversion.WriteLine("Found \"{0}\" after prepending 'System.' prefix", new object[0]);
        }
        if (type1 == null)
          LanguagePrimitives.typeConversion.WriteLine("Could not find a match for \"{0}\".", (object) typeName);
        if (type1 != null && flag1)
          LanguagePrimitives.stringToTypeCache.Add(typeName, type1);
        return type1;
      }
    }

    internal static Type ConvertStringToAttribute(string typeName)
    {
      bool flag = false;
      Type type = LanguagePrimitives.stringToAttributeCache.Get(typeName);
      if (type == null)
      {
        flag = true;
        Exception exception;
        type = LanguagePrimitives.ConvertStringToType(typeName, out exception) ?? LanguagePrimitives.ConvertStringToType(typeName + "Attribute", out exception);
      }
      if (type != null && flag)
        LanguagePrimitives.stringToAttributeCache.Add(typeName, type);
      return type;
    }

    public static object ConvertTo(object valueToConvert, Type resultType) => LanguagePrimitives.ConvertTo(valueToConvert, resultType, true, (IFormatProvider) CultureInfo.InvariantCulture, (TypeTable) null);

    public static object ConvertTo(
      object valueToConvert,
      Type resultType,
      IFormatProvider formatProvider)
    {
      return LanguagePrimitives.ConvertTo(valueToConvert, resultType, true, formatProvider, (TypeTable) null);
    }

    public static bool TryConvertTo<T>(object valueToConvert, out T result) => LanguagePrimitives.TryConvertTo<T>(valueToConvert, (IFormatProvider) CultureInfo.InvariantCulture, out result);

    public static bool TryConvertTo<T>(
      object valueToConvert,
      IFormatProvider formatProvider,
      out T result)
    {
      result = default (T);
      try
      {
        result = (T) LanguagePrimitives.ConvertTo(valueToConvert, typeof (T), formatProvider);
      }
      catch (InvalidCastException ex)
      {
        return false;
      }
      catch (ArgumentException ex)
      {
        return false;
      }
      return true;
    }

    public static bool TryConvertTo(object valueToConvert, Type resultType, out object result) => LanguagePrimitives.TryConvertTo(valueToConvert, resultType, (IFormatProvider) CultureInfo.InvariantCulture, out result);

    public static bool TryConvertTo(
      object valueToConvert,
      Type resultType,
      IFormatProvider formatProvider,
      out object result)
    {
      result = (object) null;
      try
      {
        result = LanguagePrimitives.ConvertTo(valueToConvert, resultType, formatProvider);
      }
      catch (InvalidCastException ex)
      {
        return false;
      }
      catch (ArgumentException ex)
      {
        return false;
      }
      return true;
    }

    private static MethodInfo FindCastOperator(
      string methodName,
      Type targetType,
      Type originalType,
      Type resultType)
    {
      using (LanguagePrimitives.typeConversion.TraceScope("Looking for \"{0}\" cast operator.", (object) methodName))
      {
        foreach (MethodInfo methodInfo in targetType.GetMember(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod))
        {
          if (resultType.IsAssignableFrom(methodInfo.ReturnType))
          {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(originalType))
            {
              LanguagePrimitives.typeConversion.WriteLine("Found \"{0}\" cast operator in type {1}.", (object) methodName, (object) targetType.FullName);
              return methodInfo;
            }
          }
        }
        LanguagePrimitives.typeConversion.TraceScope("Cast operator for \"{0}\" not found.", (object) methodName);
        return (MethodInfo) null;
      }
    }

    private static object ConvertNumericThroughDouble(object valueToConvert, Type resultType)
    {
      using (LanguagePrimitives.typeConversion.TraceScope("Numeric Conversion through System.Double."))
        return Convert.ChangeType(Convert.ChangeType(valueToConvert, typeof (double), (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat), resultType, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat);
    }

    private static ManagementObject ConvertToWMI(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Standard type conversion to a ManagementObject.", new object[0]);
      string path;
      try
      {
        path = PSObject.ToString((ExecutionContext) null, valueToConvert, "\n", (string) null, (IFormatProvider) null, true, true);
      }
      catch (ExtendedTypeSystemException ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Exception converting value to string: {0}", (object) ex.Message);
        throw new PSInvalidCastException("InvalidCastGetStringToWMI", (Exception) ex, "ExtendedTypeSystem", "InvalidCastExceptionNoStringForConversion", new object[2]
        {
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
      try
      {
        ManagementObject managementObject = new ManagementObject(path);
        return managementObject.SystemProperties["__CLASS"] != null ? managementObject : throw new PSInvalidCastException(ResourceManagerCache.FormatResourceString("ExtendedTypeSystem", "InvalidWMIPath", (object) path));
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        LanguagePrimitives.typeConversion.WriteLine("Exception creating WMI object: \"{0}\".", (object) ex.Message);
        throw new PSInvalidCastException("InvalidCastToWMI", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
    }

    private static ManagementObjectSearcher ConvertToWMISearcher(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Standard type conversion to a collection of ManagementObjects.", new object[0]);
      string queryString;
      try
      {
        queryString = PSObject.ToString((ExecutionContext) null, valueToConvert, "\n", (string) null, (IFormatProvider) null, true, true);
      }
      catch (ExtendedTypeSystemException ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Exception converting value to string: {0}", (object) ex.Message);
        throw new PSInvalidCastException("InvalidCastGetStringToWMISearcher", (Exception) ex, "ExtendedTypeSystem", "InvalidCastExceptionNoStringForConversion", new object[2]
        {
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
      try
      {
        return new ManagementObjectSearcher(queryString);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        LanguagePrimitives.typeConversion.WriteLine("Exception running WMI object query: \"{0}\".", (object) ex.Message);
        throw new PSInvalidCastException("InvalidCastToWMISearcher", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
    }

    private static ManagementClass ConvertToWMIClass(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Standard type conversion to a ManagementClass.", new object[0]);
      string path;
      try
      {
        path = PSObject.ToString((ExecutionContext) null, valueToConvert, "\n", (string) null, (IFormatProvider) null, true, true);
      }
      catch (ExtendedTypeSystemException ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Exception converting value to string: {0}", (object) ex.Message);
        throw new PSInvalidCastException("InvalidCastGetStringToWMIClass", (Exception) ex, "ExtendedTypeSystem", "InvalidCastExceptionNoStringForConversion", new object[2]
        {
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
      try
      {
        ManagementClass managementClass = new ManagementClass(path);
        return managementClass.SystemProperties["__CLASS"] != null ? managementClass : throw new PSInvalidCastException(ResourceManagerCache.FormatResourceString("ExtendedTypeSystem", "InvalidWMIClassPath", (object) path));
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        LanguagePrimitives.typeConversion.WriteLine("Exception creating WMI class: \"{0}\".", (object) ex.Message);
        throw new PSInvalidCastException("InvalidCastToWMIClass", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
    }

    private static DirectoryEntry ConvertToADSI(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Standard type conversion to  DirectoryEntry.", new object[0]);
      string path;
      try
      {
        path = PSObject.ToString((ExecutionContext) null, valueToConvert, "\n", (string) null, (IFormatProvider) null, true, true);
      }
      catch (ExtendedTypeSystemException ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Exception converting value to string: {0}", (object) ex.Message);
        throw new PSInvalidCastException("InvalidCastGetStringToADSIClass", (Exception) ex, "ExtendedTypeSystem", "InvalidCastExceptionNoStringForConversion", new object[2]
        {
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
      try
      {
        return new DirectoryEntry(path);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        LanguagePrimitives.typeConversion.WriteLine("Exception creating ADSI class: \"{0}\".", (object) ex.Message);
        throw new PSInvalidCastException("InvalidCastToADSIClass", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
    }

    private static XmlDocument ConvertToXml(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      using (LanguagePrimitives.typeConversion.TraceScope("Standard type conversion to XmlDocument."))
      {
        string xml;
        try
        {
          xml = PSObject.ToString((ExecutionContext) null, valueToConvert, "\n", (string) null, (IFormatProvider) null, true, true);
        }
        catch (ExtendedTypeSystemException ex)
        {
          LanguagePrimitives.typeConversion.WriteLine("Exception converting value to string: {0}", (object) ex.Message);
          throw new PSInvalidCastException("InvalidCastGetStringToXmlDocument", (Exception) ex, "ExtendedTypeSystem", "InvalidCastExceptionNoStringForConversion", new object[2]
          {
            (object) resultType.ToString(),
            (object) ex.Message
          });
        }
        XmlDocument xmlDocument = new XmlDocument();
        try
        {
          xmlDocument.LoadXml(xml);
          return xmlDocument;
        }
        catch (Exception ex)
        {
          LanguagePrimitives.typeConversion.WriteLine("Exception loading XML: \"{0}\".", (object) ex.Message);
          CommandProcessorBase.CheckForSevereException(ex);
          throw new PSInvalidCastException("InvalidCastToXmlDocument", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
          {
            (object) valueToConvert.ToString(),
            (object) resultType.ToString(),
            (object) ex.Message
          });
        }
      }
    }

    private static CultureInfo GetCultureFromFormatProvider(
      IFormatProvider formatProvider)
    {
      if (!(formatProvider is CultureInfo cultureInfo))
        cultureInfo = CultureInfo.InvariantCulture;
      return cultureInfo;
    }

    private static bool IsCustomTypeConversion(
      object valueToConvert,
      Type resultType,
      IFormatProvider formatProvider,
      out object result,
      TypeTable backupTypeTable)
    {
      using (LanguagePrimitives.typeConversion.TraceScope("Custom type conversion."))
      {
        object obj = PSObject.Base(valueToConvert);
        Type type = obj.GetType();
        object converter1 = LanguagePrimitives.GetConverter(type, backupTypeTable);
        if (converter1 != null)
        {
          if (converter1 is TypeConverter typeConverter)
          {
            LanguagePrimitives.typeConversion.WriteLine("Original type's converter is TypeConverter.", new object[0]);
            if (typeConverter.CanConvertTo(resultType))
            {
              LanguagePrimitives.typeConversion.WriteLine("TypeConverter can convert to resultType.", new object[0]);
              try
              {
                result = typeConverter.ConvertTo((ITypeDescriptorContext) null, LanguagePrimitives.GetCultureFromFormatProvider(formatProvider), obj, resultType);
                return true;
              }
              catch (Exception ex)
              {
                LanguagePrimitives.typeConversion.WriteLine("Exception converting with Original type's TypeConverter: \"{0}\".", (object) ex.Message);
                CommandProcessorBase.CheckForSevereException(ex);
                throw new PSInvalidCastException("InvalidCastTypeConvertersConvertTo", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
                {
                  (object) valueToConvert.ToString(),
                  (object) resultType.ToString(),
                  (object) ex.Message
                });
              }
            }
            else
              LanguagePrimitives.typeConversion.WriteLine("TypeConverter cannot convert to resultType.", new object[0]);
          }
          if (converter1 is PSTypeConverter psTypeConverter)
          {
            LanguagePrimitives.typeConversion.WriteLine("Original type's converter is PSTypeConverter.", new object[0]);
            PSObject sourceValue = PSObject.AsPSObject(valueToConvert);
            if (psTypeConverter.CanConvertTo(sourceValue, resultType))
            {
              LanguagePrimitives.typeConversion.WriteLine("Original type's PSTypeConverter can convert to resultType.", new object[0]);
              try
              {
                result = psTypeConverter.ConvertTo(sourceValue, resultType, formatProvider, true);
                return true;
              }
              catch (Exception ex)
              {
                LanguagePrimitives.typeConversion.WriteLine("Exception converting with Original type's PSTypeConverter: \"{0}\".", (object) ex.Message);
                CommandProcessorBase.CheckForSevereException(ex);
                throw new PSInvalidCastException("InvalidCastPSTypeConvertersConvertTo", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
                {
                  (object) valueToConvert.ToString(),
                  (object) resultType.ToString(),
                  (object) ex.Message
                });
              }
            }
            else
              LanguagePrimitives.typeConversion.WriteLine("Original type's PSTypeConverter cannot convert to resultType.", new object[0]);
          }
        }
        LanguagePrimitives.tracer.WriteLine("No converter found in original type.", new object[0]);
        object converter2 = LanguagePrimitives.GetConverter(resultType, backupTypeTable);
        if (converter2 != null)
        {
          if (converter2 is TypeConverter typeConverter)
          {
            LanguagePrimitives.typeConversion.WriteLine("Destination type's converter is TypeConverter that can convert from originalType.", new object[0]);
            if (typeConverter.CanConvertFrom(type))
            {
              LanguagePrimitives.typeConversion.WriteLine("Destination type's converter can convert from originalType.", new object[0]);
              try
              {
                result = typeConverter.ConvertFrom((ITypeDescriptorContext) null, LanguagePrimitives.GetCultureFromFormatProvider(formatProvider), obj);
                return true;
              }
              catch (Exception ex)
              {
                LanguagePrimitives.typeConversion.WriteLine("Exception converting with Destination type's TypeConverter: \"{0}\".", (object) ex.Message);
                CommandProcessorBase.CheckForSevereException(ex);
                throw new PSInvalidCastException("InvalidCastTypeConvertersConvertFrom", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
                {
                  (object) valueToConvert.ToString(),
                  (object) resultType.ToString(),
                  (object) ex.Message
                });
              }
            }
            else
              LanguagePrimitives.typeConversion.WriteLine("Destination type's converter cannot convert from originalType.", new object[0]);
          }
          if (converter2 is PSTypeConverter psTypeConverter)
          {
            LanguagePrimitives.typeConversion.WriteLine("Destination type's converter is PSTypeConverter.", new object[0]);
            PSObject sourceValue = PSObject.AsPSObject(valueToConvert);
            if (psTypeConverter.CanConvertFrom(sourceValue, resultType))
            {
              LanguagePrimitives.typeConversion.WriteLine("Destination type's converter can convert from originalType.", new object[0]);
              try
              {
                result = psTypeConverter.ConvertFrom(sourceValue, resultType, formatProvider, true);
                return true;
              }
              catch (Exception ex)
              {
                LanguagePrimitives.typeConversion.WriteLine("Exception converting with Destination type's PSTypeConverter: \"{0}\".", (object) ex.Message);
                CommandProcessorBase.CheckForSevereException(ex);
                throw new PSInvalidCastException("InvalidCastPSTypeConvertersConvertFrom", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
                {
                  (object) valueToConvert.ToString(),
                  (object) resultType.ToString(),
                  (object) ex.Message
                });
              }
            }
            else
              LanguagePrimitives.typeConversion.WriteLine("Destination type's converter cannot convert from originalType.", new object[0]);
          }
        }
        result = (object) null;
        return false;
      }
    }

    private static object ConvertNumeric(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      try
      {
        object obj = Convert.ChangeType(valueToConvert, resultType, formatProvider);
        LanguagePrimitives.typeConversion.WriteLine("Numeric conversion succeeded.", new object[0]);
        return obj;
      }
      catch (Exception ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Exception converting with IConvertible: \"{0}\".", (object) ex.Message);
        CommandProcessorBase.CheckForSevereException(ex);
        throw new PSInvalidCastException("InvalidCastIConvertible", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
    }

    private static object ConvertStringToCharArray(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Returning value to convert's ToCharArray().", new object[0]);
      return (object) ((string) valueToConvert).ToCharArray();
    }

    private static object ConvertStringToRegex(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Returning new RegEx(value to convert).", new object[0]);
      try
      {
        return (object) new Regex((string) valueToConvert);
      }
      catch (Exception ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Exception in RegEx constructor: \"{0}\".", (object) ex.Message);
        CommandProcessorBase.CheckForSevereException(ex);
        throw new PSInvalidCastException("InvalidCastFromStringToRegex", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
    }

    private static Type ConvertStringToType(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      Exception exception;
      return LanguagePrimitives.ConvertStringToType((string) valueToConvert, out exception) ?? throw new PSInvalidCastException("InvalidCastFromStringToType", exception, "ExtendedTypeSystem", "InvalidCastException", new object[3]
      {
        (object) valueToConvert.ToString(),
        (object) LanguagePrimitives.ObjectToTypeNameString(valueToConvert),
        (object) resultType.ToString()
      });
    }

    private static object ConvertStringToInteger(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      if (((string) valueToConvert).Length == 0)
      {
        LanguagePrimitives.typeConversion.WriteLine("Returning numeric zero.", new object[0]);
        return Convert.ChangeType((object) 0, resultType, (IFormatProvider) CultureInfo.InvariantCulture);
      }
      LanguagePrimitives.typeConversion.WriteLine("Converting to integer.", new object[0]);
      TypeConverter integerSystemConverter = LanguagePrimitives.GetIntegerSystemConverter(resultType);
      try
      {
        return integerSystemConverter.ConvertFrom(valueToConvert);
      }
      catch (Exception ex1)
      {
        Exception exception = ex1;
        CommandProcessorBase.CheckForSevereException(exception);
        if (exception.InnerException != null)
          exception = exception.InnerException;
        LanguagePrimitives.typeConversion.WriteLine("Exception converting to integer: \"{0}\".", (object) exception.Message);
        CommandProcessorBase.CheckForSevereException(exception);
        if (exception is FormatException)
        {
          LanguagePrimitives.typeConversion.WriteLine("Converting to integer passing through double.", new object[0]);
          try
          {
            return LanguagePrimitives.ConvertNumericThroughDouble(valueToConvert, resultType);
          }
          catch (Exception ex2)
          {
            LanguagePrimitives.typeConversion.WriteLine("Exception converting to integer through double: \"{0}\".", (object) ex2.Message);
            CommandProcessorBase.CheckForSevereException(ex2);
          }
        }
        throw new PSInvalidCastException("InvalidCastFromStringToInteger", exception, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) exception.Message
        });
      }
    }

    private static object ConvertStringToDecimal(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      if (((string) valueToConvert).Length == 0)
      {
        LanguagePrimitives.typeConversion.WriteLine("Returning numeric zero.", new object[0]);
        return Convert.ChangeType((object) 0, resultType, (IFormatProvider) CultureInfo.InvariantCulture);
      }
      LanguagePrimitives.typeConversion.WriteLine("Converting to decimal.", new object[0]);
      try
      {
        return Convert.ChangeType(valueToConvert, resultType, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat);
      }
      catch (Exception ex1)
      {
        LanguagePrimitives.typeConversion.WriteLine("Exception converting to decimal: \"{0}\". Converting to decimal passing through double.", (object) ex1.Message);
        CommandProcessorBase.CheckForSevereException(ex1);
        if (ex1 is FormatException)
        {
          try
          {
            return LanguagePrimitives.ConvertNumericThroughDouble(valueToConvert, resultType);
          }
          catch (Exception ex2)
          {
            LanguagePrimitives.typeConversion.WriteLine("Exception converting to integer through double: \"{0}\".", (object) ex2.Message);
            CommandProcessorBase.CheckForSevereException(ex2);
          }
        }
        throw new PSInvalidCastException("InvalidCastFromStringToDecimal", ex1, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex1.Message
        });
      }
    }

    private static object ConvertStringToReal(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      if (((string) valueToConvert).Length == 0)
      {
        LanguagePrimitives.typeConversion.WriteLine("Returning numeric zero.", new object[0]);
        return Convert.ChangeType((object) 0, resultType, (IFormatProvider) CultureInfo.InvariantCulture);
      }
      LanguagePrimitives.typeConversion.WriteLine("Converting to double or single.", new object[0]);
      try
      {
        return Convert.ChangeType(valueToConvert, resultType, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat);
      }
      catch (Exception ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Exception converting to double or single: \"{0}\".", (object) ex.Message);
        CommandProcessorBase.CheckForSevereException(ex);
        throw new PSInvalidCastException("InvalidCastFromStringToDoubleOrSingle", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
    }

    private static object ConvertAssignableFrom(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Result type is assignable from value to convert's type", new object[0]);
      return valueToConvert;
    }

    private static object ConvertToPSObject(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Returning PSObject.AsPSObject(valueToConvert).", new object[0]);
      return (object) PSObject.AsPSObject(valueToConvert);
    }

    private static object ConvertToVoid(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("returning AutomationNull.Value.", new object[0]);
      return (object) AutomationNull.Value;
    }

    private static object ConvertToBool(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Converting to boolean.", new object[0]);
      return (object) LanguagePrimitives.IsTrue(valueToConvert);
    }

    private static object ConvertNumericToString(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      if (originalValueToConvert != null && originalValueToConvert.TokenText != null)
        return (object) originalValueToConvert.TokenText;
      LanguagePrimitives.typeConversion.WriteLine("Converting numeric to string.", new object[0]);
      try
      {
        return Convert.ChangeType(valueToConvert, resultType, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat);
      }
      catch (Exception ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Converting numeric to string Exception: \"{0}\".", (object) ex.Message);
        CommandProcessorBase.CheckForSevereException(ex);
        throw new PSInvalidCastException("InvalidCastFromNumericToString", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
    }

    private static object ConvertNonNumericToString(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
      try
      {
        LanguagePrimitives.typeConversion.WriteLine("Converting object to string.", new object[0]);
        return (object) PSObject.ToStringParser(executionContextFromTls, valueToConvert);
      }
      catch (ExtendedTypeSystemException ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Converting object to string Exception: \"{0}\".", (object) ex.Message);
        throw new PSInvalidCastException("InvalidCastFromAnyTypeToString", (Exception) ex, "ExtendedTypeSystem", "InvalidCastCannotRetrieveString", new object[0]);
      }
    }

    private static object ConvertIDictionaryToHashtable(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Converting to Hashtable.", new object[0]);
      return (object) new Hashtable(valueToConvert as IDictionary);
    }

    private static object ConvertToPSReference(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Converting to PSReference.", new object[0]);
      return (object) new PSReference(valueToConvert);
    }

    private static object ConvertScriptBlockToDelegate(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      Exception innerException;
      try
      {
        return (object) ((ScriptBlock) valueToConvert).GetDelegate(resultType);
      }
      catch (ArgumentNullException ex)
      {
        innerException = (Exception) ex;
      }
      catch (ArgumentException ex)
      {
        innerException = (Exception) ex;
      }
      catch (MissingMethodException ex)
      {
        innerException = (Exception) ex;
      }
      catch (MemberAccessException ex)
      {
        innerException = (Exception) ex;
      }
      LanguagePrimitives.typeConversion.WriteLine("Converting script block to delegate Exception: \"{0}\".", (object) innerException.Message);
      throw new PSInvalidCastException("InvalidCastFromScriptBlockToDelegate", innerException, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
      {
        (object) valueToConvert.ToString(),
        (object) resultType.ToString(),
        (object) innerException.Message
      });
    }

    private static object ConvertToNullable(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      return LanguagePrimitives.ConvertTo(valueToConvert, Nullable.GetUnderlyingType(resultType), recursion, formatProvider, backupTable);
    }

    private static object ConvertRelatedArrays(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("The element type of result is assignable from the element type of the value to convert", new object[0]);
      return valueToConvert;
    }

    private static object ConvertUnrelatedArrays(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      ArrayList arrayList = new ArrayList();
      Array array = valueToConvert as Array;
      Type elementType = resultType.GetElementType();
      foreach (object valueToConvert1 in array)
        arrayList.Add(LanguagePrimitives.ConvertTo(valueToConvert1, elementType, false, formatProvider, backupTable));
      return (object) arrayList.ToArray(elementType);
    }

    private static object ConvertEnumerableToArray(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      try
      {
        ArrayList arrayList = new ArrayList();
        Type elementType = resultType.GetElementType();
        LanguagePrimitives.typeConversion.WriteLine("Converting elements in the value to convert to the result's element type.", new object[0]);
        foreach (object valueToConvert1 in LanguagePrimitives.GetEnumerable(valueToConvert))
          arrayList.Add(LanguagePrimitives.ConvertTo(valueToConvert1, elementType, false, formatProvider, backupTable));
        return (object) arrayList.ToArray(elementType);
      }
      catch (Exception ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Element conversion exception: \"{0}\".", (object) ex.Message);
        CommandProcessorBase.CheckForSevereException(ex);
        throw new PSInvalidCastException("InvalidCastExceptionEnumerableToArray", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
    }

    private static object ConvertScalarToArray(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Value to convert is scalar.", new object[0]);
      if (originalValueToConvert != null)
      {
        if (originalValueToConvert.TokenText != null)
          valueToConvert = (object) originalValueToConvert;
      }
      try
      {
        Type elementType = resultType.GetElementType();
        return (object) new ArrayList()
        {
          LanguagePrimitives.ConvertTo(valueToConvert, elementType, false, formatProvider, backupTable)
        }.ToArray(elementType);
      }
      catch (Exception ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Element conversion exception: \"{0}\".", (object) ex.Message);
        CommandProcessorBase.CheckForSevereException(ex);
        throw new PSInvalidCastException("InvalidCastExceptionScalarToArray", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
    }

    private static object ConvertIntegerToEnum(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      object enumValue;
      try
      {
        enumValue = Enum.ToObject(resultType, valueToConvert);
      }
      catch (Exception ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Integer to System.Enum exception: \"{0}\".", (object) ex.Message);
        CommandProcessorBase.CheckForSevereException(ex);
        throw new PSInvalidCastException("InvalidCastExceptionIntegerToEnum", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
      LanguagePrimitives.EnumSingleTypeConverter.ThrowForUndefinedEnum("UndefinedIntegerToEnum", enumValue, valueToConvert, resultType);
      return enumValue;
    }

    private static object ConvertStringToEnum(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      string str = valueToConvert as string;
      object enumValue = (object) null;
      LanguagePrimitives.typeConversion.WriteLine("Calling case sensitive Enum.Parse", new object[0]);
      try
      {
        enumValue = Enum.Parse(resultType, str);
      }
      catch (ArgumentException ex1)
      {
        LanguagePrimitives.typeConversion.WriteLine("Enum.Parse Exception: \"{0}\".", (object) ex1.Message);
        try
        {
          LanguagePrimitives.typeConversion.WriteLine("Calling case insensitive Enum.Parse", new object[0]);
          enumValue = Enum.Parse(resultType, str, true);
        }
        catch (ArgumentException ex2)
        {
          LanguagePrimitives.typeConversion.WriteLine("Enum.Parse Exception: \"{0}\".", (object) ex2.Message);
        }
        catch (Exception ex2)
        {
          CommandProcessorBase.CheckForSevereException(ex2);
          LanguagePrimitives.typeConversion.WriteLine("Case insensitive Enum.Parse threw an exception.", new object[0]);
          throw new PSInvalidCastException("CaseInsensitiveEnumParseThrewAnException", ex2, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
          {
            (object) valueToConvert.ToString(),
            (object) resultType.ToString(),
            (object) ex2.Message
          });
        }
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        LanguagePrimitives.typeConversion.WriteLine("Case Sensitive Enum.Parse threw an exception.", new object[0]);
        throw new PSInvalidCastException("CaseSensitiveEnumParseThrewAnException", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
      LanguagePrimitives.EnumSingleTypeConverter.ThrowForUndefinedEnum("EnumParseUndefined", enumValue, valueToConvert, resultType);
      LanguagePrimitives.tracer.WriteLine("returning \"{0}\" from conversion to Enum.", enumValue);
      return enumValue;
    }

    private static object ConvertEnumerableToEnum(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(valueToConvert);
      StringBuilder stringBuilder = new StringBuilder();
      bool flag = false;
      while (ParserOps.MoveNext((ExecutionContext) null, (Token) null, enumerator))
      {
        if (flag)
          stringBuilder.Append(',');
        else
          flag = true;
        if (!(enumerator.Current is string current))
          stringBuilder.Append((LanguagePrimitives.ConvertTo(enumerator.Current, resultType, recursion, formatProvider, backupTable) ?? throw new PSInvalidCastException("InvalidCastEnumStringNotFound", (Exception) null, "ExtendedTypeSystem", "InvalidCastExceptionEnumerationNoValue", new object[3]
          {
            enumerator.Current,
            (object) resultType,
            (object) LanguagePrimitives.EnumSingleTypeConverter.EnumValues(resultType)
          })).ToString());
        stringBuilder.Append(current);
      }
      return LanguagePrimitives.ConvertStringToEnum((object) stringBuilder.ToString(), resultType, recursion, originalValueToConvert, formatProvider, backupTable);
    }

    private static object ConvertIConvertible(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      try
      {
        object obj = Convert.ChangeType(valueToConvert, resultType, formatProvider);
        LanguagePrimitives.typeConversion.WriteLine("Conversion using IConvertible succeeded.", new object[0]);
        return obj;
      }
      catch (Exception ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Exception converting with IConvertible: \"{0}\".", (object) ex.Message);
        CommandProcessorBase.CheckForSevereException(ex);
        throw new PSInvalidCastException("InvalidCastIConvertible", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) resultType.ToString(),
          (object) ex.Message
        });
      }
    }

    private static object ConvertNumericIConvertible(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      return originalValueToConvert != null && originalValueToConvert.TokenText != null ? LanguagePrimitives.ConvertTo((object) originalValueToConvert.TokenText, resultType, recursion, formatProvider, backupTable) : LanguagePrimitives.ConvertTo((object) (string) LanguagePrimitives.ConvertTo(valueToConvert, typeof (string), recursion, formatProvider, backupTable), resultType, recursion, formatProvider, backupTable);
    }

    private static object ConvertNullToNumeric(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Converting null to zero.", new object[0]);
      return Convert.ChangeType((object) 0, resultType, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    private static object ConvertNullToChar(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Converting null to '0'.", new object[0]);
      return (object) char.MinValue;
    }

    private static object ConvertNullToString(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Converting null to \"\".", new object[0]);
      return (object) string.Empty;
    }

    private static object ConvertNullToPSReference(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      return (object) new PSReference((object) null);
    }

    private static object ConvertNullToRef(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      return valueToConvert;
    }

    private static object ConvertNullToBool(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Converting null to boolean.", new object[0]);
      return (object) LanguagePrimitives.IsTrue((object) null);
    }

    private static object ConvertNullToNullable(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      return (object) null;
    }

    private static object ConvertNullToSwitch(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Converting null to SwitchParameter(false).", new object[0]);
      return (object) new SwitchParameter(false);
    }

    private static object ConvertNullToVoid(
      object valueToConvert,
      Type resultType,
      bool recursion,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable)
    {
      LanguagePrimitives.typeConversion.WriteLine("Converting null to AutomationNull.Value.", new object[0]);
      return (object) AutomationNull.Value;
    }

    private static LanguagePrimitives.ConversionData CacheConversion(
      Type fromType,
      Type toType,
      LanguagePrimitives.PSConverter converter,
      ConversionRank rank)
    {
      LanguagePrimitives.ConversionTypePair key = new LanguagePrimitives.ConversionTypePair(fromType, toType);
      LanguagePrimitives.ConversionData conversionData = (LanguagePrimitives.ConversionData) null;
      lock (LanguagePrimitives.converterCache)
      {
        if (!LanguagePrimitives.converterCache.TryGetValue(key, out conversionData))
        {
          conversionData = new LanguagePrimitives.ConversionData(converter, rank);
          LanguagePrimitives.converterCache.Add(key, conversionData);
        }
      }
      return conversionData;
    }

    private static LanguagePrimitives.ConversionData GetConversionData(
      Type fromType,
      Type toType)
    {
      lock (LanguagePrimitives.converterCache)
      {
        LanguagePrimitives.ConversionData conversionData = (LanguagePrimitives.ConversionData) null;
        LanguagePrimitives.converterCache.TryGetValue(new LanguagePrimitives.ConversionTypePair(fromType, toType), out conversionData);
        return conversionData;
      }
    }

    internal static ConversionRank GetConversionRank(Type fromType, Type toType)
    {
      LanguagePrimitives.ConversionData conversionData = LanguagePrimitives.FigureConversion(fromType, toType);
      return conversionData == null ? ConversionRank.None : conversionData.rank;
    }

    private static void RebuildConversionCache()
    {
      lock (LanguagePrimitives.converterCache)
      {
        LanguagePrimitives.converterCache.Clear();
        Type type = typeof (string);
        Type fromType = typeof (LanguagePrimitives.Null);
        foreach (Type numericType in LanguagePrimitives.NumericTypes)
        {
          LanguagePrimitives.CacheConversion(numericType, type, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumericToString), ConversionRank.NumericString);
          LanguagePrimitives.CacheConversion(numericType, typeof (char), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertIConvertible), ConversionRank.NumericString);
          LanguagePrimitives.CacheConversion(fromType, numericType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNullToNumeric), ConversionRank.NullToValue);
        }
        for (int index1 = 0; index1 < LanguagePrimitives.UnsignedIntegerTypes.Length; ++index1)
        {
          LanguagePrimitives.CacheConversion(LanguagePrimitives.UnsignedIntegerTypes[index1], LanguagePrimitives.UnsignedIntegerTypes[index1], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertAssignableFrom), ConversionRank.Identity);
          LanguagePrimitives.CacheConversion(LanguagePrimitives.SignedIntegerTypes[index1], LanguagePrimitives.SignedIntegerTypes[index1], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertAssignableFrom), ConversionRank.Identity);
          LanguagePrimitives.CacheConversion(LanguagePrimitives.UnsignedIntegerTypes[index1], LanguagePrimitives.SignedIntegerTypes[index1], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
          LanguagePrimitives.CacheConversion(LanguagePrimitives.SignedIntegerTypes[index1], LanguagePrimitives.UnsignedIntegerTypes[index1], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit1);
          for (int index2 = index1 + 1; index2 < LanguagePrimitives.UnsignedIntegerTypes.Length; ++index2)
          {
            LanguagePrimitives.CacheConversion(LanguagePrimitives.UnsignedIntegerTypes[index1], LanguagePrimitives.UnsignedIntegerTypes[index2], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericImplicit);
            LanguagePrimitives.CacheConversion(LanguagePrimitives.SignedIntegerTypes[index1], LanguagePrimitives.SignedIntegerTypes[index2], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericImplicit);
            LanguagePrimitives.CacheConversion(LanguagePrimitives.UnsignedIntegerTypes[index1], LanguagePrimitives.SignedIntegerTypes[index2], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericImplicit);
            LanguagePrimitives.CacheConversion(LanguagePrimitives.SignedIntegerTypes[index1], LanguagePrimitives.UnsignedIntegerTypes[index2], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit1);
            LanguagePrimitives.CacheConversion(LanguagePrimitives.UnsignedIntegerTypes[index2], LanguagePrimitives.UnsignedIntegerTypes[index1], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
            LanguagePrimitives.CacheConversion(LanguagePrimitives.SignedIntegerTypes[index2], LanguagePrimitives.SignedIntegerTypes[index1], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
            LanguagePrimitives.CacheConversion(LanguagePrimitives.UnsignedIntegerTypes[index2], LanguagePrimitives.SignedIntegerTypes[index1], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
            LanguagePrimitives.CacheConversion(LanguagePrimitives.SignedIntegerTypes[index2], LanguagePrimitives.UnsignedIntegerTypes[index1], new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
          }
        }
        foreach (Type integerType in LanguagePrimitives.IntegerTypes)
        {
          LanguagePrimitives.CacheConversion(type, integerType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertStringToInteger), ConversionRank.NumericString);
          foreach (Type realType in LanguagePrimitives.RealTypes)
          {
            LanguagePrimitives.CacheConversion(integerType, realType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericImplicit);
            LanguagePrimitives.CacheConversion(realType, integerType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
          }
        }
        LanguagePrimitives.CacheConversion(typeof (float), typeof (double), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericImplicit);
        LanguagePrimitives.CacheConversion(typeof (double), typeof (float), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
        LanguagePrimitives.CacheConversion(typeof (float), typeof (Decimal), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
        LanguagePrimitives.CacheConversion(typeof (double), typeof (Decimal), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit);
        LanguagePrimitives.CacheConversion(typeof (Decimal), typeof (float), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit1);
        LanguagePrimitives.CacheConversion(typeof (Decimal), typeof (double), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumeric), ConversionRank.NumericExplicit1);
        LanguagePrimitives.CacheConversion(type, typeof (Regex), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertStringToRegex), ConversionRank.Language);
        LanguagePrimitives.CacheConversion(type, typeof (char[]), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertStringToCharArray), ConversionRank.StringToCharArray);
        LanguagePrimitives.CacheConversion(type, typeof (ManagementObjectSearcher), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertToWMISearcher), ConversionRank.Language);
        LanguagePrimitives.CacheConversion(type, typeof (ManagementClass), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertToWMIClass), ConversionRank.Language);
        LanguagePrimitives.CacheConversion(type, typeof (ManagementObject), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertToWMI), ConversionRank.Language);
        LanguagePrimitives.CacheConversion(type, typeof (DirectoryEntry), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertToADSI), ConversionRank.Language);
        LanguagePrimitives.CacheConversion(type, typeof (DirectorySearcher), LanguagePrimitives.FigureConstructorConversion(type, typeof (DirectorySearcher)), ConversionRank.Language);
        LanguagePrimitives.CacheConversion(type, typeof (Type), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertStringToType), ConversionRank.Language);
        LanguagePrimitives.CacheConversion(type, typeof (Decimal), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertStringToDecimal), ConversionRank.NumericString);
        LanguagePrimitives.CacheConversion(type, typeof (float), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertStringToReal), ConversionRank.NumericString);
        LanguagePrimitives.CacheConversion(type, typeof (double), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertStringToReal), ConversionRank.NumericString);
        LanguagePrimitives.CacheConversion(fromType, typeof (char), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNullToChar), ConversionRank.NullToValue);
        LanguagePrimitives.CacheConversion(fromType, type, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNullToString), ConversionRank.ToString);
        LanguagePrimitives.CacheConversion(fromType, typeof (bool), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNullToBool), ConversionRank.NullToValue);
        LanguagePrimitives.CacheConversion(fromType, typeof (PSReference), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNullToPSReference), ConversionRank.NullToRef);
        LanguagePrimitives.CacheConversion(fromType, typeof (SwitchParameter), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNullToSwitch), ConversionRank.NullToValue);
        LanguagePrimitives.CacheConversion(fromType, typeof (void), new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNullToVoid), ConversionRank.NullToValue);
      }
    }

    internal static object ConvertTo(
      object valueToConvert,
      Type resultType,
      bool recursion,
      IFormatProvider formatProvider,
      TypeTable backupTypeTable)
    {
      using (LanguagePrimitives.typeConversion.TraceScope("Converting \"{0}\" to \"{1}\".", valueToConvert, (object) resultType))
      {
        if (resultType == null)
          throw LanguagePrimitives.tracer.NewArgumentNullException(nameof (resultType));
        if (resultType == typeof (Array))
        {
          LanguagePrimitives.typeConversion.WriteLine("A conversion to Array is actually a conversion to object[].", new object[0]);
          resultType = typeof (object[]);
        }
        PSObject originalValueToConvert;
        Type fromType;
        if (valueToConvert == null || valueToConvert == AutomationNull.Value)
        {
          originalValueToConvert = (PSObject) null;
          fromType = typeof (LanguagePrimitives.Null);
        }
        else
        {
          originalValueToConvert = valueToConvert as PSObject;
          fromType = valueToConvert.GetType();
        }
        LanguagePrimitives.ConversionData conversionData1;
        LanguagePrimitives.PSConverter psConverter1 = (conversionData1 = LanguagePrimitives.FigureConversion(fromType, resultType)) != null ? conversionData1.converter : (LanguagePrimitives.PSConverter) null;
        if (psConverter1 != null)
          return psConverter1(valueToConvert, resultType, recursion, originalValueToConvert, formatProvider, backupTypeTable);
        if (originalValueToConvert != null)
        {
          valueToConvert = PSObject.Base(valueToConvert);
          fromType = valueToConvert != null ? (valueToConvert is PSObject ? typeof (LanguagePrimitives.InternalPSObject) : valueToConvert.GetType()) : typeof (LanguagePrimitives.Null);
          LanguagePrimitives.ConversionData conversionData2;
          LanguagePrimitives.PSConverter psConverter2 = (conversionData2 = LanguagePrimitives.FigureConversion(fromType, resultType)) != null ? conversionData2.converter : (LanguagePrimitives.PSConverter) null;
          if (psConverter2 != null)
            return psConverter2(valueToConvert, resultType, recursion, originalValueToConvert, formatProvider, backupTypeTable);
        }
        if (fromType.Equals(typeof (LanguagePrimitives.Null)))
        {
          if (resultType.IsEnum)
          {
            LanguagePrimitives.typeConversion.WriteLine("Issuing an error message about not being able to convert null to an Enum type.", new object[0]);
            throw new PSInvalidCastException("nullToEnumInvalidCast", (Exception) null, "ExtendedTypeSystem", "InvalidCastExceptionEnumerationNull", new object[2]
            {
              (object) resultType,
              (object) LanguagePrimitives.EnumSingleTypeConverter.EnumValues(resultType)
            });
          }
          LanguagePrimitives.typeConversion.WriteLine("Cannot convert null.", new object[0]);
          throw new PSInvalidCastException("nullToObjectInvalidCast", (Exception) null, "ExtendedTypeSystem", "InvalidCastFromNull", new object[1]
          {
            (object) resultType.ToString()
          });
        }
        LanguagePrimitives.typeConversion.WriteLine("Type Conversion failed.", new object[0]);
        throw new PSInvalidCastException("ConvertToFinalInvalidCastException", (Exception) null, "ExtendedTypeSystem", "InvalidCastException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) LanguagePrimitives.ObjectToTypeNameString(valueToConvert),
          (object) resultType.ToString()
        });
      }
    }

    private static LanguagePrimitives.ConversionData FigureLanguageConversion(
      Type fromType,
      Type toType,
      out LanguagePrimitives.PSConverter valueDependentConversion,
      out ConversionRank valueDependentRank)
    {
      valueDependentConversion = (LanguagePrimitives.PSConverter) null;
      valueDependentRank = ConversionRank.None;
      Type underlyingType = Nullable.GetUnderlyingType(toType);
      if (underlyingType != null && LanguagePrimitives.FigureConversion(fromType, underlyingType) != null)
        return LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertToNullable), ConversionRank.Language);
      if (toType.Equals(typeof (void)))
        return LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertToVoid), ConversionRank.Language);
      if (toType.Equals(typeof (bool)))
        return LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertToBool), ConversionRank.Language);
      if (toType.Equals(typeof (string)))
        return LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNonNumericToString), ConversionRank.ToString);
      if (toType.IsArray)
      {
        Type elementType = toType.GetElementType();
        if (fromType.IsArray)
          return elementType.IsAssignableFrom(fromType.GetElementType()) ? LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertRelatedArrays), ConversionRank.Language) : LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertUnrelatedArrays), ConversionRank.UnrelatedArrays);
        if (LanguagePrimitives.IsTypeEnumerable(fromType))
          return LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertEnumerableToArray), ConversionRank.Language);
        LanguagePrimitives.ConversionData conversionData = LanguagePrimitives.FigureConversion(fromType, elementType);
        if (conversionData != null)
        {
          valueDependentRank = conversionData.rank & ConversionRank.ValueDependent;
          valueDependentConversion = new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertScalarToArray);
          return (LanguagePrimitives.ConversionData) null;
        }
      }
      if (toType.Equals(typeof (Hashtable)))
        return typeof (IDictionary).IsAssignableFrom(fromType) ? LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertIDictionaryToHashtable), ConversionRank.Language) : (LanguagePrimitives.ConversionData) null;
      if (toType.Equals(typeof (PSReference)))
        return LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertToPSReference), ConversionRank.Language);
      if (toType.Equals(typeof (XmlDocument)))
        return LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertToXml), ConversionRank.Language);
      if (toType.IsSubclassOf(typeof (Delegate)) && (fromType.Equals(typeof (ScriptBlock)) || fromType.IsSubclassOf(typeof (ScriptBlock))))
        return LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertScriptBlockToDelegate), ConversionRank.Language);
      return LanguagePrimitives.IsInteger(LanguagePrimitives.GetTypeCode(fromType)) && toType.IsEnum ? LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertIntegerToEnum), ConversionRank.Language) : (LanguagePrimitives.ConversionData) null;
    }

    private static LanguagePrimitives.PSConverter FigureParseConversion(
      Type fromType,
      Type toType)
    {
      if (toType.IsEnum)
      {
        if (fromType.Equals(typeof (string)))
          return new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertStringToEnum);
        if (LanguagePrimitives.IsTypeEnumerable(fromType))
          return new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertEnumerableToEnum);
      }
      else if (fromType.Equals(typeof (string)))
      {
        BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod;
        MethodInfo methodInfo = (MethodInfo) null;
        try
        {
          methodInfo = toType.GetMethod("Parse", bindingAttr, (Binder) null, new Type[2]
          {
            typeof (string),
            typeof (IFormatProvider)
          }, (ParameterModifier[]) null);
        }
        catch (AmbiguousMatchException ex)
        {
          LanguagePrimitives.typeConversion.WriteLine("Exception finding Parse method with CultureInfo: \"{0}\".", (object) ex.Message);
        }
        catch (ArgumentException ex)
        {
          LanguagePrimitives.typeConversion.WriteLine("Exception finding Parse method with CultureInfo: \"{0}\".", (object) ex.Message);
        }
        if (methodInfo != null)
          return new LanguagePrimitives.PSConverter(new LanguagePrimitives.ConvertViaParseMethod()
          {
            parse = methodInfo
          }.ConvertWithCulture);
        try
        {
          methodInfo = toType.GetMethod("Parse", bindingAttr, (Binder) null, new Type[1]
          {
            typeof (string)
          }, (ParameterModifier[]) null);
        }
        catch (AmbiguousMatchException ex)
        {
          LanguagePrimitives.typeConversion.WriteLine("Exception finding Parse method: \"{0}\".", (object) ex.Message);
        }
        catch (ArgumentException ex)
        {
          LanguagePrimitives.typeConversion.WriteLine("Exception finding Parse method: \"{0}\".", (object) ex.Message);
        }
        if (methodInfo != null)
          return new LanguagePrimitives.PSConverter(new LanguagePrimitives.ConvertViaParseMethod()
          {
            parse = methodInfo
          }.ConvertWithoutCulture);
      }
      return (LanguagePrimitives.PSConverter) null;
    }

    internal static LanguagePrimitives.PSConverter FigureConstructorConversion(
      Type fromType,
      Type toType)
    {
      if (typeof (int).Equals(fromType) && (typeof (IList).IsAssignableFrom(toType) || typeof (ICollection).IsAssignableFrom(toType)))
      {
        LanguagePrimitives.typeConversion.WriteLine("Ignoring the collection constructor that takes an integer, since this is not semantically a conversion.", new object[0]);
        return (LanguagePrimitives.PSConverter) null;
      }
      ConstructorInfo constructorInfo = (ConstructorInfo) null;
      try
      {
        constructorInfo = toType.GetConstructor(new Type[1]
        {
          fromType
        });
      }
      catch (AmbiguousMatchException ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Exception finding Constructor: \"{0}\".", (object) ex.Message);
      }
      catch (ArgumentException ex)
      {
        LanguagePrimitives.typeConversion.WriteLine("Exception finding Constructor: \"{0}\".", (object) ex.Message);
      }
      if (constructorInfo == null)
        return (LanguagePrimitives.PSConverter) null;
      LanguagePrimitives.typeConversion.WriteLine("Found Constructor.", new object[0]);
      return new LanguagePrimitives.PSConverter(new LanguagePrimitives.ConvertViaConstructor()
      {
        constructor = constructorInfo
      }.Convert);
    }

    internal static LanguagePrimitives.PSConverter FigureCastConversion(
      Type fromType,
      Type toType,
      ref ConversionRank rank)
    {
      MethodInfo methodInfo = LanguagePrimitives.FindCastOperator("op_Implicit", toType, fromType, toType) ?? LanguagePrimitives.FindCastOperator("op_Explicit", toType, fromType, toType) ?? LanguagePrimitives.FindCastOperator("op_Implicit", fromType, fromType, toType) ?? LanguagePrimitives.FindCastOperator("op_Explicit", fromType, fromType, toType);
      if (methodInfo == null)
        return (LanguagePrimitives.PSConverter) null;
      rank = methodInfo.Name.Equals("op_Implicit", StringComparison.OrdinalIgnoreCase) ? ConversionRank.ImplicitCast : ConversionRank.ExplicitCast;
      return new LanguagePrimitives.PSConverter(new LanguagePrimitives.ConvertViaCast()
      {
        cast = methodInfo
      }.Convert);
    }

    private static bool TypeConverterPossiblyExists(Type type)
    {
      lock (LanguagePrimitives.possibleTypeConverter)
      {
        if (LanguagePrimitives.possibleTypeConverter.ContainsKey(type.FullName))
          return true;
      }
      return type.GetCustomAttributes(typeof (TypeConverterAttribute), false).Length != 0;
    }

    private static LanguagePrimitives.ConversionData FigureConversion(
      Type fromType,
      Type toType)
    {
      LanguagePrimitives.ConversionData conversionData1 = LanguagePrimitives.GetConversionData(fromType, toType);
      if (conversionData1 != null)
        return conversionData1;
      if (fromType.Equals(typeof (LanguagePrimitives.Null)))
        return LanguagePrimitives.FigureConversionFromNull(toType);
      if (toType.IsAssignableFrom(fromType))
        return LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertAssignableFrom), toType.Equals(fromType) ? ConversionRank.Identity : ConversionRank.Assignable);
      if (typeof (PSObject).IsAssignableFrom(fromType) && !typeof (LanguagePrimitives.InternalPSObject).Equals(fromType))
        return (LanguagePrimitives.ConversionData) null;
      if (toType.Equals(typeof (PSObject)))
        return LanguagePrimitives.CacheConversion(fromType, toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertToPSObject), ConversionRank.PSObject);
      LanguagePrimitives.PSConverter valueDependentConversion = (LanguagePrimitives.PSConverter) null;
      ConversionRank valueDependentRank = ConversionRank.None;
      LanguagePrimitives.ConversionData conversionData2 = LanguagePrimitives.FigureLanguageConversion(fromType, toType, out valueDependentConversion, out valueDependentRank);
      if (conversionData2 != null)
        return conversionData2;
      ConversionRank conversionRank = valueDependentConversion != null ? ConversionRank.Language : ConversionRank.None;
      LanguagePrimitives.PSConverter converter = LanguagePrimitives.FigureParseConversion(fromType, toType);
      ConversionRank rank;
      if (converter == null)
      {
        converter = LanguagePrimitives.FigureConstructorConversion(fromType, toType);
        rank = ConversionRank.Constructor;
        if (converter == null)
        {
          converter = LanguagePrimitives.FigureCastConversion(fromType, toType, ref rank);
          if (converter == null && typeof (IConvertible).IsAssignableFrom(fromType))
          {
            if (LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(fromType)))
            {
              if (!toType.IsArray && LanguagePrimitives.GetConversionRank(typeof (string), toType) != ConversionRank.None)
              {
                converter = new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNumericIConvertible);
                rank = ConversionRank.IConvertible;
              }
            }
            else if (fromType != typeof (string))
            {
              converter = new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertIConvertible);
              rank = ConversionRank.IConvertible;
            }
          }
        }
        else
          rank = ConversionRank.Constructor;
      }
      else
        rank = ConversionRank.Parse;
      if (LanguagePrimitives.TypeConverterPossiblyExists(fromType) || LanguagePrimitives.TypeConverterPossiblyExists(toType) || converter != null && valueDependentConversion != null)
      {
        converter = new LanguagePrimitives.PSConverter(new LanguagePrimitives.ConvertCheckingForCustomConverter()
        {
          tryfirstConverter = valueDependentConversion,
          fallbackConverter = converter
        }.Convert);
        if (valueDependentRank > rank)
          rank = valueDependentRank;
        else if (rank == ConversionRank.None)
          rank = ConversionRank.Custom;
      }
      else if (valueDependentConversion != null)
      {
        converter = valueDependentConversion;
        rank = ConversionRank.Language;
      }
      return converter != null ? LanguagePrimitives.CacheConversion(fromType, toType, converter, rank) : (LanguagePrimitives.ConversionData) null;
    }

    private static LanguagePrimitives.ConversionData FigureConversionFromNull(
      Type toType)
    {
      LanguagePrimitives.ConversionData conversionData = LanguagePrimitives.GetConversionData(typeof (LanguagePrimitives.Null), toType);
      if (conversionData != null)
        return conversionData;
      if (Nullable.GetUnderlyingType(toType) != null)
        return LanguagePrimitives.CacheConversion(typeof (LanguagePrimitives.Null), toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNullToNullable), ConversionRank.NullToValue);
      return !toType.IsValueType ? LanguagePrimitives.CacheConversion(typeof (LanguagePrimitives.Null), toType, new LanguagePrimitives.PSConverter(LanguagePrimitives.ConvertNullToRef), ConversionRank.NullToRef) : (LanguagePrimitives.ConversionData) null;
    }

    private static string ObjectToTypeNameString(object o)
    {
      if (o == null)
        return "null";
      PSObject psObject = PSObject.AsPSObject(o);
      return psObject.TypeNames != null && psObject.TypeNames.Count > 0 ? psObject.TypeNames[0] : ToStringCodeMethods.Type(o.GetType());
    }

    private class EnumerableTWrapper : IEnumerable
    {
      private object _enumerable;
      private Type _enumerableType;
      private DynamicMethod _getEnumerator;

      internal EnumerableTWrapper(object enumerable, Type enumerableType)
      {
        this._enumerable = enumerable;
        this._enumerableType = enumerableType;
        this.CreateGetEnumerator();
      }

      private void CreateGetEnumerator()
      {
        this._getEnumerator = new DynamicMethod("GetEnumerator", typeof (object), new Type[1]
        {
          typeof (object)
        }, typeof (LanguagePrimitives).Module, true);
        ILGenerator ilGenerator = this._getEnumerator.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Castclass, this._enumerableType);
        MethodInfo method = this._enumerableType.GetMethod("GetEnumerator", new Type[0]);
        ilGenerator.EmitCall(OpCodes.Callvirt, method, (Type[]) null);
        ilGenerator.Emit(OpCodes.Ret);
      }

      internal Type EnumeratorType => this._enumerableType.GetMethod("GetEnumerator", new Type[0]).ReturnType;

      public IEnumerator GetEnumerator() => (IEnumerator) this._getEnumerator.Invoke((object) null, new object[1]
      {
        this._enumerable
      });
    }

    private delegate IEnumerable GetEnumerableDelegate(object obj);

    private class EnumeratorTWrapper : IEnumerator
    {
      private object _enumerator;
      private Type _enumeratorType;
      private DynamicMethod _current;

      internal EnumeratorTWrapper(object enumerator, Type enumeratorType)
      {
        this._enumerator = enumerator;
        this._enumeratorType = enumeratorType;
        this.CreateCurrent();
      }

      private void CreateCurrent()
      {
        this._current = new DynamicMethod("Current", typeof (object), new Type[1]
        {
          this._enumeratorType
        }, typeof (LanguagePrimitives.EnumeratorTWrapper).Module, true);
        ILGenerator ilGenerator = this._current.GetILGenerator();
        MethodInfo method = this._enumeratorType.GetMethod("get_Current", new Type[0]);
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.EmitCall(OpCodes.Callvirt, method, (Type[]) null);
        if (method.ReturnType.IsValueType)
          ilGenerator.Emit(OpCodes.Box, method.ReturnType);
        ilGenerator.Emit(OpCodes.Ret);
      }

      public object Current => this._current.Invoke((object) null, new object[1]
      {
        this._enumerator
      });

      public bool MoveNext() => ((IEnumerator) this._enumerator).MoveNext();

      public void Reset() => ((IEnumerator) this._enumerator).Reset();
    }

    internal class EnumSingleTypeConverter : PSTypeConverter
    {
      private const int maxEnumTableSize = 100;
      private static HybridDictionary enumTable = new HybridDictionary();

      private static LanguagePrimitives.EnumSingleTypeConverter.EnumHashEntry GetEnumHashEntry(
        Type enumType)
      {
        lock (LanguagePrimitives.EnumSingleTypeConverter.enumTable)
        {
          LanguagePrimitives.EnumSingleTypeConverter.EnumHashEntry enumHashEntry1 = (LanguagePrimitives.EnumSingleTypeConverter.EnumHashEntry) LanguagePrimitives.EnumSingleTypeConverter.enumTable[(object) enumType];
          if (enumHashEntry1 != null)
            return enumHashEntry1;
          if (LanguagePrimitives.EnumSingleTypeConverter.enumTable.Count == 100)
            LanguagePrimitives.EnumSingleTypeConverter.enumTable.Clear();
          ulong allValues = 0;
          bool hasNegativeValue = false;
          Array values = Enum.GetValues(enumType);
          if (LanguagePrimitives.IsSignedInteger(Type.GetTypeCode(enumType)))
          {
            foreach (IConvertible convertible in values)
            {
              if (convertible.ToInt64((IFormatProvider) null) < 0L)
              {
                hasNegativeValue = true;
                break;
              }
              allValues |= convertible.ToUInt64((IFormatProvider) null);
            }
          }
          else
          {
            foreach (IConvertible convertible in values)
              allValues |= convertible.ToUInt64((IFormatProvider) null);
          }
          bool hasFlagsAttribute = enumType.GetCustomAttributes(typeof (FlagsAttribute), false).Length > 0;
          LanguagePrimitives.EnumSingleTypeConverter.EnumHashEntry enumHashEntry2 = new LanguagePrimitives.EnumSingleTypeConverter.EnumHashEntry(Enum.GetNames(enumType), values, allValues, hasNegativeValue, hasFlagsAttribute);
          LanguagePrimitives.EnumSingleTypeConverter.enumTable.Add((object) enumType, (object) enumHashEntry2);
          return enumHashEntry2;
        }
      }

      public override bool CanConvertFrom(object sourceValue, Type destinationType) => sourceValue != null && sourceValue is string && destinationType.IsEnum;

      private static bool IsDefinedEnum(object enumValue, Type enumType)
      {
        bool flag;
        if (enumValue == null)
        {
          flag = false;
        }
        else
        {
          LanguagePrimitives.EnumSingleTypeConverter.EnumHashEntry enumHashEntry = LanguagePrimitives.EnumSingleTypeConverter.GetEnumHashEntry(enumType);
          if (enumHashEntry.hasNegativeValue)
          {
            flag = true;
          }
          else
          {
            IConvertible convertible = (IConvertible) enumValue;
            if (LanguagePrimitives.IsSignedInteger(Type.GetTypeCode(enumValue.GetType())) && convertible.ToInt64((IFormatProvider) null) < 0L)
            {
              flag = false;
            }
            else
            {
              ulong uint64 = convertible.ToUInt64((IFormatProvider) null);
              flag = !enumHashEntry.hasFlagsAttribute ? Array.IndexOf(enumHashEntry.values, enumValue) >= 0 : (((long) uint64 | (long) enumHashEntry.allValues) ^ (long) enumHashEntry.allValues) == 0L;
            }
          }
        }
        return flag;
      }

      internal static void ThrowForUndefinedEnum(string errorId, object enumValue, Type enumType) => LanguagePrimitives.EnumSingleTypeConverter.ThrowForUndefinedEnum(errorId, enumValue, enumValue, enumType);

      internal static void ThrowForUndefinedEnum(
        string errorId,
        object enumValue,
        object valueToUseToThrow,
        Type enumType)
      {
        if (!LanguagePrimitives.EnumSingleTypeConverter.IsDefinedEnum(enumValue, enumType))
        {
          LanguagePrimitives.typeConversion.WriteLine("Value {0} is not defined in the Enum {1}.", valueToUseToThrow, (object) enumType);
          throw new PSInvalidCastException(errorId, (Exception) null, "ExtendedTypeSystem", "InvalidCastExceptionEnumerationNoValue", new object[3]
          {
            valueToUseToThrow,
            (object) enumType,
            (object) LanguagePrimitives.EnumSingleTypeConverter.EnumValues(enumType)
          });
        }
      }

      internal static string EnumValues(Type enumType)
      {
        string[] names = LanguagePrimitives.EnumSingleTypeConverter.GetEnumHashEntry(enumType).names;
        string resourceString = ResourceManagerCache.GetResourceString("ExtendedTypeSystem", "ListSeparator");
        StringBuilder stringBuilder = new StringBuilder();
        if (names.Length != 0)
        {
          for (int index = 0; index < names.Length; ++index)
          {
            stringBuilder.Append(names[index]);
            stringBuilder.Append(resourceString);
          }
          stringBuilder.Remove(stringBuilder.Length - resourceString.Length, resourceString.Length);
        }
        return stringBuilder.ToString();
      }

      public override object ConvertFrom(
        object sourceValue,
        Type destinationType,
        IFormatProvider formatProvider,
        bool ignoreCase)
      {
        return LanguagePrimitives.EnumSingleTypeConverter.BaseConvertFrom(sourceValue, destinationType, formatProvider, ignoreCase, false);
      }

      protected static object BaseConvertFrom(
        object sourceValue,
        Type destinationType,
        IFormatProvider formatProvider,
        bool ignoreCase,
        bool multipleValues)
      {
        if (!(sourceValue is string str))
          throw new PSInvalidCastException("InvalidCastEnumFromTypeNotAString", (Exception) null, "ExtendedTypeSystem", "InvalidCastException", new object[3]
          {
            sourceValue,
            (object) LanguagePrimitives.ObjectToTypeNameString(sourceValue),
            (object) destinationType
          });
        string pattern1 = str.Length != 0 ? str.Trim() : throw new PSInvalidCastException("InvalidCastEnumFromEmptyString", (Exception) null, "ExtendedTypeSystem", "InvalidCastException", new object[3]
        {
          sourceValue,
          (object) LanguagePrimitives.ObjectToTypeNameString(sourceValue),
          (object) destinationType
        });
        if (pattern1.Length == 0)
          throw new PSInvalidCastException("InvalidEnumCastFromEmptyStringAfterTrim", (Exception) null, "ExtendedTypeSystem", "InvalidCastException", new object[3]
          {
            sourceValue,
            (object) LanguagePrimitives.ObjectToTypeNameString(sourceValue),
            (object) destinationType
          });
        if (char.IsDigit(pattern1[0]) || pattern1[0] == '+' || pattern1[0] == '-')
        {
          Type underlyingType = Enum.GetUnderlyingType(destinationType);
          try
          {
            object enumValue = Enum.ToObject(destinationType, Convert.ChangeType((object) pattern1, underlyingType, formatProvider));
            LanguagePrimitives.EnumSingleTypeConverter.ThrowForUndefinedEnum("UndefinedInEnumSingleTypeConverter", enumValue, (object) pattern1, destinationType);
            return enumValue;
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
          }
        }
        string[] strArray;
        WildcardPattern[] wildcardPatternArray;
        if (!multipleValues)
        {
          strArray = !pattern1.Contains(",") ? new string[1]
          {
            pattern1
          } : throw new PSInvalidCastException("InvalidCastEnumCommaAndNoFlags", (Exception) null, "ExtendedTypeSystem", "InvalidCastExceptionEnumerationNoFlagAndComma", new object[2]
          {
            sourceValue,
            (object) destinationType
          });
          wildcardPatternArray = new WildcardPattern[1]
          {
            !WildcardPattern.ContainsWildcardCharacters(pattern1) ? (WildcardPattern) null : new WildcardPattern(pattern1, ignoreCase ? WildcardOptions.IgnoreCase : WildcardOptions.None)
          };
        }
        else
        {
          strArray = pattern1.Split(',');
          wildcardPatternArray = new WildcardPattern[strArray.Length];
          for (int index = 0; index < strArray.Length; ++index)
          {
            string pattern2 = strArray[index];
            wildcardPatternArray[index] = !WildcardPattern.ContainsWildcardCharacters(pattern2) ? (WildcardPattern) null : new WildcardPattern(pattern2, ignoreCase ? WildcardOptions.IgnoreCase : WildcardOptions.None);
          }
        }
        LanguagePrimitives.EnumSingleTypeConverter.EnumHashEntry enumHashEntry = LanguagePrimitives.EnumSingleTypeConverter.GetEnumHashEntry(destinationType);
        string[] names = enumHashEntry.names;
        Array values = enumHashEntry.values;
        ulong num = 0;
        StringComparison comparisonType = !ignoreCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        for (int index1 = 0; index1 < strArray.Length; ++index1)
        {
          string strA = strArray[index1];
          WildcardPattern wildcardPattern = wildcardPatternArray[index1];
          bool flag = false;
          for (int index2 = 0; index2 < names.Length; ++index2)
          {
            string str = names[index2];
            if (wildcardPattern != null)
            {
              if (!wildcardPattern.IsMatch(str))
                continue;
            }
            else if (string.Compare(strA, str, comparisonType) != 0)
              continue;
            if (!multipleValues && flag)
            {
              object obj1 = Enum.ToObject(destinationType, num);
              object obj2 = Enum.ToObject(destinationType, ((IConvertible) values.GetValue(index1)).ToUInt64((IFormatProvider) null));
              throw new PSInvalidCastException("InvalidCastEnumTwoStringsFoundAndNoFlags", (Exception) null, "ExtendedTypeSystem", "InvalidCastExceptionEnumerationMoreThanOneValue", new object[4]
              {
                sourceValue,
                (object) destinationType,
                obj1,
                obj2
              });
            }
            flag = true;
            num |= ((IConvertible) values.GetValue(index2)).ToUInt64((IFormatProvider) null);
          }
          if (!flag)
            throw new PSInvalidCastException("InvalidCastEnumStringNotFound", (Exception) null, "ExtendedTypeSystem", "InvalidCastExceptionEnumerationNoValue", new object[3]
            {
              (object) strA,
              (object) destinationType,
              (object) LanguagePrimitives.EnumSingleTypeConverter.EnumValues(destinationType)
            });
        }
        return Enum.ToObject(destinationType, num);
      }

      public override bool CanConvertTo(object sourceValue, Type destinationType) => false;

      public override object ConvertTo(
        object sourceValue,
        Type destinationType,
        IFormatProvider formatProvider,
        bool ignoreCase)
      {
        throw LanguagePrimitives.tracer.NewNotSupportedException();
      }

      private class EnumHashEntry
      {
        internal string[] names;
        internal Array values;
        internal ulong allValues;
        internal bool hasNegativeValue;
        internal bool hasFlagsAttribute;

        internal EnumHashEntry(
          string[] names,
          Array values,
          ulong allValues,
          bool hasNegativeValue,
          bool hasFlagsAttribute)
        {
          this.names = names;
          this.values = values;
          this.allValues = allValues;
          this.hasNegativeValue = hasNegativeValue;
          this.hasFlagsAttribute = hasFlagsAttribute;
        }
      }
    }

    internal class EnumMultipleTypeConverter : LanguagePrimitives.EnumSingleTypeConverter
    {
      public override object ConvertFrom(
        object sourceValue,
        Type destinationType,
        IFormatProvider formatProvider,
        bool ignoreCase)
      {
        return LanguagePrimitives.EnumSingleTypeConverter.BaseConvertFrom(sourceValue, destinationType, formatProvider, ignoreCase, true);
      }
    }

    private class ConvertViaParseMethod
    {
      internal MethodInfo parse;

      internal object ConvertWithCulture(
        object valueToConvert,
        Type resultType,
        bool recursion,
        PSObject originalValueToConvert,
        IFormatProvider formatProvider,
        TypeTable backupTable)
      {
        try
        {
          object obj = this.parse.Invoke((object) null, new object[2]
          {
            valueToConvert,
            (object) formatProvider
          });
          LanguagePrimitives.typeConversion.WriteLine("Parse result: {0}", obj);
          return obj;
        }
        catch (TargetInvocationException ex)
        {
          Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
          LanguagePrimitives.typeConversion.WriteLine("Exception calling Parse method with CultureInfo: \"{0}\".", (object) innerException.Message);
          throw new PSInvalidCastException("InvalidCastParseTargetInvocationWithFormatProvider", innerException, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
          {
            (object) valueToConvert.ToString(),
            (object) resultType.ToString(),
            (object) innerException.Message
          });
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          LanguagePrimitives.typeConversion.WriteLine("Exception calling Parse method with CultureInfo: \"{0}\".", (object) ex.Message);
          throw new PSInvalidCastException("InvalidCastParseExceptionWithFormatProvider", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
          {
            (object) valueToConvert.ToString(),
            (object) resultType.ToString(),
            (object) ex.Message
          });
        }
      }

      internal object ConvertWithoutCulture(
        object valueToConvert,
        Type resultType,
        bool recursion,
        PSObject originalValueToConvert,
        IFormatProvider formatProvider,
        TypeTable backupTable)
      {
        try
        {
          object obj = this.parse.Invoke((object) null, new object[1]
          {
            valueToConvert
          });
          LanguagePrimitives.typeConversion.WriteLine("Parse result: \"{0}\".", obj);
          return obj;
        }
        catch (TargetInvocationException ex)
        {
          Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
          LanguagePrimitives.typeConversion.WriteLine("Exception calling Parse method: \"{0}\".", (object) innerException.Message);
          throw new PSInvalidCastException("InvalidCastParseTargetInvocation", innerException, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
          {
            (object) valueToConvert.ToString(),
            (object) resultType.ToString(),
            (object) innerException.Message
          });
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          LanguagePrimitives.typeConversion.WriteLine("Exception calling Parse method: \"{0}\".", (object) ex.Message);
          throw new PSInvalidCastException("InvalidCastParseException", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
          {
            (object) valueToConvert.ToString(),
            (object) resultType.ToString(),
            (object) ex.Message
          });
        }
      }
    }

    private class ConvertViaConstructor
    {
      internal ConstructorInfo constructor;

      internal object Convert(
        object valueToConvert,
        Type resultType,
        bool recursion,
        PSObject originalValueToConvert,
        IFormatProvider formatProvider,
        TypeTable backupTable)
      {
        try
        {
          object obj = this.constructor.Invoke(new object[1]
          {
            valueToConvert
          });
          LanguagePrimitives.typeConversion.WriteLine("Constructor result: \"{0}\".", obj);
          return obj;
        }
        catch (TargetInvocationException ex)
        {
          Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
          LanguagePrimitives.typeConversion.WriteLine("Exception invoking Constructor: \"{0}\".", (object) innerException.Message);
          throw new PSInvalidCastException("InvalidCastConstructorTargetInvocationException", innerException, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
          {
            (object) valueToConvert.ToString(),
            (object) resultType.ToString(),
            (object) innerException.Message
          });
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          LanguagePrimitives.typeConversion.WriteLine("Exception invoking Constructor: \"{0}\".", (object) ex.Message);
          throw new PSInvalidCastException("InvalidCastConstructorException", ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
          {
            (object) valueToConvert.ToString(),
            (object) resultType.ToString(),
            (object) ex.Message
          });
        }
      }
    }

    private class ConvertViaCast
    {
      internal MethodInfo cast;

      internal object Convert(
        object valueToConvert,
        Type resultType,
        bool recursion,
        PSObject originalValueToConvert,
        IFormatProvider formatProvider,
        TypeTable backupTable)
      {
        try
        {
          return this.cast.Invoke((object) null, new object[1]
          {
            valueToConvert
          });
        }
        catch (TargetInvocationException ex)
        {
          Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
          LanguagePrimitives.typeConversion.WriteLine("Cast operator exception: \"{0}\".", (object) innerException.Message);
          throw new PSInvalidCastException("InvalidCastTargetInvocationException" + this.cast.Name, innerException, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
          {
            (object) valueToConvert.ToString(),
            (object) resultType.ToString(),
            (object) innerException.Message
          });
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          LanguagePrimitives.typeConversion.WriteLine("Cast operator exception: \"{0}\".", (object) ex.Message);
          throw new PSInvalidCastException("InvalidCastException" + this.cast.Name, ex, "ExtendedTypeSystem", "InvalidCastExceptionWithInnerException", new object[3]
          {
            (object) valueToConvert.ToString(),
            (object) resultType.ToString(),
            (object) ex.Message
          });
        }
      }
    }

    private class ConvertCheckingForCustomConverter
    {
      internal LanguagePrimitives.PSConverter tryfirstConverter;
      internal LanguagePrimitives.PSConverter fallbackConverter;

      internal object Convert(
        object valueToConvert,
        Type resultType,
        bool recursion,
        PSObject originalValueToConvert,
        IFormatProvider formatProvider,
        TypeTable backupTable)
      {
        object result = (object) null;
        if (this.tryfirstConverter != null)
        {
          try
          {
            return this.tryfirstConverter(valueToConvert, resultType, recursion, originalValueToConvert, formatProvider, backupTable);
          }
          catch (InvalidCastException ex)
          {
          }
        }
        if (LanguagePrimitives.IsCustomTypeConversion((object) originalValueToConvert ?? valueToConvert, resultType, formatProvider, out result, backupTable))
        {
          LanguagePrimitives.typeConversion.WriteLine("Custom Type Conversion succeeded.", new object[0]);
          return result;
        }
        if (this.fallbackConverter != null)
          return this.fallbackConverter(valueToConvert, resultType, recursion, originalValueToConvert, formatProvider, backupTable);
        throw new PSInvalidCastException("ConvertToFinalInvalidCastException", (Exception) null, "ExtendedTypeSystem", "InvalidCastException", new object[3]
        {
          (object) valueToConvert.ToString(),
          (object) LanguagePrimitives.ObjectToTypeNameString(valueToConvert),
          (object) resultType.ToString()
        });
      }
    }

    private class ConversionTypePair
    {
      private Type from;
      private Type to;

      internal ConversionTypePair(Type from, Type to)
      {
        this.from = from;
        this.to = to;
      }

      public override int GetHashCode() => this.from.GetHashCode() + 37 * this.to.GetHashCode();

      public override bool Equals(object other) => other is LanguagePrimitives.ConversionTypePair conversionTypePair && this.from == conversionTypePair.from && this.to == conversionTypePair.to;
    }

    internal delegate object PSConverter(
      object valueToConvert,
      Type resultType,
      bool recurse,
      PSObject originalValueToConvert,
      IFormatProvider formatProvider,
      TypeTable backupTable);

    internal delegate object PSNullConverter(object nullOrAutomationNull);

    private class ConversionData
    {
      internal LanguagePrimitives.PSConverter converter;
      internal ConversionRank rank;

      internal ConversionData(LanguagePrimitives.PSConverter converter, ConversionRank rank)
      {
        this.converter = converter;
        this.rank = rank;
      }
    }

    internal class InternalPSObject : PSObject
    {
    }

    internal class Null
    {
    }
  }
}
