// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ActionPreferenceStopException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
  [Serializable]
  public class ActionPreferenceStopException : RuntimeException
  {
    private ErrorRecord _errorRecord;

    public ActionPreferenceStopException()
      : this(ResourceManagerCache.GetResourceString("GetErrorText", "ActionPreferenceStop"))
    {
    }

    internal ActionPreferenceStopException(ErrorRecord error)
      : this(RuntimeException.RetrieveMessage(error))
      => this._errorRecord = error != null ? error : throw new ArgumentNullException(nameof (error));

    internal ActionPreferenceStopException(
      InvocationInfo invocationInfo,
      string baseName,
      string resourceId,
      params object[] args)
      : this(ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args))
    {
      this.ErrorRecord.SetInvocationInfo(invocationInfo);
    }

    internal ActionPreferenceStopException(
      InvocationInfo invocationInfo,
      ErrorRecord errorRecord,
      string baseName,
      string resourceId,
      params object[] args)
      : this(invocationInfo, baseName, resourceId, args)
    {
      this._errorRecord = errorRecord != null ? errorRecord : throw new ArgumentNullException(nameof (errorRecord));
    }

    protected ActionPreferenceStopException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      if (info.GetBoolean("HasErrorRecord"))
        this._errorRecord = (ErrorRecord) info.GetValue(nameof (ErrorRecord), typeof (ErrorRecord));
      this.SuppressPromptInInterpreter = true;
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      if (info != null)
      {
        bool flag = null != this._errorRecord;
        info.AddValue("HasErrorRecord", flag);
        if (flag)
          info.AddValue("ErrorRecord", (object) this._errorRecord);
      }
      this.SuppressPromptInInterpreter = true;
    }

    public ActionPreferenceStopException(string message)
      : base(message)
    {
      this.SetErrorCategory(ErrorCategory.OperationStopped);
      this.SetErrorId("ActionPreferenceStop");
      this.SuppressPromptInInterpreter = true;
    }

    public ActionPreferenceStopException(string message, Exception innerException)
      : base(message, innerException)
    {
      this.SetErrorCategory(ErrorCategory.OperationStopped);
      this.SetErrorId("ActionPreferenceStop");
      this.SuppressPromptInInterpreter = true;
    }

    public override ErrorRecord ErrorRecord => this._errorRecord == null ? base.ErrorRecord : this._errorRecord;
  }
}
