// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProviderIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  public sealed class ProviderIntrinsics
  {
    [TraceSource("CmdletProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CmdletProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers");
    private InternalCommand cmdlet;
    private ItemCmdletProviderIntrinsics item;
    private ChildItemCmdletProviderIntrinsics childItem;
    private ContentCmdletProviderIntrinsics content;
    private PropertyCmdletProviderIntrinsics property;
    private SecurityDescriptorCmdletProviderIntrinsics securityDescriptor;

    private ProviderIntrinsics()
    {
    }

    internal ProviderIntrinsics(Cmdlet cmdlet)
    {
      using (ProviderIntrinsics.tracer.TraceConstructor((object) this))
      {
        this.cmdlet = cmdlet != null ? (InternalCommand) cmdlet : throw ProviderIntrinsics.tracer.NewArgumentNullException(nameof (cmdlet));
        this.item = new ItemCmdletProviderIntrinsics(cmdlet);
        this.childItem = new ChildItemCmdletProviderIntrinsics(cmdlet);
        this.content = new ContentCmdletProviderIntrinsics(cmdlet);
        this.property = new PropertyCmdletProviderIntrinsics(cmdlet);
        this.securityDescriptor = new SecurityDescriptorCmdletProviderIntrinsics(cmdlet);
      }
    }

    internal ProviderIntrinsics(SessionStateInternal sessionState)
    {
      using (ProviderIntrinsics.tracer.TraceConstructor((object) this))
      {
        this.item = sessionState != null ? new ItemCmdletProviderIntrinsics(sessionState) : throw ProviderIntrinsics.tracer.NewArgumentNullException(nameof (sessionState));
        this.childItem = new ChildItemCmdletProviderIntrinsics(sessionState);
        this.content = new ContentCmdletProviderIntrinsics(sessionState);
        this.property = new PropertyCmdletProviderIntrinsics(sessionState);
        this.securityDescriptor = new SecurityDescriptorCmdletProviderIntrinsics(sessionState);
      }
    }

    public ItemCmdletProviderIntrinsics Item => this.item;

    public ChildItemCmdletProviderIntrinsics ChildItem => this.childItem;

    public ContentCmdletProviderIntrinsics Content => this.content;

    public PropertyCmdletProviderIntrinsics Property => this.property;

    public SecurityDescriptorCmdletProviderIntrinsics SecurityDescriptor => this.securityDescriptor;
  }
}
