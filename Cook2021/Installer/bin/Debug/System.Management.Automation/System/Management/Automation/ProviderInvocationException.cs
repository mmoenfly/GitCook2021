// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProviderInvocationException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;
using System.Threading;

namespace System.Management.Automation
{
  [Serializable]
  public class ProviderInvocationException : RuntimeException
  {
    [NonSerialized]
    internal ProviderInfo _providerInfo;
    [NonSerialized]
    private ErrorRecord _errorRecord;
    [NonSerialized]
    private new string _message;

    public ProviderInvocationException()
    {
    }

    protected ProviderInvocationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public ProviderInvocationException(string message)
      : base(message)
      => this._message = message;

    internal ProviderInvocationException(ProviderInfo provider, Exception innerException)
      : base(RuntimeException.RetrieveMessage(innerException), innerException)
    {
      this._message = base.Message;
      this._providerInfo = provider;
      if (innerException is IContainsErrorRecord containsErrorRecord && containsErrorRecord.ErrorRecord != null)
        this._errorRecord = new ErrorRecord(containsErrorRecord.ErrorRecord, innerException);
      else
        this._errorRecord = new ErrorRecord(innerException, "ErrorRecordNotSpecified", ErrorCategory.InvalidOperation, (object) null);
    }

    internal ProviderInvocationException(ProviderInfo provider, ErrorRecord errorRecord)
      : base(RuntimeException.RetrieveMessage(errorRecord), RuntimeException.RetrieveException(errorRecord))
    {
      if (errorRecord == null)
        throw new ArgumentNullException(nameof (errorRecord));
      this._message = base.Message;
      this._providerInfo = provider;
      this._errorRecord = errorRecord;
    }

    public ProviderInvocationException(string message, Exception innerException)
      : base(message, innerException)
      => this._message = message;

    internal ProviderInvocationException(
      string errorId,
      ProviderInfo provider,
      string path,
      Exception innerException)
      : this(errorId, provider, path, innerException, true)
    {
    }

    internal ProviderInvocationException(
      string errorId,
      ProviderInfo provider,
      string path,
      Exception innerException,
      bool useInnerExceptionMessage)
      : base(ProviderInvocationException.RetrieveMessage(errorId, provider, path, innerException), innerException)
    {
      this._providerInfo = provider;
      this._message = base.Message;
      Exception exception = !useInnerExceptionMessage ? (Exception) new ParentContainsErrorRecordException((Exception) this) : innerException;
      if (innerException is IContainsErrorRecord containsErrorRecord && containsErrorRecord.ErrorRecord != null)
        this._errorRecord = new ErrorRecord(containsErrorRecord.ErrorRecord, exception);
      else
        this._errorRecord = new ErrorRecord(exception, errorId, ErrorCategory.InvalidOperation, (object) null);
    }

    public ProviderInfo ProviderInfo => this._providerInfo;

    public override ErrorRecord ErrorRecord => this._errorRecord;

    private static string RetrieveMessage(
      string errorId,
      ProviderInfo provider,
      string path,
      Exception innerException)
    {
      if (innerException == null)
        return "";
      if (string.IsNullOrEmpty(errorId) || provider == null)
        return RuntimeException.RetrieveMessage(innerException);
      string format = ResourceManagerCache.GetResourceManager("SessionStateStrings").GetString(errorId);
      if (string.IsNullOrEmpty(format))
        return RuntimeException.RetrieveMessage(innerException);
      string str;
      if (path == null)
        str = string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, format, (object) provider.Name, (object) RuntimeException.RetrieveMessage(innerException));
      else
        str = string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, format, (object) provider.Name, (object) path, (object) RuntimeException.RetrieveMessage(innerException));
      return str;
    }

    public override string Message => !string.IsNullOrEmpty(this._message) ? this._message : base.Message;
  }
}
