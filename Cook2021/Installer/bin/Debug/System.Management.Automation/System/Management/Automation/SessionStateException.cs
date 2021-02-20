// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SessionStateException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
  [Serializable]
  public class SessionStateException : RuntimeException
  {
    private ErrorRecord _errorRecord;
    private string _itemName = string.Empty;
    private SessionStateCategory _sessionStateCategory;
    private string _errorId = nameof (SessionStateException);
    private ErrorCategory _errorCategory = ErrorCategory.InvalidArgument;
    [TraceSource("SessionStateException", "SessionStateException")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (SessionStateException), nameof (SessionStateException));

    internal SessionStateException(
      string itemName,
      SessionStateCategory sessionStateCategory,
      string errorIdAndResourceId,
      ErrorCategory errorCategory,
      params object[] messageArgs)
      : base(SessionStateException.BuildMessage(itemName, errorIdAndResourceId, messageArgs))
    {
      this._itemName = itemName;
      this._sessionStateCategory = sessionStateCategory;
      this._errorId = errorIdAndResourceId;
      this._errorCategory = errorCategory;
    }

    public SessionStateException()
    {
    }

    public SessionStateException(string message)
      : base(message)
    {
    }

    public SessionStateException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected SessionStateException(SerializationInfo info, StreamingContext context)
      : base(info, context)
      => this._sessionStateCategory = (SessionStateCategory) info.GetInt32(nameof (SessionStateCategory));

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      info.AddValue("SessionStateCategory", (int) this._sessionStateCategory);
    }

    public override ErrorRecord ErrorRecord
    {
      get
      {
        if (this._errorRecord == null)
          this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), this._errorId, this._errorCategory, (object) this._itemName);
        return this._errorRecord;
      }
    }

    public string ItemName => this._itemName;

    public SessionStateCategory SessionStateCategory => this._sessionStateCategory;

    private static string BuildMessage(
      string itemName,
      string resourceId,
      params object[] messageArgs)
    {
      try
      {
        object[] objArray;
        if (messageArgs != null && 0 < messageArgs.Length)
        {
          objArray = new object[messageArgs.Length + 1];
          objArray[0] = (object) itemName;
          messageArgs.CopyTo((Array) objArray, 1);
        }
        else
          objArray = new object[1]{ (object) itemName };
        return ResourceManagerCache.FormatResourceString("SessionStateStrings", resourceId, objArray);
      }
      catch (MissingManifestResourceException ex)
      {
        SessionStateException.tracer.TraceException((Exception) ex);
        return ResourceManagerCache.FormatResourceString("SessionStateStrings", "ResourceStringLoadError", (object) itemName, (object) "SessionStateStrings", (object) resourceId, (object) ex.Message);
      }
      catch (FormatException ex)
      {
        SessionStateException.tracer.TraceException((Exception) ex);
        return ResourceManagerCache.FormatResourceString("SessionStateStrings", "ResourceStringFormatError", (object) itemName, (object) "SessionStateStrings", (object) resourceId, (object) ex.Message);
      }
    }
  }
}
