// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSDebugContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  public class PSDebugContext
  {
    private InvocationInfo invocationInfo;
    private Breakpoint[] breakpoints;

    internal PSDebugContext(InvocationInfo invocationInfo, List<Breakpoint> breakpoints)
    {
      this.invocationInfo = invocationInfo;
      this.breakpoints = breakpoints.ToArray();
    }

    public InvocationInfo InvocationInfo => this.invocationInfo;

    public Breakpoint[] Breakpoints => this.breakpoints;
  }
}
