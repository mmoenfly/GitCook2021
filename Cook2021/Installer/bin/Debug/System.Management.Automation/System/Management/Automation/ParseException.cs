// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParseException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class ParseException : RuntimeException
  {
    private const string errorIdString = "Parse";

    protected ParseException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public ParseException()
    {
      this.SetErrorId("Parse");
      this.SetErrorCategory(ErrorCategory.ParserError);
    }

    public ParseException(string message)
      : base(message)
    {
      this.SetErrorId("Parse");
      this.SetErrorCategory(ErrorCategory.ParserError);
    }

    internal ParseException(string message, string errorId)
      : base(message)
    {
      this.SetErrorId(errorId);
      this.SetErrorCategory(ErrorCategory.ParserError);
    }

    internal ParseException(string message, string errorId, Exception innerException)
      : base(message, innerException)
    {
      this.SetErrorId(errorId);
      this.SetErrorCategory(ErrorCategory.ParserError);
    }

    public ParseException(string message, Exception innerException)
      : base(message, innerException)
    {
      this.SetErrorId("Parse");
      this.SetErrorCategory(ErrorCategory.ParserError);
    }
  }
}
