// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MshCommandRuntime
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Internal.Host;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Threading;

namespace System.Management.Automation
{
  internal class MshCommandRuntime : ICommandRuntime
  {
    private ExecutionContext context;
    private SessionState state;
    internal InternalHost CBhost;
    private PSHost host;
    private Pipe inputPipe;
    private Pipe outputPipe;
    private Pipe errorOutputPipe;
    private bool isClosed;
    private IList outVarList;
    private string outVar;
    private PipelineProcessor pipelineProcessor;
    private CommandInfo commandInfo;
    private InternalCommand thisCommand;
    internal string CBResourcesBaseName = "CommandBaseStrings";
    [TraceSource("InternalCommand", "InternalCommand")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("InternalCommand", "InternalCommand");
    private InvocationInfo myInvocation;
    private static long _lastUsedSourceId;
    private long _sourceId;
    private bool shouldLogPipelineExecutionDetail;
    private bool mergeMyErrorOutputWithSuccess;
    private bool mergeMySuccessOutputWithError;
    private bool mergeUnclaimedPreviousErrorResults;
    internal static object[] StaticEmptyArray = new object[0];
    private string errorVariable = "";
    private ArrayList errorVarList;
    private string warningVariable = "";
    private ArrayList warningVarList;
    internal bool UseSecurityContextRun = true;
    private bool isConfirmPreferenceCached;
    private ConfirmImpact confirmPreference = ConfirmImpact.High;
    private bool isDebugPreferenceSet;
    private ActionPreference debugPreference;
    private bool isDebugPreferenceCached;
    private bool isVerbosePreferenceCached;
    private ActionPreference verbosePreference;
    private bool isWarningPreferenceSet;
    private bool isWarningPreferenceCached;
    private ActionPreference warningPreference = ActionPreference.Continue;
    private bool verboseFlag;
    private bool isVerboseFlagSet;
    private bool confirmFlag;
    private bool isConfirmFlagSet;
    private bool useTransactionFlag;
    private bool useTransactionFlagSet;
    private bool debugFlag;
    private bool isDebugFlagSet;
    private bool whatIfFlag;
    private bool isWhatIfFlagSet;
    private bool isWhatIfPreferenceCached;
    private bool isErrorActionSet;
    private ActionPreference errorAction = ActionPreference.Continue;
    private bool isErrorActionPreferenceCached;
    private ActionPreference progressPreference = ActionPreference.Continue;
    private bool isProgressPreferenceSet;
    private bool isProgressPreferenceCached;
    internal MshCommandRuntime.ContinueStatus lastShouldProcessContinueStatus;
    internal MshCommandRuntime.ContinueStatus lastErrorContinueStatus;
    internal MshCommandRuntime.ContinueStatus lastDebugContinueStatus;
    internal MshCommandRuntime.ContinueStatus lastVerboseContinueStatus;
    internal MshCommandRuntime.ContinueStatus lastWarningContinueStatus;
    internal MshCommandRuntime.ContinueStatus lastProgressContinueStatus;
    private bool isScriptCmdlet;

    internal ExecutionContext Context
    {
      get => this.context;
      set => this.context = value;
    }

    public PSHost Host => this.host;

    internal bool IsClosed
    {
      get => this.isClosed;
      set => this.isClosed = value;
    }

    internal bool IsPipelineInputExpected => !this.isClosed || this.inputPipe != null && !this.inputPipe.Empty;

    internal IList OutVarList
    {
      get => this.outVarList;
      set => this.outVarList = value;
    }

    internal PipelineProcessor PipelineProcessor
    {
      set => this.pipelineProcessor = value;
      get => this.pipelineProcessor;
    }

    internal MshCommandRuntime(
      ExecutionContext context,
      CommandInfo commandInfo,
      InternalCommand thisCommand)
    {
      this.context = context;
      this.host = (PSHost) context.EngineHostInterface;
      this.CBhost = context.EngineHostInterface;
      this.commandInfo = commandInfo;
      this.thisCommand = thisCommand;
      this.shouldLogPipelineExecutionDetail = this.InitShouldLogPipelineExecutionDetail();
      this.isScriptCmdlet = thisCommand is PSScriptCmdlet;
    }

    public override string ToString() => this.commandInfo != null ? this.commandInfo.ToString() : "<NullCommandInfo>";

    internal InvocationInfo MyInvocation
    {
      get
      {
        if (this.myInvocation == null)
          this.myInvocation = this.thisCommand.MyInvocation;
        return this.myInvocation;
      }
    }

    internal bool IsStopping => this.pipelineProcessor != null && this.pipelineProcessor.Stopping;

    internal bool IsScriptCmdlet => this.isScriptCmdlet;

    public void WriteObject(object sendToPipeline)
    {
      this.ThrowIfStopping();
      if (this.UseSecurityContextRun)
      {
        if (this.pipelineProcessor == null || this.pipelineProcessor.SecurityContext == null)
          throw MshCommandRuntime.tracer.NewInvalidOperationException("pipeline", "WriteNotPermitted");
        ContextCallback callback = new ContextCallback(this.DoWriteObject);
        SecurityContext.Run(this.pipelineProcessor.SecurityContext.CreateCopy(), callback, sendToPipeline);
      }
      else
        this.DoWriteObject(sendToPipeline);
    }

    private void DoWriteObject(object sendToPipeline)
    {
      this.ThrowIfWriteNotPermitted(true);
      this._WriteObjectSkipAllowCheck(sendToPipeline);
    }

    public void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
      if (!enumerateCollection)
      {
        this.WriteObject(sendToPipeline);
      }
      else
      {
        this.ThrowIfStopping();
        if (this.UseSecurityContextRun)
        {
          if (this.pipelineProcessor == null || this.pipelineProcessor.SecurityContext == null)
            throw MshCommandRuntime.tracer.NewInvalidOperationException("pipeline", "WriteNotPermitted");
          ContextCallback callback = new ContextCallback(this.DoWriteObjects);
          SecurityContext.Run(this.pipelineProcessor.SecurityContext.CreateCopy(), callback, sendToPipeline);
        }
        else
          this.DoWriteObjects(sendToPipeline);
      }
    }

