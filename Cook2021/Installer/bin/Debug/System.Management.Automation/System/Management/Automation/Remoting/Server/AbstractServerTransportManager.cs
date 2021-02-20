// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Server.AbstractServerTransportManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Management.Automation.Internal;
using System.Threading;

namespace System.Management.Automation.Remoting.Server
{
  internal abstract class AbstractServerTransportManager : BaseTransportManager
  {
    private object syncObject = new object();
    private SerializedDataStream.OnDataAvailableCallback onDataAvailable;
    private bool shouldFlushData;
    private Guid runpacePoolInstanceId;
    private Guid powerShellInstanceId;
    private RemotingDataType dataType;
    private RemotingTargetInterface targetInterface;

    protected AbstractServerTransportManager(int fragmentSize, PSRemotingCryptoHelper cryptoHelper)
      : base(cryptoHelper)
    {
      this.Fragmentor.FragmentSize = fragmentSize;
      this.onDataAvailable = new SerializedDataStream.OnDataAvailableCallback(this.OnDataAvailable);
    }

    internal void SendDataToClient<T>(RemoteDataObject<T> data, bool flush)
    {
      using (SerializedDataStream dataToBeSent = new SerializedDataStream(this.Fragmentor.FragmentSize, this.onDataAvailable))
      {
        lock (this.syncObject)
        {
          this.shouldFlushData = flush;
          this.runpacePoolInstanceId = data.RunspacePoolId;
          this.powerShellInstanceId = data.PowerShellId;
          this.dataType = data.DataType;
          this.targetInterface = data.TargetInterface;
          this.Fragmentor.Fragment<T>(data, dataToBeSent);
        }
      }
    }

    private void OnDataAvailable(byte[] dataToSend, bool isEndFragment)
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.ServerSendData, PSOpcode.Send, PSTask.None, (object) this.runpacePoolInstanceId, (object) this.powerShellInstanceId, (object) dataToSend.Length.ToString((IFormatProvider) CultureInfo.InvariantCulture), (object) (uint) this.dataType, (object) (uint) this.targetInterface);
      this.SendDataToClient(dataToSend, isEndFragment & this.shouldFlushData);
    }

    internal void SendDataToClient(RemoteDataObject psObjectData, bool flush) => this.SendDataToClient<object>((RemoteDataObject<object>) psObjectData, flush);

    internal void ReportError(int errorCode, string methodName)
    {
      string message = string.Format((IFormatProvider) CultureInfo.InvariantCulture, ResourceManagerCache.GetResourceString("remotingerroridstrings", "GeneralError"), (object) errorCode, (object) methodName);
      ThreadPool.QueueUserWorkItem((WaitCallback) (state => this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(new PSRemotingTransportException(message)
      {
        ErrorCode = errorCode
      }, TransportMethodEnum.Unknown))));
    }

    internal void RaiseClosingEvent()
    {
      if (this.Closing == null)
        return;
      this.Closing((object) this, EventArgs.Empty);
    }

    internal event EventHandler Closing;

    protected abstract void SendDataToClient(byte[] data, bool flush);

    internal abstract void Close(Exception reasonForClose);

    internal virtual void Prepare() => this.ReceivedDataCollection.AllowTwoThreadsToProcessRawData();
  }
}
