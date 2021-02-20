// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ExecutionContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell;
using Microsoft.PowerShell.Commands.Internal.Format;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Internal.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Security;

namespace System.Management.Automation
{
  internal class ExecutionContext
  {
    private const int MaximumErrorProcessingScopes = 10;
    private PSLocalEventManager eventManager;
    private Debugger _debugger;
    private int mshdebug;
    private bool stepScript;
    private bool _scriptCommandProcessorShouldRethrowExit;
    private bool ignoreScriptDebug = true;
    private AutomationEngine _engine;
    private RunspaceConfiguration _runspaceConfiguration;
    private InitialSessionState _initialSessionState;
    private string _moduleBeingProcessed;
    private AuthorizationManager _authorizationManager;
    private ProviderNames providerNames;
    private ModuleIntrinsics _modules;
    private string _shellId;
    private SessionStateInternal _engineSessionState;
    private SessionStateInternal _topLevelSessionState;
    private PSLanguageMode _languageMode;
    private LocationGlobber _locationGlobber;
    private Dictionary<string, Assembly> _assemblyCache;
    private EngineState _engineState;
    private HelpSystem _helpSystem;
    private object _formatInfo;
    private CommandFactory commandFactory;
    private CommandProcessorBase currentCommandProcessor;
    private InternalHost myHostInterface;
    private EngineIntrinsics _engineIntrinsics;
    private PipelineWriter externalSuccessOutput;
    private PipelineWriter _externalErrorOutput;
    private PipelineWriter _externalProgressOutput;
    private Pipe shellFunctionErrorOutputPipe;
    private Pipe topLevelPipe;
    internal Dictionary<string, CommandMetadata> CommandMetadataCache = new Dictionary<string, CommandMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    internal Dictionary<string, InternalParameterMetadata> ParameterMetadataCache = new Dictionary<string, InternalParameterMetadata>((IEqualityComparer<string>) StringComparer.Ordinal);
    private int[] currentDepths = new int[2]{ 1, 1 };
    private int[] maximumScopeDepths = new int[2]
    {
      1000,
      1000
    };
    private bool ProcessingScopeDepthErrors;
    private Runspace currentRunspace;
    private bool exceptionHandlerInEnclosingStatementBlock;
    private Exception currentExceptionBeingHandled;
    private VariableDereferenceNode _underbarVariableNode = new VariableDereferenceNode("_", 0);
    private static readonly ScopedItemLookupPath _underbarVariablePath = new ScopedItemLookupPath("_");
    private VariableDereferenceNode _scriptThisNode = new VariableDereferenceNode("this", 1);
    private bool _questionMarkVariableValue = true;
    private VariableDereferenceNode _argsVariableNode = new VariableDereferenceNode("local:args", 3);
    private static readonly ScopedItemLookupPath _argsVariablePath = new ScopedItemLookupPath("local:args");
    private VariableDereferenceNode _inputVariableNode = new VariableDereferenceNode("input", 2);
    private static readonly ScopedItemLookupPath _inputVariablePath = new ScopedItemLookupPath("local:input");
    private static readonly ScopedItemLookupPath _dollarErrorVariablePath = new ScopedItemLookupPath("global:error");
    private static readonly ScopedItemLookupPath _eventDollarErrorVariablePath = new ScopedItemLookupPath("script:error");
    private VariableDereferenceNode _PSCmdletVariableNode = new VariableDereferenceNode("PSCmdlet", 4);
    private TypeTable _typeTable;
    private TypeInfoDataBaseManager _formatDBManager;
    internal PSTransactionManager transactionManager;
    private bool _assemblyCacheInitialized;
    private static bool _assemblyEventHandlerSet = false;
    private static object lockObject = new object();
    [TraceSource("ExecutionContext", "The execution context of a particular instance of the engine")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ExecutionContext), "The execution context of a particular instance of the engine");

    internal PSLocalEventManager Events => this.eventManager;

    internal Debugger Debugger => this._debugger;

    internal int PSDebug
    {
      get => this.ignoreScriptDebug || this.currentDepths[0] < 2 ? 0 : this.mshdebug;
      set => this.mshdebug = value;
    }

