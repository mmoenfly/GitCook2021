// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandDiscovery
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Security;

namespace System.Management.Automation
{
  internal class CommandDiscovery
  {
    [TraceSource("DetailedCommandDiscovery", "The detailed tracing of CommandDiscovery. Superficial tracing can be found using the CommandDiscovery category.")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("DetailedCommandDiscovery", "The detailed tracing of CommandDiscovery. Superficial tracing can be found using the CommandDiscovery category.");
    [TraceSource("CommandDiscovery", "Traces the discovery of cmdlets, scripts, functions, applications, etc.")]
    internal static PSTraceSource discoveryTracer = PSTraceSource.GetTracer(nameof (CommandDiscovery), "Traces the discovery of cmdlets, scripts, functions, applications, etc.", false);
    private Dictionary<string, CommandInfo> tokenCache = new Dictionary<string, CommandInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private LookupPathCollection cachedLookupPaths;
    private string pathCacheKey;
    private Collection<string> cachedPath;
    private string pathExtensionsCacheKey;
    private Collection<string> cachedPathExtensions;
    private bool _cmdletCacheInitialized;
    private Dictionary<string, ScriptInfo> cachedScriptInfo;
    private ExecutionContext _context;

    internal CommandDiscovery(ExecutionContext context)
    {
      this._context = context != null ? context : throw CommandDiscovery.tracer.NewArgumentNullException(nameof (context));
      CommandDiscovery.discoveryTracer.ShowHeaders = false;
      this.cachedScriptInfo = new Dictionary<string, ScriptInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      this.LoadScriptInfo();
    }

    private void AddCmdletToCache(CmdletConfigurationEntry entry)
    {
      if (this.IsSpecialCmdlet(entry.ImplementingType))
        return;
      this.AddCmdletInfoToCache(this.NewCmdletInfo(entry, SessionStateEntryVisibility.Public));
    }

    private bool IsSpecialCmdlet(Type implementingType) => string.Equals(implementingType.FullName, "Microsoft.PowerShell.Commands.OutLineOutputCommand", StringComparison.OrdinalIgnoreCase) || string.Equals(implementingType.FullName, "Microsoft.PowerShell.Commands.FormatDefaultCommand", StringComparison.OrdinalIgnoreCase);

    private CmdletInfo NewCmdletInfo(
      CmdletConfigurationEntry entry,
      SessionStateEntryVisibility visibility)
    {
      CmdletInfo cmdletInfo = new CmdletInfo(entry.Name, entry.ImplementingType, entry.HelpFileName, entry.PSSnapIn, this._context);
      cmdletInfo.Visibility = visibility;
      return cmdletInfo;
    }

    private CmdletInfo NewCmdletInfo(SessionStateCmdletEntry entry) => CommandDiscovery.NewCmdletInfo(entry, this._context);

    internal static CmdletInfo NewCmdletInfo(
      SessionStateCmdletEntry entry,
      ExecutionContext context)
    {
      CmdletInfo cmdletInfo = new CmdletInfo(entry.Name, entry.ImplementingType, entry.HelpFileName, entry.PSSnapIn, context);
      cmdletInfo.Visibility = entry.Visibility;
      cmdletInfo.SetModule(entry.Module);
      return cmdletInfo;
    }

    internal void AddCmdletInfoToCache(CmdletInfo newCmdletInfo) => CommandDiscovery.AddCmdletInfoToCache(this._context.EngineSessionState, newCmdletInfo);

    internal static void AddCmdletInfoToCache(
      SessionStateInternal sessionState,
      CmdletInfo newCmdletInfo)
    {
      bool flag = false;
      try
      {
        lock (sessionState.CmdletCache)
        {
          if (sessionState.CmdletCache.ContainsKey(newCmdletInfo.Name))
          {
            List<CmdletInfo> cmdletInfoList = sessionState.CmdletCache[newCmdletInfo.Name];
            if (!string.IsNullOrEmpty(newCmdletInfo.ModuleName))
            {
              foreach (CmdletInfo cmdletInfo in cmdletInfoList)
              {
                if (string.Equals(newCmdletInfo.FullName, cmdletInfo.FullName, StringComparison.OrdinalIgnoreCase))
                {
                  if (newCmdletInfo.ImplementingType == cmdletInfo.ImplementingType)
                    return;
                  flag = true;
                  break;
                }
              }
            }
            else
            {
              using (List<CmdletInfo>.Enumerator enumerator = cmdletInfoList.GetEnumerator())
              {
                if (enumerator.MoveNext())
                {
                  CmdletInfo current = enumerator.Current;
                  if (newCmdletInfo.ImplementingType == current.ImplementingType)
                    return;
                  flag = true;
                }
              }
            }
            if (!flag)
              sessionState.CmdletCache[newCmdletInfo.Name].Insert(0, newCmdletInfo);
          }
          else
            sessionState.CmdletCache.Add(newCmdletInfo.Name, new List<CmdletInfo>()
            {
              newCmdletInfo
            });
        }
      }
      catch (ArgumentException ex)
      {
        flag = true;
      }
      if (flag)
        throw CommandDiscovery.tracer.NewNotSupportedException("DiscoveryExceptions", "DuplicateCmdletName", (object) newCmdletInfo.Name);
    }

    internal void AddSessionStateCmdletEntryToCache(SessionStateCmdletEntry entry)
    {
      if (this.IsSpecialCmdlet(entry.ImplementingType))
        return;
      this.AddCmdletInfoToCache(this.NewCmdletInfo(entry));
    }

    private void LoadScriptInfo()
    {
      if (this._context.RunspaceConfiguration == null)
        return;
      foreach (ScriptConfigurationEntry script in (IEnumerable<ScriptConfigurationEntry>) this._context.RunspaceConfiguration.Scripts)
      {
        try
        {
          this.cachedScriptInfo.Add(script.Name, new ScriptInfo(script.Name, ScriptBlock.Create(this._context, script.Definition), this._context));
        }
        catch (ArgumentException ex)
        {
          throw CommandDiscovery.tracer.NewNotSupportedException("DiscoveryExceptions", "DuplicateScriptName", (object) script.Name);
        }
      }
    }

    internal CommandProcessorBase LookupCommandProcessor(
      string commandName,
      CommandOrigin commandOrigin,
      bool? useLocalScope)
    {
      CommandProcessorBase commandProcessorBase = this.LookupCommandProcessor(this.LookupCommandInfo(commandName, commandOrigin), commandOrigin, useLocalScope);
      commandProcessorBase.Command.MyInvocation.InvocationName = commandName;
      return commandProcessorBase;
    }

    internal CommandProcessorBase CreateScriptProcessorForMiniShell(
      ExternalScriptInfo scriptInfo,
      bool useLocalScope)
    {
      CommandDiscovery.VerifyPSVersion(scriptInfo);
      if (string.IsNullOrEmpty(scriptInfo.RequiresApplicationID))
      {
        if (scriptInfo.RequiresPSSnapIns != null && scriptInfo.RequiresPSSnapIns.Count > 0)
        {
          Collection<string> psSnapinNames = this.GetPSSnapinNames(scriptInfo.RequiresPSSnapIns);
          ScriptRequiresException requiresException = new ScriptRequiresException(scriptInfo.Name, psSnapinNames, "ScriptRequiresMissingPSSnapIns");
          CommandDiscovery.tracer.TraceException((Exception) requiresException);
          throw requiresException;
        }
        return CommandDiscovery.CreateCommandProcessorForScript(scriptInfo, this._context, useLocalScope);
      }
      if (string.Equals(this._context.ShellID, scriptInfo.RequiresApplicationID, StringComparison.OrdinalIgnoreCase))
        return CommandDiscovery.CreateCommandProcessorForScript(scriptInfo, this._context, useLocalScope);
      string pathFromRegistry = CommandDiscovery.GetShellPathFromRegistry(scriptInfo.RequiresApplicationID);
      ScriptRequiresException requiresException1 = new ScriptRequiresException(scriptInfo.Name, scriptInfo.ApplicationIDLineNumber, scriptInfo.RequiresApplicationID, pathFromRegistry, "ScriptRequiresUnmatchedShellId");
      CommandDiscovery.tracer.TraceException((Exception) requiresException1);
      throw requiresException1;
    }

    private Collection<string> GetPSSnapinNames(
      Collection<PSSnapInNameVersionPair> PSSnapins)
    {
      Collection<string> collection = new Collection<string>();
      foreach (PSSnapInNameVersionPair psSnapin in PSSnapins)
        collection.Add(CommandDiscovery.BuildPSSnapInDisplayName(psSnapin));
      return collection;
    }

    private CommandProcessorBase CreateScriptProcessorForSingleShell(
      ExternalScriptInfo scriptInfo,
      RunspaceConfigForSingleShell ssRunConfig,
      bool useLocalScope)
    {
      CommandDiscovery.VerifyPSVersion(scriptInfo);
      Collection<PSSnapInNameVersionPair> requiresPsSnapIns = scriptInfo.RequiresPSSnapIns;
      if (requiresPsSnapIns != null)
      {
        Collection<string> missingPSSnapIns = (Collection<string>) null;
        foreach (PSSnapInNameVersionPair PSSnapin in requiresPsSnapIns)
        {
          Collection<PSSnapInInfo> psSnapIn = ssRunConfig.ConsoleInfo.GetPSSnapIn(PSSnapin.PSSnapInName, false);
          if (psSnapIn == null || psSnapIn.Count == 0)
          {
            if (missingPSSnapIns == null)
              missingPSSnapIns = new Collection<string>();
            missingPSSnapIns.Add(CommandDiscovery.BuildPSSnapInDisplayName(PSSnapin));
          }
          else if (PSSnapin.Version != (Version) null && !CommandDiscovery.AreInstalledRequiresVersionsCompatible(PSSnapin.Version, psSnapIn[0].Version))
          {
            if (missingPSSnapIns == null)
              missingPSSnapIns = new Collection<string>();
            missingPSSnapIns.Add(CommandDiscovery.BuildPSSnapInDisplayName(PSSnapin));
          }
        }
        if (missingPSSnapIns != null)
        {
          ScriptRequiresException requiresException = new ScriptRequiresException(scriptInfo.Name, missingPSSnapIns, "ScriptRequiresMissingPSSnapIns");
          CommandDiscovery.tracer.TraceException((Exception) requiresException);
          throw requiresException;
        }
      }
      else if (!string.IsNullOrEmpty(scriptInfo.RequiresApplicationID))
      {
        CommandDiscovery.GetShellPathFromRegistry(scriptInfo.RequiresApplicationID);
        ScriptRequiresException requiresException = new ScriptRequiresException(scriptInfo.Name, scriptInfo.ApplicationIDLineNumber, string.Empty, string.Empty, "RequiresShellIDInvalidForSingleShell");
        CommandDiscovery.tracer.TraceException((Exception) requiresException);
        throw requiresException;
      }
      return CommandDiscovery.CreateCommandProcessorForScript(scriptInfo, this._context, useLocalScope);
    }

    internal static void VerifyPSVersion(ExternalScriptInfo scriptInfo)
    {
      Version requiresPsVersion = scriptInfo.RequiresPSVersion;
      if (requiresPsVersion != (Version) null && !Utils.IsVersionSupported(requiresPsVersion))
      {
        ScriptRequiresException requiresException = new ScriptRequiresException(scriptInfo.Name, scriptInfo.PSVersionLineNumber, requiresPsVersion, PSVersionInfo.PSVersion.ToString(), "ScriptRequiresUnmatchedPSVersion");
        CommandDiscovery.tracer.TraceException((Exception) requiresException);
        throw requiresException;
      }
    }

    private static bool AreInstalledRequiresVersionsCompatible(Version requires, Version installed) => requires.Major == installed.Major && requires.Minor <= installed.Minor;

    private static string BuildPSSnapInDisplayName(PSSnapInNameVersionPair PSSnapin)
    {
      if (PSSnapin.Version == (Version) null)
        return PSSnapin.PSSnapInName;
      return ResourceManagerCache.FormatResourceString("DiscoveryExceptions", "PSSnapInNameVersion", (object) PSSnapin.PSSnapInName, (object) PSSnapin.Version);
    }

    internal CommandProcessorBase LookupCommandProcessor(
      CommandInfo commandInfo,
      CommandOrigin commandOrigin,
      bool? useLocalScope)
    {
      if (commandInfo.CommandType == CommandTypes.Alias && (commandOrigin == CommandOrigin.Internal || commandInfo.Visibility == SessionStateEntryVisibility.Public))
      {
        AliasInfo aliasInfo = (AliasInfo) commandInfo;
        commandInfo = aliasInfo.ResolvedCommand;
        if (commandInfo == null)
        {
          CommandNotFoundException notFoundException = new CommandNotFoundException(aliasInfo.Name, (Exception) null, "AliasNotResolvedException", new object[1]
          {
            (object) aliasInfo.UnresolvedCommandName
          });
          CommandDiscovery.tracer.TraceException((Exception) notFoundException);
          throw notFoundException;
        }
        PSSQMAPI.IncrementData(CommandTypes.Alias);
      }
      CommandDiscovery.ShouldRun(this._context, (PSHost) this._context.EngineHostInterface, commandInfo, commandOrigin);
      CommandProcessorBase commandProcessorBase;
      switch (commandInfo.CommandType)
      {
        case CommandTypes.Function:
        case CommandTypes.Filter:
          FunctionInfo functionInfo = (FunctionInfo) commandInfo;
          ExecutionContext context1 = this._context;
          bool? nullable1 = useLocalScope;
          int num1 = nullable1.HasValue ? (nullable1.GetValueOrDefault() ? 1 : 0) : 1;
          commandProcessorBase = CommandDiscovery.CreateCommandProcessorForScript(functionInfo, context1, num1 != 0);
          break;
        case CommandTypes.Cmdlet:
          commandProcessorBase = (CommandProcessorBase) new CommandProcessor((CmdletInfo) commandInfo, this._context);
          break;
        case CommandTypes.ExternalScript:
          ExternalScriptInfo externalScriptInfo = (ExternalScriptInfo) commandInfo;
          externalScriptInfo.SignatureChecked = true;
          try
          {
            if (!this._context.IsSingleShell)
            {
              ExternalScriptInfo scriptInfo = externalScriptInfo;
              bool? nullable2 = useLocalScope;
              int num2 = nullable2.HasValue ? (nullable2.GetValueOrDefault() ? 1 : 0) : 1;
              commandProcessorBase = this.CreateScriptProcessorForMiniShell(scriptInfo, num2 != 0);
              break;
            }
            ExternalScriptInfo scriptInfo1 = externalScriptInfo;
            RunspaceConfigForSingleShell runspaceConfiguration = this._context.RunspaceConfiguration as RunspaceConfigForSingleShell;
            bool? nullable3 = useLocalScope;
            int num3 = nullable3.HasValue ? (nullable3.GetValueOrDefault() ? 1 : 0) : 1;
            commandProcessorBase = this.CreateScriptProcessorForSingleShell(scriptInfo1, runspaceConfiguration, num3 != 0);
            break;
          }
          catch (ScriptRequiresSyntaxException ex)
          {
            CommandNotFoundException notFoundException = new CommandNotFoundException(ex.Message, (Exception) ex);
            CommandDiscovery.tracer.TraceException((Exception) notFoundException);
            throw notFoundException;
          }
          catch (PSArgumentException ex)
          {
            CommandNotFoundException notFoundException = new CommandNotFoundException(commandInfo.Name, (Exception) ex, "ScriptRequiresInvalidFormat", new object[0]);
            CommandDiscovery.tracer.TraceException((Exception) notFoundException);
            throw notFoundException;
          }
        case CommandTypes.Application:
          commandProcessorBase = (CommandProcessorBase) new NativeCommandProcessor((ApplicationInfo) commandInfo, this._context);
          break;
        case CommandTypes.Script:
          ScriptInfo scriptInfo2 = (ScriptInfo) commandInfo;
          ExecutionContext context2 = this._context;
          bool? nullable4 = useLocalScope;
          int num4 = nullable4.HasValue ? (nullable4.GetValueOrDefault() ? 1 : 0) : 1;
          commandProcessorBase = CommandDiscovery.CreateCommandProcessorForScript(scriptInfo2, context2, num4 != 0);
          break;
        default:
          CommandNotFoundException notFoundException1 = new CommandNotFoundException(commandInfo.Name, (Exception) null, "CommandNotFoundException", new object[0]);
          CommandDiscovery.tracer.TraceException((Exception) notFoundException1);
          throw notFoundException1;
      }
      PSSQMAPI.IncrementData(commandInfo.CommandType);
      commandProcessorBase.Command.CommandOriginInternal = commandOrigin;
      commandProcessorBase.Command.MyInvocation.InvocationName = commandInfo.Name;
      return commandProcessorBase;
    }

    internal static void ShouldRun(
      ExecutionContext context,
      PSHost host,
      CommandInfo commandInfo,
      CommandOrigin commandOrigin)
    {
      try
      {
        if (commandOrigin == CommandOrigin.Runspace && commandInfo.Visibility != SessionStateEntryVisibility.Public)
        {
          CommandNotFoundException notFoundException = new CommandNotFoundException(commandInfo.Name, (Exception) null, "CommandNotFoundException", new object[0]);
          CommandDiscovery.tracer.TraceException((Exception) notFoundException);
          throw notFoundException;
        }
        context.AuthorizationManager.ShouldRunInternal(commandInfo, commandOrigin, host);
      }
      catch (PSSecurityException ex)
      {
        CommandDiscovery.tracer.TraceException((Exception) ex);
        MshLog.LogCommandHealthEvent(context, (Exception) ex, Severity.Warning);
        MshLog.LogCommandLifecycleEvent(context, CommandState.Terminated, commandInfo.Name);
        throw;
      }
    }

    private static CommandProcessorBase CreateCommandProcessorForScript(
      ScriptInfo scriptInfo,
      ExecutionContext context,
      bool useNewScope)
    {
      return CommandDiscovery.GetScriptAsCmdletProcessor((IScriptCommandInfo) scriptInfo, context, useNewScope, true) ?? (CommandProcessorBase) new ScriptCommandProcessor(scriptInfo, context, useNewScope);
    }

    private static CommandProcessorBase CreateCommandProcessorForScript(
      ExternalScriptInfo scriptInfo,
      ExecutionContext context,
      bool useNewScope)
    {
      return CommandDiscovery.GetScriptAsCmdletProcessor((IScriptCommandInfo) scriptInfo, context, useNewScope, true) ?? (CommandProcessorBase) new ScriptCommandProcessor(scriptInfo, context, useNewScope);
    }

    internal static CommandProcessorBase CreateCommandProcessorForScript(
      FunctionInfo functionInfo,
      ExecutionContext context,
      bool useNewScope)
    {
      return CommandDiscovery.GetScriptAsCmdletProcessor((IScriptCommandInfo) functionInfo, context, useNewScope, false) ?? (CommandProcessorBase) new ScriptCommandProcessor(functionInfo, context, useNewScope);
    }

    internal static CommandProcessorBase CreateCommandProcessorForScript(
      ScriptBlock scriptblock,
      ExecutionContext context,
      bool useNewScope)
    {
      return scriptblock.UsesCmdletBinding ? CommandDiscovery.GetScriptAsCmdletProcessor((IScriptCommandInfo) new FunctionInfo("", scriptblock, context), context, useNewScope, false) : (CommandProcessorBase) new ScriptCommandProcessor(scriptblock, context, useNewScope);
    }

    private static CommandProcessorBase GetScriptAsCmdletProcessor(
      IScriptCommandInfo scriptCommandInfo,
      ExecutionContext context,
      bool useNewScope,
      bool fromScriptFile)
    {
      if (scriptCommandInfo.ScriptBlock == null || !scriptCommandInfo.ScriptBlock.UsesCmdletBinding)
        return (CommandProcessorBase) null;
      CommandProcessor commandProcessor = new CommandProcessor(scriptCommandInfo, context, useNewScope);
      commandProcessor.FromScriptFile = fromScriptFile;
      ((PSScriptCmdlet) commandProcessor.Command).FromScriptFile = fromScriptFile;
      return (CommandProcessorBase) commandProcessor;
    }

    internal CommandInfo LookupCommandInfo(string commandName) => this.LookupCommandInfo(commandName, CommandOrigin.Internal);

    internal CommandInfo LookupCommandInfo(
      string commandName,
      CommandOrigin commandOrigin)
    {
      CommandInfo commandInfo = (CommandInfo) null;
      string commandName1 = commandName;
      Exception innerException = (Exception) null;
      while (true)
      {
        CommandDiscovery.discoveryTracer.WriteLine("Looking up command: {0}", (object) commandName);
        if (!string.IsNullOrEmpty(commandName))
        {
          CommandSearcher commandSearcher = new CommandSearcher(commandName, SearchResolutionOptions.AllowDuplicateCmdletNames, CommandTypes.All, this._context);
          commandSearcher.CommandOrigin = commandOrigin;
          try
          {
            if (!commandSearcher.MoveNext())
            {
              if (!commandName.Contains("-"))
              {
                CommandDiscovery.discoveryTracer.WriteLine("The command [{0}] was not found, trying again with get- prepended", (object) commandName);
                commandName = "get" + (object) '-' + commandName;
              }
              else
                goto label_13;
            }
            else
            {
              commandInfo = ((IEnumerator<CommandInfo>) commandSearcher).Current;
              goto label_13;
            }
          }
          catch (ArgumentException ex)
          {
            innerException = (Exception) ex;
            goto label_13;
          }
          catch (PathTooLongException ex)
          {
            innerException = (Exception) ex;
            goto label_13;
          }
          catch (FileLoadException ex)
          {
            innerException = (Exception) ex;
            goto label_13;
          }
          catch (FormatException ex)
          {
            innerException = (Exception) ex;
            goto label_13;
          }
          catch (MetadataException ex)
          {
            innerException = (Exception) ex;
            goto label_13;
          }
        }
        else
          break;
      }
      CommandDiscovery.discoveryTracer.TraceError("Command name empty or null");
label_13:
      if (commandInfo == null)
      {
        CommandDiscovery.discoveryTracer.TraceError("'{0}' is not recognized as a cmdlet, function, operable program or script file.", (object) commandName);
        CommandNotFoundException notFoundException = new CommandNotFoundException(commandName1, innerException, "CommandNotFoundException", new object[0]);
        CommandDiscovery.tracer.TraceException((Exception) notFoundException);
        throw notFoundException;
      }
      return commandInfo;
    }

    internal IEnumerable<string> GetCommandPathSearcher(IEnumerable<string> patterns)
    {
      IEnumerable<string> lookupDirectoryPaths = this.GetLookupDirectoryPaths();
      return (IEnumerable<string>) new CommandPathSearch(patterns, lookupDirectoryPaths, this._context);
    }

    internal IEnumerable<string> GetLookupDirectoryPaths()
    {
      LookupPathCollection lookupPathCollection = new LookupPathCollection();
      string environmentVariable = Environment.GetEnvironmentVariable("PATH");
      CommandDiscovery.discoveryTracer.WriteLine("PATH: {0}", (object) environmentVariable);
      if (environmentVariable == null || !string.Equals(this.pathCacheKey, environmentVariable, StringComparison.OrdinalIgnoreCase) || this.cachedPath == null)
      {
        this.cachedLookupPaths = (LookupPathCollection) null;
        this.pathCacheKey = environmentVariable;
        if (this.pathCacheKey != null)
        {
          string[] strArray = this.pathCacheKey.Split(new char[1]
          {
            ';'
          }, StringSplitOptions.RemoveEmptyEntries);
          if (strArray != null)
          {
            this.cachedPath = new Collection<string>();
            foreach (string str1 in strArray)
            {
              string str2 = str1.TrimStart();
              this.cachedPath.Add(str2);
              lookupPathCollection.Add(str2);
            }
          }
        }
      }
      else
        lookupPathCollection.AddRange((ICollection<string>) this.cachedPath);
      if (this.cachedLookupPaths == null)
        this.cachedLookupPaths = lookupPathCollection;
      return (IEnumerable<string>) this.cachedLookupPaths;
    }

    internal IEnumerable<string> PathExtensions
    {
      get
      {
        string environmentVariable = Environment.GetEnvironmentVariable("PATHEXT");
        Collection<string> collection;
        if (this.pathExtensionsCacheKey != null && environmentVariable != null && (this.cachedPathExtensions != null && environmentVariable.Equals(this.pathExtensionsCacheKey, StringComparison.OrdinalIgnoreCase)))
        {
          collection = this.cachedPathExtensions;
        }
        else
        {
          collection = new Collection<string>();
          if (environmentVariable != null)
          {
            string[] strArray = environmentVariable.Split(new char[1]
            {
              ';'
            }, StringSplitOptions.RemoveEmptyEntries);
            if (strArray != null)
            {
              foreach (string str in strArray)
                collection.Add(str);
            }
            this.pathExtensionsCacheKey = environmentVariable;
            this.cachedPathExtensions = collection;
          }
        }
        return (IEnumerable<string>) collection;
      }
    }

    internal Collection<CmdletInfo> GetCmdletInfo(string cmdletName)
    {
      PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(cmdletName);
      return instance == null ? new Collection<CmdletInfo>() : this.GetCmdletInfo(instance);
    }

    internal Collection<CmdletInfo> GetCmdletInfo(
      PSSnapinQualifiedName commandName)
    {
      Collection<CmdletInfo> collection = new Collection<CmdletInfo>();
      Dictionary<string, List<CmdletInfo>> cmdletCache = this._context.EngineSessionState.CmdletCache;
      while (true)
      {
        lock (cmdletCache)
        {
          if (cmdletCache.ContainsKey(commandName.ShortName))
          {
            List<CmdletInfo> cmdletInfoList = cmdletCache[commandName.ShortName];
            if (!string.IsNullOrEmpty(commandName.PSSnapInName))
            {
              foreach (CmdletInfo cmdletInfo in cmdletInfoList)
              {
                if (string.Equals(cmdletInfo.ModuleName, commandName.PSSnapInName, StringComparison.OrdinalIgnoreCase))
                  collection.Add(cmdletInfo);
              }
            }
            else
            {
              foreach (CmdletInfo cmdletInfo in cmdletInfoList)
                collection.Add(cmdletInfo);
            }
          }
        }
        if (cmdletCache != this._context.TopLevelSessionState.CmdletCache)
          cmdletCache = this._context.TopLevelSessionState.CmdletCache;
        else
          break;
      }
      return collection;
    }

    internal void UpdateCmdletCache()
    {
      if (!this._cmdletCacheInitialized)
      {
        foreach (CmdletConfigurationEntry cmdlet in (IEnumerable<CmdletConfigurationEntry>) this._context.RunspaceConfiguration.Cmdlets)
          this.AddCmdletToCache(cmdlet);
        this._cmdletCacheInitialized = true;
      }
      else
      {
        foreach (CmdletConfigurationEntry update in this._context.RunspaceConfiguration.Cmdlets.UpdateList)
        {
          if (update != null)
          {
            switch (update.Action)
            {
              case UpdateAction.Add:
                this.AddCmdletToCache(update);
                continue;
              case UpdateAction.Remove:
                this.RemoveCmdletFromCache(update);
                continue;
              default:
                continue;
            }
          }
        }
      }
    }

    private void RemoveCmdletFromCache(CmdletConfigurationEntry entry)
    {
      lock (this._context.EngineSessionState.CmdletCache)
      {
        if (!this._context.EngineSessionState.CmdletCache.ContainsKey(entry.Name))
          return;
        List<CmdletInfo> cacheEntry = this._context.EngineSessionState.CmdletCache[entry.Name];
        int cmdletRemovalIndex = this.GetCmdletRemovalIndex(cacheEntry, entry.PSSnapIn == null ? string.Empty : entry.PSSnapIn.Name);
        if (cmdletRemovalIndex >= 0)
          cacheEntry.RemoveAt(cmdletRemovalIndex);
        if (cacheEntry.Count != 0)
          return;
        this._context.EngineSessionState.CmdletCache.Remove(entry.Name);
      }
    }

    private int GetCmdletRemovalIndex(List<CmdletInfo> cacheEntry, string PSSnapin)
    {
      int num = -1;
      for (int index = 0; index < cacheEntry.Count; ++index)
      {
        if (string.Equals(cacheEntry[index].ModuleName, PSSnapin, StringComparison.OrdinalIgnoreCase))
        {
          num = index;
          break;
        }
      }
      return num;
    }

    internal ScriptInfo GetScriptInfo(string name)
    {
      ScriptInfo scriptInfo = (ScriptInfo) null;
      if (this.cachedScriptInfo.ContainsKey(name))
        scriptInfo = this.cachedScriptInfo[name];
      return scriptInfo;
    }

    internal Dictionary<string, ScriptInfo> ScriptCache => this.cachedScriptInfo;

    internal ExecutionContext Context => this._context;

    internal static string GetShellPathFromRegistry(string shellID)
    {
      string str = (string) null;
      try
      {
        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Utils.GetRegistryConfigurationPath(shellID));
        if (registryKey != null)
        {
          switch (registryKey.GetValueKind("path"))
          {
            case RegistryValueKind.String:
            case RegistryValueKind.ExpandString:
              str = registryKey.GetValue("path") as string;
              break;
          }
        }
      }
      catch (SecurityException ex)
      {
        CommandDiscovery.tracer.TraceException((Exception) ex);
      }
      catch (IOException ex)
      {
        CommandDiscovery.tracer.TraceException((Exception) ex);
      }
      catch (ArgumentException ex)
      {
        CommandDiscovery.tracer.TraceException((Exception) ex);
      }
      CommandDiscovery.tracer.WriteLine("result = {0}", str == null ? (object) "null" : (object) str);
      return str;
    }
  }
}
