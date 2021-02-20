// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.BaseDotNetAdapterForAdaptedObjects
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class BaseDotNetAdapterForAdaptedObjects : DotNetAdapter
  {
    protected override PSMemberInfoInternalCollection<T> GetMembers<T>(
      object obj)
    {
      PSMemberInfoInternalCollection<T> members = new PSMemberInfoInternalCollection<T>();
      this.AddAllProperties<T>(obj, members, true);
      this.AddAllMethods<T>(obj, members, true);
      this.AddAllEvents<T>(obj, members, true);
      return members;
    }

    protected override T GetMember<T>(object obj, string memberName)
    {
      PSProperty dotNetProperty1 = this.GetDotNetProperty<PSProperty>(obj, memberName);
      if (typeof (T).IsAssignableFrom(typeof (PSProperty)) && dotNetProperty1 != null)
        return dotNetProperty1 as T;
      if (typeof (T).Equals(typeof (PSMemberInfo)))
      {
        T dotNetMethod = PSObject.dotNetInstanceAdapter.GetDotNetMethod<T>(obj, memberName);
        if ((object) dotNetMethod != null && dotNetProperty1 == null)
          return dotNetMethod;
      }
      if (DotNetAdapter.IsTypeParameterizedProperty(typeof (T)))
      {
        PSParameterizedProperty dotNetProperty2 = PSObject.dotNetInstanceAdapter.GetDotNetProperty<PSParameterizedProperty>(obj, memberName);
        if (dotNetProperty2 != null && dotNetProperty1 == null)
          return dotNetProperty2 as T;
      }
      return default (T);
    }
  }
}
