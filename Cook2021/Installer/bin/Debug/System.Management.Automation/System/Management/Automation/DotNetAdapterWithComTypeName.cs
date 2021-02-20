// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DotNetAdapterWithComTypeName
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  internal class DotNetAdapterWithComTypeName : DotNetAdapter
  {
    private ComTypeInfo comTypeInfo;

    internal DotNetAdapterWithComTypeName(ComTypeInfo comTypeInfo) => this.comTypeInfo = comTypeInfo;

    protected override Collection<string> GetTypeNameHierarchy(object obj)
    {
      Collection<string> collection = new Collection<string>();
      for (Type type = obj.GetType(); type != null; type = type.BaseType)
      {
        if (type.FullName.Equals("System.__ComObject"))
          collection.Add(ComAdapter.GetComTypeName(this.comTypeInfo.Clsid));
        collection.Add(type.FullName);
      }
      return collection;
    }
  }
}
