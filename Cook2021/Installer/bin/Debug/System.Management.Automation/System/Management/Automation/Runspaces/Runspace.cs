// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.Runspace
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Threading;
using System.Transactions;

namespace System.Management.Automation.Runspaces
{
  public abstract class Runspace : IDisposable
  {
    internal const string ResourceBase = "Runspace";
    internal const ApartmentState DefaultApartmentState = ApartmentState.Unknown;
    [ThreadStatic]
    private static Runspace ThreadSpecificDefaultRunspace = (Runspace) null;
    private ApartmentState apartmentState = ApartmentState.Unknown;
    private Guid _instanceId = Guid.NewGuid();
    private bool _skipUserProfile;
    private long _pipelineIdSeed;
    [TraceSource("Runspace", "Runspace base class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (Runspace), "Runspace base class");

    internal Runspace()
    {
    }

    public static Runspace DefaultRunspace
    {
      get => Runspace.ThreadSpecificDefaultRunspace;
      set => Runspace.ThreadSpecificDefaultRunspace = value;
    }

    public ApartmentState ApartmentState
    {
      get => this.apartmentState;
      set
      {
        if (this.RunspaceStateInfo.State != RunspaceState.BeforeOpen)
          throw new InvalidRunspaceStateException(ResourceManagerCache.FormatResourceString(nameof (Runspace), "ChangePropertyAfterOpen"));
        this.apartmentState = value;
      }
    }

    public abstract PSThreadOptions ThreadOptions { get; set; }

    public abstract Version Version { get; }

    public abstract RunspaceStateInfo RunspaceStateInfo { get; }

    public abstract RunspaceAvailability RunspaceAvailability { get; protected set; }

    public abstract RunspaceConfiguration RunspaceConfiguration { get; }

    public abstract InitialSessionState InitialSessionState { get; }

    public Guid InstanceId => this._instanceId;

    internal System.Management.Automation.ExecutionContext ExecutionContext => this.GetExecutionContext;

    internal bool SkipUserProfile
    {
      get => this._skipUserProfile;
      set => this._skipUserProfile = value;
    }

    public abstract RunspaceConnectionInfo ConnectionInfo { get; }

    internal Version GetRemoteProtocolVersion()
    {
      Version result;
      return PSPrimitiveDictionary.TryPathGet<Version>((IDictionary) this.GetApplicationPrivateData(), out result, "PSVersionTable", "PSRemotingProtocolVersion") ? result : RemotingConstants.ProtocolVersion;
    }

    public abstract event EventHandler<RunspaceStateEventArgs> StateChanged;

    public abstract event EventHandler<RunspaceAvailabilityEventArgs> AvailabilityChanged;

    internal abstract bool HasAvailabilityChangedSubscribers { get; }

    protected abstract void OnAvailabilityChanged(RunspaceAvailabilityEventArgs e);

    internal void UpdateRunspaceAvailability(PipelineState pipelineState, bool raiseEvent)
    {
      RunspaceAvailability runspaceAvailability = this.RunspaceAvailability;
      switch (runspaceAvailability)
      {
        case RunspaceAvailability.Available:
          if (pipelineState == PipelineState.Running)
          {
            this.RunspaceAvailability = RunspaceAvailability.Busy;
            break;
          }
          break;
        case RunspaceAvailability.AvailableForNestedCommand:
          switch (pipelineState)
          {
            case PipelineState.Running:
              this.RunspaceAvailability = RunspaceAvailability.Busy;
              break;
            case PipelineState.Completed:
              this.RunspaceAvailability = this.InNestedPrompt ? RunspaceAvailability.AvailableForNestedCommand : RunspaceAvailability.Available;
              break;
          }
          break;
        case RunspaceAvailability.Busy:
          switch (pipelineState)
          {
            case PipelineState.Stopped:
            case PipelineState.Completed:
            case PipelineState.Failed:
              if (this.InNestedPrompt || !(this is RemoteRunspace) && this.Debugger.InBreakpoint)
              {
                this.RunspaceAvailability = RunspaceAvailability.AvailableForNestedCommand;
                break;
              }
              Pipeline currentlyRunningPipeline = this.GetCurrentlyRunningPipeline();
              this.RunspaceAvailability = currentlyRunningPipeline != null ? (currentlyRunningPipeline.PipelineStateInfo.State != PipelineState.Running ? RunspaceAvailability.Available : RunspaceAvailability.Busy) : RunspaceAvailability.Available;
              break;
          }
          break;
      }
      if (!raiseEvent || this.RunspaceAvailability == runspaceAvailability)
        return;
      this.OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(this.RunspaceAvailability));
    }

    internal void UpdateRunspaceAvailability(PSInvocationState invocationState, bool raiseEvent)
    {
      switch (invocationState)
      {
        case PSInvocationState.NotStarted:
          this.UpdateRunspaceAvailability(PipelineState.NotStarted, raiseEvent);
          break;
        case PSInvocationState.Running:
          this.UpdateRunspaceAvailability(PipelineState.Running, raiseEvent);
          break;
        case PSInvocationState.Stopping:
          this.UpdateRunspaceAvailability(PipelineState.Stopping, raiseEvent);
          break;
        case PSInvocationState.Stopped:
          this.UpdateRunspaceAvailability(PipelineState.Stopped, raiseEvent);
          break;
        case PSInvocationState.Completed:
          this.UpdateRunspaceAvailability(PipelineState.Completed, raiseEvent);
          break;
        case PSInvocationState.Failed:
          this.UpdateRunspaceAvailability(PipelineState.Failed, raiseEvent);
          break;
      }
    }

    protected void UpdateRunspaceAvailability(RunspaceState runspaceState, bool raiseEvent)
    {
      RunspaceAvailability runspaceAvailability = this.RunspaceAvailability;
      switch (runspaceAvailability)
      {
        case RunspaceAvailability.None:
          if (runspaceState == RunspaceState.Opened)
          {
            this.RunspaceAvailability = RunspaceAvailability.Available;
            break;
          }
          break;
        case RunspaceAvailability.Available:
        case RunspaceAvailability.AvailableForNestedCommand:
        case RunspaceAvailability.Busy:
          switch (runspaceState)
          {
            case RunspaceState.Closed:
            case RunspaceState.Closing:
            case RunspaceState.Broken:
              this.RunspaceAvailability = RunspaceAvailability.None;
              break;
          }
          break;
      }
      if (!raiseEvent || this.RunspaceAvailability == runspaceAvailability)
        return;
      this.OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(this.RunspaceAvailability));
    }