    internal bool StepScript
    {
      get => !this.ignoreScriptDebug && this.currentDepths[0] >= 2 && this.stepScript;
      set => this.stepScript = value;
    }

    internal bool IsStrictVersion(int majorVersion)
    {
      for (SessionStateScope sessionStateScope = this.EngineSessionState.CurrentScope; sessionStateScope != null; sessionStateScope = sessionStateScope.Parent)
      {
        if (sessionStateScope.StrictModeVersion != (Version) null)
          return sessionStateScope.StrictModeVersion.Major >= majorVersion;
      }
      return false;
    }

    internal bool ShouldTraceStatement
    {
      get
      {
        if (this.ignoreScriptDebug || this.currentDepths[0] < 2)
          return false;
        return this.mshdebug > 0 || this.stepScript;
      }
    }

    internal bool ScriptCommandProcessorShouldRethrowExit
    {
      get => this._scriptCommandProcessorShouldRethrowExit;
      set => this._scriptCommandProcessorShouldRethrowExit = value;
    }

    internal bool IgnoreScriptDebug
    {
      set => this.ignoreScriptDebug = value;
      get => this.ignoreScriptDebug;
    }

    internal AutomationEngine Engine => this._engine;

    internal RunspaceConfiguration RunspaceConfiguration => this._runspaceConfiguration;

    internal InitialSessionState InitialSessionState => this._initialSessionState;

    internal bool IsSingleShell => this.RunspaceConfiguration is RunspaceConfigForSingleShell;

    internal string ModuleBeingProcessed
    {
      get => this._moduleBeingProcessed;
      set => this._moduleBeingProcessed = value;
    }

    internal AuthorizationManager AuthorizationManager => this._authorizationManager;

    internal ProviderNames ProviderNames
    {
      get
      {
        if (this.providerNames == null)
          this.providerNames = !this.IsSingleShell ? (ProviderNames) new CustomShellProviderNames() : (ProviderNames) new SingleShellProviderNames();
        return this.providerNames;
      }
    }

    internal ModuleIntrinsics Modules => this._modules;

    internal string ShellID
    {
      get
      {
        if (this._shellId == null)
          this._shellId = !(this._authorizationManager is PSAuthorizationManager) || string.IsNullOrEmpty(this._authorizationManager.ShellId) ? (this._runspaceConfiguration == null || string.IsNullOrEmpty(this._runspaceConfiguration.ShellId) ? Utils.DefaultPowerShellShellID : this._runspaceConfiguration.ShellId) : this._authorizationManager.ShellId;
        return this._shellId;
      }
    }

    internal SessionStateInternal EngineSessionState
    {
      get => this._engineSessionState;
      set => this._engineSessionState = value;
    }

    internal SessionStateInternal TopLevelSessionState => this._topLevelSessionState;

    internal SessionState SessionState => this._engineSessionState.PublicSessionState;

    internal PSLanguageMode LanguageMode
    {
      get => this._languageMode;
      set => this._languageMode = value;
    }

    internal bool UseFullLanguageModeInDebugger => this._initialSessionState != null && this._initialSessionState.UseFullLanguageModeInDebugger;

    internal LocationGlobber LocationGlobber
    {
      get
      {
        this._locationGlobber = new LocationGlobber(this.SessionState);
        return this._locationGlobber;
      }
    }

    internal Dictionary<string, Assembly> AssemblyCache => this._assemblyCache;

    internal EngineState EngineState
    {
      get => this._engineState;
      set => this._engineState = value;
    }

    internal object GetVariable(string name) => this._engineSessionState.GetVariableValue(name);

    internal object GetVariable(string name, object defaultValue) => this._engineSessionState.GetVariableValue(name, defaultValue);

    internal void SetVariable(string name, object newValue) => this._engineSessionState.SetVariableValue(name, newValue, CommandOrigin.Internal);

    internal void RemoveVariable(string name) => this._engineSessionState.RemoveVariable(name);

