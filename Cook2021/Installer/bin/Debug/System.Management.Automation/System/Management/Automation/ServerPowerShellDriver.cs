// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ServerPowerShellDriver
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Security.Principal;
using System.Threading;

namespace System.Management.Automation
{
  internal class ServerPowerShellDriver
  {
    [TraceSource("SPD", "ServerPowerShellDriver")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("SPD", nameof (ServerPowerShellDriver));
    private bool extraPowerShellAlreadyScheduled;
    private PowerShell extraPowerShell;
    private PowerShell localPowerShell;
    private PSDataCollection<PSObject> localPowerShellOutput;
    private Guid clientPowerShellId;
    private Guid clientRunspacePoolId;
    private ServerPowerShellDataStructureHandler dsHandler;
    private PSDataCollection<object> input;
    private bool datasent;
    private object syncObject = new object();
    private bool noInput;
    private bool addToHistory;
    private ServerRemoteHost remoteHost;
    private ApartmentState apartmentState;
    private RemoteStreamOptions remoteStreamOptions;

    internal ServerPowerShellDriver(
      PowerShell powershell,
      PowerShell extraPowerShell,
      bool noInput,
      Guid clientPowerShellId,
      Guid clientRunspacePoolId,
      ServerRunspacePoolDriver runspacePoolDriver,
      ApartmentState apartmentState,
      HostInfo hostInfo,
      RemoteStreamOptions streamOptions,
      bool addToHistory,
      Runspace rsToUse)
    {
      this.clientPowerShellId = clientPowerShellId;
      this.clientRunspacePoolId = clientRunspacePoolId;
      this.remoteStreamOptions = streamOptions;
      this.apartmentState = apartmentState;
      this.localPowerShell = powershell;
      this.extraPowerShell = extraPowerShell;
      this.localPowerShellOutput = new PSDataCollection<PSObject>();
      this.noInput = noInput;
      this.addToHistory = addToHistory;
      if (!noInput)
      {
        this.input = new PSDataCollection<object>();
        this.input.ReleaseOnEnumeration = true;
      }
      this.dsHandler = runspacePoolDriver.DataStructureHandler.CreatePowerShellDataStructureHandler(this);
      this.remoteHost = this.dsHandler.GetHostAssociatedWithPowerShell(hostInfo, runspacePoolDriver.ServerRemoteHost);
      this.localPowerShellOutput.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleOutputDataAdded);
      if (this.localPowerShell != null)
      {
        this.localPowerShell.InvocationStateChanged += new EventHandler<PSInvocationStateChangedEventArgs>(this.HandlePowerShellInvocationStateChanged);
        this.localPowerShell.Streams.Error.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleErrorDataAdded);
        this.localPowerShell.Streams.Debug.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleDebugAdded);
        this.localPowerShell.Streams.Verbose.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleVerboseAdded);
        this.localPowerShell.Streams.Warning.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleWarningAdded);
        this.localPowerShell.Streams.Progress.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleProgressAdded);
      }
      if (extraPowerShell != null)
      {
        extraPowerShell.InvocationStateChanged += new EventHandler<PSInvocationStateChangedEventArgs>(this.HandlePowerShellInvocationStateChanged);
        extraPowerShell.Streams.Error.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleErrorDataAdded);
        extraPowerShell.Streams.Debug.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleDebugAdded);
        extraPowerShell.Streams.Verbose.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleVerboseAdded);
        extraPowerShell.Streams.Warning.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleWarningAdded);
        extraPowerShell.Streams.Progress.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleProgressAdded);
      }
      this.dsHandler.InputEndReceived += new EventHandler(this.HandleInputEndReceived);
      this.dsHandler.InputReceived += new EventHandler<RemoteDataEventArgs<object>>(this.HandleInputReceived);
      this.dsHandler.StopPowerShellReceived += new EventHandler(this.HandleStopReceived);
      this.dsHandler.HostResponseReceived += new EventHandler<RemoteDataEventArgs<RemoteHostResponse>>(this.HandleHostResponseReceived);
      if (rsToUse != null)
      {
        this.localPowerShell.Runspace = rsToUse;
        if (extraPowerShell == null)
          return;
        extraPowerShell.Runspace = rsToUse;
      }
      else
      {
        this.localPowerShell.RunspacePool = runspacePoolDriver.RunspacePool;
        if (extraPowerShell == null)
          return;
        extraPowerShell.RunspacePool = runspacePoolDriver.RunspacePool;
      }
    }

    internal Guid InstanceId
    {
      get
      {
        using (ServerPowerShellDriver.tracer.TraceProperty())
          return this.clientPowerShellId;
      }
    }

    internal RemoteStreamOptions RemoteStreamOptions => this.remoteStreamOptions;

    internal Guid RunspacePoolId
    {
      get
      {
        using (ServerPowerShellDriver.tracer.TraceProperty())
          return this.clientRunspacePoolId;
      }
    }

    internal ServerPowerShellDataStructureHandler DataStructureHandler
    {
      get
      {
        using (ServerPowerShellDriver.tracer.TraceProperty())
          return this.dsHandler;
      }
    }

    private IAsyncResult Start(bool startMainPowerShell)
    {
      using (ServerPowerShellDriver.tracer.TraceMethod())
      {
        if (startMainPowerShell)
          this.dsHandler.Prepare();
        PSInvocationSettings settings = new PSInvocationSettings();
        settings.ApartmentState = this.apartmentState;
        settings.Host = (PSHost) this.remoteHost;
        switch (WindowsIdentity.GetCurrent().ImpersonationLevel)
        {
          case TokenImpersonationLevel.Impersonation:
          case TokenImpersonationLevel.Delegation:
            settings.FlowImpersonationPolicy = true;
            break;
          default:
            settings.FlowImpersonationPolicy = false;
            break;
        }
        settings.AddToHistory = this.addToHistory;
        return startMainPowerShell ? this.localPowerShell.BeginInvoke<object, PSObject>(this.input, this.localPowerShellOutput, settings, (AsyncCallback) null, (object) null) : this.extraPowerShell.BeginInvoke<object, PSObject>(this.input, this.localPowerShellOutput, settings, (AsyncCallback) null, (object) null);
      }
    }

    internal IAsyncResult Start() => this.Start(true);

    private void HandlePowerShellInvocationStateChanged(
      object sender,
      PSInvocationStateChangedEventArgs eventArgs)
    {
      using (ServerPowerShellDriver.tracer.TraceMethod())
      {
        PSInvocationState state = eventArgs.InvocationStateInfo.State;
        switch (state)
        {
          case PSInvocationState.Stopping:
            this.remoteHost.ServerMethodExecutor.AbortAllCalls();
            break;
          case PSInvocationState.Stopped:
          case PSInvocationState.Completed:
          case PSInvocationState.Failed:
            this.SendRemainingData();
            if (state == PSInvocationState.Completed && this.extraPowerShell != null && !this.extraPowerShellAlreadyScheduled)
            {
              this.extraPowerShellAlreadyScheduled = true;
              this.Start(false);
              break;
            }
            this.dsHandler.RaiseRemoveAssociationEvent();
            this.dsHandler.SendStateChangedInformationToClient(eventArgs.InvocationStateInfo);
            break;
        }
      }
    }

    private void HandleOutputDataAdded(object sender, DataAddedEventArgs e)
    {
      using (ServerPowerShellDriver.tracer.TraceMethod())
      {
        int index = e.Index;
        lock (this.syncObject)
        {
          if (this.datasent)
            return;
          PSObject data = this.localPowerShellOutput[index];
          this.localPowerShellOutput.RemoveAt(index);
          this.dsHandler.SendOutputDataToClient(data);
        }
      }
    }

    private void HandleErrorDataAdded(object sender, DataAddedEventArgs e)
    {
      using (ServerPowerShellDriver.tracer.TraceMethod())
      {
        int index = e.Index;
        lock (this.syncObject)
        {
          if (this.datasent)
            return;
          ErrorRecord errorRecord = this.localPowerShell.Streams.Error[index];
          this.localPowerShell.Streams.Error.RemoveAt(index);
          this.dsHandler.SendErrorRecordToClient(errorRecord);
        }
      }
    }

    private void HandleProgressAdded(object sender, DataAddedEventArgs eventArgs)
    {
      using (ServerPowerShellDriver.tracer.TraceEventHandlers())
      {
        int index = eventArgs.Index;
        lock (this.syncObject)
        {
          if (this.datasent)
            return;
          ProgressRecord record = this.localPowerShell.Streams.Progress[index];
          this.localPowerShell.Streams.Progress.RemoveAt(index);
          this.dsHandler.SendProgressRecordToClient(record);
        }
      }
    }

    private void HandleWarningAdded(object sender, DataAddedEventArgs eventArgs)
    {
      using (ServerPowerShellDriver.tracer.TraceEventHandlers())
      {
        int index = eventArgs.Index;
        lock (this.syncObject)
        {
          if (this.datasent)
            return;
          WarningRecord record = this.localPowerShell.Streams.Warning[index];
          this.localPowerShell.Streams.Warning.RemoveAt(index);
          this.dsHandler.SendWarningRecordToClient(record);
        }
      }
    }

    private void HandleVerboseAdded(object sender, DataAddedEventArgs eventArgs)
    {
      using (ServerPowerShellDriver.tracer.TraceEventHandlers())
      {
        int index = eventArgs.Index;
        lock (this.syncObject)
        {
          if (this.datasent)
            return;
          VerboseRecord record = this.localPowerShell.Streams.Verbose[index];
          this.localPowerShell.Streams.Verbose.RemoveAt(index);
          this.dsHandler.SendVerboseRecordToClient(record);
        }
      }
    }

    private void HandleDebugAdded(object sender, DataAddedEventArgs eventArgs)
    {
      using (ServerPowerShellDriver.tracer.TraceEventHandlers())
      {
        int index = eventArgs.Index;
        lock (this.syncObject)
        {
          if (this.datasent)
            return;
          DebugRecord record = this.localPowerShell.Streams.Debug[index];
          this.localPowerShell.Streams.Debug.RemoveAt(index);
          this.dsHandler.SendDebugRecordToClient(record);
        }
      }
    }

    private void SendRemainingData()
    {
      using (ServerPowerShellDriver.tracer.TraceMethod())
      {
        lock (this.syncObject)
          this.datasent = true;
        for (int index = 0; index < this.localPowerShellOutput.Count; ++index)
          this.dsHandler.SendOutputDataToClient(this.localPowerShellOutput[index]);
        this.localPowerShellOutput.Clear();
        for (int index = 0; index < this.localPowerShell.Streams.Error.Count; ++index)
          this.dsHandler.SendErrorRecordToClient(this.localPowerShell.Streams.Error[index]);
        this.localPowerShell.Streams.Error.Clear();
      }
    }

    private void HandleStopReceived(object sender, EventArgs eventArgs)
    {
      using (ServerPowerShellDriver.tracer.TraceEventHandlers())
      {
        if (this.localPowerShell.InvocationStateInfo.State != PSInvocationState.Stopped && this.localPowerShell.InvocationStateInfo.State != PSInvocationState.Completed && (this.localPowerShell.InvocationStateInfo.State != PSInvocationState.Failed && this.localPowerShell.InvocationStateInfo.State != PSInvocationState.Stopping))
          this.localPowerShell.Stop();
        if (this.extraPowerShell == null || this.extraPowerShell.InvocationStateInfo.State == PSInvocationState.Stopped || (this.extraPowerShell.InvocationStateInfo.State == PSInvocationState.Completed || this.extraPowerShell.InvocationStateInfo.State == PSInvocationState.Failed) || this.extraPowerShell.InvocationStateInfo.State == PSInvocationState.Stopping)
          return;
        this.extraPowerShell.Stop();
      }
    }

    private void HandleInputReceived(object sender, RemoteDataEventArgs<object> eventArgs)
    {
      using (ServerPowerShellDriver.tracer.TraceEventHandlers())
      {
        if (this.input == null)
          return;
        this.input.Add(eventArgs.Data);
      }
    }

    private void HandleInputEndReceived(object sender, EventArgs eventArgs)
    {
      using (ServerPowerShellDriver.tracer.TraceEventHandlers())
      {
        if (this.input == null)
          return;
        this.input.Complete();
      }
    }

    private void HandleHostResponseReceived(
      object sender,
      RemoteDataEventArgs<RemoteHostResponse> eventArgs)
    {
      this.remoteHost.ServerMethodExecutor.HandleRemoteHostResponseFromClient(eventArgs.Data);
    }
  }
}
