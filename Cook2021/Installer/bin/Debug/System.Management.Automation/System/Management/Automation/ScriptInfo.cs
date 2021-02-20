// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public class ScriptInfo : CommandInfo, IScriptCommandInfo
  {
    [TraceSource("ScriptInfo", "The command information for MSH scripts that are built into the minishell.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ScriptInfo), "The command information for MSH scripts that are built into the minishell.");
    private ScriptBlock script;
    private CommandMetadata commandMetadata;

    internal ScriptInfo(string name, ScriptBlock script, ExecutionContext context)
      : base(name, CommandTypes.Script, context)
      => this.script = script != null ? script : throw ScriptInfo.tracer.NewArgumentException(nameof (script));

    internal ScriptInfo(ScriptInfo other)
      : base((CommandInfo) other)
      => this.script = other.script;

    internal override CommandInfo CreateGetCommandCopy(object[] argumentList)
    {
      ScriptInfo scriptInfo = new ScriptInfo(this);
      scriptInfo.IsGetCommandCopy = true;
      scriptInfo.Arguments = argumentList;
      return (CommandInfo) scriptInfo;
    }

    internal override HelpCategory HelpCategory => HelpCategory.ScriptCommand;

    public ScriptBlock ScriptBlock => this.script;

    public override string Definition => this.script.ToString();

    public override ReadOnlyCollection<PSTypeName> OutputType => this.ScriptBlock.OutputType;

    public override string ToString() => this.script.ToString();

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
  }
}