    internal T GetEnumPreference<T>(
      ScopedItemLookupPath preferenceVariablePath,
      T defaultPref,
      out bool defaultUsed)
    {
      CmdletProviderContext context = (CmdletProviderContext) null;
      SessionStateScope scope = (SessionStateScope) null;
      object variableValue = this.EngineSessionState.GetVariableValue(preferenceVariablePath, out context, out scope);
      if (variableValue is T obj)
      {
        defaultUsed = false;
        return obj;
      }
      defaultUsed = true;
      T obj1 = defaultPref;
      if (variableValue != null)
      {
        try
        {
          if (variableValue is string str)
          {
            obj1 = (T) Enum.Parse(typeof (T), str, true);
            defaultUsed = false;
          }
          else
          {
            obj1 = (T) variableValue;
            defaultUsed = false;
          }
        }
        catch (InvalidCastException ex)
        {
        }
        catch (ArgumentException ex)
        {
        }
      }
      return obj1;
    }

    internal bool GetBooleanPreference(
      ScopedItemLookupPath preferenceVariablePath,
      bool defaultPref,
      out bool defaultUsed)
    {
      CmdletProviderContext context = (CmdletProviderContext) null;
      SessionStateScope scope = (SessionStateScope) null;
      object variableValue = this.EngineSessionState.GetVariableValue(preferenceVariablePath, out context, out scope);
      if (variableValue == null)
      {
        defaultUsed = true;
        return defaultPref;
      }
      bool result = defaultPref;
      defaultUsed = !LanguagePrimitives.TryConvertTo<bool>(variableValue, out result);
      return !defaultUsed ? result : defaultPref;
    }

    internal HelpSystem HelpSystem
    {
      get
      {
        if (this._helpSystem == null)
          this._helpSystem = new HelpSystem(this);
        return this._helpSystem;
      }
    }

    internal object FormatInfo
    {
      get => this._formatInfo;
      set => this._formatInfo = value;
    }

    internal CommandProcessorBase CreateCommand(string command)
    {
      if (this.commandFactory == null)
        this.commandFactory = new CommandFactory(this);
      CommandProcessorBase command1 = this.commandFactory.CreateCommand(command, this.EngineSessionState.currentScope.ScopeOrigin);
      if (command1 != null && command1 is ScriptCommandProcessor)
        command1.Command.CommandOriginInternal = CommandOrigin.Internal;
      return command1;
    }

    internal CommandProcessorBase CurrentCommandProcessor
    {
      get => this.currentCommandProcessor;
      set => this.currentCommandProcessor = value;
    }

    internal CommandDiscovery CommandDiscovery => this._engine.CommandDiscovery;

    internal InternalHost EngineHostInterface => this.myHostInterface;

    internal InternalHost InternalHost => this.myHostInterface;

    internal EngineIntrinsics EngineIntrinsics
    {
      get
      {
        if (this._engineIntrinsics == null)
          this._engineIntrinsics = new EngineIntrinsics(this);
        return this._engineIntrinsics;
      }
    }

    internal PipelineWriter ExternalSuccessOutput
    {
      get => this.externalSuccessOutput;
      set => this.externalSuccessOutput = value;
    }

    internal PipelineWriter ExternalErrorOutput
    {
      get => this._externalErrorOutput;
      set => this._externalErrorOutput = value;
    }

    internal PipelineWriter ExternalProgressOutput
    {
      get => this._externalProgressOutput;
      set => this._externalProgressOutput = value;
    }

    internal void ResetRedirection()
    {
      this.shellFunctionErrorOutputPipe = (Pipe) null;
      this.topLevelPipe = (Pipe) null;
    }

    internal Pipe ShellFunctionErrorOutputPipe
    {
      get => this.shellFunctionErrorOutputPipe;
      set
      {
        if (this.topLevelPipe == null)
          this.topLevelPipe = value;
        this.shellFunctionErrorOutputPipe = value;
      }
    }

    internal void ResetShellFunctionErrorOutputPipe()
    {
      this.topLevelPipe = (Pipe) null;
      this.shellFunctionErrorOutputPipe = (Pipe) null;
    }

    internal ExecutionContext.SavedContextData SaveContextData() => new ExecutionContext.SavedContextData(this);

