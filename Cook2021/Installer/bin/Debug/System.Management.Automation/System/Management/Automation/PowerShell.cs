// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PowerShell
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Runspaces.Internal;
using System.Security.Principal;
using System.Threading;

namespace System.Management.Automation
{
  public sealed class PowerShell : IDisposable
  {
    [TraceSource("PowerShellHosting", "Powershell hosting interfaces")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PowerShellHosting", "Powershell hosting interfaces");
    private bool isGetCommandMetadataSpecialPipeline;
    private PSCommand psCommand;
    private PowerShell.Worker worker;
    private PSInvocationStateInfo invocationStateInfo;
    private PowerShellAsyncResult invokeAsyncResult;
    private PowerShellAsyncResult stopAsyncResult;
    private bool isNested;
    private object rsConnection;
    private PSDataCollection<PSObject> outputBuffer;
    private PSDataCollection<ErrorRecord> errorBuffer;
    private PSInformationalBuffers informationalBuffers;
    private PSDataStreams dataStreams;
    private bool isDisposed;
    private Guid instanceId;
    private object syncObject = new object();
    private static string resBaseName = "PowerShellStrings";
    private ClientRemotePowerShell remotePowerShell;
    private string historyString;
    private bool redirectShellErrorOutputPipe = true;
    private Runspace runspace;
    private bool runspaceOwner;
    private RunspacePool runspacePool;

    private PowerShell(PSCommand command, object rsConnection)
    {
      using (PowerShell.tracer.TraceConstructor((object) this))
      {
        this.psCommand = command;
        this.psCommand.Owner = this;
        this.rsConnection = rsConnection is RemoteRunspace remoteRunspace ? (object) remoteRunspace.RunspacePool : rsConnection;
        this.instanceId = Guid.NewGuid();
        this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.NotStarted, (Exception) null);
        this.outputBuffer = (PSDataCollection<PSObject>) null;
        this.errorBuffer = new PSDataCollection<ErrorRecord>();
        this.informationalBuffers = new PSInformationalBuffers(this.instanceId);
        this.dataStreams = new PSDataStreams(this);
      }
    }

    internal PowerShell(
      bool isScript,
      ObjectStreamBase inputstream,
      ObjectStreamBase outputstream,
      ObjectStreamBase errorstream,
      RunspacePool runspacePool)
    {
      using (PowerShell.tracer.TraceConstructor((object) this))
      {
        this.rsConnection = (object) runspacePool;
        this.instanceId = Guid.NewGuid();
        this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.NotStarted, (Exception) null);
        this.informationalBuffers = new PSInformationalBuffers(this.instanceId);
        this.dataStreams = new PSDataStreams(this);
        this.outputBuffer = ((PSDataCollectionStream<PSObject>) outputstream).ObjectStore;
        this.errorBuffer = ((PSDataCollectionStream<ErrorRecord>) errorstream).ObjectStore;
      }
    }

    internal void InitForRemotePipeline(
      CommandCollection command,
      ObjectStreamBase inputstream,
      ObjectStreamBase outputstream,
      ObjectStreamBase errorstream,
      PSInvocationSettings settings,
      bool redirectShellErrorOutputPipe)
    {
      this.psCommand = new PSCommand(command[0]);
      this.psCommand.Owner = this;
      for (int index = 1; index < command.Count; ++index)
        this.AddCommand(command[index]);
      this.redirectShellErrorOutputPipe = redirectShellErrorOutputPipe;
      this.remotePowerShell = new ClientRemotePowerShell(this, inputstream, outputstream, errorstream, this.informationalBuffers, settings, ((RunspacePool) this.rsConnection).RemoteRunspacePoolInternal);
    }

    public static PowerShell Create()
    {
      using (PowerShell.tracer.TraceMethod())
        return new PowerShell(new PSCommand(), (object) null);
    }

    public PowerShell CreateNestedPowerShell()
    {
      using (PowerShell.tracer.TraceMethod())
      {
        if (this.worker == null || this.worker.CurrentlyRunningPipeline == null)
          throw PowerShell.tracer.NewInvalidOperationException(PowerShell.resBaseName, "InvalidStateCreateNested");
        return new PowerShell(new PSCommand(), (object) this.worker.CurrentlyRunningPipeline.Runspace)
        {
          isNested = true
        };
      }
    }

    private static PowerShell Create(bool isNested, PSCommand psCommand) => new PowerShell(psCommand, (object) null)
    {
      isNested = isNested
    };

