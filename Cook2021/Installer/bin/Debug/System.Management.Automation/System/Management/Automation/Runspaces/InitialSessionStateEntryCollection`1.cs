// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.InitialSessionStateEntryCollection`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
  public sealed class InitialSessionStateEntryCollection<T> : IEnumerable<T>, IEnumerable
    where T : InitialSessionStateEntry
  {
    private Collection<T> _internalCollection;
    private object _syncObject = new object();

    public InitialSessionStateEntryCollection() => this._internalCollection = new Collection<T>();

    public InitialSessionStateEntryCollection(IEnumerable<T> items)
    {
      this._internalCollection = new Collection<T>();
      foreach (T obj in items)
        this._internalCollection.Add(obj);
    }

    public InitialSessionStateEntryCollection<T> Clone()
    {
      InitialSessionStateEntryCollection<T> stateEntryCollection;
      lock (this._syncObject)
      {
        stateEntryCollection = new InitialSessionStateEntryCollection<T>();
        foreach (T obj in this._internalCollection)
          stateEntryCollection.Add((T) obj.Clone());
      }
      return stateEntryCollection;
    }

    public void Reset()
    {
      lock (this._syncObject)
        this._internalCollection.Clear();
    }

    public int Count => this._internalCollection.Count;

    public T this[int index]
    {
      get
      {
        lock (this._syncObject)
          return this._internalCollection[index];
      }
    }

    public Collection<T> this[string name]
    {
      get
      {
        Collection<T> collection = new Collection<T>();
        lock (this._syncObject)
        {
          foreach (T obj in this._internalCollection)
          {
            if (obj.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
              collection.Add(obj);
          }
        }
        return collection;
      }
    }

    public void RemoveItem(int index)
    {
      lock (this._syncObject)
        this._internalCollection.RemoveAt(index);
    }

    public void RemoveItem(int index, int count)
    {
      lock (this._syncObject)
      {
        while (count-- > 0)
          this._internalCollection.RemoveAt(index);
      }
    }

    public void Clear()
    {
      lock (this._syncObject)
        this._internalCollection.Clear();
    }

    public void Remove(string name, object type)
    {
      if (name == null)
        throw new ArgumentNullException(nameof (name));
      lock (this._syncObject)
      {
        type1 = (Type) null;
        switch (type)
        {
          case null:
          case Type type1:
            for (int index = this._internalCollection.Count - 1; index >= 0; --index)
            {
              T obj = this._internalCollection[index];
              if ((object) obj != null && (type1 == null || obj.GetType() == type1) && string.Equals(obj.Name, name, StringComparison.OrdinalIgnoreCase))
                this._internalCollection.RemoveAt(index);
            }
            break;
          default:
            type1 = type.GetType();
            goto case null;
        }
      }
    }

    public void Add(T item)
    {
      if ((object) item == null)
        throw new ArgumentNullException(nameof (item));
      lock (this._syncObject)
        this._internalCollection.Add(item);
    }

    public void Add(IEnumerable<T> items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this._syncObject)
      {
        foreach (T obj in items)
          this._internalCollection.Add(obj);
      }
    }

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this._internalCollection.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => this._internalCollection.GetEnumerator();
  }
}
