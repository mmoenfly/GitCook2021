// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSInvocationSettings
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;
using System.Security.Principal;
using System.Threading;

namespace System.Management.Automation
{
  public sealed class PSInvocationSettings
  {
    [TraceSource("PowerShellHosting", "Powershell hosting interfaces")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PowerShellHosting", "Powershell hosting interfaces");
    private ApartmentState apartmentState;
    private PSHost host;
    private RemoteStreamOptions remoteStreamOptions;
    private bool addToHistory;
    private bool flowImpersonationPolicy;
    private WindowsIdentity windowsIdentityToImpersonate;

    public PSInvocationSettings()
    {
      this.apartmentState = ApartmentState.Unknown;
      this.host = (PSHost) null;
      this.remoteStreamOptions = (RemoteStreamOptions) 0;
      this.addToHistory = false;
    }

    public ApartmentState ApartmentState
    {
      get
      {
        using (PSInvocationSettings.tracer.TraceProperty())
          return this.apartmentState;
      }
      set
      {
        using (PSInvocationSettings.tracer.TraceProperty())
          this.apartmentState = value;
      }
    }

    public PSHost Host
    {
      get
      {
        using (PSInvocationSettings.tracer.TraceProperty())
          return this.host;
      }
      set
      {
        using (PSInvocationSettings.tracer.TraceProperty())
          this.host = value != null ? value : throw PSInvocationSettings.tracer.NewArgumentNullException(nameof (Host));
      }
    }

    public RemoteStreamOptions RemoteStreamOptions
    {
      get => this.remoteStreamOptions;
      set => this.remoteStreamOptions = value;
    }

    public bool AddToHistory
    {
      get => this.addToHistory;
      set => this.addToHistory = value;
    }

    internal bool FlowImpersonationPolicy
    {
      get => this.flowImpersonationPolicy;
      set => this.flowImpersonationPolicy = value;
    }

    internal WindowsIdentity WindowsIdentityToImpersonate
    {
      get => this.windowsIdentityToImpersonate;
      set => this.windowsIdentityToImpersonate = value;
    }
  }
}
