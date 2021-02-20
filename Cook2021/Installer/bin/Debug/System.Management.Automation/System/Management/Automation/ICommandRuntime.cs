// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ICommandRuntime
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;

namespace System.Management.Automation
{
  public interface ICommandRuntime
  {
    PSHost Host { get; }

    void WriteDebug(string text);

    void WriteError(ErrorRecord errorRecord);

    void WriteObject(object sendToPipeline);

    void WriteObject(object sendToPipeline, bool enumerateCollection);

    void WriteProgress(ProgressRecord progressRecord);

    void WriteProgress(long sourceId, ProgressRecord progressRecord);

    void WriteVerbose(string text);

    void WriteWarning(string text);

    void WriteCommandDetail(string text);

    bool ShouldProcess(string target);

    bool ShouldProcess(string target, string action);

    bool ShouldProcess(string verboseDescription, string verboseWarning, string caption);

    bool ShouldProcess(
      string verboseDescription,
      string verboseWarning,
      string caption,
      out ShouldProcessReason shouldProcessReason);

    bool ShouldContinue(string query, string caption);

    bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll);

    bool TransactionAvailable();

    PSTransactionContext CurrentPSTransaction { get; }

    void ThrowTerminatingError(ErrorRecord errorRecord);
  }
}
