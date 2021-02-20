// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSPrimitiveDictionary
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Runtime.Serialization;
using System.Xml;

namespace System.Management.Automation
{
  [Serializable]
  public sealed class PSPrimitiveDictionary : Hashtable
  {
    private static readonly Type[] handshakeFriendlyTypes = new Type[22]
    {
      typeof (bool),
      typeof (byte),
      typeof (char),
      typeof (DateTime),
      typeof (Decimal),
      typeof (double),
      typeof (Guid),
      typeof (int),
      typeof (long),
      typeof (sbyte),
      typeof (float),
      typeof (string),
      typeof (TimeSpan),
      typeof (ushort),
      typeof (uint),
      typeof (ulong),
      typeof (Uri),
      typeof (byte[]),
      typeof (Version),
      typeof (ProgressRecord),
      typeof (XmlDocument),
      typeof (PSPrimitiveDictionary)
    };

    public PSPrimitiveDictionary()
      : base((IEqualityComparer) StringComparer.OrdinalIgnoreCase)
    {
    }

    public PSPrimitiveDictionary(Hashtable other)
      : base((IEqualityComparer) StringComparer.OrdinalIgnoreCase)
    {
      if (other == null)
        throw new ArgumentNullException(nameof (other));
      foreach (DictionaryEntry dictionaryEntry in other)
      {
        if (PSObject.Base(dictionaryEntry.Value) is Hashtable other1)
          this.Add(dictionaryEntry.Key, (object) new PSPrimitiveDictionary(other1));
        else
          this.Add(dictionaryEntry.Key, dictionaryEntry.Value);
      }
    }

    private PSPrimitiveDictionary(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    private string VerifyKey(object key)
    {
      key = PSObject.Base(key);
      return key is string str ? str : throw new ArgumentException(ResourceManagerCache.FormatResourceString("Serialization", "PrimitiveHashtableInvalidKey", (object) key.GetType().FullName));
    }

    private void VerifyValue(object value)
    {
      if (value == null)
        return;
      value = PSObject.Base(value);
      Type type = value.GetType();
      foreach (Type handshakeFriendlyType in PSPrimitiveDictionary.handshakeFriendlyTypes)
      {
        if (type == handshakeFriendlyType)
          return;
      }
      if (type.IsArray || type.Equals(typeof (ArrayList)))
      {
        foreach (object obj in (IEnumerable) value)
          this.VerifyValue(obj);
      }
      else
        throw new ArgumentException(ResourceManagerCache.FormatResourceString("Serialization", "PrimitiveHashtableInvalidValue", (object) value.GetType().FullName));
    }

    public override void Add(object key, object value)
    {
      string str = this.VerifyKey(key);
      this.VerifyValue(value);
      base.Add((object) str, value);
    }

    public override object this[object key]
    {
      get => base[key];
      set
      {
        string str = this.VerifyKey(key);
        this.VerifyValue(value);
        base[(object) str] = value;
      }
    }

    public object this[string key]
    {
      get => base[(object) key];
      set
      {
        this.VerifyValue(value);
        base[(object) key] = value;
      }
    }

    public override object Clone() => (object) new PSPrimitiveDictionary((Hashtable) this);

    public void Add(string key, bool value) => this.Add((object) key, (object) value);

    public void Add(string key, bool[] value) => this.Add((object) key, (object) value);

    public void Add(string key, byte value) => this.Add((object) key, (object) value);

    public void Add(string key, byte[] value) => this.Add((object) key, (object) value);

    public void Add(string key, char value) => this.Add((object) key, (object) value);

    public void Add(string key, char[] value) => this.Add((object) key, (object) value);

    public void Add(string key, DateTime value) => this.Add((object) key, (object) value);

    public void Add(string key, DateTime[] value) => this.Add((object) key, (object) value);

    public void Add(string key, Decimal value) => this.Add((object) key, (object) value);

    public void Add(string key, Decimal[] value) => this.Add((object) key, (object) value);

    public void Add(string key, double value) => this.Add((object) key, (object) value);

    public void Add(string key, double[] value) => this.Add((object) key, (object) value);

    public void Add(string key, Guid value) => this.Add((object) key, (object) value);

    public void Add(string key, Guid[] value) => this.Add((object) key, (object) value);

    public void Add(string key, int value) => this.Add((object) key, (object) value);

    public void Add(string key, int[] value) => this.Add((object) key, (object) value);

    public void Add(string key, long value) => this.Add((object) key, (object) value);

    public void Add(string key, long[] value) => this.Add((object) key, (object) value);

    public void Add(string key, sbyte value) => this.Add((object) key, (object) value);

    public void Add(string key, sbyte[] value) => this.Add((object) key, (object) value);

    public void Add(string key, float value) => this.Add((object) key, (object) value);

    public void Add(string key, float[] value) => this.Add((object) key, (object) value);

    public void Add(string key, string value) => this.Add((object) key, (object) value);

    public void Add(string key, string[] value) => this.Add((object) key, (object) value);

    public void Add(string key, TimeSpan value) => this.Add((object) key, (object) value);

    public void Add(string key, TimeSpan[] value) => this.Add((object) key, (object) value);

    public void Add(string key, ushort value) => this.Add((object) key, (object) value);

    public void Add(string key, ushort[] value) => this.Add((object) key, (object) value);

    public void Add(string key, uint value) => this.Add((object) key, (object) value);

    public void Add(string key, uint[] value) => this.Add((object) key, (object) value);

    public void Add(string key, ulong value) => this.Add((object) key, (object) value);

    public void Add(string key, ulong[] value) => this.Add((object) key, (object) value);

    public void Add(string key, Uri value) => this.Add((object) key, (object) value);

    public void Add(string key, Uri[] value) => this.Add((object) key, (object) value);

    public void Add(string key, Version value) => this.Add((object) key, (object) value);

    public void Add(string key, Version[] value) => this.Add((object) key, (object) value);

    public void Add(string key, PSPrimitiveDictionary value) => this.Add((object) key, (object) value);

    public void Add(string key, PSPrimitiveDictionary[] value) => this.Add((object) key, (object) value);

    internal static PSPrimitiveDictionary CloneAndAddPSVersionTable(
      PSPrimitiveDictionary originalHash)
    {
      if (originalHash != null && originalHash.ContainsKey((object) "PSVersionTable"))
        return (PSPrimitiveDictionary) originalHash.Clone();
      PSPrimitiveDictionary primitiveDictionary1 = originalHash == null ? new PSPrimitiveDictionary() : (PSPrimitiveDictionary) originalHash.Clone();
      PSPrimitiveDictionary primitiveDictionary2 = new PSPrimitiveDictionary(PSVersionInfo.GetPSVersionTable());
      primitiveDictionary1.Add("PSVersionTable", primitiveDictionary2);
      return primitiveDictionary1;
    }

    internal static bool TryPathGet<T>(IDictionary data, out T result, params string[] keys)
    {
      if (data == null || !data.Contains((object) keys[0]))
      {
        result = default (T);
        return false;
      }
      if (keys.Length == 1)
        return LanguagePrimitives.TryConvertTo<T>(data[(object) keys[0]], out result);
      PSPrimitiveDictionary result1;
      if (LanguagePrimitives.TryConvertTo<PSPrimitiveDictionary>(data[(object) keys[0]], out result1) && result1 != null)
      {
        string[] strArray = new string[keys.Length - 1];
        Array.Copy((Array) keys, 1, (Array) strArray, 0, strArray.Length);
        return PSPrimitiveDictionary.TryPathGet<T>((IDictionary) result1, out result, strArray);
      }
      result = default (T);
      return false;
    }
  }
}
