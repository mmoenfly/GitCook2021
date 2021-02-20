// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.ObjectStream
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace System.Management.Automation.Internal
{
  internal sealed class ObjectStream : ObjectStreamBase, IDisposable
  {
    private ArrayList _objects;
    private bool _isOpen;
    private AutoResetEvent _readHandle;
    private ManualResetEvent _readWaitHandle;
    private ManualResetEvent _readClosedHandle;
    private AutoResetEvent _writeHandle;
    private ManualResetEvent _writeWaitHandle;
    private ManualResetEvent _writeClosedHandle;
    private PipelineReader<object> _reader;
    private PipelineReader<PSObject> _mshreader;
    private PipelineWriter _writer;
    private int _capacity = int.MaxValue;
    private object _monitorObject = new object();
    private bool _disposed;

    internal ObjectStream()
      : this(int.MaxValue)
    {
      using (ObjectStreamBase._trace.TraceConstructor((object) this))
        ;
    }

    internal ObjectStream(int capacity)
    {
      using (ObjectStreamBase._trace.TraceConstructor((object) this, (object) capacity))
      {
        this._capacity = capacity > 0 && capacity <= int.MaxValue ? capacity : throw ObjectStreamBase._trace.NewArgumentOutOfRangeException(nameof (capacity), (object) capacity);
        this._readHandle = new AutoResetEvent(false);
        this._writeHandle = new AutoResetEvent(true);
        this._readClosedHandle = new ManualResetEvent(false);
        this._writeClosedHandle = new ManualResetEvent(false);
        this._objects = new ArrayList();
        this._isOpen = true;
      }
    }

    internal override int MaxCapacity
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
          return this._capacity;
      }
    }

    internal override WaitHandle ReadHandle
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
        {
          WaitHandle waitHandle = (WaitHandle) null;
          lock (this._monitorObject)
          {
            if (this._readWaitHandle == null)
              this._readWaitHandle = new ManualResetEvent(this._objects.Count > 0 || !this._isOpen);
            waitHandle = (WaitHandle) this._readWaitHandle;
          }
          return waitHandle;
        }
      }
    }

    internal override WaitHandle WriteHandle
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
        {
          WaitHandle waitHandle = (WaitHandle) null;
          lock (this._monitorObject)
          {
            if (this._writeWaitHandle == null)
              this._writeWaitHandle = new ManualResetEvent(this._objects.Count < this._capacity || !this._isOpen);
            waitHandle = (WaitHandle) this._writeWaitHandle;
          }
          return waitHandle;
        }
      }
    }

    internal override PipelineReader<object> ObjectReader
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
        {
          PipelineReader<object> pipelineReader = (PipelineReader<object>) null;
          lock (this._monitorObject)
          {
            if (this._reader == null)
              this._reader = (PipelineReader<object>) new System.Management.Automation.Internal.ObjectReader(this);
            pipelineReader = this._reader;
          }
          return pipelineReader;
        }
      }
    }

    internal override PipelineReader<PSObject> PSObjectReader
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
        {
          PipelineReader<PSObject> pipelineReader = (PipelineReader<PSObject>) null;
          lock (this._monitorObject)
          {
            if (this._mshreader == null)
              this._mshreader = (PipelineReader<PSObject>) new System.Management.Automation.Internal.PSObjectReader(this);
            pipelineReader = this._mshreader;
          }
          return pipelineReader;
        }
      }
    }

    internal override PipelineWriter ObjectWriter
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
        {
          PipelineWriter pipelineWriter = (PipelineWriter) null;
          lock (this._monitorObject)
          {
            if (this._writer == null)
              this._writer = (PipelineWriter) new System.Management.Automation.Internal.ObjectWriter((ObjectStreamBase) this);
            pipelineWriter = this._writer;
          }
          return pipelineWriter;
        }
      }
    }

    internal override bool EndOfPipeline
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
        {
          bool flag = true;
          lock (this._monitorObject)
            flag = this._objects.Count == 0 && !this._isOpen;
          return flag;
        }
      }
    }

    internal override bool IsOpen
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
        {
          bool flag = true;
          lock (this._monitorObject)
            flag = this._isOpen;
          return flag;
        }
      }
    }

    internal override int Count
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
        {
          int num = 0;
          lock (this._monitorObject)
            num = this._objects.Count;
          return num;
        }
      }
    }

    private bool WaitRead()
    {
      using (ObjectStreamBase._trace.TraceLock(nameof (WaitRead)))
      {
        if (!this.EndOfPipeline)
        {
          try
          {
            WaitHandle.WaitAny(new WaitHandle[2]
            {
              (WaitHandle) this._readHandle,
              (WaitHandle) this._readClosedHandle
            });
          }
          catch (ObjectDisposedException ex)
          {
          }
        }
        return !this.EndOfPipeline;
      }
    }

    private bool WaitWrite()
    {
      using (ObjectStreamBase._trace.TraceLock(nameof (WaitWrite)))
      {
        if (this.IsOpen)
        {
          try
          {
            WaitHandle.WaitAny(new WaitHandle[2]
            {
              (WaitHandle) this._writeHandle,
              (WaitHandle) this._writeClosedHandle
            });
          }
          catch (ObjectDisposedException ex)
          {
          }
        }
        return this.IsOpen;
      }
    }

    private void RaiseEvents()
    {
      using (ObjectStreamBase._trace.TraceMethod())
      {
        bool flag1 = true;
        bool flag2 = true;
        bool flag3 = false;
        try
        {
          lock (this._monitorObject)
          {
            flag1 = !this._isOpen || this._objects.Count > 0;
            flag2 = !this._isOpen || this._objects.Count < this._capacity;
            flag3 = !this._isOpen && this._objects.Count == 0;
            if (this._readWaitHandle != null)
            {
              try
              {
                if (flag1)
                  this._readWaitHandle.Set();
                else
                  this._readWaitHandle.Reset();
              }
              catch (ObjectDisposedException ex)
              {
              }
            }
            if (this._writeWaitHandle != null)
            {
              try
              {
                if (flag2)
                  this._writeWaitHandle.Set();
                else
                  this._writeWaitHandle.Reset();
              }
              catch (ObjectDisposedException ex)
              {
              }
            }
          }
        }
        finally
        {
          if (flag1)
          {
            try
            {
              this._readHandle.Set();
            }
            catch (ObjectDisposedException ex)
            {
            }
          }
          if (flag2)
          {
            try
            {
              this._writeHandle.Set();
            }
            catch (ObjectDisposedException ex)
            {
            }
          }
          if (flag3)
          {
            try
            {
              this._readClosedHandle.Set();
            }
            catch (ObjectDisposedException ex)
            {
            }
          }
        }
        if (!flag1)
          return;
        this.FireDataReadyEvent((object) this, new EventArgs());
      }
    }

    internal override void Flush()
    {
      using (ObjectStreamBase._trace.TraceMethod())
      {
        bool flag = false;
        try
        {
          lock (this._monitorObject)
          {
            if (this._objects.Count <= 0)
              return;
            flag = true;
            this._objects.Clear();
          }
        }
        finally
        {
          if (flag)
            this.RaiseEvents();
        }
      }
    }

    internal override void Close()
    {
      using (ObjectStreamBase._trace.TraceMethod())
      {
        bool flag = false;
        try
        {
          lock (this._monitorObject)
          {
            if (!this._isOpen)
              return;
            flag = true;
            this._isOpen = false;
          }
        }
        finally
        {
          if (flag)
          {
            try
            {
              this._writeClosedHandle.Set();
            }
            catch (ObjectDisposedException ex)
            {
            }
            this.RaiseEvents();
          }
        }
      }
    }

    internal override object Read()
    {
      using (ObjectStreamBase._trace.TraceMethod())
      {
        Collection<object> collection = this.Read(1);
        return collection.Count == 1 ? collection[0] : (object) AutomationNull.Value;
      }
    }

    internal override Collection<object> Read(int count)
    {
      using (ObjectStreamBase._trace.TraceMethod())
      {
        if (count < 0)
          throw ObjectStreamBase._trace.NewArgumentOutOfRangeException(nameof (count), (object) count);
        if (count == 0)
          return new Collection<object>();
        Collection<object> collection = new Collection<object>();
        bool flag = false;
        while (count > 0)
        {
          if (this.WaitRead())
          {
            try
            {
              lock (this._monitorObject)
              {
                if (this._objects.Count != 0)
                {
                  flag = true;
                  int count1 = 0;
                  foreach (object obj in this._objects)
                  {
                    collection.Add(obj);
                    ++count1;
                    if (--count <= 0)
                      break;
                  }
                  this._objects.RemoveRange(0, count1);
                }
              }
            }
            finally
            {
              if (flag)
                this.RaiseEvents();
            }
          }
          else
            break;
        }
        return collection;
      }
    }

    internal override Collection<object> ReadToEnd()
    {
      using (ObjectStreamBase._trace.TraceMethod())
        return this.Read(int.MaxValue);
    }

    internal override Collection<object> NonBlockingRead(int maxRequested)
    {
      using (ObjectStreamBase._trace.TraceMethod())
      {
        Collection<object> collection = (Collection<object>) null;
        bool flag = false;
        if (maxRequested == 0)
          return new Collection<object>();
        if (maxRequested < 0)
          throw ObjectStreamBase._trace.NewArgumentOutOfRangeException(nameof (maxRequested), (object) maxRequested);
        try
        {
          lock (this._monitorObject)
          {
            int count = this._objects.Count;
            if (count > maxRequested)
              count = maxRequested;
            if (count > 0)
            {
              collection = new Collection<object>();
              for (int index = 0; index < count; ++index)
                collection.Add(this._objects[index]);
              flag = true;
              this._objects.RemoveRange(0, count);
            }
          }
        }
        finally
        {
          if (flag)
            this.RaiseEvents();
        }
        if (collection == null)
          collection = new Collection<object>();
        return collection;
      }
    }

    internal override object Peek()
    {
      using (ObjectStreamBase._trace.TraceMethod())
      {
        object obj = (object) null;
        lock (this._monitorObject)
          obj = this.EndOfPipeline || this._objects.Count == 0 ? (object) AutomationNull.Value : this._objects[0];
        return obj;
      }
    }

    internal override int Write(object obj, bool enumerateCollection)
    {
      using (ObjectStreamBase._trace.TraceMethod())
      {
        if (obj == AutomationNull.Value)
          return 0;
        if (!this.IsOpen)
        {
          Exception exceptionRecord = (Exception) new PipelineClosedException(ResourceManagerCache.GetResourceString("Pipeline", "WriteToClosedPipeline"));
          ObjectStreamBase._trace.TraceException(exceptionRecord);
          throw exceptionRecord;
        }
        ArrayList arrayList = new ArrayList();
        IEnumerable enumerable = (IEnumerable) null;
        if (enumerateCollection)
          enumerable = LanguagePrimitives.GetEnumerable(obj);
        if (enumerable == null)
        {
          arrayList.Add(obj);
        }
        else
        {
          foreach (object obj1 in enumerable)
          {
            if (AutomationNull.Value != obj1)
              arrayList.Add(obj1);
          }
        }
        int index = 0;
        int count1 = arrayList.Count;
        while (count1 > 0)
        {
          bool flag = false;
          if (this.WaitWrite())
          {
            try
            {
              lock (this._monitorObject)
              {
                if (this.IsOpen)
                {
                  int num = this._capacity - this._objects.Count;
                  if (0 < num)
                  {
                    int count2 = count1;
                    if (count2 > num)
                      count2 = num;
                    try
                    {
                      if (count2 == arrayList.Count)
                      {
                        this._objects.AddRange((ICollection) arrayList);
                        index += count2;
                        count1 -= count2;
                      }
                      else
                      {
                        this._objects.AddRange((ICollection) arrayList.GetRange(index, count2));
                        index += count2;
                        count1 -= count2;
                      }
                    }
                    finally
                    {
                      flag = true;
                    }
                  }
                }
                else
                  break;
              }
            }
            finally
            {
              if (flag)
                this.RaiseEvents();
            }
          }
          else
            break;
        }
        return index;
      }
    }

    private void DFT_AddHandler_OnDataReady(EventHandler eventHandler) => this.DataReady += eventHandler;

    private void DFT_RemoveHandler_OnDataReady(EventHandler eventHandler) => this.DataReady -= eventHandler;

    protected override void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      lock (this._monitorObject)
      {
        if (this._disposed)
          return;
        this._disposed = true;
      }
      if (!disposing)
        return;
      this._readHandle.Close();
      this._writeHandle.Close();
      this._writeClosedHandle.Close();
      this._readClosedHandle.Close();
      if (this._readWaitHandle != null)
        this._readWaitHandle.Close();
      if (this._writeWaitHandle != null)
        this._writeWaitHandle.Close();
      if (this._reader != null)
      {
        this._reader.Close();
        this._reader.WaitHandle.Close();
      }
      if (this._writer == null)
        return;
      this._writer.Close();
      this._writer.WaitHandle.Close();
    }
  }
}
