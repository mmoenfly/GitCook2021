// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSInvalidCastException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
  [Serializable]
  public class PSInvalidCastException : InvalidCastException, IContainsErrorRecord
  {
    internal const string BaseName = "ExtendedTypeSystem";
    internal const string InvalidCastExceptionMsg = "InvalidCastException";
    internal const string InvalidCastExceptionEnumerationNoFlagAndCommaMsg = "InvalidCastExceptionEnumerationNoFlagAndComma";
    internal const string InvalidCastExceptionEnumerationNoValueMsg = "InvalidCastExceptionEnumerationNoValue";
    internal const string ListSeparatorMsg = "ListSeparator";
    internal const string InvalidCastExceptionEnumerationMoreThanOneValueMsg = "InvalidCastExceptionEnumerationMoreThanOneValue";
    internal const string InvalidCastExceptionWithInnerExceptionMsg = "InvalidCastExceptionWithInnerException";
    internal const string InvalidCastFromNullMsg = "InvalidCastFromNull";
    internal const string InvalidCastExceptionEnumerationNullMsg = "InvalidCastExceptionEnumerationNull";
    internal const string InvalidCastCannotRetrieveStringMsg = "InvalidCastCannotRetrieveString";
    internal const string InvalidCastExceptionNoStringForConversionMsg = "InvalidCastExceptionNoStringForConversion";
    internal const string InvalidWMIClassPath = "InvalidWMIClassPath";
    internal const string InvalidWMIPath = "InvalidWMIPath";
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private ErrorRecord errorRecord;
    private string errorId = nameof (PSInvalidCastException);

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      info.AddValue("ErrorId", (object) this.errorId);
    }

    protected PSInvalidCastException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.errorId = info.GetString("ErrorId");
      PSInvalidCastException.tracer.TraceException((Exception) this);
    }

    public PSInvalidCastException()
      : base(typeof (PSInvalidCastException).FullName)
      => PSInvalidCastException.tracer.TraceException((Exception) this);

    public PSInvalidCastException(string message)
      : base(message)
      => PSInvalidCastException.tracer.TraceException((Exception) this);

    public PSInvalidCastException(string message, Exception innerException)
      : base(message, innerException)
      => PSInvalidCastException.tracer.TraceException((Exception) this);

    internal PSInvalidCastException(
      string errorId,
      Exception innerException,
      string baseName,
      string resourceId,
      params object[] arguments)
      : base(ResourceManagerCache.FormatResourceString(baseName, resourceId, arguments), innerException)
    {
      this.errorId = errorId;
      PSInvalidCastException.tracer.TraceException((Exception) this);
    }

    public ErrorRecord ErrorRecord
    {
      get
      {
        if (this.errorRecord == null)
          this.errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), this.errorId, ErrorCategory.InvalidArgument, (object) null);
        return this.errorRecord;
      }
    }
  }
}
