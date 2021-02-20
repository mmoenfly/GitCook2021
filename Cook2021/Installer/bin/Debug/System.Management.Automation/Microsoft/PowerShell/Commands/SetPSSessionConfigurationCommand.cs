// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.SetPSSessionConfigurationCommand
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
  [Cmdlet("Set", "PSSessionConfiguration", ConfirmImpact = ConfirmImpact.High, DefaultParameterSetName = "__AllParameterSets", SupportsShouldProcess = true)]
  public sealed class SetPSSessionConfigurationCommand : PSSessionConfigurationCommandBase
  {
    private const string setPluginSbFormat = "\r\nfunction Set-PSSessionConfiguration([PSObject]$customShellObject, \r\n     [Array]$initParametersMap,\r\n     [bool]$force,\r\n     [string]$sddl,\r\n     [bool]$isSddlSpecified,\r\n     [bool]$shouldShowUI,\r\n     [string]$resourceUri,\r\n     [string]$pluginNotFoundErrorMsg,\r\n     [string]$pluginNotPowerShellMsg)\r\n{{\r\n   $wsmanPluginDir = 'WSMan:\\localhost\\Plugin'\r\n   $pluginName = $customShellObject.Name;\r\n   $pluginDir = Join-Path \"$wsmanPluginDir\" \"$pluginName\"\r\n   if ((!$pluginName) -or !(test-path \"$pluginDir\"))\r\n   {{\r\n      Write-Error $pluginNotFoundErrorMsg\r\n      return\r\n   }}\r\n\r\n   # check if the plugin is a PowerShell plugin   \r\n   $pluginFileNamePath = Join-Path \"$pluginDir\" 'FileName'\r\n   if (!(test-path \"$pluginFileNamePath\"))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n   if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   # set Initialization Parameters\r\n   $initParametersPath = Join-Path \"$pluginDir\" 'InitializationParameters'  \r\n   foreach($initParameterName in $initParametersMap)\r\n   {{         \r\n        if ($customShellObject | get-member $initParameterName)\r\n        {{\r\n            $parampath = Join-Path \"$initParametersPath\" $initParameterName\r\n            if (test-path $parampath)\r\n            {{\r\n               remove-item -path \"$parampath\"\r\n            }}\r\n                \r\n            # 0 is an accepted value for MaximumReceivedDataSizePerCommandMB and MaximumReceivedObjectSizeMB\r\n            if (($customShellObject.$initParameterName) -or ($customShellObject.$initParameterName -eq 0))\r\n            {{\r\n               new-item -path \"$initParametersPath\" -paramname $initParameterName  -paramValue \"$($customShellObject.$initParameterName)\" -Force\r\n            }}\r\n        }}\r\n   }}\r\n\r\n   # sddl processing\r\n   if ($isSddlSpecified)\r\n   {{\r\n       $resourcesPath = Join-Path \"$pluginDir\" 'Resources'\r\n       dir -literalpath \"$resourcesPath\" | % {{\r\n            $securityPath = Join-Path \"$($_.pspath)\" 'Security'\r\n            if ((@(dir -literalpath \"$securityPath\")).count -gt 0)\r\n            {{\r\n                dir -literalpath \"$securityPath\" | % {{\r\n                    $securityIDPath = \"$($_.pspath)\"\r\n                    remove-item -path \"$securityIDPath\" -recurse -force\r\n                }} #end of securityPath\r\n\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -sddl $sddl -force\r\n                }}\r\n            }}\r\n            else\r\n            {{\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -sddl $sddl -force\r\n                }}\r\n            }}\r\n       }} # end of resources\r\n   }} #end of sddl processing\r\n   elseif ($shouldShowUI)\r\n   {{\r\n        $null = winrm configsddl $resourceUri\r\n   }}\r\n}}\r\n\r\nSet-PSSessionConfiguration $args[0] $args[1] $args[2] $args[3] $args[4] $args[5] $args[6] $args[7] $args[8]\r\n";
    private static readonly ScriptBlock setPluginSb;
    private static readonly string[] initParametersMap = new string[8]
    {
      "applicationbase",
      "assemblyname",
      "pssessionconfigurationtypename",
      "startupscript",
      "psmaximumreceivedobjectsizemb",
      "psmaximumreceiveddatasizepercommandmb",
      "pssessionthreadoptions",
      "pssessionthreadapartmentstate"
    };
    private bool isErrorReported;

    static SetPSSessionConfigurationCommand() => SetPSSessionConfigurationCommand.setPluginSb = ScriptBlock.Create(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\nfunction Set-PSSessionConfiguration([PSObject]$customShellObject, \r\n     [Array]$initParametersMap,\r\n     [bool]$force,\r\n     [string]$sddl,\r\n     [bool]$isSddlSpecified,\r\n     [bool]$shouldShowUI,\r\n     [string]$resourceUri,\r\n     [string]$pluginNotFoundErrorMsg,\r\n     [string]$pluginNotPowerShellMsg)\r\n{{\r\n   $wsmanPluginDir = 'WSMan:\\localhost\\Plugin'\r\n   $pluginName = $customShellObject.Name;\r\n   $pluginDir = Join-Path \"$wsmanPluginDir\" \"$pluginName\"\r\n   if ((!$pluginName) -or !(test-path \"$pluginDir\"))\r\n   {{\r\n      Write-Error $pluginNotFoundErrorMsg\r\n      return\r\n   }}\r\n\r\n   # check if the plugin is a PowerShell plugin   \r\n   $pluginFileNamePath = Join-Path \"$pluginDir\" 'FileName'\r\n   if (!(test-path \"$pluginFileNamePath\"))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n   if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   # set Initialization Parameters\r\n   $initParametersPath = Join-Path \"$pluginDir\" 'InitializationParameters'  \r\n   foreach($initParameterName in $initParametersMap)\r\n   {{         \r\n        if ($customShellObject | get-member $initParameterName)\r\n        {{\r\n            $parampath = Join-Path \"$initParametersPath\" $initParameterName\r\n            if (test-path $parampath)\r\n            {{\r\n               remove-item -path \"$parampath\"\r\n            }}\r\n                \r\n            # 0 is an accepted value for MaximumReceivedDataSizePerCommandMB and MaximumReceivedObjectSizeMB\r\n            if (($customShellObject.$initParameterName) -or ($customShellObject.$initParameterName -eq 0))\r\n            {{\r\n               new-item -path \"$initParametersPath\" -paramname $initParameterName  -paramValue \"$($customShellObject.$initParameterName)\" -Force\r\n            }}\r\n        }}\r\n   }}\r\n\r\n   # sddl processing\r\n   if ($isSddlSpecified)\r\n   {{\r\n       $resourcesPath = Join-Path \"$pluginDir\" 'Resources'\r\n       dir -literalpath \"$resourcesPath\" | % {{\r\n            $securityPath = Join-Path \"$($_.pspath)\" 'Security'\r\n            if ((@(dir -literalpath \"$securityPath\")).count -gt 0)\r\n            {{\r\n                dir -literalpath \"$securityPath\" | % {{\r\n                    $securityIDPath = \"$($_.pspath)\"\r\n                    remove-item -path \"$securityIDPath\" -recurse -force\r\n                }} #end of securityPath\r\n\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -sddl $sddl -force\r\n                }}\r\n            }}\r\n            else\r\n            {{\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -sddl $sddl -force\r\n                }}\r\n            }}\r\n       }} # end of resources\r\n   }} #end of sddl processing\r\n   elseif ($shouldShowUI)\r\n   {{\r\n        $null = winrm configsddl $resourceUri\r\n   }}\r\n}}\r\n\r\nSet-PSSessionConfiguration $args[0] $args[1] $args[2] $args[3] $args[4] $args[5] $args[6] $args[7] $args[8]\r\n", (object) "pwrshplugin.dll"));

    protected override void BeginProcessing()
    {
      if (this.isSddlSpecified && this.showUISpecified)
        throw new PSInvalidOperationException(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "ShowUIAndSDDLCannotExist", (object) "SecurityDescriptorSddl", (object) "ShowSecurityDescriptorUI"));
      RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
      PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
    }

    protected override void ProcessRecord()
    {
      this.WriteVerbose(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "ScsScriptMessageV", (object) "\r\nfunction Set-PSSessionConfiguration([PSObject]$customShellObject, \r\n     [Array]$initParametersMap,\r\n     [bool]$force,\r\n     [string]$sddl,\r\n     [bool]$isSddlSpecified,\r\n     [bool]$shouldShowUI,\r\n     [string]$resourceUri,\r\n     [string]$pluginNotFoundErrorMsg,\r\n     [string]$pluginNotPowerShellMsg)\r\n{{\r\n   $wsmanPluginDir = 'WSMan:\\localhost\\Plugin'\r\n   $pluginName = $customShellObject.Name;\r\n   $pluginDir = Join-Path \"$wsmanPluginDir\" \"$pluginName\"\r\n   if ((!$pluginName) -or !(test-path \"$pluginDir\"))\r\n   {{\r\n      Write-Error $pluginNotFoundErrorMsg\r\n      return\r\n   }}\r\n\r\n   # check if the plugin is a PowerShell plugin   \r\n   $pluginFileNamePath = Join-Path \"$pluginDir\" 'FileName'\r\n   if (!(test-path \"$pluginFileNamePath\"))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   $pluginFileName = get-item -literalpath \"$pluginFileNamePath\"\r\n   if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))\r\n   {{\r\n      Write-Error $pluginNotPowerShellMsg\r\n      return\r\n   }}\r\n\r\n   # set Initialization Parameters\r\n   $initParametersPath = Join-Path \"$pluginDir\" 'InitializationParameters'  \r\n   foreach($initParameterName in $initParametersMap)\r\n   {{         \r\n        if ($customShellObject | get-member $initParameterName)\r\n        {{\r\n            $parampath = Join-Path \"$initParametersPath\" $initParameterName\r\n            if (test-path $parampath)\r\n            {{\r\n               remove-item -path \"$parampath\"\r\n            }}\r\n                \r\n            # 0 is an accepted value for MaximumReceivedDataSizePerCommandMB and MaximumReceivedObjectSizeMB\r\n            if (($customShellObject.$initParameterName) -or ($customShellObject.$initParameterName -eq 0))\r\n            {{\r\n               new-item -path \"$initParametersPath\" -paramname $initParameterName  -paramValue \"$($customShellObject.$initParameterName)\" -Force\r\n            }}\r\n        }}\r\n   }}\r\n\r\n   # sddl processing\r\n   if ($isSddlSpecified)\r\n   {{\r\n       $resourcesPath = Join-Path \"$pluginDir\" 'Resources'\r\n       dir -literalpath \"$resourcesPath\" | % {{\r\n            $securityPath = Join-Path \"$($_.pspath)\" 'Security'\r\n            if ((@(dir -literalpath \"$securityPath\")).count -gt 0)\r\n            {{\r\n                dir -literalpath \"$securityPath\" | % {{\r\n                    $securityIDPath = \"$($_.pspath)\"\r\n                    remove-item -path \"$securityIDPath\" -recurse -force\r\n                }} #end of securityPath\r\n\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -sddl $sddl -force\r\n                }}\r\n            }}\r\n            else\r\n            {{\r\n                if ($sddl)\r\n                {{\r\n                    new-item -path \"$securityPath\" -sddl $sddl -force\r\n                }}\r\n            }}\r\n       }} # end of resources\r\n   }} #end of sddl processing\r\n   elseif ($shouldShowUI)\r\n   {{\r\n        $null = winrm configsddl $resourceUri\r\n   }}\r\n}}\r\n\r\nSet-PSSessionConfiguration $args[0] $args[1] $args[2] $args[3] $args[4] $args[5] $args[6] $args[7] $args[8]\r\n"));
      string action = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "CSShouldProcessAction", (object) this.CommandInfo.Name);
      string target;
      if (!this.isSddlSpecified)
        target = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "CSShouldProcessTarget", (object) this.Name);
      else
        target = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "ScsShouldProcessTargetSDDL", (object) this.Name, (object) this.sddl);
      if (!this.force && !this.ShouldProcess(target, action))
        return;
      string str1 = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "CSCmdsShellNotFound", (object) this.shellName);
      string str2 = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "CSCmdsShellNotPowerShellBased", (object) this.shellName);
      PSObject psObject = this.ConstructPropertiesForUpdate();
      int count = ((ArrayList) this.Context.DollarErrorVariable).Count;
      SetPSSessionConfigurationCommand.setPluginSb.InvokeUsingCmdlet((Cmdlet) this, true, true, (object) AutomationNull.Value, (object) new object[0], (object) AutomationNull.Value, (object) psObject, (object) SetPSSessionConfigurationCommand.initParametersMap, (object) this.force, (object) this.sddl, (object) this.isSddlSpecified, (object) this.ShowSecurityDescriptorUI.ToBool(), (object) ("http://schemas.microsoft.com/powershell/" + this.shellName), (object) str1, (object) str2);
      this.isErrorReported = ((ArrayList) this.Context.DollarErrorVariable).Count > count;
    }

    protected override void EndProcessing() => PSSessionConfigurationCommandUtilities.RestartWinRMService((PSCmdlet) this, this.isErrorReported, (bool) this.Force, this.noRestart);

    private PSObject ConstructPropertiesForUpdate()
    {
      PSObject psObject = new PSObject();
      psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Name", (object) this.shellName));
      if (this.isAssemblyNameSpecified)
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("assemblyname", (object) this.assemblyName));
      if (this.isApplicationBaseSpecified)
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("applicationbase", (object) this.applicationBase));
      if (this.isConfigurationTypeNameSpecified)
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("pssessionconfigurationtypename", (object) this.configurationTypeName));
      if (this.isConfigurationScriptSpecified)
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("startupscript", (object) this.configurationScript));
      if (this.isMaxCommandSizeMBSpecified)
      {
        object obj = this.maxCommandSizeMB.HasValue ? (object) this.maxCommandSizeMB.Value : (object) null;
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("psmaximumreceiveddatasizepercommandmb", obj));
      }
      if (this.isMaxObjectSizeMBSpecified)
      {
        object obj = this.maxObjectSizeMB.HasValue ? (object) this.maxObjectSizeMB.Value : (object) null;
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("psmaximumreceivedobjectsizemb", obj));
      }
      if (this.threadAptState.HasValue)
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("pssessionthreadapartmentstate", (object) this.threadAptState.Value));
      if (this.threadOptions.HasValue)
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("pssessionthreadoptions", (object) this.threadOptions.Value));
      return psObject;
    }
  }
}
