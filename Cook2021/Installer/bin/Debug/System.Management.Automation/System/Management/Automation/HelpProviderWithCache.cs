// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HelpProviderWithCache
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;

namespace System.Management.Automation
{
  internal abstract class HelpProviderWithCache : HelpProvider
  {
    private Hashtable _helpCache = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
    private bool _hasCustomMatch;
    private bool _cacheFullyLoaded;

    internal HelpProviderWithCache(HelpSystem helpSystem)
      : base(helpSystem)
    {
    }

    internal override IEnumerable<HelpInfo> ExactMatchHelp(
      HelpRequest helpRequest)
    {
      string target = helpRequest.Target;
      if (!this.HasCustomMatch)
      {
        if (this._helpCache.Contains((object) target))
          yield return (HelpInfo) this._helpCache[(object) target];
      }
      else
      {
        foreach (string key in (IEnumerable) this._helpCache.Keys)
        {
          if (this.CustomMatch(target, key))
            yield return (HelpInfo) this._helpCache[(object) key];
        }
      }
      if (!this.CacheFullyLoaded)
      {
        this.DoExactMatchHelp(helpRequest);
        if (this._helpCache.Contains((object) target))
          yield return (HelpInfo) this._helpCache[(object) target];
      }
    }

    protected bool HasCustomMatch
    {
      get => this._hasCustomMatch;
      set => this._hasCustomMatch = value;
    }

    protected virtual bool CustomMatch(string target, string key) => target == key;

    internal virtual void DoExactMatchHelp(HelpRequest helpRequest)
    {
    }

    internal override IEnumerable<HelpInfo> SearchHelp(
      HelpRequest helpRequest,
      bool searchOnlyContent)
    {
      string target = helpRequest.Target;
      string wildcardpattern = this.GetWildCardPattern(target);
      HelpRequest searchHelpRequest = helpRequest.Clone();
      searchHelpRequest.Target = wildcardpattern;
      if (!this.CacheFullyLoaded)
      {
        IEnumerable<HelpInfo> result = this.DoSearchHelp(searchHelpRequest);
        if (result != null)
        {
          foreach (HelpInfo helpInfo in result)
            yield return helpInfo;
        }
      }
      else
      {
        int countOfHelpInfoObjectsFound = 0;
        WildcardPattern helpMatchter = new WildcardPattern(wildcardpattern, WildcardOptions.IgnoreCase);
        foreach (string key in (IEnumerable) this._helpCache.Keys)
        {
          if (!searchOnlyContent && helpMatchter.IsMatch(key) || searchOnlyContent && ((HelpInfo) this._helpCache[(object) key]).MatchPatternInContent(helpMatchter))
          {
            ++countOfHelpInfoObjectsFound;
            yield return (HelpInfo) this._helpCache[(object) key];
            if (helpRequest.MaxResults > 0 && countOfHelpInfoObjectsFound >= helpRequest.MaxResults)
              break;
          }
        }
      }
    }

    internal virtual string GetWildCardPattern(string target) => WildcardPattern.ContainsWildcardCharacters(target) ? target : "*" + target + "*";

    internal virtual IEnumerable<HelpInfo> DoSearchHelp(HelpRequest helpRequest)
    {
      yield break;
    }

    internal void AddCache(string target, HelpInfo helpInfo) => this._helpCache[(object) target] = (object) helpInfo;

    internal HelpInfo GetCache(string target) => (HelpInfo) this._helpCache[(object) target];

    protected internal bool CacheFullyLoaded
    {
      get => this._cacheFullyLoaded;
      set => this._cacheFullyLoaded = value;
    }

    internal override void Reset()
    {
      base.Reset();
      this._helpCache.Clear();
      this._cacheFullyLoaded = false;
    }
  }
}
