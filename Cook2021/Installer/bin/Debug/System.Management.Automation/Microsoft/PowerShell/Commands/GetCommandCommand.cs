// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.GetCommandCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  [OutputType(new Type[] {typeof (AliasInfo), typeof (ApplicationInfo), typeof (FunctionInfo), typeof (CmdletInfo), typeof (ExternalScriptInfo), typeof (FilterInfo), typeof (string)})]
  [Cmdlet("Get", "Command", DefaultParameterSetName = "CmdletSet")]
  public sealed class GetCommandCommand : PSCmdlet
  {
    [TraceSource("GetCommandCmdlet", "Trace output for get-command")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("GetCommandCmdlet", "Trace output for get-command");
    private string[] names;
    private bool nameContainsWildcard;
    private string[] verbs = new string[0];
    private string[] nouns = new string[0];
    private string[] _modules = new string[0];
    private CommandTypes commandType = CommandTypes.All;
    private int totalCount = -1;
    private bool usage;
    private object[] commandArgs;
    private Dictionary<string, CommandInfo> commandsWritten = new Dictionary<string, CommandInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private List<CommandInfo> accumulatedResults = new List<CommandInfo>();
    private Collection<WildcardPattern> verbPatterns;
    private Collection<WildcardPattern> nounPatterns;
    private Collection<WildcardPattern> _modulePatterns;

    [Parameter(ParameterSetName = "AllCommandSet", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public string[] Name
    {
      get => this.names;
      set
      {
        this.nameContainsWildcard = false;
        this.names = value;
        if (value == null)
          return;
        foreach (string pattern in value)
        {
          if (WildcardPattern.ContainsWildcardCharacters(pattern))
          {
            this.nameContainsWildcard = true;
            break;
          }
        }
      }
    }

    [Parameter(ParameterSetName = "CmdletSet", ValueFromPipelineByPropertyName = true)]
    public string[] Verb
    {
      get => this.verbs;
      set
      {
        if (value == null)
          value = new string[0];
        this.verbs = value;
        this.verbPatterns = (Collection<WildcardPattern>) null;
      }
    }

    [Parameter(ParameterSetName = "CmdletSet", ValueFromPipelineByPropertyName = true)]
    public string[] Noun
    {
      get => this.nouns;
      set
      {
        if (value == null)
          value = new string[0];
        this.nouns = value;
        this.nounPatterns = (Collection<WildcardPattern>) null;
      }
    }

    [Alias(new string[] {"PSSnapin"})]
    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string[] Module
    {
      get => this._modules;
      set
      {
        if (value == null)
          value = new string[0];
        this._modules = value;
        this._modulePatterns = (Collection<WildcardPattern>) null;
      }
    }

    [Parameter(ParameterSetName = "AllCommandSet", ValueFromPipelineByPropertyName = true)]
    [Alias(new string[] {"Type"})]
    public CommandTypes CommandType
    {
      get => this.commandType;
      set => this.commandType = value;
    }

    [Parameter(ValueFromPipelineByPropertyName = true)]
    public int TotalCount
    {
      get => this.totalCount;
      set => this.totalCount = value;
    }

    [Parameter(ValueFromPipelineByPropertyName = true)]
    public SwitchParameter Syntax
    {
      get => (SwitchParameter) this.usage;
      set => this.usage = (bool) value;
    }

    [Parameter(Position = 1, ValueFromRemainingArguments = true)]
    [Alias(new string[] {"Args"})]
    [AllowEmptyCollection]
    [AllowNull]
    public object[] ArgumentList
    {
      get => this.commandArgs;
      set => this.commandArgs = value;
    }

    protected override void ProcessRecord()
    {
      if (this._modulePatterns == null)
        this._modulePatterns = SessionStateUtilities.CreateWildcardsFromStrings(this.Module, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
      switch (this.ParameterSetName)
      {
        case "CmdletSet":
          this.AccumulateMatchingCmdlets();
          break;
        case "AllCommandSet":
          this.AccumulateMatchingCommands();
          break;
      }
    }

    protected override void EndProcessing()
    {
      if (this.names == null || this.nameContainsWildcard)
        this.accumulatedResults.Sort(new Comparison<CommandInfo>(GetCommandCommand.CommandInfoComparison));
      CommandOrigin commandOrigin = this.MyInvocation.CommandOrigin;
      foreach (CommandInfo accumulatedResult in this.accumulatedResults)
      {
        if (SessionState.IsVisible(commandOrigin, accumulatedResult))
        {
          if ((bool) this.Syntax)
            this.WriteObject((object) accumulatedResult.Syntax);
          else
            this.WriteObject((object) accumulatedResult);
        }
      }
    }

    private static int CommandInfoComparison(CommandInfo x, CommandInfo y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);

    private void AccumulateMatchingCmdlets()
    {
      this.CommandType = CommandTypes.Alias | CommandTypes.Function | CommandTypes.Cmdlet;
      this.AccumulateMatchingCommands(new Collection<string>()
      {
        "*"
      });
    }

    private bool IsNounVerbMatch(CommandInfo command)
    {
      bool flag = false;
      if (this.verbPatterns == null)
        this.verbPatterns = SessionStateUtilities.CreateWildcardsFromStrings(this.Verb, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
      if (this.nounPatterns == null)
        this.nounPatterns = SessionStateUtilities.CreateWildcardsFromStrings(this.Noun, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
      if (!string.IsNullOrEmpty(command.ModuleName))
      {
        if (!SessionStateUtilities.MatchesAnyWildcardPattern(command.ModuleName, (IEnumerable<WildcardPattern>) this._modulePatterns, true))
          goto label_12;
      }
      else if (this._modulePatterns.Count > 0)
        goto label_12;
      string verb;
      string noun;
      if (command is CmdletInfo cmdletInfo)
      {
        verb = cmdletInfo.Verb;
        noun = cmdletInfo.Noun;
      }
      else if (!CmdletInfo.SplitCmdletName(command.Name, out verb, out noun))
        goto label_12;
      if (SessionStateUtilities.MatchesAnyWildcardPattern(verb, (IEnumerable<WildcardPattern>) this.verbPatterns, true) && SessionStateUtilities.MatchesAnyWildcardPattern(noun, (IEnumerable<WildcardPattern>) this.nounPatterns, true))
        flag = true;
label_12:
      return flag;
    }

    private void AccumulateMatchingCommands()
    {
      Collection<string> collection = SessionStateUtilities.ConvertArrayToCollection<string>(this.Name);
      if (collection.Count == 0)
        collection.Add("*");
      this.AccumulateMatchingCommands(collection);
    }

    private void AccumulateMatchingCommands(Collection<string> commandNames)
    {
      SearchResolutionOptions options = SearchResolutionOptions.ReturnFirstDuplicateCmdletName;
      if ((this.CommandType & CommandTypes.Alias) != (CommandTypes) 0)
        options |= SearchResolutionOptions.ResolveAliasPatterns;
      if ((this.CommandType & (CommandTypes.Function | CommandTypes.Filter)) != (CommandTypes) 0)
        options |= SearchResolutionOptions.ResolveFunctionPatterns;
      int num = 0;
      foreach (string commandName in commandNames)
      {
        try
        {
          bool flag1 = WildcardPattern.ContainsWildcardCharacters(commandName);
          if (flag1)
            options |= SearchResolutionOptions.CommandNameIsPattern;
          CommandSearcher commandSearcher = new CommandSearcher(commandName, options, this.CommandType, this.Context);
          bool flag2 = false;
label_10:
          do
          {
            CommandInfo commandInfo;
            do
            {
              do
              {
                do
                {
                  try
                  {
                    if (!commandSearcher.MoveNext())
                      goto label_40;
                  }
                  catch (ArgumentException ex)
                  {
                    this.WriteError(new ErrorRecord((Exception) ex, "GetCommandInvalidArgument", ErrorCategory.SyntaxError, (object) null));
                    goto label_10;
                  }
                  catch (PathTooLongException ex)
                  {
                    this.WriteError(new ErrorRecord((Exception) ex, "GetCommandInvalidArgument", ErrorCategory.SyntaxError, (object) null));
                    goto label_10;
                  }
                  catch (FileLoadException ex)
                  {
                    this.WriteError(new ErrorRecord((Exception) ex, "GetCommandFileLoadError", ErrorCategory.ReadError, (object) null));
                    goto label_10;
                  }
                  catch (MetadataException ex)
                  {
                    this.WriteError(new ErrorRecord((Exception) ex, "GetCommandMetadataError", ErrorCategory.MetadataError, (object) null));
                    goto label_10;
                  }
                  catch (FormatException ex)
                  {
                    this.WriteError(new ErrorRecord((Exception) ex, "GetCommandBadFileFormat", ErrorCategory.InvalidData, (object) null));
                    goto label_10;
                  }
                  commandInfo = ((IEnumerator<CommandInfo>) commandSearcher).Current;
                }
                while (!SessionState.IsVisible(this.MyInvocation.CommandOrigin, commandInfo));
              }
              while (this.IsDuplicate(commandInfo));
              flag2 = true;
              if (commandInfo.CommandType == CommandTypes.Cmdlet || (this.verbs.Length > 0 || this.nouns.Length > 0) && (commandInfo.CommandType == CommandTypes.Function || commandInfo.CommandType == CommandTypes.Filter || commandInfo.CommandType == CommandTypes.Alias))
              {
                if (!this.IsNounVerbMatch(commandInfo))
                  flag2 = false;
              }
              else if (this._modulePatterns != null && this._modulePatterns.Count > 0 && !SessionStateUtilities.MatchesAnyWildcardPattern(commandInfo.ModuleName, (IEnumerable<WildcardPattern>) this._modulePatterns, true))
                flag2 = false;
            }
            while (!flag2);
            if (this.ArgumentList != null)
            {
              switch (commandInfo)
              {
                case AliasInfo aliasInfo:
                  commandInfo = aliasInfo.ResolvedCommand;
                  break;
                case CmdletInfo _:
                case IScriptCommandInfo _:
                  break;
                default:
                  this.ThrowTerminatingError(new ErrorRecord((Exception) GetCommandCommand.tracer.NewArgumentException("ArgumentList", "DiscoveryExceptions", "CommandArgsOnlyForSingleCmdlet"), "CommandArgsOnlyForSingleCmdlet", ErrorCategory.InvalidArgument, (object) commandInfo));
                  break;
              }
            }
            bool flag3 = false;
            try
            {
              flag3 = commandInfo.ImplementsDynamicParameters;
            }
            catch (PSSecurityException ex)
            {
            }
            catch (RuntimeException ex)
            {
            }
            if (flag3)
            {
              try
              {
                CommandInfo getCommandCopy = commandInfo.CreateGetCommandCopy(this.ArgumentList);
                ReadOnlyCollection<CommandParameterSetInfo> parameterSets = getCommandCopy.ParameterSets;
                commandInfo = getCommandCopy;
              }
              catch (MetadataException ex)
              {
                this.WriteError(new ErrorRecord((Exception) ex, "GetCommandMetadataError", ErrorCategory.MetadataError, (object) commandInfo));
                goto label_10;
              }
              catch (ParameterBindingException ex)
              {
                GetCommandCommand.tracer.WriteLine((object) ex);
                if (!ex.ErrorRecord.FullyQualifiedErrorId.StartsWith("GetDynamicParametersException", StringComparison.Ordinal))
                  throw;
              }
            }
            ++num;
            if (this.TotalCount < 0 || num <= this.TotalCount)
              this.accumulatedResults.Add(commandInfo);
            else
              break;
          }
          while (this.ArgumentList == null);
label_40:
          if (!flag2)
          {
            if (!flag1)
            {
              CommandNotFoundException notFoundException = new CommandNotFoundException(commandName, (Exception) null, "CommandNotFoundException", new object[0]);
              this.WriteError(new ErrorRecord(notFoundException.ErrorRecord, (Exception) notFoundException));
            }
          }
        }
        catch (CommandNotFoundException ex)
        {
          this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
        }
      }
    }

    private bool IsDuplicate(CommandInfo info)
    {
      bool flag = false;
      string key = (string) null;
      switch (info)
      {
        case ApplicationInfo applicationInfo:
          key = applicationInfo.Path;
          break;
        case CmdletInfo cmdletInfo:
          key = cmdletInfo.FullName;
          break;
        case ScriptInfo scriptInfo:
          key = scriptInfo.Definition;
          break;
        case ExternalScriptInfo externalScriptInfo:
          key = externalScriptInfo.Path;
          break;
      }
      if (key != null)
      {
        if (this.commandsWritten.ContainsKey(key))
          flag = true;
        else
          this.commandsWritten.Add(key, info);
      }
      return flag;
    }
  }
}
