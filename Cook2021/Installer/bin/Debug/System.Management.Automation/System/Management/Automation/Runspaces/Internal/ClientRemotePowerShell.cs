// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.Internal.ClientRemotePowerShell
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;

namespace System.Management.Automation.Runspaces.Internal
{
  internal class ClientRemotePowerShell : IDisposable
  {
    protected const string WRITE_DEBUG_LINE = "WriteDebugLine";
    protected const string WRITE_VERBOSE_LINE = "WriteVerboseLine";
    protected const string WRITE_WARNING_LINE = "WriteWarningLine";
    protected const string WRITE_PROGRESS = "WriteProgress";
    [TraceSource("CRPS", "ClientRemotePowerShell")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CRPS", "ClientRemotePowerShellBase");
    protected ObjectStreamBase inputstream;
    protected ObjectStreamBase errorstream;
    protected PSInformationalBuffers informationalBuffers;
    protected PowerShell shell;
    protected Guid clientRunspacePoolId;
    protected bool noInput;
    protected PSInvocationSettings settings;
    protected ObjectStreamBase outputstream;
    protected string computerName;
    protected ClientPowerShellDataStructureHandler dataStructureHandler;
    protected bool stopCalled;
    protected PSHost hostToUse;

    internal ClientRemotePowerShell(
      PowerShell shell,
      ObjectStreamBase inputstream,
      ObjectStreamBase outputstream,
      ObjectStreamBase errorstream,
      PSInformationalBuffers informationalBuffers,
      PSInvocationSettings settings,
      RemoteRunspacePoolInternal runspacePool)
    {
      using (ClientRemotePowerShell.tracer.TraceConstructor((object) this))
      {
        this.shell = shell;
        this.informationalBuffers = informationalBuffers;
        this.InputStream = inputstream;
        this.errorstream = errorstream;
        this.outputstream = outputstream;
        this.settings = settings;
        this.clientRunspacePoolId = runspacePool.InstanceId;
        this.hostToUse = settings == null || settings.Host == null ? runspacePool.Host : settings.Host;
        this.computerName = runspacePool.ConnectionInfo.ComputerName;
        this.dataStructureHandler = runspacePool.DataStructureHandler.CreatePowerShellDataStructureHandler(this);
        this.dataStructureHandler.InvocationStateInfoReceived += new EventHandler<RemoteDataEventArgs<PSInvocationStateInfo>>(this.HandleInvocationStateInfoReceived);
        this.dataStructureHandler.OutputReceived += new EventHandler<RemoteDataEventArgs<object>>(this.HandleOutputReceived);
        this.dataStructureHandler.ErrorReceived += new EventHandler<RemoteDataEventArgs<ErrorRecord>>(this.HandleErrorReceived);
        this.dataStructureHandler.InformationalMessageReceived += new EventHandler<RemoteDataEventArgs<InformationalMessage>>(this.HandleInformationalMessageReceived);
        this.dataStructureHandler.HostCallReceived += new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleHostCallReceived);
        this.dataStructureHandler.ClosedNotificationFromRunspacePool += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleCloseNotificationFromRunspacePool);
        this.dataStructureHandler.BrokenNotificationFromRunspacePool += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleBrokenNotificationFromRunspacePool);
      }
    }

    internal Guid InstanceId
    {
      get
      {
        using (ClientRemotePowerShell.tracer.TraceProperty())
          return this.PowerShell.InstanceId;
      }
    }

    internal PowerShell PowerShell
    {
      get
      {
        using (ClientRemotePowerShell.tracer.TraceProperty())
          return this.shell;
      }
    }

    internal void SetStateInfo(PSInvocationStateInfo stateInfo)
    {
      using (ClientRemotePowerShell.tracer.TraceMethod())
        this.shell.SetStateChanged(stateInfo);
    }

    internal bool NoInput
    {
      get
      {
        using (ClientRemotePowerShell.tracer.TraceProperty())
          return this.noInput;
      }
    }

    internal ObjectStreamBase InputStream
    {
      get
      {
        using (ClientRemotePowerShell.tracer.TraceProperty())
          return this.inputstream;
      }
      set
      {
        using (ClientRemotePowerShell.tracer.TraceProperty((object) value))
        {
          this.inputstream = value;
          if (this.inputstream != null && (this.inputstream.IsOpen || this.inputstream.Count > 0))
            this.noInput = false;
          else
            this.noInput = true;
        }
      }
    }

    internal ObjectStreamBase OutputStream
    {
      get => this.outputstream;
      set => this.outputstream = value;
    }

    internal ClientPowerShellDataStructureHandler DataStructureHandler
    {
      get
      {
        using (ClientRemotePowerShell.tracer.TraceProperty())
          return this.dataStructureHandler;
      }
    }

    internal PSInvocationSettings Settings => this.settings;

    internal void UnblockCollections()
    {
      using (ClientRemotePowerShell.tracer.TraceMethod())
      {
        this.shell.ClearRemotePowerShell();
        this.outputstream.Close();
        this.errorstream.Close();
        if (this.inputstream == null)
          return;
        this.inputstream.Close();
      }
    }

    internal void StopAsync()
    {
      using (ClientRemotePowerShell.tracer.TraceMethod())
      {
        this.stopCalled = true;
        this.dataStructureHandler.SendStopPowerShellMessage();
      }
    }

    internal void SendInput() => this.dataStructureHandler.SendInput(this.inputstream);

    internal event EventHandler<RemoteDataEventArgs<RemoteHostCall>> HostCallReceived;

    private void HandleErrorReceived(object sender, RemoteDataEventArgs<ErrorRecord> eventArgs)
    {
      using (ClientRemotePowerShell.tracer.TraceEventHandlers())
        this.errorstream.Write((object) eventArgs.Data);
    }

    private void HandleOutputReceived(object sender, RemoteDataEventArgs<object> eventArgs)
    {
      using (ClientRemotePowerShell.tracer.TraceEventHandlers())
      {
        object data = eventArgs.Data;
        try
        {
          this.outputstream.Write(data);
        }
        catch (PSInvalidCastException ex)
        {
          ClientRemotePowerShell.tracer.TraceException((Exception) ex);
          this.shell.SetStateChanged(new PSInvocationStateInfo(PSInvocationState.Failed, (Exception) ex));
        }
      }
    }

    private void HandleInvocationStateInfoReceived(
      object sender,
      RemoteDataEventArgs<PSInvocationStateInfo> eventArgs)
    {
      using (ClientRemotePowerShell.tracer.TraceEventHandlers())
      {
        PSInvocationStateInfo data = eventArgs.Data;
        if (data.State != PSInvocationState.Stopped && data.State != PSInvocationState.Failed && data.State != PSInvocationState.Completed)
          return;
        this.UnblockCollections();
        this.dataStructureHandler.RaiseRemoveAssociationEvent();
        this.dataStructureHandler.CloseConnection();
        if (this.stopCalled)
          this.SetStateInfo(new PSInvocationStateInfo(PSInvocationState.Stopped, data.Reason));
        else
          this.SetStateInfo(data);
      }
    }

    private void HandleInformationalMessageReceived(
      object sender,
      RemoteDataEventArgs<InformationalMessage> eventArgs)
    {
      using (ClientRemotePowerShell.tracer.TraceEventHandlers())
      {
        InformationalMessage data = eventArgs.Data;
        switch (data.DataType)
        {
          case RemotingDataType.PowerShellDebug:
            this.informationalBuffers.AddDebug((DebugRecord) data.Message);
            break;
          case RemotingDataType.PowerShellVerbose:
            this.informationalBuffers.AddVerbose((VerboseRecord) data.Message);
            break;
          case RemotingDataType.PowerShellWarning:
            this.informationalBuffers.AddWarning((WarningRecord) data.Message);
            break;
          case RemotingDataType.PowerShellProgress:
            this.informationalBuffers.AddProgress((ProgressRecord) LanguagePrimitives.ConvertTo(data.Message, typeof (ProgressRecord), (IFormatProvider) CultureInfo.InvariantCulture));
            break;
        }
      }
    }

    private void HandleHostCallReceived(
      object sender,
      RemoteDataEventArgs<RemoteHostCall> eventArgs)
    {
      using (ClientRemotePowerShell.tracer.TraceEventHandlers())
      {
        Collection<RemoteHostCall> collection = eventArgs.Data.PerformSecurityChecksOnHostMessage(this.computerName);
        if (this.HostCallReceived != null)
        {
          if (collection.Count > 0)
          {
            foreach (object data in collection)
            {
              RemoteDataEventArgs<RemoteHostCall> e = new RemoteDataEventArgs<RemoteHostCall>(data);
              this.HostCallReceived(sender, e);
            }
          }
          this.HostCallReceived(sender, eventArgs);
        }
        else
        {
          if (collection.Count > 0)
          {
            foreach (RemoteHostCall hostcall in collection)
              this.ExecuteHostCall(hostcall);
          }
          this.ExecuteHostCall(eventArgs.Data);
        }
      }
    }

    private void ExecuteHostCall(RemoteHostCall hostcall)
    {
      if (hostcall.IsVoidMethod)
      {
        if (hostcall.IsSetShouldExitOrPopRunspace)
          this.shell.ClearRemotePowerShell();
        hostcall.ExecuteVoidMethod(this.hostToUse);
      }
      else
        this.dataStructureHandler.SendHostResponseToServer(hostcall.ExecuteNonVoidMethod(this.hostToUse));
    }

    private void HandleCloseNotificationFromRunspacePool(
      object sender,
      RemoteDataEventArgs<Exception> eventArgs)
    {
      PSInvocationStateInfo invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Stopped, eventArgs.Data);
      this.HandleInvocationStateInfoReceived(sender, new RemoteDataEventArgs<PSInvocationStateInfo>((object) invocationStateInfo));
    }

    private void HandleBrokenNotificationFromRunspacePool(
      object sender,
      RemoteDataEventArgs<Exception> eventArgs)
    {
      PSInvocationStateInfo invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Failed, eventArgs.Data);
      this.HandleInvocationStateInfoReceived(sender, new RemoteDataEventArgs<PSInvocationStateInfo>((object) invocationStateInfo));
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected void Dispose(bool disposing)
    {
      int num = disposing ? 1 : 0;
    }
  }
}
