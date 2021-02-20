// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HelpRequest
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class HelpRequest
  {
    private ProviderContext _providerContext;
    private string _target;
    private HelpCategory _helpCategory;
    private string _provider;
    private int _maxResults = -1;
    private string[] _component;
    private string[] _role;
    private string[] _functionality;
    private CommandOrigin _origin;
    [TraceSource("HelpRequest", "HelpRequest")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (HelpRequest), nameof (HelpRequest));

    internal HelpRequest(string target, HelpCategory helpCategory)
    {
      this._target = target;
      this._helpCategory = helpCategory;
      this._origin = CommandOrigin.Runspace;
    }

    internal HelpRequest Clone() => new HelpRequest(this.Target, this.HelpCategory)
    {
      Provider = this.Provider,
      MaxResults = this.MaxResults,
      Component = this.Component,
      Role = this.Role,
      Functionality = this.Functionality,
      ProviderContext = this.ProviderContext,
      CommandOrigin = this.CommandOrigin
    };

    internal ProviderContext ProviderContext
    {
      get => this._providerContext;
      set => this._providerContext = value;
    }

    internal string Target
    {
      get => this._target;
      set => this._target = value;
    }

    internal HelpCategory HelpCategory
    {
      get => this._helpCategory;
      set => this._helpCategory = value;
    }

    internal string Provider
    {
      get => this._provider;
      set => this._provider = value;
    }

    internal int MaxResults
    {
      get => this._maxResults;
      set => this._maxResults = value;
    }

    internal string[] Component
    {
      get => this._component;
      set => this._component = value;
    }

    internal string[] Role
    {
      get => this._role;
      set => this._role = value;
    }

    internal string[] Functionality
    {
      get => this._functionality;
      set => this._functionality = value;
    }

    internal CommandOrigin CommandOrigin
    {
      get => this._origin;
      set => this._origin = value;
    }

    internal void Validate()
    {
      if (string.IsNullOrEmpty(this._target) && this._helpCategory == HelpCategory.None && (string.IsNullOrEmpty(this._provider) && this._component == null) && (this._role == null && this._functionality == null))
      {
        this._target = "default";
        this._helpCategory = HelpCategory.DefaultHelp;
      }
      else
      {
        if (string.IsNullOrEmpty(this._target))
          this._target = string.IsNullOrEmpty(this._provider) || this._helpCategory != HelpCategory.None && this._helpCategory != HelpCategory.Provider ? "*" : this._provider;
        if ((this._component != null || this._role != null || this._functionality != null) && this._helpCategory == HelpCategory.None)
        {
          this._helpCategory = HelpCategory.Alias | HelpCategory.Cmdlet | HelpCategory.ScriptCommand | HelpCategory.Function | HelpCategory.Filter | HelpCategory.ExternalScript;
        }
        else
        {
          if ((this._helpCategory & HelpCategory.Cmdlet) > HelpCategory.None)
            this._helpCategory |= HelpCategory.Alias;
          if (this._helpCategory == HelpCategory.None)
            this._helpCategory = HelpCategory.All;
          this._helpCategory &= ~HelpCategory.DefaultHelp;
        }
      }
    }
  }
}
