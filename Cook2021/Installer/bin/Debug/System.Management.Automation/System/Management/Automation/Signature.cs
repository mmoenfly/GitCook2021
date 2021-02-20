// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Signature
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace System.Management.Automation
{
  public sealed class Signature
  {
    [TraceSource("Signature", "tracer for Signature")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer(nameof (Signature), "tracer for Signature");
    private string path;
    private SignatureStatus status = SignatureStatus.UnknownError;
    private uint win32Error;
    private X509Certificate2 signerCert;
    private string statusMessage = string.Empty;
    private X509Certificate2 timeStamperCert;

    public X509Certificate2 SignerCertificate
    {
      get
      {
        using (Signature.tracer.TraceProperty())
          return this.signerCert;
      }
    }

    public X509Certificate2 TimeStamperCertificate
    {
      get
      {
        using (Signature.tracer.TraceProperty())
          return this.timeStamperCert;
      }
    }

    public SignatureStatus Status
    {
      get
      {
        using (Signature.tracer.TraceProperty())
          return this.status;
      }
    }

    public string StatusMessage
    {
      get
      {
        using (Signature.tracer.TraceProperty())
          return this.statusMessage;
      }
    }

    public string Path
    {
      get
      {
        using (Signature.tracer.TraceProperty())
          return this.path;
      }
    }

    internal Signature(
      string filePath,
      uint error,
      X509Certificate2 signer,
      X509Certificate2 timestamper)
    {
      Utils.CheckArgForNullOrEmpty(Signature.tracer, filePath, nameof (filePath));
      Utils.CheckArgForNull(Signature.tracer, (object) signer, nameof (signer));
      Utils.CheckArgForNull(Signature.tracer, (object) timestamper, nameof (timestamper));
      this.Init(filePath, signer, error, timestamper);
    }

    internal Signature(string filePath, X509Certificate2 signer)
    {
      Utils.CheckArgForNullOrEmpty(Signature.tracer, filePath, nameof (filePath));
      Utils.CheckArgForNull(Signature.tracer, (object) signer, nameof (signer));
      this.Init(filePath, signer, 0U, (X509Certificate2) null);
    }

    internal Signature(string filePath, uint error, X509Certificate2 signer)
    {
      Utils.CheckArgForNullOrEmpty(Signature.tracer, filePath, nameof (filePath));
      Utils.CheckArgForNull(Signature.tracer, (object) signer, nameof (signer));
      this.Init(filePath, signer, error, (X509Certificate2) null);
    }

    internal Signature(string filePath, uint error)
    {
      Utils.CheckArgForNullOrEmpty(Signature.tracer, filePath, nameof (filePath));
      this.Init(filePath, (X509Certificate2) null, error, (X509Certificate2) null);
    }

    private void Init(
      string filePath,
      X509Certificate2 signer,
      uint error,
      X509Certificate2 timestamper)
    {
      using (Signature.tracer.TraceMethod())
      {
        this.path = filePath;
        this.win32Error = error;
        this.signerCert = signer;
        this.timeStamperCert = timestamper;
        SignatureStatus statusFromWin32Error = Signature.GetSignatureStatusFromWin32Error(error);
        this.status = statusFromWin32Error;
        this.statusMessage = Signature.GetSignatureStatusMessage(statusFromWin32Error, error, filePath);
      }
    }

    private static SignatureStatus GetSignatureStatusFromWin32Error(uint error)
    {
      using (Signature.tracer.TraceMethod((object) error))
      {
        SignatureStatus signatureStatus = SignatureStatus.UnknownError;
        switch (error)
        {
          case 0:
            signatureStatus = SignatureStatus.Valid;
            break;
          case 2148073480:
            signatureStatus = SignatureStatus.Incompatible;
            break;
          case 2148081677:
          case 2148098064:
            signatureStatus = SignatureStatus.HashMismatch;
            break;
          case 2148204545:
            signatureStatus = SignatureStatus.NotSupportedFileFormat;
            break;
          case 2148204800:
            signatureStatus = SignatureStatus.NotSigned;
            break;
          case 2148204817:
            signatureStatus = SignatureStatus.NotTrusted;
            break;
        }
        return signatureStatus;
      }
    }

    private static string GetSignatureStatusMessage(
      SignatureStatus status,
      uint error,
      string filePath)
    {
      using (Signature.tracer.TraceMethod())
      {
        string str1 = (string) null;
        string resourceId = (string) null;
        string str2 = (string) null;
        switch (status)
        {
          case SignatureStatus.Valid:
            resourceId = "MshSignature_Valid";
            break;
          case SignatureStatus.UnknownError:
            str1 = new Win32Exception(SecuritySupport.GetIntFromDWORD(error)).Message;
            break;
          case SignatureStatus.NotSigned:
            resourceId = "MshSignature_NotSigned";
            str2 = filePath;
            break;
          case SignatureStatus.HashMismatch:
            resourceId = "MshSignature_HashMismatch";
            str2 = filePath;
            break;
          case SignatureStatus.NotTrusted:
            resourceId = "MshSignature_NotTrusted";
            str2 = filePath;
            break;
          case SignatureStatus.NotSupportedFileFormat:
            resourceId = "MshSignature_NotSupportedFileFormat";
            str2 = System.IO.Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(str2))
            {
              resourceId = "MshSignature_NotSupportedFileFormat_NoExtension";
              str2 = (string) null;
              break;
            }
            break;
          case SignatureStatus.Incompatible:
            resourceId = error != 2148073480U ? "MshSignature_Incompatible" : "MshSignature_Incompatible_HashAlgorithm";
            str2 = filePath;
            break;
        }
        if (str1 == null)
        {
          if (str2 == null)
            str1 = ResourceManagerCache.GetResourceString("MshSignature", resourceId);
          else
            str1 = ResourceManagerCache.FormatResourceString("MshSignature", resourceId, (object) str2);
        }
        return str1;
      }
    }
  }
}
