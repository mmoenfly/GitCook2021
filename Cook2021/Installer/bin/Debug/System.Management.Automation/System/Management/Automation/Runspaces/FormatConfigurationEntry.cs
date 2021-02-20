// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.FormatConfigurationEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class FormatConfigurationEntry : RunspaceConfigurationEntry
  {
    [TraceSource("FormatConfigurationEntry", "FormatConfigurationEntry")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (FormatConfigurationEntry), nameof (FormatConfigurationEntry));
    private string _fileName;

    public FormatConfigurationEntry(string name, string fileName)
      : base(name)
      => this._fileName = !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileName.Trim()) ? fileName.Trim() : throw FormatConfigurationEntry.tracer.NewArgumentException(nameof (fileName));

    internal FormatConfigurationEntry(string name, string fileName, PSSnapInInfo psSnapinInfo)
      : base(name, psSnapinInfo)
      => this._fileName = !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileName.Trim()) ? fileName.Trim() : throw FormatConfigurationEntry.tracer.NewArgumentException(nameof (fileName));

    public FormatConfigurationEntry(string fileName)
      : base(fileName)
      => this._fileName = !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileName.Trim()) ? fileName.Trim() : throw FormatConfigurationEntry.tracer.NewArgumentException(nameof (fileName));

    public string FileName => this._fileName;
  }
}
