// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.ShouldProcessParameters
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  public sealed class ShouldProcessParameters
  {
    [TraceSource("ShouldProcessParameters", "This class is used to expose the ShouldProcess parameters to the command line")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ShouldProcessParameters), "This class is used to expose the ShouldProcess parameters to the command line");
    private MshCommandRuntime commandRuntime;

    internal ShouldProcessParameters(MshCommandRuntime commandRuntime)
    {
      using (ShouldProcessParameters.tracer.TraceConstructor((object) this))
        this.commandRuntime = commandRuntime != null ? commandRuntime : throw ShouldProcessParameters.tracer.NewArgumentNullException(nameof (commandRuntime));
    }

    [Alias(new string[] {"wi"})]
    [Parameter]
    public SwitchParameter WhatIf
    {
      get
      {
        using (ShouldProcessParameters.tracer.TraceProperty((object) this.commandRuntime.WhatIf))
          return this.commandRuntime.WhatIf;
      }
      set
      {
        using (ShouldProcessParameters.tracer.TraceProperty((object) value))
          this.commandRuntime.WhatIf = value;
      }
    }

    [Parameter]
    [Alias(new string[] {"cf"})]
    public SwitchParameter Confirm
    {
      get
      {
        using (ShouldProcessParameters.tracer.TraceProperty((object) this.commandRuntime.Confirm))
          return this.commandRuntime.Confirm;
      }
      set
      {
        using (ShouldProcessParameters.tracer.TraceProperty((object) value))
          this.commandRuntime.Confirm = value;
      }
    }
  }
}
