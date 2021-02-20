// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSArgumentNullException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
  [Serializable]
  public class PSArgumentNullException : ArgumentNullException, IContainsErrorRecord
  {
    private ErrorRecord _errorRecord;
    private string _errorId = "ArgumentNull";
    private new string _message;

    public PSArgumentNullException()
    {
    }

    public PSArgumentNullException(string paramName)
      : base(paramName)
    {
    }

    public PSArgumentNullException(string message, Exception innerException)
      : base(message, innerException)
      => this._message = message;

    public PSArgumentNullException(string paramName, string message)
      : base(paramName, message)
      => this._message = message;

    protected PSArgumentNullException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this._errorId = info.GetString("ErrorId");
      this._message = info.GetString("PSArgumentNullException_MessageOverride");
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      info.AddValue("ErrorId", (object) this._errorId);
      info.AddValue("PSArgumentNullException_MessageOverride", (object) this._message);
    }

    public ErrorRecord ErrorRecord
    {
      get
      {
        if (this._errorRecord == null)
          this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), this._errorId, ErrorCategory.InvalidArgument, (object) null);
        return this._errorRecord;
      }
    }

    public override string Message => !string.IsNullOrEmpty(this._message) ? this._message : base.Message;
  }
}
