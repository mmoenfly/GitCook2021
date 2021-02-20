// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SecuritySupport
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation.Internal;
using System.Management.Automation.Security;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace System.Management.Automation
{
  internal static class SecuritySupport
  {
    [TraceSource("SecuritySupport", "SecuritySupport")]
    private static PSTraceSource _tracer = PSTraceSource.GetTracer(nameof (SecuritySupport), nameof (SecuritySupport));

    internal static ExecutionPolicyScope[] ExecutionPolicyScopePreferences => new ExecutionPolicyScope[5]
    {
      ExecutionPolicyScope.MachinePolicy,
      ExecutionPolicyScope.UserPolicy,
      ExecutionPolicyScope.Process,
      ExecutionPolicyScope.CurrentUser,
      ExecutionPolicyScope.LocalMachine
    };

    internal static void SetExecutionPolicy(
      ExecutionPolicyScope scope,
      ExecutionPolicy policy,
      string shellId)
    {
      string str = "Restricted";
      string configurationPath = Utils.GetRegistryConfigurationPath(shellId);
      switch (policy)
      {
        case ExecutionPolicy.Unrestricted:
          str = "Unrestricted";
          break;
        case ExecutionPolicy.RemoteSigned:
          str = "RemoteSigned";
          break;
        case ExecutionPolicy.AllSigned:
          str = "AllSigned";
          break;
        case ExecutionPolicy.Restricted:
          str = "Restricted";
          break;
        case ExecutionPolicy.Bypass:
          str = "Bypass";
          break;
      }
      switch (scope)
      {
        case ExecutionPolicyScope.Process:
          if (policy == ExecutionPolicy.Undefined)
            str = (string) null;
          Environment.SetEnvironmentVariable("PSExecutionPolicyPreference", str);
          break;
        case ExecutionPolicyScope.CurrentUser:
          if (policy == ExecutionPolicy.Undefined)
          {
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(configurationPath, true))
            {
              if (registryKey != null)
              {
                if (registryKey.GetValue("ExecutionPolicy") != null)
                  registryKey.DeleteValue("ExecutionPolicy");
              }
            }
            SecuritySupport.CleanKeyParents(Registry.CurrentUser, configurationPath);
            break;
          }
          using (RegistryKey subKey = Registry.CurrentUser.CreateSubKey(configurationPath))
          {
            subKey.SetValue("ExecutionPolicy", (object) str, RegistryValueKind.String);
            break;
          }
        case ExecutionPolicyScope.LocalMachine:
          if (policy == ExecutionPolicy.Undefined)
          {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(configurationPath, true))
            {
              if (registryKey != null)
              {
                if (registryKey.GetValue("ExecutionPolicy") != null)
                  registryKey.DeleteValue("ExecutionPolicy");
              }
            }
            SecuritySupport.CleanKeyParents(Registry.LocalMachine, configurationPath);
            break;
          }
          using (RegistryKey subKey = Registry.LocalMachine.CreateSubKey(configurationPath))
          {
            subKey.SetValue("ExecutionPolicy", (object) str, RegistryValueKind.String);
            break;
          }
      }
    }

    private static void CleanKeyParents(RegistryKey baseKey, string keyPath)
    {
      using (RegistryKey registryKey1 = baseKey.OpenSubKey(keyPath, true))
      {
        if (registryKey1 != null && (registryKey1.ValueCount != 0 || registryKey1.SubKeyCount != 0))
          return;
        string[] strArray = keyPath.Split('\\');
        if (strArray.Length <= 2)
          return;
        string subkey = strArray[strArray.Length - 1];
        string str = keyPath.Remove(keyPath.Length - subkey.Length - 1);
        if (registryKey1 != null)
        {
          using (RegistryKey registryKey2 = baseKey.OpenSubKey(str, true))
            registryKey2.DeleteSubKey(subkey, true);
        }
        SecuritySupport.CleanKeyParents(baseKey, str);
      }
    }

    internal static ExecutionPolicy GetExecutionPolicy(string shellId)
    {
      foreach (ExecutionPolicyScope policyScopePreference in SecuritySupport.ExecutionPolicyScopePreferences)
      {
        ExecutionPolicy executionPolicy = SecuritySupport.GetExecutionPolicy(shellId, policyScopePreference);
        if (executionPolicy != ExecutionPolicy.Undefined)
          return executionPolicy;
      }
      return ExecutionPolicy.Restricted;
    }

    internal static ExecutionPolicy GetExecutionPolicy(
      string shellId,
      ExecutionPolicyScope scope)
    {
      switch (scope)
      {
        case ExecutionPolicyScope.Process:
          string environmentVariable = Environment.GetEnvironmentVariable("PSExecutionPolicyPreference");
          return !string.IsNullOrEmpty(environmentVariable) ? SecuritySupport.ParseExecutionPolicy(environmentVariable) : ExecutionPolicy.Undefined;
        case ExecutionPolicyScope.CurrentUser:
        case ExecutionPolicyScope.LocalMachine:
          string localPreferenceValue = SecuritySupport.GetLocalPreferenceValue(shellId, scope);
          return !string.IsNullOrEmpty(localPreferenceValue) ? SecuritySupport.ParseExecutionPolicy(localPreferenceValue) : ExecutionPolicy.Undefined;
        case ExecutionPolicyScope.UserPolicy:
        case ExecutionPolicyScope.MachinePolicy:
          string groupPolicyValue = SecuritySupport.GetGroupPolicyValue(shellId, scope);
          if (!string.IsNullOrEmpty(groupPolicyValue))
          {
            Process process = Process.GetCurrentProcess();
            string a = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "gpscript.exe");
            bool flag = false;
            try
            {
              for (; process != null; process = PsUtils.GetParentProcess(process))
              {
                if (string.Equals(a, PsUtils.GetMainModule(process).FileName, StringComparison.OrdinalIgnoreCase))
                {
                  flag = true;
                  break;
                }
              }
            }
            catch (Win32Exception ex)
            {
            }
            if (!flag)
              return SecuritySupport.ParseExecutionPolicy(groupPolicyValue);
          }
          return ExecutionPolicy.Undefined;
        default:
          return ExecutionPolicy.Restricted;
      }
    }

    internal static ExecutionPolicy ParseExecutionPolicy(string policy)
    {
      if (string.Equals(policy, "Bypass", StringComparison.OrdinalIgnoreCase))
        return ExecutionPolicy.Bypass;
      if (string.Equals(policy, "Unrestricted", StringComparison.OrdinalIgnoreCase))
        return ExecutionPolicy.Unrestricted;
      if (string.Equals(policy, "RemoteSigned", StringComparison.OrdinalIgnoreCase))
        return ExecutionPolicy.RemoteSigned;
      if (string.Equals(policy, "AllSigned", StringComparison.OrdinalIgnoreCase))
        return ExecutionPolicy.AllSigned;
      return string.Equals(policy, "Restricted", StringComparison.OrdinalIgnoreCase) ? ExecutionPolicy.Restricted : ExecutionPolicy.Restricted;
    }

    internal static string GetExecutionPolicy(ExecutionPolicy policy)
    {
      switch (policy)
      {
        case ExecutionPolicy.Unrestricted:
          return "Unrestricted";
        case ExecutionPolicy.RemoteSigned:
          return "RemoteSigned";
        case ExecutionPolicy.AllSigned:
          return "AllSigned";
        case ExecutionPolicy.Restricted:
          return "Restricted";
        case ExecutionPolicy.Bypass:
          return "Bypass";
        default:
          return "Restricted";
      }
    }

    [ArchitectureSensitive]
    internal static SaferPolicy GetSaferPolicy(string path)
    {
      SaferPolicy saferPolicy = SaferPolicy.Allowed;
      IntPtr pLevelHandle;
      if (!System.Management.Automation.Security.NativeMethods.SaferIdentifyLevel(1U, ref new SAFER_CODE_PROPERTIES()
      {
        cbSize = (uint) Marshal.SizeOf(typeof (SAFER_CODE_PROPERTIES)),
        dwCheckFlags = 13U,
        ImagePath = path,
        dwWVTUIChoice = 2U
      }, out pLevelHandle, "SCRIPT"))
        throw new Win32Exception(Marshal.GetLastWin32Error());
      IntPtr zero = IntPtr.Zero;
      try
      {
        if (!System.Management.Automation.Security.NativeMethods.SaferComputeTokenFromLevel(pLevelHandle, IntPtr.Zero, ref zero, 1U, IntPtr.Zero))
        {
          switch (Marshal.GetLastWin32Error())
          {
            case 786:
            case 1260:
              saferPolicy = SaferPolicy.Disallowed;
              break;
            default:
              throw new Win32Exception();
          }
        }
        else if (zero == IntPtr.Zero)
        {
          saferPolicy = SaferPolicy.Allowed;
        }
        else
        {
          saferPolicy = SaferPolicy.Disallowed;
          System.Management.Automation.Security.NativeMethods.CloseHandle(zero);
        }
      }
      finally
      {
        System.Management.Automation.Security.NativeMethods.SaferCloseLevel(pLevelHandle);
      }
      return saferPolicy;
    }

    private static string GetGroupPolicyValue(string shellId, ExecutionPolicyScope scope)
    {
      switch (scope)
      {
        case ExecutionPolicyScope.UserPolicy:
          try
          {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Policies\\Microsoft\\Windows\\PowerShell"))
            {
              switch (SecuritySupport.GetRegistryKeyFromGroupPolicyTest("Software\\Policies\\Microsoft\\Windows\\PowerShell", "EnableScripts", key))
              {
                case SecuritySupport.GroupPolicyStatus.Enabled:
                  return key.GetValue("ExecutionPolicy") as string;
                case SecuritySupport.GroupPolicyStatus.Disabled:
                  key.Close();
                  return "Restricted";
              }
            }
          }
          catch (SecurityException ex)
          {
          }
          return (string) null;
        case ExecutionPolicyScope.MachinePolicy:
          try
          {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Policies\\Microsoft\\Windows\\PowerShell"))
            {
              if (key != null)
              {
                switch (SecuritySupport.GetRegistryKeyFromGroupPolicyTest("Software\\Policies\\Microsoft\\Windows\\PowerShell", "EnableScripts", key))
                {
                  case SecuritySupport.GroupPolicyStatus.Enabled:
                    return key.GetValue("ExecutionPolicy") as string;
                  case SecuritySupport.GroupPolicyStatus.Disabled:
                    key.Close();
                    return "Restricted";
                }
              }
            }
          }
          catch (SecurityException ex)
          {
          }
          return (string) null;
        default:
          return (string) null;
      }
    }

    private static string GetLocalPreferenceValue(string shellId, ExecutionPolicyScope scope)
    {
      string configurationPath = Utils.GetRegistryConfigurationPath(shellId);
      switch (scope)
      {
        case ExecutionPolicyScope.CurrentUser:
          using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(configurationPath))
          {
            if (registryKey != null)
            {
              string str = registryKey.GetValue("ExecutionPolicy") as string;
              registryKey.Close();
              return str;
            }
            break;
          }
        case ExecutionPolicyScope.LocalMachine:
          using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(configurationPath))
          {
            if (registryKey != null)
            {
              string str = registryKey.GetValue("ExecutionPolicy") as string;
              registryKey.Close();
              return str;
            }
            break;
          }
      }
      return (string) null;
    }

    private static SecuritySupport.GroupPolicyStatus GetRegistryKeyFromGroupPolicyTest(
      string GroupPolicyKey,
      string GroupPolicyValue,
      RegistryKey key)
    {
      if (key != null)
      {
        object obj = key.GetValue(GroupPolicyValue);
        if (obj != null)
        {
          if (string.Equals(obj.ToString(), "0", StringComparison.OrdinalIgnoreCase))
            return SecuritySupport.GroupPolicyStatus.Disabled;
          return string.Equals(obj.ToString(), "1", StringComparison.OrdinalIgnoreCase) ? SecuritySupport.GroupPolicyStatus.Enabled : SecuritySupport.GroupPolicyStatus.Undefined;
        }
      }
      return SecuritySupport.GroupPolicyStatus.Undefined;
    }

    internal static void CheckIfFileExists(string filePath)
    {
      using (SecuritySupport._tracer.TraceMethod())
      {
        if (!File.Exists(filePath))
          throw new FileNotFoundException(filePath);
      }
    }

    internal static bool CertIsGoodForSigning(X509Certificate2 c)
    {
      using (SecuritySupport._tracer.TraceMethod())
      {
        if (!SecuritySupport.CertHasPrivatekey(c))
          return false;
        foreach (string str in SecuritySupport.GetCertEKU(c))
        {
          if (str == "1.3.6.1.5.5.7.3.3")
            return true;
        }
        return false;
      }
    }

    internal static bool CertHasPrivatekey(X509Certificate2 cert)
    {
      using (SecuritySupport._tracer.TraceMethod())
        return cert.HasPrivateKey;
    }

    [ArchitectureSensitive]
    internal static Collection<string> GetCertEKU(X509Certificate2 cert)
    {
      using (SecuritySupport._tracer.TraceMethod())
      {
        Collection<string> collection = new Collection<string>();
        IntPtr handle = cert.Handle;
        int pcbUsage = 0;
        IntPtr zero = IntPtr.Zero;
        if (!System.Management.Automation.Security.NativeMethods.CertGetEnhancedKeyUsage(handle, 0U, zero, out pcbUsage))
          throw new Win32Exception(Marshal.GetLastWin32Error());
        if (pcbUsage > 0)
        {
          IntPtr num = Marshal.AllocHGlobal(pcbUsage);
          try
          {
            System.Management.Automation.Security.NativeMethods.CERT_ENHKEY_USAGE certEnhkeyUsage = System.Management.Automation.Security.NativeMethods.CertGetEnhancedKeyUsage(handle, 0U, num, out pcbUsage) ? (System.Management.Automation.Security.NativeMethods.CERT_ENHKEY_USAGE) Marshal.PtrToStructure(num, typeof (System.Management.Automation.Security.NativeMethods.CERT_ENHKEY_USAGE)) : throw new Win32Exception(Marshal.GetLastWin32Error());
            IntPtr rgpszUsageIdentifier = certEnhkeyUsage.rgpszUsageIdentifier;
            for (int index = 0; (long) index < (long) certEnhkeyUsage.cUsageIdentifier; ++index)
            {
              string stringAnsi = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(rgpszUsageIdentifier, index * Marshal.SizeOf((object) rgpszUsageIdentifier)));
              collection.Add(stringAnsi);
            }
          }
          finally
          {
            Marshal.FreeHGlobal(num);
          }
        }
        return collection;
      }
    }

    internal static uint GetDWORDFromInt(int n)
    {
      using (SecuritySupport._tracer.TraceMethod())
        return (uint) (4294967296UL + (ulong) n);
    }

    internal static int GetIntFromDWORD(uint n)
    {
      using (SecuritySupport._tracer.TraceMethod())
        return (int) ((long) n - 4294967296L);
    }

    private enum GroupPolicyStatus
    {
      Enabled,
      Disabled,
      Undefined,
    }
  }
}
