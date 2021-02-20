// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceConfiguration
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell;
using Microsoft.PowerShell.Commands.Internal.Format;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation.Host;
using System.Reflection;
using System.Security;

namespace System.Management.Automation.Runspaces
{
  public abstract class RunspaceConfiguration
  {
    private RunspaceConfigurationEntryCollection<CmdletConfigurationEntry> _cmdlets;
    private RunspaceConfigurationEntryCollection<ProviderConfigurationEntry> _providers;
    private TypeTable typeTable;
    private RunspaceConfigurationEntryCollection<TypeConfigurationEntry> _types;
    private RunspaceConfigurationEntryCollection<FormatConfigurationEntry> _formats;
    private TypeInfoDataBaseManager formatDBManger = new TypeInfoDataBaseManager();
    private RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> _scripts;
    private RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> _initializationScripts;
    private RunspaceConfigurationEntryCollection<AssemblyConfigurationEntry> _assemblies;
    private AuthorizationManager _authorizationManager;
    private PSHost _host;
    private bool _initialized;
    private object _syncObject = new object();
    [TraceSource("RunspaceConfiguration", "RunspaceConfiguration")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (RunspaceConfiguration), nameof (RunspaceConfiguration));
    [TraceSource("RunspaceInit", "Initialization code for Runspace")]
    private static PSTraceSource runspaceInitTracer = PSTraceSource.GetTracer("RunspaceInit", "Initialization code for Runspace", false);

