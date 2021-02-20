// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSRSACryptoServiceProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.ComponentModel;
using System.Text;

namespace System.Management.Automation.Internal
{
  internal class PSRSACryptoServiceProvider : IDisposable
  {
    private PSSafeCryptProvHandle hProv;
    private bool canEncrypt;
    private PSSafeCryptKey hRSAKey;
    private PSSafeCryptKey hSessionKey;
    private static PSSafeCryptProvHandle _hStaticProv;
    private static PSSafeCryptKey _hStaticRSAKey;
    private static bool keyGenerated = false;
    private static object syncObject = new object();

    private PSRSACryptoServiceProvider(bool serverMode)
    {
      if (serverMode)
      {
        this.hProv = new PSSafeCryptProvHandle();
        this.CheckStatus(PSCryptoNativeUtils.CryptAcquireContext(ref this.hProv, (string) null, (string) null, 24U, 4026531840U));
        this.hRSAKey = new PSSafeCryptKey();
      }
      this.hSessionKey = new PSSafeCryptKey();
    }

    internal string GetPublicKeyAsBase64EncodedString()
    {
      uint pdwDataLen = 0;
      this.CheckStatus(PSCryptoNativeUtils.CryptExportKey(this.hRSAKey, PSSafeCryptKey.Zero, 6U, 0U, (byte[]) null, ref pdwDataLen));
      byte[] numArray = new byte[(IntPtr) pdwDataLen];
      this.CheckStatus(PSCryptoNativeUtils.CryptExportKey(this.hRSAKey, PSSafeCryptKey.Zero, 6U, 0U, numArray, ref pdwDataLen));
      return Convert.ToBase64String(numArray, Base64FormattingOptions.None);
    }

    internal string GenerateSessionKeyAndSafeExport()
    {
      this.CheckStatus(PSCryptoNativeUtils.CryptGenKey(this.hProv, 26128U, 16777221U, ref this.hSessionKey));
      uint pdwDataLen = 0;
      this.CheckStatus(PSCryptoNativeUtils.CryptExportKey(this.hSessionKey, this.hRSAKey, 1U, 0U, (byte[]) null, ref pdwDataLen));
      byte[] numArray = new byte[(IntPtr) pdwDataLen];
      this.CheckStatus(PSCryptoNativeUtils.CryptExportKey(this.hSessionKey, this.hRSAKey, 1U, 0U, numArray, ref pdwDataLen));
      this.canEncrypt = true;
      return Convert.ToBase64String(numArray, Base64FormattingOptions.None);
    }

    internal void ImportPublicKeyFromBase64EncodedString(string publicKey)
    {
      byte[] pbData = Convert.FromBase64String(publicKey);
      this.CheckStatus(PSCryptoNativeUtils.CryptImportKey(this.hProv, pbData, pbData.Length, PSSafeCryptKey.Zero, 0U, ref this.hRSAKey));
    }

    internal void ImportSessionKeyFromBase64EncodedString(string sessionKey)
    {
      byte[] pbData = Convert.FromBase64String(sessionKey);
      this.CheckStatus(PSCryptoNativeUtils.CryptImportKey(this.hProv, pbData, pbData.Length, this.hRSAKey, 0U, ref this.hSessionKey));
      this.canEncrypt = true;
    }

    internal byte[] EncryptWithSessionKey(byte[] data)
    {
      byte[] pbData = new byte[data.Length];
      Array.Copy((Array) data, 0, (Array) pbData, 0, data.Length);
      int length = pbData.Length;
      if (!PSCryptoNativeUtils.CryptEncrypt(this.hSessionKey, IntPtr.Zero, true, 0U, pbData, ref length, data.Length))
      {
        for (int index = 0; index < pbData.Length; ++index)
          pbData[index] = (byte) 0;
        pbData = new byte[length];
        Array.Copy((Array) data, 0, (Array) pbData, 0, data.Length);
        length = data.Length;
        this.CheckStatus(PSCryptoNativeUtils.CryptEncrypt(this.hSessionKey, IntPtr.Zero, true, 0U, pbData, ref length, pbData.Length));
      }
      byte[] numArray = new byte[length];
      Array.Copy((Array) pbData, 0, (Array) numArray, 0, length);
      return numArray;
    }

