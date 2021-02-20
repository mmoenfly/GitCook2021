// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSArgumentOutOfRangeException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
  [Serializable]
  public class PSArgumentOutOfRangeException : ArgumentOutOfRangeException, IContainsErrorRecord
  {
    private ErrorRecord _errorRecord;
    private string _errorId = "ArgumentOutOfRange";

    public PSArgumentOutOfRangeException()
    {
    }

    public PSArgumentOutOfRangeException(string paramName)
      : base(paramName)
    {
    }

    public PSArgumentOutOfRangeException(string paramName, object actualValue, string message)
      : base(paramName, actualValue, message)
    {
    }

    protected PSArgumentOutOfRangeException(SerializationInfo info, StreamingContext context)
      : base(info, context)
      => this._errorId = info.GetString("ErrorId");

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      info.AddValue("ErrorId", (object) this._errorId);
    }

    public PSArgumentOutOfRangeException(string message, Exception innerException)
      : base(message, innerException)
    {
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
  }
}
