// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSSecurityException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class PSSecurityException : RuntimeException
  {
    private ErrorRecord _errorRecord;
    private new string _message;

    public PSSecurityException()
    {
      this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), "UnauthorizedAccess", ErrorCategory.SecurityError, (object) null);
      this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "MshSecurityManager", "CanNotRun", new object[0]);
      this._message = this._errorRecord.ErrorDetails.Message;
    }

    protected PSSecurityException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), "UnauthorizedAccess", ErrorCategory.SecurityError, (object) null);
      this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "MshSecurityManager", "CanNotRun", new object[0]);
      this._message = this._errorRecord.ErrorDetails.Message;
    }

    public PSSecurityException(string message)
      : base(message)
    {
      this._message = message;
      this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), "UnauthorizedAccess", ErrorCategory.SecurityError, (object) null);
    }

    public PSSecurityException(string message, Exception innerException)
      : base(message, innerException)
    {
      this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), "UnauthorizedAccess", ErrorCategory.SecurityError, (object) null);
      this._errorRecord.ErrorDetails = new ErrorDetails(message);
      this._message = this._errorRecord.ErrorDetails.Message;
    }

    public override string Message => this._message;
  }
}
