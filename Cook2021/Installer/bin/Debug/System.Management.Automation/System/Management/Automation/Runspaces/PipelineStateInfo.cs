// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PipelineStateInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class PipelineStateInfo
  {
    private PipelineState _state;
    private Exception _reason;
    [TraceSource("PipelineStateInfo", "PipelineStateInfo")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (PipelineStateInfo), nameof (PipelineStateInfo));

    internal PipelineStateInfo(PipelineState state)
      : this(state, (Exception) null)
    {
    }

    internal PipelineStateInfo(PipelineState state, Exception reason)
    {
      using (PipelineStateInfo._trace.TraceConstructor((object) this))
      {
        this._state = state;
        this._reason = reason;
      }
    }

    internal PipelineStateInfo(PipelineStateInfo pipelineStateInfo)
    {
      using (PipelineStateInfo._trace.TraceConstructor((object) this))
      {
        this._state = pipelineStateInfo.State;
        this._reason = pipelineStateInfo.Reason;
      }
    }

    public PipelineState State
    {
      get
      {
        using (PipelineStateInfo._trace.TraceProperty())
          return this._state;
      }
    }

    public Exception Reason
    {
      get
      {
        using (PipelineStateInfo._trace.TraceProperty())
          return this._reason;
      }
    }

    internal PipelineStateInfo Clone()
    {
      using (PipelineStateInfo._trace.TraceMethod())
        return new PipelineStateInfo(this);
    }
  }
}
