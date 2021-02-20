// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSObjectAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  internal class PSObjectAdapter : MemberRedirectionAdapter
  {
    internal PSObjectAdapter()
    {
    }

    protected override Collection<string> GetTypeNameHierarchy(object obj)
    {
      PSObject psObject = obj as PSObject;
      Collection<string> collection = new Collection<string>();
      foreach (string typeName in psObject.TypeNames)
        collection.Add(typeName);
      return collection;
    }

    protected override T GetMember<T>(object obj, string memberName) => (obj as PSObject).Members[memberName] as T;

    protected override PSMemberInfoInternalCollection<T> GetMembers<T>(
      object obj)
    {
      PSMemberInfoInternalCollection<T> internalCollection = new PSMemberInfoInternalCollection<T>();
      foreach (PSMemberInfo member1 in ((PSObject) obj).Members)
      {
        if (member1 is T member)
          internalCollection.Add(member);
      }
      return internalCollection;
    }
  }
}
