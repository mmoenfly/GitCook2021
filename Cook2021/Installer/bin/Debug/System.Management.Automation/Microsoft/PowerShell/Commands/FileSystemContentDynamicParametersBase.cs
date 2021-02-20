// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.FileSystemContentDynamicParametersBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  public class FileSystemContentDynamicParametersBase
  {
    [TraceSource("FileSystemProvider", "The namespace navigation provider for the file system")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("FileSystemProvider", "The namespace navigation provider for the file system");
    private FileSystemCmdletProviderEncoding streamType = FileSystemCmdletProviderEncoding.String;

    [Parameter]
    public FileSystemCmdletProviderEncoding Encoding
    {
      get => this.streamType;
      set => this.streamType = value;
    }

    public System.Text.Encoding EncodingType => FileSystemContentDynamicParametersBase.GetEncodingFromEnum(this.streamType);

    private static System.Text.Encoding GetEncodingFromEnum(
      FileSystemCmdletProviderEncoding type)
    {
      System.Text.Encoding unicode = System.Text.Encoding.Unicode;
      System.Text.Encoding encoding;
      switch (type)
      {
        case FileSystemCmdletProviderEncoding.String:
          encoding = System.Text.Encoding.Unicode;
          break;
        case FileSystemCmdletProviderEncoding.Unicode:
          encoding = System.Text.Encoding.Unicode;
          break;
        case FileSystemCmdletProviderEncoding.BigEndianUnicode:
          encoding = System.Text.Encoding.BigEndianUnicode;
          break;
        case FileSystemCmdletProviderEncoding.UTF8:
          encoding = System.Text.Encoding.UTF8;
          break;
        case FileSystemCmdletProviderEncoding.UTF7:
          encoding = System.Text.Encoding.UTF7;
          break;
        case FileSystemCmdletProviderEncoding.Ascii:
          encoding = System.Text.Encoding.ASCII;
          break;
        default:
          encoding = System.Text.Encoding.Unicode;
          break;
      }
      return encoding;
    }

    public bool UsingByteEncoding
    {
      get
      {
        bool flag = this.streamType == FileSystemCmdletProviderEncoding.Byte;
        FileSystemContentDynamicParametersBase.tracer.WriteLine("result = {0}", (object) flag);
        return flag;
      }
    }

    public bool WasStreamTypeSpecified
    {
      get
      {
        bool flag = this.streamType != FileSystemCmdletProviderEncoding.String;
        FileSystemContentDynamicParametersBase.tracer.WriteLine("result = {0}", (object) flag);
        return flag;
      }
    }
  }
}
