// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PositionalCommandParameter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  internal class PositionalCommandParameter
  {
    [TraceSource("CommandMetadata", "The metadata associated with a bindable object in PowerShell.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CommandMetadata", "The metadata associated with a bindable object in PowerShell.");
    private MergedCompiledCommandParameter parameter;
    private Collection<ParameterSetSpecificMetadata> parameterSetData = new Collection<ParameterSetSpecificMetadata>();

    internal PositionalCommandParameter(MergedCompiledCommandParameter parameter)
    {
      using (PositionalCommandParameter.tracer.TraceConstructor((object) this))
        this.parameter = parameter;
    }

    internal MergedCompiledCommandParameter Parameter
    {
      get
      {
        using (PositionalCommandParameter.tracer.TraceProperty())
          return this.parameter;
      }
    }

    internal Collection<ParameterSetSpecificMetadata> ParameterSetData
    {
      get
      {
        using (PositionalCommandParameter.tracer.TraceProperty())
          return this.parameterSetData;
      }
    }
  }
}
