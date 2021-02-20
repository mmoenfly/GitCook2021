// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.WindowsErrorReporting
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Management.Automation
{
  internal class WindowsErrorReporting
  {
    private const string powerShellEventType = "PowerShell";
    private static readonly string[] powerShellModulesWithoutGlobalMembers = new string[10]
    {
      "Microsoft.PowerShell.Commands.Diagnostics.dll",
      "Microsoft.PowerShell.Commands.Management.dll",
      "Microsoft.PowerShell.Commands.Utility.dll",
      "Microsoft.PowerShell.Security.dll",
      "System.Management.Automation.dll",
      "Microsoft.PowerShell.ConsoleHost.dll",
      "CompiledComposition.Microsoft.PowerShell.GPowerShell.dll",
      "Microsoft.PowerShell.Editor.dll",
      "Microsoft.PowerShell.GPowerShell.dll",
      "Microsoft.PowerShell.GraphicalHost.dll"
    };
    private static readonly string[] powerShellModulesWithGlobalMembers = new string[7]
    {
      "powershell.exe",
      "powershell_ise.exe",
      "pspluginwkr.dll",
      "pwrshplugin.dll",
      "pwrshsip.dll",
      "pshmsglh.dll",
      "PSEvents.dll"
    };
    private static string versionOfPowerShellLibraries = string.Empty;
    private static string nameOfExe = string.Empty;
    private static IntPtr hCurrentProcess = IntPtr.Zero;
    private static IntPtr hwndMainWindow = IntPtr.Zero;
    private static string applicationName = string.Empty;
    private static string applicationPath = string.Empty;
    private static Process currentProcess = (Process) null;
    private static bool? isWindowsErrorReportingAvailable;
    private static readonly object reportCreationLock = new object();
    private static readonly object registrationLock = new object();
    private static bool registered = false;
    private static bool unattendedServerMode = false;

    private static string TruncateExeName(string nameOfExe, int maxLength)
    {
      nameOfExe = nameOfExe.Trim();
      if (nameOfExe.Length > maxLength && nameOfExe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        nameOfExe = nameOfExe.Substring(0, nameOfExe.Length - ".exe".Length);
      return WindowsErrorReporting.TruncateBucketParameter(nameOfExe, maxLength);
    }

    private static string TruncateTypeName(string typeName, int maxLength)
    {
      if (typeName.Length > maxLength)
        typeName = typeName.Substring(typeName.Length - maxLength, maxLength);
      return typeName;
    }

    private static string TruncateExceptionType(string exceptionType, int maxLength)
    {
      if (exceptionType.Length > maxLength && exceptionType.EndsWith("Exception", StringComparison.OrdinalIgnoreCase))
        exceptionType = exceptionType.Substring(0, exceptionType.Length - "Exception".Length);
      if (exceptionType.Length > maxLength)
        exceptionType = WindowsErrorReporting.TruncateTypeName(exceptionType, maxLength);
      return WindowsErrorReporting.TruncateBucketParameter(exceptionType, maxLength);
    }

    private static string TruncateBucketParameter(string message, int maxLength)
    {
      if (message == null)
        return string.Empty;
      int length1 = maxLength * 30 / 100;
      if (message.Length > maxLength)
      {
        int length2 = maxLength - length1 - "..".Length;
        message = message.Substring(0, length1) + ".." + message.Substring(message.Length - length2, length2);
      }
      return message;
    }

    private static string StackFrame2BucketParameter(StackFrame frame, int maxLength)
    {
      MethodBase method = frame.GetMethod();
      if (method == null)
        return string.Empty;
      Type declaringType = method.DeclaringType;
      if (declaringType == null)
        return WindowsErrorReporting.TruncateBucketParameter(method.Name, maxLength);
      string fullName = declaringType.FullName;
      string str = "." + method.Name;
      return WindowsErrorReporting.TruncateBucketParameter((maxLength <= str.Length ? WindowsErrorReporting.TruncateTypeName(fullName, 1) : WindowsErrorReporting.TruncateTypeName(fullName, maxLength - str.Length)) + str, maxLength);
    }

    private static string GetDeepestFrame(Exception exception, int maxLength) => WindowsErrorReporting.StackFrame2BucketParameter(new StackTrace(exception).GetFrame(0), maxLength);

    private static bool IsPowerShellModule(string moduleName, bool globalMember)
    {
      foreach (string withGlobalMember in WindowsErrorReporting.powerShellModulesWithGlobalMembers)
      {
        if (moduleName.Equals(withGlobalMember, StringComparison.OrdinalIgnoreCase))
          return true;
      }
      if (!globalMember)
      {
        foreach (string withoutGlobalMember in WindowsErrorReporting.powerShellModulesWithoutGlobalMembers)
        {
          if (moduleName.Equals(withoutGlobalMember, StringComparison.OrdinalIgnoreCase))
            return true;
        }
      }
      return false;
    }

    private static string GetDeepestPowerShellFrame(Exception exception, int maxLength)
    {
      foreach (StackFrame frame in new StackTrace(exception).GetFrames())
      {
        MethodBase method = frame.GetMethod();
        if (method != null)
        {
          Module module = method.Module;
          if (module != null)
          {
            Type declaringType = method.DeclaringType;
            if (WindowsErrorReporting.IsPowerShellModule(module.Name, declaringType == null))
              return WindowsErrorReporting.StackFrame2BucketParameter(frame, maxLength);
          }
        }
      }
      return string.Empty;
    }

    private static void SetBucketParameter(
      WindowsErrorReporting.ReportHandle reportHandle,
      WindowsErrorReporting.BucketParameterId bucketParameterId,
      string value)
    {
      WindowsErrorReporting.HandleHResult(WindowsErrorReporting.NativeMethods.WerReportSetParameter(reportHandle, bucketParameterId, bucketParameterId.ToString(), value));
    }

    private static string GetThreadName() => Thread.CurrentThread.Name ?? string.Empty;

    private static void SetBucketParameters(
      WindowsErrorReporting.ReportHandle reportHandle,
      Exception uncaughtException)
    {
      Exception exception = uncaughtException;
      while (exception.InnerException != null)
        exception = exception.InnerException;
      WindowsErrorReporting.SetBucketParameter(reportHandle, WindowsErrorReporting.BucketParameterId.NameOfExe, WindowsErrorReporting.TruncateExeName(WindowsErrorReporting.nameOfExe, 20));
      WindowsErrorReporting.SetBucketParameter(reportHandle, WindowsErrorReporting.BucketParameterId.FileVersionOfSystemManagementAutomation, WindowsErrorReporting.TruncateBucketParameter(WindowsErrorReporting.versionOfPowerShellLibraries, 16));
      WindowsErrorReporting.SetBucketParameter(reportHandle, WindowsErrorReporting.BucketParameterId.InnermostExceptionType, WindowsErrorReporting.TruncateExceptionType(exception.GetType().FullName, 40));
      WindowsErrorReporting.SetBucketParameter(reportHandle, WindowsErrorReporting.BucketParameterId.OutermostExceptionType, WindowsErrorReporting.TruncateExceptionType(uncaughtException.GetType().FullName, 40));
      WindowsErrorReporting.SetBucketParameter(reportHandle, WindowsErrorReporting.BucketParameterId.DeepestFrame, WindowsErrorReporting.GetDeepestFrame(uncaughtException, 50));
      WindowsErrorReporting.SetBucketParameter(reportHandle, WindowsErrorReporting.BucketParameterId.DeepestPowerShellFrame, WindowsErrorReporting.GetDeepestPowerShellFrame(uncaughtException, 50));
      WindowsErrorReporting.SetBucketParameter(reportHandle, WindowsErrorReporting.BucketParameterId.ThreadName, WindowsErrorReporting.TruncateBucketParameter(WindowsErrorReporting.GetThreadName(), 20));
    }

    private static void FindStaticInformation()
    {
      WindowsErrorReporting.versionOfPowerShellLibraries = FileVersionInfo.GetVersionInfo(typeof (PSObject).Assembly.Location).FileVersion;
      WindowsErrorReporting.currentProcess = Process.GetCurrentProcess();
      ProcessModule mainModule = PsUtils.GetMainModule(WindowsErrorReporting.currentProcess);
      if (mainModule != null)
        WindowsErrorReporting.applicationPath = mainModule.FileName;
      else
        WindowsErrorReporting.applicationName = "GetMainModErr";
      WindowsErrorReporting.nameOfExe = Path.GetFileName(WindowsErrorReporting.applicationPath);
      WindowsErrorReporting.hCurrentProcess = WindowsErrorReporting.currentProcess.Handle;
      WindowsErrorReporting.hwndMainWindow = WindowsErrorReporting.currentProcess.MainWindowHandle;
      WindowsErrorReporting.applicationName = WindowsErrorReporting.currentProcess.ProcessName;
    }

    private static void HandleHResult(int hresult) => Marshal.ThrowExceptionForHR(hresult);

    private static bool IsWindowsErrorReportingAvailable()
    {
      if (!WindowsErrorReporting.isWindowsErrorReportingAvailable.HasValue)
        WindowsErrorReporting.isWindowsErrorReportingAvailable = new bool?(Environment.OSVersion.Version.Major >= 6);
      return WindowsErrorReporting.isWindowsErrorReportingAvailable.Value;
    }

    private static void SubmitReport(Exception uncaughtException)
    {
      lock (WindowsErrorReporting.reportCreationLock)
      {
        if (uncaughtException == null)
          throw new ArgumentNullException(nameof (uncaughtException));
        WindowsErrorReporting.ReportInformation reportInformation = new WindowsErrorReporting.ReportInformation();
        reportInformation.dwSize = Marshal.SizeOf((object) reportInformation);
        reportInformation.hProcess = WindowsErrorReporting.hCurrentProcess;
        reportInformation.hwndParent = WindowsErrorReporting.hwndMainWindow;
        reportInformation.wzApplicationName = WindowsErrorReporting.applicationName;
        reportInformation.wzApplicationPath = WindowsErrorReporting.applicationPath;
        reportInformation.wzConsentKey = (string) null;
        reportInformation.wzDescription = (string) null;
        reportInformation.wzFriendlyEventName = (string) null;
        WindowsErrorReporting.ReportHandle reportHandle;
        WindowsErrorReporting.HandleHResult(WindowsErrorReporting.NativeMethods.WerReportCreate("PowerShell", WindowsErrorReporting.ReportType.WerReportCritical, reportInformation, out reportHandle));
        using (reportHandle)
        {
          WindowsErrorReporting.SetBucketParameters(reportHandle, uncaughtException);
          WindowsErrorReporting.HandleHResult(WindowsErrorReporting.NativeMethods.WerReportAddDump(reportHandle, WindowsErrorReporting.hCurrentProcess, IntPtr.Zero, WindowsErrorReporting.DumpType.MiniDump, IntPtr.Zero, IntPtr.Zero, (WindowsErrorReporting.DumpFlags) 0));
          WindowsErrorReporting.SubmitResult result = WindowsErrorReporting.SubmitResult.ReportFailed;
          WindowsErrorReporting.SubmitFlags flags = WindowsErrorReporting.SubmitFlags.HonorRecovery | WindowsErrorReporting.SubmitFlags.HonorRestart | WindowsErrorReporting.SubmitFlags.AddRegisteredData | WindowsErrorReporting.SubmitFlags.OutOfProcess;
          if (WindowsErrorReporting.unattendedServerMode)
            flags |= WindowsErrorReporting.SubmitFlags.Queue;
          WindowsErrorReporting.HandleHResult(WindowsErrorReporting.NativeMethods.WerReportSubmit(reportHandle, WindowsErrorReporting.Consent.NotAsked, flags, out result));
          Environment.Exit((int) result);
        }
      }
    }

    internal static void WaitForPendingReports()
    {
      lock (WindowsErrorReporting.reportCreationLock)
        ;
    }

    internal static void FailFast(Exception exception)
    {
      if (exception == null)
        throw new ArgumentNullException(nameof (exception));
      try
      {
        if (!WindowsErrorReporting.registered)
          return;
        WindowsErrorReporting.SubmitReport(exception);
      }
      finally
      {
        Environment.FailFast(exception.Message);
      }
    }

    internal static void RegisterWindowsErrorReporting(bool unattendedServer)
    {
      lock (WindowsErrorReporting.registrationLock)
      {
        if (WindowsErrorReporting.registered || !WindowsErrorReporting.IsWindowsErrorReportingAvailable())
          return;
        WindowsErrorReporting.FindStaticInformation();
        WindowsErrorReporting.unattendedServerMode = unattendedServer;
        if (unattendedServer)
          WindowsErrorReporting.HandleHResult(WindowsErrorReporting.NativeMethods.WerSetFlags(WindowsErrorReporting.ReportingFlags.Queue));
        else
          WindowsErrorReporting.HandleHResult(WindowsErrorReporting.NativeMethods.WerSetFlags((WindowsErrorReporting.ReportingFlags) 0));
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(WindowsErrorReporting.CurrentDomain_UnhandledException);
        WindowsErrorReporting.registered = true;
      }
    }

    private static void CurrentDomain_UnhandledException(
      object sender,
      UnhandledExceptionEventArgs e)
    {
      if (!(e.ExceptionObject is Exception exceptionObject))
        return;
      WindowsErrorReporting.SubmitReport(exceptionObject);
    }

    internal static void WriteMiniDump(string file) => WindowsErrorReporting.WriteMiniDump(file, WindowsErrorReporting.MiniDumpType.MiniDumpNormal);

    internal static void WriteMiniDump(string file, WindowsErrorReporting.MiniDumpType dumpType)
    {
      Process currentProcess = Process.GetCurrentProcess();
      using (FileStream fileStream = new FileStream(file, FileMode.Create))
        WindowsErrorReporting.NativeMethods.MiniDumpWriteDump(currentProcess.Handle, currentProcess.Id, fileStream.SafeFileHandle, dumpType, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
    }

    [System.Flags]
    private enum DumpFlags : uint
    {
      NoHeap_OnQueue = 1,
    }

    private enum DumpType : uint
    {
      MicroDump = 1,
      MiniDump = 2,
      HeapDump = 3,
    }

    private enum BucketParameterId : uint
    {
      NameOfExe,
      FileVersionOfSystemManagementAutomation,
      InnermostExceptionType,
      OutermostExceptionType,
      DeepestPowerShellFrame,
      DeepestFrame,
      ThreadName,
      Param7,
      Param8,
      Param9,
    }

    private enum ReportType : uint
    {
      WerReportNonCritical,
      WerReportCritical,
      WerReportApplicationCrash,
      WerReportApplicationHang,
      WerReportKernel,
      WerReportInvalid,
    }

    [System.Flags]
    internal enum MiniDumpType : uint
    {
      MiniDumpNormal = 0,
      MiniDumpWithDataSegs = 1,
      MiniDumpWithFullMemory = 2,
      MiniDumpWithHandleData = 4,
      MiniDumpFilterMemory = 8,
      MiniDumpScanMemory = 16, // 0x00000010
      MiniDumpWithUnloadedModules = 32, // 0x00000020
      MiniDumpWithIndirectlyReferencedMemory = 64, // 0x00000040
      MiniDumpFilterModulePaths = 128, // 0x00000080
      MiniDumpWithProcessThreadData = 256, // 0x00000100
      MiniDumpWithPrivateReadWriteMemory = 512, // 0x00000200
      MiniDumpWithoutOptionalData = 1024, // 0x00000400
      MiniDumpWithFullMemoryInfo = 2048, // 0x00000800
      MiniDumpWithThreadInfo = 4096, // 0x00001000
      MiniDumpWithCodeSegs = 8192, // 0x00002000
    }

    private enum Consent : uint
    {
      NotAsked = 1,
      Approved = 2,
      Denied = 3,
      AlwaysPrompt = 4,
    }

    [System.Flags]
    private enum SubmitFlags : uint
    {
      HonorRecovery = 1,
      HonorRestart = 2,
      Queue = 4,
      ShowDebug = 8,
      AddRegisteredData = 16, // 0x00000010
      OutOfProcess = 32, // 0x00000020
      NoCloseUI = 64, // 0x00000040
      NoQueue = 128, // 0x00000080
      NoArchive = 256, // 0x00000100
      StartMinimized = 512, // 0x00000200
      OutOfProcesAsync = 1024, // 0x00000400
      BypassDataThrottling = 2048, // 0x00000800
      ArchiveParametersOnly = 4096, // 0x00001000
    }

    private enum SubmitResult : uint
    {
      ReportQueued = 1,
      ReportUploaded = 2,
      ReportDebug = 3,
      ReportFailed = 4,
      Disabled = 5,
      ReportCancelled = 6,
      DisabledQueue = 7,
      ReportAsync = 8,
      CustomAction = 9,
    }

    private enum ReportingFlags : uint
    {
      NoHeap = 1,
      Queue = 2,
      DisableThreadSuspension = 4,
      QueueUpload = 8,
    }

    private class ReportHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
      private ReportHandle()
        : base(true)
      {
      }

      protected override bool ReleaseHandle() => 0 == WindowsErrorReporting.NativeMethods.WerReportCloseHandle(this.handle);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private class ReportInformation
    {
      private const int MAX_PATH = 260;
      internal int dwSize;
      internal IntPtr hProcess;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      internal string wzConsentKey;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      internal string wzFriendlyEventName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      internal string wzApplicationName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      internal string wzApplicationPath;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
      internal string wzDescription;
      internal IntPtr hwndParent;
    }

    private static class NativeMethods
    {
      internal const string WerDll = "wer.dll";

      [DllImport("wer.dll", CharSet = CharSet.Unicode)]
      internal static extern int WerReportCreate(
        [MarshalAs(UnmanagedType.LPWStr)] string pwzEventType,
        WindowsErrorReporting.ReportType repType,
        [MarshalAs(UnmanagedType.LPStruct)] WindowsErrorReporting.ReportInformation reportInformation,
        out WindowsErrorReporting.ReportHandle reportHandle);

      [DllImport("wer.dll", CharSet = CharSet.Unicode)]
      internal static extern int WerReportSetParameter(
        WindowsErrorReporting.ReportHandle reportHandle,
        WindowsErrorReporting.BucketParameterId bucketParameterId,
        [MarshalAs(UnmanagedType.LPWStr)] string name,
        [MarshalAs(UnmanagedType.LPWStr)] string value);

      [DllImport("wer.dll", CharSet = CharSet.Unicode)]
      internal static extern int WerReportAddDump(
        WindowsErrorReporting.ReportHandle reportHandle,
        IntPtr hProcess,
        IntPtr hThread,
        WindowsErrorReporting.DumpType dumpType,
        IntPtr pExceptionParam,
        IntPtr dumpCustomOptions,
        WindowsErrorReporting.DumpFlags dumpFlags);

      [DllImport("wer.dll")]
      internal static extern int WerReportSubmit(
        WindowsErrorReporting.ReportHandle reportHandle,
        WindowsErrorReporting.Consent consent,
        WindowsErrorReporting.SubmitFlags flags,
        out WindowsErrorReporting.SubmitResult result);

      [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
      [DllImport("wer.dll")]
      internal static extern int WerReportCloseHandle(IntPtr reportHandle);

      [DllImport("kernel32.dll")]
      internal static extern int WerSetFlags(WindowsErrorReporting.ReportingFlags flags);

      [DllImport("DbgHelp.dll", SetLastError = true)]
      internal static extern bool MiniDumpWriteDump(
        IntPtr hProcess,
        int processId,
        SafeFileHandle hFile,
        WindowsErrorReporting.MiniDumpType dumpType,
        IntPtr exceptionParam,
        IntPtr userStreamParam,
        IntPtr callackParam);
    }
  }
}
