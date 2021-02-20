// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SessionStateUtilities
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace System.Management.Automation
{
  internal static class SessionStateUtilities
  {
    internal static Collection<T> ConvertArrayToCollection<T>(T[] array)
    {
      Collection<T> collection = new Collection<T>();
      if (array != null)
      {
        foreach (T obj in array)
          collection.Add(obj);
      }
      return collection;
    }

    internal static bool CollectionContainsValue(
      IEnumerable collection,
      object value,
      IComparer comparer)
    {
      if (collection == null)
        throw new ArgumentNullException(nameof (collection));
      bool flag = false;
      foreach (object x in collection)
      {
        if (comparer != null)
        {
          if (comparer.Compare(x, value) == 0)
          {
            flag = true;
            break;
          }
        }
        else if (x.Equals(value))
        {
          flag = true;
          break;
        }
      }
      return flag;
    }

    internal static Collection<WildcardPattern> CreateWildcardsFromStrings(
      string[] globPatterns,
      WildcardOptions options)
    {
      return SessionStateUtilities.CreateWildcardsFromStrings(SessionStateUtilities.ConvertArrayToCollection<string>(globPatterns), options);
    }

    internal static Collection<WildcardPattern> CreateWildcardsFromStrings(
      Collection<string> globPatterns,
      WildcardOptions options)
    {
      Collection<WildcardPattern> collection = new Collection<WildcardPattern>();
      if (globPatterns != null && globPatterns.Count > 0)
      {
        foreach (string globPattern in globPatterns)
        {
          if (!string.IsNullOrEmpty(globPattern))
            collection.Add(new WildcardPattern(globPattern, options));
        }
      }
      return collection;
    }

    internal static bool MatchesAnyWildcardPattern(
      string text,
      IEnumerable<WildcardPattern> patterns,
      bool defaultValue)
    {
      bool flag1 = false;
      bool flag2 = false;
      if (patterns != null)
      {
        foreach (WildcardPattern pattern in patterns)
        {
          flag2 = true;
          if (pattern.IsMatch(text))
          {
            flag1 = true;
            break;
          }
        }
      }
      if (!flag2)
        flag1 = defaultValue;
      return flag1;
    }

    internal static FileMode GetFileModeFromOpenMode(OpenMode openMode)
    {
      FileMode fileMode = FileMode.Create;
      switch (openMode)
      {
        case OpenMode.Add:
          fileMode = FileMode.Append;
          break;
        case OpenMode.New:
          fileMode = FileMode.CreateNew;
          break;
        case OpenMode.Overwrite:
          fileMode = FileMode.Create;
          break;
      }
      return fileMode;
    }
  }
}
