// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PropertyCmdletProviderIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public sealed class PropertyCmdletProviderIntrinsics
  {
    [TraceSource("ProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers");
    private Cmdlet cmdlet;
    private SessionStateInternal sessionState;

    private PropertyCmdletProviderIntrinsics()
    {
    }

    internal PropertyCmdletProviderIntrinsics(Cmdlet cmdlet)
    {
      this.cmdlet = cmdlet != null ? cmdlet : throw PropertyCmdletProviderIntrinsics.tracer.NewArgumentNullException(nameof (cmdlet));
      this.sessionState = cmdlet.Context.EngineSessionState;
    }

    internal PropertyCmdletProviderIntrinsics(SessionStateInternal sessionState) => this.sessionState = sessionState != null ? sessionState : throw PropertyCmdletProviderIntrinsics.tracer.NewArgumentNullException(nameof (sessionState));

    public Collection<PSObject> Get(
      string path,
      Collection<string> providerSpecificPickList)
    {
      return this.sessionState.GetProperty(new string[1]
      {
        path
      }, providerSpecificPickList, false);
    }

    public Collection<PSObject> Get(
      string[] path,
      Collection<string> providerSpecificPickList,
      bool literalPath)
    {
      return this.sessionState.GetProperty(path, providerSpecificPickList, literalPath);
    }

    internal void Get(
      string path,
      Collection<string> providerSpecificPickList,
      CmdletProviderContext context)
    {
      this.sessionState.GetProperty(new string[1]{ path }, providerSpecificPickList, context);
    }

    internal object GetPropertyDynamicParameters(
      string path,
      Collection<string> providerSpecificPickList,
      CmdletProviderContext context)
    {
      return this.sessionState.GetPropertyDynamicParameters(path, providerSpecificPickList, context);
    }

    public Collection<PSObject> Set(string path, PSObject propertyValue) => this.sessionState.SetProperty(new string[1]
    {
      path
    }, propertyValue, false, false);

    public Collection<PSObject> Set(
      string[] path,
      PSObject propertyValue,
      bool force,
      bool literalPath)
    {
      return this.sessionState.SetProperty(path, propertyValue, force, literalPath);
    }

    internal void Set(string path, PSObject propertyValue, CmdletProviderContext context) => this.sessionState.SetProperty(new string[1]
    {
      path
    }, propertyValue, context);

    internal object SetPropertyDynamicParameters(
      string path,
      PSObject propertyValue,
      CmdletProviderContext context)
    {
      return this.sessionState.SetPropertyDynamicParameters(path, propertyValue, context);
    }

    public void Clear(string path, Collection<string> propertyToClear) => this.sessionState.ClearProperty(new string[1]
    {
      path
    }, propertyToClear, false, false);

    public void Clear(
      string[] path,
      Collection<string> propertyToClear,
      bool force,
      bool literalPath)
    {
      this.sessionState.ClearProperty(path, propertyToClear, force, literalPath);
    }

    internal void Clear(
      string path,
      Collection<string> propertyToClear,
      CmdletProviderContext context)
    {
      this.sessionState.ClearProperty(new string[1]{ path }, propertyToClear, context);
    }

    internal object ClearPropertyDynamicParameters(
      string path,
      Collection<string> propertyToClear,
      CmdletProviderContext context)
    {
      return this.sessionState.ClearPropertyDynamicParameters(path, propertyToClear, context);
    }

    public Collection<PSObject> New(
      string path,
      string propertyName,
      string propertyTypeName,
      object value)
    {
      return this.sessionState.NewProperty(new string[1]
      {
        path
      }, propertyName, propertyTypeName, value, false, false);
    }

    public Collection<PSObject> New(
      string[] path,
      string propertyName,
      string propertyTypeName,
      object value,
      bool force,
      bool literalPath)
    {
      return this.sessionState.NewProperty(path, propertyName, propertyTypeName, value, force, literalPath);
    }

    internal void New(
      string path,
      string propertyName,
      string type,
      object value,
      CmdletProviderContext context)
    {
      this.sessionState.NewProperty(new string[1]{ path }, propertyName, type, value, context);
    }

    internal object NewPropertyDynamicParameters(
      string path,
      string propertyName,
      string type,
      object value,
      CmdletProviderContext context)
    {
      return this.sessionState.NewPropertyDynamicParameters(path, propertyName, type, value, context);
    }

    public void Remove(string path, string propertyName) => this.sessionState.RemoveProperty(new string[1]
    {
      path
    }, propertyName, false, false);

    public void Remove(string[] path, string propertyName, bool force, bool literalPath) => this.sessionState.RemoveProperty(path, propertyName, force, literalPath);

    internal void Remove(string path, string propertyName, CmdletProviderContext context) => this.sessionState.RemoveProperty(new string[1]
    {
      path
    }, propertyName, context);

    internal object RemovePropertyDynamicParameters(
      string path,
      string propertyName,
      CmdletProviderContext context)
    {
      return this.sessionState.RemovePropertyDynamicParameters(path, propertyName, context);
    }

    public Collection<PSObject> Rename(
      string path,
      string sourceProperty,
      string destinationProperty)
    {
      return this.sessionState.RenameProperty(new string[1]
      {
        path
      }, sourceProperty, destinationProperty, false, false);
    }

    public Collection<PSObject> Rename(
      string[] path,
      string sourceProperty,
      string destinationProperty,
      bool force,
      bool literalPath)
    {
      return this.sessionState.RenameProperty(path, sourceProperty, destinationProperty, force, literalPath);
    }

    internal void Rename(
      string path,
      string sourceProperty,
      string destinationProperty,
      CmdletProviderContext context)
    {
      this.sessionState.RenameProperty(new string[1]{ path }, sourceProperty, destinationProperty, context);
    }

    internal object RenamePropertyDynamicParameters(
      string path,
      string sourceProperty,
      string destinationProperty,
      CmdletProviderContext context)
    {
      return this.sessionState.RenamePropertyDynamicParameters(path, sourceProperty, destinationProperty, context);
    }

    public Collection<PSObject> Copy(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty)
    {
      return this.sessionState.CopyProperty(new string[1]
      {
        sourcePath
      }, sourceProperty, destinationPath, destinationProperty, false, false);
    }

    public Collection<PSObject> Copy(
      string[] sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      bool force,
      bool literalPath)
    {
      return this.sessionState.CopyProperty(sourcePath, sourceProperty, destinationPath, destinationProperty, force, literalPath);
    }

    internal void Copy(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      this.sessionState.CopyProperty(new string[1]
      {
        sourcePath
      }, sourceProperty, destinationPath, destinationProperty, context);
    }

    internal object CopyPropertyDynamicParameters(
      string path,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      return this.sessionState.CopyPropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty, context);
    }

    public Collection<PSObject> Move(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty)
    {
      return this.sessionState.MoveProperty(new string[1]
      {
        sourcePath
      }, sourceProperty, destinationPath, destinationProperty, false, false);
    }

    public Collection<PSObject> Move(
      string[] sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      bool force,
      bool literalPath)
    {
      return this.sessionState.MoveProperty(sourcePath, sourceProperty, destinationPath, destinationProperty, force, literalPath);
    }

    internal void Move(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      this.sessionState.MoveProperty(new string[1]
      {
        sourcePath
      }, sourceProperty, destinationPath, destinationProperty, context);
    }

    internal object MovePropertyDynamicParameters(
      string path,
      string sourceProperty,
      string destinationPath,
      string destinationProperty,
      CmdletProviderContext context)
    {
      return this.sessionState.MovePropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty, context);
    }
  }
}