    internal bool IsTopLevelPipe(Pipe pipeToCheck) => pipeToCheck == this.topLevelPipe;

    internal Pipe RedirectErrorPipe(Pipe newPipe)
    {
      Pipe functionErrorOutputPipe = this.shellFunctionErrorOutputPipe;
      this.ShellFunctionErrorOutputPipe = newPipe;
      return functionErrorOutputPipe;
    }

    internal void RestoreErrorPipe(Pipe pipe) => this.shellFunctionErrorOutputPipe = pipe;

    internal void AppendDollarError(object obj)
    {
      switch (obj)
      {
        case ErrorRecord errorRecord:
        case Exception _:
          if (!(this.DollarErrorVariable is ArrayList dollarErrorVariable) || dollarErrorVariable.Count > 0 && (dollarErrorVariable[0] == obj || dollarErrorVariable[0] is ErrorRecord errorRecord && errorRecord != null && errorRecord.Exception == errorRecord.Exception))
            break;
          object fastValue = (object) this.EngineSessionState.CurrentScope.ErrorCapacity.FastValue;
          if (fastValue != null)
          {
            try
            {
              fastValue = LanguagePrimitives.ConvertTo(fastValue, typeof (int), (IFormatProvider) CultureInfo.InvariantCulture);
            }
            catch (PSInvalidCastException ex)
            {
            }
            catch (OverflowException ex)
            {
            }
            catch (Exception ex)
            {
              ExecutionContext.tracer.TraceException(ex);
              throw;
            }
          }
          if (!(fastValue is int num2))
            num2 = 256;
          int num1 = num2;
          if (0 > num1)
            num1 = 0;
          else if (32768 < num1)
            num1 = 32768;
          if (0 >= num1)
          {
            dollarErrorVariable.Clear();
            break;
          }
          int count = dollarErrorVariable.Count - (num1 - 1);
          if (0 < count)
            dollarErrorVariable.RemoveRange(num1 - 1, count);
          dollarErrorVariable.Insert(0, obj);
          break;
      }
    }

    internal int[] ScopeDepths
    {
      get => this.currentDepths;
      set => this.currentDepths = value;
    }

    internal void ResetScopeDepth()
    {
      this.ProcessingScopeDepthErrors = false;
      if (PsUtils.GetStackSize() < 1048576U)
        this.maximumScopeDepths[1] = 50;
      for (int index = 0; index < this.currentDepths.Length; ++index)
        this.currentDepths[index] = 1;
    }

    internal int IncrementScopeDepth(
      ExecutionContext.FeaturesThatNeedDepthHandling feature)
    {
      int index = (int) feature;
      int maximumScopeDepth = this.maximumScopeDepths[index];
      if (this.ProcessingScopeDepthErrors)
        maximumScopeDepth += 10;
      ++this.currentDepths[index];
      if (this.CurrentPipelineStopping)
        throw new PipelineStoppedException();
      if (this.currentDepths[index] > maximumScopeDepth)
      {
        this.ProcessingScopeDepthErrors = true;
        Exception exceptionRecord = feature != ExecutionContext.FeaturesThatNeedDepthHandling.ScriptScope ? (Exception) new PipelineDepthException(this.currentDepths[index], maximumScopeDepth) : (Exception) new ScriptCallDepthException(this.currentDepths[index], maximumScopeDepth);
        ExecutionContext.tracer.TraceException(exceptionRecord);
        throw exceptionRecord;
      }
      return this.currentDepths[index];
    }

    internal int DecrementScopeDepth(
      ExecutionContext.FeaturesThatNeedDepthHandling feature)
    {
      int index = (int) feature;
      if (this.currentDepths[index] > 0)
        --this.currentDepths[index];
      return this.currentDepths[index];
    }

    internal Runspace CurrentRunspace
    {
      get => this.currentRunspace;
      set => this.currentRunspace = value;
    }

    internal void PushPipelineProcessor(PipelineProcessor pp)
    {
      if (this.currentRunspace == null)
        return;
      ((LocalPipeline) this.currentRunspace.GetCurrentlyRunningPipeline())?.Stopper.Push(pp);
    }

