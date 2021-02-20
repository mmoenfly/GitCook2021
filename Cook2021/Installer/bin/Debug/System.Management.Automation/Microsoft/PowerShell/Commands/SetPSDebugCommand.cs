// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.SetPSDebugCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Set", "PSDebug")]
  public sealed class SetPSDebugCommand : PSCmdlet
  {
    [TraceSource("Parser", "Parser")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");
    private int trace = -1;
    private bool? step;
    private bool? strict;
    private bool off;

    [Parameter(ParameterSetName = "on")]
    [ValidateRange(0, 2)]
    public int Trace
    {
      set => this.trace = value;
      get => this.trace;
    }

    [Parameter(ParameterSetName = "on")]
    public SwitchParameter Step
    {
      set => this.step = new bool?((bool) value);
      get => (SwitchParameter) this.step.Value;
    }

    [Parameter(ParameterSetName = "on")]
    public SwitchParameter Strict
    {
      set => this.strict = new bool?((bool) value);
      get => (SwitchParameter) this.strict.Value;
    }

    [Parameter(ParameterSetName = "off")]
    public SwitchParameter Off
    {
      get => (SwitchParameter) this.off;
      set => this.off = (bool) value;
    }

    protected override void BeginProcessing()
    {
      if (this.off)
      {
        this.Context.PSDebug = 0;
        this.Context.StepScript = false;
        this.Context.EngineSessionState.GlobalScope.StrictModeVersion = (Version) null;
      }
      else
      {
        if (this.trace >= 0)
          this.Context.PSDebug = this.trace;
        if (this.step.HasValue)
          this.Context.StepScript = this.step.Value;
        if (!this.strict.HasValue)
          return;
        this.Context.EngineSessionState.GlobalScope.StrictModeVersion = new Version(this.strict.Value ? 1 : 0, 0);
      }
    }
  }
}
