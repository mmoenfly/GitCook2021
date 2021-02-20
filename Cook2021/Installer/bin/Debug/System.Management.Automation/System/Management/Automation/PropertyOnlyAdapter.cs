// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PropertyOnlyAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal abstract class PropertyOnlyAdapter : DotNetAdapter
  {
    protected abstract PSProperty DoGetProperty(object obj, string propertyName);

    protected abstract void DoAddAllProperties<T>(
      object obj,
      PSMemberInfoInternalCollection<T> members)
      where T : PSMemberInfo;

    protected override T GetMember<T>(object obj, string memberName)
    {
      PSProperty property = this.DoGetProperty(obj, memberName);
      if (typeof (T).IsAssignableFrom(typeof (PSProperty)) && property != null)
        return property as T;
      if (typeof (T).IsAssignableFrom(typeof (PSMethod)))
      {
        T dotNetMethod = PSObject.dotNetInstanceAdapter.GetDotNetMethod<T>(obj, memberName);
        if ((object) dotNetMethod != null && property == null)
          return dotNetMethod;
      }
      if (DotNetAdapter.IsTypeParameterizedProperty(typeof (T)))
      {
        PSParameterizedProperty dotNetProperty = PSObject.dotNetInstanceAdapter.GetDotNetProperty<PSParameterizedProperty>(obj, memberName);
        if (dotNetProperty != null && property == null)
          return dotNetProperty as T;
      }
      return default (T);
    }

    protected override PSMemberInfoInternalCollection<T> GetMembers<T>(
      object obj)
    {
      PSMemberInfoInternalCollection<T> members1 = new PSMemberInfoInternalCollection<T>();
      if (typeof (T).IsAssignableFrom(typeof (PSProperty)))
        this.DoAddAllProperties<T>(obj, members1);
      PSObject.dotNetInstanceAdapter.AddAllMethods<T>(obj, members1, true);
      if (DotNetAdapter.IsTypeParameterizedProperty(typeof (T)))
      {
        PSMemberInfoInternalCollection<PSParameterizedProperty> members2 = new PSMemberInfoInternalCollection<PSParameterizedProperty>();
        PSObject.dotNetInstanceAdapter.AddAllProperties<PSParameterizedProperty>(obj, members2, true);
        foreach (PSParameterizedProperty parameterizedProperty in (PSMemberInfoCollection<PSParameterizedProperty>) members2)
          members1.Add(parameterizedProperty as T);
      }
      return members1;
    }
  }
}
