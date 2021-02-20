// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Repository`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Remoting;

namespace System.Management.Automation
{
  public abstract class Repository<T> where T : class
  {
    private Dictionary<Guid, T> repository = new Dictionary<Guid, T>();
    private object syncObject = new object();
    private string identifier;

    public void Add(T item)
    {
      if ((object) item == null)
        throw new ArgumentNullException(this.identifier);
      lock (this.syncObject)
      {
        Guid key = this.GetKey(item);
        if (this.repository.ContainsKey(key))
          throw new ArgumentException(this.identifier);
        this.repository.Add(key, item);
      }
    }

    public void Remove(T item)
    {
      if ((object) item == null)
        throw new ArgumentNullException(this.identifier);
      lock (this.syncObject)
      {
        Guid key = this.GetKey(item);
        if (this.repository.ContainsKey(key))
          this.repository.Remove(key);
        else
          throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.ItemNotFoundInRepository, (object) "Job repository", (object) key.ToString()));
      }
    }

    internal abstract Guid GetKey(T item);

    internal Repository(string identifier) => this.identifier = identifier;

    internal List<T> Items
    {
      get
      {
        lock (this.syncObject)
          return new List<T>((IEnumerable<T>) this.repository.Values);
      }
    }

    internal T GetItem(Guid instanceId)
    {
      lock (this.syncObject)
        return this.repository.ContainsKey(instanceId) ? this.repository[instanceId] : default (T);
    }
  }
}
