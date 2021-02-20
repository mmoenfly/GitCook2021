// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParsingMetadataException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class ParsingMetadataException : MetadataException
  {
    internal const string ParsingTooManyParameterSets = "ParsingTooManyParameterSets";

    protected ParsingMetadataException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public ParsingMetadataException()
      : base(typeof (ParsingMetadataException).FullName)
    {
    }

    public ParsingMetadataException(string message)
      : base(message)
    {
    }

    public ParsingMetadataException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    internal ParsingMetadataException(
      string errorId,
      Exception innerException,
      string baseName,
      string resourceId,
      params object[] arguments)
      : base(errorId, innerException, baseName, resourceId, arguments)
    {
    }
  }
}
