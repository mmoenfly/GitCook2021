// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ChildItemCmdletProviderIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public sealed class ChildItemCmdletProviderIntrinsics
  {
    [TraceSource("CmdletProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CmdletProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers");
    private Cmdlet cmdlet;
    private SessionStateInternal sessionState;

    private ChildItemCmdletProviderIntrinsics()
    {
    }

    internal ChildItemCmdletProviderIntrinsics(Cmdlet cmdlet)
    {
      this.cmdlet = cmdlet != null ? cmdlet : throw ChildItemCmdletProviderIntrinsics.tracer.NewArgumentNullException(nameof (cmdlet));
      this.sessionState = cmdlet.Context.EngineSessionState;
    }

    internal ChildItemCmdletProviderIntrinsics(SessionStateInternal sessionState) => this.sessionState = sessionState != null ? sessionState : throw ChildItemCmdletProviderIntrinsics.tracer.NewArgumentNullException(nameof (sessionState));

    public Collection<PSObject> Get(string path, bool recurse) => this.sessionState.GetChildItems(new string[1]
    {
      path
    }, (recurse ? 1 : 0) != 0, false, false);

    public Collection<PSObject> Get(
      string[] path,
      bool recurse,
      bool force,
      bool literalPath)
    {
      return this.sessionState.GetChildItems(path, recurse, force, literalPath);
    }

    internal void Get(string path, bool recurse, CmdletProviderContext context) => this.sessionState.GetChildItems(path, recurse, context);

    internal object GetChildItemsDynamicParameters(
      string path,
      bool recurse,
      CmdletProviderContext context)
    {
      return this.sessionState.GetChildItemsDynamicParameters(path, recurse, context);
    }

    public Collection<string> GetNames(
      string path,
      ReturnContainers returnContainers,
      bool recurse)
    {
      return this.sessionState.GetChildNames(new string[1]
      {
        path
      }, returnContainers, (recurse ? 1 : 0) != 0, false, false);
    }

    public Collection<string> GetNames(
      string[] path,
      ReturnContainers returnContainers,
      bool recurse,
      bool force,
      bool literalPath)
    {
      return this.sessionState.GetChildNames(path, returnContainers, recurse, force, literalPath);
    }

    internal void GetNames(
      string path,
      ReturnContainers returnContainers,
      bool recurse,
      CmdletProviderContext context)
    {
      this.sessionState.GetChildNames(path, returnContainers, recurse, context);
    }

    internal object GetChildNamesDynamicParameters(string path, CmdletProviderContext context) => this.sessionState.GetChildNamesDynamicParameters(path, context);

    public bool HasChild(string path) => this.sessionState.HasChildItems(path, false, false);

    public bool HasChild(string path, bool force, bool literalPath) => this.sessionState.HasChildItems(path, force, literalPath);

    internal bool HasChild(string path, CmdletProviderContext context) => this.sessionState.HasChildItems(path, context);
  }
}
