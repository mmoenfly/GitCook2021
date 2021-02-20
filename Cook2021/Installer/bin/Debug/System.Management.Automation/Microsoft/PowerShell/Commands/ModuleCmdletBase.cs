// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ModuleCmdletBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
  public class ModuleCmdletBase : PSCmdlet
  {
    [TraceSource("Parser", "Parser")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");
    private string _prefix = string.Empty;
    private bool _force;
    private bool _global;
    private bool _passThru;
    private bool _baseAsCustomObject;
    private List<WildcardPattern> _functionPatterns;
    private List<WildcardPattern> _cmdletPatterns;
    private List<WildcardPattern> _variablePatterns;
    private List<WildcardPattern> _aliasPatterns;
    private Version _version;
    private object[] _arguments;
    private bool _disableNameChecking = true;
    private List<WildcardPattern> _matchAll;
    private static string[] PermittedCmdlets = new string[5]
    {
      "Import-LocalizedData",
      "ConvertFrom-StringData",
      "Write-Host",
      "Out-Host",
      "Join-Path"
    };
    private static string[] ModuleManifestMembers = new string[26]
    {
      "ModuleToProcess",
      "NestedModules",
      "GUID",
      "Author",
      "CompanyName",
      "Copyright",
      "ModuleVersion",
      "Description",
      "PowerShellVersion",
      "PowerShellHostName",
      "PowerShellHostVersion",
      "CLRVersion",
      "DotNetFrameworkVersion",
      "ProcessorArchitecture",
      "RequiredModules",
      "TypesToProcess",
      "FormatsToProcess",
      "ScriptsToProcess",
      "PrivateData",
      "RequiredAssemblies",
      "ModuleList",
      "FileList",
      "FunctionsToExport",
      "VariablesToExport",
      "AliasesToExport",
      "CmdletsToExport"
    };
    private static string[] ModuleVersionMembers = new string[3]
    {
      "ModuleName",
      "GUID",
      "ModuleVersion"
    };

    internal string BasePrefix
    {
      set => this._prefix = value;
      get => this._prefix;
    }

    internal bool BaseForce
    {
      get => this._force;
      set => this._force = value;
    }

    internal bool BaseGlobal
    {
      get => this._global;
      set => this._global = value;
    }

    internal SessionState TargetSessionState => this.BaseGlobal ? this.Context.TopLevelSessionState.PublicSessionState : this.Context.SessionState;

    internal bool BasePassThru
    {
      get => this._passThru;
      set => this._passThru = value;
    }

    internal bool BaseAsCustomObject
    {
      get => this._baseAsCustomObject;
      set => this._baseAsCustomObject = value;
    }

    internal List<WildcardPattern> BaseFunctionPatterns
    {
      get => this._functionPatterns;
      set => this._functionPatterns = value;
    }

    internal List<WildcardPattern> BaseCmdletPatterns
    {
      get => this._cmdletPatterns;
      set => this._cmdletPatterns = value;
    }

    internal List<WildcardPattern> BaseVariablePatterns
    {
      get => this._variablePatterns;
      set => this._variablePatterns = value;
    }

    internal List<WildcardPattern> BaseAliasPatterns
    {
      get => this._aliasPatterns;
      set => this._aliasPatterns = value;
    }

    internal Version BaseVersion
    {
      get => this._version;
      set => this._version = value;
    }

    protected object[] BaseArgumentList
    {
      get => this._arguments;
      set => this._arguments = value;
    }

    protected bool BaseDisableNameChecking
    {
      get => this._disableNameChecking;
      set => this._disableNameChecking = value;
    }

    internal List<WildcardPattern> MatchAll
    {
      get
      {
        if (this._matchAll == null)
        {
          this._matchAll = new List<WildcardPattern>();
          this._matchAll.Add(new WildcardPattern("*", WildcardOptions.IgnoreCase));
        }
        return this._matchAll;
      }
    }

    internal bool LoadUsingModulePath(
      bool found,
      IEnumerable<string> modulePath,
      string name,
      SessionState ss,
      out PSModuleInfo module)
    {
      string extension = Path.GetExtension(name);
      module = (PSModuleInfo) null;
      string str1;
      if (string.IsNullOrEmpty(extension) || !ModuleIntrinsics.IsPowerShellModuleExtension(extension))
      {
        str1 = name;
        extension = (string) null;
      }
      else
        str1 = name.Substring(0, name.Length - extension.Length);
      foreach (string path1 in modulePath)
      {
        string str2 = Path.Combine(path1, str1);
        if (name.IndexOfAny(Utils.DirectorySeparators) == -1)
          str2 = Path.Combine(str2, str1);
        else if (Directory.Exists(str2))
          str2 = Path.Combine(str2, Path.GetFileName(str1));
        module = this.LoadUsingExtensions(name, str2, extension, (string) null, this.BasePrefix, ss, out found);
        if (found)
          break;
      }
      return found;
    }

    private Hashtable LoadModuleManifestData(
      ExternalScriptInfo scriptInfo,
      string[] validMembers,
      ModuleCmdletBase.ManifestProcessingFlags manifestProcessingFlags,
      ref bool containedErrors)
    {
      try
      {
        RestrictedLanguageModeChecker.Check(this.Context.Engine.EngineParser, scriptInfo.ScriptBlock, (IEnumerable<string>) ModuleCmdletBase.PermittedCmdlets, true);
      }
      catch (RuntimeException ex)
      {
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          this.WriteError(new ErrorRecord((Exception) new MissingMemberException(ResourceManagerCache.FormatResourceString("Modules", "InvalidModuleManifest", (object) scriptInfo.Definition, (object) ex.Message, (object) ex.ErrorRecord.InvocationInfo.PositionMessage)), "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition));
        containedErrors = true;
        return (Hashtable) null;
      }
      object variableValue = this.Context.EngineSessionState.GetVariableValue("PSScriptRoot");
      object obj;
      try
      {
        this.Context.EngineSessionState.SetVariableValue("PSScriptRoot", (object) Path.GetDirectoryName(scriptInfo.Definition), CommandOrigin.Internal);
        obj = PSObject.Base(scriptInfo.ScriptBlock.InvokeReturnAsIs());
      }
      finally
      {
        this.Context.EngineSessionState.SetVariableValue("PSScriptRoot", variableValue);
      }
      if (!(obj is Hashtable data))
      {
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          this.WriteError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("Modules", "EmptyModuleManifest", (object) scriptInfo.Definition)), "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition));
        containedErrors = true;
        return (Hashtable) null;
      }
      if (validMembers != null && !this.ValidateManifestHash(data, validMembers, scriptInfo, manifestProcessingFlags))
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (Hashtable) null;
      }
      return data;
    }

    private bool ValidateManifestHash(
      Hashtable data,
      string[] validMembers,
      ExternalScriptInfo scriptInfo,
      ModuleCmdletBase.ManifestProcessingFlags manifestProcessingFlags)
    {
      bool flag1 = true;
      StringBuilder stringBuilder1 = new StringBuilder();
      foreach (string key in (IEnumerable) data.Keys)
      {
        bool flag2 = false;
        foreach (string validMember in validMembers)
        {
          if (key.Equals(validMember, StringComparison.OrdinalIgnoreCase))
            flag2 = true;
        }
        if (!flag2)
        {
          if (stringBuilder1.Length > 0)
            stringBuilder1.Append(", ");
          stringBuilder1.Append("'");
          stringBuilder1.Append(key);
          stringBuilder1.Append("'");
        }
      }
      if (stringBuilder1.Length > 0)
      {
        flag1 = false;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
        {
          StringBuilder stringBuilder2 = new StringBuilder("'");
          stringBuilder2.Append(validMembers[0]);
          for (int index = 1; index < validMembers.Length; ++index)
          {
            stringBuilder2.Append("', '");
            stringBuilder2.Append(validMembers[index]);
          }
          stringBuilder2.Append("'");
          this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "InvalidModuleManifestMember", (object) scriptInfo.Definition, (object) stringBuilder2, (object) stringBuilder1)), "Modules_InvalidManifestMember", ErrorCategory.InvalidData, (object) scriptInfo.Definition));
        }
      }
      return flag1;
    }

    private PSModuleInfo LoadModuleNamedInManifest(
      string moduleName,
      string moduleBase,
      bool searchModulePath,
      string prefix,
      SessionState ss,
      bool loadTypesFiles,
      bool loadFormatFiles,
      out bool found)
    {
      found = false;
      bool flag = false;
      Version baseVersion = this.BaseVersion;
      string str = ModuleCmdletBase.ResolveRootedFilePath(moduleName, this.Context);
      if (string.IsNullOrEmpty(str))
        str = Path.Combine(moduleBase, moduleName);
      else
        flag = true;
      string extension = Path.GetExtension(moduleName);
      try
      {
        this.Context.Modules.IncrementModuleNestingDepth((PSCmdlet) this, str);
        this.BaseVersion = (Version) null;
        if (!ModuleIntrinsics.IsPowerShellModuleExtension(extension))
          extension = (string) null;
        PSModuleInfo module = extension != null ? this.LoadModule(str, moduleBase, prefix, ss, out found) : this.LoadUsingExtensions(moduleName, str, extension, moduleBase, prefix, ss, out found);
        if (!found && flag)
          return (PSModuleInfo) null;
        if (searchModulePath && !found)
          found = this.LoadUsingModulePath(found, ModuleIntrinsics.GetModulePath(this.Context), moduleName, ss, out module);
        if (!found)
        {
          module = this.LoadBinaryModule(true, moduleName, (string) null, (Assembly) null, moduleBase, ss, prefix, loadTypesFiles, loadFormatFiles, out found);
          if (module != null)
            ModuleCmdletBase.AddModuleToModuleTables(this.Context, this.TargetSessionState.Internal, module);
        }
        return module;
      }
      finally
      {
        this.BaseVersion = baseVersion;
        this.Context.Modules.DecrementModuleNestingCount();
      }
    }

    internal PSModuleInfo LoadModuleManifest(
      ExternalScriptInfo scriptInfo,
      ModuleCmdletBase.ManifestProcessingFlags manifestProcessingFlags,
      Version version)
    {
      bool containedErrors = false;
      string directoryName = Path.GetDirectoryName(scriptInfo.Definition);
      Hashtable data = this.LoadModuleManifestData(scriptInfo, ModuleCmdletBase.ModuleManifestMembers, manifestProcessingFlags, ref containedErrors);
      if (data == null)
        return (PSModuleInfo) null;
      ExternalScriptInfo localizedModuleManifest = this.FindLocalizedModuleManifest(scriptInfo.Path);
      Hashtable hashtable = (Hashtable) null;
      if (localizedModuleManifest != null)
      {
        hashtable = this.LoadModuleManifestData(localizedModuleManifest, (string[]) null, manifestProcessingFlags, ref containedErrors);
        if (hashtable == null)
          return (PSModuleInfo) null;
      }
      string result1 = (string) null;
      if (!this.GetScalarFromData<string>(data, scriptInfo, "ModuleToProcess", manifestProcessingFlags, out result1))
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      if (!string.IsNullOrEmpty(result1))
      {
        PSModuleInfo psModuleInfo = (PSModuleInfo) null;
        string str = this.FixupFileName(directoryName, result1, (string) null);
        string extension = Path.GetExtension(str);
        if (!string.IsNullOrEmpty(extension) && ModuleIntrinsics.IsPowerShellModuleExtension(extension))
        {
          this.Context.Modules.ModuleTable.TryGetValue(str, out psModuleInfo);
        }
        else
        {
          foreach (string psModuleExtension in ModuleIntrinsics.PSModuleExtensions)
          {
            str = this.FixupFileName(directoryName, result1, psModuleExtension);
            this.Context.Modules.ModuleTable.TryGetValue(str, out psModuleInfo);
            if (psModuleInfo != null)
              break;
          }
        }
        if (psModuleInfo != null && (this.BaseVersion == (Version) null || psModuleInfo.Version >= this.BaseVersion))
        {
          if (!this.BaseForce)
          {
            ModuleCmdletBase.AddModuleToModuleTables(this.Context, this.TargetSessionState.Internal, psModuleInfo);
            this.ImportModuleMembers(psModuleInfo, this.BasePrefix);
            return psModuleInfo;
          }
          if (File.Exists(str))
            this.RemoveModule(psModuleInfo);
        }
      }
      Version result2;
      if (!this.GetScalarFromData<Version>(data, scriptInfo, "ModuleVersion", manifestProcessingFlags, out result2))
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (result2 == (Version) null)
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          this.WriteError(new ErrorRecord((Exception) new MissingMemberException(ResourceManagerCache.FormatResourceString("Modules", "ModuleManifestMissingModuleVersion", (object) scriptInfo.Definition)), "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition));
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (result2 < version && (manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
        return (PSModuleInfo) null;
      Version result3;
      if (!this.GetScalarFromData<Version>(data, scriptInfo, "PowerShellVersion", manifestProcessingFlags, out result3))
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (result3 != (Version) null)
      {
        Version psVersion = PSVersionInfo.PSVersion;
        if (psVersion < result3)
        {
          containedErrors = true;
          if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
            this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "ModuleManifestInsufficientPowerShellVersion", (object) psVersion, (object) scriptInfo.Definition, (object) result3)), "Modules_InsufficientPowerShellVersion", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition));
          if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
            return (PSModuleInfo) null;
        }
      }
      string result4;
      if (!this.GetScalarFromData<string>(data, scriptInfo, "PowerShellHostName", manifestProcessingFlags, out result4))
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (result4 != null)
      {
        string name = this.Context.InternalHost.Name;
        if (!string.Equals(name, result4, StringComparison.OrdinalIgnoreCase))
        {
          containedErrors = true;
          if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
            this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "InvalidPowerShellHostName", (object) name, (object) scriptInfo.Definition, (object) result4)), "Modules_InvalidPowerShellHostName", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition));
          if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
            return (PSModuleInfo) null;
        }
      }
      Version result5;
      if (!this.GetScalarFromData<Version>(data, scriptInfo, "PowerShellHostVersion", manifestProcessingFlags, out result5))
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (result5 != (Version) null)
      {
        Version version1 = this.Context.InternalHost.Version;
        if (version1 < result5)
        {
          containedErrors = true;
          if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
            this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "InvalidPowerShellHostVersion", (object) this.Context.InternalHost.Name, (object) version1, (object) scriptInfo.Definition, (object) result5)), "Modules_InsufficientPowerShellHostVersion", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition));
          if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
            return (PSModuleInfo) null;
        }
      }
      ProcessorArchitecture result6;
      if (!this.GetScalarFromData<ProcessorArchitecture>(data, scriptInfo, "ProcessorArchitecture", manifestProcessingFlags, out result6))
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (result6 != ProcessorArchitecture.None && result6 != ProcessorArchitecture.MSIL)
      {
        ProcessorArchitecture processorArchitecture = PsUtils.GetProcessorArchitecture();
        if (processorArchitecture != result6)
        {
          containedErrors = true;
          if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
            this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "InvalidProcessorArchitecture", (object) processorArchitecture, (object) scriptInfo.Definition, (object) result6)), "Modules_InvalidProcessorArchitecture", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition));
          if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
            return (PSModuleInfo) null;
        }
      }
      Version result7;
      if (!this.GetScalarFromData<Version>(data, scriptInfo, "CLRVersion", manifestProcessingFlags, out result7))
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (result7 != (Version) null)
      {
        Version version1 = Environment.Version;
        if (version1 < result7)
        {
          containedErrors = true;
          if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
            this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "ModuleManifestInsufficientCLRVersion", (object) version1, (object) scriptInfo.Definition, (object) result7)), "Modules_InsufficientCLRVersion", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition));
          if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
            return (PSModuleInfo) null;
        }
      }
      Version result8;
      if (!this.GetScalarFromData<Version>(data, scriptInfo, "DotNetFrameworkVersion", manifestProcessingFlags, out result8))
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (result8 != (Version) null && !PsUtils.IsDotNetFrameworkVersionInstalled(result8))
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "InvalidDotNetFrameworkVersion", (object) scriptInfo.Definition, (object) result8)), "Modules_InsufficientDotNetFrameworkVersion", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition));
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      Guid? result9 = new Guid?();
      if (!this.GetScalarFromData<Guid?>(data, scriptInfo, "guid", manifestProcessingFlags, out result9))
      {
        containedErrors = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      List<PSModuleInfo> psModuleInfoList = new List<PSModuleInfo>();
      ModuleSpecification[] result10;
      bool flag1;
      if (!this.GetScalarFromData<ModuleSpecification[]>(data, scriptInfo, "RequiredModules", manifestProcessingFlags, out result10))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (result10 != null && (manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.LoadElements) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
      {
        foreach (ModuleSpecification requiredModule in result10)
        {
          PSModuleInfo psModuleInfo = this.IsModuleLoaded(requiredModule, scriptInfo, manifestProcessingFlags, ref containedErrors);
          if (psModuleInfo == null)
            return (PSModuleInfo) null;
          psModuleInfoList.Add(psModuleInfo);
        }
      }
      List<string> list1;
      if (!this.GetListOfStringsFromData(data, scriptInfo, "NestedModules", manifestProcessingFlags, out list1))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      object obj = (object) null;
      if (data.Contains((object) "PrivateData"))
        obj = data[(object) "PrivateData"];
      List<WildcardPattern> list2;
      if (!this.GetListOfWildcardsFromData(data, scriptInfo, "FunctionsToExport", manifestProcessingFlags, out list2))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      List<WildcardPattern> list3;
      if (!this.GetListOfWildcardsFromData(data, scriptInfo, "VariablesToExport", manifestProcessingFlags, out list3))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      List<WildcardPattern> list4;
      if (!this.GetListOfWildcardsFromData(data, scriptInfo, "AliasesToExport", manifestProcessingFlags, out list4))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      List<WildcardPattern> list5;
      if (!this.GetListOfWildcardsFromData(data, scriptInfo, "CmdletsToExport", manifestProcessingFlags, out list5))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      InitialSessionState initialSessionState = (InitialSessionState) null;
      if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.LoadElements) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
        initialSessionState = InitialSessionState.Create();
      bool flag2 = false;
      bool flag3 = false;
      List<string> list6;
      if (!this.GetListOfStringsFromData(data, scriptInfo, "RequiredAssemblies", manifestProcessingFlags, out list6))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (list6 != null && (manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.LoadElements) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
      {
        foreach (string name in list6)
        {
          string fileName = this.FixupFileName(directoryName, name, ".dll");
          this.WriteVerbose(ResourceManagerCache.FormatResourceString("Modules", "LoadingFile", (object) "Assembly", (object) fileName));
          initialSessionState.Assemblies.Add(new SessionStateAssemblyEntry(name, fileName));
          flag2 = true;
        }
      }
      List<string> list7;
      if (!this.GetListOfFilesFromData(data, scriptInfo, "TypesToProcess", manifestProcessingFlags, directoryName, ".ps1xml", true, out list7))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (list7 != null && (manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.LoadElements) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
      {
        foreach (string fileName in list7)
        {
          this.WriteVerbose(ResourceManagerCache.FormatResourceString("Modules", "LoadingFile", (object) "TypesToProcess", (object) fileName));
          if (this.Context.RunspaceConfiguration != null)
          {
            this.Context.RunspaceConfiguration.Types.Append(new TypeConfigurationEntry(fileName));
            flag3 = true;
          }
          else
          {
            initialSessionState.Types.Add(new SessionStateTypeEntry(fileName));
            flag2 = true;
          }
        }
      }
      List<string> list8;
      if (!this.GetListOfFilesFromData(data, scriptInfo, "FormatsToProcess", manifestProcessingFlags, directoryName, ".ps1xml", true, out list8))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (list8 != null && (manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.LoadElements) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
      {
        foreach (string fileName in list8)
        {
          this.WriteVerbose(ResourceManagerCache.FormatResourceString("Modules", "LoadingFile", (object) "FormatsToProcess", (object) fileName));
          if (this.Context.RunspaceConfiguration != null)
          {
            this.Context.RunspaceConfiguration.Formats.Append(new FormatConfigurationEntry(fileName));
            flag3 = true;
          }
          else
          {
            initialSessionState.Formats.Add(new SessionStateFormatEntry(fileName));
            flag2 = true;
          }
        }
      }
      List<string> list9;
      if (!this.GetListOfFilesFromData(data, scriptInfo, "ScriptsToProcess", manifestProcessingFlags, directoryName, ".ps1", true, out list9))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      else if (list9 != null && (manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.LoadElements) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
      {
        foreach (string path in list9)
        {
          if (!Path.GetExtension(path).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
          {
            InvalidOperationException operationException = new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "ScriptsToProcessIncorrectExtension", (object) path));
            ModuleCmdletBase.WriteInvalidManifestMemberError((PSCmdlet) this, "ScriptsToProcess", scriptInfo, (Exception) operationException, manifestProcessingFlags);
            flag1 = true;
            if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
              return (PSModuleInfo) null;
          }
        }
      }
      string empty = string.Empty;
      if (data.Contains((object) "Description"))
      {
        if (hashtable != null && hashtable.Contains((object) "Description"))
          empty = (string) LanguagePrimitives.ConvertTo(hashtable[(object) "Description"], typeof (string), (IFormatProvider) CultureInfo.InvariantCulture);
        if (string.IsNullOrEmpty(empty))
          empty = (string) LanguagePrimitives.ConvertTo(data[(object) "Description"], typeof (string), (IFormatProvider) CultureInfo.InvariantCulture);
      }
      if (!this.GetListOfFilesFromData(data, scriptInfo, "FileList", manifestProcessingFlags, directoryName, "", false, out List<string> _))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      if (!this.GetScalarFromData<ModuleSpecification[]>(data, scriptInfo, "ModuleList", manifestProcessingFlags, out ModuleSpecification[] _))
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          return (PSModuleInfo) null;
      }
      if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.LoadElements) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
      {
        if (flag2)
        {
          try
          {
            initialSessionState.Bind(this.Context, true);
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            ErrorRecord errorRecord = new ErrorRecord(ex, "FormatXmlUpateException", ErrorCategory.InvalidOperation, (object) null);
            if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
              this.ThrowTerminatingError(errorRecord);
            else if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
              this.WriteError(errorRecord);
          }
        }
        if (flag3)
        {
          try
          {
            this.Context.CurrentRunspace.RunspaceConfiguration.Types.Update(true);
            this.Context.CurrentRunspace.RunspaceConfiguration.Formats.Update(true);
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            ErrorRecord errorRecord = new ErrorRecord(ex, "FormatXmlUpateException", ErrorCategory.InvalidOperation, (object) null);
            if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
              this.ThrowTerminatingError(errorRecord);
            else if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
              this.WriteError(errorRecord);
          }
        }
      }
      SessionState sessionState = (manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.LoadElements) == (ModuleCmdletBase.ManifestProcessingFlags) 0 ? (SessionState) null : new SessionState(this.Context.EngineSessionState, true, true);
      PSModuleInfo sourceModule = new PSModuleInfo(scriptInfo.Path, this.Context, sessionState);
      sourceModule.SetModuleType(ModuleType.Manifest);
      if (sessionState != null)
      {
        sessionState.Internal.SetVariableValue("PSScriptRoot", (object) Path.GetDirectoryName(scriptInfo.Path), CommandOrigin.Internal);
        sessionState.Internal.Module = sourceModule;
        if (list2 == null)
          list2 = this.MatchAll;
        if (list5 == null)
          list5 = this.MatchAll;
        if (list3 == null)
          list3 = this.MatchAll;
        if (list4 == null)
          list4 = this.MatchAll;
      }
      sourceModule.Description = empty;
      sourceModule.PrivateData = obj;
      sourceModule.SetExportedTypeFiles(new ReadOnlyCollection<string>((IList<string>) (list7 ?? new List<string>())));
      sourceModule.SetExportedFormatFiles(new ReadOnlyCollection<string>((IList<string>) (list8 ?? new List<string>())));
      sourceModule.SetVersion(result2);
      if (result9.HasValue)
        sourceModule.SetGuid(result9.Value);
      foreach (PSModuleInfo requiredModule in psModuleInfoList)
        sourceModule.AddRequiredModule(requiredModule);
      if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.LoadElements) == (ModuleCmdletBase.ManifestProcessingFlags) 0)
        return sourceModule;
      if (list9 != null)
      {
        foreach (string fileName in list9)
        {
          bool found = false;
          this.LoadModule(fileName, directoryName, string.Empty, (SessionState) null, out found);
        }
      }
      if (list1 != null && sessionState == null)
      {
        flag1 = true;
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
          this.WriteError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("Modules", "ModuleManifestNestedModulesCantGoWithModuleToProcess", (object) scriptInfo.Definition)), "Modules_BinaryModuleAndNestedModules", ErrorCategory.InvalidArgument, (object) scriptInfo.Definition));
      }
      else if (list1 != null && sessionState != null)
      {
        bool basePassThru = this.BasePassThru;
        this.BasePassThru = false;
        List<WildcardPattern> variablePatterns = this.BaseVariablePatterns;
        this.BaseVariablePatterns = this.MatchAll;
        List<WildcardPattern> functionPatterns = this.BaseFunctionPatterns;
        this.BaseFunctionPatterns = this.MatchAll;
        List<WildcardPattern> baseAliasPatterns = this.BaseAliasPatterns;
        this.BaseAliasPatterns = this.MatchAll;
        List<WildcardPattern> baseCmdletPatterns = this.BaseCmdletPatterns;
        this.BaseCmdletPatterns = this.MatchAll;
        bool disableNameChecking = this.BaseDisableNameChecking;
        this.BaseDisableNameChecking = true;
        SessionStateInternal engineSessionState = this.Context.EngineSessionState;
        try
        {
          this.Context.EngineSessionState = sessionState.Internal;
          foreach (string moduleName in list1)
          {
            bool found = false;
            PSModuleInfo nestedModule = this.LoadModuleNamedInManifest(moduleName, directoryName, true, string.Empty, (SessionState) null, true, true, out found);
            if (found)
            {
              if (nestedModule != null)
                sourceModule.AddNestedModule(nestedModule);
            }
            else
            {
              flag1 = true;
              this.ThrowTerminatingError(new ErrorRecord((Exception) new FileNotFoundException(ResourceManagerCache.FormatResourceString("Modules", "ManifestMemberNotFound", (object) moduleName, (object) "NestedModules", (object) scriptInfo.Path)), "Modules_ModuleFileNotFound", ErrorCategory.ResourceUnavailable, (object) ModuleIntrinsics.GetModuleName(scriptInfo.Path)));
            }
          }
        }
        finally
        {
          this.Context.EngineSessionState = engineSessionState;
          this.BasePassThru = basePassThru;
          this.BaseVariablePatterns = variablePatterns;
          this.BaseFunctionPatterns = functionPatterns;
          this.BaseAliasPatterns = baseAliasPatterns;
          this.BaseCmdletPatterns = baseCmdletPatterns;
          this.BaseDisableNameChecking = disableNameChecking;
        }
      }
      if (result1 != null)
      {
        if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.LoadElements) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
        {
          bool basePassThru = this.BasePassThru;
          this.BasePassThru = false;
          List<WildcardPattern> variablePatterns = this.BaseVariablePatterns;
          this.BaseVariablePatterns = new List<WildcardPattern>();
          List<WildcardPattern> functionPatterns = this.BaseFunctionPatterns;
          this.BaseFunctionPatterns = new List<WildcardPattern>();
          List<WildcardPattern> baseAliasPatterns = this.BaseAliasPatterns;
          this.BaseAliasPatterns = new List<WildcardPattern>();
          List<WildcardPattern> baseCmdletPatterns = this.BaseCmdletPatterns;
          this.BaseCmdletPatterns = new List<WildcardPattern>();
          PSModuleInfo psModuleInfo;
          try
          {
            bool found = false;
            psModuleInfo = this.LoadModuleNamedInManifest(result1, directoryName, false, this.BasePrefix, sessionState, list7 == null || 0 == list7.Count, list8 == null || 0 == list8.Count, out found);
            if (found)
            {
              if (psModuleInfo != null)
                goto label_225;
            }
            flag1 = true;
            this.ThrowTerminatingError(new ErrorRecord((Exception) new FileNotFoundException(ResourceManagerCache.FormatResourceString("Modules", "ManifestMemberNotFound", (object) result1, (object) "ModuleToProcess", (object) scriptInfo.Path)), "Modules_ModuleFileNotFound", ErrorCategory.ResourceUnavailable, (object) ModuleIntrinsics.GetModuleName(scriptInfo.Path)));
          }
          finally
          {
            this.BasePassThru = basePassThru;
            this.BaseVariablePatterns = variablePatterns;
            this.BaseFunctionPatterns = functionPatterns;
            this.BaseAliasPatterns = baseAliasPatterns;
            this.BaseCmdletPatterns = baseCmdletPatterns;
          }
label_225:
          if (psModuleInfo.SessionState == null && sessionState != null)
          {
            psModuleInfo.SessionState = sessionState;
            sessionState.Internal.Module = psModuleInfo;
          }
          else if (psModuleInfo.SessionState != null && sessionState == null)
            sessionState = psModuleInfo.SessionState;
          psModuleInfo.SetName(sourceModule.Name);
          foreach (PSModuleInfo nestedModule in sourceModule.NestedModules)
            psModuleInfo.AddNestedModule(nestedModule);
          foreach (PSModuleInfo requiredModule in sourceModule.RequiredModules)
            psModuleInfo.AddRequiredModule(requiredModule);
          psModuleInfo.SetVersion(sourceModule.Version);
          if (string.IsNullOrEmpty(psModuleInfo.Description))
            psModuleInfo.Description = empty;
          if (psModuleInfo.Version.Equals(new Version(0, 0)))
            psModuleInfo.SetVersion(result2);
          if (psModuleInfo.Guid.Equals(Guid.Empty) && result9.HasValue)
            psModuleInfo.SetGuid(result9.Value);
          if (psModuleInfo.PrivateData == null)
            psModuleInfo.PrivateData = sourceModule.PrivateData;
          if (sourceModule.ExportedTypeFiles.Count > 0)
            psModuleInfo.SetExportedTypeFiles(sourceModule.ExportedTypeFiles);
          if (sourceModule.ExportedFormatFiles.Count > 0)
            psModuleInfo.SetExportedFormatFiles(sourceModule.ExportedFormatFiles);
          sourceModule = psModuleInfo;
          if (sourceModule.ModuleType == ModuleType.Binary)
          {
            if (list5 != null)
              sourceModule.ExportedCmdlets.Clear();
            ModuleIntrinsics.ExportModuleMembers((PSCmdlet) this, sessionState.Internal, list2, list5, list4, list3);
          }
          else
          {
            if (!sessionState.Internal.UseExportList)
              ModuleIntrinsics.ExportModuleMembers((PSCmdlet) this, sessionState.Internal, this.MatchAll, this.MatchAll, (List<WildcardPattern>) null, (List<WildcardPattern>) null);
            if (list2 != null)
              ModuleCmdletBase.updateCommandCollection<FunctionInfo>(sessionState.Internal.ExportedFunctions, list2);
            if (list5 != null)
              ModuleCmdletBase.updateCommandCollection<CmdletInfo>(sourceModule.CompiledExports, list5);
            if (list4 != null)
              ModuleCmdletBase.updateCommandCollection<AliasInfo>(sessionState.Internal.ExportedAliases, list4);
            if (list3 != null)
            {
              List<PSVariable> psVariableList = new List<PSVariable>();
              foreach (PSVariable exportedVariable in sessionState.Internal.ExportedVariables)
              {
                if (SessionStateUtilities.MatchesAnyWildcardPattern(exportedVariable.Name, (IEnumerable<WildcardPattern>) list3, false))
                  psVariableList.Add(exportedVariable);
              }
              sessionState.Internal.ExportedVariables.Clear();
              sessionState.Internal.ExportedVariables.AddRange((IEnumerable<PSVariable>) psVariableList);
            }
          }
        }
        else
          sourceModule = new PSModuleInfo(scriptInfo.Path, (ExecutionContext) null, (SessionState) null);
      }
      else
        ModuleIntrinsics.ExportModuleMembers((PSCmdlet) this, sessionState.Internal, list2, list5, list4, list3);
      this.ImportModuleMembers(sourceModule, this.BasePrefix);
      return sourceModule;
    }

    private static void updateCommandCollection<T>(List<T> list, List<WildcardPattern> patterns) where T : CommandInfo
    {
      List<T> objList = new List<T>();
      foreach (T obj in list)
      {
        if (SessionStateUtilities.MatchesAnyWildcardPattern(obj.Name, (IEnumerable<WildcardPattern>) patterns, false))
          objList.Add(obj);
      }
      list.Clear();
      list.AddRange((IEnumerable<T>) objList);
    }

    private static void WriteInvalidManifestMemberError(
      PSCmdlet cmdlet,
      string manifestElement,
      ExternalScriptInfo scriptInfo,
      Exception e,
      ModuleCmdletBase.ManifestProcessingFlags manifestProcessingFlags)
    {
      CommandProcessorBase.CheckForSevereException(e);
      if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) == (ModuleCmdletBase.ManifestProcessingFlags) 0)
        return;
      ErrorRecord memberErrorRecord = ModuleCmdletBase.GenerateInvalidModuleMemberErrorRecord(manifestElement, scriptInfo, e);
      cmdlet.WriteError(memberErrorRecord);
    }

    private static ErrorRecord GenerateInvalidModuleMemberErrorRecord(
      string manifestElement,
      ExternalScriptInfo scriptInfo,
      Exception e)
    {
      return new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("Modules", "ModuleManifestInvalidManifestMember", (object) manifestElement, (object) e.Message, (object) scriptInfo.Definition)), "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition);
    }

    private PSModuleInfo IsModuleLoaded(
      ModuleSpecification requiredModule,
      ExternalScriptInfo scriptInfo,
      ModuleCmdletBase.ManifestProcessingFlags manifestProcessingFlags,
      ref bool containedErrors)
    {
      PSModuleInfo psModuleInfo = (PSModuleInfo) null;
      bool flag1 = false;
      bool flag2 = false;
      string name = requiredModule.Name;
      Guid? guid = requiredModule.Guid;
      Version version = requiredModule.Version;
      if (!containedErrors)
      {
        ModuleIntrinsics modules = this.Context.Modules;
        string[] patterns = new string[1]{ "*" };
        foreach (PSModuleInfo module in modules.GetModules(patterns, false))
        {
          if (name.Equals(module.Name, StringComparison.OrdinalIgnoreCase))
          {
            if (!guid.HasValue || guid.Value.Equals(module.Guid))
            {
              if (version != (Version) null)
              {
                if (version <= module.Version)
                {
                  psModuleInfo = module;
                  break;
                }
                flag1 = true;
              }
              else
              {
                psModuleInfo = module;
                break;
              }
            }
            else
              flag2 = true;
          }
        }
      }
      if (!containedErrors && psModuleInfo == null && (manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
      {
        string message;
        if (flag1)
          message = ResourceManagerCache.FormatResourceString("Modules", "RequiredModuleNotLoadedWrongVersion", (object) scriptInfo.Definition, (object) name, (object) version);
        else if (flag2)
          message = ResourceManagerCache.FormatResourceString("Modules", "RequiredModuleNotLoadedWrongGuid", (object) scriptInfo.Definition, (object) name, (object) guid.Value);
        else
          message = ResourceManagerCache.FormatResourceString("Modules", "RequiredModuleNotLoaded", (object) scriptInfo.Definition, (object) name);
        this.WriteError(new ErrorRecord((Exception) new MissingMemberException(message), "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition));
      }
      return psModuleInfo;
    }

    private ExternalScriptInfo FindLocalizedModuleManifest(string path)
    {
      string directoryName = Path.GetDirectoryName(path);
      string fileName = Path.GetFileName(path);
      string path1 = (string) null;
      for (CultureInfo cultureInfo = CultureInfo.CurrentUICulture; cultureInfo != null && !string.IsNullOrEmpty(cultureInfo.Name); cultureInfo = cultureInfo.Parent)
      {
        StringBuilder stringBuilder = new StringBuilder(directoryName);
        stringBuilder.Append("\\");
        stringBuilder.Append(cultureInfo.Name);
        stringBuilder.Append("\\");
        stringBuilder.Append(fileName);
        string path2 = stringBuilder.ToString();
        if (File.Exists(path2))
        {
          path1 = path2;
          break;
        }
      }
      ExternalScriptInfo externalScriptInfo = (ExternalScriptInfo) null;
      if (path1 != null)
        externalScriptInfo = new ExternalScriptInfo(Path.GetFileName(path1), path1);
      return externalScriptInfo;
    }

    private bool GetListOfStringsFromData(
      Hashtable data,
      ExternalScriptInfo scriptInfo,
      string key,
      ModuleCmdletBase.ManifestProcessingFlags manifestProcessingFlags,
      out List<string> list)
    {
      list = (List<string>) null;
      if (data.Contains((object) key))
      {
        if (data[(object) key] != null)
        {
          try
          {
            string[] strArray = (string[]) LanguagePrimitives.ConvertTo(data[(object) key], typeof (string[]), (IFormatProvider) CultureInfo.InvariantCulture);
            list = new List<string>((IEnumerable<string>) strArray);
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            ModuleCmdletBase.WriteInvalidManifestMemberError((PSCmdlet) this, key, scriptInfo, ex, manifestProcessingFlags);
            return false;
          }
        }
      }
      return true;
    }

    private bool GetListOfWildcardsFromData(
      Hashtable data,
      ExternalScriptInfo scriptInfo,
      string key,
      ModuleCmdletBase.ManifestProcessingFlags manifestProcessingFlags,
      out List<WildcardPattern> list)
    {
      list = (List<WildcardPattern>) null;
      List<string> list1;
      if (!this.GetListOfStringsFromData(data, scriptInfo, key, manifestProcessingFlags, out list1))
        return false;
      if (list1 != null)
      {
        list = new List<WildcardPattern>();
        foreach (string pattern in list1)
        {
          try
          {
            list.Add(new WildcardPattern(pattern, WildcardOptions.IgnoreCase));
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            list = (List<WildcardPattern>) null;
            ModuleCmdletBase.WriteInvalidManifestMemberError((PSCmdlet) this, key, scriptInfo, ex, manifestProcessingFlags);
            return false;
          }
        }
      }
      return true;
    }

    private bool GetListOfFilesFromData(
      Hashtable data,
      ExternalScriptInfo scriptInfo,
      string key,
      ModuleCmdletBase.ManifestProcessingFlags manifestProcessingFlags,
      string moduleBase,
      string extension,
      bool verifyFilesExist,
      out List<string> list)
    {
      list = (List<string>) null;
      List<string> list1;
      if (!this.GetListOfStringsFromData(data, scriptInfo, key, manifestProcessingFlags, out list1))
        return false;
      if (list1 != null)
      {
        list = new List<string>();
        foreach (string name in list1)
        {
          try
          {
            string str = this.FixupFileName(moduleBase, name, extension);
            if (verifyFilesExist && !File.Exists(str))
              throw new FileNotFoundException(ResourceManagerCache.FormatResourceString("SessionStateStrings", "PathNotFound", (object) str), str);
            list.Add(str);
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
              this.ThrowTerminatingError(ModuleCmdletBase.GenerateInvalidModuleMemberErrorRecord(key, scriptInfo, ex));
            list = (List<string>) null;
            ModuleCmdletBase.WriteInvalidManifestMemberError((PSCmdlet) this, key, scriptInfo, ex, manifestProcessingFlags);
            return false;
          }
        }
      }
      return true;
    }

    private bool GetScalarFromData<T>(
      Hashtable data,
      ExternalScriptInfo scriptInfo,
      string key,
      ModuleCmdletBase.ManifestProcessingFlags manifestProcessingFlags,
      out T result)
    {
      object valueToConvert = data[(object) key];
      if (valueToConvert != null)
      {
        if (valueToConvert is string)
        {
          if (string.IsNullOrEmpty((string) valueToConvert))
            goto label_3;
        }
        try
        {
          result = (T) LanguagePrimitives.ConvertTo(valueToConvert, typeof (T), (IFormatProvider) CultureInfo.InvariantCulture);
          return true;
        }
        catch (PSInvalidCastException ex)
        {
          result = default (T);
          if ((manifestProcessingFlags & ModuleCmdletBase.ManifestProcessingFlags.WriteErrors) != (ModuleCmdletBase.ManifestProcessingFlags) 0)
            this.WriteError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("Modules", "ModuleManifestInvalidValue", (object) key, (object) ex.Message, (object) scriptInfo.Definition)), "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, (object) scriptInfo.Definition));
          return false;
        }
      }
label_3:
      result = default (T);
      return true;
    }

    internal string FixupFileName(string moduleBase, string name, string extension)
    {
      string str = ModuleCmdletBase.ResolveRootedFilePath(name, this.Context);
      if (string.IsNullOrEmpty(str))
        str = Path.Combine(moduleBase, name);
      if (string.IsNullOrEmpty(Path.GetExtension(name)))
        str += extension;
      return str;
    }

    internal static bool IsRooted(string filePath) => Path.IsPathRooted(filePath) || filePath.StartsWith(".\\", StringComparison.Ordinal) || (filePath.StartsWith("./", StringComparison.Ordinal) || filePath.StartsWith("..\\", StringComparison.Ordinal)) || (filePath.StartsWith("../", StringComparison.Ordinal) || filePath.StartsWith("~/", StringComparison.Ordinal) || filePath.StartsWith("~\\", StringComparison.Ordinal)) || filePath.IndexOf(":", StringComparison.Ordinal) >= 0;

    internal static string ResolveRootedFilePath(string filePath, ExecutionContext context)
    {
      if (!ModuleCmdletBase.IsRooted(filePath))
        return (string) null;
      ProviderInfo provider = (ProviderInfo) null;
      Collection<string> providerPathFromPsPath;
      try
      {
        providerPathFromPsPath = context.SessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out provider);
      }
      catch (ItemNotFoundException ex)
      {
        return (string) null;
      }
      if (!provider.NameEquals(context.ProviderNames.FileSystem))
        throw InterpreterError.NewInterpreterException((object) filePath, typeof (RuntimeException), (Token) null, "FileOpenError", (object) provider.FullName);
      if (providerPathFromPsPath == null || providerPathFromPsPath.Count < 1)
        return (string) null;
      return providerPathFromPsPath.Count <= 1 ? providerPathFromPsPath[0] : throw InterpreterError.NewInterpreterException((object) providerPathFromPsPath, typeof (RuntimeException), (Token) null, "AmbiguousPath");
    }

    internal static string GetResolvedPath(string filePath, ExecutionContext context)
    {
      ProviderInfo provider = (ProviderInfo) null;
      Collection<string> providerPathFromPsPath;
      try
      {
        providerPathFromPsPath = context.SessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out provider);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        return (string) null;
      }
      if (!provider.NameEquals(context.ProviderNames.FileSystem))
        return (string) null;
      return providerPathFromPsPath == null || providerPathFromPsPath.Count < 1 || providerPathFromPsPath.Count > 1 ? (string) null : providerPathFromPsPath[0];
    }

    internal void RemoveModule(PSModuleInfo module)
    {
      if (!this.Context.Modules.ModuleTable.ContainsKey(module.Path))
        return;
      if (module.OnRemove != null)
        module.OnRemove.InvokeUsingCmdlet((Cmdlet) this, true, true, (object) null, (object) null, (object) null, (object) module);
      Dictionary<string, List<CmdletInfo>> cmdletCache = this.Context.EngineSessionState.CmdletCache;
      List<string> stringList1 = new List<string>();
      foreach (KeyValuePair<string, List<CmdletInfo>> keyValuePair in cmdletCache)
      {
        List<CmdletInfo> cmdletInfoList = keyValuePair.Value;
        for (int index = cmdletInfoList.Count - 1; index >= 0; --index)
        {
          if (cmdletInfoList[index].Module != null && cmdletInfoList[index].Module.Path.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
            cmdletInfoList.RemoveAt(index);
        }
        if (cmdletInfoList.Count == 0)
          stringList1.Add(keyValuePair.Key);
      }
      foreach (string key in stringList1)
        cmdletCache.Remove(key);
      if (module.ModuleType == ModuleType.Binary)
      {
        Dictionary<string, List<ProviderInfo>> providers = this.Context.TopLevelSessionState.Providers;
        List<string> stringList2 = new List<string>();
        foreach (KeyValuePair<string, List<ProviderInfo>> keyValuePair in providers)
        {
          for (int index = keyValuePair.Value.Count - 1; index >= 0; --index)
          {
            ProviderInfo pi = keyValuePair.Value[index];
            if (pi.ImplementingType.Assembly.Location.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
            {
              InitialSessionState.RemoveAllDrivesForProvider(pi, this.Context.TopLevelSessionState);
              if (this.Context.EngineSessionState != this.Context.TopLevelSessionState)
                InitialSessionState.RemoveAllDrivesForProvider(pi, this.Context.EngineSessionState);
              foreach (PSModuleInfo psModuleInfo in this.Context.Modules.ModuleTable.Values)
              {
                if (psModuleInfo.SessionState != null)
                {
                  SessionStateInternal sessionStateInternal = psModuleInfo.SessionState.Internal;
                  if (sessionStateInternal != this.Context.TopLevelSessionState && sessionStateInternal != this.Context.EngineSessionState)
                    InitialSessionState.RemoveAllDrivesForProvider(pi, this.Context.EngineSessionState);
                }
              }
              keyValuePair.Value.RemoveAt(index);
            }
          }
          if (keyValuePair.Value.Count == 0)
            stringList2.Add(keyValuePair.Key);
        }
        foreach (string key in stringList2)
          providers.Remove(key);
      }
      SessionStateInternal engineSessionState = this.Context.EngineSessionState;
      if (module.SessionState != null)
      {
        foreach (DictionaryEntry dictionaryEntry in engineSessionState.GetFunctionTable())
        {
          FunctionInfo functionInfo = (FunctionInfo) dictionaryEntry.Value;
          if (functionInfo.Module != null)
          {
            if (functionInfo.Module.Path.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
            {
              try
              {
                engineSessionState.RemoveFunction(functionInfo.Name, true);
                this.WriteVerbose(ResourceManagerCache.FormatResourceString("Modules", "RemovingImportedFunction", (object) functionInfo.Name));
              }
              catch (SessionStateUnauthorizedAccessException ex)
              {
                this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "UnableToRemoveModuleMember", (object) functionInfo.Name, (object) module.Name, (object) ex.Message), (Exception) ex), "Modules_MemberNotRemoved", ErrorCategory.PermissionDenied, (object) functionInfo.Name));
              }
            }
          }
        }
        foreach (PSVariable exportedVariable in module.SessionState.Internal.ExportedVariables)
        {
          PSVariable variable = engineSessionState.GetVariable(exportedVariable.Name);
          if (variable != null && variable == exportedVariable)
          {
            engineSessionState.RemoveVariable(variable);
            this.WriteVerbose(ResourceManagerCache.FormatResourceString("Modules", "RemovingImportedVariable", (object) variable.Name));
          }
        }
        foreach (KeyValuePair<string, AliasInfo> keyValuePair in (IEnumerable<KeyValuePair<string, AliasInfo>>) engineSessionState.GetAliasTable())
        {
          AliasInfo aliasInfo = keyValuePair.Value;
          if (aliasInfo.Module != null && aliasInfo.Module.Path.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
          {
            engineSessionState.RemoveAlias(aliasInfo.Name, true);
            this.WriteVerbose(ResourceManagerCache.FormatResourceString("Modules", "RemovingImportedAlias", (object) aliasInfo.Name));
          }
        }
      }
      foreach (string exportedFormatFile in module.ExportedFormatFiles)
      {
        if (this.Context.RunspaceConfiguration != null)
        {
          for (int index = 0; index < this.Context.RunspaceConfiguration.Formats.Count; ++index)
          {
            if (this.Context.RunspaceConfiguration.Formats[index].FileName.Equals(exportedFormatFile, StringComparison.OrdinalIgnoreCase))
            {
              this.Context.RunspaceConfiguration.Formats.RemoveItem(index);
              break;
            }
          }
          this.Context.RunspaceConfiguration.Formats.Update();
        }
        else
        {
          InitialSessionStateEntryCollection<SessionStateFormatEntry> stateEntryCollection = new InitialSessionStateEntryCollection<SessionStateFormatEntry>();
          foreach (SessionStateFormatEntry format in (IEnumerable<SessionStateFormatEntry>) this.Context.InitialSessionState.Formats)
          {
            if (!format.FileName.Equals(exportedFormatFile, StringComparison.OrdinalIgnoreCase))
              stateEntryCollection.Add(format);
          }
          this.Context.InitialSessionState.Formats.Clear();
          this.Context.InitialSessionState.Formats.Add((IEnumerable<SessionStateFormatEntry>) stateEntryCollection);
          this.Context.InitialSessionState.UpdateFormats(this.Context, false);
        }
      }
      foreach (string exportedTypeFile in module.ExportedTypeFiles)
      {
        if (this.Context.RunspaceConfiguration != null)
        {
          for (int index = 0; index < this.Context.RunspaceConfiguration.Types.Count; ++index)
          {
            if (this.Context.RunspaceConfiguration.Types[index].FileName.Equals(exportedTypeFile, StringComparison.OrdinalIgnoreCase))
            {
              this.Context.RunspaceConfiguration.Types.RemoveItem(index);
              break;
            }
          }
          this.Context.RunspaceConfiguration.Types.Update();
        }
        else
        {
          InitialSessionStateEntryCollection<SessionStateTypeEntry> stateEntryCollection = new InitialSessionStateEntryCollection<SessionStateTypeEntry>();
          foreach (SessionStateTypeEntry type in (IEnumerable<SessionStateTypeEntry>) this.Context.InitialSessionState.Types)
          {
            if (!type.FileName.Equals(exportedTypeFile, StringComparison.OrdinalIgnoreCase))
              stateEntryCollection.Add(type);
          }
          this.Context.InitialSessionState.Types.Clear();
          this.Context.InitialSessionState.Types.Add((IEnumerable<SessionStateTypeEntry>) stateEntryCollection);
          this.Context.InitialSessionState.UpdateTypes(this.Context, false);
        }
      }
      foreach (KeyValuePair<string, PSModuleInfo> keyValuePair in this.Context.Modules.ModuleTable)
      {
        PSModuleInfo psModuleInfo = keyValuePair.Value;
        if (psModuleInfo.SessionState != null && psModuleInfo.SessionState.Internal.ModuleTable.ContainsKey(keyValuePair.Key))
          psModuleInfo.SessionState.Internal.ModuleTable.Remove(keyValuePair.Key);
      }
      if (this.Context.TopLevelSessionState.ModuleTable.ContainsKey(module.Path))
        this.Context.TopLevelSessionState.ModuleTable.Remove(module.Path);
      this.Context.Modules.ModuleTable.Remove(module.Path);
    }

    internal PSModuleInfo LoadUsingExtensions(
      string moduleName,
      string fileBaseName,
      string extension,
      string moduleBase,
      string prefix,
      SessionState ss,
      out bool found)
    {
      string[] strArray;
      if (!string.IsNullOrEmpty(extension))
        strArray = new string[1]{ extension };
      else
        strArray = ModuleIntrinsics.PSModuleExtensions;
      foreach (string str in strArray)
      {
        string resolvedPath = ModuleCmdletBase.GetResolvedPath(fileBaseName + str, this.Context);
        if (resolvedPath != null && (string.IsNullOrEmpty(this.Context.ModuleBeingProcessed) || !this.Context.ModuleBeingProcessed.Equals(resolvedPath, StringComparison.OrdinalIgnoreCase)))
        {
          PSModuleInfo psModuleInfo;
          this.Context.Modules.ModuleTable.TryGetValue(resolvedPath, out psModuleInfo);
          if (!this.BaseForce && psModuleInfo != null && (this.BaseVersion == (Version) null || psModuleInfo.Version >= this.BaseVersion))
          {
            psModuleInfo = this.Context.Modules.ModuleTable[resolvedPath];
            ModuleCmdletBase.AddModuleToModuleTables(this.Context, this.TargetSessionState.Internal, psModuleInfo);
            this.ImportModuleMembers(psModuleInfo, prefix);
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
            return psModuleInfo;
          }
          if (File.Exists(resolvedPath))
          {
            if (this.BaseForce && psModuleInfo != null)
              this.RemoveModule(psModuleInfo);
            psModuleInfo = this.LoadModule(resolvedPath, moduleBase, prefix, ss, out found);
            if (found)
              return psModuleInfo;
          }
        }
      }
      found = false;
      return (PSModuleInfo) null;
    }

    internal ExternalScriptInfo GetScriptInfoForFile(
      string fileName,
      out string scriptName)
    {
      scriptName = Path.GetFileName(fileName);
      ExternalScriptInfo scriptInfo = new ExternalScriptInfo(scriptName, fileName, this.Context);
      this.Context.AuthorizationManager.ShouldRunInternal((CommandInfo) scriptInfo, CommandOrigin.Internal, (PSHost) this.Context.EngineHostInterface);
      CommandDiscovery.VerifyPSVersion(scriptInfo);
      scriptInfo.SignatureChecked = true;
      return scriptInfo;
    }

    internal PSModuleInfo LoadModule(
      string fileName,
      string moduleBase,
      string prefix,
      SessionState ss,
      out bool found)
    {
      if (!File.Exists(fileName))
      {
        found = false;
        return (PSModuleInfo) null;
      }
      string extension = Path.GetExtension(fileName);
      if (this.BaseVersion != (Version) null)
      {
        if (!extension.Equals(".psd1", StringComparison.OrdinalIgnoreCase))
        {
          found = false;
          return (PSModuleInfo) null;
        }
        if (this.Context.Modules.ModuleTable.ContainsKey(fileName) && this.Context.Modules.ModuleTable[fileName].Version >= this.BaseVersion)
        {
          found = false;
          return (PSModuleInfo) null;
        }
      }
      PSModuleInfo psModuleInfo = (PSModuleInfo) null;
      found = false;
      string moduleBeingProcessed = this.Context.ModuleBeingProcessed;
      try
      {
        this.Context.ModuleBeingProcessed = fileName;
        this.WriteVerbose(ResourceManagerCache.FormatResourceString("Modules", "LoadingModule", (object) fileName));
        string scriptName;
        if (extension.Equals(".psm1", StringComparison.OrdinalIgnoreCase))
        {
          ExternalScriptInfo scriptInfoForFile = this.GetScriptInfoForFile(fileName, out scriptName);
          try
          {
            this.Context.Modules.IncrementModuleNestingDepth((PSCmdlet) this, scriptInfoForFile.Path);
            try
            {
              psModuleInfo = this.Context.Modules.CreateModule(fileName, scriptInfoForFile, this.MyInvocation.ScriptToken, ss, this._arguments);
              psModuleInfo.SetModuleBase(moduleBase);
              if (!psModuleInfo.SessionState.Internal.UseExportList)
                ModuleIntrinsics.ExportModuleMembers((PSCmdlet) this, psModuleInfo.SessionState.Internal, this.MatchAll, this.MatchAll, (List<WildcardPattern>) null, (List<WildcardPattern>) null);
              this.ImportModuleMembers(psModuleInfo, prefix);
              ModuleCmdletBase.AddModuleToModuleTables(this.Context, this.TargetSessionState.Internal, psModuleInfo);
              found = true;
              if (this.BaseAsCustomObject)
                this.WriteObject((object) psModuleInfo.AsCustomObject());
              else if (this.BasePassThru)
                this.WriteObject((object) psModuleInfo);
            }
            catch (RuntimeException ex)
            {
              ex.ErrorRecord.PreserveInvocationInfoOnce = true;
              this.WriteError(ex.ErrorRecord);
            }
          }
          finally
          {
            this.Context.Modules.DecrementModuleNestingCount();
          }
        }
        else if (extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase))
        {
          ExternalScriptInfo scriptInfoForFile = this.GetScriptInfoForFile(fileName, out scriptName);
          this.WriteVerbose(ResourceManagerCache.FormatResourceString("Modules", "DottingScriptFile", (object) fileName));
          try
          {
            found = true;
            object variableValue1 = this.Context.EngineSessionState.GetVariableValue("PSScriptRoot");
            object variableValue2 = this.Context.EngineSessionState.GetVariableValue("MyInvocation");
            try
            {
              this.Context.EngineSessionState.SetVariableValue("PSScriptRoot", (object) Path.GetDirectoryName(fileName), CommandOrigin.Internal);
              this.Context.EngineSessionState.SetVariableValue("MyInvocation", (object) new InvocationInfo((CommandInfo) scriptInfoForFile, (Token) null), CommandOrigin.Internal);
              scriptInfoForFile.ScriptBlock.InvokeUsingCmdlet((Cmdlet) this, false, true, (object) AutomationNull.Value, (object) AutomationNull.Value, (object) AutomationNull.Value);
            }
            finally
            {
              this.Context.EngineSessionState.SetVariableValue("PSScriptRoot", variableValue1);
              this.Context.EngineSessionState.SetVariableValue("MyInvocation", variableValue2);
            }
          }
          catch (RuntimeException ex)
          {
            ex.ErrorRecord.PreserveInvocationInfoOnce = true;
            this.WriteError(ex.ErrorRecord);
          }
          catch (ExitException ex)
          {
            this.Context.SetVariable("global:LASTEXITCODE", (object) (int) ex.Argument);
          }
        }
        else if (extension.Equals(".psd1", StringComparison.OrdinalIgnoreCase))
        {
          ExternalScriptInfo scriptInfoForFile = this.GetScriptInfoForFile(fileName, out scriptName);
          found = true;
          psModuleInfo = this.LoadModuleManifest(scriptInfoForFile, ModuleCmdletBase.ManifestProcessingFlags.WriteErrors | ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError | ModuleCmdletBase.ManifestProcessingFlags.LoadElements, this.BaseVersion);
          if (psModuleInfo != null)
          {
            ModuleCmdletBase.AddModuleToModuleTables(this.Context, this.TargetSessionState.Internal, psModuleInfo);
            if (this.BasePassThru)
              this.WriteObject((object) psModuleInfo);
          }
          else if (this.BaseVersion != (Version) null)
            found = false;
        }
        else if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
        {
          psModuleInfo = this.LoadBinaryModule(false, ModuleIntrinsics.GetModuleName(fileName), fileName, (Assembly) null, moduleBase, ss, prefix, true, true, out found);
          if (found = psModuleInfo != null)
          {
            ModuleCmdletBase.AddModuleToModuleTables(this.Context, this.TargetSessionState.Internal, psModuleInfo);
            if (this.BaseAsCustomObject)
              this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "CantUseAsCustomObjectWithBinaryModule", (object) fileName)), "Modules_CantUseAsCustomObjectWithBinaryModule", ErrorCategory.PermissionDenied, (object) null));
            else if (this.BasePassThru)
              this.WriteObject((object) psModuleInfo);
          }
        }
        else
        {
          found = true;
          this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "InvalidModuleExtension", (object) extension, (object) fileName)), "Modules_InvalidModuleExtension", ErrorCategory.PermissionDenied, (object) null));
        }
      }
      finally
      {
        this.Context.ModuleBeingProcessed = moduleBeingProcessed;
      }
      return psModuleInfo;
    }

    internal PSModuleInfo LoadBinaryModule(
      bool trySnapInName,
      string moduleName,
      string fileName,
      Assembly assemblyToLoad,
      string moduleBase,
      SessionState ss,
      string prefix,
      bool loadTypes,
      bool loadFormats,
      out bool found)
    {
      if (string.IsNullOrEmpty(moduleName) && string.IsNullOrEmpty(fileName) && assemblyToLoad == null)
        throw ModuleCmdletBase.tracer.NewArgumentNullException("moduleName,fileName,assemblyToLoad");
      InitialSessionState initialSessionState = InitialSessionState.Create();
      Exception error = (Exception) null;
      bool flag = false;
      string path = string.Empty;
      Version version = new Version(0, 0, 0, 0);
      if (assemblyToLoad != null)
      {
        if (!string.IsNullOrEmpty(fileName))
        {
          path = fileName;
        }
        else
        {
          path = assemblyToLoad.Location;
          if (string.IsNullOrEmpty(fileName))
            fileName = assemblyToLoad.FullName;
        }
        if (string.IsNullOrEmpty(moduleName))
          moduleName = "dynamic_code_module_" + (object) assemblyToLoad.GetName();
        initialSessionState.ImportCmdletsFromAssembly(assemblyToLoad, out PSSnapInException _);
        version = ModuleCmdletBase.GetAssemblyVersionNumber(assemblyToLoad);
      }
      else
      {
        PSSnapInException warning;
        if (trySnapInName && PSSnapInInfo.IsPSSnapinIdValid(moduleName))
        {
          PSSnapInInfo psSnapInInfo = (PSSnapInInfo) null;
          try
          {
            psSnapInInfo = initialSessionState.ImportPSSnapIn(moduleName, out warning);
          }
          catch (PSArgumentException ex)
          {
          }
          if (psSnapInInfo != null)
          {
            flag = true;
            path = !string.IsNullOrEmpty(fileName) ? fileName : psSnapInInfo.AbsoluteModulePath;
            version = psSnapInInfo.Version;
            if (!loadTypes)
              initialSessionState.Types.Reset();
            if (!loadFormats)
              initialSessionState.Formats.Reset();
          }
        }
        if (!flag)
        {
          Assembly assembly = this.Context.AddAssembly(moduleName, fileName, out error);
          if (assembly == null)
          {
            if (error != null)
              throw error;
            found = false;
            return (PSModuleInfo) null;
          }
          version = ModuleCmdletBase.GetAssemblyVersionNumber(assembly);
          path = !string.IsNullOrEmpty(fileName) ? fileName : assembly.Location;
          initialSessionState.ImportCmdletsFromAssembly(assembly, out warning);
        }
      }
      found = true;
      PSModuleInfo module = new PSModuleInfo(moduleName, path, this.Context, ss);
      module.SetModuleType(ModuleType.Binary);
      module.SetModuleBase(moduleBase);
      module.SetVersion(version);
      List<string> stringList1 = new List<string>();
      foreach (SessionStateTypeEntry type in (IEnumerable<SessionStateTypeEntry>) initialSessionState.Types)
        stringList1.Add(type.FileName);
      if (stringList1.Count > 0)
        module.SetExportedTypeFiles(new ReadOnlyCollection<string>((IList<string>) stringList1));
      List<string> stringList2 = new List<string>();
      foreach (SessionStateFormatEntry format in (IEnumerable<SessionStateFormatEntry>) initialSessionState.Formats)
        stringList2.Add(format.FileName);
      if (stringList2.Count > 0)
        module.SetExportedFormatFiles(new ReadOnlyCollection<string>((IList<string>) stringList2));
      foreach (InitialSessionStateEntry provider in (IEnumerable<SessionStateProviderEntry>) initialSessionState.Providers)
        provider.SetModule(module);
      foreach (SessionStateCommandEntry command in (IEnumerable<SessionStateCommandEntry>) initialSessionState.Commands)
      {
        command.SetModule(module);
        SessionStateCmdletEntry entry = command as SessionStateCmdletEntry;
        if (ss != null)
          ss.Internal.ExportedCmdlets.Add(CommandDiscovery.NewCmdletInfo(entry, this.Context));
        else
          module.AddExportedCmdlet(CommandDiscovery.NewCmdletInfo(entry, this.Context));
      }
      if (this.BaseCmdletPatterns != null)
      {
        InitialSessionStateEntryCollection<SessionStateCommandEntry> commands = initialSessionState.Commands;
        for (int index = commands.Count - 1; index >= 0; --index)
        {
          SessionStateCommandEntry stateCommandEntry = commands[index];
          if (stateCommandEntry != null)
          {
            string name = stateCommandEntry.Name;
            if (!string.IsNullOrEmpty(name) && !SessionStateUtilities.MatchesAnyWildcardPattern(name, (IEnumerable<WildcardPattern>) this.BaseCmdletPatterns, false))
              commands.RemoveItem(index);
          }
        }
      }
      foreach (SessionStateCommandEntry command in (IEnumerable<SessionStateCommandEntry>) initialSessionState.Commands)
        command.Name = ModuleCmdletBase.AddPrefixToCommandName(command.Name, prefix);
      SessionStateInternal engineSessionState = this.Context.EngineSessionState;
      try
      {
        if (ss != null)
          this.Context.EngineSessionState = ss.Internal;
        initialSessionState.Bind(this.Context, true, module);
      }
      finally
      {
        this.Context.EngineSessionState = engineSessionState;
      }
      string str = module.Name + "\\";
      bool checkVerb = !this._disableNameChecking;
      bool checkNoun = !this._disableNameChecking;
      foreach (SessionStateCommandEntry command in (IEnumerable<SessionStateCommandEntry>) initialSessionState.Commands)
      {
        try
        {
          switch (command)
          {
            case SessionStateCmdletEntry _:
            case SessionStateFunctionEntry _:
              ModuleCmdletBase.ValidateCommandName(this, command.Name, ref checkVerb, ref checkNoun);
              break;
          }
          CommandInvocationIntrinsics.GetCmdlet(str + command.Name, this.Context);
        }
        catch (CommandNotFoundException ex)
        {
          this.WriteError(ex.ErrorRecord);
        }
        this.WriteVerbose(ResourceManagerCache.FormatResourceString("Modules", "ImportingCmdlet", (object) command.Name));
      }
      ModuleCmdletBase.AddModuleToModuleTables(this.Context, this.TargetSessionState.Internal, module);
      return module;
    }

    private static Version GetAssemblyVersionNumber(Assembly assemblyToLoad)
    {
      try
      {
        return new AssemblyName(assemblyToLoad.FullName).Version;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        return new Version();
      }
    }

    internal static string AddPrefixToCommandName(string commandName, string prefix)
    {
      if (string.IsNullOrEmpty(prefix))
        return commandName;
      string verb;
      string noun;
      commandName = !CmdletInfo.SplitCmdletName(commandName, out verb, out noun) ? prefix + commandName : verb + "-" + prefix + noun;
      return commandName;
    }

    internal static void AddModuleToModuleTables(
      ExecutionContext context,
      SessionStateInternal targetSessionState,
      PSModuleInfo module)
    {
      if (!context.Modules.ModuleTable.ContainsKey(module.Path))
        context.Modules.ModuleTable.Add(module.Path, module);
      if (!targetSessionState.ModuleTable.ContainsKey(module.Path))
        targetSessionState.ModuleTable.Add(module.Path, module);
      if (targetSessionState.Module == null)
        return;
      targetSessionState.Module.AddNestedModule(module);
    }

    protected internal void ImportModuleMembers(PSModuleInfo sourceModule, string prefix) => ModuleCmdletBase.ImportModuleMembers(this, this.TargetSessionState.Internal, sourceModule, prefix, this.BaseFunctionPatterns, this.BaseCmdletPatterns, this.BaseVariablePatterns, this.BaseAliasPatterns);

    internal static void ImportModuleMembers(
      ModuleCmdletBase cmdlet,
      SessionStateInternal targetSessionState,
      PSModuleInfo sourceModule,
      string prefix,
      List<WildcardPattern> functionPatterns,
      List<WildcardPattern> cmdletPatterns,
      List<WildcardPattern> variablePatterns,
      List<WildcardPattern> aliasPatterns)
    {
      if (sourceModule == null)
        throw ModuleCmdletBase.tracer.NewArgumentNullException(nameof (sourceModule));
      bool flag1 = !string.IsNullOrEmpty(prefix);
      bool flag2 = !cmdlet.BaseDisableNameChecking;
      bool flag3 = !cmdlet.BaseDisableNameChecking;
      if (targetSessionState.Module != null)
      {
        bool flag4 = false;
        foreach (PSModuleInfo nestedModule in targetSessionState.Module.NestedModules)
        {
          if (nestedModule.Path.Equals(sourceModule.Path, StringComparison.OrdinalIgnoreCase))
            flag4 = true;
        }
        if (!flag4)
          targetSessionState.Module.AddNestedModule(sourceModule);
      }
      SessionStateInternal sessionState = (SessionStateInternal) null;
      if (sourceModule.SessionState != null)
        sessionState = sourceModule.SessionState.Internal;
      bool defaultValue = functionPatterns == null && variablePatterns == null && aliasPatterns == null && cmdletPatterns == null;
      Dictionary<string, string> dictionary = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (CmdletInfo compiledExport in sourceModule.CompiledExports)
      {
        if (SessionStateUtilities.MatchesAnyWildcardPattern(compiledExport.Name, (IEnumerable<WildcardPattern>) cmdletPatterns, defaultValue))
        {
          CmdletInfo newCmdletInfo = new CmdletInfo(ModuleCmdletBase.AddPrefixToCommandName(compiledExport.Name, prefix), compiledExport.ImplementingType, compiledExport.HelpFile, compiledExport.PSSnapIn, cmdlet.Context);
          newCmdletInfo.SetModule(sourceModule);
          if (flag1)
            dictionary.Add(compiledExport.Name, newCmdletInfo.Name);
          ModuleCmdletBase.ValidateCommandName(cmdlet, newCmdletInfo.Name, ref flag2, ref flag3);
          CommandDiscovery.AddCmdletInfoToCache(targetSessionState, newCmdletInfo);
          string text = ResourceManagerCache.FormatResourceString("Modules", "ImportingCmdlet", (object) newCmdletInfo.Name);
          cmdlet.WriteVerbose(text);
        }
      }
      if (sessionState == null)
        return;
      foreach (FunctionInfo functionInfo1 in sourceModule.ExportedFunctions.Values)
      {
        if (SessionStateUtilities.MatchesAnyWildcardPattern(functionInfo1.Name, (IEnumerable<WildcardPattern>) functionPatterns, defaultValue))
        {
          string commandName = ModuleCmdletBase.AddPrefixToCommandName(functionInfo1.Name, prefix);
          if (flag1)
            dictionary.Add(functionInfo1.Name, commandName);
          FunctionInfo functionInfo2 = targetSessionState.ModuleScope.SetFunction(commandName, functionInfo1.ScriptBlock, false, CommandOrigin.Internal, targetSessionState.ExecutionContext);
          functionInfo2.SetModule(sourceModule);
          ModuleCmdletBase.ValidateCommandName(cmdlet, functionInfo2.Name, ref flag3, ref flag2);
          string text = ResourceManagerCache.FormatResourceString("Modules", "ImportingFunction", (object) commandName);
          cmdlet.WriteVerbose(text);
        }
      }
      foreach (PSVariable newVariable in sourceModule.ExportedVariables.Values)
      {
        if (SessionStateUtilities.MatchesAnyWildcardPattern(newVariable.Name, (IEnumerable<WildcardPattern>) variablePatterns, defaultValue))
        {
          newVariable.SetModule(sourceModule);
          targetSessionState.ModuleScope.NewVariable(newVariable, true, sessionState);
          string text = ResourceManagerCache.FormatResourceString("Modules", "ImportingVariable", (object) newVariable.Name);
          cmdlet.WriteVerbose(text);
        }
      }
      foreach (AliasInfo aliasInfo in sourceModule.ExportedAliases.Values)
      {
        if (SessionStateUtilities.MatchesAnyWildcardPattern(aliasInfo.Name, (IEnumerable<WildcardPattern>) aliasPatterns, defaultValue))
        {
          string commandName = ModuleCmdletBase.AddPrefixToCommandName(aliasInfo.Name, prefix);
          string definition;
          if (!flag1 || !dictionary.TryGetValue(aliasInfo.Definition, out definition))
            definition = aliasInfo.Definition;
          AliasInfo aliasToSet = new AliasInfo(commandName, definition, cmdlet.Context);
          aliasToSet.SetModule(sourceModule);
          if (flag1)
            dictionary.Add(aliasInfo.Name, aliasToSet.Name);
          targetSessionState.ModuleScope.SetAliasItem(aliasToSet, false, CommandOrigin.Internal);
          string text = ResourceManagerCache.FormatResourceString("Modules", "ImportingAlias", (object) aliasToSet.Name);
          cmdlet.WriteVerbose(text);
        }
      }
    }

    private static void ValidateCommandName(
      ModuleCmdletBase cmdlet,
      string commandName,
      ref bool checkVerb,
      ref bool checkNoun)
    {
      string verb;
      string noun;
      if (!CmdletInfo.SplitCmdletName(commandName, out verb, out noun))
        return;
      if (!Verbs.IsStandard(verb))
      {
        if (checkVerb)
        {
          string resourceString = ResourceManagerCache.GetResourceString("Modules", "ImportingNonStandardVerb");
          cmdlet.WriteWarning(resourceString);
          checkVerb = false;
        }
        string[] strArray = Verbs.SuggestedAlternates(verb);
        if (strArray == null)
        {
          string text = ResourceManagerCache.FormatResourceString("Modules", "ImportingNonStandardVerbVerbose", (object) commandName);
          cmdlet.WriteVerbose(text);
        }
        else
        {
          string resourceString = ResourceManagerCache.GetResourceString("ExtendedTypeSystem", "ListSeparator");
          StringBuilder stringBuilder = new StringBuilder();
          foreach (string str in strArray)
          {
            stringBuilder.Append(str);
            stringBuilder.Append(resourceString);
          }
          stringBuilder.Remove(stringBuilder.Length - resourceString.Length, resourceString.Length);
          string text = ResourceManagerCache.FormatResourceString("Modules", "ImportingNonStandardVerbVerboseSuggestion", (object) commandName, (object) stringBuilder);
          cmdlet.WriteVerbose(text);
        }
      }
      foreach (char ch in noun)
      {
        switch (ch)
        {
          case '"':
          case '#':
          case '$':
          case '%':
          case '&':
          case '\'':
          case '(':
          case ')':
          case '*':
          case '+':
          case ',':
          case '-':
          case '/':
          case ':':
          case ';':
          case '<':
          case '=':
          case '>':
          case '?':
          case '@':
          case '[':
          case '\\':
          case ']':
          case '^':
          case '`':
          case '{':
          case '|':
          case '}':
          case '~':
            if (checkNoun)
            {
              string resourceString = ResourceManagerCache.GetResourceString("Modules", "ImportingNonStandardNoun");
              cmdlet.WriteWarning(resourceString);
              checkNoun = false;
            }
            string text = ResourceManagerCache.FormatResourceString("Modules", "ImportingNonStandardNounVerbose", (object) commandName);
            cmdlet.WriteVerbose(text);
            return;
          default:
            continue;
        }
      }
    }

    [System.Flags]
    internal enum ManifestProcessingFlags
    {
      WriteErrors = 1,
      NullOnFirstError = 2,
      LoadElements = 4,
    }
  }
}
