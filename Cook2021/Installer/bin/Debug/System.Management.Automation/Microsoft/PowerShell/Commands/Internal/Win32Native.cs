// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Win32Native
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Microsoft.PowerShell.Commands.Internal
{
  [SuppressUnmanagedCodeSecurity]
  internal static class Win32Native
  {
    private const string resBaseName = "RegistryProviderStrings";
    internal const int KEY_QUERY_VALUE = 1;
    internal const int KEY_SET_VALUE = 2;
    internal const int KEY_CREATE_SUB_KEY = 4;
    internal const int KEY_ENUMERATE_SUB_KEYS = 8;
    internal const int KEY_NOTIFY = 16;
    internal const int KEY_CREATE_LINK = 32;
    internal const int KEY_READ = 131097;
    internal const int KEY_WRITE = 131078;
    internal const int KEY_WOW64_64KEY = 256;
    internal const int KEY_WOW64_32KEY = 512;
    internal const int REG_NONE = 0;
    internal const int REG_SZ = 1;
    internal const int REG_EXPAND_SZ = 2;
    internal const int REG_BINARY = 3;
    internal const int REG_DWORD = 4;
    internal const int REG_DWORD_LITTLE_ENDIAN = 4;
    internal const int REG_DWORD_BIG_ENDIAN = 5;
    internal const int REG_LINK = 6;
    internal const int REG_MULTI_SZ = 7;
    internal const int REG_RESOURCE_LIST = 8;
    internal const int REG_FULL_RESOURCE_DESCRIPTOR = 9;
    internal const int REG_RESOURCE_REQUIREMENTS_LIST = 10;
    internal const int REG_QWORD = 11;
    internal const int HWND_BROADCAST = 65535;
    internal const int WM_SETTINGCHANGE = 26;
    internal const uint CRYPTPROTECTMEMORY_BLOCK_SIZE = 16;
    internal const uint CRYPTPROTECTMEMORY_SAME_PROCESS = 0;
    internal const uint CRYPTPROTECTMEMORY_CROSS_PROCESS = 1;
    internal const uint CRYPTPROTECTMEMORY_SAME_LOGON = 2;
    internal const int SECURITY_ANONYMOUS = 0;
    internal const int SECURITY_SQOS_PRESENT = 1048576;
    internal const string MICROSOFT_KERBEROS_NAME = "Kerberos";
    internal const uint ANONYMOUS_LOGON_LUID = 998;
    internal const int SECURITY_ANONYMOUS_LOGON_RID = 7;
    internal const int SECURITY_AUTHENTICATED_USER_RID = 11;
    internal const int SECURITY_LOCAL_SYSTEM_RID = 18;
    internal const int SECURITY_BUILTIN_DOMAIN_RID = 32;
    internal const int DOMAIN_USER_RID_GUEST = 501;
    internal const uint SE_PRIVILEGE_DISABLED = 0;
    internal const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 1;
    internal const uint SE_PRIVILEGE_ENABLED = 2;
    internal const uint SE_PRIVILEGE_USED_FOR_ACCESS = 2147483648;
    internal const uint SE_GROUP_MANDATORY = 1;
    internal const uint SE_GROUP_ENABLED_BY_DEFAULT = 2;
    internal const uint SE_GROUP_ENABLED = 4;
    internal const uint SE_GROUP_OWNER = 8;
    internal const uint SE_GROUP_USE_FOR_DENY_ONLY = 16;
    internal const uint SE_GROUP_LOGON_ID = 3221225472;
    internal const uint SE_GROUP_RESOURCE = 536870912;
    internal const uint DUPLICATE_CLOSE_SOURCE = 1;
    internal const uint DUPLICATE_SAME_ACCESS = 2;
    internal const uint DUPLICATE_SAME_ATTRIBUTES = 4;
    internal const int READ_CONTROL = 131072;
    internal const int SYNCHRONIZE = 1048576;
    internal const int STANDARD_RIGHTS_READ = 131072;
    internal const int STANDARD_RIGHTS_WRITE = 131072;
    internal const int SEMAPHORE_MODIFY_STATE = 2;
    internal const int EVENT_MODIFY_STATE = 2;
    internal const int MUTEX_MODIFY_STATE = 1;
    internal const int MUTEX_ALL_ACCESS = 2031617;
    internal const int LMEM_FIXED = 0;
    internal const int LMEM_ZEROINIT = 64;
    internal const int LPTR = 64;
    internal const string KERNEL32 = "kernel32.dll";
    internal const string USER32 = "user32.dll";
    internal const string ADVAPI32 = "advapi32.dll";
    internal const string OLE32 = "ole32.dll";
    internal const string OLEAUT32 = "oleaut32.dll";
    internal const string SHFOLDER = "shfolder.dll";
    internal const string SHIM = "mscoree.dll";
    internal const string CRYPT32 = "crypt32.dll";
    internal const string SECUR32 = "secur32.dll";
    internal const string MSCORWKS = "mscorwks.dll";
    internal const string LSTRCPY = "lstrcpy";
    internal const string LSTRCPYN = "lstrcpyn";
    internal const string LSTRLEN = "lstrlen";
    internal const string LSTRLENA = "lstrlenA";
    internal const string LSTRLENW = "lstrlenW";
    internal const string MOVEMEMORY = "RtlMoveMemory";
    internal const int SEM_FAILCRITICALERRORS = 1;
    internal const int ERROR_SUCCESS = 0;
    internal const int ERROR_INVALID_FUNCTION = 1;
    internal const int ERROR_FILE_NOT_FOUND = 2;
    internal const int ERROR_PATH_NOT_FOUND = 3;
    internal const int ERROR_ACCESS_DENIED = 5;
    internal const int ERROR_INVALID_HANDLE = 6;
    internal const int ERROR_NOT_ENOUGH_MEMORY = 8;
    internal const int ERROR_INVALID_DATA = 13;
    internal const int ERROR_INVALID_DRIVE = 15;
    internal const int ERROR_NO_MORE_FILES = 18;
    internal const int ERROR_NOT_READY = 21;
    internal const int ERROR_BAD_LENGTH = 24;
    internal const int ERROR_SHARING_VIOLATION = 32;
    internal const int ERROR_NOT_SUPPORTED = 50;
    internal const int ERROR_FILE_EXISTS = 80;
    internal const int ERROR_INVALID_PARAMETER = 87;
    internal const int ERROR_CALL_NOT_IMPLEMENTED = 120;
    internal const int ERROR_INSUFFICIENT_BUFFER = 122;
    internal const int ERROR_INVALID_NAME = 123;
    internal const int ERROR_BAD_PATHNAME = 161;
    internal const int ERROR_ALREADY_EXISTS = 183;
    internal const int ERROR_ENVVAR_NOT_FOUND = 203;
    internal const int ERROR_FILENAME_EXCED_RANGE = 206;
    internal const int ERROR_NO_DATA = 232;
    internal const int ERROR_PIPE_NOT_CONNECTED = 233;
    internal const int ERROR_MORE_DATA = 234;
    internal const int ERROR_OPERATION_ABORTED = 995;
    internal const int ERROR_NO_TOKEN = 1008;
    internal const int ERROR_DLL_INIT_FAILED = 1114;
    internal const int ERROR_NON_ACCOUNT_SID = 1257;
    internal const int ERROR_NOT_ALL_ASSIGNED = 1300;
    internal const int ERROR_UNKNOWN_REVISION = 1305;
    internal const int ERROR_INVALID_OWNER = 1307;
    internal const int ERROR_INVALID_PRIMARY_GROUP = 1308;
    internal const int ERROR_NO_SUCH_PRIVILEGE = 1313;
    internal const int ERROR_PRIVILEGE_NOT_HELD = 1314;
    internal const int ERROR_NONE_MAPPED = 1332;
    internal const int ERROR_INVALID_ACL = 1336;
    internal const int ERROR_INVALID_SID = 1337;
    internal const int ERROR_INVALID_SECURITY_DESCR = 1338;
    internal const int ERROR_BAD_IMPERSONATION_LEVEL = 1346;
    internal const int ERROR_CANT_OPEN_ANONYMOUS = 1347;
    internal const int ERROR_NO_SECURITY_ON_OBJECT = 1350;
    internal const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 1789;
    internal const int ERROR_MIN_KTM_CODE = 6700;
    internal const int ERROR_INVALID_TRANSACTION = 6700;
    internal const int ERROR_MAX_KTM_CODE = 6799;
    internal const uint STATUS_SUCCESS = 0;
    internal const uint STATUS_SOME_NOT_MAPPED = 263;
    internal const uint STATUS_NO_MEMORY = 3221225495;
    internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 3221225524;
    internal const uint STATUS_NONE_MAPPED = 3221225587;
    internal const uint STATUS_INSUFFICIENT_RESOURCES = 3221225626;
    internal const uint STATUS_ACCESS_DENIED = 3221225506;
    internal const int INVALID_FILE_SIZE = -1;
    private const int FORMAT_MESSAGE_IGNORE_INSERTS = 512;
    private const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;
    private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;
    internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
    internal static readonly IntPtr NULL = IntPtr.Zero;
    internal static readonly uint PAGE_SIZE;

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern uint GetSecurityDescriptorLength(IntPtr byteArray);

    [DllImport("advapi32.dll", EntryPoint = "GetSecurityInfo", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern uint GetSecurityInfoByHandle(
      SafeHandle handle,
      uint objectType,
      uint securityInformation,
      out IntPtr sidOwner,
      out IntPtr sidGroup,
      out IntPtr dacl,
      out IntPtr sacl,
      out IntPtr securityDescriptor);

    [DllImport("advapi32.dll", EntryPoint = "SetSecurityInfo", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern uint SetSecurityInfoByHandle(
      SafeHandle handle,
      uint objectType,
      uint securityInformation,
      byte[] owner,
      byte[] group,
      byte[] dacl,
      byte[] sacl);

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern IntPtr LocalFree(IntPtr handle);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegConnectRegistry(
      string machineName,
      SafeRegistryHandle key,
      out SafeRegistryHandle result);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegCreateKeyEx(
      SafeRegistryHandle hKey,
      string lpSubKey,
      int Reserved,
      string lpClass,
      int dwOptions,
      int samDesigner,
      Win32Native.SECURITY_ATTRIBUTES lpSecurityAttributes,
      out SafeRegistryHandle hkResult,
      out int lpdwDisposition);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegDeleteKey(SafeRegistryHandle hKey, string lpSubKey);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegDeleteKeyTransacted(
      SafeRegistryHandle hKey,
      string lpSubKey,
      int samDesired,
      uint reserved,
      SafeTransactionHandle hTransaction,
      IntPtr pExtendedParameter);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegDeleteValue(SafeRegistryHandle hKey, string lpValueName);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegEnumKeyEx(
      SafeRegistryHandle hKey,
      int dwIndex,
      StringBuilder lpName,
      out int lpcbName,
      int[] lpReserved,
      StringBuilder lpClass,
      int[] lpcbClass,
      long[] lpftLastWriteTime);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegEnumValue(
      SafeRegistryHandle hKey,
      int dwIndex,
      StringBuilder lpValueName,
      ref int lpcbValueName,
      IntPtr lpReserved_MustBeZero,
      int[] lpType,
      byte[] lpData,
      int[] lpcbData);

    [DllImport("advapi32.dll")]
    internal static extern int RegFlushKey(SafeRegistryHandle hKey);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegOpenKeyEx(
      SafeRegistryHandle hKey,
      string lpSubKey,
      int ulOptions,
      int samDesired,
      out SafeRegistryHandle hkResult);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegOpenKeyTransacted(
      SafeRegistryHandle hKey,
      string lpSubKey,
      int ulOptions,
      int samDesired,
      out SafeRegistryHandle hkResult,
      SafeTransactionHandle hTransaction,
      IntPtr pExtendedParameter);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegQueryInfoKey(
      SafeRegistryHandle hKey,
      StringBuilder lpClass,
      int[] lpcbClass,
      IntPtr lpReserved_MustBeZero,
      ref int lpcSubKeys,
      int[] lpcbMaxSubKeyLen,
      int[] lpcbMaxClassLen,
      ref int lpcValues,
      int[] lpcbMaxValueNameLen,
      int[] lpcbMaxValueLen,
      int[] lpcbSecurityDescriptor,
      int[] lpftLastWriteTime);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegQueryValueEx(
      SafeRegistryHandle hKey,
      string lpValueName,
      int[] lpReserved,
      ref int lpType,
      [Out] byte[] lpData,
      ref int lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegQueryValueEx(
      SafeRegistryHandle hKey,
      string lpValueName,
      int[] lpReserved,
      ref int lpType,
      ref int lpData,
      ref int lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegQueryValueEx(
      SafeRegistryHandle hKey,
      string lpValueName,
      int[] lpReserved,
      ref int lpType,
      ref long lpData,
      ref int lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegQueryValueEx(
      SafeRegistryHandle hKey,
      string lpValueName,
      int[] lpReserved,
      ref int lpType,
      [Out] char[] lpData,
      ref int lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegQueryValueEx(
      SafeRegistryHandle hKey,
      string lpValueName,
      int[] lpReserved,
      ref int lpType,
      StringBuilder lpData,
      ref int lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegSetValueEx(
      SafeRegistryHandle hKey,
      string lpValueName,
      int Reserved,
      RegistryValueKind dwType,
      byte[] lpData,
      int cbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegSetValueEx(
      SafeRegistryHandle hKey,
      string lpValueName,
      int Reserved,
      RegistryValueKind dwType,
      ref int lpData,
      int cbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegSetValueEx(
      SafeRegistryHandle hKey,
      string lpValueName,
      int Reserved,
      RegistryValueKind dwType,
      ref long lpData,
      int cbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegSetValueEx(
      SafeRegistryHandle hKey,
      string lpValueName,
      int Reserved,
      RegistryValueKind dwType,
      string lpData,
      int cbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    internal static extern int RegCreateKeyTransacted(
      SafeRegistryHandle hKey,
      string lpSubKey,
      int Reserved,
      string lpClass,
      int dwOptions,
      int samDesigner,
      Win32Native.SECURITY_ATTRIBUTES lpSecurityAttributes,
      out SafeRegistryHandle hkResult,
      out int lpdwDisposition,
      SafeTransactionHandle hTransaction,
      IntPtr pExtendedParameter);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern int FormatMessage(
      int dwFlags,
      IntPtr lpSource,
      int dwMessageId,
      int dwLanguageId,
      StringBuilder lpBuffer,
      int nSize,
      IntPtr va_list_arguments);

    internal static string GetMessage(int errorCode)
    {
      StringBuilder lpBuffer = new StringBuilder(512);
      if (Win32Native.FormatMessage(12800, Win32Native.NULL, errorCode, 0, lpBuffer, lpBuffer.Capacity, Win32Native.NULL) != 0)
        return lpBuffer.ToString();
      return string.Format((IFormatProvider) CultureInfo.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "UnknownError_Num"), (object) errorCode.ToString((IFormatProvider) CultureInfo.InvariantCulture));
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    internal static extern int lstrlen(sbyte[] ptr);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    internal static extern int lstrlen(IntPtr ptr);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    internal static extern int lstrlenA(IntPtr ptr);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern int lstrlenW(IntPtr ptr);

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll")]
    internal static extern UIntPtr VirtualQuery(
      UIntPtr lpAddress,
      ref Win32Native.MEMORY_BASIC_INFORMATION lpBuffer,
      UIntPtr dwLength);

    [DllImport("kernel32.dll")]
    internal static extern void GetSystemInfo(out Win32Native.SYSTEM_INFO lpSystemInfo);

    static Win32Native()
    {
      Win32Native.SYSTEM_INFO lpSystemInfo = new Win32Native.SYSTEM_INFO();
      Win32Native.GetSystemInfo(out lpSystemInfo);
      Win32Native.PAGE_SIZE = (uint) lpSystemInfo.dwPageSize;
    }

    internal enum SECURITY_IMPERSONATION_LEVEL : short
    {
      Anonymous = 0,
      Identification = 1,
      Impersonation = 2,
      Delegation = 4,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class OSVERSIONINFO
    {
      internal int OSVersionInfoSize;
      internal int MajorVersion;
      internal int MinorVersion;
      internal int BuildNumber;
      internal int PlatformId;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      internal string CSDVersion;

      internal OSVERSIONINFO() => this.OSVersionInfoSize = Marshal.SizeOf((object) this);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class OSVERSIONINFOEX
    {
      internal int OSVersionInfoSize;
      internal int MajorVersion;
      internal int MinorVersion;
      internal int BuildNumber;
      internal int PlatformId;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      internal string CSDVersion;
      internal ushort ServicePackMajor;
      internal ushort ServicePackMinor;
      internal short SuiteMask;
      internal byte ProductType;
      internal byte Reserved;

      public OSVERSIONINFOEX() => this.OSVersionInfoSize = Marshal.SizeOf((object) this);
    }

    internal struct SYSTEM_INFO
    {
      internal int dwOemId;
      internal int dwPageSize;
      internal IntPtr lpMinimumApplicationAddress;
      internal IntPtr lpMaximumApplicationAddress;
      internal IntPtr dwActiveProcessorMask;
      internal int dwNumberOfProcessors;
      internal int dwProcessorType;
      internal int dwAllocationGranularity;
      internal short wProcessorLevel;
      internal short wProcessorRevision;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class SECURITY_ATTRIBUTES
    {
      internal int nLength;
      internal unsafe byte* pSecurityDescriptor = (byte*) null;
      internal int bInheritHandle;
    }

    [Serializable]
    internal struct WIN32_FILE_ATTRIBUTE_DATA
    {
      internal int fileAttributes;
      internal uint ftCreationTimeLow;
      internal uint ftCreationTimeHigh;
      internal uint ftLastAccessTimeLow;
      internal uint ftLastAccessTimeHigh;
      internal uint ftLastWriteTimeLow;
      internal uint ftLastWriteTimeHigh;
      internal int fileSizeHigh;
      internal int fileSizeLow;
    }

    internal struct FILE_TIME
    {
      internal uint ftTimeLow;
      internal uint ftTimeHigh;

      public FILE_TIME(long fileTime)
      {
        this.ftTimeLow = (uint) fileTime;
        this.ftTimeHigh = (uint) (fileTime >> 32);
      }

      public long ToTicks() => ((long) this.ftTimeHigh << 32) + (long) this.ftTimeLow;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct KERB_S4U_LOGON
    {
      internal uint MessageType;
      internal uint Flags;
      internal Win32Native.UNICODE_INTPTR_STRING ClientUpn;
      internal Win32Native.UNICODE_INTPTR_STRING ClientRealm;
    }

    internal struct LSA_OBJECT_ATTRIBUTES
    {
      internal int Length;
      internal IntPtr RootDirectory;
      internal IntPtr ObjectName;
      internal int Attributes;
      internal IntPtr SecurityDescriptor;
      internal IntPtr SecurityQualityOfService;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct UNICODE_STRING
    {
      internal ushort Length;
      internal ushort MaximumLength;
      [MarshalAs(UnmanagedType.LPWStr)]
      internal string Buffer;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct UNICODE_INTPTR_STRING
    {
      internal ushort Length;
      internal ushort MaxLength;
      internal IntPtr Buffer;

      internal UNICODE_INTPTR_STRING(int length, int maximumLength, IntPtr buffer)
      {
        this.Length = (ushort) length;
        this.MaxLength = (ushort) maximumLength;
        this.Buffer = buffer;
      }
    }

    internal struct LSA_TRANSLATED_NAME
    {
      internal int Use;
      internal Win32Native.UNICODE_INTPTR_STRING Name;
      internal int DomainIndex;
    }

    internal struct LSA_TRANSLATED_SID
    {
      internal int Use;
      internal uint Rid;
      internal int DomainIndex;
    }

    internal struct LSA_TRANSLATED_SID2
    {
      internal int Use;
      internal IntPtr Sid;
      internal int DomainIndex;
      private uint Flags;
    }

    internal struct LSA_TRUST_INFORMATION
    {
      internal Win32Native.UNICODE_INTPTR_STRING Name;
      internal IntPtr Sid;
    }

    internal struct LSA_REFERENCED_DOMAIN_LIST
    {
      internal int Entries;
      internal IntPtr Domains;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct LUID
    {
      internal uint LowPart;
      internal uint HighPart;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct LUID_AND_ATTRIBUTES
    {
      internal Win32Native.LUID Luid;
      internal uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct QUOTA_LIMITS
    {
      internal IntPtr PagedPoolLimit;
      internal IntPtr NonPagedPoolLimit;
      internal IntPtr MinimumWorkingSetSize;
      internal IntPtr MaximumWorkingSetSize;
      internal IntPtr PagefileLimit;
      internal IntPtr TimeLimit;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct SECURITY_LOGON_SESSION_DATA
    {
      internal uint Size;
      internal Win32Native.LUID LogonId;
      internal Win32Native.UNICODE_INTPTR_STRING UserName;
      internal Win32Native.UNICODE_INTPTR_STRING LogonDomain;
      internal Win32Native.UNICODE_INTPTR_STRING AuthenticationPackage;
      internal uint LogonType;
      internal uint Session;
      internal IntPtr Sid;
      internal long LogonTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct SID_AND_ATTRIBUTES
    {
      internal IntPtr Sid;
      internal uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct TOKEN_GROUPS
    {
      internal uint GroupCount;
      internal Win32Native.SID_AND_ATTRIBUTES Groups;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct TOKEN_PRIVILEGE
    {
      internal uint PrivilegeCount;
      internal Win32Native.LUID_AND_ATTRIBUTES Privilege;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct TOKEN_SOURCE
    {
      private const int TOKEN_SOURCE_LENGTH = 8;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      internal char[] Name;
      internal Win32Native.LUID SourceIdentifier;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct TOKEN_STATISTICS
    {
      internal Win32Native.LUID TokenId;
      internal Win32Native.LUID AuthenticationId;
      internal long ExpirationTime;
      internal uint TokenType;
      internal uint ImpersonationLevel;
      internal uint DynamicCharged;
      internal uint DynamicAvailable;
      internal uint GroupCount;
      internal uint PrivilegeCount;
      internal Win32Native.LUID ModifiedId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct TOKEN_USER
    {
      internal Win32Native.SID_AND_ATTRIBUTES User;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class MEMORYSTATUSEX
    {
      internal int length;
      internal int memoryLoad;
      internal ulong totalPhys;
      internal ulong availPhys;
      internal ulong totalPageFile;
      internal ulong availPageFile;
      internal ulong totalVirtual;
      internal ulong availVirtual;
      internal ulong availExtendedVirtual;

      internal MEMORYSTATUSEX() => this.length = Marshal.SizeOf((object) this);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class MEMORYSTATUS
    {
      internal int length;
      internal int memoryLoad;
      internal uint totalPhys;
      internal uint availPhys;
      internal uint totalPageFile;
      internal uint availPageFile;
      internal uint totalVirtual;
      internal uint availVirtual;

      internal MEMORYSTATUS() => this.length = Marshal.SizeOf((object) this);
    }

    internal struct MEMORY_BASIC_INFORMATION
    {
      internal UIntPtr BaseAddress;
      internal UIntPtr AllocationBase;
      internal uint AllocationProtect;
      internal UIntPtr RegionSize;
      internal uint State;
      internal uint Protect;
      internal uint Type;
    }
  }
}
