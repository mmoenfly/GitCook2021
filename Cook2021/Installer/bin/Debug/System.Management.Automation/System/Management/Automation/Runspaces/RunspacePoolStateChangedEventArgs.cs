// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspacePoolStateChangedEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class RunspacePoolStateChangedEventArgs : EventArgs
  {
    [TraceSource("RunspacePool", "Powershell hosting interfaces")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("RunspacePool", "Powershell hosting interfaces");
    private RunspacePoolStateInfo stateInfo;

    internal RunspacePoolStateChangedEventArgs(RunspacePoolState state)
    {
      using (RunspacePoolStateChangedEventArgs.tracer.TraceConstructor((object) this))
        this.stateInfo = new RunspacePoolStateInfo(state, (Exception) null);
    }

    internal RunspacePoolStateChangedEventArgs(RunspacePoolStateInfo stateInfo) => this.stateInfo = stateInfo;

    public RunspacePoolStateInfo RunspacePoolStateInfo
    {
      get
      {
        using (RunspacePoolStateChangedEventArgs.tracer.TraceProperty())
          return this.stateInfo;
      }
    }
  }
}
