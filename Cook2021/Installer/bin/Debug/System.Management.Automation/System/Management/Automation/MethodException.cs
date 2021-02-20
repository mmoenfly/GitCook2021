// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MethodException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class MethodException : ExtendedTypeSystemException
  {
    internal const string MethodArgumentCountExceptionMsg = "MethodArgumentCountException";
    internal const string MethodAmbiguousExceptionMsg = "MethodAmbiguousException";
    internal const string MethodArgumentConversionExceptionMsg = "MethodArgumentConversionException";
    internal const string NonRefArgumentToRefParameterMsg = "NonRefArgumentToRefParameter";
    internal const string RefArgumentToNonRefParameterMsg = "RefArgumentToNonRefParameter";

    public MethodException()
      : base(typeof (MethodException).FullName)
    {
    }

    public MethodException(string message)
      : base(message)
    {
    }

    public MethodException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    internal MethodException(
      string errorId,
      Exception innerException,
      string baseName,
      string resourceId,
      params object[] arguments)
      : base(errorId, innerException, baseName, resourceId, arguments)
    {
    }

    protected MethodException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
