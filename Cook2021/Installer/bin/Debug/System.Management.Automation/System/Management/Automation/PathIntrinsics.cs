// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PathIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Provider;

namespace System.Management.Automation
{
  public sealed class PathIntrinsics
  {
    [TraceSource("PathCommandAPI", "The APIs that are exposed to the Cmdlet base class for manipulating location in session state")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PathCommandAPI", "The APIs that are exposed to the Cmdlet base class for manipulating location in session state");
    private LocationGlobber pathResolver;
    private SessionStateInternal sessionState;

    private PathIntrinsics()
    {
    }

    internal PathIntrinsics(SessionStateInternal sessionState)
    {
      using (PathIntrinsics.tracer.TraceConstructor((object) this))
        this.sessionState = sessionState != null ? sessionState : throw PathIntrinsics.tracer.NewArgumentNullException(nameof (sessionState));
    }

    public PathInfo CurrentLocation
    {
      get
      {
        using (PathIntrinsics.tracer.TraceProperty())
          return this.sessionState.CurrentLocation;
      }
    }

    public PathInfo CurrentProviderLocation(string providerName)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.GetNamespaceCurrentLocation(providerName);
    }

    public PathInfo CurrentFileSystemLocation
    {
      get
      {
        using (PathIntrinsics.tracer.TraceProperty())
          return this.CurrentProviderLocation(this.sessionState.ExecutionContext.ProviderNames.FileSystem);
      }
    }

    public PathInfo SetLocation(string path)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.SetLocation(path);
    }

    internal PathInfo SetLocation(string path, CmdletProviderContext context)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.SetLocation(path, context);
    }

    internal bool IsCurrentLocationOrAncestor(string path, CmdletProviderContext context)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.IsCurrentLocationOrAncestor(path, context);
    }

    public void PushCurrentLocation(string stackName)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        this.sessionState.PushCurrentLocation(stackName);
    }

    public PathInfo PopLocation(string stackName)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.PopLocation(stackName);
    }

    public PathInfoStack LocationStack(string stackName)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.LocationStack(stackName);
    }

    public PathInfoStack SetDefaultLocationStack(string stackName)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.SetDefaultLocationStack(stackName);
    }

    public Collection<PathInfo> GetResolvedPSPathFromPSPath(string path)
    {
      using (PathIntrinsics.tracer.TraceMethod())
      {
        CmdletProvider providerInstance = (CmdletProvider) null;
        return this.PathResolver.GetGlobbedMonadPathsFromMonadPath(path, false, out providerInstance);
      }
    }

    internal Collection<PathInfo> GetResolvedPSPathFromPSPath(
      string path,
      CmdletProviderContext context)
    {
      using (PathIntrinsics.tracer.TraceMethod())
      {
        CmdletProvider providerInstance = (CmdletProvider) null;
        return this.PathResolver.GetGlobbedMonadPathsFromMonadPath(path, false, context, out providerInstance);
      }
    }

    public Collection<string> GetResolvedProviderPathFromPSPath(
      string path,
      out ProviderInfo provider)
    {
      using (PathIntrinsics.tracer.TraceMethod())
      {
        CmdletProvider providerInstance = (CmdletProvider) null;
        return this.PathResolver.GetGlobbedProviderPathsFromMonadPath(path, false, out provider, out providerInstance);
      }
    }

    internal Collection<string> GetResolvedProviderPathFromPSPath(
      string path,
      CmdletProviderContext context,
      out ProviderInfo provider)
    {
      using (PathIntrinsics.tracer.TraceMethod())
      {
        CmdletProvider providerInstance = (CmdletProvider) null;
        return this.PathResolver.GetGlobbedProviderPathsFromMonadPath(path, false, context, out provider, out providerInstance);
      }
    }

    public Collection<string> GetResolvedProviderPathFromProviderPath(
      string path,
      string providerId)
    {
      using (PathIntrinsics.tracer.TraceMethod())
      {
        CmdletProvider providerInstance = (CmdletProvider) null;
        return this.PathResolver.GetGlobbedProviderPathsFromProviderPath(path, false, providerId, out providerInstance);
      }
    }

    internal Collection<string> GetResolvedProviderPathFromProviderPath(
      string path,
      string providerId,
      CmdletProviderContext context)
    {
      using (PathIntrinsics.tracer.TraceMethod())
      {
        CmdletProvider providerInstance = (CmdletProvider) null;
        return this.PathResolver.GetGlobbedProviderPathsFromProviderPath(path, false, providerId, context, out providerInstance);
      }
    }

    public string GetUnresolvedProviderPathFromPSPath(string path)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.PathResolver.GetProviderPath(path);
    }

    public string GetUnresolvedProviderPathFromPSPath(
      string path,
      out ProviderInfo provider,
      out PSDriveInfo drive)
    {
      using (PathIntrinsics.tracer.TraceMethod())
      {
        CmdletProviderContext context = new CmdletProviderContext(this.sessionState.ExecutionContext);
        string providerPath = this.PathResolver.GetProviderPath(path, context, out provider, out drive);
        context.ThrowFirstErrorOrDoNothing();
        return providerPath;
      }
    }

    internal string GetUnresolvedProviderPathFromPSPath(
      string path,
      CmdletProviderContext context,
      out ProviderInfo provider,
      out PSDriveInfo drive)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.PathResolver.GetProviderPath(path, context, out provider, out drive);
    }

    public bool IsProviderQualified(string path)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return LocationGlobber.IsProviderQualifiedPath(path);
    }

    public bool IsPSAbsolute(string path, out string driveName)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.PathResolver.IsAbsolutePath(path, out driveName);
    }

    public string Combine(string parent, string child)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.MakePath(parent, child);
    }

    internal string Combine(string parent, string child, CmdletProviderContext context)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.MakePath(parent, child, context);
    }

    public string ParseParent(string path, string root)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.GetParentPath(path, root);
    }

    internal string ParseParent(string path, string root, CmdletProviderContext context)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.GetParentPath(path, root, context);
    }

    public string ParseChildName(string path)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.GetChildName(path);
    }

    internal string ParseChildName(string path, CmdletProviderContext context)
    {
      using (PathIntrinsics.tracer.TraceMethod())
        return this.sessionState.GetChildName(path, context);
    }

    public string NormalizeRelativePath(string path, string basePath)
    {
      using (PathIntrinsics.tracer.TraceMethod(path, new object[0]))
        return this.sessionState.NormalizeRelativePath(path, basePath);
    }

    internal string NormalizeRelativePath(
      string path,
      string basePath,
      CmdletProviderContext context)
    {
      using (PathIntrinsics.tracer.TraceMethod(path, new object[0]))
        return this.sessionState.NormalizeRelativePath(path, basePath, context);
    }

    public bool IsValid(string path)
    {
      using (PathIntrinsics.tracer.TraceMethod(path, new object[0]))
        return this.sessionState.IsValidPath(path);
    }

    internal bool IsValid(string path, CmdletProviderContext context)
    {
      using (PathIntrinsics.tracer.TraceMethod(path, new object[0]))
        return this.sessionState.IsValidPath(path, context);
    }

    private LocationGlobber PathResolver
    {
      get
      {
        using (PathIntrinsics.tracer.TraceProperty())
        {
          if (this.pathResolver == null)
            this.pathResolver = this.sessionState.ExecutionContext.LocationGlobber;
          return this.pathResolver;
        }
      }
    }
  }
}