    private void DoWriteObjects(object sendToPipeline)
    {
      this.ThrowIfWriteNotPermitted(true);
      this._WriteObjectsSkipAllowCheck(sendToPipeline);
    }

    public void WriteProgress(ProgressRecord progressRecord)
    {
      this.ThrowIfStopping();
      this.ThrowIfWriteNotPermitted(false);
      if (0L == this._sourceId)
        this._sourceId = Interlocked.Increment(ref MshCommandRuntime._lastUsedSourceId);
      this.WriteProgress(this._sourceId, progressRecord);
    }

    public void WriteProgress(long sourceId, ProgressRecord progressRecord)
    {
      if (progressRecord == null)
        throw MshCommandRuntime.tracer.NewArgumentNullException(nameof (progressRecord));
      if (this.Host == null || this.Host.UI == null)
      {
        MshCommandRuntime.tracer.TraceError("No host in CommandBase.WriteProgress()");
        throw MshCommandRuntime.tracer.NewInvalidOperationException();
      }
      InternalHostUserInterface ui = this.Host.UI as InternalHostUserInterface;
      ActionPreference progressPreference = this.ProgressPreference;
      if (this.WriteHelper_ShouldWrite(progressPreference, this.lastProgressContinueStatus))
        ui.WriteProgress(sourceId, progressRecord);
      this.lastProgressContinueStatus = this.WriteHelper((string) null, (string) null, progressPreference, this.lastProgressContinueStatus, "ProgressPreference");
    }

    public void WriteDebug(string text) => this.WriteDebug(new DebugRecord(text));

    internal void WriteDebug(DebugRecord record)
    {
      if (this.Host == null || this.Host.UI == null)
      {
        MshCommandRuntime.tracer.TraceError("No host in CommandBase.WriteDebug()");
        throw MshCommandRuntime.tracer.NewInvalidOperationException();
      }
      PSHostUserInterface ui = this.Host.UI;
      PSTraceSource tracer = PSTraceSource.GetTracer(this.MyInvocation.MyCommand.Name, this.MyInvocation.MyCommand.Name, false);
      ActionPreference debugPreference = this.DebugPreference;
      if (this.WriteHelper_ShouldWrite(debugPreference, this.lastDebugContinueStatus))
      {
        if (record.InvocationInfo == null)
          record.SetInvocationInfo(this.MyInvocation);
        this.CBhost.InternalUI.WriteDebugRecord(record);
        tracer.WriteLine(record.Message, new object[0]);
      }
      this.lastDebugContinueStatus = this.WriteHelper((string) null, (string) null, debugPreference, this.lastDebugContinueStatus, "DebugPreference");
    }

    public void WriteVerbose(string text) => this.WriteVerbose(new VerboseRecord(text));

    internal void WriteVerbose(VerboseRecord record)
    {
      if (this.Host == null || this.Host.UI == null)
      {
        MshCommandRuntime.tracer.TraceError("No host in CommandBase.WriteVerbose()");
        throw MshCommandRuntime.tracer.NewInvalidOperationException();
      }
      ActionPreference verbosePreference = this.VerbosePreference;
      if (this.WriteHelper_ShouldWrite(verbosePreference, this.lastVerboseContinueStatus))
      {
        if (record.InvocationInfo == null)
          record.SetInvocationInfo(this.MyInvocation);
        this.CBhost.InternalUI.WriteVerboseRecord(record);
      }
      this.lastVerboseContinueStatus = this.WriteHelper((string) null, (string) null, verbosePreference, this.lastVerboseContinueStatus, "VerbosePreference");
    }

    public void WriteWarning(string text) => this.WriteWarning(new WarningRecord(text));

    internal void WriteWarning(WarningRecord record)
    {
      if (this.Host == null || this.Host.UI == null)
      {
        MshCommandRuntime.tracer.TraceError("No host in CommandBase.WriteWarning()");
        throw MshCommandRuntime.tracer.NewInvalidOperationException();
      }
      ActionPreference warningPreference = this.WarningPreference;
      if (this.WriteHelper_ShouldWrite(warningPreference, this.lastWarningContinueStatus))
      {
        if (record.InvocationInfo == null)
          record.SetInvocationInfo(this.MyInvocation);
        this.CBhost.InternalUI.WriteWarningRecord(record);
      }
      this.AppendWarning((object) record);
      this.lastWarningContinueStatus = this.WriteHelper((string) null, (string) null, warningPreference, this.lastWarningContinueStatus, "WarningPreference");
    }

    public void WriteCommandDetail(string text)
    {
      if (!this.LogPipelineExecutionDetail)
        return;
      this.pipelineProcessor.LogExecutionInfo(this.thisCommand.MyInvocation, text);
    }

    internal bool LogPipelineExecutionDetail => this.shouldLogPipelineExecutionDetail;

    private bool InitShouldLogPipelineExecutionDetail() => this.commandInfo is CmdletInfo commandInfo && commandInfo.PSSnapIn != null && commandInfo.PSSnapIn.LogPipelineExecutionDetails;

    public bool MergeMyErrorOutputWithSuccess
    {
      get => this.mergeMyErrorOutputWithSuccess;
      set => this.mergeMyErrorOutputWithSuccess = value;
    }

    public bool MergeMySuccessOutputWithError
    {
      get => this.mergeMySuccessOutputWithError;
      set => this.mergeMySuccessOutputWithError = value;
    }

