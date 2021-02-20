// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.LocationGlobber
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Management.Automation.Provider;
using System.Text;

namespace System.Management.Automation
{
  internal sealed class LocationGlobber
  {
    [TraceSource("LocationGlobber", "The location globber converts PowerShell paths with glob characters to zero or more paths.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (LocationGlobber), "The location globber converts PowerShell paths with glob characters to zero or more paths.");
    [TraceSource("PathResolution", "Traces the path resolution algorithm.")]
    private static PSTraceSource pathResolutionTracer = PSTraceSource.GetTracer("PathResolution", "Traces the path resolution algorithm.", false);
    private SessionState sessionState;

    internal LocationGlobber(SessionState sessionState) => this.sessionState = sessionState != null ? sessionState : throw LocationGlobber.tracer.NewArgumentNullException(nameof (sessionState));

    internal Collection<PathInfo> GetGlobbedMonadPathsFromMonadPath(
      string path,
      bool allowNonexistingPaths,
      out CmdletProvider providerInstance)
    {
      CmdletProviderContext context = new CmdletProviderContext(this.sessionState.Internal.ExecutionContext);
      return this.GetGlobbedMonadPathsFromMonadPath(path, allowNonexistingPaths, context, out providerInstance);
    }

    internal Collection<PathInfo> GetGlobbedMonadPathsFromMonadPath(
      string path,
      bool allowNonexistingPaths,
      CmdletProviderContext context,
      out CmdletProvider providerInstance)
    {
      providerInstance = (CmdletProvider) null;
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      if (context == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (context));
      Collection<PathInfo> collection = new Collection<PathInfo>();
      using (LocationGlobber.pathResolutionTracer.TraceScope("Resolving MSH path \"{0}\" to MSH path", (object) path))
      {
        LocationGlobber.TraceFilters(context);
        if (LocationGlobber.IsHomePath(path))
        {
          using (LocationGlobber.pathResolutionTracer.TraceScope("Resolving HOME relative path."))
            path = this.GetHomeRelativePath(path);
        }
        bool isProviderDirectPath = LocationGlobber.IsProviderDirectPath(path);
        bool isProviderQualifiedPath = LocationGlobber.IsProviderQualifiedPath(path);
        collection = isProviderDirectPath || isProviderQualifiedPath ? this.ResolvePSPathFromProviderPath(path, context, allowNonexistingPaths, isProviderDirectPath, isProviderQualifiedPath, out providerInstance) : this.ResolveDriveQualifiedPath(path, context, allowNonexistingPaths, out providerInstance);
        if (!allowNonexistingPaths)
        {
          if (collection.Count < 1)
          {
            if (!WildcardPattern.ContainsWildcardCharacters(path))
            {
              if (context.Include.Count == 0)
              {
                if (context.Exclude.Count == 0)
                {
                  ItemNotFoundException notFoundException = new ItemNotFoundException(path, "PathNotFound");
                  LocationGlobber.tracer.TraceException((Exception) notFoundException);
                  LocationGlobber.pathResolutionTracer.TraceError("Item does not exist: {0}", (object) path);
                  throw notFoundException;
                }
              }
            }
          }
        }
      }
      return collection;
    }

    private Collection<string> ResolveProviderPathFromProviderPath(
      string providerPath,
      string providerId,
      bool allowNonexistingPaths,
      CmdletProviderContext context,
      out CmdletProvider providerInstance)
    {
      providerInstance = this.sessionState.Internal.GetProviderInstance(providerId);
      ContainerCmdletProvider containerProvider = providerInstance as ContainerCmdletProvider;
      ItemCmdletProvider itemCmdletProvider = providerInstance as ItemCmdletProvider;
      Collection<string> collection = new Collection<string>();
      if (!context.SuppressWildcardExpansion)
      {
        if (CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.ExpandWildcards, providerInstance.ProviderInfo))
        {
          LocationGlobber.pathResolutionTracer.WriteLine("Wildcard matching is being performed by the provider.", new object[0]);
          if (itemCmdletProvider != null && WildcardPattern.ContainsWildcardCharacters(providerPath))
            collection = new Collection<string>((IList<string>) itemCmdletProvider.ExpandPath(providerPath, context));
          else
            collection.Add(providerPath);
        }
        else
        {
          LocationGlobber.pathResolutionTracer.WriteLine("Wildcard matching is being performed by the engine.", new object[0]);
          if (containerProvider != null)
            collection = this.GetGlobbedProviderPathsFromProviderPath(providerPath, allowNonexistingPaths, containerProvider, context);
          else
            collection.Add(providerPath);
        }
      }
      else if (itemCmdletProvider != null)
      {
        if (itemCmdletProvider.ItemExists(providerPath, context))
          collection.Add(providerPath);
      }
      else
        collection.Add(providerPath);
      if (!allowNonexistingPaths && collection.Count < 1 && (!WildcardPattern.ContainsWildcardCharacters(providerPath) && context.Include.Count == 0) && context.Exclude.Count == 0)
      {
        ItemNotFoundException notFoundException = new ItemNotFoundException(providerPath, "PathNotFound");
        LocationGlobber.tracer.TraceException((Exception) notFoundException);
        LocationGlobber.pathResolutionTracer.TraceError("Item does not exist: {0}", (object) providerPath);
        throw notFoundException;
      }
      return collection;
    }

    private Collection<PathInfo> ResolvePSPathFromProviderPath(
      string path,
      CmdletProviderContext context,
      bool allowNonexistingPaths,
      bool isProviderDirectPath,
      bool isProviderQualifiedPath,
      out CmdletProvider providerInstance)
    {
      Collection<PathInfo> collection1 = new Collection<PathInfo>();
      providerInstance = (CmdletProvider) null;
      string providerId = (string) null;
      string providerPath = (string) null;
      if (isProviderDirectPath)
      {
        LocationGlobber.pathResolutionTracer.WriteLine("Path is PROVIDER-DIRECT", new object[0]);
        providerPath = path;
        providerId = this.sessionState.Path.CurrentLocation.Provider.Name;
      }
      else if (isProviderQualifiedPath)
      {
        LocationGlobber.pathResolutionTracer.WriteLine("Path is PROVIDER-QUALIFIED", new object[0]);
        providerPath = LocationGlobber.ParseProviderPath(path, out providerId);
      }
      LocationGlobber.pathResolutionTracer.WriteLine("PROVIDER-INTERNAL path: {0}", (object) providerPath);
      LocationGlobber.pathResolutionTracer.WriteLine("Provider: {0}", (object) providerId);
      Collection<string> collection2 = this.ResolveProviderPathFromProviderPath(providerPath, providerId, allowNonexistingPaths, context, out providerInstance);
      PSDriveInfo hiddenDrive = providerInstance.ProviderInfo.HiddenDrive;
      foreach (string path1 in collection2)
      {
        if (context.Stopping)
          throw new PipelineStoppedException();
        string path2;
        if (LocationGlobber.IsProviderDirectPath(path1))
          path2 = path1;
        else
          path2 = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}::{1}", (object) providerId, (object) path1);
        collection1.Add(new PathInfo(hiddenDrive, providerInstance.ProviderInfo, path2, this.sessionState));
        LocationGlobber.pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", (object) path2);
      }
      return collection1;
    }

