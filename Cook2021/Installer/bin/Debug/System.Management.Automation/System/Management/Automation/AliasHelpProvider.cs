// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AliasHelpProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class AliasHelpProvider : HelpProvider
  {
    private SessionState _sessionState;
    private CommandDiscovery _commandDiscovery;
    [TraceSource("AliasHelpProvider", "AliasHelpProvider")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (AliasHelpProvider), nameof (AliasHelpProvider));

    internal AliasHelpProvider(HelpSystem helpSystem)
      : base(helpSystem)
    {
      using (AliasHelpProvider.tracer.TraceConstructor((object) this))
      {
        this._sessionState = helpSystem.ExecutionContext.SessionState;
        this._commandDiscovery = helpSystem.ExecutionContext.CommandDiscovery;
      }
    }

    internal override string Name
    {
      get
      {
        using (AliasHelpProvider.tracer.TraceProperty())
          return "Alias Help Provider";
      }
    }

    internal override HelpCategory HelpCategory
    {
      get
      {
        using (AliasHelpProvider.tracer.TraceProperty())
          return HelpCategory.Alias;
      }
    }

    internal override IEnumerable<HelpInfo> ExactMatchHelp(
      HelpRequest helpRequest)
    {
      using (AliasHelpProvider.tracer.TraceMethod())
      {
        CommandInfo commandInfo = (CommandInfo) null;
        try
        {
          commandInfo = this._commandDiscovery.LookupCommandInfo(helpRequest.Target);
        }
        catch (CommandNotFoundException ex)
        {
        }
        if (commandInfo != null && commandInfo.CommandType == CommandTypes.Alias)
        {
          AliasInfo aliasInfo = (AliasInfo) commandInfo;
          HelpInfo helpInfo = (HelpInfo) AliasHelpInfo.GetHelpInfo(aliasInfo);
          if (helpInfo != null)
            yield return helpInfo;
        }
      }
    }

    internal override IEnumerable<HelpInfo> SearchHelp(
      HelpRequest helpRequest,
      bool searchOnlyContent)
    {
      using (AliasHelpProvider.tracer.TraceMethod())
      {
        if (!searchOnlyContent)
        {
          string target = helpRequest.Target;
          string pattern = target;
          if (!WildcardPattern.ContainsWildcardCharacters(target))
          {
            // ISSUE: reference to a compiler-generated field
            this.\u003Cpattern\u003E5__a += "*";
          }
          WildcardPattern matcher = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
          IDictionary<string, AliasInfo> aliasTable = this._sessionState.Internal.GetAliasTable();
          foreach (string key in (IEnumerable<string>) aliasTable.Keys)
          {
            if (matcher.IsMatch(key))
            {
              HelpRequest exactMatchHelpRequest = helpRequest.Clone();
              exactMatchHelpRequest.Target = key;
              foreach (HelpInfo helpInfo in this.ExactMatchHelp(exactMatchHelpRequest))
              {
                if (AliasHelpProvider.Match(helpInfo, helpRequest))
                  yield return helpInfo;
              }
            }
          }
        }
      }
    }

    private static bool Match(HelpInfo helpInfo, HelpRequest helpRequest) => helpRequest == null || (helpRequest.HelpCategory & helpInfo.HelpCategory) != HelpCategory.None && AliasHelpProvider.Match(helpInfo.Component, helpRequest.Component) && (AliasHelpProvider.Match(helpInfo.Role, helpRequest.Role) && AliasHelpProvider.Match(helpInfo.Functionality, helpRequest.Functionality));

    private static bool Match(string target, string[] patterns)
    {
      if (patterns == null || patterns.Length == 0)
        return true;
      foreach (string pattern in patterns)
      {
        if (AliasHelpProvider.Match(target, pattern))
          return true;
      }
      return false;
    }

    private static bool Match(string target, string pattern)
    {
      if (string.IsNullOrEmpty(pattern))
        return true;
      if (string.IsNullOrEmpty(target))
        target = "";
      return new WildcardPattern(pattern, WildcardOptions.IgnoreCase).IsMatch(target);
    }
  }
}
