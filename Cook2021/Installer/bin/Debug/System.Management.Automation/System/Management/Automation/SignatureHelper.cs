// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SignatureHelper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace System.Management.Automation
{
  internal static class SignatureHelper
  {
    [TraceSource("SignatureHelper", "tracer for SignatureHelper")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer(nameof (SignatureHelper), "tracer for SignatureHelper");

    [ArchitectureSensitive]
    internal static Signature SignFile(
      SigningOption option,
      string fileName,
      X509Certificate2 certificate,
      string timeStampServerUrl,
      string hashAlgorithm)
    {
      using (SignatureHelper.tracer.TraceMethod("file: {0}, cert: {1}", (object) SignatureHelper.GetStringValue(fileName), (object) SignatureHelper.GetCertName(certificate)))
      {
        Signature signature = (Signature) null;
        IntPtr num = IntPtr.Zero;
        uint error = 0;
        string hashAlgorithm1 = (string) null;
        Utils.CheckArgForNullOrEmpty(SignatureHelper.tracer, fileName, nameof (fileName));
        Utils.CheckArgForNull(SignatureHelper.tracer, (object) certificate, nameof (certificate));
        if (!string.IsNullOrEmpty(timeStampServerUrl) && (timeStampServerUrl.Length <= 7 || timeStampServerUrl.IndexOf("http://", StringComparison.OrdinalIgnoreCase) != 0))
          throw SignatureHelper.tracer.NewArgumentException(nameof (certificate), "Authenticode", "TimeStampUrlRequired");
        if (!string.IsNullOrEmpty(hashAlgorithm))
        {
          IntPtr oidInfo = System.Management.Automation.Security.NativeMethods.CryptFindOIDInfo(2U, hashAlgorithm, 0U);
          hashAlgorithm1 = !(oidInfo == IntPtr.Zero) ? ((System.Management.Automation.Security.NativeMethods.CRYPT_OID_INFO) Marshal.PtrToStructure(oidInfo, typeof (System.Management.Automation.Security.NativeMethods.CRYPT_OID_INFO))).pszOID : throw SignatureHelper.tracer.NewArgumentException(nameof (certificate), "Authenticode", "InvalidHashAlgorithm");
        }
        if (!SecuritySupport.CertIsGoodForSigning(certificate))
          throw SignatureHelper.tracer.NewArgumentException(nameof (certificate), "Authenticode", "CertNotGoodForSigning");
        SecuritySupport.CheckIfFileExists(fileName);
        try
        {
          string timeStampServerUrl1 = "";
          if (!string.IsNullOrEmpty(timeStampServerUrl))
            timeStampServerUrl1 = timeStampServerUrl;
          System.Management.Automation.Security.NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_INFO wizDigitalSignInfo = System.Management.Automation.Security.NativeMethods.InitSignInfoStruct(fileName, certificate, timeStampServerUrl1, hashAlgorithm1, option);
          num = Marshal.AllocCoTaskMem(Marshal.SizeOf((object) wizDigitalSignInfo));
          Marshal.StructureToPtr((object) wizDigitalSignInfo, num, false);
          bool flag = System.Management.Automation.Security.NativeMethods.CryptUIWizDigitalSign(1U, IntPtr.Zero, IntPtr.Zero, num, IntPtr.Zero);
          Marshal.DestroyStructure(wizDigitalSignInfo.pSignExtInfo, typeof (System.Management.Automation.Security.NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_EXTENDED_INFO));
          Marshal.FreeCoTaskMem(wizDigitalSignInfo.pSignExtInfo);
          if (!flag)
          {
            error = SignatureHelper.GetLastWin32Error();
            switch (error)
            {
              case 2147500037:
              case 2147942401:
                flag = true;
                break;
              case 2148073480:
                throw SignatureHelper.tracer.NewArgumentException(nameof (certificate), "Authenticode", "InvalidHashAlgorithm");
              default:
                SignatureHelper.tracer.TraceError("CryptUIWizDigitalSign: failed: {0:x}", (object) error);
                break;
            }
          }
          signature = !flag ? new Signature(fileName, error) : (string.IsNullOrEmpty(timeStampServerUrl) ? new Signature(fileName, certificate) : SignatureHelper.GetSignature(fileName, (string) null));
        }
        finally
        {
          Marshal.DestroyStructure(num, typeof (System.Management.Automation.Security.NativeMethods.CRYPTUI_WIZ_DIGITAL_SIGN_INFO));
          Marshal.FreeCoTaskMem(num);
        }
        return signature;
      }
    }

    [ArchitectureSensitive]
    internal static Signature GetSignature(string fileName, string fileContent)
    {
      using (SignatureHelper.tracer.TraceMethod("file: {0}", (object) SignatureHelper.GetStringValue(fileName)))
      {
        Utils.CheckArgForNullOrEmpty(SignatureHelper.tracer, fileName, nameof (fileName));
        SecuritySupport.CheckIfFileExists(fileName);
        Signature signature;
        try
        {
          System.Management.Automation.Security.NativeMethods.WINTRUST_DATA wtData;
          uint winTrustData = SignatureHelper.GetWinTrustData(fileName, fileContent, out wtData);
          if (winTrustData != 0U)
            SignatureHelper.tracer.WriteLine("GetWinTrustData failed: {0:x}", (object) winTrustData);
          signature = SignatureHelper.GetSignatureFromWintrustData(fileName, winTrustData, wtData);
          uint num = System.Management.Automation.Security.NativeMethods.DestroyWintrustDataStruct(wtData);
          if (num != 0U)
            SignatureHelper.tracer.WriteLine("DestroyWinTrustDataStruct failed: {0:x}", (object) num);
        }
        catch (AccessViolationException ex)
        {
          signature = new Signature(fileName, 2148204800U);
        }
        return signature;
      }
    }

    [ArchitectureSensitive]
    private static uint GetWinTrustData(
      string fileName,
      string fileContent,
      out System.Management.Automation.Security.NativeMethods.WINTRUST_DATA wtData)
    {
      using (SignatureHelper.tracer.TraceMethod(fileName, new object[0]))
      {
        uint num1 = 2147500037;
        IntPtr num2 = IntPtr.Zero;
        IntPtr num3 = IntPtr.Zero;
        Guid guid = new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");
        try
        {
          num2 = Marshal.AllocCoTaskMem(Marshal.SizeOf((object) guid));
          Marshal.StructureToPtr((object) guid, num2, false);
          System.Management.Automation.Security.NativeMethods.WINTRUST_DATA wintrustData = fileContent != null ? System.Management.Automation.Security.NativeMethods.InitWintrustDataStructFromBlob(System.Management.Automation.Security.NativeMethods.InitWintrustBlobInfoStruct(fileName, fileContent)) : System.Management.Automation.Security.NativeMethods.InitWintrustDataStructFromFile(System.Management.Automation.Security.NativeMethods.InitWintrustFileInfoStruct(fileName));
          num3 = Marshal.AllocCoTaskMem(Marshal.SizeOf((object) wintrustData));
          Marshal.StructureToPtr((object) wintrustData, num3, false);
          num1 = System.Management.Automation.Security.NativeMethods.WinVerifyTrust(IntPtr.Zero, num2, num3);
          wtData = (System.Management.Automation.Security.NativeMethods.WINTRUST_DATA) Marshal.PtrToStructure(num3, typeof (System.Management.Automation.Security.NativeMethods.WINTRUST_DATA));
        }
        finally
        {
          Marshal.DestroyStructure(num2, typeof (Guid));
          Marshal.FreeCoTaskMem(num2);
          Marshal.DestroyStructure(num3, typeof (System.Management.Automation.Security.NativeMethods.WINTRUST_DATA));
          Marshal.FreeCoTaskMem(num3);
        }
        return num1;
      }
    }

    [ArchitectureSensitive]
    private static X509Certificate2 GetCertFromChain(IntPtr pSigner)
    {
      X509Certificate2 x509Certificate2 = (X509Certificate2) null;
      IntPtr provCertFromChain = System.Management.Automation.Security.NativeMethods.WTHelperGetProvCertFromChain(pSigner, 0U);
      if (provCertFromChain != IntPtr.Zero)
        x509Certificate2 = new X509Certificate2(((System.Management.Automation.Security.NativeMethods.CRYPT_PROVIDER_CERT) Marshal.PtrToStructure(provCertFromChain, typeof (System.Management.Automation.Security.NativeMethods.CRYPT_PROVIDER_CERT))).pCert);
      return x509Certificate2;
    }

    [ArchitectureSensitive]
    private static Signature GetSignatureFromWintrustData(
      string filePath,
      uint error,
      System.Management.Automation.Security.NativeMethods.WINTRUST_DATA wtd)
    {
      Signature signature = (Signature) null;
      X509Certificate2 timestamper = (X509Certificate2) null;
      SignatureHelper.tracer.WriteLine("GetSignatureFromWintrustData: error: {0}", (object) error);
      IntPtr pProvData = System.Management.Automation.Security.NativeMethods.WTHelperProvDataFromStateData(wtd.hWVTStateData);
      if (pProvData != IntPtr.Zero)
      {
        IntPtr provSignerFromChain = System.Management.Automation.Security.NativeMethods.WTHelperGetProvSignerFromChain(pProvData, 0U, 0U, 0U);
        if (provSignerFromChain != IntPtr.Zero)
        {
          X509Certificate2 certFromChain = SignatureHelper.GetCertFromChain(provSignerFromChain);
          if (certFromChain != null)
          {
            System.Management.Automation.Security.NativeMethods.CRYPT_PROVIDER_SGNR structure = (System.Management.Automation.Security.NativeMethods.CRYPT_PROVIDER_SGNR) Marshal.PtrToStructure(provSignerFromChain, typeof (System.Management.Automation.Security.NativeMethods.CRYPT_PROVIDER_SGNR));
            if (structure.csCounterSigners == 1U)
              timestamper = SignatureHelper.GetCertFromChain(structure.pasCounterSigners);
            signature = timestamper == null ? new Signature(filePath, error, certFromChain) : new Signature(filePath, error, certFromChain, timestamper);
          }
        }
      }
      if (signature == null && error != 0U)
        signature = new Signature(filePath, error);
      return signature;
    }

    private static string GetStringValue(string s) => s != null ? s : "null";

    private static string GetCertName(X509Certificate2 c) => c != null ? c.Thumbprint : "null";

    [ArchitectureSensitive]
    private static uint GetLastWin32Error() => SecuritySupport.GetDWORDFromInt(Marshal.GetLastWin32Error());
  }
}
