// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Client.WSManClientCommandTransportManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Runspaces.Internal;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Management.Automation.Remoting.Client
{
  internal sealed class WSManClientCommandTransportManager : BaseClientCommandTransportManager
  {
    internal const string StopSignal = "powershell/signal/crtl_c";
    private IntPtr wsManShellOperationHandle;
    private IntPtr wsManCmdOperationHandle;
    private IntPtr cmdSignalOperationHandle;
    private IntPtr wsManRecieveOperationHandle;
    private IntPtr wsManSendOperationHandle;
    private long cmdContextId;
    private PrioritySendDataCollection.OnDataAvailableCallback onDataAvailableToSendCallback;
    private bool shouldStartReceivingData;
    private bool isCreateCallbackReceived;
    private bool isStopSignalPending;
    private WSManNativeApi.WSManShellAsync createCmdCompleted;
    private WSManNativeApi.WSManShellAsync receivedFromRemote;
    private WSManNativeApi.WSManShellAsync sendToRemoteCompleted;
    private GCHandle createCmdCompletedGCHandle;
    private WSManNativeApi.WSManShellAsync closeCmdCompleted;
    private WSManNativeApi.WSManShellAsync signalCmdCompleted;
    private static WSManNativeApi.WSManShellAsyncCallback cmdCreateCallback;
    private static WSManNativeApi.WSManShellAsyncCallback cmdCloseCallback;
    private static WSManNativeApi.WSManShellAsyncCallback cmdReceiveCallback;
    private static WSManNativeApi.WSManShellAsyncCallback cmdSendCallback;
    private static WSManNativeApi.WSManShellAsyncCallback cmdSignalCallback;
    private static Delegate commandCodeSendRedirect = (Delegate) null;
    private static Delegate commandSendRedirect = (Delegate) null;
    private static Dictionary<long, WSManClientCommandTransportManager> CmdTMHandles = new Dictionary<long, WSManClientCommandTransportManager>();
    private static long CmdTMSeed;

    static WSManClientCommandTransportManager()
    {
      WSManClientCommandTransportManager.cmdCreateCallback = new WSManNativeApi.WSManShellAsyncCallback(new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnCreateCmdCompleted));
      WSManClientCommandTransportManager.cmdCloseCallback = new WSManNativeApi.WSManShellAsyncCallback(new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnCloseCmdCompleted));
      WSManClientCommandTransportManager.cmdReceiveCallback = new WSManNativeApi.WSManShellAsyncCallback(new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnRemoteCmdDataReceived));
      WSManClientCommandTransportManager.cmdSendCallback = new WSManNativeApi.WSManShellAsyncCallback(new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnRemoteCmdSendCompleted));
      WSManClientCommandTransportManager.cmdSignalCallback = new WSManNativeApi.WSManShellAsyncCallback(new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnRemoteCmdSignalCompleted));
    }

    internal WSManClientCommandTransportManager(
      WSManConnectionInfo connectionInfo,
      IntPtr wsManShellOperationHandle,
      ClientRemotePowerShell shell,
      bool noInput,
      WSManClientSessionTransportManager sessnTM)
      : base(shell, sessnTM.CryptoHelper, (BaseClientSessionTransportManager) sessnTM)
    {
      this.wsManShellOperationHandle = wsManShellOperationHandle;
      this.ReceivedDataCollection.MaximumReceivedDataSize = connectionInfo.MaximumReceivedDataSizePerCommand;
      this.ReceivedDataCollection.MaximumReceivedObjectSize = connectionInfo.MaximumReceivedObjectSize;
      this.onDataAvailableToSendCallback = new PrioritySendDataCollection.OnDataAvailableCallback(this.OnDataAvailableCallback);
    }

    internal override void ConnectAsync()
    {
      byte[] firstArgument = this.serializedPipeline.ReadOrRegisterCallback((SerializedDataStream.OnDataAvailableCallback) null);
      if (firstArgument != null)
      {
        bool flag = true;
        if ((object) WSManClientCommandTransportManager.commandCodeSendRedirect != null)
        {
          object[] objArray = new object[2]
          {
            null,
            (object) firstArgument
          };
          flag = (bool) WSManClientCommandTransportManager.commandCodeSendRedirect.DynamicInvoke(objArray);
          firstArgument = (byte[]) objArray[0];
        }
        if (!flag)
          return;
        WSManNativeApi.WSManCommandArgSet manCommandArgSet = new WSManNativeApi.WSManCommandArgSet(firstArgument);
        this.cmdContextId = WSManClientCommandTransportManager.GetNextCmdTMHandleId();
        WSManClientCommandTransportManager.AddCmdTransportManager(this.cmdContextId, this);
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCreateCommand, PSOpcode.Connect, PSTask.CreateRunspace, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId);
        this.createCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), WSManClientCommandTransportManager.cmdCreateCallback);
        this.createCmdCompletedGCHandle = GCHandle.Alloc((object) this.createCmdCompleted);
        using (manCommandArgSet)
        {
          lock (this.syncObject)
          {
            if (!this.isClosed)
            {
              WSManNativeApi.WSManRunShellCommandEx(this.wsManShellOperationHandle, 0, " ", (IntPtr) manCommandArgSet, IntPtr.Zero, (IntPtr) this.createCmdCompleted, ref this.wsManCmdOperationHandle);
              BaseClientTransportManager.tracer.WriteLine("Started cmd with command context : {0} Operation context: {1}", (object) this.cmdContextId, (object) this.wsManCmdOperationHandle);
            }
          }
        }
      }
      if (!(this.wsManCmdOperationHandle == IntPtr.Zero))
        return;
      this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.RunShellCommandExFailed, new object[0]), TransportMethodEnum.RunShellCommandEx));
    }

    internal override void SendStopSignal()
    {
      lock (this.syncObject)
      {
        if (this.isClosed)
          return;
        if (!this.isCreateCallbackReceived)
        {
          this.isStopSignalPending = true;
        }
        else
        {
          this.isStopSignalPending = false;
          BaseClientTransportManager.tracer.WriteLine("Sending stop signal with command context: {0} Operation Context {1}", (object) this.cmdContextId, (object) this.wsManCmdOperationHandle);
          BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSignal, PSOpcode.Disconnect, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId, (object) "powershell/signal/crtl_c");
          this.signalCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), WSManClientCommandTransportManager.cmdSignalCallback);
          WSManNativeApi.WSManSignalShellEx(this.wsManShellOperationHandle, this.wsManCmdOperationHandle, 0, "powershell/signal/crtl_c", (IntPtr) this.signalCmdCompleted, ref this.cmdSignalOperationHandle);
        }
      }
    }

    internal override void CloseAsync()
    {
      BaseClientTransportManager.tracer.WriteLine("Closing command with command context: {0} Operation Context {1}", (object) this.cmdContextId, (object) this.wsManCmdOperationHandle);
      bool flag = false;
      lock (this.syncObject)
      {
        if (this.isClosed)
          return;
        this.isClosed = true;
        if (IntPtr.Zero == this.wsManCmdOperationHandle)
          flag = true;
      }
      base.CloseAsync();
      if (flag)
      {
        try
        {
          this.RaiseCloseCompleted();
        }
        finally
        {
          WSManClientCommandTransportManager.RemoveCmdTransportManager(this.cmdContextId);
        }
      }
      else
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCloseCommand, PSOpcode.Disconnect, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId);
        this.closeCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), WSManClientCommandTransportManager.cmdCloseCallback);
        WSManNativeApi.WSManCloseCommand(this.wsManCmdOperationHandle, 0, (IntPtr) this.closeCmdCompleted);
      }
    }

    internal override void RaiseErrorHandler(TransportErrorOccuredEventArgs eventArgs)
    {
      BaseTransportManager.ETWTracer.OperationalChannel.WriteError(PSEventId.TransportError, PSOpcode.Open, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId, (object) eventArgs.Exception.ErrorCode.ToString((IFormatProvider) CultureInfo.InvariantCulture), (object) eventArgs.Exception.Message, (object) eventArgs.Exception.StackTrace);
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteError(PSEventId.TransportError_Analytic, PSOpcode.Open, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId, (object) eventArgs.Exception.ErrorCode.ToString((IFormatProvider) CultureInfo.InvariantCulture), (object) eventArgs.Exception.Message, (object) eventArgs.Exception.StackTrace);
      base.RaiseErrorHandler(eventArgs);
    }

    internal override void ProcessPrivateData(object privateData)
    {
      if (!(bool) privateData)
        return;
      this.RaiseSignalCompleted();
    }

    internal void ClearReceiveOrSendResources(int flags, bool shouldClearSend)
    {
      if (shouldClearSend)
      {
        if (this.sendToRemoteCompleted != null)
        {
          this.sendToRemoteCompleted.Dispose();
          this.sendToRemoteCompleted = (WSManNativeApi.WSManShellAsync) null;
        }
        if (!(IntPtr.Zero != this.wsManSendOperationHandle))
          return;
        WSManNativeApi.WSManCloseOperation(this.wsManSendOperationHandle, 0);
        this.wsManSendOperationHandle = IntPtr.Zero;
      }
      else
      {
        if (flags != 1)
          return;
        if (IntPtr.Zero != this.wsManRecieveOperationHandle)
        {
          WSManNativeApi.WSManCloseOperation(this.wsManRecieveOperationHandle, 0);
          this.wsManRecieveOperationHandle = IntPtr.Zero;
        }
        if (this.receivedFromRemote == null)
          return;
        this.receivedFromRemote.Dispose();
        this.receivedFromRemote = (WSManNativeApi.WSManShellAsync) null;
      }
    }

    private static void OnCreateCmdCompleted(
      IntPtr operationContext,
      int flags,
      IntPtr error,
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      IntPtr operationHandle,
      IntPtr data)
    {
      BaseClientTransportManager.tracer.WriteLine("OnCreateCmdCompleted callback received", new object[0]);
      long cmdTMId = 0;
      WSManClientCommandTransportManager cmdTransportManager = (WSManClientCommandTransportManager) null;
      if (!WSManClientCommandTransportManager.TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
      {
        BaseClientTransportManager.tracer.WriteLine("OnCreateCmdCompleted: Unable to find a transport manager for the command context {0}.", (object) cmdTMId);
      }
      else
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCreateCommandCallbackReceived, PSOpcode.Connect, PSTask.None, (object) cmdTransportManager.RunspacePoolInstanceId, (object) cmdTransportManager.powershellInstanceId);
        if (cmdTransportManager.createCmdCompleted != null)
        {
          cmdTransportManager.createCmdCompletedGCHandle.Free();
          cmdTransportManager.createCmdCompleted.Dispose();
          cmdTransportManager.createCmdCompleted = (WSManNativeApi.WSManShellAsync) null;
        }
        cmdTransportManager.wsManCmdOperationHandle = commandOperationHandle;
        if (IntPtr.Zero != error)
        {
          WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
          if (errorStruct.errorCode != 0)
          {
            BaseClientTransportManager.tracer.WriteLine("OnCreateCmdCompleted callback: WSMan reported an error: {0}", (object) errorStruct.errorDetail);
            TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, IntPtr.Zero, errorStruct, TransportMethodEnum.RunShellCommandEx, PSRemotingErrorId.RunShellCommandExCallBackError, (object) WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail));
            cmdTransportManager.RaiseErrorHandler(eventArgs);
            return;
          }
        }
        lock (cmdTransportManager.syncObject)
        {
          cmdTransportManager.isCreateCallbackReceived = true;
          if (cmdTransportManager.isClosed)
          {
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
          }
          else
          {
            if (cmdTransportManager.serializedPipeline.Length == 0L)
              cmdTransportManager.shouldStartReceivingData = true;
            cmdTransportManager.SendOneItem();
            if (!cmdTransportManager.isStopSignalPending)
              return;
            cmdTransportManager.SendStopSignal();
          }
        }
      }
    }

    private static void OnCloseCmdCompleted(
      IntPtr operationContext,
      int flags,
      IntPtr error,
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      IntPtr operationHandle,
      IntPtr data)
    {
      BaseClientTransportManager.tracer.WriteLine("OnCloseCmdCompleted callback received for operation context {0}", (object) commandOperationHandle);
      long cmdTMId = 0;
      WSManClientCommandTransportManager cmdTransportManager = (WSManClientCommandTransportManager) null;
      if (!WSManClientCommandTransportManager.TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
      {
        BaseClientTransportManager.tracer.WriteLine("OnCloseCmdCompleted: Unable to find a transport manager for the command context {0}.", (object) cmdTMId);
      }
      else
      {
        BaseClientTransportManager.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Close completed callback received for command: {0}", (object) cmdTransportManager.cmdContextId), new object[0]);
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCloseCommandCallbackReceived, PSOpcode.Disconnect, PSTask.None, (object) cmdTransportManager.RunspacePoolInstanceId, (object) cmdTransportManager.powershellInstanceId);
        cmdTransportManager.RaiseCloseCompleted();
      }
    }

    private static void OnRemoteCmdSendCompleted(
      IntPtr operationContext,
      int flags,
      IntPtr error,
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      IntPtr operationHandle,
      IntPtr data)
    {
      BaseClientTransportManager.tracer.WriteLine("SendComplete callback received", new object[0]);
      long cmdTMId = 0;
      WSManClientCommandTransportManager cmdTransportManager = (WSManClientCommandTransportManager) null;
      if (!WSManClientCommandTransportManager.TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
      {
        BaseClientTransportManager.tracer.WriteLine("Unable to find a transport manager for the command context {0}.", (object) cmdTMId);
      }
      else
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSendShellInputExCallbackReceived, PSOpcode.Connect, PSTask.None, (object) cmdTransportManager.RunspacePoolInstanceId, (object) cmdTransportManager.powershellInstanceId);
        if (!shellOperationHandle.Equals((object) cmdTransportManager.wsManShellOperationHandle) || !commandOperationHandle.Equals((object) cmdTransportManager.wsManCmdOperationHandle))
        {
          BaseClientTransportManager.tracer.WriteLine("SendShellInputEx callback: ShellOperationHandles are not the same as the Send is initiated with", new object[0]);
          TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.CommandSendExFailed, new object[0]), TransportMethodEnum.CommandInputEx);
          cmdTransportManager.RaiseErrorHandler(eventArgs);
        }
        else
        {
          cmdTransportManager.ClearReceiveOrSendResources(flags, true);
          if (cmdTransportManager.isClosed)
          {
            BaseClientTransportManager.tracer.WriteLine("Client Command TM: Transport manager is closed. So returning", new object[0]);
          }
          else
          {
            if (IntPtr.Zero != error)
            {
              WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
              if (errorStruct.errorCode != 0 && errorStruct.errorCode != 995)
              {
                BaseClientTransportManager.tracer.WriteLine("CmdSend callback: WSMan reported an error: {0}", (object) errorStruct.errorDetail);
                TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, IntPtr.Zero, errorStruct, TransportMethodEnum.CommandInputEx, PSRemotingErrorId.CommandSendExCallBackError, (object) WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail));
                cmdTransportManager.RaiseErrorHandler(eventArgs);
                return;
              }
            }
            cmdTransportManager.SendOneItem();
          }
        }
      }
    }

    private static void OnRemoteCmdDataReceived(
      IntPtr operationContext,
      int flags,
      IntPtr error,
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      IntPtr operationHandle,
      IntPtr data)
    {
      BaseClientTransportManager.tracer.WriteLine("Remote Command DataReceived callback.", new object[0]);
      long cmdTMId = 0;
      WSManClientCommandTransportManager cmdTransportManager = (WSManClientCommandTransportManager) null;
      if (!WSManClientCommandTransportManager.TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
        BaseClientTransportManager.tracer.WriteLine("Unable to find a transport manager for the given command context {0}.", (object) cmdTMId);
      else if (!shellOperationHandle.Equals((object) cmdTransportManager.wsManShellOperationHandle) || !commandOperationHandle.Equals((object) cmdTransportManager.wsManCmdOperationHandle))
      {
        BaseClientTransportManager.tracer.WriteLine("CmdReceive callback: ShellOperationHandles are not the same as the Receive is initiated with", new object[0]);
        TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.CommandReceiveExFailed, new object[0]), TransportMethodEnum.ReceiveCommandOutputEx);
        cmdTransportManager.RaiseErrorHandler(eventArgs);
      }
      else
      {
        cmdTransportManager.ClearReceiveOrSendResources(flags, false);
        if (cmdTransportManager.isClosed)
        {
          BaseClientTransportManager.tracer.WriteLine("Client Command TM: Transport manager is closed. So returning", new object[0]);
        }
        else
        {
          if (IntPtr.Zero != error)
          {
            WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
            if (errorStruct.errorCode != 0)
            {
              BaseClientTransportManager.tracer.WriteLine("CmdReceive callback: WSMan reported an error: {0}", (object) errorStruct.errorDetail);
              TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, IntPtr.Zero, errorStruct, TransportMethodEnum.ReceiveCommandOutputEx, PSRemotingErrorId.CommandReceiveExCallBackError, (object) errorStruct.errorDetail);
              cmdTransportManager.RaiseErrorHandler(eventArgs);
              return;
            }
          }
          WSManNativeApi.WSManReceiveDataResult receiveDataResult = WSManNativeApi.WSManReceiveDataResult.UnMarshal(data);
          if (receiveDataResult.data == null)
            return;
          BaseClientTransportManager.tracer.WriteLine("Cmd Received Data : {0}", (object) receiveDataResult.data.Length);
          BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManReceiveShellOutputExCallbackReceived, PSOpcode.Receive, PSTask.None, (object) cmdTransportManager.RunspacePoolInstanceId, (object) cmdTransportManager.powershellInstanceId, (object) receiveDataResult.data.Length.ToString((IFormatProvider) CultureInfo.InvariantCulture));
          cmdTransportManager.ProcessRawData(receiveDataResult.data, receiveDataResult.stream);
        }
      }
    }

    private static void OnRemoteCmdSignalCompleted(
      IntPtr operationContext,
      int flags,
      IntPtr error,
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      IntPtr operationHandle,
      IntPtr data)
    {
      BaseClientTransportManager.tracer.WriteLine("Signal Completed callback received.", new object[0]);
      long cmdTMId = 0;
      WSManClientCommandTransportManager cmdTransportManager = (WSManClientCommandTransportManager) null;
      if (!WSManClientCommandTransportManager.TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
      {
        BaseClientTransportManager.tracer.WriteLine("Unable to find a transport manager for the given command context {0}.", (object) cmdTMId);
      }
      else
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSignalCallbackReceived, PSOpcode.Disconnect, PSTask.None, (object) cmdTransportManager.RunspacePoolInstanceId, (object) cmdTransportManager.powershellInstanceId);
        if (!shellOperationHandle.Equals((object) cmdTransportManager.wsManShellOperationHandle) || !commandOperationHandle.Equals((object) cmdTransportManager.wsManCmdOperationHandle))
        {
          BaseClientTransportManager.tracer.WriteLine("Cmd Signal callback: ShellOperationHandles are not the same as the signal is initiated with", new object[0]);
          TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.CommandSendExFailed, new object[0]), TransportMethodEnum.CommandInputEx);
          cmdTransportManager.RaiseErrorHandler(eventArgs);
        }
        else
        {
          if (IntPtr.Zero != cmdTransportManager.cmdSignalOperationHandle)
          {
            WSManNativeApi.WSManCloseOperation(cmdTransportManager.cmdSignalOperationHandle, 0);
            cmdTransportManager.cmdSignalOperationHandle = IntPtr.Zero;
          }
          if (cmdTransportManager.signalCmdCompleted != null)
          {
            cmdTransportManager.signalCmdCompleted.Dispose();
            cmdTransportManager.signalCmdCompleted = (WSManNativeApi.WSManShellAsync) null;
          }
          if (cmdTransportManager.isClosed)
          {
            BaseClientTransportManager.tracer.WriteLine("Client Command TM: Transport manager is closed. So returning", new object[0]);
          }
          else
          {
            if (IntPtr.Zero != error)
            {
              WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
              if (errorStruct.errorCode != 0)
              {
                BaseClientTransportManager.tracer.WriteLine("Cmd Signal callback: WSMan reported an error: {0}", (object) errorStruct.errorDetail);
                TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, IntPtr.Zero, errorStruct, TransportMethodEnum.CommandInputEx, PSRemotingErrorId.CommandSendExCallBackError, (object) WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail));
                cmdTransportManager.RaiseErrorHandler(eventArgs);
                return;
              }
            }
            cmdTransportManager.EnqueueAndStartProcessingThread((RemoteDataObject<PSObject>) null, (PSRemotingTransportException) null, (object) true);
          }
        }
      }
    }

    private void SendOneItem()
    {
      DataPriorityType priorityType = DataPriorityType.Default;
      byte[] data;
      if (this.serializedPipeline.Length > 0L)
      {
        data = this.serializedPipeline.ReadOrRegisterCallback((SerializedDataStream.OnDataAvailableCallback) null);
        if (this.serializedPipeline.Length == 0L)
          this.shouldStartReceivingData = true;
      }
      else
        data = this.dataToBeSent.ReadOrRegisterCallback(this.onDataAvailableToSendCallback, out priorityType);
      if (data != null)
        this.SendData(data, priorityType);
      if (!this.shouldStartReceivingData)
        return;
      this.StartReceivingData();
    }

    private void OnDataAvailableCallback(byte[] data, DataPriorityType priorityType)
    {
      BaseClientTransportManager.tracer.WriteLine("Received data from dataToBeSent store.", new object[0]);
      this.SendData(data, priorityType);
    }

    private void SendData(byte[] data, DataPriorityType priorityType)
    {
      BaseClientTransportManager.tracer.WriteLine("Command sending data of size : {0}", (object) data.Length);
      byte[] data1 = data;
      bool flag = true;
      if ((object) WSManClientCommandTransportManager.commandSendRedirect != null)
      {
        object[] objArray = new object[2]
        {
          null,
          (object) data1
        };
        flag = (bool) WSManClientCommandTransportManager.commandSendRedirect.DynamicInvoke(objArray);
        data1 = (byte[]) objArray[0];
      }
      if (!flag)
        return;
      using (WSManNativeApi.WSManData streamData = new WSManNativeApi.WSManData(data1))
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSendShellInputEx, PSOpcode.Send, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId, (object) streamData.BufferLength.ToString((IFormatProvider) CultureInfo.InvariantCulture));
        lock (this.syncObject)
        {
          if (this.isClosed)
          {
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
          }
          else
          {
            this.sendToRemoteCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), WSManClientCommandTransportManager.cmdSendCallback);
            WSManNativeApi.WSManSendShellInputEx(this.wsManShellOperationHandle, this.wsManCmdOperationHandle, 0, priorityType == DataPriorityType.Default ? "stdin" : "pr", streamData, (IntPtr) this.sendToRemoteCompleted, ref this.wsManSendOperationHandle);
          }
        }
      }
    }

    private void StartReceivingData()
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManReceiveShellOutputEx, PSOpcode.Receive, PSTask.None, (object) this.RunspacePoolInstanceId, (object) this.powershellInstanceId);
      this.shouldStartReceivingData = false;
      lock (this.syncObject)
      {
        if (this.isClosed)
        {
          BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
        }
        else
        {
          this.receivedFromRemote = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), WSManClientCommandTransportManager.cmdReceiveCallback);
          WSManNativeApi.WSManReceiveShellOutputEx(this.wsManShellOperationHandle, this.wsManCmdOperationHandle, 0, (IntPtr) WSManClientSessionTransportManager.wsManApiStaticData.OutputStreamSet, (IntPtr) this.receivedFromRemote, ref this.wsManRecieveOperationHandle);
        }
      }
    }

    internal override void Dispose(bool isDisposing)
    {
      BaseClientTransportManager.tracer.WriteLine("Disposing command with command context: {0} Operation Context: {1}", (object) this.cmdContextId, (object) this.wsManCmdOperationHandle);
      base.Dispose(isDisposing);
      WSManClientCommandTransportManager.RemoveCmdTransportManager(this.cmdContextId);
      if (this.closeCmdCompleted != null)
      {
        this.closeCmdCompleted.Dispose();
        this.closeCmdCompleted = (WSManNativeApi.WSManShellAsync) null;
      }
      this.wsManCmdOperationHandle = IntPtr.Zero;
    }

    private static long GetNextCmdTMHandleId() => Interlocked.Increment(ref WSManClientCommandTransportManager.CmdTMSeed);

    private static void AddCmdTransportManager(
      long cmdTMId,
      WSManClientCommandTransportManager cmdTransportManager)
    {
      lock (WSManClientCommandTransportManager.CmdTMHandles)
        WSManClientCommandTransportManager.CmdTMHandles.Add(cmdTMId, cmdTransportManager);
    }

    private static void RemoveCmdTransportManager(long cmdTMId)
    {
      lock (WSManClientCommandTransportManager.CmdTMHandles)
      {
        if (!WSManClientCommandTransportManager.CmdTMHandles.ContainsKey(cmdTMId))
          return;
        WSManClientCommandTransportManager.CmdTMHandles[cmdTMId] = (WSManClientCommandTransportManager) null;
        WSManClientCommandTransportManager.CmdTMHandles.Remove(cmdTMId);
      }
    }

    private static bool TryGetCmdTransportManager(
      IntPtr operationContext,
      out WSManClientCommandTransportManager cmdTransportManager,
      out long cmdTMId)
    {
      cmdTMId = operationContext.ToInt64();
      cmdTransportManager = (WSManClientCommandTransportManager) null;
      lock (WSManClientCommandTransportManager.CmdTMHandles)
        return WSManClientCommandTransportManager.CmdTMHandles.TryGetValue(cmdTMId, out cmdTransportManager);
    }
  }
}
