// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DirectoryEntryAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Management.Automation
{
  internal class DirectoryEntryAdapter : DotNetAdapter
  {
    private static DotNetAdapter dotNetAdapter = new DotNetAdapter();

    protected override Collection<string> GetTypeNameHierarchy(object obj)
    {
      Collection<string> collection = new Collection<string>();
      for (Type type = obj.GetType(); type != null; type = type.BaseType)
        collection.Add(type.FullName);
      return collection;
    }

    protected override T GetMember<T>(object obj, string memberName)
    {
      DirectoryEntry directoryEntry = (DirectoryEntry) obj;
      PropertyValueCollection property = directoryEntry.Properties[memberName];
      object adapterData = (object) property;
      PSProperty psProperty;
      try
      {
        object obj1 = directoryEntry.InvokeGet(memberName);
        if (property == null || property.Value == null && obj1 != null)
          adapterData = obj1;
        psProperty = new PSProperty(property.PropertyName, (Adapter) this, obj, adapterData);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        psProperty = (PSProperty) null;
      }
      if (adapterData == null)
        psProperty = (PSProperty) null;
      if (typeof (T).IsAssignableFrom(typeof (PSProperty)) && psProperty != null)
        return psProperty as T;
      return typeof (T).IsAssignableFrom(typeof (PSMethod)) && psProperty == null && (object) this.GetDotNetProperty<T>(obj, memberName) == null ? new PSMethod(memberName, (Adapter) this, obj, (object) null) as T : default (T);
    }

    protected override PSMemberInfoInternalCollection<T> GetMembers<T>(
      object obj)
    {
      DirectoryEntry directoryEntry = (DirectoryEntry) obj;
      PSMemberInfoInternalCollection<T> internalCollection = new PSMemberInfoInternalCollection<T>();
      if (directoryEntry.Properties == null || directoryEntry.Properties.PropertyNames == null)
        return (PSMemberInfoInternalCollection<T>) null;
      int num = 0;
      try
      {
        num = directoryEntry.Properties.PropertyNames.Count;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      if (num > 0)
      {
        foreach (PropertyValueCollection property in directoryEntry.Properties)
          internalCollection.Add(new PSProperty(property.PropertyName, (Adapter) this, obj, (object) property) as T);
      }
      return internalCollection;
    }

    protected override object PropertyGet(PSProperty property) => property.adapterData;

    protected override void PropertySet(
      PSProperty property,
      object setValue,
      bool convertIfPossible)
    {
      if (property.adapterData is PropertyValueCollection adapterData)
      {
        try
        {
          adapterData.Clear();
        }
        catch (COMException ex)
        {
          if (ex.ErrorCode == -2147467259)
          {
            if (setValue != null)
              goto label_5;
          }
          throw;
        }
label_5:
        IEnumerable enumerable = LanguagePrimitives.GetEnumerable(setValue);
        if (enumerable == null)
        {
          adapterData.Add(setValue);
        }
        else
        {
          foreach (object obj in enumerable)
            adapterData.Add(obj);
        }
      }
      else
      {
        DirectoryEntry baseObject = (DirectoryEntry) property.baseObject;
        ArrayList arrayList = new ArrayList();
        IEnumerable enumerable = LanguagePrimitives.GetEnumerable(setValue);
        if (enumerable == null)
        {
          arrayList.Add(setValue);
        }
        else
        {
          foreach (object obj in enumerable)
            arrayList.Add(obj);
        }
        baseObject.InvokeSet(property.name, arrayList.ToArray());
      }
    }

    protected override bool PropertyIsSettable(PSProperty property) => true;

    protected override bool PropertyIsGettable(PSProperty property) => true;

    protected override string PropertyType(PSProperty property)
    {
      object obj = (object) null;
      try
      {
        obj = this.BasePropertyGet(property);
      }
      catch (GetValueException ex)
      {
        Adapter.tracer.TraceException((Exception) ex);
      }
      if (obj == null)
        return typeof (object).FullName;
      return obj.GetType().FullName;
    }

    protected override string PropertyToString(PSProperty property) => base.PropertyToString(property);

    protected override AttributeCollection PropertyAttributes(PSProperty property) => base.PropertyAttributes(property);

    protected override object MethodInvoke(PSMethod method, object[] arguments)
    {
      ParameterInformation[] arguments1 = new ParameterInformation[arguments.Length];
      for (int index = 0; index < arguments.Length; ++index)
        arguments1[index] = new ParameterInformation(typeof (object), false, (object) null, false);
      MethodInformation[] methods = new MethodInformation[1]
      {
        new MethodInformation(false, false, arguments1)
      };
      object[] newArguments;
      Adapter.GetBestMethodAndArguments(method.Name, methods, arguments, out newArguments);
      DirectoryEntry baseObject = (DirectoryEntry) method.baseObject;
      Exception exception;
      try
      {
        return baseObject.Invoke(method.Name, newArguments);
      }
      catch (DirectoryServicesCOMException ex)
      {
        exception = (Exception) ex;
      }
      catch (TargetInvocationException ex)
      {
        exception = (Exception) ex;
      }
      catch (COMException ex)
      {
        exception = (Exception) ex;
      }
      return (DirectoryEntryAdapter.dotNetAdapter.GetDotNetMethod<PSMethod>(method.baseObject, method.name) ?? throw exception).Invoke(arguments);
    }

    protected override Collection<string> MethodDefinitions(PSMethod method) => base.MethodDefinitions(method);

    protected override string MethodToString(PSMethod method)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string methodDefinition in this.MethodDefinitions(method))
      {
        stringBuilder.Append(methodDefinition);
        stringBuilder.Append(", ");
      }
      stringBuilder.Remove(stringBuilder.Length - 2, 2);
      return stringBuilder.ToString();
    }
  }
}
