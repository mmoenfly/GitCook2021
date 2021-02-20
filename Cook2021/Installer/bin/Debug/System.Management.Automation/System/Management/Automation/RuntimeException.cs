// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RuntimeException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
  [Serializable]
  public class RuntimeException : SystemException, IContainsErrorRecord
  {
    private ErrorRecord _errorRecord;
    private string _errorId = nameof (RuntimeException);
    private ErrorCategory _errorCategory;
    private object _targetObject;
    private string _overrideStackTrace;
    private bool thrownByThrowStatement;
    private bool suppressPromptInInterpreter;
    private Token _errorToken;

    public RuntimeException()
    {
    }

    protected RuntimeException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this._errorId = info.GetString("ErrorId");
      this._errorCategory = (ErrorCategory) info.GetInt32("ErrorCategory");
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      info.AddValue("ErrorId", (object) this._errorId);
      info.AddValue("ErrorCategory", (int) this._errorCategory);
    }

    public RuntimeException(string message)
      : base(message)
    {
    }

    public RuntimeException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    internal RuntimeException(string message, Exception innerException, ErrorRecord errorRecord)
      : base(message, innerException)
      => this._errorRecord = errorRecord;

    public virtual ErrorRecord ErrorRecord
    {
      get
      {
        if (this._errorRecord == null)
          this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), this._errorId, this._errorCategory, this._targetObject);
        return this._errorRecord;
      }
    }

    internal void SetErrorId(string errorId)
    {
      if (!(this._errorId != errorId))
        return;
      this._errorId = errorId;
      this._errorRecord = (ErrorRecord) null;
    }

    internal void SetErrorCategory(ErrorCategory errorCategory)
    {
      if (this._errorCategory == errorCategory)
        return;
      this._errorCategory = errorCategory;
      this._errorRecord = (ErrorRecord) null;
    }

    internal void SetTargetObject(object targetObject)
    {
      this._targetObject = targetObject;
      if (this._errorRecord == null)
        return;
      this._errorRecord.SetTargetObject(targetObject);
    }

    public override string StackTrace => !string.IsNullOrEmpty(this._overrideStackTrace) ? this._overrideStackTrace : base.StackTrace;

    internal static void LockStackTrace(Exception e)
    {
      if (!(e is RuntimeException runtimeException) || !string.IsNullOrEmpty(runtimeException._overrideStackTrace))
        return;
      string stackTrace = runtimeException.StackTrace;
      if (string.IsNullOrEmpty(stackTrace))
        return;
      runtimeException._overrideStackTrace = stackTrace;
    }

    internal static string RetrieveMessage(ErrorRecord errorRecord)
    {
      if (errorRecord == null)
        return "";
      if (errorRecord.ErrorDetails != null && !string.IsNullOrEmpty(errorRecord.ErrorDetails.Message))
        return errorRecord.ErrorDetails.Message;
      return errorRecord.Exception == null ? "" : errorRecord.Exception.Message;
    }

    internal static string RetrieveMessage(Exception e)
    {
      if (e == null)
        return "";
      if (!(e is IContainsErrorRecord containsErrorRecord))
        return e.Message;
      ErrorRecord errorRecord = containsErrorRecord.ErrorRecord;
      if (errorRecord == null)
        return e.Message;
      ErrorDetails errorDetails = errorRecord.ErrorDetails;
      if (errorDetails == null)
        return e.Message;
      string message = errorDetails.Message;
      return !string.IsNullOrEmpty(message) ? message : e.Message;
    }

    internal static Exception RetrieveException(ErrorRecord errorRecord) => errorRecord?.Exception;

    public bool WasThrownFromThrowStatement
    {
      get => this.thrownByThrowStatement;
      set
      {
        this.thrownByThrowStatement = value;
        if (this._errorRecord == null || !(this._errorRecord.Exception is RuntimeException exception))
          return;
        exception.WasThrownFromThrowStatement = value;
      }
    }

    internal bool SuppressPromptInInterpreter
    {
      get => this.suppressPromptInInterpreter;
      set => this.suppressPromptInInterpreter = value;
    }

    internal Token ErrorToken
    {
      get => this._errorToken;
      set => this._errorToken = value;
    }
  }
}
