// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MUIFileSearcher
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;

namespace System.Management.Automation
{
  internal class MUIFileSearcher
  {
    private string _target;
    private Collection<string> _searchPaths;
    private SearchMode _searchMode = SearchMode.Unique;
    private Collection<string> _result;
    private Hashtable _uniqueMatches = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);

    private MUIFileSearcher(string target, Collection<string> searchPaths, SearchMode searchMode)
    {
      this._target = target;
      this._searchPaths = searchPaths;
      this._searchMode = searchMode;
    }

    private MUIFileSearcher(string target, Collection<string> searchPaths)
      : this(target, searchPaths, SearchMode.Unique)
    {
    }

    internal string Target => this._target;

    internal Collection<string> SearchPaths => this._searchPaths;

    internal SearchMode SearchMode => this._searchMode;

    internal Collection<string> Result
    {
      get
      {
        if (this._result == null)
        {
          this._result = new Collection<string>();
          this.SearchForFiles();
        }
        return this._result;
      }
    }

    private void SearchForFiles()
    {
      if (string.IsNullOrEmpty(this.Target))
        return;
      string fileName = Path.GetFileName(this.Target);
      if (string.IsNullOrEmpty(fileName))
        return;
      foreach (string normalizeSearchPath in MUIFileSearcher.NormalizeSearchPaths(this.Target, this.SearchPaths))
      {
        this.SearchForFiles(fileName, normalizeSearchPath);
        if (this.SearchMode == SearchMode.First && this.Result.Count > 0)
          break;
      }
    }

    private void SearchForFiles(string pattern, string directory)
    {
      for (CultureInfo cultureInfo = Thread.CurrentThread.CurrentUICulture; cultureInfo != null; cultureInfo = cultureInfo.Parent)
      {
        string str1 = Path.Combine(directory, cultureInfo.Name);
        if (Directory.Exists(str1))
        {
          string[] files = Directory.GetFiles(str1, pattern);
          if (files == null)
            break;
          foreach (string str2 in files)
          {
            string str3 = Path.Combine(str1, str2);
            switch (this.SearchMode)
            {
              case SearchMode.First:
                this._result.Add(str3);
                return;
              case SearchMode.All:
                this._result.Add(str3);
                break;
              case SearchMode.Unique:
                string fileName = Path.GetFileName(str2);
                string str4 = Path.Combine(directory, fileName);
                if (!this._uniqueMatches.Contains((object) str4))
                {
                  this._result.Add(str3);
                  this._uniqueMatches[(object) str4] = (object) true;
                  break;
                }
                break;
            }
          }
        }
        if (string.IsNullOrEmpty(cultureInfo.Name))
          break;
      }
    }

    private static Collection<string> NormalizeSearchPaths(
      string target,
      Collection<string> searchPaths)
    {
      Collection<string> collection = new Collection<string>();
      if (!string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(Path.GetDirectoryName(target)))
      {
        string directoryName = Path.GetDirectoryName(target);
        if (Directory.Exists(directoryName))
          collection.Add(Path.GetFullPath(directoryName));
        return collection;
      }
      if (searchPaths != null)
      {
        foreach (string searchPath in searchPaths)
        {
          if (!collection.Contains(searchPath) && Directory.Exists(searchPath))
            collection.Add(searchPath);
        }
      }
      string installationPath = MUIFileSearcher.GetMshDefaultInstallationPath();
      if (installationPath != null && !collection.Contains(installationPath) && Directory.Exists(installationPath))
        collection.Add(installationPath);
      return collection;
    }

    private static string GetMshDefaultInstallationPath()
    {
      string path = CommandDiscovery.GetShellPathFromRegistry(Utils.DefaultPowerShellShellID);
      if (path != null)
        path = Path.GetDirectoryName(path);
      return path;
    }

    internal static Collection<string> SearchFiles(string pattern) => MUIFileSearcher.SearchFiles(pattern, new Collection<string>());

    internal static Collection<string> SearchFiles(
      string pattern,
      Collection<string> searchPaths)
    {
      return new MUIFileSearcher(pattern, searchPaths).Result;
    }

    internal static string LocateFile(string file) => MUIFileSearcher.LocateFile(file, new Collection<string>());

    internal static string LocateFile(string file, Collection<string> searchPaths)
    {
      MUIFileSearcher muiFileSearcher = new MUIFileSearcher(file, searchPaths, SearchMode.First);
      return muiFileSearcher.Result == null || muiFileSearcher.Result.Count == 0 ? (string) null : muiFileSearcher.Result[0];
    }
  }
}
