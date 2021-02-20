// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.PSSessionConfigurationCommandUtilities
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Security.Principal;

namespace Microsoft.PowerShell.Commands
{
  internal static class PSSessionConfigurationCommandUtilities
  {
    internal const string restartWSManFormat = "restart-service winrm -force -confirm:$false";
    internal const string PSCustomShellTypeName = "Microsoft.PowerShell.Commands.PSSessionConfigurationCommands#PSSessionConfiguration";
    [TraceSource("PSSessionConfiguration", "PSSessionConfiguration cmdlet")]
    internal static readonly PSTraceSource Tracer = PSTraceSource.GetTracer("PSSessionConfiguration", "PSSessionConfiguration cmdlet");

    internal static void RestartWinRMService(
      PSCmdlet cmdlet,
      bool isErrorReported,
      bool force,
      bool noServiceRestart)
    {
      if (isErrorReported || noServiceRestart)
        return;
      string resourceString = ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "RestartWSManServiceAction");
      string target = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "RestartWSManServiceTarget", (object) "WinRM");
      if (!force && !cmdlet.ShouldProcess(target, resourceString))
        return;
      cmdlet.WriteVerbose(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "RestartWSManServiceMessageV"));
      cmdlet.InvokeCommand.NewScriptBlock("restart-service winrm -force -confirm:$false").InvokeUsingCmdlet((Cmdlet) cmdlet, true, true, (object) AutomationNull.Value, (object) new object[0], (object) AutomationNull.Value);
    }

    internal static void CollectShouldProcessParameters(
      PSCmdlet cmdlet,
      out bool whatIf,
      out bool confirm)
    {
      whatIf = false;
      confirm = true;
      if (!(cmdlet.CommandRuntime is MshCommandRuntime commandRuntime))
        return;
      whatIf = (bool) commandRuntime.WhatIf;
      if (!commandRuntime.IsConfirmFlagSet)
        return;
      confirm = (bool) commandRuntime.Confirm;
    }

    internal static void ThrowIfNotAdministrator()
    {
      if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        throw new InvalidOperationException(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "EDcsRequiresElevation"));
    }
  }
}
