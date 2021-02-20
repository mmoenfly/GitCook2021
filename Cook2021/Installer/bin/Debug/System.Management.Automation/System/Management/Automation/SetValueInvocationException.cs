// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SetValueInvocationException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class SetValueInvocationException : SetValueException
  {
    internal const string ExceptionWhenSettingMsg = "ExceptionWhenSetting";
    internal const string CannotSetNonManagementObjectMsg = "CannotSetNonManagementObject";

    public SetValueInvocationException()
      : base(typeof (SetValueInvocationException).FullName)
    {
    }

    public SetValueInvocationException(string message)
      : base(message)
    {
    }

    public SetValueInvocationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    internal SetValueInvocationException(
      string errorId,
      Exception innerException,
      string baseName,
      string resourceId,
      params object[] arguments)
      : base(errorId, innerException, baseName, resourceId, arguments)
    {
    }

    protected SetValueInvocationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
