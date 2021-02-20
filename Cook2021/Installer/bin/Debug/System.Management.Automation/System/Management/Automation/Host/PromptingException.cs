// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.PromptingException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation.Host
{
  [Serializable]
  public class PromptingException : HostException
  {
    public PromptingException()
      : base(ResourceManagerCache.FormatResourceString("HostInterfaceExceptionsStrings", "DefaultCtorMessageTemplate", (object) typeof (PromptingException).FullName))
      => this.SetDefaultErrorRecord();

    public PromptingException(string message)
      : base(message)
      => this.SetDefaultErrorRecord();

    public PromptingException(string message, Exception innerException)
      : base(message, innerException)
      => this.SetDefaultErrorRecord();

    public PromptingException(
      string message,
      Exception innerException,
      string errorId,
      ErrorCategory errorCategory)
      : base(message, innerException, errorId, errorCategory)
    {
    }

    protected PromptingException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    private void SetDefaultErrorRecord()
    {
      this.SetErrorCategory(ErrorCategory.ResourceUnavailable);
      this.SetErrorId(typeof (PromptingException).FullName);
    }
  }
}
