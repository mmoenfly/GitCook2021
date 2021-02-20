// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HelpSystem
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace System.Management.Automation
{
  internal class HelpSystem
  {
    private ExecutionContext _executionContext;
    private Collection<ErrorRecord> _lastErrors = new Collection<ErrorRecord>();
    private HelpCategory _lastHelpCategory;
    private bool _verboseHelpErrors;
    private Collection<string> _searchPaths;
    private ArrayList _helpProviders = new ArrayList();
    private HelpErrorTracer _helpErrorTracer;
    private CultureInfo _culture;
    [TraceSource("HelpSystem", "HelpSystem")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (HelpSystem), nameof (HelpSystem));

    internal HelpSystem(ExecutionContext context)
    {
      this._executionContext = context != null ? context : throw HelpSystem.tracer.NewArgumentNullException(nameof (ExecutionContext));
      this.Initialize();
    }

    internal ExecutionContext ExecutionContext => this._executionContext;

    internal void Initialize()
    {
      this._verboseHelpErrors = LanguagePrimitives.IsTrue(this._executionContext.GetVariable("VerboseHelpErrors", (object) false));
      this._helpErrorTracer = new HelpErrorTracer(this);
      this.InitializeHelpProviders();
    }

    internal IEnumerable<HelpInfo> GetHelp(HelpRequest helpRequest)
    {
      if (helpRequest == null)
        return (IEnumerable<HelpInfo>) null;
      helpRequest.Validate();
      this.ValidateHelpCulture();
      return this.DoGetHelp(helpRequest);
    }

    internal Collection<ErrorRecord> LastErrors => this._lastErrors;

    internal HelpCategory LastHelpCategory => this._lastHelpCategory;

    internal bool VerboseHelpErrors => this._verboseHelpErrors;

    internal Collection<string> GetSearchPaths()
    {
      if (this._searchPaths != null)
        return this._searchPaths;
      this._searchPaths = new Collection<string>();
      if (this.ExecutionContext.RunspaceConfiguration is RunspaceConfigForSingleShell runspaceConfiguration)
      {
        MshConsoleInfo consoleInfo = runspaceConfiguration.ConsoleInfo;
        if (consoleInfo == null || consoleInfo.ExternalPSSnapIns == null)
          return this._searchPaths;
        foreach (PSSnapInInfo externalPsSnapIn in consoleInfo.ExternalPSSnapIns)
          this._searchPaths.Add(externalPsSnapIn.ApplicationBase);
      }
      if (this.ExecutionContext.Modules != null)
      {
        foreach (PSModuleInfo psModuleInfo in this.ExecutionContext.Modules.ModuleTable.Values)
          this._searchPaths.Add(psModuleInfo.ModuleBase);
      }
      return this._searchPaths;
    }

    private IEnumerable<HelpInfo> DoGetHelp(HelpRequest helpRequest)
    {
      this._lastErrors.Clear();
      this._searchPaths = (Collection<string>) null;
      this._lastHelpCategory = helpRequest.HelpCategory;
      if (string.IsNullOrEmpty(helpRequest.Target))
      {
        HelpInfo helpInfo = this.GetDefaultHelp();
        if (helpInfo != null)
          yield return helpInfo;
        yield return (HelpInfo) null;
      }
      else
      {
        bool isMatchFound = false;
        if (!WildcardPattern.ContainsWildcardCharacters(helpRequest.Target))
        {
          foreach (HelpInfo helpInfo in this.ExactMatchHelp(helpRequest))
          {
            isMatchFound = true;
            yield return helpInfo;
          }
        }
        if (!isMatchFound)
        {
          foreach (HelpInfo helpInfo in this.SearchHelp(helpRequest))
          {
            isMatchFound = true;
            yield return helpInfo;
          }
          if (!isMatchFound && !WildcardPattern.ContainsWildcardCharacters(helpRequest.Target) && this.LastErrors.Count == 0)
            this.LastErrors.Add(new ErrorRecord((Exception) new HelpNotFoundException(helpRequest.Target), "HelpNotFound", ErrorCategory.ResourceUnavailable, (object) null));
        }
      }
    }

    private IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest)
    {
      bool isHelpInfoFound = false;
      for (int i = 0; i < this.HelpProviders.Count; ++i)
      {
        HelpProvider helpProvider = (HelpProvider) this.HelpProviders[i];
        if ((helpProvider.HelpCategory & helpRequest.HelpCategory) > HelpCategory.None)
        {
          foreach (HelpInfo helpInfo1 in helpProvider.ExactMatchHelp(helpRequest))
          {
            isHelpInfoFound = true;
            foreach (HelpInfo helpInfo2 in this.ForwardHelp(helpInfo1, helpRequest))
              yield return helpInfo2;
          }
        }
        if (isHelpInfoFound)
          break;
      }
    }

    private IEnumerable<HelpInfo> ForwardHelp(
      HelpInfo helpInfo,
      HelpRequest helpRequest)
    {
      Collection<HelpInfo> collection = new Collection<HelpInfo>();
      if (helpInfo.ForwardHelpCategory == HelpCategory.None && string.IsNullOrEmpty(helpInfo.ForwardTarget))
      {
        yield return helpInfo;
      }
      else
      {
        HelpCategory forwardHelpCategory = helpInfo.ForwardHelpCategory;
        bool isHelpInfoProcessed = false;
        for (int i = 0; i < this.HelpProviders.Count; ++i)
        {
          HelpProvider helpProvider = (HelpProvider) this.HelpProviders[i];
          if ((helpProvider.HelpCategory & forwardHelpCategory) == forwardHelpCategory)
          {
            isHelpInfoProcessed = true;
            using (IEnumerator<HelpInfo> enumerator = helpProvider.ProcessForwardedHelp(helpInfo, helpRequest).GetEnumerator())
            {
              if (enumerator.MoveNext())
              {
                HelpInfo fwdResult = enumerator.Current;
                foreach (HelpInfo helpInfo1 in this.ForwardHelp(fwdResult, helpRequest))
                  yield return helpInfo1;
                yield break;
              }
            }
          }
        }
        if (!isHelpInfoProcessed)
          yield return helpInfo;
      }
    }

    private HelpInfo GetDefaultHelp()
    {
      using (IEnumerator<HelpInfo> enumerator = this.ExactMatchHelp(new HelpRequest("default", HelpCategory.DefaultHelp)).GetEnumerator())
      {
        if (enumerator.MoveNext())
          return enumerator.Current;
      }
      return (HelpInfo) null;
    }

    private IEnumerable<HelpInfo> SearchHelp(HelpRequest helpRequest)
    {
      int countOfHelpInfosFound = 0;
      bool searchInHelpContent = false;
      bool shouldBreak = false;
      do
      {
        if (searchInHelpContent)
          shouldBreak = true;
        for (int i = 0; i < this.HelpProviders.Count; ++i)
        {
          HelpProvider helpProvider = (HelpProvider) this.HelpProviders[i];
          if ((helpProvider.HelpCategory & helpRequest.HelpCategory) > HelpCategory.None)
          {
            foreach (HelpInfo helpInfo in helpProvider.SearchHelp(helpRequest, searchInHelpContent))
            {
              ++countOfHelpInfosFound;
              yield return helpInfo;
              if (countOfHelpInfosFound >= helpRequest.MaxResults && helpRequest.MaxResults > 0)
                yield break;
            }
          }
        }
        if (countOfHelpInfosFound <= 0)
          searchInHelpContent = true;
        else
          goto label_13;
      }
      while (!shouldBreak);
      goto label_16;
label_13:
      yield break;
label_16:;
    }

    internal ArrayList HelpProviders => this._helpProviders;

    private void InitializeHelpProviders()
    {
      this._helpProviders.Add((object) new AliasHelpProvider(this));
      this._helpProviders.Add((object) new CommandHelpProvider(this));
      this._helpProviders.Add((object) new ProviderHelpProvider(this));
      this._helpProviders.Add((object) new HelpFileHelpProvider(this));
      this._helpProviders.Add((object) new FaqHelpProvider(this));
      this._helpProviders.Add((object) new GlossaryHelpProvider(this));
      this._helpProviders.Add((object) new GeneralHelpProvider(this));
      this._helpProviders.Add((object) new DefaultHelpProvider(this));
    }

    internal HelpErrorTracer HelpErrorTracer => this._helpErrorTracer;

    internal IDisposable Trace(string helpFile) => this._helpErrorTracer == null ? (IDisposable) null : this._helpErrorTracer.Trace(helpFile);

    internal void TraceError(ErrorRecord errorRecord)
    {
      if (this._helpErrorTracer == null)
        return;
      this._helpErrorTracer.TraceError(errorRecord);
    }

    internal void TraceErrors(Collection<ErrorRecord> errorRecords)
    {
      if (this._helpErrorTracer == null || errorRecords == null)
        return;
      this._helpErrorTracer.TraceErrors(errorRecords);
    }

    private void ValidateHelpCulture()
    {
      CultureInfo currentUiCulture = Thread.CurrentThread.CurrentUICulture;
      if (this._culture == null)
      {
        this._culture = currentUiCulture;
      }
      else
      {
        if (this._culture.Equals((object) currentUiCulture))
          return;
        this._culture = currentUiCulture;
        this.ResetHelpProviders();
      }
    }

    private void ResetHelpProviders()
    {
      if (this._helpProviders == null)
        return;
      for (int index = 0; index < this._helpProviders.Count; ++index)
        ((HelpProvider) this._helpProviders[index]).Reset();
    }
  }
}
