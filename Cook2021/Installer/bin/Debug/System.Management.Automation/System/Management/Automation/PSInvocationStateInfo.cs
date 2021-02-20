// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSInvocationStateInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public sealed class PSInvocationStateInfo
  {
    private PSInvocationState executionState;
    private Exception exceptionReason;
    [TraceSource("PowerShellHosting", "Powershell hosting interfaces")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PowerShellHosting", "Powershell hosting interfaces");

    internal PSInvocationStateInfo(PSInvocationState state, Exception reason)
    {
      using (PSInvocationStateInfo.tracer.TraceConstructor((object) this))
      {
        this.executionState = state;
        this.exceptionReason = reason;
      }
    }

    internal PSInvocationStateInfo(PipelineStateInfo pipelineStateInfo)
    {
      using (PSInvocationStateInfo.tracer.TraceConstructor((object) this))
      {
        this.executionState = (PSInvocationState) pipelineStateInfo.State;
        this.exceptionReason = pipelineStateInfo.Reason;
      }
    }

    public PSInvocationState State
    {
      get
      {
        using (PSInvocationStateInfo.tracer.TraceProperty())
          return this.executionState;
      }
    }

    public Exception Reason
    {
      get
      {
        using (PSInvocationStateInfo.tracer.TraceProperty())
          return this.exceptionReason;
      }
    }

    internal PSInvocationStateInfo Clone()
    {
      using (PSInvocationStateInfo.tracer.TraceMethod())
        return new PSInvocationStateInfo(this.executionState, this.exceptionReason);
    }
  }
}
