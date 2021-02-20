// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.RemoteHostEncoder
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Host;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;

namespace System.Management.Automation.Remoting
{
  internal class RemoteHostEncoder
  {
    private static bool IsKnownType(Type type) => KnownTypes.GetTypeSerializationInfo(type) != null;

    private static bool IsEncodingAllowedForClassOrStruct(Type type) => type == typeof (KeyInfo) || type == typeof (Coordinates) || (type == typeof (Size) || type == typeof (KeyInfo)) || (type == typeof (BufferCell) || type == typeof (Rectangle) || (type == typeof (ProgressRecord) || type == typeof (FieldDescription))) || (type == typeof (ChoiceDescription) || type == typeof (HostInfo) || type == typeof (HostDefaultData)) || type == typeof (RemoteSessionCapability);

    private static PSObject EncodeClassOrStruct(object obj)
    {
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
      {
        object obj1 = field.GetValue(obj);
        if (obj1 != null)
        {
          object obj2 = RemoteHostEncoder.EncodeObject(obj1);
          emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty(field.Name, obj2));
        }
      }
      return emptyPsObject;
    }

    private static object DecodeClassOrStruct(PSObject psObject, Type type)
    {
      object uninitializedObject = FormatterServices.GetUninitializedObject(type);
      foreach (PSPropertyInfo property in psObject.Properties)
      {
        FieldInfo field = type.GetField(property.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property.Value == null)
          throw RemoteHostExceptions.NewDecodingFailedException();
        field.SetValue(uninitializedObject, RemoteHostEncoder.DecodeObject(property.Value, field.FieldType) ?? throw RemoteHostExceptions.NewDecodingFailedException());
      }
      return uninitializedObject;
    }

