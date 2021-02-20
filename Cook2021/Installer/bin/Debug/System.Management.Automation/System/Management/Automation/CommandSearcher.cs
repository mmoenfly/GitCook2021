// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandSearcher
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation.Provider;
using System.Text;

namespace System.Management.Automation
{
  internal class CommandSearcher : 
    IEnumerable<CommandInfo>,
    IEnumerable,
    IEnumerator<CommandInfo>,
    IDisposable,
    IEnumerator
  {
    [TraceSource("CommandSearch", "CommandSearch")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CommandSearch", "CommandSearch");
    private char[] _pathSeparators = new char[3]
    {
      '\\',
      '/',
      ':'
    };
    private IEnumerator<CmdletInfo> matchingCmdlet;
    private IEnumerator<string> matchingScript;
    private string commandName;
    private SearchResolutionOptions commandResolutionOptions;
    private CommandTypes commandTypes = CommandTypes.All;
    private CommandPathSearch pathSearcher;
    private ExecutionContext _context;
    private CommandOrigin _commandOrigin = CommandOrigin.Internal;
    private IEnumerator<AliasInfo> matchingAlias;
    private IEnumerator<CommandInfo> matchingFunctionEnumerator;
    private CommandInfo _currentMatch;
    private bool canDoPathLookup;
    private CommandSearcher.CanDoPathLookupResult canDoPathLookupResult;
    private CommandSearcher.SearchState currentState;

    internal CommandSearcher(
      string commandName,
      SearchResolutionOptions options,
      CommandTypes commandTypes,
      ExecutionContext context)
    {
      if (context == null)
        throw CommandSearcher.tracer.NewArgumentNullException(nameof (context));
      this.commandName = !string.IsNullOrEmpty(commandName) ? commandName : throw CommandSearcher.tracer.NewArgumentException(nameof (commandName));
      this._context = context;
      this.commandResolutionOptions = options;
      this.commandTypes = commandTypes;
      this.Reset();
    }

    IEnumerator<CommandInfo> IEnumerable<CommandInfo>.GetEnumerator() => (IEnumerator<CommandInfo>) this;

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this;

    public bool MoveNext()
    {
      bool flag1 = false;
      this._currentMatch = (CommandInfo) null;
      if (this.currentState == CommandSearcher.SearchState.Reset)
      {
        this._currentMatch = this.ProcessResetState();
        if (this._currentMatch != null && SessionState.IsVisible(this._commandOrigin, this._currentMatch))
        {
          flag1 = true;
          goto label_48;
        }
        else
          this.currentState = CommandSearcher.SearchState.AliasResolution;
      }
      if (this.currentState == CommandSearcher.SearchState.AliasResolution)
      {
        this._currentMatch = this.ProcessAliasState();
        if (this._currentMatch != null)
        {
          flag1 = true;
          goto label_48;
        }
        else
          this.currentState = CommandSearcher.SearchState.FunctionResolution;
      }
      if (this.currentState == CommandSearcher.SearchState.FunctionResolution)
      {
        this._currentMatch = this.ProcessFunctionState();
        if (this._currentMatch != null)
        {
          flag1 = true;
          goto label_48;
        }
        else
          this.currentState = CommandSearcher.SearchState.CmdletResolution;
      }
      if (this.currentState == CommandSearcher.SearchState.CmdletResolution)
      {
        this._currentMatch = this.ProcessCmdletState();
        if (this._currentMatch != null)
        {
          flag1 = true;
          goto label_48;
        }
        else
          this.currentState = CommandSearcher.SearchState.BuiltinScriptResolution;
      }
      if (this.currentState == CommandSearcher.SearchState.BuiltinScriptResolution)
      {
        if ((this.commandTypes & (CommandTypes.ExternalScript | CommandTypes.Application)) == (CommandTypes) 0)
        {
          flag1 = false;
          goto label_48;
        }
        else
        {
          if (this._commandOrigin == CommandOrigin.Runspace && this.commandName.IndexOfAny(this._pathSeparators) >= 0)
          {
            bool flag2 = false;
            if (this._context.EngineSessionState.Applications.Count == 1 && this._context.EngineSessionState.Applications[0].Equals("*", StringComparison.OrdinalIgnoreCase) || this._context.EngineSessionState.Scripts.Count == 1 && this._context.EngineSessionState.Scripts[0].Equals("*", StringComparison.OrdinalIgnoreCase))
            {
              flag2 = true;
            }
            else
            {
              foreach (string application in this._context.EngineSessionState.Applications)
              {
                if (this.checkPath(application, this.commandName))
                {
                  flag2 = true;
                  break;
                }
              }
              if (!flag2)
              {
                foreach (string script in this._context.EngineSessionState.Scripts)
                {
                  if (this.checkPath(script, this.commandName))
                  {
                    flag2 = true;
                    break;
                  }
                }
              }
            }
            if (!flag2)
            {
              flag1 = false;
              goto label_48;
            }
          }
          this.currentState = CommandSearcher.SearchState.PowerShellPathResolution;
          this._currentMatch = this.ProcessBuiltinScriptState();
          if (this._currentMatch != null)
          {
            this.currentState = CommandSearcher.SearchState.QualifiedFileSystemPath;
            flag1 = true;
            goto label_48;
          }
        }
      }
      if (this.currentState == CommandSearcher.SearchState.PowerShellPathResolution)
      {
        this.currentState = CommandSearcher.SearchState.QualifiedFileSystemPath;
        this._currentMatch = this.ProcessPathResolutionState();
        if (this._currentMatch != null)
        {
          flag1 = true;
          goto label_48;
        }
      }
      if (this.currentState == CommandSearcher.SearchState.QualifiedFileSystemPath || this.currentState == CommandSearcher.SearchState.PathSearch)
      {
        this._currentMatch = this.ProcessQualifiedFileSystemState();
        if (this._currentMatch != null)
        {
          flag1 = true;
          goto label_48;
        }
      }
      if (this.currentState == CommandSearcher.SearchState.PathSearch)
      {
        this.currentState = CommandSearcher.SearchState.PowerShellRelativePath;
        this._currentMatch = this.ProcessPathSearchState();
        if (this._currentMatch != null)
          flag1 = true;
      }
label_48:
      return flag1;
    }

    private CommandInfo ProcessResetState()
    {
      CommandInfo commandInfo = (CommandInfo) null;
      if (this._context.EngineSessionState != null && (this.commandTypes & CommandTypes.Alias) != (CommandTypes) 0)
        commandInfo = this.GetNextAlias();
      return commandInfo;
    }

    private CommandInfo ProcessAliasState()
    {
      CommandInfo commandInfo = (CommandInfo) null;
      if (this._context.EngineSessionState != null && (this.commandTypes & (CommandTypes.Function | CommandTypes.Filter)) != (CommandTypes) 0)
        commandInfo = this.GetNextFunction();
      return commandInfo;
    }

    private CommandInfo ProcessFunctionState()
    {
      CommandInfo commandInfo = (CommandInfo) null;
      if ((this.commandTypes & CommandTypes.Cmdlet) != (CommandTypes) 0)
        commandInfo = (CommandInfo) this.GetNextCmdlet();
      return commandInfo;
    }

    private CommandInfo ProcessCmdletState()
    {
      CommandInfo commandInfo = (CommandInfo) null;
      if ((this.commandTypes & CommandTypes.Script) != (CommandTypes) 0)
        commandInfo = (CommandInfo) this.GetNextBuiltinScript();
      return commandInfo;
    }

    private CommandInfo ProcessBuiltinScriptState()
    {
      CommandInfo commandInfo = (CommandInfo) null;
      if (this._context.EngineSessionState != null && this._context.EngineSessionState.ProviderCount > 0 && CommandSearcher.IsQualifiedPSPath(this.commandName))
        commandInfo = this.GetNextFromPath();
      return commandInfo;
    }

    private CommandInfo ProcessPathResolutionState()
    {
      CommandInfo commandInfo = (CommandInfo) null;
      try
      {
        if (Path.IsPathRooted(this.commandName))
        {
          if (File.Exists(this.commandName))
          {
            try
            {
              commandInfo = this.GetInfoFromPath(this.commandName);
            }
            catch (FileLoadException ex)
            {
            }
            catch (FormatException ex)
            {
            }
            catch (MetadataException ex)
            {
            }
          }
        }
      }
      catch (ArgumentException ex)
      {
      }
      return commandInfo;
    }

    private CommandInfo ProcessQualifiedFileSystemState()
    {
      try
      {
        this.setupPathSearcher();
      }
      catch (ArgumentException ex)
      {
        this.currentState = CommandSearcher.SearchState.NoMoreMatches;
        throw;
      }
      catch (PathTooLongException ex)
      {
        this.currentState = CommandSearcher.SearchState.NoMoreMatches;
        throw;
      }
      CommandInfo commandInfo = (CommandInfo) null;
      this.currentState = CommandSearcher.SearchState.PathSearch;
      if (this.canDoPathLookup)
      {
        try
        {
          for (; commandInfo == null; commandInfo = this.GetInfoFromPath(((IEnumerator<string>) this.pathSearcher).Current))
          {
            if (!this.pathSearcher.MoveNext())
              break;
          }
        }
        catch (InvalidOperationException ex)
        {
        }
      }
      return commandInfo;
    }

    private CommandInfo ProcessPathSearchState()
    {
      CommandInfo commandInfo = (CommandInfo) null;
      string path = this.DoPowerShellRelativePathLookup();
      if (!string.IsNullOrEmpty(path))
        commandInfo = this.GetInfoFromPath(path);
      return commandInfo;
    }

    CommandInfo IEnumerator<CommandInfo>.Current => (this.currentState != CommandSearcher.SearchState.Reset || this._currentMatch != null) && (this.currentState != CommandSearcher.SearchState.NoMoreMatches && this._currentMatch != null) ? this._currentMatch : throw CommandSearcher.tracer.NewInvalidOperationException();

    object IEnumerator.Current => (object) ((IEnumerator<CommandInfo>) this).Current;

    public void Dispose()
    {
      if (this.pathSearcher != null)
      {
        this.pathSearcher.Dispose();
        this.pathSearcher = (CommandPathSearch) null;
      }
      this.Reset();
      GC.SuppressFinalize((object) this);
    }

    private CommandInfo GetNextFromPath()
    {
      CommandInfo commandInfo = (CommandInfo) null;
      CommandDiscovery.discoveryTracer.WriteLine("The name appears to be a qualified path: {0}", (object) this.commandName);
      CommandDiscovery.discoveryTracer.WriteLine("Trying to resolve the path as an PSPath", new object[0]);
      ProviderInfo provider = (ProviderInfo) null;
      Collection<string> collection = new Collection<string>();
      try
      {
        CmdletProvider providerInstance = (CmdletProvider) null;
        collection = this._context.LocationGlobber.GetGlobbedProviderPathsFromMonadPath(this.commandName, false, out provider, out providerInstance);
      }
      catch (ItemNotFoundException ex)
      {
        CommandDiscovery.discoveryTracer.TraceError("The path could not be found: {0}", (object) this.commandName);
      }
      catch (DriveNotFoundException ex)
      {
        CommandDiscovery.discoveryTracer.TraceError("A drive could not be found for the path: {0}", (object) this.commandName);
      }
      catch (ProviderNotFoundException ex)
      {
        CommandDiscovery.discoveryTracer.TraceError("A provider could not be found for the path: {0}", (object) this.commandName);
      }
      catch (InvalidOperationException ex)
      {
        CommandDiscovery.discoveryTracer.TraceError("The path specified a home directory, but the provider home directory was not set. {0}", (object) this.commandName);
      }
      catch (ProviderInvocationException ex)
      {
        CommandDiscovery.discoveryTracer.TraceError("The provider associated with the path '{0}' encountered an error: {1}", (object) this.commandName, (object) ex.Message);
      }
      catch (PSNotSupportedException ex)
      {
        CommandDiscovery.discoveryTracer.TraceError("The provider associated with the path '{0}' does not implement ContainerCmdletProvider", (object) this.commandName);
      }
      if (collection.Count > 1)
        CommandDiscovery.discoveryTracer.TraceError("The path resolved to more than one result so this path cannot be used.");
      else if (collection.Count == 1 && File.Exists(collection[0]))
      {
        string path = collection[0];
        CommandDiscovery.discoveryTracer.WriteLine("Path resolved to: {0}", (object) path);
        commandInfo = this.GetInfoFromPath(path);
      }
      return commandInfo;
    }

    private bool checkPath(string path, string commandName) => path.StartsWith(commandName, StringComparison.OrdinalIgnoreCase);

    private CommandInfo GetInfoFromPath(string path)
    {
      CommandInfo commandInfo = (CommandInfo) null;
      if (!File.Exists(path))
      {
        CommandDiscovery.discoveryTracer.TraceError("The path does not exist: {0}", (object) path);
      }
      else
      {
        string a = (string) null;
        try
        {
          a = Path.GetExtension(path);
        }
        catch (ArgumentException ex)
        {
        }
        if (a == null)
          commandInfo = (CommandInfo) null;
        else if (string.Equals(a, ".ps1", StringComparison.OrdinalIgnoreCase))
        {
          if ((this.commandTypes & CommandTypes.ExternalScript) != (CommandTypes) 0)
          {
            string fileName = Path.GetFileName(path);
            CommandDiscovery.discoveryTracer.WriteLine("Command Found: path ({0}) is a script with name: {1}", (object) path, (object) fileName);
            commandInfo = (CommandInfo) new ExternalScriptInfo(fileName, path, this._context);
          }
        }
        else if ((this.commandTypes & CommandTypes.Application) != (CommandTypes) 0)
        {
          string fileName = Path.GetFileName(path);
          CommandDiscovery.discoveryTracer.WriteLine("Command Found: path ({0}) is an application with name: {1}", (object) path, (object) fileName);
          commandInfo = (CommandInfo) new ApplicationInfo(fileName, path, this._context);
        }
      }
      return commandInfo;
    }

    private CommandInfo GetNextAlias()
    {
      CommandInfo commandInfo = (CommandInfo) null;
      if ((this.commandResolutionOptions & SearchResolutionOptions.ResolveAliasPatterns) != SearchResolutionOptions.None)
      {
        if (this.matchingAlias == null)
        {
          Collection<AliasInfo> collection = new Collection<AliasInfo>();
          WildcardPattern wildcardPattern = new WildcardPattern(this.commandName, WildcardOptions.IgnoreCase);
          foreach (KeyValuePair<string, AliasInfo> keyValuePair in (IEnumerable<KeyValuePair<string, AliasInfo>>) this._context.EngineSessionState.GetAliasTable())
          {
            if (wildcardPattern.IsMatch(keyValuePair.Key))
              collection.Add(keyValuePair.Value);
          }
          this.matchingAlias = collection.GetEnumerator();
        }
        if (!this.matchingAlias.MoveNext())
        {
          this.currentState = CommandSearcher.SearchState.AliasResolution;
          this.matchingAlias = (IEnumerator<AliasInfo>) null;
        }
        else
          commandInfo = (CommandInfo) this.matchingAlias.Current;
      }
      else
      {
        this.currentState = CommandSearcher.SearchState.AliasResolution;
        commandInfo = (CommandInfo) this._context.EngineSessionState.GetAlias(this.commandName);
      }
      if (commandInfo != null)
        CommandDiscovery.discoveryTracer.WriteLine("Alias found: {0}  {1}", (object) commandInfo.Name, (object) commandInfo.Definition);
      return commandInfo;
    }

    private CommandInfo GetNextFunction()
    {
      CommandInfo commandInfo = (CommandInfo) null;
      if ((this.commandResolutionOptions & SearchResolutionOptions.ResolveFunctionPatterns) != SearchResolutionOptions.None)
      {
        if (this.matchingFunctionEnumerator == null)
        {
          Collection<CommandInfo> collection = new Collection<CommandInfo>();
          WildcardPattern wildcardPattern = new WildcardPattern(this.commandName, WildcardOptions.IgnoreCase);
          foreach (DictionaryEntry dictionaryEntry in this._context.EngineSessionState.GetFunctionTable())
          {
            if (wildcardPattern.IsMatch((string) dictionaryEntry.Key))
              collection.Add((CommandInfo) dictionaryEntry.Value);
          }
          this.matchingFunctionEnumerator = collection.GetEnumerator();
        }
        if (!this.matchingFunctionEnumerator.MoveNext())
        {
          this.currentState = CommandSearcher.SearchState.FunctionResolution;
          this.matchingFunctionEnumerator = (IEnumerator<CommandInfo>) null;
        }
        else
          commandInfo = this.matchingFunctionEnumerator.Current;
      }
      else
      {
        this.currentState = CommandSearcher.SearchState.FunctionResolution;
        commandInfo = this.GetFunction(this.commandName);
      }
      return commandInfo;
    }

    private CommandInfo GetFunction(string function)
    {
      CommandInfo commandInfo = (CommandInfo) this._context.EngineSessionState.GetFunction(function);
      if (commandInfo != null)
      {
        if (commandInfo is FilterInfo)
          CommandDiscovery.discoveryTracer.WriteLine("Filter found: {0}", (object) function);
        else
          CommandDiscovery.discoveryTracer.WriteLine("Function found: {0}  {1}", (object) function);
      }
      else if (function.IndexOf('\\') > 0)
      {
        PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(function);
        if (instance != null && !string.IsNullOrEmpty(instance.PSSnapInName))
        {
          string psSnapInName = instance.PSSnapInName;
          PSModuleInfo psModuleInfo1 = (PSModuleInfo) null;
          List<PSModuleInfo> modules = this._context.Modules.GetModules(new string[1]
          {
            psSnapInName
          }, false);
          if (modules != null)
          {
            if (modules.Count == 1)
            {
              if (modules[0].ModuleType != ModuleType.Binary)
                psModuleInfo1 = modules[0];
            }
            else
            {
              foreach (PSModuleInfo psModuleInfo2 in modules)
              {
                if (psModuleInfo2.ModuleType != ModuleType.Binary)
                {
                  psModuleInfo1 = psModuleInfo2;
                  break;
                }
              }
            }
            if (psModuleInfo1 != null && psModuleInfo1.ExportedFunctions.ContainsKey(instance.ShortName))
              commandInfo = (CommandInfo) psModuleInfo1.ExportedFunctions[instance.ShortName];
          }
        }
      }
      return commandInfo;
    }

    private CmdletInfo GetNextCmdlet()
    {
      CmdletInfo result = (CmdletInfo) null;
      if (this.matchingCmdlet == null)
      {
        Collection<CmdletInfo> matchingCmdlets;
        if ((this.commandResolutionOptions & SearchResolutionOptions.CommandNameIsPattern) != SearchResolutionOptions.None)
        {
          matchingCmdlets = new Collection<CmdletInfo>();
          PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(this.commandName);
          if (instance == null)
            return result;
          WildcardPattern wildcardPattern = new WildcardPattern(instance.ShortName, WildcardOptions.IgnoreCase);
          Dictionary<string, List<CmdletInfo>> cmdletCache = this._context.EngineSessionState.CmdletCache;
          while (true)
          {
            lock (cmdletCache)
            {
              foreach (List<CmdletInfo> cmdletInfoList in cmdletCache.Values)
              {
                foreach (CmdletInfo cmdletInfo in cmdletInfoList)
                {
                  if (wildcardPattern.IsMatch(cmdletInfo.Name) && (string.IsNullOrEmpty(instance.PSSnapInName) || instance.PSSnapInName.Equals(cmdletInfo.ModuleName, StringComparison.OrdinalIgnoreCase)))
                    matchingCmdlets.Add(cmdletInfo);
                }
              }
            }
            if (cmdletCache != this._context.TopLevelSessionState.CmdletCache)
              cmdletCache = this._context.TopLevelSessionState.CmdletCache;
            else
              break;
          }
        }
        else
        {
          matchingCmdlets = this._context.CommandDiscovery.GetCmdletInfo(this.commandName);
          if (matchingCmdlets.Count > 1)
          {
            if ((this.commandResolutionOptions & SearchResolutionOptions.ReturnFirstDuplicateCmdletName) != SearchResolutionOptions.None)
            {
              this.matchingCmdlet = matchingCmdlets.GetEnumerator();
              while (this.matchingCmdlet.MoveNext())
              {
                if (result == null)
                  result = this.matchingCmdlet.Current;
              }
              return this.traceResult(result);
            }
            if ((this.commandResolutionOptions & SearchResolutionOptions.AllowDuplicateCmdletNames) == SearchResolutionOptions.None)
              throw this.NewAmbiguousCmdletName(this.commandName, matchingCmdlets);
          }
        }
        this.matchingCmdlet = matchingCmdlets.GetEnumerator();
      }
      if (!this.matchingCmdlet.MoveNext())
      {
        this.currentState = CommandSearcher.SearchState.CmdletResolution;
        this.matchingCmdlet = (IEnumerator<CmdletInfo>) null;
      }
      else
        result = this.matchingCmdlet.Current;
      return this.traceResult(result);
    }

    private CmdletInfo traceResult(CmdletInfo result)
    {
      if (result != null)
        CommandDiscovery.discoveryTracer.WriteLine("Cmdlet found: {0}  {1}", (object) result.Name, (object) result.ImplementingType.AssemblyQualifiedName);
      return result;
    }

    private CommandNotFoundException NewAmbiguousCmdletName(
      string name,
      Collection<CmdletInfo> matchingCmdlets)
    {
      string possibleMatches = this.GetPossibleMatches(matchingCmdlets);
      return new CommandNotFoundException(name, (Exception) null, "CmdletNameAmbiguous", new object[1]
      {
        (object) possibleMatches
      });
    }

    private string GetPossibleMatches(Collection<CmdletInfo> matchingCmdlets)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (CmdletInfo matchingCmdlet in matchingCmdlets)
        stringBuilder.AppendFormat(" {0}", (object) matchingCmdlet.FullName);
      return stringBuilder.ToString();
    }

    private ScriptInfo GetNextBuiltinScript()
    {
      ScriptInfo scriptInfo = (ScriptInfo) null;
      if ((this.commandResolutionOptions & SearchResolutionOptions.CommandNameIsPattern) != SearchResolutionOptions.None)
      {
        if (this.matchingScript == null)
        {
          Collection<string> collection = new Collection<string>();
          WildcardPattern wildcardPattern1 = new WildcardPattern(this.commandName, WildcardOptions.IgnoreCase);
          WildcardPattern wildcardPattern2 = new WildcardPattern(this.commandName + ".ps1", WildcardOptions.IgnoreCase);
          foreach (string key in this._context.CommandDiscovery.ScriptCache.Keys)
          {
            if (wildcardPattern1.IsMatch(key) || wildcardPattern2.IsMatch(key))
              collection.Add(key);
          }
          this.matchingScript = collection.GetEnumerator();
        }
        if (!this.matchingScript.MoveNext())
        {
          this.currentState = CommandSearcher.SearchState.BuiltinScriptResolution;
          this.matchingScript = (IEnumerator<string>) null;
        }
        else
          scriptInfo = this._context.CommandDiscovery.GetScriptInfo(this.matchingScript.Current);
      }
      else
      {
        this.currentState = CommandSearcher.SearchState.BuiltinScriptResolution;
        scriptInfo = this._context.CommandDiscovery.GetScriptInfo(this.commandName) ?? this._context.CommandDiscovery.GetScriptInfo(this.commandName + ".ps1");
      }
      if (scriptInfo != null)
        CommandDiscovery.discoveryTracer.WriteLine("Script found: {0}", (object) scriptInfo.Name);
      return scriptInfo;
    }

    private string DoPowerShellRelativePathLookup()
    {
      string str = (string) null;
      if (this._context.EngineSessionState != null && this._context.EngineSessionState.ProviderCount > 0 && (this.commandName[0] == '.' || this.commandName[0] == '~'))
      {
        using (CommandDiscovery.discoveryTracer.TraceScope("{0} appears to be a relative path. Trying to resolve relative path", (object) this.commandName))
          str = this.ResolvePSPath(this.commandName);
      }
      return str;
    }

    private string ResolvePSPath(string path)
    {
      string str = (string) null;
      ProviderInfo provider = (ProviderInfo) null;
      try
      {
        string providerPath = this._context.LocationGlobber.GetProviderPath(path, out provider);
        if (provider.NameEquals(this._context.ProviderNames.FileSystem))
        {
          str = providerPath;
          CommandDiscovery.discoveryTracer.WriteLine("The relative path was resolved to: {0}", (object) str);
        }
        else
          CommandDiscovery.discoveryTracer.TraceError("The relative path was not a file system path. {0}", (object) path);
      }
      catch (InvalidOperationException ex)
      {
        CommandDiscovery.discoveryTracer.TraceError("The home path was not specified for the provider. {0}", (object) path);
      }
      catch (ProviderInvocationException ex)
      {
        CommandDiscovery.discoveryTracer.TraceError("While resolving the path, \"{0}\", an error was encountered by the provider: {1}", (object) path, (object) ex.Message);
      }
      catch (ItemNotFoundException ex)
      {
        CommandDiscovery.discoveryTracer.TraceError("The path does not exist: {0}", (object) path);
      }
      catch (DriveNotFoundException ex)
      {
        CommandDiscovery.discoveryTracer.TraceError("The drive does not exist: {0}", (object) ex.ItemName);
      }
      CommandSearcher.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    internal IEnumerable<string> ConstructSearchPatternsFromName(string name)
    {
      Collection<string> collection = new Collection<string>();
      bool flag = false;
      if (!string.IsNullOrEmpty(Path.GetExtension(name)))
      {
        collection.Add(name);
        flag = true;
      }
      if ((this.commandTypes & CommandTypes.ExternalScript) != (CommandTypes) 0)
      {
        collection.Add(name + ".ps1");
        collection.Add(name + ".psm1");
        collection.Add(name + ".psd1");
      }
      if ((this.commandTypes & CommandTypes.Application) != (CommandTypes) 0)
      {
        foreach (string pathExtension in this._context.CommandDiscovery.PathExtensions)
          collection.Add(name + pathExtension);
      }
      if (!flag)
        collection.Add(name);
      return (IEnumerable<string>) collection;
    }

    private static bool IsQualifiedPSPath(string commandName)
    {
      bool flag = LocationGlobber.IsAbsolutePath(commandName) || LocationGlobber.IsProviderQualifiedPath(commandName) || LocationGlobber.IsHomePath(commandName) || LocationGlobber.IsProviderDirectPath(commandName);
      CommandSearcher.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    private static CommandSearcher.CanDoPathLookupResult CanDoPathLookup(
      string possiblePath)
    {
      CommandSearcher.CanDoPathLookupResult pathLookupResult = CommandSearcher.CanDoPathLookupResult.Yes;
      if (WildcardPattern.ContainsWildcardCharacters(possiblePath))
      {
        pathLookupResult = CommandSearcher.CanDoPathLookupResult.WildcardCharacters;
      }
      else
      {
        try
        {
          if (Path.IsPathRooted(possiblePath))
          {
            pathLookupResult = CommandSearcher.CanDoPathLookupResult.PathIsRooted;
            goto label_9;
          }
        }
        catch (ArgumentException ex)
        {
          pathLookupResult = CommandSearcher.CanDoPathLookupResult.IllegalCharacters;
          goto label_9;
        }
        if (possiblePath.IndexOfAny(Utils.DirectorySeparators) != -1)
          pathLookupResult = CommandSearcher.CanDoPathLookupResult.DirectorySeparator;
        else if (possiblePath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
          pathLookupResult = CommandSearcher.CanDoPathLookupResult.IllegalCharacters;
      }
label_9:
      CommandSearcher.tracer.WriteLine("result = {0}", (object) pathLookupResult);
      return pathLookupResult;
    }

    private void setupPathSearcher()
    {
      if (this.pathSearcher != null)
        return;
      if ((this.commandResolutionOptions & SearchResolutionOptions.CommandNameIsPattern) != SearchResolutionOptions.None)
      {
        this.canDoPathLookup = true;
        this.canDoPathLookupResult = CommandSearcher.CanDoPathLookupResult.Yes;
        this.pathSearcher = new CommandPathSearch((IEnumerable<string>) new Collection<string>()
        {
          this.commandName
        }, this._context.CommandDiscovery.GetLookupDirectoryPaths(), this._context);
      }
      else
      {
        this.canDoPathLookupResult = CommandSearcher.CanDoPathLookup(this.commandName);
        if (this.canDoPathLookupResult == CommandSearcher.CanDoPathLookupResult.Yes)
        {
          this.canDoPathLookup = true;
          this.pathSearcher = new CommandPathSearch(this.ConstructSearchPatternsFromName(this.commandName), this._context.CommandDiscovery.GetLookupDirectoryPaths(), this._context);
        }
        else if (this.canDoPathLookupResult == CommandSearcher.CanDoPathLookupResult.PathIsRooted)
        {
          this.canDoPathLookup = true;
          string directoryName = Path.GetDirectoryName(this.commandName);
          Collection<string> collection = new Collection<string>();
          collection.Add(directoryName);
          CommandDiscovery.discoveryTracer.WriteLine("The path is rooted, so only doing the lookup in the specified directory: {0}", (object) directoryName);
          string fileName = Path.GetFileName(this.commandName);
          if (!string.IsNullOrEmpty(fileName))
            this.pathSearcher = new CommandPathSearch(this.ConstructSearchPatternsFromName(fileName), (IEnumerable<string>) collection, this._context);
          else
            this.canDoPathLookup = false;
        }
        else
        {
          if (this.canDoPathLookupResult != CommandSearcher.CanDoPathLookupResult.DirectorySeparator)
            return;
          this.canDoPathLookup = true;
          string str = this.ResolvePSPath(Path.GetDirectoryName(this.commandName));
          CommandDiscovery.discoveryTracer.WriteLine("The path is relative, so only doing the lookup in the specified directory: {0}", (object) str);
          if (str == null)
          {
            this.canDoPathLookup = false;
          }
          else
          {
            Collection<string> collection = new Collection<string>();
            collection.Add(str);
            string fileName = Path.GetFileName(this.commandName);
            if (!string.IsNullOrEmpty(fileName))
              this.pathSearcher = new CommandPathSearch(this.ConstructSearchPatternsFromName(fileName), (IEnumerable<string>) collection, this._context);
            else
              this.canDoPathLookup = false;
          }
        }
      }
    }

    public void Reset()
    {
      if (this._commandOrigin == CommandOrigin.Runspace)
      {
        if (this._context.EngineSessionState.Applications.Count == 0)
          this.commandTypes &= ~CommandTypes.Application;
        if (this._context.EngineSessionState.Scripts.Count == 0)
          this.commandTypes &= ~CommandTypes.ExternalScript;
      }
      if (this.pathSearcher != null)
        this.pathSearcher.Reset();
      this._currentMatch = (CommandInfo) null;
      this.currentState = CommandSearcher.SearchState.Reset;
      this.matchingAlias = (IEnumerator<AliasInfo>) null;
      this.matchingCmdlet = (IEnumerator<CmdletInfo>) null;
      this.matchingScript = (IEnumerator<string>) null;
    }

    internal CommandOrigin CommandOrigin
    {
      get => this._commandOrigin;
      set => this._commandOrigin = value;
    }

    private enum CanDoPathLookupResult
    {
      Yes,
      PathIsRooted,
      WildcardCharacters,
      DirectorySeparator,
      IllegalCharacters,
    }

    private enum SearchState
    {
      Reset,
      AliasResolution,
      FunctionResolution,
      CmdletResolution,
      BuiltinScriptResolution,
      PowerShellPathResolution,
      QualifiedFileSystemPath,
      PathSearch,
      GetPathSearch,
      PowerShellRelativePath,
      NoMoreMatches,
    }
  }
}
