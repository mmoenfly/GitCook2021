// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptCommandProcessor
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class ScriptCommandProcessor : CommandProcessorBase
  {
    private ScriptBlock _scriptBlock;
    private bool _fromPipeline;
    private bool _interactiveCommand;
    private bool _dontUseScopeCommandOrigin;
    private bool _rethrowExitException;
    private ArrayList _input = new ArrayList();
    private bool _exitWasCalled;
    private bool _argsBound;
    private ScriptParameterBinderController _scriptParameterBinderController;
    [TraceSource("ScriptCommandProcessor", "ScriptCommandProcessor")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ScriptCommandProcessor), nameof (ScriptCommandProcessor));

    internal override ScriptBlock ScriptBlock => this._scriptBlock;

    internal ScriptCommandProcessor(
      string script,
      ExecutionContext context,
      bool isFilter,
      bool useLocalScope,
      bool interactiveCommand,
      CommandOrigin origin)
    {
      this.Context = context;
      this._interactiveCommand = interactiveCommand;
      this._dontUseScopeCommandOrigin = false;
      this._rethrowExitException = this.Context.ScriptCommandProcessorShouldRethrowExit;
      this.Context.ScriptCommandProcessorShouldRethrowExit = false;
      if (origin == CommandOrigin.Runspace && this.Context.LanguageMode == PSLanguageMode.NoLanguage)
        throw InterpreterError.NewInterpreterException((object) script, typeof (ParseException), (Token) null, "ScriptsNotAllowed");
      this._scriptBlock = this.Context.Engine.ParseScriptBlock(script, interactiveCommand);
      if (origin == CommandOrigin.Runspace)
      {
        switch (this.Context.LanguageMode)
        {
          case PSLanguageMode.FullLanguage:
            break;
          case PSLanguageMode.RestrictedLanguage:
            RestrictedLanguageModeChecker.Check(this.Context.Engine.EngineParser, this._scriptBlock, (IEnumerable<string>) null, false);
            break;
          default:
            throw new InvalidOperationException("Invalid langage mode was set when building a ScriptCommandProcessor");
        }
      }
      this.CommandInfo = (CommandInfo) new ScriptInfo(string.Empty, this._scriptBlock, context);
      this._fromPipeline = true;
      this.SetupCommand();
      this.Command.CommandOriginInternal = origin;
      this.UseLocalScope = useLocalScope;
    }

    internal ScriptCommandProcessor(
      ScriptBlock scriptBlock,
      ExecutionContext context,
      bool useLocalScope)
    {
      this.Context = context;
      this._scriptBlock = scriptBlock;
      this._rethrowExitException = this.Context.ScriptCommandProcessorShouldRethrowExit;
      this.Context.ScriptCommandProcessorShouldRethrowExit = false;
      this._dontUseScopeCommandOrigin = false;
      this.CommandInfo = (CommandInfo) new ScriptInfo(string.Empty, this._scriptBlock, context);
      if (!this._scriptBlock.IsSynthesized)
        this._fromPipeline = true;
      this.SetupCommand();
      if (scriptBlock.SessionStateInternal != null)
        this.CommandSessionState = scriptBlock.SessionStateInternal;
      this.UseLocalScope = useLocalScope;
    }

    internal ScriptCommandProcessor(
      FunctionInfo functionInfo,
      ExecutionContext context,
      bool useLocalScope)
      : base((CommandInfo) functionInfo)
    {
      this.Context = context;
      this._scriptBlock = functionInfo.ScriptBlock;
      this._dontUseScopeCommandOrigin = true;
      this._rethrowExitException = this.Context.ScriptCommandProcessorShouldRethrowExit;
      this.Context.ScriptCommandProcessorShouldRethrowExit = false;
      this.SetupCommand();
      if (this._scriptBlock.SessionStateInternal != null)
        this.CommandSessionState = this._scriptBlock.SessionStateInternal;
      this.UseLocalScope = useLocalScope;
    }

    internal ScriptCommandProcessor(
      FilterInfo filterInfo,
      ExecutionContext context,
      bool useLocalScope)
      : base((CommandInfo) filterInfo)
    {
      this.Context = context;
      this._scriptBlock = filterInfo.ScriptBlock;
      this._dontUseScopeCommandOrigin = true;
      this._rethrowExitException = this.Context.ScriptCommandProcessorShouldRethrowExit;
      this.Context.ScriptCommandProcessorShouldRethrowExit = false;
      this.SetupCommand();
      if (this._scriptBlock.SessionStateInternal != null)
        this.CommandSessionState = this._scriptBlock.SessionStateInternal;
      this.UseLocalScope = useLocalScope;
    }

    internal ScriptCommandProcessor(
      ScriptInfo scriptInfo,
      ExecutionContext context,
      bool useLocalScope)
      : base((CommandInfo) scriptInfo)
    {
      this.Context = context;
      this.FromScriptFile = true;
      this._rethrowExitException = this.Context.ScriptCommandProcessorShouldRethrowExit;
      this.Context.ScriptCommandProcessorShouldRethrowExit = false;
      this._scriptBlock = scriptInfo.ScriptBlock;
      this._dontUseScopeCommandOrigin = true;
      this.SetupCommand();
      if (this._scriptBlock.SessionStateInternal != null)
        this.CommandSessionState = this._scriptBlock.SessionStateInternal;
      if (!useLocalScope)
        return;
      this.UseLocalScope = useLocalScope;
      if (this.CommandSessionState == null)
        this.CommandSessionState = context.EngineSessionState;
      this.CommandScope = this.CommandSessionState.NewScope(this.FromScriptFile);
    }

    internal ScriptCommandProcessor(
      ExternalScriptInfo scriptInfo,
      ExecutionContext context,
      bool useLocalScope)
      : base((CommandInfo) scriptInfo)
    {
      this.Context = context;
      this.FromScriptFile = true;
      this._rethrowExitException = this.Context.ScriptCommandProcessorShouldRethrowExit;
      this.Context.ScriptCommandProcessorShouldRethrowExit = false;
      this._scriptBlock = scriptInfo.ScriptBlock;
      this._dontUseScopeCommandOrigin = true;
      this.SetupCommand();
      if (this._scriptBlock.SessionStateInternal != null)
        this.CommandSessionState = this._scriptBlock.SessionStateInternal;
      if (!useLocalScope)
        return;
      this.UseLocalScope = useLocalScope;
      if (this.CommandSessionState == null)
        this.CommandSessionState = context.EngineSessionState;
      this.CommandScope = this.CommandSessionState.NewScope(this.FromScriptFile);
    }

    private void SetupCommand()
    {
      ScriptCommand scriptCommand = new ScriptCommand();
      scriptCommand.CommandInfo = this.CommandInfo;
      this.Command = (InternalCommand) scriptCommand;
      this.Command.commandRuntime = (ICommandRuntime) (this.commandRuntime = new MshCommandRuntime(this.Context, this.CommandInfo, (InternalCommand) scriptCommand));
      this.CommandSessionState = this.Context.EngineSessionState;
    }

    internal override void Prepare(params CommandParameterInternal[] parameters)
    {
      InternalCommand command = this.Command;
      this.ParameterBinderController.BackupDefaultParameters();
      foreach (CommandParameterInternal parameter in parameters)
        this.arguments.Add(parameter);
      this.commandRuntime.ClearOutputAndErrorPipes();
    }

    internal override void DoBegin()
    {
      if (this.RanBeginAlready)
        return;
      this.RanBeginAlready = true;
      this.SetCurrentScopeToExecutionScope();
      CommandProcessorBase commandProcessor = this.Context.CurrentCommandProcessor;
      try
      {
        this.Context.CurrentCommandProcessor = (CommandProcessorBase) this;
        if (this._scriptBlock.Begin == null)
          return;
        this.RunClause(this._scriptBlock.Begin, (object) AutomationNull.Value, (object) this._input);
      }
      finally
      {
        this.Context.CurrentCommandProcessor = commandProcessor;
        this.RestorePreviousScope();
      }
    }

    internal override void ProcessRecord()
    {
      if (this._exitWasCalled)
        return;
      if (!this.RanBeginAlready)
      {
        this.RanBeginAlready = true;
        if (this._scriptBlock.Begin != null)
          this.RunClause(this._scriptBlock.Begin, (object) AutomationNull.Value, (object) this._input);
      }
      if (this._scriptBlock.Process != null && !this.IsPipelineInputExpected())
        this.RunClause(this._scriptBlock.Process, (object) null, (object) this._input);
      else if (this._scriptBlock.Process != null && this.IsPipelineInputExpected())
      {
        this.DoProcessRecordWithInput();
      }
      else
      {
        if (this._scriptBlock.Process != null || !this.IsPipelineInputExpected() || this.CommandRuntime.InputPipe.ExternalReader != null)
          return;
        while (this.Read())
          this._input.Add((object) this.Command.CurrentPipelineObject);
      }
    }

    private void DoProcessRecordWithInput()
    {
      while (this.Read())
      {
        this._input.Add((object) this.Command.CurrentPipelineObject);
        ++this.Command.MyInvocation.PipelineIterationInfo[this.Command.MyInvocation.PipelinePosition];
        this.RunClause(this._scriptBlock.Process, (object) this.Command.CurrentPipelineObject, (object) this._input);
        this._input.Clear();
      }
    }

    internal override void Complete()
    {
      if (this._exitWasCalled)
        return;
      if (this._scriptBlock.Process != null && this.IsPipelineInputExpected())
        this.DoProcessRecordWithInput();
      if (this._scriptBlock.End != null)
      {
        if (this.CommandRuntime.InputPipe.ExternalReader == null)
        {
          if (this.IsPipelineInputExpected())
          {
            while (this.Read())
              this._input.Add((object) this.Command.CurrentPipelineObject);
          }
          this.RunClause(this._scriptBlock.End, (object) AutomationNull.Value, (object) this._input);
        }
        else
          this.RunClause(this._scriptBlock.End, (object) AutomationNull.Value, (object) this.CommandRuntime.InputPipe.ExternalReader.GetReadEnumerator());
      }
      else
      {
        if (this._scriptBlock.Begin != null || this._scriptBlock.Process != null || !this.Context.Debugger.IsOn)
          return;
        this.Context.Debugger.CheckCommand(this.Command.MyInvocation);
      }
    }

    internal void StopProcessing()
    {
    }

    private void RunClause(ParseTreeNode clause, object dollarUnderbar, object inputToProcess)
    {
      if (clause == null)
        return;
      if (this.Context.Debugger.IsOn)
        this.Context.Debugger.CheckCommand(this.Command.MyInvocation);
      bool flag = false;
      try
      {
        this.Context.Debugger.PushMethodCall(this.Command.MyInvocation, this._scriptBlock);
        if (this.FromScriptFile)
        {
          if (this.CommandInfo is ExternalScriptInfo commandInfo)
            this.Context.Debugger.PushRunning(commandInfo.Path, this._scriptBlock, true);
          else
            this.Context.Debugger.PushRunning(this.CommandInfo.Name, this._scriptBlock, true);
          flag = true;
        }
        else if (this._scriptBlock.File != null && this._scriptBlock.File.Length > 0)
        {
          this.Context.Debugger.PushRunning(this._scriptBlock.File, this._scriptBlock, false);
          flag = true;
        }
        if (this.Context.PSDebug > 1)
        {
          if (string.IsNullOrEmpty(this.CommandInfo.Name))
            ScriptTrace.Trace(this.context, 1, "TraceEnteringScriptBlock");
          else if (string.IsNullOrEmpty(this._scriptBlock.File))
            ScriptTrace.Trace(this.context, 1, "TraceEnteringFunction", (object) this.CommandInfo.Name);
          else
            ScriptTrace.Trace(this.context, 1, "TraceEnteringFunctionDefinedInFile", (object) this.CommandInfo.Name, (object) this._scriptBlock.File);
        }
        this.EnterScope();
        if (!this._scriptBlock.IsSynthesized)
        {
          if (inputToProcess != AutomationNull.Value)
          {
            if (inputToProcess == null)
            {
              this.Context.InputVariable = (object) MshCommandRuntime.StaticEmptyArray.GetEnumerator();
            }
            else
            {
              IList list = inputToProcess as IList;
              this.Context.InputVariable = list != null ? (object) list.GetEnumerator() : (object) LanguagePrimitives.GetEnumerator(inputToProcess);
            }
          }
          if (dollarUnderbar != AutomationNull.Value)
            this.Context.UnderbarVariable = dollarUnderbar;
        }
        object sendToPipeline = (object) AutomationNull.Value;
        Pipe functionErrorOutputPipe = this.Context.ShellFunctionErrorOutputPipe;
        try
        {
          if (this.commandRuntime.MergeMyErrorOutputWithSuccess)
            this.Context.RedirectErrorPipe(this.commandRuntime.OutputPipe);
          else if (this.commandRuntime.ErrorOutputPipe.IsRedirected)
            this.Context.RedirectErrorPipe(this.commandRuntime.ErrorOutputPipe);
          sendToPipeline = this.ExecuteWithCatch(clause, (Array) null);
        }
        finally
        {
          this.Context.RestoreErrorPipe(functionErrorOutputPipe);
          this.ExitScope();
        }
        if (sendToPipeline == AutomationNull.Value)
          return;
        IEnumerator enumerator = LanguagePrimitives.GetEnumerator(sendToPipeline);
        if (enumerator != null)
        {
          while (enumerator.MoveNext())
            this.commandRuntime._WriteObjectSkipAllowCheck(enumerator.Current);
        }
        else
          this.commandRuntime._WriteObjectSkipAllowCheck(sendToPipeline);
      }
      finally
      {
        this.Context.Debugger.PopMethodCall();
        if (flag)
          this.Context.Debugger.PopRunning();
      }
    }

    private object ExecuteWithCatch(ParseTreeNode ptn, Array inputToProcess)
    {
      object newValue = (object) null;
      CommandOrigin scopeOrigin = this.Context.EngineSessionState.currentScope.ScopeOrigin;
      if (string.IsNullOrEmpty(this.CommandInfo.Name))
      {
        if (!this._fromPipeline)
          goto label_3;
      }
      newValue = this.Context.GetVariable("MyInvocation");
      this.Context.SetVariable("MyInvocation", (object) this.Command.MyInvocation);
label_3:
      try
      {
        if (this._dontUseScopeCommandOrigin)
          this.Context.EngineSessionState.currentScope.ScopeOrigin = CommandOrigin.Internal;
        else
          this.Context.EngineSessionState.currentScope.ScopeOrigin = this.Command.CommandOrigin;
        return ptn.Execute(inputToProcess, this.commandRuntime.OutputPipe, this.Context);
      }
      catch (ExitException ex)
      {
        if (!this.FromScriptFile || this._rethrowExitException)
        {
          throw;
        }
        else
        {
          this._exitWasCalled = true;
          int num = (int) ex.Argument;
          this.Command.Context.SetVariable("global:LASTEXITCODE", (object) num);
          if (num != 0)
            this.commandRuntime.PipelineProcessor.ExecutionFailed = true;
          return (object) AutomationNull.Value;
        }
      }
      catch (TerminateException ex)
      {
        throw;
      }
      catch (ReturnException ex)
      {
        return ex.Argument;
      }
      catch (RuntimeException ex)
      {
        this.ManageScriptException(ex);
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        ScriptCommandProcessor.tracer.TraceException(ex);
        throw this.ManageInvocationException(ex);
      }
      finally
      {
        this.Context.EngineSessionState.currentScope.ScopeOrigin = scopeOrigin;
        if (!string.IsNullOrEmpty(this.CommandInfo.Name) || this._fromPipeline)
          this.Context.SetVariable("MyInvocation", newValue);
      }
    }

    private void EnterScope()
    {
      if (!this._argsBound)
      {
        if (!this._scriptBlock.IsSynthesized)
          this.ScriptParameterBinderController.BindCommandLineParameters(this.arguments);
        this._argsBound = true;
      }
      this.Context.IncrementScopeDepth(ExecutionContext.FeaturesThatNeedDepthHandling.ScriptScope);
    }

    private void ExitScope()
    {
      this.Context.DecrementScopeDepth(ExecutionContext.FeaturesThatNeedDepthHandling.ScriptScope);
      if (this.UseLocalScope)
        return;
      this.ParameterBinderController.RestoreDefaultParameterValues((IEnumerable<MergedCompiledCommandParameter>) null);
    }

    internal override bool UseLocalScope
    {
      get => base.UseLocalScope;
      set => base.UseLocalScope = value;
    }

    internal ScriptParameterBinderController ScriptParameterBinderController
    {
      get
      {
        if (this._scriptParameterBinderController == null)
        {
          this._scriptParameterBinderController = (ScriptParameterBinderController) this.NewParameterBinderController(this.Command);
          this._scriptParameterBinderController.CommandLineParameters.SetPSBoundParametersVariable(this.Context);
          this._scriptParameterBinderController.CommandLineParameters.UpdateInvocationInfo(this.Command.MyInvocation);
          this.Command.MyInvocation.UnboundArguments = this._scriptParameterBinderController.DollarArgs;
        }
        return this._scriptParameterBinderController;
      }
    }

    internal override ParameterBinderController NewParameterBinderController(
      InternalCommand command)
    {
      return (ParameterBinderController) new ScriptParameterBinderController(this._scriptBlock, command.MyInvocation, this.Context, command, this.UseLocalScope);
    }

    internal override bool IsHelpRequested(out string helpTarget, out HelpCategory helpCategory)
    {
      if (this.arguments != null && this.CommandInfo != null && !string.IsNullOrEmpty(this.CommandInfo.Name))
      {
        foreach (CommandParameterInternal parameterInternal in this.arguments)
        {
          if (parameterInternal.Token != null && parameterInternal.Token.Is("-?"))
          {
            if (this.CommandInfo is IScriptCommandInfo commandInfo)
            {
              HelpInfo helpInfo = commandInfo.ScriptBlock.GetHelpInfo(this.Context, this.CommandInfo);
              if (helpInfo != null)
              {
                helpTarget = helpInfo.Name;
                helpCategory = helpInfo.HelpCategory;
                return true;
              }
              break;
            }
            break;
          }
        }
      }
      return base.IsHelpRequested(out helpTarget, out helpCategory);
    }
  }
}