    internal bool MergeUnclaimedPreviousErrorResults
    {
      get => this.mergeUnclaimedPreviousErrorResults;
      set => this.mergeUnclaimedPreviousErrorResults = value;
    }

    internal string OutVariable
    {
      get => this.outVar;
      set => this.outVar = value;
    }

    internal void SetupOutVariable()
    {
      if (string.IsNullOrEmpty(this.OutVariable))
        return;
      if (this.context.LanguageMode != PSLanguageMode.FullLanguage)
        throw InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), (Token) null, "VariableReferenceNotSupportedInDataSection");
      if (this.state == null)
        this.state = new SessionState(this.context.EngineSessionState);
      string name = this.OutVariable;
      if (name.StartsWith("+", StringComparison.Ordinal))
      {
        name = name.Substring(1);
        object obj = PSObject.Base(this.state.PSVariable.GetValue(name));
        this.outVarList = obj as IList;
        if (this.outVarList == null)
        {
          this.outVarList = (IList) new ArrayList();
          if (obj != null)
            this.outVarList.Add(obj);
        }
        else if (this.outVarList.IsFixedSize)
        {
          ArrayList arrayList = new ArrayList();
          arrayList.AddRange((ICollection) this.outVarList);
          this.outVarList = (IList) arrayList;
        }
      }
      else
        this.outVarList = (IList) new ArrayList();
      this.state.PSVariable.Set(name, (object) this.outVarList);
    }

    private void AppendPipeObjectToOutVariable(object obj)
    {
      if (!this.isScriptCmdlet && this.outVarList != null)
        this.outVarList.Add(obj);
      this.OutputPipe.UpdateScriptCmdletVariable(ScriptCmdletVariable.Output, obj);
    }

    internal int OutBuffer
    {
      get => this.OutputPipe.OutBufferCount;
      set => this.OutputPipe.OutBufferCount = value;
    }

    public bool ShouldProcess(string target) => this.DoShouldProcess(ResourceManagerCache.FormatResourceString(this.CBResourcesBaseName, "ShouldProcessMessage", (object) this.MyInvocation.MyCommand.Name, (object) target), (string) null, (string) null, out ShouldProcessReason _);

    public bool ShouldProcess(string target, string action) => this.DoShouldProcess(ResourceManagerCache.FormatResourceString(this.CBResourcesBaseName, "ShouldProcessMessage", (object) action, (object) target, null), (string) null, (string) null, out ShouldProcessReason _);

    public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption) => this.DoShouldProcess(verboseDescription, verboseWarning, caption, out ShouldProcessReason _);

    public bool ShouldProcess(
      string verboseDescription,
      string verboseWarning,
      string caption,
      out ShouldProcessReason shouldProcessReason)
    {
      return this.DoShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
    }

    private bool DoShouldProcess(
      string verboseDescription,
      string verboseWarning,
      string caption,
      out ShouldProcessReason shouldProcessReason)
    {
      this.ThrowIfStopping();
      shouldProcessReason = ShouldProcessReason.None;
      switch (this.lastShouldProcessContinueStatus)
      {
        case MshCommandRuntime.ContinueStatus.YesToAll:
          return true;
        case MshCommandRuntime.ContinueStatus.NoToAll:
          return false;
        default:
          if ((bool) this.WhatIf)
          {
            this.ThrowIfWriteNotPermitted(false);
            shouldProcessReason = ShouldProcessReason.WhatIf;
            this.CBhost.UI.WriteLine(ResourceManagerCache.FormatResourceString(this.CBResourcesBaseName, "ShouldProcessWhatIfMessage", (object) verboseDescription));
            return false;
          }
          CommandMetadata commandMetadata = this.commandInfo.CommandMetadata;
          if (commandMetadata == null)
            return true;
          ConfirmImpact confirmImpact = commandMetadata.ConfirmImpact;
          ConfirmImpact confirmPreference = this.ConfirmPreference;
          if (confirmPreference == ConfirmImpact.None || confirmPreference > confirmImpact)
          {
            if (this.Verbose)
            {
              this.ThrowIfWriteNotPermitted(false);
              this.WriteVerbose(verboseDescription);
            }
            return true;
          }
          if (string.IsNullOrEmpty(verboseWarning))
            verboseWarning = ResourceManagerCache.FormatResourceString(this.CBResourcesBaseName, "ShouldProcessWarningFallback", (object) verboseDescription);
          this.ThrowIfWriteNotPermitted(false);
          this.lastShouldProcessContinueStatus = this.InquireHelper(verboseWarning, caption, true, true, false);
          switch (this.lastShouldProcessContinueStatus)
          {
            case MshCommandRuntime.ContinueStatus.No:
            case MshCommandRuntime.ContinueStatus.NoToAll:
              return false;
            default:
              return true;
          }
      }
    }

    public bool ShouldContinue(string query, string caption)
    {
      bool yesToAll = false;
      bool noToAll = false;
      return this.DoShouldContinue(query, caption, false, ref yesToAll, ref noToAll);
    }

    public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll) => this.DoShouldContinue(query, caption, true, ref yesToAll, ref noToAll);

    private bool DoShouldContinue(
      string query,
      string caption,
      bool supportsToAllOptions,
      ref bool yesToAll,
      ref bool noToAll)
    {
      this.ThrowIfStopping();
      this.ThrowIfWriteNotPermitted(false);
      if (noToAll)
        return false;
      if (yesToAll)
        return true;
      switch (this.InquireHelper(query, caption, supportsToAllOptions, supportsToAllOptions, false))
      {
        case MshCommandRuntime.ContinueStatus.No:
          return false;
        case MshCommandRuntime.ContinueStatus.YesToAll:
          yesToAll = true;
          break;
        case MshCommandRuntime.ContinueStatus.NoToAll:
          noToAll = true;
          return false;
      }
      return true;
    }

    public bool TransactionAvailable() => this.UseTransactionFlagSet && this.Context.TransactionManager.HasTransaction;

    public PSTransactionContext CurrentPSTransaction
    {
      get
      {
        if (!this.TransactionAvailable())
          throw new InvalidOperationException(this.UseTransactionFlagSet ? ResourceManagerCache.GetResourceString("TransactionStrings", "NoTransactionAvailable") : ResourceManagerCache.GetResourceString("TransactionStrings", "CmdletRequiresUseTx"));
        return new PSTransactionContext(this.Context.TransactionManager);
      }
    }

    public void ThrowTerminatingError(ErrorRecord errorRecord)
    {
      this.ThrowIfStopping();
      if (errorRecord == null)
        throw MshCommandRuntime.tracer.NewArgumentNullException(nameof (errorRecord));
      errorRecord.SetInvocationInfo(this.MyInvocation);
      if (errorRecord.ErrorDetails != null && errorRecord.ErrorDetails.TextLookupError != null)
      {
        Exception textLookupError = errorRecord.ErrorDetails.TextLookupError;
        errorRecord.ErrorDetails.TextLookupError = (Exception) null;
        MshLog.LogCommandHealthEvent(this.context, textLookupError, Severity.Warning);
      }
      if (errorRecord.Exception != null)
      {
        if (string.IsNullOrEmpty(errorRecord.Exception.StackTrace))
        {
          try
          {
            throw errorRecord.Exception;
          }
          catch (Exception ex)
          {
          }
        }
      }
      throw this.ManageException((Exception) new CmdletInvocationException(errorRecord));
    }

    internal Pipe InputPipe
    {
      get
      {
        if (this.inputPipe == null)
          this.inputPipe = new Pipe();
        return this.inputPipe;
      }
      set => this.inputPipe = value;
    }

    internal Pipe OutputPipe
    {
      get
      {
        if (this.outputPipe == null)
          this.outputPipe = new Pipe();
        return this.outputPipe;
      }
      set => this.outputPipe = value;
    }

    internal object[] GetResultsAsArray() => this.outputPipe == null ? MshCommandRuntime.StaticEmptyArray : this.outputPipe.ToArray();

    internal Pipe ErrorOutputPipe
    {
      get
      {
        if (this.errorOutputPipe == null)
          this.errorOutputPipe = new Pipe();
        return this.errorOutputPipe;
      }
      set => this.errorOutputPipe = value;
    }

    internal void ClearOutputAndErrorPipes()
    {
      if (this.errorOutputPipe != null)
        this.errorOutputPipe.Clear();
      if (this.outputPipe == null)
        return;
      this.outputPipe.Clear();
    }

    internal void ThrowIfStopping()
    {
      if (this.IsStopping)
        throw new PipelineStoppedException();
    }

    internal void ThrowIfWriteNotPermitted(bool needsToWriteToPipeline)
    {
      if (this.pipelineProcessor == null || this.thisCommand != this.pipelineProcessor._permittedToWrite || needsToWriteToPipeline && !this.pipelineProcessor._permittedToWriteToPipeline || Thread.CurrentThread != this.pipelineProcessor._permittedToWriteThread)
        throw MshCommandRuntime.tracer.NewInvalidOperationException("Pipeline", "WriteNotPermitted");
    }

    internal IDisposable AllowThisCommandToWrite(bool permittedToWriteToPipeline) => (IDisposable) new MshCommandRuntime.AllowWrite(this.thisCommand, permittedToWriteToPipeline);

    public Exception ManageException(Exception e)
    {
      if (e == null)
        throw MshCommandRuntime.tracer.NewArgumentNullException(nameof (e));
      if (this.pipelineProcessor != null)
        this.pipelineProcessor.RecordFailure(e, this.thisCommand);
      switch (e)
      {
        case HaltCommandException _:
        case PipelineStoppedException _:
        case ExitNestedPromptException _:
label_6:
          return (Exception) new PipelineStoppedException();
        default:
          this.AppendError((object) e);
          MshLog.LogCommandHealthEvent(this.context, e, Severity.Warning);
          goto label_6;
      }
    }

    internal string ErrorVariable
    {
      get => this.errorVariable;
      set => this.errorVariable = value;
    }

    internal void SetupErrorVariable()
    {
      if (string.IsNullOrEmpty(this.ErrorVariable))
        return;
      if (this.context.LanguageMode != PSLanguageMode.FullLanguage)
        throw InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), (Token) null, "VariableReferenceNotSupportedInDataSection");
      if (this.state == null)
        this.state = new SessionState(this.context.EngineSessionState);
      string name = this.ErrorVariable;
      if (name.StartsWith("+", StringComparison.Ordinal))
      {
        name = name.Substring(1);
        object obj1 = PSObject.Base(this.state.PSVariable.GetValue(name));
        this.errorVarList = obj1 as ArrayList;
        if (this.errorVarList == null)
        {
          this.errorVarList = new ArrayList();
          if (obj1 != null && AutomationNull.Value != obj1)
          {
            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(obj1);
            if (enumerable != null)
            {
              foreach (object obj2 in enumerable)
                this.errorVarList.Add(obj2);
            }
            else
              this.errorVarList.Add(obj1);
          }
        }
      }
      else
        this.errorVarList = new ArrayList();
      this.state.PSVariable.Set(name, (object) this.errorVarList);
    }

    internal void AppendError(object obj) => this.AppendError(obj, false);

    internal void AppendError(object obj, bool goingToPipeline)
    {
      if (obj == null)
        return;
      this.AppendDollarError(obj);
      if (this.errorVarList != null && (!goingToPipeline || !this.isScriptCmdlet))
        this.errorVarList.Add(obj);
      this.OutputPipe.UpdateScriptCmdletVariable(ScriptCmdletVariable.Error, obj);
    }

    private void AppendDollarError(object obj)
    {
      if (obj is Exception && (this.pipelineProcessor == null || !this.pipelineProcessor.TopLevel))
        return;
      this.context.AppendDollarError(obj);
    }

    internal string WarningVariable
    {
      get => this.warningVariable;
      set => this.warningVariable = value;
    }

    internal void SetupWarningVariable()
    {
      if (string.IsNullOrEmpty(this.WarningVariable))
        return;
      if (this.context.LanguageMode != PSLanguageMode.FullLanguage)
        throw InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), (Token) null, "VariableReferenceNotSupportedInDataSection");
      if (this.state == null)
        this.state = new SessionState(this.context.EngineSessionState);
      string name = this.WarningVariable;
      if (name.StartsWith("+", StringComparison.Ordinal))
      {
        name = name.Substring(1);
        object obj1 = PSObject.Base(this.state.PSVariable.GetValue(name));
        this.warningVarList = obj1 as ArrayList;
        if (this.warningVarList == null)
        {
          this.warningVarList = new ArrayList();
          if (obj1 != null && AutomationNull.Value != obj1)
          {
            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(obj1);
            if (enumerable != null)
            {
              foreach (object obj2 in enumerable)
                this.warningVarList.Add(obj2);
            }
            else
              this.warningVarList.Add(obj1);
          }
        }
      }
      else
        this.warningVarList = new ArrayList();
      this.state.PSVariable.Set(name, (object) this.warningVarList);
    }

    internal void AppendWarning(object obj)
    {
      if (obj == null || this.warningVarList == null)
        return;
      this.warningVarList.Add(obj);
    }

    internal void _WriteObjectSkipAllowCheck(object sendToPipeline)
    {
      this.ThrowIfStopping();
      if (AutomationNull.Value == sendToPipeline)
        return;
      sendToPipeline = (object) LanguagePrimitives.AsPSObjectOrNull(sendToPipeline);
      this.AppendPipeObjectToOutVariable(sendToPipeline);
      if (this.MergeMySuccessOutputWithError)
      {
        sendToPipeline = (object) ErrorRecord.MakeRedirectedException(sendToPipeline);
        this.ErrorOutputPipe.Add(sendToPipeline);
      }
      else
        this.OutputPipe.Add(sendToPipeline);
    }

    internal void _WriteObjectsSkipAllowCheck(object sendToPipeline)
    {
      IEnumerable enumerable = LanguagePrimitives.GetEnumerable(sendToPipeline);
      if (enumerable == null)
      {
        this._WriteObjectSkipAllowCheck(sendToPipeline);
      }
      else
      {
        this.ThrowIfStopping();
        ArrayList arrayList = new ArrayList();
        foreach (object obj1 in enumerable)
        {
          if (AutomationNull.Value != obj1)
          {
            object obj2 = (object) LanguagePrimitives.AsPSObjectOrNull(obj1);
            arrayList.Add(obj2);
            this.AppendPipeObjectToOutVariable(obj2);
          }
        }
        if (this.MergeMySuccessOutputWithError)
          this.ErrorOutputPipe.AddItemsWithRedirect((object) arrayList, true);
        else
          this.OutputPipe.AddItems((object) arrayList);
      }
    }

    public void WriteError(ErrorRecord errorRecord)
    {
      this.ThrowIfStopping();
      if (this.UseSecurityContextRun)
      {
        if (this.pipelineProcessor == null || this.pipelineProcessor.SecurityContext == null)
          throw MshCommandRuntime.tracer.NewInvalidOperationException("pipeline", "WriteNotPermitted");
        ContextCallback callback = new ContextCallback(this.DoWriteError);
        SecurityContext.Run(this.pipelineProcessor.SecurityContext.CreateCopy(), callback, (object) errorRecord);
      }
      else
        this.DoWriteError((object) errorRecord);
    }

    private void DoWriteError(object obj)
    {
      if (!(obj is ErrorRecord errorRecord))
        throw MshCommandRuntime.tracer.NewArgumentNullException("errorRecord");
      if ((bool) this.UseTransaction && this.context.TransactionManager.RollbackPreference != RollbackSeverity.TerminatingError && this.context.TransactionManager.RollbackPreference != RollbackSeverity.Never)
        this.context.TransactionManager.Rollback(true);
      if (errorRecord.PreserveInvocationInfoOnce)
        errorRecord.PreserveInvocationInfoOnce = false;
      else
        errorRecord.SetInvocationInfo(this.MyInvocation);
      this.ThrowIfWriteNotPermitted(true);
      this._WriteErrorSkipAllowCheck(errorRecord);
    }

    internal void _WriteErrorSkipAllowCheck(ErrorRecord errorRecord)
    {
      this.ThrowIfStopping();
      if (errorRecord.ErrorDetails != null && errorRecord.ErrorDetails.TextLookupError != null)
      {
        Exception textLookupError = errorRecord.ErrorDetails.TextLookupError;
        errorRecord.ErrorDetails.TextLookupError = (Exception) null;
        MshLog.LogCommandHealthEvent(this.context, textLookupError, Severity.Warning);
      }
      this.pipelineProcessor.ExecutionFailed = true;
      if (this.shouldLogPipelineExecutionDetail)
        this.pipelineProcessor.LogExecutionError(this.thisCommand.MyInvocation, errorRecord);
      ActionPreference actionPreference = this.ErrorAction;
      if (actionPreference == ActionPreference.SilentlyContinue)
      {
        this.AppendError((object) errorRecord);
      }
      else
      {
        if (MshCommandRuntime.ContinueStatus.YesToAll == this.lastErrorContinueStatus)
          actionPreference = ActionPreference.Continue;
        switch (actionPreference)
        {
          case ActionPreference.Stop:
            ActionPreferenceStopException preferenceStopException = new ActionPreferenceStopException(this.MyInvocation, errorRecord, this.CBResourcesBaseName, "ErrorPreferenceStop", new object[2]
            {
              (object) "ErrorActionPreference",
              (object) errorRecord.ToString()
            });
            MshCommandRuntime.tracer.TraceException((Exception) preferenceStopException);
            throw this.ManageException((Exception) preferenceStopException);
          case ActionPreference.Inquire:
            this.lastErrorContinueStatus = this.InquireHelper(RuntimeException.RetrieveMessage(errorRecord), (string) null, true, false, true);
            break;
        }
        this.AppendError((object) errorRecord, true);
        PSObject psObject = PSObject.AsPSObject((object) errorRecord);
        PSNoteProperty psNoteProperty = new PSNoteProperty("writeErrorStream", (object) true);
        psObject.Properties.Add((PSPropertyInfo) psNoteProperty);
        if (this.mergeMyErrorOutputWithSuccess)
          this.OutputPipe.Add((object) psObject);
        else
          this.ErrorOutputPipe.Add((object) psObject);
      }
    }

    internal ConfirmImpact ConfirmPreference
    {
      get
      {
        if ((bool) this.Confirm)
          return ConfirmImpact.Low;
        if (this.Debug)
          return this.isConfirmFlagSet ? ConfirmImpact.None : ConfirmImpact.Low;
        if (this.isConfirmFlagSet)
          return ConfirmImpact.None;
        if (!this.isConfirmPreferenceCached)
        {
          bool defaultUsed = false;
          this.confirmPreference = this.Context.GetEnumPreference<ConfirmImpact>(InitialSessionState.confirmPreferenceVariablePath, this.confirmPreference, out defaultUsed);
          this.isConfirmPreferenceCached = true;
        }
        return this.confirmPreference;
      }
    }

    internal ActionPreference DebugPreference
    {
      get
      {
        if (this.isDebugPreferenceSet)
          return this.debugPreference;
        if (this.isDebugFlagSet)
          return this.Debug ? ActionPreference.Inquire : ActionPreference.SilentlyContinue;
        if (!this.isDebugPreferenceCached)
        {
          bool defaultUsed = false;
          this.debugPreference = this.context.GetEnumPreference<ActionPreference>(InitialSessionState.debugPreferenceVariablePath, this.debugPreference, out defaultUsed);
          this.isDebugPreferenceCached = true;
        }
        return this.debugPreference;
      }
      set
      {
        this.debugPreference = value;
        this.isDebugPreferenceSet = true;
      }
    }

    internal ActionPreference VerbosePreference
    {
      get
      {
        if (this.isVerboseFlagSet)
          return this.Verbose ? ActionPreference.Continue : ActionPreference.SilentlyContinue;
        if (this.Debug)
          return ActionPreference.Inquire;
        if (!this.isVerbosePreferenceCached)
        {
          bool defaultUsed = false;
          this.verbosePreference = this.context.GetEnumPreference<ActionPreference>(InitialSessionState.verbosePreferenceVariablePath, this.verbosePreference, out defaultUsed);
        }
        return this.verbosePreference;
      }
    }

    internal ActionPreference WarningPreference
    {
      get
      {
        if (this.isWarningPreferenceSet)
          return this.warningPreference;
        if (this.Debug)
          return ActionPreference.Inquire;
        if (this.Verbose)
          return ActionPreference.Continue;
        if (!this.isWarningPreferenceCached)
        {
          bool defaultUsed = false;
          this.warningPreference = this.context.GetEnumPreference<ActionPreference>(InitialSessionState.warningPreferenceVariablePath, this.warningPreference, out defaultUsed);
        }
        return this.warningPreference;
      }
      set
      {
        this.warningPreference = value;
        this.isWarningPreferenceSet = true;
      }
    }

    internal bool Verbose
    {
      get => this.verboseFlag;
      set
      {
        this.verboseFlag = value;
        this.isVerboseFlagSet = true;
      }
    }

    internal bool IsVerboseFlagSet => this.isVerboseFlagSet;

    internal SwitchParameter Confirm
    {
      get => (SwitchParameter) this.confirmFlag;
      set
      {
        this.confirmFlag = (bool) value;
        this.isConfirmFlagSet = true;
      }
    }

    internal bool IsConfirmFlagSet => this.isConfirmFlagSet;

    internal SwitchParameter UseTransaction
    {
      get => (SwitchParameter) this.useTransactionFlag;
      set
      {
        this.useTransactionFlag = (bool) value;
        this.useTransactionFlagSet = true;
      }
    }

    internal bool UseTransactionFlagSet => this.useTransactionFlagSet;

    internal bool Debug
    {
      get => this.debugFlag;
      set
      {
        this.debugFlag = value;
        this.isDebugFlagSet = true;
      }
    }

    internal bool IsDebugFlagSet => this.isDebugFlagSet;

    internal SwitchParameter WhatIf
    {
      get
      {
        if (!this.isWhatIfFlagSet && !this.isWhatIfPreferenceCached)
        {
          bool defaultUsed = false;
          this.whatIfFlag = this.context.GetBooleanPreference(InitialSessionState.whatIfPreferenceVariablePath, this.whatIfFlag, out defaultUsed);
          this.isWhatIfPreferenceCached = true;
        }
        return (SwitchParameter) this.whatIfFlag;
      }
      set
      {
        this.whatIfFlag = (bool) value;
        this.isWhatIfFlagSet = true;
      }
    }

    internal bool IsWhatIfFlagSet => this.isWhatIfFlagSet;

    internal ActionPreference ErrorAction
    {
      get
      {
        if (this.isErrorActionSet)
          return this.errorAction;
        if (this.Debug)
          return ActionPreference.Inquire;
        if (this.Verbose)
          return ActionPreference.Continue;
        if (!this.isErrorActionPreferenceCached)
        {
          bool defaultUsed = false;
          this.errorAction = this.context.GetEnumPreference<ActionPreference>(InitialSessionState.errorActionPreferenceVariablePath, this.errorAction, out defaultUsed);
          this.isErrorActionPreferenceCached = true;
        }
        return this.errorAction;
      }
      set
      {
        this.errorAction = value;
        this.isErrorActionSet = true;
      }
    }

    internal bool IsErrorActionSet => this.isErrorActionSet;

    internal ActionPreference ProgressPreference
    {
      get
      {
        if (this.isProgressPreferenceSet || this.isProgressPreferenceCached)
          return this.progressPreference;
        bool defaultUsed = false;
        this.progressPreference = this.context.GetEnumPreference<ActionPreference>(InitialSessionState.progressPreferenceVariablePath, this.progressPreference, out defaultUsed);
        this.isProgressPreferenceCached = true;
        return this.progressPreference;
      }
      set
      {
        this.progressPreference = value;
        this.isProgressPreferenceSet = true;
      }
    }

    internal bool WriteHelper_ShouldWrite(
      ActionPreference preference,
      MshCommandRuntime.ContinueStatus lastContinueStatus)
    {
      this.ThrowIfStopping();
      this.ThrowIfWriteNotPermitted(false);
      switch (lastContinueStatus)
      {
        case MshCommandRuntime.ContinueStatus.YesToAll:
          return true;
        case MshCommandRuntime.ContinueStatus.NoToAll:
          return false;
        default:
          switch (preference)
          {
            case ActionPreference.SilentlyContinue:
              return false;
            case ActionPreference.Stop:
            case ActionPreference.Continue:
            case ActionPreference.Inquire:
              return true;
            default:
              return true;
          }
      }
    }

    internal MshCommandRuntime.ContinueStatus WriteHelper(
      string inquireCaption,
      string inquireMessage,
      ActionPreference preference,
      MshCommandRuntime.ContinueStatus lastContinueStatus,
      string preferenceVariableName)
    {
      switch (lastContinueStatus)
      {
        case MshCommandRuntime.ContinueStatus.YesToAll:
          return MshCommandRuntime.ContinueStatus.YesToAll;
        case MshCommandRuntime.ContinueStatus.NoToAll:
          return MshCommandRuntime.ContinueStatus.NoToAll;
        default:
          switch (preference)
          {
            case ActionPreference.SilentlyContinue:
            case ActionPreference.Continue:
              return MshCommandRuntime.ContinueStatus.Yes;
            case ActionPreference.Stop:
              ActionPreferenceStopException preferenceStopException1 = new ActionPreferenceStopException(this.MyInvocation, this.CBResourcesBaseName, "PreferenceStop", new object[1]
              {
                (object) preferenceVariableName
              });
              MshCommandRuntime.tracer.TraceException((Exception) preferenceStopException1);
              throw this.ManageException((Exception) preferenceStopException1);
            case ActionPreference.Inquire:
              return this.InquireHelper(inquireMessage, inquireCaption, true, false, true);
            default:
              ActionPreferenceStopException preferenceStopException2 = new ActionPreferenceStopException(this.MyInvocation, this.CBResourcesBaseName, "PreferenceInvalid", new object[2]
              {
                (object) preferenceVariableName,
                (object) preference
              });
              MshCommandRuntime.tracer.TraceException((Exception) preferenceStopException2);
              throw this.ManageException((Exception) preferenceStopException2);
          }
      }
    }

    internal MshCommandRuntime.ContinueStatus InquireHelper(
      string inquireMessage,
      string inquireCaption,
      bool allowYesToAll,
      bool allowNoToAll,
      bool replaceNoWithHalt)
    {
      Collection<ChoiceDescription> choices = new Collection<ChoiceDescription>();
      int num1 = 0;
      int num2 = int.MaxValue;
      int num3 = int.MaxValue;
      int num4 = int.MaxValue;
      int num5 = int.MaxValue;
      int num6 = int.MaxValue;
      string resourceString1 = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "ContinueOneLabel");
      string resourceString2 = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "ContinueOneHelpMessage");
      choices.Add(new ChoiceDescription(resourceString1, resourceString2));
      int num7 = num1;
      int num8 = num7 + 1;
      int num9 = num7;
      if (allowYesToAll)
      {
        string resourceString3 = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "ContinueAllLabel");
        string resourceString4 = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "ContinueAllHelpMessage");
        choices.Add(new ChoiceDescription(resourceString3, resourceString4));
        num2 = num8++;
      }
      int num10;
      if (replaceNoWithHalt)
      {
        string resourceString3 = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "HaltLabel");
        string resourceString4 = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "HaltHelpMessage");
        choices.Add(new ChoiceDescription(resourceString3, resourceString4));
        int num11 = num8;
        num10 = num11 + 1;
        num3 = num11;
      }
      else
      {
        string resourceString3 = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "SkipOneLabel");
        string resourceString4 = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "SkipOneHelpMessage");
        choices.Add(new ChoiceDescription(resourceString3, resourceString4));
        int num11 = num8;
        num10 = num11 + 1;
        num4 = num11;
      }
      if (allowNoToAll)
      {
        string resourceString3 = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "SkipAllLabel");
        string resourceString4 = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "SkipAllHelpMessage");
        choices.Add(new ChoiceDescription(resourceString3, resourceString4));
        num5 = num10++;
      }
      if (!this.IsUsingRemoteHost())
      {
        string resourceString3 = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "PauseLabel");
        string helpMessage = ResourceManagerCache.FormatResourceString(this.CBResourcesBaseName, "PauseHelpMessage", (object) "exit");
        choices.Add(new ChoiceDescription(resourceString3, helpMessage));
        int num11 = num10;
        int num12 = num11 + 1;
        num6 = num11;
      }
      if (string.IsNullOrEmpty(inquireMessage))
        inquireMessage = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "ShouldContinuePromptCaption");
      if (string.IsNullOrEmpty(inquireCaption))
        inquireCaption = ResourceManagerCache.GetResourceString(this.CBResourcesBaseName, "InquireCaptionDefault");
      int num13;
      while (true)
      {
        num13 = this.CBhost.UI.PromptForChoice(inquireCaption, inquireMessage, choices, 0);
        if (num9 != num13)
        {
          if (num2 != num13)
          {
            if (num3 != num13)
            {
              if (num4 != num13)
              {
                if (num5 != num13)
                {
                  if (num6 == num13)
                    this.CBhost.EnterNestedPrompt(this.thisCommand);
                  else
                    goto label_25;
                }
                else
                  goto label_22;
              }
              else
                goto label_20;
            }
            else
              goto label_18;
          }
          else
            goto label_16;
        }
        else
          break;
      }
      return MshCommandRuntime.ContinueStatus.Yes;
