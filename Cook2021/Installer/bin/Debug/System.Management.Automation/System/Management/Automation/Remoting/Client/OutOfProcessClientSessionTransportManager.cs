// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Client.OutOfProcessClientSessionTransportManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Runspaces.Internal;
using System.Net;
using System.Text;
using System.Timers;

namespace System.Management.Automation.Remoting.Client
{
  internal class OutOfProcessClientSessionTransportManager : BaseClientSessionTransportManager
  {
    private Process serverProcess;
    private NewProcessConnectionInfo connectionInfo;
    private PrioritySendDataCollection.OnDataAvailableCallback onDataAvailableToSendCallback;
    private OutOfProcessTextWriter stdInWriter;
    private OutOfProcessUtils.DataProcessingDelegates dataProcessingCallbacks;
    private Dictionary<Guid, OutOfProcessClientCommandTransportManager> cmdTransportManagers;
    private System.Timers.Timer closeTimeOutTimer;

    internal OutOfProcessClientSessionTransportManager(
      Guid runspaceId,
      NewProcessConnectionInfo connectionInfo,
      PSRemotingCryptoHelper cryptoHelper)
      : base(runspaceId, cryptoHelper)
    {
      this.onDataAvailableToSendCallback = new PrioritySendDataCollection.OnDataAvailableCallback(this.OnDataAvailableCallback);
      this.cmdTransportManagers = new Dictionary<Guid, OutOfProcessClientCommandTransportManager>();
      this.connectionInfo = connectionInfo;
      this.dataProcessingCallbacks = new OutOfProcessUtils.DataProcessingDelegates();
      this.dataProcessingCallbacks.DataPacketReceived += new OutOfProcessUtils.DataPacketReceived(this.OnDataPacketReceived);
      this.dataProcessingCallbacks.DataAckPacketReceived += new OutOfProcessUtils.DataAckPacketReceived(this.OnDataAckPacketReceived);
      this.dataProcessingCallbacks.CommandCreationPacketReceived += new OutOfProcessUtils.CommandCreationPacketReceived(this.OnCommandCreationPacketReceived);
      this.dataProcessingCallbacks.CommandCreationAckReceived += new OutOfProcessUtils.CommandCreationAckReceived(this.OnCommandCreationAckReceived);
      this.dataProcessingCallbacks.SignalPacketReceived += new OutOfProcessUtils.SignalPacketReceived(this.OnSignalPacketReceived);
      this.dataProcessingCallbacks.SignalAckPacketReceived += new OutOfProcessUtils.SignalAckPacketReceived(this.OnSiganlAckPacketReceived);
      this.dataProcessingCallbacks.ClosePacketReceived += new OutOfProcessUtils.ClosePacketReceived(this.OnClosePacketReceived);
      this.dataProcessingCallbacks.CloseAckPacketReceived += new OutOfProcessUtils.CloseAckPacketReceived(this.OnCloseAckReceived);
      this.dataToBeSent.Fragmentor = this.Fragmentor;
      this.ReceivedDataCollection.MaximumReceivedDataSize = new int?();
      this.ReceivedDataCollection.MaximumReceivedObjectSize = new int?(10485760);
      this.closeTimeOutTimer = new System.Timers.Timer(60000.0);
      this.closeTimeOutTimer.Elapsed += new ElapsedEventHandler(this.OnCloseTimeOutTimerElapsed);
    }

