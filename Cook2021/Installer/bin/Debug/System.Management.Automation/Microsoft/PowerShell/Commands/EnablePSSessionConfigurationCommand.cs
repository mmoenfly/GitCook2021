// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.EnablePSSessionConfigurationCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Security.AccessControl;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Enable", "PSSessionConfiguration", ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
  public sealed class EnablePSSessionConfigurationCommand : PSCmdlet
  {
    private const string setWSManConfigCommand = "Set-WSManQuickConfig";
    private const string enablePluginSbFormat = "\r\nfunction Enable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $sddl,\r\n    \r\n    [Parameter()]\r\n    [bool]\r\n    $isSDDLSpecified,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $queryForSet,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForSet,\r\n        \r\n    [Parameter()]\r\n    [string]\r\n    $queryForQC,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForQC,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $shouldProcessDescForQC\r\n    )\r\n     \r\n    begin\r\n    {{    \r\n        if ($force -or $pscmdlet.ShouldProcess($shouldProcessDescForQC, $queryForQC, $captionForQC))\r\n        {{\r\n            # get the status of winrm before Quick Config. if it is already\r\n            # running..restart the service after Quick Config.\r\n            $svc = get-service winrm\r\n            {0} -force\r\n            if ($svc.Status -match \"Running\")\r\n            {{\r\n               Restart-Service winrm -force -confirm:$false\r\n            }}\r\n        }}       \r\n    }} #end of Begin block   \r\n        \r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n          \r\n          if (!$isSDDLSpecified)\r\n          {{\r\n             $sddlTemp = $_.SecurityDescriptorSddl\r\n             $securityIdentifierToPurge = $null\r\n             # strip out Disable-Everyone DACL from the SDDL\r\n             if ($sddlTemp)\r\n             {{\r\n                # construct SID for \"EveryOne\"\r\n                [system.security.principal.wellknownsidtype]$evst = \"worldsid\"\r\n                $everyOneSID = new-object system.security.principal.securityidentifier $evst,$null\r\n                                \r\n                $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddlTemp                \r\n                $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $everyOneSID))\r\n                    {{\r\n                       $securityIdentifierToPurge = $_.securityidentifier                       \r\n                    }}\r\n                }}\r\n             \r\n                if ($securityIdentifierToPurge)\r\n                {{\r\n                   $sd.discretionaryacl.purge($securityIdentifierToPurge)\r\n\r\n                   # if there is no discretionaryacl..add Builtin Administrators\r\n                   # to the DACL group as this is the default WSMan behavior\r\n                   if ($sd.discretionaryacl.count -eq 0)\r\n                   {{\r\n                      [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                      $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                      $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n                   }}\r\n\r\n                   $sddl = $sd.GetSddlForm(\"all\")\r\n                }}\r\n             }} # if ($sddlTemp)\r\n          }} # if (!$isSDDLSpecified) \r\n          \r\n          $qMessage = $queryForSet -f $_.name,$sddl\r\n          if (($sddl -or $isSDDLSpecified) -and ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForSet)))\r\n          {{\r\n              $null = Set-PSSessionConfiguration -Name $_.Name -SecurityDescriptorSddl $sddl -NoServiceRestart -force\r\n          }}\r\n       }} #end of Get-PSSessionConfiguration | foreach\r\n    }} # end of Process block\r\n}}\r\n\r\n$_ | Enable-PSSessionConfiguration -force $args[0] -sddl $args[1] -isSDDLSpecified $args[2] -queryForSet $args[3] -captionForSet $args[4] -queryForQC $args[5] -captionForQC $args[6] -whatif:$args[7] -confirm:$args[8] -shouldProcessDescForQC $args[9]\r\n";
    private static ScriptBlock enablePluginSb = ScriptBlock.Create(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\nfunction Enable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $sddl,\r\n    \r\n    [Parameter()]\r\n    [bool]\r\n    $isSDDLSpecified,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $queryForSet,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForSet,\r\n        \r\n    [Parameter()]\r\n    [string]\r\n    $queryForQC,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForQC,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $shouldProcessDescForQC\r\n    )\r\n     \r\n    begin\r\n    {{    \r\n        if ($force -or $pscmdlet.ShouldProcess($shouldProcessDescForQC, $queryForQC, $captionForQC))\r\n        {{\r\n            # get the status of winrm before Quick Config. if it is already\r\n            # running..restart the service after Quick Config.\r\n            $svc = get-service winrm\r\n            {0} -force\r\n            if ($svc.Status -match \"Running\")\r\n            {{\r\n               Restart-Service winrm -force -confirm:$false\r\n            }}\r\n        }}       \r\n    }} #end of Begin block   \r\n        \r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n          \r\n          if (!$isSDDLSpecified)\r\n          {{\r\n             $sddlTemp = $_.SecurityDescriptorSddl\r\n             $securityIdentifierToPurge = $null\r\n             # strip out Disable-Everyone DACL from the SDDL\r\n             if ($sddlTemp)\r\n             {{\r\n                # construct SID for \"EveryOne\"\r\n                [system.security.principal.wellknownsidtype]$evst = \"worldsid\"\r\n                $everyOneSID = new-object system.security.principal.securityidentifier $evst,$null\r\n                                \r\n                $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddlTemp                \r\n                $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $everyOneSID))\r\n                    {{\r\n                       $securityIdentifierToPurge = $_.securityidentifier                       \r\n                    }}\r\n                }}\r\n             \r\n                if ($securityIdentifierToPurge)\r\n                {{\r\n                   $sd.discretionaryacl.purge($securityIdentifierToPurge)\r\n\r\n                   # if there is no discretionaryacl..add Builtin Administrators\r\n                   # to the DACL group as this is the default WSMan behavior\r\n                   if ($sd.discretionaryacl.count -eq 0)\r\n                   {{\r\n                      [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                      $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                      $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n                   }}\r\n\r\n                   $sddl = $sd.GetSddlForm(\"all\")\r\n                }}\r\n             }} # if ($sddlTemp)\r\n          }} # if (!$isSDDLSpecified) \r\n          \r\n          $qMessage = $queryForSet -f $_.name,$sddl\r\n          if (($sddl -or $isSDDLSpecified) -and ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForSet)))\r\n          {{\r\n              $null = Set-PSSessionConfiguration -Name $_.Name -SecurityDescriptorSddl $sddl -NoServiceRestart -force\r\n          }}\r\n       }} #end of Get-PSSessionConfiguration | foreach\r\n    }} # end of Process block\r\n}}\r\n\r\n$_ | Enable-PSSessionConfiguration -force $args[0] -sddl $args[1] -isSDDLSpecified $args[2] -queryForSet $args[3] -captionForSet $args[4] -queryForQC $args[5] -captionForQC $args[6] -whatif:$args[7] -confirm:$args[8] -shouldProcessDescForQC $args[9]\r\n", (object) "Set-WSManQuickConfig"));
    private string[] shellName;
    private Collection<string> shellsToEnable = new Collection<string>();
    private bool force;
    internal string sddl;
    internal bool isSddlSpecified;

    [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
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

    [Parameter]
    public string SecurityDescriptorSddl
    {
      get => this.sddl;
      set
      {
        this.sddl = string.IsNullOrEmpty(value) || new CommonSecurityDescriptor(false, false, value) != null ? value : throw new NotSupportedException();
        this.isSddlSpecified = true;
      }
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
        this.shellsToEnable.Add(str);
    }

    protected override void EndProcessing()
    {
      if (this.shellsToEnable.Count == 0)
        this.shellsToEnable.Add("Microsoft.PowerShell");
      this.WriteVerbose(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "EcsScriptMessageV", (object) "\r\nfunction Enable-PSSessionConfiguration\r\n{{\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter(Position=0, ValueFromPipeline=$true)]\r\n    [System.String]\r\n    $Name,\r\n\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $sddl,\r\n    \r\n    [Parameter()]\r\n    [bool]\r\n    $isSDDLSpecified,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $queryForSet,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForSet,\r\n        \r\n    [Parameter()]\r\n    [string]\r\n    $queryForQC,\r\n    \r\n    [Parameter()]\r\n    [string]\r\n    $captionForQC,\r\n\r\n    [Parameter()]\r\n    [string]\r\n    $shouldProcessDescForQC\r\n    )\r\n     \r\n    begin\r\n    {{    \r\n        if ($force -or $pscmdlet.ShouldProcess($shouldProcessDescForQC, $queryForQC, $captionForQC))\r\n        {{\r\n            # get the status of winrm before Quick Config. if it is already\r\n            # running..restart the service after Quick Config.\r\n            $svc = get-service winrm\r\n            {0} -force\r\n            if ($svc.Status -match \"Running\")\r\n            {{\r\n               Restart-Service winrm -force -confirm:$false\r\n            }}\r\n        }}       \r\n    }} #end of Begin block   \r\n        \r\n    process\r\n    {{\r\n       Get-PSSessionConfiguration $name | % {{\r\n          \r\n          if (!$isSDDLSpecified)\r\n          {{\r\n             $sddlTemp = $_.SecurityDescriptorSddl\r\n             $securityIdentifierToPurge = $null\r\n             # strip out Disable-Everyone DACL from the SDDL\r\n             if ($sddlTemp)\r\n             {{\r\n                # construct SID for \"EveryOne\"\r\n                [system.security.principal.wellknownsidtype]$evst = \"worldsid\"\r\n                $everyOneSID = new-object system.security.principal.securityidentifier $evst,$null\r\n                                \r\n                $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddlTemp                \r\n                $sd.DiscretionaryAcl | % {{\r\n                    if (($_.acequalifier -eq \"accessdenied\") -and ($_.securityidentifier -match $everyOneSID))\r\n                    {{\r\n                       $securityIdentifierToPurge = $_.securityidentifier                       \r\n                    }}\r\n                }}\r\n             \r\n                if ($securityIdentifierToPurge)\r\n                {{\r\n                   $sd.discretionaryacl.purge($securityIdentifierToPurge)\r\n\r\n                   # if there is no discretionaryacl..add Builtin Administrators\r\n                   # to the DACL group as this is the default WSMan behavior\r\n                   if ($sd.discretionaryacl.count -eq 0)\r\n                   {{\r\n                      [system.security.principal.wellknownsidtype]$bast = \"BuiltinAdministratorsSid\"\r\n                      $basid = new-object system.security.principal.securityidentifier $bast,$null\r\n                      $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')\r\n                   }}\r\n\r\n                   $sddl = $sd.GetSddlForm(\"all\")\r\n                }}\r\n             }} # if ($sddlTemp)\r\n          }} # if (!$isSDDLSpecified) \r\n          \r\n          $qMessage = $queryForSet -f $_.name,$sddl\r\n          if (($sddl -or $isSDDLSpecified) -and ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForSet)))\r\n          {{\r\n              $null = Set-PSSessionConfiguration -Name $_.Name -SecurityDescriptorSddl $sddl -NoServiceRestart -force\r\n          }}\r\n       }} #end of Get-PSSessionConfiguration | foreach\r\n    }} # end of Process block\r\n}}\r\n\r\n$_ | Enable-PSSessionConfiguration -force $args[0] -sddl $args[1] -isSDDLSpecified $args[2] -queryForSet $args[3] -captionForSet $args[4] -queryForQC $args[5] -captionForQC $args[6] -whatif:$args[7] -confirm:$args[8] -shouldProcessDescForQC $args[9]\r\n"));
      bool whatIf = false;
      bool confirm = true;
      PSSessionConfigurationCommandUtilities.CollectShouldProcessParameters((PSCmdlet) this, out whatIf, out confirm);
      string str1 = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "EcsWSManQCCaption");
      string str2 = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "EcsWSManQCQuery", (object) "Set-WSManQuickConfig");
      string str3 = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "EcsWSManShouldProcessDesc", (object) "Set-WSManQuickConfig");
      string str4 = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "CSShouldProcessAction", (object) "Set-PSSessionConfiguration");
      string resourceString = ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "EcsShouldProcessTarget");
      EnablePSSessionConfigurationCommand.enablePluginSb.InvokeUsingCmdlet((Cmdlet) this, true, true, (object) this.shellsToEnable, (object) new object[0], (object) AutomationNull.Value, (object) this.force, (object) this.sddl, (object) this.isSddlSpecified, (object) resourceString, (object) str4, (object) str2, (object) str1, (object) whatIf, (object) confirm, (object) str3);
    }
  }
}
