// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PathUtils
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using System.Text;

namespace System.Management.Automation
{
  internal static class PathUtils
  {
    [TraceSource("PathUtils", "PathUtils")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PathUtils), nameof (PathUtils));

    internal static void MasterStreamOpen(
      PSCmdlet cmdlet,
      string filePath,
      string encoding,
      bool defaultEncoding,
      bool Append,
      bool Force,
      bool NoClobber,
      out FileStream fileStream,
      out StreamWriter streamWriter,
      out FileInfo readOnlyFileInfo)
    {
      fileStream = (FileStream) null;
      streamWriter = (StreamWriter) null;
      readOnlyFileInfo = (FileInfo) null;
      string str = PathUtils.ResolveFilePath(filePath, cmdlet);
      Encoding encoding1 = EncodingConversion.Convert((Cmdlet) cmdlet, encoding);
      try
      {
        FileMode mode = FileMode.Create;
        if (Append)
          mode = FileMode.Append;
        else if (NoClobber)
          mode = FileMode.CreateNew;
        if (Force && (Append || !NoClobber) && File.Exists(str))
        {
          FileInfo fileInfo = new FileInfo(str);
          if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
          {
            readOnlyFileInfo = fileInfo;
            fileInfo.Attributes &= ~FileAttributes.ReadOnly;
          }
        }
        FileShare share = Force ? FileShare.ReadWrite : FileShare.Read;
        fileStream = new FileStream(str, mode, FileAccess.Write, share);
        if (defaultEncoding)
          streamWriter = new StreamWriter((Stream) fileStream);
        else
          streamWriter = new StreamWriter((Stream) fileStream, encoding1);
      }
      catch (ArgumentException ex)
      {
        PathUtils.ReportFileOpenFailure((Cmdlet) cmdlet, str, (Exception) ex);
      }
      catch (IOException ex)
      {
        if (NoClobber && File.Exists(str))
          cmdlet.ThrowTerminatingError(new ErrorRecord((Exception) ex, nameof (NoClobber), ErrorCategory.ResourceExists, (object) str)
          {
            ErrorDetails = new ErrorDetails((Cmdlet) cmdlet, nameof (PathUtils), "UtilityFileExistsNoClobber", new object[2]
            {
              (object) filePath,
              (object) nameof (NoClobber)
            })
          });
        PathUtils.ReportFileOpenFailure((Cmdlet) cmdlet, str, (Exception) ex);
      }
      catch (UnauthorizedAccessException ex)
      {
        PathUtils.ReportFileOpenFailure((Cmdlet) cmdlet, str, (Exception) ex);
      }
      catch (NotSupportedException ex)
      {
        PathUtils.ReportFileOpenFailure((Cmdlet) cmdlet, str, (Exception) ex);
      }
      catch (SecurityException ex)
      {
        PathUtils.ReportFileOpenFailure((Cmdlet) cmdlet, str, (Exception) ex);
      }
    }

    internal static void ReportFileOpenFailure(Cmdlet cmdlet, string filePath, Exception e)
    {
      ErrorRecord errorRecord = new ErrorRecord(e, "FileOpenFailure", ErrorCategory.OpenError, (object) null);
      cmdlet.ThrowTerminatingError(errorRecord);
    }

    internal static string ResolveFilePath(string filePath, PSCmdlet command)
    {
      string str;
      try
      {
        ProviderInfo provider = (ProviderInfo) null;
        Collection<string> providerPathFromPsPath = command.SessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out provider);
        if (!provider.NameEquals(command.Context.ProviderNames.FileSystem))
          PathUtils.ReportWrongProviderType((Cmdlet) command, provider.FullName);
        if (providerPathFromPsPath.Count > 1)
          PathUtils.ReportMultipleFilesNotSupported((Cmdlet) command);
        if (providerPathFromPsPath.Count == 0)
          PathUtils.ReportWildcardingFailure((Cmdlet) command, filePath);
        str = providerPathFromPsPath[0];
      }
      catch (ItemNotFoundException ex)
      {
        str = (string) null;
      }
      if (string.IsNullOrEmpty(str))
      {
        CmdletProviderContext context = new CmdletProviderContext((Cmdlet) command);
        ProviderInfo provider = (ProviderInfo) null;
        PSDriveInfo drive = (PSDriveInfo) null;
        str = command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(filePath, context, out provider, out drive);
        context.ThrowFirstErrorOrDoNothing();
        if (!provider.NameEquals(command.Context.ProviderNames.FileSystem))
          PathUtils.ReportWrongProviderType((Cmdlet) command, provider.FullName);
      }
      return str;
    }

    internal static void ReportWrongProviderType(Cmdlet cmdlet, string providerId)
    {
      string message = ResourceManagerCache.FormatResourceString(nameof (PathUtils), "OutFile_ReadWriteFileNotFileSystemProvider", (object) providerId);
      cmdlet.ThrowTerminatingError(new ErrorRecord((Exception) PathUtils.tracer.NewInvalidOperationException(), "ReadWriteFileNotFileSystemProvider", ErrorCategory.InvalidArgument, (object) null)
      {
        ErrorDetails = new ErrorDetails(message)
      });
    }

    internal static void ReportMultipleFilesNotSupported(Cmdlet cmdlet)
    {
      string message = ResourceManagerCache.FormatResourceString(nameof (PathUtils), "OutFile_MultipleFilesNotSupported");
      cmdlet.ThrowTerminatingError(new ErrorRecord((Exception) PathUtils.tracer.NewInvalidOperationException(), "ReadWriteMultipleFilesNotSupported", ErrorCategory.InvalidArgument, (object) null)
      {
        ErrorDetails = new ErrorDetails(message)
      });
    }

    internal static void ReportWildcardingFailure(Cmdlet cmdlet, string filePath)
    {
      string message = ResourceManagerCache.FormatResourceString(nameof (PathUtils), "OutFile_DidNotResolveFile", (object) filePath);
      cmdlet.ThrowTerminatingError(new ErrorRecord((Exception) new FileNotFoundException(), "FileOpenFailure", ErrorCategory.OpenError, (object) filePath)
      {
        ErrorDetails = new ErrorDetails(message)
      });
    }
  }
}
