// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CmdletInvocationException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
  [Serializable]
  public class CmdletInvocationException : RuntimeException
  {
    private ErrorRecord _errorRecord;

    internal CmdletInvocationException(ErrorRecord errorRecord)
      : base(RuntimeException.RetrieveMessage(errorRecord), RuntimeException.RetrieveException(errorRecord))
    {
      this._errorRecord = errorRecord != null ? errorRecord : throw new ArgumentNullException(nameof (errorRecord));
      Exception exception = errorRecord.Exception;
    }

    internal CmdletInvocationException(Exception innerException, InvocationInfo invocationInfo)
      : base(RuntimeException.RetrieveMessage(innerException), innerException)
    {
      if (innerException == null)
        throw new ArgumentNullException(nameof (innerException));
      this._errorRecord = !(innerException is IContainsErrorRecord containsErrorRecord) || containsErrorRecord.ErrorRecord == null ? new ErrorRecord(innerException, innerException.GetType().FullName, ErrorCategory.NotSpecified, (object) null) : new ErrorRecord(containsErrorRecord.ErrorRecord, innerException);
      this._errorRecord.SetInvocationInfo(invocationInfo);
    }

    public CmdletInvocationException()
    {
    }

    public CmdletInvocationException(string message)
      : base(message)
    {
    }

    public CmdletInvocationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected CmdletInvocationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      if (!info.GetBoolean("HasErrorRecord"))
        return;
      this._errorRecord = (ErrorRecord) info.GetValue(nameof (ErrorRecord), typeof (ErrorRecord));
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      bool flag = null != this._errorRecord;
      info.AddValue("HasErrorRecord", flag);
      if (!flag)
        return;
      info.AddValue("ErrorRecord", (object) this._errorRecord);
    }

    public override ErrorRecord ErrorRecord => this._errorRecord;
  }
}
