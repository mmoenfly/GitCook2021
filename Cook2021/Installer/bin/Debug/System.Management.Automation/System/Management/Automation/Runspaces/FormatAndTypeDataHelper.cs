// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.FormatAndTypeDataHelper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal.Format;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation.Host;
using System.Reflection;
using System.Text;

namespace System.Management.Automation.Runspaces
{
  internal static class FormatAndTypeDataHelper
  {
    private const string FileNotFound = "FileNotFound";
    private const string CannotFindRegistryKey = "CannotFindRegistryKey";
    private const string CannotFindRegistryKeyPath = "CannotFindRegistryKeyPath";
    private const string EntryShouldBeMshXml = "EntryShouldBeMshXml";
    private const string DuplicateFile = "DuplicateFile";
    internal const string ValidationException = "ValidationException";

    private static string GetBaseFolder(
      RunspaceConfiguration runspaceConfiguration,
      Collection<string> independentErrors)
    {
      string pathFromRegistry = CommandDiscovery.GetShellPathFromRegistry(runspaceConfiguration.ShellId);
      string path;
      if (pathFromRegistry == null)
      {
        path = Path.GetDirectoryName(PsUtils.GetMainModule(Process.GetCurrentProcess()).FileName);
      }
      else
      {
        path = Path.GetDirectoryName(pathFromRegistry);
        if (!Directory.Exists(path))
        {
          string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
          string str = ResourceManagerCache.FormatResourceString("TypesXml", "CannotFindRegistryKeyPath", (object) path, (object) Utils.GetRegistryConfigurationPath(runspaceConfiguration.ShellId), (object) "\\Path", (object) directoryName);
          independentErrors.Add(str);
          path = directoryName;
        }
      }
      return path;
    }

    internal static Collection<PSSnapInTypeAndFormatErrors> GetFormatAndTypesErrors(
      RunspaceConfiguration runspaceConfiguration,
      PSHost host,
      IEnumerable configurationEntryCollection,
      RunspaceConfigurationCategory category,
      Collection<string> independentErrors)
    {
      Collection<PSSnapInTypeAndFormatErrors> collection = new Collection<PSSnapInTypeAndFormatErrors>();
      string baseFolder = FormatAndTypeDataHelper.GetBaseFolder(runspaceConfiguration, independentErrors);
      Hashtable fullFileNameHash = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
      foreach (object configurationEntry1 in configurationEntryCollection)
      {
        string fileName;
        string str;
        if (category == RunspaceConfigurationCategory.Types)
        {
          TypeConfigurationEntry configurationEntry2 = (TypeConfigurationEntry) configurationEntry1;
          fileName = configurationEntry2.FileName;
          str = configurationEntry2.PSSnapIn == null ? runspaceConfiguration.ShellId : configurationEntry2.PSSnapIn.Name;
        }
        else
        {
          FormatConfigurationEntry configurationEntry2 = (FormatConfigurationEntry) configurationEntry1;
          fileName = configurationEntry2.FileName;
          str = configurationEntry2.PSSnapIn == null ? runspaceConfiguration.ShellId : configurationEntry2.PSSnapIn.Name;
        }
        string checkFullFileName1 = FormatAndTypeDataHelper.GetAndCheckFullFileName(str, fullFileNameHash, runspaceConfiguration, baseFolder, fileName, independentErrors);
        if (checkFullFileName1 != null)
        {
          if (checkFullFileName1.EndsWith("filelist.ps1xml", StringComparison.OrdinalIgnoreCase))
          {
            foreach (string readFile in runspaceConfiguration.TypeTable.ReadFiles(str, checkFullFileName1, independentErrors, runspaceConfiguration.AuthorizationManager, host))
            {
              string checkFullFileName2 = FormatAndTypeDataHelper.GetAndCheckFullFileName(str, fullFileNameHash, runspaceConfiguration, baseFolder, readFile, independentErrors);
              if (checkFullFileName2 != null)
                collection.Add(new PSSnapInTypeAndFormatErrors(str, checkFullFileName2));
            }
          }
          else
            collection.Add(new PSSnapInTypeAndFormatErrors(str, checkFullFileName1));
        }
      }
      return collection;
    }

    private static string GetAndCheckFullFileName(
      string psSnapinName,
      Hashtable fullFileNameHash,
      RunspaceConfiguration runspaceConfiguration,
      string baseFolder,
      string baseFileName,
      Collection<string> independentErrors)
    {
      string path = !Path.IsPathRooted(baseFileName) ? Path.Combine(baseFolder, baseFileName) : baseFileName;
      if (!File.Exists(path))
      {
        string str = ResourceManagerCache.FormatResourceString("TypesXml", "FileNotFound", (object) psSnapinName, (object) path);
        independentErrors.Add(str);
        return (string) null;
      }
      string str1 = (string) fullFileNameHash[(object) path];
      if (str1 != null)
      {
        string str2 = ResourceManagerCache.FormatResourceString("TypesXml", "DuplicateFile", (object) psSnapinName, (object) path, (object) str1);
        independentErrors.Add(str2);
        return (string) null;
      }
      if (!path.EndsWith(".ps1xml", StringComparison.OrdinalIgnoreCase))
      {
        string str2 = ResourceManagerCache.FormatResourceString("TypesXml", "EntryShouldBeMshXml", (object) psSnapinName, (object) path);
        independentErrors.Add(str2);
        return (string) null;
      }
      fullFileNameHash.Add((object) path, (object) psSnapinName);
      return path;
    }

    internal static void ThrowExceptionOnError(
      string errorId,
      Collection<string> independentErrors,
      Collection<PSSnapInTypeAndFormatErrors> PSSnapinFilesCollection,
      RunspaceConfigurationCategory category)
    {
      Collection<string> collection = new Collection<string>();
      foreach (string independentError in independentErrors)
        collection.Add(independentError);
      foreach (PSSnapInTypeAndFormatErrors psSnapinFiles in PSSnapinFilesCollection)
      {
        foreach (string error in psSnapinFiles.Errors)
          collection.Add(error);
      }
      if (collection.Count != 0)
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append('\n');
        foreach (string str in collection)
        {
          stringBuilder.Append(str);
          stringBuilder.Append('\n');
        }
        string message = "";
        switch (category)
        {
          case RunspaceConfigurationCategory.Types:
            message = ResourceManagerCache.FormatResourceString("ExtendedTypeSystem", "TypesXmlError", (object) stringBuilder.ToString());
            break;
          case RunspaceConfigurationCategory.Formats:
            message = XmlLoadingResourceManager.FormatString("FormatLoadingErrors", (object) stringBuilder.ToString());
            break;
        }
        RuntimeException runtimeException = new RuntimeException(message);
        runtimeException.SetErrorId(errorId);
        throw runtimeException;
      }
    }
  }
}
