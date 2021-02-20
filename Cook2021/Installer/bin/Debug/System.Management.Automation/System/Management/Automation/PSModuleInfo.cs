// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSModuleInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public sealed class PSModuleInfo
  {
    internal const string DynamicModulePrefixString = "__DynamicModule_";
    [TraceSource("SessionState", "PSModuleInfo Class")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (SessionState), "PSModuleInfo Class");
    private ExecutionContext _context;
    private string _name = string.Empty;
    private string _path = string.Empty;
    private string _description = string.Empty;
    private Guid _guid;
    private string _moduleBase;
    private object _privateData;
    private Version _version = new Version(0, 0);
    private ModuleType _moduleType;
    private ModuleAccessMode _accessMode;
    private List<CmdletInfo> _compiledExports = new List<CmdletInfo>();
    private ReadOnlyCollection<PSModuleInfo> _readonlyNestedModules;
    private List<PSModuleInfo> _nestedModules = new List<PSModuleInfo>();
    private ReadOnlyCollection<PSModuleInfo> _readonlyRequiredModules;
    private List<PSModuleInfo> _requiredModules = new List<PSModuleInfo>();
    internal static string[] builtinVariables = new string[17]
    {
      "_",
      "this",
      "input",
      "args",
      "true",
      "false",
      "null",
      "MaximumErrorCount",
      "MaximumVariableCount",
      "MaximumFunctionCount",
      "MaximumAliasCount",
      "MaximumDriveCount",
      "Error",
      "PSScriptRoot",
      "MyInvocation",
      "ExecutionContext",
      "StackTrace"
    };
    private SessionState _sessionState;
    private ScriptBlock cleanUpScript;
    private ReadOnlyCollection<string> exportedFormatFiles = new ReadOnlyCollection<string>((IList<string>) new List<string>());
    private ReadOnlyCollection<string> exportedTypeFiles = new ReadOnlyCollection<string>((IList<string>) new List<string>());

    internal static void SetDefaultDynamicNameAndPath(PSModuleInfo module)
    {
      string str = Guid.NewGuid().ToString();
      module._path = str;
      module._name = "__DynamicModule_" + str;
    }

    internal PSModuleInfo(string path, ExecutionContext context, SessionState sessionState)
      : this((string) null, path, context, sessionState)
    {
    }

    internal PSModuleInfo(
      string name,
      string path,
      ExecutionContext context,
      SessionState sessionState)
    {
      this._context = context;
      if (path != null)
      {
        string resolvedPath = ModuleCmdletBase.GetResolvedPath(path, this._context);
        this._path = resolvedPath == null ? path : resolvedPath;
      }
      this._name = name != null || this._path == null ? name : ModuleIntrinsics.GetModuleName(this._path);
      this._sessionState = sessionState;
      if (sessionState == null)
        return;
      sessionState.Internal.Module = this;
    }

    public PSModuleInfo(bool linkToGlobal)
    {
      this._context = LocalPipeline.GetExecutionContextFromTLS();
      if (this._context == null)
        throw new InvalidOperationException(nameof (PSModuleInfo));
      PSModuleInfo.SetDefaultDynamicNameAndPath(this);
      this._sessionState = new SessionState(this._context.EngineSessionState, true, linkToGlobal);
      this._sessionState.Internal.Module = this;
    }

    public PSModuleInfo(ScriptBlock scriptBlock)
    {
      this._context = LocalPipeline.GetExecutionContextFromTLS();
      if (this._context == null)
        throw new InvalidOperationException(nameof (PSModuleInfo));
      PSModuleInfo.SetDefaultDynamicNameAndPath(this);
      this._sessionState = new SessionState(this._context.EngineSessionState, true, true);
      this._sessionState.Internal.Module = this;
      SessionStateInternal engineSessionState = this._context.EngineSessionState;
      try
      {
        ArrayList resultList = (ArrayList) null;
        this._context.EngineSessionState = this._sessionState.Internal;
        this._context.EngineSessionState.SetVariableValue("PSScriptRoot", (object) this._path);
        scriptBlock = scriptBlock.Clone(true);
        scriptBlock.SessionState = this._sessionState;
        if (scriptBlock == null)
          throw PSModuleInfo.tracer.NewInvalidOperationException();
        scriptBlock.InvokeWithPipe(false, true, (object) AutomationNull.Value, (object) AutomationNull.Value, (object) AutomationNull.Value, (Pipe) null, ref resultList);
      }
      finally
      {
        this._context.EngineSessionState = engineSessionState;
      }
    }

    public override string ToString() => this.Name;

    public string Name => this._name;

    internal void SetName(string name) => this._name = name;

    public string Path => this._path;

    public string Description
    {
      get => this._description;
      set
      {
        if (value != null)
          this._description = value;
        else
          this._description = string.Empty;
      }
    }

    public Guid Guid => this._guid;

    internal void SetGuid(Guid guid) => this._guid = guid;

    public string ModuleBase
    {
      get
      {
        if (this._moduleBase == null)
          this._moduleBase = string.IsNullOrEmpty(this._path) ? string.Empty : System.IO.Path.GetDirectoryName(this._path);
        return this._moduleBase;
      }
    }

    internal void SetModuleBase(string moduleBase) => this._moduleBase = moduleBase;

    public object PrivateData
    {
      get => this._privateData;
      set => this._privateData = value;
    }

    public Version Version => this._version;

    internal void SetVersion(Version version) => this._version = version;

    public ModuleType ModuleType => this._moduleType;

    internal void SetModuleType(ModuleType moduleType) => this._moduleType = moduleType;

    public ModuleAccessMode AccessMode
    {
      get => this._accessMode;
      set => this._accessMode = this._accessMode != ModuleAccessMode.Constant ? value : throw PSModuleInfo.tracer.NewInvalidOperationException();
    }

    public Dictionary<string, FunctionInfo> ExportedFunctions
    {
      get
      {
        Dictionary<string, FunctionInfo> dictionary = new Dictionary<string, FunctionInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        if (this._sessionState != null && this._sessionState.Internal.ExportedFunctions != null)
        {
          foreach (FunctionInfo exportedFunction in this._sessionState.Internal.ExportedFunctions)
            dictionary.Add(exportedFunction.Name, exportedFunction);
        }
        return dictionary;
      }
    }

    public Dictionary<string, CmdletInfo> ExportedCmdlets
    {
      get
      {
        Dictionary<string, CmdletInfo> dictionary = new Dictionary<string, CmdletInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        foreach (CmdletInfo compiledExport in this.CompiledExports)
          dictionary.Add(compiledExport.Name, compiledExport);
        return dictionary;
      }
    }

    internal void AddExportedCmdlet(CmdletInfo cmdlet) => this._compiledExports.Add(cmdlet);

    internal List<CmdletInfo> CompiledExports
    {
      get
      {
        if (this._sessionState != null && this._sessionState.Internal.ExportedCmdlets != null && this._sessionState.Internal.ExportedCmdlets.Count > 0)
        {
          foreach (CmdletInfo exportedCmdlet in this._sessionState.Internal.ExportedCmdlets)
            this._compiledExports.Add(exportedCmdlet);
          this._sessionState.Internal.ExportedCmdlets.Clear();
        }
        return this._compiledExports;
      }
    }

    public ReadOnlyCollection<PSModuleInfo> NestedModules
    {
      get
      {
        if (this._readonlyNestedModules == null)
          this._readonlyNestedModules = new ReadOnlyCollection<PSModuleInfo>((IList<PSModuleInfo>) this._nestedModules);
        return this._readonlyNestedModules;
      }
    }

    internal void AddNestedModule(PSModuleInfo nestedModule) => PSModuleInfo.AddModuleToList(nestedModule, this._nestedModules);

    public ReadOnlyCollection<PSModuleInfo> RequiredModules
    {
      get
      {
        if (this._readonlyRequiredModules == null)
          this._readonlyRequiredModules = new ReadOnlyCollection<PSModuleInfo>((IList<PSModuleInfo>) this._requiredModules);
        return this._readonlyRequiredModules;
      }
    }

    internal void AddRequiredModule(PSModuleInfo requiredModule) => PSModuleInfo.AddModuleToList(requiredModule, this._requiredModules);

    private static void AddModuleToList(PSModuleInfo module, List<PSModuleInfo> moduleList)
    {
      foreach (PSModuleInfo module1 in moduleList)
      {
        if (module1.Path.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
          return;
      }
      moduleList.Add(module);
    }

    public Dictionary<string, PSVariable> ExportedVariables
    {
      get
      {
        Dictionary<string, PSVariable> dictionary = new Dictionary<string, PSVariable>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        if (this._sessionState == null || this._sessionState.Internal.ExportedVariables == null)
          return dictionary;
        foreach (PSVariable exportedVariable in this._sessionState.Internal.ExportedVariables)
          dictionary.Add(exportedVariable.Name, exportedVariable);
        return dictionary;
      }
    }

    public Dictionary<string, AliasInfo> ExportedAliases
    {
      get
      {
        Dictionary<string, AliasInfo> dictionary = new Dictionary<string, AliasInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        if (this._sessionState == null || this._sessionState.Internal.ExportedAliases == null)
          return dictionary;
        foreach (AliasInfo exportedAlias in this._sessionState.Internal.ExportedAliases)
          dictionary.Add(exportedAlias.Name, exportedAlias);
        return dictionary;
      }
    }

    public SessionState SessionState
    {
      get => this._sessionState;
      set => this._sessionState = value;
    }

    public ScriptBlock NewBoundScriptBlock(ScriptBlock scriptBlockToBind)
    {
      if (this._sessionState == null || this._context == null)
        throw PSModuleInfo.tracer.NewInvalidOperationException("Modules", "InvalidOperationOnBinaryModule");
      ScriptBlock scriptBlock = (ScriptBlock) null;
      lock (this._context.EngineSessionState)
      {
        SessionStateInternal engineSessionState = this._context.EngineSessionState;
        try
        {
          this._context.EngineSessionState = this._sessionState.Internal;
          scriptBlock = scriptBlockToBind.Clone(true);
          scriptBlock.SessionState = this._sessionState;
        }
        finally
        {
          this._context.EngineSessionState = engineSessionState;
        }
      }
      return scriptBlock;
    }

    public object Invoke(ScriptBlock sb, params object[] args)
    {
      if (sb == null)
        return (object) null;
      SessionStateInternal sessionStateInternal = sb.SessionStateInternal;
      try
      {
        sb.SessionStateInternal = this._sessionState.Internal;
        return sb.InvokeReturnAsIs(args);
      }
      finally
      {
        sb.SessionStateInternal = sessionStateInternal;
      }
    }

    internal void CaptureLocals()
    {
      if (this._sessionState == null)
        throw PSModuleInfo.tracer.NewInvalidOperationException("Modules", "InvalidOperationOnBinaryModule");
      foreach (PSVariable psVariable in (IEnumerable<PSVariable>) this._context.EngineSessionState.CurrentScope.Variables.Values)
      {
        try
        {
          if (psVariable.Options == ScopedItemOptions.None)
            this._sessionState.Internal.NewVariable(new PSVariable(psVariable.Name, psVariable.Value, psVariable.Options, psVariable.Attributes, psVariable.Description), false);
        }
        catch (SessionStateException ex)
        {
        }
      }
    }

    public PSObject AsCustomObject()
    {
      if (this._sessionState == null)
        throw PSModuleInfo.tracer.NewInvalidOperationException("Modules", "InvalidOperationOnBinaryModule");
      PSObject psObject = new PSObject();
      foreach (KeyValuePair<string, FunctionInfo> exportedFunction in this.ExportedFunctions)
      {
        FunctionInfo functionInfo = exportedFunction.Value;
        if (functionInfo != null)
        {
          PSScriptMethod psScriptMethod = new PSScriptMethod(functionInfo.Name, functionInfo.ScriptBlock);
          psObject.Members.Add((PSMemberInfo) psScriptMethod);
        }
      }
      foreach (KeyValuePair<string, PSVariable> exportedVariable in this.ExportedVariables)
      {
        PSVariable variable = exportedVariable.Value;
        if (variable != null)
        {
          PSVariableProperty variableProperty = new PSVariableProperty(variable);
          psObject.Members.Add((PSMemberInfo) variableProperty);
        }
      }
      return psObject;
    }

    public ScriptBlock OnRemove
    {
      get => this.cleanUpScript;
      set => this.cleanUpScript = value;
    }

    public ReadOnlyCollection<string> ExportedFormatFiles => this.exportedFormatFiles;

    internal void SetExportedFormatFiles(ReadOnlyCollection<string> files) => this.exportedFormatFiles = files;

    public ReadOnlyCollection<string> ExportedTypeFiles => this.exportedTypeFiles;

    internal void SetExportedTypeFiles(ReadOnlyCollection<string> files) => this.exportedTypeFiles = files;
  }
}
