// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ReceiveDataCollection
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.IO;
using System.Management.Automation.Internal;

namespace System.Management.Automation.Remoting
{
  internal class ReceiveDataCollection : IDisposable
  {
    [TraceSource("Transport", "Traces BaseWSManTransportManager")]
    private static PSTraceSource baseTracer = PSTraceSource.GetTracer("Transport", "Traces BaseWSManTransportManager");
    private Fragmentor defragmentor;
    private IETWTracer etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Transport);
    private MemoryStream pendingDataStream;
    private MemoryStream dataToProcessStream;
    private long currentObjectId;
    private long currentFrgId;
    private int? maxReceivedObjectSize;
    private int totalReceivedObjectSizeSoFar;
    private bool isCreateByClientTM;
    private object syncObject;
    private bool isDisposed;
    private int numberOfThreadsProcessing;
    private int maxNumberOfThreadsToAllowForProcessing = 1;

    internal ReceiveDataCollection(Fragmentor defragmentor, bool createdByClientTM)
    {
      this.pendingDataStream = new MemoryStream();
      this.syncObject = new object();
      this.defragmentor = defragmentor;
      this.isCreateByClientTM = createdByClientTM;
    }

    internal int? MaximumReceivedObjectSize
    {
      set => this.maxReceivedObjectSize = value;
    }

    internal void AllowTwoThreadsToProcessRawData() => this.maxNumberOfThreadsToAllowForProcessing = 2;