    public static RunspaceConfiguration Create(string assemblyName)
    {
      using (RunspaceConfiguration.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(assemblyName))
          throw RunspaceConfiguration.tracer.NewArgumentNullException(nameof (assemblyName));
        Assembly assembly1 = (Assembly) null;
        foreach (Assembly assembly2 in AppDomain.CurrentDomain.GetAssemblies())
        {
          if (string.Equals(assembly2.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
          {
            assembly1 = assembly2;
            break;
          }
        }
        if (assembly1 == null)
          assembly1 = Assembly.Load(assemblyName);
        return RunspaceConfiguration.Create(assembly1);
      }
    }

    public static RunspaceConfiguration Create(
      string consoleFilePath,
      out PSConsoleLoadException warnings)
    {
      using (RunspaceConfiguration.tracer.TraceMethod())
        return (RunspaceConfiguration) RunspaceConfigForSingleShell.Create(consoleFilePath, out warnings);
    }

    public static RunspaceConfiguration Create()
    {
      using (RunspaceConfiguration.tracer.TraceMethod())
        return (RunspaceConfiguration) RunspaceConfigForSingleShell.CreateDefaultConfiguration();
    }

    private static RunspaceConfiguration Create(Assembly assembly)
    {
      using (RunspaceConfiguration.tracer.TraceMethod())
      {
        object[] objArray = assembly != null ? assembly.GetCustomAttributes(typeof (RunspaceConfigurationTypeAttribute), false) : throw RunspaceConfiguration.tracer.NewArgumentNullException(nameof (assembly));
        if (objArray == null || objArray.Length == 0)
          throw new RunspaceConfigurationAttributeException("RunspaceConfigurationAttributeNotExist", assembly.FullName);
        RunspaceConfigurationTypeAttribute configurationTypeAttribute = objArray.Length <= 1 ? (RunspaceConfigurationTypeAttribute) objArray[0] : throw new RunspaceConfigurationAttributeException("RunspaceConfigurationAttributeDuplicate", assembly.FullName);
        try
        {
          return RunspaceConfiguration.Create(assembly.GetType(configurationTypeAttribute.RunspaceConfigurationType, true));
        }
        catch (SecurityException ex)
        {
          throw new RunspaceConfigurationTypeException(assembly.FullName, configurationTypeAttribute.RunspaceConfigurationType);
        }
      }
    }

    private static RunspaceConfiguration Create(Type runspaceConfigType)
    {
      using (RunspaceConfiguration.tracer.TraceMethod())
      {
        MethodInfo method = runspaceConfigType.GetMethod(nameof (Create), BindingFlags.Static | BindingFlags.Public);
        return method == null ? (RunspaceConfiguration) null : (RunspaceConfiguration) method.Invoke((object) null, (object[]) null);
      }
    }

    public abstract string ShellId { get; }

    public PSSnapInInfo AddPSSnapIn(string name, out PSSnapInException warning) => this.DoAddPSSnapIn(name, out warning);

    internal virtual PSSnapInInfo DoAddPSSnapIn(
      string name,
      out PSSnapInException warning)
    {
      throw RunspaceConfiguration.tracer.NewNotSupportedException();
    }

    public PSSnapInInfo RemovePSSnapIn(string name, out PSSnapInException warning) => this.DoRemovePSSnapIn(name, out warning);

    internal virtual PSSnapInInfo DoRemovePSSnapIn(
      string name,
      out PSSnapInException warning)
    {
      throw RunspaceConfiguration.tracer.NewNotSupportedException();
    }

    public virtual RunspaceConfigurationEntryCollection<CmdletConfigurationEntry> Cmdlets
    {
      get
      {
        using (RunspaceConfiguration.tracer.TraceProperty())
        {
          if (this._cmdlets == null)
            this._cmdlets = new RunspaceConfigurationEntryCollection<CmdletConfigurationEntry>();
          return this._cmdlets;
        }
      }
    }

    public virtual RunspaceConfigurationEntryCollection<ProviderConfigurationEntry> Providers
    {
      get
      {
        using (RunspaceConfiguration.tracer.TraceProperty())
        {
          if (this._providers == null)
            this._providers = new RunspaceConfigurationEntryCollection<ProviderConfigurationEntry>();
          return this._providers;
        }
      }
    }

    internal TypeTable TypeTable
    {
      get
      {
        if (this.typeTable == null)
          this.typeTable = new TypeTable();
        return this.typeTable;
      }
    }

    public virtual RunspaceConfigurationEntryCollection<TypeConfigurationEntry> Types
    {
      get
      {
        using (RunspaceConfiguration.tracer.TraceProperty())
        {
          if (this._types == null)
            this._types = new RunspaceConfigurationEntryCollection<TypeConfigurationEntry>();
          return this._types;
        }
      }
    }

    public virtual RunspaceConfigurationEntryCollection<FormatConfigurationEntry> Formats
    {
      get
      {
        using (RunspaceConfiguration.tracer.TraceProperty())
        {
          if (this._formats == null)
            this._formats = new RunspaceConfigurationEntryCollection<FormatConfigurationEntry>();
          return this._formats;
        }
      }
    }

    internal TypeInfoDataBaseManager FormatDBManager => this.formatDBManger;

    public virtual RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> Scripts
    {
      get
      {
        using (RunspaceConfiguration.tracer.TraceProperty())
        {
          if (this._scripts == null)
            this._scripts = new RunspaceConfigurationEntryCollection<ScriptConfigurationEntry>();
          return this._scripts;
        }
      }
    }

    public virtual RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> InitializationScripts
    {
      get
      {
        using (RunspaceConfiguration.tracer.TraceProperty())
        {
          if (this._initializationScripts == null)
            this._initializationScripts = new RunspaceConfigurationEntryCollection<ScriptConfigurationEntry>();
          return this._initializationScripts;
        }
      }
    }

    public virtual RunspaceConfigurationEntryCollection<AssemblyConfigurationEntry> Assemblies
    {
      get
      {
        using (RunspaceConfiguration.tracer.TraceProperty())
        {
          if (this._assemblies == null)
            this._assemblies = new RunspaceConfigurationEntryCollection<AssemblyConfigurationEntry>();
          return this._assemblies;
        }
      }
    }

    public virtual AuthorizationManager AuthorizationManager
    {
      get
      {
        if (this._authorizationManager == null)
          this._authorizationManager = (AuthorizationManager) new PSAuthorizationManager(this.ShellId);
        return this._authorizationManager;
      }
    }

    internal void Bind(ExecutionContext executionContext)
    {
      this._host = (PSHost) executionContext.EngineHostInterface;
      this.Initialize(executionContext);
      this.Assemblies.OnUpdate += new RunspaceConfigurationEntryUpdateEventHandler(executionContext.UpdateAssemblyCache);
      RunspaceConfiguration.runspaceInitTracer.WriteLine("initializing assembly list", new object[0]);
      try
      {
        this.Assemblies.Update(true);
      }
      catch (RuntimeException ex)
      {
        RunspaceConfiguration.runspaceInitTracer.WriteLine("assembly list initialization failed", new object[0]);
        RunspaceConfiguration.runspaceInitTracer.TraceException((Exception) ex);
        RunspaceConfiguration.tracer.TraceException((Exception) ex);
        MshLog.LogEngineHealthEvent(executionContext, 103, (Exception) ex, Severity.Error);
        executionContext.ReportEngineStartupError(ex.Message);
        throw;
      }
      if (executionContext.CommandDiscovery != null)
      {
        this.Cmdlets.OnUpdate += new RunspaceConfigurationEntryUpdateEventHandler(executionContext.CommandDiscovery.UpdateCmdletCache);
        RunspaceConfiguration.runspaceInitTracer.WriteLine("initializing cmdlet list", new object[0]);
        try
        {
          this.Cmdlets.Update(true);
        }
        catch (PSNotSupportedException ex)
        {
          RunspaceConfiguration.runspaceInitTracer.WriteLine("cmdlet list initialization failed", new object[0]);
          RunspaceConfiguration.runspaceInitTracer.TraceException((Exception) ex);
          RunspaceConfiguration.tracer.TraceException((Exception) ex);
          MshLog.LogEngineHealthEvent(executionContext, 103, (Exception) ex, Severity.Error);
          executionContext.ReportEngineStartupError(ex.Message);
          throw;
        }
      }
      if (executionContext.EngineSessionState == null)
        return;
      this.Providers.OnUpdate += new RunspaceConfigurationEntryUpdateEventHandler(executionContext.EngineSessionState.UpdateProviders);
      RunspaceConfiguration.runspaceInitTracer.WriteLine("initializing provider list", new object[0]);
      try
      {
        this.Providers.Update(true);
      }
      catch (PSNotSupportedException ex)
      {
        RunspaceConfiguration.runspaceInitTracer.WriteLine("provider list initialization failed", new object[0]);
        RunspaceConfiguration.runspaceInitTracer.TraceException((Exception) ex);
        RunspaceConfiguration.tracer.TraceException((Exception) ex);
        MshLog.LogEngineHealthEvent(executionContext, 103, (Exception) ex, Severity.Error);
        executionContext.ReportEngineStartupError(ex.Message);
        throw;
      }
    }

    internal void Unbind(ExecutionContext executionContext)
    {
      if (executionContext == null)
        return;
      if (executionContext.CommandDiscovery != null)
        this.Cmdlets.OnUpdate -= new RunspaceConfigurationEntryUpdateEventHandler(executionContext.CommandDiscovery.UpdateCmdletCache);
      if (executionContext.EngineSessionState != null)
        this.Providers.OnUpdate -= new RunspaceConfigurationEntryUpdateEventHandler(executionContext.EngineSessionState.UpdateProviders);
      this.Assemblies.OnUpdate -= new RunspaceConfigurationEntryUpdateEventHandler(executionContext.UpdateAssemblyCache);
    }

    internal void Initialize(ExecutionContext executionContext)
    {
      lock (this._syncObject)
      {
        if (this._initialized)
          return;
        this._initialized = true;
        this.Types.OnUpdate += new RunspaceConfigurationEntryUpdateEventHandler(this.UpdateTypes);
        this.Formats.OnUpdate += new RunspaceConfigurationEntryUpdateEventHandler(this.UpdateFormats);
        RunspaceConfiguration.runspaceInitTracer.WriteLine("initializing types information", new object[0]);
        try
        {
          this.UpdateTypes();
        }
        catch (RuntimeException ex)
        {
          RunspaceConfiguration.runspaceInitTracer.WriteLine("type information initialization failed", new object[0]);
          RunspaceConfiguration.runspaceInitTracer.TraceException((Exception) ex);
          RunspaceConfiguration.tracer.TraceException((Exception) ex);
          MshLog.LogEngineHealthEvent(executionContext, 103, (Exception) ex, Severity.Warning);
          executionContext.ReportEngineStartupError(ex.Message);
        }
        RunspaceConfiguration.runspaceInitTracer.WriteLine("initializing format information", new object[0]);
        try
        {
          this.UpdateFormats();
        }
        catch (RuntimeException ex)
        {
          RunspaceConfiguration.runspaceInitTracer.WriteLine("format information initialization failed", new object[0]);
          RunspaceConfiguration.runspaceInitTracer.TraceException((Exception) ex);
          RunspaceConfiguration.tracer.TraceException((Exception) ex);
          MshLog.LogEngineHealthEvent(executionContext, 103, (Exception) ex, Severity.Warning);
          executionContext.ReportEngineStartupError(ex.Message);
        }
      }
    }

    internal void UpdateTypes()
    {
      using (RunspaceConfiguration.tracer.TraceMethod())
      {
        Collection<string> independentErrors = new Collection<string>();
        Collection<PSSnapInTypeAndFormatErrors> formatAndTypesErrors = FormatAndTypeDataHelper.GetFormatAndTypesErrors(this, this._host, (IEnumerable) this.Types, RunspaceConfigurationCategory.Types, independentErrors);
        this.TypeTable.Update(formatAndTypesErrors, this._authorizationManager, this._host);
        FormatAndTypeDataHelper.ThrowExceptionOnError("ErrorsUpdatingTypes", independentErrors, formatAndTypesErrors, RunspaceConfigurationCategory.Types);
      }
    }

    private void UpdateFormats()
    {
      using (RunspaceConfiguration.tracer.TraceMethod())
      {
        Collection<string> independentErrors = new Collection<string>();
        Collection<PSSnapInTypeAndFormatErrors> formatAndTypesErrors = FormatAndTypeDataHelper.GetFormatAndTypesErrors(this, this._host, (IEnumerable) this.Formats, RunspaceConfigurationCategory.Formats, independentErrors);
        this.FormatDBManager.UpdateDataBase(formatAndTypesErrors, this.AuthorizationManager, this._host);
        FormatAndTypeDataHelper.ThrowExceptionOnError("ErrorsUpdatingFormats", independentErrors, formatAndTypesErrors, RunspaceConfigurationCategory.Formats);
      }
    }
  }
}
