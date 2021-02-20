// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.LookupPathCollection
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  internal class LookupPathCollection : Collection<string>
  {
    internal LookupPathCollection()
    {
    }

    internal LookupPathCollection(IEnumerable<string> collection)
    {
      foreach (string str in collection)
        this.Add(str);
    }

    public int Add(string item)
    {
      int num = -1;
      if (!this.Contains(item))
      {
        base.Add(item);
        num = base.IndexOf(item);
      }
      return num;
    }

    internal void AddRange(ICollection<string> collection)
    {
      foreach (string str in (IEnumerable<string>) collection)
        this.Add(str);
    }

    public new bool Contains(string item)
    {
      bool flag = false;
      foreach (string b in (Collection<string>) this)
      {
        if (string.Equals(item, b, StringComparison.OrdinalIgnoreCase))
        {
          flag = true;
          break;
        }
      }
      return flag;
    }

    internal Collection<int> IndexOfRelativePath()
    {
      Collection<int> collection = new Collection<int>();
      for (int index = 0; index < this.Count; ++index)
      {
        string str = this[index];
        if (!string.IsNullOrEmpty(str) && str.StartsWith(".", StringComparison.CurrentCulture))
          collection.Add(index);
      }
      return collection;
    }

    public new int IndexOf(string item)
    {
      if (string.IsNullOrEmpty(item))
        throw CommandDiscovery.tracer.NewArgumentException(nameof (item));
      int num = -1;
      for (int index = 0; index < this.Count; ++index)
      {
        if (string.Equals(this[index], item, StringComparison.OrdinalIgnoreCase))
        {
          num = index;
          break;
        }
      }
      return num;
    }
  }
}
