// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.InternalDeserializer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Xml;

namespace System.Management.Automation
{
  internal class InternalDeserializer
  {
    private const int MaxDepthBelowTopLevel = 50;
    private XmlReader _reader;
    private DeserializationContext _context;
    private TypeTable _typeTable;
    private int depthBelowTopLevel;
    private Version _version;
    private readonly ReferenceIdHandlerForDeserializer<object> objectRefIdHandler;
    private readonly ReferenceIdHandlerForDeserializer<ConsolidatedString> typeRefIdHandler;
    private bool isStopping;
    [TraceSource("InternalDeserializer", "InternalDeserializer class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (InternalDeserializer), "InternalDeserializer class");
    private static IETWTracer etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Serializer);

    private bool UnknownTagsAllowed => this._version.Minor > 1;

    private bool DuplicateRefIdsAllowed => true;

    internal InternalDeserializer(XmlReader reader, DeserializationContext context)
    {
      this._reader = reader;
      this._context = context;
      this.objectRefIdHandler = new ReferenceIdHandlerForDeserializer<object>();
      this.typeRefIdHandler = new ReferenceIdHandlerForDeserializer<ConsolidatedString>();
    }

    internal TypeTable TypeTable
    {
      get => this._typeTable;
      set => this._typeTable = value;
    }

    internal void ValidateVersion(string version)
    {
      this._version = (Version) null;
      Exception innerException = (Exception) null;
      try
      {
        this._version = new Version(version);
      }
      catch (ArgumentException ex)
      {
        innerException = (Exception) ex;
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      if (innerException != null)
        throw this.NewXmlException("InvalidVersion", innerException);
      if (this._version.Major != 1)
        throw this.NewXmlException("UnexpectedVersion", (Exception) null, (object) this._version.Major);
    }

    private object ReadOneDeserializedObject(out string streamName)
    {
      if (this._reader.NodeType != XmlNodeType.Element)
        throw this.NewXmlException("InvalidNodeType", (Exception) null, (object) this._reader.NodeType.ToString(), (object) XmlNodeType.Element.ToString());
      InternalDeserializer._trace.WriteLine("Processing start node {0}", (object) this._reader.LocalName);
      streamName = this._reader.GetAttribute("S");
      if (this.IsNextElement("Nil"))
      {
        this.Skip();
        return (object) null;
      }
      if (this.IsNextElement("Ref"))
      {
        string attribute = this._reader.GetAttribute("RefId");
        object obj = attribute != null ? this.objectRefIdHandler.GetReferencedObject(attribute) : throw this.NewXmlException("AttributeExpected", (Exception) null, (object) "RefId");
        if (obj == null)
          throw this.NewXmlException("InvalidReferenceId", (Exception) null, (object) attribute);
        this.Skip();
        return obj;
      }
      if (this.IsNextElement("SS") && this._context.cryptoHelper != null)
      {
        InternalDeserializer._trace.WriteLine("Deserializing secure string", new object[0]);
        return this.ReadSecureString();
      }
      TypeSerializationInfo serializationInfoFromItemTag = KnownTypes.GetTypeSerializationInfoFromItemTag(this._reader.LocalName);
      if (serializationInfoFromItemTag != null)
      {
        InternalDeserializer._trace.WriteLine("Primitive Knowntype Element {0}", (object) serializationInfoFromItemTag.ItemTag);
        return this.ReadPrimaryKnownType(serializationInfoFromItemTag);
      }
      if (this.IsNextElement("Obj"))
      {
        InternalDeserializer._trace.WriteLine("PSObject Element", new object[0]);
        return (object) this.ReadPSObject();
      }
      InternalDeserializer._trace.TraceError("Invalid element {0} tag found", (object) this._reader.LocalName);
      throw this.NewXmlException("InvalidElementTag", (Exception) null, (object) this._reader.LocalName);
    }

    internal void Stop() => this.isStopping = true;

    private void CheckIfStopping()
    {
      if (this.isStopping)
        throw InternalDeserializer._trace.NewInvalidOperationException("Serialization", "Stopping");
    }

    internal object ReadOneObject(out string streamName)
    {
      try
      {
        this.CheckIfStopping();
        ++this.depthBelowTopLevel;
        if (this.depthBelowTopLevel == 50)
          throw this.NewXmlException("DeserializationTooDeep", (Exception) null);
        object valueToConvert = this.ReadOneDeserializedObject(out streamName);
        if (valueToConvert == null)
          return (object) null;
        PSObject psObject = PSObject.AsPSObject(valueToConvert);
        Type forDeserialization = psObject.GetTargetTypeForDeserialization(this._typeTable);
        if (forDeserialization != null)
        {
          Exception exception;
          try
          {
            object obj = LanguagePrimitives.ConvertTo(valueToConvert, forDeserialization, true, (IFormatProvider) CultureInfo.InvariantCulture, this._typeTable);
            InternalDeserializer.etwTracer.AnalyticChannel.WriteVerbose(PSEventId.Serializer_RehydrationSuccess, PSOpcode.Rehydration, PSTask.Serialization, (object) psObject.InternalTypeNames.Key, (object) forDeserialization.FullName, (object) obj.GetType().FullName);
            return obj;
          }
          catch (InvalidCastException ex)
          {
            exception = (Exception) ex;
          }
          catch (ArgumentException ex)
          {
            exception = (Exception) ex;
          }
          InternalDeserializer.etwTracer.AnalyticChannel.WriteError(PSEventId.Serializer_RehydrationFailure, PSOpcode.Rehydration, PSTask.Serialization, (object) psObject.InternalTypeNames.Key, (object) forDeserialization.FullName, (object) exception.ToString(), exception.InnerException == null ? (object) string.Empty : (object) exception.InnerException.ToString());
        }
        return valueToConvert;
      }
      finally
      {
        --this.depthBelowTopLevel;
      }
    }

    private object ReadOneObject() => this.ReadOneObject(out string _);

    private PSObject ReadPSObject()
    {
      PSObject psObject = this.ReadAttributeAndCreatePSObject();
      if (!this.ReadStartElementAndHandleEmpty("Obj"))
        return psObject;
      bool overrideTypeInfo = true;
      while (this._reader.NodeType == XmlNodeType.Element)
      {
        if (this.IsNextElement("TN") || this.IsNextElement("TNRef"))
        {
          this.ReadTypeNames(psObject);
          overrideTypeInfo = false;
        }
        else if (this.IsNextElement("Props"))
          this.ReadProperties(psObject);
        else if (this.IsNextElement("MS"))
          this.ReadMemberSet((PSMemberInfoCollection<PSMemberInfo>) psObject.InstanceMembers);
        else if (this.IsNextElement("ToString"))
        {
          psObject.ToStringFromDeserialization = this.ReadDecodedElementString("ToString");
          psObject.InstanceMembers.Add(PSObject.dotNetInstanceAdapter.GetDotNetMethod<PSMemberInfo>((object) psObject, "ToString"));
        }
        else
        {
          object obj = (object) null;
          ContainerType ct = ContainerType.None;
          TypeSerializationInfo serializationInfoFromItemTag = KnownTypes.GetTypeSerializationInfoFromItemTag(this._reader.LocalName);
          if (serializationInfoFromItemTag != null)
          {
            InternalDeserializer._trace.WriteLine("Primitive Knowntype Element {0}", (object) serializationInfoFromItemTag.ItemTag);
            obj = this.ReadPrimaryKnownType(serializationInfoFromItemTag);
          }
          else if (this.IsKnownContainerTag(out ct))
          {
            InternalDeserializer._trace.WriteLine("Found container node {0}", (object) ct);
            obj = this.ReadKnownContainer(ct);
          }
          else if (this.IsNextElement("Obj"))
          {
            InternalDeserializer._trace.WriteLine("Found PSObject node", new object[0]);
            obj = this.ReadOneObject();
          }
          else
          {
            InternalDeserializer._trace.WriteLine("Unknwon tag {0} encountered", (object) this._reader.LocalName);
            if (this.UnknownTagsAllowed)
              this.Skip();
            else
              throw this.NewXmlException("InvalidElementTag", (Exception) null, (object) this._reader.LocalName);
          }
          if (obj != null)
            psObject.SetCoreOnDeserialization(obj, overrideTypeInfo);
        }
      }
      this.ReadEndElement();
      return psObject;
    }

    private PSObject ReadAttributeAndCreatePSObject()
    {
      string attribute = this._reader.GetAttribute("RefId");
      PSObject psObject = new PSObject();
      if (attribute != null)
      {
        InternalDeserializer._trace.WriteLine("Read PSObject with refId: {0}", (object) attribute);
        this.objectRefIdHandler.SetRefId((object) psObject, attribute, this.DuplicateRefIdsAllowed);
      }
      return psObject;
    }

    private void ReadTypeNames(PSObject dso)
    {
      if (this.IsNextElement("TN"))
      {
        Collection<string> strings = new Collection<string>();
        string attribute = this._reader.GetAttribute("RefId");
        InternalDeserializer._trace.WriteLine("Processing TypeNamesTag with refId {0}", (object) attribute);
        if (this.ReadStartElementAndHandleEmpty("TN"))
        {
          while (this._reader.NodeType == XmlNodeType.Element)
          {
            if (this.IsNextElement("T"))
            {
              string type = this.ReadDecodedElementString("T");
              if (type != null && type.Length > 0)
              {
                Deserializer.AddDeserializationPrefix(ref type);
                strings.Add(type);
              }
            }
            else
              throw this.NewXmlException("InvalidElementTag", (Exception) null, (object) this._reader.LocalName);
          }
          this.ReadEndElement();
        }
        dso.InternalTypeNames = new ConsolidatedString(strings);
        if (attribute == null)
          return;
        this.typeRefIdHandler.SetRefId(dso.InternalTypeNames, attribute, this.DuplicateRefIdsAllowed);
      }
      else
      {
        if (!this.IsNextElement("TNRef"))
          return;
        string attribute = this._reader.GetAttribute("RefId");
        InternalDeserializer._trace.WriteLine("Processing TypeNamesReferenceTag with refId {0}", (object) attribute);
        ConsolidatedString other = attribute != null ? this.typeRefIdHandler.GetReferencedObject(attribute) : throw this.NewXmlException("AttributeExpected", (Exception) null, (object) "RefId");
        if (other == null)
          throw this.NewXmlException("InvalidTypeHierarchyReferenceId", (Exception) null, (object) attribute);
        this._context.LogExtraMemoryUsage(other.Key.Length * 2 - 29);
        dso.InternalTypeNames = new ConsolidatedString(other);
        this.Skip();
      }
    }

    private void ReadProperties(PSObject dso)
    {
      dso.isDeserialized = true;
      dso.adaptedMembers = new PSMemberInfoInternalCollection<PSPropertyInfo>();
      dso.clrMembers = new PSMemberInfoInternalCollection<PSPropertyInfo>();
      if (!this.ReadStartElementAndHandleEmpty("Props"))
        return;
      while (this._reader.NodeType == XmlNodeType.Element)
      {
        PSProperty psProperty = new PSProperty(this.ReadNameAttribute(), this.ReadOneObject());
        dso.adaptedMembers.Add((PSPropertyInfo) psProperty);
      }
      this.ReadEndElement();
    }

    private void ReadMemberSet(PSMemberInfoCollection<PSMemberInfo> collection)
    {
      if (!this.ReadStartElementAndHandleEmpty("MS"))
        return;
      while (this._reader.NodeType == XmlNodeType.Element)
      {
        if (this.IsNextElement("MS"))
        {
          PSMemberSet psMemberSet = new PSMemberSet(this.ReadNameAttribute());
          collection.Add((PSMemberInfo) psMemberSet);
          this.ReadMemberSet(psMemberSet.Members);
        }
        else
        {
          PSNoteProperty psNoteProperty = this.ReadNoteProperty();
          collection.Add((PSMemberInfo) psNoteProperty);
        }
      }
      this.ReadEndElement();
    }

    private PSNoteProperty ReadNoteProperty() => new PSNoteProperty(this.ReadNameAttribute(), this.ReadOneObject());

    private bool IsKnownContainerTag(out ContainerType ct)
    {
      ct = !this.IsNextElement("DCT") ? (!this.IsNextElement("QUE") ? (!this.IsNextElement("STK") ? (!this.IsNextElement("LST") ? (!this.IsNextElement("IE") ? ContainerType.None : ContainerType.Enumerable) : ContainerType.List) : ContainerType.Stack) : ContainerType.Queue) : ContainerType.Dictionary;
      return ct != ContainerType.None;
    }

    private object ReadKnownContainer(ContainerType ct)
    {
      switch (ct)
      {
        case ContainerType.Dictionary:
          return this.ReadDictionary(ct);
        case ContainerType.Queue:
        case ContainerType.Stack:
        case ContainerType.List:
        case ContainerType.Enumerable:
          return this.ReadListContainer(ct);
        default:
          return (object) null;
      }
    }

    private object ReadListContainer(ContainerType ct)
    {
      ArrayList arrayList = new ArrayList();
      if (this.ReadStartElementAndHandleEmpty(this._reader.LocalName))
      {
        while (this._reader.NodeType == XmlNodeType.Element)
          arrayList.Add(this.ReadOneObject());
        this.ReadEndElement();
      }
      if (ct == ContainerType.Stack)
      {
        arrayList.Reverse();
        return (object) new Stack((ICollection) arrayList);
      }
      return ct == ContainerType.Queue ? (object) new Queue((ICollection) arrayList) : (object) arrayList;
    }

    private object ReadDictionary(ContainerType ct)
    {
      Hashtable hashtable = new Hashtable();
      if (this.ReadStartElementAndHandleEmpty("DCT"))
      {
        while (this._reader.NodeType == XmlNodeType.Element)
        {
          this.ReadStartElement("En");
          if (this._reader.NodeType != XmlNodeType.Element)
            throw this.NewXmlException("DictionaryKeyNotSpecified", (Exception) null);
          if (string.Compare(this.ReadNameAttribute(), "Key", StringComparison.OrdinalIgnoreCase) != 0)
            throw this.NewXmlException("InvalidDictionaryKeyName", (Exception) null);
          object key = this.ReadOneObject();
          if (key == null)
            throw this.NewXmlException("NullAsDictionaryKey", (Exception) null);
          if (this._reader.NodeType != XmlNodeType.Element)
            throw this.NewXmlException("DictionaryValueNotSpecified", (Exception) null);
          if (string.Compare(this.ReadNameAttribute(), "Value", StringComparison.OrdinalIgnoreCase) != 0)
            throw this.NewXmlException("InvalidDictionaryValueName", (Exception) null);
          object obj = this.ReadOneObject();
          hashtable.Add(key, obj);
          this.ReadEndElement();
        }
        this.ReadEndElement();
      }
      return (object) hashtable;
    }

    internal static object DeserializeBoolean(InternalDeserializer deserializer)
    {
      try
      {
        return (object) XmlConvert.ToBoolean(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        throw deserializer.NewXmlException("InvalidPrimitiveType", (Exception) ex, (object) typeof (bool).FullName);
      }
    }

    internal static object DeserializeByte(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToByte(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (byte).FullName);
    }

    internal static object DeserializeChar(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) (char) XmlConvert.ToUInt16(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (char).FullName);
    }

    internal static object DeserializeDateTime(InternalDeserializer deserializer)
    {
      try
      {
        return (object) XmlConvert.ToDateTime(deserializer._reader.ReadElementString(), XmlDateTimeSerializationMode.RoundtripKind);
      }
      catch (FormatException ex)
      {
        throw deserializer.NewXmlException("InvalidPrimitiveType", (Exception) ex, (object) typeof (DateTime).FullName);
      }
    }

    internal static object DeserializeDecimal(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToDecimal(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (Decimal).FullName);
    }

    internal static object DeserializeDouble(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToDouble(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (double).FullName);
    }

    internal static object DeserializeGuid(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToGuid(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (Guid).FullName);
    }

    internal static object DeserializeVersion(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) new Version(deserializer._reader.ReadElementString());
      }
      catch (ArgumentException ex)
      {
        innerException = (Exception) ex;
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (Version).FullName);
    }

