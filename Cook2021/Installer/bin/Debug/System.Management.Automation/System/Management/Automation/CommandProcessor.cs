// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandProcessor
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Reflection;

namespace System.Management.Automation
{
  internal class CommandProcessor : CommandProcessorBase
  {
    [TraceSource("CommandProcessor", "CommandProcessor")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CommandProcessor), nameof (CommandProcessor));
    private CmdletParameterBinderController cmdletParameterBinderController;
    private bool firstCallToRead = true;
    private bool bailInNextCall;

    internal CommandProcessor(CmdletInfo cmdletInfo, ExecutionContext context)
      : base((CommandInfo) cmdletInfo)
    {
      this.context = context;
      this.Init(cmdletInfo);
    }

    internal CommandProcessor(
      IScriptCommandInfo scriptCommandInfo,
      ExecutionContext context,
      bool useLocalScope)
      : base(scriptCommandInfo as CommandInfo)
    {
      this.context = context;
      this._useLocalScope = useLocalScope;
      this.Init(scriptCommandInfo);
    }

    internal CommandProcessor(string commandName, ExecutionContext context)
    {
      this.context = context;
      CmdletInfo cmdletInformation = (CmdletInfo) null;
      CommandInfo commandInfo = context.CommandDiscovery.LookupCommandInfo(commandName);
      if (commandInfo.CommandType == CommandTypes.Alias)
        commandInfo = ((AliasInfo) commandInfo).ResolvedCommand;
      if (commandInfo != null && commandInfo.CommandType == CommandTypes.Cmdlet)
        cmdletInformation = (CmdletInfo) commandInfo;
      this.CommandInfo = commandInfo;
      if (cmdletInformation == null)
        throw (ArgumentException) CommandProcessor.tracer.NewArgumentException(nameof (commandName), "DiscoveryExceptions", "CommandNameNotCmdlet", (object) nameof (commandName), (object) commandName);
      this.Init(cmdletInformation);
    }

    internal override ParameterBinderController NewParameterBinderController(
      InternalCommand command)
    {
      if (!(command is Cmdlet cmdlet))
        throw CommandProcessor.tracer.NewArgumentException(nameof (command));
      ParameterBinderBase parameterBinder = !(this.CommandInfo is IScriptCommandInfo commandInfo) ? (ParameterBinderBase) new ReflectionParameterBinder((object) cmdlet, cmdlet) : (ParameterBinderBase) new ScriptParameterBinder(commandInfo.ScriptBlock, cmdlet.MyInvocation, this.context, (InternalCommand) cmdlet);
      this.cmdletParameterBinderController = new CmdletParameterBinderController(cmdlet, this.CommandInfo.CommandMetadata, parameterBinder);
      return (ParameterBinderController) this.cmdletParameterBinderController;
    }

    internal CmdletParameterBinderController CmdletParameterBinderController
    {
      get
      {
        if (this.cmdletParameterBinderController == null)
          this.NewParameterBinderController(this.Command);
        return this.cmdletParameterBinderController;
      }
    }

    internal bool BindCommandLineParameters(params CommandParameterInternal[] parameters)
    {
      using (this.commandRuntime.AllowThisCommandToWrite(false))
      {
        CommandProcessor.tracer.WriteLine("Parameters: {0}, {1}", (object) parameters, (object) parameters.Length);
        foreach (CommandParameterInternal parameter in parameters)
          this.arguments.Add(parameter);
        if (this.Command is PSScriptCmdlet command)
        {
          command.PrepareForBinding(this.UseLocalScope);
          this.CmdletParameterBinderController.CommandLineParameters.SetPSBoundParametersVariable(this.Context);
        }
        this.CmdletParameterBinderController.CommandLineParameters.UpdateInvocationInfo(this.Command.MyInvocation);
        this.Command.MyInvocation.UnboundArguments = new List<object>();
        return this.CmdletParameterBinderController.BindCommandLineParameters(this.arguments);
      }
    }

    internal override void Prepare(params CommandParameterInternal[] parameters)
    {
      if (this.CommandInfo is CmdletInfo commandInfo)
        PSSQMAPI.IncrementData(commandInfo);
      this.commandRuntime.ClearOutputAndErrorPipes();
      this.BindCommandLineParameters(parameters);
    }

