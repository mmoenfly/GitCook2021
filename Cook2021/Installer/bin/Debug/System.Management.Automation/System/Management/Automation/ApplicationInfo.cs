// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ApplicationInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public class ApplicationInfo : CommandInfo
  {
    [TraceSource("ApplicationInfo", "The command information for applications that are not directly executable by MSH.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ApplicationInfo), "The command information for applications that are not directly executable by MSH.");
    private ExecutionContext context;
    private string path = string.Empty;
    private string extension = string.Empty;
    private ReadOnlyCollection<PSTypeName> _outputType;

    internal ApplicationInfo(string name, string path, ExecutionContext context)
      : base(name, CommandTypes.Application)
    {
      using (ApplicationInfo.tracer.TraceConstructor((object) this))
      {
        if (string.IsNullOrEmpty(path))
          throw ApplicationInfo.tracer.NewArgumentException(nameof (path));
        if (context == null)
          throw ApplicationInfo.tracer.NewArgumentNullException(nameof (context));
        this.path = path;
        this.extension = System.IO.Path.GetExtension(path);
        this.context = context;
      }
    }

    public string Path
    {
      get
      {
        using (ApplicationInfo.tracer.TraceProperty(this.path, new object[0]))
          return this.path;
      }
    }

    public string Extension
    {
      get
      {
        using (ApplicationInfo.tracer.TraceProperty(this.extension, new object[0]))
          return this.extension;
      }
    }

    public override string Definition => this.Path;

    public override SessionStateEntryVisibility Visibility
    {
      get => this.context.EngineSessionState.CheckApplicationVisibility(this.path);
      set => throw ApplicationInfo.tracer.NewNotImplementedException();
    }

    public override ReadOnlyCollection<PSTypeName> OutputType
    {
      get
      {
        if (this._outputType == null)
          this._outputType = new ReadOnlyCollection<PSTypeName>((IList<PSTypeName>) new List<PSTypeName>()
          {
            new PSTypeName(typeof (string))
          });
        return this._outputType;
      }
    }
  }
}
