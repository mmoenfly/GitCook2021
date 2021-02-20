// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceStateEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class RunspaceStateEventArgs : EventArgs
  {
    private RunspaceStateInfo _runspaceStateInfo;
    [TraceSource("RunspaceStateEventArgs", "RunspaceStateEventArgs")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (RunspaceStateEventArgs), nameof (RunspaceStateEventArgs));

    internal RunspaceStateEventArgs(RunspaceStateInfo runspaceStateInfo) => this._runspaceStateInfo = runspaceStateInfo != null ? runspaceStateInfo : throw RunspaceStateEventArgs._trace.NewArgumentNullException(nameof (runspaceStateInfo));

    public RunspaceStateInfo RunspaceStateInfo => this._runspaceStateInfo;
  }
}