    public PowerShell AddCommand(string cmdlet)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          this.AssertChangesAreAccepted();
          this.psCommand.AddCommand(cmdlet);
          return this;
        }
      }
    }

    public PowerShell AddCommand(string cmdlet, bool useLocalScope)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          this.AssertChangesAreAccepted();
          this.psCommand.AddCommand(cmdlet, useLocalScope);
          return this;
        }
      }
    }

    public PowerShell AddScript(string script)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          this.AssertChangesAreAccepted();
          this.psCommand.AddScript(script);
          return this;
        }
      }
    }

    public PowerShell AddScript(string script, bool useLocalScope)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          this.AssertChangesAreAccepted();
          this.psCommand.AddScript(script, useLocalScope);
          return this;
        }
      }
    }

    internal PowerShell AddCommand(Command command)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          this.AssertChangesAreAccepted();
          this.psCommand.AddCommand(command);
          return this;
        }
      }
    }

    public PowerShell AddParameter(string parameterName, object value)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          if (this.psCommand.Commands.Count == 0)
            throw PowerShell.tracer.NewInvalidOperationException(PowerShell.resBaseName, "ParameterRequiresCommand");
          this.AssertChangesAreAccepted();
          this.psCommand.AddParameter(parameterName, value);
          return this;
        }
      }
    }

    public PowerShell AddParameter(string parameterName)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          if (this.psCommand.Commands.Count == 0)
            throw PowerShell.tracer.NewInvalidOperationException(PowerShell.resBaseName, "ParameterRequiresCommand");
          this.AssertChangesAreAccepted();
          this.psCommand.AddParameter(parameterName);
          return this;
        }
      }
    }

    public PowerShell AddParameters(IList parameters)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          if (parameters == null)
            throw PowerShell.tracer.NewArgumentNullException(nameof (parameters));
          if (this.psCommand.Commands.Count == 0)
            throw PowerShell.tracer.NewInvalidOperationException(PowerShell.resBaseName, "ParameterRequiresCommand");
          this.AssertChangesAreAccepted();
          foreach (object parameter in (IEnumerable) parameters)
            this.psCommand.AddParameter((string) null, parameter);
          return this;
        }
      }
    }

    public PowerShell AddParameters(IDictionary parameters)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          if (parameters == null)
            throw PowerShell.tracer.NewArgumentNullException(nameof (parameters));
          if (this.psCommand.Commands.Count == 0)
            throw PowerShell.tracer.NewInvalidOperationException(PowerShell.resBaseName, "ParameterRequiresCommand");
          this.AssertChangesAreAccepted();
          foreach (DictionaryEntry parameter in parameters)
          {
            if (!(parameter.Key is string key))
              throw PowerShell.tracer.NewArgumentException(nameof (parameters), PowerShell.resBaseName, "KeyMustBeString");
            this.psCommand.AddParameter(key, parameter.Value);
          }
          return this;
        }
      }
    }

    public PowerShell AddArgument(object value)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        lock (this.syncObject)
        {
          if (this.psCommand.Commands.Count == 0)
            throw PowerShell.tracer.NewInvalidOperationException(PowerShell.resBaseName, "ParameterRequiresCommand");
          this.AssertChangesAreAccepted();
          this.psCommand.AddArgument(value);
          return this;
        }
      }
    }

    public PSCommand Commands
    {
      get
      {
        using (PowerShell.tracer.TraceProperty())
          return this.psCommand;
      }
      set
      {
        using (PowerShell.tracer.TraceProperty())
        {
          if (value == null)
            throw PowerShell.tracer.NewArgumentNullException("Command");
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.psCommand = value.Clone();
            this.psCommand.Owner = this;
          }
        }
      }
    }

    public PSDataStreams Streams => this.dataStreams;

    internal PSDataCollection<ErrorRecord> ErrorBuffer
    {
      get
      {
        using (PowerShell.tracer.TraceProperty())
          return this.errorBuffer;
      }
      set
      {
        using (PowerShell.tracer.TraceProperty())
        {
          if (value == null)
            throw PowerShell.tracer.NewArgumentNullException("Error");
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.errorBuffer = value;
          }
        }
      }
    }

    internal PSDataCollection<ProgressRecord> ProgressBuffer
    {
      get
      {
        using (PowerShell.tracer.TraceProperty())
          return this.informationalBuffers.Progress;
      }
      set
      {
        using (PowerShell.tracer.TraceProperty())
        {
          if (value == null)
            throw PowerShell.tracer.NewArgumentNullException("Progress");
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.informationalBuffers.Progress = value;
          }
        }
      }
    }

    internal PSDataCollection<VerboseRecord> VerboseBuffer
    {
      get
      {
        using (PowerShell.tracer.TraceProperty())
          return this.informationalBuffers.Verbose;
      }
      set
      {
        using (PowerShell.tracer.TraceProperty())
        {
          if (value == null)
            throw PowerShell.tracer.NewArgumentNullException("Verbose");
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.informationalBuffers.Verbose = value;
          }
        }
      }
    }

    internal PSDataCollection<DebugRecord> DebugBuffer
    {
      get
      {
        using (PowerShell.tracer.TraceProperty())
          return this.informationalBuffers.Debug;
      }
      set
      {
        using (PowerShell.tracer.TraceProperty())
        {
          if (value == null)
            throw PowerShell.tracer.NewArgumentNullException("Debug");
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.informationalBuffers.Debug = value;
          }
        }
      }
    }

    internal PSDataCollection<WarningRecord> WarningBuffer
    {
      get
      {
        using (PowerShell.tracer.TraceProperty())
          return this.informationalBuffers.Warning;
      }
      set
      {
        using (PowerShell.tracer.TraceProperty())
        {
          if (value == null)
            throw PowerShell.tracer.NewArgumentNullException("Warning");
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            this.informationalBuffers.Warning = value;
          }
        }
      }
    }

    internal bool RedirectShellErrorOutputPipe
    {
      get => this.redirectShellErrorOutputPipe;
      set => this.redirectShellErrorOutputPipe = value;
    }

    public Guid InstanceId
    {
      get
      {
        using (PowerShell.tracer.TraceProperty())
          return this.instanceId;
      }
    }

    public PSInvocationStateInfo InvocationStateInfo
    {
      get
      {
        using (PowerShell.tracer.TraceProperty())
          return this.invocationStateInfo;
      }
    }

    public bool IsNested
    {
      get
      {
        using (PowerShell.tracer.TraceProperty())
          return this.isNested;
      }
    }

    public event EventHandler<PSInvocationStateChangedEventArgs> InvocationStateChanged;

    public Runspace Runspace
    {
      get
      {
        if (this.runspace == null && this.runspacePool == null)
        {
          lock (this.syncObject)
          {
            if (this.runspace == null)
            {
              if (this.runspacePool == null)
              {
                this.AssertChangesAreAccepted();
                this.SetRunspace(RunspaceFactory.CreateRunspace(), true);
                this.Runspace.Open();
              }
            }
          }
        }
        return this.runspace;
      }
      set
      {
        using (PowerShell.tracer.TraceProperty())
        {
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            if (this.runspace != null && this.runspaceOwner)
            {
              this.runspace.Dispose();
              this.runspace = (Runspace) null;
              this.runspaceOwner = false;
            }
            this.SetRunspace(value, false);
          }
        }
      }
    }

    private void SetRunspace(Runspace runspace, bool owner)
    {
      this.rsConnection = runspace is RemoteRunspace remoteRunspace ? (object) remoteRunspace.RunspacePool : (object) runspace;
      this.runspace = runspace;
      this.runspaceOwner = owner;
      this.runspacePool = (RunspacePool) null;
    }

    public RunspacePool RunspacePool
    {
      get => this.runspacePool;
      set
      {
        using (PowerShell.tracer.TraceProperty())
        {
          lock (this.syncObject)
          {
            this.AssertChangesAreAccepted();
            if (this.runspace != null && this.runspaceOwner)
            {
              this.runspace.Dispose();
              this.runspace = (Runspace) null;
              this.runspaceOwner = false;
            }
            this.rsConnection = (object) value;
            this.runspacePool = value;
            this.runspace = (Runspace) null;
          }
        }
      }
    }

    internal object GetRunspaceConnection() => this.rsConnection;

    public Collection<PSObject> Invoke()
    {
      using (PowerShell.tracer.TraceMethod())
        return this.Invoke((IEnumerable) null, (PSInvocationSettings) null);
    }

    public Collection<PSObject> Invoke(IEnumerable input)
    {
      using (PowerShell.tracer.TraceMethod())
        return this.Invoke(input, (PSInvocationSettings) null);
    }

    public Collection<PSObject> Invoke(
      IEnumerable input,
      PSInvocationSettings settings)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        Collection<PSObject> collection = new Collection<PSObject>();
        PSDataCollection<PSObject> output = new PSDataCollection<PSObject>((IList<PSObject>) collection);
        this.CoreInvoke<PSObject>(input, output, settings);
        return collection;
      }
    }

    public Collection<T> Invoke<T>()
    {
      using (PowerShell.tracer.TraceMethod())
      {
        Collection<T> collection = new Collection<T>();
        this.Invoke<T>((IEnumerable) null, (IList<T>) collection, (PSInvocationSettings) null);
        return collection;
      }
    }

    public Collection<T> Invoke<T>(IEnumerable input)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        Collection<T> collection = new Collection<T>();
        this.Invoke<T>(input, (IList<T>) collection, (PSInvocationSettings) null);
        return collection;
      }
    }

    public Collection<T> Invoke<T>(IEnumerable input, PSInvocationSettings settings)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        Collection<T> collection = new Collection<T>();
        this.Invoke<T>(input, (IList<T>) collection, settings);
        return collection;
      }
    }

    public void Invoke<T>(IEnumerable input, IList<T> output)
    {
      using (PowerShell.tracer.TraceMethod())
        this.Invoke<T>(input, output, (PSInvocationSettings) null);
    }

    public void Invoke<T>(IEnumerable input, IList<T> output, PSInvocationSettings settings)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        PSDataCollection<T> output1 = output != null ? new PSDataCollection<T>(output) : throw PowerShell.tracer.NewArgumentNullException(nameof (output));
        this.CoreInvoke<T>(input, output1, settings);
      }
    }

    public IAsyncResult BeginInvoke()
    {
      using (PowerShell.tracer.TraceMethod())
        return this.BeginInvoke<object>((PSDataCollection<object>) null, (PSInvocationSettings) null, (AsyncCallback) null, (object) null);
    }

    public IAsyncResult BeginInvoke<T>(PSDataCollection<T> input)
    {
      using (PowerShell.tracer.TraceMethod())
        return this.BeginInvoke<T>(input, (PSInvocationSettings) null, (AsyncCallback) null, (object) null);
    }

    public IAsyncResult BeginInvoke<T>(
      PSDataCollection<T> input,
      PSInvocationSettings settings,
      AsyncCallback callback,
      object state)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        if (this.outputBuffer != null)
          return this.CoreInvokeAsync<T, PSObject>(input, this.outputBuffer, settings, callback, state, (PSDataCollection<PSObject>) null);
        PSDataCollection<PSObject> psDataCollection = new PSDataCollection<PSObject>();
        return this.CoreInvokeAsync<T, PSObject>(input, psDataCollection, settings, callback, state, psDataCollection);
      }
    }

    public IAsyncResult BeginInvoke<TInput, TOutput>(
      PSDataCollection<TInput> input,
      PSDataCollection<TOutput> output)
    {
      using (PowerShell.tracer.TraceMethod())
        return this.BeginInvoke<TInput, TOutput>(input, output, (PSInvocationSettings) null, (AsyncCallback) null, (object) null);
    }

    public IAsyncResult BeginInvoke<TInput, TOutput>(
      PSDataCollection<TInput> input,
      PSDataCollection<TOutput> output,
      PSInvocationSettings settings,
      AsyncCallback callback,
      object state)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        if (output == null)
          throw PowerShell.tracer.NewArgumentNullException(nameof (output));
        return this.CoreInvokeAsync<TInput, TOutput>(input, output, settings, callback, state, (PSDataCollection<PSObject>) null);
      }
    }

    public PSDataCollection<PSObject> EndInvoke(IAsyncResult asyncResult)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        try
        {
          if (asyncResult == null)
            throw PowerShell.tracer.NewArgumentNullException(nameof (asyncResult));
          if (!(asyncResult is PowerShellAsyncResult shellAsyncResult) || shellAsyncResult.OwnerId != this.instanceId || !shellAsyncResult.IsAssociatedWithAsyncInvoke)
            throw PowerShell.tracer.NewArgumentException(nameof (asyncResult), PowerShell.resBaseName, "AsyncResultNotOwned", (object) "IAsyncResult", (object) "BeginInvoke");
          shellAsyncResult.EndInvoke();
          return shellAsyncResult.Output;
        }
        catch (InvalidRunspacePoolStateException ex)
        {
          if (this.runspace != null)
            throw ex.ToInvalidRunspaceStateException();
          throw;
        }
      }
    }

    public void Stop()
    {
      using (PowerShell.tracer.TraceMethod())
        this.CoreStop(true, (AsyncCallback) null, (object) null).AsyncWaitHandle.WaitOne();
    }

    public IAsyncResult BeginStop(AsyncCallback callback, object state)
    {
      using (PowerShell.tracer.TraceMethod())
        return this.CoreStop(false, callback, state);
    }

    public void EndStop(IAsyncResult asyncResult)
    {
      using (PowerShell.tracer.TraceMethod())
      {
        if (asyncResult == null)
          throw PowerShell.tracer.NewArgumentNullException(nameof (asyncResult));
        if (!(asyncResult is PowerShellAsyncResult shellAsyncResult) || shellAsyncResult.OwnerId != this.instanceId || shellAsyncResult.IsAssociatedWithAsyncInvoke)
          throw PowerShell.tracer.NewArgumentException(nameof (asyncResult), PowerShell.resBaseName, "AsyncResultNotOwned", (object) "IAsyncResult", (object) "BeginStop");
        shellAsyncResult.EndInvoke();
      }
    }

    private void PipelineStateChanged(object source, PipelineStateEventArgs stateEventArgs) => this.SetStateChanged(new PSInvocationStateInfo(stateEventArgs.PipelineStateInfo));

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    internal bool IsGetCommandMetadataSpecialPipeline
    {
      get => this.isGetCommandMetadataSpecialPipeline;
      set => this.isGetCommandMetadataSpecialPipeline = value;
    }

    private bool IsCommandRunning() => this.InvocationStateInfo.State == PSInvocationState.Running;

    private void AssertExecutionNotStarted()
    {
      this.AssertNotDisposed();
      if (this.IsCommandRunning())
        throw new InvalidOperationException(ResourceManagerCache.FormatResourceString(PowerShell.resBaseName, "ExecutionAlreadyStarted"));
    }

    internal void AssertChangesAreAccepted()
    {
      lock (this.syncObject)
      {
        this.AssertNotDisposed();
        if (this.IsCommandRunning())
          throw new InvalidPowerShellStateException(this.InvocationStateInfo.State);
      }
    }

    private void AssertNotDisposed()
    {
      if (this.isDisposed)
        throw PowerShell.tracer.NewObjectDisposedException(nameof (PowerShell));
    }

    private void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      lock (this.syncObject)
      {
        if (this.isDisposed)
          return;
      }
      if (this.invocationStateInfo.State == PSInvocationState.Running)
        this.Stop();
      lock (this.syncObject)
        this.isDisposed = true;
      if (this.outputBuffer != null)
        this.outputBuffer.Dispose();
      if (this.errorBuffer != null)
        this.errorBuffer.Dispose();
      if (this.runspaceOwner)
        this.runspace.Dispose();
      if (this.remotePowerShell != null)
        this.remotePowerShell.Dispose();
      this.invokeAsyncResult = (PowerShellAsyncResult) null;
      this.stopAsyncResult = (PowerShellAsyncResult) null;
    }

    private void InternalClearSuppressExceptions()
    {
      lock (this.syncObject)
      {
        if (this.worker == null)
          return;
        this.worker.InternalClearSuppressExceptions();
      }
    }

    private void RaiseStateChangeEvent(PSInvocationStateInfo stateInfo)
    {
      if (this.runspace is RemoteRunspace)
        this.runspace.UpdateRunspaceAvailability(this.invocationStateInfo.State, true);
      if (this.InvocationStateChanged == null)
        return;
      this.InvocationStateChanged((object) this, new PSInvocationStateChangedEventArgs(stateInfo));
    }

    internal void SetStateChanged(PSInvocationStateInfo stateInfo)
    {
      PSInvocationStateInfo invocationStateInfo = stateInfo;
      lock (this.syncObject)
      {
        switch (this.invocationStateInfo.State)
        {
          case PSInvocationState.Running:
            if (stateInfo.State == PSInvocationState.Running)
              return;
            break;
          case PSInvocationState.Stopping:
            if (stateInfo.State == PSInvocationState.Running || stateInfo.State == PSInvocationState.Stopping)
              return;
            if (stateInfo.State == PSInvocationState.Completed || stateInfo.State == PSInvocationState.Failed)
            {
              invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Stopped, stateInfo.Reason);
              break;
            }
            break;
          case PSInvocationState.Stopped:
            return;
          case PSInvocationState.Completed:
            return;
          case PSInvocationState.Failed:
            return;
        }
        this.invocationStateInfo = invocationStateInfo;
      }
      switch (this.invocationStateInfo.State)
      {
        case PSInvocationState.Running:
        case PSInvocationState.Stopping:
          this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
          break;
        case PSInvocationState.Stopped:
        case PSInvocationState.Completed:
        case PSInvocationState.Failed:
          this.InternalClearSuppressExceptions();
          bool flag = false;
          try
          {
            this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
            if (this.invokeAsyncResult != null)
              this.invokeAsyncResult.SetAsCompleted(this.invocationStateInfo.Reason);
            if (this.stopAsyncResult == null)
              break;
            this.stopAsyncResult.SetAsCompleted((Exception) null);
            break;
          }
          catch (Exception ex)
          {
            flag = true;
            throw;
          }
          finally
          {
            if (flag && this.stopAsyncResult != null)
              this.stopAsyncResult.Release();
          }
      }
    }

    internal void ClearRemotePowerShell()
    {
      lock (this.syncObject)
      {
        if (this.remotePowerShell == null)
          return;
        this.remotePowerShell.Dispose();
        this.remotePowerShell = (ClientRemotePowerShell) null;
      }
    }

    private void CoreInvoke<TOutput>(
      IEnumerable input,
      PSDataCollection<TOutput> output,
      PSInvocationSettings settings)
    {
      PSDataCollection<object> input1 = (PSDataCollection<object>) null;
      if (input != null)
      {
        input1 = new PSDataCollection<object>();
        foreach (object obj in input)
          input1.Add(obj);
        input1.Complete();
      }
      if (this.rsConnection is RunspacePool rsConnection && rsConnection.IsRemote)
      {
        this.CoreInvokeAsync<object, TOutput>(input1, output, settings, (AsyncCallback) null, (object) null, (PSDataCollection<PSObject>) null).AsyncWaitHandle.WaitOne();
        if (PSInvocationState.Failed == this.invocationStateInfo.State && this.invocationStateInfo.Reason != null)
          throw this.invocationStateInfo.Reason;
      }
      else
      {
        this.Prepare<object, TOutput>(input1, output, settings, true);
        try
        {
          if (!this.isNested)
          {
            if (rsConnection != null)
            {
              this.VerifyThreadSettings(settings, rsConnection.ApartmentState, rsConnection.ThreadOptions, false);
              this.worker.GetRunspaceAsyncResult = rsConnection.BeginGetRunspace((AsyncCallback) null, (object) null);
              this.worker.GetRunspaceAsyncResult.AsyncWaitHandle.WaitOne();
              rsToUse = rsConnection.EndGetRunspace(this.worker.GetRunspaceAsyncResult);
            }
            else if (this.rsConnection is Runspace rsToUse)
            {
              this.VerifyThreadSettings(settings, rsToUse.ApartmentState, rsToUse.ThreadOptions, false);
              if (rsToUse.RunspaceStateInfo.State != RunspaceState.Opened)
                throw new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString(PowerShell.resBaseName, "InvalidRunspaceState", (object) RunspaceState.Opened, (object) rsToUse.RunspaceStateInfo.State), rsToUse.RunspaceStateInfo.State, RunspaceState.Opened);
            }
            this.worker.CreateRunspaceIfNeededAndDoWork(rsToUse, true);
          }
          else
            this.worker.ConstructPipelineAndDoWork(this.rsConnection as Runspace, true);
        }
        catch (Exception ex)
        {
          this.SetStateChanged(new PSInvocationStateInfo(PSInvocationState.Failed, ex));
          if (ex is InvalidRunspacePoolStateException poolStateException && this.runspace != null)
            throw poolStateException.ToInvalidRunspaceStateException();
          throw;
        }
      }
    }

    private IAsyncResult CoreInvokeAsync<TInput, TOutput>(
      PSDataCollection<TInput> input,
      PSDataCollection<TOutput> output,
      PSInvocationSettings settings,
      AsyncCallback callback,
      object state,
      PSDataCollection<PSObject> asyncResultOutput)
    {
      RunspacePool rsConnection1 = this.rsConnection as RunspacePool;
      this.Prepare<TInput, TOutput>(input, output, settings, rsConnection1 == null || !rsConnection1.IsRemote);
      this.invokeAsyncResult = new PowerShellAsyncResult(this.instanceId, callback, state, asyncResultOutput, true);
      try
      {
        if (this.isNested)
          throw PowerShell.tracer.NewInvalidOperationException(PowerShell.resBaseName, "NestedPowerShellInvokeAsync");
        if (rsConnection1 != null)
        {
          this.VerifyThreadSettings(settings, rsConnection1.ApartmentState, rsConnection1.ThreadOptions, rsConnection1.IsRemote);
          rsConnection1.AssertPoolIsOpen();
          if (rsConnection1.IsRemote)
          {
            this.worker = (PowerShell.Worker) null;
            lock (this.syncObject)
            {
              this.AssertExecutionNotStarted();
              this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Running, (Exception) null);
              ObjectStreamBase inputstream = (ObjectStreamBase) null;
              if (input != null)
                inputstream = (ObjectStreamBase) new PSDataCollectionStream<TInput>(this.instanceId, input);
              if (this.remotePowerShell == null)
              {
                if (inputstream == null)
                {
                  inputstream = (ObjectStreamBase) new ObjectStream();
                  inputstream.Close();
                }
                this.remotePowerShell = new ClientRemotePowerShell(this, inputstream, (ObjectStreamBase) new PSDataCollectionStream<TOutput>(this.instanceId, output), (ObjectStreamBase) new PSDataCollectionStream<ErrorRecord>(this.instanceId, this.errorBuffer), this.informationalBuffers, settings, rsConnection1.RemoteRunspacePoolInternal);
              }
              else
              {
                if (inputstream != null)
                  this.remotePowerShell.InputStream = inputstream;
                if (output != null)
                  this.remotePowerShell.OutputStream = (ObjectStreamBase) new PSDataCollectionStream<TOutput>(this.instanceId, output);
              }
              rsConnection1.RemoteRunspacePoolInternal.CreatePowerShellOnServerAndInvoke(this.remotePowerShell);
            }
            this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
          }
          else
            this.worker.GetRunspaceAsyncResult = rsConnection1.BeginGetRunspace(new AsyncCallback(this.worker.RunspaceAvailableCallback), (object) null);
        }
        else if (this.rsConnection is LocalRunspace rsConnection2)
        {
          this.VerifyThreadSettings(settings, rsConnection2.ApartmentState, rsConnection2.ThreadOptions, false);
          if (rsConnection2.RunspaceStateInfo.State != RunspaceState.Opened)
            throw new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString(PowerShell.resBaseName, "InvalidRunspaceState", (object) RunspaceState.Opened, (object) rsConnection2.RunspaceStateInfo.State), rsConnection2.RunspaceStateInfo.State, RunspaceState.Opened);
          this.worker.CreateRunspaceIfNeededAndDoWork((Runspace) rsConnection2, false);
        }
        else
          ThreadPool.QueueUserWorkItem(new WaitCallback(this.worker.CreateRunspaceIfNeededAndDoWork), this.rsConnection);
      }
      catch (Exception ex)
      {
        this.invokeAsyncResult = (PowerShellAsyncResult) null;
        this.SetStateChanged(new PSInvocationStateInfo(PSInvocationState.Failed, ex));
        if (ex is InvalidRunspacePoolStateException poolStateException && this.runspace != null)
          throw poolStateException.ToInvalidRunspaceStateException();
        throw;
      }
      return (IAsyncResult) this.invokeAsyncResult;
    }

    private void VerifyThreadSettings(
      PSInvocationSettings settings,
      ApartmentState runspaceApartmentState,
      PSThreadOptions runspaceThreadOptions,
      bool isRemote)
    {
      ApartmentState apartmentState = settings == null || settings.ApartmentState == ApartmentState.Unknown ? runspaceApartmentState : settings.ApartmentState;
      switch (runspaceThreadOptions)
      {
        case PSThreadOptions.ReuseThread:
          if (apartmentState == runspaceApartmentState)
            break;
          throw new InvalidOperationException(ResourceManagerCache.GetResourceString(PowerShell.resBaseName, "ApartmentStateMismatch"));
        case PSThreadOptions.UseCurrentThread:
          if (isRemote || apartmentState == ApartmentState.Unknown || apartmentState == Thread.CurrentThread.GetApartmentState())
            break;
          throw new InvalidOperationException(ResourceManagerCache.GetResourceString(PowerShell.resBaseName, "ApartmentStateMismatchCurrentThread"));
      }
    }

    private void Prepare<TInput, TOutput>(
      PSDataCollection<TInput> input,
      PSDataCollection<TOutput> output,
      PSInvocationSettings settings,
      bool shouldCreateWorker)
    {
      lock (this.syncObject)
      {
        if (this.psCommand == null || this.psCommand.Commands == null || this.psCommand.Commands.Count == 0)
          throw PowerShell.tracer.NewInvalidOperationException(PowerShell.resBaseName, "NoCommandToInvoke");
        this.AssertExecutionNotStarted();
        if (shouldCreateWorker)
        {
          this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Running, (Exception) null);
          if (settings != null && settings.FlowImpersonationPolicy)
            settings.WindowsIdentityToImpersonate = WindowsIdentity.GetCurrent(false);
          ObjectStreamBase inputStream;
          if (input != null)
          {
            inputStream = (ObjectStreamBase) new PSDataCollectionStream<TInput>(this.instanceId, input);
          }
          else
          {
            inputStream = (ObjectStreamBase) new ObjectStream();
            inputStream.Close();
          }
          ObjectStreamBase outputStream = (ObjectStreamBase) new PSDataCollectionStream<TOutput>(this.instanceId, output);
          this.worker = new PowerShell.Worker(inputStream, outputStream, settings, this);
        }
      }
      if (!shouldCreateWorker)
        return;
      this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
    }

    private IAsyncResult CoreStop(
      bool isSyncCall,
      AsyncCallback callback,
      object state)
    {
      bool flag = false;
      Queue<PSInvocationStateInfo> invocationStateInfoQueue = new Queue<PSInvocationStateInfo>();
      lock (this.syncObject)
      {
        this.AssertNotDisposed();
        switch (this.invocationStateInfo.State)
        {
          case PSInvocationState.NotStarted:
            this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Stopping, (Exception) null);
            invocationStateInfoQueue.Enqueue(new PSInvocationStateInfo(PSInvocationState.Stopped, (Exception) null));
            break;
          case PSInvocationState.Running:
            this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Stopping, (Exception) null);
            flag = true;
            break;
          case PSInvocationState.Stopping:
          case PSInvocationState.Stopped:
          case PSInvocationState.Completed:
          case PSInvocationState.Failed:
            this.stopAsyncResult = new PowerShellAsyncResult(this.instanceId, callback, state, (PSDataCollection<PSObject>) null, false);
            this.stopAsyncResult.SetAsCompleted((Exception) null);
            return (IAsyncResult) this.stopAsyncResult;
        }
        this.stopAsyncResult = new PowerShellAsyncResult(this.instanceId, callback, state, (PSDataCollection<PSObject>) null, false);
      }
      this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
      if (this.rsConnection is RunspacePool rsConnection && rsConnection.IsRemote)
      {
        if (this.remotePowerShell != null)
        {
          this.remotePowerShell.StopAsync();
          if (isSyncCall)
            this.stopAsyncResult.AsyncWaitHandle.WaitOne();
        }
      }
      else if (flag)
        this.worker.Stop(isSyncCall);
      else if (isSyncCall)
        this.StopHelper((object) invocationStateInfoQueue);
      else
        ThreadPool.QueueUserWorkItem(new WaitCallback(this.StopThreadProc), (object) invocationStateInfoQueue);
      return (IAsyncResult) this.stopAsyncResult;
    }

    private void StopHelper(object state)
    {
      Queue<PSInvocationStateInfo> invocationStateInfoQueue = state as Queue<PSInvocationStateInfo>;
      while (invocationStateInfoQueue.Count > 0)
        this.SetStateChanged(invocationStateInfoQueue.Dequeue());
      this.InternalClearSuppressExceptions();
    }

    private void StopThreadProc(object state)
    {
      try
      {
        this.StopHelper(state);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw;
      }
    }

    internal ClientRemotePowerShell RemotePowerShell => this.remotePowerShell;

    internal string HistoryString
    {
      get => this.historyString;
      set => this.historyString = value;
    }

    internal static PowerShell FromPSObjectForRemoting(PSObject powerShellAsPSObject)
    {
      if (powerShellAsPSObject == null)
        throw PowerShell.tracer.NewArgumentNullException(nameof (powerShellAsPSObject));
      PSCommand psCommand = (PSCommand) null;
      foreach (PSObject commandAsPSObject in RemotingDecoder.EnumerateListProperty<PSObject>(powerShellAsPSObject, "Cmds"))
      {
        Command command = Command.FromPSObjectForRemoting(commandAsPSObject);
        if (psCommand == null)
          psCommand = new PSCommand(command);
        else
          psCommand.AddCommand(command);
      }
      PowerShell powerShell = PowerShell.Create(RemotingDecoder.GetPropertyValue<bool>(powerShellAsPSObject, "IsNested"), psCommand);
      powerShell.HistoryString = RemotingDecoder.GetPropertyValue<string>(powerShellAsPSObject, "History");
      powerShell.RedirectShellErrorOutputPipe = RemotingDecoder.GetPropertyValue<bool>(powerShellAsPSObject, "RedirectShellErrorOutputPipe");
      return powerShell;
    }

    internal PSObject ToPSObjectForRemoting()
    {
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      List<PSObject> psObjectList = new List<PSObject>(this.Commands.Commands.Count);
      foreach (Command command in (Collection<Command>) this.Commands.Commands)
        psObjectList.Add(command.ToPSObjectForRemoting());
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Cmds", (object) psObjectList));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("IsNested", (object) this.IsNested));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("History", (object) this.historyString));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("RedirectShellErrorOutputPipe", (object) this.RedirectShellErrorOutputPipe));
      return emptyPsObject;
    }

    private sealed class Worker
    {
      private ObjectStreamBase inputStream;
      private ObjectStreamBase outputStream;
      private ObjectStreamBase errorStream;
      private PSInvocationSettings settings;
      private IAsyncResult getRunspaceAsyncResult;
      private Pipeline currentlyRunningPipeline;
      private bool isNotActive;
      private PowerShell shell;
      private object syncObject = new object();

      internal Worker(
        ObjectStreamBase inputStream,
        ObjectStreamBase outputStream,
        PSInvocationSettings settings,
        PowerShell shell)
      {
        this.inputStream = inputStream;
        this.outputStream = outputStream;
        this.errorStream = (ObjectStreamBase) new PSDataCollectionStream<ErrorRecord>(shell.instanceId, shell.errorBuffer);
        this.settings = settings;
        this.shell = shell;
      }

      internal IAsyncResult GetRunspaceAsyncResult
      {
        get => this.getRunspaceAsyncResult;
        set => this.getRunspaceAsyncResult = value;
      }

      internal Pipeline CurrentlyRunningPipeline => this.currentlyRunningPipeline;

      internal void CreateRunspaceIfNeededAndDoWork(object state) => this.CreateRunspaceIfNeededAndDoWork(state as Runspace, false);

      internal void CreateRunspaceIfNeededAndDoWork(Runspace rsToUse, bool isSync)
      {
        try
        {
          if (!(rsToUse is LocalRunspace))
          {
            lock (this.shell.syncObject)
            {
              if (this.shell.runspace != null)
              {
                rsToUse = this.shell.runspace;
              }
              else
              {
                Runspace runspace = this.settings == null || this.settings.Host == null ? RunspaceFactory.CreateRunspace() : RunspaceFactory.CreateRunspace(this.settings.Host);
                this.shell.SetRunspace(runspace, true);
                rsToUse = runspace;
                rsToUse.Open();
              }
            }
          }
          this.ConstructPipelineAndDoWork(rsToUse, isSync);
        }
        catch (Exception ex)
        {
          lock (this.syncObject)
          {
            if (this.isNotActive)
              return;
            this.isNotActive = true;
          }
          this.shell.PipelineStateChanged((object) this, new PipelineStateEventArgs(new PipelineStateInfo(PipelineState.Failed, ex)));
          if (!isSync)
            CommandProcessorBase.CheckForSevereException(ex);
          else
            throw;
        }
      }

      internal void RunspaceAvailableCallback(IAsyncResult asyncResult)
      {
        try
        {
          RunspacePool rsConnection = this.shell.rsConnection as RunspacePool;
          Runspace runspace = rsConnection.EndGetRunspace(asyncResult);
          if (this.ConstructPipelineAndDoWork(runspace, false))
            return;
          rsConnection.ReleaseRunspace(runspace);
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          lock (this.syncObject)
          {
            if (this.isNotActive)
              return;
            this.isNotActive = true;
          }
          this.shell.PipelineStateChanged((object) this, new PipelineStateEventArgs(new PipelineStateInfo(PipelineState.Failed, ex)));
        }
      }

      internal bool ConstructPipelineAndDoWork(Runspace rs, bool performSyncInvoke)
      {
        lock (this.syncObject)
        {
          if (this.isNotActive)
            return false;
          if (!(rs is LocalRunspace runspace))
            throw PowerShell.tracer.NewNotImplementedException();
          CommandCollection commands = this.shell.Commands.Commands;
          int num1 = this.settings == null || !this.settings.AddToHistory ? 0 : 1;
          int num2 = this.shell.IsNested ? 1 : 0;
          ObjectStreamBase inputStream = this.inputStream;
          ObjectStreamBase outputStream = this.outputStream;
          ObjectStreamBase errorStream = this.errorStream;
          PSInformationalBuffers informationalBuffers = this.shell.informationalBuffers;
          LocalPipeline localPipeline = new LocalPipeline(runspace, commands, num1 != 0, num2 != 0, inputStream, outputStream, errorStream, informationalBuffers);
          if (!string.IsNullOrEmpty(this.shell.HistoryString))
            localPipeline.SetHistoryString(this.shell.HistoryString);
          localPipeline.RedirectShellErrorOutputPipe = this.shell.RedirectShellErrorOutputPipe;
          this.currentlyRunningPipeline = (Pipeline) localPipeline;
        }
        this.currentlyRunningPipeline.InvocationSettings = this.settings;
        this.currentlyRunningPipeline.StateChanged += new EventHandler<PipelineStateEventArgs>(this.shell.PipelineStateChanged);
        if (performSyncInvoke)
          this.currentlyRunningPipeline.Invoke();
        else
          this.currentlyRunningPipeline.InvokeAsync();
        return true;
      }

      internal void Stop(bool isSyncCall)
      {
        lock (this.syncObject)
        {
          if (this.isNotActive)
            return;
          this.isNotActive = true;
          if (this.currentlyRunningPipeline != null)
          {
            if (isSyncCall)
            {
              this.currentlyRunningPipeline.Stop();
              return;
            }
            this.currentlyRunningPipeline.StopAsync();
            return;
          }
          if (this.getRunspaceAsyncResult != null)
            (this.shell.rsConnection as RunspacePool).CancelGetRunspace(this.getRunspaceAsyncResult);
        }
        Queue<PSInvocationStateInfo> invocationStateInfoQueue = new Queue<PSInvocationStateInfo>();
        invocationStateInfoQueue.Enqueue(new PSInvocationStateInfo(PSInvocationState.Stopped, (Exception) null));
        if (isSyncCall)
          this.shell.StopHelper((object) invocationStateInfoQueue);
        else
          ThreadPool.QueueUserWorkItem(new WaitCallback(this.shell.StopThreadProc), (object) invocationStateInfoQueue);
      }

      internal void InternalClearSuppressExceptions()
      {
        try
        {
          if (this.settings != null && this.settings.WindowsIdentityToImpersonate != null)
          {
            this.settings.WindowsIdentityToImpersonate.Dispose();
            this.settings.WindowsIdentityToImpersonate = (WindowsIdentity) null;
          }
          this.inputStream.Close();
          this.outputStream.Close();
          this.errorStream.Close();
          if (this.currentlyRunningPipeline == null)
            return;
          this.currentlyRunningPipeline.StateChanged -= new EventHandler<PipelineStateEventArgs>(this.shell.PipelineStateChanged);
          if (this.getRunspaceAsyncResult == null && this.shell.rsConnection == null)
            this.currentlyRunningPipeline.Runspace.Close();
          else if (this.shell.rsConnection is RunspacePool rsConnection)
            rsConnection.ReleaseRunspace(this.currentlyRunningPipeline.Runspace);
          this.currentlyRunningPipeline.Dispose();
        }
        catch (ArgumentException ex)
        {
        }
        catch (InvalidOperationException ex)
        {
        }
        catch (InvalidRunspaceStateException ex)
        {
        }
        catch (InvalidRunspacePoolStateException ex)
        {
        }
        this.currentlyRunningPipeline = (Pipeline) null;
      }
    }
  }
}
