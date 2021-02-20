// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterSetPromptingData
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class ParameterSetPromptingData
  {
    private bool isDefaultSet;
    private uint parameterSet;
    private Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> pipelineableMandatoryParameters = new Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata>();
    private Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> pipelineableMandatoryByValueParameters = new Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata>();
    private Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> pipelineableMandatoryByPropertyNameParameters = new Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata>();
    private Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> nonpipelineableMandatoryParameters = new Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata>();
    private Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> nonpipelineableOptionalParameters = new Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata>();

    internal ParameterSetPromptingData(uint parameterSet, bool isDefaultSet)
    {
      this.parameterSet = parameterSet;
      this.isDefaultSet = isDefaultSet;
    }

    internal bool IsDefaultSet => this.isDefaultSet;

    internal uint ParameterSet => this.parameterSet;

    internal bool IsAllSet => this.parameterSet == uint.MaxValue;

    internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> PipelineableMandatoryParameters => this.pipelineableMandatoryParameters;

    internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> PipelineableMandatoryByValueParameters => this.pipelineableMandatoryByValueParameters;

    internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> PipelineableMandatoryByPropertyNameParameters => this.pipelineableMandatoryByPropertyNameParameters;

    internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> NonpipelineableMandatoryParameters => this.nonpipelineableMandatoryParameters;

    internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> NonpipelineableOptionalParameters => this.nonpipelineableOptionalParameters;
  }
}
