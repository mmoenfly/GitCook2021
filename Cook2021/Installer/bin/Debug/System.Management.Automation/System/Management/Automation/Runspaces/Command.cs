// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.Command
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
  public sealed class Command
  {
    private PipelineResultTypes _mergeUnclaimedPreviousCommandResults;
    private PipelineResultTypes _mergeMyResult;
    private PipelineResultTypes _mergeToResult;
    private CommandParameterCollection _parameters = new CommandParameterCollection();
    private string _command = string.Empty;
    private bool _isScript;
    private bool? _useLocalScope;
    [TraceSource("Command", "Command base class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (Command), "Command base class");

    public Command(string command)
      : this(command, false, new bool?())
    {
    }

    public Command(string command, bool isScript)
      : this(command, isScript, new bool?())
    {
    }

    public Command(string command, bool isScript, bool useLocalScope)
    {
      this._command = command != null ? command : throw Command._trace.NewArgumentNullException(nameof (command));
      this._isScript = isScript;
      this._useLocalScope = new bool?(useLocalScope);
    }

    internal Command(string command, bool isScript, bool? useLocalScope)
    {
      this._command = command != null ? command : throw Command._trace.NewArgumentNullException(nameof (command));
      this._isScript = isScript;
      this._useLocalScope = useLocalScope;
    }

    internal Command(
      string command,
      bool isScript,
      bool? useLocalScope,
      bool mergeUnclaimedPreviousErrorResults)
      : this(command, isScript, useLocalScope)
    {
      if (!mergeUnclaimedPreviousErrorResults)
        return;
      this._mergeUnclaimedPreviousCommandResults = PipelineResultTypes.Output | PipelineResultTypes.Error;
    }

    internal Command(Command command)
    {
      this._isScript = command._isScript;
      this._useLocalScope = command._useLocalScope;
      this._command = command._command;
      this._mergeMyResult = command._mergeMyResult;
      this._mergeToResult = command._mergeToResult;
      this._mergeUnclaimedPreviousCommandResults = command._mergeUnclaimedPreviousCommandResults;
      foreach (CommandParameter parameter in (Collection<CommandParameter>) command.Parameters)
        this.Parameters.Add(new CommandParameter(parameter.Name, parameter.Value));
    }

    public CommandParameterCollection Parameters => this._parameters;

    public string CommandText => this._command;

    public bool IsScript => this._isScript;

    public bool UseLocalScope => this._useLocalScope ?? false;

    internal bool? UseLocalScopeNullable => this._useLocalScope;

    internal Command Clone() => new Command(this);

    public override string ToString() => this._command;

    public PipelineResultTypes MergeUnclaimedPreviousCommandResults
    {
      get => this._mergeUnclaimedPreviousCommandResults;
      set
      {
        if (value == PipelineResultTypes.None)
          this._mergeUnclaimedPreviousCommandResults = value;
        else
          this._mergeUnclaimedPreviousCommandResults = value == (PipelineResultTypes.Output | PipelineResultTypes.Error) ? value : throw Command._trace.NewNotSupportedException();
      }
    }

    internal PipelineResultTypes MergeMyResult => this._mergeMyResult;

    internal PipelineResultTypes MergeToResult => this._mergeToResult;

    public void MergeMyResults(PipelineResultTypes myResult, PipelineResultTypes toResult)
    {
      if (myResult == PipelineResultTypes.None && toResult == PipelineResultTypes.None)
      {
        this._mergeMyResult = myResult;
        this._mergeToResult = toResult;
      }
      else
      {
        if (myResult != PipelineResultTypes.Error)
          throw Command._trace.NewArgumentException(nameof (myResult), "Runspace", "InvalidValueMyResult");
        if (toResult != PipelineResultTypes.Output)
          throw Command._trace.NewArgumentException(nameof (myResult), "Runspace", "InvalidValueToResult");
        this._mergeMyResult = myResult;
        this._mergeToResult = toResult;
      }
    }

    private void SetMergeSettingsOnCommandProcessor(CommandProcessorBase commandProcessor)
    {
      MshCommandRuntime commandRuntime = commandProcessor.Command.commandRuntime as MshCommandRuntime;
      if (this._mergeUnclaimedPreviousCommandResults != PipelineResultTypes.None && commandRuntime != null)
        commandRuntime.MergeUnclaimedPreviousErrorResults = true;
      if (this._mergeMyResult == PipelineResultTypes.None || this._mergeToResult == PipelineResultTypes.None)
        return;
      commandRuntime.MergeMyErrorOutputWithSuccess = true;
    }

    internal CommandProcessorBase CreateCommandProcessor(
      ExecutionContext executionContext,
      CommandFactory commandFactory,
      bool addToHistory)
    {
      CommandProcessorBase commandProcessor;
      if (this.IsScript)
      {
        string commandText = this.CommandText;
        ExecutionContext context = executionContext;
        bool? useLocalScope = this._useLocalScope;
        int num1 = useLocalScope.HasValue ? (useLocalScope.GetValueOrDefault() ? 1 : 0) : 0;
        int num2 = addToHistory ? 1 : 0;
        commandProcessor = (CommandProcessorBase) new ScriptCommandProcessor(commandText, context, false, num1 != 0, num2 != 0, CommandOrigin.Runspace);
      }
      else
      {
        if (this._useLocalScope.HasValue && !this._useLocalScope.Value)
        {
          switch (executionContext.LanguageMode)
          {
            case PSLanguageMode.RestrictedLanguage:
            case PSLanguageMode.NoLanguage:
              throw new RuntimeException(ResourceManagerCache.FormatResourceString("Runspace", "UseLocalScopeNotAllowed", (object) "UseLocalScope", (object) PSLanguageMode.RestrictedLanguage.ToString(), (object) PSLanguageMode.NoLanguage.ToString()));
          }
        }
        commandProcessor = commandFactory.CreateCommand(this.CommandText, CommandOrigin.Runspace, this._useLocalScope);
      }
      CommandParameterCollection parameters = this.Parameters;
      if (parameters != null)
      {
        foreach (CommandParameter publicParameter in (Collection<CommandParameter>) parameters)
        {
          CommandParameterInternal parameterInternal = CommandParameter.ToCommandParameterInternal(publicParameter);
          commandProcessor.AddParameter(parameterInternal);
        }
      }
      string helpTarget;
      HelpCategory helpCategory;
      if (commandProcessor.IsHelpRequested(out helpTarget, out helpCategory))
        commandProcessor = CommandProcessorBase.CreateGetHelpCommandProcessor(executionContext, helpTarget, helpCategory);
      this.SetMergeSettingsOnCommandProcessor(commandProcessor);
      return commandProcessor;
    }

    internal static Command FromPSObjectForRemoting(PSObject commandAsPSObject)
    {
      Command command = commandAsPSObject != null ? new Command(RemotingDecoder.GetPropertyValue<string>(commandAsPSObject, "Cmd"), RemotingDecoder.GetPropertyValue<bool>(commandAsPSObject, "IsScript"), RemotingDecoder.GetPropertyValue<bool?>(commandAsPSObject, "UseLocalScope")) : throw Command._trace.NewArgumentNullException(nameof (commandAsPSObject));
      PipelineResultTypes propertyValue1 = RemotingDecoder.GetPropertyValue<PipelineResultTypes>(commandAsPSObject, "MergeMyResult");
      PipelineResultTypes propertyValue2 = RemotingDecoder.GetPropertyValue<PipelineResultTypes>(commandAsPSObject, "MergeToResult");
      command.MergeMyResults(propertyValue1, propertyValue2);
      command.MergeUnclaimedPreviousCommandResults = RemotingDecoder.GetPropertyValue<PipelineResultTypes>(commandAsPSObject, "MergePreviousResults");
      foreach (PSObject parameterAsPSObject in RemotingDecoder.EnumerateListProperty<PSObject>(commandAsPSObject, "Args"))
        command.Parameters.Add(CommandParameter.FromPSObjectForRemoting(parameterAsPSObject));
      return command;
    }

    internal PSObject ToPSObjectForRemoting()
    {
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Cmd", (object) this.CommandText));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("IsScript", (object) this.IsScript));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("UseLocalScope", (object) this.UseLocalScopeNullable));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("MergeMyResult", (object) this.MergeMyResult));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("MergeToResult", (object) this.MergeToResult));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("MergePreviousResults", (object) this.MergeUnclaimedPreviousCommandResults));
      List<PSObject> psObjectList = new List<PSObject>(this.Parameters.Count);
      foreach (CommandParameter parameter in (Collection<CommandParameter>) this.Parameters)
        psObjectList.Add(parameter.ToPSObjectForRemoting());
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Args", (object) psObjectList));
      return emptyPsObject;
    }
  }
}
