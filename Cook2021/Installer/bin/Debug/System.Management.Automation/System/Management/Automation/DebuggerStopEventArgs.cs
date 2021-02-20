// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DebuggerStopEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public class DebuggerStopEventArgs : EventArgs
  {
    private InvocationInfo invocationInfo;
    private ReadOnlyCollection<Breakpoint> breakpoints;
    private DebuggerResumeAction resumeAction;

    internal DebuggerStopEventArgs(InvocationInfo invocationInfo, List<Breakpoint> breakpoints)
    {
      this.invocationInfo = invocationInfo;
      this.breakpoints = new ReadOnlyCollection<Breakpoint>((IList<Breakpoint>) breakpoints);
      this.resumeAction = DebuggerResumeAction.Continue;
    }

    public InvocationInfo InvocationInfo => this.invocationInfo;

    public ReadOnlyCollection<Breakpoint> Breakpoints => this.breakpoints;

    public DebuggerResumeAction ResumeAction
    {
      get => this.resumeAction;
      set => this.resumeAction = value;
    }
  }
}
