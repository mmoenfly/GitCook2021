// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CacheTable
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Management.Automation
{
  internal class CacheTable
  {
    internal Collection<object> memberCollection;
    private HybridDictionary indexes;

    internal CacheTable()
    {
      this.memberCollection = new Collection<object>();
      this.indexes = new HybridDictionary(true);
    }

    internal void Add(string name, object member)
    {
      this.indexes[(object) name] = (object) new int?(this.memberCollection.Count);
      this.memberCollection.Add(member);
    }

    internal object this[string name]
    {
      get
      {
        object index = this.indexes[(object) name];
        return index == null ? (object) null : this.memberCollection[((int?) index).Value];
      }
    }
  }
}
