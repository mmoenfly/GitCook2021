// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.KnownTypes
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Xml;

namespace System.Management.Automation
{
  internal static class KnownTypes
  {
    private static readonly TypeSerializationInfo _xdInfo = new TypeSerializationInfo(typeof (XmlDocument), "XD", "XD", new TypeSerializerDelegate(InternalSerializer.WriteXmlDocument), new TypeDeserializerDelegate(InternalDeserializer.DeserializeXmlDocument));
    private static readonly TypeSerializationInfo[] _TypeSerializationInfo = new TypeSerializationInfo[23]
    {
      new TypeSerializationInfo(typeof (bool), "B", "B", new TypeSerializerDelegate(InternalSerializer.WriteBoolean), new TypeDeserializerDelegate(InternalDeserializer.DeserializeBoolean)),
      new TypeSerializationInfo(typeof (byte), "By", "By", (TypeSerializerDelegate) null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeByte)),
      new TypeSerializationInfo(typeof (char), "C", "C", new TypeSerializerDelegate(InternalSerializer.WriteChar), new TypeDeserializerDelegate(InternalDeserializer.DeserializeChar)),
      new TypeSerializationInfo(typeof (DateTime), "DT", "DT", new TypeSerializerDelegate(InternalSerializer.WriteDateTime), new TypeDeserializerDelegate(InternalDeserializer.DeserializeDateTime)),
      new TypeSerializationInfo(typeof (Decimal), "D", "D", (TypeSerializerDelegate) null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeDecimal)),
      new TypeSerializationInfo(typeof (double), "Db", "Db", new TypeSerializerDelegate(InternalSerializer.WriteDouble), new TypeDeserializerDelegate(InternalDeserializer.DeserializeDouble)),
      new TypeSerializationInfo(typeof (Guid), "G", "G", (TypeSerializerDelegate) null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeGuid)),
      new TypeSerializationInfo(typeof (short), "I16", "I16", (TypeSerializerDelegate) null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeInt16)),
      new TypeSerializationInfo(typeof (int), "I32", "I32", (TypeSerializerDelegate) null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeInt32)),
      new TypeSerializationInfo(typeof (long), "I64", "I64", (TypeSerializerDelegate) null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeInt64)),
      new TypeSerializationInfo(typeof (sbyte), "SB", "SB", (TypeSerializerDelegate) null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeSByte)),
      new TypeSerializationInfo(typeof (float), "Sg", "Sg", new TypeSerializerDelegate(InternalSerializer.WriteSingle), new TypeDeserializerDelegate(InternalDeserializer.DeserializeSingle)),
      new TypeSerializationInfo(typeof (ScriptBlock), "SBK", "SBK", new TypeSerializerDelegate(InternalSerializer.WriteScriptBlock), new TypeDeserializerDelegate(InternalDeserializer.DeserializeScriptBlock)),
      new TypeSerializationInfo(typeof (string), "S", "S", new TypeSerializerDelegate(InternalSerializer.WriteEncodedString), new TypeDeserializerDelegate(InternalDeserializer.DeserializeString)),
      new TypeSerializationInfo(typeof (TimeSpan), "TS", "TS", new TypeSerializerDelegate(InternalSerializer.WriteTimeSpan), new TypeDeserializerDelegate(InternalDeserializer.DeserializeTimeSpan)),
      new TypeSerializationInfo(typeof (ushort), "U16", "U16", (TypeSerializerDelegate) null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeUInt16)),
      new TypeSerializationInfo(typeof (uint), "U32", "U32", (TypeSerializerDelegate) null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeUInt32)),
      new TypeSerializationInfo(typeof (ulong), "U64", "U64", (TypeSerializerDelegate) null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeUInt64)),
      new TypeSerializationInfo(typeof (Uri), "URI", "URI", new TypeSerializerDelegate(InternalSerializer.WriteUri), new TypeDeserializerDelegate(InternalDeserializer.DeserializeUri)),
      new TypeSerializationInfo(typeof (byte[]), "BA", "BA", new TypeSerializerDelegate(InternalSerializer.WriteByteArray), new TypeDeserializerDelegate(InternalDeserializer.DeserializeByteArray)),
      new TypeSerializationInfo(typeof (Version), "Version", "Version", new TypeSerializerDelegate(InternalSerializer.WriteVersion), new TypeDeserializerDelegate(InternalDeserializer.DeserializeVersion)),
      KnownTypes._xdInfo,
      new TypeSerializationInfo(typeof (ProgressRecord), "PR", "PR", new TypeSerializerDelegate(InternalSerializer.WriteProgressRecord), new TypeDeserializerDelegate(InternalDeserializer.DeserializeProgressRecord))
    };
    private static readonly Hashtable _knownTableKeyType = new Hashtable();
    private static readonly Hashtable _knownTableKeyItemTag = new Hashtable();
    [TraceSource("Serialization", "KnownTypes class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("Serialization", "KnownTypes class");

    static KnownTypes()
    {
      for (int index = 0; index < KnownTypes._TypeSerializationInfo.Length; ++index)
      {
        KnownTypes._knownTableKeyType.Add((object) KnownTypes._TypeSerializationInfo[index].Type.FullName, (object) KnownTypes._TypeSerializationInfo[index]);
        KnownTypes._knownTableKeyItemTag.Add((object) KnownTypes._TypeSerializationInfo[index].ItemTag, (object) KnownTypes._TypeSerializationInfo[index]);
      }
    }

    internal static TypeSerializationInfo GetTypeSerializationInfo(Type type)
    {
      TypeSerializationInfo xdInfo = (TypeSerializationInfo) KnownTypes._knownTableKeyType[(object) type.FullName];
      if (xdInfo == null && typeof (XmlDocument).IsAssignableFrom(type))
        xdInfo = KnownTypes._xdInfo;
      return xdInfo;
    }

    internal static TypeSerializationInfo GetTypeSerializationInfoFromItemTag(
      string itemTag)
    {
      return (TypeSerializationInfo) KnownTypes._knownTableKeyItemTag[(object) itemTag];
    }
  }
}