    internal void ProcessRawData(
      byte[] data,
      ReceiveDataCollection.OnDataAvailableCallback callback)
    {
      lock (this.syncObject)
      {
        if (this.isDisposed)
          return;
        ++this.numberOfThreadsProcessing;
        int threadsProcessing = this.numberOfThreadsProcessing;
        int allowForProcessing = this.maxNumberOfThreadsToAllowForProcessing;
      }
      try
      {
        this.pendingDataStream.Write(data, 0, data.Length);
        while (this.pendingDataStream.Length > 21L)
        {
          byte[] buffer1 = this.pendingDataStream.GetBuffer();
          long objectId = FragmentedRemoteObject.GetObjectId(buffer1, 0);
          if (objectId <= 0L)
            throw new PSRemotingTransportException(PSRemotingErrorId.ObjectIdCannotBeLessThanZero, new object[0]);
          long fragmentId = FragmentedRemoteObject.GetFragmentId(buffer1, 0);
          bool isStartFragment = FragmentedRemoteObject.GetIsStartFragment(buffer1, 0);
          bool isEndFragment = FragmentedRemoteObject.GetIsEndFragment(buffer1, 0);
          int blobLength = FragmentedRemoteObject.GetBlobLength(buffer1, 0);
          ReceiveDataCollection.baseTracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Object Id: {0}", (object) objectId), new object[0]);
          ReceiveDataCollection.baseTracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Fragment Id: {0}", (object) fragmentId), new object[0]);
          ReceiveDataCollection.baseTracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Start Flag: {0}", (object) isStartFragment), new object[0]);
          ReceiveDataCollection.baseTracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "End Flag: {0}", (object) isEndFragment), new object[0]);
          ReceiveDataCollection.baseTracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Blob Length: {0}", (object) blobLength), new object[0]);
          int count;
          try
          {
            count = checked (21 + blobLength);
          }
          catch (OverflowException ex)
          {
            ReceiveDataCollection.baseTracer.WriteLine("Fragement too big.", new object[0]);
            this.ResetRecieveData();
            throw new PSRemotingTransportException(PSRemotingErrorId.ObjectIsTooBig, new object[0]);
          }
          if (this.pendingDataStream.Length < (long) count)
          {
            ReceiveDataCollection.baseTracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Not enough data to process packet. Data is less than expected blob length. Data length {0}. Expected Length {1}.", (object) this.pendingDataStream.Length, (object) count), new object[0]);
            return;
          }
          if (this.maxReceivedObjectSize.HasValue)
          {
            this.totalReceivedObjectSizeSoFar += count;
            if (this.totalReceivedObjectSizeSoFar < 0 || this.totalReceivedObjectSizeSoFar > this.maxReceivedObjectSize.Value)
            {
              ReceiveDataCollection.baseTracer.WriteLine("ObjectSize > MaxReceivedObjectSize. ObjectSize is {0}. MaxReceivedObjectSize is {1}", (object) this.totalReceivedObjectSizeSoFar, (object) this.maxReceivedObjectSize);
              PSRemotingTransportException transportException;
              if (this.isCreateByClientTM)
                transportException = new PSRemotingTransportException(PSRemotingErrorId.ReceivedObjectSizeExceededMaximumClient, new object[2]
                {
                  (object) this.totalReceivedObjectSizeSoFar,
                  (object) this.maxReceivedObjectSize
                });
              else
                transportException = new PSRemotingTransportException(PSRemotingErrorId.ReceivedObjectSizeExceededMaximumServer, new object[2]
                {
                  (object) this.totalReceivedObjectSizeSoFar,
                  (object) this.maxReceivedObjectSize
                });
              this.ResetRecieveData();
              throw transportException;
            }
          }
          this.pendingDataStream.Seek(0L, SeekOrigin.Begin);
          byte[] numArray = new byte[count];
          this.pendingDataStream.Read(numArray, 0, count);
          this.etwTracer.AnalyticChannel.WriteVerbose(PSEventId.ReceivedRemotingFragment, PSOpcode.Receive, PSTask.None, (object) objectId, (object) fragmentId, (object) (isStartFragment ? 1 : 0), (object) (isEndFragment ? 1 : 0), (object) (uint) blobLength, (object) new PSETWBinaryBlob(numArray, 21, blobLength));
          byte[] buffer2 = (byte[]) null;
          if ((long) count < this.pendingDataStream.Length)
          {
            buffer2 = new byte[this.pendingDataStream.Length - (long) count];
            this.pendingDataStream.Read(buffer2, 0, (int) (this.pendingDataStream.Length - (long) count));
          }
          this.pendingDataStream.Close();
          this.pendingDataStream = new MemoryStream();
          if (buffer2 != null)
            this.pendingDataStream.Write(buffer2, 0, buffer2.Length);
          if (isStartFragment)
          {
            this.currentObjectId = objectId;
            this.dataToProcessStream = new MemoryStream();
          }
          else
          {
            if (objectId != this.currentObjectId)
            {
              ReceiveDataCollection.baseTracer.WriteLine("ObjectId != CurrentObjectId", new object[0]);
              this.ResetRecieveData();
              throw new PSRemotingTransportException(PSRemotingErrorId.ObjectIdsNotMatching, new object[0]);
            }
            if (fragmentId != this.currentFrgId + 1L)
            {
              ReceiveDataCollection.baseTracer.WriteLine("Fragment Id is not in sequence.", new object[0]);
              this.ResetRecieveData();
              throw new PSRemotingTransportException(PSRemotingErrorId.FragmetIdsNotInSequence, new object[0]);
            }
          }
          this.currentFrgId = fragmentId;
          this.dataToProcessStream.Write(numArray, 21, blobLength);
          if (isEndFragment)
          {
            try
            {
              this.dataToProcessStream.Seek(0L, SeekOrigin.Begin);
              RemoteDataObject<PSObject> from = RemoteDataObject<PSObject>.CreateFrom((Stream) this.dataToProcessStream, this.defragmentor);
              ReceiveDataCollection.baseTracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Runspace Id: {0}", (object) from.RunspacePoolId), new object[0]);
              ReceiveDataCollection.baseTracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "PowerShell Id: {0}", (object) from.PowerShellId), new object[0]);
              callback(from);
            }
            finally
            {
              this.ResetRecieveData();
            }
            if (this.isDisposed)
              return;
          }
        }
        ReceiveDataCollection.baseTracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Not enough data to process. Data is less than header length. Data length is {0}. Header Length {1}.", (object) this.pendingDataStream.Length, (object) 21), new object[0]);
      }
      finally
      {
        lock (this.syncObject)
        {
          if (this.isDisposed && this.numberOfThreadsProcessing == 1)
            this.ReleaseResources();
          --this.numberOfThreadsProcessing;
        }
      }
    }

    private void ResetRecieveData()
    {
      if (this.dataToProcessStream != null)
        this.dataToProcessStream.Dispose();
      this.currentObjectId = 0L;
      this.currentFrgId = 0L;
      this.totalReceivedObjectSizeSoFar = 0;
    }

    private void ReleaseResources()
    {
      if (this.pendingDataStream != null)
      {
        this.pendingDataStream.Dispose();
        this.pendingDataStream = (MemoryStream) null;
      }
      if (this.dataToProcessStream != null)
      {
        this.dataToProcessStream.Dispose();
        this.dataToProcessStream = (MemoryStream) null;
      }
      if (this.etwTracer == null)
        return;
      this.etwTracer.Dispose();
      this.etwTracer = (IETWTracer) null;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    internal virtual void Dispose(bool isDisposing)
    {
      lock (this.syncObject)
      {
        this.isDisposed = true;
        if (this.numberOfThreadsProcessing != 0)
          return;
        this.ReleaseResources();
      }
    }

    internal delegate void OnDataAvailableCallback(RemoteDataObject<PSObject> data);
  }
}
