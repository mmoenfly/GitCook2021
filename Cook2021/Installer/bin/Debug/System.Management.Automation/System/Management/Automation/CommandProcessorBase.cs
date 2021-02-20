// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandProcessorBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal abstract class CommandProcessorBase : IDisposable
  {
    [TraceSource("CommandProcessorBase", "CommandProcessorBase")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CommandProcessorBase), nameof (CommandProcessorBase));
    private InternalCommand command;
    internal bool RanBeginAlready;
    internal bool _addedToPipelineAlready;
    private CommandInfo commandInfo;
    private bool _fromScriptFile;
    private bool _redirectShellErrorOutputPipe;
    protected MshCommandRuntime commandRuntime;
    private ParameterBinderController parameterBinderController;
    protected bool _useLocalScope;
    protected ExecutionContext context;
    private SessionStateInternal _commandSessionState;
    private SessionStateScope _commandScope;
    private ActivationRecord _activationRecord;
    private SessionStateScope _previousScope;
    private ActivationRecord _previousActivationRecord;
    private SessionStateInternal _previousCommandSessionState;
    internal Collection<CommandParameterInternal> arguments = new Collection<CommandParameterInternal>();
    private bool firstCallToRead = true;
    private static bool alreadyFailing = false;
    private bool disposed;

    internal CommandProcessorBase()
    {
    }

    internal CommandProcessorBase(CommandInfo commandInfo) => this.commandInfo = commandInfo != null ? commandInfo : throw CommandProcessorBase.tracer.NewArgumentNullException(nameof (commandInfo));

    internal bool AddedToPipelineAlready
    {
      get => this._addedToPipelineAlready;
      set => this._addedToPipelineAlready = value;
    }

    internal CommandInfo CommandInfo
    {
      get => this.commandInfo;
      set => this.commandInfo = value;
    }

    public bool FromScriptFile
    {
      get => this._fromScriptFile;
      set => this._fromScriptFile = value;
    }

    internal bool RedirectShellErrorOutputPipe
    {
      get => this._redirectShellErrorOutputPipe;
      set => this._redirectShellErrorOutputPipe = value;
    }

    internal InternalCommand Command
    {
      get => this.command;
      set
      {
        if (value != null)
        {
          value.commandRuntime = (ICommandRuntime) this.commandRuntime;
          if (this.command != null)
            value.CommandInfo = this.command.CommandInfo;
          if (value.Context == null && this.context != null)
            value.Context = this.context;
        }
        this.command = value;
        this.parameterBinderController = (ParameterBinderController) null;
      }
    }

    internal MshCommandRuntime CommandRuntime
    {
      get => this.commandRuntime;
      set => this.commandRuntime = value;
    }

    internal abstract ParameterBinderController NewParameterBinderController(
      InternalCommand internalCommand);

    internal ParameterBinderController ParameterBinderController
    {
      get
      {
        if (this.parameterBinderController == null)
          this.parameterBinderController = this.NewParameterBinderController(this.Command);
        return this.parameterBinderController;
      }
    }

    internal virtual bool UseLocalScope
    {
      get => this._useLocalScope;
      set => this._useLocalScope = value;
    }

    internal ExecutionContext Context
    {
      get => this.context;
      set => this.context = value;
    }

    internal virtual ScriptBlock ScriptBlock => (ScriptBlock) null;

    internal virtual bool IsHelpRequested(out string helpTarget, out HelpCategory helpCategory)
    {
      helpTarget = (string) null;
      helpCategory = HelpCategory.None;
      return false;
    }

    internal static CommandProcessorBase CreateGetHelpCommandProcessor(
      ExecutionContext context,
      string helpTarget,
      HelpCategory helpCategory)
    {
      if (context == null)
        throw CommandProcessorBase.tracer.NewArgumentNullException(nameof (context));
      if (string.IsNullOrEmpty(helpTarget))
        throw CommandProcessorBase.tracer.NewArgumentNullException(nameof (helpTarget));
      CommandProcessorBase command = context.CreateCommand("get-help");
      command.AddParameter(new CommandParameterInternal("Name", (object) helpTarget));
      command.AddParameter(new CommandParameterInternal("Category", (object) helpCategory.ToString()));
      return command;
    }

    internal bool IsPipelineInputExpected() => this.commandRuntime.IsPipelineInputExpected;

    internal SessionStateInternal CommandSessionState
    {
      get => this._commandSessionState;
      set => this._commandSessionState = value;
    }

    protected internal SessionStateScope CommandScope
    {
      get => this._commandScope;
      set => this._commandScope = value;
    }

    internal void SetCurrentScopeToExecutionScope()
    {
      if (this._commandSessionState == null)
        this._commandSessionState = this.Context.EngineSessionState;
      this._previousScope = this._commandSessionState.CurrentScope;
      this._previousActivationRecord = this._commandSessionState.CurrentActivationRecord;
      this._previousCommandSessionState = this.Context.EngineSessionState;
      this.Context.EngineSessionState = this._commandSessionState;
      if (this._useLocalScope)
      {
        if (this._commandScope == null)
          this._commandScope = this._commandSessionState.NewScope(false);
        this._commandSessionState.CurrentScope = this._commandScope;
      }
      else
        this._commandSessionState.CurrentScope = this._commandScope == null ? this.commandRuntime.PipelineProcessor.ExecutionScope : this._commandScope;
      ScriptBlock scriptBlock = this.ScriptBlock;
      if (scriptBlock != null)
      {
        if (this._activationRecord == null)
          this._activationRecord = !scriptBlock.IsSynthesized ? new ActivationRecord(scriptBlock.PipelineSlots, scriptBlock.VariableSlots, this._commandSessionState.CurrentScope) : this._commandSessionState.CurrentActivationRecord;
      }
      else
        this._activationRecord = new ActivationRecord();
      this._commandSessionState.CurrentActivationRecord = this._activationRecord;
    }

    internal void RestorePreviousScope()
    {
      this.Context.EngineSessionState = this._previousCommandSessionState;
      if (this._previousScope != null)
        this._commandSessionState.CurrentScope = this._previousScope;
      this._commandSessionState.CurrentActivationRecord = this._previousActivationRecord;
    }

    internal void AddParameter(string name, object value)
    {
      if (string.IsNullOrEmpty(name))
        throw CommandProcessorBase.tracer.NewArgumentException(nameof (name));
      this.arguments.Add(new CommandParameterInternal(name, value));
    }

    internal void AddParameter(object value) => this.arguments.Add(new CommandParameterInternal(value));

    internal void AddParameter(CommandParameterInternal parameter)
    {
      if (parameter == null)
        throw CommandProcessorBase.tracer.NewArgumentNullException(nameof (parameter));
      this.arguments.Add(parameter);
    }

    internal abstract void Prepare(params CommandParameterInternal[] parameters);

    internal void DoPrepare(params CommandParameterInternal[] parameters)
    {
      CommandProcessorBase commandProcessor = this.context.CurrentCommandProcessor;
      try
      {
        this.Context.CurrentCommandProcessor = this;
        this.SetCurrentScopeToExecutionScope();
        this.Prepare(parameters);
      }
      finally
      {
        this.Context.CurrentCommandProcessor = commandProcessor;
        this.RestorePreviousScope();
      }
    }

    internal virtual void DoBegin()
    {
      if (this.RanBeginAlready)
        return;
      this.RanBeginAlready = true;
      Pipe functionErrorOutputPipe = this.context.ShellFunctionErrorOutputPipe;
      CommandProcessorBase commandProcessor = this.context.CurrentCommandProcessor;
      try
      {
        if (this.RedirectShellErrorOutputPipe || this.context.ShellFunctionErrorOutputPipe != null)
          this.context.ShellFunctionErrorOutputPipe = this.commandRuntime.ErrorOutputPipe;
        this.context.CurrentCommandProcessor = this;
        using (this.commandRuntime.AllowThisCommandToWrite(true))
        {
          using (ParameterBinderBase.bindingTracer.TraceScope("CALLING BeginProcessing"))
          {
            this.SetCurrentScopeToExecutionScope();
            this.Command.DoBeginProcessing();
          }
        }
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        ParameterBinderBase.bindingTracer.TraceException(ex);
        CommandProcessorBase.tracer.TraceException(ex);
        throw this.ManageInvocationException(ex);
      }
      finally
      {
        this.context.ShellFunctionErrorOutputPipe = functionErrorOutputPipe;
        this.context.CurrentCommandProcessor = commandProcessor;
        this.RestorePreviousScope();
      }
    }

    internal abstract void ProcessRecord();

    internal void DoExecute()
    {
      CommandProcessorBase commandProcessor = this.context.CurrentCommandProcessor;
      try
      {
        this.Context.CurrentCommandProcessor = this;
        this.SetCurrentScopeToExecutionScope();
        this.Context.IncrementScopeDepth(ExecutionContext.FeaturesThatNeedDepthHandling.CommandsInPipeline);
        this.ProcessRecord();
      }
      finally
      {
        this.Context.CurrentCommandProcessor = commandProcessor;
        this.Context.DecrementScopeDepth(ExecutionContext.FeaturesThatNeedDepthHandling.CommandsInPipeline);
        this.RestorePreviousScope();
      }
    }

    internal virtual void Complete()
    {
      this.ProcessRecord();
      try
      {
        using (this.commandRuntime.AllowThisCommandToWrite(true))
        {
          using (ParameterBinderBase.bindingTracer.TraceScope("CALLING EndProcessing"))
            this.Command.DoEndProcessing();
        }
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        ParameterBinderBase.bindingTracer.TraceException(ex);
        CommandProcessorBase.tracer.TraceException(ex);
        throw this.ManageInvocationException(ex);
      }
    }

    internal void DoComplete()
    {
      Pipe functionErrorOutputPipe = this.context.ShellFunctionErrorOutputPipe;
      CommandProcessorBase commandProcessor = this.context.CurrentCommandProcessor;
      try
      {
        if (this.RedirectShellErrorOutputPipe || this.context.ShellFunctionErrorOutputPipe != null)
          this.context.ShellFunctionErrorOutputPipe = this.commandRuntime.ErrorOutputPipe;
        this.context.CurrentCommandProcessor = this;
        this.SetCurrentScopeToExecutionScope();
        this.Complete();
      }
      finally
      {
        this.context.ShellFunctionErrorOutputPipe = functionErrorOutputPipe;
        this.context.CurrentCommandProcessor = commandProcessor;
        if (this._useLocalScope && this._commandScope != null)
          this._commandSessionState.RemoveScope(this._commandScope);
        if (this._previousScope != null)
          this._commandSessionState.CurrentScope = this._previousScope;
        this._commandSessionState.CurrentActivationRecord = this._previousActivationRecord;
        if (this._previousCommandSessionState != null)
          this.Context.EngineSessionState = this._previousCommandSessionState;
      }
    }

    public override string ToString() => this.commandInfo != null ? this.commandInfo.ToString() : "<NullCommandInfo>";

    internal virtual bool Read()
    {
      if (this.firstCallToRead)
        this.firstCallToRead = false;
      object obj = this.commandRuntime.InputPipe.Retrieve();
      if (obj == AutomationNull.Value)
        return false;
      if (this.Command.MyInvocation.PipelinePosition == 1)
        ++this.Command.MyInvocation.PipelineIterationInfo[0];
      this.Command.CurrentPipelineObject = LanguagePrimitives.AsPSObjectOrNull(obj);
      return true;
    }

    internal static void CheckForSevereException(Exception e)
    {
      switch (e)
      {
        case AccessViolationException _:
        case StackOverflowException _:
          try
          {
            if (CommandProcessorBase.alreadyFailing)
              break;
            CommandProcessorBase.alreadyFailing = true;
            MshLog.LogCommandHealthEvent(LocalPipeline.GetExecutionContextFromTLS(), e, Severity.Critical);
            break;
          }
          finally
          {
            WindowsErrorReporting.FailFast(e);
          }
      }
    }

    internal PipelineStoppedException ManageInvocationException(Exception e)
    {
      try
      {
        if (this.Command == null)
          return new PipelineStoppedException();
        switch (e)
        {
          case ProviderInvocationException innerException:
            e = (Exception) new CmdletProviderInvocationException(innerException, this.Command.MyInvocation);
            goto label_4;
          case PipelineStoppedException _:
          case CmdletInvocationException _:
          case ActionPreferenceStopException _:
          case HaltCommandException _:
          case FlowControlException _:
          case ScriptCallDepthException _:
label_4:
            if ((bool) this.commandRuntime.UseTransaction)
            {
              bool flag = false;
              for (Exception exception = e; exception != null; exception = exception.InnerException)
              {
                if (exception is TimeoutException)
                {
                  flag = true;
                  break;
                }
              }
              if (flag)
              {
                ErrorRecord errorRecord = new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.GetResourceString("TransactionStrings", "TransactionTimedOut")), "TRANSACTION_TIMEOUT", ErrorCategory.InvalidOperation, (object) e);
                errorRecord.SetInvocationInfo(this.Command.MyInvocation);
                e = (Exception) new CmdletInvocationException(errorRecord);
              }
              if (this.context.TransactionManager.HasTransaction && this.context.TransactionManager.RollbackPreference != RollbackSeverity.Never)
                this.Context.TransactionManager.Rollback(true);
            }
            return (PipelineStoppedException) this.commandRuntime.ManageException(e);
          default:
            e = (Exception) new CmdletInvocationException(e, this.Command.MyInvocation);
            goto label_4;
        }
      }
      catch (Exception ex)
      {
        CommandProcessorBase.tracer.TraceException(ex);
        throw;
      }
    }

    internal void ManageScriptException(RuntimeException e)
    {
      if (this.Command != null && this.commandRuntime.PipelineProcessor != null)
      {
        this.commandRuntime.PipelineProcessor.RecordFailure((Exception) e, this.Command);
        if (!(e is PipelineStoppedException) && !e.WasThrownFromThrowStatement)
          this.commandRuntime.AppendError((object) e);
      }
      throw new PipelineStoppedException();
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      if (disposing && this.Command is IDisposable command)
        command.Dispose();
      this.disposed = true;
    }

    ~CommandProcessorBase()
    {
      CommandProcessorBase.tracer.TraceFinalizer((object) this);
      this.Dispose(false);
    }
  }
}
