// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSDataCollectionStream`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace System.Management.Automation.Internal
{
  internal sealed class PSDataCollectionStream<T> : ObjectStreamBase
  {
    private PSDataCollection<T> _objects;
    private Guid psInstanceId;
    private bool isOpen;
    private PipelineWriter _writer;
    private PipelineReader<object> _objectReader;
    private PipelineReader<PSObject> _psobjectReader;
    private PipelineReader<object> _objectReaderForPipeline;
    private PipelineReader<PSObject> _psobjectReaderForPipeline;
    private object _syncObject = new object();
    private bool _disposed;

    internal PSDataCollectionStream(Guid psInstanceId, PSDataCollection<T> storeToUse)
    {
      using (ObjectStreamBase._trace.TraceConstructor((object) this))
      {
        this._objects = storeToUse != null ? storeToUse : throw ObjectStreamBase._trace.NewArgumentNullException(nameof (storeToUse));
        this.psInstanceId = psInstanceId;
        this.isOpen = true;
        storeToUse.AddRef();
        storeToUse.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleDataAdded);
        storeToUse.Completed += new EventHandler(this.HandleClosed);
      }
    }

    internal PSDataCollection<T> ObjectStore => this._objects;

    internal override int Count
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
          return this._objects.Count;
      }
    }

    internal override bool EndOfPipeline
    {
      get
      {
        lock (this._syncObject)
          return this._objects.Count == 0 && !this.isOpen;
      }
    }

    internal override bool IsOpen
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
          return this.isOpen && this._objects.IsOpen;
      }
    }

    internal override int MaxCapacity => throw ObjectStreamBase._trace.NewNotSupportedException();

    internal override PipelineReader<object> ObjectReader
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
        {
          if (this._objectReader == null)
          {
            lock (this._syncObject)
            {
              if (this._objectReader == null)
                this._objectReader = (PipelineReader<object>) new PSDataCollectionReader<T, object>(this);
            }
          }
          return this._objectReader;
        }
      }
    }

    internal PipelineReader<object> GetObjectReaderForPipeline(
      string computerName,
      Guid runspaceId)
    {
      using (ObjectStreamBase._trace.TraceProperty())
      {
        if (this._objectReaderForPipeline == null)
        {
          lock (this._syncObject)
          {
            if (this._objectReaderForPipeline == null)
              this._objectReaderForPipeline = (PipelineReader<object>) new PSDataCollectionPipelineReader<T, object>(this, computerName, runspaceId);
          }
        }
        return this._objectReaderForPipeline;
      }
    }

    internal override PipelineReader<PSObject> PSObjectReader
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
        {
          if (this._psobjectReader == null)
          {
            lock (this._syncObject)
            {
              if (this._psobjectReader == null)
                this._psobjectReader = (PipelineReader<PSObject>) new PSDataCollectionReader<T, PSObject>(this);
            }
          }
          return this._psobjectReader;
        }
      }
    }

    internal PipelineReader<PSObject> GetPSObjectReaderForPipeline(
      string computerName,
      Guid runspaceId)
    {
      using (ObjectStreamBase._trace.TraceProperty())
      {
        if (this._psobjectReaderForPipeline == null)
        {
          lock (this._syncObject)
          {
            if (this._psobjectReaderForPipeline == null)
              this._psobjectReaderForPipeline = (PipelineReader<PSObject>) new PSDataCollectionPipelineReader<T, PSObject>(this, computerName, runspaceId);
          }
        }
        return this._psobjectReaderForPipeline;
      }
    }

    internal override PipelineWriter ObjectWriter
    {
      get
      {
        using (ObjectStreamBase._trace.TraceProperty())
        {
          if (this._writer == null)
          {
            lock (this._syncObject)
            {
              if (this._writer == null)
                this._writer = (PipelineWriter) new PSDataCollectionWriter<T>(this);
            }
          }
          return this._writer;
        }
      }
    }

    internal override WaitHandle ReadHandle => this._objects.WaitHandle;

    internal override int Write(object obj, bool enumerateCollection)
    {
      using (ObjectStreamBase._trace.TraceMethod())
      {
        if (obj == AutomationNull.Value)
          return 0;
        if (!this.IsOpen)
        {
          Exception exceptionRecord = (Exception) new PipelineClosedException(ResourceManagerCache.GetResourceString("PSDataBufferStrings", "WriteToClosedBuffer"));
          ObjectStreamBase._trace.TraceException(exceptionRecord);
          throw exceptionRecord;
        }
        Collection<T> collection = new Collection<T>();
        IEnumerable enumerable = (IEnumerable) null;
        if (enumerateCollection)
          enumerable = LanguagePrimitives.GetEnumerable(obj);
        if (enumerable == null)
        {
          collection.Add((T) LanguagePrimitives.ConvertTo(obj, typeof (T), (IFormatProvider) CultureInfo.InvariantCulture));
        }
        else
        {
          foreach (object obj1 in enumerable)
          {
            if (AutomationNull.Value != obj1)
              collection.Add((T) LanguagePrimitives.ConvertTo(obj, typeof (T), (IFormatProvider) CultureInfo.InvariantCulture));
          }
        }
        this._objects.InternalAddRange(this.psInstanceId, (ICollection) collection);
        return collection.Count;
      }
    }

    internal override void Close()
    {
      bool flag = false;
      lock (this._syncObject)
      {
        if (this.isOpen)
        {
          this._objects.DecrementRef();
          this._objects.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleDataAdded);
          this._objects.Completed -= new EventHandler(this.HandleClosed);
          flag = true;
          this.isOpen = false;
        }
      }
      if (!flag)
        return;
      this.FireDataReadyEvent((object) this, EventArgs.Empty);
    }

    private void HandleClosed(object sender, EventArgs e) => this.Close();

    private void HandleDataAdded(object sender, DataAddedEventArgs e) => this.FireDataReadyEvent((object) this, new EventArgs());

    protected override void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      lock (this._syncObject)
      {
        if (this._disposed)
          return;
        this._disposed = true;
      }
      if (!disposing)
        return;
      this._objects.Dispose();
      this.Close();
      if (this._objectReaderForPipeline != null)
        ((ObjectReaderBase<object>) this._objectReaderForPipeline).Dispose();
      if (this._psobjectReaderForPipeline == null)
        return;
      ((ObjectReaderBase<PSObject>) this._psobjectReaderForPipeline).Dispose();
    }
  }
}
