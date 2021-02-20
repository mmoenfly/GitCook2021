// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSInvocationStateChangedEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public sealed class PSInvocationStateChangedEventArgs : EventArgs
  {
    private PSInvocationStateInfo executionStateInfo;
    [TraceSource("PowerShellHosting", "Powershell hosting interfaces")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PowerShellHosting", "Powershell hosting interfaces");

    internal PSInvocationStateChangedEventArgs(PSInvocationStateInfo psStateInfo)
    {
      using (PSInvocationStateChangedEventArgs.tracer.TraceConstructor((object) this))
        this.executionStateInfo = psStateInfo;
    }

    public PSInvocationStateInfo InvocationStateInfo
    {
      get
      {
        using (PSInvocationStateChangedEventArgs.tracer.TraceProperty())
          return this.executionStateInfo;
      }
    }
  }
}
