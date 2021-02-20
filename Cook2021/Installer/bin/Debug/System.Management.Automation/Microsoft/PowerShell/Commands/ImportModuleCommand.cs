// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ImportModuleCommand
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
  [System.Management.Automation.Cmdlet("Import", "Module", DefaultParameterSetName = "Name")]
  public sealed class ImportModuleCommand : ModuleCmdletBase
  {
    private string[] _name = new string[0];
    private System.Reflection.Assembly[] _assembly;
    private string[] _functionImportList = new string[0];
    private string[] _cmdletImportList = new string[0];
    private string[] _variableExportList;
    private string[] _aliasExportList;
    private PSModuleInfo[] _moduleInfo = new PSModuleInfo[0];

    [Parameter]
    public SwitchParameter Global
    {
      set => this.BaseGlobal = (bool) value;
      get => (SwitchParameter) this.BaseGlobal;
    }

    [Parameter]
    [ValidateNotNull]
    public string Prefix
    {
      set => this.BasePrefix = value;
      get => this.BasePrefix;
    }

    [Parameter(Mandatory = true, ParameterSetName = "Name", Position = 0, ValueFromPipeline = true)]
    public string[] Name
    {
      set => this._name = value;
      get => this._name;
    }

    [Parameter(Mandatory = true, ParameterSetName = "Assembly", Position = 0, ValueFromPipeline = true)]
    public System.Reflection.Assembly[] Assembly
    {
      get => this._assembly;
      set => this._assembly = value;
    }

    [Parameter]
    [ValidateNotNull]
    public string[] Function
    {
      set
      {
        if (value == null)
          return;
        this._functionImportList = value;
        this.BaseFunctionPatterns = new List<WildcardPattern>();
        foreach (string functionImport in this._functionImportList)
          this.BaseFunctionPatterns.Add(new WildcardPattern(functionImport, WildcardOptions.IgnoreCase));
      }
      get => this._functionImportList;
    }

    [Parameter]
    [ValidateNotNull]
    public string[] Cmdlet
    {
      set
      {
        if (value == null)
          return;
        this._cmdletImportList = value;
        this.BaseCmdletPatterns = new List<WildcardPattern>();
        foreach (string cmdletImport in this._cmdletImportList)
          this.BaseCmdletPatterns.Add(new WildcardPattern(cmdletImport, WildcardOptions.IgnoreCase));
      }
      get => this._cmdletImportList;
    }

    [Parameter]
    [ValidateNotNull]
    public string[] Variable
    {
      set
      {
        if (value == null)
          return;
        this._variableExportList = value;
        this.BaseVariablePatterns = new List<WildcardPattern>();
        foreach (string variableExport in this._variableExportList)
          this.BaseVariablePatterns.Add(new WildcardPattern(variableExport, WildcardOptions.IgnoreCase));
      }
      get => this._variableExportList;
    }

    [Parameter]
    [ValidateNotNull]
    public string[] Alias
    {
      set
      {
        if (value == null)
          return;
        this._aliasExportList = value;
        this.BaseAliasPatterns = new List<WildcardPattern>();
        foreach (string aliasExport in this._aliasExportList)
          this.BaseAliasPatterns.Add(new WildcardPattern(aliasExport, WildcardOptions.IgnoreCase));
      }
      get => this._aliasExportList;
    }

    [Parameter]
    public SwitchParameter Force
    {
      get => (SwitchParameter) this.BaseForce;
      set => this.BaseForce = (bool) value;
    }

    [Parameter]
    public SwitchParameter PassThru
    {
      get => (SwitchParameter) this.BasePassThru;
      set => this.BasePassThru = (bool) value;
    }

    [Parameter]
    public SwitchParameter AsCustomObject
    {
      get => (SwitchParameter) this.BaseAsCustomObject;
      set => this.BaseAsCustomObject = (bool) value;
    }

    [Parameter(ParameterSetName = "Name")]
    public Version Version
    {
      get => this.BaseVersion;
      set => this.BaseVersion = value;
    }

    [Parameter(Mandatory = true, ParameterSetName = "ModuleInfo", Position = 0, ValueFromPipeline = true)]
    public PSModuleInfo[] ModuleInfo
    {
      set => this._moduleInfo = value;
      get => this._moduleInfo;
    }

    [Parameter]
    [System.Management.Automation.Alias(new string[] {"Args"})]
    public object[] ArgumentList
    {
      get => this.BaseArgumentList;
      set => this.BaseArgumentList = value;
    }

    [Parameter]
    public SwitchParameter DisableNameChecking
    {
      get => (SwitchParameter) this.BaseDisableNameChecking;
      set => this.BaseDisableNameChecking = (bool) value;
    }

    public ImportModuleCommand() => this.BaseDisableNameChecking = false;

    protected override void ProcessRecord()
    {
      bool found = false;
      foreach (PSModuleInfo psModuleInfo1 in this._moduleInfo)
      {
        if (!this.BaseForce && this.Context.Modules.ModuleTable.ContainsKey(psModuleInfo1.Path))
        {
          ModuleCmdletBase.AddModuleToModuleTables(this.Context, this.TargetSessionState.Internal, psModuleInfo1);
          this.ImportModuleMembers(psModuleInfo1, this.BasePrefix);
          if (this.BaseAsCustomObject)
          {
            if (psModuleInfo1.ModuleType != ModuleType.Script)
              this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "CantUseAsCustomObjectWithBinaryModule", (object) psModuleInfo1.Path)), "Modules_CantUseAsCustomObjectWithBinaryModule", ErrorCategory.PermissionDenied, (object) null));
            else
              this.WriteObject((object) psModuleInfo1.AsCustomObject());
          }
          else if (this.BasePassThru)
            this.WriteObject((object) psModuleInfo1);
        }
        else
        {
          PSModuleInfo module;
          if (this.Context.Modules.ModuleTable.TryGetValue(psModuleInfo1.Path, out module))
            this.RemoveModule(module);
          PSModuleInfo psModuleInfo2 = psModuleInfo1;
          try
          {
            if (psModuleInfo1.SessionState == null)
            {
              if (File.Exists(psModuleInfo1.Path))
                this.LoadModule(psModuleInfo1.Path, (string) null, this.BasePrefix, (SessionState) null, out found);
            }
            else if (!string.IsNullOrEmpty(psModuleInfo1.Name))
            {
              found = true;
              ModuleCmdletBase.AddModuleToModuleTables(this.Context, this.TargetSessionState.Internal, psModuleInfo2);
              if (psModuleInfo2.SessionState != null)
                this.ImportModuleMembers(psModuleInfo2, this.BasePrefix);
              if (this.BaseAsCustomObject && psModuleInfo2.SessionState != null)
                this.WriteObject((object) psModuleInfo1.AsCustomObject());
              else if (this.BasePassThru)
                this.WriteObject((object) psModuleInfo2);
            }
          }
          catch (IOException ex)
          {
          }
        }
      }
      if (this._assembly != null)
      {
        foreach (System.Reflection.Assembly assemblyToLoad in this._assembly)
        {
          if (assemblyToLoad != null)
          {
            PSModuleInfo module = this.LoadBinaryModule(false, (string) null, (string) null, assemblyToLoad, (string) null, (SessionState) null, this.BasePrefix, false, false, out found);
            if (found = module != null)
              ModuleCmdletBase.AddModuleToModuleTables(this.Context, this.TargetSessionState.Internal, module);
          }
        }
      }
      IEnumerable<string> modulePath = ModuleIntrinsics.GetModulePath(this.Context);
      foreach (string str1 in this.Name)
      {
        string str2 = ModuleCmdletBase.ResolveRootedFilePath(str1, this.Context);
        if (!string.IsNullOrEmpty(str2))
        {
          bool flag = false;
          if (!this.BaseForce && this.Context.Modules.ModuleTable.ContainsKey(str2))
          {
            PSModuleInfo psModuleInfo = this.Context.Modules.ModuleTable[str2];
            if (this.BaseVersion == (Version) null || psModuleInfo.ModuleType != ModuleType.Manifest || psModuleInfo.Version >= this.BaseVersion)
            {
              flag = true;
              ModuleCmdletBase.AddModuleToModuleTables(this.Context, this.TargetSessionState.Internal, psModuleInfo);
              this.ImportModuleMembers(psModuleInfo, this.BasePrefix);
              if (this.BaseAsCustomObject)
              {
                if (psModuleInfo.ModuleType != ModuleType.Script)
                  this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "CantUseAsCustomObjectWithBinaryModule", (object) psModuleInfo.Path)), "Modules_CantUseAsCustomObjectWithBinaryModule", ErrorCategory.PermissionDenied, (object) null));
                else
                  this.WriteObject((object) psModuleInfo.AsCustomObject());
              }
              else if (this.BasePassThru)
                this.WriteObject((object) psModuleInfo);
              found = true;
            }
          }
          if (!flag)
          {
            if (File.Exists(str2))
            {
              PSModuleInfo module;
              if (this.Context.Modules.ModuleTable.TryGetValue(str2, out module))
                this.RemoveModule(module);
              this.LoadModule(str2, (string) null, this.BasePrefix, (SessionState) null, out found);
            }
            else if (Directory.Exists(str2))
            {
              string str3 = Path.Combine(str2, Path.GetFileName(str2));
              this.LoadUsingExtensions(str3, str3, (string) null, (string) null, this.BasePrefix, (SessionState) null, out found);
            }
          }
        }
        else if (ModuleCmdletBase.IsRooted(str1))
          this.LoadUsingExtensions(str1, str1, (string) null, (string) null, this.BasePrefix, (SessionState) null, out found);
        else
          found = this.LoadUsingModulePath(found, modulePath, str1, (SessionState) null, out PSModuleInfo _);
        if (!found)
          this.WriteError(new ErrorRecord((Exception) new FileNotFoundException(ResourceManagerCache.FormatResourceString("Modules", "ModuleNotFound", (object) str1)), "Modules_ModuleNotFound", ErrorCategory.ResourceUnavailable, (object) str1));
      }
    }
  }
}
