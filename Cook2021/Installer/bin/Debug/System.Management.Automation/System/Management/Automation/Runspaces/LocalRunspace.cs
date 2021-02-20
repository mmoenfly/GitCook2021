// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.LocalRunspace
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;
using System.Collections.Generic;
using System.Management.Automation.Host;
using System.Management.Automation.Remoting;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
  internal sealed class LocalRunspace : RunspaceBase
  {
    private const string resTableName = "RunspaceInit";
    private PSPrimitiveDictionary applicationPrivateData;
    private PSThreadOptions createThreadOptions;
    private JobRepository _jobRepository;
    private RunspaceRepository _runspaceRepository;
    private PipelineThread pipelineThread;
    private bool _disposed;
    private CommandFactory _commandFactory;
    private AutomationEngine _engine;
    private History _history;
    private ManualResetEvent remoteRunspaceCloseCompleted = new ManualResetEvent(false);
    [TraceSource("LocalRunspace", "LocalRunspace")]
    internal static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (LocalRunspace), nameof (LocalRunspace));
    [TraceSource("RunspaceInit", "Initialization code for Runspace")]
    private static PSTraceSource runspaceInitTracer = PSTraceSource.GetTracer("RunspaceInit", "Initialization code for Runspace", false);

    internal LocalRunspace(PSHost host, RunspaceConfiguration runspaceConfig)
      : base(host, runspaceConfig)
    {
    }

    internal LocalRunspace(
      PSHost host,
      InitialSessionState initialSessionState,
      bool suppressClone)
      : base(host, initialSessionState, suppressClone)
    {
    }

    internal LocalRunspace(PSHost host, InitialSessionState initialSessionState)
      : base(host, initialSessionState)
    {
    }

    public override PSPrimitiveDictionary GetApplicationPrivateData()
    {
      if (this.applicationPrivateData == null)
      {
        lock (this.SyncRoot)
        {
          if (this.applicationPrivateData == null)
            this.applicationPrivateData = new PSPrimitiveDictionary();
        }
      }
      return this.applicationPrivateData;
    }

    internal override void SetApplicationPrivateData(PSPrimitiveDictionary applicationPrivateData) => this.applicationPrivateData = applicationPrivateData;

    public override PSEventManager Events
    {
      get
      {
        System.Management.Automation.ExecutionContext executionContext = this.GetExecutionContext;
        return executionContext == null ? (PSEventManager) null : (PSEventManager) executionContext.Events;
      }
    }

    public override PSThreadOptions ThreadOptions
    {
      get => this.createThreadOptions;
      set
      {
        lock (this.SyncRoot)
        {
          if (value == this.createThreadOptions)
            return;
          if (this.RunspaceStateInfo.State != RunspaceState.BeforeOpen && (this.ApartmentState != ApartmentState.MTA && this.ApartmentState != ApartmentState.Unknown || value != PSThreadOptions.ReuseThread))
            throw new InvalidOperationException(ResourceManagerCache.FormatResourceString("Runspace", "InvalidThreadOptionsChange"));
          this.createThreadOptions = value;
        }
      }
    }

    protected override Pipeline CoreCreatePipeline(
      string command,
      bool addToHistory,
      bool isNested)
    {
      if (this._disposed)
        throw LocalRunspace._trace.NewObjectDisposedException("runspace");
      return (Pipeline) new LocalPipeline(this, command, addToHistory, isNested);
    }

    internal override System.Management.Automation.ExecutionContext GetExecutionContext => this._engine == null ? (System.Management.Automation.ExecutionContext) null : this._engine.Context;

    internal override bool InNestedPrompt
    {
      get
      {
        System.Management.Automation.ExecutionContext executionContext = this.GetExecutionContext;
        return executionContext != null && executionContext.InternalHost.HostInNestedPrompt();
      }
    }

    internal CommandFactory CommandFactory => this._commandFactory;

    internal History History => this._history;

    internal JobRepository JobRepository => this._jobRepository;

    internal RunspaceRepository RunspaceRepository => this._runspaceRepository;

    protected override void OpenHelper(bool syncCall)
    {
      if (syncCall)
        this.DoOpenHelper();
      else
        new Thread(new ThreadStart(this.OpenThreadProc)).Start();
    }

    private void OpenThreadProc()
    {
      try
      {
        this.DoOpenHelper();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        LocalRunspace._trace.TraceException(ex);
      }
    }

    private void DoOpenHelper()
    {
      if (this._disposed)
        throw LocalRunspace._trace.NewObjectDisposedException("runspace");
      bool flag = false;
      LocalRunspace.runspaceInitTracer.WriteLine("begin open runspace", new object[0]);
      try
      {
        this._engine = this.InitialSessionState == null ? new AutomationEngine(this.Host, this.RunspaceConfiguration, (InitialSessionState) null) : new AutomationEngine(this.Host, (RunspaceConfiguration) null, this.InitialSessionState);
        this._engine.Context.CurrentRunspace = (Runspace) this;
        MshLog.LogEngineLifecycleEvent(this._engine.Context, EngineState.Available);
        flag = true;
        this._commandFactory = new CommandFactory(this._engine.Context);
        this._history = new History(this._engine.Context);
        this._jobRepository = new JobRepository();
        this._runspaceRepository = new RunspaceRepository();
        LocalRunspace.runspaceInitTracer.WriteLine("initializing built-in aliases and variable information", new object[0]);
        this.InitializeDefaults();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        LocalRunspace.runspaceInitTracer.WriteLine("Runspace open failed", new object[0]);
        LocalRunspace.runspaceInitTracer.TraceException(ex);
        LocalRunspace._trace.TraceException(ex);
        this.LogEngineHealthEvent(ex);
        if (flag)
          MshLog.LogEngineLifecycleEvent(this._engine.Context, EngineState.Stopped);
        this.SetRunspaceState(RunspaceState.Broken, ex);
        this.RaiseRunspaceStateEvents();
        throw;
      }
      this.SetRunspaceState(RunspaceState.Opened);
      this.RaiseRunspaceStateEvents();
      LocalRunspace.runspaceInitTracer.WriteLine("runspace opened successfully", new object[0]);
      if (this.InitialSessionState == null)
        return;
      foreach (string key in this.InitialSessionState.ModulesToImport.Keys)
      {
        System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create();
        if (!this.InitialSessionState.ThrowOnRunspaceOpenError)
        {
          Command command = new Command("Import-Module");
          command.Parameters.Add("Name", (object) key);
          command.MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
          powerShell.AddCommand(command);
          powerShell.AddCommand("Out-Default");
        }
        else
          powerShell.AddCommand("Import-Module").AddParameter("Name", (object) key);
        powerShell.Runspace = (Runspace) this;
        powerShell.Invoke();
        if (this.InitialSessionState.ThrowOnRunspaceOpenError && powerShell.Streams.Error.Count > 0)
        {
          Exception exception = powerShell.Streams.Error[0].Exception;
          LocalRunspace.runspaceInitTracer.WriteLine("Runspace open failed while loading module '{0}': First error {1}", (object) key, (object) exception);
          RunspaceOpenModuleLoadException moduleLoadException = new RunspaceOpenModuleLoadException(key, powerShell.Streams.Error);
          LocalRunspace._trace.TraceException((Exception) moduleLoadException);
          this.LogEngineHealthEvent((Exception) moduleLoadException);
          if (flag)
            MshLog.LogEngineLifecycleEvent(this._engine.Context, EngineState.Stopped);
          this.SetRunspaceState(RunspaceState.Broken, (Exception) moduleLoadException);
          this.RaiseRunspaceStateEvents();
          throw moduleLoadException;
        }
      }
    }

    internal void LogEngineHealthEvent(Exception exception) => this.LogEngineHealthEvent(exception, Severity.Error, 103, (Dictionary<string, string>) null);

    internal void LogEngineHealthEvent(
      Exception exception,
      Severity severity,
      int id,
      Dictionary<string, string> additionalInfo)
    {
      MshLog.LogEngineHealthEvent(new LogContext()
      {
        EngineVersion = this.Version.ToString(),
        HostId = this.Host.InstanceId.ToString(),
        HostName = this.Host.Name,
        HostVersion = this.Host.Version.ToString(),
        RunspaceId = this.InstanceId.ToString(),
        Severity = severity.ToString(),
        ShellId = this.RunspaceConfiguration != null ? this.RunspaceConfiguration.ShellId : Utils.DefaultPowerShellShellID
      }, id, exception, additionalInfo);
    }

    internal PipelineThread GetPipelineThread()
    {
      if (this.pipelineThread == null)
        this.pipelineThread = new PipelineThread(this.ApartmentState);
      return this.pipelineThread;
    }

    protected override void CloseHelper(bool syncCall)
    {
      if (syncCall)
        this.DoCloseHelper();
      else
        new Thread(new ThreadStart(this.CloseThreadProc)).Start();
    }

    private void CloseThreadProc()
    {
      try
      {
        this.DoCloseHelper();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        LocalRunspace._trace.TraceException(ex);
      }
    }

    private void DoCloseHelper()
    {
      this.StopPipelines();
      this.CloseAllRemoteRunspaces();
      this._engine.Context.RunspaceClosingNotification();
      MshLog.LogEngineLifecycleEvent(this._engine.Context, EngineState.Stopped);
      this._engine = (AutomationEngine) null;
      this._commandFactory = (CommandFactory) null;
      this.SetRunspaceState(RunspaceState.Closed);
      this.RaiseRunspaceStateEvents();
    }

    private void CloseAllRemoteRunspaces()
    {
      ThrottleManager throttleManager = new ThrottleManager();
      throttleManager.ThrottleComplete += new EventHandler<EventArgs>(this.HandleThrottleComplete);
      foreach (PSSession runspace in this.RunspaceRepository.Runspaces)
      {
        IThrottleOperation operation = (IThrottleOperation) new CloseRunspaceOperationHelper((RemoteRunspace) runspace.Runspace);
        throttleManager.AddOperation(operation);
      }
      throttleManager.EndSubmitOperations();
      this.remoteRunspaceCloseCompleted.WaitOne();
    }

    private void HandleThrottleComplete(object sender, EventArgs e) => this.remoteRunspaceCloseCompleted.Set();

    protected override void DoSetVariable(string name, object value)
    {
      if (this._disposed)
        throw LocalRunspace._trace.NewObjectDisposedException("runspace");
      this._engine.Context.SetVariable(name, value);
    }

    protected override object DoGetVariable(string name)
    {
      if (this._disposed)
        throw LocalRunspace._trace.NewObjectDisposedException("runspace");
      return this._engine.Context.GetVariable(name);
    }

    protected override List<string> DoApplications
    {
      get
      {
        if (this._disposed)
          throw LocalRunspace._trace.NewObjectDisposedException("runspace");
        return this._engine.Context.EngineSessionState.Applications;
      }
    }

    protected override List<string> DoScripts
    {
      get
      {
        if (this._disposed)
          throw LocalRunspace._trace.NewObjectDisposedException("runspace");
        return this._engine.Context.EngineSessionState.Scripts;
      }
    }

    protected override DriveManagementIntrinsics DoDrive
    {
      get
      {
        if (this._disposed)
          throw LocalRunspace._trace.NewObjectDisposedException("runspace");
        return this._engine.Context.SessionState.Drive;
      }
    }

    protected override PSLanguageMode DoLanguageMode
    {
      get
      {
        if (this._disposed)
          throw LocalRunspace._trace.NewObjectDisposedException("runspace");
        return this._engine.Context.SessionState.LanguageMode;
      }
      set
      {
        if (this._disposed)
          throw LocalRunspace._trace.NewObjectDisposedException("runspace");
        this._engine.Context.SessionState.LanguageMode = value;
      }
    }

    protected override PSModuleInfo DoModule
    {
      get
      {
        if (this._disposed)
          throw LocalRunspace._trace.NewObjectDisposedException("runspace");
        return this._engine.Context.EngineSessionState.Module;
      }
    }

    protected override PathIntrinsics DoPath
    {
      get
      {
        if (this._disposed)
          throw LocalRunspace._trace.NewObjectDisposedException("runspace");
        return this._engine.Context.SessionState.Path;
      }
    }

    protected override CmdletProviderManagementIntrinsics DoProvider
    {
      get
      {
        if (this._disposed)
          throw LocalRunspace._trace.NewObjectDisposedException("runspace");
        return this._engine.Context.SessionState.Provider;
      }
    }

    protected override PSVariableIntrinsics DoPSVariable
    {
      get
      {
        if (this._disposed)
          throw LocalRunspace._trace.NewObjectDisposedException("runspace");
        return this._engine.Context.SessionState.PSVariable;
      }
    }

    protected override CommandInvocationIntrinsics DoInvokeCommand
    {
      get
      {
        if (this._disposed)
          throw LocalRunspace._trace.NewObjectDisposedException("runspace");
        return this._engine.Context.EngineIntrinsics.InvokeCommand;
      }
    }

    protected override ProviderIntrinsics DoInvokeProvider
    {
      get
      {
        if (this._disposed)
          throw LocalRunspace._trace.NewObjectDisposedException("runspace");
        return this._engine.Context.EngineIntrinsics.InvokeProvider;
      }
    }

    protected override void Dispose(bool disposing)
    {
      try
      {
        if (this._disposed)
          return;
        lock (this.SyncRoot)
        {
          if (this._disposed)
            return;
          this._disposed = true;
        }
        if (!disposing)
          return;
        this.Close();
        this.remoteRunspaceCloseCompleted.Close();
      }
      finally
      {
        base.Dispose(disposing);
      }
    }

    public override void Close()
    {
      if (this.Events != null)
        this.Events.GenerateEvent("PowerShell.Exiting", (object) null, new object[0], (PSObject) null, true);
      base.Close();
      if (this.pipelineThread == null)
        return;
      this.pipelineThread.Close();
    }

    internal AutomationEngine Engine => this._engine;

    private void InitializeDefaults()
    {
      SessionStateInternal engineSessionState = this._engine.Context.EngineSessionState;
      engineSessionState.InitializeFixedVariables();
      if (this.RunspaceConfiguration == null)
        return;
      bool addSetStrictMode = true;
      foreach (RunspaceConfigurationEntry cmdlet in (IEnumerable<CmdletConfigurationEntry>) this.RunspaceConfiguration.Cmdlets)
      {
        if (cmdlet.Name.Equals("Set-StrictMode", StringComparison.OrdinalIgnoreCase))
        {
          addSetStrictMode = false;
          break;
        }
      }
      engineSessionState.AddBuiltInEntries(addSetStrictMode);
    }
  }
}
