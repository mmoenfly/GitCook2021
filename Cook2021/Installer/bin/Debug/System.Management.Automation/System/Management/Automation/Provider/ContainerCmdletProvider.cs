// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Provider.ContainerCmdletProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation.Provider
{
  public abstract class ContainerCmdletProvider : ItemCmdletProvider
  {
    internal void GetChildItems(string path, bool recurse, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        this.GetChildItems(path, recurse);
      }
    }

    internal object GetChildItemsDynamicParameters(
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.GetChildItemsDynamicParameters(path, recurse);
      }
    }

    internal void GetChildNames(
      string path,
      ReturnContainers returnContainers,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        this.GetChildNames(path, returnContainers);
      }
    }

    internal virtual bool ConvertPath(
      string path,
      string filter,
      ref string updatedPath,
      ref string updatedFilter,
      CmdletProviderContext context)
    {
      this.Context = context;
      return this.ConvertPath(path, filter, ref updatedPath, ref updatedFilter);
    }

    internal object GetChildNamesDynamicParameters(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.GetChildNamesDynamicParameters(path);
      }
    }

    internal void RenameItem(string path, string newName, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        this.RenameItem(path, newName);
      }
    }

    internal object RenameItemDynamicParameters(
      string path,
      string newName,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.RenameItemDynamicParameters(path, newName);
      }
    }

    internal void NewItem(
      string path,
      string type,
      object newItemValue,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        this.NewItem(path, type, newItemValue);
      }
    }

    internal object NewItemDynamicParameters(
      string path,
      string type,
      object newItemValue,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.NewItemDynamicParameters(path, type, newItemValue);
      }
    }

    internal void RemoveItem(string path, bool recurse, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        this.RemoveItem(path, recurse);
      }
    }

    internal object RemoveItemDynamicParameters(
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.RemoveItemDynamicParameters(path, recurse);
      }
    }

    internal bool HasChildItems(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.HasChildItems(path);
      }
    }

    internal void CopyItem(
      string path,
      string copyPath,
      bool recurse,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        this.CopyItem(path, copyPath, recurse);
      }
    }

    internal object CopyItemDynamicParameters(
      string path,
      string destination,
      bool recurse,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.CopyItemDynamicParameters(path, destination, recurse);
      }
    }

    protected virtual void GetChildItems(string path, bool recurse)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual object GetChildItemsDynamicParameters(string path, bool recurse)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected virtual void GetChildNames(string path, ReturnContainers returnContainers)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual bool ConvertPath(
      string path,
      string filter,
      ref string updatedPath,
      ref string updatedFilter)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return false;
    }

    protected virtual object GetChildNamesDynamicParameters(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected virtual void RenameItem(string path, string newName)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual object RenameItemDynamicParameters(string path, string newName)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected virtual void NewItem(string path, string itemTypeName, object newItemValue)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual object NewItemDynamicParameters(
      string path,
      string itemTypeName,
      object newItemValue)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected virtual void RemoveItem(string path, bool recurse)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual object RemoveItemDynamicParameters(string path, bool recurse)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected virtual bool HasChildItems(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual void CopyItem(string path, string copyPath, bool recurse)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual object CopyItemDynamicParameters(
      string path,
      string destination,
      bool recurse)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }
  }
}
