// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SessionStateInternal
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation.Provider;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using System.Threading;

namespace System.Management.Automation
{
  internal sealed class SessionStateInternal
  {
    private const string resTableName = "RunspaceInit";
    private const string startingDefaultStackName = "default";
    [TraceSource("SessionState", "SessionState Class")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("SessionState", "SessionState Class");
    private LocationGlobber globberPrivate;
    private ExecutionContext _context;
    private SessionState _publicSessionState;
    private ProviderIntrinsics _invokeProvider;
    private PSModuleInfo _module;
    private Dictionary<string, PSModuleInfo> _moduleTable = new Dictionary<string, PSModuleInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private List<string> _scripts = new List<string>((IEnumerable<string>) new string[1]
    {
      "*"
    });
    private List<string> _applications = new List<string>((IEnumerable<string>) new string[1]
    {
      "*"
    });
    private List<CmdletInfo> _exportedCmdlets = new List<CmdletInfo>();
    private Dictionary<string, List<CmdletInfo>> cachedCmdletInfo = new Dictionary<string, List<CmdletInfo>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private List<AliasInfo> _exportedAliases = new List<AliasInfo>();
    private PSDriveInfo currentDrive;
    private static char[] _charactersInvalidInDriveName = new char[5]
    {
      ':',
      '/',
      '\\',
      '.',
      '~'
    };
    private List<FunctionInfo> _exportedFunctions = new List<FunctionInfo>();
    private bool _useExportList;
    private ScopedItemLookupPath _pwdVariablePath;
    private Dictionary<string, Stack<PathInfo>> workingLocationStack;
    private string defaultStackName = "default";
    private Dictionary<string, List<ProviderInfo>> _providers = new Dictionary<string, List<ProviderInfo>>(100, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private Dictionary<ProviderInfo, PSDriveInfo> _providersCurrentWorkingDrive = new Dictionary<ProviderInfo, PSDriveInfo>();
    private bool _providersInitialized;
    internal SessionStateScope currentScope;
    private ActivationRecord currentActiviationRecord;
    private SessionStateScope _globalScope;
    private SessionStateScope _moduleScope;
    private List<PSVariable> _exportedVariables = new List<PSVariable>();

    internal static ISecurityDescriptorCmdletProvider GetPermissionProviderInstance(
      CmdletProvider providerInstance)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (providerInstance == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (providerInstance));
        return providerInstance is ISecurityDescriptorCmdletProvider descriptorCmdletProvider ? descriptorCmdletProvider : throw SessionStateInternal.tracer.NewNotSupportedException("ProviderBaseSecurity", "ISecurityDescriptorCmdletProvider_NotSupported");
      }
    }

    internal Collection<PSObject> GetSecurityDescriptor(
      string path,
      AccessControlSections sections)
    {
      using (SessionStateInternal.tracer.TraceMethod(path, new object[0]))
      {
        if (path == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
        CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
        this.GetSecurityDescriptor(path, sections, context);
        context.ThrowFirstErrorOrDoNothing();
        return context.GetAccumulatedObjects() ?? new Collection<PSObject>();
      }
    }

    internal void GetSecurityDescriptor(
      string path,
      AccessControlSections sections,
      CmdletProviderContext context)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (path == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        foreach (string path1 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out provider, out providerInstance))
          this.GetSecurityDescriptor(providerInstance, path1, sections, context);
      }
    }

    private void GetSecurityDescriptor(
      CmdletProvider providerInstance,
      string path,
      AccessControlSections sections,
      CmdletProviderContext context)
    {
      using (SessionStateInternal.tracer.TraceMethod(path, new object[0]))
      {
        SessionStateInternal.GetPermissionProviderInstance(providerInstance);
        try
        {
          providerInstance.GetSecurityDescriptor(path, sections, context);
        }
        catch (PipelineStoppedException ex)
        {
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          SessionStateInternal.tracer.TraceException(ex);
          throw this.NewProviderInvocationException("GetSecurityDescriptorProviderException", providerInstance.ProviderInfo, path, ex);
        }
      }
    }

