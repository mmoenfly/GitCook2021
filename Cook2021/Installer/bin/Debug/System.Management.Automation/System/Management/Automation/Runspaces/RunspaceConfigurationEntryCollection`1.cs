// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceConfigurationEntryCollection`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
  public sealed class RunspaceConfigurationEntryCollection<T> : IEnumerable<T>, IEnumerable
    where T : RunspaceConfigurationEntry
  {
    private Collection<T> _data = new Collection<T>();
    private int _builtInEnd;
    private Collection<T> _updateList = new Collection<T>();
    private object _syncObject = new object();
    [TraceSource("RunspaceConfigurationEntryCollection", "RunspaceConfigurationEntryCollection")]
    private static PSTraceSource _tracer = PSTraceSource.GetTracer(nameof (RunspaceConfigurationEntryCollection<T>), nameof (RunspaceConfigurationEntryCollection<T>));

    public RunspaceConfigurationEntryCollection()
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceConstructor((object) this))
        ;
    }

    public RunspaceConfigurationEntryCollection(IEnumerable<T> items)
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceConstructor((object) this))
      {
        if (items == null)
          throw RunspaceConfigurationEntryCollection<T>._tracer.NewArgumentNullException("item");
        this.AddBuiltInItem(items);
      }
    }

    internal Collection<T> UpdateList => this._updateList;

    public T this[int index]
    {
      get
      {
        lock (this._syncObject)
          return this._data[index];
      }
    }

    public int Count
    {
      get
      {
        lock (this._syncObject)
          return this._data.Count;
      }
    }

    public void Reset()
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
      {
        lock (this._syncObject)
        {
          for (int index = this._data.Count - 1; index >= 0; --index)
          {
            if (!this._data[index].BuiltIn)
            {
              this.RecordRemove(this._data[index]);
              this._data.RemoveAt(index);
            }
          }
          this._builtInEnd = this._data.Count;
        }
      }
    }

    public void RemoveItem(int index)
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
      {
        lock (this._syncObject)
        {
          if (index < 0 || index >= this._data.Count)
            throw RunspaceConfigurationEntryCollection<T>._tracer.NewArgumentOutOfRangeException(nameof (index), (object) index);
          this.RecordRemove(this._data[index]);
          this._data.RemoveAt(index);
          if (index >= this._builtInEnd)
            return;
          --this._builtInEnd;
        }
      }
    }

    public void RemoveItem(int index, int count)
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
      {
        lock (this._syncObject)
        {
          if (index < 0 || index + count > this._data.Count)
            throw RunspaceConfigurationEntryCollection<T>._tracer.NewArgumentOutOfRangeException(nameof (index), (object) index);
          for (int index1 = index + count - 1; index1 >= index; --index1)
          {
            this.RecordRemove(this._data[index1]);
            this._data.RemoveAt(index1);
          }
          int num = Math.Min(count, this._builtInEnd - index);
          if (num <= 0)
            return;
          this._builtInEnd -= num;
        }
      }
    }

    public void Prepend(T item)
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
      {
        lock (this._syncObject)
        {
          this.RecordAdd(item);
          item._builtIn = false;
          this._data.Insert(0, item);
          ++this._builtInEnd;
        }
      }
    }

    public void Prepend(IEnumerable<T> items)
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
      {
        lock (this._syncObject)
        {
          int num = 0;
          foreach (T t in items)
          {
            this.RecordAdd(t);
            t._builtIn = false;
            this._data.Insert(num++, t);
            ++this._builtInEnd;
          }
        }
      }
    }

    public void Append(T item)
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
      {
        lock (this._syncObject)
        {
          this.RecordAdd(item);
          item._builtIn = false;
          this._data.Add(item);
        }
      }
    }

    public void Append(IEnumerable<T> items)
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
      {
        lock (this._syncObject)
        {
          foreach (T t in items)
          {
            this.RecordAdd(t);
            t._builtIn = false;
            this._data.Add(t);
          }
        }
      }
    }

    internal void AddBuiltInItem(T item)
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
      {
        lock (this._syncObject)
        {
          item._builtIn = true;
          this.RecordAdd(item);
          this._data.Insert(this._builtInEnd, item);
          ++this._builtInEnd;
        }
      }
    }

    internal void AddBuiltInItem(IEnumerable<T> items)
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
      {
        lock (this._syncObject)
        {
          foreach (T t in items)
          {
            t._builtIn = true;
            this.RecordAdd(t);
            this._data.Insert(this._builtInEnd, t);
            ++this._builtInEnd;
          }
        }
      }
    }

    internal void RemovePSSnapIn(string PSSnapinName)
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
      {
        lock (this._syncObject)
        {
          for (int index = this._data.Count - 1; index >= 0; --index)
          {
            if (this._data[index].PSSnapIn != null && this._data[index].PSSnapIn.Name.Equals(PSSnapinName, StringComparison.Ordinal))
            {
              this.RecordRemove(this._data[index]);
              this._data.RemoveAt(index);
              if (index < this._builtInEnd)
                --this._builtInEnd;
            }
          }
        }
      }
    }

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this._data.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => this._data.GetEnumerator();

    public void Update()
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
        this.Update(false);
    }

    internal void Update(bool force)
    {
      using (RunspaceConfigurationEntryCollection<T>._tracer.TraceMethod())
      {
        lock (this._syncObject)
        {
          if (this.OnUpdate == null || !force && this._updateList.Count <= 0)
            return;
          this.OnUpdate();
          foreach (T update in this._updateList)
            update._action = UpdateAction.None;
          this._updateList.Clear();
        }
      }
    }

    private void RecordRemove(T t)
    {
      if (t.Action == UpdateAction.Add)
      {
        t._action = UpdateAction.None;
        this._updateList.Remove(t);
      }
      else
      {
        t._action = UpdateAction.Remove;
        this._updateList.Add(t);
      }
    }

    private void RecordAdd(T t)
    {
      if (t.Action == UpdateAction.Remove)
      {
        t._action = UpdateAction.None;
        this._updateList.Remove(t);
      }
      else
      {
        t._action = UpdateAction.Add;
        this._updateList.Add(t);
      }
    }

    internal event RunspaceConfigurationEntryUpdateEventHandler OnUpdate;
  }
}
