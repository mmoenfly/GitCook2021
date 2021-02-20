// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ServerRunspacePoolDriver
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Remoting;
using System.Management.Automation.Remoting.Server;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace System.Management.Automation
{
  internal class ServerRunspacePoolDriver
  {
    [TraceSource("SRPD", "ServerRunspacePoolDriver")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("SRPD", nameof (ServerRunspacePoolDriver));
    private RunspacePool localRunspacePool;
    private ConfigurationDataFromXML configData;
    private PSPrimitiveDictionary applicationPrivateData;
    private Guid clientRunspacePoolId;
    private ServerRunspacePoolDataStructureHandler dsHandler;
    private Dictionary<Guid, ServerPowerShellDriver> associatedShells = new Dictionary<Guid, ServerPowerShellDriver>();
    private ServerRemoteHost remoteHost;
    private bool isClosed;
    private RemoteSessionCapability serverCapability;
    internal EventHandler<EventArgs> Closed;

    internal ServerRunspacePoolDriver(
      Guid clientRunspacePoolId,
      int minRunspaces,
      int maxRunspaces,
      PSThreadOptions threadOptions,
      ApartmentState apartmentState,
      HostInfo hostInfo,
      InitialSessionState initialSessionState,
      PSPrimitiveDictionary applicationPrivateData,
      ConfigurationDataFromXML configData,
      AbstractServerSessionTransportManager transportManager,
      bool isAdministrator,
      RemoteSessionCapability serverCapability)
    {
      using (ServerRunspacePoolDriver.tracer.TraceConstructor((object) this))
      {
        this.serverCapability = serverCapability;
        ServerRemoteHost serverRemoteHost = new ServerRemoteHost(clientRunspacePoolId, Guid.Empty, hostInfo, (AbstractServerTransportManager) transportManager);
        this.remoteHost = serverRemoteHost;
        this.configData = configData;
        this.applicationPrivateData = applicationPrivateData;
        this.localRunspacePool = RunspaceFactory.CreateRunspacePool(minRunspaces, maxRunspaces, initialSessionState, (PSHost) serverRemoteHost);
        PSThreadOptions psThreadOptions = configData.ShellThreadOptions.HasValue ? configData.ShellThreadOptions.Value : PSThreadOptions.UseCurrentThread;
        if (threadOptions == PSThreadOptions.Default || threadOptions == psThreadOptions)
        {
          this.localRunspacePool.ThreadOptions = psThreadOptions;
        }
        else
        {
          if (!isAdministrator)
            throw new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.MustBeAdminToOverrideThreadOptions));
          this.localRunspacePool.ThreadOptions = threadOptions;
        }
        ApartmentState apartmentState1 = configData.ShellThreadApartmentState.HasValue ? configData.ShellThreadApartmentState.Value : ApartmentState.Unknown;
        this.localRunspacePool.ApartmentState = apartmentState == ApartmentState.Unknown || apartmentState == apartmentState1 ? apartmentState1 : apartmentState;
        this.clientRunspacePoolId = clientRunspacePoolId;
        this.dsHandler = new ServerRunspacePoolDataStructureHandler(this, transportManager);
        this.localRunspacePool.StateChanged += new EventHandler<RunspacePoolStateChangedEventArgs>(this.HandleRunspacePoolStateChanged);
        this.localRunspacePool.ForwardEvent += new EventHandler<PSEventArgs>(this.HandleRunspacePoolForwardEvent);
        this.localRunspacePool.RunspaceCreated += new EventHandler<RunspaceCreatedEventArgs>(this.HandleRunspaceCreated);
        this.localRunspacePool.RunspaceCreated += new EventHandler<RunspaceCreatedEventArgs>(this.HandleRunspaceCreatedForTypeTable);
        this.dsHandler.CreateAndInvokePowerShell += new EventHandler<RemoteDataEventArgs<RemoteDataObject<PSObject>>>(this.HandleCreateAndInvokePowerShell);
        this.dsHandler.GetCommandMetadata += new EventHandler<RemoteDataEventArgs<RemoteDataObject<PSObject>>>(this.HandleGetCommandMetadata);
        this.dsHandler.HostResponseReceived += new EventHandler<RemoteDataEventArgs<RemoteHostResponse>>(this.HandleHostResponseReceived);
        this.dsHandler.SetMaxRunspacesReceived += new EventHandler<RemoteDataEventArgs<PSObject>>(this.HandleSetMaxRunspacesReceived);
        this.dsHandler.SetMinRunspacesReceived += new EventHandler<RemoteDataEventArgs<PSObject>>(this.HandleSetMinRunspacesReceived);
        this.dsHandler.GetAvailableRunspacesReceived += new EventHandler<RemoteDataEventArgs<PSObject>>(this.HandleGetAvailalbeRunspacesReceived);
      }
    }

    internal ServerRunspacePoolDataStructureHandler DataStructureHandler
    {
      get
      {
        using (ServerRunspacePoolDriver.tracer.TraceProperty())
          return this.dsHandler;
      }
    }

    internal ServerRemoteHost ServerRemoteHost => this.remoteHost;

    internal Guid InstanceId
    {
      get
      {
        using (ServerRunspacePoolDriver.tracer.TraceProperty())
          return this.clientRunspacePoolId;
      }
    }

    internal RunspacePool RunspacePool
    {
      get
      {
        using (ServerRunspacePoolDriver.tracer.TraceProperty())
          return this.localRunspacePool;
      }
    }

    internal void Start() => this.localRunspacePool.Open();

    internal void Close()
    {
      if (this.isClosed)
        return;
      this.isClosed = true;
      this.localRunspacePool.Close();
      this.localRunspacePool.Dispose();
      if (this.Closed == null)
        return;
      this.Closed((object) this, EventArgs.Empty);
    }

    private void HandleRunspaceCreatedForTypeTable(object sender, RunspaceCreatedEventArgs args)
    {
      this.dsHandler.TypeTable = args.Runspace.ExecutionContext.TypeTable;
      this.localRunspacePool.RunspaceCreated -= new EventHandler<RunspaceCreatedEventArgs>(this.HandleRunspaceCreatedForTypeTable);
    }

    private void HandleRunspaceCreated(object sender, RunspaceCreatedEventArgs args)
    {
      try
      {
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        args.Runspace.ExecutionContext.EngineSessionState.SetLocation(folderPath);
      }
      catch (ArgumentException ex)
      {
      }
      catch (ProviderNotFoundException ex)
      {
      }
      catch (DriveNotFoundException ex)
      {
      }
      catch (ProviderInvocationException ex)
      {
      }
      Command command = (Command) null;
      if (!string.IsNullOrEmpty(this.configData.StartupScript))
        command = new Command(this.configData.StartupScript, false, false);
      else if (!string.IsNullOrEmpty(this.configData.InitializationScriptForOutOfProcessRunspace))
        command = new Command(this.configData.InitializationScriptForOutOfProcessRunspace, true, false);
      if (command == null)
        return;
      HostInfo hostInfo = this.remoteHost.HostInfo;
      command.MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
      PowerShell powershell = PowerShell.Create();
      powershell.AddCommand(command).AddCommand("out-default");
      IAsyncResult asyncResult = new ServerPowerShellDriver(powershell, (PowerShell) null, true, Guid.Empty, this.InstanceId, this, args.Runspace.ApartmentState, hostInfo, RemoteStreamOptions.AddInvocationInfo, false, args.Runspace).Start();
      powershell.EndInvoke(asyncResult);
      ArrayList dollarErrorVariable = (ArrayList) powershell.Runspace.GetExecutionContext.DollarErrorVariable;
      if (dollarErrorVariable.Count > 0)
      {
        string str = (dollarErrorVariable[0] as ErrorRecord).ToString();
        throw ServerRunspacePoolDriver.tracer.NewInvalidOperationException("RemotingErrorIdStrings", PSRemotingErrorId.StartupScriptThrewTerminatingError.ToString(), (object) str);
      }
      if (this.localRunspacePool.RunspacePoolStateInfo.State != RunspacePoolState.Opening)
        return;
      object valueToConvert = args.Runspace.SessionStateProxy.PSVariable.GetValue("global:PSApplicationPrivateData");
      if (valueToConvert == null)
        return;
      this.applicationPrivateData = (PSPrimitiveDictionary) LanguagePrimitives.ConvertTo(valueToConvert, typeof (PSPrimitiveDictionary), true, (IFormatProvider) CultureInfo.InvariantCulture, (TypeTable) null);
    }

    private void HandleRunspacePoolStateChanged(
      object sender,
      RunspacePoolStateChangedEventArgs eventArgs)
    {
      using (ServerRunspacePoolDriver.tracer.TraceMethod())
      {
        RunspacePoolState state = eventArgs.RunspacePoolStateInfo.State;
        Exception reason = eventArgs.RunspacePoolStateInfo.Reason;
        switch (state)
        {
          case RunspacePoolState.Opened:
            this.dsHandler.SendApplicationPrivateDataToClient(this.applicationPrivateData, this.serverCapability);
            this.dsHandler.SendStateInfoToClient(new RunspacePoolStateInfo(state, reason));
            break;
          case RunspacePoolState.Closed:
          case RunspacePoolState.Closing:
          case RunspacePoolState.Broken:
            this.dsHandler.SendStateInfoToClient(new RunspacePoolStateInfo(state, reason));
            break;
        }
      }
    }

    private void HandleRunspacePoolForwardEvent(object sender, PSEventArgs e)
    {
      using (ServerRunspacePoolDriver.tracer.TraceMethod())
      {
        if (!e.ForwardEvent)
          return;
        this.dsHandler.SendPSEventArgsToClient(e);
      }
    }

    private void HandleCreateAndInvokePowerShell(
      object sender,
      RemoteDataEventArgs<RemoteDataObject<PSObject>> eventArgs)
    {
      using (ServerRunspacePoolDriver.tracer.TraceEventHandlers())
      {
        RemoteDataObject<PSObject> data = eventArgs.Data;
        HostInfo hostInfo = RemotingDecoder.GetHostInfo(data.Data);
        ApartmentState apartmentState = RemotingDecoder.GetApartmentState((object) data.Data);
        RemoteStreamOptions remoteStreamOptions = RemotingDecoder.GetRemoteStreamOptions((object) data.Data);
        PowerShell powerShell = RemotingDecoder.GetPowerShell((object) data.Data);
        bool noInput = RemotingDecoder.GetNoInput((object) data.Data);
        bool addToHistory = RemotingDecoder.GetAddToHistory((object) data.Data);
        new ServerPowerShellDriver(powerShell, (PowerShell) null, noInput, data.PowerShellId, data.RunspacePoolId, this, apartmentState, hostInfo, remoteStreamOptions, addToHistory, (Runspace) null).Start();
      }
    }

    private void HandleGetCommandMetadata(
      object sender,
      RemoteDataEventArgs<RemoteDataObject<PSObject>> eventArgs)
    {
      using (ServerRunspacePoolDriver.tracer.TraceEventHandlers())
      {
        RemoteDataObject<PSObject> data = eventArgs.Data;
        PowerShell discoveryPipeline1 = RemotingDecoder.GetCommandDiscoveryPipeline((object) data.Data);
        discoveryPipeline1.AddParameter("ErrorAction", (object) "SilentlyContinue").AddCommand("Measure-Object").AddCommand("Select-Object").AddParameter("Property", (object) "Count");
        PowerShell discoveryPipeline2 = RemotingDecoder.GetCommandDiscoveryPipeline((object) data.Data);
        discoveryPipeline2.AddCommand("Select-Object").AddParameter("Property", (object) new string[7]
        {
          "Name",
          "Namespace",
          "HelpUri",
          "CommandType",
          "ResolvedCommandName",
          "OutputType",
          "Parameters"
        });
        new ServerPowerShellDriver(discoveryPipeline1, discoveryPipeline2, true, data.PowerShellId, data.RunspacePoolId, this, ApartmentState.Unknown, new HostInfo((PSHost) null)
        {
          UseRunspaceHost = true
        }, (RemoteStreamOptions) 0, false, (Runspace) null).Start();
      }
    }

    private void HandleHostResponseReceived(
      object sender,
      RemoteDataEventArgs<RemoteHostResponse> eventArgs)
    {
      this.remoteHost.ServerMethodExecutor.HandleRemoteHostResponseFromClient(eventArgs.Data);
    }

    private void HandleSetMaxRunspacesReceived(
      object sender,
      RemoteDataEventArgs<PSObject> eventArgs)
    {
      PSObject data = eventArgs.Data;
      int maxRunspaces = (int) data.Properties["MaxRunspaces"].Value;
      this.dsHandler.SendResponseToClient((long) data.Properties["ci"].Value, (object) this.localRunspacePool.SetMaxRunspaces(maxRunspaces));
    }

    private void HandleSetMinRunspacesReceived(
      object sender,
      RemoteDataEventArgs<PSObject> eventArgs)
    {
      PSObject data = eventArgs.Data;
      int minRunspaces = (int) data.Properties["MinRunspaces"].Value;
      this.dsHandler.SendResponseToClient((long) data.Properties["ci"].Value, (object) this.localRunspacePool.SetMinRunspaces(minRunspaces));
    }

    private void HandleGetAvailalbeRunspacesReceived(
      object sender,
      RemoteDataEventArgs<PSObject> eventArgs)
    {
      this.dsHandler.SendResponseToClient((long) eventArgs.Data.Properties["ci"].Value, (object) this.localRunspacePool.GetAvailableRunspaces());
    }
  }
}