    internal byte[] DecryptWithSessionKey(byte[] data)
    {
      byte[] pbData = new byte[data.Length];
      Array.Copy((Array) data, 0, (Array) pbData, 0, data.Length);
      int length = pbData.Length;
      if (!PSCryptoNativeUtils.CryptDecrypt(this.hSessionKey, IntPtr.Zero, true, 0U, pbData, ref length))
      {
        pbData = new byte[length];
        Array.Copy((Array) data, 0, (Array) pbData, 0, data.Length);
        this.CheckStatus(PSCryptoNativeUtils.CryptDecrypt(this.hSessionKey, IntPtr.Zero, true, 0U, pbData, ref length));
      }
      byte[] numArray = new byte[length];
      Array.Copy((Array) pbData, 0, (Array) numArray, 0, length);
      for (int index = 0; index < pbData.Length; ++index)
        pbData[index] = (byte) 0;
      return numArray;
    }

    internal void GenerateKeyPair()
    {
      if (!PSRSACryptoServiceProvider.keyGenerated)
      {
        lock (PSRSACryptoServiceProvider.syncObject)
        {
          if (!PSRSACryptoServiceProvider.keyGenerated)
          {
            PSRSACryptoServiceProvider._hStaticProv = new PSSafeCryptProvHandle();
            this.CheckStatus(PSCryptoNativeUtils.CryptAcquireContext(ref PSRSACryptoServiceProvider._hStaticProv, (string) null, (string) null, 24U, 4026531840U));
            PSRSACryptoServiceProvider._hStaticRSAKey = new PSSafeCryptKey();
            this.CheckStatus(PSCryptoNativeUtils.CryptGenKey(PSRSACryptoServiceProvider._hStaticProv, 1U, 134217729U, ref PSRSACryptoServiceProvider._hStaticRSAKey));
            PSRSACryptoServiceProvider.keyGenerated = true;
          }
        }
      }
      this.hProv = PSRSACryptoServiceProvider._hStaticProv;
      this.hRSAKey = PSRSACryptoServiceProvider._hStaticRSAKey;
    }

    internal bool CanEncrypt
    {
      get => this.canEncrypt;
      set => this.canEncrypt = value;
    }

    internal static PSRSACryptoServiceProvider GetRSACryptoServiceProviderForClient() => new PSRSACryptoServiceProvider(false)
    {
      hProv = PSRSACryptoServiceProvider._hStaticProv,
      hRSAKey = PSRSACryptoServiceProvider._hStaticRSAKey
    };

    internal static PSRSACryptoServiceProvider GetRSACryptoServiceProviderForServer() => new PSRSACryptoServiceProvider(true);

    private void CheckStatus(bool value)
    {
      if (!value)
      {
        uint lastError = PSCryptoNativeUtils.GetLastError();
        StringBuilder message = new StringBuilder(new Win32Exception((int) lastError).Message);
        throw new PSCryptoException(lastError, message);
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      if (this.hSessionKey != null)
      {
        if (!this.hSessionKey.IsInvalid)
          this.hSessionKey.Dispose();
        this.hSessionKey = (PSSafeCryptKey) null;
      }
      if (PSRSACryptoServiceProvider._hStaticRSAKey == null && this.hRSAKey != null)
      {
        if (!this.hRSAKey.IsInvalid)
          this.hRSAKey.Dispose();
        this.hRSAKey = (PSSafeCryptKey) null;
      }
      if (PSRSACryptoServiceProvider._hStaticProv != null || this.hProv == null)
        return;
      if (!this.hProv.IsInvalid)
        this.hProv.Dispose();
      this.hProv = (PSSafeCryptProvHandle) null;
    }

    ~PSRSACryptoServiceProvider() => this.Dispose(true);
  }
}
