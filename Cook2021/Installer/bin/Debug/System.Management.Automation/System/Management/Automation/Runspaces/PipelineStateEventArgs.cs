// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PipelineStateEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class PipelineStateEventArgs : EventArgs
  {
    private PipelineStateInfo _pipelineStateInfo;
    [TraceSource("PipelineStateEventArgs", "PipelineStateEventArgs")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (PipelineStateEventArgs), nameof (PipelineStateEventArgs));

    internal PipelineStateEventArgs(PipelineStateInfo pipelineStateInfo)
    {
      using (PipelineStateEventArgs._trace.TraceConstructor((object) this))
        this._pipelineStateInfo = pipelineStateInfo;
    }

    public PipelineStateInfo PipelineStateInfo => this._pipelineStateInfo;
  }
}
