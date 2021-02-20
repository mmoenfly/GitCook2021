// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptBlockToPowerShellNotSupportedException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class ScriptBlockToPowerShellNotSupportedException : RuntimeException
  {
    [TraceSource("ScriptBlockToPowerShellNotSupportedException", "Traces exceptions thrown when conversion from ScriptBlock to PowerShell is forbidden")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ScriptBlockToPowerShellNotSupportedException), "Traces exceptions thrown when conversion from ScriptBlock to PowerShell is forbidden");

    public ScriptBlockToPowerShellNotSupportedException()
      : base(typeof (ScriptBlockToPowerShellNotSupportedException).FullName)
    {
    }

    public ScriptBlockToPowerShellNotSupportedException(string message)
      : base(message)
    {
    }

    public ScriptBlockToPowerShellNotSupportedException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    internal ScriptBlockToPowerShellNotSupportedException(
      string errorId,
      Exception innerException,
      string baseName,
      string resourceId,
      params object[] arguments)
      : base(ResourceManagerCache.FormatResourceString(baseName, resourceId, arguments), innerException)
    {
      this.SetErrorId(errorId);
      ScriptBlockToPowerShellNotSupportedException.tracer.TraceException((Exception) this);
    }

    protected ScriptBlockToPowerShellNotSupportedException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    {
    }
  }
}
