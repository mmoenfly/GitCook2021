// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.IncompleteParseException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class IncompleteParseException : ParseException
  {
    private const string errorIdString = "IncompleteParse";

    protected IncompleteParseException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public IncompleteParseException() => this.SetErrorId("IncompleteParse");

    public IncompleteParseException(string message)
      : base(message)
      => this.SetErrorId("IncompleteParse");

    internal IncompleteParseException(string message, string errorId)
      : base(message, errorId)
    {
    }

    internal IncompleteParseException(string message, string errorId, Exception innerException)
      : base(message, errorId, innerException)
    {
    }

    public IncompleteParseException(string message, Exception innerException)
      : base(message, innerException)
      => this.SetErrorId("IncompleteParse");
  }
}