    internal static object DeserializeInt16(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToInt16(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (short).FullName);
    }

    internal static object DeserializeInt32(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToInt32(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (int).FullName);
    }

    internal static object DeserializeInt64(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToInt64(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (long).FullName);
    }

    internal static object DeserializeSByte(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToSByte(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (sbyte).FullName);
    }

    internal static object DeserializeSingle(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToSingle(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (float).FullName);
    }

    internal static object DeserializeScriptBlock(InternalDeserializer deserializer) => (object) deserializer.ReadDecodedElementString("SBK");

    internal static object DeserializeString(InternalDeserializer deserializer) => (object) deserializer.ReadDecodedElementString("S");

    internal static object DeserializeTimeSpan(InternalDeserializer deserializer)
    {
      try
      {
        return (object) XmlConvert.ToTimeSpan(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        throw deserializer.NewXmlException("InvalidPrimitiveType", (Exception) ex, (object) typeof (TimeSpan).FullName);
      }
    }

    internal static object DeserializeUInt16(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToUInt16(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (ushort).FullName);
    }

    internal static object DeserializeUInt32(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToUInt32(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (uint).FullName);
    }

    internal static object DeserializeUInt64(InternalDeserializer deserializer)
    {
      Exception innerException;
      try
      {
        return (object) XmlConvert.ToUInt64(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (ulong).FullName);
    }

