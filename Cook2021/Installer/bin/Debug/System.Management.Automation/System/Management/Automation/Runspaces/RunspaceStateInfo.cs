// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceStateInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class RunspaceStateInfo
  {
    private RunspaceState _state;
    private Exception _reason;
    [TraceSource("RunspaceStateInfo", "RunspaceStateInfo")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (RunspaceStateInfo), nameof (RunspaceStateInfo));

    internal RunspaceStateInfo(RunspaceState state)
      : this(state, (Exception) null)
    {
    }

    internal RunspaceStateInfo(RunspaceState state, Exception reason)
    {
      this._state = state;
      this._reason = reason;
    }

    internal RunspaceStateInfo(RunspaceStateInfo runspaceStateInfo)
    {
      this._state = runspaceStateInfo.State;
      this._reason = runspaceStateInfo.Reason;
    }

    public RunspaceState State => this._state;

    public Exception Reason => this._reason;

    public override string ToString() => this._state.ToString();

    internal RunspaceStateInfo Clone() => new RunspaceStateInfo(this);
  }
}
