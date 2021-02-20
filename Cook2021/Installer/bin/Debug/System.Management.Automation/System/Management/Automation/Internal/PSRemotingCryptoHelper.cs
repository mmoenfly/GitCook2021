// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSRemotingCryptoHelper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Management.Automation.Internal
{
  internal abstract class PSRemotingCryptoHelper : IDisposable
  {
    protected PSRSACryptoServiceProvider _rsaCryptoProvider;
    protected ManualResetEvent _keyExchangeCompleted = new ManualResetEvent(false);
    protected object syncObject = new object();
    private bool keyExchangeStarted;

    internal string EncryptSecureString(SecureString secureString)
    {
      string str = (string) null;
      if (!this._rsaCryptoProvider.CanEncrypt)
      {
        try
        {
          lock (this.syncObject)
          {
            if (!this._rsaCryptoProvider.CanEncrypt)
            {
              if (!this.keyExchangeStarted)
              {
                this.keyExchangeStarted = true;
                this._keyExchangeCompleted.Reset();
                this.Session.StartKeyExchange();
              }
            }
          }
        }
        finally
        {
          this._keyExchangeCompleted.WaitOne();
        }
      }
      if (this._rsaCryptoProvider.CanEncrypt)
      {
        IntPtr bstr = Marshal.SecureStringToBSTR(secureString);
        if (bstr != IntPtr.Zero)
        {
          byte[] data = new byte[secureString.Length * 2];
          for (int ofs = 0; ofs < data.Length; ++ofs)
            data[ofs] = Marshal.ReadByte(bstr, ofs);
          Marshal.ZeroFreeBSTR(bstr);
          try
          {
            str = Convert.ToBase64String(this._rsaCryptoProvider.EncryptWithSessionKey(data));
          }
          finally
          {
            for (int index = 0; index < data.Length; ++index)
              data[index] = (byte) 0;
          }
        }
      }
      return str;
    }

    internal SecureString DecryptSecureString(string encryptedString)
    {
      SecureString secureString = (SecureString) null;
      this._keyExchangeCompleted.WaitOne();
      if (this._rsaCryptoProvider.CanEncrypt)
      {
        byte[] data;
        try
        {
          data = Convert.FromBase64String(encryptedString);
        }
        catch (FormatException ex)
        {
          throw new PSCryptoException();
        }
        if (data != null)
        {
          byte[] numArray = this._rsaCryptoProvider.DecryptWithSessionKey(data);
          secureString = new SecureString();
          ushort num1 = 0;
          try
          {
            for (int index = 0; index < numArray.Length; index += 2)
            {
              ushort num2 = (ushort) ((uint) numArray[index] + (uint) (ushort) ((uint) numArray[index + 1] << 8));
              secureString.AppendChar((char) num2);
              num1 = (ushort) 0;
            }
          }
          finally
          {
            num1 = (ushort) 0;
            for (int index = 0; index < numArray.Length; index += 2)
            {
              numArray[index] = (byte) 0;
              numArray[index + 1] = (byte) 0;
            }
          }
        }
      }
      return secureString;
    }

    internal abstract RemoteSession Session { get; set; }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      if (this._rsaCryptoProvider != null)
        this._rsaCryptoProvider.Dispose();
      this._rsaCryptoProvider = (PSRSACryptoServiceProvider) null;
      this._keyExchangeCompleted.Close();
    }

    internal void CompleteKeyExchange() => this._keyExchangeCompleted.Set();
  }
}
