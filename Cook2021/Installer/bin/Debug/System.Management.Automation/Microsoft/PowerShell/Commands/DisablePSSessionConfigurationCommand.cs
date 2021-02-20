// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.DisablePSSessionConfigurationCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Disable", "PSSessionConfiguration", ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
  public sealed class DisablePSSessionConfigurationCommand : PSCmdlet
  {
    private const string disablePluginSbFormat = "\r\nfunction Disable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $queryForSet,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForSet)\r\n    \r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n           \r\n           $sddl = $_.SecurityDescriptorSddl\r\n           if (!$sddl)\r\n           {{\r\n               # Disable EveryOne from accessing this session configuration\r\n               $sddl = \"O:NSG:BAD:P(D;;GA;;;WD)S:P\"\r\n           }}\r\n           else\r\n           {{           \r\n              # construct SID for \"EveryOne\"\r\n              [system.security.principal.wellknownsidtype]$evst = \"worldsid\"\r\n              $everyOneSID = new-object system.security.principal.securityidentifier $evst,$null\r\n              \r\n              # add disable everyone to the existing sddl              \r\n              $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddl    \r\n              $disableEveryOneExists = $false            \r\n              $sd.DiscretionaryAcl | % {{\r\n                 if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $everyOneSID))\r\n                 {{\r\n                    $disableEveryOneExists = $true              \r\n                 }}\r\n              }}\r\n              \r\n              if (!$disableEveryOneExists)\r\n              {{\r\n                 $sd.DiscretionaryAcl.AddAccess(\"deny\", $everyOneSID, 268435456, \"None\", \"None\")\r\n                 $sddl = $sd.GetSddlForm(\"all\")\r\n              }} #end if (!$disableEveryOneExists)   \r\n              else\r\n              {{                   \r\n                    # since disable everyone already exists..we dont need to change anything.\r\n                    $sddl = $null      \r\n              }}           \r\n           }} #end of if (!$sddl) \r\n           \r\n           $qMessage = $queryForSet -f $_.name,$sddl\r\n           if (($sddl) -and ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForSet)))\r\n           {{\r\n              $null = Set-PSSessionConfiguration -Name $_.Name -SecurityDescriptorSddl $sddl -NoServiceRestart -force\r\n           }}\r\n       }} # end of foreach block\r\n    }} #end of process block\r\n}}\r\n\r\n$_ | Disable-PSSessionConfiguration -force $args[0] -queryForSet $args[1] -captionForSet $args[2] -whatif:$args[3] -confirm:$args[4]\r\n";
    private static ScriptBlock disablePluginSb = ScriptBlock.Create(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\nfunction Disable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $queryForSet,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForSet)\r\n    \r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n           \r\n           $sddl = $_.SecurityDescriptorSddl\r\n           if (!$sddl)\r\n           {{\r\n               # Disable EveryOne from accessing this session configuration\r\n               $sddl = \"O:NSG:BAD:P(D;;GA;;;WD)S:P\"\r\n           }}\r\n           else\r\n           {{           \r\n              # construct SID for \"EveryOne\"\r\n              [system.security.principal.wellknownsidtype]$evst = \"worldsid\"\r\n              $everyOneSID = new-object system.security.principal.securityidentifier $evst,$null\r\n              \r\n              # add disable everyone to the existing sddl              \r\n              $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddl    \r\n              $disableEveryOneExists = $false            \r\n              $sd.DiscretionaryAcl | % {{\r\n                 if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $everyOneSID))\r\n                 {{\r\n                    $disableEveryOneExists = $true              \r\n                 }}\r\n              }}\r\n              \r\n              if (!$disableEveryOneExists)\r\n              {{\r\n                 $sd.DiscretionaryAcl.AddAccess(\"deny\", $everyOneSID, 268435456, \"None\", \"None\")\r\n                 $sddl = $sd.GetSddlForm(\"all\")\r\n              }} #end if (!$disableEveryOneExists)   \r\n              else\r\n              {{                   \r\n                    # since disable everyone already exists..we dont need to change anything.\r\n                    $sddl = $null      \r\n              }}           \r\n           }} #end of if (!$sddl) \r\n           \r\n           $qMessage = $queryForSet -f $_.name,$sddl\r\n           if (($sddl) -and ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForSet)))\r\n           {{\r\n              $null = Set-PSSessionConfiguration -Name $_.Name -SecurityDescriptorSddl $sddl -NoServiceRestart -force\r\n           }}\r\n       }} # end of foreach block\r\n    }} #end of process block\r\n}}\r\n\r\n$_ | Disable-PSSessionConfiguration -force $args[0] -queryForSet $args[1] -captionForSet $args[2] -whatif:$args[3] -confirm:$args[4]\r\n"));
    private string[] shellName;
    private Collection<string> shellsToDisable = new Collection<string>();
    private bool force;

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string[] Name
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

    protected override void BeginProcessing()
    {
      RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
      PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
    }

    protected override void ProcessRecord()
    {
      if (this.shellName == null)
        return;
      foreach (string str in this.shellName)
        this.shellsToDisable.Add(str);
    }

    protected override void EndProcessing()
    {
      if (this.shellsToDisable.Count == 0)
        this.shellsToDisable.Add("Microsoft.PowerShell");
      this.WriteWarning(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "DcsWarningMessage"));
      this.WriteVerbose(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "EcsScriptMessageV", (object) "\r\nfunction Disable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $queryForSet,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForSet)\r\n    \r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n           \r\n           $sddl = $_.SecurityDescriptorSddl\r\n           if (!$sddl)\r\n           {{\r\n               # Disable EveryOne from accessing this session configuration\r\n               $sddl = \"O:NSG:BAD:P(D;;GA;;;WD)S:P\"\r\n           }}\r\n           else\r\n           {{           \r\n              # construct SID for \"EveryOne\"\r\n              [system.security.principal.wellknownsidtype]$evst = \"worldsid\"\r\n              $everyOneSID = new-object system.security.principal.securityidentifier $evst,$null\r\n              \r\n              # add disable everyone to the existing sddl              \r\n              $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddl    \r\n              $disableEveryOneExists = $false            \r\n              $sd.DiscretionaryAcl | % {{\r\n                 if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $everyOneSID))\r\n                 {{\r\n                    $disableEveryOneExists = $true              \r\n                 }}\r\n              }}\r\n              \r\n              if (!$disableEveryOneExists)\r\n              {{\r\n                 $sd.DiscretionaryAcl.AddAccess(\"deny\", $everyOneSID, 268435456, \"None\", \"None\")\r\n                 $sddl = $sd.GetSddlForm(\"all\")\r\n              }} #end if (!$disableEveryOneExists)   \r\n              else\r\n              {{                   \r\n                    # since disable everyone already exists..we dont need to change anything.\r\n                    $sddl = $null      \r\n              }}           \r\n           }} #end of if (!$sddl) \r\n           \r\n           $qMessage = $queryForSet -f $_.name,$sddl\r\n           if (($sddl) -and ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForSet)))\r\n           {{\r\n              $null = Set-PSSessionConfiguration -Name $_.Name -SecurityDescriptorSddl $sddl -NoServiceRestart -force\r\n           }}\r\n       }} # end of foreach block\r\n    }} #end of process block\r\n}}\r\n\r\n$_ | Disable-PSSessionConfiguration -force $args[0] -queryForSet $args[1] -captionForSet $args[2] -whatif:$args[3] -confirm:$args[4]\r\n"));
      bool whatIf = false;
      bool confirm = true;
      PSSessionConfigurationCommandUtilities.CollectShouldProcessParameters((PSCmdlet) this, out whatIf, out confirm);
      string str = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "CSShouldProcessAction", (object) "Set-PSSessionConfiguration");
      string resourceString = ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "DcsShouldProcessTarget");
      DisablePSSessionConfigurationCommand.disablePluginSb.InvokeUsingCmdlet((Cmdlet) this, true, true, (object) this.shellsToDisable, (object) new object[0], (object) AutomationNull.Value, (object) this.force, (object) resourceString, (object) str, (object) whatIf, (object) confirm);
    }
  }
}
