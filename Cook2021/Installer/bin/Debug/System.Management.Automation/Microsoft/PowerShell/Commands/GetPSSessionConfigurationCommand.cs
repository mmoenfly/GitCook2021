// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.GetPSSessionConfigurationCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Get", "PSSessionConfiguration")]
  public sealed class GetPSSessionConfigurationCommand : PSCmdlet
  {
    private const string getPluginSbFormat = "\r\nfunction ExtractPluginProperties([string]$pluginDir, \r\n   $objectToWriteTo)\r\n{{\r\n  # since we are directly acting on plugin name..no need to perform wildcard search\r\n  foreach($element in (dir -literalpath \"$pluginDir\"))\r\n  {{\r\n    if (!$element.PSIsContainer)\r\n    {{\r\n      if ($element.Name -eq 'sddl')\r\n      {{         \r\n         $objectToWriteTo = $objectToWriteTo | add-member -membertype noteproperty -name 'SecurityDescriptorSddl' -value $element.Value -force -passthru\r\n      }}\r\n      else\r\n      {{\r\n        $objectToWriteTo = $objectToWriteTo | add-member -membertype noteproperty -name $element.Name -value $element.Value -force -passthru\r\n      }}  \t  \r\n    }}\r\n    else\r\n    {{\r\n       $temp = Join-Path \"$pluginDir\" \"$($element.Name)\"\r\n       ExtractPluginProperties  $temp $objectToWriteTo\r\n    }}   \r\n  }}\r\n}}\r\n\r\n$shellNotErrMsgFormat = $args[1]\r\n$args[0] | foreach {{\r\n  $shellsFound = 0;\r\n  $filter = $_\r\n  dir 'WSMan:\\localhost\\Plugin\\' | ? {{ $_.name -like \"$filter\" }} | foreach {{\r\n     $customPluginObject = new-object object     \r\n     $customPluginObject.pstypenames.Insert(0, '{0}')\r\n     ExtractPluginProperties \"$($_.PSPath)\" $customPluginObject\r\n     # this is powershell based custom shell only if its plugin dll is pwrshplugin.dll\r\n     if (($customPluginObject.FileName) -and ($customPluginObject.FileName -match '{1}'))\r\n     {{\r\n           $shellsFound++\r\n           $customPluginObject\r\n     }}\r\n    }} # end of foreach\r\n   \r\n    if (!$shellsFound -and !([System.Management.Automation.WildcardPattern]::ContainsWildcardCharacters($_)))\r\n    {{\r\n      $errMsg = $shellNotErrMsgFormat -f $_\r\n      Write-Error $errMsg \r\n    }}     \r\n  }}\r\n";
    private static readonly ScriptBlock getPluginSb = ScriptBlock.Create(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\nfunction ExtractPluginProperties([string]$pluginDir, \r\n   $objectToWriteTo)\r\n{{\r\n  # since we are directly acting on plugin name..no need to perform wildcard search\r\n  foreach($element in (dir -literalpath \"$pluginDir\"))\r\n  {{\r\n    if (!$element.PSIsContainer)\r\n    {{\r\n      if ($element.Name -eq 'sddl')\r\n      {{         \r\n         $objectToWriteTo = $objectToWriteTo | add-member -membertype noteproperty -name 'SecurityDescriptorSddl' -value $element.Value -force -passthru\r\n      }}\r\n      else\r\n      {{\r\n        $objectToWriteTo = $objectToWriteTo | add-member -membertype noteproperty -name $element.Name -value $element.Value -force -passthru\r\n      }}  \t  \r\n    }}\r\n    else\r\n    {{\r\n       $temp = Join-Path \"$pluginDir\" \"$($element.Name)\"\r\n       ExtractPluginProperties  $temp $objectToWriteTo\r\n    }}   \r\n  }}\r\n}}\r\n\r\n$shellNotErrMsgFormat = $args[1]\r\n$args[0] | foreach {{\r\n  $shellsFound = 0;\r\n  $filter = $_\r\n  dir 'WSMan:\\localhost\\Plugin\\' | ? {{ $_.name -like \"$filter\" }} | foreach {{\r\n     $customPluginObject = new-object object     \r\n     $customPluginObject.pstypenames.Insert(0, '{0}')\r\n     ExtractPluginProperties \"$($_.PSPath)\" $customPluginObject\r\n     # this is powershell based custom shell only if its plugin dll is pwrshplugin.dll\r\n     if (($customPluginObject.FileName) -and ($customPluginObject.FileName -match '{1}'))\r\n     {{\r\n           $shellsFound++\r\n           $customPluginObject\r\n     }}\r\n    }} # end of foreach\r\n   \r\n    if (!$shellsFound -and !([System.Management.Automation.WildcardPattern]::ContainsWildcardCharacters($_)))\r\n    {{\r\n      $errMsg = $shellNotErrMsgFormat -f $_\r\n      Write-Error $errMsg \r\n    }}     \r\n  }}\r\n", (object) "Microsoft.PowerShell.Commands.PSSessionConfigurationCommands#PSSessionConfiguration", (object) "pwrshplugin.dll"));
    private string[] shellName;

    [Parameter(Mandatory = false, Position = 0)]
    public string[] Name
    {
      get => this.shellName;
      set => this.shellName = value;
    }

    protected override void BeginProcessing()
    {
      RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
      PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
    }

    protected override void ProcessRecord()
    {
      this.WriteVerbose(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "GcsScriptMessageV", (object) "\r\nfunction ExtractPluginProperties([string]$pluginDir, \r\n   $objectToWriteTo)\r\n{{\r\n  # since we are directly acting on plugin name..no need to perform wildcard search\r\n  foreach($element in (dir -literalpath \"$pluginDir\"))\r\n  {{\r\n    if (!$element.PSIsContainer)\r\n    {{\r\n      if ($element.Name -eq 'sddl')\r\n      {{         \r\n         $objectToWriteTo = $objectToWriteTo | add-member -membertype noteproperty -name 'SecurityDescriptorSddl' -value $element.Value -force -passthru\r\n      }}\r\n      else\r\n      {{\r\n        $objectToWriteTo = $objectToWriteTo | add-member -membertype noteproperty -name $element.Name -value $element.Value -force -passthru\r\n      }}  \t  \r\n    }}\r\n    else\r\n    {{\r\n       $temp = Join-Path \"$pluginDir\" \"$($element.Name)\"\r\n       ExtractPluginProperties  $temp $objectToWriteTo\r\n    }}   \r\n  }}\r\n}}\r\n\r\n$shellNotErrMsgFormat = $args[1]\r\n$args[0] | foreach {{\r\n  $shellsFound = 0;\r\n  $filter = $_\r\n  dir 'WSMan:\\localhost\\Plugin\\' | ? {{ $_.name -like \"$filter\" }} | foreach {{\r\n     $customPluginObject = new-object object     \r\n     $customPluginObject.pstypenames.Insert(0, '{0}')\r\n     ExtractPluginProperties \"$($_.PSPath)\" $customPluginObject\r\n     # this is powershell based custom shell only if its plugin dll is pwrshplugin.dll\r\n     if (($customPluginObject.FileName) -and ($customPluginObject.FileName -match '{1}'))\r\n     {{\r\n           $shellsFound++\r\n           $customPluginObject\r\n     }}\r\n    }} # end of foreach\r\n   \r\n    if (!$shellsFound -and !([System.Management.Automation.WildcardPattern]::ContainsWildcardCharacters($_)))\r\n    {{\r\n      $errMsg = $shellNotErrMsgFormat -f $_\r\n      Write-Error $errMsg \r\n    }}     \r\n  }}\r\n"));
      string resourceString = ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "CustomShellNotFound");
      object obj = (object) "*";
      if (this.shellName != null)
        obj = (object) this.shellName;
      GetPSSessionConfigurationCommand.getPluginSb.InvokeUsingCmdlet((Cmdlet) this, true, true, (object) AutomationNull.Value, (object) new object[0], (object) AutomationNull.Value, obj, (object) resourceString);
    }
  }
}
