// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSInstaller
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;

namespace System.Management.Automation
{
  public abstract class PSInstaller : Installer
  {
    private static string[] MshRegistryRoots => new string[1]
    {
      "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\PowerShell\\" + PSVersionInfo.RegistryVersionKey + "\\"
    };

    internal abstract string RegKey { get; }

    internal abstract Dictionary<string, object> RegValues { get; }

    public override sealed void Install(IDictionary stateSaver)
    {
      base.Install(stateSaver);
      this.WriteRegistry();
    }

    private void WriteRegistry()
    {
      foreach (string mshRegistryRoot in PSInstaller.MshRegistryRoots)
      {
        RegistryKey registryKey = this.GetRegistryKey(mshRegistryRoot + this.RegKey);
        foreach (string key in this.RegValues.Keys)
          registryKey.SetValue(key, this.RegValues[key]);
      }
    }

    private RegistryKey GetRegistryKey(string keyPath) => PSInstaller.GetRootHive(keyPath)?.CreateSubKey(PSInstaller.GetSubkeyPath(keyPath));

    private static string GetSubkeyPath(string keyPath)
    {
      int num = keyPath.IndexOf('\\');
      return num > 0 ? keyPath.Substring(num + 1) : (string) null;
    }

    private static RegistryKey GetRootHive(string keyPath)
    {
      int length = keyPath.IndexOf('\\');
      switch ((length <= 0 ? keyPath : keyPath.Substring(0, length)).ToUpperInvariant())
      {
        case "HKEY_CURRENT_USER":
          return Registry.CurrentUser;
        case "HKEY_LOCAL_MACHINE":
          return Registry.LocalMachine;
        case "HKEY_CLASSES_ROOT":
          return Registry.ClassesRoot;
        case "HKEY_CURRENT_CONFIG":
          return Registry.CurrentConfig;
        case "HKEY_DYN_DATA":
          return Registry.DynData;
        case "HKEY_PERFORMANCE_DATA":
          return Registry.PerformanceData;
        case "HKEY_USERS":
          return Registry.Users;
        default:
          return (RegistryKey) null;
      }
    }

    public override sealed void Uninstall(IDictionary savedState)
    {
      base.Uninstall(savedState);
      if (this.Context != null && this.Context.Parameters != null && (this.Context.Parameters.ContainsKey("RegFile") && !string.IsNullOrEmpty(this.Context.Parameters["RegFile"])))
        return;
      int length = this.RegKey.LastIndexOf('\\');
      string str;
      string subkey;
      if (length >= 0)
      {
        str = this.RegKey.Substring(0, length);
        subkey = this.RegKey.Substring(length + 1);
      }
      else
      {
        str = "";
        subkey = this.RegKey;
      }
      foreach (string mshRegistryRoot in PSInstaller.MshRegistryRoots)
        this.GetRegistryKey(mshRegistryRoot + str).DeleteSubKey(subkey);
    }

    public override sealed void Rollback(IDictionary savedState) => this.Uninstall(savedState);
  }
}