    internal void PopPipelineProcessor()
    {
      if (this.currentRunspace == null)
        return;
      ((LocalPipeline) this.currentRunspace.GetCurrentlyRunningPipeline())?.Stopper.Pop();
    }

    internal bool CurrentPipelineStopping
    {
      get
      {
        if (this.currentRunspace == null)
          return false;
        LocalPipeline currentlyRunningPipeline = (LocalPipeline) this.currentRunspace.GetCurrentlyRunningPipeline();
        return currentlyRunningPipeline != null && currentlyRunningPipeline.IsStopping;
      }
    }

    internal bool ExceptionHandlerInEnclosingStatementBlock
    {
      get => this.exceptionHandlerInEnclosingStatementBlock;
      set => this.exceptionHandlerInEnclosingStatementBlock = value;
    }

    internal Exception CurrentExceptionBeingHandled
    {
      get => this.currentExceptionBeingHandled;
      set => this.currentExceptionBeingHandled = value;
    }

    internal object UnderbarVariable
    {
      get => this._underbarVariableNode.Execute(this.EngineSessionState.ExecutionContext);
      set => this._underbarVariableNode.SetValue(value, this.EngineSessionState.ExecutionContext);
    }

    internal static ScopedItemLookupPath UnderbarVariablePath => ExecutionContext._underbarVariablePath;

    internal object ScriptThisVariable
    {
      get => this._scriptThisNode.Execute(this.EngineSessionState.ExecutionContext);
      set => this._scriptThisNode.SetValue(value, this.EngineSessionState.ExecutionContext);
    }

    internal bool QuestionMarkVariableValue
    {
      get => this._questionMarkVariableValue;
      set => this._questionMarkVariableValue = value;
    }

    internal object ArgsVariable
    {
      get => this._argsVariableNode.Execute(this.EngineSessionState.ExecutionContext);
      set => this._argsVariableNode.SetValue(value, this.EngineSessionState.ExecutionContext);
    }

    internal static ScopedItemLookupPath ArgsVariablePath => ExecutionContext._argsVariablePath;

    internal object InputVariable
    {
      get => this._inputVariableNode.Execute(this.EngineSessionState.ExecutionContext);
      set => this._inputVariableNode.SetValue(value, this.EngineSessionState.ExecutionContext);
    }

    internal static ScopedItemLookupPath InputVariablePath => ExecutionContext._inputVariablePath;

    internal object DollarErrorVariable
    {
      get
      {
        CmdletProviderContext context = (CmdletProviderContext) null;
        SessionStateScope scope = (SessionStateScope) null;
        return this.eventManager.IsExecutingEventAction ? this.EngineSessionState.GetVariableValue(ExecutionContext._eventDollarErrorVariablePath, out context, out scope) : this.EngineSessionState.GetVariableValue(ExecutionContext._dollarErrorVariablePath, out context, out scope);
      }
      set => this.EngineSessionState.SetVariable(ExecutionContext._dollarErrorVariablePath, value, true, CommandOrigin.Internal);
    }

    internal ActionPreference DebugPreferenceVariable
    {
      get
      {
        bool defaultUsed = false;
        return this.GetEnumPreference<ActionPreference>(InitialSessionState.debugPreferenceVariablePath, ActionPreference.SilentlyContinue, out defaultUsed);
      }
      set => this.EngineSessionState.SetVariable(InitialSessionState.debugPreferenceVariablePath, LanguagePrimitives.ConvertTo((object) value, typeof (ActionPreference), (IFormatProvider) CultureInfo.InvariantCulture), true, CommandOrigin.Internal);
    }

    internal ActionPreference VerbosePreferenceVariable
    {
      get
      {
        bool defaultUsed = false;
        return this.GetEnumPreference<ActionPreference>(InitialSessionState.verbosePreferenceVariablePath, ActionPreference.SilentlyContinue, out defaultUsed);
      }
      set => this.EngineSessionState.SetVariable(InitialSessionState.verbosePreferenceVariablePath, LanguagePrimitives.ConvertTo((object) value, typeof (ActionPreference), (IFormatProvider) CultureInfo.InvariantCulture), true, CommandOrigin.Internal);
    }

