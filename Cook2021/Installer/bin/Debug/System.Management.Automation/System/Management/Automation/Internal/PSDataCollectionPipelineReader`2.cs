// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSDataCollectionPipelineReader`2
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation.Internal
{
  internal class PSDataCollectionPipelineReader<DataStoreType, ReturnType> : 
    ObjectReaderBase<ReturnType>
  {
    private PSDataCollection<DataStoreType> datastore;
    private string computerName;
    private Guid runspaceId;

    internal PSDataCollectionPipelineReader(
      PSDataCollectionStream<DataStoreType> stream,
      string computerName,
      Guid runspaceId)
      : base((ObjectStreamBase) stream)
    {
      this.datastore = stream.ObjectStore;
      this.computerName = computerName;
      this.runspaceId = runspaceId;
    }

    internal string ComputerName => this.computerName;

    internal Guid RunspaceId => this.runspaceId;

    public override Collection<ReturnType> Read(int count) => throw new NotSupportedException();

    public override ReturnType Read()
    {
      using (ObjectReaderBase<ReturnType>._trace.TraceMethod())
      {
        object inputObject = (object) AutomationNull.Value;
        if (this.datastore.Count > 0)
          inputObject = (object) this.datastore.ReadAndRemove(1);
        return this.ConvertToReturnType(inputObject);
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
      for (int index = maxRequested; index > 0 && this.datastore.Count > 0; --index)
        collection.Add(this.ConvertToReturnType((object) this.datastore.ReadAndRemove(1)[0]));
      return collection;
    }

    public override ReturnType Peek() => throw new NotSupportedException();

    private ReturnType ConvertToReturnType(object inputObject)
    {
      Type o = typeof (ReturnType);
      if (!typeof (PSObject).Equals(o) && !typeof (object).Equals(o))
        throw ObjectReaderBase<ReturnType>._trace.NewNotSupportedException();
      ReturnType result = default (ReturnType);
      LanguagePrimitives.TryConvertTo<ReturnType>(inputObject, out result);
      return result;
    }

    protected override void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      this.datastore.Dispose();
    }
  }
}
