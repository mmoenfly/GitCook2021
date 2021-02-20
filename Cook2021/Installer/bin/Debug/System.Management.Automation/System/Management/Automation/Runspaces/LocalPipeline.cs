// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.LocalPipeline
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell;
using Microsoft.PowerShell.Commands;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Internal.Host;
using System.Security;
using System.Security.Principal;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
  internal sealed class LocalPipeline : PipelineBase
  {
    private PipelineStopper _stopper;
    private DateTime _pipelineStartTime;
    private long _historyIdForThisPipeline = -1;
    private bool useExternalInput;
    [TraceSource("LocalPipeline", "LocalPipeline")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (LocalPipeline), nameof (LocalPipeline));
    private List<long> _invokeHistoryIds = new List<long>();
    private bool _disposed;

    internal LocalPipeline(
      LocalRunspace runspace,
      string command,
      bool addToHistory,
      bool isNested)
      : base((Runspace) runspace, command, addToHistory, isNested)
    {
      this._stopper = new PipelineStopper(this);
      this.InitStreams();
    }

    internal LocalPipeline(
      LocalRunspace runspace,
      CommandCollection command,
      bool addToHistory,
      bool isNested,
      ObjectStreamBase inputStream,
      ObjectStreamBase outputStream,
      ObjectStreamBase errorStream,
      PSInformationalBuffers infoBuffers)
      : base((Runspace) runspace, command, addToHistory, isNested, inputStream, outputStream, errorStream, infoBuffers)
    {
      this._stopper = new PipelineStopper(this);
      this.InitStreams();
    }

    internal LocalPipeline(LocalPipeline pipeline)
      : base((PipelineBase) pipeline)
    {
      this._stopper = new PipelineStopper(this);
      this.InitStreams();
    }

    public override Pipeline Copy()
    {
      if (this._disposed)
        throw LocalPipeline._trace.NewObjectDisposedException("pipeline");
      return (Pipeline) new LocalPipeline(this);
    }

    protected override void StartPipelineExecution()
    {
      if (this._disposed)
        throw LocalPipeline._trace.NewObjectDisposedException("pipeline");
      this.useExternalInput = this.InputStream.IsOpen || this.InputStream.Count > 0;
      switch (this.LocalRunspace.ThreadOptions)
      {
        case PSThreadOptions.Default:
        case PSThreadOptions.UseNewThread:
          Thread invokeThread = new Thread(new ThreadStart(this.InvokeThreadProc), LocalPipeline.MaxStack);
          this.SetupInvokeThread(invokeThread, true);
          ApartmentState state = this.InvocationSettings == null || this.InvocationSettings.ApartmentState == ApartmentState.Unknown ? this.LocalRunspace.ApartmentState : this.InvocationSettings.ApartmentState;
          if (state != ApartmentState.Unknown)
            invokeThread.SetApartmentState(state);
          invokeThread.Start();
          break;
        case PSThreadOptions.ReuseThread:
          if (this.IsNested)
          {
            this.SetupInvokeThread(Thread.CurrentThread, true);
            this.InvokeThreadProc();
            break;
          }
          PipelineThread pipelineThread = this.LocalRunspace.GetPipelineThread();
          this.SetupInvokeThread(pipelineThread.Worker, true);
          pipelineThread.Start(new ThreadStart(this.InvokeThreadProc));
          break;
        case PSThreadOptions.UseCurrentThread:
          Thread pipelineExecutionThread = this.NestedPipelineExecutionThread;
          CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
          CultureInfo currentUiCulture = Thread.CurrentThread.CurrentUICulture;
          try
          {
            this.SetupInvokeThread(Thread.CurrentThread, false);
            this.InvokeThreadProc();
            break;
          }
          finally
          {
            this.NestedPipelineExecutionThread = pipelineExecutionThread;
            Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentUiCulture;
          }
      }
    }

    private void SetupInvokeThread(Thread invokeThread, bool changeName)
    {
      this.NestedPipelineExecutionThread = invokeThread;
      invokeThread.CurrentCulture = this.LocalRunspace.ExecutionContext.EngineHostInterface.CurrentCulture;
      invokeThread.CurrentUICulture = this.LocalRunspace.ExecutionContext.EngineHostInterface.CurrentUICulture;
      if (invokeThread.Name != null || !changeName)
        return;
      invokeThread.Name = "Pipeline Execution Thread";
    }

    internal static int MaxStack
    {
      get
      {
        int num = LocalPipeline.ReadRegistryInt("PipelineMaxStackSizeMB", 10);
        if (num < 10)
          num = 10;
        else if (num > 100)
          num = 100;
        return num * 1000000;
      }
    }

    private void InvokeHelper()
    {
      PipelineProcessor pipelineProcessor = (PipelineProcessor) null;
      try
      {
        this.RaisePipelineStateEvents();
        this.RecordPipelineStartTime();
        try
        {
          pipelineProcessor = this.CreatePipelineProcessor();
        }
        catch (Exception ex)
        {
          if (this.SetPipelineSessionState)
            this.Runspace.ExecutionContext.AppendDollarError((object) ex);
          throw;
        }
        if (this.useExternalInput)
          pipelineProcessor.ExternalInput = this.InputStream.ObjectReader;
        pipelineProcessor.ExternalSuccessOutput = this.OutputStream.ObjectWriter;
        pipelineProcessor.ExternalErrorOutput = this.ErrorStream.ObjectWriter;
        this.LocalRunspace.ExecutionContext.InternalHost.InternalUI.SetInformationalMessageBuffers(this.InformationalBuffers);
        bool flag = true;
        bool enclosingStatementBlock = this.LocalRunspace.ExecutionContext.ExceptionHandlerInEnclosingStatementBlock;
        this.LocalRunspace.ExecutionContext.ExceptionHandlerInEnclosingStatementBlock = false;
        try
        {
          this._stopper.Push(pipelineProcessor);
          if (!this.AddToHistory)
          {
            flag = this.LocalRunspace.ExecutionContext.QuestionMarkVariableValue;
            this.LocalRunspace.ExecutionContext.IgnoreScriptDebug = true;
          }
          else
            this.LocalRunspace.ExecutionContext.IgnoreScriptDebug = false;
          if (!this.IsNested)
            this.LocalRunspace.ExecutionContext.ResetScopeDepth();
          this.LocalRunspace.ExecutionContext.ResetRedirection();
          try
          {
            pipelineProcessor.Execute();
          }
          catch (ExitException ex1)
          {
            int exitCode = 1;
            if (this.IsNested)
            {
              try
              {
                this.LocalRunspace.ExecutionContext.SetVariable("global:LASTEXITCODE", (object) (int) ex1.Argument);
              }
              finally
              {
                try
                {
                  LocalPipeline._trace.WriteLine("exiting nested prompt at top level from exit statement", new object[0]);
                  this.LocalRunspace.ExecutionContext.EngineHostInterface.ExitNestedPrompt();
                }
                catch (ExitNestedPromptException ex2)
                {
                }
              }
            }
            else
            {
              try
              {
                exitCode = (int) ex1.Argument;
              }
              finally
              {
                this.LocalRunspace.ExecutionContext.EngineHostInterface.SetShouldExit(exitCode);
              }
            }
          }
          catch (ExitNestedPromptException ex)
          {
            LocalPipeline._trace.WriteLine("exiting nested prompt at top level from direct method call", new object[0]);
          }
          catch (FlowControlException ex)
          {
          }
        }
        finally
        {
          if (this.LocalRunspace.Events is PSLocalEventManager events)
            events.ProcessPendingActions();
          this.LocalRunspace.ExecutionContext.ExceptionHandlerInEnclosingStatementBlock = enclosingStatementBlock;
          this.LocalRunspace.ExecutionContext.InternalHost.InternalUI.SetInformationalMessageBuffers((PSInformationalBuffers) null);
          if (!this.AddToHistory)
            this.LocalRunspace.ExecutionContext.QuestionMarkVariableValue = flag;
          this._stopper.Pop();
        }
      }
      finally
      {
        pipelineProcessor?.Dispose();
      }
    }

    internal static int ReadRegistryInt(string policyValueName, int defaultValue)
    {
      RegistryKey registryKey;
      try
      {
        registryKey = Registry.LocalMachine.OpenSubKey(Utils.GetRegistryConfigurationPrefix());
      }
      catch (SecurityException ex)
      {
        LocalPipeline._trace.TraceError("User doesn't have access to open policy key " + policyValueName);
        return defaultValue;
      }
      if (registryKey == null)
        return defaultValue;
      object obj;
      try
      {
        obj = registryKey.GetValue(policyValueName);
      }
      catch (SecurityException ex)
      {
        LocalPipeline._trace.TraceError("User doesn't have access to read policy value " + policyValueName);
        return defaultValue;
      }
      if (obj is int num)
        return num;
      LocalPipeline._trace.TraceError("Policy value is not DWORD " + policyValueName);
      return defaultValue;
    }

    private void InvokeThreadProc()
    {
      bool flag = false;
      Runspace defaultRunspace = Runspace.DefaultRunspace;
      try
      {
        WindowsImpersonationContext impersonationContext = (WindowsImpersonationContext) null;
        try
        {
          if (this.InvocationSettings != null && this.InvocationSettings.FlowImpersonationPolicy)
            impersonationContext = new WindowsIdentity(this.InvocationSettings.WindowsIdentityToImpersonate.Token).Impersonate();
          if (this.InvocationSettings != null && this.InvocationSettings.Host != null)
          {
            if (this.InvocationSettings.Host is InternalHost host)
              this.LocalRunspace.ExecutionContext.InternalHost.SetHostRef(host.ExternalHost);
            else
              this.LocalRunspace.ExecutionContext.InternalHost.SetHostRef(this.InvocationSettings.Host);
          }
          if (this.LocalRunspace.ExecutionContext.InternalHost.ExternalHost.ShouldSetThreadUILanguageToZero)
          {
            int num = (int) NativeCultureResolver.SetThreadUILanguage((short) 0);
          }
          Runspace.DefaultRunspace = (Runspace) this.LocalRunspace;
          this.InvokeHelper();
          this.SetPipelineState(PipelineState.Completed);
        }
        finally
        {
          if (impersonationContext != null)
          {
            try
            {
              impersonationContext.Undo();
              impersonationContext.Dispose();
            }
            catch (SecurityException ex)
            {
              LocalPipeline._trace.TraceWarning("There is some probelm undoing the impersonation context");
              LocalPipeline._trace.TraceException((Exception) ex);
            }
          }
        }
      }
      catch (PipelineStoppedException ex)
      {
        this.SetPipelineState(PipelineState.Stopped, (Exception) ex);
      }
      catch (RuntimeException ex)
      {
        flag = ex is IncompleteParseException;
        this.SetPipelineState(PipelineState.Failed, (Exception) ex);
      }
      catch (ScriptCallDepthException ex)
      {
        this.SetPipelineState(PipelineState.Failed, (Exception) ex);
      }
      catch (SecurityException ex)
      {
        this.SetPipelineState(PipelineState.Failed, (Exception) ex);
      }
      catch (ThreadAbortException ex)
      {
        this.SetPipelineState(PipelineState.Failed, (Exception) ex);
      }
      catch (HaltCommandException ex)
      {
        this.SetPipelineState(PipelineState.Completed);
      }
      finally
      {
        if (this.LocalRunspace.ExecutionContext.InternalHost.IsHostRefSet)
          this.LocalRunspace.ExecutionContext.InternalHost.RevertHostRef();
        Runspace.DefaultRunspace = defaultRunspace;
        if (!flag)
        {
          try
          {
            bool inBreakpoint = this.LocalRunspace.ExecutionContext.Debugger.InBreakpoint;
            if (this._historyIdForThisPipeline == -1L)
              this.AddHistoryEntry(inBreakpoint);
            else
              this.UpdateHistoryEntryAddedByAddHistoryCmdlet(inBreakpoint);
          }
          catch (TerminateException ex)
          {
          }
        }
        if (this.OutputStream.IsOpen)
        {
          try
          {
            this.OutputStream.Close();
          }
          catch (ObjectDisposedException ex)
          {
            LocalPipeline._trace.TraceException((Exception) ex);
          }
        }
        if (this.ErrorStream.IsOpen)
        {
          try
          {
            this.ErrorStream.Close();
          }
          catch (ObjectDisposedException ex)
          {
            LocalPipeline._trace.TraceException((Exception) ex);
          }
        }
        if (this.InputStream.IsOpen)
        {
          try
          {
            this.InputStream.Close();
          }
          catch (ObjectDisposedException ex)
          {
            LocalPipeline._trace.TraceException((Exception) ex);
          }
        }
        this.LocalRunspace.RemoveFromRunningPipelineList((PipelineBase) this);
        if (!this.SyncInvokeCall)
          this.RaisePipelineStateEvents();
      }
    }

    protected override void ImplementStop(bool syncCall)
    {
      if (syncCall)
        this.StopHelper();
      else
        new Thread(new ThreadStart(this.StopThreadProc)).Start();
    }

    private void StopThreadProc() => this.StopHelper();

    internal PipelineStopper Stopper => this._stopper;

    private void StopHelper()
    {
      this.LocalRunspace.StopNestedPipelines((Pipeline) this);
      if (this.InputStream.IsOpen)
      {
        try
        {
          this.InputStream.Close();
        }
        catch (ObjectDisposedException ex)
        {
          LocalPipeline._trace.TraceException((Exception) ex);
        }
      }
      this._stopper.Stop();
      this.PipelineFinishedEvent.WaitOne();
    }

    internal bool IsStopping => this._stopper.IsStopping;

    private PipelineProcessor CreatePipelineProcessor()
    {
      CommandCollection commands = this.Commands;
      if (commands == null || commands.Count == 0)
        throw LocalPipeline._trace.NewInvalidOperationException("Runspace", "NoCommandInPipeline");
      PipelineProcessor pipelineProcessor = new PipelineProcessor();
      pipelineProcessor.TopLevel = true;
      bool flag = false;
      try
      {
        foreach (Command command in (Collection<Command>) commands)
        {
          CommandProcessorBase commandProcessor = command.CreateCommandProcessor(this.LocalRunspace.ExecutionContext, this.LocalRunspace.CommandFactory, this.AddToHistory);
          commandProcessor.RedirectShellErrorOutputPipe = this.RedirectShellErrorOutputPipe;
          pipelineProcessor.Add(commandProcessor);
        }
        return pipelineProcessor;
      }
      catch (RuntimeException ex)
      {
        flag = true;
        throw;
      }
      catch (Exception ex)
      {
        flag = true;
        CommandProcessorBase.CheckForSevereException(ex);
        throw new RuntimeException(ResourceManagerCache.GetResourceString("Pipeline", "CannotCreatePipeline"), ex);
      }
      finally
      {
        if (flag)
          pipelineProcessor.Dispose();
      }
    }

    private void InitStreams()
    {
      if (this.LocalRunspace.ExecutionContext == null)
        return;
      this.LocalRunspace.ExecutionContext.ExternalErrorOutput = this.ErrorStream.ObjectWriter;
      this.LocalRunspace.ExecutionContext.ExternalSuccessOutput = this.OutputStream.ObjectWriter;
    }

    private void RecordPipelineStartTime() => this._pipelineStartTime = DateTime.Now;

    private void AddHistoryEntry(bool skipIfLocked)
    {
      if (!this.AddToHistory)
        return;
      this.LocalRunspace.History.AddEntry(this.InstanceId, this.HistoryString, this.PipelineState, this._pipelineStartTime, DateTime.Now, skipIfLocked);
    }

    internal void AddHistoryEntryFromAddHistoryCmdlet()
    {
      if (this._historyIdForThisPipeline != -1L || !this.AddToHistory)
        return;
      this._historyIdForThisPipeline = this.LocalRunspace.History.AddEntry(this.InstanceId, this.HistoryString, this.PipelineState, this._pipelineStartTime, DateTime.Now, false);
    }

    internal void UpdateHistoryEntryAddedByAddHistoryCmdlet(bool skipIfLocked)
    {
      if (!this.AddToHistory || this._historyIdForThisPipeline == -1L)
        return;
      this.LocalRunspace.History.UpdateEntry(this._historyIdForThisPipeline, this.PipelineState, DateTime.Now, skipIfLocked);
    }

    internal override void SetHistoryString(string historyString) => this.HistoryString = historyString;

    internal static System.Management.Automation.ExecutionContext GetExecutionContextFromTLS() => Runspace.DefaultRunspace?.ExecutionContext;

    private LocalRunspace LocalRunspace => (LocalRunspace) this.Runspace;

    internal bool PresentInInvokeHistoryEntryList(HistoryInfo entry) => this._invokeHistoryIds.Contains(entry.Id);

    internal void AddToInvokeHistoryEntryList(HistoryInfo entry) => this._invokeHistoryIds.Add(entry.Id);

    internal void RemoveFromInvokeHistoryEntryList(HistoryInfo entry) => this._invokeHistoryIds.Remove(entry.Id);

    protected override void Dispose(bool disposing)
    {
      try
      {
        if (this._disposed)
          return;
        this._disposed = true;
        if (!disposing)
          return;
        this.Stop();
      }
      finally
      {
        base.Dispose(disposing);
      }
    }
  }
}
