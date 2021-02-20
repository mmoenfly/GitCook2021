// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.GetModuleCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  [OutputType(new Type[] {typeof (PSModuleInfo)})]
  [Cmdlet("Get", "Module", DefaultParameterSetName = "Loaded")]
  public sealed class GetModuleCommand : ModuleCmdletBase
  {
    private string[] _name;
    private bool _all;
    private bool _listAvailableModules;

    [Parameter(ParameterSetName = "Available", Position = 0, ValueFromPipeline = true)]
    [Parameter(ParameterSetName = "Loaded", Position = 0, ValueFromPipeline = true)]
    public string[] Name
    {
      set => this._name = value;
      get => this._name;
    }

    [Parameter]
    public SwitchParameter All
    {
      get => (SwitchParameter) this._all;
      set => this._all = (bool) value;
    }

    [Parameter(ParameterSetName = "Available")]
    public SwitchParameter ListAvailable
    {
      get => (SwitchParameter) this._listAvailableModules;
      set => this._listAvailableModules = (bool) value;
    }

    protected override void ProcessRecord()
    {
      if (this._listAvailableModules)
      {
        IEnumerable<WildcardPattern> wildcardsFromStrings = (IEnumerable<WildcardPattern>) SessionStateUtilities.CreateWildcardsFromStrings(this._name, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
        foreach (PSModuleInfo availableModule in (IEnumerable<PSModuleInfo>) this.GetAvailableModules())
        {
          if (SessionStateUtilities.MatchesAnyWildcardPattern(availableModule.Name, wildcardsFromStrings, true))
            this.WriteObject((object) availableModule);
        }
      }
      else
      {
        foreach (object module in this.Context.Modules.GetModules(this._name, this._all))
          this.WriteObject(module);
      }
    }

    private List<PSModuleInfo> GetAvailableModules()
    {
      List<PSModuleInfo> availableModules = new List<PSModuleInfo>();
      foreach (string directory in ModuleIntrinsics.GetModulePath(this.Context))
      {
        try
        {
          if (this.All.IsPresent)
            this.GetAllAvailableModules(directory, availableModules);
          else
            this.GetDefaultAvailableModules(directory, availableModules);
        }
        catch (IOException ex)
        {
        }
        catch (UnauthorizedAccessException ex)
        {
        }
      }
      return availableModules;
    }

    private void RecurseDirectories(string directory, Action<string> directoryAction)
    {
      string[] strArray = new string[0];
      string[] directories;
      try
      {
        directories = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);
      }
      catch (IOException ex)
      {
        return;
      }
      catch (UnauthorizedAccessException ex)
      {
        return;
      }
      foreach (string str in directories)
      {
        try
        {
          directoryAction(str);
        }
        catch (IOException ex)
        {
        }
        catch (UnauthorizedAccessException ex)
        {
        }
      }
    }

    private void GetAllAvailableModules(string directory, List<PSModuleInfo> availableModules)
    {
      this.RecurseDirectories(directory, (Action<string>) (subDirectory => this.GetAllAvailableModules(subDirectory, availableModules)));
      foreach (string file in Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly))
      {
        foreach (string psModuleExtension in ModuleIntrinsics.PSModuleExtensions)
        {
          if (Path.GetExtension(file).Equals(psModuleExtension, StringComparison.OrdinalIgnoreCase))
          {
            PSModuleInfo infoForGetModule = this.CreateModuleInfoForGetModule(file);
            if (infoForGetModule != null)
            {
              availableModules.Add(infoForGetModule);
              break;
            }
          }
        }
      }
    }

    private void GetDefaultAvailableModules(string directory, List<PSModuleInfo> availableModules)
    {
      this.RecurseDirectories(directory, (Action<string>) (subDirectory => this.GetDefaultAvailableModules(subDirectory, availableModules)));
      foreach (string psModuleExtension in ModuleIntrinsics.PSModuleExtensions)
      {
        string str = Path.Combine(directory, Path.GetFileName(directory)) + psModuleExtension;
        if (File.Exists(str))
        {
          PSModuleInfo infoForGetModule = this.CreateModuleInfoForGetModule(str);
          if (infoForGetModule != null)
          {
            availableModules.Add(infoForGetModule);
            break;
          }
        }
      }
    }

    private PSModuleInfo CreateModuleInfoForGetModule(string file)
    {
      PSModuleInfo psModuleInfo = (PSModuleInfo) null;
      string extension = Path.GetExtension(file);
      if (extension.Equals(".psd1", StringComparison.OrdinalIgnoreCase))
      {
        try
        {
          psModuleInfo = this.LoadModuleManifest(this.GetScriptInfoForFile(file, out string _), ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError, (Version) null);
        }
        catch (PSSecurityException ex)
        {
        }
        catch (RuntimeException ex)
        {
        }
        catch (IOException ex)
        {
        }
        catch (UnauthorizedAccessException ex)
        {
        }
      }
      if (psModuleInfo == null)
        psModuleInfo = new PSModuleInfo(file, (ExecutionContext) null, (SessionState) null);
      if (extension.Equals(".psd1", StringComparison.OrdinalIgnoreCase))
        psModuleInfo.SetModuleType(ModuleType.Manifest);
      else if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
        psModuleInfo.SetModuleType(ModuleType.Binary);
      else
        psModuleInfo.SetModuleType(ModuleType.Script);
      return psModuleInfo;
    }
  }
}
