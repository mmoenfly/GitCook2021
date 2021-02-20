// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Client.OutOfProcessClientCommandTransportManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces.Internal;
using System.Timers;

namespace System.Management.Automation.Remoting.Client
{
  internal class OutOfProcessClientCommandTransportManager : BaseClientCommandTransportManager
  {
    private OutOfProcessTextWriter stdInWriter;
    private PrioritySendDataCollection.OnDataAvailableCallback onDataAvailableToSendCallback;
    private Timer signalTimeOutTimer;

    internal OutOfProcessClientCommandTransportManager(
      ClientRemotePowerShell cmd,
      bool noInput,
      OutOfProcessClientSessionTransportManager sessnTM,
      OutOfProcessTextWriter stdInWriter)
      : base(cmd, sessnTM.CryptoHelper, (BaseClientSessionTransportManager) sessnTM)
    {
      this.stdInWriter = stdInWriter;
      this.onDataAvailableToSendCallback = new PrioritySendDataCollection.OnDataAvailableCallback(this.OnDataAvailableCallback);
      this.signalTimeOutTimer = new Timer(60000.0);
      this.signalTimeOutTimer.Elapsed += new ElapsedEventHandler(this.OnSignalTimeOutTimerElapsed);
    }

    internal override void ConnectAsync()
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCreateCommand, PSOpcode.Connect, PSTask.CreateRunspace, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId);
      this.stdInWriter.WriteLine(OutOfProcessUtils.CreateCommandPacket(this.powershellInstanceId));
    }

    internal override void CloseAsync()
    {
      lock (this.syncObject)
      {
        if (this.isClosed)
          return;
        this.isClosed = true;
      }
      base.CloseAsync();
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCloseCommand, PSOpcode.Disconnect, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId);
      this.stdInWriter.WriteLine(OutOfProcessUtils.CreateClosePacket(this.powershellInstanceId));
    }

    internal override void SendStopSignal()
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSignal, PSOpcode.Disconnect, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId, (object) "stopsignal");
      base.CloseAsync();
      this.stdInWriter.WriteLine(OutOfProcessUtils.CreateSignalPacket(this.powershellInstanceId));
      this.signalTimeOutTimer.Start();
    }

    internal override void Dispose(bool isDisposing)
    {
      base.Dispose(isDisposing);
      this.signalTimeOutTimer.Dispose();
    }

    internal void OnCreateCmdCompleted()
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCreateCommandCallbackReceived, PSOpcode.Connect, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId);
      lock (this.syncObject)
      {
        if (this.isClosed)
          BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
        else
          this.SendOneItem();
      }
    }

    internal void OnRemoteCmdSendCompleted()
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSendShellInputExCallbackReceived, PSOpcode.Connect, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId);
      if (this.isClosed)
        BaseClientTransportManager.tracer.WriteLine("Client Command TM: Transport manager is closed. So returning", new object[0]);
      else
        this.SendOneItem();
    }

    internal void OnRemoteCmdDataReceived(byte[] rawData, string stream)
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManReceiveShellOutputExCallbackReceived, PSOpcode.Receive, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId, (object) rawData.Length.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      if (this.isClosed)
        BaseClientTransportManager.tracer.WriteLine("Client Command TM: Transport manager is closed. So returning", new object[0]);
      else
        this.ProcessRawData(rawData, stream);
    }

    internal void OnRemoteCmdSignalCompleted()
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSignalCallbackReceived, PSOpcode.Disconnect, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId);
      if (this.isClosed)
        return;
      this.EnqueueAndStartProcessingThread((RemoteDataObject<PSObject>) null, (PSRemotingTransportException) null, (object) true);
    }

    internal void OnSignalTimeOutTimerElapsed(object source, ElapsedEventArgs e)
    {
      this.signalTimeOutTimer.Stop();
      if (this.isClosed)
        return;
      this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.IPCSignalTimedOut, new object[0]), TransportMethodEnum.ReceiveShellOutputEx));
    }

    internal override void ProcessPrivateData(object privateData)
    {
      if (!(bool) privateData)
        return;
      this.RaiseSignalCompleted();
    }

    internal void OnCloseCmdCompleted()
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCloseCommandCallbackReceived, PSOpcode.Disconnect, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId);
      this.RaiseCloseCompleted();
    }

    private void SendOneItem()
    {
      DataPriorityType priorityType = DataPriorityType.Default;
      byte[] data = this.serializedPipeline.Length <= 0L ? this.dataToBeSent.ReadOrRegisterCallback(this.onDataAvailableToSendCallback, out priorityType) : this.serializedPipeline.ReadOrRegisterCallback((SerializedDataStream.OnDataAvailableCallback) null);
      if (data == null)
        return;
      this.SendData(data, priorityType);
    }

    private void SendData(byte[] data, DataPriorityType priorityType)
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSendShellInputEx, PSOpcode.Send, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId, (object) data.Length.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      lock (this.syncObject)
      {
        if (this.isClosed)
          return;
        this.stdInWriter.WriteLine(OutOfProcessUtils.CreateDataPacket(data, priorityType, this.powershellInstanceId));
      }
    }

    private void OnDataAvailableCallback(byte[] data, DataPriorityType priorityType)
    {
      BaseClientTransportManager.tracer.WriteLine("Received data from dataToBeSent store.", new object[0]);
      this.SendData(data, priorityType);
    }
  }
}
