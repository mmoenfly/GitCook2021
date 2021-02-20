// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public abstract class CommandInfo : IHasSessionStateEntryVisibility
  {
    private string name = string.Empty;
    private CommandTypes type = CommandTypes.Application;
    private ExecutionContext context;
    private CommandInfo copiedCommand;
    private SessionStateEntryVisibility _visibility;
    private string _moduleName;
    private PSModuleInfo _module;
    internal CommandMetadata _externalCommandMetadata;
    internal ReadOnlyCollection<CommandParameterSetInfo> parameterSets;
    private bool isGetCommandCopy;
    private object[] arguments;

    internal CommandInfo(string name, CommandTypes type)
    {
      this.name = name != null ? name : throw new ArgumentNullException(nameof (name));
      this.type = type;
    }

    internal CommandInfo(string name, CommandTypes type, ExecutionContext context)
      : this(name, type)
      => this.context = context != null ? context : throw new ArgumentNullException(nameof (context));

    internal CommandInfo(CommandInfo other)
    {
      this._module = other._module;
      this._visibility = other._visibility;
      this.arguments = other.arguments;
      this.context = other.context;
      this.name = other.name;
      this.type = other.type;
      this.copiedCommand = other;
    }

    public string Name => this.name;

    public CommandTypes CommandType => this.type;

    internal ExecutionContext Context => this.context;

    internal virtual HelpCategory HelpCategory => HelpCategory.None;

    internal CommandInfo CopiedCommand
    {
      get => this.copiedCommand;
      set => this.copiedCommand = value;
    }

    internal void SetCommandType(CommandTypes newType) => this.type = newType;

    public abstract string Definition { get; }

    internal void Rename(string newName) => this.name = !string.IsNullOrEmpty(newName) ? newName : throw new ArgumentNullException(nameof (newName));

    public override string ToString() => this.name;

    public virtual SessionStateEntryVisibility Visibility
    {
      get => this.copiedCommand == null ? this._visibility : this.copiedCommand.Visibility;
      set
      {
        if (this.copiedCommand == null)
          this._visibility = value;
        else
          this.copiedCommand.Visibility = value;
      }
    }

    internal virtual CommandMetadata CommandMetadata => throw new InvalidOperationException();

    internal virtual string Syntax => this.Definition;

    public string ModuleName
    {
      get
      {
        if (this._moduleName == null)
        {
          if (this._module != null && !string.IsNullOrEmpty(this._module.Name))
            this._moduleName = this._module.Name;
          else if (this is CmdletInfo cmdletInfo && cmdletInfo.PSSnapIn != null)
            this._moduleName = cmdletInfo.PSSnapInName;
          if (this._moduleName == null)
            return string.Empty;
        }
        return this._moduleName;
      }
    }

    public PSModuleInfo Module => this._module;

    internal void SetModule(PSModuleInfo module) => this._module = module;

    internal virtual bool ImplementsDynamicParameters => false;

    private MergedCommandParameterMetadata GetMergedCommandParameterMetdata()
    {
      CommandProcessor commandProcessor1;
      if (this is IScriptCommandInfo scriptCommandInfo)
      {
        commandProcessor1 = new CommandProcessor(scriptCommandInfo, this.context, true);
      }
      else
      {
        commandProcessor1 = new CommandProcessor((CmdletInfo) this, this.context);
        commandProcessor1.UseLocalScope = true;
      }
      ParameterBinderController.AddArgumentsToCommandProcessor((CommandProcessorBase) commandProcessor1, this.Arguments);
      CommandProcessorBase commandProcessor2 = this.Context.CurrentCommandProcessor;
      try
      {
        this.Context.CurrentCommandProcessor = (CommandProcessorBase) commandProcessor1;
        commandProcessor1.SetCurrentScopeToExecutionScope();
        commandProcessor1.CmdletParameterBinderController.BindCommandLineParametersNoValidation(commandProcessor1.arguments);
      }
      finally
      {
        this.Context.CurrentCommandProcessor = commandProcessor2;
        commandProcessor1.RestorePreviousScope();
      }
      return commandProcessor1.CmdletParameterBinderController.BindableParameters;
    }

    public virtual Dictionary<string, ParameterMetadata> Parameters
    {
      get
      {
        if (this.ImplementsDynamicParameters && this.Context != null)
        {
          MergedCommandParameterMetadata parameterMetdata = this.GetMergedCommandParameterMetdata();
          Dictionary<string, ParameterMetadata> dictionary = new Dictionary<string, ParameterMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
          foreach (KeyValuePair<string, MergedCompiledCommandParameter> bindableParameter in parameterMetdata.BindableParameters)
            dictionary.Add(bindableParameter.Key, new ParameterMetadata(bindableParameter.Value.Parameter));
          return dictionary;
        }
        if (this._externalCommandMetadata == null)
          this._externalCommandMetadata = new CommandMetadata(this, true);
        return this._externalCommandMetadata.Parameters;
      }
    }

    public ReadOnlyCollection<CommandParameterSetInfo> ParameterSets
    {
      get
      {
        if (this.parameterSets == null)
          this.parameterSets = new ReadOnlyCollection<CommandParameterSetInfo>((IList<CommandParameterSetInfo>) this.GenerateCommandParameterSetInfo());
        return this.parameterSets;
      }
    }

    public abstract ReadOnlyCollection<PSTypeName> OutputType { get; }

    internal virtual CommandInfo CreateGetCommandCopy(object[] argumentList) => throw new InvalidOperationException();

    internal Collection<CommandParameterSetInfo> GenerateCommandParameterSetInfo() => !this.IsGetCommandCopy || !this.ImplementsDynamicParameters ? CommandInfo.GetCacheableMetadata(this.CommandMetadata) : CommandInfo.GetParameterMetadata(this.CommandMetadata, this.GetMergedCommandParameterMetdata());

    internal bool IsGetCommandCopy
    {
      get => this.isGetCommandCopy;
      set => this.isGetCommandCopy = value;
    }

    internal object[] Arguments
    {
      get => this.arguments;
      set => this.arguments = value;
    }

    internal static Collection<CommandParameterSetInfo> GetCacheableMetadata(
      CommandMetadata metadata)
    {
      return CommandInfo.GetParameterMetadata(metadata, metadata.StaticCommandParameterMetadata);
    }

    internal static Collection<CommandParameterSetInfo> GetParameterMetadata(
      CommandMetadata metadata,
      MergedCommandParameterMetadata parameterMetadata)
    {
      Collection<CommandParameterSetInfo> collection = new Collection<CommandParameterSetInfo>();
      int parameterSetCount = parameterMetadata.ParameterSetCount;
      if (parameterSetCount == 0)
      {
        string name = "__AllParameterSets";
        collection.Add(new CommandParameterSetInfo(name, false, uint.MaxValue, parameterMetadata));
      }
      else
      {
        for (int index = 0; index < parameterSetCount; ++index)
        {
          uint num = (uint) (1 << index);
          string parameterSetName = parameterMetadata.GetParameterSetName(num);
          bool isDefaultParameterSet = ((int) num & (int) metadata.DefaultParameterSetFlag) != 0;
          collection.Add(new CommandParameterSetInfo(parameterSetName, isDefaultParameterSet, num, parameterMetadata));
        }
      }
      return collection;
    }
  }
}
