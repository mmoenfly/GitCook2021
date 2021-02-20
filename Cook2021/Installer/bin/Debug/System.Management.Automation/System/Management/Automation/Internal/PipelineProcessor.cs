// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PipelineProcessor
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Management.Automation.Internal
{
  internal class PipelineProcessor : IDisposable
  {
    [TraceSource("PipelineProcessor", "PipelineProcessor")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PipelineProcessor), nameof (PipelineProcessor));
    private List<CommandProcessorBase> _commands = new List<CommandProcessorBase>();
    private List<PipelineProcessor> _redirectionPipes;
    private PipelineReader<object> externalInputPipe;
    private PipelineWriter externalSuccessOutput;
    private PipelineWriter externalErrorOutput;
    private bool executionStarted;
    private bool topLevel;
    private bool stopping;
    private SessionStateScope executionScope;
    private Exception firstTerminatingError;
    private bool _linkedSuccessOutput;
    private bool _linkedErrorOutput;
    internal SecurityContext SecurityContext = SecurityContext.Capture();
    private bool disposed;
    private bool executionFailed;
    private bool terminatingErrorLogged;
    private List<string> logBuffer = new List<string>();
    private object StopReasonLock = new object();
    internal InternalCommand _permittedToWrite;
    internal bool _permittedToWriteToPipeline;
    internal Thread _permittedToWriteThread;
    private LocalPipeline localPipeline;

    public void Dispose()
    {
      using (PipelineProcessor.tracer.TraceDispose((object) this))
      {
        this.Dispose(true);
        GC.SuppressFinalize((object) this);
      }
    }

    private void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      if (disposing)
        this.DisposeCommands();
      this.disposed = true;
    }

    ~PipelineProcessor()
    {
      PipelineProcessor.tracer.TraceFinalizer((object) this);
      this.Dispose(false);
    }

    internal bool ExecutionFailed
    {
      get => this.executionFailed;
      set => this.executionFailed = value;
    }

    internal void LogExecutionInfo(InvocationInfo invocationInfo, string text) => this.logBuffer.Add(ResourceManagerCache.FormatResourceString("Pipeline", "PipelineExecutionInformation", (object) this.GetCommand(invocationInfo), (object) text));

    internal void LogExecutionParameterBinding(
      InvocationInfo invocationInfo,
      string parameterName,
      string parameterValue)
    {
      this.logBuffer.Add(ResourceManagerCache.FormatResourceString("Pipeline", "PipelineExecutionParameterBinding", (object) this.GetCommand(invocationInfo), (object) parameterName, (object) parameterValue));
    }

    internal void LogExecutionError(InvocationInfo invocationInfo, ErrorRecord errorRecord)
    {
      if (errorRecord == null)
        return;
      this.logBuffer.Add(ResourceManagerCache.FormatResourceString("Pipeline", "PipelineExecutionNonTerminatingError", (object) this.GetCommand(invocationInfo), (object) errorRecord.ToString()));
    }

    internal void LogExecutionException(Exception exception)
    {
      this.executionFailed = true;
      if (this.terminatingErrorLogged)
        return;
      this.terminatingErrorLogged = true;
      if (exception == null || !this.NeedToLog())
        return;
      this.logBuffer.Add(ResourceManagerCache.FormatResourceString("Pipeline", "PipelineExecutionTerminatingError", (object) this.GetCommand(exception), (object) exception.Message));
    }

    private string GetCommand(InvocationInfo invocationInfo) => invocationInfo == null || invocationInfo.MyCommand == null ? "" : invocationInfo.MyCommand.Name;

    private string GetCommand(Exception exception) => exception is IContainsErrorRecord containsErrorRecord && containsErrorRecord.ErrorRecord != null ? this.GetCommand(containsErrorRecord.ErrorRecord.InvocationInfo) : "";

    internal void FlushLog()
    {
      if (this._commands == null || this._commands.Count <= 0 || this.logBuffer.Count == 0)
        return;
      MshLog.LogPipelineExecutionDetailEvent(this._commands[0].Command.Context, this.logBuffer, this._commands[0].Command.MyInvocation);
    }

    private bool NeedToLog()
    {
      if (this._commands == null)
        return false;
      foreach (CommandProcessorBase command in this._commands)
      {
        if (command.Command.commandRuntime is MshCommandRuntime commandRuntime && commandRuntime.LogPipelineExecutionDetail)
          return true;
      }
      return false;
    }

    internal int Add(CommandProcessorBase commandProcessor)
    {
      commandProcessor.CommandRuntime.PipelineProcessor = this;
      return this.AddCommand(commandProcessor, this._commands.Count, false);
    }

    internal void AddRedirectionPipe(PipelineProcessor pipelineProcessor)
    {
      if (pipelineProcessor == null)
        throw PipelineProcessor.tracer.NewArgumentNullException(nameof (pipelineProcessor));
      if (this._redirectionPipes == null)
        this._redirectionPipes = new List<PipelineProcessor>();
      this._redirectionPipes.Add(pipelineProcessor);
    }

    internal Array Execute() => this.Execute((Array) null);

    internal Array Execute(Array input) => this.SynchronousExecute(input, (Hashtable) null);

    internal int AddCommand(
      CommandProcessorBase commandProcessor,
      int readFromCommand,
      bool readErrorQueue)
    {
      if (commandProcessor == null)
        throw PipelineProcessor.tracer.NewArgumentNullException(nameof (commandProcessor));
      if (this._commands == null)
        throw PipelineProcessor.tracer.NewInvalidOperationException();
      if (this.disposed)
        throw PipelineProcessor.tracer.NewObjectDisposedException(nameof (PipelineProcessor));
      if (this.executionStarted)
        throw PipelineProcessor.tracer.NewInvalidOperationException("Pipeline", "ExecutionAlreadyStarted");
      if (commandProcessor.AddedToPipelineAlready)
        throw PipelineProcessor.tracer.NewInvalidOperationException("Pipeline", "CommandProcessorAlreadyUsed");
      if (this._commands.Count == 0)
      {
        if (readFromCommand != 0)
          throw PipelineProcessor.tracer.NewArgumentException(nameof (readFromCommand), "Pipeline", "FirstCommandCannotHaveInput");
        commandProcessor.AddedToPipelineAlready = true;
      }
      else
      {
        if (readFromCommand > this._commands.Count || readFromCommand <= 0)
          throw PipelineProcessor.tracer.NewArgumentException(nameof (readFromCommand), "Pipeline", "InvalidCommandNumber");
        CommandProcessorBase command1 = this._commands[readFromCommand - 1];
        if (command1 == null || command1.CommandRuntime == null)
          throw PipelineProcessor.tracer.NewInvalidOperationException();
        Pipe pipe = readErrorQueue ? command1.CommandRuntime.ErrorOutputPipe : command1.CommandRuntime.OutputPipe;
        if (pipe == null)
          throw PipelineProcessor.tracer.NewInvalidOperationException();
        if (pipe.DownstreamCmdlet != null)
          throw PipelineProcessor.tracer.NewInvalidOperationException("Pipeline", "PipeAlreadyTaken");
        commandProcessor.AddedToPipelineAlready = true;
        commandProcessor.CommandRuntime.InputPipe = pipe;
        pipe.DownstreamCmdlet = commandProcessor;
        if (commandProcessor.CommandRuntime.MergeUnclaimedPreviousErrorResults)
        {
          for (int index = 0; index < this._commands.Count; ++index)
          {
            CommandProcessorBase command2 = this._commands[index];
            if (command2 == null || command2.CommandRuntime == null)
              throw PipelineProcessor.tracer.NewInvalidOperationException();
            if (command2.CommandRuntime.ErrorOutputPipe.DownstreamCmdlet == null && command2.CommandRuntime.ErrorOutputPipe.ExternalWriter == null)
              command2.CommandRuntime.ErrorOutputPipe = pipe;
          }
        }
      }
      this._commands.Add(commandProcessor);
      commandProcessor.CommandRuntime.PipelineProcessor = this;
      return this._commands.Count;
    }

    internal Array SynchronousExecute(Array input, Hashtable errorResults) => input == null ? this.SynchronousExecuteEnumerate((object) AutomationNull.Value, errorResults, true) : this.SynchronousExecuteEnumerate((object) input, errorResults, true);

    internal Array SynchronousExecuteEnumerate(
      object input,
      Hashtable errorResults,
      bool enumerate)
    {
      if (this.Stopping)
        throw new PipelineStoppedException();
      Exception exception = (Exception) null;
      try
      {
        this.Start(input != AutomationNull.Value);
        CommandProcessorBase command1 = this._commands[0];
        if (this.ExternalInput != null)
          command1.CommandRuntime.InputPipe.ExternalReader = this.ExternalInput;
        this.Inject(input, enumerate);
        for (int index = 0; index < this._commands.Count; ++index)
        {
          CommandProcessorBase command2 = this._commands[index];
          if (command2 == null)
            throw PipelineProcessor.tracer.NewInvalidOperationException();
          command2.DoComplete();
          MshLog.LogCommandLifecycleEvent(command2.Command.Context, CommandState.Stopped, command2.Command.MyInvocation);
        }
        if (this.firstTerminatingError != null)
        {
          this.LogExecutionException(this.firstTerminatingError);
          throw this.firstTerminatingError;
        }
        return this.RetrieveResults(errorResults);
      }
      catch (RuntimeException ex)
      {
        PipelineProcessor.tracer.TraceException((Exception) ex);
        exception = this.firstTerminatingError == null ? (Exception) ex : this.firstTerminatingError;
        this.LogExecutionException(exception);
      }
      catch (InvalidComObjectException ex)
      {
        PipelineProcessor.tracer.TraceException((Exception) ex);
        if (this.firstTerminatingError != null)
        {
          exception = this.firstTerminatingError;
        }
        else
        {
          exception = (Exception) new RuntimeException(ResourceManagerCache.FormatResourceString("Parser", "InvalidComObjectException", (object) ex.Message), (Exception) ex);
          ((RuntimeException) exception).SetErrorId("InvalidComObjectException");
        }
        this.LogExecutionException(exception);
      }
      finally
      {
        this.DisposeCommands();
      }
      RuntimeException.LockStackTrace(exception);
      throw exception;
    }

    internal Array DoComplete()
    {
      if (this.Stopping)
        throw new PipelineStoppedException();
      if (!this.executionStarted)
        throw PipelineProcessor.tracer.NewInvalidOperationException("Pipeline", "PipelineNotStarted");
      Exception exception = (Exception) null;
      try
      {
        for (int index = 0; index < this._commands.Count; ++index)
        {
          CommandProcessorBase command = this._commands[index];
          command.DoComplete();
          MshLog.LogCommandLifecycleEvent(command.Command.Context, CommandState.Stopped, command.Command.MyInvocation);
        }
        if (this.firstTerminatingError != null)
        {
          this.LogExecutionException(this.firstTerminatingError);
          throw this.firstTerminatingError;
        }
        return this.RetrieveResults(new Hashtable());
      }
      catch (RuntimeException ex)
      {
        PipelineProcessor.tracer.TraceException((Exception) ex);
        exception = this.firstTerminatingError == null ? (Exception) ex : this.firstTerminatingError;
        this.LogExecutionException(exception);
      }
      catch (InvalidComObjectException ex)
      {
        PipelineProcessor.tracer.TraceException((Exception) ex);
        if (this.firstTerminatingError != null)
        {
          exception = this.firstTerminatingError;
        }
        else
        {
          exception = (Exception) new RuntimeException(ResourceManagerCache.FormatResourceString("Parser", "InvalidComObjectException", (object) ex.Message), (Exception) ex);
          ((RuntimeException) exception).SetErrorId("InvalidComObjectException");
        }
        this.LogExecutionException(exception);
      }
      finally
      {
        this.DisposeCommands();
      }
      RuntimeException.LockStackTrace(exception);
      throw exception;
    }

    internal void StartStepping(bool expectInput)
    {
      try
      {
        this.Start(expectInput);
        if (this.firstTerminatingError != null)
          throw this.firstTerminatingError;
      }
      catch (PipelineStoppedException ex)
      {
        PipelineProcessor.tracer.TraceException((Exception) ex);
        this.DisposeCommands();
        if (this.firstTerminatingError != null)
          throw this.firstTerminatingError;
        throw;
      }
    }

    internal Array Step(object input) => this.DoStepItems(input, (Hashtable) null, false);

    internal Array Step(object input, Hashtable errorResults) => errorResults != null ? this.DoStepItems(input, errorResults, false) : throw PipelineProcessor.tracer.NewArgumentNullException(nameof (errorResults));

    internal Array StepArray(Array input) => this.DoStepItems((object) input, (Hashtable) null, true);

    internal Array StepArray(Array input, Hashtable errorResults) => errorResults != null ? this.DoStepItems((object) input, errorResults, true) : throw PipelineProcessor.tracer.NewArgumentNullException(nameof (errorResults));

    internal void Stop()
    {
      if (!this.RecordFailure((Exception) new PipelineStoppedException(), (InternalCommand) null))
        return;
      List<CommandProcessorBase> commands = this._commands;
      if (commands == null)
        return;
      for (int index = 0; index < commands.Count; ++index)
      {
        CommandProcessorBase commandProcessorBase = commands[index];
        if (commandProcessorBase == null)
          throw PipelineProcessor.tracer.NewInvalidOperationException();
        try
        {
          commandProcessorBase.Command.DoStopProcessing();
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          PipelineProcessor.tracer.TraceException(ex);
        }
        finally
        {
          MshLog.LogCommandLifecycleEvent(commandProcessorBase.Context, CommandState.Terminated, commandProcessorBase.Command.MyInvocation);
        }
      }
    }

    private Array DoStepItems(object input, Hashtable errorResults, bool enumerate)
    {
      if (this.Stopping)
        throw new PipelineStoppedException();
      try
      {
        this.Start(true);
        this.Inject(input, enumerate);
        if (this.firstTerminatingError != null)
          throw this.firstTerminatingError;
        return this.RetrieveResults(errorResults);
      }
      catch (PipelineStoppedException ex)
      {
        PipelineProcessor.tracer.TraceException((Exception) ex);
        this.DisposeCommands();
        if (this.firstTerminatingError != null)
          throw this.firstTerminatingError;
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        PipelineProcessor.tracer.TraceException(ex);
        this.DisposeCommands();
        throw;
      }
    }

    private void Start(bool incomingStream)
    {
      if (this.disposed)
        throw PipelineProcessor.tracer.NewObjectDisposedException(nameof (PipelineProcessor));
      if (this.Stopping)
        throw new PipelineStoppedException();
      if (this.executionStarted)
        return;
      CommandProcessorBase commandProcessorBase = this._commands != null && this._commands.Count != 0 ? this._commands[0] : throw PipelineProcessor.tracer.NewInvalidOperationException("Pipeline", "PipelineExecuteRequiresAtLeastOneCommand");
      if (commandProcessorBase == null || commandProcessorBase.CommandRuntime == null)
        throw PipelineProcessor.tracer.NewInvalidOperationException("Pipeline", "PipelineExecuteRequiresAtLeastOneCommand");
      if (this.executionScope == null)
        this.executionScope = commandProcessorBase.Context.EngineSessionState.CurrentScope;
      CommandProcessorBase command1 = this._commands[this._commands.Count - 1];
      if (command1 == null || command1.CommandRuntime == null)
        throw PipelineProcessor.tracer.NewInvalidOperationException();
      if (this.ExternalSuccessOutput != null)
        command1.CommandRuntime.OutputPipe.ExternalWriter = this.ExternalSuccessOutput;
      this.SetExternalErrorOutput();
      if (this.ExternalInput == null && !incomingStream)
        commandProcessorBase.CommandRuntime.IsClosed = true;
      this.executionStarted = true;
      int[] numArray = new int[this._commands.Count + 1];
      for (int index = 0; index < this._commands.Count; ++index)
      {
        CommandProcessorBase command2 = this._commands[index];
        if (command2 == null)
          throw PipelineProcessor.tracer.NewInvalidOperationException();
        MshLog.LogCommandLifecycleEvent(command2.Context, CommandState.Started, command2.Command.MyInvocation);
        InvocationInfo myInvocation = command2.Command.MyInvocation;
        myInvocation.PipelinePosition = index + 1;
        myInvocation.PipelineLength = this._commands.Count;
        myInvocation.PipelineIterationInfo = numArray;
        command2.DoPrepare();
        myInvocation.ExpectingInput = command2.IsPipelineInputExpected();
      }
      this.SetupOutErrorVariable();
      for (int index = 0; index < this._commands.Count; ++index)
        this._commands[index].DoBegin();
    }

    private void SetExternalErrorOutput()
    {
      if (this.ExternalErrorOutput == null)
        return;
      for (int index = 0; index < this._commands.Count; ++index)
      {
        Pipe errorOutputPipe = this._commands[index].CommandRuntime.ErrorOutputPipe;
        if (!errorOutputPipe.IsRedirected)
          errorOutputPipe.ExternalWriter = this.ExternalErrorOutput;
      }
    }

    private void SetupOutErrorVariable()
    {
      for (int index = 0; index < this._commands.Count; ++index)
      {
        CommandProcessorBase command = this._commands[index];
        if (command == null || command.CommandRuntime == null)
          throw PipelineProcessor.tracer.NewInvalidOperationException();
        command.CommandRuntime.SetupOutVariable();
        command.CommandRuntime.SetupErrorVariable();
        command.CommandRuntime.SetupWarningVariable();
      }
    }

    private void Inject(object input, bool enumerate)
    {
      CommandProcessorBase command = this._commands[0];
      if (command == null || command.CommandRuntime == null)
        throw PipelineProcessor.tracer.NewInvalidOperationException("Pipeline", "PipelineExecuteRequiresAtLeastOneCommand");
      if (input != AutomationNull.Value)
      {
        if (enumerate)
        {
          IEnumerator enumerator = LanguagePrimitives.GetEnumerator(input);
          if (enumerator != null)
            command.CommandRuntime.InputPipe = new Pipe(enumerator);
          else
            command.CommandRuntime.InputPipe.Add(input);
        }
        else
          command.CommandRuntime.InputPipe.Add(input);
      }
      command.DoExecute();
    }

    private Array RetrieveResults(Hashtable errorResults)
    {
      if (!this._linkedErrorOutput)
      {
        for (int index = 0; index < this._commands.Count; ++index)
        {
          CommandProcessorBase command = this._commands[index];
          if (command == null || command.CommandRuntime == null)
            throw PipelineProcessor.tracer.NewInvalidOperationException();
          Pipe errorOutputPipe = command.CommandRuntime.ErrorOutputPipe;
          if (errorOutputPipe.DownstreamCmdlet == null && !errorOutputPipe.Empty)
          {
            errorResults?.Add((object) (index + 1), (object) errorOutputPipe.ToArray());
            errorOutputPipe.Clear();
          }
        }
      }
      if (this._linkedSuccessOutput)
        return (Array) MshCommandRuntime.StaticEmptyArray;
      CommandProcessorBase command1 = this._commands[this._commands.Count - 1];
      Array array = command1 != null && command1.CommandRuntime != null ? (Array) command1.CommandRuntime.GetResultsAsArray() : throw PipelineProcessor.tracer.NewInvalidOperationException();
      command1.CommandRuntime.OutputPipe.Clear();
      return array ?? (Array) MshCommandRuntime.StaticEmptyArray;
    }

    internal void LinkPipelineSuccessOutput(Pipe pipeToUse)
    {
      CommandProcessorBase command = this._commands[this._commands.Count - 1];
      if (command == null || command.CommandRuntime == null)
        throw PipelineProcessor.tracer.NewInvalidOperationException();
      command.CommandRuntime.OutputPipe = pipeToUse;
      this._linkedSuccessOutput = true;
    }

    internal void LinkPipelineErrorOutput(Pipe pipeToUse)
    {
      for (int index = 0; index < this._commands.Count; ++index)
      {
        CommandProcessorBase command = this._commands[index];
        if (command == null || command.CommandRuntime == null)
          throw PipelineProcessor.tracer.NewInvalidOperationException();
        Pipe errorOutputPipe = command.CommandRuntime.ErrorOutputPipe;
        if (command.CommandRuntime.ErrorOutputPipe.DownstreamCmdlet == null)
          command.CommandRuntime.ErrorOutputPipe = pipeToUse;
      }
      this._linkedErrorOutput = true;
    }

    private void DisposeCommands()
    {
      this.stopping = true;
      this.FlushLog();
      if (this._commands != null)
      {
        for (int index = 0; index < this._commands.Count; ++index)
        {
          CommandProcessorBase command = this._commands[index];
          if (command != null)
          {
            try
            {
              command.Dispose();
            }
            catch (Exception ex)
            {
              CommandProcessorBase.CheckForSevereException(ex);
              PipelineProcessor.tracer.TraceException(ex);
              InvocationInfo invocationInfo = (InvocationInfo) null;
              if (command.Command != null)
                invocationInfo = command.Command.MyInvocation;
              Exception exception;
              if (ex is ProviderInvocationException innerException)
              {
                exception = (Exception) new CmdletProviderInvocationException(innerException, invocationInfo);
              }
              else
              {
                exception = (Exception) new CmdletInvocationException(ex, invocationInfo);
                MshLog.LogCommandHealthEvent(command.Command.Context, exception, Severity.Warning);
              }
              this.RecordFailure(exception, command.Command);
            }
          }
        }
      }
      this._commands = (List<CommandProcessorBase>) null;
      if (this._redirectionPipes != null)
      {
        foreach (PipelineProcessor redirectionPipe in this._redirectionPipes)
        {
          try
          {
            redirectionPipe?.Dispose();
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
          }
        }
      }
      this._redirectionPipes = (List<PipelineProcessor>) null;
    }

    internal bool RecordFailure(Exception e, InternalCommand command)
    {
      bool flag = false;
      lock (this.StopReasonLock)
      {
        if (this.firstTerminatingError == null)
        {
          RuntimeException.LockStackTrace(e);
          this.firstTerminatingError = e;
        }
        else if (!(this.firstTerminatingError is PipelineStoppedException) && command != null && command.Context != null)
        {
          Exception innerException = e;
          while (true)
          {
            switch (innerException)
            {
              case TargetInvocationException _:
              case CmdletInvocationException _ when innerException.InnerException != null:
                innerException = innerException.InnerException;
                continue;
              case PipelineStoppedException _:
                goto label_8;
              default:
                goto label_7;
            }
          }
label_7:
          InvalidOperationException operationException = new InvalidOperationException(ResourceManagerCache.FormatResourceString("Pipeline", "SecondFailure", (object) this.firstTerminatingError.GetType().Name, (object) this.firstTerminatingError.StackTrace, (object) innerException.GetType().Name, (object) innerException.StackTrace), innerException);
          PipelineProcessor.tracer.TraceException((Exception) operationException, true);
          MshLog.LogCommandHealthEvent(command.Context, (Exception) operationException, Severity.Warning);
        }
label_8:
        flag = this.stopping;
        this.stopping = true;
      }
      return !flag;
    }

    internal PipelineReader<object> ExternalInput
    {
      get => this.externalInputPipe;
      set
      {
        if (this.executionStarted)
          throw PipelineProcessor.tracer.NewInvalidOperationException("Pipeline", "ExecutionAlreadyStarted");
        this.externalInputPipe = value;
      }
    }

    internal PipelineWriter ExternalSuccessOutput
    {
      get => this.externalSuccessOutput;
      set
      {
        if (this.executionStarted)
          throw PipelineProcessor.tracer.NewInvalidOperationException("Pipeline", "ExecutionAlreadyStarted");
        this.externalSuccessOutput = value;
      }
    }

    internal PipelineWriter ExternalErrorOutput
    {
      get => this.externalErrorOutput;
      set
      {
        if (this.executionStarted)
          throw PipelineProcessor.tracer.NewInvalidOperationException("Pipeline", "ExecutionAlreadyStarted");
        this.externalErrorOutput = value;
      }
    }

    internal bool ExecutionStarted => this.executionStarted;

    internal bool Stopping => this.localPipeline != null && this.localPipeline.IsStopping;

    internal LocalPipeline LocalPipeline
    {
      get => this.localPipeline;
      set => this.localPipeline = value;
    }

    internal bool TopLevel
    {
      get => this.topLevel;
      set => this.topLevel = value;
    }

    internal SessionStateScope ExecutionScope
    {
      get => this.executionScope;
      set => this.executionScope = value;
    }
  }
}
