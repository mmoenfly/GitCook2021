// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MetadataException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class MetadataException : RuntimeException
  {
    internal const string MetadataMemberInitialization = "MetadataMemberInitialization";
    internal const string BaseName = "Metadata";
    [TraceSource("Metadata", "Metadata Attributes")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("Metadata", "Metadata Attributes");

    protected MetadataException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.SetErrorCategory(ErrorCategory.MetadataError);
      MetadataException.tracer.TraceException((Exception) this);
    }

    public MetadataException()
      : base(typeof (MetadataException).FullName)
    {
      this.SetErrorCategory(ErrorCategory.MetadataError);
      MetadataException.tracer.TraceException((Exception) this);
    }

    public MetadataException(string message)
      : base(message)
    {
      this.SetErrorCategory(ErrorCategory.MetadataError);
      MetadataException.tracer.TraceException((Exception) this);
    }

    public MetadataException(string message, Exception innerException)
      : base(message, innerException)
    {
      this.SetErrorCategory(ErrorCategory.MetadataError);
      MetadataException.tracer.TraceException((Exception) this);
    }

    internal MetadataException(
      string errorId,
      Exception innerException,
      string baseName,
      string resourceId,
      params object[] arguments)
      : base(ResourceManagerCache.FormatResourceString(baseName, resourceId, arguments), innerException)
    {
      this.SetErrorCategory(ErrorCategory.MetadataError);
      this.SetErrorId(errorId);
      MetadataException.tracer.TraceException((Exception) this);
    }
  }
}