    private Collection<PathInfo> ResolveDriveQualifiedPath(
      string path,
      CmdletProviderContext context,
      bool allowNonexistingPaths,
      out CmdletProvider providerInstance)
    {
      providerInstance = (CmdletProvider) null;
      PSDriveInfo workingDriveForPath = (PSDriveInfo) null;
      Collection<PathInfo> collection1 = new Collection<PathInfo>();
      LocationGlobber.pathResolutionTracer.WriteLine("Path is DRIVE-QUALIFIED", new object[0]);
      string relativePathFromPsPath = this.GetDriveRootRelativePathFromPSPath(path, context, true, out workingDriveForPath, out providerInstance);
      LocationGlobber.pathResolutionTracer.WriteLine("DRIVE-RELATIVE path: {0}", (object) relativePathFromPsPath);
      LocationGlobber.pathResolutionTracer.WriteLine("Drive: {0}", (object) workingDriveForPath.Name);
      LocationGlobber.pathResolutionTracer.WriteLine("Provider: {0}", (object) workingDriveForPath.Provider);
      context.Drive = workingDriveForPath;
      providerInstance = (CmdletProvider) this.sessionState.Internal.GetContainerProviderInstance(workingDriveForPath.Provider);
      ContainerCmdletProvider provider = providerInstance as ContainerCmdletProvider;
      ItemCmdletProvider itemCmdletProvider = providerInstance as ItemCmdletProvider;
      ProviderInfo providerInfo = providerInstance.ProviderInfo;
      string str1 = (string) null;
      string str2;
      string str3;
      if (workingDriveForPath.Hidden)
      {
        str2 = LocationGlobber.GetProviderQualifiedPath(relativePathFromPsPath, providerInfo);
        str3 = relativePathFromPsPath;
      }
      else
      {
        str2 = LocationGlobber.GetDriveQualifiedPath(relativePathFromPsPath, workingDriveForPath);
        str3 = this.GetProviderPath(path, context);
      }
      LocationGlobber.pathResolutionTracer.WriteLine("PROVIDER path: {0}", (object) str3);
      Collection<string> collection2 = new Collection<string>();
      if (!context.SuppressWildcardExpansion)
      {
        if (CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.ExpandWildcards, providerInfo))
        {
          LocationGlobber.pathResolutionTracer.WriteLine("Wildcard matching is being performed by the provider.", new object[0]);
          if (itemCmdletProvider != null && WildcardPattern.ContainsWildcardCharacters(relativePathFromPsPath))
          {
            foreach (string providerPath in itemCmdletProvider.ExpandPath(str3, context))
              collection2.Add(this.GetDriveRootRelativePathFromProviderPath(providerPath, workingDriveForPath, context));
          }
          else
            collection2.Add(this.GetDriveRootRelativePathFromProviderPath(str3, workingDriveForPath, context));
        }
        else
        {
          LocationGlobber.pathResolutionTracer.WriteLine("Wildcard matching is being performed by the engine.", new object[0]);
          collection2 = this.ExpandMshGlobPath(relativePathFromPsPath, allowNonexistingPaths, workingDriveForPath, provider, context);
        }
      }
      else if (itemCmdletProvider != null)
      {
        if (allowNonexistingPaths || itemCmdletProvider.ItemExists(str3, context))
          collection2.Add(str2);
      }
      else
        collection2.Add(str2);
      if (!allowNonexistingPaths && collection2.Count < 1 && (!WildcardPattern.ContainsWildcardCharacters(path) && context.Include.Count == 0) && context.Exclude.Count == 0)
      {
        ItemNotFoundException notFoundException = new ItemNotFoundException(path, "PathNotFound");
        LocationGlobber.tracer.TraceException((Exception) notFoundException);
        LocationGlobber.pathResolutionTracer.TraceError("Item does not exist: {0}", (object) path);
        throw notFoundException;
      }
      foreach (string path1 in collection2)
      {
        if (context.Stopping)
          throw new PipelineStoppedException();
        str1 = (string) null;
        string path2 = !workingDriveForPath.Hidden ? LocationGlobber.GetDriveQualifiedPath(path1, workingDriveForPath) : (!LocationGlobber.IsProviderDirectPath(path1) ? LocationGlobber.GetProviderQualifiedPath(path1, providerInfo) : path1);
        collection1.Add(new PathInfo(workingDriveForPath, providerInfo, path2, this.sessionState));
        LocationGlobber.pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", (object) path2);
      }
      return collection1;
    }

