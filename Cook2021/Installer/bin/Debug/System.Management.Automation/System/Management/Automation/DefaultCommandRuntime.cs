// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DefaultCommandRuntime
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Host;

namespace System.Management.Automation
{
  internal class DefaultCommandRuntime : ICommandRuntime
  {
    private ArrayList output;
    private PSHost host;

    public DefaultCommandRuntime(ArrayList outputArrayList) => this.output = outputArrayList != null ? outputArrayList : throw new ArgumentNullException(nameof (outputArrayList));

    public PSHost Host
    {
      set => this.host = value;
      get => this.host;
    }

    public void WriteDebug(string text)
    {
    }

    public void WriteError(ErrorRecord errorRecord)
    {
      if (errorRecord.Exception != null)
        throw errorRecord.Exception;
      throw new InvalidOperationException(errorRecord.ToString());
    }

    public void WriteObject(object sendToPipeline) => this.output.Add(sendToPipeline);

    public void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
      if (enumerateCollection)
      {
        IEnumerator enumerator = LanguagePrimitives.GetEnumerator(sendToPipeline);
        if (enumerator == null)
        {
          this.output.Add(sendToPipeline);
        }
        else
        {
          while (enumerator.MoveNext())
            this.output.Add(enumerator.Current);
        }
      }
      else
        this.output.Add(sendToPipeline);
    }

    public void WriteProgress(ProgressRecord progressRecord)
    {
    }

    public void WriteProgress(long sourceId, ProgressRecord progressRecord)
    {
    }

    public void WriteVerbose(string text)
    {
    }

    public void WriteWarning(string text)
    {
    }

    public void WriteCommandDetail(string text)
    {
    }

    public bool ShouldProcess(string target) => true;

    public bool ShouldProcess(string target, string action) => true;

    public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption) => true;

    public bool ShouldProcess(
      string verboseDescription,
      string verboseWarning,
      string caption,
      out ShouldProcessReason shouldProcessReason)
    {
      shouldProcessReason = ShouldProcessReason.None;
      return true;
    }

    public bool ShouldContinue(string query, string caption) => true;

    public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll) => true;

    public bool TransactionAvailable() => false;

    public PSTransactionContext CurrentPSTransaction => throw new InvalidOperationException(ResourceManagerCache.GetResourceString("TransactionStrings", "CmdletRequiresUseTx"));

    public void ThrowTerminatingError(ErrorRecord errorRecord)
    {
      if (errorRecord.Exception != null)
        throw errorRecord.Exception;
      throw new InvalidOperationException(errorRecord.ToString());
    }
  }
}
