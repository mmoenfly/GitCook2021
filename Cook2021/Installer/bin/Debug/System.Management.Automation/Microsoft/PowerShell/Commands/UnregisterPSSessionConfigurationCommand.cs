// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.UnregisterPSSessionConfigurationCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Unregister", "PSSessionConfiguration", ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
  public sealed class UnregisterPSSessionConfigurationCommand : PSCmdlet
  {
    private const string removePluginSbFormat = "\r\nfunction Unregister-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(\r\n       $filter,\r\n       $action,\r\n       $targetTemplate,\r\n       $shellNotErrMsgFormat,\r\n       [bool]$force)\r\n\r\n    process\r\n    {{\r\n        $shellsFound = 0\r\n        dir 'WSMan:\\localhost\\Plugin\\' | ? {{ $_.name -like \"$filter\" }} | % {{\r\n            $pluginFileNamePath = join-path \"$($_.pspath)\" 'FileName'\r\n            if (!(test-path \"$pluginFileNamePath\"))\r\n            {{\r\n                return\r\n            }}\r\n\r\n           $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n           if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n           {{\r\n                return  \r\n           }}\r\n           \r\n           $shellsFound++\r\n\r\n           $shouldProcessTargetString = $targetTemplate -f $_.name\r\n           if($force -or $pscmdlet.ShouldProcess($shouldProcessTargetString, $action))\r\n           {{\r\n                remove-item -literalpath \"$($_.pspath)\" -recurse -force\r\n           }}\r\n        }}\r\n\r\n        if (!$shellsFound)\r\n        {{\r\n            $errMsg = $shellNotErrMsgFormat -f $filter\r\n            Write-Error $errMsg \r\n        }}\r\n    }} # end of Process block\r\n}}\r\n\r\nUnregister-PSSessionConfiguration -filter $args[0] -whatif:$args[1] -confirm:$args[2] -action $args[3] -targetTemplate $args[4] -shellNotErrMsgFormat $args[5] -force $args[6]\r\n";
    private static readonly ScriptBlock removePluginSb = ScriptBlock.Create(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\nfunction Unregister-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(\r\n       $filter,\r\n       $action,\r\n       $targetTemplate,\r\n       $shellNotErrMsgFormat,\r\n       [bool]$force)\r\n\r\n    process\r\n    {{\r\n        $shellsFound = 0\r\n        dir 'WSMan:\\localhost\\Plugin\\' | ? {{ $_.name -like \"$filter\" }} | % {{\r\n            $pluginFileNamePath = join-path \"$($_.pspath)\" 'FileName'\r\n            if (!(test-path \"$pluginFileNamePath\"))\r\n            {{\r\n                return\r\n            }}\r\n\r\n           $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n           if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n           {{\r\n                return  \r\n           }}\r\n           \r\n           $shellsFound++\r\n\r\n           $shouldProcessTargetString = $targetTemplate -f $_.name\r\n           if($force -or $pscmdlet.ShouldProcess($shouldProcessTargetString, $action))\r\n           {{\r\n                remove-item -literalpath \"$($_.pspath)\" -recurse -force\r\n           }}\r\n        }}\r\n\r\n        if (!$shellsFound)\r\n        {{\r\n            $errMsg = $shellNotErrMsgFormat -f $filter\r\n            Write-Error $errMsg \r\n        }}\r\n    }} # end of Process block\r\n}}\r\n\r\nUnregister-PSSessionConfiguration -filter $args[0] -whatif:$args[1] -confirm:$args[2] -action $args[3] -targetTemplate $args[4] -shellNotErrMsgFormat $args[5] -force $args[6]\r\n", (object) "pwrshplugin.dll"));
    private bool isErrorReported;
    private string shellName;
    private bool force;
    private bool noRestart;
    private bool shouldOfferRestart;

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    public string Name
    {
      get => this.shellName;
      set => this.shellName = value;
    }

    [Parameter]
    public SwitchParameter Force
    {
      get => (SwitchParameter) this.force;
      set => this.force = (bool) value;
    }

    [Parameter]
    public SwitchParameter NoServiceRestart
    {
      get => (SwitchParameter) this.noRestart;
      set => this.noRestart = (bool) value;
    }

    protected override void BeginProcessing()
    {
      RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
      PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
    }

    protected override void ProcessRecord()
    {
      this.WriteVerbose(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "RcsScriptMessageV", (object) "\r\nfunction Unregister-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(\r\n       $filter,\r\n       $action,\r\n       $targetTemplate,\r\n       $shellNotErrMsgFormat,\r\n       [bool]$force)\r\n\r\n    process\r\n    {{\r\n        $shellsFound = 0\r\n        dir 'WSMan:\\localhost\\Plugin\\' | ? {{ $_.name -like \"$filter\" }} | % {{\r\n            $pluginFileNamePath = join-path \"$($_.pspath)\" 'FileName'\r\n            if (!(test-path \"$pluginFileNamePath\"))\r\n            {{\r\n                return\r\n            }}\r\n\r\n           $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n           if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n           {{\r\n                return  \r\n           }}\r\n           \r\n           $shellsFound++\r\n\r\n           $shouldProcessTargetString = $targetTemplate -f $_.name\r\n           if($force -or $pscmdlet.ShouldProcess($shouldProcessTargetString, $action))\r\n           {{\r\n                remove-item -literalpath \"$($_.pspath)\" -recurse -force\r\n           }}\r\n        }}\r\n\r\n        if (!$shellsFound)\r\n        {{\r\n            $errMsg = $shellNotErrMsgFormat -f $filter\r\n            Write-Error $errMsg \r\n        }}\r\n    }} # end of Process block\r\n}}\r\n\r\nUnregister-PSSessionConfiguration -filter $args[0] -whatif:$args[1] -confirm:$args[2] -action $args[3] -targetTemplate $args[4] -shellNotErrMsgFormat $args[5] -force $args[6]\r\n"));
      string str = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "CSShouldProcessAction", (object) this.CommandInfo.Name);
      string resourceString1 = ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "CSShouldProcessTarget");
      string resourceString2 = ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "CustomShellNotFound");
      bool whatIf = false;
      bool confirm = true;
      PSSessionConfigurationCommandUtilities.CollectShouldProcessParameters((PSCmdlet) this, out whatIf, out confirm);
      int count = ((ArrayList) this.Context.DollarErrorVariable).Count;
      object sendToPipeline = UnregisterPSSessionConfigurationCommand.removePluginSb.InvokeUsingCmdlet((Cmdlet) this, true, true, (object) AutomationNull.Value, (object) new object[0], (object) AutomationNull.Value, (object) this.shellName, (object) whatIf, (object) confirm, (object) str, (object) resourceString1, (object) resourceString2, (object) this.force);
      if (sendToPipeline != AutomationNull.Value)
        this.WriteObject(sendToPipeline);
      this.isErrorReported = ((ArrayList) this.Context.DollarErrorVariable).Count > count;
      this.shouldOfferRestart = true;
    }

    protected override void EndProcessing() => PSSessionConfigurationCommandUtilities.RestartWinRMService((PSCmdlet) this, !this.shouldOfferRestart || this.isErrorReported, (bool) this.Force, !this.shouldOfferRestart || this.noRestart);
  }
}
