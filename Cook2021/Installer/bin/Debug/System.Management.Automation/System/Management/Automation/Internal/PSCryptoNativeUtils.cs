// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSCryptoNativeUtils
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.InteropServices;

namespace System.Management.Automation.Internal
{
  internal class PSCryptoNativeUtils
  {
    public const uint CRYPT_VERIFYCONTEXT = 4026531840;
    public const uint CRYPT_EXPORTABLE = 1;
    public const int CRYPT_CREATE_SALT = 4;
    public const int PROV_RSA_FULL = 1;
    public const int PROV_RSA_AES = 24;
    public const int AT_KEYEXCHANGE = 1;
    public const int CALG_RSA_KEYX = 41984;
    public const int ALG_CLASS_KEY_EXCHANGE = 40960;
    public const int ALG_TYPE_RSA = 1024;
    public const int ALG_SID_RSA_ANY = 0;
    public const int PUBLICKEYBLOB = 6;
    public const int SIMPLEBLOB = 1;
    public const int CALG_AES_256 = 26128;
    public const int ALG_CLASS_DATA_ENCRYPT = 24576;
    public const int ALG_TYPE_BLOCK = 1536;
    public const int ALG_SID_AES_256 = 16;
    public const int CALG_AES_128 = 26126;
    public const int ALG_SID_AES_128 = 14;

    [DllImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptGenKey(
      PSSafeCryptProvHandle hProv,
      uint Algid,
      uint dwFlags,
      ref PSSafeCryptKey phKey);

    [DllImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptDestroyKey(IntPtr hKey);

    [DllImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptAcquireContext(
      ref PSSafeCryptProvHandle phProv,
      [MarshalAs(UnmanagedType.LPWStr), In] string szContainer,
      [MarshalAs(UnmanagedType.LPWStr), In] string szProvider,
      uint dwProvType,
      uint dwFlags);

    [DllImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptReleaseContext(IntPtr hProv, uint dwFlags);

    [DllImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptEncrypt(
      PSSafeCryptKey hKey,
      IntPtr hHash,
      [MarshalAs(UnmanagedType.Bool)] bool Final,
      uint dwFlags,
      byte[] pbData,
      ref int pdwDataLen,
      int dwBufLen);

    [DllImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptDecrypt(
      PSSafeCryptKey hKey,
      IntPtr hHash,
      [MarshalAs(UnmanagedType.Bool)] bool Final,
      uint dwFlags,
      byte[] pbData,
      ref int pdwDataLen);

    [DllImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptExportKey(
      PSSafeCryptKey hKey,
      PSSafeCryptKey hExpKey,
      uint dwBlobType,
      uint dwFlags,
      byte[] pbData,
      ref uint pdwDataLen);

    [DllImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptImportKey(
      PSSafeCryptProvHandle hProv,
      byte[] pbData,
      int dwDataLen,
      PSSafeCryptKey hPubKey,
      uint dwFlags,
      ref PSSafeCryptKey phKey);

    [DllImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptDuplicateKey(
      PSSafeCryptKey hKey,
      ref uint pdwReserved,
      uint dwFlags,
      ref PSSafeCryptKey phKey);

    [DllImport("kernel32.dll")]
    public static extern uint GetLastError();
  }
}
