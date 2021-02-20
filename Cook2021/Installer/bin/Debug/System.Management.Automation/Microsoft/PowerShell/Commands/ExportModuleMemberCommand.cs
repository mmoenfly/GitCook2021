// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ExportModuleMemberCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  [System.Management.Automation.Cmdlet("Export", "ModuleMember")]
  public sealed class ExportModuleMemberCommand : PSCmdlet
  {
    [TraceSource("Parser", "Parser")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");
    private string[] _functionList;
    private List<WildcardPattern> _functionPatterns;
    private string[] _cmdletList;
    private List<WildcardPattern> _cmdletPatterns;
    private string[] _variableExportList;
    private List<WildcardPattern> _variablePatterns;
    private string[] _aliasExportList;
    private List<WildcardPattern> _aliasPatterns;

    [AllowEmptyCollection]
    [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string[] Function
    {
      set
      {
        this._functionList = value;
        this._functionPatterns = new List<WildcardPattern>();
        if (this._functionList == null)
          return;
        foreach (string function in this._functionList)
          this._functionPatterns.Add(new WildcardPattern(function, WildcardOptions.IgnoreCase));
      }
      get => this._functionList;
    }

    [Parameter(ValueFromPipelineByPropertyName = true)]
    [AllowEmptyCollection]
    public string[] Cmdlet
    {
      set
      {
        this._cmdletList = value;
        this._cmdletPatterns = new List<WildcardPattern>();
        if (this._cmdletList == null)
          return;
        foreach (string cmdlet in this._cmdletList)
          this._cmdletPatterns.Add(new WildcardPattern(cmdlet, WildcardOptions.IgnoreCase));
      }
      get => this._cmdletList;
    }

    [ValidateNotNull]
    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string[] Variable
    {
      set
      {
        this._variableExportList = value;
        this._variablePatterns = new List<WildcardPattern>();
        if (this._variableExportList == null)
          return;
        foreach (string variableExport in this._variableExportList)
          this._variablePatterns.Add(new WildcardPattern(variableExport, WildcardOptions.IgnoreCase));
      }
      get => this._variableExportList;
    }

    [ValidateNotNull]
    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string[] Alias
    {
      set
      {
        this._aliasExportList = value;
        this._aliasPatterns = new List<WildcardPattern>();
        if (this._aliasExportList == null)
          return;
        foreach (string aliasExport in this._aliasExportList)
          this._aliasPatterns.Add(new WildcardPattern(aliasExport, WildcardOptions.IgnoreCase));
      }
      get => this._aliasExportList;
    }

    protected override void ProcessRecord()
    {
      if (this.Context.EngineSessionState == this.Context.TopLevelSessionState)
        this.ThrowTerminatingError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "CanOnlyBeUsedFromWithinAModule")), "Modules_CanOnlyExecuteExportModuleMemberInsideAModule", ErrorCategory.PermissionDenied, (object) null));
      ModuleIntrinsics.ExportModuleMembers((PSCmdlet) this, this.Context.EngineSessionState, this._functionPatterns, this._cmdletPatterns, this._aliasPatterns, this._variablePatterns);
    }
  }
}