    internal ActionPreference ErrorActionPreferenceVariable
    {
      get
      {
        bool defaultUsed = false;
        return this.GetEnumPreference<ActionPreference>(InitialSessionState.errorActionPreferenceVariablePath, ActionPreference.Continue, out defaultUsed);
      }
      set => this.EngineSessionState.SetVariable(InitialSessionState.errorActionPreferenceVariablePath, LanguagePrimitives.ConvertTo((object) value, typeof (ActionPreference), (IFormatProvider) CultureInfo.InvariantCulture), true, CommandOrigin.Internal);
    }

    internal object WhatIfPreferenceVariable
    {
      get
      {
        CmdletProviderContext context = (CmdletProviderContext) null;
        SessionStateScope scope = (SessionStateScope) null;
        return this.EngineSessionState.GetVariableValue(InitialSessionState.whatIfPreferenceVariablePath, out context, out scope);
      }
      set => this.EngineSessionState.SetVariable(InitialSessionState.whatIfPreferenceVariablePath, value, true, CommandOrigin.Internal);
    }

    internal ConfirmImpact ConfirmPreferenceVariable
    {
      get
      {
        bool defaultUsed = false;
        return this.GetEnumPreference<ConfirmImpact>(InitialSessionState.confirmPreferenceVariablePath, ConfirmImpact.High, out defaultUsed);
      }
      set => this.EngineSessionState.SetVariable(InitialSessionState.confirmPreferenceVariablePath, LanguagePrimitives.ConvertTo((object) value, typeof (ConfirmImpact), (IFormatProvider) CultureInfo.InvariantCulture), true, CommandOrigin.Internal);
    }

    internal object PSCmdletVariable
    {
      get => this._PSCmdletVariableNode.Execute(this.EngineSessionState.ExecutionContext);
      set => this._PSCmdletVariableNode.SetValue(value, this.EngineSessionState.ExecutionContext);
    }

    internal void RunspaceClosingNotification()
    {
      if (this.RunspaceConfiguration != null)
        this.RunspaceConfiguration.Unbind(this);
      this.EngineSessionState.RunspaceClosingNotification();
    }

    internal TypeTable TypeTable
    {
      get
      {
        if (this.RunspaceConfiguration != null && this.RunspaceConfiguration.TypeTable != null)
          return this.RunspaceConfiguration.TypeTable;
        if (this._typeTable == null)
          this._typeTable = new TypeTable();
        return this._typeTable;
      }
      set
      {
        if (this.RunspaceConfiguration != null)
          throw new NotImplementedException("set_TypeTable()");
        this._typeTable = value;
      }
    }

    internal TypeInfoDataBaseManager FormatDBManager
    {
      get
      {
        if (this.RunspaceConfiguration != null && this.RunspaceConfiguration.FormatDBManager != null)
          return this.RunspaceConfiguration.FormatDBManager;
        if (this._formatDBManager == null)
        {
          this._formatDBManager = new TypeInfoDataBaseManager();
          this._formatDBManager.Update(this.AuthorizationManager, (PSHost) this.EngineHostInterface);
        }
        return this._formatDBManager;
      }
      set
      {
        if (this.RunspaceConfiguration != null)
          throw new NotImplementedException("set_FormatDBManager()");
        this._formatDBManager = value;
      }
    }

    internal PSTransactionManager TransactionManager => this.transactionManager;

