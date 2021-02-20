// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.BaseWMIAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace System.Management.Automation
{
  internal abstract class BaseWMIAdapter : Adapter
  {
    private static HybridDictionary instanceMethodCacheTable = new HybridDictionary();

    protected override Collection<string> GetTypeNameHierarchy(object obj)
    {
      ManagementBaseObject managementBaseObject = obj as ManagementBaseObject;
      Collection<string> typeNameHierarchy = base.GetTypeNameHierarchy(obj);
      Collection<string> collection = new Collection<string>();
      StringBuilder stringBuilder = new StringBuilder(typeNameHierarchy[0]);
      stringBuilder.Append("#");
      stringBuilder.Append(managementBaseObject.SystemProperties["__NAMESPACE"].Value);
      stringBuilder.Append("\\");
      stringBuilder.Append(managementBaseObject.SystemProperties["__CLASS"].Value);
      Adapter.tracer.WriteLine("Adding first type {0}", (object) stringBuilder);
      collection.Add(stringBuilder.ToString());
      foreach (string str in typeNameHierarchy)
      {
        Adapter.tracer.WriteLine("Adding base type {0}", (object) str);
        collection.Add(str);
      }
      return collection;
    }

    protected override T GetMember<T>(object obj, string memberName)
    {
      Adapter.tracer.WriteLine("Getting member with name {0}", (object) memberName);
      if (!(obj is ManagementBaseObject wmiObject))
        return default (T);
      PSProperty property = this.DoGetProperty(wmiObject, memberName);
      if (typeof (T).IsAssignableFrom(typeof (PSProperty)) && property != null)
        return property as T;
      if (typeof (T).IsAssignableFrom(typeof (PSMethod)))
      {
        T managementObjectMethod = this.GetManagementObjectMethod<T>(wmiObject, memberName);
        if ((object) managementObjectMethod != null && property == null)
          return managementObjectMethod;
      }
      return default (T);
    }

    protected override PSMemberInfoInternalCollection<T> GetMembers<T>(
      object obj)
    {
      ManagementBaseObject wmiObject = (ManagementBaseObject) obj;
      PSMemberInfoInternalCollection<T> members = new PSMemberInfoInternalCollection<T>();
      this.AddAllProperties<T>(wmiObject, members);
      this.AddAllMethods<T>(wmiObject, members);
      return members;
    }

    protected override object MethodInvoke(PSMethod method, object[] arguments) => this.AuxillaryInvokeMethod(method.baseObject as ManagementObject, (BaseWMIAdapter.WMIMethodCacheEntry) method.adapterData, arguments);

    protected override Collection<string> MethodDefinitions(PSMethod method) => new Collection<string>()
    {
      ((BaseWMIAdapter.WMIMethodCacheEntry) method.adapterData).MethodDefinition
    };

    protected override bool PropertyIsSettable(PSProperty property)
    {
      ManagementBaseObject baseObject = property.baseObject as ManagementBaseObject;
      try
      {
        return (bool) BaseWMIAdapter.CreateClassFrmObject(baseObject).GetPropertyQualifierValue(property.Name, "Write");
      }
      catch (ManagementException ex)
      {
        Adapter.tracer.TraceException((Exception) ex);
        return true;
      }
      catch (UnauthorizedAccessException ex)
      {
        Adapter.tracer.TraceException((Exception) ex);
        return true;
      }
    }

    protected override bool PropertyIsGettable(PSProperty property) => true;

    protected override string PropertyType(PSProperty property)
    {
      PropertyData adapterData = property.adapterData as PropertyData;
      string embeddedObjectTypeName = BaseWMIAdapter.GetDotNetType(adapterData).ToString();
      if (adapterData.Type == CimType.Object)
      {
        embeddedObjectTypeName = BaseWMIAdapter.GetEmbeddedObjectTypeName(adapterData);
        if (adapterData.IsArray)
          embeddedObjectTypeName += "[]";
      }
      return embeddedObjectTypeName;
    }

    protected override object PropertyGet(PSProperty property) => (property.adapterData as PropertyData).Value;

    protected override void PropertySet(
      PSProperty property,
      object setValue,
      bool convertIfPossible)
    {
      if (!(property.baseObject is ManagementBaseObject))
        throw new SetValueInvocationException("CannotSetNonManagementObjectMsg", (Exception) null, "ExtendedTypeSystem", "CannotSetNonManagementObject", new object[3]
        {
          (object) property.Name,
          (object) property.baseObject.GetType().FullName,
          (object) typeof (ManagementBaseObject).FullName
        });
      PropertyData pData = this.PropertyIsSettable(property) ? property.adapterData as PropertyData : throw new SetValueException("ReadOnlyWMIProperty", (Exception) null, "ExtendedTypeSystem", "ReadOnlyProperty", new object[1]
      {
        (object) property.Name
      });
      if (convertIfPossible && setValue != null)
      {
        Type dotNetType = BaseWMIAdapter.GetDotNetType(pData);
        setValue = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, dotNetType, (IFormatProvider) CultureInfo.InvariantCulture);
      }
      pData.Value = setValue;
    }

    protected override string PropertyToString(PSProperty property)
    {
      StringBuilder stringBuilder = new StringBuilder();
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

    protected override AttributeCollection PropertyAttributes(PSProperty property) => (AttributeCollection) null;

    protected static CacheTable GetInstanceMethodTable(
      ManagementBaseObject wmiObject,
      bool staticBinding)
    {
      lock (BaseWMIAdapter.instanceMethodCacheTable)
      {
        string str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}#{1}", (object) wmiObject.ClassPath.Path, (object) staticBinding.ToString());
        CacheTable methodTable = (CacheTable) BaseWMIAdapter.instanceMethodCacheTable[(object) str];
        if (methodTable != null)
        {
          Adapter.tracer.WriteLine("Returning method information from internal cache", new object[0]);
          return methodTable;
        }
        Adapter.tracer.WriteLine("Method information not found in internal cache. Constructing one", new object[0]);
        try
        {
          methodTable = new CacheTable();
          if (!(wmiObject is ManagementClass mgmtClass))
            mgmtClass = BaseWMIAdapter.CreateClassFrmObject(wmiObject);
          BaseWMIAdapter.PopulateMethodTable(mgmtClass, methodTable, staticBinding);
          BaseWMIAdapter.instanceMethodCacheTable[(object) str] = (object) methodTable;
        }
        catch (ManagementException ex)
        {
          Adapter.tracer.TraceException((Exception) ex);
        }
        catch (UnauthorizedAccessException ex)
        {
          Adapter.tracer.TraceException((Exception) ex);
        }
        return methodTable;
      }
    }

    private static void PopulateMethodTable(
      ManagementClass mgmtClass,
      CacheTable methodTable,
      bool staticBinding)
    {
      MethodDataCollection methods = mgmtClass.Methods;
      if (methods == null)
        return;
      ManagementPath classPath = mgmtClass.ClassPath;
      foreach (MethodData methodData in methods)
      {
        if (BaseWMIAdapter.IsStaticMethod(methodData) == staticBinding)
        {
          string name = methodData.Name;
          BaseWMIAdapter.WMIMethodCacheEntry methodCacheEntry = new BaseWMIAdapter.WMIMethodCacheEntry(name, classPath.Path, methodData);
          methodTable.Add(name, (object) methodCacheEntry);
        }
      }
    }

    private static ManagementClass CreateClassFrmObject(
      ManagementBaseObject mgmtBaseObject)
    {
      if (!(mgmtBaseObject is ManagementClass managementClass))
      {
        managementClass = new ManagementClass(mgmtBaseObject.ClassPath);
        if (mgmtBaseObject is ManagementObject managementObject)
        {
          managementClass.Scope = managementObject.Scope;
          managementClass.Options = managementObject.Options;
        }
      }
      return managementClass;
    }

    protected static string GetEmbeddedObjectTypeName(PropertyData pData)
    {
      string str = typeof (object).FullName;
      if (pData == null)
        return str;
      try
      {
        str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}#{1}", (object) typeof (ManagementObject).FullName, (object) ((string) pData.Qualifiers["cimtype"].Value).Replace("object:", ""));
      }
      catch (ManagementException ex)
      {
        Adapter.tracer.TraceException((Exception) ex);
      }
      return str;
    }

    protected static Type GetDotNetType(PropertyData pData)
    {
      Adapter.tracer.WriteLine("Getting DotNet Type for CimType : {0}", (object) pData.Type);
      string fullName;
      switch (pData.Type)
      {
        case CimType.SInt16:
          fullName = typeof (short).FullName;
          break;
        case CimType.SInt32:
          fullName = typeof (int).FullName;
          break;
        case CimType.Real32:
          fullName = typeof (float).FullName;
          break;
        case CimType.Real64:
          fullName = typeof (double).FullName;
          break;
        case CimType.String:
          fullName = typeof (string).FullName;
          break;
        case CimType.Boolean:
          fullName = typeof (bool).FullName;
          break;
        case CimType.SInt8:
          fullName = typeof (sbyte).FullName;
          break;
        case CimType.UInt8:
          fullName = typeof (byte).FullName;
          break;
        case CimType.UInt16:
          fullName = typeof (ushort).FullName;
          break;
        case CimType.UInt32:
          fullName = typeof (uint).FullName;
          break;
        case CimType.SInt64:
          fullName = typeof (long).FullName;
          break;
        case CimType.UInt64:
          fullName = typeof (ulong).FullName;
          break;
        case CimType.DateTime:
          fullName = typeof (string).FullName;
          break;
        case CimType.Reference:
          fullName = typeof (string).FullName;
          break;
        case CimType.Char16:
          fullName = typeof (char).FullName;
          break;
        default:
          fullName = typeof (object).FullName;
          break;
      }
      if (pData.IsArray)
        fullName += "[]";
      return Type.GetType(fullName);
    }

    protected static bool IsStaticMethod(MethodData mdata)
    {
      try
      {
        QualifierData qualifier = mdata.Qualifiers["static"];
        if (qualifier == null)
          return false;
        bool result = false;
        LanguagePrimitives.TryConvertTo<bool>(qualifier.Value, out result);
        return result;
      }
      catch (ManagementException ex)
      {
        Adapter.tracer.TraceException((Exception) ex);
      }
      return false;
    }

    private object AuxillaryInvokeMethod(
      ManagementObject obj,
      BaseWMIAdapter.WMIMethodCacheEntry mdata,
      object[] arguments)
    {
      MethodInformation[] methods = new MethodInformation[1]
      {
        mdata.MethodInfoStructure
      };
      object[] newArguments;
      Adapter.GetBestMethodAndArguments(mdata.Name, methods, arguments, out newArguments);
      ParameterInformation[] parameters = mdata.MethodInfoStructure.parameters;
      Adapter.tracer.WriteLine("Parameters found {0}. Arguments supplied {0}", (object) parameters.Length, (object) newArguments.Length);
      ManagementBaseObject methodParameters = BaseWMIAdapter.CreateClassFrmObject((ManagementBaseObject) obj).GetMethodParameters(mdata.Name);
      for (int index = 0; index < parameters.Length; ++index)
      {
        BaseWMIAdapter.WMIParameterInformation parameterInformation = (BaseWMIAdapter.WMIParameterInformation) parameters[index];
        if (index < arguments.Length && arguments[index] == null)
          newArguments[index] = (object) null;
        methodParameters[parameterInformation.Name] = newArguments[index];
      }
      return this.InvokeManagementMethod(obj, mdata.Name, methodParameters);
    }

    internal static void UpdateParameters(
      ManagementBaseObject parameters,
      SortedList parametersList)
    {
      if (parameters == null)
        return;
      foreach (PropertyData property in parameters.Properties)
      {
        int num = -1;
        BaseWMIAdapter.WMIParameterInformation parameterInformation = new BaseWMIAdapter.WMIParameterInformation(property.Name, BaseWMIAdapter.GetDotNetType(property));
        try
        {
          num = (int) property.Qualifiers["ID"].Value;
        }
        catch (ManagementException ex)
        {
          Adapter.tracer.TraceException((Exception) ex);
        }
        if (num < 0)
          num = parametersList.Count;
        parametersList[(object) num] = (object) parameterInformation;
      }
    }

    internal static MethodInformation GetMethodInformation(MethodData mData)
    {
      SortedList parametersList = new SortedList();
      BaseWMIAdapter.UpdateParameters(mData.InParameters, parametersList);
      BaseWMIAdapter.WMIParameterInformation[] parameterInformationArray = new BaseWMIAdapter.WMIParameterInformation[parametersList.Count];
      if (parametersList.Count > 0)
        parametersList.Values.CopyTo((Array) parameterInformationArray, 0);
      return new MethodInformation(false, true, (ParameterInformation[]) parameterInformationArray);
    }

    internal static string GetMethodDefinition(MethodData mData)
    {
      SortedList parametersList = new SortedList();
      BaseWMIAdapter.UpdateParameters(mData.InParameters, parametersList);
      StringBuilder stringBuilder1 = new StringBuilder();
      if (parametersList.Count > 0)
      {
        foreach (BaseWMIAdapter.WMIParameterInformation parameterInformation in (IEnumerable) parametersList.Values)
        {
          string embeddedObjectTypeName = parameterInformation.parameterType.ToString();
          PropertyData property = mData.InParameters.Properties[parameterInformation.Name];
          if (property.Type == CimType.Object)
          {
            embeddedObjectTypeName = BaseWMIAdapter.GetEmbeddedObjectTypeName(property);
            if (property.IsArray)
              embeddedObjectTypeName += "[]";
          }
          stringBuilder1.Append(embeddedObjectTypeName);
          stringBuilder1.Append(" ");
          stringBuilder1.Append(parameterInformation.Name);
          stringBuilder1.Append(", ");
        }
      }
      if (stringBuilder1.Length > 2)
        stringBuilder1.Remove(stringBuilder1.Length - 2, 2);
      Adapter.tracer.WriteLine("Constructing method definition for method {0}", (object) mData.Name);
      StringBuilder stringBuilder2 = new StringBuilder();
      stringBuilder2.Append("System.Management.ManagementBaseObject ");
      stringBuilder2.Append(mData.Name);
      stringBuilder2.Append("(");
      stringBuilder2.Append(stringBuilder1.ToString());
      stringBuilder2.Append(")");
      string str = stringBuilder2.ToString();
      Adapter.tracer.WriteLine("Definition constructed: {0}", (object) str);
      return str;
    }

    protected abstract void AddAllProperties<T>(
      ManagementBaseObject wmiObject,
      PSMemberInfoInternalCollection<T> members)
      where T : PSMemberInfo;

    protected abstract void AddAllMethods<T>(
      ManagementBaseObject wmiObject,
      PSMemberInfoInternalCollection<T> members)
      where T : PSMemberInfo;

    protected abstract object InvokeManagementMethod(
      ManagementObject wmiObject,
      string methodName,
      ManagementBaseObject inParams);

    protected abstract T GetManagementObjectMethod<T>(
      ManagementBaseObject wmiObject,
      string methodName)
      where T : PSMemberInfo;

    protected abstract PSProperty DoGetProperty(
      ManagementBaseObject wmiObject,
      string propertyName);

    internal class WMIMethodCacheEntry
    {
      private string name;
      private string classPath;
      private MethodInformation methodInfoStructure;
      private string methodDefinition;

      public string Name => this.name;

      public string ClassPath => this.classPath;

      public MethodInformation MethodInfoStructure => this.methodInfoStructure;

      public string MethodDefinition => this.methodDefinition;

      internal WMIMethodCacheEntry(string n, string cPath, MethodData mData)
      {
        this.name = n;
        this.classPath = cPath;
        this.methodInfoStructure = BaseWMIAdapter.GetMethodInformation(mData);
        this.methodDefinition = BaseWMIAdapter.GetMethodDefinition(mData);
      }
    }

    internal class WMIParameterInformation : ParameterInformation
    {
      private string name;

      public string Name => this.name;

      public WMIParameterInformation(string name, Type ty)
        : base(ty, true, (object) null, false)
        => this.name = name;
    }
  }
}
