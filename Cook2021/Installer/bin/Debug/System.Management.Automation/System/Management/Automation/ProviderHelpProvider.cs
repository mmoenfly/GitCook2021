// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProviderHelpProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Xml;

namespace System.Management.Automation
{
  internal class ProviderHelpProvider : HelpProviderWithCache
  {
    private SessionState _sessionState;
    private Hashtable _helpFiles = new Hashtable();
    [TraceSource("ProviderHelpProvider", "ProviderHelpProvider")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ProviderHelpProvider), nameof (ProviderHelpProvider));

    internal ProviderHelpProvider(HelpSystem helpSystem)
      : base(helpSystem)
    {
      using (ProviderHelpProvider.tracer.TraceConstructor((object) this))
        this._sessionState = helpSystem.ExecutionContext.SessionState;
    }

    internal override string Name
    {
      get
      {
        using (ProviderHelpProvider.tracer.TraceProperty())
          return "Provider Help Provider";
      }
    }

    internal override HelpCategory HelpCategory
    {
      get
      {
        using (ProviderHelpProvider.tracer.TraceProperty())
          return HelpCategory.Provider;
      }
    }

    internal override IEnumerable<HelpInfo> ExactMatchHelp(
      HelpRequest helpRequest)
    {
      using (ProviderHelpProvider.tracer.TraceMethod())
      {
        Collection<ProviderInfo> matchingProviders = (Collection<ProviderInfo>) null;
        try
        {
          matchingProviders = this._sessionState.Provider.Get(helpRequest.Target);
        }
        catch (ProviderNotFoundException ex)
        {
          if (this.HelpSystem.LastHelpCategory == HelpCategory.Provider)
            this.HelpSystem.LastErrors.Add(new ErrorRecord((Exception) ex, "ProviderLoadError", ErrorCategory.ResourceUnavailable, (object) null)
            {
              ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "ProviderLoadError", new object[2]
              {
                (object) helpRequest.Target,
                (object) ex.Message
              })
            });
        }
        if (matchingProviders != null)
        {
          foreach (ProviderInfo providerInfo in matchingProviders)
          {
            try
            {
              this.LoadHelpFile(providerInfo);
            }
            catch (IOException ex)
            {
              this.ReportHelpFileError((Exception) ex, helpRequest.Target, providerInfo.HelpFile);
            }
            catch (SecurityException ex)
            {
              this.ReportHelpFileError((Exception) ex, helpRequest.Target, providerInfo.HelpFile);
            }
            catch (XmlException ex)
            {
              this.ReportHelpFileError((Exception) ex, helpRequest.Target, providerInfo.HelpFile);
            }
            HelpInfo helpInfo = this.GetCache(providerInfo.PSSnapInName + "\\" + providerInfo.Name);
            if (helpInfo != null)
              yield return helpInfo;
          }
        }
      }
    }

    private static string GetProviderAssemblyPath(ProviderInfo providerInfo)
    {
      if (providerInfo == null)
        return (string) null;
      return providerInfo.ImplementingType == null ? (string) null : Path.GetDirectoryName(providerInfo.ImplementingType.Assembly.Location);
    }

    private void LoadHelpFile(ProviderInfo providerInfo)
    {
      using (ProviderHelpProvider.tracer.TraceMethod())
      {
        string str1 = providerInfo != null ? providerInfo.HelpFile : throw ProviderHelpProvider.tracer.NewArgumentNullException(nameof (providerInfo));
        if (string.IsNullOrEmpty(str1) || this._helpFiles.Contains((object) str1))
          return;
        string file = str1;
        PSSnapInInfo psSnapIn = providerInfo.PSSnapIn;
        Collection<string> searchPaths = new Collection<string>();
        if (psSnapIn != null)
          file = Path.Combine(psSnapIn.ApplicationBase, str1);
        else if (providerInfo.Module != null && !string.IsNullOrEmpty(providerInfo.Module.Path))
        {
          file = Path.Combine(providerInfo.Module.ModuleBase, str1);
        }
        else
        {
          searchPaths.Add(this.GetDefaultShellSearchPath());
          searchPaths.Add(ProviderHelpProvider.GetProviderAssemblyPath(providerInfo));
        }
        string str2 = MUIFileSearcher.LocateFile(file, searchPaths);
        if (string.IsNullOrEmpty(str2))
          throw new FileNotFoundException(str1);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(str2);
        this._helpFiles[(object) str1] = (object) 0;
        XmlNode xmlNode = (XmlNode) null;
        if (xmlDocument.HasChildNodes)
        {
          for (int i = 0; i < xmlDocument.ChildNodes.Count; ++i)
          {
            XmlNode childNode = xmlDocument.ChildNodes[i];
            if (childNode.NodeType == XmlNodeType.Element && string.Compare(childNode.Name, "helpItems", StringComparison.OrdinalIgnoreCase) == 0)
            {
              xmlNode = childNode;
              break;
            }
          }
        }
        if (xmlNode == null)
          return;
        using (this.HelpSystem.Trace(str2))
        {
          if (!xmlNode.HasChildNodes)
            return;
          for (int i = 0; i < xmlNode.ChildNodes.Count; ++i)
          {
            XmlNode childNode = xmlNode.ChildNodes[i];
            if (childNode.NodeType == XmlNodeType.Element && string.Compare(childNode.Name, "providerHelp", StringComparison.OrdinalIgnoreCase) == 0)
            {
              HelpInfo helpInfo = (HelpInfo) ProviderHelpInfo.Load(childNode);
              if (helpInfo != null)
              {
                this.HelpSystem.TraceErrors(helpInfo.Errors);
                helpInfo.FullHelp.TypeNames.Insert(0, string.Format((IFormatProvider) CultureInfo.InvariantCulture, "ProviderHelpInfo#{0}#{1}", (object) providerInfo.PSSnapInName, (object) helpInfo.Name));
                if (!string.IsNullOrEmpty(providerInfo.PSSnapInName))
                {
                  helpInfo.FullHelp.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSSnapIn", (object) providerInfo.PSSnapIn));
                  helpInfo.FullHelp.TypeNames.Insert(1, string.Format((IFormatProvider) CultureInfo.InvariantCulture, "ProviderHelpInfo#{0}", (object) providerInfo.PSSnapInName));
                }
                this.AddCache(providerInfo.PSSnapInName + "\\" + helpInfo.Name, helpInfo);
              }
            }
          }
        }
      }
    }

    internal override IEnumerable<HelpInfo> SearchHelp(
      HelpRequest helpRequest,
      bool searchOnlyContent)
    {
      using (ProviderHelpProvider.tracer.TraceMethod())
      {
        int countOfHelpInfoObjectsFound = 0;
        string target = helpRequest.Target;
        string pattern = target;
        WildcardPattern wildCardPattern = (WildcardPattern) null;
        bool decoratedSearch = !WildcardPattern.ContainsWildcardCharacters(target);
        if (!searchOnlyContent)
        {
          if (decoratedSearch)
          {
            // ISSUE: reference to a compiler-generated field
            this.\u003Cpattern\u003E5__d += "*";
          }
        }
        else
        {
          string pattern1 = helpRequest.Target;
          if (decoratedSearch)
            pattern1 = "*" + helpRequest.Target + "*";
          wildCardPattern = new WildcardPattern(pattern1, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
          pattern = "*";
        }
        PSSnapinQualifiedName snapinQualifiedNameForPattern = PSSnapinQualifiedName.GetInstance(pattern);
        if (snapinQualifiedNameForPattern != null)
        {
          foreach (ProviderInfo providerInfo in this._sessionState.Provider.GetAll())
          {
            if (providerInfo.IsMatch(pattern))
            {
              try
              {
                this.LoadHelpFile(providerInfo);
              }
              catch (IOException ex)
              {
                if (!decoratedSearch)
                  this.ReportHelpFileError((Exception) ex, providerInfo.Name, providerInfo.HelpFile);
              }
              catch (SecurityException ex)
              {
                if (!decoratedSearch)
                  this.ReportHelpFileError((Exception) ex, providerInfo.Name, providerInfo.HelpFile);
              }
              catch (XmlException ex)
              {
                if (!decoratedSearch)
                  this.ReportHelpFileError((Exception) ex, providerInfo.Name, providerInfo.HelpFile);
              }
              HelpInfo helpInfo = this.GetCache(providerInfo.PSSnapInName + "\\" + providerInfo.Name);
              if (helpInfo != null && (!searchOnlyContent || helpInfo.MatchPatternInContent(wildCardPattern)))
              {
                ++countOfHelpInfoObjectsFound;
                yield return helpInfo;
                if (countOfHelpInfoObjectsFound >= helpRequest.MaxResults && helpRequest.MaxResults > 0)
                  break;
              }
            }
          }
        }
      }
    }

    internal override IEnumerable<HelpInfo> ProcessForwardedHelp(
      HelpInfo helpInfo,
      HelpRequest helpRequest)
    {
      ProviderCommandHelpInfo providerCommandHelpInfo = new ProviderCommandHelpInfo(helpInfo, helpRequest.ProviderContext);
      yield return (HelpInfo) providerCommandHelpInfo;
    }

    internal override void Reset()
    {
      using (ProviderHelpProvider.tracer.TraceMethod())
      {
        base.Reset();
        this._helpFiles.Clear();
      }
    }
  }
}
