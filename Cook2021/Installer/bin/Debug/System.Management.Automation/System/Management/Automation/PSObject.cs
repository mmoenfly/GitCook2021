// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSObject
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Xml;

namespace System.Management.Automation
{
  [TypeDescriptionProvider(typeof (PSObjectTypeDescriptionProvider))]
  public class PSObject : IFormattable, IComparable
  {
    public const string AdaptedMemberSetName = "psadapted";
    public const string ExtendedMemberSetName = "psextended";
    public const string BaseObjectMemberSetName = "psbase";
    internal const string PSObjectMemberSetName = "psobject";
    internal const string MshTypeNames = "pstypenames";
    private static Collection<CollectionEntry<PSMemberInfo>> memberCollection = PSObject.GetMemberCollection(PSMemberViewTypes.All);
    private static Collection<CollectionEntry<PSMethodInfo>> methodCollection = PSObject.GetMethodCollection();
    private static Collection<CollectionEntry<PSPropertyInfo>> propertyCollection = PSObject.GetPropertyCollection(PSMemberViewTypes.All);
    internal static readonly DotNetAdapter dotNetInstanceAdapter = new DotNetAdapter();
    private static readonly DotNetAdapter baseAdapterForAdaptedObjects = (DotNetAdapter) new BaseDotNetAdapterForAdaptedObjects();
    internal static readonly DotNetAdapter dotNetStaticAdapter = new DotNetAdapter(true);
    private static readonly PSObject.AdapterSet dotNetInstanceAdapterSet = new PSObject.AdapterSet((Adapter) PSObject.dotNetInstanceAdapter, (DotNetAdapter) null);
    private static readonly PSObject.AdapterSet managementObjectAdapter = new PSObject.AdapterSet((Adapter) new ManagementObjectAdapter(), PSObject.dotNetInstanceAdapter);
    private static readonly PSObject.AdapterSet managementClassAdapter = new PSObject.AdapterSet((Adapter) new ManagementClassApdapter(), PSObject.dotNetInstanceAdapter);
    private static readonly PSObject.AdapterSet directoryEntryAdapter = new PSObject.AdapterSet((Adapter) new DirectoryEntryAdapter(), PSObject.dotNetInstanceAdapter);
    private static readonly PSObject.AdapterSet dataRowViewAdapter = new PSObject.AdapterSet((Adapter) new DataRowViewAdapter(), PSObject.baseAdapterForAdaptedObjects);
    private static readonly PSObject.AdapterSet dataRowAdapter = new PSObject.AdapterSet((Adapter) new DataRowAdapter(), PSObject.baseAdapterForAdaptedObjects);
    private static readonly PSObject.AdapterSet xmlNodeAdapter = new PSObject.AdapterSet((Adapter) new XmlNodeAdapter(), PSObject.baseAdapterForAdaptedObjects);
    private static readonly PSObject.AdapterSet mshMemberSetAdapter = new PSObject.AdapterSet((Adapter) new PSMemberSetAdapter(), (DotNetAdapter) null);
    private static readonly PSObject.AdapterSet mshObjectAdapter = new PSObject.AdapterSet((Adapter) new PSObjectAdapter(), (DotNetAdapter) null);
    private object lockObject = new object();
    private ExecutionContext _context;
    internal string TokenText;
    private object immediateBaseObject;
    private PSObject.AdapterSet adapterSet;
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    internal bool hasGeneratedReservedMembers;
    internal PSMemberInfoInternalCollection<PSMemberInfo> _instanceMembers;
    internal PSMemberInfoInternalCollection<PSPropertyInfo> adaptedMembers;
    internal PSMemberInfoInternalCollection<PSPropertyInfo> clrMembers;
    internal bool immediateBaseObjectIsEmpty;
    internal static PSTraceSource memberResolution = PSTraceSource.GetTracer("MemberResolution", "Traces the resolution from member name to the member. A member can be a property, method, etc.", false);
    private PSMemberInfoIntegratingCollection<PSMemberInfo> _members;
    private PSMemberInfoIntegratingCollection<PSPropertyInfo> _properties;
    private PSMemberInfoIntegratingCollection<PSMethodInfo> _methods;
    internal ConsolidatedString _typeNames;
    internal bool isDeserialized;
    private string toStringFromDeserialization;
    internal bool preserveToString;
    internal bool preserveToStringSet;

    internal TypeTable GetTypeTable()
    {
      ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
      if (executionContextFromTls != null && executionContextFromTls != this.Context)
      {
        PSObject.tracer.WriteLine("resetting PSObject context", new object[0]);
        this.Context = executionContextFromTls;
      }
      if (this.Context != null)
        return this.Context.TypeTable;
      PSObject.tracer.WriteLine("no context", new object[0]);
      return (TypeTable) null;
    }

    internal static T TypeTableGetMemberDelegate<T>(PSObject msjObj, string name) where T : PSMemberInfo
    {
      TypeTable typeTable = msjObj.GetTypeTable();
      return PSObject.TypeTableGetMemberDelegate<T>(msjObj, typeTable, name);
    }