    internal void UpdateAssemblyCache()
    {
      string str = "";
      if (this.RunspaceConfiguration == null)
        return;
      if (!this._assemblyCacheInitialized)
      {
        foreach (AssemblyConfigurationEntry assembly in (IEnumerable<AssemblyConfigurationEntry>) this.RunspaceConfiguration.Assemblies)
        {
          Exception error = (Exception) null;
          this.AddAssembly(assembly.Name, assembly.FileName, out error);
          if (error != null)
            str = str + "\n" + error.Message;
        }
        this._assemblyCacheInitialized = true;
      }
      else
      {
        foreach (AssemblyConfigurationEntry update in this.RunspaceConfiguration.Assemblies.UpdateList)
        {
          switch (update.Action)
          {
            case UpdateAction.Add:
              Exception error = (Exception) null;
              this.AddAssembly(update.Name, update.FileName, out error);
              if (error != null)
              {
                str = str + "\n" + error.Message;
                continue;
              }
              continue;
            case UpdateAction.Remove:
              this.RemoveAssembly(update.Name);
              continue;
            default:
              continue;
          }
        }
      }
      if (!string.IsNullOrEmpty(str))
        throw new RuntimeException(ResourceManagerCache.FormatResourceString("MiniShellErrors", "UpdateAssemblyErrors", (object) str));
    }

    internal Assembly AddAssembly(string name, string filename, out Exception error)
    {
      Assembly assembly = this.LoadAssembly(name, filename, out error);
      if (assembly == null)
        return (Assembly) null;
      if (this._assemblyCache.ContainsKey(assembly.FullName))
        return assembly;
      this._assemblyCache.Add(assembly.FullName, assembly);
      if (this._assemblyCache.ContainsKey(assembly.GetName().Name))
        return assembly;
      this._assemblyCache.Add(assembly.GetName().Name, assembly);
      return assembly;
    }

    internal void RemoveAssembly(string name)
    {
      if (!this._assemblyCache.ContainsKey(name))
        return;
      Assembly assembly = this._assemblyCache[name];
      if (assembly == null)
        return;
      this._assemblyCache.Remove(name);
      this._assemblyCache.Remove(assembly.GetName().Name);
    }

    private Assembly LoadAssembly(string name, string filename, out Exception error)
    {
      Assembly assembly = (Assembly) null;
      error = (Exception) null;
      string str = (string) null;
      if (!string.IsNullOrEmpty(name))
      {
        str = name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? Path.GetFileNameWithoutExtension(name) : name;
        try
        {
          assembly = Assembly.Load(str);
        }
        catch (FileNotFoundException ex)
        {
          error = (Exception) ex;
        }
        catch (FileLoadException ex)
        {
          error = (Exception) ex;
          return (Assembly) null;
        }
        catch (BadImageFormatException ex)
        {
          error = (Exception) ex;
          return (Assembly) null;
        }
        catch (SecurityException ex)
        {
          error = (Exception) ex;
          return (Assembly) null;
        }
      }
      if (assembly != null)
        return assembly;
      if (!string.IsNullOrEmpty(filename))
      {
        error = (Exception) null;
        try
        {
          return Assembly.LoadFrom(filename);
        }
        catch (FileNotFoundException ex)
        {
          error = (Exception) ex;
        }
        catch (FileLoadException ex)
        {
          error = (Exception) ex;
          return (Assembly) null;
        }
        catch (BadImageFormatException ex)
        {
          error = (Exception) ex;
          return (Assembly) null;
        }
        catch (SecurityException ex)
        {
          error = (Exception) ex;
          return (Assembly) null;
        }
      }
      if (!string.IsNullOrEmpty(str))
      {
        error = (Exception) null;
        try
        {
          return Assembly.LoadWithPartialName(str);
        }
        catch (FileNotFoundException ex)
        {
          error = (Exception) ex;
        }
        catch (FileLoadException ex)
        {
          error = (Exception) ex;
        }
        catch (BadImageFormatException ex)
        {
          error = (Exception) ex;
        }
        catch (SecurityException ex)
        {
          error = (Exception) ex;
        }
      }
      return (Assembly) null;
    }

    internal void ReportEngineStartupError(
      string baseName,
      string resourceId,
      params object[] arguments)
    {
      try
      {
        this.EngineHostInterface?.UI?.WriteErrorLine(ResourceManagerCache.FormatResourceString(baseName, resourceId, arguments));
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        ExecutionContext.tracer.TraceException(ex);
      }
    }

    internal void ReportEngineStartupError(string error)
    {
      try
      {
        this.EngineHostInterface?.UI?.WriteErrorLine(error);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        ExecutionContext.tracer.TraceException(ex);
      }
    }

