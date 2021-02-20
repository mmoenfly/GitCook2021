// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ManagementObjectAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class ManagementObjectAdapter : ManagementClassApdapter
  {
    protected override void AddAllProperties<T>(
      ManagementBaseObject wmiObject,
      PSMemberInfoInternalCollection<T> members)
    {
      base.AddAllProperties<T>(wmiObject, members);
      if (wmiObject.Properties == null)
        return;
      foreach (PropertyData property in wmiObject.Properties)
        members.Add(new PSProperty(property.Name, (Adapter) this, (object) wmiObject, (object) property) as T);
    }

    protected override PSProperty DoGetProperty(
      ManagementBaseObject wmiObject,
      string propertyName)
    {
      PSProperty property1 = base.DoGetProperty(wmiObject, propertyName);
      if (property1 != null)
        return property1;
      try
      {
        PropertyData property2 = wmiObject.Properties[propertyName];
        return new PSProperty(property2.Name, (Adapter) this, (object) wmiObject, (object) property2);
      }
      catch (ManagementException ex)
      {
        Adapter.tracer.TraceException((Exception) ex);
      }
      return (PSProperty) null;
    }

    protected override object InvokeManagementMethod(
      ManagementObject obj,
      string methodName,
      ManagementBaseObject inParams)
    {
      Adapter.tracer.WriteLine("Invoking class method: {0}", (object) methodName);
      try
      {
        return (object) obj.InvokeMethod(methodName, inParams, (InvokeMethodOptions) null);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw new MethodInvocationException("WMIMethodException", ex, "ExtendedTypeSystem", "WMIMethodInvocationException", new object[2]
        {
          (object) methodName,
          (object) ex.Message
        });
      }
    }

    protected override void AddAllMethods<T>(
      ManagementBaseObject wmiObject,
      PSMemberInfoInternalCollection<T> members)
    {
      if (!typeof (T).IsAssignableFrom(typeof (PSMethod)))
        return;
      foreach (BaseWMIAdapter.WMIMethodCacheEntry member in BaseWMIAdapter.GetInstanceMethodTable(wmiObject, false).memberCollection)
      {
        if ((object) members[member.Name] == null)
        {
          Adapter.tracer.WriteLine("Adding method {0}", (object) member.Name);
          members.Add(new PSMethod(member.Name, (Adapter) this, (object) wmiObject, (object) member) as T);
        }
      }
    }

    protected override T GetManagementObjectMethod<T>(
      ManagementBaseObject wmiObject,
      string methodName)
    {
      if (!typeof (T).IsAssignableFrom(typeof (PSMethod)))
        return default (T);
      BaseWMIAdapter.WMIMethodCacheEntry methodCacheEntry = (BaseWMIAdapter.WMIMethodCacheEntry) BaseWMIAdapter.GetInstanceMethodTable(wmiObject, false)[methodName];
      return methodCacheEntry == null ? default (T) : new PSMethod(methodCacheEntry.Name, (Adapter) this, (object) wmiObject, (object) methodCacheEntry) as T;
    }
  }
}
