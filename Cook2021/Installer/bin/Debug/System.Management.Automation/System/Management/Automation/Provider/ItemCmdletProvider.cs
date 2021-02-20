// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Provider.ItemCmdletProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation.Provider
{
  public abstract class ItemCmdletProvider : DriveCmdletProvider
  {
    internal void GetItem(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        this.GetItem(path);
      }
    }

    internal object GetItemDynamicParameters(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.GetItemDynamicParameters(path);
      }
    }

    internal void SetItem(string path, object value, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        CmdletProvider.providerBaseTracer.WriteLine("ItemCmdletProvider.SetItem", new object[0]);
        this.Context = context;
        this.SetItem(path, value);
      }
    }

    internal object SetItemDynamicParameters(
      string path,
      object value,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.SetItemDynamicParameters(path, value);
      }
    }

    internal void ClearItem(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        CmdletProvider.providerBaseTracer.WriteLine("ItemCmdletProvider.ClearItem", new object[0]);
        this.Context = context;
        this.ClearItem(path);
      }
    }

    internal object ClearItemDynamicParameters(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.ClearItemDynamicParameters(path);
      }
    }

    internal void InvokeDefaultAction(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        CmdletProvider.providerBaseTracer.WriteLine("ItemCmdletProvider.InvokeDefaultAction", new object[0]);
        this.Context = context;
        this.InvokeDefaultAction(path);
      }
    }

    internal object InvokeDefaultActionDynamicParameters(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.InvokeDefaultActionDynamicParameters(path);
      }
    }

    internal bool ItemExists(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        bool flag = false;
        try
        {
          flag = this.ItemExists(path);
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
        }
        return flag;
      }
    }

    internal object ItemExistsDynamicParameters(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.ItemExistsDynamicParameters(path);
      }
    }

    internal bool IsValidPath(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.IsValidPath(path);
      }
    }

    internal string[] ExpandPath(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.ExpandPath(path);
      }
    }

    protected virtual void GetItem(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual object GetItemDynamicParameters(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected virtual void SetItem(string path, object value)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual object SetItemDynamicParameters(string path, object value)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected virtual void ClearItem(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual object ClearItemDynamicParameters(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected virtual void InvokeDefaultAction(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual object InvokeDefaultActionDynamicParameters(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected virtual bool ItemExists(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual object ItemExistsDynamicParameters(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected abstract bool IsValidPath(string path);

    protected virtual string[] ExpandPath(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return new string[1]{ path };
    }
  }
}
