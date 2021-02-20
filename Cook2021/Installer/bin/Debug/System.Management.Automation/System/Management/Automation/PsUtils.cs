// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PsUtils
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Management.Automation
{
  internal static class PsUtils
  {
    internal static ProcessModule GetMainModule(Process targetProcess)
    {
      int num = 0;
      ProcessModule processModule = (ProcessModule) null;
      while (processModule == null)
      {
        try
        {
          processModule = targetProcess.MainModule;
        }
        catch (Win32Exception ex)
        {
          ++num;
          Thread.Sleep(100);
          if (num == 5)
            throw;
        }
      }
      return processModule;
    }

    internal static Process GetParentProcess(Process current)
    {
      using (ManagementObject managementObject = new ManagementObject(string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, "win32_process.handle='{0}'", (object) current.Id)))
      {
        managementObject.Get();
        int int32 = Convert.ToInt32(managementObject["ParentProcessId"], (IFormatProvider) CultureInfo.CurrentCulture);
        if (int32 == 0)
          return (Process) null;
        try
        {
          Process processById = Process.GetProcessById(int32);
          return processById.StartTime <= current.StartTime ? processById : (Process) null;
        }
        catch (ArgumentException ex)
        {
          return (Process) null;
        }
      }
    }

    internal static unsafe uint GetStackSize()
    {
      Microsoft.PowerShell.Commands.Internal.Win32Native.MEMORY_BASIC_INFORMATION lpBuffer = new Microsoft.PowerShell.Commands.Internal.Win32Native.MEMORY_BASIC_INFORMATION();
      UIntPtr dwLength = new UIntPtr((uint) sizeof (Microsoft.PowerShell.Commands.Internal.Win32Native.MEMORY_BASIC_INFORMATION));
      IntPtr num = (IntPtr) Microsoft.PowerShell.Commands.Internal.Win32Native.VirtualQuery(new UIntPtr((uint) &lpBuffer - Microsoft.PowerShell.Commands.Internal.Win32Native.PAGE_SIZE), ref lpBuffer, dwLength);
      return (uint) (lpBuffer.BaseAddress.ToUInt64() - lpBuffer.AllocationBase.ToUInt64() + lpBuffer.RegionSize.ToUInt64());
    }

    internal static bool IsDotNetFrameworkVersionInstalled(Version requiredVersion)
    {
      int majorVersion;
      int minorVersion;
      int minimumSpVersion;
      return PsUtils.FrameworkRegistryInstallation.CanCheckFrameworkInstallation(requiredVersion, out majorVersion, out minorVersion, out minimumSpVersion) ? PsUtils.FrameworkRegistryInstallation.IsFrameworkInstalled(majorVersion, minorVersion, minimumSpVersion) : PsUtils.FileSystemIsDotNetFrameworkVersionInstalled(requiredVersion);
    }

    private static bool FileSystemIsDotNetFrameworkVersionInstalled(Version requiredVersion)
    {
      string path1 = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "Microsoft.NET");
      try
      {
        string path2 = "Framework";
        if (PsUtils.GetProcessorArchitecture() != ProcessorArchitecture.X86 && requiredVersion >= new Version(2, 0))
          path2 = "Framework64";
        string[] directories = Directory.GetDirectories(Path.Combine(path1, path2), string.Format((IFormatProvider) null, "v{0}.{1}*", (object) requiredVersion.Major, (object) requiredVersion.Minor));
        if (directories == null || directories.Length == 0)
          return false;
        if (requiredVersion.Build != -1 || requiredVersion.Revision != -1)
        {
          string str = Path.Combine(directories[0], "mscorlib.dll");
          if (File.Exists(str))
          {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(str);
            if (new Version(versionInfo.FileMajorPart < 0 ? 0 : versionInfo.FileMajorPart, versionInfo.FileMinorPart < 0 ? 0 : versionInfo.FileMinorPart, versionInfo.FileBuildPart < 0 ? 0 : versionInfo.FileBuildPart, versionInfo.FilePrivatePart < 0 ? 0 : versionInfo.FilePrivatePart) < requiredVersion)
              return false;
          }
        }
        return true;
      }
      catch (UnauthorizedAccessException ex)
      {
        return false;
      }
      catch (ArgumentException ex)
      {
        return false;
      }
      catch (PathTooLongException ex)
      {
        return false;
      }
      catch (IOException ex)
      {
        return false;
      }
    }

    internal static ProcessorArchitecture GetProcessorArchitecture()
    {
      PsUtils.NativeMethods.SYSTEM_INFO lpSystemInfo = new PsUtils.NativeMethods.SYSTEM_INFO();
      PsUtils.NativeMethods.GetSystemInfo(ref lpSystemInfo);
      switch (lpSystemInfo.wProcessorArchitecture)
      {
        case 0:
          return ProcessorArchitecture.X86;
        case 6:
          return ProcessorArchitecture.IA64;
        case 9:
          return ProcessorArchitecture.Amd64;
        default:
          return ProcessorArchitecture.None;
      }
    }

    internal static class FrameworkRegistryInstallation
    {
      private static Version V3_5 = new Version(3, 5, 21022, 8);
      private static Version V3_5sp1 = new Version(3, 5, 30729, 1);
      private static Version V3_0 = new Version(3, 0, 4506, 30);
      private static Version V3_0sp1 = new Version(3, 0, 4506, 648);
      private static Version V3_0sp2 = new Version(3, 0, 4506, 2152);
      private static Version V2_0 = new Version(2, 0, 50727, 42);
      private static Version V2_0sp1 = new Version(2, 0, 50727, 1433);
      private static Version V2_0sp2 = new Version(2, 0, 50727, 3053);
      private static Version V1_1 = new Version(1, 1, 4322, 573);
      private static Version V1_1sp1 = new Version(1, 1, 4322, 2032);
      private static Version V1_1sp1Server = new Version(1, 1, 4322, 2300);
      private static Version V3_5_00 = new Version(3, 5, 0, 0);
      private static Version V3_0_00 = new Version(3, 0, 0, 0);
      private static Version V2_0_00 = new Version(2, 0, 0, 0);
      private static Version V1_1_00 = new Version(1, 1, 0, 0);

      private static bool GetRegistryNames(
        int majorVersion,
        int minorVersion,
        out string installKeyName,
        out string installValueName,
        out string spKeyName)
      {
        installKeyName = (string) null;
        spKeyName = (string) null;
        installValueName = (string) null;
        if (majorVersion == 1 && minorVersion == 1)
        {
          installKeyName = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v1.1.4322";
          spKeyName = installKeyName;
          installValueName = "Install";
          return true;
        }
        if (majorVersion == 2 && minorVersion == 0)
        {
          installKeyName = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v2.0.50727";
          spKeyName = installKeyName;
          installValueName = "Install";
          return true;
        }
        if (majorVersion == 3 && minorVersion == 0)
        {
          installKeyName = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v3.0\\Setup";
          spKeyName = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v3.0";
          installValueName = "InstallSuccess";
          return true;
        }
        if (majorVersion != 3 || minorVersion != 5)
          return false;
        installKeyName = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v3.5";
        spKeyName = installKeyName;
        installValueName = "Install";
        return true;
      }

      private static int? GetRegistryKeyValueInt(RegistryKey key, string valueName)
      {
        try
        {
          return key.GetValue(valueName) is int num ? new int?(num) : new int?();
        }
        catch (ObjectDisposedException ex)
        {
          return new int?();
        }
        catch (SecurityException ex)
        {
          return new int?();
        }
        catch (IOException ex)
        {
          return new int?();
        }
        catch (UnauthorizedAccessException ex)
        {
          return new int?();
        }
      }

      private static RegistryKey GetRegistryKeySubKey(RegistryKey key, string subKeyName)
      {
        try
        {
          return key.OpenSubKey(subKeyName);
        }
        catch (ObjectDisposedException ex)
        {
          return (RegistryKey) null;
        }
        catch (SecurityException ex)
        {
          return (RegistryKey) null;
        }
        catch (ArgumentException ex)
        {
          return (RegistryKey) null;
        }
      }

      internal static bool CanCheckFrameworkInstallation(
        Version version,
        out int majorVersion,
        out int minorVersion,
        out int minimumSpVersion)
      {
        majorVersion = -1;
        minorVersion = -1;
        minimumSpVersion = -1;
        if (version == PsUtils.FrameworkRegistryInstallation.V3_5 || version == PsUtils.FrameworkRegistryInstallation.V3_5_00)
        {
          majorVersion = 3;
          minorVersion = 5;
          minimumSpVersion = 0;
          return true;
        }
        if (version == PsUtils.FrameworkRegistryInstallation.V3_5sp1)
        {
          majorVersion = 3;
          minorVersion = 5;
          minimumSpVersion = 1;
          return true;
        }
        if (version == PsUtils.FrameworkRegistryInstallation.V3_0 || version == PsUtils.FrameworkRegistryInstallation.V3_0_00)
        {
          majorVersion = 3;
          minorVersion = 0;
          minimumSpVersion = 0;
          return true;
        }
        if (version == PsUtils.FrameworkRegistryInstallation.V3_0sp1)
        {
          majorVersion = 3;
          minorVersion = 0;
          minimumSpVersion = 1;
          return true;
        }
        if (version == PsUtils.FrameworkRegistryInstallation.V3_0sp2)
        {
          majorVersion = 3;
          minorVersion = 0;
          minimumSpVersion = 2;
          return true;
        }
        if (version == PsUtils.FrameworkRegistryInstallation.V2_0 || version == PsUtils.FrameworkRegistryInstallation.V2_0_00)
        {
          majorVersion = 2;
          minorVersion = 0;
          minimumSpVersion = 0;
          return true;
        }
        if (version == PsUtils.FrameworkRegistryInstallation.V2_0sp1)
        {
          majorVersion = 2;
          minorVersion = 0;
          minimumSpVersion = 1;
          return true;
        }
        if (version == PsUtils.FrameworkRegistryInstallation.V2_0sp2)
        {
          majorVersion = 2;
          minorVersion = 0;
          minimumSpVersion = 2;
          return true;
        }
        if (version == PsUtils.FrameworkRegistryInstallation.V1_1 || version == PsUtils.FrameworkRegistryInstallation.V1_1_00)
        {
          majorVersion = 1;
          minorVersion = 1;
          minimumSpVersion = 0;
          return true;
        }
        if (!(version == PsUtils.FrameworkRegistryInstallation.V1_1sp1) && !(version == PsUtils.FrameworkRegistryInstallation.V1_1sp1Server))
          return false;
        majorVersion = 1;
        minorVersion = 1;
        minimumSpVersion = 1;
        return true;
      }

      internal static bool IsFrameworkInstalled(Version version)
      {
        int majorVersion;
        int minorVersion;
        int minimumSpVersion;
        return PsUtils.FrameworkRegistryInstallation.CanCheckFrameworkInstallation(version, out majorVersion, out minorVersion, out minimumSpVersion) && PsUtils.FrameworkRegistryInstallation.IsFrameworkInstalled(majorVersion, minorVersion, minimumSpVersion);
      }

      internal static bool IsFrameworkInstalled(
        int majorVersion,
        int minorVersion,
        int minimumSPVersion)
      {
        string installKeyName;
        string installValueName;
        string spKeyName;
        if (!PsUtils.FrameworkRegistryInstallation.GetRegistryNames(majorVersion, minorVersion, out installKeyName, out installValueName, out spKeyName))
          return false;
        RegistryKey registryKeySubKey1 = PsUtils.FrameworkRegistryInstallation.GetRegistryKeySubKey(Registry.LocalMachine, installKeyName);
        if (registryKeySubKey1 == null)
          return false;
        int? registryKeyValueInt1 = PsUtils.FrameworkRegistryInstallation.GetRegistryKeyValueInt(registryKeySubKey1, installValueName);
        if (!registryKeyValueInt1.HasValue)
          return false;
        int? nullable1 = registryKeyValueInt1;
        if ((nullable1.GetValueOrDefault() != 1 ? 1 : (!nullable1.HasValue ? 1 : 0)) != 0)
          return false;
        if (minimumSPVersion > 0)
        {
          RegistryKey registryKeySubKey2 = PsUtils.FrameworkRegistryInstallation.GetRegistryKeySubKey(Registry.LocalMachine, spKeyName);
          if (registryKeySubKey2 == null)
            return false;
          int? registryKeyValueInt2 = PsUtils.FrameworkRegistryInstallation.GetRegistryKeyValueInt(registryKeySubKey2, "SP");
          if (!registryKeyValueInt2.HasValue)
            return false;
          int? nullable2 = registryKeyValueInt2;
          int num = minimumSPVersion;
          if ((nullable2.GetValueOrDefault() >= num ? 0 : (nullable2.HasValue ? 1 : 0)) != 0)
            return false;
        }
        return true;
      }
    }

    private static class NativeMethods
    {
      internal const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
      internal const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
      internal const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
      internal const ushort PROCESSOR_ARCHITECTURE_UNKNOWN = 65535;

      [DllImport("kernel32.dll")]
      internal static extern void GetSystemInfo(ref PsUtils.NativeMethods.SYSTEM_INFO lpSystemInfo);

      internal struct SYSTEM_INFO
      {
        public ushort wProcessorArchitecture;
        public ushort wReserved;
        public uint dwPageSize;
        public IntPtr lpMinimumApplicationAddress;
        public IntPtr lpMaximumApplicationAddress;
        public UIntPtr dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public ushort wProcessorLevel;
        public ushort wProcessorRevision;
      }
    }
  }
}
