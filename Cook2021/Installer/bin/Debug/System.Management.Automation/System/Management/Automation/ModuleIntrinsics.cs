// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ModuleIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Internal;
using System.Security;

namespace System.Management.Automation
{
  internal class ModuleIntrinsics
  {
    [TraceSource("SessionState", "SessionState Class")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("SessionState", "Module Intrinsics Class");
    private ExecutionContext _context;
    private Dictionary<string, PSModuleInfo> _moduleTable = new Dictionary<string, PSModuleInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private static int MaxModuleNestingDepth = 10;
    private int _moduleNestingDepth;
    internal static string[] PSModuleProcessableExtensions = new string[4]
    {
      ".psd1",
      ".ps1",
      ".psm1",
      ".dll"
    };
    internal static string[] PSModuleExtensions = new string[3]
    {
      ".psd1",
      ".psm1",
      ".dll"
    };

    internal ModuleIntrinsics(ExecutionContext context)
    {
      this._context = context;
      ModuleIntrinsics.SetModulePath();
    }

    internal Dictionary<string, PSModuleInfo> ModuleTable => this._moduleTable;

    internal void IncrementModuleNestingDepth(PSCmdlet cmdlet, string path)
    {
      if (++this._moduleNestingDepth <= ModuleIntrinsics.MaxModuleNestingDepth)
        return;
      ErrorRecord errorRecord = new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "ModuleTooDeeplyNested", (object) path, (object) ModuleIntrinsics.MaxModuleNestingDepth)), "Modules_ModuleTooDeeplyNested", ErrorCategory.InvalidOperation, (object) path);
      cmdlet.ThrowTerminatingError(errorRecord);
    }

    internal void DecrementModuleNestingCount() => --this._moduleNestingDepth;

    internal int ModuleNestingDepth => this._moduleNestingDepth;

    internal PSModuleInfo CreateModule(
      string name,
      string path,
      ScriptBlock scriptBlock,
      SessionState ss,
      out ArrayList results,
      params object[] arguments)
    {
      return this.CreateModuleImplementation(name, path, (object) scriptBlock, (Token) null, ss, out results, arguments);
    }

    internal PSModuleInfo CreateModule(
      string path,
      ExternalScriptInfo scriptInfo,
      Token callerToken,
      SessionState ss,
      params object[] arguments)
    {
      return this.CreateModuleImplementation(ModuleIntrinsics.GetModuleName(path), path, (object) scriptInfo, callerToken, ss, out ArrayList _, arguments);
    }

    private PSModuleInfo CreateModuleImplementation(
      string name,
      string path,
      object moduleCode,
      Token callerToken,
      SessionState ss,
      out ArrayList result,
      params object[] arguments)
    {
      if (ss == null)
        ss = new SessionState(this._context.EngineSessionState, true, true);
      SessionStateInternal engineSessionState = this._context.EngineSessionState;
      PSModuleInfo psModuleInfo = new PSModuleInfo(name, path, this._context, ss);
      ss.Internal.Module = psModuleInfo;
      bool flag = false;
      int num = 0;
      try
      {
        ArrayList resultList = (ArrayList) null;
        this._context.EngineSessionState = ss.Internal;
        this._context.EngineSessionState.SetVariableValue("PSScriptRoot", (object) Path.GetDirectoryName(path), CommandOrigin.Internal);
        switch (moduleCode)
        {
          case ExternalScriptInfo externalScriptInfo:
            this._context.EngineSessionState.SetVariableValue("MyInvocation", (object) new InvocationInfo((CommandInfo) externalScriptInfo, callerToken), CommandOrigin.Internal);
            scriptBlock = externalScriptInfo.ScriptBlock;
            break;
          case ScriptBlock scriptBlock:
            scriptBlock = scriptBlock.Clone(true);
            scriptBlock.SessionState = ss;
            break;
          case string _:
            scriptBlock = ScriptBlock.Create(this._context, (string) moduleCode);
            break;
        }
        if (scriptBlock == null)
          throw ModuleIntrinsics.tracer.NewInvalidOperationException();
        scriptBlock.SessionStateInternal = ss.Internal;
        try
        {
          if (arguments == null)
            scriptBlock.InvokeWithPipe(false, true, (object) AutomationNull.Value, (object) AutomationNull.Value, (object) AutomationNull.Value, (Pipe) null, ref resultList);
          else
            scriptBlock.InvokeWithPipe(false, true, (object) AutomationNull.Value, (object) AutomationNull.Value, (object) AutomationNull.Value, (Pipe) null, ref resultList, arguments);
        }
        catch (ExitException ex)
        {
          num = (int) ex.Argument;
          flag = true;
        }
        result = resultList;
      }
      finally
      {
        this._context.EngineSessionState = engineSessionState;
      }
      if (flag)
        this._context.SetVariable("global:LASTEXITCODE", (object) num);
      return psModuleInfo;
    }

    internal ScriptBlock CreateBoundScriptBlock(ScriptBlock sb, bool linkToGlobal) => new PSModuleInfo(linkToGlobal).NewBoundScriptBlock(sb);

    internal List<PSModuleInfo> GetModules(string[] patterns, bool all)
    {
      if (patterns == null)
        patterns = new string[1]{ "*" };
      List<WildcardPattern> wildcardPatternList = new List<WildcardPattern>();
      foreach (string pattern in patterns)
        wildcardPatternList.Add(new WildcardPattern(pattern, WildcardOptions.IgnoreCase));
      List<PSModuleInfo> psModuleInfoList = new List<PSModuleInfo>();
      if (all)
      {
        foreach (string key in this.ModuleTable.Keys)
        {
          PSModuleInfo psModuleInfo = this.ModuleTable[key];
          if (SessionStateUtilities.MatchesAnyWildcardPattern(psModuleInfo.Name, (IEnumerable<WildcardPattern>) wildcardPatternList, false))
            psModuleInfoList.Add(psModuleInfo);
        }
      }
      else
      {
        Dictionary<string, bool> dictionary = new Dictionary<string, bool>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        foreach (string key in this._context.EngineSessionState.ModuleTable.Keys)
        {
          PSModuleInfo psModuleInfo = this._context.EngineSessionState.ModuleTable[key];
          if (SessionStateUtilities.MatchesAnyWildcardPattern(psModuleInfo.Name, (IEnumerable<WildcardPattern>) wildcardPatternList, false))
          {
            psModuleInfoList.Add(psModuleInfo);
            dictionary[key] = true;
          }
        }
        if (this._context.EngineSessionState != this._context.TopLevelSessionState)
        {
          foreach (string key in this._context.TopLevelSessionState.ModuleTable.Keys)
          {
            if (!dictionary.ContainsKey(key))
            {
              PSModuleInfo psModuleInfo = this.ModuleTable[key];
              if (SessionStateUtilities.MatchesAnyWildcardPattern(psModuleInfo.Name, (IEnumerable<WildcardPattern>) wildcardPatternList, false))
                psModuleInfoList.Add(psModuleInfo);
            }
          }
        }
      }
      return psModuleInfoList;
    }

    internal static bool IsPowerShellModuleExtension(string extension)
    {
      foreach (string processableExtension in ModuleIntrinsics.PSModuleProcessableExtensions)
      {
        if (extension.Equals(processableExtension, StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    internal static string GetModuleName(string path)
    {
      string path1 = path == null ? string.Empty : Path.GetFileName(path);
      string extension = Path.GetExtension(path1);
      return !string.IsNullOrEmpty(extension) && ModuleIntrinsics.IsPowerShellModuleExtension(extension) ? path1.Substring(0, path1.Length - extension.Length) : path1;
    }

    internal static string GetPersonalModulePath() => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Utils.ProductNameForDirectory), Utils.ModuleDirectory);

    private static string GetSystemwideModulePath()
    {
      string str = (string) null;
      string powerShellShellId = Utils.DefaultPowerShellShellID;
      string path1 = (string) null;
      try
      {
        path1 = Utils.GetApplicationBase(powerShellShellId);
      }
      catch (SecurityException ex)
      {
        ModuleIntrinsics.tracer.TraceException((Exception) ex);
      }
      if (!string.IsNullOrEmpty(path1))
        str = Path.Combine(path1, Utils.ModuleDirectory);
      return str;
    }

    private static string GetExpandedEnvironmentVariable(
      string name,
      EnvironmentVariableTarget target)
    {
      string name1 = Environment.GetEnvironmentVariable(name, target);
      if (!string.IsNullOrEmpty(name1))
        name1 = Environment.ExpandEnvironmentVariables(name1);
      return name1;
    }

    internal static void SetModulePath()
    {
      string environmentVariable1 = ModuleIntrinsics.GetExpandedEnvironmentVariable("PSMODULEPATH", EnvironmentVariableTarget.Process);
      string environmentVariable2 = ModuleIntrinsics.GetExpandedEnvironmentVariable("PSMODULEPATH", EnvironmentVariableTarget.Machine);
      string environmentVariable3 = ModuleIntrinsics.GetExpandedEnvironmentVariable("PSMODULEPATH", EnvironmentVariableTarget.User);
      string str1;
      if (environmentVariable1 == null)
      {
        string str2 = (environmentVariable3 ?? ModuleIntrinsics.GetPersonalModulePath()) + (object) ';';
        str1 = environmentVariable2 != null ? str2 + environmentVariable2 : str2 + ModuleIntrinsics.GetSystemwideModulePath();
      }
      else if (environmentVariable2 != null)
      {
        if (environmentVariable3 == null)
        {
          if (!environmentVariable2.Equals(environmentVariable1, StringComparison.OrdinalIgnoreCase))
            return;
          str1 = ModuleIntrinsics.GetPersonalModulePath() + (object) ';' + environmentVariable2;
        }
        else
        {
          string str2 = environmentVariable3 + (object) ';' + environmentVariable2;
          if (!str2.Equals(environmentVariable1, StringComparison.OrdinalIgnoreCase) && !environmentVariable2.Equals(environmentVariable1, StringComparison.OrdinalIgnoreCase) && !environmentVariable3.Equals(environmentVariable1, StringComparison.OrdinalIgnoreCase))
            return;
          str1 = str2;
        }
      }
      else
      {
        if (environmentVariable3 == null || !environmentVariable3.Equals(environmentVariable1, StringComparison.OrdinalIgnoreCase))
          return;
        str1 = environmentVariable3 + (object) ';' + ModuleIntrinsics.GetSystemwideModulePath();
      }
      Environment.SetEnvironmentVariable("PSMODULEPATH", str1);
    }

    internal static IEnumerable<string> GetModulePath(ExecutionContext context)
    {
      List<string> stringList = new List<string>();
      string environmentVariable = Environment.GetEnvironmentVariable("PSMODULEPATH");
      if (environmentVariable == null)
      {
        ModuleIntrinsics.SetModulePath();
        environmentVariable = Environment.GetEnvironmentVariable("PSMODULEPATH");
      }
      if (environmentVariable.Trim().Length == 0)
        return (IEnumerable<string>) stringList;
      string str1 = environmentVariable;
      char[] separator = new char[1]{ ';' };
      foreach (string pattern in str1.Split(separator, StringSplitOptions.RemoveEmptyEntries))
      {
        try
        {
          ProviderInfo provider = (ProviderInfo) null;
          IEnumerable<string> providerPathFromPsPath = (IEnumerable<string>) context.SessionState.Path.GetResolvedProviderPathFromPSPath(WildcardPattern.Escape(pattern), out provider);
          if (provider.NameEquals(context.ProviderNames.FileSystem))
          {
            foreach (string str2 in providerPathFromPsPath)
              stringList.Add(str2);
          }
        }
        catch (ItemNotFoundException ex)
        {
        }
      }
      return (IEnumerable<string>) stringList;
    }

    private static void SortAndRemoveDuplicates<T>(List<T> input, Converter<T, string> keyGetter)
    {
      input.Sort((Comparison<T>) ((x, y) => string.Compare(keyGetter(x), keyGetter(y), true)));
      bool flag = true;
      string str1 = (string) null;
      List<T> objList = new List<T>(input.Count);
      foreach (T input1 in input)
      {
        string str2 = keyGetter(input1);
        if (flag || !str2.Equals(str1, StringComparison.OrdinalIgnoreCase))
          objList.Add(input1);
        str1 = str2;
        flag = false;
      }
      input.Clear();
      input.AddRange((IEnumerable<T>) objList);
    }

    internal static void ExportModuleMembers(
      PSCmdlet cmdlet,
      SessionStateInternal sessionState,
      List<WildcardPattern> functionPatterns,
      List<WildcardPattern> cmdletPatterns,
      List<WildcardPattern> aliasPatterns,
      List<WildcardPattern> variablePatterns)
    {
      sessionState.UseExportList = true;
      if (functionPatterns != null)
      {
        foreach (KeyValuePair<string, FunctionInfo> keyValuePair in (IEnumerable<KeyValuePair<string, FunctionInfo>>) sessionState.ModuleScope.FunctionTable)
        {
          if (SessionStateUtilities.MatchesAnyWildcardPattern(keyValuePair.Key, (IEnumerable<WildcardPattern>) functionPatterns, false))
          {
            string text = ResourceManagerCache.FormatResourceString("Modules", "ExportingFunction", (object) keyValuePair.Key);
            cmdlet.WriteVerbose(text);
            sessionState.ExportedFunctions.Add(keyValuePair.Value);
          }
        }
        ModuleIntrinsics.SortAndRemoveDuplicates<FunctionInfo>(sessionState.ExportedFunctions, (Converter<FunctionInfo, string>) (ci => ci.Name));
      }
      if (cmdletPatterns != null)
      {
        IDictionary<string, List<CmdletInfo>> cmdletCache = (IDictionary<string, List<CmdletInfo>>) sessionState.CmdletCache;
        if (sessionState.Module.CompiledExports.Count > 0)
        {
          CmdletInfo[] array = sessionState.Module.CompiledExports.ToArray();
          sessionState.Module.CompiledExports.Clear();
          foreach (CmdletInfo cmdletInfo1 in array)
          {
            if (SessionStateUtilities.MatchesAnyWildcardPattern(cmdletInfo1.Name, (IEnumerable<WildcardPattern>) cmdletPatterns, false))
            {
              string text = ResourceManagerCache.FormatResourceString("Modules", "ExportingCmdlet", (object) cmdletInfo1.Name);
              cmdlet.WriteVerbose(text);
              CmdletInfo cmdletInfo2 = new CmdletInfo(cmdletInfo1.Name, cmdletInfo1.ImplementingType, cmdletInfo1.HelpFile, (PSSnapInInfo) null, cmdletInfo1.Context);
              cmdletInfo2.SetModule(sessionState.Module);
              sessionState.Module.CompiledExports.Add(cmdletInfo2);
            }
          }
        }
        foreach (KeyValuePair<string, List<CmdletInfo>> keyValuePair in (IEnumerable<KeyValuePair<string, List<CmdletInfo>>>) cmdletCache)
        {
          if (SessionStateUtilities.MatchesAnyWildcardPattern(keyValuePair.Key, (IEnumerable<WildcardPattern>) cmdletPatterns, false))
          {
            string text = ResourceManagerCache.FormatResourceString("Modules", "ExportingCmdlet", (object) keyValuePair.Key);
            cmdlet.WriteVerbose(text);
            CmdletInfo cmdletInfo1 = keyValuePair.Value[0];
            CmdletInfo cmdletInfo2 = new CmdletInfo(cmdletInfo1.Name, cmdletInfo1.ImplementingType, cmdletInfo1.HelpFile, (PSSnapInInfo) null, cmdletInfo1.Context);
            cmdletInfo2.SetModule(sessionState.Module);
            sessionState.Module.CompiledExports.Add(cmdletInfo2);
          }
        }
        ModuleIntrinsics.SortAndRemoveDuplicates<CmdletInfo>(sessionState.Module.CompiledExports, (Converter<CmdletInfo, string>) (ci => ci.Name));
      }
      if (variablePatterns != null)
      {
        foreach (KeyValuePair<string, PSVariable> variable in (IEnumerable<KeyValuePair<string, PSVariable>>) sessionState.ModuleScope.Variables)
        {
          if (!variable.Value.IsAllScope && Array.IndexOf<string>(PSModuleInfo.builtinVariables, variable.Key) == -1 && SessionStateUtilities.MatchesAnyWildcardPattern(variable.Key, (IEnumerable<WildcardPattern>) variablePatterns, false))
          {
            string text = ResourceManagerCache.FormatResourceString("Modules", "ExportingVariable", (object) variable.Key);
            cmdlet.WriteVerbose(text);
            sessionState.ExportedVariables.Add(variable.Value);
          }
        }
        ModuleIntrinsics.SortAndRemoveDuplicates<PSVariable>(sessionState.ExportedVariables, (Converter<PSVariable, string>) (v => v.Name));
      }
      if (aliasPatterns == null)
        return;
      foreach (AliasInfo aliasInfo in sessionState.ModuleScope.AliasTable)
      {
        if ((aliasInfo.Options & ScopedItemOptions.AllScope) == ScopedItemOptions.None && SessionStateUtilities.MatchesAnyWildcardPattern(aliasInfo.Name, (IEnumerable<WildcardPattern>) aliasPatterns, false))
        {
          string text = ResourceManagerCache.FormatResourceString("Modules", "ExportingAlias", (object) aliasInfo.Name);
          cmdlet.WriteVerbose(text);
          sessionState.ExportedAliases.Add(aliasInfo);
        }
      }
      ModuleIntrinsics.SortAndRemoveDuplicates<AliasInfo>(sessionState.ExportedAliases, (Converter<AliasInfo, string>) (ci => ci.Name));
    }
  }
}
