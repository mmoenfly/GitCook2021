// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ResourceRetriever
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;

namespace System.Management.Automation
{
  internal class ResourceRetriever : MarshalByRefObject
  {
    internal string GetStringResource(
      string assemblyName,
      string modulePath,
      string baseName,
      string resourceID)
    {
      string str1 = (string) null;
      Assembly assembly = ResourceRetriever.LoadAssembly(assemblyName, modulePath);
      if (assembly != null)
      {
        CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
        Stream manifestResourceStream;
        while (true)
        {
          string str2 = baseName;
          if (!string.IsNullOrEmpty(cultureInfo.Name))
            str2 = str2 + "." + cultureInfo.Name;
          string name = str2 + ".resources";
          manifestResourceStream = assembly.GetManifestResourceStream(name);
          if (manifestResourceStream == null && !string.IsNullOrEmpty(cultureInfo.Name))
            cultureInfo = cultureInfo.Parent;
          else
            break;
        }
        if (manifestResourceStream != null)
          str1 = ResourceRetriever.GetString(manifestResourceStream, resourceID);
      }
      return str1;
    }

    private static Assembly LoadAssembly(string assemblyName, string modulePath)
    {
      AssemblyName assemblyName1 = new AssemblyName(assemblyName);
      string directoryName = Path.GetDirectoryName(modulePath);
      string fileName = Path.GetFileName(modulePath);
      CultureInfo culture = CultureInfo.CurrentUICulture;
      Assembly assembly;
      while (true)
      {
        assembly = ResourceRetriever.LoadAssemblyForCulture(culture, assemblyName1, directoryName, fileName);
        if (assembly == null && !string.IsNullOrEmpty(culture.Name))
          culture = culture.Parent;
        else
          break;
      }
      return assembly;
    }

    private static Assembly LoadAssemblyForCulture(
      CultureInfo culture,
      AssemblyName assemblyName,
      string moduleBase,
      string moduleFile)
    {
      Assembly assembly = (Assembly) null;
      assemblyName.CultureInfo = culture;
      try
      {
        assembly = Assembly.ReflectionOnlyLoad(assemblyName.FullName);
      }
      catch (FileLoadException ex)
      {
      }
      catch (BadImageFormatException ex)
      {
      }
      catch (FileNotFoundException ex)
      {
      }
      if (assembly != null)
        return assembly;
      string name = assemblyName.Name;
      try
      {
        assemblyName.Name = name + ".resources";
        assembly = Assembly.ReflectionOnlyLoad(assemblyName.FullName);
      }
      catch (FileLoadException ex)
      {
      }
      catch (BadImageFormatException ex)
      {
      }
      catch (FileNotFoundException ex)
      {
      }
      if (assembly != null)
        return assembly;
      assemblyName.Name = name;
      string str = Path.Combine(Path.Combine(moduleBase, culture.Name), moduleFile);
      if (File.Exists(str))
      {
        try
        {
          assembly = Assembly.ReflectionOnlyLoadFrom(str);
        }
        catch (FileLoadException ex)
        {
        }
        catch (BadImageFormatException ex)
        {
        }
        catch (FileNotFoundException ex)
        {
        }
      }
      return assembly;
    }

    private static string GetString(Stream stream, string resourceID)
    {
      string str = (string) null;
      foreach (DictionaryEntry dictionaryEntry in new ResourceReader(stream))
      {
        if (string.Equals(resourceID, (string) dictionaryEntry.Key, StringComparison.OrdinalIgnoreCase))
        {
          str = (string) dictionaryEntry.Value;
          break;
        }
      }
      return str;
    }
  }
}
