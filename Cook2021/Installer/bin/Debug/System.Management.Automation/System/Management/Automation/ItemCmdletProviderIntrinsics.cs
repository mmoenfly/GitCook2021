// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ItemCmdletProviderIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public sealed class ItemCmdletProviderIntrinsics
  {
    [TraceSource("ProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers");
    private Cmdlet cmdlet;
    private SessionStateInternal sessionState;

    private ItemCmdletProviderIntrinsics()
    {
    }

    internal ItemCmdletProviderIntrinsics(Cmdlet cmdlet)
    {
      this.cmdlet = cmdlet != null ? cmdlet : throw ItemCmdletProviderIntrinsics.tracer.NewArgumentNullException(nameof (cmdlet));
      this.sessionState = cmdlet.Context.EngineSessionState;
    }

    internal ItemCmdletProviderIntrinsics(SessionStateInternal sessionState) => this.sessionState = sessionState != null ? sessionState : throw ItemCmdletProviderIntrinsics.tracer.NewArgumentNullException(nameof (sessionState));

    public Collection<PSObject> Get(string path) => this.sessionState.GetItem(new string[1]
    {
      path
    }, false, false);

    public Collection<PSObject> Get(string[] path, bool force, bool literalPath) => this.sessionState.GetItem(path, force, literalPath);

    internal void Get(string path, CmdletProviderContext context) => this.sessionState.GetItem(new string[1]
    {
      path
    }, context);

    internal object GetItemDynamicParameters(string path, CmdletProviderContext context) => this.sessionState.GetItemDynamicParameters(path, context);

    public Collection<PSObject> Set(string path, object value) => this.sessionState.SetItem(new string[1]
    {
      path
    }, value, false, false);

    public Collection<PSObject> Set(
      string[] path,
      object value,
      bool force,
      bool literalPath)
    {
      return this.sessionState.SetItem(path, value, force, literalPath);
    }

    internal void Set(string path, object value, CmdletProviderContext context) => this.sessionState.SetItem(new string[1]
    {
      path
    }, value, context);

    internal object SetItemDynamicParameters(
      string path,
      object value,
      CmdletProviderContext context)
    {
      return this.sessionState.SetItemDynamicParameters(path, value, context);
    }

    public Collection<PSObject> Clear(string path) => this.sessionState.ClearItem(new string[1]
    {
      path
    }, false, false);

    public Collection<PSObject> Clear(
      string[] path,
      bool force,
      bool literalPath)
    {
      return this.sessionState.ClearItem(path, force, literalPath);
    }

    internal void Clear(string path, CmdletProviderContext context) => this.sessionState.ClearItem(new string[1]
    {
      path
    }, context);

    internal object ClearItemDynamicParameters(string path, CmdletProviderContext context) => this.sessionState.ClearItemDynamicParameters(path, context);

    public void Invoke(string path) => this.sessionState.InvokeDefaultAction(new string[1]
    {
      path
    }, false);

    public void Invoke(string[] path, bool literalPath) => this.sessionState.InvokeDefaultAction(path, literalPath);

    internal void Invoke(string path, CmdletProviderContext context) => this.sessionState.InvokeDefaultAction(new string[1]
    {
      path
    }, context);

    internal object InvokeItemDynamicParameters(string path, CmdletProviderContext context) => this.sessionState.InvokeDefaultActionDynamicParameters(path, context);

    public Collection<PSObject> Rename(string path, string newName) => this.sessionState.RenameItem(path, newName, false);

    public Collection<PSObject> Rename(string path, string newName, bool force) => this.sessionState.RenameItem(path, newName, force);

    internal void Rename(string path, string newName, CmdletProviderContext context) => this.sessionState.RenameItem(path, newName, context);

    internal object RenameItemDynamicParameters(
      string path,
      string newName,
      CmdletProviderContext context)
    {
      return this.sessionState.RenameItemDynamicParameters(path, newName, context);
    }

    public Collection<PSObject> New(
      string path,
      string name,
      string itemTypeName,
      object content)
    {
      return this.sessionState.NewItem(new string[1]{ path }, name, itemTypeName, content, false);
    }

    public Collection<PSObject> New(
      string[] path,
      string name,
      string itemTypeName,
      object content,
      bool force)
    {
      return this.sessionState.NewItem(path, name, itemTypeName, content, force);
    }

    internal void New(
      string path,
      string name,
      string type,
      object content,
      CmdletProviderContext context)
    {
      this.sessionState.NewItem(new string[1]{ path }, name, type, content, context);
    }

    internal object NewItemDynamicParameters(
      string path,
      string type,
      object content,
      CmdletProviderContext context)
    {
      return this.sessionState.NewItemDynamicParameters(path, type, content, context);
    }

    public void Remove(string path, bool recurse) => this.sessionState.RemoveItem(new string[1]
    {
      path
    }, (recurse ? 1 : 0) != 0, false, false);

    public void Remove(string[] path, bool recurse, bool force, bool literalPath) => this.sessionState.RemoveItem(path, recurse, force, literalPath);

    internal void Remove(string path, bool recurse, CmdletProviderContext context) => this.sessionState.RemoveItem(new string[1]
    {
      path
    }, (recurse ? 1 : 0) != 0, context);

    internal object RemoveItemDynamicParameters(
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      return this.sessionState.RemoveItemDynamicParameters(path, recurse, context);
    }

    public Collection<PSObject> Copy(
      string path,
      string destinationPath,
      bool recurse,
      CopyContainers copyContainers)
    {
      return this.sessionState.CopyItem(new string[1]
      {
        path
      }, destinationPath, (recurse ? 1 : 0) != 0, copyContainers, false, false);
    }

    public Collection<PSObject> Copy(
      string[] path,
      string destinationPath,
      bool recurse,
      CopyContainers copyContainers,
      bool force,
      bool literalPath)
    {
      return this.sessionState.CopyItem(path, destinationPath, recurse, copyContainers, force, literalPath);
    }

    internal void Copy(
      string path,
      string destinationPath,
      bool recurse,
      CopyContainers copyContainers,
      CmdletProviderContext context)
    {
      this.sessionState.CopyItem(new string[1]{ path }, destinationPath, (recurse ? 1 : 0) != 0, copyContainers, context);
    }

    internal object CopyItemDynamicParameters(
      string path,
      string destination,
      bool recurse,
      CmdletProviderContext context)
    {
      return this.sessionState.CopyItemDynamicParameters(path, destination, recurse, context);
    }

    public Collection<PSObject> Move(string path, string destination) => this.sessionState.MoveItem(new string[1]
    {
      path
    }, destination, false, false);

    public Collection<PSObject> Move(
      string[] path,
      string destination,
      bool force,
      bool literalPath)
    {
      return this.sessionState.MoveItem(path, destination, force, literalPath);
    }

    internal void Move(string path, string destination, CmdletProviderContext context) => this.sessionState.MoveItem(new string[1]
    {
      path
    }, destination, context);

    internal object MoveItemDynamicParameters(
      string path,
      string destination,
      CmdletProviderContext context)
    {
      return this.sessionState.MoveItemDynamicParameters(path, destination, context);
    }

    public bool Exists(string path) => this.sessionState.ItemExists(path, false, false);

    public bool Exists(string path, bool force, bool literalPath) => this.sessionState.ItemExists(path, force, literalPath);

    internal bool Exists(string path, CmdletProviderContext context) => this.sessionState.ItemExists(path, context);

    internal object ItemExistsDynamicParameters(string path, CmdletProviderContext context) => this.sessionState.ItemExistsDynamicParameters(path, context);

    public bool IsContainer(string path) => this.sessionState.IsItemContainer(path);

    internal bool IsContainer(string path, CmdletProviderContext context) => this.sessionState.IsItemContainer(path, context);
  }
}
