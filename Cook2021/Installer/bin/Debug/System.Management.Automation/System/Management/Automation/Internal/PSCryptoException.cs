// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSCryptoException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;
using System.Text;

namespace System.Management.Automation.Internal
{
  [Serializable]
  internal class PSCryptoException : Exception
  {
    private uint _errorCode;

    internal uint ErrorCode => this._errorCode;

    public PSCryptoException()
      : this(0U, new StringBuilder(string.Empty))
    {
    }

    public PSCryptoException(uint errorCode, StringBuilder message)
      : base(message.ToString())
      => this._errorCode = errorCode;

    public PSCryptoException(string message)
      : this(message, (Exception) null)
    {
    }

    public PSCryptoException(string message, Exception innerException)
      : base(message, innerException)
      => this._errorCode = uint.MaxValue;

    protected PSCryptoException(SerializationInfo info, StreamingContext context)
      : base(info, context)
      => this._errorCode = 268435455U;

    public override void GetObjectData(SerializationInfo info, StreamingContext context) => base.GetObjectData(info, context);
  }
}