    private static T TypeTableGetMemberDelegate<T>(
      PSObject msjObj,
      TypeTable typeTableToUse,
      string name)
      where T : PSMemberInfo
    {
      if (typeTableToUse == null)
        return default (T);
      PSMemberInfo member = typeTableToUse.GetMembers<PSMemberInfo>(msjObj.InternalTypeNames)[name];
      if (member == null)
      {
        PSObject.memberResolution.WriteLine("\"{0}\" NOT present in type table.", (object) name);
        return default (T);
      }
      if (member is T obj)
      {
        PSObject.memberResolution.WriteLine("\"{0}\" present in type table.", (object) name);
        return obj;
      }
      PSObject.memberResolution.WriteLine("\"{0}\" from types table ignored because it has type {1} instead of {2}.", (object) name, (object) member.GetType(), (object) typeof (T));
      return default (T);
    }

    internal static PSMemberInfoInternalCollection<T> TypeTableGetMembersDelegate<T>(
      PSObject msjObj)
      where T : PSMemberInfo
    {
      TypeTable typeTable = msjObj.GetTypeTable();
      return PSObject.TypeTableGetMembersDelegate<T>(msjObj, typeTable);
    }

    internal static PSMemberInfoInternalCollection<T> TypeTableGetMembersDelegate<T>(
      PSObject msjObj,
      TypeTable typeTableToUse)
      where T : PSMemberInfo
    {
      if (typeTableToUse == null)
        return new PSMemberInfoInternalCollection<T>();
      PSMemberInfoInternalCollection<T> members = typeTableToUse.GetMembers<T>(msjObj.InternalTypeNames);
      PSObject.memberResolution.WriteLine("Type table members: {0}.", (object) members.Count);
      return members;
    }

    private static T AdapterGetMemberDelegate<T>(PSObject msjObj, string name) where T : PSMemberInfo
    {
      if (msjObj.isDeserialized)
      {
        if (msjObj.adaptedMembers == null)
        {
          PSObject.tracer.WriteLine("This is a serialized object, but no adapted members have been found.", new object[0]);
          return default (T);
        }
        T adaptedMember = msjObj.adaptedMembers[name] as T;
        PSObject.memberResolution.WriteLine("Serialized adapted member: {0}.", (object) adaptedMember == null ? (object) "not found" : (object) adaptedMember.Name);
        return adaptedMember;
      }
      T member = msjObj.InternalAdapter.BaseGetMember<T>(msjObj.immediateBaseObject, name);
      PSObject.memberResolution.WriteLine("Adapted member: {0}.", (object) member == null ? (object) "not found" : (object) member.Name);
      return member;
    }

    internal static PSMemberInfoInternalCollection<U> TransformMemberInfoCollection<T, U>(
      PSMemberInfoCollection<T> source)
      where T : PSMemberInfo
      where U : PSMemberInfo
    {
      PSMemberInfoInternalCollection<U> internalCollection = new PSMemberInfoInternalCollection<U>();
      foreach (T obj in source)
      {
        if (obj is U member)
          internalCollection.Add(member);
      }
      return internalCollection;
    }

    private static PSMemberInfoInternalCollection<T> AdapterGetMembersDelegate<T>(
      PSObject msjObj)
      where T : PSMemberInfo
    {
      if (msjObj.isDeserialized)
      {
        if (msjObj.adaptedMembers == null)
        {
          PSObject.tracer.WriteLine("This is a serialized object, but no adapted members have been found.", new object[0]);
          return new PSMemberInfoInternalCollection<T>();
        }
        PSObject.memberResolution.WriteLine("Serialized adapted members: {0}.", (object) msjObj.adaptedMembers.Count);
        return PSObject.TransformMemberInfoCollection<PSPropertyInfo, T>((PSMemberInfoCollection<PSPropertyInfo>) msjObj.adaptedMembers);
      }
      PSMemberInfoInternalCollection<T> members = msjObj.InternalAdapter.BaseGetMembers<T>(msjObj.immediateBaseObject);
      PSObject.memberResolution.WriteLine("Adapted members: {0}.", (object) (members.Count - members.countHidden));
      return members;
    }

    private static PSMemberInfoInternalCollection<T> DotNetGetMembersDelegate<T>(
      PSObject msjObj)
      where T : PSMemberInfo
    {
      if (msjObj.InternalAdapterSet.DotNetAdapter == null)
        return new PSMemberInfoInternalCollection<T>();
      PSMemberInfoInternalCollection<T> members = msjObj.InternalAdapterSet.DotNetAdapter.BaseGetMembers<T>(msjObj.immediateBaseObject);
      PSObject.memberResolution.WriteLine("DotNet members: {0}.", (object) (members.Count - members.countHidden));
      return members;
    }

    private static T DotNetGetMemberDelegate<T>(PSObject msjObj, string name) where T : PSMemberInfo
    {
      if (msjObj.InternalAdapterSet.DotNetAdapter == null)
        return default (T);
      T member = msjObj.InternalAdapterSet.DotNetAdapter.BaseGetMember<T>(msjObj.immediateBaseObject, name);
      PSObject.memberResolution.WriteLine("DotNet member: {0}.", (object) member == null ? (object) "not found" : (object) member.Name);
      return member;
    }

    internal static Collection<CollectionEntry<PSMemberInfo>> GetMemberCollection(
      PSMemberViewTypes viewType)
    {
      return PSObject.GetMemberCollection(viewType, (TypeTable) null);
    }

