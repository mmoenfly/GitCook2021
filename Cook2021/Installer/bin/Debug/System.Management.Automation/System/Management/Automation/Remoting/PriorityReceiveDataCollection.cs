// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.PriorityReceiveDataCollection
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Xml;

namespace System.Management.Automation.Remoting
{
  internal class PriorityReceiveDataCollection : IDisposable
  {
    private Fragmentor defragmentor;
    private ReceiveDataCollection[] recvdData;
    private bool isCreateByClientTM;

    internal PriorityReceiveDataCollection(Fragmentor defragmentor, bool createdByClientTM)
    {
      this.defragmentor = defragmentor;
      string[] names = Enum.GetNames(typeof (DataPriorityType));
      this.recvdData = new ReceiveDataCollection[names.Length];
      for (int index = 0; index < names.Length; ++index)
        this.recvdData[index] = new ReceiveDataCollection(defragmentor, createdByClientTM);
      this.isCreateByClientTM = createdByClientTM;
    }

    internal int? MaximumReceivedDataSize
    {
      set => this.defragmentor.DeserializationContext.MaximumAllowedMemory = value;
    }

    internal int? MaximumReceivedObjectSize
    {
      set
      {
        foreach (ReceiveDataCollection receiveDataCollection in this.recvdData)
          receiveDataCollection.MaximumReceivedObjectSize = value;
      }
    }

    internal void AllowTwoThreadsToProcessRawData()
    {
      for (int index = 0; index < this.recvdData.Length; ++index)
        this.recvdData[index].AllowTwoThreadsToProcessRawData();
    }

    internal void ProcessRawData(
      byte[] data,
      DataPriorityType priorityType,
      ReceiveDataCollection.OnDataAvailableCallback callback)
    {
      try
      {
        this.defragmentor.DeserializationContext.LogExtraMemoryUsage(data.Length);
      }
      catch (XmlException ex)
      {
        PSRemotingTransportException transportException;
        if (this.isCreateByClientTM)
          transportException = new PSRemotingTransportException(PSRemotingErrorId.ReceivedDataSizeExceededMaximumClient, new object[1]
          {
            (object) this.defragmentor.DeserializationContext.MaximumAllowedMemory.Value
          });
        else
          transportException = new PSRemotingTransportException(PSRemotingErrorId.ReceivedDataSizeExceededMaximumServer, new object[1]
          {
            (object) this.defragmentor.DeserializationContext.MaximumAllowedMemory.Value
          });
        throw transportException;
      }
      this.recvdData[(int) priorityType].ProcessRawData(data, callback);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    internal virtual void Dispose(bool isDisposing)
    {
      if (this.recvdData == null)
        return;
      for (int index = 0; index < this.recvdData.Length; ++index)
        this.recvdData[index].Dispose();
    }
  }
}