    private static bool IsCollection(Type type) => type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof (Collection<>));

    private static bool IsGenericIEnumerableOfInt(Type type) => type.Equals(typeof (IEnumerable<int>));

    private static PSObject EncodeCollection(IList collection)
    {
      ArrayList arrayList = new ArrayList();
      foreach (object obj in (IEnumerable) collection)
        arrayList.Add(RemoteHostEncoder.EncodeObject(obj));
      return new PSObject((object) arrayList);
    }

    private static IList DecodeCollection(PSObject psObject, Type collectionType)
    {
      Type genericArgument = collectionType.GetGenericArguments()[0];
      ArrayList baseObject = RemoteHostEncoder.SafelyGetBaseObject<ArrayList>(psObject);
      IList instance = (IList) Activator.CreateInstance(collectionType);
      foreach (object obj in baseObject)
        instance.Add(RemoteHostEncoder.DecodeObject(obj, genericArgument));
      return instance;
    }

    private static bool IsDictionary(Type type) => type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof (Dictionary<,>));

    private static PSObject EncodeDictionary(IDictionary dictionary)
    {
      if (RemoteHostEncoder.IsObjectDictionaryType(dictionary.GetType()))
        return RemoteHostEncoder.EncodeObjectDictionary(dictionary);
      Hashtable hashtable = new Hashtable();
      foreach (object key in (IEnumerable) dictionary.Keys)
        hashtable.Add(RemoteHostEncoder.EncodeObject(key), RemoteHostEncoder.EncodeObject(dictionary[key]));
      return new PSObject((object) hashtable);
    }

    private static IDictionary DecodeDictionary(PSObject psObject, Type dictionaryType)
    {
      if (RemoteHostEncoder.IsObjectDictionaryType(dictionaryType))
        return RemoteHostEncoder.DecodeObjectDictionary(psObject, dictionaryType);
      Type[] genericArguments = dictionaryType.GetGenericArguments();
      Type type1 = genericArguments[0];
      Type type2 = genericArguments[1];
      Hashtable baseObject = RemoteHostEncoder.SafelyGetBaseObject<Hashtable>(psObject);
      IDictionary instance = (IDictionary) Activator.CreateInstance(dictionaryType);
      foreach (object key in (IEnumerable) baseObject.Keys)
        instance.Add(RemoteHostEncoder.DecodeObject(key, type1), RemoteHostEncoder.DecodeObject(baseObject[key], type2));
      return instance;
    }

    private static PSObject EncodePSObject(PSObject psObject) => psObject;

    private static PSObject DecodePSObject(object obj) => obj is PSObject ? (PSObject) obj : new PSObject(obj);

    private static PSObject EncodeException(Exception exception)
    {
      ErrorRecord errorRecord = exception is IContainsErrorRecord containsErrorRecord ? new ErrorRecord(containsErrorRecord.ErrorRecord, exception) : new ErrorRecord(exception, "RemoteHostExecutionException", ErrorCategory.NotSpecified, (object) null);
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      errorRecord.ToPSObjectForRemoting(emptyPsObject);
      return emptyPsObject;
    }

    private static Exception DecodeException(PSObject psObject) => (ErrorRecord.FromPSObjectForRemoting(psObject) ?? throw RemoteHostExceptions.NewDecodingErrorForErrorRecordException()).Exception;

    private static FieldDescription UpcastFieldDescriptionSubclassAndDropAttributes(
      FieldDescription fieldDescription1)
    {
      FieldDescription fieldDescription = new FieldDescription(fieldDescription1.Name);
      fieldDescription.Label = fieldDescription1.Label;
      fieldDescription.HelpMessage = fieldDescription1.HelpMessage;
      fieldDescription.IsMandatory = fieldDescription1.IsMandatory;
      fieldDescription.DefaultValue = fieldDescription1.DefaultValue;
      fieldDescription.SetParameterTypeName(fieldDescription1.ParameterTypeName);
      fieldDescription.SetParameterTypeFullName(fieldDescription1.ParameterTypeFullName);
      fieldDescription.SetParameterAssemblyFullName(fieldDescription1.ParameterAssemblyFullName);
      return fieldDescription;
    }

    internal static object EncodeObject(object obj)
    {
      if (obj == null)
        return (object) null;
      Type type = obj.GetType();
      if (obj is PSObject)
        return (object) RemoteHostEncoder.EncodePSObject((PSObject) obj);
      if (obj is ProgressRecord)
        return (object) ((ProgressRecord) obj).ToPSObjectForRemoting();
      if (RemoteHostEncoder.IsKnownType(type))
        return obj;
      if (type.IsEnum)
        return (object) (int) obj;
      if (obj is CultureInfo)
        return (object) obj.ToString();
      if (obj is Exception)
        return (object) RemoteHostEncoder.EncodeException((Exception) obj);
      if (type == typeof (object[]))
        return (object) RemoteHostEncoder.EncodeObjectArray((object[]) obj);
      if (type.IsArray)
        return (object) RemoteHostEncoder.EncodeArray((Array) obj);
      if (obj is IList && RemoteHostEncoder.IsCollection(type))
        return (object) RemoteHostEncoder.EncodeCollection((IList) obj);
      if (obj is IDictionary && RemoteHostEncoder.IsDictionary(type))
        return (object) RemoteHostEncoder.EncodeDictionary((IDictionary) obj);
      if (type.IsSubclassOf(typeof (FieldDescription)) || type == typeof (FieldDescription))
        return (object) RemoteHostEncoder.EncodeClassOrStruct((object) RemoteHostEncoder.UpcastFieldDescriptionSubclassAndDropAttributes((FieldDescription) obj));
      if (RemoteHostEncoder.IsEncodingAllowedForClassOrStruct(type))
        return (object) RemoteHostEncoder.EncodeClassOrStruct(obj);
      switch (obj)
      {
        case RemoteHostCall _:
          return (object) ((RemoteHostCall) obj).Encode();
        case RemoteHostResponse _:
          return (object) ((RemoteHostResponse) obj).Encode();
        case SecureString _:
          return obj;
        case PSCredential _:
          return obj;
        default:
          if (RemoteHostEncoder.IsGenericIEnumerableOfInt(type))
            return (object) RemoteHostEncoder.EncodeCollection((IList) obj);
          throw RemoteHostExceptions.NewRemoteHostDataEncodingNotSupportedException(type);
      }
    }

    internal static object DecodeObject(object obj, Type type)
    {
      if (obj == null)
        return obj;
      if (type == typeof (PSObject))
        return (object) RemoteHostEncoder.DecodePSObject(obj);
      if (type == typeof (ProgressRecord))
        return (object) ProgressRecord.FromPSObjectForRemoting(PSObject.AsPSObject(obj));
      if (RemoteHostEncoder.IsKnownType(type) || obj is SecureString || obj is PSCredential)
        return obj;
      switch (obj)
      {
        case PSObject _ when type == typeof (PSCredential):
          PSObject psObject = (PSObject) obj;
          try
          {
            return (object) new PSCredential((string) psObject.Properties["UserName"].Value, (SecureString) psObject.Properties["Password"].Value);
          }
          catch (GetValueException ex)
          {
            return (object) (PSCredential) null;
          }
        case int _ when type.IsEnum:
          return Enum.ToObject(type, (int) obj);
        case string _ when type == typeof (CultureInfo):
          return (object) new CultureInfo((string) obj);
        case PSObject _ when type == typeof (Exception):
          return (object) RemoteHostEncoder.DecodeException((PSObject) obj);
        case PSObject _ when type == typeof (object[]):
          return (object) RemoteHostEncoder.DecodeObjectArray((PSObject) obj);
        case PSObject _ when type.IsArray:
          return (object) RemoteHostEncoder.DecodeArray((PSObject) obj, type);
        case PSObject _ when RemoteHostEncoder.IsCollection(type):
          return (object) RemoteHostEncoder.DecodeCollection((PSObject) obj, type);
        case PSObject _ when RemoteHostEncoder.IsDictionary(type):
          return (object) RemoteHostEncoder.DecodeDictionary((PSObject) obj, type);
        case PSObject _ when RemoteHostEncoder.IsEncodingAllowedForClassOrStruct(type):
          return RemoteHostEncoder.DecodeClassOrStruct((PSObject) obj, type);
        case PSObject _ when RemoteHostEncoder.IsGenericIEnumerableOfInt(type):
          return (object) RemoteHostEncoder.DecodeCollection((PSObject) obj, typeof (Collection<int>));
        case PSObject _ when type == typeof (RemoteHostCall):
          return (object) RemoteHostCall.Decode((PSObject) obj);
        case PSObject _ when type == typeof (RemoteHostResponse):
          return (object) RemoteHostResponse.Decode((PSObject) obj);
        default:
          throw RemoteHostExceptions.NewRemoteHostDataDecodingNotSupportedException(type);
      }
    }

    internal static void EncodeAndAddAsProperty(
      PSObject psObject,
      string propertyName,
      object propertyValue)
    {
      if (propertyValue == null)
        return;
      psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty(propertyName, RemoteHostEncoder.EncodeObject(propertyValue)));
    }

    internal static object DecodePropertyValue(
      PSObject psObject,
      string propertyName,
      Type propertyValueType)
    {
      ReadOnlyPSMemberInfoCollection<PSPropertyInfo> memberInfoCollection = psObject.Properties.Match(propertyName);
      return memberInfoCollection.Count == 0 ? (object) null : RemoteHostEncoder.DecodeObject(memberInfoCollection[0].Value, propertyValueType);
    }

    private static PSObject EncodeObjectArray(object[] objects)
    {
      ArrayList arrayList = new ArrayList();
      foreach (object obj in objects)
        arrayList.Add((object) RemoteHostEncoder.EncodeObjectWithType(obj));
      return new PSObject((object) arrayList);
    }

    private static object[] DecodeObjectArray(PSObject psObject)
    {
      ArrayList baseObject = RemoteHostEncoder.SafelyGetBaseObject<ArrayList>(psObject);
      object[] objArray = new object[baseObject.Count];
      for (int index = 0; index < baseObject.Count; ++index)
        objArray[index] = RemoteHostEncoder.DecodeObjectWithType(baseObject[index]);
      return objArray;
    }

    private static PSObject EncodeObjectWithType(object obj)
    {
      if (obj == null)
        return (PSObject) null;
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("T", (object) obj.GetType().ToString()));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("V", RemoteHostEncoder.EncodeObject(obj)));
      return emptyPsObject;
    }

    private static object DecodeObjectWithType(object obj)
    {
      if (obj == null)
        return (object) null;
      PSObject psObject = RemoteHostEncoder.SafelyCastObject<PSObject>(obj);
      Type type = Type.GetType(RemoteHostEncoder.SafelyGetPropertyValue<string>(psObject, "T"));
      return RemoteHostEncoder.DecodeObject(RemoteHostEncoder.SafelyGetPropertyValue<object>(psObject, "V"), type);
    }

    private static bool ArrayIsZeroBased(Array array)
    {
      int rank = array.Rank;
      for (int dimension = 0; dimension < rank; ++dimension)
      {
        if (array.GetLowerBound(dimension) != 0)
          return false;
      }
      return true;
    }

    private static PSObject EncodeArray(Array array)
    {
      array.GetType().GetElementType();
      int rank = array.Rank;
      int[] lengths = new int[rank];
      for (int dimension = 0; dimension < rank; ++dimension)
        lengths[dimension] = array.GetUpperBound(dimension) + 1;
      Indexer indexer = new Indexer(lengths);
      ArrayList arrayList = new ArrayList();
      foreach (int[] numArray in indexer)
      {
        object obj = array.GetValue(numArray);
        arrayList.Add(RemoteHostEncoder.EncodeObject(obj));
      }
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("mae", (object) arrayList));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("mal", (object) lengths));
      return emptyPsObject;
    }

    private static Array DecodeArray(PSObject psObject, Type type)
    {
      Type elementType = type.GetElementType();
      ArrayList baseObject = RemoteHostEncoder.SafelyGetBaseObject<ArrayList>(RemoteHostEncoder.SafelyGetPropertyValue<PSObject>(psObject, "mae"));
      int[] array = (int[]) RemoteHostEncoder.SafelyGetBaseObject<ArrayList>(RemoteHostEncoder.SafelyGetPropertyValue<PSObject>(psObject, "mal")).ToArray(typeof (int));
      Indexer indexer = new Indexer(array);
      Array instance = Array.CreateInstance(elementType, array);
      int num = 0;
      foreach (int[] numArray in indexer)
      {
        object obj = RemoteHostEncoder.DecodeObject(baseObject[num++], elementType);
        instance.SetValue(obj, numArray);
      }
      return instance;
    }

    private static bool IsObjectDictionaryType(Type dictionaryType)
    {
      if (!RemoteHostEncoder.IsDictionary(dictionaryType))
        return false;
      Type[] genericArguments = dictionaryType.GetGenericArguments();
      return genericArguments.Length == 2 && genericArguments[1] == typeof (object);
    }

    private static PSObject EncodeObjectDictionary(IDictionary dictionary)
    {
      Hashtable hashtable = new Hashtable();
      foreach (object key in (IEnumerable) dictionary.Keys)
        hashtable.Add(RemoteHostEncoder.EncodeObject(key), (object) RemoteHostEncoder.EncodeObjectWithType(dictionary[key]));
      return new PSObject((object) hashtable);
    }

    private static IDictionary DecodeObjectDictionary(
      PSObject psObject,
      Type dictionaryType)
    {
      Type[] genericArguments = dictionaryType.GetGenericArguments();
      Type type1 = genericArguments[0];
      Type type2 = genericArguments[1];
      Hashtable baseObject = RemoteHostEncoder.SafelyGetBaseObject<Hashtable>(psObject);
      IDictionary instance = (IDictionary) Activator.CreateInstance(dictionaryType);
      foreach (object key in (IEnumerable) baseObject.Keys)
        instance.Add(RemoteHostEncoder.DecodeObject(key, type1), RemoteHostEncoder.DecodeObjectWithType(baseObject[key]));
      return instance;
    }

    private static T SafelyGetBaseObject<T>(PSObject psObject) => psObject != null && psObject.BaseObject != null && psObject.BaseObject is T ? (T) psObject.BaseObject : throw RemoteHostExceptions.NewDecodingFailedException();

    private static T SafelyCastObject<T>(object obj) => obj is T obj1 ? obj1 : throw RemoteHostExceptions.NewDecodingFailedException();

    private static T SafelyGetPropertyValue<T>(PSObject psObject, string key)
    {
      PSPropertyInfo property = psObject.Properties[key];
      return property != null && property.Value != null && property.Value is T ? (T) property.Value : throw RemoteHostExceptions.NewDecodingFailedException();
    }
  }
}
