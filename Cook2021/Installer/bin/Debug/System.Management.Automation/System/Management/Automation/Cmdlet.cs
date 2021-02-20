// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Cmdlet
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Internal;
using System.Resources;
using System.Threading;

namespace System.Management.Automation
{
  public abstract class Cmdlet : InternalCommand
  {
    [TraceSource("Cmdlet", "Cmdlet")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (Cmdlet), nameof (Cmdlet));
    private string _parameterSetName = "";

    public bool Stopping
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return this.IsStopping;
      }
    }

    internal string _ParameterSetName => this._parameterSetName;

    internal void SetParameterSetName(string parameterSetName) => this._parameterSetName = parameterSetName;

    internal override void DoBeginProcessing()
    {
      if (this.CommandRuntime is MshCommandRuntime commandRuntime && (bool) commandRuntime.UseTransaction && !this.Context.TransactionManager.HasTransaction)
      {
        string resourceString = ResourceManagerCache.GetResourceString("TransactionStrings", "NoTransactionStarted");
        if (this.Context.TransactionManager.IsLastTransactionCommitted)
          resourceString = ResourceManagerCache.GetResourceString("TransactionStrings", "NoTransactionStartedFromCommit");
        else if (this.Context.TransactionManager.IsLastTransactionRolledBack)
          resourceString = ResourceManagerCache.GetResourceString("TransactionStrings", "NoTransactionStartedFromRollback");
        throw new InvalidOperationException(resourceString);
      }
      this.BeginProcessing();
    }

    internal override void DoProcessRecord() => this.ProcessRecord();

    internal override void DoEndProcessing() => this.EndProcessing();

    internal override void DoStopProcessing() => this.StopProcessing();

    protected Cmdlet()
    {
      using (Cmdlet.tracer.TraceConstructor((object) this))
        ;
    }

    public virtual string GetResourceString(string baseName, string resourceId)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (string.IsNullOrEmpty(baseName))
          throw Cmdlet.tracer.NewArgumentNullException(nameof (baseName));
        if (string.IsNullOrEmpty(resourceId))
          throw Cmdlet.tracer.NewArgumentNullException(nameof (resourceId));
        ResourceManager resourceManager = ResourceManagerCache.GetResourceManager(this.GetType().Assembly, baseName);
        string str;
        try
        {
          str = resourceManager.GetString(resourceId, Thread.CurrentThread.CurrentUICulture);
        }
        catch (MissingManifestResourceException ex)
        {
          throw Cmdlet.tracer.NewArgumentException(nameof (baseName), "GetErrorText", "ResourceBaseNameFailure", (object) baseName);
        }
        return str != null ? str : throw Cmdlet.tracer.NewArgumentException(nameof (resourceId), "GetErrorText", "ResourceIdFailure", (object) resourceId);
      }
    }

    public ICommandRuntime CommandRuntime
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return this.commandRuntime;
      }
      set
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          this.commandRuntime = value;
      }
    }

    public void WriteError(ErrorRecord errorRecord)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (this.commandRuntime == null)
          throw new NotImplementedException(nameof (WriteError));
        this.commandRuntime.WriteError(errorRecord);
      }
    }

    public void WriteObject(object sendToPipeline)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (this.commandRuntime == null)
          throw new NotImplementedException(nameof (WriteObject));
        this.commandRuntime.WriteObject(sendToPipeline);
      }
    }

    public void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (this.commandRuntime == null)
          throw new NotImplementedException(nameof (WriteObject));
        this.commandRuntime.WriteObject(sendToPipeline, enumerateCollection);
      }
    }

    public void WriteVerbose(string text)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (this.commandRuntime == null)
          throw new NotImplementedException(nameof (WriteVerbose));
        this.commandRuntime.WriteVerbose(text);
      }
    }

    public void WriteWarning(string text)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (this.commandRuntime == null)
          throw new NotImplementedException(nameof (WriteWarning));
        this.commandRuntime.WriteWarning(text);
      }
    }

    public void WriteCommandDetail(string text)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (this.commandRuntime == null)
          throw new NotImplementedException(nameof (WriteCommandDetail));
        this.commandRuntime.WriteCommandDetail(text);
      }
    }

    public void WriteProgress(ProgressRecord progressRecord)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (this.commandRuntime == null)
          throw new NotImplementedException(nameof (WriteProgress));
        this.commandRuntime.WriteProgress(progressRecord);
      }
    }

    internal void WriteProgress(long sourceId, ProgressRecord progressRecord)
    {
      if (this.commandRuntime == null)
        throw new NotImplementedException(nameof (WriteProgress));
      this.commandRuntime.WriteProgress(sourceId, progressRecord);
    }

    public void WriteDebug(string text)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (this.commandRuntime == null)
          throw new NotImplementedException(nameof (WriteDebug));
        this.commandRuntime.WriteDebug(text);
      }
    }

    public bool ShouldProcess(string target)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.commandRuntime == null || this.commandRuntime.ShouldProcess(target);
    }

    public bool ShouldProcess(string target, string action)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.commandRuntime == null || this.commandRuntime.ShouldProcess(target, action);
    }

    public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.commandRuntime == null || this.commandRuntime.ShouldProcess(verboseDescription, verboseWarning, caption);
    }

    public bool ShouldProcess(
      string verboseDescription,
      string verboseWarning,
      string caption,
      out ShouldProcessReason shouldProcessReason)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (this.commandRuntime != null)
          return this.commandRuntime.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
        shouldProcessReason = ShouldProcessReason.None;
        return true;
      }
    }

    public bool ShouldContinue(string query, string caption)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.commandRuntime == null || this.commandRuntime.ShouldContinue(query, caption);
    }

    public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.commandRuntime == null || this.commandRuntime.ShouldContinue(query, caption, ref yesToAll, ref noToAll);
    }

    internal ArrayList GetResults()
    {
      if (this.commandRuntime != null)
        throw new InvalidOperationException();
      if (this is PSCmdlet)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("CommandBaseStrings", "CannotInvokePSCmdletsDirectly"));
      ArrayList outputArrayList = new ArrayList();
      this.CommandRuntime = (ICommandRuntime) new DefaultCommandRuntime(outputArrayList);
      this.BeginProcessing();
      this.ProcessRecord();
      this.EndProcessing();
      return outputArrayList;
    }

    public IEnumerable Invoke()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        ArrayList data = this.GetResults();
        for (int i = 0; i < data.Count; ++i)
          yield return data[i];
      }
    }

    public IEnumerable<T> Invoke<T>()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        ArrayList data = this.GetResults();
        for (int i = 0; i < data.Count; ++i)
          yield return (T) data[i];
      }
    }

    public bool TransactionAvailable()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.commandRuntime != null ? this.commandRuntime.TransactionAvailable() : throw new NotImplementedException(nameof (TransactionAvailable));
    }

    public PSTransactionContext CurrentPSTransaction => this.commandRuntime != null ? this.commandRuntime.CurrentPSTransaction : throw new NotImplementedException(nameof (CurrentPSTransaction));

    public void ThrowTerminatingError(ErrorRecord errorRecord)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (errorRecord == null)
          throw new ArgumentNullException(nameof (errorRecord));
        if (this.commandRuntime != null)
        {
          this.commandRuntime.ThrowTerminatingError(errorRecord);
        }
        else
        {
          if (errorRecord.Exception != null)
            throw errorRecord.Exception;
          throw new InvalidOperationException(errorRecord.ToString());
        }
      }
    }

    protected virtual void BeginProcessing()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        ;
    }

    protected virtual void ProcessRecord()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        ;
    }

    protected virtual void EndProcessing()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        ;
    }

    protected virtual void StopProcessing()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        ;
    }
  }
}
