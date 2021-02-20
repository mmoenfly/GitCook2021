// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CmdletProviderContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Provider;

namespace System.Management.Automation
{
  internal sealed class CmdletProviderContext
  {
    [TraceSource("CmdletProviderContext", "The context under which a core command is being run.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CmdletProviderContext), "The context under which a core command is being run.");
    private CmdletProviderContext copiedContext;
    private ExecutionContext executionContext;
    private PSCredential credentials = PSCredential.Empty;
    private PSDriveInfo drive;
    private bool force;
    private string _filter;
    private Collection<string> _include;
    private Collection<string> _exclude;
    private bool suppressWildcardExpansion;
    private Cmdlet command;
    private CommandOrigin _origin = CommandOrigin.Internal;
    private bool streamObjects;
    private bool streamErrors;
    private Collection<PSObject> accumulatedObjects = new Collection<PSObject>();
    private Collection<ErrorRecord> accumulatedErrorObjects = new Collection<ErrorRecord>();
    private CmdletProvider providerInstance;
    private object dynamicParameters;
    private bool stopping;
    private Collection<CmdletProviderContext> stopReferrals = new Collection<CmdletProviderContext>();

    internal CmdletProviderContext(ExecutionContext executionContext)
    {
      this.executionContext = executionContext != null ? executionContext : throw CmdletProviderContext.tracer.NewArgumentNullException(nameof (executionContext));
      this._origin = CommandOrigin.Internal;
      this.drive = executionContext.EngineSessionState.CurrentDrive;
      if (executionContext.CurrentCommandProcessor == null || !(executionContext.CurrentCommandProcessor.Command is Cmdlet))
        return;
      this.command = (Cmdlet) executionContext.CurrentCommandProcessor.Command;
    }

    internal CmdletProviderContext(ExecutionContext executionContext, CommandOrigin origin)
    {
      this.executionContext = executionContext != null ? executionContext : throw CmdletProviderContext.tracer.NewArgumentNullException(nameof (executionContext));
      this._origin = origin;
    }

    internal CmdletProviderContext(PSCmdlet command, PSCredential credentials, PSDriveInfo drive)
    {
      this.command = command != null ? (Cmdlet) command : throw CmdletProviderContext.tracer.NewArgumentNullException(nameof (command));
      this._origin = command.CommandOrigin;
      if (credentials != null)
        this.credentials = credentials;
      this.drive = drive;
      if (command.Host == null)
        throw CmdletProviderContext.tracer.NewArgumentException("command.Host");
      this.executionContext = command.Context != null ? command.Context : throw CmdletProviderContext.tracer.NewArgumentException("command.Context");
      this.streamObjects = true;
      this.streamErrors = true;
    }

    internal CmdletProviderContext(PSCmdlet command, PSCredential credentials)
    {
      this.command = command != null ? (Cmdlet) command : throw CmdletProviderContext.tracer.NewArgumentNullException(nameof (command));
      this._origin = command.CommandOrigin;
      if (credentials != null)
        this.credentials = credentials;
      if (command.Host == null)
        throw CmdletProviderContext.tracer.NewArgumentException("command.Host");
      this.executionContext = command.Context != null ? command.Context : throw CmdletProviderContext.tracer.NewArgumentException("command.Context");
      this.streamObjects = true;
      this.streamErrors = true;
    }

    internal CmdletProviderContext(Cmdlet command)
    {
      this.command = command != null ? command : throw CmdletProviderContext.tracer.NewArgumentNullException(nameof (command));
      this._origin = command.CommandOrigin;
      this.executionContext = command.Context != null ? command.Context : throw CmdletProviderContext.tracer.NewArgumentException("command.Context");
      this.streamObjects = true;
      this.streamErrors = true;
    }

    internal CmdletProviderContext(CmdletProviderContext contextToCopyFrom)
    {
      this.executionContext = contextToCopyFrom != null ? contextToCopyFrom.ExecutionContext : throw CmdletProviderContext.tracer.NewArgumentNullException(nameof (contextToCopyFrom));
      this.command = contextToCopyFrom.command;
      if (contextToCopyFrom.Credential != null)
        this.credentials = contextToCopyFrom.Credential;
      this.drive = contextToCopyFrom.Drive;
      this.force = (bool) contextToCopyFrom.Force;
      this.CopyFilters(contextToCopyFrom);
      this.suppressWildcardExpansion = contextToCopyFrom.SuppressWildcardExpansion;
      this.dynamicParameters = contextToCopyFrom.DynamicParameters;
      this._origin = contextToCopyFrom._origin;
      this.stopping = contextToCopyFrom.Stopping;
      contextToCopyFrom.StopReferrals.Add(this);
      this.copiedContext = contextToCopyFrom;
    }

    internal CommandOrigin Origin => this._origin;

    internal ExecutionContext ExecutionContext => this.executionContext;

    internal CmdletProvider ProviderInstance
    {
      get => this.providerInstance;
      set => this.providerInstance = value;
    }

    private void CopyFilters(CmdletProviderContext context)
    {
      this._include = context.Include;
      this._exclude = context.Exclude;
      this._filter = context.Filter;
    }

    internal void RemoveStopReferral()
    {
      if (this.copiedContext == null)
        return;
      this.copiedContext.StopReferrals.Remove(this);
    }

    internal object DynamicParameters
    {
      get => this.dynamicParameters;
      set => this.dynamicParameters = value;
    }

    internal bool PassThru
    {
      get => this.streamObjects;
      set => this.streamObjects = value;
    }

    internal PSDriveInfo Drive
    {
      get => this.drive;
      set => this.drive = value;
    }

    internal PSCredential Credential
    {
      get
      {
        PSCredential psCredential = this.credentials;
        if (this.credentials == null && this.drive != (PSDriveInfo) null)
          psCredential = this.drive.Credential;
        return psCredential;
      }
    }

    internal bool UseTransaction => this.command != null && this.command.CommandRuntime != null && this.command.CommandRuntime is MshCommandRuntime commandRuntime && (bool) commandRuntime.UseTransaction;

    public bool TransactionAvailable() => this.command != null && this.command.TransactionAvailable();

    public PSTransactionContext CurrentPSTransaction => this.command != null ? this.command.CurrentPSTransaction : (PSTransactionContext) null;

    internal SwitchParameter Force
    {
      get => (SwitchParameter) this.force;
      set => this.force = (bool) value;
    }

    internal string Filter
    {
      get => this._filter;
      set => this._filter = value;
    }

    internal Collection<string> Include => this._include;

    internal Collection<string> Exclude => this._exclude;

    public bool SuppressWildcardExpansion
    {
      get => this.suppressWildcardExpansion;
      internal set => this.suppressWildcardExpansion = value;
    }

    internal bool ShouldProcess(string target)
    {
      bool flag = true;
      if (this.command != null)
        flag = this.command.ShouldProcess(target);
      CmdletProviderContext.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool ShouldProcess(string target, string action)
    {
      bool flag = true;
      if (this.command != null)
        flag = this.command.ShouldProcess(target, action);
      CmdletProviderContext.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
    {
      bool flag = true;
      if (this.command != null)
        flag = this.command.ShouldProcess(verboseDescription, verboseWarning, caption);
      CmdletProviderContext.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool ShouldProcess(
      string verboseDescription,
      string verboseWarning,
      string caption,
      out ShouldProcessReason shouldProcessReason)
    {
      bool flag = true;
      if (this.command != null)
        flag = this.command.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
      else
        shouldProcessReason = ShouldProcessReason.None;
      CmdletProviderContext.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool ShouldContinue(string query, string caption)
    {
      bool flag = true;
      if (this.command != null)
        flag = this.command.ShouldContinue(query, caption);
      CmdletProviderContext.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool ShouldContinue(
      string query,
      string caption,
      ref bool yesToAll,
      ref bool noToAll)
    {
      bool flag = true;
      if (this.command != null)
      {
        flag = this.command.ShouldContinue(query, caption, ref yesToAll, ref noToAll);
      }
      else
      {
        yesToAll = false;
        noToAll = false;
      }
      CmdletProviderContext.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal void WriteVerbose(string text)
    {
      if (this.command == null)
        return;
      this.command.WriteVerbose(text);
    }

    internal void WriteWarning(string text)
    {
      if (this.command == null)
        return;
      this.command.WriteWarning(text);
    }

    internal void WriteProgress(ProgressRecord record)
    {
      if (this.command == null)
        return;
      this.command.WriteProgress(record);
    }

    internal void WriteDebug(string text)
    {
      if (this.command == null)
        return;
      this.command.WriteDebug(text);
    }

    internal void SetFilters(Collection<string> include, Collection<string> exclude, string filter)
    {
      this._include = include;
      this._exclude = exclude;
      this._filter = filter;
    }

    internal Collection<PSObject> GetAccumulatedObjects()
    {
      Collection<PSObject> accumulatedObjects = this.accumulatedObjects;
      this.accumulatedObjects = new Collection<PSObject>();
      return accumulatedObjects;
    }

    internal Collection<ErrorRecord> GetAccumulatedErrorObjects()
    {
      Collection<ErrorRecord> accumulatedErrorObjects = this.accumulatedErrorObjects;
      this.accumulatedErrorObjects = new Collection<ErrorRecord>();
      return accumulatedErrorObjects;
    }

    internal void ThrowFirstErrorOrDoNothing() => this.ThrowFirstErrorOrDoNothing(true);

    internal void ThrowFirstErrorOrDoNothing(bool wrapExceptionInProviderException)
    {
      if (!this.HasErrors())
        return;
      Collection<ErrorRecord> accumulatedErrorObjects = this.GetAccumulatedErrorObjects();
      if (accumulatedErrorObjects == null || accumulatedErrorObjects.Count <= 0)
        return;
      if (wrapExceptionInProviderException)
      {
        ProviderInfo provider = (ProviderInfo) null;
        if (this.ProviderInstance != null)
          provider = this.ProviderInstance.ProviderInfo;
        ProviderInvocationException invocationException = new ProviderInvocationException(provider, accumulatedErrorObjects[0]);
        CmdletProviderContext.tracer.TraceException((Exception) invocationException);
        MshLog.LogProviderHealthEvent(this.ExecutionContext, provider != null ? provider.Name : "unknown provider", (Exception) invocationException, Severity.Warning);
        throw invocationException;
      }
      CmdletProviderContext.tracer.TraceException(accumulatedErrorObjects[0].Exception);
      throw accumulatedErrorObjects[0].Exception;
    }

    internal void WriteErrorsToContext(CmdletProviderContext errorContext)
    {
      if (errorContext == null)
        throw CmdletProviderContext.tracer.NewArgumentNullException(nameof (errorContext));
      if (!this.HasErrors())
        return;
      foreach (ErrorRecord accumulatedErrorObject in this.GetAccumulatedErrorObjects())
        errorContext.WriteError(accumulatedErrorObject);
    }

    internal void WriteObject(object obj)
    {
      if (this.Stopping)
      {
        PipelineStoppedException stoppedException = new PipelineStoppedException();
        CmdletProviderContext.tracer.TraceException((Exception) stoppedException);
        throw stoppedException;
      }
      if (this.streamObjects)
      {
        if (this.command != null)
        {
          CmdletProviderContext.tracer.WriteLine("Writing to command pipeline", new object[0]);
          this.command.WriteObject(obj);
        }
        else
        {
          InvalidOperationException operationException = (InvalidOperationException) CmdletProviderContext.tracer.NewInvalidOperationException("SessionStateStrings", "OutputStreamingNotEnabled");
          CmdletProviderContext.tracer.TraceException((Exception) operationException);
          throw operationException;
        }
      }
      else
      {
        CmdletProviderContext.tracer.WriteLine("Writing to accumulated objects", new object[0]);
        this.accumulatedObjects.Add(PSObject.AsPSObject(obj));
      }
    }

    internal void WriteError(ErrorRecord errorRecord)
    {
      if (this.Stopping)
      {
        PipelineStoppedException stoppedException = new PipelineStoppedException();
        CmdletProviderContext.tracer.TraceException((Exception) stoppedException);
        throw stoppedException;
      }
      if (this.streamErrors)
      {
        if (this.command != null)
        {
          CmdletProviderContext.tracer.WriteLine("Writing error package to command error pipe", new object[0]);
          this.command.WriteError(errorRecord);
        }
        else
        {
          InvalidOperationException operationException = (InvalidOperationException) CmdletProviderContext.tracer.NewInvalidOperationException("SessionStateStrings", "ErrorStreamingNotEnabled");
          CmdletProviderContext.tracer.TraceException((Exception) operationException);
          throw operationException;
        }
      }
      else
      {
        this.accumulatedErrorObjects.Add(errorRecord);
        if (errorRecord.ErrorDetails == null || errorRecord.ErrorDetails.TextLookupError == null)
          return;
        Exception textLookupError = errorRecord.ErrorDetails.TextLookupError;
        errorRecord.ErrorDetails.TextLookupError = (Exception) null;
        MshLog.LogProviderHealthEvent(this.ExecutionContext, this.ProviderInstance.ProviderInfo.Name, textLookupError, Severity.Warning);
      }
    }

    internal bool HasErrors()
    {
      bool flag = false;
      if (this.accumulatedErrorObjects != null && this.accumulatedErrorObjects.Count > 0)
        flag = true;
      CmdletProviderContext.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal void StopProcessing()
    {
      this.stopping = true;
      if (this.providerInstance != null)
        this.providerInstance.StopProcessing();
      foreach (CmdletProviderContext stopReferral in this.StopReferrals)
        stopReferral.StopProcessing();
    }

    internal bool Stopping => this.stopping;

    internal Collection<CmdletProviderContext> StopReferrals => this.stopReferrals;
  }
}
