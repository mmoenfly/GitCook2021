// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Security.NativeMethods
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace System.Management.Automation.Security
{
  internal static class NativeMethods
  {
    [DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool CertEnumSystemStore(
      NativeMethods.CertStoreFlags Flags,
      IntPtr notUsed1,
      IntPtr notUsed2,
      NativeMethods.CertEnumSystemStoreCallBackProto fn);

    [DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool CertGetEnhancedKeyUsage(
      IntPtr pCertContext,
      uint dwFlags,
      IntPtr pUsage,
      out int pcbUsage);

    [DllImport("cryptUI.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool CryptUIWizDigitalSign(
      uint dwFlags,
      IntPtr hwndParentNotUsed,
      IntPtr pwszWizardTitleNotUsed,
      IntPtr pDigitalSignInfo,
      IntPtr ppSignContextNotUsed);

    [ArchitectureSensitive]
    internal static NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO InitSignInfoExtendedStruct(
      string description,
      string moreInfoUrl,
      string hashAlgorithm)
    {
      NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO signExtendedInfo = new NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO();
      signExtendedInfo.dwSize = (uint) Marshal.SizeOf((object) signExtendedInfo);
      signExtendedInfo.dwAttrFlagsNotUsed = 0U;
      signExtendedInfo.pwszDescription = description;
      signExtendedInfo.pwszMoreInfoLocation = moreInfoUrl;
      signExtendedInfo.pszHashAlg = (string) null;
      signExtendedInfo.pwszSigningCertDisplayStringNotUsed = IntPtr.Zero;
      signExtendedInfo.hAdditionalCertStoreNotUsed = IntPtr.Zero;
      signExtendedInfo.psAuthenticatedNotUsed = IntPtr.Zero;
      signExtendedInfo.psUnauthenticatedNotUsed = IntPtr.Zero;
      if (hashAlgorithm != null)
        signExtendedInfo.pszHashAlg = hashAlgorithm;
      return signExtendedInfo;
    }

    [DllImport("crypt32.dll")]
    internal static extern IntPtr CryptFindOIDInfo(
      uint dwKeyType,
      [MarshalAs(UnmanagedType.LPWStr)] string pvKey,
      uint dwGroupId);

    [ArchitectureSensitive]
    internal static uint GetCertChoiceFromSigningOption(SigningOption option)
    {
      uint num;
      switch (option)
      {
        case SigningOption.AddOnlyCertificate:
          num = 0U;
          break;
        case SigningOption.AddFullCertificateChain:
          num = 1U;
          break;
        case SigningOption.AddFullCertificateChainExceptRoot:
          num = 2U;
          break;
        default:
          num = 2U;
          break;
      }
      return num;
    }

    [ArchitectureSensitive]
    internal static NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_INFO InitSignInfoStruct(
      string fileName,
      X509Certificate2 signingCert,
      string timeStampServerUrl,
      string hashAlgorithm,
      SigningOption option)
    {
      NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_INFO wizDigitalSignInfo = new NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_INFO();
      wizDigitalSignInfo.dwSize = (uint) Marshal.SizeOf((object) wizDigitalSignInfo);
      wizDigitalSignInfo.dwSubjectChoice = 1U;
      wizDigitalSignInfo.pwszFileName = fileName;
      wizDigitalSignInfo.dwSigningCertChoice = 1U;
      wizDigitalSignInfo.pSigningCertContext = signingCert.Handle;
      wizDigitalSignInfo.pwszTimestampURL = timeStampServerUrl;
      wizDigitalSignInfo.dwAdditionalCertChoice = NativeMethods.GetCertChoiceFromSigningOption(option);
      NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO signExtendedInfo = NativeMethods.InitSignInfoExtendedStruct("", "", hashAlgorithm);
      IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf((object) signExtendedInfo));
      Marshal.StructureToPtr((object) signExtendedInfo, ptr, false);
      wizDigitalSignInfo.pSignExtInfo = ptr;
      return wizDigitalSignInfo;
    }

    [DllImport("wintrust.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern uint WinVerifyTrust(
      IntPtr hWndNotUsed,
      IntPtr pgActionID,
      IntPtr pWinTrustData);

    [ArchitectureSensitive]
    internal static NativeMethods.WINTRUST_FILE_INFO InitWintrustFileInfoStruct(
      string fileName)
    {
      NativeMethods.WINTRUST_FILE_INFO wintrustFileInfo = new NativeMethods.WINTRUST_FILE_INFO();
      wintrustFileInfo.cbStruct = (uint) Marshal.SizeOf((object) wintrustFileInfo);
      wintrustFileInfo.pcwszFilePath = fileName;
      wintrustFileInfo.hFileNotUsed = IntPtr.Zero;
      wintrustFileInfo.pgKnownSubjectNotUsed = IntPtr.Zero;
      return wintrustFileInfo;
    }

    [ArchitectureSensitive]
    internal static NativeMethods.WINTRUST_BLOB_INFO InitWintrustBlobInfoStruct(
      string fileName,
      string content)
    {
      NativeMethods.WINTRUST_BLOB_INFO wintrustBlobInfo = new NativeMethods.WINTRUST_BLOB_INFO();
      byte[] bytes = Encoding.Unicode.GetBytes(content);
      wintrustBlobInfo.gSubject.Data1 = 1614531615U;
      wintrustBlobInfo.gSubject.Data2 = (ushort) 19289;
      wintrustBlobInfo.gSubject.Data3 = (ushort) 19976;
      wintrustBlobInfo.gSubject.Data4 = new byte[8]
      {
        (byte) 183,
        (byte) 36,
        (byte) 210,
        (byte) 198,
        (byte) 41,
        (byte) 126,
        (byte) 243,
        (byte) 81
      };
      wintrustBlobInfo.cbStruct = (uint) Marshal.SizeOf((object) wintrustBlobInfo);
      wintrustBlobInfo.pcwszDisplayName = fileName;
      wintrustBlobInfo.cbMemObject = (uint) bytes.Length;
      wintrustBlobInfo.pbMemObject = Marshal.AllocCoTaskMem(bytes.Length);
      Marshal.Copy(bytes, 0, wintrustBlobInfo.pbMemObject, bytes.Length);
      return wintrustBlobInfo;
    }

    [ArchitectureSensitive]
    internal static NativeMethods.WINTRUST_DATA InitWintrustDataStructFromFile(
      NativeMethods.WINTRUST_FILE_INFO wfi)
    {
      NativeMethods.WINTRUST_DATA wintrustData = new NativeMethods.WINTRUST_DATA();
      wintrustData.cbStruct = (uint) Marshal.SizeOf((object) wintrustData);
      wintrustData.pPolicyCallbackData = IntPtr.Zero;
      wintrustData.pSIPClientData = IntPtr.Zero;
      wintrustData.dwUIChoice = 2U;
      wintrustData.fdwRevocationChecks = 0U;
      wintrustData.dwUnionChoice = 1U;
      IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf((object) wfi));
      Marshal.StructureToPtr((object) wfi, ptr, false);
      wintrustData.Choice.pFile = ptr;
      wintrustData.dwStateAction = 1U;
      wintrustData.hWVTStateData = IntPtr.Zero;
      wintrustData.pwszURLReference = (string) null;
      wintrustData.dwProvFlags = 0U;
      return wintrustData;
    }

    [ArchitectureSensitive]
    internal static NativeMethods.WINTRUST_DATA InitWintrustDataStructFromBlob(
      NativeMethods.WINTRUST_BLOB_INFO wbi)
    {
      NativeMethods.WINTRUST_DATA wintrustData = new NativeMethods.WINTRUST_DATA();
      wintrustData.cbStruct = (uint) Marshal.SizeOf((object) wbi);
      wintrustData.pPolicyCallbackData = IntPtr.Zero;
      wintrustData.pSIPClientData = IntPtr.Zero;
      wintrustData.dwUIChoice = 2U;
      wintrustData.fdwRevocationChecks = 0U;
      wintrustData.dwUnionChoice = 3U;
      IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf((object) wbi));
      Marshal.StructureToPtr((object) wbi, ptr, false);
      wintrustData.Choice.pBlob = ptr;
      wintrustData.dwStateAction = 1U;
      wintrustData.hWVTStateData = IntPtr.Zero;
      wintrustData.pwszURLReference = (string) null;
      wintrustData.dwProvFlags = 0U;
      return wintrustData;
    }

    [ArchitectureSensitive]
    internal static uint DestroyWintrustDataStruct(NativeMethods.WINTRUST_DATA wtd)
    {
      uint num1 = 2147500037;
      IntPtr num2 = IntPtr.Zero;
      IntPtr num3 = IntPtr.Zero;
      Guid guid = new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");
      try
      {
        num2 = Marshal.AllocCoTaskMem(Marshal.SizeOf((object) guid));
        Marshal.StructureToPtr((object) guid, num2, false);
        wtd.dwStateAction = 2U;
        num3 = Marshal.AllocCoTaskMem(Marshal.SizeOf((object) wtd));
        Marshal.StructureToPtr((object) wtd, num3, false);
        num1 = NativeMethods.WinVerifyTrust(IntPtr.Zero, num2, num3);
        wtd = (NativeMethods.WINTRUST_DATA) Marshal.PtrToStructure(num3, typeof (NativeMethods.WINTRUST_DATA));
      }
      finally
      {
        Marshal.DestroyStructure(num3, typeof (NativeMethods.WINTRUST_DATA));
        Marshal.FreeCoTaskMem(num3);
        Marshal.DestroyStructure(num2, typeof (Guid));
        Marshal.FreeCoTaskMem(num2);
      }
      if (wtd.dwUnionChoice == 3U)
      {
        Marshal.FreeCoTaskMem(((NativeMethods.WINTRUST_BLOB_INFO) Marshal.PtrToStructure(wtd.Choice.pBlob, typeof (NativeMethods.WINTRUST_BLOB_INFO))).pbMemObject);
        Marshal.DestroyStructure(wtd.Choice.pBlob, typeof (NativeMethods.WINTRUST_BLOB_INFO));
        Marshal.FreeCoTaskMem(wtd.Choice.pBlob);
      }
      else
      {
        Marshal.DestroyStructure(wtd.Choice.pFile, typeof (NativeMethods.WINTRUST_FILE_INFO));
        Marshal.FreeCoTaskMem(wtd.Choice.pFile);
      }
      return num1;
    }

    [DllImport("wintrust.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr WTHelperProvDataFromStateData(IntPtr hStateData);

    [DllImport("wintrust.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr WTHelperGetProvSignerFromChain(
      IntPtr pProvData,
      uint idxSigner,
      uint fCounterSigner,
      uint idxCounterSigner);

    [DllImport("wintrust.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr WTHelperGetProvCertFromChain(IntPtr pSgnr, uint idxCert);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SaferIdentifyLevel(
      uint dwNumProperties,
      [In] ref SAFER_CODE_PROPERTIES pCodeProperties,
      out IntPtr pLevelHandle,
      [MarshalAs(UnmanagedType.LPWStr), In] string bucket);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SaferComputeTokenFromLevel(
      [In] IntPtr LevelHandle,
      [In] IntPtr InAccessToken,
      ref IntPtr OutAccessToken,
      uint dwFlags,
      IntPtr lpReserved);

    [DllImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SaferCloseLevel([In] IntPtr hLevelHandle);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CloseHandle([In] IntPtr hObject);

    internal delegate bool CertEnumSystemStoreCallBackProto(
      [MarshalAs(UnmanagedType.LPWStr)] string storeName,
      uint dwFlagsNotUsed,
      IntPtr notUsed1,
      IntPtr notUsed2,
      IntPtr notUsed3);

    [System.Flags]
    internal enum CertStoreFlags
    {
      CERT_SYSTEM_STORE_CURRENT_USER = 65536, // 0x00010000
      CERT_SYSTEM_STORE_LOCAL_MACHINE = 131072, // 0x00020000
    }

    [System.Flags]
    internal enum CryptUIFlags
    {
      CRYPTUI_WIZ_NO_UI = 1,
    }

    internal struct CRYPTUI_WIZ_DIGITAL_SIGN_INFO
    {
      internal uint dwSize;
      internal uint dwSubjectChoice;
      [MarshalAs(UnmanagedType.LPWStr)]
      internal string pwszFileName;
      internal uint dwSigningCertChoice;
      internal IntPtr pSigningCertContext;
      [MarshalAs(UnmanagedType.LPWStr)]
      internal string pwszTimestampURL;
      internal uint dwAdditionalCertChoice;
      internal IntPtr pSignExtInfo;
    }

    [System.Flags]
    internal enum SignInfoSubjectChoice
    {
      CRYPTUI_WIZ_DIGITAL_SIGN_SUBJECT_FILE = 1,
    }

    [System.Flags]
    internal enum SignInfoCertChoice
    {
      CRYPTUI_WIZ_DIGITAL_SIGN_CERT = 1,
    }

    [System.Flags]
    internal enum SignInfoAdditionalCertChoice
    {
      CRYPTUI_WIZ_DIGITAL_SIGN_ADD_CHAIN = 1,
      CRYPTUI_WIZ_DIGITAL_SIGN_ADD_CHAIN_NO_ROOT = 2,
    }

    internal struct CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO
    {
      internal uint dwSize;
      internal uint dwAttrFlagsNotUsed;
      [MarshalAs(UnmanagedType.LPWStr)]
      internal string pwszDescription;
      [MarshalAs(UnmanagedType.LPWStr)]
      internal string pwszMoreInfoLocation;
      [MarshalAs(UnmanagedType.LPStr)]
      internal string pszHashAlg;
      internal IntPtr pwszSigningCertDisplayStringNotUsed;
      internal IntPtr hAdditionalCertStoreNotUsed;
      internal IntPtr psAuthenticatedNotUsed;
      internal IntPtr psUnauthenticatedNotUsed;
    }

    internal struct CRYPT_OID_INFO
    {
      public uint cbSize;
      [MarshalAs(UnmanagedType.LPStr)]
      public string pszOID;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string pwszName;
      public uint dwGroupId;
      public NativeMethods.Anonymous_a3ae7823_8a1d_432c_bc07_a72b6fc6c7d8 Union1;
      public NativeMethods.CRYPT_ATTR_BLOB ExtraInfo;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct Anonymous_a3ae7823_8a1d_432c_bc07_a72b6fc6c7d8
    {
      [FieldOffset(0)]
      public uint dwValue;
      [FieldOffset(0)]
      public uint Algid;
      [FieldOffset(0)]
      public uint dwLength;
    }

    internal struct CRYPT_ATTR_BLOB
    {
      public uint cbData;
      public IntPtr pbData;
    }

    internal struct WINTRUST_FILE_INFO
    {
      internal uint cbStruct;
      [MarshalAs(UnmanagedType.LPWStr)]
      internal string pcwszFilePath;
      internal IntPtr hFileNotUsed;
      internal IntPtr pgKnownSubjectNotUsed;
    }

    internal struct WINTRUST_BLOB_INFO
    {
      internal uint cbStruct;
      internal NativeMethods.GUID gSubject;
      [MarshalAs(UnmanagedType.LPWStr)]
      internal string pcwszDisplayName;
      internal uint cbMemObject;
      internal IntPtr pbMemObject;
      internal uint cbMemSignedMsg;
      internal IntPtr pbMemSignedMsg;
    }

    internal struct GUID
    {
      internal uint Data1;
      internal ushort Data2;
      internal ushort Data3;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      internal byte[] Data4;
    }

    [System.Flags]
    internal enum WintrustUIChoice
    {
      WTD_UI_ALL = 1,
      WTD_UI_NONE = 2,
      WTD_UI_NOBAD = WTD_UI_NONE | WTD_UI_ALL, // 0x00000003
      WTD_UI_NOGOOD = 4,
    }

    [System.Flags]
    internal enum WintrustUnionChoice
    {
      WTD_CHOICE_FILE = 1,
      WTD_CHOICE_BLOB = 3,
    }

    [System.Flags]
    internal enum WintrustProviderFlags
    {
      WTD_PROV_FLAGS_MASK = 65535, // 0x0000FFFF
      WTD_USE_IE4_TRUST_FLAG = 1,
      WTD_NO_IE4_CHAIN_FLAG = 2,
      WTD_NO_POLICY_USAGE_FLAG = 4,
      WTD_REVOCATION_CHECK_NONE = 16, // 0x00000010
      WTD_REVOCATION_CHECK_END_CERT = 32, // 0x00000020
      WTD_REVOCATION_CHECK_CHAIN = 64, // 0x00000040
      WTD_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 128, // 0x00000080
      WTD_SAFER_FLAG = 256, // 0x00000100
      WTD_HASH_ONLY_FLAG = 512, // 0x00000200
      WTD_USE_DEFAULT_OSVER_CHECK = 1024, // 0x00000400
      WTD_LIFETIME_SIGNING_FLAG = 2048, // 0x00000800
      WTD_CACHE_ONLY_URL_RETRIEVAL = 4096, // 0x00001000
    }

    [System.Flags]
    internal enum WintrustAction
    {
      WTD_STATEACTION_IGNORE = 0,
      WTD_STATEACTION_VERIFY = 1,
      WTD_STATEACTION_CLOSE = 2,
      WTD_STATEACTION_AUTO_CACHE = WTD_STATEACTION_CLOSE | WTD_STATEACTION_VERIFY, // 0x00000003
      WTD_STATEACTION_AUTO_CACHE_FLUSH = 4,
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct WinTrust_Choice
    {
      [FieldOffset(0)]
      internal IntPtr pFile;
      [FieldOffset(0)]
      internal IntPtr pCatalog;
      [FieldOffset(0)]
      internal IntPtr pBlob;
      [FieldOffset(0)]
      internal IntPtr pSgnr;
      [FieldOffset(0)]
      internal IntPtr pCert;
    }

    internal struct WINTRUST_DATA
    {
      internal uint cbStruct;
      internal IntPtr pPolicyCallbackData;
      internal IntPtr pSIPClientData;
      internal uint dwUIChoice;
      internal uint fdwRevocationChecks;
      internal uint dwUnionChoice;
      internal NativeMethods.WinTrust_Choice Choice;
      internal uint dwStateAction;
      internal IntPtr hWVTStateData;
      [MarshalAs(UnmanagedType.LPWStr)]
      internal string pwszURLReference;
      internal uint dwProvFlags;
      internal uint dwUIContext;
    }

    internal struct CRYPT_PROVIDER_CERT
    {
      private uint cbStruct;
      internal IntPtr pCert;
      private uint fCommercial;
      private uint fTrustedRoot;
      private uint fSelfSigned;
      private uint fTestCert;
      private uint dwRevokedReason;
      private uint dwConfidence;
      private uint dwError;
      private IntPtr pTrustListContext;
      private uint fTrustListSignerCert;
      private IntPtr pCtlContext;
      private uint dwCtlError;
      private uint fIsCyclic;
      private IntPtr pChainElement;
    }

    internal struct CRYPT_PROVIDER_SGNR
    {
      private uint cbStruct;
      private System.Runtime.InteropServices.ComTypes.FILETIME sftVerifyAsOf;
      private uint csCertChain;
      private IntPtr pasCertChain;
      private uint dwSignerType;
      private IntPtr psSigner;
      private uint dwError;
      internal uint csCounterSigners;
      internal IntPtr pasCounterSigners;
      private IntPtr pChainContext;
    }

    internal struct CERT_ENHKEY_USAGE
    {
      internal uint cUsageIdentifier;
      internal IntPtr rgpszUsageIdentifier;
    }
  }
}