    internal static object DeserializeUri(InternalDeserializer deserializer)
    {
      try
      {
        return (object) new Uri(deserializer.ReadDecodedElementString("URI"), UriKind.RelativeOrAbsolute);
      }
      catch (UriFormatException ex)
      {
        throw deserializer.NewXmlException("InvalidPrimitiveType", (Exception) ex, (object) typeof (Uri).FullName);
      }
    }

    internal static object DeserializeByteArray(InternalDeserializer deserializer)
    {
      try
      {
        return (object) Convert.FromBase64String(deserializer._reader.ReadElementString());
      }
      catch (FormatException ex)
      {
        throw deserializer.NewXmlException("InvalidPrimitiveType", (Exception) ex, (object) typeof (byte[]).FullName);
      }
    }

    internal static object DeserializeXmlDocument(InternalDeserializer deserializer)
    {
      string xml = deserializer.ReadDecodedElementString("XD");
      try
      {
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);
        return (object) xmlDocument;
      }
      catch (XmlException ex)
      {
        throw deserializer.NewXmlException("InvalidPrimitiveType", (Exception) ex, (object) typeof (XmlDocument).FullName);
      }
    }

    internal static object DeserializeProgressRecord(InternalDeserializer deserializer)
    {
      deserializer.ReadStartElement("PR");
      string activity = (string) null;
      string str1 = (string) null;
      string str2 = (string) null;
      string statusDescription = (string) null;
      int activityId = 0;
      int num1 = 0;
      int num2 = 0;
      int num3 = 0;
      Exception innerException = (Exception) null;
      try
      {
        activity = deserializer.ReadDecodedElementString("AV");
        activityId = int.Parse(deserializer.ReadDecodedElementString("AI"), (IFormatProvider) CultureInfo.InvariantCulture);
        object obj = deserializer.ReadOneObject();
        str1 = obj == null ? (string) null : obj.ToString();
        num1 = int.Parse(deserializer.ReadDecodedElementString("PI"), (IFormatProvider) CultureInfo.InvariantCulture);
        num2 = int.Parse(deserializer.ReadDecodedElementString("PC"), (IFormatProvider) CultureInfo.InvariantCulture);
        str2 = deserializer.ReadDecodedElementString("T");
        num3 = int.Parse(deserializer.ReadDecodedElementString("SR"), (IFormatProvider) CultureInfo.InvariantCulture);
        statusDescription = deserializer.ReadDecodedElementString("SD");
      }
      catch (FormatException ex)
      {
        innerException = (Exception) ex;
      }
      catch (OverflowException ex)
      {
        innerException = (Exception) ex;
      }
      if (innerException != null)
        throw deserializer.NewXmlException("InvalidPrimitiveType", innerException, (object) typeof (ulong).FullName);
      deserializer.ReadEndElement();
      ProgressRecordType progressRecordType;
      try
      {
        progressRecordType = (ProgressRecordType) Enum.Parse(typeof (ProgressRecordType), str2, true);
      }
      catch (ArgumentException ex)
      {
        throw deserializer.NewXmlException("InvalidPrimitiveType", (Exception) ex, (object) typeof (ProgressRecord).FullName);
      }
      ProgressRecord progressRecord = new ProgressRecord(activityId, activity, statusDescription);
      if (!string.IsNullOrEmpty(str1))
        progressRecord.CurrentOperation = str1;
      progressRecord.ParentActivityId = num1;
      progressRecord.PercentComplete = num2;
      progressRecord.RecordType = progressRecordType;
      progressRecord.SecondsRemaining = num3;
      return (object) progressRecord;
    }

