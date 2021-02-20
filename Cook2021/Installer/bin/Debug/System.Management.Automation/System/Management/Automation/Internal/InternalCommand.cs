// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.InternalCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;

namespace System.Management.Automation.Internal
{
  public abstract class InternalCommand
  {
    internal string CBResourcesBaseName = "CommandBaseStrings";
    internal ICommandRuntime commandRuntime;
    [TraceSource("InternalCommand", "InternalCommand")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (InternalCommand), nameof (InternalCommand));
    private Token callingToken;
    private InvocationInfo myInvocation;
    internal PSObject currentObjectInPipeline = AutomationNull.Value;
    private PSHost CBhost;
    private SessionState state;
    private CommandInfo commandInfo;
    private ExecutionContext context;
    internal CommandOrigin CommandOriginInternal = CommandOrigin.Internal;

    internal InternalCommand()
    {
      using (InternalCommand.tracer.TraceConstructor((object) this))
        this.CommandInfo = (CommandInfo) null;
    }

    internal Token CallingToken
    {
      get => this.callingToken;
      set => this.callingToken = value;
    }

    internal InvocationInfo MyInvocation
    {
      get
      {
        if (this.myInvocation == null)
          this.myInvocation = new InvocationInfo(this);
        return this.myInvocation;
      }
    }

    internal PSObject CurrentPipelineObject
    {
      get => this.currentObjectInPipeline;
      set => this.currentObjectInPipeline = value;
    }

    internal PSHost PSHostInternal => this.CBhost;

    internal SessionState State => this.state;

    internal bool IsStopping => this.commandRuntime is MshCommandRuntime commandRuntime && commandRuntime.IsStopping;

    internal CommandInfo CommandInfo
    {
      get => this.commandInfo;
      set => this.commandInfo = value;
    }

    internal ExecutionContext Context
    {
      get => this.context;
      set
      {
        this.context = value != null ? value : throw InternalCommand.tracer.NewArgumentNullException(nameof (Context));
        this.CBhost = (PSHost) this.context.EngineHostInterface;
        this.state = new SessionState(this.context.EngineSessionState);
      }
    }

    public CommandOrigin CommandOrigin => this.CommandOriginInternal;

    internal virtual void DoBeginProcessing()
    {
    }

    internal virtual void DoProcessRecord()
    {
    }

    internal virtual void DoEndProcessing()
    {
    }

    internal virtual void DoStopProcessing()
    {
    }

    internal void ThrowIfStopping()
    {
      if (this.IsStopping)
        throw new PipelineStoppedException();
    }
  }
}
