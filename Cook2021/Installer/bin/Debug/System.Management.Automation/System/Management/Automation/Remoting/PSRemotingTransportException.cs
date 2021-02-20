// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.PSRemotingTransportException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation.Remoting
{
  [Serializable]
  public class PSRemotingTransportException : RuntimeException
  {
    private int _errorCode;
    private string _transportMessage;

    public PSRemotingTransportException()
      : base(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.DefaultRemotingExceptionMessage, (object) typeof (PSRemotingTransportException).FullName))
      => this.SetDefaultErrorRecord();

    public PSRemotingTransportException(string message)
      : base(message)
      => this.SetDefaultErrorRecord();

    public PSRemotingTransportException(string message, Exception innerException)
      : base(message, innerException)
      => this.SetDefaultErrorRecord();

    internal PSRemotingTransportException(PSRemotingErrorId errorId, params object[] args)
      : base(PSRemotingErrorInvariants.FormatResourceString(errorId, args))
    {
      this.SetDefaultErrorRecord();
      this._errorCode = (int) errorId;
    }

    internal PSRemotingTransportException(
      Exception innerException,
      PSRemotingErrorId errorId,
      params object[] args)
      : base(PSRemotingErrorInvariants.FormatResourceString(errorId, args), innerException)
    {
      this.SetDefaultErrorRecord();
    }

    protected PSRemotingTransportException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this._errorCode = info != null ? info.GetInt32(nameof (ErrorCode)) : throw new PSArgumentNullException(nameof (info));
      this._transportMessage = info.GetString(nameof (TransportMessage));
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      info.AddValue("ErrorCode", this._errorCode);
      info.AddValue("TransportMessage", (object) this._transportMessage);
    }

    protected void SetDefaultErrorRecord()
    {
      this.SetErrorCategory(ErrorCategory.ResourceUnavailable);
      this.SetErrorId(typeof (PSRemotingDataStructureException).FullName);
    }

    public int ErrorCode
    {
      get => this._errorCode;
      set => this._errorCode = value;
    }

    public string TransportMessage
    {
      get => this._transportMessage;
      set => this._transportMessage = value;
    }
  }
}