    private bool IsNextElement(string tag)
    {
      if (!(this._reader.LocalName == tag))
        return false;
      return (this._context.options & DeserializationOptions.NoNamespace) != DeserializationOptions.None || this._reader.NamespaceURI == "http://schemas.microsoft.com/powershell/2004/04";
    }

    internal bool ReadStartElementAndHandleEmpty(string element)
    {
      bool flag = this._reader.IsEmptyElement;
      this.ReadStartElement(element);
      if (!flag && this._reader.NodeType == XmlNodeType.EndElement)
      {
        this.ReadEndElement();
        flag = true;
      }
      return !flag;
    }

    private void ReadStartElement(string element)
    {
      if (DeserializationOptions.NoNamespace == (this._context.options & DeserializationOptions.NoNamespace))
        this._reader.ReadStartElement(element);
      else
        this._reader.ReadStartElement(element, "http://schemas.microsoft.com/powershell/2004/04");
      int content = (int) this._reader.MoveToContent();
    }

    private void ReadEndElement()
    {
      this._reader.ReadEndElement();
      int content = (int) this._reader.MoveToContent();
    }

    private string ReadDecodedElementString(string element)
    {
      this.CheckIfStopping();
      string s = DeserializationOptions.NoNamespace != (this._context.options & DeserializationOptions.NoNamespace) ? this._reader.ReadElementString(element, "http://schemas.microsoft.com/powershell/2004/04") : this._reader.ReadElementString(element);
      int content = (int) this._reader.MoveToContent();
      return InternalDeserializer.DecodeString(s);
    }

