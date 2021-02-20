// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSRemotingCryptoHelperServer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  internal class PSRemotingCryptoHelperServer : PSRemotingCryptoHelper
  {
    private RemoteSession _session;

    internal PSRemotingCryptoHelperServer() => this._rsaCryptoProvider = PSRSACryptoServiceProvider.GetRSACryptoServiceProviderForServer();

    internal bool ImportRemotePublicKey(string publicKeyAsString)
    {
      try
      {
        this._rsaCryptoProvider.ImportPublicKeyFromBase64EncodedString(publicKeyAsString);
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

    internal bool ExportEncryptedSessionKey(out string encryptedSessionKey)
    {
      try
      {
        encryptedSessionKey = this._rsaCryptoProvider.GenerateSessionKeyAndSafeExport();
      }
      catch (PSCryptoException ex)
      {
        encryptedSessionKey = string.Empty;
        return false;
      }
      return true;
    }

    internal static PSRemotingCryptoHelperServer GetTestRemotingCryptHelperServer()
    {
      PSRemotingCryptoHelperServer cryptoHelperServer = new PSRemotingCryptoHelperServer();
      cryptoHelperServer.Session = (RemoteSession) new TestHelperSession();
      return cryptoHelperServer;
    }
  }
}
