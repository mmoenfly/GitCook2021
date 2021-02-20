// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SessionState
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public sealed class SessionState
  {
    [TraceSource("State", "The APIs that are exposed to the Cmdlet base class for manipulating the engine state")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("State", "The APIs that are exposed to the Cmdlet base class for manipulating the engine state");
    private SessionStateInternal sessionState;
    private DriveManagementIntrinsics drive;
    private CmdletProviderManagementIntrinsics provider;
    private PathIntrinsics path;
    private PSVariableIntrinsics variable;

    internal SessionState(SessionStateInternal sessionState)
    {
      using (SessionState.tracer.TraceConstructor((object) this))
        this.sessionState = sessionState != null ? sessionState : throw SessionState.tracer.NewArgumentNullException(nameof (sessionState));
    }

    internal SessionState(SessionStateInternal parent, bool createAsChild, bool linkToGlobal)
    {
      ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
      if (executionContextFromTls == null)
        throw new InvalidOperationException("ExecutionContext");
      this.sessionState = !createAsChild ? new SessionStateInternal(executionContextFromTls) : new SessionStateInternal(parent, linkToGlobal, executionContextFromTls);
      this.sessionState.PublicSessionState = this;
    }

    public SessionState()
      : this((SessionStateInternal) null, false, false)
    {
    }

    public DriveManagementIntrinsics Drive
    {
      get
      {
        if (this.drive == null)
          this.drive = new DriveManagementIntrinsics(this.sessionState);
        return this.drive;
      }
    }

    public CmdletProviderManagementIntrinsics Provider
    {
      get
      {
        if (this.provider == null)
          this.provider = new CmdletProviderManagementIntrinsics(this.sessionState);
        return this.provider;
      }
    }

    public PathIntrinsics Path
    {
      get
      {
        if (this.path == null)
          this.path = new PathIntrinsics(this.sessionState);
        return this.path;
      }
    }

    public PSVariableIntrinsics PSVariable
    {
      get
      {
        if (this.variable == null)
          this.variable = new PSVariableIntrinsics(this.sessionState);
        return this.variable;
      }
    }

    public PSLanguageMode LanguageMode
    {
      get => this.sessionState.LanguageMode;
      set => this.sessionState.LanguageMode = value;
    }

    public bool UseFullLanguageModeInDebugger => this.sessionState.UseFullLanguageModeInDebugger;

    public List<string> Scripts => this.sessionState.Scripts;

    public List<string> Applications => this.sessionState.Applications;

    public PSModuleInfo Module => this.sessionState.Module;

    public ProviderIntrinsics InvokeProvider => this.sessionState.InvokeProvider;

    public CommandInvocationIntrinsics InvokeCommand => this.sessionState.ExecutionContext.EngineIntrinsics.InvokeCommand;

    public static void ThrowIfNotVisible(CommandOrigin origin, object valueToCheck)
    {
      if (SessionState.IsVisible(origin, valueToCheck))
        return;
      switch (valueToCheck)
      {
        case System.Management.Automation.PSVariable psVariable:
          SessionStateException sessionStateException1 = new SessionStateException(psVariable.Name, SessionStateCategory.Variable, "VariableIsPrivate", ErrorCategory.PermissionDenied, new object[0]);
          SessionState.tracer.TraceException((Exception) sessionStateException1);
          throw sessionStateException1;
        case CommandInfo commandInfo:
          string itemName = (string) null;
          if (commandInfo != null)
            itemName = commandInfo.Name;
          SessionStateException sessionStateException2 = itemName == null ? new SessionStateException("", SessionStateCategory.Command, "CommandIsPrivate", ErrorCategory.PermissionDenied, new object[0]) : new SessionStateException(itemName, SessionStateCategory.Command, "NamedCommandIsPrivate", ErrorCategory.PermissionDenied, new object[0]);
          SessionState.tracer.TraceException((Exception) sessionStateException2);
          throw sessionStateException2;
        default:
          SessionStateException sessionStateException3 = new SessionStateException((string) null, SessionStateCategory.Resource, "ResourceIsPrivate", ErrorCategory.PermissionDenied, new object[0]);
          SessionState.tracer.TraceException((Exception) sessionStateException3);
          throw sessionStateException3;
      }
    }

    public static bool IsVisible(CommandOrigin origin, object valueToCheck) => origin == CommandOrigin.Internal || !(valueToCheck is IHasSessionStateEntryVisibility stateEntryVisibility) || stateEntryVisibility.Visibility == SessionStateEntryVisibility.Public;

    public static bool IsVisible(CommandOrigin origin, System.Management.Automation.PSVariable variable)
    {
      if (origin == CommandOrigin.Internal)
        return true;
      if (variable == null)
        throw SessionState.tracer.NewArgumentNullException(nameof (variable));
      return variable.Visibility == SessionStateEntryVisibility.Public;
    }

    public static bool IsVisible(CommandOrigin origin, CommandInfo commandInfo)
    {
      if (origin == CommandOrigin.Internal)
        return true;
      if (commandInfo == null)
        throw SessionState.tracer.NewArgumentNullException(nameof (commandInfo));
      return commandInfo.Visibility == SessionStateEntryVisibility.Public;
    }

    internal SessionStateInternal Internal => this.sessionState;
  }
}