    private void Skip()
    {
      this._reader.Skip();
      int content = (int) this._reader.MoveToContent();
    }

    private object ReadPrimaryKnownType(TypeSerializationInfo pktInfo)
    {
      object obj = pktInfo.Deserializer(this);
      int content = (int) this._reader.MoveToContent();
      return obj;
    }

    private object ReadSecureString()
    {
      string encryptedString = this._reader.ReadElementString();
      try
      {
        object obj = (object) this._context.cryptoHelper.DecryptSecureString(encryptedString);
        int content = (int) this._reader.MoveToContent();
        return obj;
      }
      catch (PSCryptoException ex)
      {
        throw this.NewXmlException("DeserializeSecureStringFailed", (Exception) null);
      }
    }

    private XmlException NewXmlException(
      string resourceId,
      Exception innerException,
      params object[] args)
    {
      string message = ResourceManagerCache.FormatResourceString("Serialization", resourceId, args);
      XmlException xmlException = (XmlException) null;
      if (this._reader is XmlTextReader reader && reader.HasLineInfo())
        xmlException = new XmlException(message, innerException, reader.LineNumber, reader.LinePosition);
      if (xmlException == null)
        xmlException = new XmlException(message, innerException);
      InternalDeserializer._trace.TraceException((Exception) xmlException);
      return xmlException;
    }

    private string ReadNameAttribute() => InternalDeserializer.DecodeString(this._reader.GetAttribute("N") ?? throw this.NewXmlException("AttributeExpected", (Exception) null, (object) "N"));

    private static string DecodeString(string s) => XmlConvert.DecodeName(s);
  }
}
