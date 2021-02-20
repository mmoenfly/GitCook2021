// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SecurityDescriptorCmdletProviderIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Security.AccessControl;

namespace System.Management.Automation
{
  public sealed class SecurityDescriptorCmdletProviderIntrinsics
  {
    [TraceSource("ProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers");
    private Cmdlet cmdlet;
    private SessionStateInternal sessionState;

    private SecurityDescriptorCmdletProviderIntrinsics()
    {
    }

    internal SecurityDescriptorCmdletProviderIntrinsics(Cmdlet cmdlet)
    {
      using (SecurityDescriptorCmdletProviderIntrinsics.tracer.TraceConstructor((object) this))
      {
        this.cmdlet = cmdlet != null ? cmdlet : throw SecurityDescriptorCmdletProviderIntrinsics.tracer.NewArgumentNullException(nameof (cmdlet));
        this.sessionState = cmdlet.Context.EngineSessionState;
      }
    }

    internal SecurityDescriptorCmdletProviderIntrinsics(SessionStateInternal sessionState)
    {
      using (SecurityDescriptorCmdletProviderIntrinsics.tracer.TraceConstructor((object) this))
        this.sessionState = sessionState != null ? sessionState : throw SecurityDescriptorCmdletProviderIntrinsics.tracer.NewArgumentNullException(nameof (sessionState));
    }

    public Collection<PSObject> Get(
      string path,
      AccessControlSections includeSections)
    {
      using (SecurityDescriptorCmdletProviderIntrinsics.tracer.TraceMethod())
        return this.sessionState.GetSecurityDescriptor(path, includeSections);
    }

    internal void Get(
      string path,
      AccessControlSections includeSections,
      CmdletProviderContext context)
    {
      using (SecurityDescriptorCmdletProviderIntrinsics.tracer.TraceMethod())
        this.sessionState.GetSecurityDescriptor(path, includeSections, context);
    }

    public Collection<PSObject> Set(string path, ObjectSecurity sd)
    {
      using (SecurityDescriptorCmdletProviderIntrinsics.tracer.TraceMethod())
        return this.sessionState.SetSecurityDescriptor(path, sd);
    }

    internal void Set(string path, ObjectSecurity sd, CmdletProviderContext context)
    {
      using (SecurityDescriptorCmdletProviderIntrinsics.tracer.TraceMethod())
        this.sessionState.SetSecurityDescriptor(path, sd, context);
    }

    public ObjectSecurity NewFromPath(
      string path,
      AccessControlSections includeSections)
    {
      using (SecurityDescriptorCmdletProviderIntrinsics.tracer.TraceMethod())
        return this.sessionState.NewSecurityDescriptorFromPath(path, includeSections);
    }

    public ObjectSecurity NewOfType(
      string providerId,
      string type,
      AccessControlSections includeSections)
    {
      using (SecurityDescriptorCmdletProviderIntrinsics.tracer.TraceMethod())
        return this.sessionState.NewSecurityDescriptorOfType(providerId, type, includeSections);
    }
  }
}
