// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.FunctionInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;

namespace System.Management.Automation
{
  public class FunctionInfo : CommandInfo, IScriptCommandInfo
  {
    [TraceSource("FunctionInfo", "The command information for MSH functions.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (FunctionInfo), "The command information for MSH functions.");
    private ScriptBlock _function;
    private ScopedItemOptions options;
    private string description = string.Empty;
    private CommandMetadata commandMetadata;

    internal FunctionInfo(string name, ScriptBlock function, ExecutionContext context)
      : base(name, CommandTypes.Function, context)
    {
      this._function = function != null ? function : throw FunctionInfo.tracer.NewArgumentNullException(nameof (function));
      this.SetModule(function.Module);
    }

    internal FunctionInfo(
      string name,
      ScriptBlock function,
      ScopedItemOptions options,
      ExecutionContext context)
      : base(name, CommandTypes.Function, context)
    {
      this._function = function != null ? function : throw FunctionInfo.tracer.NewArgumentNullException(nameof (function));
      this.SetModule(function.Module);
      this.options = options;
    }

    internal FunctionInfo(FunctionInfo other)
      : base((CommandInfo) other)
    {
      this._function = other._function;
      this.description = other.description;
      this.options = other.options;
    }

    internal override CommandInfo CreateGetCommandCopy(object[] arguments)
    {
      FunctionInfo functionInfo = new FunctionInfo(this);
      functionInfo.IsGetCommandCopy = true;
      functionInfo.Arguments = arguments;
      return (CommandInfo) functionInfo;
    }

    internal override HelpCategory HelpCategory => HelpCategory.Function;

    public ScriptBlock ScriptBlock => this._function;

    internal void SetScriptBlock(ScriptBlock function, bool force)
    {
      if (function == null)
        throw FunctionInfo.tracer.NewArgumentNullException(nameof (function));
      if ((this.options & ScopedItemOptions.Constant) != ScopedItemOptions.None)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.Name, SessionStateCategory.Function, "FunctionIsConstant");
        FunctionInfo.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      if (!force && (this.options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.Name, SessionStateCategory.Function, "FunctionIsReadOnly");
        FunctionInfo.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      this._function = function;
      this.SetModule(function.Module);
      this.commandMetadata = (CommandMetadata) null;
      this.parameterSets = (ReadOnlyCollection<CommandParameterSetInfo>) null;
      this._externalCommandMetadata = (CommandMetadata) null;
    }

    public bool CmdletBinding => this.ScriptBlock.UsesCmdletBinding;

    public string DefaultParameterSet => this.CmdletBinding ? this.CommandMetadata.DefaultParameterSetName : (string) null;

    public override string Definition => this._function.ToString();

    public ScopedItemOptions Options
    {
      get => this.CopiedCommand == null ? this.options : ((FunctionInfo) this.CopiedCommand).Options;
      set
      {
        if (this.CopiedCommand == null)
        {
          if ((this.options & ScopedItemOptions.Constant) != ScopedItemOptions.None)
          {
            SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.Name, SessionStateCategory.Function, "FunctionIsConstant");
            FunctionInfo.tracer.TraceException((Exception) unauthorizedAccessException);
            throw unauthorizedAccessException;
          }
          if ((value & ScopedItemOptions.Constant) != ScopedItemOptions.None)
          {
            SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.Name, SessionStateCategory.Function, "FunctionCannotBeMadeConstant");
            FunctionInfo.tracer.TraceException((Exception) unauthorizedAccessException);
            throw unauthorizedAccessException;
          }
          if ((value & ScopedItemOptions.AllScope) == ScopedItemOptions.None && (this.options & ScopedItemOptions.AllScope) != ScopedItemOptions.None)
          {
            SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.Name, SessionStateCategory.Function, "FunctionAllScopeOptionCannotBeRemoved");
            FunctionInfo.tracer.TraceException((Exception) unauthorizedAccessException);
            throw unauthorizedAccessException;
          }
          this.options = value;
        }
        else
          ((FunctionInfo) this.CopiedCommand).Options = value;
      }
    }

    public string Description
    {
      get => this.CopiedCommand == null ? this.description : ((FunctionInfo) this.CopiedCommand).Description;
      set
      {
        if (this.CopiedCommand == null)
          this.description = value;
        else
          ((FunctionInfo) this.CopiedCommand).Description = value;
      }
    }

    internal override string Syntax
    {
      get
      {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (CommandParameterSetInfo parameterSet in this.ParameterSets)
          stringBuilder.AppendLine(string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, "{0} {1}", (object) this.Name, (object) parameterSet.ToString()));
        return stringBuilder.ToString();
      }
    }

    internal override bool ImplementsDynamicParameters => this.ScriptBlock.DynamicParams != null;

    internal override CommandMetadata CommandMetadata
    {
      get
      {
        if (this.commandMetadata == null)
          this.commandMetadata = new CommandMetadata(this.ScriptBlock, this.Name, LocalPipeline.GetExecutionContextFromTLS());
        return this.commandMetadata;
      }
    }

    public override ReadOnlyCollection<PSTypeName> OutputType => this.ScriptBlock.OutputType;
  }
}
