// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.FileSystemContentReaderDynamicParameters
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  public class FileSystemContentReaderDynamicParameters : FileSystemContentDynamicParametersBase
  {
    [TraceSource("FileSystemProvider", "The namespace navigation provider for the file system")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("FileSystemProvider", "The namespace navigation provider for the file system");
    private string delimiter = "\n";
    private bool wait;
    private bool delimiterSpecified;

    [Parameter]
    public string Delimiter
    {
      get => this.delimiter;
      set
      {
        this.delimiterSpecified = true;
        this.delimiter = value;
      }
    }

    [Parameter]
    public SwitchParameter Wait
    {
      get => (SwitchParameter) this.wait;
      set => this.wait = (bool) value;
    }

    public bool DelimiterSpecified => this.delimiterSpecified;
  }
}