    internal static Collection<CollectionEntry<PSMemberInfo>> GetMemberCollection(
      PSMemberViewTypes viewType,
      TypeTable backupTypeTable)
    {
      Collection<CollectionEntry<PSMemberInfo>> collection = new Collection<CollectionEntry<PSMemberInfo>>();
      if ((viewType & PSMemberViewTypes.Extended) == PSMemberViewTypes.Extended)
      {
        if (backupTypeTable == null)
          collection.Add(new CollectionEntry<PSMemberInfo>(new CollectionEntry<PSMemberInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSMemberInfo>), new CollectionEntry<PSMemberInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSMemberInfo>), true, true, "type table members"));
        else
          collection.Add(new CollectionEntry<PSMemberInfo>((CollectionEntry<PSMemberInfo>.GetMembersDelegate) (msjObj => PSObject.TypeTableGetMembersDelegate<PSMemberInfo>(msjObj, backupTypeTable)), (CollectionEntry<PSMemberInfo>.GetMemberDelegate) ((msjObj, name) => PSObject.TypeTableGetMemberDelegate<PSMemberInfo>(msjObj, backupTypeTable, name)), true, true, "type table members"));
      }
      if ((viewType & PSMemberViewTypes.Adapted) == PSMemberViewTypes.Adapted)
        collection.Add(new CollectionEntry<PSMemberInfo>(new CollectionEntry<PSMemberInfo>.GetMembersDelegate(PSObject.AdapterGetMembersDelegate<PSMemberInfo>), new CollectionEntry<PSMemberInfo>.GetMemberDelegate(PSObject.AdapterGetMemberDelegate<PSMemberInfo>), false, false, "adapted members"));
      if ((viewType & PSMemberViewTypes.Base) == PSMemberViewTypes.Base)
        collection.Add(new CollectionEntry<PSMemberInfo>(new CollectionEntry<PSMemberInfo>.GetMembersDelegate(PSObject.DotNetGetMembersDelegate<PSMemberInfo>), new CollectionEntry<PSMemberInfo>.GetMemberDelegate(PSObject.DotNetGetMemberDelegate<PSMemberInfo>), false, false, "clr members"));
      return collection;
    }

    private static Collection<CollectionEntry<PSMethodInfo>> GetMethodCollection() => new Collection<CollectionEntry<PSMethodInfo>>()
    {
      new CollectionEntry<PSMethodInfo>(new CollectionEntry<PSMethodInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSMethodInfo>), new CollectionEntry<PSMethodInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSMethodInfo>), true, true, "type table members"),
      new CollectionEntry<PSMethodInfo>(new CollectionEntry<PSMethodInfo>.GetMembersDelegate(PSObject.AdapterGetMembersDelegate<PSMethodInfo>), new CollectionEntry<PSMethodInfo>.GetMemberDelegate(PSObject.AdapterGetMemberDelegate<PSMethodInfo>), false, false, "adapted members"),
      new CollectionEntry<PSMethodInfo>(new CollectionEntry<PSMethodInfo>.GetMembersDelegate(PSObject.DotNetGetMembersDelegate<PSMethodInfo>), new CollectionEntry<PSMethodInfo>.GetMemberDelegate(PSObject.DotNetGetMemberDelegate<PSMethodInfo>), false, false, "clr members")
    };

    internal static Collection<CollectionEntry<PSPropertyInfo>> GetPropertyCollection(
      PSMemberViewTypes viewType)
    {
      return PSObject.GetPropertyCollection(viewType, (TypeTable) null);
    }

    internal static Collection<CollectionEntry<PSPropertyInfo>> GetPropertyCollection(
      PSMemberViewTypes viewType,
      TypeTable backupTypeTable)
    {
      Collection<CollectionEntry<PSPropertyInfo>> collection = new Collection<CollectionEntry<PSPropertyInfo>>();
      if ((viewType & PSMemberViewTypes.Extended) == PSMemberViewTypes.Extended)
      {
        if (backupTypeTable == null)
          collection.Add(new CollectionEntry<PSPropertyInfo>(new CollectionEntry<PSPropertyInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSPropertyInfo>), new CollectionEntry<PSPropertyInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSPropertyInfo>), true, true, "type table members"));
        else
          collection.Add(new CollectionEntry<PSPropertyInfo>((CollectionEntry<PSPropertyInfo>.GetMembersDelegate) (msjObj => PSObject.TypeTableGetMembersDelegate<PSPropertyInfo>(msjObj, backupTypeTable)), (CollectionEntry<PSPropertyInfo>.GetMemberDelegate) ((msjObj, name) => PSObject.TypeTableGetMemberDelegate<PSPropertyInfo>(msjObj, backupTypeTable, name)), true, true, "type table members"));
      }
      if ((viewType & PSMemberViewTypes.Adapted) == PSMemberViewTypes.Adapted)
        collection.Add(new CollectionEntry<PSPropertyInfo>(new CollectionEntry<PSPropertyInfo>.GetMembersDelegate(PSObject.AdapterGetMembersDelegate<PSPropertyInfo>), new CollectionEntry<PSPropertyInfo>.GetMemberDelegate(PSObject.AdapterGetMemberDelegate<PSPropertyInfo>), false, false, "adapted members"));
      if ((viewType & PSMemberViewTypes.Base) == PSMemberViewTypes.Base)
        collection.Add(new CollectionEntry<PSPropertyInfo>(new CollectionEntry<PSPropertyInfo>.GetMembersDelegate(PSObject.DotNetGetMembersDelegate<PSPropertyInfo>), new CollectionEntry<PSPropertyInfo>.GetMemberDelegate(PSObject.DotNetGetMemberDelegate<PSPropertyInfo>), false, false, "clr members"));
      return collection;
    }

    private void CommonInitialization(object obj)
    {
      if (obj is PSCustomObject)
        this.immediateBaseObjectIsEmpty = true;
      this.Context = LocalPipeline.GetExecutionContextFromTLS();
      this.immediateBaseObject = obj;
    }

    private void Refresh(object obj)
    {
      if (obj is PSCustomObject)
        this.immediateBaseObjectIsEmpty = true;
      this.InstanceMembers = new PSMemberInfoInternalCollection<PSMemberInfo>();
      this._members = new PSMemberInfoIntegratingCollection<PSMemberInfo>((object) this, PSObject.memberCollection);
      this._properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>((object) this, PSObject.propertyCollection);
      this._methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>((object) this, PSObject.methodCollection);
      this.Context = LocalPipeline.GetExecutionContextFromTLS();
      this.immediateBaseObject = obj;
      this.adapterSet = this.GetMappedAdapter();
      this.InternalTypeNames = new ConsolidatedString(this.InternalAdapter.BaseGetTypeNameHierarchy(obj));
    }

    private PSObject.AdapterSet GetMappedAdapter()
    {
      object immediateBaseObject = this.immediateBaseObject;
      Type type = immediateBaseObject.GetType();
      TypeTable typeTable = this.GetTypeTable();
      if (typeTable != null)
      {
        PSObject.AdapterSet typeAdapter = typeTable.GetTypeAdapter(type);
        if (typeAdapter != null)
          return typeAdapter;
      }
      if (typeof (ManagementClass).IsAssignableFrom(type))
        return PSObject.managementClassAdapter;
      if (typeof (ManagementBaseObject).IsAssignableFrom(type))
        return PSObject.managementObjectAdapter;
      if (typeof (DirectoryEntry).IsAssignableFrom(type))
        return PSObject.directoryEntryAdapter;
      if (typeof (DataRowView).IsAssignableFrom(type))
        return PSObject.dataRowViewAdapter;
      if (typeof (DataRow).IsAssignableFrom(type))
        return PSObject.dataRowAdapter;
      if (typeof (XmlNode).IsAssignableFrom(type))
        return PSObject.xmlNodeAdapter;
      if (typeof (PSMemberSet).IsAssignableFrom(type))
        return PSObject.mshMemberSetAdapter;
      if (typeof (PSObject).IsAssignableFrom(type))
        return PSObject.mshObjectAdapter;
      if (!type.IsCOMObject)
        return PSObject.dotNetInstanceAdapterSet;
      if (type.FullName.Equals("System.__ComObject"))
      {
        ComTypeInfo dispatchTypeInfo = ComTypeInfo.GetDispatchTypeInfo(immediateBaseObject);
        return dispatchTypeInfo != null ? new PSObject.AdapterSet((Adapter) new ComAdapter(immediateBaseObject, dispatchTypeInfo), PSObject.dotNetInstanceAdapter) : PSObject.dotNetInstanceAdapterSet;
      }
      ComTypeInfo dispatchTypeInfo1 = ComTypeInfo.GetDispatchTypeInfo(immediateBaseObject);
      return dispatchTypeInfo1 != null ? new PSObject.AdapterSet((Adapter) new DotNetAdapterWithComTypeName(dispatchTypeInfo1), (DotNetAdapter) null) : PSObject.dotNetInstanceAdapterSet;
    }

    internal static PSObject.AdapterSet CreateThirdPartyAdapterSet(
      Type adaptedType,
      PSPropertyAdapter adapter)
    {
      return new PSObject.AdapterSet((Adapter) new ThirdPartyAdapter(adaptedType, adapter), PSObject.baseAdapterForAdaptedObjects);
    }

    public PSObject() => this.CommonInitialization((object) PSCustomObject.SelfInstance);

    public PSObject(object obj)
    {
      if (obj == null)
        throw PSObject.tracer.NewArgumentNullException(nameof (obj));
      using (PSObject.tracer.TraceMethod("object = {0}", obj))
        this.CommonInitialization(obj);
    }

    internal ExecutionContext Context
    {
      get
      {
        if (this._context == null)
        {
          lock (this.lockObject)
          {
            if (this._context == null)
              this._context = LocalPipeline.GetExecutionContextFromTLS();
          }
        }
        return this._context;
      }
      set => this._context = value;
    }

    internal Adapter InternalAdapter
    {
      get => this.InternalAdapterSet.OriginalAdapter;
      set => this.InternalAdapterSet.OriginalAdapter = value;
    }

    internal Adapter InternalBaseDotNetAdapter => (Adapter) this.InternalAdapterSet.DotNetAdapter;

    private PSObject.AdapterSet InternalAdapterSet
    {
      get
      {
        if (this.adapterSet == null)
        {
          lock (this.lockObject)
          {
            if (this.adapterSet == null)
              this.adapterSet = this.GetMappedAdapter();
          }
        }
        return this.adapterSet;
      }
    }

    internal PSMemberInfoInternalCollection<PSMemberInfo> InstanceMembers
    {
      get
      {
        if (this._instanceMembers == null)
        {
          lock (this.lockObject)
          {
            if (this._instanceMembers == null)
              this._instanceMembers = new PSMemberInfoInternalCollection<PSMemberInfo>();
          }
        }
        return this._instanceMembers;
      }
      set => this._instanceMembers = value;
    }

    public PSMemberInfoCollection<PSMemberInfo> Members
    {
      get
      {
        if (this._members == null)
        {
          lock (this.lockObject)
          {
            if (this._members == null)
              this._members = new PSMemberInfoIntegratingCollection<PSMemberInfo>((object) this, PSObject.memberCollection);
          }
        }
        return (PSMemberInfoCollection<PSMemberInfo>) this._members;
      }
    }

    public PSMemberInfoCollection<PSPropertyInfo> Properties
    {
      get
      {
        if (this._properties == null)
        {
          lock (this.lockObject)
          {
            if (this._properties == null)
              this._properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>((object) this, PSObject.propertyCollection);
          }
        }
        return (PSMemberInfoCollection<PSPropertyInfo>) this._properties;
      }
    }

    public PSMemberInfoCollection<PSMethodInfo> Methods
    {
      get
      {
        if (this._methods == null)
        {
          lock (this.lockObject)
          {
            if (this._methods == null)
              this._methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>((object) this, PSObject.methodCollection);
          }
        }
        return (PSMemberInfoCollection<PSMethodInfo>) this._methods;
      }
    }

    public object ImmediateBaseObject => this.immediateBaseObject;

    public object BaseObject
    {
      get
      {
        psObject = this;
        object immediateBaseObject;
        do
        {
          immediateBaseObject = psObject.immediateBaseObject;
        }
        while (immediateBaseObject is PSObject psObject);
        return immediateBaseObject;
      }
    }

    public Collection<string> TypeNames => (Collection<string>) this.InternalTypeNames;

    internal ConsolidatedString InternalTypeNames
    {
      get
      {
        if (this._typeNames == null)
        {
          lock (this.lockObject)
          {
            if (this._typeNames == null)
              this._typeNames = new ConsolidatedString(this.InternalAdapter.BaseGetTypeNameHierarchy(this.immediateBaseObject));
          }
        }
        return this._typeNames;
      }
      set => this._typeNames = value;
    }

    internal static object Base(object obj)
    {
      if (!(obj is PSObject psObject))
        return obj;
      if (psObject == AutomationNull.Value)
        return (object) null;
      if (psObject.immediateBaseObjectIsEmpty)
        return obj;
      object immediateBaseObject;
      do
      {
        immediateBaseObject = psObject.immediateBaseObject;
      }
      while (immediateBaseObject is PSObject psObject && !psObject.immediateBaseObjectIsEmpty);
      return immediateBaseObject;
    }

    internal static PSMemberInfo GetStaticCLRMember(object obj, string methodName)
    {
      obj = PSObject.Base(obj);
      if (obj != null && obj is Type)
      {
        switch (methodName)
        {
          case "":
          case null:
            break;
          default:
            return PSObject.dotNetStaticAdapter.BaseGetMember<PSMemberInfo>(obj, methodName);
        }
      }
      return (PSMemberInfo) null;
    }

    public static PSObject AsPSObject(object obj)
    {
      if (obj == null)
        throw PSObject.tracer.NewArgumentNullException(nameof (obj));
      return obj is PSObject psObject ? psObject : new PSObject(obj);
    }

    private static string GetSeparator(ExecutionContext context, string separator)
    {
      if (separator != null)
        return separator;
      if (context != null)
      {
        object variable = context.GetVariable("OFS");
        if (variable != null)
          return variable.ToString();
      }
      return " ";
    }

    internal static string ToStringEnumerator(
      ExecutionContext context,
      IEnumerator enumerator,
      string separator,
      string format,
      IFormatProvider formatProvider)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string separator1 = PSObject.GetSeparator(context, separator);
      while (enumerator.MoveNext())
      {
        object current = enumerator.Current;
        stringBuilder.Append(PSObject.ToString(context, current, separator, format, formatProvider, false, false));
        stringBuilder.Append(separator1);
      }
      if (stringBuilder.Length == 0)
        return string.Empty;
      int length = separator1.Length;
      stringBuilder.Remove(stringBuilder.Length - length, length);
      return stringBuilder.ToString();
    }

    internal static string ToStringEnumerable(
      ExecutionContext context,
      IEnumerable enumerable,
      string separator,
      string format,
      IFormatProvider formatProvider)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string separator1 = PSObject.GetSeparator(context, separator);
      foreach (object obj in enumerable)
      {
        if (obj != null)
        {
          PSObject psObject = PSObject.AsPSObject(obj);
          stringBuilder.Append(PSObject.ToString(context, (object) psObject, separator, format, formatProvider, false, false));
        }
        stringBuilder.Append(separator1);
      }
      if (stringBuilder.Length == 0)
        return string.Empty;
      int length = separator1.Length;
      stringBuilder.Remove(stringBuilder.Length - length, length);
      return stringBuilder.ToString();
    }

    private static string ToStringEmptyBaseObject(
      ExecutionContext context,
      PSObject mshObj,
      string separator,
      string format,
      IFormatProvider formatProvider)
    {
      StringBuilder stringBuilder = new StringBuilder();
      ReadOnlyPSMemberInfoCollection<PSPropertyInfo> memberInfoCollection = mshObj.Properties.Match("*");
      if (memberInfoCollection.Count == 0)
        return string.Empty;
      stringBuilder.Append("@{");
      string str = "; ";
      foreach (PSPropertyInfo psPropertyInfo in memberInfoCollection)
      {
        stringBuilder.Append(psPropertyInfo.Name);
        stringBuilder.Append("=");
        stringBuilder.Append(PSObject.ToString(context, psPropertyInfo.Value, separator, format, formatProvider, false, false));
        stringBuilder.Append(str);
      }
      int length = str.Length;
      stringBuilder.Remove(stringBuilder.Length - length, length);
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }

    internal static string ToStringParser(ExecutionContext context, object obj) => PSObject.ToString(context, obj, (string) null, (string) null, (IFormatProvider) CultureInfo.InvariantCulture, true, true);

    internal static string ToString(
      ExecutionContext context,
      object obj,
      string separator,
      string format,
      IFormatProvider formatProvider,
      bool recurse,
      bool unravelEnumeratorOnRecurse)
    {
      if (!(obj is PSObject mshObj))
      {
        if (obj == null)
          return string.Empty;
        switch (Type.GetTypeCode(obj.GetType()))
        {
          case TypeCode.SByte:
          case TypeCode.Byte:
          case TypeCode.Int16:
          case TypeCode.UInt16:
          case TypeCode.Int32:
          case TypeCode.UInt32:
          case TypeCode.Int64:
          case TypeCode.UInt64:
            return obj.ToString();
          case TypeCode.Single:
            return ((float) obj).ToString(formatProvider);
          case TypeCode.Double:
            return ((double) obj).ToString(formatProvider);
          case TypeCode.Decimal:
            return ((Decimal) obj).ToString(formatProvider);
          case TypeCode.DateTime:
            return ((DateTime) obj).ToString(formatProvider);
          case TypeCode.String:
            return (string) obj;
          default:
            if (recurse)
            {
              IEnumerable enumerable = LanguagePrimitives.GetEnumerable(obj);
              if (enumerable != null)
              {
                try
                {
                  return PSObject.ToStringEnumerable(context, enumerable, separator, format, formatProvider);
                }
                catch (Exception ex)
                {
                  CommandProcessorBase.CheckForSevereException(ex);
                }
              }
              if (unravelEnumeratorOnRecurse)
              {
                IEnumerator enumerator = LanguagePrimitives.GetEnumerator(obj);
                if (enumerator != null)
                {
                  try
                  {
                    return PSObject.ToStringEnumerator(context, enumerator, separator, format, formatProvider);
                  }
                  catch (Exception ex)
                  {
                    CommandProcessorBase.CheckForSevereException(ex);
                  }
                }
              }
            }
            IFormattable formattable = obj as IFormattable;
            try
            {
              if (formattable != null)
                return formattable.ToString(format, formatProvider);
              return obj is Type type ? ToStringCodeMethods.Type(type) : obj.ToString();
            }
            catch (Exception ex)
            {
              CommandProcessorBase.CheckForSevereException(ex);
              throw new ExtendedTypeSystemException("ToStringObjectBasicException", ex, "ExtendedTypeSystem", "ToStringException", new object[1]
              {
                (object) ex.Message
              });
            }
        }
      }
      else
      {
        if (!(mshObj.InstanceMembers[nameof (ToString)] is PSMethodInfo psMethodInfo) && mshObj.TypeNames.Count != 0)
        {
          TypeTable typeTable = mshObj.GetTypeTable();
          if (typeTable != null)
          {
            psMethodInfo = typeTable.GetMembers<PSMethodInfo>(mshObj.InternalTypeNames)[nameof (ToString)];
            if (psMethodInfo != null)
            {
              psMethodInfo = (PSMethodInfo) psMethodInfo.Copy();
              psMethodInfo.instance = mshObj;
            }
          }
        }
        if (psMethodInfo != null)
        {
          try
          {
            if (formatProvider == null || psMethodInfo.OverloadDefinitions.Count <= 1)
              return psMethodInfo.Invoke().ToString();
            return psMethodInfo.Invoke((object) format, (object) formatProvider).ToString();
          }
          catch (MethodException ex)
          {
            throw new ExtendedTypeSystemException("MethodExceptionNullFormatProvider", (Exception) ex, "ExtendedTypeSystem", "ToStringException", new object[1]
            {
              (object) ex.Message
            });
          }
        }
        else
        {
          if (recurse)
          {
            if (mshObj.immediateBaseObjectIsEmpty)
            {
              try
              {
                return PSObject.ToStringEmptyBaseObject(context, mshObj, separator, format, formatProvider);
              }
              catch (Exception ex)
              {
                CommandProcessorBase.CheckForSevereException(ex);
              }
            }
            IEnumerable enumerable = LanguagePrimitives.GetEnumerable((object) mshObj);
            if (enumerable != null)
            {
              try
              {
                return PSObject.ToStringEnumerable(context, enumerable, separator, format, formatProvider);
              }
              catch (Exception ex)
              {
                CommandProcessorBase.CheckForSevereException(ex);
              }
            }
            if (unravelEnumeratorOnRecurse)
            {
              IEnumerator enumerator = LanguagePrimitives.GetEnumerator((object) mshObj);
              if (enumerator != null)
              {
                try
                {
                  return PSObject.ToStringEnumerator(context, enumerator, separator, format, formatProvider);
                }
                catch (Exception ex)
                {
                  CommandProcessorBase.CheckForSevereException(ex);
                }
              }
            }
          }
          if (mshObj.TokenText != null)
            return mshObj.TokenText;
          object immediateBaseObject = mshObj.immediateBaseObject;
          IFormattable formattable = immediateBaseObject as IFormattable;
          try
          {
            return (formattable != null ? formattable.ToString(format, formatProvider) : immediateBaseObject.ToString()) ?? string.Empty;
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            throw new ExtendedTypeSystemException("ToStringPSObjectBasicException", ex, "ExtendedTypeSystem", "ToStringException", new object[1]
            {
              (object) ex.Message
            });
          }
        }
      }
    }

    public override string ToString() => this.toStringFromDeserialization != null ? this.toStringFromDeserialization : PSObject.ToString(this.Context, (object) this, (string) null, (string) null, (IFormatProvider) null, true, false);

    public string ToString(string format, IFormatProvider formatProvider) => this.toStringFromDeserialization != null ? this.toStringFromDeserialization : PSObject.ToString(this.Context, (object) this, (string) null, format, formatProvider, true, false);

    private string PrivateToString()
    {
      try
      {
        return this.ToString();
      }
      catch (ExtendedTypeSystemException ex)
      {
        return this.BaseObject.GetType().FullName;
      }
    }

    public virtual PSObject Copy()
    {
      PSObject psObject = (PSObject) this.MemberwiseClone();
      psObject.Refresh(this.immediateBaseObject);
      foreach (PSMemberInfo instanceMember in (PSMemberInfoCollection<PSMemberInfo>) this.InstanceMembers)
      {
        if (!instanceMember.isHidden)
          psObject.Members.Add(instanceMember);
      }
      psObject.hasGeneratedReservedMembers = false;
      psObject.TypeNames.Clear();
      foreach (string internalTypeName in (Collection<string>) this.InternalTypeNames)
        psObject.TypeNames.Add(internalTypeName);
      if (psObject.immediateBaseObject is ICloneable immediateBaseObject)
        psObject.immediateBaseObject = immediateBaseObject.Clone();
      if (psObject.immediateBaseObject is System.ValueType)
      {
        Array instance = Array.CreateInstance(psObject.immediateBaseObject.GetType(), 1);
        instance.SetValue(this.immediateBaseObject, 0);
        psObject.immediateBaseObject = instance.GetValue(0);
      }
      return psObject;
    }

    public int CompareTo(object obj)
    {
      if (object.ReferenceEquals((object) this, obj))
        return 0;
      try
      {
        return LanguagePrimitives.Compare(this.BaseObject, obj);
      }
      catch (ArgumentException ex)
      {
        throw new ExtendedTypeSystemException("PSObjectCompareTo", (Exception) ex, "ExtendedTypeSystem", "NotTheSameTypeOrNotIcomparable", new object[3]
        {
          (object) this.PrivateToString(),
          (object) PSObject.AsPSObject(obj).ToString(),
          (object) "IComparable"
        });
      }
    }

    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals((object) this, obj))
        return true;
      return !object.ReferenceEquals(this.BaseObject, (object) PSCustomObject.SelfInstance) && LanguagePrimitives.Equals(this.BaseObject, obj);
    }

    public override int GetHashCode() => this.BaseObject.GetHashCode();

    internal int GetReferenceHashCode() => base.GetHashCode();

    internal static object GetNoteSettingValue(
      PSMemberSet settings,
      string noteName,
      object defaultValue,
      Type expectedType,
      bool shouldReplicateInstance,
      PSObject ownerObject)
    {
      if (settings == null)
        return defaultValue;
      if (shouldReplicateInstance)
        settings.ReplicateInstance(ownerObject);
      if (!(settings.Members[noteName] is PSNoteProperty member))
        return defaultValue;
      object obj = member.Value;
      return obj == null || !obj.GetType().Equals(expectedType) ? defaultValue : member.Value;
    }

    internal int GetSerializationDepth(TypeTable backupTypeTable)
    {
      int num = 0;
      TypeTable typeTableToUse = backupTypeTable != null ? backupTypeTable : this.GetTypeTable();
      if (typeTableToUse != null)
        num = (int) PSObject.GetNoteSettingValue(PSObject.TypeTableGetMemberDelegate<PSMemberSet>(this, typeTableToUse, "PSStandardMembers"), "SerializationDepth", (object) 0, typeof (int), true, this);
      return num;
    }

    internal PSPropertyInfo GetStringSerializationSource(TypeTable backupTypeTable) => this.GetPSStandardMember(backupTypeTable, "StringSerializationSource") as PSPropertyInfo;

    internal SerializationMethod GetSerializationMethod(TypeTable backupTypeTable)
    {
      SerializationMethod serializationMethod = SerializationMethod.AllPublicProperties;
      TypeTable typeTableToUse = backupTypeTable != null ? backupTypeTable : this.GetTypeTable();
      if (typeTableToUse != null)
        serializationMethod = (SerializationMethod) PSObject.GetNoteSettingValue(PSObject.TypeTableGetMemberDelegate<PSMemberSet>(this, typeTableToUse, "PSStandardMembers"), "SerializationMethod", (object) SerializationMethod.AllPublicProperties, typeof (SerializationMethod), true, this);
      return serializationMethod;
    }

    internal PSMemberSet PSStandardMembers
    {
      get
      {
        PSMemberSet psMemberSet = PSObject.TypeTableGetMemberDelegate<PSMemberSet>(this, nameof (PSStandardMembers));
        if (psMemberSet != null)
        {
          psMemberSet = (PSMemberSet) psMemberSet.Copy();
          psMemberSet.ReplicateInstance(this);
        }
        return psMemberSet;
      }
    }

    internal PSMemberInfo GetPSStandardMember(
      TypeTable backupTypeTable,
      string memberName)
    {
      PSMemberInfo psMemberInfo = (PSMemberInfo) null;
      TypeTable typeTableToUse = backupTypeTable != null ? backupTypeTable : this.GetTypeTable();
      if (typeTableToUse != null)
      {
        PSMemberSet memberDelegate = PSObject.TypeTableGetMemberDelegate<PSMemberSet>(this, typeTableToUse, "PSStandardMembers");
        if (memberDelegate != null)
        {
          memberDelegate.ReplicateInstance(this);
          psMemberInfo = new PSMemberInfoIntegratingCollection<PSMemberInfo>((object) memberDelegate, PSObject.GetMemberCollection(PSMemberViewTypes.All, backupTypeTable))[memberName];
        }
      }
      return psMemberInfo;
    }

    internal Type GetTargetTypeForDeserialization(TypeTable backupTypeTable)
    {
      PSMemberInfo psStandardMember = this.GetPSStandardMember(backupTypeTable, "TargetTypeForDeserialization");
      return psStandardMember != null ? psStandardMember.Value as Type : (Type) null;
    }

    internal Collection<string> GetSpecificPropertiesToSerialize(TypeTable backupTypeTable)
    {
      TypeTable typeTable = backupTypeTable != null ? backupTypeTable : this.GetTypeTable();
      return typeTable != null ? typeTable.GetSpecificProperties(this.InternalTypeNames) : new Collection<string>((IList<string>) new List<string>());
    }

    internal bool ShouldSerializeAdapter() => this.isDeserialized ? this.adaptedMembers != null : !this.immediateBaseObjectIsEmpty;

    internal bool ShouldSerializeBase()
    {
      if (this.isDeserialized)
        return this.adaptedMembers != this.clrMembers;
      return !this.immediateBaseObjectIsEmpty && !this.InternalAdapter.GetType().Equals(typeof (DotNetAdapter));
    }

    internal PSMemberInfoInternalCollection<PSPropertyInfo> GetAdaptedProperties() => this.GetProperties(this.adaptedMembers, this.InternalAdapter);

    internal PSMemberInfoInternalCollection<PSPropertyInfo> GetBaseProperties() => this.GetProperties(this.clrMembers, (Adapter) PSObject.dotNetInstanceAdapter);

    private PSMemberInfoInternalCollection<PSPropertyInfo> GetProperties(
      PSMemberInfoInternalCollection<PSPropertyInfo> serializedMembers,
      Adapter particularAdapter)
    {
      if (this.isDeserialized)
        return serializedMembers;
      PSMemberInfoInternalCollection<PSPropertyInfo> internalCollection = new PSMemberInfoInternalCollection<PSPropertyInfo>();
      foreach (PSPropertyInfo member in (PSMemberInfoCollection<PSPropertyInfo>) particularAdapter.BaseGetMembers<PSPropertyInfo>(this.immediateBaseObject))
        internalCollection.Add(member);
      return internalCollection;
    }

    internal void SetCoreOnDeserialization(object value, bool overrideTypeInfo)
    {
      this.immediateBaseObjectIsEmpty = false;
      this.immediateBaseObject = value;
      this.adapterSet = this.GetMappedAdapter();
      if (!overrideTypeInfo)
        return;
      this.InternalTypeNames = new ConsolidatedString(this.InternalAdapter.BaseGetTypeNameHierarchy(value));
    }

    internal bool PreserveToString
    {
      get
      {
        if (this.preserveToStringSet)
          return this.preserveToString;
        this.preserveToStringSet = true;
        if (this.TypeNames.Count == 0)
          return false;
        this.preserveToString = false;
        return this.preserveToString;
      }
    }

    internal string ToStringFromDeserialization
    {
      get => this.toStringFromDeserialization;
      set => this.toStringFromDeserialization = value;
    }

    internal class AdapterSet
    {
      private Adapter originalAdapter;
      private DotNetAdapter ultimatedotNetAdapter;

      internal Adapter OriginalAdapter
      {
        get => this.originalAdapter;
        set => this.originalAdapter = value;
      }

      internal DotNetAdapter DotNetAdapter => this.ultimatedotNetAdapter;

      internal AdapterSet(Adapter adapter, DotNetAdapter dotnetAdapter)
      {
        this.originalAdapter = adapter;
        this.ultimatedotNetAdapter = dotnetAdapter;
      }
    }
  }
}
