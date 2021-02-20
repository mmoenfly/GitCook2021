// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.InternalSerializer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Management.Automation
{
  internal class InternalSerializer
  {
    internal const string DefaultVersion = "1.1.0.1";
    private const int MaxDepthBelowTopLevel = 50;
    private XmlWriter _writer;
    private SerializationContext _context;
    private TypeTable _typeTable;
    private int depthBelowTopLevel;
    private readonly ReferenceIdHandlerForSerializer<object> objectRefIdHandler;
    private readonly ReferenceIdHandlerForSerializer<ConsolidatedString> typeRefIdHandler;
    private bool isStopping;
    private bool? canUseDefaultRunspaceInThreadSafeManner;
    private Collection<CollectionEntry<PSMemberInfo>> extendedMembersCollection;
    private Collection<CollectionEntry<PSPropertyInfo>> allPropertiesCollection;
    [TraceSource("InternalSerializer", "InternalSerializer class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (InternalSerializer), "InternalSerializer class");
    private static IETWTracer etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Serializer);

    internal InternalSerializer(XmlWriter writer, SerializationContext context)
    {
      this._writer = writer;
      this._context = context;
      IDictionary<object, ulong> dictionary = (IDictionary<object, ulong>) null;
      if ((this._context.options & SerializationOptions.NoObjectRefIds) == SerializationOptions.None)
        dictionary = (IDictionary<object, ulong>) new WeakReferenceDictionary<ulong>();
      this.objectRefIdHandler = new ReferenceIdHandlerForSerializer<object>(dictionary);
      this.typeRefIdHandler = new ReferenceIdHandlerForSerializer<ConsolidatedString>((IDictionary<ConsolidatedString, ulong>) new Dictionary<ConsolidatedString, ulong>((IEqualityComparer<ConsolidatedString>) new InternalSerializer.ConsolidatedStringEqualityComparer()));
    }

    internal TypeTable TypeTable
    {
      get => this._typeTable;
      set => this._typeTable = value;
    }

    internal void Start()
    {
      if (SerializationOptions.NoRootElement == (this._context.options & SerializationOptions.NoRootElement))
        return;
      this.WriteStartElement("Objs");
      this.WriteAttribute("Version", "1.1.0.1");
    }

    internal void End()
    {
      if (SerializationOptions.NoRootElement != (this._context.options & SerializationOptions.NoRootElement))
        this._writer.WriteEndElement();
      this._writer.Flush();
    }

    internal void Stop() => this.isStopping = true;

    private void CheckIfStopping()
    {
      if (this.isStopping)
        throw InternalSerializer._trace.NewInvalidOperationException("Serialization", "Stopping");
    }

    internal void WriteOneTopLevelObject(object source, string streamName) => this.WriteOneObject(source, streamName, (string) null, this._context.depth);

    private void WriteOneObject(object source, string streamName, string property, int depth)
    {
      this.CheckIfStopping();
      if (source == null)
      {
        this.WriteNull(streamName, property);
      }
      else
      {
        try
        {
          ++this.depthBelowTopLevel;
          if (this.HandleMaxDepth(source, streamName, property) || this.HandlePrimitiveKnownType(source, streamName, property))
            return;
          depth = this.GetDepthOfSerialization(source, depth);
          string refId = this.objectRefIdHandler.GetRefId(source);
          if (refId != null)
          {
            this.WritePSObjectReference(streamName, property, refId);
          }
          else
          {
            if (this.HandleSecureString(source, streamName, property) || this.HandlePrimitiveKnownTypePSObject(source, streamName, property, depth) || this.HandleKnownContainerTypes(source, streamName, property, depth))
              return;
            PSObject source1 = PSObject.AsPSObject(source);
            if (depth == 0 || this.SerializeAsString(source1))
              this.HandlePSObjectAsString(source1, streamName, property, depth);
            else
              this.HandleComplexTypePSObject(source, streamName, property, depth);
          }
        }
        finally
        {
          --this.depthBelowTopLevel;
        }
      }
    }

    private bool HandleMaxDepth(object source, string streamName, string property)
    {
      if (this.depthBelowTopLevel != 50)
        return false;
      InternalSerializer.etwTracer.AnalyticChannel.WriteError(PSEventId.Serializer_MaxDepthWhenSerializing, PSOpcode.Exception, PSTask.Serialization, (object) source.GetType().AssemblyQualifiedName, (object) property, (object) this.depthBelowTopLevel);
      this.HandlePrimitiveKnownType((object) ResourceManagerCache.GetResourceString("Serialization", "DeserializationTooDeep"), streamName, property);
      return true;
    }

    private bool HandlePrimitiveKnownType(object source, string streamName, string property)
    {
      TypeSerializationInfo serializationInfo = KnownTypes.GetTypeSerializationInfo(source.GetType());
      if (serializationInfo == null)
        return false;
      InternalSerializer.WriteOnePrimitiveKnownType(this, streamName, property, source, serializationInfo);
      return true;
    }

    private bool HandleSecureString(object source, string streamName, string property)
    {
      if (this._context.cryptoHelper == null)
        return false;
      PSObject psObject = !(source is SecureString secureString) ? source as PSObject : PSObject.AsPSObject((object) secureString);
      if (psObject != null && !psObject.immediateBaseObjectIsEmpty && (psObject.ImmediateBaseObject.GetType() == typeof (SecureString) && this._context.cryptoHelper != null))
      {
        SecureString immediateBaseObject = psObject.ImmediateBaseObject as SecureString;
        try
        {
          string text = this._context.cryptoHelper.EncryptSecureString(immediateBaseObject);
          if (property != null)
          {
            this.WriteStartElement("SS");
            this.WriteNameAttribute(property);
          }
          else
            this.WriteStartElement("SS");
          if (streamName != null)
            this.WriteAttribute("S", streamName);
          this._writer.WriteString(text);
          this._writer.WriteEndElement();
          return true;
        }
        catch (PSCryptoException ex)
        {
        }
      }
      return false;
    }

    private bool HandlePrimitiveKnownTypePSObject(
      object source,
      string streamName,
      string property,
      int depth)
    {
      bool flag = false;
      if (source is PSObject source1 && !source1.immediateBaseObjectIsEmpty)
      {
        object immediateBaseObject = source1.ImmediateBaseObject;
        TypeSerializationInfo serializationInfo = KnownTypes.GetTypeSerializationInfo(immediateBaseObject.GetType());
        if (serializationInfo != null)
        {
          this.WritePrimitiveTypePSObject(source1, immediateBaseObject, serializationInfo, streamName, property, depth);
          flag = true;
        }
      }
      return flag;
    }

    private bool HandleKnownContainerTypes(
      object source,
      string streamName,
      string property,
      int depth)
    {
      ContainerType ct = ContainerType.None;
      PSObject source1 = source as PSObject;
      IEnumerable enumerable = (IEnumerable) null;
      IDictionary dictionary = (IDictionary) null;
      if (source1 != null && source1.immediateBaseObjectIsEmpty)
        return false;
      this.GetKnownContainerTypeInfo(source1 != null ? source1.ImmediateBaseObject : source, out ct, out dictionary, out enumerable);
      if (ct == ContainerType.None)
        return false;
      string refId = this.objectRefIdHandler.SetRefId(source);
      this.WriteStartOfPSObject(source1 ?? PSObject.AsPSObject(source), streamName, property, refId, true, false);
      switch (ct)
      {
        case ContainerType.Dictionary:
          this.WriteDictionary(dictionary, "DCT", depth);
          break;
        case ContainerType.Queue:
          this.WriteEnumerable(enumerable, "QUE", depth);
          break;
        case ContainerType.Stack:
          this.WriteEnumerable(enumerable, "STK", depth);
          break;
        case ContainerType.List:
          this.WriteEnumerable(enumerable, "LST", depth);
          break;
        case ContainerType.Enumerable:
          this.WriteEnumerable(enumerable, "IE", depth);
          break;
      }
      if (depth != 0)
      {
        if (ct == ContainerType.Enumerable || source1 != null && source1.isDeserialized)
        {
          PSObject source2 = PSObject.AsPSObject(source);
          PSMemberInfoInternalCollection<PSPropertyInfo> propertiesToSerialize = this.GetSpecificPropertiesToSerialize(source2);
          this.WritePSObjectProperties(source2, depth, (IEnumerable<PSPropertyInfo>) propertiesToSerialize);
          this.SerializeExtendedProperties(source2, depth, (IEnumerable<PSPropertyInfo>) propertiesToSerialize);
        }
        else if (source1 != null)
          this.SerializeInstanceProperties(source1, depth);
      }
      this._writer.WriteEndElement();
      return true;
    }

    private void GetKnownContainerTypeInfo(
      object source,
      out ContainerType ct,
      out IDictionary dictionary,
      out IEnumerable enumerable)
    {
      ct = ContainerType.None;
      dictionary = (IDictionary) null;
      enumerable = (IEnumerable) null;
      dictionary = source as IDictionary;
      if (dictionary != null)
      {
        ct = ContainerType.Dictionary;
      }
      else
      {
        switch (source)
        {
          case Stack _:
            ct = ContainerType.Stack;
            enumerable = LanguagePrimitives.GetEnumerable(source);
            break;
          case Queue _:
            ct = ContainerType.Queue;
            enumerable = LanguagePrimitives.GetEnumerable(source);
            break;
          case IList _:
            ct = ContainerType.List;
            enumerable = LanguagePrimitives.GetEnumerable(source);
            break;
          default:
            Type type = source.GetType();
            if (type.IsGenericType)
            {
              if (InternalSerializer.DerivesFromGenericType(type, typeof (Stack<>)))
              {
                ct = ContainerType.Stack;
                enumerable = LanguagePrimitives.GetEnumerable(source);
                break;
              }
              if (InternalSerializer.DerivesFromGenericType(type, typeof (Queue<>)))
              {
                ct = ContainerType.Queue;
                enumerable = LanguagePrimitives.GetEnumerable(source);
                break;
              }
              if (InternalSerializer.DerivesFromGenericType(type, typeof (List<>)))
              {
                ct = ContainerType.List;
                enumerable = LanguagePrimitives.GetEnumerable(source);
                break;
              }
              break;
            }
            break;
        }
        if (ct == ContainerType.None)
        {
          try
          {
            enumerable = LanguagePrimitives.GetEnumerable(source);
            if (enumerable != null)
              ct = ContainerType.Enumerable;
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            InternalSerializer.etwTracer.AnalyticChannel.WriteWarning(PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception, PSTask.Serialization, (object) source.GetType().AssemblyQualifiedName, (object) ex.ToString());
          }
        }
        if (ct != ContainerType.None)
          return;
        enumerable = source as IEnumerable;
        if (enumerable == null)
          return;
        ct = ContainerType.Enumerable;
      }
    }

    private static bool DerivesFromGenericType(Type derived, Type baseType)
    {
      for (; derived != null; derived = derived.BaseType)
      {
        if (derived.IsGenericType)
          derived = derived.GetGenericTypeDefinition();
        if (derived == baseType)
          return true;
      }
      return false;
    }

    private void WritePSObjectReference(string streamName, string property, string refId)
    {
      this.WriteStartElement("Ref");
      if (streamName != null)
        this.WriteAttribute("S", streamName);
      if (property != null)
        this.WriteNameAttribute(property);
      this.WriteAttribute("RefId", refId);
      this._writer.WriteEndElement();
    }

    private static bool PSObjectHasModifiedTypesCollection(PSObject pso)
    {
      Collection<string> typeNames = pso.TypeNames;
      Collection<string> typeNameHierarchy = pso.InternalAdapter.BaseGetTypeNameHierarchy(pso.ImmediateBaseObject);
      if (typeNames.Count != typeNameHierarchy.Count)
        return true;
      IEnumerator<string> enumerator1 = typeNames.GetEnumerator();
      IEnumerator<string> enumerator2 = typeNameHierarchy.GetEnumerator();
      while (enumerator1.MoveNext() && enumerator2.MoveNext())
      {
        if (!enumerator1.Current.Equals(enumerator2.Current, StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    private void WritePrimitiveTypePSObject(
      PSObject source,
      object primitive,
      TypeSerializationInfo pktInfo,
      string streamName,
      string property,
      int depth)
    {
      bool hasModifiedTypesCollection = InternalSerializer.PSObjectHasModifiedTypesCollection(source);
      if (InternalSerializer.PSObjectHasNotes(source) || hasModifiedTypesCollection)
        this.WritePrimitiveTypePSObjectWithNotes(source, primitive, hasModifiedTypesCollection, pktInfo, streamName, property, depth);
      else if (primitive != null)
        InternalSerializer.WriteOnePrimitiveKnownType(this, streamName, property, primitive, pktInfo);
      else
        this.WriteNull(streamName, property);
    }

    private void WritePrimitiveTypePSObjectWithNotes(
      PSObject source,
      object primitive,
      bool hasModifiedTypesCollection,
      TypeSerializationInfo pktInfo,
      string streamName,
      string property,
      int depth)
    {
      string refId = this.objectRefIdHandler.SetRefId((object) source);
      this.WriteStartOfPSObject(source, streamName, property, refId, hasModifiedTypesCollection, source.ToStringFromDeserialization != null);
      if (pktInfo != null)
        InternalSerializer.WriteOnePrimitiveKnownType(this, streamName, (string) null, primitive, pktInfo);
      this.SerializeInstanceProperties(source, depth);
      this._writer.WriteEndElement();
    }

    private void HandleComplexTypePSObject(
      object source,
      string streamName,
      string property,
      int depth)
    {
      PSObject psObject = PSObject.AsPSObject(source);
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      bool flag4 = false;
      if (!psObject.immediateBaseObjectIsEmpty)
      {
        if (psObject.ImmediateBaseObject is ErrorRecord immediateBaseObject)
        {
          immediateBaseObject.ToPSObjectForRemoting(psObject);
          flag1 = true;
        }
        else if (psObject.ImmediateBaseObject is InformationalRecord immediateBaseObject)
        {
          immediateBaseObject.ToPSObjectForRemoting(psObject);
          flag2 = true;
        }
        else
        {
          flag3 = psObject.ImmediateBaseObject is Enum;
          flag4 = psObject.ImmediateBaseObject is PSObject;
        }
      }
      bool writeToString = true;
      if (psObject.ToStringFromDeserialization == null && psObject.immediateBaseObjectIsEmpty)
        writeToString = false;
      string refId = this.objectRefIdHandler.SetRefId(source);
      this.WriteStartOfPSObject(psObject, streamName, property, refId, true, writeToString);
      PSMemberInfoInternalCollection<PSPropertyInfo> propertiesToSerialize = this.GetSpecificPropertiesToSerialize(psObject);
      if (flag3)
      {
        object immediateBaseObject = psObject.ImmediateBaseObject;
        this.WriteOneObject(Convert.ChangeType(immediateBaseObject, Enum.GetUnderlyingType(immediateBaseObject.GetType()), (IFormatProvider) CultureInfo.InvariantCulture), (string) null, (string) null, depth);
      }
      else if (flag4)
        this.WriteOneObject(psObject.ImmediateBaseObject, (string) null, (string) null, depth);
      else if (!flag1 && !flag2)
        this.WritePSObjectProperties(psObject, depth, (IEnumerable<PSPropertyInfo>) propertiesToSerialize);
      this.SerializeExtendedProperties(psObject, depth, (IEnumerable<PSPropertyInfo>) propertiesToSerialize);
      this._writer.WriteEndElement();
    }

    private void WriteStartOfPSObject(
      PSObject mshObject,
      string streamName,
      string property,
      string refId,
      bool writeTypeNames,
      bool writeToString)
    {
      this.WriteStartElement("Obj");
      if (streamName != null)
        this.WriteAttribute("S", streamName);
      if (property != null)
        this.WriteNameAttribute(property);
      if (refId != null)
        this.WriteAttribute("RefId", refId);
      if (writeTypeNames)
      {
        ConsolidatedString internalTypeNames = mshObject.InternalTypeNames;
        if (internalTypeNames.Count > 0)
        {
          string refId1 = this.typeRefIdHandler.GetRefId(internalTypeNames);
          if (refId1 == null)
          {
            this.WriteStartElement("TN");
            this.WriteAttribute("RefId", this.typeRefIdHandler.SetRefId(internalTypeNames));
            foreach (string str in (Collection<string>) internalTypeNames)
              this.WriteEncodedElementString("T", str);
            this._writer.WriteEndElement();
          }
          else
          {
            this.WriteStartElement("TNRef");
            this.WriteAttribute("RefId", refId1);
            this._writer.WriteEndElement();
          }
        }
      }
      if (!writeToString)
        return;
      string toString = this.GetToString((object) mshObject);
      if (toString == null)
        return;
      this.WriteEncodedElementString("ToString", toString);
    }

    private static bool PSObjectHasNotes(PSObject source) => source.InstanceMembers != null && source.InstanceMembers.Count > 0;

    private bool CanUseDefaultRunspaceInThreadSafeManner
    {
      get
      {
        if (!this.canUseDefaultRunspaceInThreadSafeManner.HasValue)
        {
          this.canUseDefaultRunspaceInThreadSafeManner = new bool?(false);
          if (Runspace.DefaultRunspace is RunspaceBase defaultRunspace && defaultRunspace.GetCurrentlyRunningPipeline() is LocalPipeline currentlyRunningPipeline && currentlyRunningPipeline.NestedPipelineExecutionThread != null)
            this.canUseDefaultRunspaceInThreadSafeManner = new bool?(currentlyRunningPipeline.NestedPipelineExecutionThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId);
        }
        return this.canUseDefaultRunspaceInThreadSafeManner.Value;
      }
    }

    private object GetPropertyValueInThreadSafeManner(PSPropertyInfo property, out bool success)
    {
      if (!property.IsGettable)
      {
        success = false;
        return (object) null;
      }
      if (property is PSAliasProperty psAliasProperty)
        property = psAliasProperty.ReferencedMember as PSPropertyInfo;
      if (property is PSScriptProperty psScriptProperty)
      {
        if (!this.CanUseDefaultRunspaceInThreadSafeManner)
        {
          InternalSerializer.etwTracer.AnalyticChannel.WriteWarning(PSEventId.Serializer_ScriptPropertyWithoutRunspace, PSOpcode.Exception, PSTask.Serialization, (object) property.Name, property.instance == null ? (object) string.Empty : (object) property.instance.InternalTypeNames.Key, (object) psScriptProperty.GetterScript.ToString());
          success = false;
          return (object) null;
        }
      }
      try
      {
        object obj = property.Value;
        success = true;
        return obj;
      }
      catch (GetValueException ex)
      {
        InternalSerializer.etwTracer.AnalyticChannel.WriteWarning(PSEventId.Serializer_PropertyGetterFailed, PSOpcode.Exception, PSTask.Serialization, (object) property.Name, property.instance == null ? (object) string.Empty : (object) property.instance.InternalTypeNames.Key, (object) ex.ToString(), ex.InnerException == null ? (object) string.Empty : (object) ex.InnerException.ToString());
        success = false;
        return (object) null;
      }
    }

    private void WriteMemberInfoCollection(
      IEnumerable<PSMemberInfo> me,
      int depth,
      bool writeEnclosingMemberSetElementTag)
    {
      bool flag = false;
      foreach (PSMemberInfo psMemberInfo in me)
      {
        if (psMemberInfo.ShouldSerialize)
        {
          int depth1 = psMemberInfo.IsInstance ? depth : depth - 1;
          if (psMemberInfo.MemberType == (psMemberInfo.MemberType & PSMemberTypes.Properties))
          {
            bool success;
            object threadSafeManner = this.GetPropertyValueInThreadSafeManner((PSPropertyInfo) psMemberInfo, out success);
            if (success)
            {
              if (writeEnclosingMemberSetElementTag && !flag)
              {
                flag = true;
                this.WriteStartElement("MS");
              }
              this.WriteOneObject(threadSafeManner, (string) null, psMemberInfo.Name, depth1);
            }
          }
          else if (psMemberInfo.MemberType == PSMemberTypes.MemberSet)
          {
            if (writeEnclosingMemberSetElementTag && !flag)
            {
              flag = true;
              this.WriteStartElement("MS");
            }
            this.WriteMemberSet((PSMemberSet) psMemberInfo, depth1);
          }
        }
      }
      if (!flag)
        return;
      this._writer.WriteEndElement();
    }

    private void WriteMemberSet(PSMemberSet set, int depth)
    {
      if (!set.ShouldSerialize)
        return;
      this.WriteStartElement("MS");
      this.WriteNameAttribute(set.Name);
      this.WriteMemberInfoCollection((IEnumerable<PSMemberInfo>) set.Members, depth, false);
      this._writer.WriteEndElement();
    }

    private PSMemberInfoInternalCollection<PSPropertyInfo> GetSpecificPropertiesToSerialize(
      PSObject source)
    {
      if (source == null)
        return (PSMemberInfoInternalCollection<PSPropertyInfo>) null;
      if (source.GetSerializationMethod(this._typeTable) != SerializationMethod.SpecificProperties)
        return (PSMemberInfoInternalCollection<PSPropertyInfo>) null;
      InternalSerializer.etwTracer.AnalyticChannel.WriteVerbose(PSEventId.Serializer_ModeOverride, PSOpcode.SerializationSettings, PSTask.Serialization, (object) source.InternalTypeNames.Key, (object) 2U);
      PSMemberInfoInternalCollection<PSPropertyInfo> internalCollection = new PSMemberInfoInternalCollection<PSPropertyInfo>();
      PSMemberInfoIntegratingCollection<PSPropertyInfo> integratingCollection = new PSMemberInfoIntegratingCollection<PSPropertyInfo>((object) source, this.AllPropertiesCollection);
      foreach (string name in source.GetSpecificPropertiesToSerialize(this._typeTable))
      {
        PSPropertyInfo member = integratingCollection[name];
        if (member == null)
          InternalSerializer.etwTracer.AnalyticChannel.WriteWarning(PSEventId.Serializer_SpecificPropertyMissing, PSOpcode.Exception, PSTask.Serialization, (object) source.InternalTypeNames.Key, (object) name);
        else
          internalCollection.Add(member);
      }
      return internalCollection;
    }

    private void WritePSObjectProperties(
      PSObject source,
      int depth,
      IEnumerable<PSPropertyInfo> specificPropertiesToSerialize)
    {
      --depth;
      if (specificPropertiesToSerialize != null)
      {
        this.SerializeProperties(specificPropertiesToSerialize, "Props", depth);
      }
      else
      {
        if (!source.ShouldSerializeAdapter())
          return;
        IEnumerable<PSPropertyInfo> adaptedProperties = (IEnumerable<PSPropertyInfo>) source.GetAdaptedProperties();
        if (adaptedProperties == null)
          return;
        this.SerializeProperties(adaptedProperties, "Props", depth);
      }
    }

    private void SerializeInstanceProperties(PSObject source, int depth)
    {
      PSMemberInfoCollection<PSMemberInfo> instanceMembers = (PSMemberInfoCollection<PSMemberInfo>) source.InstanceMembers;
      if (instanceMembers == null)
        return;
      this.WriteMemberInfoCollection((IEnumerable<PSMemberInfo>) instanceMembers, depth, true);
    }

    private Collection<CollectionEntry<PSMemberInfo>> ExtendedMembersCollection
    {
      get
      {
        if (this.extendedMembersCollection == null)
          this.extendedMembersCollection = PSObject.GetMemberCollection(PSMemberViewTypes.Extended, this._typeTable);
        return this.extendedMembersCollection;
      }
    }

    private Collection<CollectionEntry<PSPropertyInfo>> AllPropertiesCollection
    {
      get
      {
        if (this.allPropertiesCollection == null)
          this.allPropertiesCollection = PSObject.GetPropertyCollection(PSMemberViewTypes.All, this._typeTable);
        return this.allPropertiesCollection;
      }
    }

    private void SerializeExtendedProperties(
      PSObject source,
      int depth,
      IEnumerable<PSPropertyInfo> specificPropertiesToSerialize)
    {
      IEnumerable<PSMemberInfo> me;
      if (specificPropertiesToSerialize == null)
      {
        me = (IEnumerable<PSMemberInfo>) new PSMemberInfoIntegratingCollection<PSMemberInfo>((object) source, this.ExtendedMembersCollection).Match("*", PSMemberTypes.Properties | PSMemberTypes.PropertySet | PSMemberTypes.MemberSet, MshMemberMatchOptions.IncludeHidden | MshMemberMatchOptions.OnlySerializable);
      }
      else
      {
        List<PSMemberInfo> psMemberInfoList = new List<PSMemberInfo>((IEnumerable<PSMemberInfo>) source.InstanceMembers);
        me = (IEnumerable<PSMemberInfo>) psMemberInfoList;
        foreach (PSMemberInfo psMemberInfo in specificPropertiesToSerialize)
        {
          if (!psMemberInfo.IsInstance && !(psMemberInfo is PSProperty))
            psMemberInfoList.Add(psMemberInfo);
        }
      }
      if (me == null)
        return;
      this.WriteMemberInfoCollection(me, depth, true);
    }

    private void SerializeProperties(
      IEnumerable<PSPropertyInfo> propertyCollection,
      string name,
      int depth)
    {
      bool flag = false;
      foreach (PSMemberInfo property in propertyCollection)
      {
        if (property is PSProperty psProperty)
        {
          if (!flag)
          {
            this.WriteStartElement(name);
            flag = true;
          }
          bool success;
          object threadSafeManner = this.GetPropertyValueInThreadSafeManner((PSPropertyInfo) psProperty, out success);
          if (success)
            this.WriteOneObject(threadSafeManner, (string) null, psProperty.Name, depth);
        }
      }
      if (!flag)
        return;
      this._writer.WriteEndElement();
    }

    private void WriteEnumerable(IEnumerable enumerable, string tag, int depth)
    {
      this.WriteStartElement(tag);
      IEnumerator enumerator;
      try
      {
        enumerator = enumerable.GetEnumerator();
        enumerator.Reset();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        InternalSerializer.etwTracer.AnalyticChannel.WriteWarning(PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception, PSTask.Serialization, (object) enumerable.GetType().AssemblyQualifiedName, (object) ex.ToString());
        enumerator = (IEnumerator) null;
      }
      if (enumerator != null)
      {
        while (true)
        {
          object current;
          try
          {
            if (enumerator.MoveNext())
              current = enumerator.Current;
            else
              break;
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            InternalSerializer.etwTracer.AnalyticChannel.WriteWarning(PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception, PSTask.Serialization, (object) enumerable.GetType().AssemblyQualifiedName, (object) ex.ToString());
            break;
          }
          this.WriteOneObject(current, (string) null, (string) null, depth);
        }
      }
      this._writer.WriteEndElement();
    }

    private void WriteDictionary(IDictionary dictionary, string tag, int depth)
    {
      this.WriteStartElement(tag);
      IDictionaryEnumerator dictionaryEnumerator = (IDictionaryEnumerator) null;
      try
      {
        dictionaryEnumerator = dictionary.GetEnumerator();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        InternalSerializer.etwTracer.AnalyticChannel.WriteWarning(PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception, PSTask.Serialization, (object) dictionary.GetType().AssemblyQualifiedName, (object) ex.ToString());
      }
      if (dictionaryEnumerator != null)
      {
        while (true)
        {
          object key;
          object source;
          try
          {
            if (dictionaryEnumerator.MoveNext())
            {
              key = dictionaryEnumerator.Key;
              source = dictionaryEnumerator.Value;
            }
            else
              break;
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            InternalSerializer.etwTracer.AnalyticChannel.WriteWarning(PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception, PSTask.Serialization, (object) dictionary.GetType().AssemblyQualifiedName, (object) ex.ToString());
            break;
          }
          if (key != null)
          {
            this.WriteStartElement("En");
            this.WriteOneObject(key, (string) null, "Key", depth);
            this.WriteOneObject(source, (string) null, "Value", depth);
            this._writer.WriteEndElement();
          }
          else
            break;
        }
      }
      this._writer.WriteEndElement();
    }

    private void HandlePSObjectAsString(
      PSObject source,
      string streamName,
      string property,
      int depth)
    {
      string serializationString = this.GetSerializationString(source);
      TypeSerializationInfo pktInfo = (TypeSerializationInfo) null;
      if (serializationString != null)
        pktInfo = KnownTypes.GetTypeSerializationInfo(serializationString.GetType());
      this.WritePrimitiveTypePSObject(source, (object) serializationString, pktInfo, streamName, property, depth);
    }

    private string GetSerializationString(PSObject source)
    {
      PSPropertyInfo serializationSource = source.GetStringSerializationSource(this._typeTable);
      string str = (string) null;
      if (serializationSource != null)
      {
        bool success;
        object threadSafeManner = this.GetPropertyValueInThreadSafeManner(serializationSource, out success);
        if (success && threadSafeManner != null)
          str = this.GetToString(threadSafeManner);
      }
      else
        str = this.GetToString((object) source);
      return str;
    }

    private string GetToString(object source)
    {
      string str = (string) null;
      try
      {
        str = source.ToString();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        InternalSerializer.etwTracer.AnalyticChannel.WriteWarning(PSEventId.Serializer_ToStringFailed, PSOpcode.Exception, PSTask.Serialization, (object) source.GetType().AssemblyQualifiedName, (object) ex.ToString());
      }
      return str;
    }

    private bool SerializeAsString(PSObject source)
    {
      if (source.GetSerializationMethod(this._typeTable) != SerializationMethod.String)
        return false;
      InternalSerializer.etwTracer.AnalyticChannel.WriteVerbose(PSEventId.Serializer_ModeOverride, PSOpcode.SerializationSettings, PSTask.Serialization, (object) source.InternalTypeNames.Key, (object) 1U);
      return true;
    }

    private int GetDepthOfSerialization(object source, int depth)
    {
      PSObject psObject = PSObject.AsPSObject(source);
      if (psObject == null)
        return depth;
      if ((this._context.options & SerializationOptions.UseDepthFromTypes) != SerializationOptions.None)
      {
        int serializationDepth = psObject.GetSerializationDepth(this._typeTable);
        if (serializationDepth > 0 && serializationDepth != depth)
        {
          InternalSerializer.etwTracer.AnalyticChannel.WriteVerbose(PSEventId.Serializer_DepthOverride, PSOpcode.SerializationSettings, PSTask.Serialization, (object) psObject.InternalTypeNames.Key, (object) depth, (object) serializationDepth, (object) this.depthBelowTopLevel);
          return serializationDepth;
        }
      }
      return (this._context.options & SerializationOptions.PreserveSerializationSettingOfOriginal) != SerializationOptions.None && psObject.isDeserialized && depth <= 0 ? 1 : depth;
    }

    private void WriteNull(string streamName, string property)
    {
      this.WriteStartElement("Nil");
      if (streamName != null)
        this.WriteAttribute("S", streamName);
      if (property != null)
        this.WriteNameAttribute(property);
      this._writer.WriteEndElement();
    }

    private static void WriteRawString(
      InternalSerializer serializer,
      string streamName,
      string property,
      string raw,
      TypeSerializationInfo entry)
    {
      if (property != null)
      {
        serializer.WriteStartElement(entry.PropertyTag);
        serializer.WriteNameAttribute(property);
      }
      else
        serializer.WriteStartElement(entry.ItemTag);
      if (streamName != null)
        serializer.WriteAttribute("S", streamName);
      serializer._writer.WriteRaw(raw);
      serializer._writer.WriteEndElement();
    }

    private static void WriteOnePrimitiveKnownType(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      if (entry.Serializer == null)
      {
        string raw = source.ToString();
        InternalSerializer.WriteRawString(serializer, streamName, property, raw, entry);
      }
      else
        entry.Serializer(serializer, streamName, property, source, entry);
    }

    internal static void WriteDateTime(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      InternalSerializer.WriteRawString(serializer, streamName, property, XmlConvert.ToString((DateTime) source, XmlDateTimeSerializationMode.RoundtripKind), entry);
    }

    internal static void WriteVersion(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      InternalSerializer.WriteRawString(serializer, streamName, property, source.ToString(), entry);
    }

    internal static void WriteScriptBlock(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      InternalSerializer.WriteEncodedString(serializer, streamName, property, (object) source.ToString(), entry);
    }

    internal static void WriteUri(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      InternalSerializer.WriteEncodedString(serializer, streamName, property, (object) source.ToString(), entry);
    }

    internal static void WriteEncodedString(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      if (property != null)
      {
        serializer.WriteStartElement(entry.PropertyTag);
        serializer.WriteNameAttribute(property);
      }
      else
        serializer.WriteStartElement(entry.ItemTag);
      if (streamName != null)
        serializer.WriteAttribute("S", streamName);
      string text = InternalSerializer.EncodeString((string) source);
      serializer._writer.WriteString(text);
      serializer._writer.WriteEndElement();
    }

    internal static void WriteDouble(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      InternalSerializer.WriteRawString(serializer, streamName, property, XmlConvert.ToString((double) source), entry);
    }

    internal static void WriteChar(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      InternalSerializer.WriteRawString(serializer, streamName, property, XmlConvert.ToString((ushort) (char) source), entry);
    }

    internal static void WriteBoolean(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      InternalSerializer.WriteRawString(serializer, streamName, property, XmlConvert.ToString((bool) source), entry);
    }

    internal static void WriteSingle(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      InternalSerializer.WriteRawString(serializer, streamName, property, XmlConvert.ToString((float) source), entry);
    }

    internal static void WriteTimeSpan(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      InternalSerializer.WriteRawString(serializer, streamName, property, XmlConvert.ToString((TimeSpan) source), entry);
    }

    internal static void WriteByteArray(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      byte[] buffer = (byte[]) source;
      if (property != null)
      {
        serializer.WriteStartElement(entry.PropertyTag);
        serializer.WriteNameAttribute(property);
      }
      else
        serializer.WriteStartElement(entry.ItemTag);
      if (streamName != null)
        serializer.WriteAttribute("S", streamName);
      serializer._writer.WriteBase64(buffer, 0, buffer.Length);
      serializer._writer.WriteEndElement();
    }

    internal static void WriteXmlDocument(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      string outerXml = ((XmlNode) source).OuterXml;
      InternalSerializer.WriteEncodedString(serializer, streamName, property, (object) outerXml, entry);
    }

    internal static void WriteProgressRecord(
      InternalSerializer serializer,
      string streamName,
      string property,
      object source,
      TypeSerializationInfo entry)
    {
      ProgressRecord progressRecord = (ProgressRecord) source;
      serializer.WriteStartElement(entry.PropertyTag);
      if (property != null)
        serializer.WriteNameAttribute(property);
      if (streamName != null)
        serializer.WriteAttribute("S", streamName);
      serializer.WriteEncodedElementString("AV", progressRecord.Activity.ToString());
      serializer.WriteEncodedElementString("AI", progressRecord.ActivityId.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      serializer.WriteOneObject((object) progressRecord.CurrentOperation, (string) null, (string) null, 1);
      serializer.WriteEncodedElementString("PI", progressRecord.ParentActivityId.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      serializer.WriteEncodedElementString("PC", progressRecord.PercentComplete.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      serializer.WriteEncodedElementString("T", progressRecord.RecordType.ToString());
      serializer.WriteEncodedElementString("SR", progressRecord.SecondsRemaining.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      serializer.WriteEncodedElementString("SD", progressRecord.StatusDescription);
      serializer._writer.WriteEndElement();
    }

    private void WriteStartElement(string elementTag)
    {
      if (SerializationOptions.NoNamespace == (this._context.options & SerializationOptions.NoNamespace))
        this._writer.WriteStartElement(elementTag);
      else
        this._writer.WriteStartElement(elementTag, "http://schemas.microsoft.com/powershell/2004/04");
    }

    private void WriteAttribute(string name, string value) => this._writer.WriteAttributeString(name, value);

    private void WriteNameAttribute(string value) => this.WriteAttribute("N", InternalSerializer.EncodeString(value));

    private static string EncodeString(string s)
    {
      StringBuilder stringBuilder = new StringBuilder(s.Length + 20);
      foreach (char c in s.Replace("_x", "_x005f_x"))
      {
        switch (char.GetUnicodeCategory(c))
        {
          case UnicodeCategory.Control:
          case UnicodeCategory.Surrogate:
            stringBuilder.Append("_x");
            stringBuilder.Append(((int) c).ToString("X4", (IFormatProvider) CultureInfo.InvariantCulture));
            stringBuilder.Append('_');
            break;
          default:
            stringBuilder.Append(c);
            break;
        }
      }
      return stringBuilder.ToString();
    }

    private void WriteEncodedElementString(string name, string value)
    {
      this.CheckIfStopping();
      value = InternalSerializer.EncodeString(value);
      if (SerializationOptions.NoNamespace == (this._context.options & SerializationOptions.NoNamespace))
        this._writer.WriteElementString(name, value);
      else
        this._writer.WriteElementString(name, "http://schemas.microsoft.com/powershell/2004/04", value);
    }

    private class ConsolidatedStringEqualityComparer : IEqualityComparer<ConsolidatedString>
    {
      bool IEqualityComparer<ConsolidatedString>.Equals(
        ConsolidatedString x,
        ConsolidatedString y)
      {
        return x.Key.Equals(y.Key, StringComparison.Ordinal);
      }

      int IEqualityComparer<ConsolidatedString>.GetHashCode(
        ConsolidatedString obj)
      {
        return obj.Key.GetHashCode();
      }
    }
  }
}