    internal Collection<PSObject> SetSecurityDescriptor(
      string path,
      ObjectSecurity securityDescriptor)
    {
      using (SessionStateInternal.tracer.TraceMethod(path, new object[0]))
      {
        if (path == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
        if (securityDescriptor == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (securityDescriptor));
        CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
        this.SetSecurityDescriptor(path, securityDescriptor, context);
        context.ThrowFirstErrorOrDoNothing();
        return context.GetAccumulatedObjects() ?? new Collection<PSObject>();
      }
    }

    internal void SetSecurityDescriptor(
      string path,
      ObjectSecurity securityDescriptor,
      CmdletProviderContext context)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (path == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
        if (securityDescriptor == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (securityDescriptor));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        foreach (string path1 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out provider, out providerInstance))
          this.SetSecurityDescriptor(providerInstance, path1, securityDescriptor, context);
      }
    }

    private void SetSecurityDescriptor(
      CmdletProvider providerInstance,
      string path,
      ObjectSecurity securityDescriptor,
      CmdletProviderContext context)
    {
      using (SessionStateInternal.tracer.TraceMethod(path, new object[0]))
      {
        SessionStateInternal.GetPermissionProviderInstance(providerInstance);
        try
        {
          providerInstance.SetSecurityDescriptor(path, securityDescriptor, context);
        }
        catch (PipelineStoppedException ex)
        {
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          throw;
        }
        catch (PrivilegeNotHeldException ex)
        {
          context.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
        catch (UnauthorizedAccessException ex)
        {
          context.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
        catch (NotSupportedException ex)
        {
          context.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.InvalidOperation, (object) path));
        }
        catch (SystemException ex)
        {
          CommandProcessorBase.CheckForSevereException((Exception) ex);
          context.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.InvalidOperation, (object) path));
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          SessionStateInternal.tracer.TraceException(ex);
          throw this.NewProviderInvocationException("SetSecurityDescriptorProviderException", providerInstance.ProviderInfo, path, ex);
        }
      }
    }

    internal ObjectSecurity NewSecurityDescriptorFromPath(
      string path,
      AccessControlSections sections)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (path == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, out provider, out providerInstance);
        if (pathsFromMonadPath.Count == 1)
          return this.NewSecurityDescriptorFromPath(providerInstance, pathsFromMonadPath[0], sections);
        throw SessionStateInternal.tracer.NewArgumentException(nameof (path));
      }
    }

    private ObjectSecurity NewSecurityDescriptorFromPath(
      CmdletProvider providerInstance,
      string path,
      AccessControlSections sections)
    {
      using (SessionStateInternal.tracer.TraceMethod(path, new object[0]))
      {
        ISecurityDescriptorCmdletProvider providerInstance1 = SessionStateInternal.GetPermissionProviderInstance(providerInstance);
        ObjectSecurity objectSecurity;
        try
        {
          objectSecurity = providerInstance1.NewSecurityDescriptorFromPath(path, sections);
        }
        catch (PipelineStoppedException ex)
        {
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          SessionStateInternal.tracer.TraceException(ex);
          throw this.NewProviderInvocationException("NewSecurityDescriptorProviderException", providerInstance.ProviderInfo, path, ex);
        }
        return objectSecurity;
      }
    }

    internal ObjectSecurity NewSecurityDescriptorOfType(
      string providerId,
      string type,
      AccessControlSections sections)
    {
      using (SessionStateInternal.tracer.TraceMethod())
        return this.NewSecurityDescriptorOfType(this.GetProviderInstance(providerId), type, sections);
    }

    internal ObjectSecurity NewSecurityDescriptorOfType(
      CmdletProvider providerInstance,
      string type,
      AccessControlSections sections)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (type == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (type));
        ISecurityDescriptorCmdletProvider descriptorCmdletProvider = providerInstance != null ? SessionStateInternal.GetPermissionProviderInstance(providerInstance) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (providerInstance));
        ObjectSecurity objectSecurity;
        try
        {
          objectSecurity = descriptorCmdletProvider.NewSecurityDescriptorOfType(type, sections);
        }
        catch (PipelineStoppedException ex)
        {
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          SessionStateInternal.tracer.TraceException(ex);
          throw this.NewProviderInvocationException("NewSecurityDescriptorProviderException", providerInstance.ProviderInfo, type, ex);
        }
        return objectSecurity;
      }
    }

    internal SessionStateInternal(ExecutionContext context)
      : this((SessionStateInternal) null, false, context)
    {
    }

    internal SessionStateInternal(
      SessionStateInternal parent,
      bool linkToGlobal,
      ExecutionContext context)
    {
      this._context = context != null ? context : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (context));
      this.workingLocationStack = new Dictionary<string, Stack<PathInfo>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      this._globalScope = new SessionStateScope((SessionStateScope) null);
      this._moduleScope = this._globalScope;
      this.currentScope = this._globalScope;
      PSVariable psVariable = new PSVariable("Error", (object) new ArrayList(), ScopedItemOptions.Constant);
      this._globalScope.SetVariable(psVariable.Name, (object) psVariable, false, false, this);
      this._globalScope.ScriptScope = this._globalScope;
      if (parent != null)
      {
        this._globalScope.Parent = parent.GlobalScope;
        this.CopyProviders(parent);
        this.CurrentDrive = parent.CurrentDrive;
        if (linkToGlobal)
          this._globalScope = parent.GlobalScope;
      }
      this.currentActiviationRecord = new ActivationRecord();
    }

    internal LocationGlobber Globber
    {
      get
      {
        if (this.globberPrivate == null)
          this.globberPrivate = this._context.LocationGlobber;
        return this.globberPrivate;
      }
    }

    internal ExecutionContext ExecutionContext => this._context;

    internal SessionState PublicSessionState
    {
      get
      {
        if (this._publicSessionState == null)
          this._publicSessionState = new SessionState(this);
        return this._publicSessionState;
      }
      set => this._publicSessionState = value;
    }

    internal ProviderIntrinsics InvokeProvider
    {
      get
      {
        if (this._invokeProvider == null)
          this._invokeProvider = new ProviderIntrinsics(this);
        return this._invokeProvider;
      }
    }

    internal PSModuleInfo Module
    {
      get => this._module;
      set => this._module = value;
    }

    internal Dictionary<string, PSModuleInfo> ModuleTable => this._moduleTable;

    internal PSLanguageMode LanguageMode
    {
      get => this._context.LanguageMode;
      set => this._context.LanguageMode = value;
    }

    internal bool UseFullLanguageModeInDebugger => this._context.UseFullLanguageModeInDebugger;

    public List<string> Scripts => this._scripts;

    internal SessionStateEntryVisibility CheckScriptVisibility(
      string scriptPath)
    {
      return this.checkPathVisibility(this._scripts, scriptPath);
    }

    public List<string> Applications => this._applications;

    internal List<CmdletInfo> ExportedCmdlets => this._exportedCmdlets;

    internal void AddSessionStateEntry(SessionStateCmdletEntry entry) => this.ExecutionContext.CommandDiscovery.AddSessionStateCmdletEntryToCache(entry);

    internal void AddSessionStateEntry(SessionStateApplicationEntry entry) => this.Applications.Add(entry.Path);

    internal void AddSessionStateEntry(SessionStateScriptEntry entry) => this.Scripts.Add(entry.Path);

    internal void InitializeFixedVariables()
    {
      this.SetVariableAtScope(new PSVariable("Host", (object) this._context.EngineHostInterface, ScopedItemOptions.Constant | ScopedItemOptions.AllScope, ResourceManagerCache.GetResourceString("RunspaceInit", "PSHostDescription")), "global", true, CommandOrigin.Internal);
      this.SetVariableAtScope(new PSVariable("HOME", (object) (Environment.GetEnvironmentVariable("HomeDrive") + Environment.GetEnvironmentVariable("HomePath")), ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope, ResourceManagerCache.GetResourceString("RunspaceInit", "HOMEDescription")), "global", true, CommandOrigin.Internal);
      this.SetVariableAtScope(new PSVariable("ExecutionContext", (object) this._context.EngineIntrinsics, ScopedItemOptions.Constant | ScopedItemOptions.AllScope, ResourceManagerCache.GetResourceString("RunspaceInit", "ExecutionContextDescription")), "global", true, CommandOrigin.Internal);
      this.SetVariableAtScope(new PSVariable("PSVersionTable", (object) PSVersionInfo.GetPSVersionTable(), ScopedItemOptions.Constant | ScopedItemOptions.AllScope, ResourceManagerCache.GetResourceString("RunspaceInit", "PSVersionTableDescription")), "global", true, CommandOrigin.Internal);
      this.SetVariableAtScope(new PSVariable("PID", (object) Process.GetCurrentProcess().Id, ScopedItemOptions.Constant | ScopedItemOptions.AllScope, ResourceManagerCache.GetResourceString("RunspaceInit", "PIDDescription")), "global", true, CommandOrigin.Internal);
      this.SetVariableAtScope((PSVariable) new PSCultureVariable(), "global", true, CommandOrigin.Internal);
      this.SetVariableAtScope((PSVariable) new PSUICultureVariable(), "global", true, CommandOrigin.Internal);
      string shellId = this._context.ShellID;
      this.SetVariableAtScope(new PSVariable("ShellId", (object) shellId, ScopedItemOptions.Constant | ScopedItemOptions.AllScope, ResourceManagerCache.GetResourceString("RunspaceInit", "MshShellIdDescription")), "global", true, CommandOrigin.Internal);
      string str = "";
      try
      {
        str = Utils.GetApplicationBase(shellId);
      }
      catch (SecurityException ex)
      {
        SessionStateInternal.tracer.TraceException((Exception) ex);
      }
      this.SetVariableAtScope(new PSVariable("PSHOME", (object) str, ScopedItemOptions.Constant | ScopedItemOptions.AllScope, ResourceManagerCache.GetResourceString("RunspaceInit", "PSHOMEDescription")), "global", true, CommandOrigin.Internal);
      this.SetConsoleVariable();
    }

    internal void SetConsoleVariable()
    {
      string str = string.Empty;
      if (this._context.RunspaceConfiguration is RunspaceConfigForSingleShell runspaceConfiguration && runspaceConfiguration.ConsoleInfo != null && !string.IsNullOrEmpty(runspaceConfiguration.ConsoleInfo.Filename))
        str = runspaceConfiguration.ConsoleInfo.Filename;
      this.SetVariableAtScope(new PSVariable("ConsoleFileName", (object) str, ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope, ResourceManagerCache.GetResourceString("RunspaceInit", "ConsoleDescription")), "global", true, CommandOrigin.Internal);
    }

    internal void AddBuiltInEntries(bool addSetStrictMode)
    {
      this.AddBuiltInVariables();
      this.AddBuiltInFunctions();
      this.AddBuiltInAliases();
      if (!addSetStrictMode)
        return;
      this.AddSessionStateEntry(new SessionStateFunctionEntry("Set-StrictMode", ""));
    }

    internal void AddBuiltInVariables()
    {
      foreach (SessionStateVariableEntry builtInVariable in InitialSessionState.BuiltInVariables)
        this.AddSessionStateEntry(builtInVariable);
    }

    internal void AddBuiltInFunctions()
    {
      foreach (SessionStateFunctionEntry builtInFunction in InitialSessionState.BuiltInFunctions)
        this.AddSessionStateEntry(builtInFunction);
    }

    internal void AddBuiltInAliases()
    {
      foreach (SessionStateAliasEntry builtInAlias in InitialSessionState.BuiltInAliases)
        this.AddSessionStateEntry(builtInAlias);
    }

    internal SessionStateEntryVisibility CheckApplicationVisibility(
      string applicationPath)
    {
      return this.checkPathVisibility(this._applications, applicationPath);
    }

    private SessionStateEntryVisibility checkPathVisibility(
      List<string> list,
      string path)
    {
      if (list == null || list.Count == 0 || string.IsNullOrEmpty(path))
        return SessionStateEntryVisibility.Private;
      if (list.Contains("*"))
        return SessionStateEntryVisibility.Public;
      foreach (string a in list)
      {
        if (string.Equals(a, path, StringComparison.OrdinalIgnoreCase))
          return SessionStateEntryVisibility.Public;
      }
      return SessionStateEntryVisibility.Private;
    }

    internal Dictionary<string, List<CmdletInfo>> CmdletCache => this.cachedCmdletInfo;

    internal void RunspaceClosingNotification()
    {
      if (this == this._context.TopLevelSessionState || this.Providers.Count <= 0)
        return;
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      Collection<string> collection = new Collection<string>();
      foreach (string key in this.Providers.Keys)
        collection.Add(key);
      foreach (string providerName in collection)
        this.RemoveProvider(providerName, true, context);
    }

    internal ProviderInvocationException NewProviderInvocationException(
      string resourceId,
      ProviderInfo provider,
      string path,
      Exception e)
    {
      return this.NewProviderInvocationException(resourceId, provider, path, e, true);
    }

    internal ProviderInvocationException NewProviderInvocationException(
      string resourceId,
      ProviderInfo provider,
      string path,
      Exception e,
      bool useInnerExceptionErrorMessage)
    {
      if (e is ProviderInvocationException invocationException)
      {
        invocationException._providerInfo = provider;
        return invocationException;
      }
      ProviderInvocationException invocationException1 = new ProviderInvocationException(resourceId, provider, path, e, useInnerExceptionErrorMessage);
      MshLog.LogProviderHealthEvent(this._context, provider.Name, (Exception) invocationException1, Severity.Warning);
      return invocationException1;
    }

    internal void AddSessionStateEntry(SessionStateAliasEntry entry)
    {
      AliasInfo alias = new AliasInfo(entry.Name, entry.Definition, this.ExecutionContext, entry.Options);
      alias.Visibility = entry.Visibility;
      alias.SetModule(entry.Module);
      if (!string.IsNullOrEmpty(entry.Description))
        alias.Description = entry.Description;
      this.SetAliasItemAtScope(alias, "global", true, CommandOrigin.Internal);
    }

    internal IDictionary<string, AliasInfo> GetAliasTable()
    {
      Dictionary<string, AliasInfo> dictionary = new Dictionary<string, AliasInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (SessionStateScope sessionStateScope in (IEnumerable<SessionStateScope>) new SessionStateScopeEnumerator(this, this.currentScope))
      {
        foreach (AliasInfo aliasInfo in sessionStateScope.AliasTable)
        {
          if (!dictionary.ContainsKey(aliasInfo.Name) && ((aliasInfo.Options & ScopedItemOptions.Private) == ScopedItemOptions.None || sessionStateScope == this.currentScope))
            dictionary.Add(aliasInfo.Name, aliasInfo);
        }
      }
      return (IDictionary<string, AliasInfo>) dictionary;
    }

    internal IDictionary<string, AliasInfo> GetAliasTableAtScope(string scopeID)
    {
      Dictionary<string, AliasInfo> dictionary = new Dictionary<string, AliasInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      SessionStateScope scopeById = this.GetScopeByID(scopeID);
      foreach (AliasInfo aliasInfo in scopeById.AliasTable)
      {
        if ((aliasInfo.Options & ScopedItemOptions.Private) == ScopedItemOptions.None || scopeById == this.currentScope)
          dictionary.Add(aliasInfo.Name, aliasInfo);
      }
      return (IDictionary<string, AliasInfo>) dictionary;
    }

    internal List<AliasInfo> ExportedAliases => this._exportedAliases;

    internal AliasInfo GetAlias(string aliasName, CommandOrigin origin)
    {
      AliasInfo aliasInfo = (AliasInfo) null;
      if (string.IsNullOrEmpty(aliasName))
        return aliasInfo;
      foreach (SessionStateScope sessionStateScope in (IEnumerable<SessionStateScope>) new SessionStateScopeEnumerator(this, this.currentScope))
      {
        aliasInfo = sessionStateScope.GetAlias(aliasName);
        if (aliasInfo != null)
        {
          SessionState.ThrowIfNotVisible(origin, (object) aliasInfo);
          if ((aliasInfo.Options & ScopedItemOptions.Private) != ScopedItemOptions.None)
          {
            if (sessionStateScope != this.currentScope)
              aliasInfo = (AliasInfo) null;
            else
              break;
          }
          else
            break;
        }
      }
      return aliasInfo;
    }

    internal AliasInfo GetAlias(string aliasName) => this.GetAlias(aliasName, CommandOrigin.Internal);

    internal AliasInfo GetAliasAtScope(string aliasName, string scopeID)
    {
      AliasInfo aliasInfo1 = (AliasInfo) null;
      if (string.IsNullOrEmpty(aliasName))
        return aliasInfo1;
      SessionStateScope scopeById = this.GetScopeByID(scopeID);
      AliasInfo aliasInfo2 = scopeById.GetAlias(aliasName);
      if (aliasInfo2 != null && (aliasInfo2.Options & ScopedItemOptions.Private) != ScopedItemOptions.None && scopeById != this.currentScope)
        aliasInfo2 = (AliasInfo) null;
      return aliasInfo2;
    }

    internal AliasInfo SetAliasValue(
      string aliasName,
      string value,
      bool force,
      CommandOrigin origin)
    {
      if (string.IsNullOrEmpty(aliasName))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (aliasName));
      if (string.IsNullOrEmpty(value))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (value));
      return this.currentScope.SetAliasValue(aliasName, value, this.ExecutionContext, force, origin);
    }

    internal AliasInfo SetAliasValue(string aliasName, string value, bool force) => this.SetAliasValue(aliasName, value, force, CommandOrigin.Internal);

    internal AliasInfo SetAliasValue(
      string aliasName,
      string value,
      ScopedItemOptions options,
      bool force,
      CommandOrigin origin)
    {
      using (SessionStateInternal.tracer.TraceMethod(aliasName, new object[0]))
      {
        if (string.IsNullOrEmpty(aliasName))
          throw SessionStateInternal.tracer.NewArgumentException(nameof (aliasName));
        if (string.IsNullOrEmpty(value))
          throw SessionStateInternal.tracer.NewArgumentException(nameof (value));
        return this.currentScope.SetAliasValue(aliasName, value, options, this.ExecutionContext, force, origin);
      }
    }

    internal AliasInfo SetAliasValue(
      string aliasName,
      string value,
      ScopedItemOptions options,
      bool force)
    {
      return this.SetAliasValue(aliasName, value, options, force, CommandOrigin.Internal);
    }

    internal AliasInfo SetAliasItem(AliasInfo alias, bool force, CommandOrigin origin)
    {
      if (alias == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (alias));
      return this.currentScope.SetAliasItem(alias, force, origin);
    }

    internal AliasInfo SetAliasItemAtScope(
      AliasInfo alias,
      string scopeID,
      bool force,
      CommandOrigin origin)
    {
      if (alias == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (alias));
      if (string.Equals(scopeID, "PRIVATE", StringComparison.OrdinalIgnoreCase))
        alias.Options |= ScopedItemOptions.Private;
      return this.GetScopeByID(scopeID).SetAliasItem(alias, force, origin);
    }

    internal AliasInfo SetAliasItemAtScope(AliasInfo alias, string scopeID, bool force) => this.SetAliasItemAtScope(alias, scopeID, force, CommandOrigin.Internal);

    internal void RemoveAlias(string aliasName, bool force)
    {
      using (SessionStateInternal.tracer.TraceMethod(aliasName, new object[0]))
      {
        if (string.IsNullOrEmpty(aliasName))
          throw SessionStateInternal.tracer.NewArgumentException(nameof (aliasName));
        foreach (SessionStateScope sessionStateScope in (IEnumerable<SessionStateScope>) new SessionStateScopeEnumerator(this, this.currentScope))
        {
          AliasInfo alias = sessionStateScope.GetAlias(aliasName);
          if (alias != null)
          {
            if ((alias.Options & ScopedItemOptions.Private) == ScopedItemOptions.None || sessionStateScope == this.currentScope)
            {
              sessionStateScope.RemoveAlias(aliasName, force);
              break;
            }
          }
        }
      }
    }

    internal bool ItemExists(string path, bool force, bool literalPath)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      bool flag = this.ItemExists(path, context);
      context.ThrowFirstErrorOrDoNothing();
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool ItemExists(string path, CmdletProviderContext context)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      bool flag = false;
      try
      {
        foreach (string path1 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context, out provider, out providerInstance))
        {
          flag = this.ItemExists(providerInstance, path1, context);
          if (flag)
            break;
        }
      }
      catch (ItemNotFoundException ex)
      {
        flag = false;
      }
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool ItemExists(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      ItemCmdletProvider providerInstance1 = SessionStateInternal.GetItemProviderInstance(providerInstance);
      try
      {
        return providerInstance1.ItemExists(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("ItemExistsProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal object ItemExistsDynamicParameters(string path, CmdletProviderContext context)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.ItemExistsDynamicParameters(providerInstance, pathsFromMonadPath[0], context1) : (object) null;
    }

    private object ItemExistsDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        return providerInstance1.ItemExistsDynamicParameters(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("ItemExistsDynamicParametersProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal bool IsValidPath(string path)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      bool flag = this.IsValidPath(path, context);
      context.ThrowFirstErrorOrDoNothing();
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool IsValidPath(string path, CmdletProviderContext context)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      ProviderInfo provider = (ProviderInfo) null;
      PSDriveInfo drive = (PSDriveInfo) null;
      string providerPath = this.Globber.GetProviderPath(path, context, out provider, out drive);
      bool flag = this.IsValidPath((CmdletProvider) this.GetItemProviderInstance(provider), providerPath, context);
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    private bool IsValidPath(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      ItemCmdletProvider providerInstance1 = SessionStateInternal.GetItemProviderInstance(providerInstance);
      try
      {
        return providerInstance1.IsValidPath(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("IsValidPathProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal bool IsItemContainer(string path)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      bool flag = this.IsItemContainer(path, context);
      context.ThrowFirstErrorOrDoNothing();
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool IsItemContainer(string path, CmdletProviderContext context)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      bool flag = false;
      try
      {
        foreach (string path1 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context, out provider, out providerInstance))
        {
          flag = this.IsItemContainer(providerInstance, path1, context);
          if (!flag)
            break;
        }
      }
      catch (ItemNotFoundException ex)
      {
        flag = false;
      }
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    private bool IsItemContainer(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      bool flag;
      try
      {
        NavigationCmdletProvider providerInstance1 = SessionStateInternal.GetNavigationProviderInstance(providerInstance);
        try
        {
          flag = providerInstance1.IsItemContainer(path, context);
        }
        catch (PipelineStoppedException ex)
        {
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          SessionStateInternal.tracer.TraceException(ex);
          throw this.NewProviderInvocationException("IsItemContainerProviderException", providerInstance1.ProviderInfo, path, ex);
        }
      }
      catch (NotSupportedException ex1)
      {
        try
        {
          SessionStateInternal.GetContainerProviderInstance(providerInstance);
          flag = path.Length == 0;
        }
        catch (NotSupportedException ex2)
        {
          flag = false;
        }
      }
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal void RemoveItem(string[] paths, bool recurse, bool force, bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.RemoveItem(paths, recurse, context);
      context.ThrowFirstErrorOrDoNothing();
    }

    internal void RemoveItem(string[] paths, bool recurse, CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        foreach (string path2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider, out providerInstance))
          this.RemoveItem(providerInstance, path2, recurse, context);
      }
    }

    internal void RemoveItem(
      string providerId,
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      this.RemoveItem(this.GetProviderInstance(providerId), path, recurse, context);
    }

    internal void RemoveItem(
      CmdletProvider providerInstance,
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        providerInstance1.RemoveItem(path, recurse, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("RemoveItemProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal object RemoveItemDynamicParameters(
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.RemoveItemDynamicParameters(providerInstance, pathsFromMonadPath[0], recurse, context1) : (object) null;
    }

    private object RemoveItemDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        return providerInstance1.RemoveItemDynamicParameters(path, recurse, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("RemoveItemProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> GetChildItems(
      string[] paths,
      bool recurse,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      foreach (string path in paths)
      {
        if (path == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        this.GetChildItems(path, recurse, context);
      }
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void GetChildItems(string path, bool recurse, CmdletProviderContext context)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      if (context == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (context));
      ProviderInfo provider = (ProviderInfo) null;
      if (LocationGlobber.ShouldPerformGlobbing(path, context))
      {
        Collection<string> include = context.Include;
        Collection<string> exclude = context.Exclude;
        if (recurse)
          context.SetFilters(new Collection<string>(), new Collection<string>(), context.Filter);
        CmdletProvider providerInstance = (CmdletProvider) null;
        Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out provider, out providerInstance);
        context.SetFilters(include, exclude, context.Filter);
        if (include != null && include.Count > 0 || exclude != null && exclude.Count > 0)
        {
          foreach (string path1 in pathsFromMonadPath)
          {
            if (context.Stopping)
              break;
            this.DoManualGetChildItems(providerInstance, path1, recurse, context);
          }
        }
        else
        {
          bool flag = !LocationGlobber.StringContainsGlobCharacters(path);
          foreach (string path1 in pathsFromMonadPath)
          {
            if (context.Stopping)
              break;
            if ((flag || recurse) && this.IsItemContainer(providerInstance, path1, context))
              this.GetChildItems(providerInstance, path1, recurse, context);
            else
              this.GetItemPrivate(providerInstance, path1, context);
          }
        }
      }
      else
      {
        PSDriveInfo drive = (PSDriveInfo) null;
        path = this.Globber.GetProviderPath(path, context, out provider, out drive);
        if (drive != (PSDriveInfo) null)
          context.Drive = drive;
        ContainerCmdletProvider providerInstance = this.GetContainerProviderInstance(provider);
        if (path != null && this.ItemExists((CmdletProvider) providerInstance, path, context))
        {
          if (this.IsItemContainer((CmdletProvider) providerInstance, path, context))
            this.GetChildItems((CmdletProvider) providerInstance, path, recurse, context);
          else
            this.GetItemPrivate((CmdletProvider) providerInstance, path, context);
        }
        else
        {
          ItemNotFoundException notFoundException = new ItemNotFoundException(path, "PathNotFound");
          SessionStateInternal.tracer.TraceException((Exception) notFoundException);
          throw notFoundException;
        }
      }
    }

    private void GetChildItems(
      CmdletProvider providerInstance,
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        providerInstance1.GetChildItems(path, recurse, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetChildrenProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    private void DoManualGetChildItems(
      CmdletProvider providerInstance,
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      Collection<WildcardPattern> wildcardsFromStrings1 = SessionStateUtilities.CreateWildcardsFromStrings(context.Include, WildcardOptions.IgnoreCase);
      Collection<WildcardPattern> wildcardsFromStrings2 = SessionStateUtilities.CreateWildcardsFromStrings(context.Exclude, WildcardOptions.IgnoreCase);
      if (this.IsItemContainer(providerInstance, path, context))
      {
        CmdletProviderContext context1 = new CmdletProviderContext(context);
        Collection<PSObject> collection = (Collection<PSObject>) null;
        try
        {
          this.GetChildNames(providerInstance, path, recurse ? ReturnContainers.ReturnAllContainers : ReturnContainers.ReturnMatchingContainers, context1);
          context1.WriteErrorsToContext(context);
          collection = context1.GetAccumulatedObjects();
        }
        finally
        {
          context1.RemoveStopReferral();
        }
        for (int index = 0; index < collection.Count && !context.Stopping; ++index)
        {
          if (collection[index].BaseObject is string baseObject)
          {
            string path1 = this.MakePath(providerInstance, path, baseObject, context);
            if (path1 != null)
            {
              if (SessionStateUtilities.MatchesAnyWildcardPattern(baseObject, (IEnumerable<WildcardPattern>) wildcardsFromStrings1, true) && !SessionStateUtilities.MatchesAnyWildcardPattern(baseObject, (IEnumerable<WildcardPattern>) wildcardsFromStrings2, false))
                this.GetItemPrivate(providerInstance, path1, context);
              if (this.IsItemContainer(providerInstance, path1, context) && recurse)
              {
                if (context.Stopping)
                  break;
                this.DoManualGetChildItems(providerInstance, path1, recurse, context);
              }
            }
          }
        }
      }
      else
      {
        string text;
        try
        {
          text = this.GetChildName(providerInstance, path, context);
        }
        catch (PSNotSupportedException ex)
        {
          text = path;
        }
        if (!SessionStateUtilities.MatchesAnyWildcardPattern(text, (IEnumerable<WildcardPattern>) wildcardsFromStrings1, true) || SessionStateUtilities.MatchesAnyWildcardPattern(text, (IEnumerable<WildcardPattern>) wildcardsFromStrings2, false))
          return;
        this.GetItemPrivate(providerInstance, path, context);
      }
    }

    internal object GetChildItemsDynamicParameters(
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      this.Globber.GetProviderPath(path, out provider);
      if (!this.HasGetChildItemDynamicParameters(provider))
        return (object) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.GetChildItemsDynamicParameters(providerInstance, pathsFromMonadPath[0], recurse, context1) : (object) null;
    }

    private bool HasGetChildItemDynamicParameters(ProviderInfo providerInfo)
    {
      Type type = providerInfo.ImplementingType;
      MethodInfo method;
      do
      {
        method = type.GetMethod("GetChildItemsDynamicParameters", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
        type = type.BaseType;
      }
      while (method == null && type != null && type != typeof (ContainerCmdletProvider));
      return method != null;
    }

    private object GetChildItemsDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        return providerInstance1.GetChildItemsDynamicParameters(path, recurse, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetChildrenDynamicParametersProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal Collection<string> GetChildNames(
      string[] paths,
      ReturnContainers returnContainers,
      bool recurse,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      foreach (string path in paths)
      {
        if (path == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        this.GetChildNames(path, returnContainers, recurse, context);
      }
      context.ThrowFirstErrorOrDoNothing();
      Collection<PSObject> accumulatedObjects = context.GetAccumulatedObjects();
      Collection<string> collection = new Collection<string>();
      foreach (PSObject psObject in accumulatedObjects)
        collection.Add(psObject.BaseObject as string);
      return collection;
    }

    internal void GetChildNames(
      string path,
      ReturnContainers returnContainers,
      bool recurse,
      CmdletProviderContext context)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      Collection<WildcardPattern> wildcardsFromStrings1 = SessionStateUtilities.CreateWildcardsFromStrings(context.Include, WildcardOptions.IgnoreCase);
      Collection<WildcardPattern> wildcardsFromStrings2 = SessionStateUtilities.CreateWildcardsFromStrings(context.Exclude, WildcardOptions.IgnoreCase);
      if (LocationGlobber.ShouldPerformGlobbing(path, context))
      {
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        CmdletProviderContext context1 = new CmdletProviderContext(context);
        context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
        Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context1, out provider, out providerInstance);
        if (context1.Drive != (PSDriveInfo) null)
          context.Drive = context1.Drive;
        bool flag1 = LocationGlobber.StringContainsGlobCharacters(path);
        foreach (string str in pathsFromMonadPath)
        {
          if (context.Stopping)
            break;
          if ((!flag1 || recurse) && this.IsItemContainer(providerInstance, str, context))
            this.DoGetChildNamesManually(providerInstance, str, string.Empty, returnContainers, wildcardsFromStrings1, wildcardsFromStrings2, context, recurse);
          else if (providerInstance is NavigationCmdletProvider)
          {
            string childName = this.GetChildName(providerInstance, str, context);
            bool flag2 = SessionStateUtilities.MatchesAnyWildcardPattern(childName, (IEnumerable<WildcardPattern>) wildcardsFromStrings1, true);
            bool flag3 = SessionStateUtilities.MatchesAnyWildcardPattern(childName, (IEnumerable<WildcardPattern>) wildcardsFromStrings2, false);
            if (flag2 && !flag3)
              context.WriteObject((object) childName);
          }
          else
            context.WriteObject((object) str);
        }
      }
      else
      {
        ProviderInfo provider = (ProviderInfo) null;
        PSDriveInfo drive = (PSDriveInfo) null;
        string providerPath = this.Globber.GetProviderPath(path, context, out provider, out drive);
        ContainerCmdletProvider providerInstance = this.GetContainerProviderInstance(provider);
        if (drive != (PSDriveInfo) null)
          context.Drive = drive;
        if (!providerInstance.ItemExists(providerPath, context))
        {
          ItemNotFoundException notFoundException = new ItemNotFoundException(providerPath, "PathNotFound");
          SessionStateInternal.tracer.TraceException((Exception) notFoundException);
          throw notFoundException;
        }
        if (recurse)
          this.DoGetChildNamesManually((CmdletProvider) providerInstance, providerPath, string.Empty, returnContainers, wildcardsFromStrings1, wildcardsFromStrings2, context, recurse);
        else
          this.GetChildNames((CmdletProvider) providerInstance, providerPath, returnContainers, context);
      }
    }

    private void DoGetChildNamesManually(
      CmdletProvider providerInstance,
      string providerPath,
      string relativePath,
      ReturnContainers returnContainers,
      Collection<WildcardPattern> includeMatcher,
      Collection<WildcardPattern> excludeMatcher,
      CmdletProviderContext context,
      bool recurse)
    {
      string path1 = this.MakePath(providerInstance, providerPath, relativePath, context);
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      try
      {
        this.GetChildNames(providerInstance, path1, ReturnContainers.ReturnMatchingContainers, context1);
        foreach (PSObject accumulatedObject in context1.GetAccumulatedObjects())
        {
          if (context.Stopping)
            return;
          if (accumulatedObject.BaseObject is string baseObject && SessionStateUtilities.MatchesAnyWildcardPattern(baseObject, (IEnumerable<WildcardPattern>) includeMatcher, true) && !SessionStateUtilities.MatchesAnyWildcardPattern(baseObject, (IEnumerable<WildcardPattern>) excludeMatcher, false))
          {
            string str = this.MakePath(providerInstance, relativePath, baseObject, context);
            context.WriteObject((object) str);
          }
        }
        if (!recurse)
          return;
        this.GetChildNames(providerInstance, path1, ReturnContainers.ReturnAllContainers, context1);
        foreach (PSObject accumulatedObject in context1.GetAccumulatedObjects())
        {
          if (context.Stopping)
            break;
          if (accumulatedObject.BaseObject is string baseObject)
          {
            string str = this.MakePath(providerInstance, relativePath, baseObject, context);
            string path2 = this.MakePath(providerInstance, providerPath, str, context);
            if (this.IsItemContainer(providerInstance, path2, context))
              this.DoGetChildNamesManually(providerInstance, providerPath, str, returnContainers, includeMatcher, excludeMatcher, context, true);
          }
        }
      }
      finally
      {
        context1.RemoveStopReferral();
      }
    }

    private void GetChildNames(
      CmdletProvider providerInstance,
      string path,
      ReturnContainers returnContainers,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        providerInstance1.GetChildNames(path, returnContainers, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetChildNamesProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal object GetChildNamesDynamicParameters(string path, CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      object obj = (object) null;
      if (pathsFromMonadPath.Count > 0)
        obj = this.GetChildNamesDynamicParameters(providerInstance, pathsFromMonadPath[0], context1);
      return obj;
    }

    private object GetChildNamesDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        return providerInstance1.GetChildNamesDynamicParameters(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetChildNamesDynamicParametersProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> RenameItem(
      string path,
      string newName,
      bool force)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      this.RenameItem(path, newName, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void RenameItem(string path, string newName, CmdletProviderContext context)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out provider, out providerInstance);
      if (pathsFromMonadPath.Count == 1)
      {
        this.RenameItem(providerInstance, pathsFromMonadPath[0], newName, context);
      }
      else
      {
        ArgumentException argumentException = (ArgumentException) SessionStateInternal.tracer.NewArgumentException(nameof (path), "SessionStateStrings", "RenameMultipleItemError");
        context.WriteError(new ErrorRecord((Exception) argumentException, "RenameMultipleItemError", ErrorCategory.InvalidArgument, (object) pathsFromMonadPath));
      }
    }

    private void RenameItem(
      CmdletProvider providerInstance,
      string path,
      string newName,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        providerInstance1.RenameItem(path, newName, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("RenameItemProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal object RenameItemDynamicParameters(
      string path,
      string newName,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.RenameItemDynamicParameters(providerInstance, pathsFromMonadPath[0], newName, context1) : (object) null;
    }

    private object RenameItemDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      string newName,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        return providerInstance1.RenameItemDynamicParameters(path, newName, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("RenameItemDynamicParametersProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> NewItem(
      string[] paths,
      string name,
      string type,
      object content,
      bool force)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      this.NewItem(paths, name, type, content, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void NewItem(
      string[] paths,
      string name,
      string type,
      object content,
      CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      foreach (string path1 in paths)
      {
        if (path1 == null)
          SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        Collection<string> collection = new Collection<string>();
        if (string.IsNullOrEmpty(name))
        {
          string providerPath = this.Globber.GetProviderPath(path1, context, out provider, out PSDriveInfo _);
          providerInstance = this.GetProviderInstance(provider);
          collection.Add(providerPath);
        }
        else
          collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, true, context, out provider, out providerInstance);
        foreach (string parent in collection)
        {
          string path2 = parent;
          if (!string.IsNullOrEmpty(name))
            path2 = this.MakePath(providerInstance, parent, name, context);
          this.NewItemPrivate(providerInstance, path2, type, content, context);
        }
      }
    }

    private void NewItemPrivate(
      CmdletProvider providerInstance,
      string path,
      string type,
      object content,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        providerInstance1.NewItem(path, type, content, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("NewItemProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal object NewItemDynamicParameters(
      string path,
      string type,
      object newItemValue,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.NewItemDynamicParameters(providerInstance, pathsFromMonadPath[0], type, newItemValue, context1) : (object) null;
    }

    private object NewItemDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      string type,
      object newItemValue,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        return providerInstance1.NewItemDynamicParameters(path, type, newItemValue, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("NewItemDynamicParametersProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal bool HasChildItems(string path, bool force, bool literalPath)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      bool flag = this.HasChildItems(path, context);
      context.ThrowFirstErrorOrDoNothing();
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool HasChildItems(string path, CmdletProviderContext context)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out provider, out providerInstance);
      bool flag = false;
      foreach (string path1 in pathsFromMonadPath)
      {
        flag = this.HasChildItems(providerInstance, path1, context);
        if (flag)
          break;
      }
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool HasChildItems(string providerId, string path)
    {
      if (string.IsNullOrEmpty(providerId))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (providerId));
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      bool flag = this.HasChildItems(providerId, path, context);
      context.ThrowFirstErrorOrDoNothing();
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool HasChildItems(string providerId, string path, CmdletProviderContext context)
    {
      bool flag = this.HasChildItems((CmdletProvider) this.GetContainerProviderInstance(providerId), path, context);
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    private bool HasChildItems(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        return providerInstance1.HasChildItems(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("HasChildItemsProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> CopyItem(
      string[] paths,
      string copyPath,
      bool recurse,
      CopyContainers copyContainers,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (copyPath == null)
        copyPath = string.Empty;
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.CopyItem(paths, copyPath, recurse, copyContainers, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void CopyItem(
      string[] paths,
      string copyPath,
      bool recurse,
      CopyContainers copyContainers,
      CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (copyPath == null)
        copyPath = string.Empty;
      PSDriveInfo drive = (PSDriveInfo) null;
      ProviderInfo provider1 = (ProviderInfo) null;
      string providerPath = this.Globber.GetProviderPath(copyPath, context, out provider1, out drive);
      SessionStateInternal.tracer.WriteLine("providerDestinationPath = {0}", (object) providerPath);
      ProviderInfo provider2 = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      foreach (string path in paths)
      {
        if (path == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out provider2, out providerInstance);
        if (provider2 != provider1)
        {
          ArgumentException argumentException = (ArgumentException) SessionStateInternal.tracer.NewArgumentException("path", "SessionStateStrings", "CopyItemSourceAndDestinationNotSameProvider");
          SessionStateInternal.tracer.TraceException((Exception) argumentException);
          context.WriteError(new ErrorRecord((Exception) argumentException, "CopyItemSourceAndDestinationNotSameProvider", ErrorCategory.InvalidArgument, (object) pathsFromMonadPath));
          break;
        }
        bool flag1 = this.IsItemContainer(providerInstance, providerPath, context);
        SessionStateInternal.tracer.WriteLine("destinationIsContainer = {0}", (object) flag1);
        foreach (string str in pathsFromMonadPath)
        {
          if (context.Stopping)
            return;
          bool flag2 = this.IsItemContainer(providerInstance, str, context);
          SessionStateInternal.tracer.WriteLine("sourcIsContainer = {0}", (object) flag2);
          if (flag2)
          {
            if (flag1)
            {
              if (!recurse && copyContainers == CopyContainers.CopyChildrenOfTargetContainer)
              {
                Exception exception = (Exception) SessionStateInternal.tracer.NewArgumentException("path", "SessionStateStrings", "CopyContainerToContainerWithoutRecurseOrContainer");
                context.WriteError(new ErrorRecord(exception, "CopyContainerToContainerWithoutRecurseOrContainer", ErrorCategory.InvalidArgument, (object) str));
              }
              else if (recurse && copyContainers == CopyContainers.CopyChildrenOfTargetContainer)
                this.CopyRecurseToSingleContainer(providerInstance, str, providerPath, context);
              else
                this.CopyItem(providerInstance, str, providerPath, recurse, context);
            }
            else if (this.ItemExists(providerInstance, providerPath, context))
            {
              Exception exception = (Exception) SessionStateInternal.tracer.NewArgumentException("path", "SessionStateStrings", "CopyContainerItemToLeafError");
              context.WriteError(new ErrorRecord(exception, "CopyContainerItemToLeafError", ErrorCategory.InvalidArgument, (object) str));
            }
            else
              this.CopyItem(providerInstance, str, providerPath, recurse, context);
          }
          else
            this.CopyItem(providerInstance, str, providerPath, recurse, context);
        }
      }
    }

    private void CopyItem(
      CmdletProvider providerInstance,
      string path,
      string copyPath,
      bool recurse,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        providerInstance1.CopyItem(path, copyPath, recurse, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("CopyItemProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    private void CopyRecurseToSingleContainer(
      CmdletProvider providerInstance,
      string sourcePath,
      string destinationPath,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      foreach (string childName in this.GetChildNames(new string[1]
      {
        sourcePath
      }, ReturnContainers.ReturnMatchingContainers, true, false, false))
      {
        if (context.Stopping)
          break;
        string path = this.MakePath(providerInstance.ProviderInfo, sourcePath, childName, context);
        this.CopyItem((CmdletProvider) providerInstance1, path, destinationPath, false, context);
      }
    }

    internal object CopyItemDynamicParameters(
      string path,
      string destination,
      bool recurse,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.CopyItemDynamicParameters(providerInstance, pathsFromMonadPath[0], destination, recurse, context1) : (object) null;
    }

    private object CopyItemDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      string destination,
      bool recurse,
      CmdletProviderContext context)
    {
      ContainerCmdletProvider providerInstance1 = SessionStateInternal.GetContainerProviderInstance(providerInstance);
      try
      {
        return providerInstance1.CopyItemDynamicParameters(path, destination, recurse, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("CopyItemDynamicParametersProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal Collection<IContentReader> GetContentReader(
      string[] paths,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      Collection<IContentReader> contentReader = this.GetContentReader(paths, context);
      context.ThrowFirstErrorOrDoNothing();
      return contentReader;
    }

    internal Collection<IContentReader> GetContentReader(
      string[] paths,
      CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      Collection<IContentReader> collection = new Collection<IContentReader>();
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        foreach (string path2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider, out providerInstance))
        {
          IContentReader contentReaderPrivate = this.GetContentReaderPrivate(providerInstance, path2, context);
          if (contentReaderPrivate != null)
            collection.Add(contentReaderPrivate);
          context.ThrowFirstErrorOrDoNothing(true);
        }
      }
      return collection;
    }

    private IContentReader GetContentReaderPrivate(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.GetContentReader(path, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetContentReaderProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal object GetContentReaderDynamicParameters(string path, CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.GetContentReaderDynamicParameters(providerInstance, pathsFromMonadPath[0], context1) : (object) null;
    }

    private object GetContentReaderDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.GetContentReaderDynamicParameters(path, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetContentReaderDynamicParametersProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal Collection<IContentWriter> GetContentWriter(
      string[] paths,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      Collection<IContentWriter> contentWriter = this.GetContentWriter(paths, context);
      context.ThrowFirstErrorOrDoNothing();
      return contentWriter;
    }

    internal Collection<IContentWriter> GetContentWriter(
      string[] paths,
      CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      Collection<IContentWriter> collection = new Collection<IContentWriter>();
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        foreach (string path2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, true, context, out provider, out providerInstance))
        {
          IContentWriter contentWriterPrivate = this.GetContentWriterPrivate(providerInstance, path2, context);
          if (contentWriterPrivate != null)
            collection.Add(contentWriterPrivate);
        }
      }
      return collection;
    }

    private IContentWriter GetContentWriterPrivate(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.GetContentWriter(path, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetContentWriterProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal object GetContentWriterDynamicParameters(string path, CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.GetContentWriterDynamicParameters(providerInstance, pathsFromMonadPath[0], context1) : (object) null;
    }

    private object GetContentWriterDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.GetContentWriterDynamicParameters(path, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetContentWriterDynamicParametersProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal void ClearContent(string[] paths, bool force, bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.ClearContent(paths, context);
      context.ThrowFirstErrorOrDoNothing();
    }

    internal void ClearContent(string[] paths, CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      foreach (string path1 in paths)
      {
        if (path1 == null)
          SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        foreach (string path2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider, out providerInstance))
          this.ClearContentPrivate(providerInstance, path2, context);
      }
    }

    private void ClearContentPrivate(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      try
      {
        providerInstance.ClearContent(path, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("ClearContentProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal object ClearContentDynamicParameters(string path, CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.ClearContentDynamicParameters(providerInstance, pathsFromMonadPath[0], context1) : (object) null;
    }

    private object ClearContentDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.ClearContentDynamicParameters(path, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("ClearContentDynamicParametersProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal PSDriveInfo NewDrive(PSDriveInfo drive, string scopeID)
    {
      if (drive == (PSDriveInfo) null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (drive));
      PSDriveInfo psDriveInfo = (PSDriveInfo) null;
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      this.NewDrive(drive, scopeID, context);
      context.ThrowFirstErrorOrDoNothing();
      Collection<PSObject> accumulatedObjects = context.GetAccumulatedObjects();
      if (accumulatedObjects != null && accumulatedObjects.Count > 0 && !accumulatedObjects[0].immediateBaseObjectIsEmpty)
        psDriveInfo = (PSDriveInfo) accumulatedObjects[0].BaseObject;
      return psDriveInfo;
    }

    internal void NewDrive(PSDriveInfo drive, string scopeID, CmdletProviderContext context)
    {
      if (drive == (PSDriveInfo) null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (drive));
      if (context == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (context));
      PSDriveInfo newDrive = SessionStateInternal.IsValidDriveName(drive.Name) ? this.ValidateDriveWithProvider(drive, context, true) : throw (ArgumentException) SessionStateInternal.tracer.NewArgumentException("drive.Name", "SessionStateStrings", "DriveNameIllegalCharacters");
      if (newDrive == (PSDriveInfo) null)
        return;
      if (string.Compare(newDrive.Name, drive.Name, true, Thread.CurrentThread.CurrentCulture) == 0)
      {
        try
        {
          SessionStateScope sessionStateScope = this.currentScope;
          if (!string.IsNullOrEmpty(scopeID))
            sessionStateScope = this.GetScopeByID(scopeID);
          sessionStateScope.NewDrive(newDrive);
        }
        catch (ArgumentException ex)
        {
          context.WriteError(new ErrorRecord((Exception) ex, "NewDriveError", ErrorCategory.InvalidArgument, (object) newDrive));
          return;
        }
        catch (SessionStateException ex)
        {
          throw;
        }
        if (this.ProvidersCurrentWorkingDrive[drive.Provider] == (PSDriveInfo) null)
          this.ProvidersCurrentWorkingDrive[drive.Provider] = drive;
        context.WriteObject((object) newDrive);
      }
      else
      {
        ProviderInvocationException invocationException = this.NewProviderInvocationException("NewDriveProviderFailed", drive.Provider, drive.Root, (Exception) SessionStateInternal.tracer.NewArgumentException("root"));
        SessionStateInternal.tracer.TraceException((Exception) invocationException);
        throw invocationException;
      }
    }

    private static bool IsValidDriveName(string name)
    {
      bool flag = true;
      if (string.IsNullOrEmpty(name))
        flag = false;
      else if (name.IndexOfAny(SessionStateInternal._charactersInvalidInDriveName) >= 0)
        flag = false;
      return flag;
    }

    private string GetProviderRootFromSpecifiedRoot(string root, ProviderInfo provider)
    {
      string str = root;
      SessionState sessionState = new SessionState(this._context.TopLevelSessionState);
      ProviderInfo provider1 = (ProviderInfo) null;
      try
      {
        Collection<string> providerPathFromPsPath = sessionState.Path.GetResolvedProviderPathFromPSPath(root, out provider1);
        if (providerPathFromPsPath != null)
        {
          if (providerPathFromPsPath.Count == 1)
          {
            if (provider.NameEquals(provider1.FullName))
            {
              if (new ProviderIntrinsics(this).Item.Exists(root))
                str = providerPathFromPsPath[0];
            }
          }
        }
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (DriveNotFoundException ex)
      {
      }
      catch (ProviderNotFoundException ex)
      {
      }
      catch (ItemNotFoundException ex)
      {
      }
      catch (NotSupportedException ex)
      {
      }
      catch (InvalidOperationException ex)
      {
      }
      catch (ProviderInvocationException ex)
      {
      }
      catch (ArgumentException ex)
      {
      }
      return str;
    }

    internal object NewDriveDynamicParameters(string providerId, CmdletProviderContext context)
    {
      if (providerId == null)
        return (object) null;
      DriveCmdletProvider providerInstance = this.GetDriveProviderInstance(providerId);
      try
      {
        return providerInstance.NewDriveDynamicParameters(context);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("NewDriveDynamicParametersProviderException", providerInstance.ProviderInfo, (string) null, ex);
      }
    }

    internal PSDriveInfo GetDrive(string name)
    {
      if (name == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (name));
      PSDriveInfo drive = (PSDriveInfo) null;
      SessionStateScopeEnumerator stateScopeEnumerator = new SessionStateScopeEnumerator(this, this.CurrentScope);
      int num = 0;
      foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) stateScopeEnumerator)
      {
        drive = scope.GetDrive(name);
        if (drive != (PSDriveInfo) null)
        {
          if (drive.IsAutoMounted)
          {
            if (drive.IsAutoMountedManuallyRemoved)
            {
              DriveNotFoundException notFoundException = new DriveNotFoundException(name, "DriveNotFound");
              SessionStateInternal.tracer.TraceException((Exception) notFoundException);
              throw notFoundException;
            }
            if (!this.ValidateOrRemoveAutoMountedDrive(drive, scope))
              drive = (PSDriveInfo) null;
          }
          if (drive != (PSDriveInfo) null)
          {
            SessionStateInternal.tracer.WriteLine("Drive found in scope {0}", (object) num);
            break;
          }
        }
        ++num;
      }
      if (drive == (PSDriveInfo) null && this == this._context.TopLevelSessionState)
        drive = this.AutomountFileSystemDrive(name);
      if (drive == (PSDriveInfo) null)
      {
        DriveNotFoundException notFoundException = new DriveNotFoundException(name, "DriveNotFound");
        SessionStateInternal.tracer.TraceException((Exception) notFoundException);
        throw notFoundException;
      }
      return drive;
    }

    internal PSDriveInfo GetDrive(string name, string scopeID)
    {
      if (name == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (name));
      PSDriveInfo drive = (PSDriveInfo) null;
      if (string.IsNullOrEmpty(scopeID))
      {
        foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) new SessionStateScopeEnumerator(this, this.CurrentScope))
        {
          drive = scope.GetDrive(name);
          if (drive != (PSDriveInfo) null)
          {
            if (drive.IsAutoMounted && !this.ValidateOrRemoveAutoMountedDrive(drive, scope))
              drive = (PSDriveInfo) null;
            if (drive != (PSDriveInfo) null)
              break;
          }
        }
        if (drive == (PSDriveInfo) null)
          drive = this.AutomountFileSystemDrive(name);
      }
      else
      {
        SessionStateScope scopeById = this.GetScopeByID(scopeID);
        drive = scopeById.GetDrive(name);
        if (drive != (PSDriveInfo) null)
        {
          if (drive.IsAutoMounted && !this.ValidateOrRemoveAutoMountedDrive(drive, scopeById))
            drive = (PSDriveInfo) null;
        }
        else if (scopeById == this._globalScope)
          drive = this.AutomountFileSystemDrive(name);
      }
      return drive;
    }

    private PSDriveInfo AutomountFileSystemDrive(string name)
    {
      PSDriveInfo psDriveInfo = (PSDriveInfo) null;
      if (name.Length == 1)
      {
        try
        {
          psDriveInfo = this.AutomountFileSystemDrive(new DriveInfo(name));
        }
        catch (PipelineStoppedException ex)
        {
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
        }
      }
      return psDriveInfo;
    }

    private PSDriveInfo AutomountFileSystemDrive(DriveInfo systemDriveInfo)
    {
      PSDriveInfo newDrive = (PSDriveInfo) null;
      if (!this.IsProviderLoaded(this.ExecutionContext.ProviderNames.FileSystem))
      {
        SessionStateInternal.tracer.WriteLine("The {0} provider is not loaded", (object) this.ExecutionContext.ProviderNames.FileSystem);
        return newDrive;
      }
      try
      {
        DriveCmdletProvider providerInstance = this.GetDriveProviderInstance(this.ExecutionContext.ProviderNames.FileSystem);
        if (providerInstance != null)
        {
          PSDriveInfo drive = new PSDriveInfo(systemDriveInfo.Name.Substring(0, 1), providerInstance.ProviderInfo, systemDriveInfo.RootDirectory.FullName, systemDriveInfo.VolumeLabel, (PSCredential) null);
          drive.IsAutoMounted = true;
          CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
          drive.DriveBeingCreated = true;
          newDrive = this.ValidateDriveWithProvider(providerInstance, drive, context, false);
          drive.DriveBeingCreated = false;
          if (newDrive != (PSDriveInfo) null)
          {
            if (!context.HasErrors())
              this._globalScope.NewDrive(newDrive);
          }
        }
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        MshLog.LogProviderHealthEvent(this.ExecutionContext, this.ExecutionContext.ProviderNames.FileSystem, ex, Severity.Warning);
      }
      return newDrive;
    }

    private bool ValidateOrRemoveAutoMountedDrive(PSDriveInfo drive, SessionStateScope scope)
    {
      bool flag;
      try
      {
        flag = new DriveInfo(drive.Name).DriveType != DriveType.NoRootDirectory;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        flag = false;
      }
      if (!flag)
      {
        DriveCmdletProvider driveCmdletProvider = (DriveCmdletProvider) null;
        try
        {
          driveCmdletProvider = this.GetDriveProviderInstance(this.ExecutionContext.ProviderNames.FileSystem);
        }
        catch (NotSupportedException ex)
        {
        }
        catch (ProviderNotFoundException ex)
        {
        }
        if (driveCmdletProvider != null)
        {
          CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
          try
          {
            driveCmdletProvider.RemoveDrive(drive, context);
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
          }
          scope.RemoveDrive(drive);
        }
      }
      return flag;
    }

    internal Collection<PSDriveInfo> GetDrivesForProvider(string providerId)
    {
      if (string.IsNullOrEmpty(providerId))
        return this.Drives((string) null);
      this.GetSingleProvider(providerId);
      Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
      foreach (PSDriveInfo drive in this.Drives((string) null))
      {
        if (drive != (PSDriveInfo) null && drive.Provider.NameEquals(providerId))
          collection.Add(drive);
      }
      return collection;
    }

    internal void RemoveDrive(string driveName, bool force, string scopeID)
    {
      PSDriveInfo drive = driveName != null ? this.GetDrive(driveName, scopeID) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (driveName));
      if (drive == (PSDriveInfo) null)
      {
        DriveNotFoundException notFoundException = new DriveNotFoundException(driveName, "DriveNotFound");
        SessionStateInternal.tracer.TraceException((Exception) notFoundException);
        throw notFoundException;
      }
      this.RemoveDrive(drive, force, scopeID);
    }

    internal void RemoveDrive(
      string driveName,
      bool force,
      string scopeID,
      CmdletProviderContext context)
    {
      PSDriveInfo drive = driveName != null ? this.GetDrive(driveName, scopeID) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (driveName));
      if (drive == (PSDriveInfo) null)
      {
        DriveNotFoundException notFoundException = new DriveNotFoundException(driveName, "DriveNotFound");
        context.WriteError(new ErrorRecord(notFoundException.ErrorRecord, (Exception) notFoundException));
      }
      else
        this.RemoveDrive(drive, force, scopeID, context);
    }

    internal void RemoveDrive(PSDriveInfo drive, bool force, string scopeID)
    {
      if (drive == (PSDriveInfo) null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (drive));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      this.RemoveDrive(drive, force, scopeID, context);
      if (!context.HasErrors() || force)
        return;
      context.ThrowFirstErrorOrDoNothing();
    }

    internal void RemoveDrive(
      PSDriveInfo drive,
      bool force,
      string scopeID,
      CmdletProviderContext context)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        bool flag = false;
        try
        {
          flag = this.CanRemoveDrive(drive, context);
        }
        catch (PipelineStoppedException ex)
        {
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          throw;
        }
        catch (ProviderInvocationException ex)
        {
          if (!force)
            throw;
        }
        if (flag || force)
        {
          if (string.IsNullOrEmpty(scopeID))
          {
            foreach (SessionStateScope sessionStateScope in (IEnumerable<SessionStateScope>) new SessionStateScopeEnumerator(this, this.CurrentScope))
            {
              try
              {
                PSDriveInfo drive1 = sessionStateScope.GetDrive(drive.Name);
                if (drive1 != (PSDriveInfo) null)
                {
                  sessionStateScope.RemoveDrive(drive);
                  if (!(this.ProvidersCurrentWorkingDrive[drive.Provider] == drive1))
                    break;
                  this.ProvidersCurrentWorkingDrive[drive.Provider] = (PSDriveInfo) null;
                  break;
                }
              }
              catch (ArgumentException ex)
              {
              }
            }
          }
          else
          {
            this.GetScopeByID(scopeID).RemoveDrive(drive);
            if (!(this.ProvidersCurrentWorkingDrive[drive.Provider] == drive))
              return;
            this.ProvidersCurrentWorkingDrive[drive.Provider] = (PSDriveInfo) null;
          }
        }
        else
        {
          PSInvalidOperationException operationException = SessionStateInternal.tracer.NewInvalidOperationException("SessionStateStrings", "DriveRemovalPreventedByProvider", (object) drive.Name, (object) drive.Provider);
          context.WriteError(new ErrorRecord(operationException.ErrorRecord, (Exception) operationException));
        }
      }
    }

    private bool CanRemoveDrive(PSDriveInfo drive, CmdletProviderContext context)
    {
      if (context == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (context));
      if (drive == (PSDriveInfo) null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (drive));
      SessionStateInternal.tracer.WriteLine("Drive name = {0}", (object) drive.Name);
      context.Drive = drive;
      DriveCmdletProvider providerInstance = this.GetDriveProviderInstance(drive.Provider);
      bool flag = false;
      PSDriveInfo psDriveInfo;
      try
      {
        psDriveInfo = providerInstance.RemoveDrive(drive, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw this.NewProviderInvocationException("RemoveDriveProviderException", providerInstance.ProviderInfo, (string) null, ex);
      }
      if (psDriveInfo != (PSDriveInfo) null && string.Compare(psDriveInfo.Name, drive.Name, true, Thread.CurrentThread.CurrentCulture) == 0)
        flag = true;
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal Collection<PSDriveInfo> Drives(string scope)
    {
      Dictionary<string, PSDriveInfo> dictionary = new Dictionary<string, PSDriveInfo>();
      SessionStateScope scope1 = this.currentScope;
      if (!string.IsNullOrEmpty(scope))
        scope1 = this.GetScopeByID(scope);
      foreach (SessionStateScope scope2 in (IEnumerable<SessionStateScope>) new SessionStateScopeEnumerator(this, scope1))
      {
        foreach (PSDriveInfo drive in scope2.Drives)
        {
          if (drive != (PSDriveInfo) null)
          {
            bool flag = true;
            if (drive.IsAutoMounted)
              flag = this.ValidateOrRemoveAutoMountedDrive(drive, scope2);
            if (flag && !dictionary.ContainsKey(drive.Name))
              dictionary[drive.Name] = drive;
          }
        }
        if (scope != null)
        {
          if (scope.Length > 0)
            break;
        }
      }
      try
      {
        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
          if (drive != null && drive.DriveType != DriveType.Fixed)
          {
            string key = drive.Name.Substring(0, 1);
            if (!dictionary.ContainsKey(key))
            {
              PSDriveInfo psDriveInfo = this.AutomountFileSystemDrive(drive);
              if (psDriveInfo != (PSDriveInfo) null)
                dictionary[psDriveInfo.Name] = psDriveInfo;
            }
          }
        }
      }
      catch (IOException ex)
      {
      }
      catch (UnauthorizedAccessException ex)
      {
      }
      Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
      foreach (PSDriveInfo psDriveInfo in dictionary.Values)
        collection.Add(psDriveInfo);
      return collection;
    }

    internal PSDriveInfo CurrentDrive
    {
      get => this != this._context.TopLevelSessionState ? this._context.TopLevelSessionState.CurrentDrive : this.currentDrive;
      set
      {
        if (value == (PSDriveInfo) null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (value));
        if (this != this._context.TopLevelSessionState)
          this._context.TopLevelSessionState.CurrentDrive = value;
        else
          this.currentDrive = value;
      }
    }

    internal Collection<PSObject> NewProperty(
      string[] paths,
      string property,
      string type,
      object value,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (property == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (property));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.NewProperty(paths, property, type, value, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void NewProperty(
      string[] paths,
      string property,
      string type,
      object value,
      CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (property == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (property));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        foreach (string path2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider, out providerInstance))
          this.NewProperty(providerInstance, path2, property, type, value, context);
      }
    }

    private void NewProperty(
      CmdletProvider providerInstance,
      string path,
      string property,
      string type,
      object value,
      CmdletProviderContext context)
    {
      try
      {
        providerInstance.NewProperty(path, property, type, value, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("NewPropertyProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal object NewPropertyDynamicParameters(
      string path,
      string propertyName,
      string type,
      object value,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.NewPropertyDynamicParameters(providerInstance, pathsFromMonadPath[0], propertyName, type, value, context1) : (object) null;
    }

    private object NewPropertyDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      string propertyName,
      string type,
      object value,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.NewPropertyDynamicParameters(path, propertyName, type, value, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("NewPropertyDynamicParametersProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal void RemoveProperty(string[] paths, string property, bool force, bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (property == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (property));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.RemoveProperty(paths, property, context);
      context.ThrowFirstErrorOrDoNothing();
    }

    internal void RemoveProperty(string[] paths, string property, CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (property == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (property));
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        foreach (string path2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider, out providerInstance))
          this.RemoveProperty(providerInstance, path2, property, context);
      }
    }

    private void RemoveProperty(
      CmdletProvider providerInstance,
      string path,
      string property,
      CmdletProviderContext context)
    {
      try
      {
        providerInstance.RemoveProperty(path, property, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("RemovePropertyProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal object RemovePropertyDynamicParameters(
      string path,
      string propertyName,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.RemovePropertyDynamicParameters(providerInstance, pathsFromMonadPath[0], propertyName, context1) : (object) null;
    }

    private object RemovePropertyDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      string propertyName,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.RemovePropertyDynamicParameters(path, propertyName, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("RemovePropertyDynamicParametersProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> CopyProperty(
      string[] sourcePaths,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      bool force,
      bool literalPath)
    {
      if (sourcePaths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourcePaths));
      if (sourceProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourceProperty));
      if (destinationPath == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (destinationPath));
      if (destinationProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (destinationProperty));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.CopyProperty(sourcePaths, sourceProperty, destinationPath, destinationProperty, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void CopyProperty(
      string[] sourcePaths,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      if (sourcePaths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourcePaths));
      if (sourceProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourceProperty));
      if (destinationPath == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (destinationPath));
      if (destinationProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (destinationProperty));
      foreach (string sourcePath1 in sourcePaths)
      {
        if (sourcePath1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourcePaths));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        Collection<string> pathsFromMonadPath1 = this.Globber.GetGlobbedProviderPathsFromMonadPath(sourcePath1, false, context, out provider, out providerInstance);
        if (pathsFromMonadPath1.Count > 0)
        {
          Collection<string> include = context.Include;
          Collection<string> exclude = context.Exclude;
          string filter = context.Filter;
          context.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
          Collection<string> pathsFromMonadPath2 = this.Globber.GetGlobbedProviderPathsFromMonadPath(destinationPath, false, context, out provider, out providerInstance);
          context.SetFilters(include, exclude, filter);
          foreach (string sourcePath2 in pathsFromMonadPath1)
          {
            foreach (string destinationPath1 in pathsFromMonadPath2)
              this.CopyProperty(providerInstance, sourcePath2, sourceProperty, destinationPath1, destinationProperty, context);
          }
        }
      }
    }

    private void CopyProperty(
      CmdletProvider providerInstance,
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      try
      {
        providerInstance.CopyProperty(sourcePath, sourceProperty, destinationPath, destinationProperty, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("CopyPropertyProviderException", providerInstance.ProviderInfo, sourcePath, ex);
      }
    }

    internal object CopyPropertyDynamicParameters(
      string path,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.CopyPropertyDynamicParameters(providerInstance, pathsFromMonadPath[0], sourceProperty, destinationPath, destinationProperty, context1) : (object) null;
    }

    private object CopyPropertyDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.CopyPropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("CopyPropertyDynamicParametersProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> MoveProperty(
      string[] sourcePaths,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      bool force,
      bool literalPath)
    {
      if (sourcePaths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourcePaths));
      if (sourceProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourceProperty));
      if (destinationPath == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (destinationPath));
      if (destinationProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (destinationProperty));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.MoveProperty(sourcePaths, sourceProperty, destinationPath, destinationProperty, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void MoveProperty(
      string[] sourcePaths,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      if (sourcePaths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourcePaths));
      if (sourceProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourceProperty));
      if (destinationPath == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (destinationPath));
      if (destinationProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (destinationProperty));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(destinationPath, false, context1, out provider, out providerInstance);
      if (pathsFromMonadPath.Count > 1)
      {
        ArgumentException argumentException = (ArgumentException) SessionStateInternal.tracer.NewArgumentException(nameof (destinationPath), "SessionStateStrings", "MovePropertyDestinationResolveToSingle");
        SessionStateInternal.tracer.TraceException((Exception) argumentException);
        context.WriteError(new ErrorRecord((Exception) argumentException, argumentException.GetType().FullName, ErrorCategory.InvalidArgument, (object) pathsFromMonadPath));
      }
      else
      {
        foreach (string sourcePath1 in sourcePaths)
        {
          if (sourcePath1 == null)
            throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourcePaths));
          foreach (string sourcePath2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(sourcePath1, false, context, out provider, out providerInstance))
            this.MoveProperty(providerInstance, sourcePath2, sourceProperty, pathsFromMonadPath[0], destinationProperty, context);
        }
      }
    }

    private void MoveProperty(
      CmdletProvider providerInstance,
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      try
      {
        providerInstance.MoveProperty(sourcePath, sourceProperty, destinationPath, destinationProperty, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("MovePropertyProviderException", providerInstance.ProviderInfo, sourcePath, ex);
      }
    }

    internal object MovePropertyDynamicParameters(
      string path,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.MovePropertyDynamicParameters(providerInstance, pathsFromMonadPath[0], sourceProperty, destinationPath, destinationProperty, context1) : (object) null;
    }

    private object MovePropertyDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.MovePropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("MovePropertyDynamicParametersProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> RenameProperty(
      string[] sourcePaths,
      string sourceProperty,
      string destinationProperty,
      bool force,
      bool literalPath)
    {
      if (sourcePaths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourcePaths));
      if (sourceProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourceProperty));
      if (destinationProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (destinationProperty));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.RenameProperty(sourcePaths, sourceProperty, destinationProperty, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void RenameProperty(
      string[] paths,
      string sourceProperty,
      string destinationProperty,
      CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (sourceProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (sourceProperty));
      if (destinationProperty == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (destinationProperty));
      foreach (string path in paths)
      {
        if (path == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        foreach (string sourcePath in this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out provider, out providerInstance))
          this.RenameProperty(providerInstance, sourcePath, sourceProperty, destinationProperty, context);
      }
    }

    private void RenameProperty(
      CmdletProvider providerInstance,
      string sourcePath,
      string sourceProperty,
      string destinationProperty,
      CmdletProviderContext context)
    {
      try
      {
        providerInstance.RenameProperty(sourcePath, sourceProperty, destinationProperty, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("RenamePropertyProviderException", providerInstance.ProviderInfo, sourcePath, ex);
      }
    }

    internal object RenamePropertyDynamicParameters(
      string path,
      string sourceProperty,
      string destinationProperty,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.RenamePropertyDynamicParameters(providerInstance, pathsFromMonadPath[0], sourceProperty, destinationProperty, context1) : (object) null;
    }

    private object RenamePropertyDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      string sourceProperty,
      string destinationProperty,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.RenamePropertyDynamicParameters(path, sourceProperty, destinationProperty, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("RenamePropertyDynamicParametersProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal void AddSessionStateEntry(SessionStateFunctionEntry entry)
    {
      ScriptBlock function = entry.ScriptBlock.Clone();
      ParseTreeNode end = function.End;
      if (end != null && end is StatementListNode)
      {
        StatementListNode statementListNode = (StatementListNode) end;
        if (statementListNode.Statements.Length == 1 && statementListNode.Statements[0] is FunctionDeclarationNode && statementListNode.Statements[0].NodeToken.TokenText.Equals(entry.Name, StringComparison.OrdinalIgnoreCase))
          throw SessionStateInternal.tracer.NewArgumentException(nameof (entry));
      }
      FunctionInfo functionInfo = this.SetFunction(entry.Name, function, entry.Options, false, CommandOrigin.Internal);
      functionInfo.Visibility = entry.Visibility;
      functionInfo.SetModule(entry.Module);
    }

    internal IDictionary GetFunctionTable()
    {
      SessionStateScopeEnumerator stateScopeEnumerator = new SessionStateScopeEnumerator(this, this.currentScope);
      Dictionary<string, FunctionInfo> dictionary = new Dictionary<string, FunctionInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (SessionStateScope sessionStateScope in (IEnumerable<SessionStateScope>) stateScopeEnumerator)
      {
        foreach (FunctionInfo functionInfo in sessionStateScope.FunctionTable.Values)
        {
          if (!dictionary.ContainsKey(functionInfo.Name))
            dictionary.Add(functionInfo.Name, functionInfo);
        }
      }
      return (IDictionary) dictionary;
    }

    internal IDictionary<string, FunctionInfo> GetFunctionTableAtScope(
      string scopeID)
    {
      Dictionary<string, FunctionInfo> dictionary = new Dictionary<string, FunctionInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      SessionStateScope scopeById = this.GetScopeByID(scopeID);
      foreach (FunctionInfo functionInfo1 in scopeById.FunctionTable.Values)
      {
        FunctionInfo functionInfo2 = functionInfo1;
        if (((functionInfo2 == null ? functionInfo1.Options : functionInfo2.Options) & ScopedItemOptions.Private) == ScopedItemOptions.None || scopeById == this.currentScope)
          dictionary.Add(functionInfo1.Name, functionInfo1);
      }
      return (IDictionary<string, FunctionInfo>) dictionary;
    }

    internal List<FunctionInfo> ExportedFunctions => this._exportedFunctions;

    internal bool UseExportList
    {
      get => this._useExportList;
      set => this._useExportList = value;
    }

    internal FunctionInfo GetFunction(string name, CommandOrigin origin)
    {
      if (string.IsNullOrEmpty(name))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (name));
      FunctionInfo functionInfo = (FunctionInfo) null;
      FunctionScopeItemSearcher scopeItemSearcher = new FunctionScopeItemSearcher(this, (ScopedItemLookupPath) new FunctionLookupPath(name), origin);
      if (scopeItemSearcher.MoveNext())
        functionInfo = ((IEnumerator<FunctionInfo>) scopeItemSearcher).Current;
      return functionInfo;
    }

    internal FunctionInfo GetFunction(string name) => this.GetFunction(name, CommandOrigin.Internal);

    internal FunctionInfo SetFunctionRaw(
      string name,
      ScriptBlock function,
      CommandOrigin origin)
    {
      if (string.IsNullOrEmpty(name))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (name));
      if (function == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (function));
      string itemName = name;
      FunctionLookupPath functionLookupPath = new FunctionLookupPath(name);
      name = functionLookupPath.LookupPath.NamespaceSpecificString;
      if (string.IsNullOrEmpty(name))
      {
        SessionStateException sessionStateException = new SessionStateException(itemName, SessionStateCategory.Function, "ScopedFunctionMustHaveName", ErrorCategory.InvalidArgument, new object[0]);
        SessionStateInternal.tracer.TraceException((Exception) sessionStateException);
        throw sessionStateException;
      }
      ScopedItemOptions options = ScopedItemOptions.None;
      if (functionLookupPath.IsPrivate)
        options |= ScopedItemOptions.Private;
      return new FunctionScopeItemSearcher(this, (ScopedItemLookupPath) functionLookupPath, origin).InitialScope.SetFunction(name, function, options, false, origin, this.ExecutionContext);
    }

    internal FunctionInfo SetFunctionRaw(string name, ScriptBlock function) => this.SetFunctionRaw(name, function, CommandOrigin.Internal);

    internal FunctionInfo SetFunction(
      string name,
      ScriptBlock function,
      ScopedItemOptions options,
      bool force,
      CommandOrigin origin)
    {
      if (string.IsNullOrEmpty(name))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (name));
      if (function == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (function));
      string itemName = name;
      FunctionLookupPath functionLookupPath = new FunctionLookupPath(name);
      name = functionLookupPath.LookupPath.NamespaceSpecificString;
      if (string.IsNullOrEmpty(name))
      {
        SessionStateException sessionStateException = new SessionStateException(itemName, SessionStateCategory.Function, "ScopedFunctionMustHaveName", ErrorCategory.InvalidArgument, new object[0]);
        SessionStateInternal.tracer.TraceException((Exception) sessionStateException);
        throw sessionStateException;
      }
      if (functionLookupPath.IsPrivate)
        options |= ScopedItemOptions.Private;
      return new FunctionScopeItemSearcher(this, (ScopedItemLookupPath) functionLookupPath, origin).InitialScope.SetFunction(name, function, options, force, origin, this.ExecutionContext);
    }

    internal FunctionInfo SetFunction(
      string name,
      ScriptBlock function,
      bool force,
      CommandOrigin origin)
    {
      if (string.IsNullOrEmpty(name))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (name));
      if (function == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (function));
      string itemName = name;
      FunctionLookupPath functionLookupPath = new FunctionLookupPath(name);
      name = functionLookupPath.LookupPath.NamespaceSpecificString;
      if (string.IsNullOrEmpty(name))
      {
        SessionStateException sessionStateException = new SessionStateException(itemName, SessionStateCategory.Function, "ScopedFunctionMustHaveName", ErrorCategory.InvalidArgument, new object[0]);
        SessionStateInternal.tracer.TraceException((Exception) sessionStateException);
        throw sessionStateException;
      }
      ScopedItemOptions options1 = ScopedItemOptions.None;
      if (functionLookupPath.IsPrivate)
        options1 |= ScopedItemOptions.Private;
      FunctionScopeItemSearcher scopeItemSearcher = new FunctionScopeItemSearcher(this, (ScopedItemLookupPath) functionLookupPath, origin);
      SessionStateScope initialScope = scopeItemSearcher.InitialScope;
      FunctionInfo functionInfo1;
      if (scopeItemSearcher.MoveNext())
      {
        SessionStateScope currentLookupScope = scopeItemSearcher.CurrentLookupScope;
        name = scopeItemSearcher.Name;
        if (functionLookupPath.IsPrivate)
        {
          FunctionInfo function1 = currentLookupScope.GetFunction(name);
          FunctionInfo functionInfo2 = function1;
          ScopedItemOptions options2 = functionInfo2 == null ? options1 | function1.Options : options1 | functionInfo2.Options;
          functionInfo1 = currentLookupScope.SetFunction(name, function, options2, force, origin, this.ExecutionContext);
        }
        else
          functionInfo1 = currentLookupScope.SetFunction(name, function, force, origin, this.ExecutionContext);
      }
      else
        functionInfo1 = !functionLookupPath.IsPrivate ? initialScope.SetFunction(name, function, force, origin, this.ExecutionContext) : initialScope.SetFunction(name, function, options1, force, origin, this.ExecutionContext);
      return functionInfo1;
    }

    internal FunctionInfo SetFunction(string name, ScriptBlock function, bool force) => this.SetFunction(name, function, force, CommandOrigin.Internal);

    internal void RemoveFunction(string name, bool force, CommandOrigin origin)
    {
      if (string.IsNullOrEmpty(name))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (name));
      SessionStateScope sessionStateScope = this.currentScope;
      FunctionScopeItemSearcher scopeItemSearcher = new FunctionScopeItemSearcher(this, (ScopedItemLookupPath) new FunctionLookupPath(name), origin);
      if (scopeItemSearcher.MoveNext())
        sessionStateScope = scopeItemSearcher.CurrentLookupScope;
      sessionStateScope.RemoveFunction(name, force);
    }

    internal void RemoveFunction(string name, bool force) => this.RemoveFunction(name, force, CommandOrigin.Internal);

    internal void RemoveFunction(string name, PSModuleInfo module)
    {
      FunctionInfo function = this.GetFunction(name);
      if (function == null || function.ScriptBlock == null || (function.ScriptBlock.File == null || !function.ScriptBlock.File.Equals(module.Path, StringComparison.OrdinalIgnoreCase)))
        return;
      this.RemoveFunction(name, true);
    }

    internal Collection<PSObject> GetItem(
      string[] paths,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.GetItem(paths, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void GetItem(string[] paths, CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        foreach (string path2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider, out providerInstance))
          this.GetItemPrivate(providerInstance, path2, context);
      }
    }

    private void GetItemPrivate(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      ItemCmdletProvider providerInstance1 = SessionStateInternal.GetItemProviderInstance(providerInstance);
      try
      {
        providerInstance1.GetItem(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetItemProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal object GetItemDynamicParameters(string path, CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.GetItemDynamicParameters(providerInstance, pathsFromMonadPath[0], context1) : (object) null;
    }

    private object GetItemDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      ItemCmdletProvider providerInstance1 = SessionStateInternal.GetItemProviderInstance(providerInstance);
      try
      {
        return providerInstance1.GetItemDynamicParameters(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetItemDynamicParametersProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> SetItem(
      string[] paths,
      object value,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.SetItem(paths, value, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void SetItem(string[] paths, object value, CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, true, context, out provider, out providerInstance);
        if (pathsFromMonadPath != null)
        {
          foreach (string path2 in pathsFromMonadPath)
            this.SetItem(providerInstance, path2, value, context);
        }
      }
    }

    private void SetItem(
      CmdletProvider providerInstance,
      string path,
      object value,
      CmdletProviderContext context)
    {
      ItemCmdletProvider providerInstance1 = SessionStateInternal.GetItemProviderInstance(providerInstance);
      try
      {
        providerInstance1.SetItem(path, value, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("SetItemProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal object SetItemDynamicParameters(
      string path,
      object value,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.SetItemDynamicParameters(providerInstance, pathsFromMonadPath[0], value, context1) : (object) null;
    }

    private object SetItemDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      object value,
      CmdletProviderContext context)
    {
      ItemCmdletProvider providerInstance1 = SessionStateInternal.GetItemProviderInstance(providerInstance);
      try
      {
        return providerInstance1.SetItemDynamicParameters(path, value, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("SetItemDynamicParametersProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> ClearItem(
      string[] paths,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.ClearItem(paths, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void ClearItem(string[] paths, CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider, out providerInstance);
        if (pathsFromMonadPath != null)
        {
          foreach (string path2 in pathsFromMonadPath)
            this.ClearItemPrivate(providerInstance, path2, context);
        }
      }
    }

    private void ClearItemPrivate(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      ItemCmdletProvider providerInstance1 = SessionStateInternal.GetItemProviderInstance(providerInstance);
      try
      {
        providerInstance1.ClearItem(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("ClearItemProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal object ClearItemDynamicParameters(string path, CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.ClearItemDynamicParameters(providerInstance, pathsFromMonadPath[0], context1) : (object) null;
    }

    private object ClearItemDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      ItemCmdletProvider providerInstance1 = SessionStateInternal.GetItemProviderInstance(providerInstance);
      try
      {
        return providerInstance1.ClearItemDynamicParameters(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("ClearItemProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal void InvokeDefaultAction(string[] paths, bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.SuppressWildcardExpansion = literalPath;
      this.InvokeDefaultAction(paths, context);
      context.ThrowFirstErrorOrDoNothing();
    }

    internal void InvokeDefaultAction(string[] paths, CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider, out providerInstance);
        if (pathsFromMonadPath != null)
        {
          foreach (string path2 in pathsFromMonadPath)
            this.InvokeDefaultActionPrivate(providerInstance, path2, context);
        }
      }
    }

    private void InvokeDefaultActionPrivate(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      ItemCmdletProvider providerInstance1 = SessionStateInternal.GetItemProviderInstance(providerInstance);
      try
      {
        providerInstance1.InvokeDefaultAction(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("InvokeDefaultActionProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal object InvokeDefaultActionDynamicParameters(string path, CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.InvokeDefaultActionDynamicParameters(providerInstance, pathsFromMonadPath[0], context1) : (object) null;
    }

    private object InvokeDefaultActionDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      ItemCmdletProvider providerInstance1 = SessionStateInternal.GetItemProviderInstance(providerInstance);
      try
      {
        return providerInstance1.InvokeDefaultActionDynamicParameters(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("InvokeDefaultActionDynamicParametersProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal PathInfo CurrentLocation
    {
      get
      {
        if (this.CurrentDrive == (PSDriveInfo) null)
          throw SessionStateInternal.tracer.NewInvalidOperationException();
        PathInfo pathInfo = new PathInfo(this.CurrentDrive, this.CurrentDrive.Provider, this.CurrentDrive.CurrentLocation, new SessionState(this));
        SessionStateInternal.tracer.WriteLine("result = {0}", (object) pathInfo);
        return pathInfo;
      }
    }

    internal PathInfo GetNamespaceCurrentLocation(string namespaceID)
    {
      if (namespaceID == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (namespaceID));
      PSDriveInfo drive = (PSDriveInfo) null;
      if (namespaceID.Length == 0)
        this.ProvidersCurrentWorkingDrive.TryGetValue(this.CurrentDrive.Provider, out drive);
      else
        this.ProvidersCurrentWorkingDrive.TryGetValue(this.GetSingleProvider(namespaceID), out drive);
      if (drive == (PSDriveInfo) null)
      {
        DriveNotFoundException notFoundException = new DriveNotFoundException(namespaceID, "DriveNotFound");
        SessionStateInternal.tracer.TraceException((Exception) notFoundException);
        throw notFoundException;
      }
      new CmdletProviderContext(this.ExecutionContext).Drive = drive;
      string path = !drive.Hidden ? LocationGlobber.GetDriveQualifiedPath(drive.CurrentLocation, drive) : (!LocationGlobber.IsProviderDirectPath(drive.CurrentLocation) ? LocationGlobber.GetProviderQualifiedPath(drive.CurrentLocation, drive.Provider) : drive.CurrentLocation);
      PathInfo pathInfo = new PathInfo(drive, drive.Provider, path, new SessionState(this));
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) pathInfo);
      return pathInfo;
    }

    internal PathInfo SetLocation(string path) => this.SetLocation(path, (CmdletProviderContext) null);

    internal PathInfo SetLocation(string path, CmdletProviderContext context)
    {
      string path1 = path != null ? path : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      string driveName = (string) null;
      string providerId1 = (string) null;
      PSDriveInfo currentDrive = this.CurrentDrive;
      if (LocationGlobber.IsHomePath(path))
        path = this.Globber.GetHomeRelativePath(path);
      if (LocationGlobber.IsProviderDirectPath(path))
        this.CurrentDrive = this.CurrentLocation.Provider.HiddenDrive;
      else if (LocationGlobber.IsProviderQualifiedPath(path, out providerId1))
        this.CurrentDrive = this.GetSingleProvider(providerId1).HiddenDrive;
      else if (this.Globber.IsAbsolutePath(path, out driveName))
        this.CurrentDrive = this.GetDrive(driveName);
      if (context == null)
        context = new CmdletProviderContext(this.ExecutionContext);
      if (this.CurrentDrive != (PSDriveInfo) null)
        context.Drive = this.CurrentDrive;
      CmdletProvider providerInstance = (CmdletProvider) null;
      Collection<PathInfo> pathsFromMonadPath;
      try
      {
        pathsFromMonadPath = this.Globber.GetGlobbedMonadPathsFromMonadPath(path, false, context, out providerInstance);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        this.CurrentDrive = currentDrive;
        throw;
      }
      if (pathsFromMonadPath.Count == 0)
      {
        this.CurrentDrive = currentDrive;
        throw new ItemNotFoundException(path, "PathNotFound");
      }
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      bool flag4 = false;
      for (int index = 0; index < pathsFromMonadPath.Count; ++index)
      {
        CmdletProviderContext context1 = new CmdletProviderContext(context);
        PathInfo pathInfo = pathsFromMonadPath[index];
        string str = path;
        try
        {
          string providerId2 = (string) null;
          flag4 = LocationGlobber.IsProviderQualifiedPath(pathInfo.Path, out providerId2);
          if (flag4)
          {
            string path2 = LocationGlobber.RemoveProviderQualifier(pathInfo.Path);
            try
            {
              str = this.NormalizeRelativePath(this.GetSingleProvider(providerId2), path2, string.Empty, context1);
            }
            catch (NotSupportedException ex)
            {
            }
            catch (PipelineStoppedException ex)
            {
              throw;
            }
            catch (ActionPreferenceStopException ex)
            {
              throw;
            }
            catch (Exception ex)
            {
              CommandProcessorBase.CheckForSevereException(ex);
              SessionStateInternal.tracer.TraceException(ex);
              this.CurrentDrive = currentDrive;
              throw;
            }
          }
          else
          {
            try
            {
              str = this.NormalizeRelativePath(pathInfo.Path, this.CurrentDrive.Root, context1);
            }
            catch (NotSupportedException ex)
            {
            }
            catch (PipelineStoppedException ex)
            {
              throw;
            }
            catch (ActionPreferenceStopException ex)
            {
              throw;
            }
            catch (Exception ex)
            {
              CommandProcessorBase.CheckForSevereException(ex);
              SessionStateInternal.tracer.TraceException(ex);
              this.CurrentDrive = currentDrive;
              throw;
            }
          }
          if (context1.HasErrors())
          {
            this.CurrentDrive = currentDrive;
            context1.ThrowFirstErrorOrDoNothing();
          }
        }
        finally
        {
          context1.RemoveStopReferral();
        }
        bool flag5 = false;
        CmdletProviderContext context2 = new CmdletProviderContext(context);
        context2.SuppressWildcardExpansion = true;
        try
        {
          flag5 = this.IsItemContainer(pathInfo.Path, context2);
          if (context2.HasErrors())
          {
            this.CurrentDrive = currentDrive;
            context2.ThrowFirstErrorOrDoNothing();
          }
        }
        catch (NotSupportedException ex)
        {
          SessionStateInternal.tracer.TraceException((Exception) ex);
          if (str.Length == 0)
            flag5 = true;
        }
        finally
        {
          context2.RemoveStopReferral();
        }
        if (flag5)
        {
          if (flag1)
          {
            this.CurrentDrive = currentDrive;
            throw SessionStateInternal.tracer.NewArgumentException(nameof (path), "SessionStateStrings", "PathResolvedToMultiple", (object) path1);
          }
          path = str;
          flag2 = true;
          flag3 = flag4;
          flag1 = true;
        }
      }
      if (flag2)
      {
        if (!LocationGlobber.IsProviderDirectPath(path) && (path.StartsWith('\\'.ToString(), StringComparison.CurrentCulture) && !flag3))
          path = path.Substring(1);
        SessionStateInternal.tracer.WriteLine("New working path = {0}", (object) path);
        this.CurrentDrive.CurrentLocation = path;
        this.ProvidersCurrentWorkingDrive[this.CurrentDrive.Provider] = this.CurrentDrive;
        this.SetVariable(this.PWDVariablePath, (object) this.CurrentLocation, false, true, CommandOrigin.Internal);
        return this.CurrentLocation;
      }
      this.CurrentDrive = currentDrive;
      throw new ItemNotFoundException(path1, "PathNotFound");
    }

    internal ScopedItemLookupPath PWDVariablePath
    {
      get
      {
        if (this._pwdVariablePath == null)
          this._pwdVariablePath = new ScopedItemLookupPath("global:PWD");
        return this._pwdVariablePath;
      }
    }

    internal bool IsCurrentLocationOrAncestor(string path, CmdletProviderContext context)
    {
      using (SessionStateInternal.tracer.TraceMethod(path, new object[0]))
      {
        bool flag = false;
        if (path == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
        PSDriveInfo drive1 = (PSDriveInfo) null;
        ProviderInfo provider1 = (ProviderInfo) null;
        string str1 = this.Globber.GetProviderPath(path, context, out provider1, out drive1);
        if (drive1 != (PSDriveInfo) null)
        {
          SessionStateInternal.tracer.WriteLine("Tracing drive", new object[0]);
          drive1.Trace();
        }
        if (drive1 != (PSDriveInfo) null)
          context.Drive = drive1;
        if (drive1 == this.CurrentDrive)
        {
          CmdletProviderContext context1 = new CmdletProviderContext(context);
          try
          {
            str1 = this.NormalizeRelativePath(path, (string) null, context1);
          }
          catch (NotSupportedException ex)
          {
          }
          catch (PipelineStoppedException ex)
          {
            throw;
          }
          catch (ActionPreferenceStopException ex)
          {
            throw;
          }
          finally
          {
            context1.RemoveStopReferral();
          }
          if (context1.HasErrors())
            context1.ThrowFirstErrorOrDoNothing();
          SessionStateInternal.tracer.WriteLine("Provider path = {0}", (object) str1);
          PSDriveInfo drive2 = (PSDriveInfo) null;
          ProviderInfo provider2 = (ProviderInfo) null;
          string providerPath = this.Globber.GetProviderPath(".", context, out provider2, out drive2);
          SessionStateInternal.tracer.WriteLine("Current working path = {0}", (object) providerPath);
          SessionStateInternal.tracer.WriteLine("Comparing {0} to {1}", (object) str1, (object) providerPath);
          if (string.Compare(str1, providerPath, true, Thread.CurrentThread.CurrentCulture) == 0)
          {
            SessionStateInternal.tracer.WriteLine("The path is the current working directory", new object[0]);
            flag = true;
          }
          else
          {
            string str2 = providerPath;
            while (str2.Length > 0)
            {
              str2 = this.GetParentPath(drive1.Provider, str2, string.Empty, context);
              SessionStateInternal.tracer.WriteLine("Comparing {0} to {1}", (object) str2, (object) str1);
              if (string.Compare(str2, str1, true, Thread.CurrentThread.CurrentCulture) == 0)
              {
                SessionStateInternal.tracer.WriteLine("The path is a parent of the current working directory: {0}", (object) str2);
                flag = true;
                break;
              }
            }
          }
        }
        else
          SessionStateInternal.tracer.WriteLine("Drives are not the same", new object[0]);
        SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
        return flag;
      }
    }

    internal void PushCurrentLocation(string stackName)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(stackName))
          stackName = this.defaultStackName;
        ProviderInfo provider = this.CurrentDrive.Provider;
        string mshQualifiedPath = LocationGlobber.GetMshQualifiedPath(this.CurrentDrive.CurrentLocation, this.CurrentDrive);
        PathInfo pathInfo = new PathInfo(this.CurrentDrive, provider, mshQualifiedPath, new SessionState(this));
        SessionStateInternal.tracer.WriteLine("Pushing drive: {0} directory: {1}", (object) this.CurrentDrive.Name, (object) mshQualifiedPath);
        Stack<PathInfo> pathInfoStack = (Stack<PathInfo>) null;
        if (!this.workingLocationStack.TryGetValue(stackName, out pathInfoStack))
        {
          pathInfoStack = new Stack<PathInfo>();
          this.workingLocationStack[stackName] = pathInfoStack;
        }
        pathInfoStack.Push(pathInfo);
      }
    }

    internal PathInfo PopLocation(string stackName)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(stackName))
          stackName = this.defaultStackName;
        if (WildcardPattern.ContainsWildcardCharacters(stackName))
        {
          bool flag = false;
          WildcardPattern wildcardPattern = new WildcardPattern(stackName, WildcardOptions.IgnoreCase);
          foreach (string key in this.workingLocationStack.Keys)
          {
            if (wildcardPattern.IsMatch(key))
            {
              flag = !flag ? true : throw SessionStateInternal.tracer.NewArgumentException(nameof (stackName), "SessionStateStrings", "StackNameResolvedToMultiple", (object) stackName);
              stackName = key;
            }
          }
        }
        PathInfo pathInfo1 = this.CurrentLocation;
        try
        {
          Stack<PathInfo> pathInfoStack = (Stack<PathInfo>) null;
          if (!this.workingLocationStack.TryGetValue(stackName, out pathInfoStack))
          {
            if (!string.Equals(stackName, "default", StringComparison.OrdinalIgnoreCase))
              throw SessionStateInternal.tracer.NewArgumentException(nameof (stackName), "SessionStateStrings", "StackNotFound", (object) stackName);
            return (PathInfo) null;
          }
          PathInfo pathInfo2 = pathInfoStack.Pop();
          pathInfo1 = this.SetLocation(LocationGlobber.GetMshQualifiedPath(WildcardPattern.Escape(pathInfo2.Path), pathInfo2.GetDrive()));
          if (pathInfoStack.Count == 0)
          {
            if (!string.Equals(stackName, "default", StringComparison.OrdinalIgnoreCase))
              this.workingLocationStack.Remove(stackName);
          }
        }
        catch (InvalidOperationException ex)
        {
        }
        SessionStateInternal.tracer.WriteLine("result = {0}", (object) pathInfo1);
        return pathInfo1;
      }
    }

    internal PathInfoStack LocationStack(string stackName)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(stackName))
          stackName = this.defaultStackName;
        Stack<PathInfo> locationStack = (Stack<PathInfo>) null;
        if (!this.workingLocationStack.TryGetValue(stackName, out locationStack))
        {
          if (!string.Equals(stackName, "default", StringComparison.OrdinalIgnoreCase))
            throw SessionStateInternal.tracer.NewArgumentException(nameof (stackName));
          locationStack = new Stack<PathInfo>();
        }
        return new PathInfoStack(stackName, locationStack);
      }
    }

    internal PathInfoStack SetDefaultLocationStack(string stackName)
    {
      using (SessionStateInternal.tracer.TraceMethod(stackName, new object[0]))
      {
        if (string.IsNullOrEmpty(stackName))
          stackName = "default";
        if (!this.workingLocationStack.ContainsKey(stackName))
        {
          if (string.Equals(stackName, "default", StringComparison.OrdinalIgnoreCase))
            return new PathInfoStack("default", new Stack<PathInfo>());
          ItemNotFoundException notFoundException = new ItemNotFoundException(stackName, "StackNotFound");
          SessionStateInternal.tracer.TraceException((Exception) notFoundException);
          throw notFoundException;
        }
        this.defaultStackName = stackName;
        Stack<PathInfo> workingLocation = this.workingLocationStack[this.defaultStackName];
        return workingLocation != null ? new PathInfoStack(this.defaultStackName, workingLocation) : (PathInfoStack) null;
      }
    }

    internal string GetParentPath(string path, string root)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      string parentPath = this.GetParentPath(path, root, context);
      context.ThrowFirstErrorOrDoNothing();
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) parentPath);
      return parentPath;
    }

    internal string GetParentPath(string path, string root, CmdletProviderContext context)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      try
      {
        PSDriveInfo drive = (PSDriveInfo) null;
        ProviderInfo provider = (ProviderInfo) null;
        this.Globber.GetProviderPath(path, context1, out provider, out drive);
        if (context1.HasErrors())
        {
          context1.WriteErrorsToContext(context);
          return (string) null;
        }
        if (drive != (PSDriveInfo) null)
          context.Drive = drive;
        bool isProviderQualified = false;
        bool isDriveQualified = false;
        string qualifier = (string) null;
        string path1 = this.RemoveQualifier(path, out qualifier, out isProviderQualified, out isDriveQualified);
        string path2 = this.GetParentPath(provider, path1, root, context);
        if (!string.IsNullOrEmpty(qualifier) && !string.IsNullOrEmpty(path2))
          path2 = this.AddQualifier(path2, qualifier, isProviderQualified, isDriveQualified);
        SessionStateInternal.tracer.WriteLine("result = {0}", (object) path2);
        return path2;
      }
      finally
      {
        context1.RemoveStopReferral();
      }
    }

    private string AddQualifier(
      string path,
      string qualifier,
      bool isProviderQualified,
      bool isDriveQualified)
    {
      string format = "{1}";
      if (isProviderQualified)
        format = "{0}::{1}";
      else if (isDriveQualified)
        format = "{0}:{1}";
      return string.Format((IFormatProvider) CultureInfo.InvariantCulture, format, (object) qualifier, (object) path);
    }

    private string RemoveQualifier(
      string path,
      out string qualifier,
      out bool isProviderQualified,
      out bool isDriveQualified)
    {
      string str = path;
      qualifier = (string) null;
      isProviderQualified = false;
      isDriveQualified = false;
      if (LocationGlobber.IsProviderQualifiedPath(path, out qualifier))
      {
        isProviderQualified = true;
        int num = path.IndexOf("::", StringComparison.Ordinal);
        if (num != -1)
          str = path.Substring(num + 2);
      }
      else if (this.Globber.IsAbsolutePath(path, out qualifier))
      {
        isDriveQualified = true;
        str = path.Substring(qualifier.Length + 1);
      }
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    internal string GetParentPath(
      ProviderInfo provider,
      string path,
      string root,
      CmdletProviderContext context)
    {
      return this.GetParentPath(this.GetProviderInstance(provider), path, root, context);
    }

    internal string GetParentPath(
      CmdletProvider providerInstance,
      string path,
      string root,
      CmdletProviderContext context)
    {
      NavigationCmdletProvider providerInstance1 = SessionStateInternal.GetNavigationProviderInstance(providerInstance);
      try
      {
        return providerInstance1.GetParentPath(path, root, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetParentPathProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal string NormalizeRelativePath(string path, string basePath)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      string str = this.NormalizeRelativePath(path, basePath, context);
      context.ThrowFirstErrorOrDoNothing();
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    internal string NormalizeRelativePath(
      string path,
      string basePath,
      CmdletProviderContext context)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      try
      {
        PSDriveInfo drive = (PSDriveInfo) null;
        ProviderInfo provider = (ProviderInfo) null;
        string path1 = this.Globber.GetProviderPath(path, context1, out provider, out drive);
        if (context1.HasErrors())
        {
          context1.WriteErrorsToContext(context);
          return (string) null;
        }
        if (path1 == null || provider == null)
        {
          Exception exception = (Exception) SessionStateInternal.tracer.NewArgumentException(nameof (path));
          context.WriteError(new ErrorRecord(exception, "NormalizePathNullResult", ErrorCategory.InvalidArgument, (object) path));
          return (string) null;
        }
        if (drive != (PSDriveInfo) null)
        {
          context.Drive = drive;
          if (this.GetProviderInstance(provider) is NavigationCmdletProvider && !string.IsNullOrEmpty(drive.Root) && path.StartsWith(drive.Root, StringComparison.OrdinalIgnoreCase))
            path1 = path;
        }
        return this.NormalizeRelativePath(provider, path1, basePath, context);
      }
      finally
      {
        context1.RemoveStopReferral();
      }
    }

    internal string NormalizeRelativePath(
      ProviderInfo provider,
      string path,
      string basePath,
      CmdletProviderContext context)
    {
      switch (this.GetProviderInstance(provider))
      {
        case NavigationCmdletProvider navigationCmdletProvider:
          try
          {
            path = navigationCmdletProvider.NormalizeRelativePath(path, basePath, context);
            goto label_6;
          }
          catch (PipelineStoppedException ex)
          {
            throw;
          }
          catch (ActionPreferenceStopException ex)
          {
            throw;
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            SessionStateInternal.tracer.TraceException(ex);
            throw this.NewProviderInvocationException("NormalizeRelativePathProviderException", navigationCmdletProvider.ProviderInfo, path, ex);
          }
        case ContainerCmdletProvider _:
label_6:
          return path;
        default:
          throw SessionStateInternal.tracer.NewNotSupportedException();
      }
    }

    internal string MakePath(string parent, string child)
    {
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      return this.MakePath(parent, child, context);
    }

    internal string MakePath(string parent, string child, CmdletProviderContext context)
    {
      if (context == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (context));
      if (parent == null && child == null)
        throw SessionStateInternal.tracer.NewArgumentException(nameof (parent));
      ProviderInfo provider = this.CurrentDrive.Provider;
      string path;
      if (context.Drive == (PSDriveInfo) null)
      {
        bool flag1 = LocationGlobber.IsProviderQualifiedPath(parent);
        bool flag2 = LocationGlobber.IsAbsolutePath(parent);
        if (flag1 || flag2)
        {
          PSDriveInfo drive = (PSDriveInfo) null;
          this.Globber.GetProviderPath(parent, context, out provider, out drive);
          if (drive == (PSDriveInfo) null && flag1)
            drive = provider.HiddenDrive;
          context.Drive = drive;
        }
        else
          context.Drive = this.CurrentDrive;
        path = this.MakePath(provider, parent, child, context);
        if (flag2)
          path = LocationGlobber.GetDriveQualifiedPath(path, context.Drive);
        else if (flag1)
          path = LocationGlobber.GetProviderQualifiedPath(path, provider);
      }
      else
        path = this.MakePath(context.Drive.Provider, parent, child, context);
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) path);
      return path;
    }

    internal string MakePath(
      ProviderInfo provider,
      string parent,
      string child,
      CmdletProviderContext context)
    {
      return this.MakePath(provider.CreateInstance(), parent, child, context);
    }

    internal string MakePath(
      CmdletProvider providerInstance,
      string parent,
      string child,
      CmdletProviderContext context)
    {
      switch (providerInstance)
      {
        case NavigationCmdletProvider navigationCmdletProvider:
          try
          {
            return navigationCmdletProvider.MakePath(parent, child, context);
          }
          catch (PipelineStoppedException ex)
          {
            throw;
          }
          catch (ActionPreferenceStopException ex)
          {
            throw;
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            SessionStateInternal.tracer.TraceException(ex);
            throw this.NewProviderInvocationException("MakePathProviderException", navigationCmdletProvider.ProviderInfo, parent, ex);
          }
        case ContainerCmdletProvider _:
          return child;
        default:
          throw SessionStateInternal.tracer.NewNotSupportedException();
      }
    }

    internal string GetChildName(string path)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      string childName = this.GetChildName(path, context);
      context.ThrowFirstErrorOrDoNothing();
      SessionStateInternal.tracer.WriteLine("result = {0}", (object) childName);
      return childName;
    }

    internal string GetChildName(string path, CmdletProviderContext context)
    {
      if (path == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (path));
      PSDriveInfo drive = (PSDriveInfo) null;
      ProviderInfo provider = (ProviderInfo) null;
      string providerPath = this.Globber.GetProviderPath(path, context, out provider, out drive);
      if (drive != (PSDriveInfo) null)
        context.Drive = drive;
      return this.GetChildName(provider, providerPath, context);
    }

    private string GetChildName(ProviderInfo provider, string path, CmdletProviderContext context) => this.GetChildName(provider.CreateInstance(), path, context);

    private string GetChildName(
      CmdletProvider providerInstance,
      string path,
      CmdletProviderContext context)
    {
      NavigationCmdletProvider providerInstance1 = SessionStateInternal.GetNavigationProviderInstance(providerInstance);
      try
      {
        return providerInstance1.GetChildName(path, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetChildNameProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> MoveItem(
      string[] paths,
      string destination,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.MoveItem(paths, destination, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void MoveItem(string[] paths, string destination, CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (destination == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (destination));
      ProviderInfo provider1 = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      Collection<PathInfo> pathsFromMonadPath1 = this.Globber.GetGlobbedMonadPathsFromMonadPath(destination, true, context, out providerInstance);
      if (pathsFromMonadPath1.Count > 1)
      {
        ArgumentException argumentException = (ArgumentException) SessionStateInternal.tracer.NewArgumentException(nameof (destination), "SessionStateStrings", "MoveItemOneDestination");
        SessionStateInternal.tracer.TraceException((Exception) argumentException);
        context.WriteError(new ErrorRecord((Exception) argumentException, argumentException.GetType().FullName, ErrorCategory.InvalidArgument, (object) destination));
      }
      else
      {
        foreach (string path1 in paths)
        {
          if (path1 == null)
            throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
          Collection<string> pathsFromMonadPath2 = this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider1, out providerInstance);
          if (pathsFromMonadPath2.Count > 1 && pathsFromMonadPath1.Count > 0 && !this.IsItemContainer(pathsFromMonadPath1[0].Path))
          {
            ArgumentException argumentException = (ArgumentException) SessionStateInternal.tracer.NewArgumentException("path", "SessionStateStrings", "MoveItemPathMultipleDestinationNotContainer");
            SessionStateInternal.tracer.TraceException((Exception) argumentException);
            context.WriteError(new ErrorRecord((Exception) argumentException, argumentException.GetType().FullName, ErrorCategory.InvalidArgument, (object) pathsFromMonadPath1[0]));
          }
          else
          {
            PSDriveInfo drive = (PSDriveInfo) null;
            ProviderInfo provider2 = (ProviderInfo) null;
            CmdletProviderContext context1 = new CmdletProviderContext(this.ExecutionContext);
            string destination1 = pathsFromMonadPath1.Count <= 0 ? this.Globber.GetProviderPath(destination, context1, out provider2, out drive) : this.Globber.GetProviderPath(pathsFromMonadPath1[0].Path, context1, out provider2, out drive);
            if (!string.Equals(provider1.FullName, provider2.FullName, StringComparison.OrdinalIgnoreCase))
            {
              ArgumentException argumentException = (ArgumentException) SessionStateInternal.tracer.NewArgumentException(nameof (destination), "SessionStateStrings", "MoveItemSourceAndDestinationNotSameProvider");
              SessionStateInternal.tracer.TraceException((Exception) argumentException);
              context.WriteError(new ErrorRecord((Exception) argumentException, argumentException.GetType().FullName, ErrorCategory.InvalidArgument, (object) pathsFromMonadPath2));
            }
            else
            {
              foreach (string path2 in pathsFromMonadPath2)
                this.MoveItemPrivate(providerInstance, path2, destination1, context);
            }
          }
        }
      }
    }

    private void MoveItemPrivate(
      CmdletProvider providerInstance,
      string path,
      string destination,
      CmdletProviderContext context)
    {
      NavigationCmdletProvider providerInstance1 = SessionStateInternal.GetNavigationProviderInstance(providerInstance);
      try
      {
        providerInstance1.MoveItem(path, destination, context);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("MoveItemProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal object MoveItemDynamicParameters(
      string path,
      string destination,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.MoveItemDynamicParameters(providerInstance, pathsFromMonadPath[0], destination, context1) : (object) null;
    }

    private object MoveItemDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      string destination,
      CmdletProviderContext context)
    {
      NavigationCmdletProvider providerInstance1 = SessionStateInternal.GetNavigationProviderInstance(providerInstance);
      try
      {
        return providerInstance1.MoveItemDynamicParameters(path, destination, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("MoveItemDynamicParametersProviderException", providerInstance1.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> GetProperty(
      string[] paths,
      Collection<string> providerSpecificPickList,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException("path");
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.SuppressWildcardExpansion = literalPath;
      this.GetProperty(paths, providerSpecificPickList, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void GetProperty(
      string[] paths,
      Collection<string> providerSpecificPickList,
      CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        foreach (string path2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider, out providerInstance))
          this.GetPropertyPrivate(providerInstance, path2, providerSpecificPickList, context);
      }
    }

    private void GetPropertyPrivate(
      CmdletProvider providerInstance,
      string path,
      Collection<string> providerSpecificPickList,
      CmdletProviderContext context)
    {
      try
      {
        providerInstance.GetProperty(path, providerSpecificPickList, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetPropertyProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal object GetPropertyDynamicParameters(
      string path,
      Collection<string> providerSpecificPickList,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.GetPropertyDynamicParameters(providerInstance, pathsFromMonadPath[0], providerSpecificPickList, context1) : (object) null;
    }

    private object GetPropertyDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      Collection<string> providerSpecificPickList,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.GetPropertyDynamicParameters(path, providerSpecificPickList, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("GetPropertyDynamicParametersProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal Collection<PSObject> SetProperty(
      string[] paths,
      PSObject property,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (property == null)
        throw SessionStateInternal.tracer.NewArgumentNullException("properties");
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.SetProperty(paths, property, context);
      context.ThrowFirstErrorOrDoNothing();
      return context.GetAccumulatedObjects();
    }

    internal void SetProperty(string[] paths, PSObject property, CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (property == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (property));
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider, out providerInstance);
        if (pathsFromMonadPath != null)
        {
          foreach (string path2 in pathsFromMonadPath)
            this.SetPropertyPrivate(providerInstance, path2, property, context);
        }
      }
    }

    private void SetPropertyPrivate(
      CmdletProvider providerInstance,
      string path,
      PSObject property,
      CmdletProviderContext context)
    {
      try
      {
        providerInstance.SetProperty(path, property, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("SetPropertyProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal object SetPropertyDynamicParameters(
      string path,
      PSObject propertyValue,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.SetPropertyDynamicParameters(providerInstance, pathsFromMonadPath[0], propertyValue, context1) : (object) null;
    }

    private object SetPropertyDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      PSObject propertyValue,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.SetPropertyDynamicParameters(path, propertyValue, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("SetPropertyDynamicParametersProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal void ClearProperty(
      string[] paths,
      Collection<string> propertyToClear,
      bool force,
      bool literalPath)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (propertyToClear == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (propertyToClear));
      CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
      context.Force = (SwitchParameter) force;
      context.SuppressWildcardExpansion = literalPath;
      this.ClearProperty(paths, propertyToClear, context);
      context.ThrowFirstErrorOrDoNothing();
    }

    internal void ClearProperty(
      string[] paths,
      Collection<string> propertyToClear,
      CmdletProviderContext context)
    {
      if (paths == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
      if (propertyToClear == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (propertyToClear));
      foreach (string path1 in paths)
      {
        if (path1 == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (paths));
        ProviderInfo provider = (ProviderInfo) null;
        CmdletProvider providerInstance = (CmdletProvider) null;
        foreach (string path2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(path1, false, context, out provider, out providerInstance))
          this.ClearPropertyPrivate(providerInstance, path2, propertyToClear, context);
      }
    }

    private void ClearPropertyPrivate(
      CmdletProvider providerInstance,
      string path,
      Collection<string> propertyToClear,
      CmdletProviderContext context)
    {
      try
      {
        providerInstance.ClearProperty(path, propertyToClear, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("ClearPropertyProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal object ClearPropertyDynamicParameters(
      string path,
      Collection<string> propertyToClear,
      CmdletProviderContext context)
    {
      if (path == null)
        return (object) null;
      ProviderInfo provider = (ProviderInfo) null;
      CmdletProvider providerInstance = (CmdletProvider) null;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), (string) null);
      Collection<string> pathsFromMonadPath = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context1, out provider, out providerInstance);
      return pathsFromMonadPath.Count > 0 ? this.ClearPropertyDynamicParameters(providerInstance, pathsFromMonadPath[0], propertyToClear, context1) : (object) null;
    }

    private object ClearPropertyDynamicParameters(
      CmdletProvider providerInstance,
      string path,
      Collection<string> propertyToClear,
      CmdletProviderContext context)
    {
      try
      {
        return providerInstance.ClearPropertyDynamicParameters(path, propertyToClear, context);
      }
      catch (NotSupportedException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        SessionStateInternal.tracer.TraceException(ex);
        throw this.NewProviderInvocationException("ClearPropertyDynamicParametersProviderException", providerInstance.ProviderInfo, path, ex);
      }
    }

    internal Dictionary<string, List<ProviderInfo>> Providers => this == this._context.TopLevelSessionState ? this._providers : this._context.TopLevelSessionState.Providers;

    internal Dictionary<ProviderInfo, PSDriveInfo> ProvidersCurrentWorkingDrive => this == this._context.TopLevelSessionState ? this._providersCurrentWorkingDrive : this._context.TopLevelSessionState.ProvidersCurrentWorkingDrive;

    internal void UpdateProviders()
    {
      if (this.ExecutionContext.RunspaceConfiguration == null)
        throw SessionStateInternal.tracer.NewInvalidOperationException();
      if (this == this._context.TopLevelSessionState && !this._providersInitialized)
      {
        foreach (ProviderConfigurationEntry provider in (IEnumerable<ProviderConfigurationEntry>) this.ExecutionContext.RunspaceConfiguration.Providers)
          this.AddProvider(provider);
        this._providersInitialized = true;
      }
      else
      {
        foreach (ProviderConfigurationEntry update in this.ExecutionContext.RunspaceConfiguration.Providers.UpdateList)
        {
          switch (update.Action)
          {
            case UpdateAction.Add:
              this.AddProvider(update);
              continue;
            case UpdateAction.Remove:
              this.RemoveProvider(update);
              continue;
            default:
              continue;
          }
        }
      }
    }

    internal void AddSessionStateEntry(SessionStateProviderEntry providerEntry) => this.AddProvider(providerEntry.ImplementingType, providerEntry.Name, providerEntry.HelpFileName, providerEntry.PSSnapIn, providerEntry.Module);

    private ProviderInfo AddProvider(ProviderConfigurationEntry providerConfig) => this.AddProvider(providerConfig.ImplementingType, providerConfig.Name, providerConfig.HelpFileName, providerConfig.PSSnapIn, (PSModuleInfo) null);

    private ProviderInfo AddProvider(
      Type implementingType,
      string name,
      string helpFileName,
      PSSnapInInfo psSnapIn,
      PSModuleInfo module)
    {
      ProviderInfo provider = (ProviderInfo) null;
      try
      {
        provider = new ProviderInfo(new SessionState(this), implementingType, name, helpFileName, psSnapIn);
        provider.SetModule(module);
        this.NewProvider(provider);
        MshLog.LogProviderLifecycleEvent(this.ExecutionContext, provider.Name, ProviderState.Started);
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (ActionPreferenceStopException ex)
      {
        throw;
      }
      catch (SessionStateException ex)
      {
        if (ex.GetType() == typeof (SessionStateException))
          throw;
        else
          this.ExecutionContext.ReportEngineStartupError((Exception) ex);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        this.ExecutionContext.ReportEngineStartupError(ex);
        SessionStateInternal.tracer.TraceException(ex);
      }
      return provider;
    }

    private PSDriveInfo ValidateDriveWithProvider(
      PSDriveInfo drive,
      CmdletProviderContext context,
      bool resolvePathIfPossible)
    {
      using (SessionStateInternal.tracer.TraceMethod((object) drive.Provider))
        return this.ValidateDriveWithProvider(this.GetDriveProviderInstance(drive.Provider), drive, context, resolvePathIfPossible);
    }

    private PSDriveInfo ValidateDriveWithProvider(
      DriveCmdletProvider driveProvider,
      PSDriveInfo drive,
      CmdletProviderContext context,
      bool resolvePathIfPossible)
    {
      using (SessionStateInternal.tracer.TraceMethod(drive.Name, new object[0]))
      {
        drive.DriveBeingCreated = true;
        if (this.CurrentDrive != (PSDriveInfo) null && resolvePathIfPossible)
        {
          string fromSpecifiedRoot = this.GetProviderRootFromSpecifiedRoot(drive.Root, drive.Provider);
          if (fromSpecifiedRoot != null)
            drive.SetRoot(fromSpecifiedRoot);
        }
        PSDriveInfo psDriveInfo = (PSDriveInfo) null;
        try
        {
          psDriveInfo = driveProvider.NewDrive(drive, context);
        }
        catch (PipelineStoppedException ex)
        {
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          ProviderInvocationException invocationException = this.NewProviderInvocationException("NewDriveProviderException", driveProvider.ProviderInfo, drive.Root, ex);
          context.WriteError(new ErrorRecord(invocationException.ErrorRecord, (Exception) invocationException));
        }
        finally
        {
          drive.DriveBeingCreated = false;
        }
        return psDriveInfo;
      }
    }

    internal CmdletProvider GetProviderInstance(string providerId)
    {
      using (SessionStateInternal.tracer.TraceMethod(providerId, new object[0]))
        return providerId != null ? this.GetProviderInstance(this.GetSingleProvider(providerId)) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (providerId));
    }

    internal CmdletProvider GetProviderInstance(ProviderInfo provider)
    {
      using (SessionStateInternal.tracer.TraceMethod((object) provider))
        return provider != null ? provider.CreateInstance() : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (provider));
    }

    internal static ProviderNameAmbiguousException NewAmbiguousProviderName(
      string name,
      Collection<ProviderInfo> matchingProviders)
    {
      using (SessionStateInternal.tracer.TraceMethod(name, new object[0]))
      {
        string possibleMatches = SessionStateInternal.GetPossibleMatches(matchingProviders);
        ProviderNameAmbiguousException ambiguousException = new ProviderNameAmbiguousException(name, "ProviderNameAmbiguous", matchingProviders, new object[1]
        {
          (object) possibleMatches
        });
        SessionStateInternal.tracer.TraceException((Exception) ambiguousException);
        return ambiguousException;
      }
    }

    private static string GetPossibleMatches(Collection<ProviderInfo> matchingProviders)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (ProviderInfo matchingProvider in matchingProviders)
        stringBuilder.AppendFormat(" {0}", (object) matchingProvider.FullName);
      return stringBuilder.ToString();
    }

    internal DriveCmdletProvider GetDriveProviderInstance(string providerId)
    {
      using (SessionStateInternal.tracer.TraceMethod(providerId, new object[0]))
      {
        if (providerId == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (providerId));
        return this.GetProviderInstance(providerId) is DriveCmdletProvider providerInstance ? providerInstance : throw SessionStateInternal.tracer.NewNotSupportedException("SessionStateStrings", "DriveCmdletProvider_NotSupported");
      }
    }

    internal DriveCmdletProvider GetDriveProviderInstance(ProviderInfo provider)
    {
      using (SessionStateInternal.tracer.TraceMethod((object) provider))
      {
        if (provider == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (provider));
        return this.GetProviderInstance(provider) is DriveCmdletProvider providerInstance ? providerInstance : throw SessionStateInternal.tracer.NewNotSupportedException("SessionStateStrings", "DriveCmdletProvider_NotSupported");
      }
    }

    private static DriveCmdletProvider GetDriveProviderInstance(
      CmdletProvider providerInstance)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (providerInstance == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (providerInstance));
        return providerInstance is DriveCmdletProvider driveCmdletProvider ? driveCmdletProvider : throw SessionStateInternal.tracer.NewNotSupportedException("SessionStateStrings", "DriveCmdletProvider_NotSupported");
      }
    }

    internal ItemCmdletProvider GetItemProviderInstance(string providerId)
    {
      using (SessionStateInternal.tracer.TraceMethod(providerId, new object[0]))
      {
        if (providerId == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (providerId));
        return this.GetProviderInstance(providerId) is ItemCmdletProvider providerInstance ? providerInstance : throw SessionStateInternal.tracer.NewNotSupportedException("SessionStateStrings", "ItemCmdletProvider_NotSupported");
      }
    }

    internal ItemCmdletProvider GetItemProviderInstance(ProviderInfo provider)
    {
      using (SessionStateInternal.tracer.TraceMethod((object) provider))
      {
        if (provider == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (provider));
        return this.GetProviderInstance(provider) is ItemCmdletProvider providerInstance ? providerInstance : throw SessionStateInternal.tracer.NewNotSupportedException("SessionStateStrings", "ItemCmdletProvider_NotSupported");
      }
    }

    private static ItemCmdletProvider GetItemProviderInstance(
      CmdletProvider providerInstance)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (providerInstance == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (providerInstance));
        return providerInstance is ItemCmdletProvider itemCmdletProvider ? itemCmdletProvider : throw SessionStateInternal.tracer.NewNotSupportedException("SessionStateStrings", "ItemCmdletProvider_NotSupported");
      }
    }

    internal ContainerCmdletProvider GetContainerProviderInstance(
      string providerId)
    {
      using (SessionStateInternal.tracer.TraceMethod(providerId, new object[0]))
      {
        if (providerId == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (providerId));
        return this.GetProviderInstance(providerId) is ContainerCmdletProvider providerInstance ? providerInstance : throw SessionStateInternal.tracer.NewNotSupportedException("SessionStateStrings", "ContainerCmdletProvider_NotSupported");
      }
    }

    internal ContainerCmdletProvider GetContainerProviderInstance(
      ProviderInfo provider)
    {
      using (SessionStateInternal.tracer.TraceMethod((object) provider))
      {
        if (provider == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (provider));
        return this.GetProviderInstance(provider) is ContainerCmdletProvider providerInstance ? providerInstance : throw SessionStateInternal.tracer.NewNotSupportedException("SessionStateStrings", "ContainerCmdletProvider_NotSupported");
      }
    }

    private static ContainerCmdletProvider GetContainerProviderInstance(
      CmdletProvider providerInstance)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (providerInstance == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (providerInstance));
        return providerInstance is ContainerCmdletProvider containerCmdletProvider ? containerCmdletProvider : throw SessionStateInternal.tracer.NewNotSupportedException("SessionStateStrings", "ContainerCmdletProvider_NotSupported");
      }
    }

    internal NavigationCmdletProvider GetNavigationProviderInstance(
      ProviderInfo provider)
    {
      using (SessionStateInternal.tracer.TraceMethod((object) provider))
      {
        if (provider == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (provider));
        return this.GetProviderInstance(provider) is NavigationCmdletProvider providerInstance ? providerInstance : throw SessionStateInternal.tracer.NewNotSupportedException("SessionStateStrings", "NavigationCmdletProvider_NotSupported");
      }
    }

    private static NavigationCmdletProvider GetNavigationProviderInstance(
      CmdletProvider providerInstance)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (providerInstance == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (providerInstance));
        return providerInstance is NavigationCmdletProvider navigationCmdletProvider ? navigationCmdletProvider : throw SessionStateInternal.tracer.NewNotSupportedException("SessionStateStrings", "NavigationCmdletProvider_NotSupported");
      }
    }

    internal bool IsProviderLoaded(string name)
    {
      using (SessionStateInternal.tracer.TraceMethod(name, new object[0]))
      {
        bool flag = false;
        if (string.IsNullOrEmpty(name))
          throw SessionStateInternal.tracer.NewArgumentException(nameof (name));
        try
        {
          flag = this.GetSingleProvider(name) != null;
        }
        catch (ProviderNotFoundException ex)
        {
        }
        SessionStateInternal.tracer.WriteLine("result = {0}", (object) flag);
        return flag;
      }
    }

    internal Collection<ProviderInfo> GetProvider(string name)
    {
      using (SessionStateInternal.tracer.TraceMethod(name, new object[0]))
      {
        PSSnapinQualifiedName providerName = !string.IsNullOrEmpty(name) ? PSSnapinQualifiedName.GetInstance(name) : throw SessionStateInternal.tracer.NewArgumentException(nameof (name));
        if (providerName == null)
        {
          ProviderNotFoundException notFoundException = new ProviderNotFoundException(name, SessionStateCategory.CmdletProvider, "ProviderNotFoundBadFormat", new object[0]);
          SessionStateInternal.tracer.TraceException((Exception) notFoundException);
          throw notFoundException;
        }
        return this.GetProvider(providerName);
      }
    }

    internal ProviderInfo GetSingleProvider(string name)
    {
      using (SessionStateInternal.tracer.TraceMethod(name, new object[0]))
      {
        Collection<ProviderInfo> provider = this.GetProvider(name);
        if (provider.Count == 1)
          return provider[0];
        if (provider.Count == 0)
        {
          ProviderNotFoundException notFoundException = new ProviderNotFoundException(name, SessionStateCategory.CmdletProvider, "ProviderNotFound", new object[0]);
          SessionStateInternal.tracer.TraceException((Exception) notFoundException);
          throw notFoundException;
        }
        throw SessionStateInternal.NewAmbiguousProviderName(name, provider);
      }
    }

    internal Collection<ProviderInfo> GetProvider(
      PSSnapinQualifiedName providerName)
    {
      using (SessionStateInternal.tracer.TraceMethod((object) providerName))
      {
        Collection<ProviderInfo> collection = new Collection<ProviderInfo>();
        if (providerName == null)
        {
          ProviderNotFoundException notFoundException = new ProviderNotFoundException(providerName.ToString(), SessionStateCategory.CmdletProvider, "ProviderNotFound", new object[0]);
          SessionStateInternal.tracer.TraceException((Exception) notFoundException);
          throw notFoundException;
        }
        List<ProviderInfo> providerInfoList = (List<ProviderInfo>) null;
        if (!this.Providers.TryGetValue(providerName.ShortName, out providerInfoList))
        {
          ProviderNotFoundException notFoundException = new ProviderNotFoundException(providerName.ToString(), SessionStateCategory.CmdletProvider, "ProviderNotFound", new object[0]);
          SessionStateInternal.tracer.TraceException((Exception) notFoundException);
          throw notFoundException;
        }
        if (this.ExecutionContext.IsSingleShell && !string.IsNullOrEmpty(providerName.PSSnapInName))
        {
          foreach (ProviderInfo providerInfo in providerInfoList)
          {
            if (string.Equals(providerInfo.PSSnapInName, providerName.PSSnapInName, StringComparison.OrdinalIgnoreCase))
              collection.Add(providerInfo);
          }
        }
        else
        {
          foreach (ProviderInfo providerInfo in providerInfoList)
            collection.Add(providerInfo);
        }
        return collection;
      }
    }

    internal IEnumerable<ProviderInfo> ProviderList
    {
      get
      {
        using (SessionStateInternal.tracer.TraceProperty())
        {
          Collection<ProviderInfo> collection = new Collection<ProviderInfo>();
          foreach (List<ProviderInfo> providerInfoList in this.Providers.Values)
          {
            foreach (ProviderInfo providerInfo in providerInfoList)
              collection.Add(providerInfo);
          }
          return (IEnumerable<ProviderInfo>) collection;
        }
      }
    }

    internal void CopyProviders(SessionStateInternal ss)
    {
      if (ss == null || ss.Providers == null)
        return;
      this._providers = new Dictionary<string, List<ProviderInfo>>();
      foreach (KeyValuePair<string, List<ProviderInfo>> provider in ss._providers)
        this._providers.Add(provider.Key, provider.Value);
    }

    internal void InitializeProvider(
      CmdletProvider providerInstance,
      ProviderInfo provider,
      CmdletProviderContext context)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        if (provider == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (provider));
        if (context == null)
          context = new CmdletProviderContext(this.ExecutionContext);
        List<PSDriveInfo> psDriveInfoList = new List<PSDriveInfo>();
        DriveCmdletProvider providerInstance1 = SessionStateInternal.GetDriveProviderInstance(providerInstance);
        if (providerInstance1 != null)
        {
          try
          {
            Collection<PSDriveInfo> collection = providerInstance1.InitializeDefaultDrives(context);
            if (collection != null)
            {
              if (collection.Count > 0)
              {
                psDriveInfoList.AddRange((IEnumerable<PSDriveInfo>) collection);
                this.ProvidersCurrentWorkingDrive[provider] = collection[0];
              }
            }
          }
          catch (PipelineStoppedException ex)
          {
            throw;
          }
          catch (ActionPreferenceStopException ex)
          {
            throw;
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            SessionStateInternal.tracer.TraceException(ex);
            ProviderInvocationException invocationException = this.NewProviderInvocationException("InitializeDefaultDrivesException", provider, string.Empty, ex);
            context.WriteError(new ErrorRecord((Exception) invocationException, "InitializeDefaultDrivesException", ErrorCategory.InvalidOperation, (object) provider));
          }
        }
        if (psDriveInfoList == null || psDriveInfoList.Count <= 0)
          return;
        foreach (PSDriveInfo drive in psDriveInfoList)
        {
          if (!(drive == (PSDriveInfo) null))
          {
            if (provider.NameEquals(drive.Provider.FullName))
            {
              try
              {
                PSDriveInfo newDrive = this.ValidateDriveWithProvider(providerInstance1, drive, context, false);
                if (newDrive != (PSDriveInfo) null)
                  this._globalScope.NewDrive(newDrive);
              }
              catch (SessionStateException ex)
              {
                SessionStateInternal.tracer.TraceException((Exception) ex);
                context.WriteError(ex.ErrorRecord);
              }
            }
          }
        }
      }
    }

    internal ProviderInfo NewProvider(ProviderInfo provider)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        ProviderInfo providerInfo = provider != null ? this.ProviderExists(provider) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (provider));
        if (providerInfo != null)
        {
          if (providerInfo.ImplementingType == provider.ImplementingType)
            return providerInfo;
          SessionStateException sessionStateException = new SessionStateException(provider.Name, SessionStateCategory.CmdletProvider, "CmdletProviderAlreadyExists", ErrorCategory.ResourceExists, new object[0]);
          SessionStateInternal.tracer.TraceException((Exception) sessionStateException);
          throw sessionStateException;
        }
        CmdletProvider instance = provider.CreateInstance();
        CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(this.ExecutionContext);
        ProviderInfo providerInfoToSet;
        try
        {
          providerInfoToSet = instance.Start(provider, cmdletProviderContext);
          instance.SetProviderInformation(providerInfoToSet);
        }
        catch (PipelineStoppedException ex)
        {
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          throw;
        }
        catch (InvalidOperationException ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          throw this.NewProviderInvocationException("ProviderStartException", provider, (string) null, ex);
        }
        cmdletProviderContext.ThrowFirstErrorOrDoNothing(true);
        if (providerInfoToSet == null)
          throw SessionStateInternal.tracer.NewInvalidOperationException("SessionStateStrings", "InvalidProviderInfoNull");
        if (providerInfoToSet != provider)
        {
          if (!string.Equals(providerInfoToSet.Name, provider.Name, StringComparison.OrdinalIgnoreCase))
            throw SessionStateInternal.tracer.NewInvalidOperationException("SessionStateStrings", "InvalidProviderInfo");
          provider = providerInfoToSet;
        }
        try
        {
          this.NewProviderEntry(provider);
        }
        catch (ArgumentException ex)
        {
          SessionStateInternal.tracer.TraceException((Exception) ex);
          SessionStateException sessionStateException = new SessionStateException(provider.Name, SessionStateCategory.CmdletProvider, "ProviderAlreadyExists", ErrorCategory.ResourceExists, new object[0]);
          SessionStateInternal.tracer.TraceException((Exception) sessionStateException);
          throw sessionStateException;
        }
        catch (NotSupportedException ex)
        {
          SessionStateInternal.tracer.TraceException((Exception) ex);
          throw;
        }
        this.ProvidersCurrentWorkingDrive.Add(provider, (PSDriveInfo) null);
        bool flag = false;
        try
        {
          this.InitializeProvider(instance, provider, cmdletProviderContext);
          cmdletProviderContext.ThrowFirstErrorOrDoNothing(true);
        }
        catch (PipelineStoppedException ex)
        {
          flag = true;
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          flag = true;
          throw;
        }
        catch (NotSupportedException ex)
        {
          flag = false;
        }
        catch (SessionStateException ex)
        {
          flag = true;
          throw;
        }
        finally
        {
          if (flag)
          {
            this.Providers.Remove(provider.Name.ToString());
            this.ProvidersCurrentWorkingDrive.Remove(provider);
            provider = (ProviderInfo) null;
          }
        }
        return provider;
      }
    }

    private ProviderInfo ProviderExists(ProviderInfo provider)
    {
      List<ProviderInfo> providerInfoList = (List<ProviderInfo>) null;
      if (this.Providers.TryGetValue(provider.Name, out providerInfoList))
      {
        foreach (ProviderInfo providerInfo in providerInfoList)
        {
          if (provider.NameEquals(providerInfo.FullName))
            return providerInfo;
        }
      }
      return (ProviderInfo) null;
    }

    private void NewProviderEntry(ProviderInfo provider)
    {
      if (!this.Providers.ContainsKey(provider.Name))
      {
        this.Providers.Add(provider.Name, new List<ProviderInfo>());
      }
      else
      {
        foreach (ProviderInfo providerInfo in this.Providers[provider.Name])
        {
          if (string.Equals(providerInfo.PSSnapIn.Name, provider.PSSnapIn.Name, StringComparison.Ordinal))
          {
            SessionStateException sessionStateException = new SessionStateException(provider.Name, SessionStateCategory.CmdletProvider, "ProviderAlreadyExists", ErrorCategory.ResourceExists, new object[0]);
            SessionStateInternal.tracer.TraceException((Exception) sessionStateException);
            throw sessionStateException;
          }
        }
      }
      this.Providers[provider.Name].Add(provider);
    }

    private void RemoveProvider(ProviderConfigurationEntry entry)
    {
      using (SessionStateInternal.tracer.TraceMethod())
      {
        try
        {
          CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
          this.RemoveProvider(this.GetProviderName(entry), true, context);
          context.ThrowFirstErrorOrDoNothing();
        }
        catch (PipelineStoppedException ex)
        {
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          this.ExecutionContext.ReportEngineStartupError(ex);
          SessionStateInternal.tracer.TraceException(ex);
        }
      }
    }

    private string GetProviderName(ProviderConfigurationEntry entry)
    {
      string str = entry.Name;
      if (entry.PSSnapIn != null)
        str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}\\{1}", (object) entry.PSSnapIn.Name, (object) entry.Name);
      return str;
    }

    internal void RemoveProvider(string providerName, bool force, CmdletProviderContext context)
    {
      using (SessionStateInternal.tracer.TraceMethod(providerName, new object[0]))
      {
        if (context == null)
          throw SessionStateInternal.tracer.NewArgumentNullException(nameof (context));
        if (string.IsNullOrEmpty(providerName))
          throw SessionStateInternal.tracer.NewArgumentException(nameof (providerName));
        bool flag = false;
        ProviderInfo singleProvider;
        try
        {
          singleProvider = this.GetSingleProvider(providerName);
        }
        catch (ProviderNotFoundException ex)
        {
          return;
        }
        try
        {
          CmdletProvider providerInstance = this.GetProviderInstance(singleProvider);
          if (providerInstance == null)
          {
            ProviderNotFoundException notFoundException = new ProviderNotFoundException(providerName, SessionStateCategory.CmdletProvider, "ProviderNotFound", new object[0]);
            context.WriteError(new ErrorRecord(notFoundException.ErrorRecord, (Exception) notFoundException));
            flag = true;
          }
          else
          {
            int num = 0;
            foreach (PSDriveInfo psDriveInfo in this.GetDrivesForProvider(providerName))
            {
              if (psDriveInfo != (PSDriveInfo) null)
              {
                ++num;
                break;
              }
            }
            if (num > 0)
            {
              if (force)
              {
                foreach (PSDriveInfo drive in this.GetDrivesForProvider(providerName))
                {
                  if (drive != (PSDriveInfo) null)
                    this.RemoveDrive(drive, true, (string) null);
                }
              }
              else
              {
                flag = true;
                SessionStateException sessionStateException = new SessionStateException(providerName, SessionStateCategory.CmdletProvider, "RemoveDrivesBeforeRemovingProvider", ErrorCategory.InvalidOperation, new object[0]);
                context.WriteError(new ErrorRecord(sessionStateException.ErrorRecord, (Exception) sessionStateException));
                return;
              }
            }
            try
            {
              providerInstance.Stop(context);
            }
            catch (PipelineStoppedException ex)
            {
              throw;
            }
            catch (ActionPreferenceStopException ex)
            {
              throw;
            }
          }
        }
        catch (PipelineStoppedException ex)
        {
          throw;
        }
        catch (ActionPreferenceStopException ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          SessionStateInternal.tracer.TraceException(ex, true);
          flag = true;
          context.WriteError(new ErrorRecord(ex, "RemoveProviderUnexpectedException", ErrorCategory.InvalidArgument, (object) providerName));
        }
        finally
        {
          if (force || !flag)
          {
            MshLog.LogProviderLifecycleEvent(this.ExecutionContext, providerName, ProviderState.Stopped);
            this.RemoveProviderFromCollection(singleProvider);
            this.ProvidersCurrentWorkingDrive.Remove(singleProvider);
          }
        }
      }
    }

    private void RemoveProviderFromCollection(ProviderInfo provider)
    {
      if (!this.Providers.ContainsKey(provider.Name))
        return;
      List<ProviderInfo> provider1 = this.Providers[provider.Name];
      if (provider1.Count == 1 && provider1[0].NameEquals(provider.FullName))
        this.Providers.Remove(provider.Name);
      else
        provider1.Remove(provider);
    }

    internal int ProviderCount
    {
      get
      {
        int num = 0;
        foreach (List<ProviderInfo> providerInfoList in this.Providers.Values)
          num += providerInfoList.Count;
        return num;
      }
    }

    internal SessionStateScope GetScopeByID(string scopeID)
    {
      SessionStateScope sessionStateScope = this.currentScope;
      if (scopeID != null && scopeID.Length > 0)
      {
        if (string.Equals(scopeID, "GLOBAL", StringComparison.OrdinalIgnoreCase))
          sessionStateScope = this._globalScope;
        else if (string.Equals(scopeID, "LOCAL", StringComparison.OrdinalIgnoreCase))
          sessionStateScope = this.currentScope;
        else if (string.Equals(scopeID, "PRIVATE", StringComparison.OrdinalIgnoreCase))
          sessionStateScope = this.currentScope;
        else if (string.Equals(scopeID, "SCRIPT", StringComparison.OrdinalIgnoreCase))
        {
          sessionStateScope = this.currentScope.ScriptScope;
        }
        else
        {
          try
          {
            int scopeID1 = int.Parse(scopeID, (IFormatProvider) Thread.CurrentThread.CurrentCulture);
            if (scopeID1 < 0)
              throw SessionStateInternal.tracer.NewArgumentOutOfRangeException(nameof (scopeID), (object) scopeID);
            sessionStateScope = this.GetScopeByID(scopeID1) ?? this.currentScope;
          }
          catch (FormatException ex)
          {
            throw SessionStateInternal.tracer.NewArgumentException(nameof (scopeID));
          }
          catch (OverflowException ex)
          {
            throw SessionStateInternal.tracer.NewArgumentOutOfRangeException(nameof (scopeID), (object) scopeID);
          }
        }
      }
      return sessionStateScope;
    }

    internal SessionStateScope GetScopeByID(int scopeID)
    {
      SessionStateScope sessionStateScope = this.currentScope;
      int num = scopeID;
      for (; scopeID > 0 && sessionStateScope != null; --scopeID)
        sessionStateScope = sessionStateScope.Parent;
      if (sessionStateScope == null && scopeID >= 0)
      {
        ArgumentOutOfRangeException ofRangeException = (ArgumentOutOfRangeException) SessionStateInternal.tracer.NewArgumentOutOfRangeException(nameof (scopeID), (object) num, "SessionStateStrings", "ScopeIDExceedsAvailableScopes", (object) num);
        SessionStateInternal.tracer.TraceException((Exception) ofRangeException);
        throw ofRangeException;
      }
      return sessionStateScope;
    }

    internal ActivationRecord CurrentActivationRecord
    {
      get => this.currentActiviationRecord;
      set => this.currentActiviationRecord = value;
    }

    internal SessionStateScope GlobalScope
    {
      get => this._globalScope;
      set => this._globalScope = value;
    }

    internal SessionStateScope ModuleScope => this._moduleScope;

    internal SessionStateScope CurrentScope
    {
      get => this.currentScope;
      set => this.currentScope = value;
    }

    internal SessionStateScope ScriptScope => this.currentScope.ScriptScope;

    internal SessionStateScope NewScope() => this.NewScope(false);

    internal SessionStateScope NewScope(bool isScriptScope)
    {
      SessionStateScope sessionStateScope = new SessionStateScope(this.currentScope);
      if (isScriptScope)
        sessionStateScope.ScriptScope = sessionStateScope;
      return sessionStateScope;
    }

    internal void RemoveScope(SessionStateScope scope)
    {
      if (scope == this._globalScope)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException("GLOBAL", SessionStateCategory.Scope, "GlobalScopeCannotRemove");
        SessionStateInternal.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      if (scope.Children.Count > 0)
      {
        for (int index = scope.Children.Count - 1; index >= 0; --index)
          this.RemoveScope(scope.Children[index]);
      }
      foreach (PSDriveInfo drive in scope.Drives)
      {
        if (!(drive == (PSDriveInfo) null))
        {
          CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
          try
          {
            this.CanRemoveDrive(drive, context);
          }
          catch (PipelineStoppedException ex)
          {
            throw;
          }
          catch (ActionPreferenceStopException ex)
          {
            throw;
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            SessionStateInternal.tracer.TraceException(ex, true);
          }
        }
      }
      scope.RemoveAllDrives();
      scope.CloseRunspaces();
      if (scope.Parent != null)
        scope.Parent.Children.Remove(scope);
      if (scope == this.currentScope && this.currentScope.Parent != null)
        this.currentScope = this.currentScope.Parent;
      scope.Parent = (SessionStateScope) null;
    }

    internal void AddSessionStateEntry(SessionStateVariableEntry entry) => this.SetVariableAtScope(new PSVariable(entry.Name, entry.Value, entry.Options, entry.Attributes, entry.Description)
    {
      Visibility = entry.Visibility
    }, "global", true, CommandOrigin.Internal);

    internal PSVariable GetVariable(string name, CommandOrigin origin)
    {
      ScopedItemLookupPath variablePath = name != null ? new ScopedItemLookupPath(name) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (name));
      SessionStateScope scope = (SessionStateScope) null;
      return this.GetVariableItem(variablePath, out scope, origin);
    }

    internal PSVariable GetVariable(string name) => this.GetVariable(name, CommandOrigin.Internal);

    internal object GetVariableValue(string name)
    {
      ScopedItemLookupPath variablePath = name != null ? new ScopedItemLookupPath(name) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (name));
      CmdletProviderContext context = (CmdletProviderContext) null;
      SessionStateScope scope = (SessionStateScope) null;
      return this.GetVariableValue(variablePath, out context, out scope);
    }

    internal object GetVariableValue(string name, object defaultValue) => this.GetVariableValue(name) ?? defaultValue;

    internal object GetVariableValue(
      ScopedItemLookupPath variablePath,
      out CmdletProviderContext context,
      out SessionStateScope scope)
    {
      context = (CmdletProviderContext) null;
      scope = (SessionStateScope) null;
      object obj = (object) null;
      if (variablePath.IsScopedItem)
      {
        PSVariable variableItem = this.GetVariableItem(variablePath, out scope);
        if (variableItem != null)
          obj = variableItem.Value;
      }
      else
        obj = this.GetVariableValueFromProvider(variablePath, out context, out scope, this.currentScope.ScopeOrigin);
      return obj;
    }

    internal object GetVariableValueFromProvider(
      ScopedItemLookupPath variablePath,
      out CmdletProviderContext context,
      out SessionStateScope scope,
      CommandOrigin origin)
    {
      scope = (SessionStateScope) null;
      if (variablePath == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (variablePath));
      context = (CmdletProviderContext) null;
      DriveScopeItemSearcher scopeItemSearcher = new DriveScopeItemSearcher(this, variablePath);
      object obj = (object) null;
      if (scopeItemSearcher.MoveNext())
      {
        PSDriveInfo current = ((IEnumerator<PSDriveInfo>) scopeItemSearcher).Current;
        if (!(current == (PSDriveInfo) null))
        {
          context = new CmdletProviderContext(this.ExecutionContext, origin);
          context.Drive = current;
          Collection<IContentReader> contentReader1;
          try
          {
            contentReader1 = this.GetContentReader(new string[1]
            {
              variablePath.LookupPath.ToString()
            }, context);
          }
          catch (ItemNotFoundException ex)
          {
            goto label_26;
          }
          catch (DriveNotFoundException ex)
          {
            goto label_26;
          }
          catch (ProviderNotFoundException ex)
          {
            goto label_26;
          }
          catch (NotImplementedException ex)
          {
            ProviderInfo provider = (ProviderInfo) null;
            this.Globber.GetProviderPath(variablePath.LookupPath.ToString(), out provider);
            throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", provider, variablePath.LookupPath.ToString(), (Exception) ex, false);
          }
          catch (NotSupportedException ex)
          {
            ProviderInfo provider = (ProviderInfo) null;
            this.Globber.GetProviderPath(variablePath.LookupPath.ToString(), out provider);
            throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", provider, variablePath.LookupPath.ToString(), (Exception) ex, false);
          }
          if (contentReader1 != null && contentReader1.Count != 0)
          {
            if (contentReader1.Count > 1)
            {
              foreach (IContentReader contentReader2 in contentReader1)
                contentReader2.Close();
              PSArgumentException argumentException = SessionStateInternal.tracer.NewArgumentException("path", "SessionStateStrings", "VariablePathResolvedToMultiple", (object) variablePath.ToString());
              ProviderInfo provider = (ProviderInfo) null;
              this.Globber.GetProviderPath(variablePath.LookupPath.ToString(), out provider);
              throw this.NewProviderInvocationException("ProviderVariableSyntaxInvalid", provider, variablePath.LookupPath.ToString(), (Exception) argumentException);
            }
            IContentReader contentReader3 = contentReader1[0];
            try
            {
              IList list = contentReader3.Read(-1L);
              if (list != null)
                obj = list.Count != 0 ? (list.Count != 1 ? (object) list : list[0]) : (object) null;
            }
            catch (Exception ex)
            {
              ProviderInfo provider = (ProviderInfo) null;
              this.Globber.GetProviderPath(variablePath.LookupPath.ToString(), out provider);
              CommandProcessorBase.CheckForSevereException(ex);
              ProviderInvocationException invocationException = new ProviderInvocationException("ProviderContentReadError", provider, variablePath.LookupPath.ToString(), ex);
              SessionStateInternal.tracer.TraceException((Exception) invocationException);
              throw invocationException;
            }
            finally
            {
              contentReader3.Close();
            }
          }
        }
      }
label_26:
      return obj;
    }

    internal PSVariable GetVariableItem(
      ScopedItemLookupPath variablePath,
      out SessionStateScope scope,
      CommandOrigin origin)
    {
      scope = (SessionStateScope) null;
      if (variablePath == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (variablePath));
      if (!variablePath.IsScopedItem)
        variablePath = new ScopedItemLookupPath(":" + variablePath.ToString());
      VariableScopeItemSearcher scopeItemSearcher = new VariableScopeItemSearcher(this, variablePath, origin);
      PSVariable psVariable = (PSVariable) null;
      if (scopeItemSearcher.MoveNext())
      {
        psVariable = ((IEnumerator<PSVariable>) scopeItemSearcher).Current;
        scope = scopeItemSearcher.CurrentLookupScope;
      }
      return psVariable;
    }

    internal PSVariable GetVariableItem(
      ScopedItemLookupPath variablePath,
      out SessionStateScope scope)
    {
      return this.GetVariableItem(variablePath, out scope, CommandOrigin.Internal);
    }

    internal PSVariable GetVariableAtScope(string name, string scopeID)
    {
      ScopedItemLookupPath scopedItemLookupPath = name != null ? new ScopedItemLookupPath(name) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (name));
      SessionStateScope scopeById = this.GetScopeByID(scopeID);
      PSVariable psVariable = (PSVariable) null;
      if (scopedItemLookupPath.IsScopedItem)
        psVariable = scopeById.GetVariable(scopedItemLookupPath.LookupPath.ToString());
      return psVariable;
    }

    internal object GetVariableValueAtScope(string name, string scopeID)
    {
      ScopedItemLookupPath scopedItemLookupPath = name != null ? new ScopedItemLookupPath(name) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (name));
      SessionStateScope scopeById = this.GetScopeByID(scopeID);
      object obj = (object) null;
      if (scopedItemLookupPath.IsScopedItem)
      {
        obj = (object) scopeById.GetVariable(scopedItemLookupPath.LookupPath.ToString());
      }
      else
      {
        PSDriveInfo drive = scopeById.GetDrive(scopedItemLookupPath.LookupPath.NamespaceID);
        if (drive != (PSDriveInfo) null)
        {
          CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
          context.Drive = drive;
          Collection<IContentReader> contentReader1;
          try
          {
            contentReader1 = this.GetContentReader(new string[1]
            {
              scopedItemLookupPath.LookupPath.ToString()
            }, context);
          }
          catch (ItemNotFoundException ex)
          {
            return (object) null;
          }
          catch (DriveNotFoundException ex)
          {
            return (object) null;
          }
          catch (ProviderNotFoundException ex)
          {
            return (object) null;
          }
          catch (NotImplementedException ex)
          {
            ProviderInfo provider = (ProviderInfo) null;
            this.Globber.GetProviderPath(scopedItemLookupPath.LookupPath.ToString(), out provider);
            throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", provider, scopedItemLookupPath.LookupPath.ToString(), (Exception) ex, false);
          }
          catch (NotSupportedException ex)
          {
            ProviderInfo provider = (ProviderInfo) null;
            this.Globber.GetProviderPath(scopedItemLookupPath.LookupPath.ToString(), out provider);
            throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", provider, scopedItemLookupPath.LookupPath.ToString(), (Exception) ex, false);
          }
          if (contentReader1 == null || contentReader1.Count == 0)
            return (object) null;
          if (contentReader1.Count > 1)
          {
            foreach (IContentReader contentReader2 in contentReader1)
              contentReader2.Close();
            PSArgumentException argumentException = SessionStateInternal.tracer.NewArgumentException("path", "SessionStateStrings", "VariablePathResolvedToMultiple", (object) name);
            ProviderInfo provider = (ProviderInfo) null;
            this.Globber.GetProviderPath(scopedItemLookupPath.LookupPath.ToString(), out provider);
            throw this.NewProviderInvocationException("ProviderVariableSyntaxInvalid", provider, scopedItemLookupPath.LookupPath.ToString(), (Exception) argumentException);
          }
          IContentReader contentReader3 = contentReader1[0];
          try
          {
            IList list = contentReader3.Read(-1L);
            if (list != null)
              obj = list.Count != 0 ? (list.Count != 1 ? (object) list : list[0]) : (object) null;
          }
          catch (Exception ex)
          {
            ProviderInfo provider = (ProviderInfo) null;
            this.Globber.GetProviderPath(scopedItemLookupPath.LookupPath.ToString(), out provider);
            CommandProcessorBase.CheckForSevereException(ex);
            ProviderInvocationException invocationException = new ProviderInvocationException("ProviderContentReadError", provider, scopedItemLookupPath.LookupPath.ToString(), ex);
            SessionStateInternal.tracer.TraceException((Exception) invocationException);
            throw invocationException;
          }
          finally
          {
            contentReader3.Close();
          }
        }
      }
      if (obj != null)
      {
        if (obj is PSVariable psVariable)
        {
          obj = psVariable.Value;
        }
        else
        {
          try
          {
            obj = ((DictionaryEntry) obj).Value;
          }
          catch (InvalidCastException ex)
          {
          }
        }
      }
      return obj;
    }

    internal void SetVariableValue(string name, object newValue, CommandOrigin origin)
    {
      if (name == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (name));
      this.SetVariable(new ScopedItemLookupPath(name), newValue, true, origin);
    }

    internal void SetVariableValue(string name, object newValue) => this.SetVariableValue(name, newValue, CommandOrigin.Internal);

    internal object SetVariable(PSVariable variable, bool force, CommandOrigin origin)
    {
      if (variable == null || string.IsNullOrEmpty(variable.Name))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (variable));
      return this.SetVariable(new ScopedItemLookupPath(variable.Name)
      {
        IsScopedItem = true
      }, (object) variable, false, force, origin);
    }

    internal object SetVariable(
      ScopedItemLookupPath variablePath,
      object newValue,
      bool asValue,
      CommandOrigin origin)
    {
      return this.SetVariable(variablePath, newValue, asValue, false, origin);
    }

    internal object SetVariable(
      ScopedItemLookupPath variablePath,
      object newValue,
      bool asValue,
      bool force,
      CommandOrigin origin)
    {
      object obj = (object) null;
      if (variablePath == null)
        throw SessionStateInternal.tracer.NewArgumentNullException(nameof (variablePath));
      CmdletProviderContext context1 = (CmdletProviderContext) null;
      SessionStateScope scope = (SessionStateScope) null;
      if (variablePath.IsScopedItem)
      {
        PSVariable psVariable = ((!variablePath.IsGlobal ? (!variablePath.IsLocal ? (!variablePath.IsPrivate ? (!variablePath.IsScript ? this.currentScope : this.currentScope.ScriptScope) : this.currentScope) : this.currentScope) : this._globalScope) ?? this.currentScope).SetVariable(variablePath.LookupPath.ToString(), newValue, asValue, force, this, origin);
        if (variablePath.IsPrivate && psVariable != null)
          psVariable.Options |= ScopedItemOptions.Private;
        obj = (object) psVariable;
      }
      else
      {
        this.GetVariableValue(variablePath, out context1, out scope);
        Collection<IContentWriter> contentWriter1;
        try
        {
          if (context1 != null)
          {
            try
            {
              CmdletProviderContext context2 = new CmdletProviderContext(context1);
              this.ClearContent(new string[1]
              {
                variablePath.LookupPath.ToString()
              }, context2);
            }
            catch (NotSupportedException ex)
            {
            }
            catch (ItemNotFoundException ex)
            {
            }
            contentWriter1 = this.GetContentWriter(new string[1]
            {
              variablePath.LookupPath.ToString()
            }, context1);
            context1.ThrowFirstErrorOrDoNothing(true);
          }
          else
          {
            try
            {
              this.ClearContent(new string[1]
              {
                variablePath.LookupPath.ToString()
              }, false, false);
            }
            catch (NotSupportedException ex)
            {
            }
            catch (ItemNotFoundException ex)
            {
            }
            contentWriter1 = this.GetContentWriter(new string[1]
            {
              variablePath.LookupPath.ToString()
            }, false, false);
          }
        }
        catch (NotImplementedException ex)
        {
          ProviderInfo provider = (ProviderInfo) null;
          this.Globber.GetProviderPath(variablePath.LookupPath.ToString(), out provider);
          throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", provider, variablePath.LookupPath.ToString(), (Exception) ex, false);
        }
        catch (NotSupportedException ex)
        {
          ProviderInfo provider = (ProviderInfo) null;
          this.Globber.GetProviderPath(variablePath.LookupPath.ToString(), out provider);
          throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", provider, variablePath.LookupPath.ToString(), (Exception) ex, false);
        }
        if (contentWriter1 == null || contentWriter1.Count == 0)
        {
          ItemNotFoundException notFoundException = new ItemNotFoundException(variablePath.ToString(), "PathNotFound");
          SessionStateInternal.tracer.TraceException((Exception) notFoundException);
          throw notFoundException;
        }
        if (contentWriter1.Count > 1)
        {
          foreach (IContentWriter contentWriter2 in contentWriter1)
            contentWriter2.Close();
          PSArgumentException argumentException = SessionStateInternal.tracer.NewArgumentException("path", "SessionStateStrings", "VariablePathResolvedToMultiple", (object) variablePath.ToString());
          ProviderInfo provider = (ProviderInfo) null;
          this.Globber.GetProviderPath(variablePath.LookupPath.ToString(), out provider);
          throw this.NewProviderInvocationException("ProviderVariableSyntaxInvalid", provider, variablePath.LookupPath.ToString(), (Exception) argumentException);
        }
        IContentWriter contentWriter3 = contentWriter1[0];
        if (!(newValue is IList content))
          content = (IList) new object[1]{ newValue };
        try
        {
          contentWriter3.Write(content);
        }
        catch (Exception ex)
        {
          ProviderInfo provider = (ProviderInfo) null;
          this.Globber.GetProviderPath(variablePath.LookupPath.ToString(), out provider);
          CommandProcessorBase.CheckForSevereException(ex);
          ProviderInvocationException invocationException = new ProviderInvocationException("ProviderContentWriteError", provider, variablePath.LookupPath.ToString(), ex);
          SessionStateInternal.tracer.TraceException((Exception) invocationException);
          throw invocationException;
        }
        finally
        {
          contentWriter3.Close();
        }
      }
      return obj;
    }

    internal object SetVariableAtScope(
      string name,
      object value,
      string scopeID,
      bool force,
      CommandOrigin origin)
    {
      ScopedItemLookupPath scopedItemLookupPath = name != null ? new ScopedItemLookupPath(name) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (name));
      SessionStateScope scopeById = this.GetScopeByID(scopeID);
      object obj = (object) null;
      if (scopedItemLookupPath.IsScopedItem)
      {
        obj = (object) scopeById.SetVariable(scopedItemLookupPath.LookupPath.ToString(), value, true, force, this, origin);
      }
      else
      {
        PSDriveInfo drive = scopeById.GetDrive(scopedItemLookupPath.LookupPath.NamespaceID);
        if (drive != (PSDriveInfo) null)
        {
          CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(this.ExecutionContext);
          cmdletProviderContext.Drive = drive;
          cmdletProviderContext.Force = (SwitchParameter) force;
          Collection<IContentWriter> collection = new Collection<IContentWriter>();
          Collection<IContentWriter> contentWriter1;
          try
          {
            if (cmdletProviderContext != null)
            {
              try
              {
                CmdletProviderContext context = new CmdletProviderContext(cmdletProviderContext);
                this.ClearContent(new string[1]
                {
                  scopedItemLookupPath.LookupPath.ToString()
                }, context);
              }
              catch (NotSupportedException ex)
              {
              }
              catch (ItemNotFoundException ex)
              {
              }
              contentWriter1 = this.GetContentWriter(new string[1]
              {
                scopedItemLookupPath.LookupPath.ToString()
              }, cmdletProviderContext);
            }
            else
            {
              try
              {
                this.ClearContent(new string[1]
                {
                  scopedItemLookupPath.LookupPath.ToString()
                }, false, false);
              }
              catch (NotSupportedException ex)
              {
              }
              catch (ItemNotFoundException ex)
              {
              }
              contentWriter1 = this.GetContentWriter(new string[1]
              {
                scopedItemLookupPath.LookupPath.ToString()
              }, false, false);
            }
          }
          catch (NotImplementedException ex)
          {
            ProviderInfo provider = (ProviderInfo) null;
            this.Globber.GetProviderPath(scopedItemLookupPath.LookupPath.ToString(), out provider);
            throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", provider, scopedItemLookupPath.LookupPath.ToString(), (Exception) ex, false);
          }
          catch (NotSupportedException ex)
          {
            ProviderInfo provider = (ProviderInfo) null;
            this.Globber.GetProviderPath(scopedItemLookupPath.LookupPath.ToString(), out provider);
            throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", provider, scopedItemLookupPath.LookupPath.ToString(), (Exception) ex, false);
          }
          if (contentWriter1 == null || contentWriter1.Count == 0)
          {
            ItemNotFoundException notFoundException = new ItemNotFoundException(scopedItemLookupPath.ToString(), "PathNotFound");
            SessionStateInternal.tracer.TraceException((Exception) notFoundException);
            throw notFoundException;
          }
          if (contentWriter1.Count > 1)
          {
            foreach (IContentWriter contentWriter2 in contentWriter1)
              contentWriter2.Close();
            PSArgumentException argumentException = SessionStateInternal.tracer.NewArgumentException("path", "SessionStateStrings", "VariablePathResolvedToMultiple", (object) scopedItemLookupPath.ToString());
            ProviderInfo provider = (ProviderInfo) null;
            this.Globber.GetProviderPath(scopedItemLookupPath.LookupPath.ToString(), out provider);
            throw this.NewProviderInvocationException("ProviderVariableSyntaxInvalid", provider, scopedItemLookupPath.LookupPath.ToString(), (Exception) argumentException);
          }
          IContentWriter contentWriter3 = contentWriter1[0];
          if (!(value is IList content))
            content = (IList) new object[1]{ value };
          try
          {
            obj = (object) contentWriter3.Write(content);
          }
          catch (Exception ex)
          {
            ProviderInfo provider = (ProviderInfo) null;
            this.Globber.GetProviderPath(scopedItemLookupPath.LookupPath.ToString(), out provider);
            CommandProcessorBase.CheckForSevereException(ex);
            ProviderInvocationException invocationException = new ProviderInvocationException("ProviderContentWriteError", provider, scopedItemLookupPath.LookupPath.ToString(), ex);
            SessionStateInternal.tracer.TraceException((Exception) invocationException);
            throw invocationException;
          }
          finally
          {
            contentWriter3.Close();
          }
        }
      }
      return obj;
    }

    internal object SetVariableAtScope(
      PSVariable variable,
      string scopeID,
      bool force,
      CommandOrigin origin)
    {
      if (variable == null || string.IsNullOrEmpty(variable.Name))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (variable));
      return (object) this.GetScopeByID(scopeID).SetVariable(new ScopedItemLookupPath(variable.Name)
      {
        IsScopedItem = true
      }.LookupPath.ToString(), (object) variable, false, force, this, origin);
    }

    internal object SetVariableAtScope(PSVariable variable, string scopeID, bool force) => this.SetVariableAtScope(variable, scopeID, force, CommandOrigin.Internal);

    internal object NewVariable(PSVariable variable, bool force)
    {
      if (variable == null || string.IsNullOrEmpty(variable.Name))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (variable));
      return (object) this.CurrentScope.NewVariable(variable, force, this);
    }

    internal object NewVariableAtScope(PSVariable variable, string scopeID, bool force)
    {
      if (variable == null || string.IsNullOrEmpty(variable.Name))
        throw SessionStateInternal.tracer.NewArgumentException(nameof (variable));
      return (object) this.GetScopeByID(scopeID).NewVariable(variable, force, this);
    }

    internal void RemoveVariable(string name) => this.RemoveVariable(name, false);

    internal void RemoveVariable(string name, bool force)
    {
      ScopedItemLookupPath variablePath = name != null ? new ScopedItemLookupPath(name) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (name));
      SessionStateScope scope = (SessionStateScope) null;
      if (variablePath.IsScopedItem)
      {
        if (this.GetVariableItem(variablePath, out scope) == null)
          return;
        scope.RemoveVariable(variablePath.LookupPath.ToString(), force);
      }
      else
      {
        CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
        context.Force = (SwitchParameter) force;
        this.RemoveItem(new string[1]
        {
          variablePath.LookupPath.ToString()
        }, false, context);
        context.ThrowFirstErrorOrDoNothing();
      }
    }

    internal void RemoveVariable(PSVariable variable) => this.RemoveVariable(variable, false);

    internal void RemoveVariable(PSVariable variable, bool force)
    {
      ScopedItemLookupPath variablePath = variable != null ? new ScopedItemLookupPath(variable.Name) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (variable));
      SessionStateScope scope = (SessionStateScope) null;
      if (this.GetVariableItem(variablePath, out scope) == null)
        return;
      scope.RemoveVariable(variablePath.LookupPath.ToString(), force);
    }

    internal void RemoveVariableAtScope(string name, string scopeID) => this.RemoveVariableAtScope(name, scopeID, false);

    internal void RemoveVariableAtScope(string name, string scopeID, bool force)
    {
      ScopedItemLookupPath scopedItemLookupPath = !string.IsNullOrEmpty(name) ? new ScopedItemLookupPath(name) : throw SessionStateInternal.tracer.NewArgumentException(nameof (name));
      SessionStateScope scopeById = this.GetScopeByID(scopeID);
      if (scopedItemLookupPath.IsScopedItem)
      {
        scopeById.RemoveVariable(scopedItemLookupPath.LookupPath.ToString(), force);
      }
      else
      {
        PSDriveInfo drive = scopeById.GetDrive(scopedItemLookupPath.LookupPath.NamespaceID);
        if (!(drive != (PSDriveInfo) null))
          return;
        CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
        context.Drive = drive;
        context.Force = (SwitchParameter) force;
        this.RemoveItem(new string[1]
        {
          scopedItemLookupPath.LookupPath.ToString()
        }, false, context);
        context.ThrowFirstErrorOrDoNothing();
      }
    }

    internal void RemoveVariableAtScope(PSVariable variable, string scopeID) => this.RemoveVariableAtScope(variable, scopeID, false);

    internal void RemoveVariableAtScope(PSVariable variable, string scopeID, bool force)
    {
      ScopedItemLookupPath scopedItemLookupPath = variable != null ? new ScopedItemLookupPath(variable.Name) : throw SessionStateInternal.tracer.NewArgumentNullException(nameof (variable));
      this.GetScopeByID(scopeID).RemoveVariable(scopedItemLookupPath.LookupPath.ToString(), force);
    }

    internal IDictionary<string, PSVariable> GetVariableTable()
    {
      SessionStateScopeEnumerator stateScopeEnumerator = new SessionStateScopeEnumerator(this, this.currentScope);
      Dictionary<string, PSVariable> dictionary = new Dictionary<string, PSVariable>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (SessionStateScope sessionStateScope in (IEnumerable<SessionStateScope>) stateScopeEnumerator)
      {
        foreach (KeyValuePair<string, PSVariable> variable in (IEnumerable<KeyValuePair<string, PSVariable>>) sessionStateScope.Variables)
        {
          if (!dictionary.ContainsKey(variable.Key) && (!variable.Value.IsPrivate || sessionStateScope == this.currentScope))
            dictionary.Add(variable.Key, variable.Value);
        }
      }
      return (IDictionary<string, PSVariable>) dictionary;
    }

    internal IDictionary<string, PSVariable> GetVariableTableAtScope(
      string scopeID)
    {
      Dictionary<string, PSVariable> dictionary = new Dictionary<string, PSVariable>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (KeyValuePair<string, PSVariable> variable in (IEnumerable<KeyValuePair<string, PSVariable>>) this.GetScopeByID(scopeID).Variables)
      {
        if (!dictionary.ContainsKey(variable.Key))
          dictionary.Add(variable.Key, variable.Value);
      }
      return (IDictionary<string, PSVariable>) dictionary;
    }

    internal List<PSVariable> ExportedVariables => this._exportedVariables;
  }
}
