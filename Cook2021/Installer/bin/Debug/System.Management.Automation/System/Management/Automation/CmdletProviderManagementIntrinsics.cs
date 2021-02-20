// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CmdletProviderManagementIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Provider;

namespace System.Management.Automation
{
  public sealed class CmdletProviderManagementIntrinsics
  {
    [TraceSource("ProviderCommandAPI", "The APIs that are exposed to the Cmdlet base class for manipulating providers")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ProviderCommandAPI", "The APIs that are exposed to the Cmdlet base class for manipulating providers");
    private SessionStateInternal sessionState;

    private CmdletProviderManagementIntrinsics()
    {
    }

    internal CmdletProviderManagementIntrinsics(SessionStateInternal sessionState)
    {
      using (CmdletProviderManagementIntrinsics.tracer.TraceConstructor((object) this))
        this.sessionState = sessionState != null ? sessionState : throw CmdletProviderManagementIntrinsics.tracer.NewArgumentNullException(nameof (sessionState));
    }

    public Collection<ProviderInfo> Get(string name)
    {
      using (CmdletProviderManagementIntrinsics.tracer.TraceMethod())
        return this.sessionState.GetProvider(name);
    }

    public ProviderInfo GetOne(string name)
    {
      using (CmdletProviderManagementIntrinsics.tracer.TraceMethod())
        return this.sessionState.GetSingleProvider(name);
    }

    public IEnumerable<ProviderInfo> GetAll()
    {
      using (CmdletProviderManagementIntrinsics.tracer.TraceMethod())
        return this.sessionState.ProviderList;
    }

    internal static bool CheckProviderCapabilities(
      ProviderCapabilities capability,
      ProviderInfo provider)
    {
      using (CmdletProviderManagementIntrinsics.tracer.TraceMethod(provider.Name, new object[0]))
        return (provider.Capabilities & capability) != ProviderCapabilities.None;
    }

    internal int Count
    {
      get
      {
        using (CmdletProviderManagementIntrinsics.tracer.TraceProperty((object) this.sessionState.ProviderCount))
          return this.sessionState.ProviderCount;
      }
    }
  }
}
