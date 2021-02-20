// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AliasInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public class AliasInfo : CommandInfo
  {
    [TraceSource("AliasInfo", "The command information for aliases. Aliases refer to other commands.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (AliasInfo), "The command information for aliases. Aliases refer to other commands.");
    private string _definition = string.Empty;
    private ScopedItemOptions options;
    private string description = string.Empty;
    private string unresolvedCommandName;
    private ExecutionContext _context;

    internal AliasInfo(string name, string definition, ExecutionContext context)
      : base(name, CommandTypes.Alias)
    {
      if (string.IsNullOrEmpty(definition))
        throw AliasInfo.tracer.NewArgumentException(nameof (definition));
      if (context == null)
        throw AliasInfo.tracer.NewArgumentNullException(nameof (context));
      this._definition = definition;
      this._context = context;
      this.SetModule(context.SessionState.Internal.Module);
    }

    internal AliasInfo(
      string name,
      string definition,
      ExecutionContext context,
      ScopedItemOptions options)
      : base(name, CommandTypes.Alias)
    {
      if (string.IsNullOrEmpty(definition))
        throw AliasInfo.tracer.NewArgumentException(nameof (definition));
      if (context == null)
        throw AliasInfo.tracer.NewArgumentNullException(nameof (context));
      this._definition = definition;
      this._context = context;
      this.options = options;
      this.SetModule(context.SessionState.Internal.Module);
    }

    internal override HelpCategory HelpCategory => HelpCategory.Alias;

    public CommandInfo ReferencedCommand
    {
      get
      {
        CommandInfo commandInfo = (CommandInfo) null;
        CommandSearcher commandSearcher = new CommandSearcher(this._definition, SearchResolutionOptions.ReturnFirstDuplicateCmdletName, CommandTypes.All, this._context);
        if (commandSearcher.MoveNext())
          commandInfo = ((IEnumerator<CommandInfo>) commandSearcher).Current;
        return commandInfo;
      }
    }

    public CommandInfo ResolvedCommand
    {
      get
      {
        List<string> stringList = new List<string>();
        stringList.Add(this.Name);
        string definition = this._definition;
        CommandInfo commandInfo = this.ReferencedCommand;
        while (commandInfo != null && commandInfo.CommandType == CommandTypes.Alias)
        {
          commandInfo = ((AliasInfo) commandInfo).ReferencedCommand;
          if (commandInfo is AliasInfo)
          {
            if (SessionStateUtilities.CollectionContainsValue((IEnumerable) stringList, (object) commandInfo.Name, (IComparer) StringComparer.OrdinalIgnoreCase))
            {
              commandInfo = (CommandInfo) null;
              break;
            }
            stringList.Add(commandInfo.Name);
            definition = commandInfo.Definition;
          }
        }
        if (commandInfo == null)
          this.unresolvedCommandName = definition;
        return commandInfo;
      }
    }

    public override string Definition => this._definition;

    internal void SetDefinition(string definition, bool force)
    {
      if ((this.options & ScopedItemOptions.Constant) != ScopedItemOptions.None || !force && (this.options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.Name, SessionStateCategory.Alias, "AliasNotWritable");
        AliasInfo.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      this._definition = definition;
    }

    public ScopedItemOptions Options
    {
      get => this.options;
      set => this.SetOptions(value, false);
    }

    internal void SetOptions(ScopedItemOptions newOptions, bool force)
    {
      if ((this.options & ScopedItemOptions.Constant) != ScopedItemOptions.None)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.Name, SessionStateCategory.Alias, "AliasIsConstant");
        AliasInfo.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      if (!force && (this.options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.Name, SessionStateCategory.Alias, "AliasIsReadOnly");
        AliasInfo.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      if ((newOptions & ScopedItemOptions.Constant) != ScopedItemOptions.None)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.Name, SessionStateCategory.Alias, "AliasCannotBeMadeConstant");
        AliasInfo.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      if ((newOptions & ScopedItemOptions.AllScope) == ScopedItemOptions.None && (this.options & ScopedItemOptions.AllScope) != ScopedItemOptions.None)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.Name, SessionStateCategory.Alias, "AliasAllScopeOptionCannotBeRemoved");
        AliasInfo.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      this.options = newOptions;
    }

    public string Description
    {
      get => this.description;
      set => this.description = value;
    }

    internal string UnresolvedCommandName => this.unresolvedCommandName;

    public override ReadOnlyCollection<PSTypeName> OutputType => this.ResolvedCommand?.OutputType;
  }
}
