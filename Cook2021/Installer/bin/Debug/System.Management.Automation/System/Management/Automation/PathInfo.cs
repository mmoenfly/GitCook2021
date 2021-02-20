// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PathInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public sealed class PathInfo
  {
    [TraceSource("PathInfo", "An object that represents a path in Monad.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PathInfo), "An object that represents a path in Monad.");
    private string providerPath;
    private SessionState sessionState;
    private PSDriveInfo drive;
    private ProviderInfo provider;
    private string path = string.Empty;

    public PSDriveInfo Drive
    {
      get
      {
        using (PathInfo.tracer.TraceProperty())
        {
          PSDriveInfo psDriveInfo = (PSDriveInfo) null;
          if (this.drive != (PSDriveInfo) null && !this.drive.Hidden)
            psDriveInfo = this.drive;
          return psDriveInfo;
        }
      }
    }

    public ProviderInfo Provider
    {
      get
      {
        using (PathInfo.tracer.TraceProperty())
          return this.provider;
      }
    }

    internal PSDriveInfo GetDrive()
    {
      using (PathInfo.tracer.TraceMethod())
        return this.drive;
    }

    public string ProviderPath
    {
      get
      {
        using (PathInfo.tracer.TraceProperty())
        {
          if (this.providerPath == null)
            this.providerPath = this.sessionState.Internal.ExecutionContext.LocationGlobber.GetProviderPath(this.Path);
          PathInfo.tracer.WriteLine("result = {0}", (object) this.providerPath);
          return this.providerPath;
        }
      }
    }

    public string Path
    {
      get
      {
        using (PathInfo.tracer.TraceProperty((object) this))
          return this.ToString();
      }
    }

    public override string ToString()
    {
      using (PathInfo.tracer.TraceMethod(this.path, new object[0]))
      {
        string path = this.path;
        string str = this.drive == (PSDriveInfo) null || this.drive.Hidden ? LocationGlobber.GetProviderQualifiedPath(this.path, this.provider) : LocationGlobber.GetDriveQualifiedPath(this.path, this.drive);
        PathInfo.tracer.WriteLine("result = {0}", (object) str);
        return str;
      }
    }

    internal PathInfo(
      PSDriveInfo drive,
      ProviderInfo provider,
      string path,
      SessionState sessionState)
    {
      using (PathInfo.tracer.TraceConstructor((object) this))
      {
        if (provider == null)
          throw PathInfo.tracer.NewArgumentNullException(nameof (provider));
        if (path == null)
          throw PathInfo.tracer.NewArgumentNullException(nameof (path));
        if (sessionState == null)
          throw PathInfo.tracer.NewArgumentNullException(nameof (sessionState));
        this.drive = drive;
        this.provider = provider;
        this.path = path;
        this.sessionState = sessionState;
      }
    }
  }
}
