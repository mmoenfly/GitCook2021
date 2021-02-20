// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Utils
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Management.Automation
{
  internal static class Utils
  {
    internal static string DefaultPowerShellShellID = "Microsoft.PowerShell";
    internal static string ProductNameForDirectory = "WindowsPowerShell";
    internal static string ModuleDirectory = "Modules";
    internal static char[] DirectorySeparators = new char[2]
    {
      Path.DirectorySeparatorChar,
      Path.AltDirectorySeparatorChar
    };

    internal static void CheckArgForNullOrEmpty(PSTraceSource tracer, string arg, string argName)
    {
      switch (arg)
      {
        case "":
          throw tracer.NewArgumentException(argName);
        case null:
          throw tracer.NewArgumentNullException(argName);
      }
    }

    internal static void CheckArgForNull(PSTraceSource tracer, object arg, string argName)
    {
      if (arg == null)
        throw tracer.NewArgumentNullException(argName);
    }

    internal static void CheckSecureStringArg(
      PSTraceSource tracer,
      SecureString arg,
      string argName)
    {
      if (arg == null)
        throw tracer.NewArgumentNullException(argName);
      if (arg.Length == 0)
        throw tracer.NewArgumentException(argName);
    }

    [ArchitectureSensitive]
    internal static string GetStringFromSecureString(SecureString ss)
    {
      IntPtr globalAllocUnicode = Marshal.SecureStringToGlobalAllocUnicode(ss);
      string stringUni = Marshal.PtrToStringUni(globalAllocUnicode);
      Marshal.ZeroFreeGlobalAllocUnicode(globalAllocUnicode);
      return stringUni;
    }

    internal static TypeTable GetTypeTableFromExecutionContextTLS() => LocalPipeline.GetExecutionContextFromTLS()?.TypeTable;

    internal static string GetApplicationBase(string shellId)
    {
      string name = "Software\\Microsoft\\PowerShell\\" + PSVersionInfo.RegistryVersionKey + "\\PowerShellEngine";
      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name))
      {
        if (registryKey != null)
          return registryKey.GetValue("ApplicationBase") as string;
      }
      Assembly entryAssembly = Assembly.GetEntryAssembly();
      if (entryAssembly != null)
        return Path.GetDirectoryName(entryAssembly.Location);
      Assembly assembly = Assembly.GetAssembly(typeof (PSObject));
      return assembly != null ? Path.GetDirectoryName(assembly.Location) : "";
    }

    internal static string GetCurrentMajorVersion() => PSVersionInfo.PSVersion.Major.ToString((IFormatProvider) CultureInfo.InvariantCulture);

    internal static Version StringToVersion(string versionString)
    {
      if (string.IsNullOrEmpty(versionString))
        return (Version) null;
      int num = 0;
      foreach (char ch in versionString)
      {
        if (ch == '.')
        {
          ++num;
          if (num > 1)
            return (Version) null;
        }
      }
      if (num == 0)
        versionString += ".0";
      try
      {
        return new Version(versionString);
      }
      catch (ArgumentException ex)
      {
      }
      catch (FormatException ex)
      {
      }
      catch (OverflowException ex)
      {
      }
      return (Version) null;
    }

    internal static bool IsVersionSupported(string ver) => Utils.IsVersionSupported(Utils.StringToVersion(ver.ToString()));

    internal static bool IsVersionSupported(Version checkVersion)
    {
      if (checkVersion == (Version) null)
        return false;
      foreach (Version compatibleVersion in PSVersionInfo.PSCompatibleVersions)
      {
        if (checkVersion.Major == compatibleVersion.Major && checkVersion.Minor <= compatibleVersion.Minor)
          return true;
      }
      return false;
    }

    internal static string GetRegistryConfigurationPrefix() => "SOFTWARE\\Microsoft\\PowerShell\\" + PSVersionInfo.RegistryVersionKey + "\\ShellIds";

    internal static string GetRegistryConfigurationPath(string shellID) => Utils.GetRegistryConfigurationPrefix() + "\\" + shellID;
  }
}
