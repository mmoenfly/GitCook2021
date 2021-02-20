// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.PathResolver
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  internal class PathResolver
  {
    [TraceSource("PathResolver", "PathResolver")]
    internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PathResolver), nameof (PathResolver));

    internal string ResolveProviderAndPath(
      string path,
      PSCmdlet cmdlet,
      bool allowNonexistingPaths,
      string resourceBaseName,
      string multipeProviderErrorId)
    {
      PathInfo pathInfo = this.ResolvePath(path, allowNonexistingPaths, cmdlet);
      if (pathInfo.Provider.ImplementingType == typeof (FileSystemProvider))
        return pathInfo.ProviderPath;
      throw PathResolver.tracer.NewInvalidOperationException(resourceBaseName, multipeProviderErrorId, (object) pathInfo.Provider.Name);
    }

    private PathInfo ResolvePath(
      string pathToResolve,
      bool allowNonexistingPaths,
      PSCmdlet cmdlet)
    {
      CmdletProviderContext context = new CmdletProviderContext((Cmdlet) cmdlet);
      Collection<PathInfo> collection = new Collection<PathInfo>();
      try
      {
        foreach (PathInfo pathInfo in cmdlet.SessionState.Path.GetResolvedPSPathFromPSPath(pathToResolve, context))
          collection.Add(pathInfo);
      }
      catch (PSNotSupportedException ex)
      {
        cmdlet.ThrowTerminatingError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
      }
      catch (DriveNotFoundException ex)
      {
        cmdlet.ThrowTerminatingError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
      }
      catch (ProviderNotFoundException ex)
      {
        cmdlet.ThrowTerminatingError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
      }
      catch (ItemNotFoundException ex)
      {
        if (allowNonexistingPaths)
        {
          ProviderInfo provider = (ProviderInfo) null;
          PSDriveInfo drive = (PSDriveInfo) null;
          string providerPathFromPsPath = cmdlet.SessionState.Path.GetUnresolvedProviderPathFromPSPath(pathToResolve, context, out provider, out drive);
          PathInfo pathInfo = new PathInfo(drive, provider, providerPathFromPsPath, cmdlet.SessionState);
          collection.Add(pathInfo);
        }
        else
          cmdlet.ThrowTerminatingError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
      }
      if (collection.Count == 1)
        return collection[0];
      Exception exception = (Exception) PathResolver.tracer.NewNotSupportedException();
      cmdlet.ThrowTerminatingError(new ErrorRecord(exception, "NotSupported", ErrorCategory.NotImplemented, (object) collection));
      return (PathInfo) null;
    }
  }
}