label_16:
      return MshCommandRuntime.ContinueStatus.YesToAll;
label_18:
      ActionPreferenceStopException preferenceStopException1 = new ActionPreferenceStopException(this.MyInvocation, this.CBResourcesBaseName, "InquireHalt", new object[0]);
      MshCommandRuntime.tracer.TraceException((Exception) preferenceStopException1);
      throw this.ManageException((Exception) preferenceStopException1);
label_20:
      return MshCommandRuntime.ContinueStatus.No;
label_22:
      return MshCommandRuntime.ContinueStatus.NoToAll;
label_25:
      if (-1 == num13)
      {
        ActionPreferenceStopException preferenceStopException2 = new ActionPreferenceStopException(this.MyInvocation, this.CBResourcesBaseName, "InquireCtrlC", new object[0]);
        MshCommandRuntime.tracer.TraceException((Exception) preferenceStopException2);
        throw this.ManageException((Exception) preferenceStopException2);
      }
      InvalidOperationException operationException = (InvalidOperationException) MshCommandRuntime.tracer.NewInvalidOperationException();
      MshCommandRuntime.tracer.TraceException((Exception) operationException);
      throw this.ManageException((Exception) operationException);
    }

    private bool IsUsingRemoteHost() => this.CBhost.ExternalHost is ServerRemoteHost;

    internal void SetPipeWriteEventHandler()
    {
      if (this.outVarList == null && this.errorVarList == null || this.outputPipe == null)
        return;
      this.outputPipe.ScriptCmdletVariableUpdate += new EventHandler<ScriptCmdletVariableUpdateEventArgs>(this.OnUpdateScriptCmdletVariable);
    }

    internal void ResetPipeWriteEventHandler()
    {
      if (this.outVarList == null && this.errorVarList == null || this.outputPipe == null)
        return;
      this.outputPipe.ScriptCmdletVariableUpdate -= new EventHandler<ScriptCmdletVariableUpdateEventArgs>(this.OnUpdateScriptCmdletVariable);
    }

    private void OnUpdateScriptCmdletVariable(object sender, ScriptCmdletVariableUpdateEventArgs e)
    {
      switch (e.Variable)
      {
        case ScriptCmdletVariable.Output:
          if (this.outVarList == null)
            break;
          this.outVarList.Add(e.Value);
          break;
        case ScriptCmdletVariable.Error:
          if (this.errorVarList == null)
            break;
          if (e.Value != null && e.Value is PSObject)
          {
            this.errorVarList.Add(((PSObject) e.Value).BaseObject);
            break;
          }
          this.errorVarList.Add(e.Value);
          break;
      }
    }

    private class AllowWrite : IDisposable
    {
      private PipelineProcessor _pp;
      private InternalCommand _wasPermittedToWrite;
      private bool _wasPermittedToWriteToPipeline;
      private Thread _wasPermittedToWriteThread;

      internal AllowWrite(InternalCommand permittedToWrite, bool permittedToWriteToPipeline)
      {
        if (permittedToWrite == null)
          throw MshCommandRuntime.tracer.NewArgumentNullException(nameof (permittedToWrite));
        if (!(permittedToWrite.commandRuntime is MshCommandRuntime commandRuntime))
          throw MshCommandRuntime.tracer.NewArgumentNullException("permittedToWrite.CommandRuntime");
        this._pp = commandRuntime.PipelineProcessor;
        this._wasPermittedToWrite = this._pp != null ? this._pp._permittedToWrite : throw MshCommandRuntime.tracer.NewArgumentNullException("permittedToWrite.CommandRuntime.PipelineProcessor");
        this._wasPermittedToWriteToPipeline = this._pp._permittedToWriteToPipeline;
        this._wasPermittedToWriteThread = this._pp._permittedToWriteThread;
        this._pp._permittedToWrite = permittedToWrite;
        this._pp._permittedToWriteToPipeline = permittedToWriteToPipeline;
        this._pp._permittedToWriteThread = Thread.CurrentThread;
      }

      public void Dispose()
      {
        this._pp._permittedToWrite = this._wasPermittedToWrite;
        this._pp._permittedToWriteToPipeline = this._wasPermittedToWriteToPipeline;
        this._pp._permittedToWriteThread = this._wasPermittedToWriteThread;
        GC.SuppressFinalize((object) this);
      }
    }

    internal enum ContinueStatus
    {
      Yes,
      No,
      YesToAll,
      NoToAll,
    }
  }
}
