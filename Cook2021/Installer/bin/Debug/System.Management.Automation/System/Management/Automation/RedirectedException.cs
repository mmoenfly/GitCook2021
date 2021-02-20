// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RedirectedException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class RedirectedException : RuntimeException
  {
    public RedirectedException()
    {
      this.SetErrorId(nameof (RedirectedException));
      this.SetErrorCategory(ErrorCategory.NotSpecified);
    }

    public RedirectedException(string message)
      : base(message)
    {
      this.SetErrorId(nameof (RedirectedException));
      this.SetErrorCategory(ErrorCategory.NotSpecified);
    }

    public RedirectedException(string message, Exception innerException)
      : base(message, innerException)
    {
      this.SetErrorId(nameof (RedirectedException));
      this.SetErrorCategory(ErrorCategory.NotSpecified);
    }

    protected RedirectedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
