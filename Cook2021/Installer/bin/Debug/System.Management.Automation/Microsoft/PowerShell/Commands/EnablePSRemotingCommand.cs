// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.EnablePSRemotingCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Enable", "PSRemoting", ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
  public sealed class EnablePSRemotingCommand : PSCmdlet
  {
    private const string enableRemotingSbFormat = "\r\nfunction Enable-PSRemoting\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $queryForRegisterDefault,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForRegisterDefault\r\n)\r\n\r\n    end\r\n    {{\r\n        # Enable all Session Configurations\r\n        try {{\r\n            $null = $PSBoundParameters.Remove(\"queryForRegisterDefault\")  \r\n            $null = $PSBoundParameters.Remove(\"captionForRegisterDefault\")  \r\n   \r\n            $PSBoundParameters.Add(\"Name\",\"*\")\r\n\r\n            # first try to enable all the sessions\r\n            Enable-PSSessionConfiguration @PSBoundParameters\r\n            # make sure default powershell end points exist \r\n            #  ie., Microsoft.PowerShell\r\n            #       and Microsoft.PowerShell32 (wow64)\r\n            \r\n            $endPoint = Get-PSSessionConfiguration {0} 2>&1\r\n            $qMessage = $queryForRegisterDefault -f \"{0}\",\"Register-PSSessionConfiguration {0} -force\"\r\n            if ((!$endpoint) -and \r\n                ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForRegisterDefault)))\r\n            {{\r\n                $null = Register-PSSessionConfiguration {0} -force\r\n            }}  \r\n\r\n            $pa = $env:PROCESSOR_ARCHITECTURE\r\n            if ($pa -eq \"x86\")\r\n            {{\r\n                # on 64-bit platforms, wow64 bit process has the correct architecture\r\n                # available in processor_architew6432 varialbe\r\n                $pa = $env:PROCESSOR_ARCHITEW6432\r\n            }}\r\n            if ((($pa -eq \"amd64\") -or ($pa -eq \"ia64\")) -and (test-path $env:windir\\syswow64\\pwrshplugin.dll))\r\n            {{\r\n                # Check availability of WOW64 endpoint. Register if not available.\r\n                $endPoint = Get-PSSessionConfiguration {0}32 2>&1\r\n                $qMessage = $queryForRegisterDefault -f \"{0}32\",\"Register-PSSessionConfiguration {0}32 -processorarchitecture x86 -force\"\r\n                if ((!$endpoint) -and \r\n                    ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForRegisterDefault)))\r\n                {{\r\n                    $null = Register-PSSessionConfiguration {0}32 -processorarchitecture x86 -force\r\n                }}  \r\n            }}\r\n        }} \r\n        catch {{\r\n            throw\r\n        }}  # end of catch   \r\n    }} # end of end block\r\n}} # end of Enable-PSRemoting\r\n\r\nEnable-PSRemoting -force $args[0] -queryForRegisterDefault $args[1] -captionForRegisterDefault $args[2] -whatif:$args[3] -confirm:$args[4]\r\n";
    private static ScriptBlock enableRemotingSb = ScriptBlock.Create(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\nfunction Enable-PSRemoting\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $queryForRegisterDefault,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForRegisterDefault\r\n)\r\n\r\n    end\r\n    {{\r\n        # Enable all Session Configurations\r\n        try {{\r\n            $null = $PSBoundParameters.Remove(\"queryForRegisterDefault\")  \r\n            $null = $PSBoundParameters.Remove(\"captionForRegisterDefault\")  \r\n   \r\n            $PSBoundParameters.Add(\"Name\",\"*\")\r\n\r\n            # first try to enable all the sessions\r\n            Enable-PSSessionConfiguration @PSBoundParameters\r\n            # make sure default powershell end points exist \r\n            #  ie., Microsoft.PowerShell\r\n            #       and Microsoft.PowerShell32 (wow64)\r\n            \r\n            $endPoint = Get-PSSessionConfiguration {0} 2>&1\r\n            $qMessage = $queryForRegisterDefault -f \"{0}\",\"Register-PSSessionConfiguration {0} -force\"\r\n            if ((!$endpoint) -and \r\n                ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForRegisterDefault)))\r\n            {{\r\n                $null = Register-PSSessionConfiguration {0} -force\r\n            }}  \r\n\r\n            $pa = $env:PROCESSOR_ARCHITECTURE\r\n            if ($pa -eq \"x86\")\r\n            {{\r\n                # on 64-bit platforms, wow64 bit process has the correct architecture\r\n                # available in processor_architew6432 varialbe\r\n                $pa = $env:PROCESSOR_ARCHITEW6432\r\n            }}\r\n            if ((($pa -eq \"amd64\") -or ($pa -eq \"ia64\")) -and (test-path $env:windir\\syswow64\\pwrshplugin.dll))\r\n            {{\r\n                # Check availability of WOW64 endpoint. Register if not available.\r\n                $endPoint = Get-PSSessionConfiguration {0}32 2>&1\r\n                $qMessage = $queryForRegisterDefault -f \"{0}32\",\"Register-PSSessionConfiguration {0}32 -processorarchitecture x86 -force\"\r\n                if ((!$endpoint) -and \r\n                    ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForRegisterDefault)))\r\n                {{\r\n                    $null = Register-PSSessionConfiguration {0}32 -processorarchitecture x86 -force\r\n                }}  \r\n            }}\r\n        }} \r\n        catch {{\r\n            throw\r\n        }}  # end of catch   \r\n    }} # end of end block\r\n}} # end of Enable-PSRemoting\r\n\r\nEnable-PSRemoting -force $args[0] -queryForRegisterDefault $args[1] -captionForRegisterDefault $args[2] -whatif:$args[3] -confirm:$args[4]\r\n", (object) "Microsoft.PowerShell"));
    private bool force;

    [Parameter]
    public SwitchParameter Force
    {
      get => (SwitchParameter) this.force;
      set => this.force = (bool) value;
    }

    protected override void BeginProcessing()
    {
      RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
      PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
    }

    protected override void EndProcessing()
    {
      bool whatIf = false;
      bool confirm = true;
      PSSessionConfigurationCommandUtilities.CollectShouldProcessParameters((PSCmdlet) this, out whatIf, out confirm);
      string resourceString1 = ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "ERemotingCaption");
      string resourceString2 = ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "ERemotingQuery");
      EnablePSRemotingCommand.enableRemotingSb.InvokeUsingCmdlet((Cmdlet) this, true, true, (object) AutomationNull.Value, (object) new object[0], (object) AutomationNull.Value, (object) this.force, (object) resourceString2, (object) resourceString1, (object) whatIf, (object) confirm);
    }
  }
}
