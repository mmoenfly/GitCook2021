// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Provider.CmdletProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Resources;
using System.Security.AccessControl;
using System.Threading;

namespace System.Management.Automation.Provider
{
  public abstract class CmdletProvider : IResourceSupplier
  {
    private CmdletProviderContext contextBase;
    private ProviderInfo providerInformation;
    [TraceSource("CmdletProviderClasses", "The namespace provider base classes tracer")]
    internal static PSTraceSource providerBaseTracer = PSTraceSource.GetTracer("CmdletProviderClasses", "The namespace provider base classes tracer");

    internal void SetProviderInformation(ProviderInfo providerInfoToSet)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod())
        this.providerInformation = providerInfoToSet != null ? providerInfoToSet : throw CmdletProvider.providerBaseTracer.NewArgumentNullException(nameof (providerInfoToSet));
    }

    internal CmdletProviderContext Context
    {
      get
      {
        using (CmdletProvider.providerBaseTracer.TraceProperty())
          return this.contextBase;
      }
      set
      {
        using (CmdletProvider.providerBaseTracer.TraceProperty())
        {
          if (value == null)
            throw CmdletProvider.providerBaseTracer.NewArgumentNullException(nameof (value));
          if (value.Credential != null && value.Credential != PSCredential.Empty && !CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Credentials, this.providerInformation))
            throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "Credentials_NotSupported");
          if (!string.IsNullOrEmpty(value.Filter) && !CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Filter, this.providerInformation))
            throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "Filter_NotSupported");
          if (value.UseTransaction && !CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Transactions, this.providerInformation))
            throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "Transactions_NotSupported");
          this.contextBase = value;
          this.contextBase.ProviderInstance = this;
        }
      }
    }

    internal ProviderInfo Start(
      ProviderInfo providerInfo,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod())
      {
        this.Context = cmdletProviderContext;
        return this.Start(providerInfo);
      }
    }

    internal object StartDynamicParameters(CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod())
      {
        this.Context = cmdletProviderContext;
        return this.StartDynamicParameters();
      }
    }

    internal void Stop(CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod())
      {
        this.Context = cmdletProviderContext;
        this.Stop();
      }
    }

    protected internal virtual void StopProcessing()
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod())
        ;
    }

    internal void GetProperty(
      string path,
      Collection<string> providerSpecificPickList,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        if (!(this is IPropertyCmdletProvider propertyCmdletProvider))
          throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "IPropertyCmdletProvider_NotSupported");
        propertyCmdletProvider.GetProperty(path, providerSpecificPickList);
      }
    }

    internal object GetPropertyDynamicParameters(
      string path,
      Collection<string> providerSpecificPickList,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return !(this is IPropertyCmdletProvider propertyCmdletProvider) ? (object) null : propertyCmdletProvider.GetPropertyDynamicParameters(path, providerSpecificPickList);
      }
    }

    internal void SetProperty(
      string path,
      PSObject propertyValue,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        if (!(this is IPropertyCmdletProvider propertyCmdletProvider))
          throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "IPropertyCmdletProvider_NotSupported");
        propertyCmdletProvider.SetProperty(path, propertyValue);
      }
    }

    internal object SetPropertyDynamicParameters(
      string path,
      PSObject propertyValue,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return !(this is IPropertyCmdletProvider propertyCmdletProvider) ? (object) null : propertyCmdletProvider.SetPropertyDynamicParameters(path, propertyValue);
      }
    }

    internal void ClearProperty(
      string path,
      Collection<string> propertyName,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        if (!(this is IPropertyCmdletProvider propertyCmdletProvider))
          throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "IPropertyCmdletProvider_NotSupported");
        propertyCmdletProvider.ClearProperty(path, propertyName);
      }
    }

    internal object ClearPropertyDynamicParameters(
      string path,
      Collection<string> providerSpecificPickList,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return !(this is IPropertyCmdletProvider propertyCmdletProvider) ? (object) null : propertyCmdletProvider.ClearPropertyDynamicParameters(path, providerSpecificPickList);
      }
    }

    internal void NewProperty(
      string path,
      string propertyName,
      string propertyTypeName,
      object value,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        if (!(this is IDynamicPropertyCmdletProvider propertyCmdletProvider))
          throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "IDynamicPropertyCmdletProvider_NotSupported");
        propertyCmdletProvider.NewProperty(path, propertyName, propertyTypeName, value);
      }
    }

    internal object NewPropertyDynamicParameters(
      string path,
      string propertyName,
      string propertyTypeName,
      object value,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return !(this is IDynamicPropertyCmdletProvider propertyCmdletProvider) ? (object) null : propertyCmdletProvider.NewPropertyDynamicParameters(path, propertyName, propertyTypeName, value);
      }
    }

    internal void RemoveProperty(
      string path,
      string propertyName,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        if (!(this is IDynamicPropertyCmdletProvider propertyCmdletProvider))
          throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "IDynamicPropertyCmdletProvider_NotSupported");
        propertyCmdletProvider.RemoveProperty(path, propertyName);
      }
    }

    internal object RemovePropertyDynamicParameters(
      string path,
      string propertyName,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return !(this is IDynamicPropertyCmdletProvider propertyCmdletProvider) ? (object) null : propertyCmdletProvider.RemovePropertyDynamicParameters(path, propertyName);
      }
    }

    internal void RenameProperty(
      string path,
      string propertyName,
      string newPropertyName,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        if (!(this is IDynamicPropertyCmdletProvider propertyCmdletProvider))
          throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "IDynamicPropertyCmdletProvider_NotSupported");
        propertyCmdletProvider.RenameProperty(path, propertyName, newPropertyName);
      }
    }

    internal object RenamePropertyDynamicParameters(
      string path,
      string sourceProperty,
      string destinationProperty,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return !(this is IDynamicPropertyCmdletProvider propertyCmdletProvider) ? (object) null : propertyCmdletProvider.RenamePropertyDynamicParameters(path, sourceProperty, destinationProperty);
      }
    }

    internal void CopyProperty(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(sourcePath, new object[0]))
      {
        this.Context = cmdletProviderContext;
        if (!(this is IDynamicPropertyCmdletProvider propertyCmdletProvider))
          throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "IDynamicPropertyCmdletProvider_NotSupported");
        propertyCmdletProvider.CopyProperty(sourcePath, sourceProperty, destinationPath, destinationProperty);
      }
    }

    internal object CopyPropertyDynamicParameters(
      string path,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return !(this is IDynamicPropertyCmdletProvider propertyCmdletProvider) ? (object) null : propertyCmdletProvider.CopyPropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty);
      }
    }

    internal void MoveProperty(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(sourcePath, new object[0]))
      {
        this.Context = cmdletProviderContext;
        if (!(this is IDynamicPropertyCmdletProvider propertyCmdletProvider))
          throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "IDynamicPropertyCmdletProvider_NotSupported");
        propertyCmdletProvider.MoveProperty(sourcePath, sourceProperty, destinationPath, destinationProperty);
      }
    }

    internal object MovePropertyDynamicParameters(
      string path,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return !(this is IDynamicPropertyCmdletProvider propertyCmdletProvider) ? (object) null : propertyCmdletProvider.MovePropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty);
      }
    }

    internal IContentReader GetContentReader(
      string path,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return this is IContentCmdletProvider contentCmdletProvider ? contentCmdletProvider.GetContentReader(path) : throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "IContentCmdletProvider_NotSupported");
      }
    }

    internal object GetContentReaderDynamicParameters(
      string path,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return !(this is IContentCmdletProvider contentCmdletProvider) ? (object) null : contentCmdletProvider.GetContentReaderDynamicParameters(path);
      }
    }

    internal IContentWriter GetContentWriter(
      string path,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return this is IContentCmdletProvider contentCmdletProvider ? contentCmdletProvider.GetContentWriter(path) : throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "IContentCmdletProvider_NotSupported");
      }
    }

    internal object GetContentWriterDynamicParameters(
      string path,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return !(this is IContentCmdletProvider contentCmdletProvider) ? (object) null : contentCmdletProvider.GetContentWriterDynamicParameters(path);
      }
    }

    internal void ClearContent(string path, CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        if (!(this is IContentCmdletProvider contentCmdletProvider))
          throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "IContentCmdletProvider_NotSupported");
        contentCmdletProvider.ClearContent(path);
      }
    }

    internal object ClearContentDynamicParameters(
      string path,
      CmdletProviderContext cmdletProviderContext)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = cmdletProviderContext;
        return !(this is IContentCmdletProvider contentCmdletProvider) ? (object) null : contentCmdletProvider.ClearContentDynamicParameters(path);
      }
    }

    protected virtual ProviderInfo Start(ProviderInfo providerInfo)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return providerInfo;
    }

    protected virtual object StartDynamicParameters()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected virtual void Stop()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        ;
    }

    public bool Stopping
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return this.Context.Stopping;
      }
    }

    public SessionState SessionState
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return new SessionState(this.Context.ExecutionContext.EngineSessionState);
      }
    }

    public ProviderIntrinsics InvokeProvider
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return new ProviderIntrinsics(this.Context.ExecutionContext.EngineSessionState);
      }
    }

    public CommandInvocationIntrinsics InvokeCommand
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return new CommandInvocationIntrinsics(this.Context.ExecutionContext);
      }
    }

    public PSCredential Credential
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return this.Context.Credential;
      }
    }

    protected internal ProviderInfo ProviderInfo
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return this.providerInformation;
      }
    }

    protected PSDriveInfo PSDriveInfo
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return this.Context.Drive;
      }
    }

    protected object DynamicParameters
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return this.Context.DynamicParameters;
      }
    }

    public SwitchParameter Force
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return this.Context.Force;
      }
    }

    public string Filter
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return this.Context.Filter;
      }
    }

    public Collection<string> Include
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return this.Context.Include;
      }
    }

    public Collection<string> Exclude
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return this.Context.Exclude;
      }
    }

    public PSHost Host
    {
      get
      {
        using (PSTransactionManager.GetEngineProtectionScope())
          return (PSHost) this.Context.ExecutionContext.EngineHostInterface;
      }
    }

    public virtual string GetResourceString(string baseName, string resourceId)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (string.IsNullOrEmpty(baseName))
          throw CmdletProvider.providerBaseTracer.NewArgumentException(nameof (baseName));
        if (string.IsNullOrEmpty(resourceId))
          throw CmdletProvider.providerBaseTracer.NewArgumentException(nameof (resourceId));
        ResourceManager resourceManager = ResourceManagerCache.GetResourceManager(this.GetType().Assembly, baseName);
        string str;
        try
        {
          str = resourceManager.GetString(resourceId, Thread.CurrentThread.CurrentUICulture);
        }
        catch (MissingManifestResourceException ex)
        {
          throw CmdletProvider.providerBaseTracer.NewArgumentException(nameof (baseName), "GetErrorText", "ResourceBaseNameFailure", (object) baseName);
        }
        return str != null ? str : throw CmdletProvider.providerBaseTracer.NewArgumentException(nameof (resourceId), "GetErrorText", "ResourceIdFailure", (object) resourceId);
      }
    }

    public void ThrowTerminatingError(ErrorRecord errorRecord)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (errorRecord == null)
          throw CmdletProvider.providerBaseTracer.NewArgumentNullException(nameof (errorRecord));
        if (errorRecord.ErrorDetails != null && errorRecord.ErrorDetails.TextLookupError != null)
        {
          Exception textLookupError = errorRecord.ErrorDetails.TextLookupError;
          errorRecord.ErrorDetails.TextLookupError = (Exception) null;
          MshLog.LogProviderHealthEvent(this.Context.ExecutionContext, this.ProviderInfo.Name, textLookupError, Severity.Warning);
        }
        ProviderInvocationException invocationException = new ProviderInvocationException(this.ProviderInfo, errorRecord);
        MshLog.LogProviderHealthEvent(this.Context.ExecutionContext, this.ProviderInfo.Name, (Exception) invocationException, Severity.Warning);
        throw invocationException;
      }
    }

    public bool ShouldProcess(string target)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.Context.ShouldProcess(target);
    }

    public bool ShouldProcess(string target, string action)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.Context.ShouldProcess(target, action);
    }

    public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.Context.ShouldProcess(verboseDescription, verboseWarning, caption);
    }

    public bool ShouldProcess(
      string verboseDescription,
      string verboseWarning,
      string caption,
      out ShouldProcessReason shouldProcessReason)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.Context.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
    }

    public bool ShouldContinue(string query, string caption)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.Context.ShouldContinue(query, caption);
    }

    public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.Context.ShouldContinue(query, caption, ref yesToAll, ref noToAll);
    }

    public bool TransactionAvailable()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.Context != null && this.Context.TransactionAvailable();
    }

    public PSTransactionContext CurrentPSTransaction => this.Context == null ? (PSTransactionContext) null : this.Context.CurrentPSTransaction;

    public void WriteVerbose(string text)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        this.Context.WriteVerbose(text);
    }

    public void WriteWarning(string text)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        this.Context.WriteWarning(text);
    }

    public void WriteProgress(ProgressRecord progressRecord)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (progressRecord == null)
          throw CmdletProvider.providerBaseTracer.NewArgumentNullException(nameof (progressRecord));
        this.Context.WriteProgress(progressRecord);
      }
    }

    public void WriteDebug(string text)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        this.Context.WriteDebug(text);
    }

    private void WriteObject(object item, string path, bool isContainer)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        PSObject psObject = this.WrapOutputInPSObject(item, path);
        PSNoteProperty psNoteProperty = new PSNoteProperty("PSIsContainer", (object) isContainer);
        psObject.Properties.Add((PSPropertyInfo) psNoteProperty, true);
        CmdletProvider.providerBaseTracer.WriteLine("Attaching {0} = {1}", (object) "PSIsContainer", (object) isContainer);
        this.Context.WriteObject((object) psObject);
      }
    }

    private void WriteObject(object item, string path)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
        this.Context.WriteObject((object) this.WrapOutputInPSObject(item, path));
    }

    private PSObject WrapOutputInPSObject(object item, string path)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        PSObject psObject1 = item != null ? new PSObject(item) : throw CmdletProvider.providerBaseTracer.NewArgumentNullException(nameof (item));
        if (item is PSObject psObject)
        {
          psObject1.TypeNames.Clear();
          foreach (string typeName in psObject.TypeNames)
            psObject1.TypeNames.Add(typeName);
        }
        string providerQualifiedPath = LocationGlobber.GetProviderQualifiedPath(path, this.ProviderInfo);
        PSNoteProperty psNoteProperty1 = new PSNoteProperty("PSPath", (object) providerQualifiedPath);
        psObject1.Properties.Add((PSPropertyInfo) psNoteProperty1, true);
        CmdletProvider.providerBaseTracer.WriteLine("Attaching {0} = {1}", (object) "PSPath", (object) providerQualifiedPath);
        if (this is NavigationCmdletProvider navigationCmdletProvider && path != null)
        {
          string path1 = !(this.PSDriveInfo != (PSDriveInfo) null) ? navigationCmdletProvider.GetParentPath(path, string.Empty, this.Context) : navigationCmdletProvider.GetParentPath(path, this.PSDriveInfo.Root, this.Context);
          string str = string.Empty;
          if (!string.IsNullOrEmpty(path1))
            str = LocationGlobber.GetProviderQualifiedPath(path1, this.ProviderInfo);
          PSNoteProperty psNoteProperty2 = new PSNoteProperty("PSParentPath", (object) str);
          psObject1.Properties.Add((PSPropertyInfo) psNoteProperty2, true);
          CmdletProvider.providerBaseTracer.WriteLine("Attaching {0} = {1}", (object) "PSParentPath", (object) str);
          string childName = navigationCmdletProvider.GetChildName(path, this.Context);
          PSNoteProperty psNoteProperty3 = new PSNoteProperty("PSChildName", (object) childName);
          psObject1.Properties.Add((PSPropertyInfo) psNoteProperty3, true);
          CmdletProvider.providerBaseTracer.WriteLine("Attaching {0} = {1}", (object) "PSChildName", (object) childName);
        }
        if (this.PSDriveInfo != (PSDriveInfo) null)
        {
          PSNoteProperty psNoteProperty2 = new PSNoteProperty("PSDrive", (object) this.PSDriveInfo);
          psObject1.Properties.Add((PSPropertyInfo) psNoteProperty2, true);
          CmdletProvider.providerBaseTracer.WriteLine("Attaching {0} = {1}", (object) "PSDrive", (object) this.PSDriveInfo);
        }
        PSNoteProperty psNoteProperty4 = new PSNoteProperty("PSProvider", (object) this.ProviderInfo);
        psObject1.Properties.Add((PSPropertyInfo) psNoteProperty4, true);
        CmdletProvider.providerBaseTracer.WriteLine("Attaching {0} = {1}", (object) "PSProvider", (object) this.ProviderInfo);
        return psObject1;
      }
    }

    public void WriteItemObject(object item, string path, bool isContainer)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        this.WriteObject(item, path, isContainer);
    }

    public void WritePropertyObject(object propertyValue, string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        this.WriteObject(propertyValue, path);
    }

    public void WriteSecurityDescriptorObject(ObjectSecurity securityDescriptor, string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        this.WriteObject((object) securityDescriptor, path);
    }

    public void WriteError(ErrorRecord errorRecord)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (errorRecord == null)
          throw CmdletProvider.providerBaseTracer.NewArgumentNullException(nameof (errorRecord));
        if (errorRecord.ErrorDetails != null && errorRecord.ErrorDetails.TextLookupError != null)
          MshLog.LogProviderHealthEvent(this.Context.ExecutionContext, this.ProviderInfo.Name, errorRecord.ErrorDetails.TextLookupError, Severity.Warning);
        this.Context.WriteError(errorRecord);
      }
    }

    internal void GetSecurityDescriptor(
      string path,
      AccessControlSections sections,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        ISecurityDescriptorCmdletProvider permissionProvider = this as ISecurityDescriptorCmdletProvider;
        CmdletProvider.CheckIfSecurityDescriptorInterfaceIsSupported(permissionProvider);
        permissionProvider.GetSecurityDescriptor(path, sections);
      }
    }

    internal void SetSecurityDescriptor(
      string path,
      ObjectSecurity securityDescriptor,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        ISecurityDescriptorCmdletProvider permissionProvider = this as ISecurityDescriptorCmdletProvider;
        CmdletProvider.CheckIfSecurityDescriptorInterfaceIsSupported(permissionProvider);
        permissionProvider.SetSecurityDescriptor(path, securityDescriptor);
      }
    }

    private static void CheckIfSecurityDescriptorInterfaceIsSupported(
      ISecurityDescriptorCmdletProvider permissionProvider)
    {
      if (permissionProvider == null)
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("ProviderBaseSecurity", "ISecurityDescriptorCmdletProvider_NotSupported");
    }
  }
}