    internal Collection<string> GetGlobbedProviderPathsFromMonadPath(
      string path,
      bool allowNonexistingPaths,
      out ProviderInfo provider,
      out CmdletProvider providerInstance)
    {
      providerInstance = (CmdletProvider) null;
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.sessionState.Internal.ExecutionContext);
      return this.GetGlobbedProviderPathsFromMonadPath(path, allowNonexistingPaths, context, out provider, out providerInstance);
    }

    internal Collection<string> GetGlobbedProviderPathsFromMonadPath(
      string path,
      bool allowNonexistingPaths,
      CmdletProviderContext context,
      out ProviderInfo provider,
      out CmdletProvider providerInstance)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      if (context == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (context));
      using (LocationGlobber.pathResolutionTracer.TraceScope("Resolving MSH path \"{0}\" to PROVIDER-INTERNAL path", (object) path))
      {
        LocationGlobber.TraceFilters(context);
        if (LocationGlobber.IsProviderQualifiedPath(path))
          context.Drive = (PSDriveInfo) null;
        PSDriveInfo drive = (PSDriveInfo) null;
        if (this.GetProviderPath(path, context, out provider, out drive) == null)
        {
          providerInstance = (CmdletProvider) null;
          LocationGlobber.tracer.WriteLine("provider returned a null path so return an empty array", new object[0]);
          LocationGlobber.pathResolutionTracer.WriteLine("Provider '{0}' returned null", (object) provider);
          return new Collection<string>();
        }
        if (drive != (PSDriveInfo) null)
          context.Drive = drive;
        Collection<string> collection = new Collection<string>();
        foreach (PathInfo pathInfo in this.GetGlobbedMonadPathsFromMonadPath(path, allowNonexistingPaths, context, out providerInstance))
          collection.Add(pathInfo.ProviderPath);
        return collection;
      }
    }

    internal Collection<string> GetGlobbedProviderPathsFromProviderPath(
      string path,
      bool allowNonexistingPaths,
      string providerId,
      out CmdletProvider providerInstance)
    {
      providerInstance = (CmdletProvider) null;
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.sessionState.Internal.ExecutionContext);
      Collection<string> fromProviderPath = this.GetGlobbedProviderPathsFromProviderPath(path, allowNonexistingPaths, providerId, context, out providerInstance);
      if (context.HasErrors())
      {
        ErrorRecord accumulatedErrorObject = context.GetAccumulatedErrorObjects()[0];
        if (accumulatedErrorObject != null)
        {
          LocationGlobber.tracer.TraceException(accumulatedErrorObject.Exception);
          throw accumulatedErrorObject.Exception;
        }
      }
      return fromProviderPath;
    }

    internal Collection<string> GetGlobbedProviderPathsFromProviderPath(
      string path,
      bool allowNonexistingPaths,
      string providerId,
      CmdletProviderContext context,
      out CmdletProvider providerInstance)
    {
      providerInstance = (CmdletProvider) null;
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      if (providerId == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (providerId));
      if (context == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (context));
      using (LocationGlobber.pathResolutionTracer.TraceScope("Resolving PROVIDER-INTERNAL path \"{0}\" to PROVIDER-INTERNAL path", (object) path))
      {
        LocationGlobber.TraceFilters(context);
        return this.ResolveProviderPathFromProviderPath(path, providerId, allowNonexistingPaths, context, out providerInstance);
      }
    }

    internal string GetProviderPath(string path)
    {
      ProviderInfo provider = (ProviderInfo) null;
      return this.GetProviderPath(path, out provider);
    }

    internal string GetProviderPath(string path, out ProviderInfo provider)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      CmdletProviderContext context = new CmdletProviderContext(this.sessionState.Internal.ExecutionContext);
      PSDriveInfo drive = (PSDriveInfo) null;
      provider = (ProviderInfo) null;
      string providerPath = this.GetProviderPath(path, context, out provider, out drive);
      if (context.HasErrors())
      {
        Collection<ErrorRecord> accumulatedErrorObjects = context.GetAccumulatedErrorObjects();
        if (accumulatedErrorObjects != null && accumulatedErrorObjects.Count > 0)
        {
          LocationGlobber.tracer.TraceException(accumulatedErrorObjects[0].Exception);
          throw accumulatedErrorObjects[0].Exception;
        }
      }
      return providerPath;
    }

    internal string GetProviderPath(string path, CmdletProviderContext context)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      PSDriveInfo drive = (PSDriveInfo) null;
      ProviderInfo provider = (ProviderInfo) null;
      return this.GetProviderPath(path, context, out provider, out drive);
    }

    internal string GetProviderPath(
      string path,
      CmdletProviderContext context,
      out ProviderInfo provider,
      out PSDriveInfo drive)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      if (context == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (context));
      provider = (ProviderInfo) null;
      drive = (PSDriveInfo) null;
      if (LocationGlobber.IsHomePath(path))
      {
        using (LocationGlobber.pathResolutionTracer.TraceScope("Resolving HOME relative path."))
          path = this.GetHomeRelativePath(path);
      }
      string str;
      if (LocationGlobber.IsProviderDirectPath(path))
      {
        LocationGlobber.pathResolutionTracer.WriteLine("Path is PROVIDER-DIRECT", new object[0]);
        str = path;
        drive = (PSDriveInfo) null;
        provider = this.sessionState.Path.CurrentLocation.Provider;
        LocationGlobber.pathResolutionTracer.WriteLine("PROVIDER-INTERNAL path: {0}", (object) str);
        LocationGlobber.pathResolutionTracer.WriteLine("Provider: {0}", (object) provider);
      }
      else if (LocationGlobber.IsProviderQualifiedPath(path))
      {
        LocationGlobber.pathResolutionTracer.WriteLine("Path is PROVIDER-QUALIFIED", new object[0]);
        string providerId = (string) null;
        str = LocationGlobber.ParseProviderPath(path, out providerId);
        drive = (PSDriveInfo) null;
        provider = this.sessionState.Internal.GetSingleProvider(providerId);
        LocationGlobber.pathResolutionTracer.WriteLine("PROVIDER-INTERNAL path: {0}", (object) str);
        LocationGlobber.pathResolutionTracer.WriteLine("Provider: {0}", (object) provider);
      }
      else
      {
        LocationGlobber.pathResolutionTracer.WriteLine("Path is DRIVE-QUALIFIED", new object[0]);
        CmdletProvider providerInstance = (CmdletProvider) null;
        string relativePathFromPsPath = this.GetDriveRootRelativePathFromPSPath(path, context, false, out drive, out providerInstance);
        LocationGlobber.pathResolutionTracer.WriteLine("DRIVE-RELATIVE path: {0}", (object) relativePathFromPsPath);
        LocationGlobber.pathResolutionTracer.WriteLine("Drive: {0}", (object) drive.Name);
        LocationGlobber.pathResolutionTracer.WriteLine("Provider: {0}", (object) drive.Provider);
        context.Drive = drive;
        str = !drive.Hidden ? this.GetProviderSpecificPath(drive, relativePathFromPsPath, context) : relativePathFromPsPath;
        provider = drive.Provider;
      }
      LocationGlobber.tracer.WriteLine("result = {0}", (object) str);
      LocationGlobber.pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", (object) str);
      return str;
    }

    internal static bool IsProviderQualifiedPath(string path)
    {
      string providerId = (string) null;
      return LocationGlobber.IsProviderQualifiedPath(path, out providerId);
    }

    internal static bool IsProviderQualifiedPath(string path, out string providerId)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      providerId = (string) null;
      bool flag = false;
      if (path.Length == 0)
        flag = false;
      else if (path.StartsWith(".\\", StringComparison.Ordinal) || path.StartsWith("./", StringComparison.Ordinal))
      {
        flag = false;
      }
      else
      {
        int length = path.IndexOf(':');
        if (length == -1 || length + 1 >= path.Length || path[length + 1] != ':')
          flag = false;
        else if (length > 0)
        {
          flag = true;
          providerId = path.Substring(0, length);
          LocationGlobber.tracer.WriteLine("providerId = {0}", (object) providerId);
        }
      }
      LocationGlobber.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal static bool IsAbsolutePath(string path)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      bool flag = false;
      if (path.Length == 0)
        flag = false;
      else if (path.StartsWith(".\\", StringComparison.Ordinal))
      {
        flag = false;
      }
      else
      {
        int num = path.IndexOf(":", StringComparison.Ordinal);
        if (num == -1)
          flag = false;
        else if (num > 0)
          flag = true;
      }
      LocationGlobber.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool IsAbsolutePath(string path, out string driveName)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      bool flag = false;
      driveName = !(this.sessionState.Drive.Current != (PSDriveInfo) null) ? (string) null : this.sessionState.Drive.Current.Name;
      if (path.Length == 0)
        flag = false;
      else if (path.StartsWith(".\\", StringComparison.Ordinal) || path.StartsWith("./", StringComparison.Ordinal))
      {
        flag = false;
      }
      else
      {
        int length = path.IndexOf(":", StringComparison.CurrentCulture);
        if (length == -1)
          flag = false;
        else if (length > 0)
        {
          driveName = path.Substring(0, length);
          flag = true;
        }
      }
      LocationGlobber.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    private static string RemoveGlobEscaping(string path) => path != null ? WildcardPattern.Unescape(path) : throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));

    internal bool IsShellVirtualDrive(string driveName, out SessionStateScope scope)
    {
      if (driveName == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (driveName));
      bool flag = false;
      if (string.Compare(driveName, "GLOBAL", StringComparison.OrdinalIgnoreCase) == 0)
      {
        LocationGlobber.tracer.WriteLine("match found: {0}", (object) "GLOBAL");
        flag = true;
        scope = this.sessionState.Internal.GlobalScope;
      }
      else if (string.Compare(driveName, "LOCAL", StringComparison.OrdinalIgnoreCase) == 0)
      {
        LocationGlobber.tracer.WriteLine("match found: {0}", (object) driveName);
        flag = true;
        scope = this.sessionState.Internal.CurrentScope;
      }
      else
        scope = (SessionStateScope) null;
      LocationGlobber.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal string GetDriveRootRelativePathFromPSPath(
      string path,
      CmdletProviderContext context,
      bool escapeCurrentLocation,
      out PSDriveInfo workingDriveForPath,
      out CmdletProvider providerInstance)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      workingDriveForPath = (PSDriveInfo) null;
      string driveName = (string) null;
      if (this.sessionState.Drive.Current != (PSDriveInfo) null)
        driveName = this.sessionState.Drive.Current.Name;
      bool flag = false;
      if (this.IsAbsolutePath(path, out driveName))
      {
        LocationGlobber.tracer.WriteLine("Drive Name: {0}", (object) driveName);
        try
        {
          workingDriveForPath = this.sessionState.Drive.Get(driveName);
        }
        catch (DriveNotFoundException ex)
        {
          string str = this.sessionState.Drive.Current.Root.Replace('/', '\\');
          if (str.IndexOf(":", StringComparison.CurrentCulture) >= 0 && path.Replace('/', '\\').StartsWith(str, StringComparison.OrdinalIgnoreCase))
          {
            flag = true;
            path = path.Substring(str.Length);
            path = path.TrimStart('\\');
            path = '\\'.ToString() + path;
            workingDriveForPath = this.sessionState.Drive.Current;
          }
          if (!flag)
            throw;
        }
        if (!flag)
          path = path.Substring(driveName.Length + 1);
      }
      else
        workingDriveForPath = this.sessionState.Drive.Current;
      if (workingDriveForPath == (PSDriveInfo) null)
      {
        ItemNotFoundException notFoundException = new ItemNotFoundException(path, "PathNotFound");
        LocationGlobber.tracer.TraceException((Exception) notFoundException);
        LocationGlobber.pathResolutionTracer.TraceError("Item does not exist: {0}", (object) path);
        throw notFoundException;
      }
      try
      {
        providerInstance = (CmdletProvider) this.sessionState.Internal.GetContainerProviderInstance(workingDriveForPath.Provider);
        context.Drive = workingDriveForPath;
        return this.GenerateRelativePath(workingDriveForPath, path, escapeCurrentLocation, providerInstance, context);
      }
      catch (PSNotSupportedException ex)
      {
        providerInstance = (CmdletProvider) null;
        return "";
      }
    }

    private string GetDriveRootRelativePathFromProviderPath(
      string providerPath,
      PSDriveInfo drive,
      CmdletProviderContext context)
    {
      string child = "";
      CmdletProvider providerInstance = (CmdletProvider) this.sessionState.Internal.GetContainerProviderInstance(drive.Provider);
      NavigationCmdletProvider navigationCmdletProvider = providerInstance as NavigationCmdletProvider;
      providerPath = providerPath.Replace('/', '\\');
      providerPath = providerPath.TrimEnd('\\');
      string str1 = drive.Root.Replace('/', '\\');
      char[] chArray = new char[1]{ '\\' };
      for (string str2 = str1.TrimEnd(chArray); !string.IsNullOrEmpty(providerPath) && !providerPath.Equals(str2, StringComparison.OrdinalIgnoreCase); providerPath = this.sessionState.Internal.GetParentPath(providerInstance, providerPath, drive.Root, context))
        child = string.IsNullOrEmpty(child) ? navigationCmdletProvider.GetChildName(providerPath, context) : this.sessionState.Internal.MakePath(providerInstance, navigationCmdletProvider.GetChildName(providerPath, context), child, context);
      return child;
    }

    internal string GenerateRelativePath(
      PSDriveInfo drive,
      string path,
      bool escapeCurrentLocation,
      CmdletProvider providerInstance,
      CmdletProviderContext context)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      string str1 = !(drive == (PSDriveInfo) null) ? drive.CurrentLocation : throw LocationGlobber.tracer.NewArgumentNullException(nameof (drive));
      if (!string.IsNullOrEmpty(str1) && str1.StartsWith(drive.Root, StringComparison.Ordinal))
        str1 = str1.Substring(drive.Root.Length);
      if (escapeCurrentLocation)
        str1 = WildcardPattern.Escape(str1);
      if (!string.IsNullOrEmpty(path))
      {
        if (path[0] == '\\' || path[0] == '/')
        {
          str1 = string.Empty;
          path = path.Substring(1);
          LocationGlobber.tracer.WriteLine("path = {0}", (object) path);
        }
        else
        {
          while (path.Length > 0 && this.HasRelativePathTokens(path))
          {
            if (context.Stopping)
              throw new PipelineStoppedException();
            bool flag1 = false;
            bool flag2 = path.StartsWith("..", StringComparison.Ordinal);
            bool flag3 = path.Length == 2;
            bool flag4 = path.Length > 2 && (path[2] == '\\' || path[2] == '/');
            bool flag5;
            if (flag2 && (flag3 || flag4))
            {
              if (!string.IsNullOrEmpty(str1))
                str1 = this.sessionState.Internal.GetParentPath(providerInstance, str1, drive.Root, context);
              LocationGlobber.tracer.WriteLine("Parent path = {0}", (object) str1);
              path = path.Substring(2);
              LocationGlobber.tracer.WriteLine("path = {0}", (object) path);
              flag5 = true;
              if (path.Length != 0)
              {
                if (path[0] == '\\' || path[0] == '/')
                  path = path.Substring(1);
                LocationGlobber.tracer.WriteLine("path = {0}", (object) path);
                if (path.Length == 0)
                  break;
              }
              else
                break;
            }
            else
            {
              if (path.Equals(".", StringComparison.OrdinalIgnoreCase))
              {
                flag5 = true;
                path = string.Empty;
                break;
              }
              if (path.StartsWith(".\\", StringComparison.Ordinal) || path.StartsWith("./", StringComparison.Ordinal))
              {
                path = path.Substring(".\\".Length);
                flag1 = true;
                LocationGlobber.tracer.WriteLine("path = {0}", (object) path);
                if (path.Length == 0)
                  break;
              }
              if (path.Length == 0 || !flag1)
                break;
            }
          }
        }
      }
      if (!string.IsNullOrEmpty(path))
        str1 = this.sessionState.Internal.MakePath(providerInstance, str1, path, context);
      if (providerInstance is NavigationCmdletProvider navigationCmdletProvider)
      {
        string path1 = this.sessionState.Internal.MakePath(context.Drive.Root, str1, context);
        string str2 = navigationCmdletProvider.ContractRelativePath(path1, context.Drive.Root, false, context);
        str1 = string.IsNullOrEmpty(str2) ? "" : (!str2.StartsWith(context.Drive.Root, StringComparison.Ordinal) ? str2 : str2.Substring(context.Drive.Root.Length));
      }
      LocationGlobber.tracer.WriteLine("result = {0}", (object) str1);
      return str1;
    }

    private bool HasRelativePathTokens(string path)
    {
      string str = path.Replace('/', '\\');
      return str.Equals(".", StringComparison.OrdinalIgnoreCase) || str.Equals("..", StringComparison.OrdinalIgnoreCase) || (str.Contains("\\.\\") || str.Contains("\\..\\")) || (str.EndsWith("\\..", StringComparison.OrdinalIgnoreCase) || str.EndsWith("\\.", StringComparison.OrdinalIgnoreCase) || (str.StartsWith("..\\", StringComparison.OrdinalIgnoreCase) || str.StartsWith(".\\", StringComparison.OrdinalIgnoreCase))) || str.StartsWith("~", StringComparison.OrdinalIgnoreCase);
    }

    private string GetProviderSpecificPath(
      PSDriveInfo drive,
      string workingPath,
      CmdletProviderContext context)
    {
      if (drive == (PSDriveInfo) null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (drive));
      if (workingPath == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (workingPath));
      drive.Trace();
      LocationGlobber.tracer.WriteLine("workingPath = {0}", (object) workingPath);
      string parent = drive.Root;
      try
      {
        parent = this.sessionState.Internal.MakePath(drive.Provider, parent, workingPath, context);
      }
      catch (NotSupportedException ex)
      {
      }
      return parent;
    }

    private static string ParseProviderPath(string path, out string providerId)
    {
      int length = path != null ? path.IndexOf("::", StringComparison.Ordinal) : throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      providerId = length > 0 ? path.Substring(0, length) : throw (ArgumentException) LocationGlobber.tracer.NewArgumentException(nameof (path), "SessionStateStrings", "NotProviderQualifiedPath");
      string str = path.Substring(length + "::".Length);
      LocationGlobber.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    internal Collection<string> GetGlobbedProviderPathsFromProviderPath(
      string path,
      bool allowNonexistingPaths,
      ContainerCmdletProvider containerProvider,
      CmdletProviderContext context)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      if (containerProvider == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (containerProvider));
      if (context == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (context));
      return this.ExpandGlobPath(path, allowNonexistingPaths, containerProvider, context);
    }

    internal static bool StringContainsGlobCharacters(string path) => path != null ? WildcardPattern.ContainsWildcardCharacters(path) : throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));

    internal static bool ShouldPerformGlobbing(string path, CmdletProviderContext context)
    {
      bool flag1 = false;
      if (path != null)
        flag1 = LocationGlobber.StringContainsGlobCharacters(path);
      bool flag2 = false;
      bool flag3 = false;
      if (context != null)
      {
        bool flag4 = context.Include != null && context.Include.Count > 0;
        LocationGlobber.pathResolutionTracer.WriteLine("INCLUDE filter present: {0}", (object) flag4);
        bool flag5 = context.Exclude != null && context.Exclude.Count > 0;
        LocationGlobber.pathResolutionTracer.WriteLine("EXCLUDE filter present: {0}", (object) flag5);
        flag2 = flag4 || flag5;
        flag3 = context.SuppressWildcardExpansion;
        LocationGlobber.pathResolutionTracer.WriteLine("NOGLOB parameter present: {0}", (object) flag3);
      }
      LocationGlobber.pathResolutionTracer.WriteLine("Path contains wildcard characters: {0}", (object) flag1);
      bool flag6 = (flag1 || flag2) && !flag3;
      LocationGlobber.tracer.WriteLine("result = {0}", (object) flag6);
      return flag6;
    }

    private Collection<string> ExpandMshGlobPath(
      string path,
      bool allowNonexistingPaths,
      PSDriveInfo drive,
      ContainerCmdletProvider provider,
      CmdletProviderContext context)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      if (provider == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (provider));
      if (drive == (PSDriveInfo) null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (drive));
      LocationGlobber.tracer.WriteLine("path = {0}", (object) path);
      NavigationCmdletProvider navigationCmdletProvider = provider as NavigationCmdletProvider;
      Collection<string> collection = new Collection<string>();
      using (LocationGlobber.pathResolutionTracer.TraceScope("EXPANDING WILDCARDS"))
      {
        if (LocationGlobber.ShouldPerformGlobbing(path, context))
        {
          StringCollection currentDirs = new StringCollection();
          Stack<string> stringStack = new Stack<string>();
          using (LocationGlobber.pathResolutionTracer.TraceScope("Tokenizing path"))
          {
            while (LocationGlobber.StringContainsGlobCharacters(path))
            {
              if (context.Stopping)
                throw new PipelineStoppedException();
              string str = path;
              if (navigationCmdletProvider != null)
                str = navigationCmdletProvider.GetChildName(path, context);
              if (!string.IsNullOrEmpty(str))
              {
                LocationGlobber.tracer.WriteLine("Pushing leaf element: {0}", (object) str);
                LocationGlobber.pathResolutionTracer.WriteLine("Leaf element: {0}", (object) str);
                stringStack.Push(str);
                if (navigationCmdletProvider != null)
                {
                  string parentPath = navigationCmdletProvider.GetParentPath(path, drive.Root, context);
                  path = !string.Equals(parentPath, path, StringComparison.OrdinalIgnoreCase) ? parentPath : throw LocationGlobber.tracer.NewInvalidOperationException("SessionStateStrings", "ProviderImplementationInconsistent", (object) provider.ProviderInfo.Name, (object) path);
                }
                else
                  path = string.Empty;
                LocationGlobber.tracer.WriteLine("New path: {0}", (object) path);
                LocationGlobber.pathResolutionTracer.WriteLine("Parent path: {0}", (object) path);
              }
              else
                break;
            }
            LocationGlobber.tracer.WriteLine("Base container path: {0}", (object) path);
            if (stringStack.Count == 0)
            {
              string str = path;
              if (navigationCmdletProvider != null)
              {
                str = navigationCmdletProvider.GetChildName(path, context);
                if (!string.IsNullOrEmpty(str))
                  path = navigationCmdletProvider.GetParentPath(path, (string) null, context);
              }
              else
                path = string.Empty;
              stringStack.Push(str);
              LocationGlobber.pathResolutionTracer.WriteLine("Leaf element: {0}", (object) str);
            }
            LocationGlobber.pathResolutionTracer.WriteLine("Root path of resolution: {0}", (object) path);
          }
          currentDirs.Add(path);
          while (stringStack.Count > 0)
          {
            if (context.Stopping)
              throw new PipelineStoppedException();
            string leafElement = stringStack.Pop();
            currentDirs = this.GenerateNewPSPathsWithGlobLeaf(currentDirs, drive, leafElement, stringStack.Count == 0, provider, context);
            if (stringStack.Count > 0)
            {
              using (LocationGlobber.pathResolutionTracer.TraceScope("Checking matches to ensure they are containers"))
              {
                int index = 0;
                while (index < currentDirs.Count)
                {
                  if (context.Stopping)
                    throw new PipelineStoppedException();
                  string mshQualifiedPath = LocationGlobber.GetMshQualifiedPath(currentDirs[index], drive);
                  if (navigationCmdletProvider != null && !this.sessionState.Internal.IsItemContainer(mshQualifiedPath, context))
                  {
                    LocationGlobber.tracer.WriteLine("Removing {0} because it is not a container", (object) currentDirs[index]);
                    LocationGlobber.pathResolutionTracer.WriteLine("{0} is not a container", (object) currentDirs[index]);
                    currentDirs.RemoveAt(index);
                  }
                  else if (navigationCmdletProvider != null)
                  {
                    LocationGlobber.pathResolutionTracer.WriteLine("{0} is a container", (object) currentDirs[index]);
                    ++index;
                  }
                }
              }
            }
          }
          foreach (string str in currentDirs)
          {
            LocationGlobber.pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", (object) str);
            collection.Add(str);
          }
        }
        else
        {
          string path1 = context.SuppressWildcardExpansion ? path : LocationGlobber.RemoveGlobEscaping(path);
          string format = "{0}:" + (object) '\\' + "{1}";
          if (drive.Hidden)
            format = !LocationGlobber.IsProviderDirectPath(path1) ? "{0}::{1}" : "{1}";
          else if (path.StartsWith('\\'.ToString(), StringComparison.Ordinal))
            format = "{0}:{1}";
          string path2 = string.Format((IFormatProvider) CultureInfo.InvariantCulture, format, (object) drive.Name, (object) path1);
          if (allowNonexistingPaths || provider.ItemExists(this.GetProviderPath(path2, context), context))
          {
            LocationGlobber.pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", (object) path2);
            collection.Add(path2);
          }
          else
          {
            ItemNotFoundException notFoundException = new ItemNotFoundException(path2, "PathNotFound");
            LocationGlobber.tracer.TraceException((Exception) notFoundException);
            LocationGlobber.pathResolutionTracer.TraceError("Item does not exist: {0}", (object) path);
            throw notFoundException;
          }
        }
      }
      return collection;
    }

    internal static string GetMshQualifiedPath(string path, PSDriveInfo drive) => !drive.Hidden ? LocationGlobber.GetDriveQualifiedPath(path, drive) : (!LocationGlobber.IsProviderDirectPath(path) ? LocationGlobber.GetProviderQualifiedPath(path, drive.Provider) : path);

    internal static string RemoveMshQualifier(string path, PSDriveInfo drive) => !drive.Hidden ? LocationGlobber.RemoveDriveQualifier(path) : LocationGlobber.RemoveProviderQualifier(path);

    internal static string GetDriveQualifiedPath(string path, PSDriveInfo drive)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      if (drive == (PSDriveInfo) null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (drive));
      string str = path;
      bool flag = true;
      int length = path.IndexOf(':');
      if (length != -1)
      {
        if (drive.Hidden)
          flag = false;
        else if (string.Equals(path.Substring(0, length), drive.Name, StringComparison.OrdinalIgnoreCase))
          flag = false;
      }
      if (flag)
      {
        string format = "{0}:" + (object) '\\' + "{1}";
        if (path.StartsWith('\\'.ToString(), StringComparison.Ordinal))
          format = "{0}:{1}";
        str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, format, (object) drive.Name, (object) path);
      }
      LocationGlobber.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    private static string RemoveDriveQualifier(string path)
    {
      string str = path;
      int num = path.IndexOf(":", StringComparison.Ordinal);
      if (num != -1)
      {
        if (path[num + 1] == '\\' || path[num + 1] == '/')
          ++num;
        str = path.Substring(num + 1);
      }
      LocationGlobber.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    internal static string GetProviderQualifiedPath(string path, ProviderInfo provider)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      if (provider == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (provider));
      string str = path;
      bool flag = false;
      int length = path.IndexOf("::", StringComparison.Ordinal);
      if (length != -1)
      {
        string providerName = path.Substring(0, length);
        if (provider.NameEquals(providerName))
          flag = true;
      }
      if (!flag)
        str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}{1}{2}", (object) provider.FullName, (object) "::", (object) path);
      LocationGlobber.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    internal static string RemoveProviderQualifier(string path)
    {
      string str = path;
      int num = path.IndexOf("::", StringComparison.Ordinal);
      if (num != -1)
        str = path.Substring(num + "::".Length);
      LocationGlobber.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    private StringCollection GenerateNewPSPathsWithGlobLeaf(
      StringCollection currentDirs,
      PSDriveInfo drive,
      string leafElement,
      bool isLastLeaf,
      ContainerCmdletProvider provider,
      CmdletProviderContext context)
    {
      if (currentDirs == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (currentDirs));
      NavigationCmdletProvider navigationCmdletProvider = provider != null ? provider as NavigationCmdletProvider : throw LocationGlobber.tracer.NewArgumentNullException(nameof (provider));
      StringCollection stringCollection = new StringCollection();
      if (leafElement != null && leafElement.Length > 0 && LocationGlobber.StringContainsGlobCharacters(leafElement) || isLastLeaf)
      {
        WildcardPattern stringMatcher = new WildcardPattern(LocationGlobber.ConvertMshEscapeToRegexEscape(leafElement), WildcardOptions.IgnoreCase);
        Collection<WildcardPattern> wildcardsFromStrings1 = SessionStateUtilities.CreateWildcardsFromStrings(context.Include, WildcardOptions.IgnoreCase);
        Collection<WildcardPattern> wildcardsFromStrings2 = SessionStateUtilities.CreateWildcardsFromStrings(context.Exclude, WildcardOptions.IgnoreCase);
        foreach (string currentDir in currentDirs)
        {
          using (LocationGlobber.pathResolutionTracer.TraceScope("Expanding wildcards for items under '{0}'", (object) currentDir))
          {
            if (context.Stopping)
              throw new PipelineStoppedException();
            string modifiedDirPath = string.Empty;
            Collection<PSObject> childNamesInDir = this.GetChildNamesInDir(currentDir, leafElement, !isLastLeaf, context, false, drive, provider, out modifiedDirPath);
            if (childNamesInDir == null)
            {
              LocationGlobber.tracer.TraceError("GetChildNames returned a null array");
              LocationGlobber.pathResolutionTracer.WriteLine("No child names returned for '{0}'", (object) currentDir);
            }
            else
            {
              foreach (PSObject childObject in childNamesInDir)
              {
                if (context.Stopping)
                  throw new PipelineStoppedException();
                string childName = string.Empty;
                if (LocationGlobber.IsChildNameAMatch(childObject, stringMatcher, wildcardsFromStrings1, wildcardsFromStrings2, out childName))
                {
                  string str = childName;
                  if (navigationCmdletProvider != null)
                    str = LocationGlobber.GetMshQualifiedPath(this.sessionState.Internal.MakePath(LocationGlobber.RemoveMshQualifier(modifiedDirPath, drive), childName, context), drive);
                  LocationGlobber.tracer.WriteLine("Adding child path to dirs {0}", (object) str);
                  stringCollection.Add(str);
                }
              }
            }
          }
        }
      }
      else
      {
        LocationGlobber.tracer.WriteLine("LeafElement does not contain any glob characters so do a MakePath", new object[0]);
        foreach (string currentDir in currentDirs)
        {
          using (LocationGlobber.pathResolutionTracer.TraceScope("Expanding intermediate containers under '{0}'", (object) currentDir))
          {
            if (context.Stopping)
              throw new PipelineStoppedException();
            string regexEscape = LocationGlobber.ConvertMshEscapeToRegexEscape(leafElement);
            string mshQualifiedPath = LocationGlobber.GetMshQualifiedPath(context.SuppressWildcardExpansion ? currentDir : LocationGlobber.RemoveGlobEscaping(currentDir), drive);
            string path = regexEscape;
            if (navigationCmdletProvider != null)
              path = LocationGlobber.GetMshQualifiedPath(this.sessionState.Internal.MakePath(LocationGlobber.RemoveMshQualifier(mshQualifiedPath, drive), regexEscape, context), drive);
            if (this.sessionState.Internal.ItemExists(path, context))
            {
              LocationGlobber.tracer.WriteLine("Adding child path to dirs {0}", (object) path);
              LocationGlobber.pathResolutionTracer.WriteLine("Valid intermediate container: {0}", (object) path);
              stringCollection.Add(path);
            }
          }
        }
      }
      return stringCollection;
    }

    internal Collection<string> ExpandGlobPath(
      string path,
      bool allowNonexistingPaths,
      ContainerCmdletProvider provider,
      CmdletProviderContext context)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      if (provider == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (provider));
      string updatedPath = (string) null;
      string updatedFilter = (string) null;
      string filter = context.Filter;
      bool flag = provider.ConvertPath(path, context.Filter, ref updatedPath, ref updatedFilter, context);
      if (flag)
      {
        LocationGlobber.tracer.WriteLine("Provider converted path and filter.", new object[0]);
        LocationGlobber.tracer.WriteLine("Original path: " + path, new object[0]);
        LocationGlobber.tracer.WriteLine("Converted path: " + updatedPath, new object[0]);
        LocationGlobber.tracer.WriteLine("Original filter: " + context.Filter, new object[0]);
        LocationGlobber.tracer.WriteLine("Converted filter: " + updatedFilter, new object[0]);
        path = updatedPath;
        filter = context.Filter;
      }
      NavigationCmdletProvider navigationCmdletProvider = provider as NavigationCmdletProvider;
      LocationGlobber.tracer.WriteLine("path = {0}", (object) path);
      Collection<string> collection = new Collection<string>();
      using (LocationGlobber.pathResolutionTracer.TraceScope("EXPANDING WILDCARDS"))
      {
        if (LocationGlobber.ShouldPerformGlobbing(path, context))
        {
          StringCollection currentDirs = new StringCollection();
          Stack<string> stringStack = new Stack<string>();
          using (LocationGlobber.pathResolutionTracer.TraceScope("Tokenizing path"))
          {
            while (LocationGlobber.StringContainsGlobCharacters(path))
            {
              if (context.Stopping)
                throw new PipelineStoppedException();
              string str = path;
              if (navigationCmdletProvider != null)
                str = navigationCmdletProvider.GetChildName(path, context);
              if (!string.IsNullOrEmpty(str))
              {
                LocationGlobber.tracer.WriteLine("Pushing leaf element: {0}", (object) str);
                LocationGlobber.pathResolutionTracer.WriteLine("Leaf element: {0}", (object) str);
                stringStack.Push(str);
                if (navigationCmdletProvider != null)
                {
                  string root = string.Empty;
                  if (context != null)
                  {
                    PSDriveInfo drive = context.Drive;
                    if (drive != (PSDriveInfo) null)
                      root = drive.Root;
                  }
                  string parentPath = navigationCmdletProvider.GetParentPath(path, root, context);
                  path = !string.Equals(parentPath, path, StringComparison.OrdinalIgnoreCase) ? parentPath : throw LocationGlobber.tracer.NewInvalidOperationException("SessionStateStrings", "ProviderImplementationInconsistent", (object) provider.ProviderInfo.Name, (object) path);
                }
                else
                  path = string.Empty;
                LocationGlobber.tracer.WriteLine("New path: {0}", (object) path);
                LocationGlobber.pathResolutionTracer.WriteLine("Parent path: {0}", (object) path);
              }
              else
                break;
            }
            LocationGlobber.tracer.WriteLine("Base container path: {0}", (object) path);
            if (stringStack.Count == 0)
            {
              string str = path;
              if (navigationCmdletProvider != null)
              {
                str = navigationCmdletProvider.GetChildName(path, context);
                if (!string.IsNullOrEmpty(str))
                  path = navigationCmdletProvider.GetParentPath(path, (string) null, context);
              }
              else
                path = string.Empty;
              stringStack.Push(str);
              LocationGlobber.pathResolutionTracer.WriteLine("Leaf element: {0}", (object) str);
            }
            LocationGlobber.pathResolutionTracer.WriteLine("Root path of resolution: {0}", (object) path);
          }
          currentDirs.Add(path);
          while (stringStack.Count > 0)
          {
            if (context.Stopping)
              throw new PipelineStoppedException();
            string leafElement = stringStack.Pop();
            currentDirs = this.GenerateNewPathsWithGlobLeaf(currentDirs, leafElement, stringStack.Count == 0, provider, context);
            if (stringStack.Count > 0)
            {
              using (LocationGlobber.pathResolutionTracer.TraceScope("Checking matches to ensure they are containers"))
              {
                int index = 0;
                while (index < currentDirs.Count)
                {
                  if (context.Stopping)
                    throw new PipelineStoppedException();
                  if (navigationCmdletProvider != null && !navigationCmdletProvider.IsItemContainer(currentDirs[index], context))
                  {
                    LocationGlobber.tracer.WriteLine("Removing {0} because it is not a container", (object) currentDirs[index]);
                    LocationGlobber.pathResolutionTracer.WriteLine("{0} is not a container", (object) currentDirs[index]);
                    currentDirs.RemoveAt(index);
                  }
                  else if (navigationCmdletProvider != null)
                  {
                    LocationGlobber.pathResolutionTracer.WriteLine("{0} is a container", (object) currentDirs[index]);
                    ++index;
                  }
                }
              }
            }
          }
          foreach (string str in currentDirs)
          {
            LocationGlobber.pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", (object) str);
            collection.Add(str);
          }
        }
        else
        {
          string path1 = context.SuppressWildcardExpansion ? path : LocationGlobber.RemoveGlobEscaping(path);
          if (allowNonexistingPaths || provider.ItemExists(path1, context))
          {
            LocationGlobber.pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", (object) path1);
            collection.Add(path1);
          }
          else
          {
            ItemNotFoundException notFoundException = new ItemNotFoundException(path, "PathNotFound");
            LocationGlobber.tracer.TraceException((Exception) notFoundException);
            LocationGlobber.pathResolutionTracer.TraceError("Item does not exist: {0}", (object) path);
            throw notFoundException;
          }
        }
      }
      if (flag)
        context.Filter = filter;
      return collection;
    }

    internal StringCollection GenerateNewPathsWithGlobLeaf(
      StringCollection currentDirs,
      string leafElement,
      bool isLastLeaf,
      ContainerCmdletProvider provider,
      CmdletProviderContext context)
    {
      if (currentDirs == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (currentDirs));
      NavigationCmdletProvider navigationCmdletProvider = provider != null ? provider as NavigationCmdletProvider : throw LocationGlobber.tracer.NewArgumentNullException(nameof (provider));
      StringCollection stringCollection = new StringCollection();
      if (leafElement != null && leafElement.Length > 0 && (LocationGlobber.StringContainsGlobCharacters(leafElement) || isLastLeaf))
      {
        WildcardPattern stringMatcher = new WildcardPattern(LocationGlobber.ConvertMshEscapeToRegexEscape(leafElement), WildcardOptions.IgnoreCase);
        Collection<WildcardPattern> wildcardsFromStrings1 = SessionStateUtilities.CreateWildcardsFromStrings(context.Include, WildcardOptions.IgnoreCase);
        Collection<WildcardPattern> wildcardsFromStrings2 = SessionStateUtilities.CreateWildcardsFromStrings(context.Exclude, WildcardOptions.IgnoreCase);
        foreach (string currentDir in currentDirs)
        {
          using (LocationGlobber.pathResolutionTracer.TraceScope("Expanding wildcards for items under '{0}'", (object) currentDir))
          {
            if (context.Stopping)
              throw new PipelineStoppedException();
            string modifiedDirPath = (string) null;
            Collection<PSObject> childNamesInDir = this.GetChildNamesInDir(currentDir, leafElement, !isLastLeaf, context, true, (PSDriveInfo) null, provider, out modifiedDirPath);
            if (childNamesInDir == null)
            {
              LocationGlobber.tracer.TraceError("GetChildNames returned a null array");
              LocationGlobber.pathResolutionTracer.WriteLine("No child names returned for '{0}'", (object) currentDir);
            }
            else
            {
              foreach (PSObject childObject in childNamesInDir)
              {
                if (context.Stopping)
                  throw new PipelineStoppedException();
                string childName = string.Empty;
                if (LocationGlobber.IsChildNameAMatch(childObject, stringMatcher, wildcardsFromStrings1, wildcardsFromStrings2, out childName))
                {
                  string str = childName;
                  if (navigationCmdletProvider != null)
                    str = navigationCmdletProvider.MakePath(modifiedDirPath, childName, context);
                  LocationGlobber.tracer.WriteLine("Adding child path to dirs {0}", (object) str);
                  stringCollection.Add(str);
                }
              }
            }
          }
        }
      }
      else
      {
        LocationGlobber.tracer.WriteLine("LeafElement does not contain any glob characters so do a MakePath", new object[0]);
        foreach (string currentDir in currentDirs)
        {
          using (LocationGlobber.pathResolutionTracer.TraceScope("Expanding intermediate containers under '{0}'", (object) currentDir))
          {
            if (context.Stopping)
              throw new PipelineStoppedException();
            string regexEscape = LocationGlobber.ConvertMshEscapeToRegexEscape(leafElement);
            string parent = context.SuppressWildcardExpansion ? currentDir : LocationGlobber.RemoveGlobEscaping(currentDir);
            string path = regexEscape;
            if (navigationCmdletProvider != null)
              path = navigationCmdletProvider.MakePath(parent, regexEscape, context);
            if (provider.ItemExists(path, context))
            {
              LocationGlobber.tracer.WriteLine("Adding child path to dirs {0}", (object) path);
              stringCollection.Add(path);
              LocationGlobber.pathResolutionTracer.WriteLine("Valid intermediate container: {0}", (object) path);
            }
          }
        }
      }
      return stringCollection;
    }

    private Collection<PSObject> GetChildNamesInDir(
      string dir,
      string leafElement,
      bool getAllContainers,
      CmdletProviderContext context,
      bool dirIsProviderPath,
      PSDriveInfo drive,
      ContainerCmdletProvider provider,
      out string modifiedDirPath)
    {
      string updatedPath = (string) null;
      string updatedFilter = (string) null;
      string filter = context.Filter;
      bool flag = provider.ConvertPath(leafElement, context.Filter, ref updatedPath, ref updatedFilter, context);
      if (flag)
      {
        LocationGlobber.tracer.WriteLine("Provider converted path and filter.", new object[0]);
        LocationGlobber.tracer.WriteLine("Original path: " + leafElement, new object[0]);
        LocationGlobber.tracer.WriteLine("Converted path: " + updatedPath, new object[0]);
        LocationGlobber.tracer.WriteLine("Original filter: " + context.Filter, new object[0]);
        LocationGlobber.tracer.WriteLine("Converted filter: " + updatedFilter, new object[0]);
        leafElement = updatedPath;
        context.Filter = updatedFilter;
      }
      ReturnContainers returnContainers = ReturnContainers.ReturnAllContainers;
      if (!getAllContainers)
        returnContainers = ReturnContainers.ReturnMatchingContainers;
      CmdletProviderContext context1 = new CmdletProviderContext(context);
      context1.SetFilters(new Collection<string>(), new Collection<string>(), context.Filter);
      try
      {
        string path = context.SuppressWildcardExpansion ? dir : LocationGlobber.RemoveGlobEscaping(dir);
        modifiedDirPath = (string) null;
        if (dirIsProviderPath)
        {
          modifiedDirPath = path;
        }
        else
        {
          modifiedDirPath = LocationGlobber.GetMshQualifiedPath(path, drive);
          ProviderInfo provider1 = (ProviderInfo) null;
          CmdletProvider providerInstance = (CmdletProvider) null;
          Collection<string> pathsFromMonadPath = this.GetGlobbedProviderPathsFromMonadPath(modifiedDirPath, false, context1, out provider1, out providerInstance);
          if (pathsFromMonadPath.Count > 0)
          {
            path = pathsFromMonadPath[0];
          }
          else
          {
            if (flag)
              context.Filter = filter;
            return new Collection<PSObject>();
          }
        }
        provider.GetChildNames(path, returnContainers, context1);
        if (context1.HasErrors())
        {
          Collection<ErrorRecord> accumulatedErrorObjects = context1.GetAccumulatedErrorObjects();
          if (accumulatedErrorObjects != null && accumulatedErrorObjects.Count > 0)
          {
            foreach (ErrorRecord errorRecord in accumulatedErrorObjects)
              context.WriteError(errorRecord);
          }
        }
        Collection<PSObject> accumulatedObjects = context1.GetAccumulatedObjects();
        if (flag)
          context.Filter = filter;
        return accumulatedObjects;
      }
      finally
      {
        context1.RemoveStopReferral();
      }
    }

    private static bool IsChildNameAMatch(
      PSObject childObject,
      WildcardPattern stringMatcher,
      Collection<WildcardPattern> includeMatcher,
      Collection<WildcardPattern> excludeMatcher,
      out string childName)
    {
      bool flag1 = false;
      childName = (string) null;
      object baseObject = childObject.BaseObject;
      if (baseObject is PSCustomObject)
      {
        LocationGlobber.tracer.TraceError("GetChildNames returned a null object");
      }
      else
      {
        childName = baseObject as string;
        if (childName == null)
        {
          LocationGlobber.tracer.TraceError("GetChildNames returned an object that wasn't a string");
        }
        else
        {
          LocationGlobber.pathResolutionTracer.WriteLine("Name returned from provider: {0}", (object) childName);
          bool flag2 = WildcardPattern.ContainsWildcardCharacters(stringMatcher.Pattern);
          bool flag3 = stringMatcher.IsMatch(childName);
          LocationGlobber.tracer.WriteLine("isChildMatch = {0}", (object) flag3);
          bool flag4 = includeMatcher.Count > 0;
          bool flag5 = excludeMatcher.Count > 0;
          bool flag6 = SessionStateUtilities.MatchesAnyWildcardPattern(childName, (IEnumerable<WildcardPattern>) includeMatcher, true);
          LocationGlobber.tracer.WriteLine("isIncludeMatch = {0}", (object) flag6);
          if (flag3 || flag2 && flag4 && flag6)
          {
            LocationGlobber.pathResolutionTracer.WriteLine("Path wildcard match: {0}", (object) childName);
            flag1 = true;
            if (flag4 && !flag6)
            {
              LocationGlobber.pathResolutionTracer.WriteLine("Not included match: {0}", (object) childName);
              flag1 = false;
            }
            if (flag5 && SessionStateUtilities.MatchesAnyWildcardPattern(childName, (IEnumerable<WildcardPattern>) excludeMatcher, false))
            {
              LocationGlobber.pathResolutionTracer.WriteLine("Excluded match: {0}", (object) childName);
              flag1 = false;
            }
          }
          else
            LocationGlobber.pathResolutionTracer.WriteLine("NOT path wildcard match: {0}", (object) childName);
        }
      }
      LocationGlobber.tracer.WriteLine("result = {0}; childName = {1}", (object) flag1, (object) childName);
      return flag1;
    }

    private static string ConvertMshEscapeToRegexEscape(string path)
    {
      char[] chArray = path != null ? path.ToCharArray() : throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < chArray.GetLength(0); ++index)
      {
        if (chArray[index] == '`')
        {
          if (index + 1 < chArray.GetLength(0))
          {
            if (chArray[index + 1] == '`')
            {
              stringBuilder.Append('`');
              ++index;
            }
            else
              stringBuilder.Append('\\');
          }
          else
            stringBuilder.Append('\\');
        }
        else if (chArray[index] == '\\')
          stringBuilder.Append("\\\\");
        else
          stringBuilder.Append(chArray[index]);
      }
      LocationGlobber.tracer.WriteLine("Original path: {0} Converted to: {1}", (object) path, (object) stringBuilder.ToString());
      return stringBuilder.ToString();
    }

    internal static bool IsHomePath(string path)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      bool flag = false;
      if (LocationGlobber.IsProviderQualifiedPath(path))
      {
        int num = path.IndexOf("::", StringComparison.Ordinal);
        if (num != -1)
          path = path.Substring(num + "::".Length);
      }
      if (path.IndexOf("~", StringComparison.Ordinal) == 0)
      {
        if (path.Length == 1)
          flag = true;
        else if (path.Length > 1 && (path[1] == '\\' || path[1] == '/'))
          flag = true;
      }
      LocationGlobber.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal static bool IsProviderDirectPath(string path)
    {
      if (path == null)
        throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      bool flag = false;
      if (path.StartsWith("\\\\", StringComparison.Ordinal) || path.StartsWith("//", StringComparison.Ordinal))
        flag = true;
      LocationGlobber.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal string GetHomeRelativePath(string path)
    {
      string str = path != null ? path : throw LocationGlobber.tracer.NewArgumentNullException(nameof (path));
      if (LocationGlobber.IsHomePath(path))
      {
        ProviderInfo provider = this.sessionState.Drive.Current.Provider;
        if (LocationGlobber.IsProviderQualifiedPath(path))
        {
          int length = path.IndexOf("::", StringComparison.Ordinal);
          if (length != -1)
          {
            provider = this.sessionState.Internal.GetSingleProvider(path.Substring(0, length));
            path = path.Substring(length + "::".Length);
          }
        }
        if (path.IndexOf("~", StringComparison.Ordinal) == 0)
        {
          path = path.Length <= 1 || path[1] != '\\' && path[1] != '/' ? path.Substring(1) : path.Substring(2);
          if (provider.Home != null && provider.Home.Length > 0)
          {
            CmdletProviderContext context = new CmdletProviderContext(this.sessionState.Internal.ExecutionContext);
            LocationGlobber.pathResolutionTracer.WriteLine("Getting home path for provider: {0}", (object) provider.Name);
            LocationGlobber.pathResolutionTracer.WriteLine("Provider HOME path: {0}", (object) provider.Home);
            path = !string.IsNullOrEmpty(path) ? this.sessionState.Internal.MakePath(provider, provider.Home, path, context) : provider.Home;
            LocationGlobber.pathResolutionTracer.WriteLine("HOME relative path: {0}", (object) path);
          }
          else
          {
            InvalidOperationException operationException = (InvalidOperationException) LocationGlobber.tracer.NewInvalidOperationException("SessionStateStrings", "HomePathNotSet", (object) provider.Name);
            LocationGlobber.tracer.TraceException((Exception) operationException);
            LocationGlobber.pathResolutionTracer.TraceError("HOME path not set for provider: {0}", (object) provider.Name);
            throw operationException;
          }
        }
        str = path;
      }
      LocationGlobber.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    private static void TraceFilters(CmdletProviderContext context)
    {
      if ((LocationGlobber.pathResolutionTracer.Options & PSTraceSourceOptions.WriteLine) == PSTraceSourceOptions.None)
        return;
      LocationGlobber.pathResolutionTracer.WriteLine("Filter: {0}", context.Filter == null ? (object) string.Empty : (object) context.Filter);
      if (context.Include != null)
      {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (string str in context.Include)
          stringBuilder.AppendFormat("{0} ", (object) str);
        LocationGlobber.pathResolutionTracer.WriteLine("Include: {0}", (object) stringBuilder.ToString());
      }
      if (context.Exclude == null)
        return;
      StringBuilder stringBuilder1 = new StringBuilder();
      foreach (string str in context.Exclude)
        stringBuilder1.AppendFormat("{0} ", (object) str);
      LocationGlobber.pathResolutionTracer.WriteLine("Exclude: {0}", (object) stringBuilder1.ToString());
    }
  }
}
