// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.WildcardPatternException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class WildcardPatternException : RuntimeException
  {
    [NonSerialized]
    private ErrorRecord _errorRecord;
    [TraceSource("WildcardPatternException", "WildcardPatternException")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (WildcardPatternException), nameof (WildcardPatternException));

    internal WildcardPatternException(ErrorRecord errorRecord)
      : base(RuntimeException.RetrieveMessage(errorRecord))
      => this._errorRecord = errorRecord != null ? errorRecord : throw new ArgumentNullException(nameof (errorRecord));

    public WildcardPatternException()
    {
    }

    public WildcardPatternException(string message)
      : base(message)
    {
    }

    public WildcardPatternException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected WildcardPatternException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