    internal override void ConnectAsync()
    {
      string path = Path.Combine(Utils.GetApplicationBase(Utils.DefaultPowerShellShellID), "powershell.exe");
      if (this.connectionInfo != null && this.connectionInfo.RunAs32)
      {
        string environmentVariable = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
        if (environmentVariable.Equals("amd64", StringComparison.OrdinalIgnoreCase) || environmentVariable.Equals("ia64", StringComparison.OrdinalIgnoreCase))
        {
          path = path.ToLowerInvariant().Replace("\\system32\\", "\\syswow64\\");
          if (!System.IO.File.Exists(path))
            throw new PSInvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.IPCWowComponentNotPresent, (object) path));
        }
      }
      string str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "-s -NoLogo -NoProfile");
      if (this.connectionInfo.InitializationScript != null && !string.IsNullOrEmpty(this.connectionInfo.InitializationScript.ToString()))
      {
        string base64String = Convert.ToBase64String(Encoding.Unicode.GetBytes(this.connectionInfo.InitializationScript.ToString()));
        str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0} -EncodedCommand {1}", (object) str, (object) base64String);
      }
      ProcessStartInfo processStartInfo = new ProcessStartInfo();
      processStartInfo.FileName = path;
      processStartInfo.Arguments = str;
      processStartInfo.UseShellExecute = false;
      processStartInfo.RedirectStandardInput = true;
      processStartInfo.RedirectStandardOutput = true;
      processStartInfo.RedirectStandardError = true;
      processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      processStartInfo.CreateNoWindow = true;
      processStartInfo.LoadUserProfile = true;
      if (this.connectionInfo.Credential != null)
      {
        NetworkCredential networkCredential = this.connectionInfo.Credential.GetNetworkCredential();
        processStartInfo.UserName = networkCredential.UserName;
        processStartInfo.Domain = !string.IsNullOrEmpty(networkCredential.Domain) ? networkCredential.Domain : ".";
        processStartInfo.Password = this.connectionInfo.Credential.Password;
      }
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCreateShell, PSOpcode.Connect, PSTask.CreateRunspace, (object) this.RunspacePoolInstanceId);
      try
      {
        lock (this.syncObject)
        {
          if (this.isClosed)
            return;
          this.serverProcess = new Process();
          this.serverProcess.StartInfo = processStartInfo;
          this.serverProcess.EnableRaisingEvents = true;
          this.serverProcess.OutputDataReceived += new DataReceivedEventHandler(this.OnOutputDataReceived);
          this.serverProcess.ErrorDataReceived += new DataReceivedEventHandler(this.OnErrorDataReceived);
          this.serverProcess.Exited += new EventHandler(this.OnExited);
          this.serverProcess.Start();
          this.serverProcess.BeginOutputReadLine();
          this.serverProcess.BeginErrorReadLine();
          this.stdInWriter = new OutOfProcessTextWriter((TextWriter) this.serverProcess.StandardInput);
        }
      }
      catch (Win32Exception ex)
      {
        this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.IPCExceptionLaunchingProcess, new object[1]
        {
          (object) ex.Message
        })
        {
          ErrorCode = ex.ErrorCode
        }, TransportMethodEnum.CreateShellEx));
        return;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.IPCExceptionLaunchingProcess, new object[1]
        {
          (object) ex.Message
        }), TransportMethodEnum.CreateShellEx));
        return;
      }
      this.SendOneItem();
    }

    internal override void CloseAsync()
    {
      bool flag = false;
      lock (this.syncObject)
      {
        if (this.isClosed)
          return;
        this.isClosed = true;
        if (this.stdInWriter == null)
          flag = true;
      }
      base.CloseAsync();
      if (flag)
      {
        this.RaiseCloseCompleted();
      }
      else
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCloseShell, PSOpcode.Disconnect, PSTask.None, (object) this.RunspacePoolInstanceId);
        this.stdInWriter.WriteLine(OutOfProcessUtils.CreateClosePacket(Guid.Empty));
        this.closeTimeOutTimer.Start();
      }
    }

    internal override BaseClientCommandTransportManager CreateClientCommandTransportManager(
      RunspaceConnectionInfo connectionInfo,
      ClientRemotePowerShell cmd,
      bool noInput)
    {
      OutOfProcessClientCommandTransportManager cmdTM = new OutOfProcessClientCommandTransportManager(cmd, noInput, this, this.stdInWriter);
      this.AddCommandTransportManager(cmd.InstanceId, cmdTM);
      return (BaseClientCommandTransportManager) cmdTM;
    }

    internal override void Dispose(bool isDisposing)
    {
      base.Dispose(isDisposing);
      this.cmdTransportManagers.Clear();
      this.closeTimeOutTimer.Dispose();
      this.KillServerProcess();
      if (this.serverProcess == null)
        return;
      this.serverProcess.Dispose();
    }

    private void KillServerProcess()
    {
      if (this.serverProcess == null)
        return;
      try
      {
        if (this.serverProcess.HasExited)
          return;
        this.serverProcess.Exited -= new EventHandler(this.OnExited);
        this.serverProcess.CancelOutputRead();
        this.serverProcess.CancelErrorRead();
        this.serverProcess.Kill();
      }
      catch (Win32Exception ex1)
      {
        try
        {
          Process.GetProcessById(this.serverProcess.Id).Kill();
        }
        catch (Exception ex2)
        {
          CommandProcessorBase.CheckForSevereException(ex2);
        }
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
    }

    private void OnCloseSessionCompleted()
    {
      this.RaiseCloseCompleted();
      this.KillServerProcess();
    }

    private void AddCommandTransportManager(
      Guid key,
      OutOfProcessClientCommandTransportManager cmdTM)
    {
      lock (this.syncObject)
        this.cmdTransportManagers.Add(key, cmdTM);
    }

    internal override void RemoveCommandTransportManager(Guid key)
    {
      lock (this.syncObject)
      {
        if (this.isClosed)
          return;
        this.cmdTransportManagers.Remove(key);
      }
    }

    private OutOfProcessClientCommandTransportManager GetCommandTransportManager(
      Guid key)
    {
      lock (this.syncObject)
      {
        OutOfProcessClientCommandTransportManager transportManager = (OutOfProcessClientCommandTransportManager) null;
        this.cmdTransportManagers.TryGetValue(key, out transportManager);
        return transportManager;
      }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      try
      {
        OutOfProcessUtils.ProcessData(e.Data, this.dataProcessingCallbacks);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.IPCErrorProcessingServerData, new object[1]
        {
          (object) ex.Message
        }), TransportMethodEnum.ReceiveShellOutputEx));
      }
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
      lock (this.syncObject)
      {
        if (this.isClosed)
          return;
      }
      this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.IPCServerProcessReportedError, new object[1]
      {
        (object) e.Data
      }), TransportMethodEnum.Unknown));
    }

    private void OnExited(object sender, EventArgs e)
    {
      lock (this.syncObject)
      {
        if (this.isClosed)
          return;
      }
      this.stdInWriter.StopWriting();
      this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.IPCServerProcessExited, new object[0]), TransportMethodEnum.Unknown));
    }

    private void SendOneItem()
    {
      DataPriorityType priorityType;
      byte[] data = this.dataToBeSent.ReadOrRegisterCallback(this.onDataAvailableToSendCallback, out priorityType);
      if (data == null)
        return;
      this.SendData(data, priorityType);
    }

    private void OnDataAvailableCallback(byte[] data, DataPriorityType priorityType)
    {
      BaseClientTransportManager.tracer.WriteLine("Received data to be sent from the callback.", new object[0]);
      this.SendData(data, priorityType);
    }

    private void SendData(byte[] data, DataPriorityType priorityType)
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSendShellInputEx, PSOpcode.Send, PSTask.None, (object) this.RunspacePoolInstanceId, (object) Guid.Empty, (object) data.Length.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      lock (this.syncObject)
      {
        if (this.isClosed)
          return;
        this.stdInWriter.WriteLine(OutOfProcessUtils.CreateDataPacket(data, priorityType, Guid.Empty));
      }
    }

    private void OnRemoteSessionSendCompleted()
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSendShellInputExCallbackReceived, PSOpcode.Connect, PSTask.None, (object) this.RunspacePoolInstanceId, (object) Guid.Empty);
      this.SendOneItem();
    }

    private void OnDataPacketReceived(byte[] rawData, string stream, Guid psGuid)
    {
      string stream1 = "stdout";
      if (stream.Equals(DataPriorityType.PromptResponse.ToString(), StringComparison.OrdinalIgnoreCase))
        stream1 = "pr";
      if (psGuid == Guid.Empty)
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManReceiveShellOutputExCallbackReceived, PSOpcode.Receive, PSTask.None, (object) this.RunspacePoolInstanceId, (object) Guid.Empty, (object) rawData.Length.ToString((IFormatProvider) CultureInfo.InvariantCulture));
        this.ProcessRawData(rawData, stream1);
      }
      else
        this.GetCommandTransportManager(psGuid)?.OnRemoteCmdDataReceived(rawData, stream1);
    }

    private void OnDataAckPacketReceived(Guid psGuid)
    {
      if (psGuid == Guid.Empty)
        this.OnRemoteSessionSendCompleted();
      else
        this.GetCommandTransportManager(psGuid)?.OnRemoteCmdSendCompleted();
    }

    private void OnCommandCreationPacketReceived(Guid psGuid) => throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, new object[1]
    {
      (object) "Command"
    });

    private void OnCommandCreationAckReceived(Guid psGuid) => (this.GetCommandTransportManager(psGuid) ?? throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownCommandGuid, new object[2]
    {
      (object) psGuid.ToString(),
      (object) "CommandAck"
    })).OnCreateCmdCompleted();

    private void OnSignalPacketReceived(Guid psGuid) => throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, new object[1]
    {
      (object) "Signal"
    });

    private void OnSiganlAckPacketReceived(Guid psGuid)
    {
      if (psGuid == Guid.Empty)
        throw new PSRemotingTransportException(PSRemotingErrorId.IPCNoSignalForSession, new object[1]
        {
          (object) "SignalAck"
        });
      this.GetCommandTransportManager(psGuid)?.OnRemoteCmdSignalCompleted();
    }

    private void OnClosePacketReceived(Guid psGuid) => throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, new object[1]
    {
      (object) "Close"
    });

    private void OnCloseAckReceived(Guid psGuid)
    {
      if (psGuid == Guid.Empty)
        this.OnCloseSessionCompleted();
      else
        this.GetCommandTransportManager(psGuid)?.OnCloseCmdCompleted();
    }

    internal void OnCloseTimeOutTimerElapsed(object source, ElapsedEventArgs e)
    {
      this.closeTimeOutTimer.Stop();
      this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.IPCCloseTimedOut, new object[0]), TransportMethodEnum.CloseShellOperationEx));
    }
  }
}