    internal override void ProcessRecord()
    {
      if (!this.RanBeginAlready)
      {
        this.RanBeginAlready = true;
        try
        {
          using (this.commandRuntime.AllowThisCommandToWrite(true))
            this.Command.DoBeginProcessing();
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          ParameterBinderBase.bindingTracer.TraceException(ex);
          CommandProcessor.tracer.TraceException(ex);
          throw this.ManageInvocationException(ex);
        }
      }
      while (this.Read())
      {
        Pipe functionErrorOutputPipe = this.context.ShellFunctionErrorOutputPipe;
        Exception exception = (Exception) null;
        try
        {
          if (this.RedirectShellErrorOutputPipe || this.context.ShellFunctionErrorOutputPipe != null)
            this.context.ShellFunctionErrorOutputPipe = this.commandRuntime.ErrorOutputPipe;
          using (this.commandRuntime.AllowThisCommandToWrite(true))
          {
            if (this.Context.Debugger.IsOn && !(this.Command is PSScriptCmdlet))
              this.Context.Debugger.CheckCommand(this.Command.MyInvocation);
            ++this.Command.MyInvocation.PipelineIterationInfo[this.Command.MyInvocation.PipelinePosition];
            this.Command.DoProcessRecord();
          }
        }
        catch (RuntimeException ex)
        {
          if (ex.WasThrownFromThrowStatement)
            throw;
          else
            exception = (Exception) ex;
        }
        catch (Exception ex)
        {
          exception = ex;
        }
        finally
        {
          this.context.ShellFunctionErrorOutputPipe = functionErrorOutputPipe;
        }
        if (exception != null)
        {
          CommandProcessorBase.CheckForSevereException(exception);
          ParameterBinderBase.bindingTracer.TraceException(exception);
          CommandProcessor.tracer.TraceException(exception);
          throw this.ManageInvocationException(exception);
        }
      }
    }

    internal override sealed bool Read()
    {
      if (this.bailInNextCall)
        return false;
      this.Command.ThrowIfStopping();
      if (this.firstCallToRead)
      {
        this.firstCallToRead = false;
        if (!this.IsPipelineInputExpected())
        {
          this.bailInNextCall = true;
          return true;
        }
      }
      bool flag = false;
      while (!flag)
      {
        object obj = this.commandRuntime.InputPipe.Retrieve();
        if (obj == AutomationNull.Value)
        {
          this.Command.CurrentPipelineObject = (PSObject) null;
          return false;
        }
        if (this.Command.MyInvocation.PipelinePosition == 1)
          ++this.Command.MyInvocation.PipelineIterationInfo[0];
        try
        {
          if (!this.ProcessInputPipelineObject(true, obj))
          {
            this.WriteInputObjectError(obj, "InputObjectNotBound");
            continue;
          }
        }
        catch (ParameterBindingException ex)
        {
          ex.ErrorRecord.SetTargetObject(obj);
          this.commandRuntime._WriteErrorSkipAllowCheck(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
          continue;
        }
        Collection<MergedCompiledCommandParameter> missingMandatoryParameters = new Collection<MergedCompiledCommandParameter>();
        using (ParameterBinderBase.bindingTracer.TraceScope("MANDATORY PARAMETER CHECK on cmdlet [{0}]", (object) this.CommandInfo.Name))
          flag = this.CmdletParameterBinderController.HandleUnboundMandatoryParameters(false, false, out missingMandatoryParameters);
        if (!flag)
        {
          string str = CmdletParameterBinderController.BuildMissingParamsString(missingMandatoryParameters);
          this.WriteInputObjectError(obj, "InputObjectMissingMandatory", (object) str);
        }
      }
      return true;
    }

    private void WriteInputObjectError(
      object inputObject,
      string resourceAndErrorId,
      params object[] args)
    {
      ErrorRecord errorRecord = new ErrorRecord((Exception) new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, this.Command.MyInvocation.ScriptToken, (string) null, (Type) null, inputObject == null ? (Type) null : inputObject.GetType(), "ParameterBinderStrings", resourceAndErrorId, args), resourceAndErrorId, ErrorCategory.InvalidArgument, inputObject);
      errorRecord.SetInvocationInfo(this.Command.MyInvocation);
      this.commandRuntime._WriteErrorSkipAllowCheck(errorRecord);
    }

