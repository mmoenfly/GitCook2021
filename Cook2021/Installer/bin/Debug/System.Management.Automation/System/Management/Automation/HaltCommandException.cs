// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HaltCommandException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class HaltCommandException : SystemException
  {
    public HaltCommandException()
      : base(ResourceManagerCache.FormatResourceString("AutomationExceptions", nameof (HaltCommandException)))
    {
    }

    public HaltCommandException(string message)
      : base(message)
    {
    }

    public HaltCommandException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected HaltCommandException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
