// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSDataCollectionReader`2
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation.Internal
{
  internal class PSDataCollectionReader<DataStoreType, ReturnType> : ObjectReaderBase<ReturnType>
  {
    private PSDataCollectionEnumerator<DataStoreType> enumerator;

    public PSDataCollectionReader(PSDataCollectionStream<DataStoreType> stream)
      : base((ObjectStreamBase) stream)
      => this.enumerator = (PSDataCollectionEnumerator<DataStoreType>) stream.ObjectStore.GetEnumerator();

    public override Collection<ReturnType> Read(int count) => throw new NotSupportedException();

    public override ReturnType Read()
    {
      using (ObjectReaderBase<ReturnType>._trace.TraceMethod())
      {
        object current = (object) AutomationNull.Value;
        if (this.enumerator.MoveNext())
          current = this.enumerator.Current;
        return this.ConvertToReturnType(current);
      }
    }

    public override Collection<ReturnType> ReadToEnd() => throw new NotSupportedException();

    public override Collection<ReturnType> NonBlockingRead() => this.NonBlockingRead(int.MaxValue);

    public override Collection<ReturnType> NonBlockingRead(int maxRequested)
    {
      if (maxRequested < 0)
        throw ObjectReaderBase<ReturnType>._trace.NewArgumentOutOfRangeException(nameof (maxRequested), (object) maxRequested);
      if (maxRequested == 0)
        return new Collection<ReturnType>();
      Collection<ReturnType> collection = new Collection<ReturnType>();
      int num = maxRequested;
      while (num > 0 && this.enumerator.MoveNext(false))
        collection.Add(this.ConvertToReturnType(this.enumerator.Current));
      return collection;
    }

    public override ReturnType Peek() => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      this._stream.Close();
    }

    private ReturnType ConvertToReturnType(object inputObject)
    {
      Type o = typeof (ReturnType);
      if (!typeof (PSObject).Equals(o) && !typeof (object).Equals(o))
        throw ObjectReaderBase<ReturnType>._trace.NewNotSupportedException();
      ReturnType result = default (ReturnType);
      LanguagePrimitives.TryConvertTo<ReturnType>(inputObject, out result);
      return result;
    }
  }
}
