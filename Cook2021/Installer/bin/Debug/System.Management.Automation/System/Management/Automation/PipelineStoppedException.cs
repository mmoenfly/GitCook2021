﻿// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PipelineStoppedException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class PipelineStoppedException : RuntimeException
  {
    public PipelineStoppedException()
      : base(ResourceManagerCache.GetResourceString("GetErrorText", nameof (PipelineStoppedException)))
    {
      this.SetErrorId("PipelineStopped");
      this.SetErrorCategory(ErrorCategory.OperationStopped);
    }

    protected PipelineStoppedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public PipelineStoppedException(string message)
      : base(message)
    {
    }

    public PipelineStoppedException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}