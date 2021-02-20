// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DriveNotFoundException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class DriveNotFoundException : SessionStateException
  {
    internal DriveNotFoundException(string itemName, string errorIdAndResourceId)
      : base(itemName, SessionStateCategory.Drive, errorIdAndResourceId, ErrorCategory.ObjectNotFound)
    {
    }

    public DriveNotFoundException()
    {
    }

    public DriveNotFoundException(string message)
      : base(message)
    {
    }

    public DriveNotFoundException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected DriveNotFoundException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
