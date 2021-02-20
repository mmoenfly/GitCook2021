// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSScriptCmdlet
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class PSScriptCmdlet : PSCmdlet, IDisposable, IDynamicParameters
  {
    private ScriptBlock _scriptblock;
    private bool _useLocalScope;
    private bool _fromScriptFile;
    private object _oldInput;
    private object _oldArgs;
    private object _oldUnderbar;
    private object _oldCmdlet;
    private ActionPreference _oldDebugPreference;
    private ActionPreference _oldVerbosePreference;
    private ActionPreference _oldErrorActionPreference;
    private object _oldWhatIfPreference;
    private ConfirmImpact _oldConfirmPreference;
    private ArrayList _input = new ArrayList();
    private bool _exitWasCalled;
    private bool disposed;
    [TraceSource("ScriptAsCmdlet", "Trace output for script cmdlets")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ScriptAsCmdlet", "Trace output for script cmdlets");

    internal PSScriptCmdlet(ScriptBlock scriptblock) => this._scriptblock = scriptblock;

    internal ScriptBlock ScriptBlock
    {
      get => this._scriptblock;
      set => this._scriptblock = value;
    }

    internal bool UseLocalScope
    {
      get => this._useLocalScope;
      set => this._useLocalScope = value;
    }

    internal bool FromScriptFile
    {
      get => this._fromScriptFile;
      set => this._fromScriptFile = value;
    }

    internal MshCommandRuntime MshCommandRuntime => (MshCommandRuntime) this.commandRuntime;

    internal void PrepareForBinding(bool useLocalScope) => this.UseLocalScope = useLocalScope;

    private void EnterScope()
    {
      this.BackupSpecialVariables();
      this.Context.PSCmdletVariable = (object) this;
      this.MshCommandRuntime.SetPipeWriteEventHandler();
      this.Context.IncrementScopeDepth(ExecutionContext.FeaturesThatNeedDepthHandling.ScriptScope);
    }

    private void ExitScope()
    {
      this.Context.DecrementScopeDepth(ExecutionContext.FeaturesThatNeedDepthHandling.ScriptScope);
      this.MshCommandRuntime.ResetPipeWriteEventHandler();
      this.RestoreSpecialVariables();
    }

    private void BackupSpecialVariables()
    {
      if (!this.UseLocalScope)
      {
        this._oldInput = this.Context.InputVariable;
        this._oldArgs = this.Context.ArgsVariable;
        this._oldUnderbar = this.Context.UnderbarVariable;
      }
      this._oldCmdlet = this.Context.PSCmdletVariable;
      this.BackupDebugPreference();
      this.BackupVerbosePreference();
      this.BackupErrorActionPreference();
      this.BackupWhatIfPreference();
      this.BackupConfirmPreference();
    }

    private void RestoreSpecialVariables()
    {
      if (!this.UseLocalScope)
      {
        this.Context.InputVariable = this._oldInput;
        this.Context.ArgsVariable = this._oldArgs;
        this.Context.UnderbarVariable = this._oldUnderbar;
      }
      this.Context.PSCmdletVariable = this._oldCmdlet;
      this.RestoreDebugPreference();
      this.RestoreVerbosePreference();
      this.RestoreErrorActionPreference();
      this.RestoreWhatIfPreference();
      this.RestoreConfirmPreference();
    }

    private void BackupDebugPreference()
    {
      if (!this.MshCommandRuntime.IsDebugFlagSet)
        return;
      this._oldDebugPreference = this.Context.DebugPreferenceVariable;
      if (this.MshCommandRuntime.Debug)
        this.Context.DebugPreferenceVariable = ActionPreference.Inquire;
      else
        this.Context.DebugPreferenceVariable = ActionPreference.SilentlyContinue;
    }

    private void RestoreDebugPreference()
    {
      if (!this.MshCommandRuntime.IsDebugFlagSet)
        return;
      this.Context.DebugPreferenceVariable = this._oldDebugPreference;
    }

    private void BackupVerbosePreference()
    {
      if (!this.MshCommandRuntime.IsVerboseFlagSet)
        return;
      this._oldVerbosePreference = this.Context.VerbosePreferenceVariable;
      if (this.MshCommandRuntime.Verbose)
        this.Context.VerbosePreferenceVariable = ActionPreference.Continue;
      else
        this.Context.VerbosePreferenceVariable = ActionPreference.SilentlyContinue;
    }

    private void RestoreVerbosePreference()
    {
      if (!this.MshCommandRuntime.IsVerboseFlagSet)
        return;
      this.Context.VerbosePreferenceVariable = this._oldVerbosePreference;
    }

    private void BackupErrorActionPreference()
    {
      if (!this.MshCommandRuntime.IsErrorActionSet)
        return;
      this._oldErrorActionPreference = this.Context.ErrorActionPreferenceVariable;
      this.Context.ErrorActionPreferenceVariable = this.MshCommandRuntime.ErrorAction;
    }

    private void RestoreErrorActionPreference()
    {
      if (!this.MshCommandRuntime.IsErrorActionSet)
        return;
      this.Context.ErrorActionPreferenceVariable = this._oldErrorActionPreference;
    }

    private void BackupWhatIfPreference()
    {
      if (!this.MshCommandRuntime.IsWhatIfFlagSet)
        return;
      this._oldWhatIfPreference = this.Context.WhatIfPreferenceVariable;
      this.Context.WhatIfPreferenceVariable = (object) this.MshCommandRuntime.WhatIf;
    }

    private void RestoreWhatIfPreference()
    {
      if (!this.MshCommandRuntime.IsWhatIfFlagSet)
        return;
      this.Context.WhatIfPreferenceVariable = this._oldWhatIfPreference;
    }

    private void BackupConfirmPreference()
    {
      if (!this.MshCommandRuntime.IsConfirmFlagSet)
        return;
      this._oldConfirmPreference = this.Context.ConfirmPreferenceVariable;
      if ((bool) this.MshCommandRuntime.Confirm)
        this.Context.ConfirmPreferenceVariable = ConfirmImpact.Low;
      else
        this.Context.ConfirmPreferenceVariable = ConfirmImpact.None;
    }

    private void RestoreConfirmPreference()
    {
      if (!this.MshCommandRuntime.IsConfirmFlagSet)
        return;
      this.Context.ConfirmPreferenceVariable = this._oldConfirmPreference;
    }

    protected override void BeginProcessing()
    {
      if (this._scriptblock.Begin == null)
        return;
      if (this.Context.Debugger.IsOn)
        this.Context.Debugger.CheckCommand(this.MyInvocation);
      this.RunClause(this._scriptblock.Begin, (object) AutomationNull.Value, (object) this._input);
    }

    protected override void ProcessRecord()
    {
      if (this._exitWasCalled)
        return;
      if (this._scriptblock.Process != null)
      {
        if (this.Context.Debugger.IsOn)
          this.Context.Debugger.CheckCommand(this.MyInvocation);
        this._input.Add((object) this.CurrentPipelineObject);
        this.RunClause(this._scriptblock.Process, (object) this.CurrentPipelineObject, (object) this._input);
        this._input.Clear();
      }
      else
        this._input.Add((object) this.CurrentPipelineObject);
    }

    protected override void EndProcessing()
    {
      if (this._exitWasCalled || this._scriptblock.End == null)
        return;
      if (this.Context.Debugger.IsOn)
        this.Context.Debugger.CheckCommand(this.MyInvocation);
      object[] objArray = (object[]) null;
      if (this._input != null)
        objArray = this._input.ToArray();
      this.RunClause(this._scriptblock.End, (object) AutomationNull.Value, (object) objArray);
    }

    private void RunClause(ParseTreeNode clause, object dollarUnderbar, object inputToProcess)
    {
      if (clause == null)
        return;
      if (this.Context.PSDebug > 1)
      {
        if (string.IsNullOrEmpty(this.CommandInfo.Name))
          ScriptTrace.Trace(this.Context, 1, "TraceEnteringScriptBlock");
        else if (string.IsNullOrEmpty(this._scriptblock.File))
          ScriptTrace.Trace(this.Context, 1, "TraceEnteringFunction", (object) this.CommandInfo.Name);
        else
          ScriptTrace.Trace(this.Context, 1, "TraceEnteringFunctionDefinedInFile", (object) this.CommandInfo.Name, (object) this._scriptblock.File);
      }
      this.Context.Debugger.PushMethodCall(this.MyInvocation, this._scriptblock);
      bool flag = false;
      if (this._scriptblock.File != null && this._scriptblock.File.Length > 0)
      {
        this.Context.Debugger.PushRunning(this._scriptblock.File, this._scriptblock, this.FromScriptFile);
        flag = true;
      }
      this.EnterScope();
      if (inputToProcess != AutomationNull.Value)
      {
        if (inputToProcess == null)
          inputToProcess = (object) new object[0];
        this.Context.InputVariable = (object) LanguagePrimitives.GetEnumerator(inputToProcess);
      }
      if (dollarUnderbar != AutomationNull.Value)
        this.Context.UnderbarVariable = dollarUnderbar;
      object sendToPipeline = (object) AutomationNull.Value;
      Pipe functionErrorOutputPipe = this.Context.ShellFunctionErrorOutputPipe;
      try
      {
        if (this.MshCommandRuntime.MergeMyErrorOutputWithSuccess)
        {
          if (this.MshCommandRuntime.OutputPipe != null)
            this.Context.RedirectErrorPipe(this.MshCommandRuntime.OutputPipe);
        }
        else if (this.MshCommandRuntime.ErrorOutputPipe.IsRedirected)
          this.Context.RedirectErrorPipe(this.MshCommandRuntime.ErrorOutputPipe);
        sendToPipeline = this.ExecuteWithCatch(clause, this.MshCommandRuntime.OutputPipe);
      }
      finally
      {
        this.Context.RestoreErrorPipe(functionErrorOutputPipe);
        this.ExitScope();
        if (flag)
          this.Context.Debugger.PopRunning();
        this.Context.Debugger.PopMethodCall();
      }
      if (sendToPipeline == AutomationNull.Value)
        return;
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(sendToPipeline);
      if (enumerator != null)
      {
        while (enumerator.MoveNext())
          this.MshCommandRuntime._WriteObjectSkipAllowCheck(enumerator.Current);
      }
      else
        this.MshCommandRuntime._WriteObjectSkipAllowCheck(sendToPipeline);
    }

    private object ExecuteWithCatch(ParseTreeNode ptn, Pipe outputPipe)
    {
      object newValue = (object) null;
      if (!string.IsNullOrEmpty(this.CommandInfo.Name))
      {
        newValue = this.Context.GetVariable("MyInvocation");
        this.Context.SetVariable("MyInvocation", (object) this.MyInvocation);
      }
      try
      {
        return ptn.Execute((Array) null, outputPipe, this.Context);
      }
      catch (ExitException ex)
      {
        if (!this._fromScriptFile)
        {
          throw;
        }
        else
        {
          this._exitWasCalled = true;
          int num = (int) ex.Argument;
          this.Context.SetVariable("global:LASTEXITCODE", (object) num);
          if (num != 0)
            this.MshCommandRuntime.PipelineProcessor.ExecutionFailed = true;
          return (object) AutomationNull.Value;
        }
      }
      catch (ReturnException ex)
      {
        return ex.Argument;
      }
      catch (RuntimeException ex)
      {
        if (this.MshCommandRuntime.PipelineProcessor != null)
        {
          this.MshCommandRuntime.PipelineProcessor.RecordFailure((Exception) ex, (InternalCommand) this);
          if (!(ex is PipelineStoppedException))
            this.MshCommandRuntime.AppendError((object) ex);
        }
        throw new PipelineStoppedException();
      }
      finally
      {
        if (!string.IsNullOrEmpty(this.CommandInfo.Name))
          this.Context.SetVariable("MyInvocation", newValue);
      }
    }

    public object GetDynamicParameters()
    {
      object obj = (object) null;
      this.EnterScope();
      try
      {
        obj = this.ExecuteWithCatch(this._scriptblock.DynamicParams, (Pipe) null);
        if (obj != null)
          obj = PSObject.Base(obj);
      }
      finally
      {
        this.ExitScope();
      }
      return obj == null || !(obj is object[]) ? obj : throw PSScriptCmdlet.tracer.NewInvalidOperationException("AutomationExceptions", "DynamicParametersWrongType", obj);
    }

    public void Dispose()
    {
      if (this.disposed)
        return;
      this.disposed = true;
    }
  }
}
