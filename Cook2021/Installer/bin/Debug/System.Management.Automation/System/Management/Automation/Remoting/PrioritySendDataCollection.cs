// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.PrioritySendDataCollection
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting
{
  internal class PrioritySendDataCollection
  {
    [TraceSource("PrioritySendDataCollection", "PrioritySendDataCollection")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PrioritySendDataCollection), nameof (PrioritySendDataCollection));
    private SerializedDataStream[] dataToBeSent;
    private Fragmentor fragmentor;
    private object[] syncObjects;
    private PrioritySendDataCollection.OnDataAvailableCallback onDataAvailableCallback;
    private SerializedDataStream.OnDataAvailableCallback onSendCollectionDataAvailable;
    private bool isHandlingCallback;
    private object readSyncObject = new object();

    internal PrioritySendDataCollection() => this.onSendCollectionDataAvailable = new SerializedDataStream.OnDataAvailableCallback(this.OnDataAvailable);

    internal Fragmentor Fragmentor
    {
      get => this.fragmentor;
      set
      {
        this.fragmentor = value;
        string[] names = Enum.GetNames(typeof (DataPriorityType));
        this.dataToBeSent = new SerializedDataStream[names.Length];
        this.syncObjects = new object[names.Length];
        for (int index = 0; index < names.Length; ++index)
        {
          this.dataToBeSent[index] = new SerializedDataStream(this.fragmentor.FragmentSize);
          this.syncObjects[index] = new object();
        }
      }
    }

    internal void Add<T>(RemoteDataObject<T> data, DataPriorityType priority)
    {
      using (PrioritySendDataCollection.tracer.TraceMethod((object) data.DataType))
      {
        lock (this.syncObjects[(int) priority])
          this.fragmentor.Fragment<T>(data, this.dataToBeSent[(int) priority]);
      }
    }

    internal void Add<T>(RemoteDataObject<T> data) => this.Add<T>(data, DataPriorityType.Default);

    internal void Clear()
    {
      lock (this.syncObjects[1])
        this.dataToBeSent[1].Dispose();
      lock (this.syncObjects[0])
        this.dataToBeSent[0].Dispose();
    }

    internal byte[] ReadOrRegisterCallback(
      PrioritySendDataCollection.OnDataAvailableCallback callback,
      out DataPriorityType priorityType)
    {
      lock (this.readSyncObject)
      {
        priorityType = DataPriorityType.Default;
        byte[] numArray = this.dataToBeSent[1].ReadOrRegisterCallback(this.onSendCollectionDataAvailable);
        priorityType = DataPriorityType.PromptResponse;
        if (numArray == null)
        {
          numArray = this.dataToBeSent[0].ReadOrRegisterCallback(this.onSendCollectionDataAvailable);
          priorityType = DataPriorityType.Default;
        }
        if (numArray == null)
          this.onDataAvailableCallback = callback;
        return numArray;
      }
    }

    private void OnDataAvailable(byte[] data, bool isEndFragment)
    {
      lock (this.readSyncObject)
      {
        if (this.isHandlingCallback)
          return;
        this.isHandlingCallback = true;
      }
      if (this.onDataAvailableCallback != null)
      {
        DataPriorityType priorityType;
        byte[] data1 = this.ReadOrRegisterCallback(this.onDataAvailableCallback, out priorityType);
        if (data1 != null)
        {
          PrioritySendDataCollection.OnDataAvailableCallback availableCallback = this.onDataAvailableCallback;
          this.onDataAvailableCallback = (PrioritySendDataCollection.OnDataAvailableCallback) null;
          availableCallback(data1, priorityType);
        }
      }
      this.isHandlingCallback = false;
    }

    internal delegate void OnDataAvailableCallback(byte[] data, DataPriorityType priorityType);
  }
}
