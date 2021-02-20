// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.PSPrincipal
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Security.Principal;

namespace System.Management.Automation.Remoting
{
  public sealed class PSPrincipal : IPrincipal
  {
    private PSIdentity psIdentity;
    private WindowsIdentity windowsIdentity;

    public PSIdentity Identity => this.psIdentity;

    public WindowsIdentity WindowsIdentity => this.windowsIdentity;

    IIdentity IPrincipal.Identity => (IIdentity) this.Identity;

    public bool IsInRole(string role) => this.windowsIdentity != null && new WindowsPrincipal(this.windowsIdentity).IsInRole(role);

    internal bool IsInRole(WindowsBuiltInRole role) => this.windowsIdentity != null && new WindowsPrincipal(this.windowsIdentity).IsInRole(role);

    internal PSPrincipal(PSIdentity psIdentity, WindowsIdentity windowsIdentity)
    {
      this.psIdentity = psIdentity;
      this.windowsIdentity = windowsIdentity;
    }
  }
}
