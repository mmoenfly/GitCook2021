// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.AssemblyConfigurationEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class AssemblyConfigurationEntry : RunspaceConfigurationEntry
  {
    private string _fileName;
    [TraceSource("AssemblyConfigurationEntry", "AssemblyConfigurationEntry")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (AssemblyConfigurationEntry), nameof (AssemblyConfigurationEntry));

    public AssemblyConfigurationEntry(string name, string fileName)
      : base(name)
    {
      using (AssemblyConfigurationEntry.tracer.TraceConstructor((object) this))
        this._fileName = !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileName.Trim()) ? fileName.Trim() : throw AssemblyConfigurationEntry.tracer.NewArgumentNullException(nameof (fileName));
    }

    internal AssemblyConfigurationEntry(string name, string fileName, PSSnapInInfo psSnapinInfo)
      : base(name, psSnapinInfo)
    {
      using (AssemblyConfigurationEntry.tracer.TraceConstructor((object) this))
        this._fileName = !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileName.Trim()) ? fileName.Trim() : throw AssemblyConfigurationEntry.tracer.NewArgumentNullException(nameof (fileName));
    }

    public string FileName => this._fileName;
  }
}