    private bool ProcessInputPipelineObject(bool bindPipelineInput, object inputObject)
    {
      bool flag = true;
      PSObject inputToOperateOn = (PSObject) null;
      if (inputObject != null)
        inputToOperateOn = PSObject.AsPSObject(inputObject);
      this.Command.CurrentPipelineObject = inputToOperateOn;
      if (bindPipelineInput)
      {
        if (this.Command is PSScriptCmdlet)
          ((PSScriptCmdlet) this.Command).PrepareForBinding(this.UseLocalScope);
        flag = this.CmdletParameterBinderController.BindPipelineParameters(inputToOperateOn);
      }
      return flag;
    }

    private void Init(CmdletInfo cmdletInformation)
    {
      Cmdlet cmdlet = (Cmdlet) null;
      Exception exception = (Exception) null;
      string errorIdAndResourceId = "CmdletNotFoundException";
      try
      {
        cmdlet = (Cmdlet) Activator.CreateInstance(cmdletInformation.ImplementingType);
      }
      catch (TargetInvocationException ex)
      {
        CommandProcessor.tracer.TraceException((Exception) ex);
        CmdletInvocationException invocationException = new CmdletInvocationException(ex.InnerException, (InvocationInfo) null);
        MshLog.LogCommandHealthEvent(this.context, (Exception) invocationException, Severity.Warning);
        throw invocationException;
      }
      catch (MemberAccessException ex)
      {
        exception = (Exception) ex;
      }
      catch (TypeLoadException ex)
      {
        exception = (Exception) ex;
      }
      catch (InvalidCastException ex)
      {
        exception = (Exception) ex;
        errorIdAndResourceId = "CmdletDoesNotDeriveFromCmdletType";
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        CommandProcessor.tracer.TraceException(ex);
        MshLog.LogCommandHealthEvent(this.context, ex, Severity.Warning);
        throw;
      }
      if (exception != null)
      {
        CommandProcessor.tracer.TraceException(exception);
        MshLog.LogCommandHealthEvent(this.context, exception, Severity.Warning);
        CommandNotFoundException notFoundException = new CommandNotFoundException(cmdletInformation.Name, exception, errorIdAndResourceId, new object[1]
        {
          (object) exception.Message
        });
        CommandProcessor.tracer.TraceException((Exception) notFoundException);
        throw notFoundException;
      }
      this.Command = (InternalCommand) cmdlet;
      this.InitCommon();
    }

    private void Init(IScriptCommandInfo scriptCommandInfo)
    {
      PSScriptCmdlet psScriptCmdlet = new PSScriptCmdlet(scriptCommandInfo.ScriptBlock);
      psScriptCmdlet.UseLocalScope = this._useLocalScope;
      if (scriptCommandInfo.ScriptBlock.SessionStateInternal != null)
        this.CommandSessionState = scriptCommandInfo.ScriptBlock.SessionStateInternal;
      this.Command = (InternalCommand) psScriptCmdlet;
      this.InitCommon();
    }

    private void InitCommon()
    {
      this.Command.CommandInfo = this.CommandInfo;
      this.Command.Context = this.context;
      try
      {
        this.commandRuntime = new MshCommandRuntime(this.context, this.CommandInfo, this.Command);
        this.Command.commandRuntime = (ICommandRuntime) this.commandRuntime;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        CommandProcessor.tracer.TraceException(ex);
        MshLog.LogCommandHealthEvent(this.context, ex, Severity.Warning);
        throw;
      }
    }

    internal override bool IsHelpRequested(out string helpTarget, out HelpCategory helpCategory)
    {
      if (this.arguments != null)
      {
        foreach (CommandParameterInternal parameterInternal in this.arguments)
        {
          if (parameterInternal.Token != null && parameterInternal.Token.Is("-?"))
          {
            helpTarget = this.Command == null || this.Command.MyInvocation == null || string.IsNullOrEmpty(this.Command.MyInvocation.InvocationName) ? this.CommandInfo.Name : this.Command.MyInvocation.InvocationName;
            helpCategory = this.CommandInfo.HelpCategory;
            return true;
          }
        }
      }
      return base.IsHelpRequested(out helpTarget, out helpCategory);
    }

    internal override ScriptBlock ScriptBlock => this.Command is PSScriptCmdlet command ? command.ScriptBlock : (ScriptBlock) null;
  }
}
