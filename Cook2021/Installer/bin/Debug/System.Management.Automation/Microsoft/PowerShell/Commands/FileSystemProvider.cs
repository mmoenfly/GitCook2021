// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.FileSystemProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
  [OutputType(new Type[] {typeof (FileSecurity), typeof (DirectorySecurity)}, ProviderCmdlet = "Get-Acl")]
  [CmdletProvider("FileSystem", ProviderCapabilities.Filter | ProviderCapabilities.ShouldProcess)]
  [OutputType(new Type[] {typeof (FileInfo), typeof (DirectoryInfo), typeof (string)}, ProviderCmdlet = "Get-ChildItem")]
  [OutputType(new Type[] {typeof (FileInfo), typeof (DirectoryInfo)}, ProviderCmdlet = "Get-Item")]
  public sealed class FileSystemProvider : 
    NavigationCmdletProvider,
    IContentCmdletProvider,
    IPropertyCmdletProvider,
    ISecurityDescriptorCmdletProvider
  {
    public const string ProviderName = "FileSystem";
    [TraceSource("FileSystemProvider", "The namespace navigation provider for the file system")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (FileSystemProvider), "The namespace navigation provider for the file system");

    private static string NormalizePath(string path)
    {
      string str = path.Replace('/', '\\');
      FileSystemProvider.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    private static FileSystemInfo GetFileSystemInfo(string path, ref bool isContainer)
    {
      isContainer = false;
      if (File.Exists(path))
        return (FileSystemInfo) new FileInfo(path);
      if (!Directory.Exists(path))
        return (FileSystemInfo) null;
      isContainer = true;
      return (FileSystemInfo) new DirectoryInfo(path);
    }

    protected override ProviderInfo Start(ProviderInfo providerInfo)
    {
      if (providerInfo != null && string.IsNullOrEmpty(providerInfo.Home))
      {
        string environmentVariable1 = Environment.GetEnvironmentVariable("HOMEDRIVE");
        string environmentVariable2 = Environment.GetEnvironmentVariable("HOMEPATH");
        if (!string.IsNullOrEmpty(environmentVariable1) && !string.IsNullOrEmpty(environmentVariable2))
        {
          string path = this.MakePath(environmentVariable1, environmentVariable2);
          if (Directory.Exists(path))
          {
            FileSystemProvider.tracer.WriteLine("Home = {0}", (object) path);
            providerInfo.Home = path;
          }
          else
            FileSystemProvider.tracer.WriteLine("Not setting home directory {0} - does not exist", (object) path);
        }
      }
      return providerInfo;
    }

    protected override PSDriveInfo NewDrive(PSDriveInfo drive)
    {
      if (drive == (PSDriveInfo) null)
        throw FileSystemProvider.tracer.NewArgumentNullException(nameof (drive));
      if (string.IsNullOrEmpty(drive.Root))
        throw FileSystemProvider.tracer.NewArgumentException("drive.Root");
      bool flag1 = true;
      PSDriveInfo psDriveInfo = (PSDriveInfo) null;
      try
      {
        if (new DriveInfo(Path.GetPathRoot(drive.Root)).DriveType != DriveType.Fixed)
          flag1 = false;
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
      }
      bool flag2 = true;
      if (flag1)
        flag2 = this.ItemExists(drive.Root) && this.IsItemContainer(drive.Root);
      if (flag2)
        psDriveInfo = drive;
      else
        this.WriteError(new ErrorRecord((Exception) new IOException(StringUtil.Format(ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "DriveRootError"), (object) drive.Root)), "DriveRootError", ErrorCategory.ReadError, (object) drive));
      drive.Trace();
      return psDriveInfo;
    }

    protected override Collection<PSDriveInfo> InitializeDefaultDrives()
    {
      Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
      DriveInfo[] drives = DriveInfo.GetDrives();
      if (drives != null)
      {
        foreach (DriveInfo driveInfo in drives)
        {
          if (this.Stopping)
          {
            collection.Clear();
            break;
          }
          string name = driveInfo.Name.Substring(0, 1);
          string description = string.Empty;
          string root = driveInfo.Name;
          if (driveInfo.DriveType == DriveType.Fixed)
          {
            try
            {
              description = driveInfo.VolumeLabel;
            }
            catch (IOException ex)
            {
            }
            catch (SecurityException ex)
            {
            }
            catch (UnauthorizedAccessException ex)
            {
            }
          }
          try
          {
            if (driveInfo.DriveType == DriveType.Fixed)
            {
              if (driveInfo.RootDirectory.Exists)
                root = driveInfo.RootDirectory.FullName;
              else
                continue;
            }
            PSDriveInfo psDriveInfo = new PSDriveInfo(name, this.ProviderInfo, root, description, (PSCredential) null);
            if (driveInfo.DriveType != DriveType.Fixed)
              psDriveInfo.IsAutoMounted = true;
            collection.Add(psDriveInfo);
          }
          catch (IOException ex)
          {
          }
          catch (SecurityException ex)
          {
          }
          catch (UnauthorizedAccessException ex)
          {
          }
        }
      }
      return collection;
    }

    protected override bool IsValidPath(string path)
    {
      if (string.IsNullOrEmpty(path))
        return false;
      path = FileSystemProvider.NormalizePath(path);
      path = FileSystemProvider.EnsureDriveIsRooted(path);
      if (!FileSystemProvider.IsAbsolutePath(path))
      {
        if (!FileSystemProvider.IsUNCPath(path))
          return false;
      }
      try
      {
        FileInfo fileInfo = new FileInfo(path);
      }
      catch (Exception ex)
      {
        switch (ex)
        {
          case ArgumentNullException _:
          case ArgumentException _:
          case SecurityException _:
          case UnauthorizedAccessException _:
          case PathTooLongException _:
          case NotSupportedException _:
            return false;
          default:
            throw;
        }
      }
      return true;
    }

    protected override void GetItem(string path)
    {
      bool isContainer = false;
      if (string.IsNullOrEmpty(path))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      try
      {
        FileSystemInfo fileSystemItem = this.GetFileSystemItem(path, ref isContainer, false);
        if (fileSystemItem != null)
          this.WriteItemObject((object) fileSystemItem, fileSystemItem.FullName, isContainer);
        else
          this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "ItemNotFound", (object) path)), "ItemNotFound", ErrorCategory.ObjectNotFound, (object) path));
      }
      catch (IOException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "GetItemIOError", ErrorCategory.ReadError, (object) path));
      }
      catch (UnauthorizedAccessException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "GetItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
      }
    }

    private FileSystemInfo GetFileSystemItem(
      string path,
      ref bool isContainer,
      bool showHidden)
    {
      path = FileSystemProvider.NormalizePath(path);
      FileSystemInfo fileSystemInfo = (FileSystemInfo) null;
      FileInfo fileInfo = new FileInfo(path);
      bool exists1 = fileInfo.Exists;
      bool flag1 = (fileInfo.Attributes & FileAttributes.Directory) != (FileAttributes) 0;
      bool flag2 = (fileInfo.Attributes & FileAttributes.Hidden) != (FileAttributes) 0;
      if (exists1 && !flag1 && (!flag2 || (bool) this.Force || showHidden))
      {
        fileSystemInfo = (FileSystemInfo) fileInfo;
        FileSystemProvider.tracer.WriteLine("Got FileInfo: {0}", (object) fileInfo);
      }
      else
      {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        bool flag3 = string.Compare(Path.GetPathRoot(path), directoryInfo.FullName, StringComparison.OrdinalIgnoreCase) == 0;
        bool exists2 = directoryInfo.Exists;
        bool flag4 = (directoryInfo.Attributes & FileAttributes.Hidden) != (FileAttributes) 0;
        if (exists2 && (flag3 || !flag4 || ((bool) this.Force || showHidden)))
        {
          fileSystemInfo = (FileSystemInfo) directoryInfo;
          isContainer = true;
          FileSystemProvider.tracer.WriteLine("Got DirectoryInfo: {0}", (object) directoryInfo);
        }
      }
      return fileSystemInfo;
    }

    protected override void InvokeDefaultAction(string path)
    {
      path = !string.IsNullOrEmpty(path) ? FileSystemProvider.NormalizePath(path) : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "InvokeItemAction");
      if (!this.ShouldProcess(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "InvokeItemResourceFileTemplate", (object) path), resourceString))
        return;
      Process.Start(path);
    }

    protected override void GetChildItems(string path, bool recurse) => this.GetPathItems(path, recurse, false, ReturnContainers.ReturnMatchingContainers);

    protected override void GetChildNames(string path, ReturnContainers returnContainers) => this.GetPathItems(path, false, true, returnContainers);

    protected override bool ConvertPath(
      string path,
      string filter,
      ref string updatedPath,
      ref string updatedFilter)
    {
      if (!string.IsNullOrEmpty(filter) || path.Contains('\\'.ToString()) || (path.Contains('/'.ToString()) || path.Contains("`")))
        return false;
      updatedPath = path;
      updatedFilter = Regex.Replace(path, "\\[.*?\\]", "?");
      return true;
    }

    private void GetPathItems(
      string path,
      bool recurse,
      bool nameOnly,
      ReturnContainers returnContainers)
    {
      path = !string.IsNullOrEmpty(path) ? FileSystemProvider.NormalizePath(path) : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      DirectoryInfo directory = new DirectoryInfo(path);
      if (directory.Exists)
      {
        this.Dir(directory, recurse, nameOnly, returnContainers);
      }
      else
      {
        FileInfo fileInfo = new FileInfo(path);
        if (fileInfo.Exists)
        {
          if ((fileInfo.Attributes & FileAttributes.Hidden) != (FileAttributes) 0 && !(bool) this.Force)
            return;
          if (nameOnly)
            this.WriteItemObject((object) fileInfo.Name, fileInfo.FullName, false);
          else
            this.WriteItemObject((object) fileInfo, path, false);
        }
        else
          this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "ItemDoesNotExist", (object) path)), "ItemDoesNotExist", ErrorCategory.ObjectNotFound, (object) path));
      }
    }

    private void Dir(
      DirectoryInfo directory,
      bool recurse,
      bool nameOnly,
      ReturnContainers returnContainers)
    {
      DirectoryInfo[] directoryInfoArray = (DirectoryInfo[]) null;
      List<FileSystemInfo> fileSystemInfoList = new List<FileSystemInfo>();
      try
      {
        if (recurse)
          directoryInfoArray = directory.GetDirectories();
        if (this.Filter != null && this.Filter.Length > 0)
        {
          if (returnContainers == ReturnContainers.ReturnAllContainers)
            fileSystemInfoList.AddRange((IEnumerable<FileSystemInfo>) directory.GetDirectories());
          else
            fileSystemInfoList.AddRange((IEnumerable<FileSystemInfo>) directory.GetDirectories(this.Filter));
          if (this.Stopping)
            return;
          fileSystemInfoList.AddRange((IEnumerable<FileSystemInfo>) directory.GetFiles(this.Filter));
        }
        else
        {
          fileSystemInfoList.AddRange((IEnumerable<FileSystemInfo>) directory.GetDirectories());
          if (this.Stopping)
            return;
          fileSystemInfoList.AddRange((IEnumerable<FileSystemInfo>) directory.GetFiles());
        }
        foreach (FileSystemInfo fileSystemInfo in fileSystemInfoList)
        {
          if (this.Stopping)
            return;
          bool flag = false;
          if (!(bool) this.Force)
            flag = (fileSystemInfo.Attributes & FileAttributes.Hidden) != (FileAttributes) 0;
          if ((bool) this.Force || !flag)
          {
            if (nameOnly)
              this.WriteItemObject((object) fileSystemInfo.Name, fileSystemInfo.FullName, false);
            else if (fileSystemInfo is FileInfo)
              this.WriteItemObject((object) fileSystemInfo, fileSystemInfo.FullName, false);
            else
              this.WriteItemObject((object) fileSystemInfo, fileSystemInfo.FullName, true);
          }
        }
        if (!recurse || directoryInfoArray == null)
          return;
        foreach (DirectoryInfo directory1 in directoryInfoArray)
        {
          if (this.Stopping)
            break;
          bool flag = false;
          if (!(bool) this.Force)
            flag = (directory1.Attributes & FileAttributes.Hidden) != (FileAttributes) 0;
          if ((bool) this.Force || !flag)
            this.Dir(directory1, recurse, nameOnly, returnContainers);
        }
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "DirArgumentError", ErrorCategory.InvalidArgument, (object) directory.FullName));
      }
      catch (IOException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "DirIOError", ErrorCategory.ReadError, (object) directory.FullName));
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "DirUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) directory.FullName));
      }
    }

    public static string Mode(PSObject instance)
    {
      if (instance == null)
        return string.Empty;
      FileSystemInfo baseObject = (FileSystemInfo) instance.BaseObject;
      if (baseObject == null)
        return string.Empty;
      string str1 = "";
      string str2 = (baseObject.Attributes & FileAttributes.Directory) != FileAttributes.Directory ? str1 + "-" : str1 + "d";
      string str3 = (baseObject.Attributes & FileAttributes.Archive) != FileAttributes.Archive ? str2 + "-" : str2 + "a";
      string str4 = (baseObject.Attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly ? str3 + "-" : str3 + "r";
      string str5 = (baseObject.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden ? str4 + "-" : str4 + "h";
      return (baseObject.Attributes & FileAttributes.System) != FileAttributes.System ? str5 + "-" : str5 + "s";
    }

    protected override void RenameItem(string path, string newName)
    {
      path = !string.IsNullOrEmpty(path) ? FileSystemProvider.NormalizePath(path) : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      if (string.IsNullOrEmpty(newName))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (newName));
      if (newName.StartsWith(".\\", StringComparison.OrdinalIgnoreCase) || newName.StartsWith("./", StringComparison.OrdinalIgnoreCase))
        newName = newName.Remove(0, 2);
      else if (string.Equals(Path.GetDirectoryName(path), Path.GetDirectoryName(newName), StringComparison.OrdinalIgnoreCase))
        newName = Path.GetFileName(newName);
      if (string.Compare(Path.GetFileName(newName), newName, StringComparison.OrdinalIgnoreCase) != 0)
        throw FileSystemProvider.tracer.NewArgumentException(nameof (newName), "FileSystemProviderStrings", "RenameError");
      if (this.IsReservedDeviceName(newName))
      {
        this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "TargetCannotContainDeviceName", (object) newName)), "RenameError", ErrorCategory.WriteError, (object) newName));
      }
      else
      {
        try
        {
          bool isContainer = this.IsItemContainer(path);
          if (isContainer)
          {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            string destDirName = this.MakePath(directoryInfo.Parent.FullName, newName);
            string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "RenameItemActionDirectory");
            if (!this.ShouldProcess(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "RenameItemResourceFileTemplate", (object) directoryInfo.FullName, (object) destDirName), resourceString))
              return;
            directoryInfo.MoveTo(destDirName);
            FileSystemInfo fileSystemInfo = (FileSystemInfo) directoryInfo;
            this.WriteItemObject((object) fileSystemInfo, fileSystemInfo.FullName, isContainer);
          }
          else
          {
            FileInfo fileInfo = new FileInfo(path);
            string destFileName = this.MakePath(fileInfo.DirectoryName, newName);
            string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "RenameItemActionFile");
            if (!this.ShouldProcess(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "RenameItemResourceFileTemplate", (object) fileInfo.FullName, (object) destFileName), resourceString))
              return;
            fileInfo.MoveTo(destFileName);
            FileSystemInfo fileSystemInfo = (FileSystemInfo) fileInfo;
            this.WriteItemObject((object) fileSystemInfo, fileSystemInfo.FullName, isContainer);
          }
        }
        catch (ArgumentException ex)
        {
          FileSystemProvider.tracer.TraceException((Exception) ex);
          this.WriteError(new ErrorRecord((Exception) ex, "RenameItemArgumentError", ErrorCategory.InvalidArgument, (object) path));
        }
        catch (IOException ex)
        {
          FileSystemProvider.tracer.TraceException((Exception) ex);
          this.WriteError(new ErrorRecord((Exception) ex, "RenameItemIOError", ErrorCategory.WriteError, (object) path));
        }
        catch (UnauthorizedAccessException ex)
        {
          FileSystemProvider.tracer.TraceException((Exception) ex);
          this.WriteError(new ErrorRecord((Exception) ex, "RenameItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
        }
      }
    }

    protected override void NewItem(string path, string type, object value)
    {
      if (string.IsNullOrEmpty(path))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      if (string.IsNullOrEmpty(type))
      {
        type = this.PromptNewItemType();
        if (string.IsNullOrEmpty(type))
          throw FileSystemProvider.tracer.NewArgumentException(nameof (type));
      }
      path = FileSystemProvider.NormalizePath(path);
      if ((bool) this.Force && !this.CreateIntermediateDirectories(path))
        return;
      switch (FileSystemProvider.GetItemType(type))
      {
        case FileSystemProvider.ItemType.File:
          try
          {
            FileMode mode = FileMode.CreateNew;
            if ((bool) this.Force)
              mode = FileMode.Create;
            string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "NewItemActionFile");
            if (!this.ShouldProcess(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "NewItemActionTemplate", (object) path), resourceString))
              break;
            using (FileStream fileStream = new FileStream(path, mode, FileAccess.Write, FileShare.None))
            {
              if (value != null)
              {
                StreamWriter streamWriter = new StreamWriter((Stream) fileStream);
                streamWriter.Write(value.ToString());
                streamWriter.Flush();
                streamWriter.Close();
              }
            }
            this.WriteItemObject((object) new FileInfo(path), path, false);
            break;
          }
          catch (IOException ex)
          {
            FileSystemProvider.tracer.TraceException((Exception) ex);
            this.WriteError(new ErrorRecord((Exception) ex, "NewItemIOError", ErrorCategory.WriteError, (object) path));
            break;
          }
          catch (UnauthorizedAccessException ex)
          {
            FileSystemProvider.tracer.TraceException((Exception) ex);
            this.WriteError(new ErrorRecord((Exception) ex, "NewItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
            break;
          }
        case FileSystemProvider.ItemType.Directory:
          this.CreateDirectory(path, true);
          break;
        default:
          throw FileSystemProvider.tracer.NewArgumentException(nameof (type), "FileSystemProviderStrings", "UnknownType");
      }
    }

    private string PromptNewItemType()
    {
      string str = (string) null;
      if (this.Host != null)
      {
        FieldDescription fieldDescription = new FieldDescription("Type");
        fieldDescription.SetParameterType(typeof (string));
        Collection<FieldDescription> descriptions = new Collection<FieldDescription>();
        descriptions.Add(fieldDescription);
        try
        {
          Dictionary<string, PSObject> dictionary = this.Host.UI.Prompt(string.Empty, string.Empty, descriptions);
          if (dictionary != null)
          {
            if (dictionary.Count > 0)
            {
              using (Dictionary<string, PSObject>.ValueCollection.Enumerator enumerator = dictionary.Values.GetEnumerator())
              {
                if (enumerator.MoveNext())
                  str = (string) LanguagePrimitives.ConvertTo((object) enumerator.Current, typeof (string), (IFormatProvider) Thread.CurrentThread.CurrentCulture);
              }
            }
          }
        }
        catch (NotImplementedException ex)
        {
        }
      }
      return str;
    }

    private static FileSystemProvider.ItemType GetItemType(string input)
    {
      FileSystemProvider.ItemType itemType = FileSystemProvider.ItemType.Unknown;
      WildcardPattern wildcardPattern = new WildcardPattern(input + "*", WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
      if (wildcardPattern.IsMatch("directory") || wildcardPattern.IsMatch("container"))
        itemType = FileSystemProvider.ItemType.Directory;
      else if (wildcardPattern.IsMatch("file"))
        itemType = FileSystemProvider.ItemType.File;
      return itemType;
    }

    private void CreateDirectory(string path, bool streamOutput)
    {
      string parentPath = this.GetParentPath(path, (string) null);
      string childName = this.GetChildName(path);
      ErrorRecord error = (ErrorRecord) null;
      if (!(bool) this.Force && this.ItemExists(path, out error))
        this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "DirectoryExist", (object) path)), "DirectoryExist", ErrorCategory.ResourceExists, (object) path));
      else if (error != null)
      {
        this.WriteError(error);
      }
      else
      {
        try
        {
          string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "NewItemActionDirectory");
          if (!this.ShouldProcess(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "NewItemActionTemplate", (object) path), resourceString))
            return;
          DirectoryInfo subdirectory = new DirectoryInfo(parentPath).CreateSubdirectory(childName);
          if (!streamOutput)
            return;
          this.WriteItemObject((object) subdirectory, path, true);
        }
        catch (ArgumentException ex)
        {
          FileSystemProvider.tracer.TraceException((Exception) ex);
          this.WriteError(new ErrorRecord((Exception) ex, "CreateDirectoryArgumentError", ErrorCategory.InvalidArgument, (object) path));
        }
        catch (IOException ex)
        {
          if ((bool) this.Force)
            return;
          FileSystemProvider.tracer.TraceException((Exception) ex);
          this.WriteError(new ErrorRecord((Exception) ex, "CreateDirectoryIOError", ErrorCategory.WriteError, (object) path));
        }
        catch (UnauthorizedAccessException ex)
        {
          FileSystemProvider.tracer.TraceException((Exception) ex);
          this.WriteError(new ErrorRecord((Exception) ex, "CreateDirectoryUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
        }
      }
    }

    private bool CreateIntermediateDirectories(string path)
    {
      bool flag = false;
      if (string.IsNullOrEmpty(path))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      try
      {
        Stack<string> stringStack = new Stack<string>();
        string strB = path;
        do
        {
          string root = string.Empty;
          if (this.PSDriveInfo != (PSDriveInfo) null)
            root = this.PSDriveInfo.Root;
          string parentPath = this.GetParentPath(path, root);
          if (!string.IsNullOrEmpty(parentPath) && string.Compare(parentPath, strB, StringComparison.OrdinalIgnoreCase) != 0 && !this.ItemExists(parentPath))
          {
            stringStack.Push(parentPath);
            strB = parentPath;
          }
          else
            break;
        }
        while (!string.IsNullOrEmpty(strB));
        foreach (string path1 in stringStack)
          this.CreateDirectory(path1, false);
        flag = true;
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "CreateIntermediateDirectoriesArgumentError", ErrorCategory.InvalidArgument, (object) path));
      }
      catch (IOException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "CreateIntermediateDirectoriesIOError", ErrorCategory.WriteError, (object) path));
      }
      catch (UnauthorizedAccessException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "CreateIntermediateDirectoriesUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
      }
      FileSystemProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    protected override void RemoveItem(string path, bool recurse)
    {
      if (string.IsNullOrEmpty(path))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      try
      {
        path = FileSystemProvider.NormalizePath(path);
        bool isContainer = false;
        FileSystemInfo fileSystemInfo = FileSystemProvider.GetFileSystemInfo(path, ref isContainer);
        if (fileSystemInfo == null)
          this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "ItemDoesNotExist", (object) path)), "ItemDoesNotExist", ErrorCategory.ObjectNotFound, (object) path));
        else if (isContainer)
          this.RemoveDirectoryInfoItem((DirectoryInfo) fileSystemInfo, recurse, (bool) this.Force, true);
        else
          this.RemoveFileInfoItem((FileInfo) fileSystemInfo, (bool) this.Force);
      }
      catch (IOException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "RemoveItemIOError", ErrorCategory.WriteError, (object) path));
      }
      catch (UnauthorizedAccessException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "RemoveItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
      }
    }

    private void RemoveDirectoryInfoItem(
      DirectoryInfo directory,
      bool recurse,
      bool force,
      bool rootOfRemoval)
    {
      bool flag = true;
      if (rootOfRemoval || recurse)
      {
        string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "RemoveItemActionDirectory");
        flag = this.ShouldProcess(directory.FullName, resourceString);
      }
      if ((directory.Attributes & FileAttributes.ReparsePoint) != (FileAttributes) 0 && !(bool) this.Force)
      {
        this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "DirectoryReparsePoint", (object) directory.FullName)), "DirectoryNotEmpty", ErrorCategory.WriteError, (object) directory));
      }
      else
      {
        if (!flag)
          return;
        DirectoryInfo[] directories = directory.GetDirectories();
        for (int index = 0; index < directories.Length; ++index)
        {
          if (this.Stopping)
            return;
          if (directories[index] != null)
            this.RemoveDirectoryInfoItem(directories[index], recurse, force, false);
        }
        FileInfo[] fileInfoArray = string.IsNullOrEmpty(this.Filter) ? directory.GetFiles() : directory.GetFiles(this.Filter);
        for (int index = 0; index < fileInfoArray.Length; ++index)
        {
          if (this.Stopping)
            return;
          if (fileInfoArray[index] != null)
          {
            if (recurse)
              this.RemoveFileInfoItem(fileInfoArray[index], force);
            else
              this.RemoveFileSystemItem((FileSystemInfo) fileInfoArray[index], force);
          }
        }
        if (FileSystemProvider.DirectoryInfoHasChildItems(directory) && !force)
          this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "DirectoryNotEmpty", (object) directory.FullName)), "DirectoryNotEmpty", ErrorCategory.WriteError, (object) directory));
        else
          this.RemoveFileSystemItem((FileSystemInfo) directory, force);
      }
    }

    private void RemoveFileInfoItem(FileInfo file, bool force)
    {
      string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "RemoveItemActionFile");
      if (!this.ShouldProcess(file.FullName, resourceString))
        return;
      this.RemoveFileSystemItem((FileSystemInfo) file, force);
    }

    private void RemoveFileSystemItem(FileSystemInfo fileSystemInfo, bool force)
    {
      if (!(bool) this.Force && (fileSystemInfo.Attributes & (FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System)) != (FileAttributes) 0)
      {
        Exception exception = (Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "PermissionError"));
        ErrorDetails errorDetails = new ErrorDetails((IResourceSupplier) this, "FileSystemProviderStrings", "CannotRemoveItem", new object[2]
        {
          (object) fileSystemInfo.FullName,
          (object) exception.Message
        });
        this.WriteError(new ErrorRecord(exception, "RemoveFileSystemItemUnAuthorizedAccess", ErrorCategory.PermissionDenied, (object) fileSystemInfo)
        {
          ErrorDetails = errorDetails
        });
      }
      else
      {
        FileAttributes attributes = fileSystemInfo.Attributes;
        bool flag = false;
        try
        {
          if (force)
          {
            fileSystemInfo.Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System);
            flag = true;
          }
          fileSystemInfo.Delete();
          if (!force)
            return;
          flag = false;
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          ErrorDetails errorDetails = new ErrorDetails((IResourceSupplier) this, "FileSystemProviderStrings", "CannotRemoveItem", new object[2]
          {
            (object) fileSystemInfo.FullName,
            (object) ex.Message
          });
          switch (ex)
          {
            case SecurityException _:
            case UnauthorizedAccessException _:
              FileSystemProvider.tracer.TraceException(ex);
              this.WriteError(new ErrorRecord(ex, "RemoveFileSystemItemUnAuthorizedAccess", ErrorCategory.PermissionDenied, (object) fileSystemInfo)
              {
                ErrorDetails = errorDetails
              });
              break;
            case ArgumentException _:
              FileSystemProvider.tracer.TraceException(ex);
              this.WriteError(new ErrorRecord(ex, "RemoveFileSystemItemArgumentError", ErrorCategory.InvalidArgument, (object) fileSystemInfo)
              {
                ErrorDetails = errorDetails
              });
              break;
            case IOException _:
            case FileNotFoundException _:
            case DirectoryNotFoundException _:
              FileSystemProvider.tracer.TraceException(ex);
              this.WriteError(new ErrorRecord(ex, "RemoveFileSystemItemIOError", ErrorCategory.WriteError, (object) fileSystemInfo)
              {
                ErrorDetails = errorDetails
              });
              break;
            default:
              throw;
          }
        }
        finally
        {
          if (flag)
          {
            try
            {
              if (fileSystemInfo.Exists)
                fileSystemInfo.Attributes = attributes;
            }
            catch (Exception ex)
            {
              CommandProcessorBase.CheckForSevereException(ex);
              switch (ex)
              {
                case DirectoryNotFoundException _:
                case SecurityException _:
                case ArgumentException _:
                case FileNotFoundException _:
                case IOException _:
                  ErrorDetails errorDetails = new ErrorDetails((IResourceSupplier) this, "FileSystemProviderStrings", "CannotRestoreAttributes", new object[2]
                  {
                    (object) fileSystemInfo.FullName,
                    (object) ex.Message
                  });
                  FileSystemProvider.tracer.TraceException(ex);
                  this.WriteError(new ErrorRecord(ex, "RemoveFileSystemItemCannotRestoreAttributes", ErrorCategory.PermissionDenied, (object) fileSystemInfo)
                  {
                    ErrorDetails = errorDetails
                  });
                  break;
                default:
                  throw;
              }
            }
          }
        }
      }
    }

    protected override bool ItemExists(string path)
    {
      ErrorRecord error = (ErrorRecord) null;
      bool flag = this.ItemExists(path, out error);
      if (error != null)
        this.WriteError(error);
      return flag;
    }

    private bool ItemExists(string path, out ErrorRecord error)
    {
      error = (ErrorRecord) null;
      if (string.IsNullOrEmpty(path))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      bool flag = false;
      path = FileSystemProvider.NormalizePath(path);
      try
      {
        if (new FileInfo(path).Exists)
          flag = true;
        else if (new DirectoryInfo(path).Exists)
          flag = true;
      }
      catch (SecurityException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        error = new ErrorRecord((Exception) ex, "ItemExistsSecurityError", ErrorCategory.PermissionDenied, (object) path);
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        error = new ErrorRecord((Exception) ex, "ItemExistsArgumentError", ErrorCategory.InvalidArgument, (object) path);
      }
      catch (UnauthorizedAccessException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        error = new ErrorRecord((Exception) ex, "ItemExistsUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path);
      }
      catch (PathTooLongException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        error = new ErrorRecord((Exception) ex, "ItemExistsPathTooLongError", ErrorCategory.InvalidArgument, (object) path);
      }
      catch (NotSupportedException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        error = new ErrorRecord((Exception) ex, "ItemExistsNotSupportedError", ErrorCategory.InvalidOperation, (object) path);
      }
      catch (Exception ex)
      {
        FileSystemProvider.tracer.TraceException(ex);
        throw;
      }
      return flag;
    }

    protected override bool HasChildItems(string path)
    {
      path = !string.IsNullOrEmpty(path) ? FileSystemProvider.NormalizePath(path) : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      try
      {
        return FileSystemProvider.DirectoryInfoHasChildItems(new DirectoryInfo(path));
      }
      catch (ArgumentNullException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        return false;
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        return false;
      }
      catch (UnauthorizedAccessException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        return false;
      }
      catch (IOException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        return false;
      }
      catch (Exception ex)
      {
        FileSystemProvider.tracer.TraceException(ex);
        throw;
      }
    }

    private static bool DirectoryInfoHasChildItems(DirectoryInfo directory)
    {
      bool flag = false;
      if (directory.GetFileSystemInfos().Length > 0)
        flag = true;
      FileSystemProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    protected override void CopyItem(string path, string destinationPath, bool recurse)
    {
      if (string.IsNullOrEmpty(path))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      if (string.IsNullOrEmpty(destinationPath))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (destinationPath));
      path = FileSystemProvider.NormalizePath(path);
      destinationPath = FileSystemProvider.NormalizePath(destinationPath);
      if (path.Equals(destinationPath, StringComparison.OrdinalIgnoreCase))
        this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "CopyError", (object) path)), "CopyError", ErrorCategory.WriteError, (object) path));
      else if (this.IsItemContainer(path))
        this.CopyDirectoryInfoItem(new DirectoryInfo(path), destinationPath, recurse, (bool) this.Force);
      else
        this.CopyFileInfoItem(new FileInfo(path), destinationPath, (bool) this.Force);
    }

    private void CopyDirectoryInfoItem(
      DirectoryInfo directory,
      string destination,
      bool recurse,
      bool force)
    {
      if (this.IsItemContainer(destination))
        destination = this.MakePath(destination, directory.Name);
      FileSystemProvider.tracer.WriteLine("destination = {0}", (object) destination);
      string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "CopyItemActionDirectory");
      if (!this.ShouldProcess(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "CopyItemResourceFileTemplate", (object) directory.FullName, (object) destination), resourceString))
        return;
      this.CreateDirectory(destination, true);
      if (!recurse)
        return;
      FileInfo[] fileInfoArray = !string.IsNullOrEmpty(this.Filter) ? directory.GetFiles(this.Filter) : directory.GetFiles();
      for (int index = 0; index < fileInfoArray.Length; ++index)
      {
        if (this.Stopping)
          return;
        if (fileInfoArray[index] != null)
        {
          try
          {
            this.CopyFileInfoItem(fileInfoArray[index], destination, force);
          }
          catch (ArgumentException ex)
          {
            FileSystemProvider.tracer.TraceException((Exception) ex);
            this.WriteError(new ErrorRecord((Exception) ex, "CopyDirectoryInfoItemArgumentError", ErrorCategory.InvalidArgument, (object) fileInfoArray[index]));
          }
          catch (IOException ex)
          {
            FileSystemProvider.tracer.TraceException((Exception) ex);
            this.WriteError(new ErrorRecord((Exception) ex, "CopyDirectoryInfoItemIOError", ErrorCategory.WriteError, (object) fileInfoArray[index]));
          }
          catch (UnauthorizedAccessException ex)
          {
            FileSystemProvider.tracer.TraceException((Exception) ex);
            this.WriteError(new ErrorRecord((Exception) ex, "CopyDirectoryInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) fileInfoArray[index]));
          }
        }
      }
      DirectoryInfo[] directories = directory.GetDirectories();
      for (int index = 0; index < directories.Length && !this.Stopping; ++index)
      {
        if (directories[index] != null)
        {
          try
          {
            this.CopyDirectoryInfoItem(directories[index], destination, recurse, force);
          }
          catch (ArgumentException ex)
          {
            FileSystemProvider.tracer.TraceException((Exception) ex);
            this.WriteError(new ErrorRecord((Exception) ex, "CopyDirectoryInfoItemArgumentError", ErrorCategory.InvalidArgument, (object) directories[index]));
          }
          catch (IOException ex)
          {
            FileSystemProvider.tracer.TraceException((Exception) ex);
            this.WriteError(new ErrorRecord((Exception) ex, "CopyDirectoryInfoItemIOError", ErrorCategory.WriteError, (object) directories[index]));
          }
          catch (UnauthorizedAccessException ex)
          {
            FileSystemProvider.tracer.TraceException((Exception) ex);
            this.WriteError(new ErrorRecord((Exception) ex, "CopyDirectoryInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) directories[index]));
          }
        }
      }
    }

    private bool IsReservedDeviceName(string destinationPath)
    {
      string[] strArray = new string[25]
      {
        "CON",
        "PRN",
        "AUX",
        "CLOCK$",
        "NUL",
        "COM0",
        "COM1",
        "COM2",
        "COM3",
        "COM4",
        "COM5",
        "COM6",
        "COM7",
        "COM8",
        "COM9",
        "LPT0",
        "LPT1",
        "LPT2",
        "LPT3",
        "LPT4",
        "LPT5",
        "LPT6",
        "LPT7",
        "LPT8",
        "LPT9"
      };
      foreach (string a in strArray)
      {
        if (string.Equals(a, Path.GetFileName(destinationPath), StringComparison.OrdinalIgnoreCase) || string.Equals(a, Path.GetFileNameWithoutExtension(destinationPath), StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    private void CopyFileInfoItem(FileInfo file, string destinationPath, bool force)
    {
      if (this.IsItemContainer(destinationPath))
        destinationPath = this.MakePath(destinationPath, file.Name);
      if (destinationPath.Equals(file.FullName, StringComparison.OrdinalIgnoreCase))
        this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "CopyError", (object) destinationPath)), "CopyError", ErrorCategory.WriteError, (object) destinationPath));
      else if (this.IsReservedDeviceName(destinationPath))
      {
        this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "TargetCannotContainDeviceName", (object) destinationPath)), "CopyError", ErrorCategory.WriteError, (object) destinationPath));
      }
      else
      {
        string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "CopyItemActionFile");
        if (!this.ShouldProcess(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "CopyItemResourceFileTemplate", (object) file.FullName, (object) destinationPath), resourceString))
          return;
        try
        {
          file.CopyTo(destinationPath, true);
          this.WriteItemObject((object) new FileInfo(destinationPath), destinationPath, false);
        }
        catch (UnauthorizedAccessException ex1)
        {
          if (force)
          {
            try
            {
              new FileInfo(destinationPath).Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden);
            }
            catch (Exception ex2)
            {
              switch (ex2)
              {
                case FileNotFoundException _:
                case DirectoryNotFoundException _:
                case SecurityException _:
                case ArgumentException _:
                case IOException _:
                  FileSystemProvider.tracer.TraceException(ex2);
                  this.WriteError(new ErrorRecord((Exception) ex1, "CopyFileInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) file));
                  break;
                default:
                  throw;
              }
            }
            file.CopyTo(destinationPath, true);
            this.WriteItemObject((object) new FileInfo(destinationPath), destinationPath, false);
          }
          else
          {
            FileSystemProvider.tracer.TraceException((Exception) ex1);
            this.WriteError(new ErrorRecord((Exception) ex1, "CopyFileInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) file));
          }
        }
      }
    }

    protected override string GetParentPath(string path, string root)
    {
      string path1 = base.GetParentPath(path, root);
      if (FileSystemProvider.IsUNCPath(path))
      {
        if (path1.LastIndexOf('\\') < 3)
          path1 = string.Empty;
      }
      else
        path1 = FileSystemProvider.EnsureDriveIsRooted(path1);
      return path1;
    }

    private static bool IsAbsolutePath(string path)
    {
      bool flag = false;
      if (path.IndexOf(':') != -1)
        flag = true;
      FileSystemProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    private static bool IsUNCPath(string path)
    {
      bool flag = false;
      if (path.StartsWith("\\\\", StringComparison.Ordinal))
        flag = true;
      FileSystemProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    private static bool IsUNCRoot(string path)
    {
      bool flag = false;
      if (!string.IsNullOrEmpty(path) && FileSystemProvider.IsUNCPath(path))
      {
        int startIndex = path.Length - 1;
        if (path[path.Length - 1] == '\\')
          --startIndex;
        int num1 = 0;
        do
        {
          int num2 = path.LastIndexOf('\\', startIndex);
          if (num2 != -1)
          {
            startIndex = num2 - 1;
            if (startIndex >= 3)
              ++num1;
            else
              break;
          }
          else
            break;
        }
        while (startIndex > 3);
        if (num1 == 1)
          flag = true;
      }
      FileSystemProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    private static bool IsPathRoot(string path)
    {
      if (string.IsNullOrEmpty(path))
        return false;
      bool flag1 = string.Equals(path, Path.GetPathRoot(path), StringComparison.OrdinalIgnoreCase);
      bool flag2 = FileSystemProvider.IsUNCRoot(path);
      bool flag3 = flag1 || flag2;
      FileSystemProvider.tracer.WriteLine("result = {0}; isDriveRoot = {1}; isUNCRoot = {2}", (object) flag3, (object) flag1, (object) flag2);
      return flag3;
    }

    protected override string NormalizeRelativePath(string path, string basePath)
    {
      if (string.IsNullOrEmpty(path) || !this.IsValidPath(path))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      if (basePath == null)
        basePath = string.Empty;
      FileSystemProvider.tracer.WriteLine("basePath = {0}", (object) basePath);
      path = FileSystemProvider.NormalizePath(path);
      path = FileSystemProvider.EnsureDriveIsRooted(path);
      path = this.NormalizeRelativePathHelper(path, basePath);
      basePath = FileSystemProvider.NormalizePath(basePath);
      basePath = FileSystemProvider.EnsureDriveIsRooted(basePath);
      string str1;
      try
      {
        str1 = path;
        string str2 = path;
        if (!str2.EndsWith(string.Concat((object) '\\'), StringComparison.OrdinalIgnoreCase))
          str2 += (string) (object) '\\';
        string str3 = basePath;
        if (!str3.EndsWith(string.Concat((object) '\\'), StringComparison.OrdinalIgnoreCase))
          str3 += (string) (object) '\\';
        if (str2.StartsWith(str3, StringComparison.OrdinalIgnoreCase))
        {
          if (!string.IsNullOrEmpty(str1))
          {
            if (!FileSystemProvider.IsUNCPath(str1) && !str1.StartsWith(basePath, StringComparison.CurrentCulture))
              str1 = this.MakePath(basePath, str1);
            if (FileSystemProvider.IsPathRoot(str1))
            {
              str1 = FileSystemProvider.EnsureDriveIsRooted(str1);
            }
            else
            {
              string parentPath = this.GetParentPath(str1, string.Empty);
              if (string.IsNullOrEmpty(parentPath))
                return string.Empty;
              string childName = this.GetChildName(str1);
              string[] strArray = Directory.GetFiles(parentPath, childName);
              if (strArray == null || strArray.Length == 0)
                strArray = Directory.GetDirectories(parentPath, childName);
              if (strArray == null || strArray.Length == 0)
              {
                this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "ItemDoesNotExist", (object) path)), "ItemDoesNotExist", ErrorCategory.ObjectNotFound, (object) path));
              }
              else
              {
                str1 = strArray[0];
                if (str1.StartsWith(basePath, StringComparison.CurrentCulture))
                  str1 = str1.Substring(basePath.Length);
                else
                  this.WriteError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "PathOutSideBasePath", (object) path)), "PathOutSideBasePath", ErrorCategory.InvalidArgument, (object) null));
              }
            }
          }
        }
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "NormalizeRelativePathArgumentError", ErrorCategory.InvalidArgument, (object) path));
      }
      catch (DirectoryNotFoundException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "NormalizeRelativePathDirectoryNotFoundError", ErrorCategory.ObjectNotFound, (object) path));
      }
      catch (IOException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "NormalizeRelativePathIOError", ErrorCategory.ReadError, (object) path));
      }
      catch (UnauthorizedAccessException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "NormalizeRelativePathUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
      }
      FileSystemProvider.tracer.WriteLine("result = {0}", (object) str1);
      return str1;
    }

    private string NormalizeRelativePathHelper(string path, string basePath)
    {
      switch (path)
      {
        case "":
          return string.Empty;
        case null:
          throw FileSystemProvider.tracer.NewArgumentNullException(nameof (path));
        default:
          if (basePath == null)
            basePath = string.Empty;
          FileSystemProvider.tracer.WriteLine("basePath = {0}", (object) basePath);
          path = path.Replace('/', '\\');
          string str1 = path;
          path = path.TrimEnd('\\');
          basePath = basePath.Replace('/', '\\');
          basePath = basePath.TrimEnd('\\');
          path = this.RemoveRelativeTokens(path);
          string str2;
          if (string.Equals(path, basePath, StringComparison.OrdinalIgnoreCase) && !str1.EndsWith(string.Concat((object) '\\'), StringComparison.OrdinalIgnoreCase))
            str2 = this.MakePath("..", this.GetChildName(path));
          else if (!(path + (object) '\\').StartsWith(basePath + (object) '\\', StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(basePath))
          {
            str2 = string.Empty;
            string commonBase = this.GetCommonBase(path, basePath);
            int count = this.TokenizePathToStack(basePath, commonBase).Count;
            if (string.IsNullOrEmpty(commonBase))
              --count;
            for (int index = 0; index < count; ++index)
              str2 = this.MakePath("..", str2);
            if (!string.IsNullOrEmpty(commonBase))
            {
              if (string.Equals(path, commonBase, StringComparison.OrdinalIgnoreCase) && !path.EndsWith(string.Concat((object) '\\'), StringComparison.OrdinalIgnoreCase))
              {
                string childName = this.GetChildName(path);
                str2 = this.MakePath(this.MakePath("..", str2), childName);
              }
              else
              {
                foreach (string child in this.TokenizePathToStack(path, commonBase).ToArray())
                  str2 = this.MakePath(str2, child);
              }
            }
          }
          else if (FileSystemProvider.IsPathRoot(path))
          {
            str2 = !string.IsNullOrEmpty(basePath) ? string.Empty : path;
          }
          else
          {
            Stack<string> stack = this.TokenizePathToStack(path, basePath);
            Stack<string> stringStack = new Stack<string>();
            Stack<string> normalizedPathStack;
            try
            {
              normalizedPathStack = this.NormalizeThePath(basePath, stack);
            }
            catch (ArgumentException ex)
            {
              this.WriteError(new ErrorRecord((Exception) ex, "NormalizeRelativePathHelperArgumentError", ErrorCategory.InvalidArgument, (object) null));
              str2 = (string) null;
              goto label_25;
            }
            str2 = this.CreateNormalizedRelativePathFromStack(normalizedPathStack);
          }
label_25:
          FileSystemProvider.tracer.WriteLine("result = {0}", (object) str2);
          return str2;
      }
    }

    private string RemoveRelativeTokens(string path) => this.CreateNormalizedRelativePathFromStack(this.NormalizeThePath("", this.TokenizePathToStack(path, "")));

    private string GetCommonBase(string path1, string path2)
    {
      while (!string.Equals(path1, path2, StringComparison.OrdinalIgnoreCase))
      {
        if (path2.Length > path1.Length)
          path2 = this.GetParentPath(path2, (string) null);
        else
          path1 = this.GetParentPath(path1, (string) null);
      }
      return path1;
    }

    private Stack<string> TokenizePathToStack(string path, string basePath)
    {
      Stack<string> stringStack = new Stack<string>();
      string path1 = path;
      string str = path;
      while (path1.Length > basePath.Length)
      {
        string childName = this.GetChildName(path1);
        if (string.IsNullOrEmpty(childName))
        {
          FileSystemProvider.tracer.WriteLine("tokenizedPathStack.Push({0})", (object) path1);
          stringStack.Push(path1);
          break;
        }
        FileSystemProvider.tracer.WriteLine("tokenizedPathStack.Push({0})", (object) childName);
        stringStack.Push(childName);
        path1 = this.GetParentPath(path1, basePath);
        if (path1.Length >= str.Length || FileSystemProvider.IsPathRoot(path1))
        {
          if (string.IsNullOrEmpty(basePath))
          {
            FileSystemProvider.tracer.WriteLine("tokenizedPathStack.Push({0})", (object) path1);
            stringStack.Push(path1);
            break;
          }
          break;
        }
        str = path1;
      }
      return stringStack;
    }

    private Stack<string> NormalizeThePath(string basepath, Stack<string> tokenizedPathStack)
    {
      Stack<string> stringStack = new Stack<string>();
      string str1 = basepath;
      while (tokenizedPathStack.Count > 0)
      {
        string child = tokenizedPathStack.Pop();
        FileSystemProvider.tracer.WriteLine("childName = {0}", (object) child);
        if (!child.Equals(".", StringComparison.OrdinalIgnoreCase))
        {
          if (child.Equals("..", StringComparison.OrdinalIgnoreCase))
          {
            string str2 = stringStack.Count > 0 ? stringStack.Pop() : throw FileSystemProvider.tracer.NewArgumentException("path", "FileSystemProviderStrings", "PathOutSideBasePath");
            str1 = str1.Length <= str2.Length ? "" : str1.Substring(0, str1.Length - str2.Length - 1);
            FileSystemProvider.tracer.WriteLine("normalizedPathStack.Pop() : {0}", (object) str2);
          }
          else
          {
            str1 = this.MakePath(str1, child);
            bool isContainer = false;
            FileSystemInfo fileSystemInfo = FileSystemProvider.GetFileSystemInfo(str1, ref isContainer);
            if (fileSystemInfo != null)
            {
              if (fileSystemInfo.FullName.Length < str1.Length)
                throw FileSystemProvider.tracer.NewArgumentException("path", "FileSystemProviderStrings", "ItemDoesNotExist", (object) str1);
              if (fileSystemInfo.Name.Length >= child.Length)
                child = fileSystemInfo.Name;
            }
            else if (!isContainer && tokenizedPathStack.Count == 0)
              throw FileSystemProvider.tracer.NewArgumentException("path", "FileSystemProviderStrings", "ItemDoesNotExist", (object) str1);
            FileSystemProvider.tracer.WriteLine("normalizedPathStack.Push({0})", (object) child);
            stringStack.Push(child);
          }
        }
      }
      return stringStack;
    }

    private string CreateNormalizedRelativePathFromStack(Stack<string> normalizedPathStack)
    {
      string child = string.Empty;
      while (normalizedPathStack.Count > 0)
        child = !string.IsNullOrEmpty(child) ? this.MakePath(normalizedPathStack.Pop(), child) : normalizedPathStack.Pop();
      FileSystemProvider.tracer.WriteLine("result = {0}", (object) child);
      return child;
    }

    protected override string GetChildName(string path)
    {
      path = !string.IsNullOrEmpty(path) ? path.Replace('/', '\\') : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      path = path.TrimEnd('\\');
      int num = path.LastIndexOf('\\');
      string str = num != -1 ? (!FileSystemProvider.IsUNCPath(path) ? path.Substring(num + 1) : (!FileSystemProvider.IsUNCRoot(path) ? path.Substring(num + 1) : string.Empty)) : FileSystemProvider.EnsureDriveIsRooted(path);
      FileSystemProvider.tracer.WriteLine("Result = {0}", (object) str);
      return str;
    }

    private static string EnsureDriveIsRooted(string path)
    {
      string str = path;
      int num = path.IndexOf(':');
      if (num != -1 && num + 1 == path.Length)
        str = path + (object) '\\';
      FileSystemProvider.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    protected override bool IsItemContainer(string path)
    {
      path = !string.IsNullOrEmpty(path) ? FileSystemProvider.NormalizePath(path) : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      bool flag = false;
      if (new DirectoryInfo(path).Exists)
        flag = true;
      FileSystemProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    protected override void MoveItem(string path, string destination)
    {
      if (string.IsNullOrEmpty(path))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      if (string.IsNullOrEmpty(destination))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (destination));
      path = FileSystemProvider.NormalizePath(path);
      destination = FileSystemProvider.NormalizePath(destination);
      if (this.IsReservedDeviceName(destination))
      {
        this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "TargetCannotContainDeviceName", (object) destination)), "MoveError", ErrorCategory.WriteError, (object) destination));
      }
      else
      {
        try
        {
          bool flag = this.IsItemContainer(path);
          FileSystemProvider.tracer.WriteLine("Moving {0} to {1}", (object) path, (object) destination);
          if (flag)
          {
            DirectoryInfo directory = new DirectoryInfo(path);
            if (this.ItemExists(destination) && this.IsItemContainer(destination))
              destination = this.MakePath(destination, directory.Name);
            string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "MoveItemActionDirectory");
            if (!this.ShouldProcess(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "MoveItemResourceFileTemplate", (object) directory.FullName, (object) destination), resourceString))
              return;
            this.MoveDirectoryInfoItem(directory, destination, (bool) this.Force);
          }
          else
          {
            FileInfo file = new FileInfo(path);
            if (this.IsItemContainer(destination))
              destination = this.MakePath(destination, file.Name);
            string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "MoveItemActionFile");
            if (!this.ShouldProcess(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "MoveItemResourceFileTemplate", (object) file.FullName, (object) destination), resourceString))
              return;
            this.MoveFileInfoItem(file, destination, (bool) this.Force);
          }
        }
        catch (ArgumentException ex)
        {
          FileSystemProvider.tracer.TraceException((Exception) ex);
          this.WriteError(new ErrorRecord((Exception) ex, "MoveItemArgumentError", ErrorCategory.InvalidArgument, (object) path));
        }
        catch (IOException ex)
        {
          FileSystemProvider.tracer.TraceException((Exception) ex);
          this.WriteError(new ErrorRecord((Exception) ex, "MoveItemIOError", ErrorCategory.WriteError, (object) path));
        }
        catch (UnauthorizedAccessException ex)
        {
          FileSystemProvider.tracer.TraceException((Exception) ex);
          this.WriteError(new ErrorRecord((Exception) ex, "MoveItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
        }
      }
    }

    private void MoveFileInfoItem(FileInfo file, string destination, bool force)
    {
      try
      {
        file.MoveTo(destination);
        this.WriteItemObject((object) file, file.FullName, false);
      }
      catch (UnauthorizedAccessException ex1)
      {
        if (force)
        {
          try
          {
            file.Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden);
            file.MoveTo(destination);
            this.WriteItemObject((object) file, file.FullName, false);
          }
          catch (Exception ex2)
          {
            switch (ex2)
            {
              case IOException _:
              case ArgumentNullException _:
              case ArgumentException _:
              case SecurityException _:
              case UnauthorizedAccessException _:
              case FileNotFoundException _:
              case DirectoryNotFoundException _:
              case PathTooLongException _:
              case NotSupportedException _:
                this.WriteError(new ErrorRecord((Exception) ex1, "MoveFileInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) file));
                break;
              default:
                throw;
            }
          }
        }
        else
          this.WriteError(new ErrorRecord((Exception) ex1, "MoveFileInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) file));
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "MoveFileInfoItemArgumentError", ErrorCategory.InvalidArgument, (object) file));
      }
      catch (IOException ex1)
      {
        if (force && File.Exists(destination))
        {
          FileInfo fileInfo = new FileInfo(destination);
          if (fileInfo != null)
          {
            try
            {
              fileInfo.Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden);
              fileInfo.Delete();
              file.MoveTo(destination);
              this.WriteItemObject((object) file, file.FullName, false);
            }
            catch (Exception ex2)
            {
              switch (ex2)
              {
                case FileNotFoundException _:
                case DirectoryNotFoundException _:
                case UnauthorizedAccessException _:
                case SecurityException _:
                case ArgumentException _:
                case PathTooLongException _:
                case NotSupportedException _:
                case ArgumentNullException _:
                case IOException _:
                  FileSystemProvider.tracer.TraceException((Exception) ex1);
                  this.WriteError(new ErrorRecord((Exception) ex1, "MoveFileInfoItemIOError", ErrorCategory.WriteError, (object) fileInfo));
                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            FileSystemProvider.tracer.TraceException((Exception) ex1);
            this.WriteError(new ErrorRecord((Exception) ex1, "MoveFileInfoItemIOError", ErrorCategory.WriteError, (object) file));
          }
        }
        else
        {
          FileSystemProvider.tracer.TraceException((Exception) ex1);
          this.WriteError(new ErrorRecord((Exception) ex1, "MoveFileInfoItemIOError", ErrorCategory.WriteError, (object) file));
        }
      }
    }

    private void MoveDirectoryInfoItem(DirectoryInfo directory, string destination, bool force)
    {
      try
      {
        directory.MoveTo(destination);
        this.WriteItemObject((object) directory, directory.FullName, true);
      }
      catch (UnauthorizedAccessException ex1)
      {
        if (force)
        {
          try
          {
            directory.Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden);
            directory.MoveTo(destination);
            this.WriteItemObject((object) directory, directory.FullName, true);
          }
          catch (Exception ex2)
          {
            switch (ex2)
            {
              case FileNotFoundException _:
              case ArgumentNullException _:
              case DirectoryNotFoundException _:
              case SecurityException _:
              case ArgumentException _:
              case IOException _:
                this.WriteError(new ErrorRecord((Exception) ex1, "MoveDirectoryItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) directory));
                break;
              default:
                throw;
            }
          }
        }
        else
          this.WriteError(new ErrorRecord((Exception) ex1, "MoveDirectoryItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) directory));
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "MoveDirectoryItemArgumentError", ErrorCategory.InvalidArgument, (object) directory));
      }
      catch (IOException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "MoveDirectoryItemIOError", ErrorCategory.WriteError, (object) directory));
      }
    }

    public void GetProperty(string path, Collection<string> providerSpecificPickList)
    {
      path = !string.IsNullOrEmpty(path) ? FileSystemProvider.NormalizePath(path) : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      PSObject psObject = (PSObject) null;
      try
      {
        FileSystemInfo fileSystemInfo = (FileSystemInfo) null;
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        if (directoryInfo.Exists)
        {
          fileSystemInfo = (FileSystemInfo) directoryInfo;
        }
        else
        {
          FileInfo fileInfo = new FileInfo(path);
          if (fileInfo.Exists)
            fileSystemInfo = (FileSystemInfo) fileInfo;
        }
        if (fileSystemInfo == null)
          this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "ItemDoesNotExist", (object) path)), "ItemDoesNotExist", ErrorCategory.ObjectNotFound, (object) path));
        else if (providerSpecificPickList == null || providerSpecificPickList.Count == 0)
        {
          psObject = PSObject.AsPSObject((object) fileSystemInfo);
        }
        else
        {
          foreach (string providerSpecificPick in providerSpecificPickList)
          {
            if (providerSpecificPick != null)
            {
              if (providerSpecificPick.Length > 0)
              {
                try
                {
                  PSMemberInfo property = (PSMemberInfo) PSObject.AsPSObject((object) fileSystemInfo).Properties[providerSpecificPick];
                  if (property != null)
                  {
                    object obj = property.Value;
                    if (psObject == null)
                      psObject = new PSObject();
                    psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty(providerSpecificPick, obj));
                  }
                  else
                    this.WriteError(new ErrorRecord((Exception) new IOException(StringUtil.Format(ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "PropertyNotFound"), (object) providerSpecificPick)), "GetValueError", ErrorCategory.ReadError, (object) providerSpecificPick));
                }
                catch (GetValueException ex)
                {
                  this.WriteError(new ErrorRecord((Exception) ex, "GetValueError", ErrorCategory.ReadError, (object) providerSpecificPick));
                }
              }
            }
          }
        }
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "GetPropertyArgumentError", ErrorCategory.InvalidArgument, (object) path));
      }
      catch (IOException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "GetPropertyIOError", ErrorCategory.ReadError, (object) path));
      }
      catch (UnauthorizedAccessException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "GetPropertyUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
      }
      if (psObject == null)
        return;
      this.WritePropertyObject((object) psObject, path);
    }

    public object GetPropertyDynamicParameters(
      string path,
      Collection<string> providerSpecificPickList)
    {
      return (object) null;
    }

    public void SetProperty(string path, PSObject propertyToSet)
    {
      if (string.IsNullOrEmpty(path))
        throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      if (propertyToSet == null)
        throw FileSystemProvider.tracer.NewArgumentNullException(nameof (propertyToSet));
      path = FileSystemProvider.NormalizePath(path);
      PSObject psObject1 = new PSObject();
      PSObject psObject2 = (PSObject) null;
      bool flag1 = false;
      DirectoryInfo directoryInfo = new DirectoryInfo(path);
      if (directoryInfo.Exists)
      {
        flag1 = true;
        psObject2 = PSObject.AsPSObject((object) directoryInfo);
      }
      else
      {
        FileInfo fileInfo = new FileInfo(path);
        if (fileInfo.Exists)
          psObject2 = PSObject.AsPSObject((object) fileInfo);
      }
      if (psObject2 != null)
      {
        bool flag2 = false;
        foreach (PSMemberInfo property1 in propertyToSet.Properties)
        {
          object obj = property1.Value;
          string action = !flag1 ? ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "SetPropertyActionFile") : ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "SetPropertyActionDirectory");
          string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "SetPropertyResourceTemplate");
          obj.ToString();
          string str;
          try
          {
            str = PSObject.AsPSObject(obj).ToString();
          }
          catch (Exception ex)
          {
            throw;
          }
          if (this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, resourceString, (object) path, (object) property1.Name, (object) str), action))
          {
            PSMemberInfo property2 = (PSMemberInfo) PSObject.AsPSObject((object) psObject2).Properties[property1.Name];
            if (property2 != null)
            {
              if (string.Compare(property1.Name, "attributes", StringComparison.OrdinalIgnoreCase) == 0)
              {
                if (!(obj is FileAttributes fileAttributes))
                  fileAttributes = (FileAttributes) Enum.Parse(typeof (FileAttributes), str, true);
                if ((fileAttributes & ~(FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive | FileAttributes.Normal)) != (FileAttributes) 0)
                {
                  this.WriteError(new ErrorRecord((Exception) new IOException(StringUtil.Format(ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "AttributesNotSupported"), (object) property1)), "SetPropertyError", ErrorCategory.ReadError, (object) property1));
                  continue;
                }
              }
              property2.Value = obj;
              psObject1.Properties.Add((PSPropertyInfo) new PSNoteProperty(property1.Name, obj));
              flag2 = true;
            }
            else
              this.WriteError(new ErrorRecord((Exception) new IOException(StringUtil.Format(ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "PropertyNotFound"), (object) property1)), "SetPropertyError", ErrorCategory.ReadError, (object) property1));
          }
        }
        if (!flag2)
          return;
        this.WritePropertyObject((object) psObject1, path);
      }
      else
        this.WriteError(new ErrorRecord((Exception) new IOException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "ItemDoesNotExist", (object) path)), "ItemDoesNotExist", ErrorCategory.ObjectNotFound, (object) path));
    }

    public object SetPropertyDynamicParameters(string path, PSObject propertyValue) => (object) null;

    public void ClearProperty(string path, Collection<string> propertiesToClear)
    {
      path = !string.IsNullOrEmpty(path) ? FileSystemProvider.NormalizePath(path) : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      if (propertiesToClear == null || propertiesToClear.Count == 0)
        throw FileSystemProvider.tracer.NewArgumentNullException(nameof (propertiesToClear));
      if (propertiesToClear.Count <= 1)
      {
        if (string.Compare("Attributes", propertiesToClear[0], true, this.Host.CurrentCulture) == 0)
        {
          try
          {
            string resourceString;
            FileSystemInfo fileSystemInfo;
            if (this.IsItemContainer(path))
            {
              resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "ClearPropertyActionDirectory");
              fileSystemInfo = (FileSystemInfo) new DirectoryInfo(path);
            }
            else
            {
              resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "ClearPropertyActionFile");
              fileSystemInfo = (FileSystemInfo) new FileInfo(path);
            }
            if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "ClearPropertyResourceTemplate"), (object) fileSystemInfo.FullName, (object) propertiesToClear[0]), resourceString))
              return;
            fileSystemInfo.Attributes = FileAttributes.Normal;
            this.WritePropertyObject((object) new PSObject()
            {
              Properties = {
                (PSPropertyInfo) new PSNoteProperty(propertiesToClear[0], (object) fileSystemInfo.Attributes)
              }
            }, path);
            return;
          }
          catch (UnauthorizedAccessException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, "ClearPropertyUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
            return;
          }
          catch (ArgumentException ex)
          {
            FileSystemProvider.tracer.TraceException((Exception) ex);
            this.WriteError(new ErrorRecord((Exception) ex, "ClearPropertyArgumentError", ErrorCategory.InvalidArgument, (object) path));
            return;
          }
          catch (IOException ex)
          {
            FileSystemProvider.tracer.TraceException((Exception) ex);
            this.WriteError(new ErrorRecord((Exception) ex, "ClearPropertyIOError", ErrorCategory.WriteError, (object) path));
            return;
          }
        }
      }
      throw FileSystemProvider.tracer.NewArgumentException(nameof (propertiesToClear), "FileSystemProviderStrings", "CannotClearProperty");
    }

    public object ClearPropertyDynamicParameters(string path, Collection<string> propertiesToClear) => (object) null;

    public IContentReader GetContentReader(string path)
    {
      path = !string.IsNullOrEmpty(path) ? FileSystemProvider.NormalizePath(path) : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      string delimiter = "\n";
      Encoding encodingType = Encoding.Default;
      bool waitForChanges = false;
      bool usingByteEncoding = false;
      bool flag = false;
      if (this.DynamicParameters != null && this.DynamicParameters is FileSystemContentReaderDynamicParameters dynamicParameters)
      {
        flag = dynamicParameters.DelimiterSpecified;
        if (flag)
          delimiter = dynamicParameters.Delimiter;
        usingByteEncoding = dynamicParameters.UsingByteEncoding;
        if (dynamicParameters.WasStreamTypeSpecified)
          encodingType = dynamicParameters.EncodingType;
        waitForChanges = (bool) dynamicParameters.Wait;
      }
      FileSystemContentReaderWriter contentReaderWriter = (FileSystemContentReaderWriter) null;
      try
      {
        if (flag)
        {
          if (usingByteEncoding)
            this.WriteError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "DelimiterError"), "delimiter"), "GetContentReaderArgumentError", ErrorCategory.InvalidArgument, (object) path));
          else
            contentReaderWriter = new FileSystemContentReaderWriter(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, delimiter, encodingType, waitForChanges, (CmdletProvider) this);
        }
        else
          contentReaderWriter = new FileSystemContentReaderWriter(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, encodingType, usingByteEncoding, waitForChanges, (CmdletProvider) this);
      }
      catch (PathTooLongException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentReaderPathTooLongError", ErrorCategory.InvalidArgument, (object) path));
      }
      catch (FileNotFoundException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentReaderFileNotFoundError", ErrorCategory.ObjectNotFound, (object) path));
      }
      catch (DirectoryNotFoundException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentReaderDirectoryNotFoundError", ErrorCategory.ObjectNotFound, (object) path));
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentReaderArgumentError", ErrorCategory.InvalidArgument, (object) path));
      }
      catch (IOException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentReaderIOError", ErrorCategory.ReadError, (object) path));
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentReaderSecurityError", ErrorCategory.PermissionDenied, (object) path));
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentReaderUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
      }
      return (IContentReader) contentReaderWriter;
    }

    public object GetContentReaderDynamicParameters(string path) => (object) new FileSystemContentReaderDynamicParameters();

    public IContentWriter GetContentWriter(string path)
    {
      path = !string.IsNullOrEmpty(path) ? FileSystemProvider.NormalizePath(path) : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      bool usingByteEncoding = false;
      Encoding encodingType = Encoding.Default;
      FileMode mode = FileMode.OpenOrCreate;
      if (this.DynamicParameters != null && this.DynamicParameters is FileSystemContentWriterDynamicParameters dynamicParameters)
      {
        usingByteEncoding = dynamicParameters.UsingByteEncoding;
        if (dynamicParameters.WasStreamTypeSpecified)
          encodingType = dynamicParameters.EncodingType;
      }
      FileSystemContentReaderWriter contentReaderWriter = (FileSystemContentReaderWriter) null;
      try
      {
        contentReaderWriter = new FileSystemContentReaderWriter(path, mode, FileAccess.Write, FileShare.Write, encodingType, usingByteEncoding, false, (CmdletProvider) this);
      }
      catch (PathTooLongException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentWriterPathTooLongError", ErrorCategory.InvalidArgument, (object) path));
      }
      catch (FileNotFoundException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentWriterFileNotFoundError", ErrorCategory.ObjectNotFound, (object) path));
      }
      catch (DirectoryNotFoundException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentWriterDirectoryNotFoundError", ErrorCategory.ObjectNotFound, (object) path));
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentWriterArgumentError", ErrorCategory.InvalidArgument, (object) path));
      }
      catch (IOException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentWriterIOError", ErrorCategory.WriteError, (object) path));
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentWriterSecurityError", ErrorCategory.PermissionDenied, (object) path));
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "GetContentWriterUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
      }
      return (IContentWriter) contentReaderWriter;
    }

    public object GetContentWriterDynamicParameters(string path) => (object) new FileSystemContentWriterDynamicParameters();

    public void ClearContent(string path)
    {
      path = !string.IsNullOrEmpty(path) ? FileSystemProvider.NormalizePath(path) : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
      string resourceString = ResourceManagerCache.GetResourceString("FileSystemProviderStrings", "ClearContentActionFile");
      if (!this.ShouldProcess(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "ClearContentesourceTemplate", (object) path), resourceString))
        return;
      try
      {
        new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.Write).Close();
        this.WriteItemObject((object) "", path, false);
      }
      catch (ArgumentException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "ClearContentArgumentError", ErrorCategory.InvalidArgument, (object) path));
      }
      catch (IOException ex)
      {
        FileSystemProvider.tracer.TraceException((Exception) ex);
        this.WriteError(new ErrorRecord((Exception) ex, "ClearContentIOError", ErrorCategory.WriteError, (object) path));
      }
      catch (UnauthorizedAccessException ex1)
      {
        if ((bool) this.Force)
        {
          FileAttributes attributes = File.GetAttributes(path);
          try
          {
            File.SetAttributes(path, File.GetAttributes(path) & ~(FileAttributes.ReadOnly | FileAttributes.Hidden));
            new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.Write).Close();
            this.WriteItemObject((object) "", path, false);
          }
          catch (UnauthorizedAccessException ex2)
          {
            this.WriteError(new ErrorRecord((Exception) ex2, "RemoveFileSystemItemUnAuthorizedAccess", ErrorCategory.PermissionDenied, (object) path));
          }
          finally
          {
            File.SetAttributes(path, attributes);
          }
        }
        else
        {
          FileSystemProvider.tracer.TraceException((Exception) ex1);
          this.WriteError(new ErrorRecord((Exception) ex1, "ClearContentUnauthorizedAccessError", ErrorCategory.PermissionDenied, (object) path));
        }
      }
    }

    public object ClearContentDynamicParameters(string path) => (object) null;

    public void GetSecurityDescriptor(string path, AccessControlSections sections)
    {
      using (FileSystemProvider.tracer.TraceMethod(path, new object[0]))
      {
        ObjectSecurity securityDescriptor = (ObjectSecurity) null;
        path = FileSystemProvider.NormalizePath(path);
        if (string.IsNullOrEmpty(path))
          throw FileSystemProvider.tracer.NewArgumentNullException(nameof (path));
        if ((sections & ~AccessControlSections.All) != AccessControlSections.None)
          throw FileSystemProvider.tracer.NewArgumentException(nameof (sections));
        try
        {
          securityDescriptor = !Directory.Exists(path) ? (ObjectSecurity) new FileSecurity(path, sections) : (ObjectSecurity) new DirectorySecurity(path, sections);
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
        this.WriteSecurityDescriptorObject(securityDescriptor, path);
      }
    }

    public void SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor)
    {
      using (FileSystemProvider.tracer.TraceMethod(path, new object[0]))
      {
        path = !string.IsNullOrEmpty(path) ? FileSystemProvider.NormalizePath(path) : throw FileSystemProvider.tracer.NewArgumentException(nameof (path));
        if (securityDescriptor == null)
          throw FileSystemProvider.tracer.NewArgumentNullException(nameof (securityDescriptor));
        if (!File.Exists(path) && !Directory.Exists(path))
          this.ThrowTerminatingError(FileSystemProvider.CreateErrorRecord(path, "SetSecurityDescriptor_FileNotFound"));
        if (!(securityDescriptor is FileSystemSecurity fileSystemSecurity))
          throw FileSystemProvider.tracer.NewArgumentException(nameof (securityDescriptor));
        try
        {
          this.SetSecurityDescriptor(path, (ObjectSecurity) fileSystemSecurity, AccessControlSections.All);
        }
        catch (PrivilegeNotHeldException ex)
        {
          ObjectSecurity accessControl = (ObjectSecurity) File.GetAccessControl(path);
          Type targetType = typeof (NTAccount);
          AccessControlSections sections = AccessControlSections.All;
          if (fileSystemSecurity.GetAuditRules(true, true, targetType).Count == 0 && fileSystemSecurity.AreAuditRulesProtected == accessControl.AreAccessRulesProtected)
            sections &= ~AccessControlSections.Audit;
          if (fileSystemSecurity.GetOwner(targetType) == accessControl.GetOwner(targetType))
            sections &= ~AccessControlSections.Owner;
          if (fileSystemSecurity.GetGroup(targetType) == accessControl.GetGroup(targetType))
            sections &= ~AccessControlSections.Group;
          this.SetSecurityDescriptor(path, (ObjectSecurity) fileSystemSecurity, sections);
        }
      }
    }

    private void SetSecurityDescriptor(
      string path,
      ObjectSecurity sd,
      AccessControlSections sections)
    {
      byte[] descriptorBinaryForm = sd.GetSecurityDescriptorBinaryForm();
      if (Directory.Exists(path))
      {
        DirectorySecurity directorySecurity = new DirectorySecurity();
        directorySecurity.SetSecurityDescriptorBinaryForm(descriptorBinaryForm, sections);
        Directory.SetAccessControl(path, directorySecurity);
        this.WriteSecurityDescriptorObject((ObjectSecurity) directorySecurity, path);
      }
      else
      {
        FileSecurity fileSecurity = new FileSecurity();
        fileSecurity.SetSecurityDescriptorBinaryForm(descriptorBinaryForm, sections);
        File.SetAccessControl(path, fileSecurity);
        this.WriteSecurityDescriptorObject((ObjectSecurity) fileSecurity, path);
      }
    }

    public ObjectSecurity NewSecurityDescriptorFromPath(
      string path,
      AccessControlSections sections)
    {
      using (FileSystemProvider.tracer.TraceMethod())
        return FileSystemProvider.NewSecurityDescriptor(!this.IsItemContainer(path) ? FileSystemProvider.ItemType.File : FileSystemProvider.ItemType.Directory);
    }

    public ObjectSecurity NewSecurityDescriptorOfType(
      string type,
      AccessControlSections sections)
    {
      using (FileSystemProvider.tracer.TraceMethod())
        return FileSystemProvider.NewSecurityDescriptor(FileSystemProvider.GetItemType(type));
    }

    private static ObjectSecurity NewSecurityDescriptor(
      FileSystemProvider.ItemType itemType)
    {
      ObjectSecurity objectSecurity = (ObjectSecurity) null;
      switch (itemType)
      {
        case FileSystemProvider.ItemType.File:
          objectSecurity = (ObjectSecurity) new FileSecurity();
          break;
        case FileSystemProvider.ItemType.Directory:
          objectSecurity = (ObjectSecurity) new DirectorySecurity();
          break;
      }
      return objectSecurity;
    }

    private static ErrorRecord CreateErrorRecord(string path, string errorId) => new ErrorRecord((Exception) new FileNotFoundException(ResourceManagerCache.FormatResourceString("FileSystemProviderStrings", "FileNotFound", (object) path)), errorId, ErrorCategory.ObjectNotFound, (object) null);

    private enum ItemType
    {
      Unknown,
      File,
      Directory,
    }
  }
}
