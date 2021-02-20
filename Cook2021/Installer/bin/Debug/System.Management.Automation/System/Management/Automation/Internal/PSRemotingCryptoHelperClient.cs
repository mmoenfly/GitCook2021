// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSRemotingCryptoHelperClient
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  internal class PSRemotingCryptoHelperClient : PSRemotingCryptoHelper
  {
    private RemoteSession _session;

    internal PSRemotingCryptoHelperClient() => this._rsaCryptoProvider = PSRSACryptoServiceProvider.GetRSACryptoServiceProviderForClient();

    internal bool ExportLocalPublicKey(out string publicKeyAsString)
    {
      try
      {
        this._rsaCryptoProvider.GenerateKeyPair();
      }
      catch (PSCryptoException ex)
      {
        throw;
      }
      try
      {
        publicKeyAsString = this._rsaCryptoProvider.GetPublicKeyAsBase64EncodedString();
      }
      catch (PSCryptoException ex)
      {
        publicKeyAsString = string.Empty;
        return false;
      }
      return true;
    }

    internal bool ImportEncryptedSessionKey(string encryptedSessionKey)
    {
      try
      {
        this._rsaCryptoProvider.ImportSessionKeyFromBase64EncodedString(encryptedSessionKey);
      }
      catch (PSCryptoException ex)
      {
        return false;
      }
      return true;
    }

    internal override RemoteSession Session
    {
      get => this._session;
      set => this._session = value;
    }

    internal static PSRemotingCryptoHelperClient GetTestRemotingCryptHelperClient()
    {
      PSRemotingCryptoHelperClient cryptoHelperClient = new PSRemotingCryptoHelperClient();
      cryptoHelperClient.Session = (RemoteSession) new TestHelperSession();
      return cryptoHelperClient;
    }
  }
}