    internal void UpdateRunspaceAvailability(RunspaceAvailability availability, bool raiseEvent)
    {
      RunspaceAvailability runspaceAvailability = this.RunspaceAvailability;
      this.RunspaceAvailability = availability;
      if (!raiseEvent || this.RunspaceAvailability == runspaceAvailability)
        return;
      this.OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(this.RunspaceAvailability));
    }

    internal void RaiseAvailabilityChangedEvent(RunspaceAvailability availability) => this.OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(availability));

    public abstract void Open();

    public abstract void OpenAsync();

    public abstract void Close();

    public abstract void CloseAsync();

    public abstract Pipeline CreatePipeline();

    public abstract Pipeline CreatePipeline(string command);

    public abstract Pipeline CreatePipeline(string command, bool addToHistory);

    public abstract Pipeline CreateNestedPipeline();

    public abstract Pipeline CreateNestedPipeline(string command, bool addToHistory);

    internal abstract Pipeline GetCurrentlyRunningPipeline();

    public abstract PSPrimitiveDictionary GetApplicationPrivateData();

    internal abstract void SetApplicationPrivateData(PSPrimitiveDictionary applicationPrivateData);

    public SessionStateProxy SessionStateProxy => this.GetSessionStateProxy();

    internal abstract SessionStateProxy GetSessionStateProxy();

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    internal abstract System.Management.Automation.ExecutionContext GetExecutionContext { get; }

    internal abstract bool InNestedPrompt { get; }

    public Debugger Debugger => this.GetExecutionContext.Debugger;

    public abstract PSEventManager Events { get; }

    public void SetBaseTransaction(CommittableTransaction transaction) => this.ExecutionContext.TransactionManager.SetBaseTransaction(transaction, RollbackSeverity.Error);

    public void SetBaseTransaction(CommittableTransaction transaction, RollbackSeverity severity) => this.ExecutionContext.TransactionManager.SetBaseTransaction(transaction, severity);

    public void ClearBaseTransaction() => this.ExecutionContext.TransactionManager.ClearBaseTransaction();

    internal long GeneratePipelineId() => Interlocked.Increment(ref this._pipelineIdSeed);
  }
}
