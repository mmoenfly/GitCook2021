// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.SessionStateFunctionEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class SessionStateFunctionEntry : SessionStateCommandEntry
  {
    private string _definition;
    private ScriptBlock _scriptBlock;
    private ScopedItemOptions _options;

    public SessionStateFunctionEntry(string name, string definition, ScopedItemOptions options)
      : base(name, SessionStateEntryVisibility.Public)
    {
      this._definition = definition;
      this._commandType = CommandTypes.Function;
      this._options = options;
    }

    internal SessionStateFunctionEntry(
      string name,
      string definition,
      ScopedItemOptions options,
      SessionStateEntryVisibility visibility)
      : base(name, visibility)
    {
      this._definition = definition;
      this._commandType = CommandTypes.Function;
      this._options = options;
    }

    public SessionStateFunctionEntry(string name, string definition)
      : base(name)
    {
      this._definition = definition;
      this._commandType = CommandTypes.Function;
      this._scriptBlock = ScriptBlock.Create(this._definition);
      MergedCommandParameterMetadata parameterMetadata = this._scriptBlock.ParameterMetadata;
      RuntimeDefinedParameterDictionary definedParameters = this._scriptBlock.RuntimeDefinedParameters;
    }

    public override InitialSessionStateEntry Clone()
    {
      SessionStateFunctionEntry stateFunctionEntry = new SessionStateFunctionEntry(this.Name, this._definition, this._options, this.Visibility);
      stateFunctionEntry._scriptBlock = this._scriptBlock;
      stateFunctionEntry.SetModule(this.Module);
      return (InitialSessionStateEntry) stateFunctionEntry;
    }

    public string Definition => this._definition;

    internal ScriptBlock ScriptBlock
    {
      get => this._scriptBlock;
      set => this._scriptBlock = value;
    }

    public ScopedItemOptions Options => this._options;
  }
}
