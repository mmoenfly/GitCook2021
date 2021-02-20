// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSDataCollection`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace System.Management.Automation
{
  public class PSDataCollection<T> : 
    IList<T>,
    ICollection<T>,
    IEnumerable<T>,
    IList,
    ICollection,
    IEnumerable,
    IDisposable
  {
    [TraceSource("PowerShellHosting", "Powershell hosting interfaces")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PowerShellHosting", "Powershell hosting interfaces");
    private IList<T> data;
    private ManualResetEvent readWaitHandle;
    private bool isOpen = true;
    private bool releaseOnEnumeration;
    private bool isEnumerated;
    private int refCount;
    private object syncObject = new object();
    private bool isDisposed;
    private static string resBaseName = "PSDataBufferStrings";

    public PSDataCollection()
      : this((IList<T>) new List<T>())
    {
      using (PSDataCollection<T>.tracer.TraceConstructor((object) this))
        ;
    }

    public PSDataCollection(IEnumerable<T> items)
      : this((IList<T>) new List<T>(items))
    {
      using (PSDataCollection<T>.tracer.TraceConstructor((object) this))
        this.Complete();
    }

    public PSDataCollection(int capacity)
      : this((IList<T>) new List<T>(capacity))
    {
      using (PSDataCollection<T>.tracer.TraceConstructor((object) this))
        ;
    }

    internal PSDataCollection(IList<T> listToUse)
    {
      using (PSDataCollection<T>.tracer.TraceConstructor((object) this, (object) listToUse))
        this.data = listToUse;
    }

    public event EventHandler<DataAddedEventArgs> DataAdded;

    public event EventHandler Completed;

    public bool IsOpen
    {
      get
      {
        using (PSDataCollection<T>.tracer.TraceProperty())
        {
          lock (this.syncObject)
            return this.isOpen;
        }
      }
    }

    internal bool ReleaseOnEnumeration
    {
      get
      {
        lock (this.syncObject)
          return this.releaseOnEnumeration;
      }
      set
      {
        lock (this.syncObject)
          this.releaseOnEnumeration = value;
      }
    }

    internal bool IsEnumerated
    {
      get
      {
        lock (this.syncObject)
          return this.isEnumerated;
      }
      set
      {
        lock (this.syncObject)
          this.isEnumerated = value;
      }
    }

    public void Complete()
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        bool flag = false;
        try
        {
          lock (this.syncObject)
          {
            if (!this.isOpen)
              return;
            this.isOpen = false;
            flag = true;
            Monitor.PulseAll(this.syncObject);
          }
        }
        finally
        {
          if (flag)
          {
            if (this.readWaitHandle != null)
              this.readWaitHandle.Set();
            EventHandler completed = this.Completed;
            if (completed != null)
              completed((object) this, EventArgs.Empty);
          }
        }
      }
    }

    public T this[int index]
    {
      get
      {
        using (PSDataCollection<T>.tracer.TraceProperty())
        {
          lock (this.syncObject)
            return this.data[index];
        }
      }
      set
      {
        using (PSDataCollection<T>.tracer.TraceProperty())
        {
          lock (this.syncObject)
          {
            if (index < 0 || index >= this.data.Count)
              throw PSDataCollection<T>.tracer.NewArgumentOutOfRangeException(nameof (index), (object) index, PSDataCollection<T>.resBaseName, "IndexOutOfRange", (object) 0, (object) (this.data.Count - 1));
            this.data[index] = value;
          }
        }
      }
    }

    public int IndexOf(T item)
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        lock (this.syncObject)
          return this.InternalIndexOf(item);
      }
    }

    public void Insert(int index, T item)
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        lock (this.syncObject)
          this.InternalInsertItem(Guid.Empty, index, item);
        this.RaiseEvents(Guid.Empty, index);
      }
    }

    public void RemoveAt(int index)
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          if (index < 0 || index >= this.data.Count)
            throw PSDataCollection<T>.tracer.NewArgumentOutOfRangeException(nameof (index), (object) index, PSDataCollection<T>.resBaseName, "IndexOutOfRange", (object) 0, (object) (this.data.Count - 1));
          this.RemoveItem(index);
        }
      }
    }

    public int Count
    {
      get
      {
        using (PSDataCollection<T>.tracer.TraceProperty())
        {
          lock (this.syncObject)
            return this.data.Count;
        }
      }
    }

    public bool IsReadOnly
    {
      get
      {
        using (PSDataCollection<T>.tracer.TraceProperty())
          return false;
      }
    }

    public void Add(T item)
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
        this.InternalAdd(Guid.Empty, item);
    }

    public void Clear()
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        lock (this.syncObject)
          this.data.Clear();
      }
    }

    public bool Contains(T item)
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        lock (this.syncObject)
          return this.data.Contains(item);
      }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        lock (this.syncObject)
          this.data.CopyTo(array, arrayIndex);
      }
    }

    public bool Remove(T item)
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          int index = this.InternalIndexOf(item);
          if (index < 0)
            return false;
          this.RemoveItem(index);
          return true;
        }
      }
    }

    public IEnumerator<T> GetEnumerator() => (IEnumerator<T>) new PSDataCollectionEnumerator<T>(this);

    int IList.Add(object value)
    {
      PSDataCollection<T>.VerifyValueType(value);
      int count = this.data.Count;
      this.InternalAdd(Guid.Empty, (T) value);
      this.RaiseEvents(Guid.Empty, count);
      return count;
    }

    bool IList.Contains(object value)
    {
      PSDataCollection<T>.VerifyValueType(value);
      return this.Contains((T) value);
    }

    int IList.IndexOf(object value)
    {
      PSDataCollection<T>.VerifyValueType(value);
      return this.IndexOf((T) value);
    }

    void IList.Insert(int index, object value)
    {
      PSDataCollection<T>.VerifyValueType(value);
      this.Insert(index, (T) value);
    }

    void IList.Remove(object value)
    {
      PSDataCollection<T>.VerifyValueType(value);
      this.Remove((T) value);
    }

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => false;

    object IList.this[int index]
    {
      get => (object) this[index];
      set
      {
        PSDataCollection<T>.VerifyValueType(value);
        this[index] = (T) value;
      }
    }

    bool ICollection.IsSynchronized => true;

    object ICollection.SyncRoot => this.syncObject;

    void ICollection.CopyTo(Array array, int index)
    {
      lock (this.syncObject)
        this.data.CopyTo((T[]) array, index);
    }

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) new PSDataCollectionEnumerator<T>(this);

    public Collection<T> ReadAll()
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
        return this.ReadAndRemove(0);
    }

    internal Collection<T> ReadAndRemove(int readCount)
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        int num = readCount > 0 ? readCount : int.MaxValue;
        lock (this.syncObject)
        {
          Collection<T> collection = new Collection<T>();
          for (int index = 0; index < num && this.data.Count > 0; ++index)
          {
            collection.Add(this.data[0]);
            this.data.RemoveAt(0);
          }
          if (this.readWaitHandle != null)
          {
            if (this.data.Count > 0 || !this.isOpen)
              this.readWaitHandle.Set();
            else
              this.readWaitHandle.Reset();
          }
          return collection;
        }
      }
    }

    protected virtual void InsertItem(Guid psInstanceId, int index, T item) => this.data.Insert(index, item);

    protected virtual void RemoveItem(int index) => this.data.RemoveAt(index);

    internal WaitHandle WaitHandle
    {
      get
      {
        using (PSDataCollection<T>.tracer.TraceProperty())
        {
          if (this.readWaitHandle == null)
          {
            lock (this.syncObject)
            {
              if (this.readWaitHandle == null)
                this.readWaitHandle = new ManualResetEvent(this.data.Count > 0 || !this.isOpen);
            }
          }
          return (WaitHandle) this.readWaitHandle;
        }
      }
    }

    private void RaiseEvents(Guid psInstanceId, int index)
    {
      lock (this.syncObject)
      {
        if (this.readWaitHandle != null)
        {
          if (this.data.Count > 0 || !this.isOpen)
            this.readWaitHandle.Set();
          else
            this.readWaitHandle.Reset();
        }
        Monitor.PulseAll(this.syncObject);
      }
      EventHandler<DataAddedEventArgs> dataAdded = this.DataAdded;
      if (dataAdded == null)
        return;
      dataAdded((object) this, new DataAddedEventArgs(psInstanceId, index));
    }

    private void InternalInsertItem(Guid psInstanceId, int index, T item)
    {
      if (!this.isOpen)
        throw PSDataCollection<T>.tracer.NewInvalidOperationException(PSDataCollection<T>.resBaseName, "WriteToClosedBuffer");
      this.InsertItem(psInstanceId, index, item);
    }

    internal void InternalAdd(Guid psInstanceId, T item)
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        int index = -1;
        lock (this.syncObject)
        {
          index = this.data.Count;
          this.InternalInsertItem(psInstanceId, index, item);
        }
        if (index <= -1)
          return;
        this.RaiseEvents(psInstanceId, index);
      }
    }

    internal void InternalAddRange(Guid psInstanceId, ICollection collection)
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        if (collection == null)
          throw PSDataCollection<T>.tracer.NewArgumentNullException(nameof (collection));
        int index = -1;
        bool flag = false;
        lock (this.syncObject)
        {
          if (!this.isOpen)
            throw PSDataCollection<T>.tracer.NewInvalidOperationException(PSDataCollection<T>.resBaseName, "WriteToClosedBuffer");
          index = this.data.Count;
          foreach (object obj in (IEnumerable) collection)
          {
            this.InsertItem(psInstanceId, this.data.Count, (T) obj);
            flag = true;
          }
        }
        if (!flag)
          return;
        this.RaiseEvents(psInstanceId, index);
      }
    }

    internal void AddRef()
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        lock (this.syncObject)
          ++this.refCount;
      }
    }

    internal void DecrementRef()
    {
      using (PSDataCollection<T>.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          --this.refCount;
          if (this.refCount != 0)
            return;
          if (this.readWaitHandle != null)
            this.readWaitHandle.Set();
          Monitor.PulseAll(this.syncObject);
        }
      }
    }

    private int InternalIndexOf(T item)
    {
      int count = this.data.Count;
      for (int index = 0; index < count; ++index)
      {
        if (object.Equals((object) this.data[index], (object) item))
          return index;
      }
      return -1;
    }

    private static void VerifyValueType(object value)
    {
      if (value == null)
      {
        if (typeof (T).IsValueType)
          throw PSDataCollection<T>.tracer.NewArgumentNullException(nameof (value), PSDataCollection<T>.resBaseName, "ValueNullReference");
      }
      else if (!(value is T))
        throw PSDataCollection<T>.tracer.NewArgumentException(nameof (value), PSDataCollection<T>.resBaseName, "CannotConvertToGenericType", (object) value.GetType().FullName, (object) typeof (T).FullName);
    }

    private static void VerifyCollectionType(ICollection value)
    {
      Type[] genericArguments = value.GetType().GetGenericArguments();
      if (1 != genericArguments.Length)
        throw PSDataCollection<T>.tracer.NewInvalidOperationException();
      if (genericArguments[0].Equals(typeof (T)))
        throw PSDataCollection<T>.tracer.NewArgumentException(nameof (value), PSDataCollection<T>.resBaseName, "CannotConvertToGenericType", (object) genericArguments[0].FullName, (object) typeof (T).FullName);
    }

    internal object SyncObject => this.syncObject;

    internal int RefCount
    {
      get => this.refCount;
      set
      {
        lock (this.syncObject)
          this.refCount = value;
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected void Dispose(bool disposing)
    {
      if (!disposing || this.isDisposed)
        return;
      lock (this.syncObject)
      {
        if (this.isDisposed)
          return;
        this.isDisposed = true;
      }
      this.Complete();
      if (this.readWaitHandle == null)
        return;
      this.readWaitHandle.Close();
    }
  }
}
