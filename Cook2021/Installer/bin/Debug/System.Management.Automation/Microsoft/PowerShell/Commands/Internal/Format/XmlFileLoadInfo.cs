// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.XmlFileLoadInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class XmlFileLoadInfo
  {
    internal string fileDirectory;
    internal string filePath;
    internal Collection<string> errors;
    internal string psSnapinName;

    internal XmlFileLoadInfo()
    {
    }

    internal XmlFileLoadInfo(
      string dir,
      string path,
      Collection<string> errors,
      string psSnapinName)
    {
      this.fileDirectory = dir;
      this.filePath = path;
      this.errors = errors;
      this.psSnapinName = psSnapinName;
    }
  }
}
