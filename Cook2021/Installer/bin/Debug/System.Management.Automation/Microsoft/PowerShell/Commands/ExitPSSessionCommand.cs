// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ExitPSSessionCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Remoting;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Exit", "PSSession")]
  public class ExitPSSessionCommand : PSRemotingCmdlet
  {
    protected override void ProcessRecord()
    {
      if (!(this.Host is IHostSupportsInteractiveSession host))
        this.WriteError(new ErrorRecord((Exception) new ArgumentException(this.GetMessage(PSRemotingErrorId.HostDoesNotSupportPushRunspace)), PSRemotingErrorId.HostDoesNotSupportPushRunspace.ToString(), ErrorCategory.InvalidArgument, (object) null));
      else
        host.PopRunspace();
    }
  }
}
