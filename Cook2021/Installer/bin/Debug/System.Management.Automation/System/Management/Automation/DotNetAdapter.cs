// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DotNetAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace System.Management.Automation
{
  internal class DotNetAdapter : Adapter
  {
    private const BindingFlags instanceBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
    private const BindingFlags staticBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
    private bool isStatic;
    private static HybridDictionary instancePropertyCacheTable = new HybridDictionary();
    private static HybridDictionary staticPropertyCacheTable = new HybridDictionary();
    private static HybridDictionary instanceMethodCacheTable = new HybridDictionary();
    private static HybridDictionary staticMethodCacheTable = new HybridDictionary();
    private static HybridDictionary instanceEventCacheTable = new HybridDictionary();
    private static HybridDictionary staticEventCacheTable = new HybridDictionary();

    internal DotNetAdapter()
    {
    }

    internal DotNetAdapter(bool isStatic) => this.isStatic = isStatic;

    private static bool SameSignature(MethodInfo method1, MethodInfo method2)
    {
      if (method1.GetGenericArguments().Length != method2.GetGenericArguments().Length)
        return false;
      ParameterInfo[] parameters1 = method1.GetParameters();
      ParameterInfo[] parameters2 = method2.GetParameters();
      if (parameters1.Length != parameters2.Length)
        return false;
      for (int index = 0; index < parameters1.Length; ++index)
      {
        if (!parameters1[index].ParameterType.Equals(parameters2[index].ParameterType) || parameters1[index].IsOut != parameters2[index].IsOut || parameters1[index].IsOptional != parameters2[index].IsOptional)
          return false;
      }
      return true;
    }

    private static void AddOverload(ArrayList previousMethodEntry, MethodInfo method)
    {
      bool flag = true;
      foreach (MethodInfo method1 in previousMethodEntry)
      {
        if (DotNetAdapter.SameSignature(method1, method))
        {
          flag = false;
          break;
        }
      }
      if (!flag)
        return;
      previousMethodEntry.Add((object) method);
    }

    private static void PopulateMethodReflectionTable(
      Type type,
      MethodInfo[] methods,
      CacheTable typeMethods)
    {
      foreach (MethodInfo method in methods)
      {
        if (method.DeclaringType.Equals(type))
        {
          string name = method.Name;
          ArrayList typeMethod = (ArrayList) typeMethods[name];
          if (typeMethod == null)
            typeMethods.Add(name, (object) new ArrayList()
            {
              (object) method
            });
          else
            DotNetAdapter.AddOverload(typeMethod, method);
        }
      }
      if (type.BaseType == null)
        return;
      DotNetAdapter.PopulateMethodReflectionTable(type.BaseType, methods, typeMethods);
    }

    private static void PopulateMethodReflectionTable(
      Type type,
      CacheTable typeMethods,
      BindingFlags bindingFlags)
    {
      MethodInfo[] methods = type.GetMethods(bindingFlags);
      DotNetAdapter.PopulateMethodReflectionTable(type, methods, typeMethods);
      for (int index = 0; index < typeMethods.memberCollection.Count; ++index)
        typeMethods.memberCollection[index] = (object) new DotNetAdapter.MethodCacheEntry((MethodInfo[]) ((ArrayList) typeMethods.memberCollection[index]).ToArray(typeof (MethodInfo)));
    }

    private static void PopulateEventReflectionTable(
      Type type,
      CacheTable typeEvents,
      BindingFlags bindingFlags)
    {
      foreach (EventInfo eventInfo in type.GetEvents(bindingFlags))
      {
        string name = eventInfo.Name;
        ArrayList typeEvent = (ArrayList) typeEvents[name];
        if (typeEvent == null)
          typeEvents.Add(name, (object) new ArrayList()
          {
            (object) eventInfo
          });
        else
          typeEvent.Add((object) eventInfo);
      }
      for (int index = 0; index < typeEvents.memberCollection.Count; ++index)
        typeEvents.memberCollection[index] = (object) new DotNetAdapter.EventCacheEntry((EventInfo[]) ((ArrayList) typeEvents.memberCollection[index]).ToArray(typeof (EventInfo)));
    }

    private static bool PropertyAlreadyPresent(ArrayList previousProperties, PropertyInfo property)
    {
      bool flag1 = false;
      ParameterInfo[] indexParameters1 = property.GetIndexParameters();
      int length = indexParameters1.Length;
      foreach (PropertyInfo previousProperty in previousProperties)
      {
        ParameterInfo[] indexParameters2 = previousProperty.GetIndexParameters();
        if (indexParameters2.Length == length)
        {
          bool flag2 = true;
          for (int index = 0; index < indexParameters2.Length; ++index)
          {
            if (!indexParameters2[index].ParameterType.Equals(indexParameters1[index].ParameterType))
            {
              flag2 = false;
              break;
            }
          }
          if (flag2)
          {
            flag1 = true;
            break;
          }
        }
      }
      return flag1;
    }

    private static void PopulatePropertyReflectionTable(
      Type type,
      CacheTable typeProperties,
      BindingFlags bindingFlags)
    {
      foreach (PropertyInfo property in type.GetProperties(bindingFlags))
      {
        string name = property.Name;
        ArrayList typeProperty = (ArrayList) typeProperties[name];
        if (typeProperty == null)
        {
          typeProperties.Add(name, (object) new ArrayList()
          {
            (object) property
          });
        }
        else
        {
          PropertyInfo propertyInfo = (PropertyInfo) typeProperty[0];
          if (!string.Equals(property.Name, propertyInfo.Name, StringComparison.Ordinal))
            throw new ExtendedTypeSystemException("NotACLSComplaintProperty", (Exception) null, "ExtendedTypeSystem", "NotAClsCompliantFieldProperty", new object[3]
            {
              (object) property.Name,
              (object) type.FullName,
              (object) propertyInfo.Name
            });
          if (!DotNetAdapter.PropertyAlreadyPresent(typeProperty, property))
            typeProperty.Add((object) property);
        }
      }
      for (int index = 0; index < typeProperties.memberCollection.Count; ++index)
      {
        ArrayList member = (ArrayList) typeProperties.memberCollection[index];
        PropertyInfo property = (PropertyInfo) member[0];
        typeProperties.memberCollection[index] = member.Count > 1 || property.GetIndexParameters().Length != 0 ? (object) new DotNetAdapter.ParameterizedPropertyCacheEntry((ArrayList) typeProperties.memberCollection[index]) : (object) new DotNetAdapter.PropertyCacheEntry(property);
      }
      foreach (FieldInfo field in type.GetFields(bindingFlags))
      {
        string name = field.Name;
        DotNetAdapter.PropertyCacheEntry typeProperty = (DotNetAdapter.PropertyCacheEntry) typeProperties[name];
        if (typeProperty == null)
          typeProperties.Add(name, (object) new DotNetAdapter.PropertyCacheEntry(field));
        else if (!string.Equals(typeProperty.member.Name, name))
          throw new ExtendedTypeSystemException("NotACLSComplaintField", (Exception) null, "ExtendedTypeSystem", "NotAClsCompliantFieldProperty", new object[3]
          {
            (object) name,
            (object) type.FullName,
            (object) typeProperty.member.Name
          });
      }
    }

    private static CacheTable GetStaticPropertyReflectionTable(object obj)
    {
      lock (DotNetAdapter.staticPropertyCacheTable)
      {
        CacheTable cacheTable = (CacheTable) DotNetAdapter.staticPropertyCacheTable[obj];
        if (cacheTable != null)
          return cacheTable;
        CacheTable typeProperties = new CacheTable();
        DotNetAdapter.PopulatePropertyReflectionTable((Type) obj, typeProperties, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        DotNetAdapter.staticPropertyCacheTable[obj] = (object) typeProperties;
        return typeProperties;
      }
    }

    private static CacheTable GetStaticMethodReflectionTable(object obj)
    {
      lock (DotNetAdapter.staticMethodCacheTable)
      {
        CacheTable cacheTable = (CacheTable) DotNetAdapter.staticMethodCacheTable[obj];
        if (cacheTable != null)
          return cacheTable;
        CacheTable typeMethods = new CacheTable();
        DotNetAdapter.PopulateMethodReflectionTable((Type) obj, typeMethods, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        DotNetAdapter.staticMethodCacheTable[obj] = (object) typeMethods;
        return typeMethods;
      }
    }

    private static CacheTable GetStaticEventReflectionTable(object obj)
    {
      lock (DotNetAdapter.staticEventCacheTable)
      {
        CacheTable cacheTable = (CacheTable) DotNetAdapter.staticEventCacheTable[obj];
        if (cacheTable != null)
          return cacheTable;
        CacheTable typeEvents = new CacheTable();
        DotNetAdapter.PopulateEventReflectionTable((Type) obj, typeEvents, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        DotNetAdapter.staticEventCacheTable[obj] = (object) typeEvents;
        return typeEvents;
      }
    }

    private static CacheTable GetInstancePropertyReflectionTable(object obj)
    {
      lock (DotNetAdapter.instancePropertyCacheTable)
      {
        Type type = obj.GetType();
        CacheTable cacheTable = (CacheTable) DotNetAdapter.instancePropertyCacheTable[(object) type];
        if (cacheTable != null)
          return cacheTable;
        CacheTable typeProperties = new CacheTable();
        DotNetAdapter.PopulatePropertyReflectionTable(type, typeProperties, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        DotNetAdapter.instancePropertyCacheTable[(object) type] = (object) typeProperties;
        return typeProperties;
      }
    }

    private static CacheTable GetInstanceMethodReflectionTable(object obj)
    {
      lock (DotNetAdapter.instanceMethodCacheTable)
      {
        Type type = obj.GetType();
        CacheTable cacheTable = (CacheTable) DotNetAdapter.instanceMethodCacheTable[(object) type];
        if (cacheTable != null)
          return cacheTable;
        CacheTable typeMethods = new CacheTable();
        DotNetAdapter.PopulateMethodReflectionTable(type, typeMethods, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        DotNetAdapter.instanceMethodCacheTable[(object) type] = (object) typeMethods;
        return typeMethods;
      }
    }

    private static CacheTable GetInstanceEventReflectionTable(object obj)
    {
      lock (DotNetAdapter.instanceEventCacheTable)
      {
        Type type = obj.GetType();
        CacheTable cacheTable = (CacheTable) DotNetAdapter.instanceEventCacheTable[(object) type];
        if (cacheTable != null)
          return cacheTable;
        CacheTable typeEvents = new CacheTable();
        DotNetAdapter.PopulateEventReflectionTable(type, typeEvents, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        DotNetAdapter.instanceEventCacheTable[(object) type] = (object) typeEvents;
        return typeEvents;
      }
    }

    internal static bool IsTypeParameterizedProperty(Type t) => t.Equals(typeof (PSMemberInfo)) || t.Equals(typeof (PSParameterizedProperty));

    internal T GetDotNetProperty<T>(object obj, string propertyName) where T : PSMemberInfo
    {
      bool flag1 = typeof (T).IsAssignableFrom(typeof (PSProperty));
      bool flag2 = DotNetAdapter.IsTypeParameterizedProperty(typeof (T));
      if (!flag1 && !flag2)
        return default (T);
      switch (!this.isStatic ? DotNetAdapter.GetInstancePropertyReflectionTable(obj)[propertyName] : DotNetAdapter.GetStaticPropertyReflectionTable(obj)[propertyName])
      {
        case null:
          return default (T);
        case DotNetAdapter.PropertyCacheEntry propertyCacheEntry when flag1:
          return new PSProperty(propertyCacheEntry.member.Name, (Adapter) this, obj, (object) propertyCacheEntry) as T;
        case DotNetAdapter.ParameterizedPropertyCacheEntry propertyCacheEntry when flag2:
          return new PSParameterizedProperty(propertyCacheEntry.propertyName, (Adapter) this, obj, (object) propertyCacheEntry) as T;
        default:
          return default (T);
      }
    }

    internal T GetDotNetMethod<T>(object obj, string methodName) where T : PSMemberInfo
    {
      if (!typeof (T).IsAssignableFrom(typeof (PSMethod)))
        return default (T);
      DotNetAdapter.MethodCacheEntry methodCacheEntry = !this.isStatic ? (DotNetAdapter.MethodCacheEntry) DotNetAdapter.GetInstanceMethodReflectionTable(obj)[methodName] : (DotNetAdapter.MethodCacheEntry) DotNetAdapter.GetStaticMethodReflectionTable(obj)[methodName];
      return methodCacheEntry == null ? default (T) : new PSMethod(methodCacheEntry[0].method.Name, (Adapter) this, obj, (object) methodCacheEntry) as T;
    }

    internal T GetDotNetEvent<T>(object obj, string eventName) where T : PSMemberInfo
    {
      if (!typeof (T).IsAssignableFrom(typeof (PSEvent)))
        return default (T);
      DotNetAdapter.EventCacheEntry eventCacheEntry = (DotNetAdapter.EventCacheEntry) (!this.isStatic ? DotNetAdapter.GetInstanceEventReflectionTable(obj) : DotNetAdapter.GetStaticEventReflectionTable(obj))[eventName];
      return eventCacheEntry == null ? default (T) : new PSEvent(eventCacheEntry.events[0]) as T;
    }

    internal void AddAllProperties<T>(
      object obj,
      PSMemberInfoInternalCollection<T> members,
      bool ignoreDuplicates)
      where T : PSMemberInfo
    {
      bool flag1 = typeof (T).IsAssignableFrom(typeof (PSProperty));
      bool flag2 = DotNetAdapter.IsTypeParameterizedProperty(typeof (T));
      if (!flag1 && !flag2)
        return;
      foreach (object member in (!this.isStatic ? DotNetAdapter.GetInstancePropertyReflectionTable(obj) : DotNetAdapter.GetStaticPropertyReflectionTable(obj)).memberCollection)
      {
        if (member is DotNetAdapter.PropertyCacheEntry propertyCacheEntry)
        {
          if (flag1 && (!ignoreDuplicates || (object) members[propertyCacheEntry.member.Name] == null))
            members.Add(new PSProperty(propertyCacheEntry.member.Name, (Adapter) this, obj, (object) propertyCacheEntry) as T);
        }
        else if (flag2)
        {
          DotNetAdapter.ParameterizedPropertyCacheEntry propertyCacheEntry = (DotNetAdapter.ParameterizedPropertyCacheEntry) member;
          if (!ignoreDuplicates || (object) members[propertyCacheEntry.propertyName] == null)
            members.Add(new PSParameterizedProperty(propertyCacheEntry.propertyName, (Adapter) this, obj, (object) propertyCacheEntry) as T);
        }
      }
    }

    internal void AddAllMethods<T>(
      object obj,
      PSMemberInfoInternalCollection<T> members,
      bool ignoreDuplicates)
      where T : PSMemberInfo
    {
      if (!typeof (T).IsAssignableFrom(typeof (PSMethod)))
        return;
      foreach (DotNetAdapter.MethodCacheEntry member in (!this.isStatic ? DotNetAdapter.GetInstanceMethodReflectionTable(obj) : DotNetAdapter.GetStaticMethodReflectionTable(obj)).memberCollection)
      {
        string name = member[0].method.Name;
        if (!ignoreDuplicates || (object) members[name] == null)
        {
          bool isSpecialName = member[0].method.IsSpecialName;
          members.Add(new PSMethod(name, (Adapter) this, obj, (object) member, isSpecialName) as T);
        }
      }
    }

    internal void AddAllEvents<T>(
      object obj,
      PSMemberInfoInternalCollection<T> members,
      bool ignoreDuplicates)
      where T : PSMemberInfo
    {
      if (!typeof (T).IsAssignableFrom(typeof (PSEvent)))
        return;
      foreach (DotNetAdapter.EventCacheEntry member in (!this.isStatic ? DotNetAdapter.GetInstanceEventReflectionTable(obj) : DotNetAdapter.GetStaticEventReflectionTable(obj)).memberCollection)
      {
        if (!ignoreDuplicates || (object) members[member.events[0].Name] == null)
          members.Add(new PSEvent(member.events[0]) as T);
      }
    }

    private bool PropertyIsStatic(PSProperty property) => property.adapterData is DotNetAdapter.PropertyCacheEntry adapterData && adapterData.isStatic;

    private bool ParameterizedPropertyIsStatic(PSParameterizedProperty property) => property.adapterData is DotNetAdapter.ParameterizedPropertyCacheEntry adapterData && adapterData.isStatic;

    protected override T GetMember<T>(object obj, string memberName)
    {
      T obj1 = default (T);
      T dotNetProperty = this.GetDotNetProperty<T>(obj, memberName);
      return (object) dotNetProperty != null ? dotNetProperty : this.GetDotNetMethod<T>(obj, memberName);
    }

    protected override PSMemberInfoInternalCollection<T> GetMembers<T>(
      object obj)
    {
      PSMemberInfoInternalCollection<T> members = new PSMemberInfoInternalCollection<T>();
      this.AddAllProperties<T>(obj, members, false);
      this.AddAllMethods<T>(obj, members, false);
      this.AddAllEvents<T>(obj, members, false);
      return members;
    }

    protected override AttributeCollection PropertyAttributes(PSProperty property) => ((DotNetAdapter.PropertyCacheEntry) property.adapterData).Attributes;

    protected override string PropertyToString(PSProperty property)
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (this.PropertyIsStatic(property))
        stringBuilder.Append("static ");
      stringBuilder.Append(this.PropertyType(property));
      stringBuilder.Append(" ");
      stringBuilder.Append(property.Name);
      stringBuilder.Append(" {");
      if (this.PropertyIsGettable(property))
        stringBuilder.Append("get;");
      if (this.PropertyIsSettable(property))
        stringBuilder.Append("set;");
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }

    protected override object PropertyGet(PSProperty property)
    {
      DotNetAdapter.PropertyCacheEntry adapterData = (DotNetAdapter.PropertyCacheEntry) property.adapterData;
      if (adapterData.member is PropertyInfo member)
      {
        if (adapterData.writeOnly)
          throw new GetValueException("WriteOnlyProperty", (Exception) null, "ExtendedTypeSystem", "WriteOnlyProperty", new object[1]
          {
            (object) member.Name
          });
        return adapterData.useReflection ? member.GetValue(property.baseObject, (object[]) null) : adapterData.getterDelegate(property.baseObject);
      }
      FieldInfo member1 = adapterData.member as FieldInfo;
      return adapterData.useReflection ? member1.GetValue(property.baseObject) : adapterData.getterDelegate(property.baseObject);
    }

    protected override void PropertySet(
      PSProperty property,
      object setValue,
      bool convertIfPossible)
    {
      DotNetAdapter.PropertyCacheEntry adapterData = (DotNetAdapter.PropertyCacheEntry) property.adapterData;
      if (adapterData.readOnly)
        throw new SetValueException("ReadOnlyProperty", (Exception) null, "ExtendedTypeSystem", "ReadOnlyProperty", new object[1]
        {
          (object) adapterData.member.Name
        });
      if (adapterData.member is PropertyInfo member)
      {
        if (convertIfPossible)
          setValue = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, member.PropertyType, (IFormatProvider) CultureInfo.InvariantCulture);
        if (adapterData.useReflection)
          member.SetValue(property.baseObject, setValue, (object[]) null);
        else
          adapterData.setterDelegate(property.baseObject, setValue);
      }
      else
      {
        FieldInfo member = adapterData.member as FieldInfo;
        if (convertIfPossible)
          setValue = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, member.FieldType, (IFormatProvider) CultureInfo.InvariantCulture);
        if (adapterData.useReflection)
          member.SetValue(property.baseObject, setValue);
        else
          adapterData.setterDelegate(property.baseObject, setValue);
      }
    }

    protected override bool PropertyIsSettable(PSProperty property) => !((DotNetAdapter.PropertyCacheEntry) property.adapterData).readOnly;

    protected override bool PropertyIsGettable(PSProperty property) => !((DotNetAdapter.PropertyCacheEntry) property.adapterData).writeOnly;

    protected override string PropertyType(PSProperty property) => ((DotNetAdapter.PropertyCacheEntry) property.adapterData).propertyType.FullName;

    internal static object AuxiliaryConstructorInvoke(
      MethodInformation methodInformation,
      object[] arguments,
      object[] originalArguments)
    {
      object obj;
      try
      {
        obj = ((ConstructorInfo) methodInformation.method).Invoke(arguments);
      }
      catch (TargetInvocationException ex)
      {
        Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
        throw new MethodInvocationException("DotNetconstructorTargetInvocation", innerException, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
        {
          (object) ".ctor",
          (object) arguments.Length,
          (object) innerException.Message
        });
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw new MethodInvocationException("DotNetconstructorException", ex, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
        {
          (object) ".ctor",
          (object) arguments.Length,
          (object) ex.Message
        });
      }
      Adapter.SetReferences(arguments, methodInformation, originalArguments);
      return obj;
    }

    internal static object AuxiliaryMethodInvoke(
      object target,
      object[] arguments,
      MethodInformation methodInformation,
      object[] originalArguments)
    {
      object obj;
      try
      {
        obj = methodInformation.Invoke(target, arguments);
      }
      catch (TargetInvocationException ex)
      {
        if (ex.InnerException is FlowControlException || ex.InnerException is ScriptCallDepthException)
          throw ex.InnerException;
        if (ex.InnerException is ParameterBindingException)
          throw ex.InnerException;
        Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
        throw new MethodInvocationException("DotNetMethodTargetInvocation", innerException, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
        {
          (object) methodInformation.method.Name,
          (object) arguments.Length,
          (object) innerException.Message
        });
      }
      catch (ParameterBindingException ex)
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
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (methodInformation.method.DeclaringType == typeof (SteppablePipeline) && (methodInformation.method.Name.Equals("Begin") || methodInformation.method.Name.Equals("Process") || methodInformation.method.Name.Equals("End")))
          throw;
        else
          throw new MethodInvocationException("DotNetMethodException", ex, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
          {
            (object) methodInformation.method.Name,
            (object) arguments.Length,
            (object) ex.Message
          });
      }
      Adapter.SetReferences(arguments, methodInformation, originalArguments);
      return methodInformation.method is MethodInfo method && method.ReturnType != typeof (void) ? obj : (object) AutomationNull.Value;
    }

    internal static MethodInformation[] GetMethodInformationArray(
      MethodBase[] methods)
    {
      MethodInformation[] methodInformationArray = new MethodInformation[methods.Length];
      for (int index = 0; index < methods.Length; ++index)
        methodInformationArray[index] = new MethodInformation((string) null, methods[index], 0);
      return methodInformationArray;
    }

    internal static object MethodInvokeDotNet(
      string methodName,
      object target,
      MethodInformation[] methodInformation,
      object[] arguments)
    {
      object[] newArguments;
      MethodInformation methodAndArguments = Adapter.GetBestMethodAndArguments(methodName, methodInformation, arguments, out newArguments);
      string methodDefinition = methodAndArguments.methodDefinition;
      ScriptTrace.Trace(1, "TraceMethodCall", (object) methodDefinition);
      PSObject.memberResolution.WriteLine("Calling Method: {0}", (object) methodDefinition);
      CallsiteCacheEntryFlags flags = CallsiteCacheEntryFlags.None;
      if (methodAndArguments.method.IsStatic)
        flags = CallsiteCacheEntryFlags.Static;
      Adapter.CacheMethod(methodAndArguments, target, methodName, arguments, flags);
      return DotNetAdapter.AuxiliaryMethodInvoke(target, newArguments, methodAndArguments, arguments);
    }

    internal static object ConstructorInvokeDotNet(
      Type type,
      ConstructorInfo[] constructors,
      object[] arguments)
    {
      MethodInformation cachedMethod = Adapter.FindCachedMethod(type, ".ctor", arguments, CallsiteCacheEntryFlags.Constructor);
      MethodInformation[] methods;
      if (cachedMethod != null)
        methods = new MethodInformation[1]{ cachedMethod };
      else
        methods = DotNetAdapter.GetMethodInformationArray((MethodBase[]) constructors);
      object[] newArguments;
      MethodInformation methodAndArguments = Adapter.GetBestMethodAndArguments(type.Name, methods, arguments, out newArguments);
      if ((PSObject.memberResolution.Options & PSTraceSourceOptions.WriteLine) != PSTraceSourceOptions.None)
        PSObject.memberResolution.WriteLine("Calling Constructor: {0}", (object) DotNetAdapter.GetMethodInfoOverloadDefinition((string) null, methodAndArguments.method, 0));
      if (cachedMethod == null)
        Adapter.CacheMethod(methodAndArguments, (object) type, ".ctor", arguments, CallsiteCacheEntryFlags.Constructor);
      return DotNetAdapter.AuxiliaryConstructorInvoke(methodAndArguments, newArguments, arguments);
    }

    internal static void ParameterizedPropertyInvokeSet(
      string propertyName,
      object target,
      object valuetoSet,
      MethodInformation[] methodInformation,
      object[] arguments,
      bool addToCache)
    {
      object[] newArguments;
      MethodInformation methodAndArguments = Adapter.GetBestMethodAndArguments(propertyName, methodInformation, arguments, out newArguments);
      PSObject.memberResolution.WriteLine("Calling Set Method: {0}", (object) methodAndArguments.methodDefinition);
      ParameterInfo[] parameters = methodAndArguments.method.GetParameters();
      Type parameterType = parameters[parameters.Length - 1].ParameterType;
      object obj;
      try
      {
        obj = Adapter.PropertySetAndMethodArgumentConvertTo(valuetoSet, parameterType, (IFormatProvider) CultureInfo.InvariantCulture);
      }
      catch (InvalidCastException ex)
      {
        throw new MethodException("PropertySetterConversionInvalidCastArgument", (Exception) ex, "ExtendedTypeSystem", "MethodArgumentConversionException", new object[5]
        {
          (object) (arguments.Length - 1),
          valuetoSet,
          (object) propertyName,
          (object) parameterType,
          (object) ex.Message
        });
      }
      object[] arguments1 = new object[newArguments.Length + 1];
      for (int index = 0; index < newArguments.Length; ++index)
        arguments1[index] = newArguments[index];
      arguments1[newArguments.Length] = obj;
      if (addToCache)
      {
        CallsiteCacheEntryFlags flags = CallsiteCacheEntryFlags.ParameterizedSetter;
        if (methodAndArguments.method.IsStatic)
          flags |= CallsiteCacheEntryFlags.Static;
        Adapter.CacheMethod(methodAndArguments, target, propertyName, arguments, flags);
      }
      DotNetAdapter.AuxiliaryMethodInvoke(target, arguments1, methodAndArguments, arguments);
    }

    internal static string GetMethodInfoOverloadDefinition(
      string memberName,
      MethodBase methodEntry,
      int parametersToIgnore)
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (methodEntry.IsStatic)
        stringBuilder.Append("static ");
      if (methodEntry is MethodInfo methodInfo)
      {
        stringBuilder.Append(ToStringCodeMethods.Type(methodInfo.ReturnType));
        stringBuilder.Append(" ");
      }
      stringBuilder.Append(memberName == null ? methodEntry.Name : memberName);
      if (methodEntry.IsGenericMethodDefinition)
      {
        stringBuilder.Append("[");
        bool flag = true;
        foreach (Type genericArgument in methodEntry.GetGenericArguments())
        {
          if (!flag)
            stringBuilder.Append(", ");
          stringBuilder.Append(ToStringCodeMethods.Type(genericArgument));
          flag = false;
        }
        stringBuilder.Append("]");
      }
      stringBuilder.Append("(");
      ParameterInfo[] parameters = methodEntry.GetParameters();
      int num = parameters.Length - parametersToIgnore;
      if (num > 0)
      {
        for (int index = 0; index < num; ++index)
        {
          ParameterInfo parameterInfo = parameters[index];
          if (parameterInfo.ParameterType.IsArray && index == num - 1 && parameterInfo.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length != 0)
            stringBuilder.Append("Params ");
          stringBuilder.Append(ToStringCodeMethods.Type(parameterInfo.ParameterType));
          stringBuilder.Append(" ");
          stringBuilder.Append(parameterInfo.Name);
          stringBuilder.Append(", ");
        }
        stringBuilder.Remove(stringBuilder.Length - 2, 2);
      }
      stringBuilder.Append(")");
      return stringBuilder.ToString();
    }

    protected override object MethodInvoke(PSMethod method, object[] arguments)
    {
      DotNetAdapter.MethodCacheEntry adapterData = (DotNetAdapter.MethodCacheEntry) method.adapterData;
      return DotNetAdapter.MethodInvokeDotNet(method.Name, method.baseObject, adapterData.methodInformationStructures, arguments);
    }

    protected override Collection<string> MethodDefinitions(PSMethod method)
    {
      DotNetAdapter.MethodCacheEntry adapterData = (DotNetAdapter.MethodCacheEntry) method.adapterData;
      Collection<string> collection = new Collection<string>();
      foreach (MethodInformation informationStructure in adapterData.methodInformationStructures)
        collection.Add(informationStructure.methodDefinition);
      return collection;
    }

    protected override string ParameterizedPropertyType(PSParameterizedProperty property) => ((DotNetAdapter.ParameterizedPropertyCacheEntry) property.adapterData).propertyType.FullName;

    protected override bool ParameterizedPropertyIsSettable(PSParameterizedProperty property) => !((DotNetAdapter.ParameterizedPropertyCacheEntry) property.adapterData).readOnly;

    protected override bool ParameterizedPropertyIsGettable(PSParameterizedProperty property) => !((DotNetAdapter.ParameterizedPropertyCacheEntry) property.adapterData).writeOnly;

    protected override object ParameterizedPropertyGet(
      PSParameterizedProperty property,
      object[] arguments)
    {
      DotNetAdapter.ParameterizedPropertyCacheEntry adapterData = (DotNetAdapter.ParameterizedPropertyCacheEntry) property.adapterData;
      return DotNetAdapter.MethodInvokeDotNet(property.Name, property.baseObject, adapterData.getterInformation, arguments);
    }

    protected override void ParameterizedPropertySet(
      PSParameterizedProperty property,
      object setValue,
      object[] arguments)
    {
      DotNetAdapter.ParameterizedPropertyCacheEntry adapterData = (DotNetAdapter.ParameterizedPropertyCacheEntry) property.adapterData;
      DotNetAdapter.ParameterizedPropertyInvokeSet(adapterData.propertyName, property.baseObject, setValue, adapterData.setterInformation, arguments, true);
    }

    protected override Collection<string> ParameterizedPropertyDefinitions(
      PSParameterizedProperty property)
    {
      DotNetAdapter.ParameterizedPropertyCacheEntry adapterData = (DotNetAdapter.ParameterizedPropertyCacheEntry) property.adapterData;
      Collection<string> collection = new Collection<string>();
      foreach (string str in adapterData.propertyDefinition)
        collection.Add(str);
      return collection;
    }

    protected override string ParameterizedPropertyToString(PSParameterizedProperty property)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string propertyDefinition in this.ParameterizedPropertyDefinitions(property))
      {
        stringBuilder.Append(propertyDefinition);
        stringBuilder.Append(", ");
      }
      stringBuilder.Remove(stringBuilder.Length - 2, 2);
      return stringBuilder.ToString();
    }

    internal class MethodCacheEntry
    {
      internal MethodInformation[] methodInformationStructures;

      internal MethodCacheEntry(MethodInfo[] methods) => this.methodInformationStructures = DotNetAdapter.GetMethodInformationArray((MethodBase[]) methods);

      internal MethodInformation this[int i] => this.methodInformationStructures[i];
    }

    internal class EventCacheEntry
    {
      internal EventInfo[] events;

      internal EventCacheEntry(EventInfo[] events) => this.events = events;
    }

    internal class ParameterizedPropertyCacheEntry
    {
      internal MethodInformation[] getterInformation;
      internal MethodInformation[] setterInformation;
      internal string propertyName;
      internal bool readOnly;
      internal bool isStatic;
      internal bool writeOnly;
      internal Type propertyType;
      internal string[] propertyDefinition;

      internal ParameterizedPropertyCacheEntry(ArrayList properties)
      {
        PropertyInfo property1 = (PropertyInfo) properties[0];
        this.propertyName = property1.Name;
        this.propertyType = property1.PropertyType;
        List<MethodInfo> methodInfoList1 = new List<MethodInfo>();
        List<MethodInfo> methodInfoList2 = new List<MethodInfo>();
        List<string> stringList = new List<string>();
        foreach (PropertyInfo property2 in properties)
        {
          if (!property2.PropertyType.Equals(this.propertyType))
            this.propertyType = typeof (object);
          MethodInfo getMethod = property2.GetGetMethod();
          StringBuilder stringBuilder1 = new StringBuilder();
          StringBuilder stringBuilder2 = new StringBuilder();
          if (getMethod != null)
          {
            stringBuilder2.Append("get;");
            stringBuilder1.Append(DotNetAdapter.GetMethodInfoOverloadDefinition(this.propertyName, (MethodBase) getMethod, 0));
            methodInfoList1.Add(getMethod);
          }
          MethodInfo setMethod = property2.GetSetMethod();
          if (setMethod != null)
          {
            stringBuilder2.Append("set;");
            if (stringBuilder1.Length == 0)
              stringBuilder1.Append(DotNetAdapter.GetMethodInfoOverloadDefinition(this.propertyName, (MethodBase) setMethod, 1));
            methodInfoList2.Add(setMethod);
          }
          stringBuilder1.Append(" {");
          stringBuilder1.Append((object) stringBuilder2);
          stringBuilder1.Append("}");
          stringList.Add(stringBuilder1.ToString());
        }
        this.propertyDefinition = stringList.ToArray();
        this.isStatic = methodInfoList1.Count == 0 ? methodInfoList2[0].IsStatic : methodInfoList1[0].IsStatic;
        this.writeOnly = methodInfoList1.Count == 0;
        this.readOnly = methodInfoList2.Count == 0;
        this.getterInformation = new MethodInformation[methodInfoList1.Count];
        for (int index = 0; index < methodInfoList1.Count; ++index)
          this.getterInformation[index] = new MethodInformation(this.propertyName, (MethodBase) methodInfoList1[index], 0);
        this.setterInformation = new MethodInformation[methodInfoList2.Count];
        for (int index = 0; index < methodInfoList2.Count; ++index)
          this.setterInformation[index] = new MethodInformation(this.propertyName, (MethodBase) methodInfoList2[index], 1);
      }
    }

    internal class PropertyCacheEntry
    {
      internal MemberInfo member;
      internal DotNetAdapter.PropertyCacheEntry.GetterDelegate getterDelegate;
      internal DotNetAdapter.PropertyCacheEntry.SetterDelegate setterDelegate;
      internal bool useReflection;
      internal bool readOnly;
      internal bool writeOnly;
      internal bool isStatic;
      internal Type propertyType;
      private AttributeCollection attributes;

      internal static DotNetAdapter.PropertyCacheEntry.GetterDelegate GetFieldGetter(
        FieldInfo field)
      {
        DynamicMethod dynamicMethod = new DynamicMethod("getter", typeof (object), new Type[1]
        {
          typeof (object)
        }, typeof (Adapter).Module, true);
        ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
        if (!field.IsStatic)
        {
          ilGenerator.Emit(OpCodes.Ldarg_0);
          ilGenerator.Emit(OpCodes.Ldfld, field);
        }
        else
          ilGenerator.Emit(OpCodes.Ldsfld, field);
        if (field.FieldType.IsValueType)
          ilGenerator.Emit(OpCodes.Box, field.FieldType);
        else if (field.FieldType.IsPointer)
        {
          MethodInfo method1 = typeof (Pointer).GetMethod("Box");
          MethodInfo method2 = typeof (Type).GetMethod("GetTypeFromHandle");
          ilGenerator.Emit(OpCodes.Ldtoken, field.FieldType);
          ilGenerator.EmitCall(OpCodes.Call, method2, (Type[]) null);
          ilGenerator.EmitCall(OpCodes.Call, method1, (Type[]) null);
        }
        ilGenerator.Emit(OpCodes.Ret);
        return (DotNetAdapter.PropertyCacheEntry.GetterDelegate) dynamicMethod.CreateDelegate(typeof (DotNetAdapter.PropertyCacheEntry.GetterDelegate));
      }

      internal static DotNetAdapter.PropertyCacheEntry.SetterDelegate GetFieldSetter(
        FieldInfo field)
      {
        DynamicMethod dynamicMethod = new DynamicMethod("setter", typeof (void), new Type[2]
        {
          typeof (object),
          typeof (object)
        }, typeof (Adapter).Module, true);
        ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
        if (!field.IsStatic)
          ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_1);
        if (field.FieldType.IsValueType)
        {
          ilGenerator.Emit(OpCodes.Unbox, field.FieldType);
          ilGenerator.Emit(OpCodes.Ldobj, field.FieldType);
        }
        if (field.IsStatic)
          ilGenerator.Emit(OpCodes.Stsfld, field);
        else
          ilGenerator.Emit(OpCodes.Stfld, field);
        ilGenerator.Emit(OpCodes.Ret);
        return (DotNetAdapter.PropertyCacheEntry.SetterDelegate) dynamicMethod.CreateDelegate(typeof (DotNetAdapter.PropertyCacheEntry.SetterDelegate));
      }

      internal static DotNetAdapter.PropertyCacheEntry.GetterDelegate GetPropertyGetter(
        PropertyInfo property,
        MethodInfo getterMethodInfo)
      {
        DynamicMethod dynamicMethod = new DynamicMethod("getter", typeof (object), new Type[1]
        {
          typeof (object)
        }, typeof (Adapter).Module, true);
        ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
        if (!getterMethodInfo.IsStatic)
          ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.EmitCall(OpCodes.Call, getterMethodInfo, (Type[]) null);
        if (property.PropertyType.IsValueType)
          ilGenerator.Emit(OpCodes.Box, property.PropertyType);
        else if (property.PropertyType.IsPointer)
        {
          MethodInfo method1 = typeof (Pointer).GetMethod("Box");
          MethodInfo method2 = typeof (Type).GetMethod("GetTypeFromHandle");
          ilGenerator.Emit(OpCodes.Ldtoken, property.PropertyType);
          ilGenerator.EmitCall(OpCodes.Call, method2, (Type[]) null);
          ilGenerator.EmitCall(OpCodes.Call, method1, (Type[]) null);
        }
        ilGenerator.Emit(OpCodes.Ret);
        return (DotNetAdapter.PropertyCacheEntry.GetterDelegate) dynamicMethod.CreateDelegate(typeof (DotNetAdapter.PropertyCacheEntry.GetterDelegate));
      }

      internal static DotNetAdapter.PropertyCacheEntry.SetterDelegate GetPropertySetter(
        PropertyInfo property,
        MethodInfo setterMethodInfo)
      {
        DynamicMethod dynamicMethod = new DynamicMethod("setter", typeof (void), new Type[2]
        {
          typeof (object),
          typeof (object)
        }, typeof (Adapter).Module, true);
        ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
        if (!setterMethodInfo.IsStatic)
          ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_1);
        if (property.PropertyType.IsValueType)
        {
          ilGenerator.Emit(OpCodes.Unbox, property.PropertyType);
          ilGenerator.Emit(OpCodes.Ldobj, property.PropertyType);
        }
        ilGenerator.EmitCall(OpCodes.Call, setterMethodInfo, (Type[]) null);
        ilGenerator.Emit(OpCodes.Ret);
        return (DotNetAdapter.PropertyCacheEntry.SetterDelegate) dynamicMethod.CreateDelegate(typeof (DotNetAdapter.PropertyCacheEntry.SetterDelegate));
      }

      internal PropertyCacheEntry(PropertyInfo property)
      {
        this.member = (MemberInfo) property;
        this.propertyType = property.PropertyType;
        if (property.DeclaringType.IsValueType || property.PropertyType.IsGenericType || (property.DeclaringType.IsGenericType || property.DeclaringType.IsCOMObject) || property.PropertyType.IsCOMObject)
        {
          this.readOnly = property.GetSetMethod() == null;
          this.writeOnly = property.GetGetMethod() == null;
          this.useReflection = true;
        }
        else
        {
          MethodInfo getMethod = property.GetGetMethod();
          if (getMethod != null)
          {
            this.isStatic = getMethod.IsStatic;
            this.getterDelegate = DotNetAdapter.PropertyCacheEntry.GetPropertyGetter(property, getMethod);
          }
          else
            this.writeOnly = true;
          MethodInfo setMethod = property.GetSetMethod();
          if (setMethod != null)
          {
            this.isStatic = setMethod.IsStatic;
            this.setterDelegate = DotNetAdapter.PropertyCacheEntry.GetPropertySetter(property, setMethod);
          }
          else
            this.readOnly = true;
        }
      }

      internal PropertyCacheEntry(FieldInfo field)
      {
        this.member = (MemberInfo) field;
        this.isStatic = field.IsStatic;
        this.propertyType = field.FieldType;
        if (field.IsLiteral || field.IsInitOnly)
          this.readOnly = true;
        if (field.IsLiteral || field.DeclaringType.IsValueType || (field.FieldType.IsGenericType || field.DeclaringType.IsGenericType))
        {
          this.useReflection = true;
        }
        else
        {
          this.getterDelegate = DotNetAdapter.PropertyCacheEntry.GetFieldGetter(field);
          this.setterDelegate = DotNetAdapter.PropertyCacheEntry.GetFieldSetter(field);
        }
      }

      internal AttributeCollection Attributes
      {
        get
        {
          if (this.attributes == null)
          {
            object[] customAttributes = this.member.GetCustomAttributes(true);
            Attribute[] attributeArray = new Attribute[customAttributes.Length];
            for (int index = 0; index < customAttributes.Length; ++index)
              attributeArray[index] = (Attribute) customAttributes[index];
            this.attributes = new AttributeCollection(attributeArray);
          }
          return this.attributes;
        }
      }

      internal delegate object GetterDelegate(object instance);

      internal delegate void SetterDelegate(object instance, object setValue);
    }
  }
}
