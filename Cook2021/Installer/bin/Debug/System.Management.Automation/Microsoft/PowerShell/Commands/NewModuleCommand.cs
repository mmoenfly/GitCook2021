// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.NewModuleCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  [OutputType(new Type[] {typeof (PSModuleInfo)})]
  [System.Management.Automation.Cmdlet("New", "Module", DefaultParameterSetName = "ScriptBlock")]
  public sealed class NewModuleCommand : ModuleCmdletBase
  {
    private string _name;
    private ScriptBlock _scriptBlock;
    private string[] _functionImportList = new string[0];
    private string[] _cmdletImportList = new string[0];
    private bool _returnResult;
    private bool _asCustomObject;
    private object[] _arguments;

    [Parameter(Mandatory = true, ParameterSetName = "Name", Position = 0, ValueFromPipeline = true)]
    public string Name
    {
      set => this._name = value;
      get => this._name;
    }

    [Parameter(Mandatory = true, ParameterSetName = "ScriptBlock", Position = 0)]
    [ValidateNotNull]
    [Parameter(Mandatory = true, ParameterSetName = "Name", Position = 1)]
    public ScriptBlock ScriptBlock
    {
      get => this._scriptBlock;
      set => this._scriptBlock = value;
    }

    [Parameter]
    [ValidateNotNull]
    public string[] Function
    {
      set
      {
        if (value == null)
          return;
        this._functionImportList = value;
        this.BaseFunctionPatterns = new List<WildcardPattern>();
        foreach (string functionImport in this._functionImportList)
          this.BaseFunctionPatterns.Add(new WildcardPattern(functionImport, WildcardOptions.IgnoreCase));
      }
      get => this._functionImportList;
    }

    [Parameter]
    [ValidateNotNull]
    public string[] Cmdlet
    {
      set
      {
        if (value == null)
          return;
        this._cmdletImportList = value;
        this.BaseCmdletPatterns = new List<WildcardPattern>();
        foreach (string cmdletImport in this._cmdletImportList)
          this.BaseCmdletPatterns.Add(new WildcardPattern(cmdletImport, WildcardOptions.IgnoreCase));
      }
      get => this._cmdletImportList;
    }

    [Parameter]
    public SwitchParameter ReturnResult
    {
      get => (SwitchParameter) this._returnResult;
      set => this._returnResult = (bool) value;
    }

    [Parameter]
    public SwitchParameter AsCustomObject
    {
      get => (SwitchParameter) this._asCustomObject;
      set => this._asCustomObject = (bool) value;
    }

    [Parameter(ValueFromRemainingArguments = true)]
    [Alias(new string[] {"Args"})]
    public object[] ArgumentList
    {
      get => this._arguments;
      set => this._arguments = value;
    }

    protected override void EndProcessing()
    {
      if (this._scriptBlock == null)
        return;
      string path = Guid.NewGuid().ToString();
      if (string.IsNullOrEmpty(this._name))
        this._name = "__DynamicModule_" + path;
      try
      {
        this.Context.Modules.IncrementModuleNestingDepth((PSCmdlet) this, this._name);
        ArrayList results = (ArrayList) null;
        PSModuleInfo sourceModule = (PSModuleInfo) null;
        try
        {
          sourceModule = this.Context.Modules.CreateModule(this._name, path, this._scriptBlock, (SessionState) null, out results, this._arguments);
          if (!sourceModule.SessionState.Internal.UseExportList)
          {
            List<WildcardPattern> cmdletPatterns = this.BaseCmdletPatterns != null ? this.BaseCmdletPatterns : this.MatchAll;
            List<WildcardPattern> functionPatterns = this.BaseFunctionPatterns != null ? this.BaseFunctionPatterns : this.MatchAll;
            ModuleIntrinsics.ExportModuleMembers((PSCmdlet) this, sourceModule.SessionState.Internal, functionPatterns, cmdletPatterns, this.BaseAliasPatterns, this.BaseVariablePatterns);
          }
        }
        catch (RuntimeException ex)
        {
          ex.ErrorRecord.PreserveInvocationInfoOnce = true;
          this.WriteError(ex.ErrorRecord);
        }
        if (sourceModule == null)
          return;
        if (this._returnResult)
        {
          this.ImportModuleMembers(sourceModule, string.Empty);
          this.WriteObject((object) results, true);
        }
        else if (this._asCustomObject)
        {
          this.WriteObject((object) sourceModule.AsCustomObject());
        }
        else
        {
          this.ImportModuleMembers(sourceModule, string.Empty);
          this.WriteObject((object) sourceModule);
        }
      }
      finally
      {
        this.Context.Modules.DecrementModuleNestingCount();
      }
    }
  }
}
