// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.RegisterPSSessionConfigurationCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Security;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Register", "PSSessionConfiguration", ConfirmImpact = ConfirmImpact.High, DefaultParameterSetName = "__AllParameterSets", SupportsShouldProcess = true)]
  public sealed class RegisterPSSessionConfigurationCommand : PSSessionConfigurationCommandBase
  {
    private const string newPluginSbFormat = "\r\nfunction Register-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(  \r\n      [string]$filepath,\r\n      [string]$pluginName,\r\n      [bool]$shouldShowUI,\r\n      [bool]$force,\r\n      [bool]$noServiceRestart,\r\n      [string]$restartWSManTarget,\r\n      [string]$restartWSManAction,\r\n      [string]$restartWSManRequired\r\n    )\r\n\r\n    process\r\n    {{\r\n        new-item -path WSMan:\\localhost\\Plugin -file \"$filepath\" -name \"$pluginName\"\r\n        # $? is to make sure the last operation is succeeded\r\n        if ($? -and $shouldShowUI)\r\n        {{\r\n           if ($noServiceRestart)\r\n           {{\r\n               write-error $restartWSManRequired\r\n               return\r\n           }}\r\n\r\n           if ($force -or $pscmdlet.shouldprocess($restartWSManTarget, $restartWSManAction))\r\n           {{\r\n               restart-service winrm -force\r\n               $null = winrm configsddl \"{0}$pluginName\"\r\n           }}\r\n           else\r\n           {{\r\n               write-error $restartWSManRequired\r\n           }}\r\n        }}\r\n    }}\r\n}}\r\n\r\nRegister-PSSessionConfiguration -filepath $args[0] -pluginName $args[1] -shouldShowUI $args[2] -force $args[3] -noServiceRestart $args[4] -whatif:$args[5] -confirm:$args[6] -restartWSManTarget $args[7] -restartWSManAction $args[8] -restartWSManRequired $args[9]\r\n";
    private const string pluginXmlFormat = "\r\n<PlugInConfiguration xmlns='http://schemas.microsoft.com/wbem/wsman/1/config/PluginConfiguration'\r\n                     Name='{0}'\r\n                     Filename='%windir%\\system32\\{1}'\r\n                     SDKVersion='1'\r\n                     XmlRenderingType='text' {2} >\r\n  <InitializationParameters>\r\n    <Param Name='PSVersion'  Value='2.0' />\r\n{3}\r\n  </InitializationParameters> \r\n  <Resources>\r\n    <Resource ResourceUri='{4}' SupportsOptions='true' ExactMatch='true'>\r\n{5}\r\n      <Capability Type='Shell' />\r\n    </Resource>\r\n  </Resources>\r\n</PlugInConfiguration>\r\n";
    private const string architectureAttribFormat = "\r\n                     Architecture='{0}'";
    private const string initParamFormat = "\r\n    <Param Name='{0}'  Value='{1}' />{2}\r\n";
    private const string securityElementFormat = "\r\n      <Security Uri='{0}' ExactMatch='true' Sddl='{1}'>\r\n      </Security>\r\n";
    private static readonly ScriptBlock newPluginSb = ScriptBlock.Create(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\nfunction Register-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(  \r\n      [string]$filepath,\r\n      [string]$pluginName,\r\n      [bool]$shouldShowUI,\r\n      [bool]$force,\r\n      [bool]$noServiceRestart,\r\n      [string]$restartWSManTarget,\r\n      [string]$restartWSManAction,\r\n      [string]$restartWSManRequired\r\n    )\r\n\r\n    process\r\n    {{\r\n        new-item -path WSMan:\\localhost\\Plugin -file \"$filepath\" -name \"$pluginName\"\r\n        # $? is to make sure the last operation is succeeded\r\n        if ($? -and $shouldShowUI)\r\n        {{\r\n           if ($noServiceRestart)\r\n           {{\r\n               write-error $restartWSManRequired\r\n               return\r\n           }}\r\n\r\n           if ($force -or $pscmdlet.shouldprocess($restartWSManTarget, $restartWSManAction))\r\n           {{\r\n               restart-service winrm -force\r\n               $null = winrm configsddl \"{0}$pluginName\"\r\n           }}\r\n           else\r\n           {{\r\n               write-error $restartWSManRequired\r\n           }}\r\n        }}\r\n    }}\r\n}}\r\n\r\nRegister-PSSessionConfiguration -filepath $args[0] -pluginName $args[1] -shouldShowUI $args[2] -force $args[3] -noServiceRestart $args[4] -whatif:$args[5] -confirm:$args[6] -restartWSManTarget $args[7] -restartWSManAction $args[8] -restartWSManRequired $args[9]\r\n", (object) "http://schemas.microsoft.com/powershell/"));
    private bool isErrorReported;
    private string architecture;

    [ValidateSet(new string[] {"x86", "amd64", "ia64"})]
    [Parameter]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] {"PA"})]
    public string ProcessorArchitecture
    {
      get => this.architecture;
      set => this.architecture = value;
    }

    protected override void BeginProcessing()
    {
      if (this.isSddlSpecified && this.showUISpecified)
        throw new PSInvalidOperationException(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "ShowUIAndSDDLCannotExist", (object) "SecurityDescriptorSddl", (object) "ShowSecurityDescriptorUI"));
      RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
      PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
    }

    protected override void ProcessRecord()
    {
      this.WriteVerbose(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "NcsScriptMessageV", (object) "\r\nfunction Register-PSSessionConfiguration\r\n{{\r\n    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\n    param(  \r\n      [string]$filepath,\r\n      [string]$pluginName,\r\n      [bool]$shouldShowUI,\r\n      [bool]$force,\r\n      [bool]$noServiceRestart,\r\n      [string]$restartWSManTarget,\r\n      [string]$restartWSManAction,\r\n      [string]$restartWSManRequired\r\n    )\r\n\r\n    process\r\n    {{\r\n        new-item -path WSMan:\\localhost\\Plugin -file \"$filepath\" -name \"$pluginName\"\r\n        # $? is to make sure the last operation is succeeded\r\n        if ($? -and $shouldShowUI)\r\n        {{\r\n           if ($noServiceRestart)\r\n           {{\r\n               write-error $restartWSManRequired\r\n               return\r\n           }}\r\n\r\n           if ($force -or $pscmdlet.shouldprocess($restartWSManTarget, $restartWSManAction))\r\n           {{\r\n               restart-service winrm -force\r\n               $null = winrm configsddl \"{0}$pluginName\"\r\n           }}\r\n           else\r\n           {{\r\n               write-error $restartWSManRequired\r\n           }}\r\n        }}\r\n    }}\r\n}}\r\n\r\nRegister-PSSessionConfiguration -filepath $args[0] -pluginName $args[1] -shouldShowUI $args[2] -force $args[3] -noServiceRestart $args[4] -whatif:$args[5] -confirm:$args[6] -restartWSManTarget $args[7] -restartWSManAction $args[8] -restartWSManRequired $args[9]\r\n"));
      if (!this.force)
      {
        string action = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "CSShouldProcessAction", (object) this.CommandInfo.Name);
        string target;
        if (this.isSddlSpecified)
          target = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "NcsShouldProcessTargetSDDL", (object) this.Name, (object) this.sddl);
        else
          target = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "CSShouldProcessTargetAdminEnable", (object) this.Name);
        if (!this.ShouldProcess(target, action))
          return;
      }
      string tmpFileName = this.ConstructTemporaryFile(this.ConstructPluginContent());
      try
      {
        string resourceString = ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "RestartWSManServiceAction");
        string str1 = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "RestartWSManServiceTarget", (object) "WinRM");
        string str2 = ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "RestartWSManRequiredShowUI", (object) string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Set-PSSessionConfiguration {0} -ShowSecurityDescriptorUI", (object) this.shellName));
        bool whatIf = false;
        bool confirm = true;
        PSSessionConfigurationCommandUtilities.CollectShouldProcessParameters((PSCmdlet) this, out whatIf, out confirm);
        int count = ((ArrayList) this.Context.DollarErrorVariable).Count;
        object sendToPipeline = RegisterPSSessionConfigurationCommand.newPluginSb.InvokeUsingCmdlet((Cmdlet) this, true, true, (object) AutomationNull.Value, (object) new object[0], (object) AutomationNull.Value, (object) tmpFileName, (object) this.shellName, (object) this.ShowSecurityDescriptorUI.ToBool(), (object) this.force, (object) this.NoServiceRestart, (object) whatIf, (object) confirm, (object) str1, (object) resourceString, (object) str2);
        if (sendToPipeline != AutomationNull.Value)
          this.WriteObject(sendToPipeline);
        this.isErrorReported = ((ArrayList) this.Context.DollarErrorVariable).Count > count;
      }
      finally
      {
        this.DeleteFile(tmpFileName);
      }
    }

    protected override void EndProcessing()
    {
      if ((bool) this.ShowSecurityDescriptorUI)
        return;
      PSSessionConfigurationCommandUtilities.RestartWinRMService((PSCmdlet) this, this.isErrorReported, (bool) this.Force, this.noRestart);
    }

    private void DeleteFile(string tmpFileName)
    {
      Exception exception = (Exception) null;
      try
      {
        File.Delete(tmpFileName);
      }
      catch (UnauthorizedAccessException ex)
      {
        exception = (Exception) ex;
      }
      catch (ArgumentException ex)
      {
        exception = (Exception) ex;
      }
      catch (PathTooLongException ex)
      {
        exception = (Exception) ex;
      }
      catch (DirectoryNotFoundException ex)
      {
        exception = (Exception) ex;
      }
      catch (IOException ex)
      {
        exception = (Exception) ex;
      }
      catch (NotSupportedException ex)
      {
        exception = (Exception) ex;
      }
      if (exception != null)
        throw PSSessionConfigurationCommandUtilities.Tracer.NewInvalidOperationException("RemotingErrorIdStrings", "NcsCannotDeleteFileAfterInstall", (object) tmpFileName, (object) exception.Message);
    }

    private string ConstructTemporaryFile(string pluginContent)
    {
      string str = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + "psshell.xml";
      Exception exception = (Exception) null;
      if (File.Exists(str))
      {
        FileInfo fileInfo = new FileInfo(str);
        if (fileInfo != null)
        {
          try
          {
            fileInfo.Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden);
            fileInfo.Delete();
          }
          catch (FileNotFoundException ex)
          {
            exception = (Exception) ex;
          }
          catch (DirectoryNotFoundException ex)
          {
            exception = (Exception) ex;
          }
          catch (UnauthorizedAccessException ex)
          {
            exception = (Exception) ex;
          }
          catch (SecurityException ex)
          {
            exception = (Exception) ex;
          }
          catch (ArgumentNullException ex)
          {
            exception = (Exception) ex;
          }
          catch (ArgumentException ex)
          {
            exception = (Exception) ex;
          }
          catch (PathTooLongException ex)
          {
            exception = (Exception) ex;
          }
          catch (NotSupportedException ex)
          {
            exception = (Exception) ex;
          }
          catch (IOException ex)
          {
            exception = (Exception) ex;
          }
          if (exception != null)
            throw PSSessionConfigurationCommandUtilities.Tracer.NewInvalidOperationException("RemotingErrorIdStrings", "NcsCannotDeleteFile", (object) str, (object) exception.Message);
        }
      }
      try
      {
        StreamWriter text = File.CreateText(str);
        text.Write(pluginContent);
        text.Flush();
        text.Close();
      }
      catch (UnauthorizedAccessException ex)
      {
        exception = (Exception) ex;
      }
      catch (ArgumentException ex)
      {
        exception = (Exception) ex;
      }
      catch (PathTooLongException ex)
      {
        exception = (Exception) ex;
      }
      catch (DirectoryNotFoundException ex)
      {
        exception = (Exception) ex;
      }
      if (exception != null)
        throw PSSessionConfigurationCommandUtilities.Tracer.NewInvalidOperationException("RemotingErrorIdStrings", "NcsCannotWritePluginContent", (object) str, (object) exception.Message);
      return str;
    }

    private string ConstructPluginContent()
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (!string.IsNullOrEmpty(this.assemblyName))
        stringBuilder.Append(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n    <Param Name='{0}'  Value='{1}' />{2}\r\n", (object) "assemblyname", (object) this.assemblyName, (object) Environment.NewLine));
      if (!string.IsNullOrEmpty(this.applicationBase))
        stringBuilder.Append(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n    <Param Name='{0}'  Value='{1}' />{2}\r\n", (object) "applicationbase", (object) this.applicationBase, (object) Environment.NewLine));
      if (!string.IsNullOrEmpty(this.configurationTypeName))
        stringBuilder.Append(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n    <Param Name='{0}'  Value='{1}' />{2}\r\n", (object) "pssessionconfigurationtypename", (object) this.configurationTypeName, (object) Environment.NewLine));
      if (!string.IsNullOrEmpty(this.configurationScript))
        stringBuilder.Append(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n    <Param Name='{0}'  Value='{1}' />{2}\r\n", (object) "startupscript", (object) this.configurationScript, (object) Environment.NewLine));
      if (this.maxCommandSizeMB.HasValue)
        stringBuilder.Append(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n    <Param Name='{0}'  Value='{1}' />{2}\r\n", (object) "psmaximumreceiveddatasizepercommandmb", (object) this.maxCommandSizeMB.Value, (object) Environment.NewLine));
      if (this.maxObjectSizeMB.HasValue)
        stringBuilder.Append(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n    <Param Name='{0}'  Value='{1}' />{2}\r\n", (object) "psmaximumreceivedobjectsizemb", (object) this.maxObjectSizeMB.Value, (object) Environment.NewLine));
      if (this.threadAptState.HasValue)
        stringBuilder.Append(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n    <Param Name='{0}'  Value='{1}' />{2}\r\n", (object) "pssessionthreadapartmentstate", (object) this.threadAptState.Value, (object) Environment.NewLine));
      if (this.threadOptions.HasValue)
        stringBuilder.Append(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n    <Param Name='{0}'  Value='{1}' />{2}\r\n", (object) "pssessionthreadoptions", (object) this.threadOptions.Value, (object) Environment.NewLine));
      string str1 = "";
      if (!string.IsNullOrEmpty(this.sddl))
        str1 = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n      <Security Uri='{0}' ExactMatch='true' Sddl='{1}'>\r\n      </Security>\r\n", (object) ("http://schemas.microsoft.com/powershell/" + this.shellName), (object) this.sddl);
      string str2 = "";
      if (!string.IsNullOrEmpty(this.architecture))
      {
        string str3 = "32";
        switch (this.architecture.ToLowerInvariant())
        {
          case "x86":
            str3 = "32";
            break;
          case "amd64":
          case "ia64":
            str3 = "64";
            break;
        }
        str2 = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n                     Architecture='{0}'", (object) str3);
      }
      return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n<PlugInConfiguration xmlns='http://schemas.microsoft.com/wbem/wsman/1/config/PluginConfiguration'\r\n                     Name='{0}'\r\n                     Filename='%windir%\\system32\\{1}'\r\n                     SDKVersion='1'\r\n                     XmlRenderingType='text' {2} >\r\n  <InitializationParameters>\r\n    <Param Name='PSVersion'  Value='2.0' />\r\n{3}\r\n  </InitializationParameters> \r\n  <Resources>\r\n    <Resource ResourceUri='{4}' SupportsOptions='true' ExactMatch='true'>\r\n{5}\r\n      <Capability Type='Shell' />\r\n    </Resource>\r\n  </Resources>\r\n</PlugInConfiguration>\r\n", (object) this.shellName, (object) "pwrshplugin.dll", (object) str2, (object) stringBuilder.ToString(), (object) ("http://schemas.microsoft.com/powershell/" + this.shellName), (object) str1);
    }
  }
}
