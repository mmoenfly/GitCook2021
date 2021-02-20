// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Client.BaseClientTransportManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Threading;

namespace System.Management.Automation.Remoting.Client
{
  internal abstract class BaseClientTransportManager : BaseTransportManager, IDisposable
  {
    [TraceSource("ClientTransport", "Traces ClientTransportManager")]
    protected static PSTraceSource tracer = PSTraceSource.GetTracer("ClientTransport", "Traces ClientTransportManager");
    protected bool isClosed;
    protected object syncObject = new object();
    protected PrioritySendDataCollection dataToBeSent;
    private Queue<BaseClientTransportManager.ReceivedDataInformation> callbackNotificationQueue;
    private ReceiveDataCollection.OnDataAvailableCallback onDataAvailableCallback;
    private bool isServicingCallbacks;
    private Guid runspacePoolInstanceId;

    protected BaseClientTransportManager(Guid runspaceId, PSRemotingCryptoHelper cryptoHelper)
      : base(cryptoHelper)
    {
      this.runspacePoolInstanceId = runspaceId;
      this.dataToBeSent = new PrioritySendDataCollection();
      this.onDataAvailableCallback = new ReceiveDataCollection.OnDataAvailableCallback(this.OnDataAvailableHandler);
      this.callbackNotificationQueue = new Queue<BaseClientTransportManager.ReceivedDataInformation>();
    }

    internal event EventHandler<EventArgs> ConnectCompleted;

    internal event EventHandler<EventArgs> CloseCompleted;

    internal PrioritySendDataCollection DataToBeSentCollection => this.dataToBeSent;

    internal Guid RunspacePoolInstanceId => this.runspacePoolInstanceId;

    internal void RaiseConnectCompleted()
    {
      if (this.ConnectCompleted == null)
        return;
      this.ConnectCompleted((object) this, EventArgs.Empty);
    }

    internal void RaiseCloseCompleted()
    {
      if (this.CloseCompleted == null)
        return;
      this.CloseCompleted((object) this, EventArgs.Empty);
    }

    internal override void ProcessRawData(byte[] data, string stream)
    {
      if (this.isClosed)
        return;
      try
      {
        this.ProcessRawData(data, stream, this.onDataAvailableCallback);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        BaseClientTransportManager.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Exception processing data. {0}", (object) ex.Message), new object[0]);
        this.EnqueueAndStartProcessingThread((RemoteDataObject<PSObject>) null, new PSRemotingTransportException(ex.Message), (object) null);
      }
    }

    private void OnDataAvailableHandler(RemoteDataObject<PSObject> remoteObject) => this.EnqueueAndStartProcessingThread(remoteObject, (PSRemotingTransportException) null, (object) null);

    internal void EnqueueAndStartProcessingThread(
      RemoteDataObject<PSObject> remoteObject,
      PSRemotingTransportException transportException,
      object privateData)
    {
      if (this.isClosed)
        return;
      lock (this.callbackNotificationQueue)
      {
        if (remoteObject != null || transportException != null || privateData != null)
        {
          BaseClientTransportManager.ReceivedDataInformation receivedDataInformation = new BaseClientTransportManager.ReceivedDataInformation();
          receivedDataInformation.remoteObject = remoteObject;
          receivedDataInformation.transportException = transportException;
          receivedDataInformation.privateData = privateData;
          if (remoteObject != null && (remoteObject.DataType == RemotingDataType.PublicKey || remoteObject.DataType == RemotingDataType.EncryptedSessionKey || remoteObject.DataType == RemotingDataType.PublicKeyRequest))
            this.CryptoHelper.Session.BaseSessionDataStructureHandler.RaiseKeyExchangeMessageReceived(remoteObject);
          else
            this.callbackNotificationQueue.Enqueue(receivedDataInformation);
        }
        if (this.isServicingCallbacks || this.callbackNotificationQueue.Count <= 0)
          return;
        this.isServicingCallbacks = true;
        ThreadPool.QueueUserWorkItem(new WaitCallback(this.ServicePendingCallbacks));
      }
    }

    internal void ServicePendingCallbacks(object objectToProcess)
    {
      BaseClientTransportManager.tracer.WriteLine("ServicePendingCallbacks thread is starting", new object[0]);
      BaseTransportManager.ETWTracer.ReplaceActivityIdForCurrentThread(this.runspacePoolInstanceId, PSEventId.OperationalTransferEventRunspacePool, PSEventId.AnalyticTransferEventRunspacePool, PSKeyword.Transport, PSTask.None);
      try
      {
        while (!this.isClosed)
        {
          BaseClientTransportManager.ReceivedDataInformation receivedDataInformation = (BaseClientTransportManager.ReceivedDataInformation) null;
          lock (this.callbackNotificationQueue)
          {
            if (this.callbackNotificationQueue.Count <= 0)
              break;
            receivedDataInformation = this.callbackNotificationQueue.Dequeue();
          }
          if (receivedDataInformation != null)
          {
            if (receivedDataInformation.transportException != null)
            {
              this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(receivedDataInformation.transportException, TransportMethodEnum.ReceiveShellOutputEx));
              break;
            }
            if (receivedDataInformation.privateData != null)
              this.ProcessPrivateData(receivedDataInformation.privateData);
            else
              this.OnDataAvailableCallback(receivedDataInformation.remoteObject);
          }
        }
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        BaseClientTransportManager.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Exception processing data. {0}", (object) ex.Message), new object[0]);
        this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(new PSRemotingTransportException(ex.Message), TransportMethodEnum.ReceiveShellOutputEx));
      }
      finally
      {
        lock (this.callbackNotificationQueue)
        {
          BaseClientTransportManager.tracer.WriteLine("ServicePendingCallbacks thread is exiting", new object[0]);
          this.isServicingCallbacks = false;
          this.EnqueueAndStartProcessingThread((RemoteDataObject<PSObject>) null, (PSRemotingTransportException) null, (object) null);
        }
      }
    }

    internal virtual void ProcessPrivateData(object privateData)
    {
    }

    internal abstract void ConnectAsync();

    internal virtual void CloseAsync() => this.dataToBeSent.Clear();

    ~BaseClientTransportManager()
    {
      if (this.isClosed)
      {
        this.Dispose(false);
      }
      else
      {
        this.CloseCompleted += (EventHandler<EventArgs>) ((source, args) => this.Dispose(false));
        this.CloseAsync();
      }
    }

    internal override void Dispose(bool isDisposing)
    {
      this.ConnectCompleted = (EventHandler<EventArgs>) null;
      this.CloseCompleted = (EventHandler<EventArgs>) null;
      base.Dispose(isDisposing);
    }

    internal class ReceivedDataInformation
    {
      internal RemoteDataObject<PSObject> remoteObject;
      internal PSRemotingTransportException transportException;
      internal object privateData;
    }
  }
}