    internal void ReportEngineStartupError(Exception e)
    {
      try
      {
        this.EngineHostInterface?.UI?.WriteErrorLine(e.Message);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        ExecutionContext.tracer.TraceException(ex);
      }
    }

    internal void ReportEngineStartupError(ErrorRecord errorRecord)
    {
      try
      {
        this.EngineHostInterface?.UI?.WriteErrorLine(errorRecord.ToString());
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        ExecutionContext.tracer.TraceException(ex);
      }
    }

    internal ExecutionContext(
      AutomationEngine engine,
      PSHost hostInterface,
      RunspaceConfiguration runspaceConfiguration)
    {
      this._runspaceConfiguration = runspaceConfiguration;
      this._authorizationManager = runspaceConfiguration.AuthorizationManager;
      this.InitializeCommon(engine, hostInterface);
    }

    internal ExecutionContext(
      AutomationEngine engine,
      PSHost hostInterface,
      InitialSessionState initialSessionState)
    {
      this._initialSessionState = initialSessionState;
      this._authorizationManager = initialSessionState.AuthorizationManager;
      this.InitializeCommon(engine, hostInterface);
    }

    private void InitializeCommon(AutomationEngine engine, PSHost hostInterface)
    {
      this._engine = engine;
      if (!ExecutionContext._assemblyEventHandlerSet)
      {
        lock (ExecutionContext.lockObject)
        {
          if (!ExecutionContext._assemblyEventHandlerSet)
          {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ExecutionContext.PowerShellAssemblyResolveHandler);
            ExecutionContext._assemblyEventHandlerSet = true;
          }
        }
      }
      this._debugger = new Debugger(this);
      this.eventManager = new PSLocalEventManager(this);
      this.transactionManager = new PSTransactionManager();
      this.myHostInterface = hostInterface as InternalHost;
      if (this.myHostInterface == null)
        this.myHostInterface = new InternalHost(hostInterface, this);
      this._assemblyCache = new Dictionary<string, Assembly>();
      this._topLevelSessionState = this._engineSessionState = new SessionStateInternal(this);
      if (this._authorizationManager == null)
        this._authorizationManager = new AuthorizationManager((string) null);
      this._modules = new ModuleIntrinsics(this);
    }

    private static Assembly PowerShellAssemblyResolveHandler(
      object sender,
      ResolveEventArgs args)
    {
      ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
      return executionContextFromTls != null && executionContextFromTls._assemblyCache != null && executionContextFromTls._assemblyCache.ContainsKey(args.Name) ? executionContextFromTls._assemblyCache[args.Name] : (Assembly) null;
    }

    internal class SavedContextData
    {
      private bool StepScript;
      private bool IgnoreScriptDebug;
      private int PSDebug;
      private Pipe ShellFunctionErrorOutputPipe;
      private Pipe TopLevelPipe;
      private int[] ScopeDepths;
      private bool ProcessingScopeDepthErrors;

      public SavedContextData(ExecutionContext context)
      {
        this.ScopeDepths = context.ScopeDepths;
        this.ProcessingScopeDepthErrors = context.ProcessingScopeDepthErrors;
        this.StepScript = context.StepScript;
        this.IgnoreScriptDebug = context.IgnoreScriptDebug;
        this.PSDebug = context.PSDebug;
        this.ShellFunctionErrorOutputPipe = context.ShellFunctionErrorOutputPipe;
        this.TopLevelPipe = context.topLevelPipe;
      }

      public void RestoreContextData(ExecutionContext context)
      {
        context.ScopeDepths = this.ScopeDepths;
        context.ProcessingScopeDepthErrors = this.ProcessingScopeDepthErrors;
        context.StepScript = this.StepScript;
        context.IgnoreScriptDebug = this.IgnoreScriptDebug;
        context.PSDebug = this.PSDebug;
        context.ShellFunctionErrorOutputPipe = this.ShellFunctionErrorOutputPipe;
        context.topLevelPipe = this.TopLevelPipe;
      }
    }

    internal enum FeaturesThatNeedDepthHandling
    {
      ScriptScope,
      CommandsInPipeline,
    }
  }
}
