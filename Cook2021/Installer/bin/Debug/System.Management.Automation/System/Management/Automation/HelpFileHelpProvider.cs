// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HelpFileHelpProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;

namespace System.Management.Automation
{
  internal class HelpFileHelpProvider : HelpProviderWithCache
  {
    private Hashtable _helpFiles = new Hashtable();
    [TraceSource("HelpFileHelpProvider", "HelpFileHelpProvider")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (HelpFileHelpProvider), nameof (HelpFileHelpProvider));

    internal HelpFileHelpProvider(HelpSystem helpSystem)
      : base(helpSystem)
    {
      using (HelpFileHelpProvider.tracer.TraceConstructor((object) this))
        ;
    }

    internal override string Name
    {
      get
      {
        using (HelpFileHelpProvider.tracer.TraceProperty())
          return "HelpFile Help Provider";
      }
    }

    internal override HelpCategory HelpCategory
    {
      get
      {
        using (HelpFileHelpProvider.tracer.TraceProperty())
          return HelpCategory.HelpFile;
      }
    }

    internal override IEnumerable<HelpInfo> ExactMatchHelp(
      HelpRequest helpRequest)
    {
      using (HelpFileHelpProvider.tracer.TraceMethod())
      {
        int countHelpInfosFound = 0;
        string helpFileName = helpRequest.Target + ".help.txt";
        Collection<string> filesMatched = MUIFileSearcher.SearchFiles(helpFileName, this.GetSearchPaths());
        foreach (string str in filesMatched)
        {
          if (!this._helpFiles.ContainsKey((object) str))
          {
            try
            {
              this.LoadHelpFile(str);
            }
            catch (IOException ex)
            {
              this.ReportHelpFileError((Exception) ex, helpRequest.Target, str);
            }
            catch (SecurityException ex)
            {
              this.ReportHelpFileError((Exception) ex, helpRequest.Target, str);
            }
          }
          HelpInfo helpInfo = this.GetCache(str);
          if (helpInfo != null)
          {
            ++countHelpInfosFound;
            yield return helpInfo;
            if (countHelpInfosFound >= helpRequest.MaxResults && helpRequest.MaxResults > 0)
              break;
          }
        }
      }
    }

    internal override IEnumerable<HelpInfo> SearchHelp(
      HelpRequest helpRequest,
      bool searchOnlyContent)
    {
      using (HelpFileHelpProvider.tracer.TraceMethod())
      {
        string target = helpRequest.Target;
        string pattern = target;
        int countOfHelpInfoObjectsFound = 0;
        WildcardPattern wildCardPattern = (WildcardPattern) null;
        if (!searchOnlyContent && !WildcardPattern.ContainsWildcardCharacters(target))
          pattern = "*" + pattern + "*";
        if (searchOnlyContent)
        {
          string pattern1 = helpRequest.Target;
          if (!WildcardPattern.ContainsWildcardCharacters(helpRequest.Target))
            pattern1 = "*" + pattern1 + "*";
          wildCardPattern = new WildcardPattern(pattern1, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
          pattern = "*";
        }
        // ISSUE: reference to a compiler-generated field
        this.\u003Cpattern\u003E5__e += ".help.txt";
        Collection<string> files = MUIFileSearcher.SearchFiles(pattern, this.GetSearchPaths());
        if (files != null)
        {
          foreach (string str in files)
          {
            if (!this._helpFiles.ContainsKey((object) str))
            {
              try
              {
                this.LoadHelpFile(str);
              }
              catch (IOException ex)
              {
                this.ReportHelpFileError((Exception) ex, helpRequest.Target, str);
              }
              catch (SecurityException ex)
              {
                this.ReportHelpFileError((Exception) ex, helpRequest.Target, str);
              }
            }
            HelpFileHelpInfo helpInfo = this.GetCache(str) as HelpFileHelpInfo;
            if (helpInfo != null && (!searchOnlyContent || helpInfo.MatchPatternInContent(wildCardPattern)))
            {
              ++countOfHelpInfoObjectsFound;
              yield return (HelpInfo) helpInfo;
              if (countOfHelpInfoObjectsFound >= helpRequest.MaxResults && helpRequest.MaxResults > 0)
                break;
            }
          }
        }
      }
    }

    private HelpInfo LoadHelpFile(string path)
    {
      using (HelpFileHelpProvider.tracer.TraceMethod())
      {
        string fileName = Path.GetFileName(path);
        if (!path.EndsWith(".help.txt", StringComparison.OrdinalIgnoreCase))
          return (HelpInfo) null;
        string name = fileName.Substring(0, fileName.Length - 9);
        if (string.IsNullOrEmpty(name))
          return (HelpInfo) null;
        HelpInfo cache = this.GetCache(path);
        if (cache != null)
          return cache;
        TextReader textReader = (TextReader) new StreamReader(path);
        string text = (string) null;
        try
        {
          text = textReader.ReadToEnd();
        }
        finally
        {
          textReader.Close();
        }
        this._helpFiles[(object) path] = (object) 0;
        HelpInfo helpInfo = (HelpInfo) HelpFileHelpInfo.GetHelpInfo(name, text, path);
        this.AddCache(path, helpInfo);
        return helpInfo;
      }
    }

    internal override void Reset()
    {
      using (HelpFileHelpProvider.tracer.TraceMethod())
      {
        base.Reset();
        this._helpFiles.Clear();
      }
    }
  }
}
