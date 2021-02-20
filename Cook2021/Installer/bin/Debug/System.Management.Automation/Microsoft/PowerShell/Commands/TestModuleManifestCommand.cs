// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.TestModuleManifestCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Test", "ModuleManifest")]
  [OutputType(new Type[] {typeof (PSModuleInfo)})]
  public sealed class TestModuleManifestCommand : ModuleCmdletBase
  {
    private string _path;

    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string Path
    {
      get => this._path;
      set => this._path = value;
    }

    protected override void ProcessRecord()
    {
      ProviderInfo provider = (ProviderInfo) null;
      Collection<string> providerPathFromPsPath;
      try
      {
        providerPathFromPsPath = this.SessionState.Path.GetResolvedProviderPathFromPSPath(this._path, out provider);
      }
      catch (ItemNotFoundException ex)
      {
        this.WriteError(new ErrorRecord((Exception) new FileNotFoundException(ResourceManagerCache.FormatResourceString("Modules", "ModuleNotFound", (object) this._path)), "Modules_ModuleNotFound", ErrorCategory.ResourceUnavailable, (object) this._path));
        return;
      }
      if (!provider.NameEquals(this.Context.ProviderNames.FileSystem))
        throw InterpreterError.NewInterpreterException((object) this._path, typeof (RuntimeException), (Token) null, "FileOpenError", (object) provider.FullName);
      if (providerPathFromPsPath == null || providerPathFromPsPath.Count < 1)
      {
        this.WriteError(new ErrorRecord((Exception) new FileNotFoundException(ResourceManagerCache.FormatResourceString("Modules", "ModuleNotFound", (object) this._path)), "Modules_ModuleNotFound", ErrorCategory.ResourceUnavailable, (object) this._path));
      }
      else
      {
        string str = providerPathFromPsPath.Count <= 1 ? providerPathFromPsPath[0] : throw InterpreterError.NewInterpreterException((object) providerPathFromPsPath, typeof (RuntimeException), (Token) null, "AmbiguousPath");
        if (System.IO.Path.GetExtension(str).Equals(".psd1", StringComparison.OrdinalIgnoreCase))
        {
          PSModuleInfo psModuleInfo = this.LoadModuleManifest(this.GetScriptInfoForFile(str, out string _), ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, (Version) null);
          if (psModuleInfo == null)
            return;
          this.WriteObject((object) psModuleInfo);
        }
        else
          this.ThrowTerminatingError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "InvalidModuleManifestPath", (object) str)), "Modules_InvalidModuleManifestPath", ErrorCategory.InvalidArgument, (object) this._path));
      }
    }
  }
}
