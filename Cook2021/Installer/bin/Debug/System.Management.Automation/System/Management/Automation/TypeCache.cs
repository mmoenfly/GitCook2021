// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TypeCache
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal abstract class TypeCache
  {
    internal Dictionary<string, Type> cache = new Dictionary<string, Type>(256, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);

    internal void AddAssemblyLoadEventHandler() => AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(this.currentDomain_AssemblyLoad);

    private void currentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args) => this.Reset();

    internal void Add(string typeName, Type type)
    {
      lock (this.cache)
      {
        if (!this.cache.ContainsKey(typeName))
          this.cache.Add(typeName, type);
        else if (this.cache[typeName] != type)
          throw new InvalidOperationException();
      }
    }

    internal bool Remove(string typeName)
    {
      lock (this.cache)
        return this.cache.Remove(typeName);
    }

    internal Type Get(string typeName)
    {
      Type type;
      return this.cache.TryGetValue(typeName, out type) ? type : (Type) null;
    }

    internal abstract void Reset();
  }
}
