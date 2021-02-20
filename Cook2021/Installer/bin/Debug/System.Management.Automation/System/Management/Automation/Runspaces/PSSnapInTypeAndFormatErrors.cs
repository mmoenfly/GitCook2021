// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PSSnapInTypeAndFormatErrors
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
  internal class PSSnapInTypeAndFormatErrors
  {
    public string psSnapinName;
    private string fullPath;
    private FormatTable formatTable;
    private Collection<string> errors;

    internal PSSnapInTypeAndFormatErrors(string psSnapinName, string fullPath)
    {
      this.psSnapinName = psSnapinName;
      this.fullPath = fullPath;
      this.errors = new Collection<string>();
    }

    internal PSSnapInTypeAndFormatErrors(string psSnapinName, FormatTable formatTable)
    {
      this.psSnapinName = psSnapinName;
      this.formatTable = formatTable;
      this.errors = new Collection<string>();
    }

    internal string FullPath => this.fullPath;

    internal FormatTable FormatTable => this.formatTable;

    internal Collection<string> Errors
    {
      get => this.errors;
      set => this.errors = value;
    }

    internal string PSSnapinName => this.psSnapinName;
  }
}
