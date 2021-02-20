// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.PSRemotingCmdlet
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Remoting;

namespace Microsoft.PowerShell.Commands
{
  public abstract class PSRemotingCmdlet : PSCmdlet
  {
    protected const string ComputerNameParameterSet = "ComputerName";
    protected const string SessionParameterSet = "Session";
    private static string LOCALHOST = "localhost";
    private bool _skipWinRMCheck;

    protected override void BeginProcessing()
    {
      if (this._skipWinRMCheck)
        return;
      RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
    }

    internal void WriteStreamObject(PSStreamObject psstreamObject)
    {
      switch (psstreamObject.objectType)
      {
        case PSStreamObjectType.Output:
          this.WriteObject(psstreamObject.value);
          break;
        case PSStreamObjectType.Error:
          ErrorRecord errorRecord = (ErrorRecord) psstreamObject.value;
          errorRecord.PreserveInvocationInfoOnce = true;
          this.WriteError(errorRecord);
          break;
        case PSStreamObjectType.Verbose:
          this.WriteVerbose((string) psstreamObject.value);
          break;
        case PSStreamObjectType.Debug:
          this.WriteDebug((string) psstreamObject.value);
          break;
        case PSStreamObjectType.MethodExecutor:
          ((ClientMethodExecutor) psstreamObject.value).Execute((Cmdlet) this);
          break;
        case PSStreamObjectType.Warning:
          this.WriteWarning((string) psstreamObject.value);
          break;
      }
    }

    protected void ResolveComputerNames(string[] computerNames, out string[] resolvedComputerNames)
    {
      if (computerNames == null)
      {
        resolvedComputerNames = new string[1];
        resolvedComputerNames[0] = this.ResolveComputerName(".");
      }
      else if (computerNames.Length == 0)
      {
        resolvedComputerNames = new string[0];
      }
      else
      {
        resolvedComputerNames = new string[computerNames.Length];
        for (int index = 0; index < resolvedComputerNames.Length; ++index)
          resolvedComputerNames[index] = this.ResolveComputerName(computerNames[index]);
      }
    }

    protected string ResolveComputerName(string computerName) => string.Equals(computerName, ".", StringComparison.OrdinalIgnoreCase) ? PSRemotingCmdlet.LOCALHOST : computerName;

    internal string GetMessage(PSRemotingErrorId errorId) => this.GetMessage(errorId, (object[]) null);

    internal string GetMessage(PSRemotingErrorId errorId, params object[] args) => args == null ? ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", errorId.ToString()) : ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", errorId.ToString(), args);

    internal bool SkipWinRMCheck
    {
      get => this._skipWinRMCheck;
      set => this._skipWinRMCheck = value;
    }
  }
}
