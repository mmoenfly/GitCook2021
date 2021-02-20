// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.RegistryProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
  [OutputType(new Type[] {typeof (RegistryKey), typeof (string)}, ProviderCmdlet = "Get-ChildItem")]
  [OutputType(new Type[] {typeof (RegistrySecurity)}, ProviderCmdlet = "Get-Acl")]
  [CmdletProvider("Registry", ProviderCapabilities.ShouldProcess | ProviderCapabilities.Transactions)]
  [OutputType(new Type[] {typeof (RegistryKey)}, ProviderCmdlet = "Get-Item")]
  public sealed class RegistryProvider : 
    NavigationCmdletProvider,
    IDynamicPropertyCmdletProvider,
    IPropertyCmdletProvider,
    ISecurityDescriptorCmdletProvider
  {
    public const string ProviderName = "Registry";
    private const string resBaseName = "RegistryProviderStrings";
    private const string charactersThatNeedEscaping = ".*?[]:";
    [TraceSource("RegistryProvider", "The namespace navigation provider for the Windows Registry")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (RegistryProvider), "The namespace navigation provider for the Windows Registry");
    private static readonly string[] hiveNames = new string[6]
    {
      "HKEY_LOCAL_MACHINE",
      "HKEY_CURRENT_USER",
      "HKEY_CLASSES_ROOT",
      "HKEY_CURRENT_CONFIG",
      "HKEY_USERS",
      "HKEY_PERFORMANCE_DATA"
    };
    private static readonly string[] hiveShortNames = new string[6]
    {
      "HKLM",
      "HKCU",
      "HKCR",
      "HKCC",
      "HKU",
      "HKPD"
    };
    private static readonly RegistryKey[] wellKnownHives = new RegistryKey[6]
    {
      Registry.LocalMachine,
      Registry.CurrentUser,
      Registry.ClassesRoot,
      Registry.CurrentConfig,
      Registry.Users,
      Registry.PerformanceData
    };
    private static readonly TransactedRegistryKey[] wellKnownHivesTx = new TransactedRegistryKey[5]
    {
      TransactedRegistry.LocalMachine,
      TransactedRegistry.CurrentUser,
      TransactedRegistry.ClassesRoot,
      TransactedRegistry.CurrentConfig,
      TransactedRegistry.Users
    };

    protected override PSDriveInfo NewDrive(PSDriveInfo drive)
    {
      if (drive == (PSDriveInfo) null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (drive));
      if (!this.ItemExists(drive.Root))
      {
        Exception exception = (Exception) new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "NewDriveRootDoesNotExist"));
        this.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, (object) drive.Root));
      }
      return drive;
    }

    protected override Collection<PSDriveInfo> InitializeDefaultDrives() => new Collection<PSDriveInfo>()
    {
      new PSDriveInfo("HKLM", this.ProviderInfo, "HKEY_LOCAL_MACHINE", ResourceManagerCache.GetResourceString("RegistryProviderStrings", "HKLMDriveDescription"), (PSCredential) null),
      new PSDriveInfo("HKCU", this.ProviderInfo, "HKEY_CURRENT_USER", ResourceManagerCache.GetResourceString("RegistryProviderStrings", "HKCUDriveDescription"), (PSCredential) null)
    };

    protected override bool IsValidPath(string path)
    {
      bool flag = true;
      string path1 = this.NormalizePath(path).TrimStart('\\').TrimEnd('\\');
      int length = path1.IndexOf('\\');
      if (length != -1)
        path1 = path1.Substring(0, length);
      if (string.IsNullOrEmpty(path1))
        flag = true;
      else if (this.GetHiveRoot(path1) == null)
        flag = false;
      RegistryProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    protected override void GetItem(string path)
    {
      IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
      if (pathWriteIfError == null)
        return;
      this.WriteRegistryItemObject(pathWriteIfError, path);
    }

    protected override void SetItem(string path, object value)
    {
      if (string.IsNullOrEmpty(path))
        throw RegistryProvider.tracer.NewArgumentException(nameof (path));
      string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "SetItemAction");
      if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "SetItemResourceTemplate"), (object) path, value), resourceString))
        return;
      string str = (string) null;
      IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
      if (pathWriteIfError == null)
        return;
      bool flag = false;
      if (this.DynamicParameters != null)
      {
        if (this.DynamicParameters is RegistryProviderSetItemDynamicParameter dynamicParameters)
        {
          try
          {
            RegistryValueKind type = dynamicParameters.Type;
            pathWriteIfError.SetValue(str, value, type);
            flag = true;
          }
          catch (ArgumentException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.InvalidArgument, (object) str));
            pathWriteIfError.Close();
            return;
          }
          catch (IOException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
            pathWriteIfError.Close();
            return;
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
            pathWriteIfError.Close();
            return;
          }
          catch (UnauthorizedAccessException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
            pathWriteIfError.Close();
            return;
          }
        }
      }
      if (!flag)
      {
        try
        {
          pathWriteIfError.SetValue(str, value);
        }
        catch (IOException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
          pathWriteIfError.Close();
          return;
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
          pathWriteIfError.Close();
          return;
        }
        catch (UnauthorizedAccessException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
          pathWriteIfError.Close();
          return;
        }
      }
      object obj = RegistryProvider.ReadExistingKeyValue(pathWriteIfError, str);
      pathWriteIfError.Close();
      this.WriteItemObject(obj, path, false);
    }

    protected override object SetItemDynamicParameters(string path, object value) => (object) new RegistryProviderSetItemDynamicParameter();

    protected override void ClearItem(string path)
    {
      if (string.IsNullOrEmpty(path))
        throw RegistryProvider.tracer.NewArgumentException(nameof (path));
      string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "ClearItemAction");
      if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "ClearItemResourceTemplate"), (object) path), resourceString))
        return;
      IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
      if (pathWriteIfError == null)
        return;
      string[] strArray = new string[0];
      string[] valueNames;
      try
      {
        valueNames = pathWriteIfError.GetValueNames();
      }
      catch (IOException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.ReadError, (object) path));
        return;
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        return;
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        return;
      }
      for (int index = 0; index < valueNames.Length; ++index)
      {
        try
        {
          pathWriteIfError.DeleteValue(valueNames[index]);
        }
        catch (IOException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
        catch (UnauthorizedAccessException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
      }
      this.WriteRegistryItemObject(pathWriteIfError, path);
    }

    protected override void GetChildItems(string path, bool recurse)
    {
      RegistryProvider.tracer.WriteLine("recurse = {0}", (object) recurse);
      if (path == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
      if (this.IsHiveContainer(path))
      {
        foreach (string hiveName in RegistryProvider.hiveNames)
        {
          if (this.Stopping)
            break;
          this.GetItem(hiveName);
        }
      }
      else
      {
        IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
        if (pathWriteIfError == null)
          return;
        try
        {
          string[] subKeyNames = pathWriteIfError.GetSubKeyNames();
          pathWriteIfError.Close();
          if (subKeyNames == null)
            return;
          foreach (string child in subKeyNames)
          {
            if (this.Stopping)
              break;
            if (!string.IsNullOrEmpty(child))
            {
              string path1 = path;
              try
              {
                path1 = this.MakePath(path, child);
                if (!string.IsNullOrEmpty(path1))
                {
                  IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(path1, false);
                  if (regkeyForPath != null)
                    this.WriteRegistryItemObject(regkeyForPath, path1);
                  if (recurse)
                    this.GetChildItems(path1, recurse);
                }
              }
              catch (IOException ex)
              {
                this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.ReadError, (object) path1));
              }
              catch (SecurityException ex)
              {
                this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path1));
              }
              catch (UnauthorizedAccessException ex)
              {
                this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path1));
              }
            }
          }
        }
        catch (IOException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.ReadError, (object) path));
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
        catch (UnauthorizedAccessException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
      }
    }

    protected override void GetChildNames(string path, ReturnContainers returnContainers)
    {
      switch (path)
      {
        case "":
          foreach (string hiveName in RegistryProvider.hiveNames)
          {
            if (this.Stopping)
              break;
            this.WriteItemObject((object) hiveName, hiveName, true);
          }
          break;
        case null:
          throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
        default:
          IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
          if (pathWriteIfError == null)
            break;
          try
          {
            string[] subKeyNames = pathWriteIfError.GetSubKeyNames();
            pathWriteIfError.Close();
            for (int index = 0; index < subKeyNames.Length && !this.Stopping; ++index)
            {
              string child = RegistryProvider.EscapeChildName(subKeyNames[index]);
              string path1 = this.MakePath(path, child);
              this.WriteItemObject((object) child, path1, true);
            }
            break;
          }
          catch (IOException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.ReadError, (object) path));
            break;
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
            break;
          }
          catch (UnauthorizedAccessException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
            break;
          }
      }
    }

    private static string EscapeSpecialChars(string path)
    {
      StringBuilder stringBuilder = new StringBuilder();
      TextElementEnumerator elementEnumerator = StringInfo.GetTextElementEnumerator(path);
      while (elementEnumerator.MoveNext())
      {
        string textElement = elementEnumerator.GetTextElement();
        if (textElement.Contains(".*?[]:"))
          stringBuilder.Append("`");
        stringBuilder.Append(textElement);
      }
      RegistryProvider.tracer.WriteLine("result = {0}", (object) stringBuilder);
      return stringBuilder.ToString();
    }

    private static string EscapeChildName(string name)
    {
      StringBuilder stringBuilder = new StringBuilder();
      TextElementEnumerator elementEnumerator = StringInfo.GetTextElementEnumerator(name);
      while (elementEnumerator.MoveNext())
      {
        string textElement = elementEnumerator.GetTextElement();
        if (textElement.Contains(".*?[]:"))
          stringBuilder.Append("`");
        stringBuilder.Append(textElement);
      }
      RegistryProvider.tracer.WriteLine("result = {0}", (object) stringBuilder);
      return stringBuilder.ToString();
    }

    protected override void RenameItem(string path, string newName)
    {
      if (string.IsNullOrEmpty(path))
        throw RegistryProvider.tracer.NewArgumentException(nameof (path));
      if (string.IsNullOrEmpty(newName))
        throw RegistryProvider.tracer.NewArgumentException(nameof (newName));
      RegistryProvider.tracer.WriteLine("newName = {0}", (object) newName);
      string str = this.MakePath(this.GetParentPath(path, (string) null), newName);
      if (this.ItemExists(str))
      {
        Exception exception = (Exception) new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "RenameItemAlreadyExists"));
        this.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, (object) str));
      }
      else
      {
        string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "RenameItemAction");
        if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "RenameItemResourceTemplate"), (object) path, (object) str), resourceString))
          return;
        this.MoveRegistryItem(path, str);
      }
    }

    protected override void NewItem(string path, string type, object newItem)
    {
      if (string.IsNullOrEmpty(path))
        throw RegistryProvider.tracer.NewArgumentException(nameof (path));
      string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "NewItemAction");
      if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "NewItemResourceTemplate"), (object) path), resourceString))
        return;
      IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(path, false);
      if (regkeyForPath != null)
      {
        if (!(bool) this.Force)
        {
          Exception exception = (Exception) new IOException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "KeyAlreadyExists"));
          this.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.ResourceExists, (object) regkeyForPath));
          regkeyForPath.Close();
          return;
        }
        regkeyForPath.Close();
        this.RemoveItem(path, false);
      }
      if ((bool) this.Force && !this.CreateIntermediateKeys(path))
        return;
      string parentPath = this.GetParentPath(path, (string) null);
      string childName = this.GetChildName(path);
      IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(parentPath, true);
      if (pathWriteIfError == null)
        return;
      try
      {
        IRegistryWrapper subKey = pathWriteIfError.CreateSubKey(childName);
        pathWriteIfError.Close();
        try
        {
          if (newItem != null)
          {
            RegistryValueKind kind;
            if (!this.ParseKind(type, out kind))
              return;
            this.SetRegistryValue(subKey, string.Empty, newItem, kind, path, false);
          }
        }
        catch (Exception ex)
        {
          switch (ex)
          {
            case ArgumentException _:
            case InvalidCastException _:
            case IOException _:
            case SecurityException _:
            case UnauthorizedAccessException _:
              this.WriteError(new ErrorRecord(ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) subKey)
              {
                ErrorDetails = new ErrorDetails(ResourceManagerCache.FormatResourceString("RegistryProviderStrings", "KeyCreatedValueFailed", (object) childName))
              });
              break;
            default:
              throw;
          }
        }
        this.WriteRegistryItemObject(subKey, path);
      }
      catch (IOException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
      }
      catch (ArgumentException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.InvalidArgument, (object) path));
      }
    }

    protected override void RemoveItem(string path, bool recurse)
    {
      if (string.IsNullOrEmpty(path))
        throw RegistryProvider.tracer.NewArgumentException(nameof (path));
      RegistryProvider.tracer.WriteLine("recurse = {0}", (object) recurse);
      string parentPath = this.GetParentPath(path, (string) null);
      string childName = this.GetChildName(path);
      IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(parentPath, true);
      if (pathWriteIfError == null)
        return;
      string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "RemoveKeyAction");
      if (this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "RemoveKeyResourceTemplate"), (object) path), resourceString))
      {
        try
        {
          pathWriteIfError.DeleteSubKeyTree(childName);
        }
        catch (ArgumentException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
        }
        catch (IOException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
        catch (UnauthorizedAccessException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
      }
      pathWriteIfError.Close();
    }

    protected override bool ItemExists(string path)
    {
      bool flag = false;
      if (path == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
      try
      {
        if (this.IsHiveContainer(path))
        {
          flag = true;
        }
        else
        {
          IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(path, false);
          if (regkeyForPath != null)
          {
            flag = true;
            regkeyForPath.Close();
          }
        }
      }
      catch (IOException ex)
      {
      }
      catch (SecurityException ex)
      {
        flag = true;
      }
      catch (UnauthorizedAccessException ex)
      {
        flag = true;
      }
      RegistryProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    protected override bool HasChildItems(string path)
    {
      bool flag = false;
      if (path == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
      try
      {
        if (this.IsHiveContainer(path))
        {
          flag = RegistryProvider.hiveNames.Length > 0;
        }
        else
        {
          IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(path, false);
          if (regkeyForPath != null)
          {
            flag = regkeyForPath.SubKeyCount > 0;
            regkeyForPath.Close();
          }
        }
      }
      catch (IOException ex)
      {
        RegistryProvider.tracer.TraceException((Exception) ex);
        flag = false;
      }
      catch (SecurityException ex)
      {
        RegistryProvider.tracer.TraceException((Exception) ex);
        flag = false;
      }
      catch (UnauthorizedAccessException ex)
      {
        RegistryProvider.tracer.TraceException((Exception) ex);
        flag = false;
      }
      RegistryProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    protected override void CopyItem(string path, string destination, bool recurse)
    {
      if (string.IsNullOrEmpty(path))
        throw RegistryProvider.tracer.NewArgumentException(nameof (path));
      if (string.IsNullOrEmpty(destination))
        throw RegistryProvider.tracer.NewArgumentException(nameof (destination));
      RegistryProvider.tracer.WriteLine("destination = {0}", (object) destination);
      RegistryProvider.tracer.WriteLine("recurse = {0}", (object) recurse);
      IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
      if (pathWriteIfError == null)
        return;
      try
      {
        this.CopyRegistryKey(pathWriteIfError, path, destination, recurse, true, false);
      }
      catch (IOException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
      }
      pathWriteIfError.Close();
    }

    private bool CopyRegistryKey(
      IRegistryWrapper key,
      string path,
      string destination,
      bool recurse,
      bool streamResult,
      bool streamFirstOnly)
    {
      bool flag1 = true;
      if (recurse && this.ErrorIfDestinationIsSourceOrChildOfSource(path, destination))
        return false;
      RegistryProvider.tracer.WriteLine("destination = {0}", (object) destination);
      IRegistryWrapper registryWrapper = this.GetRegkeyForPath(destination, true);
      string childName = this.GetChildName(path);
      string str1 = destination;
      if (registryWrapper == null)
      {
        str1 = this.GetParentPath(destination, (string) null);
        childName = this.GetChildName(destination);
        registryWrapper = this.GetRegkeyForPathWriteIfError(str1, true);
      }
      if (registryWrapper == null)
        return false;
      string str2 = this.MakePath(str1, childName);
      string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "CopyKeyAction");
      if (this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "CopyKeyResourceTemplate"), (object) path, (object) destination), resourceString))
      {
        IRegistryWrapper subKey = registryWrapper.CreateSubKey(childName);
        string[] valueNames = key.GetValueNames();
        for (int index = 0; index < valueNames.Length; ++index)
        {
          if (this.Stopping)
          {
            registryWrapper.Close();
            subKey.Close();
            return false;
          }
          subKey.SetValue(valueNames[index], key.GetValue(valueNames[index], (object) null, RegistryValueOptions.DoNotExpandEnvironmentNames), key.GetValueKind(valueNames[index]));
        }
        if (streamResult)
        {
          this.WriteRegistryItemObject(subKey, str2);
          if (streamFirstOnly)
            streamResult = false;
        }
      }
      registryWrapper.Close();
      if (recurse)
      {
        string[] subKeyNames = key.GetSubKeyNames();
        for (int index = 0; index < subKeyNames.Length; ++index)
        {
          if (this.Stopping)
            return false;
          string path1 = this.MakePath(path, subKeyNames[index]);
          string destination1 = this.MakePath(str2, subKeyNames[index]);
          IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(path1, false);
          bool flag2 = this.CopyRegistryKey(regkeyForPath, path1, destination1, recurse, streamResult, streamFirstOnly);
          regkeyForPath.Close();
          if (!flag2)
            flag1 = flag2;
        }
      }
      return flag1;
    }

    private bool ErrorIfDestinationIsSourceOrChildOfSource(
      string sourcePath,
      string destinationPath)
    {
      RegistryProvider.tracer.WriteLine("destinationPath = {0}", (object) destinationPath);
      bool flag = false;
      string parentPath;
      for (; string.Compare(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase) != 0; destinationPath = parentPath)
      {
        parentPath = this.GetParentPath(destinationPath, (string) null);
        if (string.IsNullOrEmpty(parentPath) || string.Compare(parentPath, destinationPath, StringComparison.OrdinalIgnoreCase) == 0)
          goto label_5;
      }
      flag = true;
label_5:
      if (flag)
      {
        Exception exception = (Exception) new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "DestinationChildOfSource"));
        this.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, (object) destinationPath));
      }
      RegistryProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    protected override bool IsItemContainer(string path)
    {
      if (path == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
      bool flag = false;
      if (this.IsHiveContainer(path))
      {
        flag = true;
      }
      else
      {
        try
        {
          IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(path, false);
          if (regkeyForPath != null)
          {
            regkeyForPath.Close();
            flag = true;
          }
        }
        catch (IOException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.ReadError, (object) path));
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
        catch (UnauthorizedAccessException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
      }
      RegistryProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    protected override void MoveItem(string path, string destination)
    {
      if (string.IsNullOrEmpty(path))
        throw RegistryProvider.tracer.NewArgumentException(nameof (path));
      if (string.IsNullOrEmpty(destination))
        throw RegistryProvider.tracer.NewArgumentException(nameof (destination));
      RegistryProvider.tracer.WriteLine("destination = {0}", (object) destination);
      string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "MoveItemAction");
      if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "MoveItemResourceTemplate"), (object) path, (object) destination), resourceString))
        return;
      this.MoveRegistryItem(path, destination);
    }

    private void MoveRegistryItem(string path, string destination)
    {
      IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
      if (pathWriteIfError == null)
        return;
      bool flag;
      try
      {
        flag = this.CopyRegistryKey(pathWriteIfError, path, destination, true, true, true);
      }
      catch (IOException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
        pathWriteIfError.Close();
        return;
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        pathWriteIfError.Close();
        return;
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        pathWriteIfError.Close();
        return;
      }
      pathWriteIfError.Close();
      if (string.Equals(this.GetParentPath(path, (string) null), destination, StringComparison.OrdinalIgnoreCase))
        flag = false;
      if (!flag)
        return;
      try
      {
        this.RemoveItem(path, true);
      }
      catch (IOException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
      }
    }

    public void GetProperty(string path, Collection<string> providerSpecificPickList)
    {
      if (path == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
      if (!this.CheckOperationNotAllowedOnHiveContainer(path))
        return;
      IRegistryWrapper key;
      Collection<string> filteredCollection;
      this.GetFilteredRegistryKeyProperties(path, providerSpecificPickList, true, false, out key, out filteredCollection);
      if (key == null)
        return;
      bool flag = false;
      PSObject psObject = new PSObject();
      foreach (string name1 in filteredCollection)
      {
        string name2 = name1;
        if (string.IsNullOrEmpty(name1))
          name2 = this.GetLocalizedDefaultToken();
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty(name2, key.GetValue(name1)));
        flag = true;
      }
      key.Close();
      if (!flag)
        return;
      this.WritePropertyObject((object) psObject, path);
    }

    public void SetProperty(string path, PSObject propertyValue)
    {
      if (path == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
      if (!this.CheckOperationNotAllowedOnHiveContainer(path))
        return;
      if (propertyValue == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (propertyValue));
      IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
      if (pathWriteIfError == null)
        return;
      RegistryValueKind kind = RegistryValueKind.Unknown;
      if (this.DynamicParameters != null && this.DynamicParameters is RegistryProviderSetItemDynamicParameter dynamicParameters)
        kind = dynamicParameters.Type;
      string resourceString1 = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "SetPropertyAction");
      string resourceString2 = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "SetPropertyResourceTemplate");
      foreach (PSMemberInfo property in propertyValue.Properties)
      {
        object obj = property.Value;
        if (this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, resourceString2, (object) path, (object) property.Name), resourceString1))
        {
          try
          {
            this.SetRegistryValue(pathWriteIfError, property.Name, obj, kind, path);
          }
          catch (InvalidCastException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
          }
          catch (IOException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) property.Name));
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) property.Name));
          }
          catch (UnauthorizedAccessException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) property.Name));
          }
        }
      }
      pathWriteIfError.Close();
    }

    public object SetPropertyDynamicParameters(string path, PSObject propertyValue) => (object) new RegistryProviderSetItemDynamicParameter();

    public void ClearProperty(string path, Collection<string> propertyToClear)
    {
      if (path == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
      if (!this.CheckOperationNotAllowedOnHiveContainer(path))
        return;
      IRegistryWrapper key;
      Collection<string> filteredCollection;
      this.GetFilteredRegistryKeyProperties(path, propertyToClear, false, true, out key, out filteredCollection);
      if (key == null)
        return;
      string resourceString1 = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "ClearPropertyAction");
      string resourceString2 = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "ClearPropertyResourceTemplate");
      bool flag = false;
      PSObject psObject = new PSObject();
      foreach (string valueName in filteredCollection)
      {
        if (this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, resourceString2, (object) path, (object) valueName), resourceString1))
        {
          object obj = this.ResetRegistryKeyValue(key, valueName);
          string name = valueName;
          if (string.IsNullOrEmpty(valueName))
            name = this.GetLocalizedDefaultToken();
          psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty(name, obj));
          flag = true;
        }
      }
      key.Close();
      if (!flag)
        return;
      this.WritePropertyObject((object) psObject, path);
    }

    public object GetPropertyDynamicParameters(
      string path,
      Collection<string> providerSpecificPickList)
    {
      return (object) null;
    }

    public object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear) => (object) null;

    public void NewProperty(string path, string propertyName, string type, object value)
    {
      if (path == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
      if (!this.CheckOperationNotAllowedOnHiveContainer(path))
        return;
      IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
      if (pathWriteIfError == null)
        return;
      string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "NewPropertyAction");
      if (this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "NewPropertyResourceTemplate"), (object) path, (object) propertyName), resourceString))
      {
        RegistryValueKind kind;
        if (!this.ParseKind(type, out kind))
        {
          pathWriteIfError.Close();
          return;
        }
        try
        {
          if ((bool) this.Force || pathWriteIfError.GetValue(propertyName) == null)
          {
            this.SetRegistryValue(pathWriteIfError, propertyName, value, kind, path);
          }
          else
          {
            IOException ioException = new IOException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "PropertyAlreadyExists"));
            this.WriteError(new ErrorRecord((Exception) ioException, ioException.GetType().FullName, ErrorCategory.ResourceExists, (object) path));
            pathWriteIfError.Close();
            return;
          }
        }
        catch (ArgumentException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
        }
        catch (InvalidCastException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
        }
        catch (IOException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
        catch (UnauthorizedAccessException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
      }
      pathWriteIfError.Close();
    }

    public void RemoveProperty(string path, string propertyName)
    {
      if (path == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
      if (!this.CheckOperationNotAllowedOnHiveContainer(path))
        return;
      IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
      if (pathWriteIfError == null)
        return;
      WildcardPattern wildcardPattern = new WildcardPattern(propertyName, WildcardOptions.IgnoreCase);
      bool hadAMatch = false;
      foreach (string valueName in pathWriteIfError.GetValueNames())
      {
        if ((this.Context.SuppressWildcardExpansion || wildcardPattern.IsMatch(valueName)) && (!this.Context.SuppressWildcardExpansion || string.Equals(valueName, propertyName, StringComparison.OrdinalIgnoreCase)))
        {
          string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "RemovePropertyAction");
          if (this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "RemovePropertyResourceTemplate"), (object) path, (object) valueName), resourceString))
          {
            string propertyName1 = this.GetPropertyName(valueName);
            try
            {
              hadAMatch = true;
              pathWriteIfError.DeleteValue(propertyName1);
            }
            catch (IOException ex)
            {
              this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) propertyName1));
            }
            catch (SecurityException ex)
            {
              this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) propertyName1));
            }
            catch (UnauthorizedAccessException ex)
            {
              this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) propertyName1));
            }
          }
        }
      }
      pathWriteIfError.Close();
      this.WriteErrorIfPerfectMatchNotFound(hadAMatch, path, propertyName);
    }

    public void RenameProperty(string path, string sourceProperty, string destinationProperty)
    {
      if (path == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
      if (!this.CheckOperationNotAllowedOnHiveContainer(path))
        return;
      IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
      if (pathWriteIfError == null)
        return;
      string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "RenamePropertyAction");
      if (this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "RenamePropertyResourceTemplate"), (object) path, (object) sourceProperty, (object) destinationProperty), resourceString))
      {
        try
        {
          this.MoveProperty(pathWriteIfError, pathWriteIfError, sourceProperty, destinationProperty);
        }
        catch (IOException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) path));
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
        catch (UnauthorizedAccessException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        }
      }
      pathWriteIfError.Close();
    }

    public void CopyProperty(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty)
    {
      if (sourcePath == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (sourcePath));
      if (destinationPath == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (destinationPath));
      if (!this.CheckOperationNotAllowedOnHiveContainer(sourcePath, destinationPath))
        return;
      IRegistryWrapper pathWriteIfError1 = this.GetRegkeyForPathWriteIfError(sourcePath, false);
      if (pathWriteIfError1 == null)
        return;
      IRegistryWrapper pathWriteIfError2 = this.GetRegkeyForPathWriteIfError(destinationPath, true);
      if (pathWriteIfError2 == null)
        return;
      string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "CopyPropertyAction");
      if (this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "CopyPropertyResourceTemplate"), (object) sourcePath, (object) sourceProperty, (object) destinationPath, (object) destinationProperty), resourceString))
      {
        try
        {
          this.CopyProperty(pathWriteIfError1, pathWriteIfError2, sourceProperty, destinationProperty, true);
        }
        catch (IOException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) sourcePath));
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) sourcePath));
        }
        catch (UnauthorizedAccessException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) sourcePath));
        }
      }
      pathWriteIfError1.Close();
    }

    public void MoveProperty(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty)
    {
      if (sourcePath == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (sourcePath));
      if (destinationPath == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (destinationPath));
      if (!this.CheckOperationNotAllowedOnHiveContainer(sourcePath, destinationPath))
        return;
      IRegistryWrapper pathWriteIfError1 = this.GetRegkeyForPathWriteIfError(sourcePath, true);
      if (pathWriteIfError1 == null)
        return;
      IRegistryWrapper pathWriteIfError2 = this.GetRegkeyForPathWriteIfError(destinationPath, true);
      if (pathWriteIfError2 == null)
        return;
      string resourceString = ResourceManagerCache.GetResourceString("RegistryProviderStrings", "MovePropertyAction");
      if (this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "MovePropertyResourceTemplate"), (object) sourcePath, (object) sourceProperty, (object) destinationPath, (object) destinationProperty), resourceString))
      {
        try
        {
          this.MoveProperty(pathWriteIfError1, pathWriteIfError2, sourceProperty, destinationProperty);
        }
        catch (IOException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) sourcePath));
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) sourcePath));
        }
        catch (UnauthorizedAccessException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) sourcePath));
        }
      }
      pathWriteIfError1.Close();
      pathWriteIfError2.Close();
    }

    protected override string GetParentPath(string path, string root)
    {
      string parentPath = base.GetParentPath(path, root);
      if (!string.Equals(parentPath, root, StringComparison.OrdinalIgnoreCase))
      {
        bool flag1 = this.ItemExists(path);
        bool flag2 = false;
        if (!flag1)
          flag2 = this.ItemExists(this.MakePath(root, path));
        if (!string.IsNullOrEmpty(parentPath) && (flag1 || flag2))
        {
          do
          {
            string path1 = parentPath;
            if (flag2)
              path1 = this.MakePath(root, parentPath);
            if (!this.ItemExists(path1))
              parentPath = base.GetParentPath(parentPath, root);
            else
              break;
          }
          while (!string.IsNullOrEmpty(parentPath));
        }
      }
      return RegistryProvider.EnsureDriveIsRooted(parentPath);
    }

    protected override string GetChildName(string path) => base.GetChildName(path).Replace('\\', '/');

    private static string EnsureDriveIsRooted(string path)
    {
      string str = path;
      int num = path.IndexOf(':');
      if (num != -1 && num + 1 == path.Length)
        str = path + (object) '\\';
      RegistryProvider.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    public object NewPropertyDynamicParameters(
      string path,
      string propertyName,
      string type,
      object value)
    {
      return (object) null;
    }

    public object RemovePropertyDynamicParameters(string path, string propertyName) => (object) null;

    public object RenamePropertyDynamicParameters(
      string path,
      string sourceProperty,
      string destinationProperty)
    {
      return (object) null;
    }

    public object CopyPropertyDynamicParameters(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty)
    {
      return (object) null;
    }

    public object MovePropertyDynamicParameters(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty)
    {
      return (object) null;
    }

    private void CopyProperty(
      IRegistryWrapper sourceKey,
      IRegistryWrapper destinationKey,
      string sourceProperty,
      string destinationProperty,
      bool writeOnSuccess)
    {
      string propertyName = this.GetPropertyName(sourceProperty);
      this.GetPropertyName(destinationProperty);
      object obj = sourceKey.GetValue(sourceProperty);
      RegistryValueKind valueKind = sourceKey.GetValueKind(sourceProperty);
      destinationKey.SetValue(destinationProperty, obj, valueKind);
      if (!writeOnSuccess)
        return;
      this.WriteWrappedPropertyObject(obj, propertyName, sourceKey.Name);
    }

    private void MoveProperty(
      IRegistryWrapper sourceKey,
      IRegistryWrapper destinationKey,
      string sourceProperty,
      string destinationProperty)
    {
      string propertyName1 = this.GetPropertyName(sourceProperty);
      string propertyName2 = this.GetPropertyName(destinationProperty);
      try
      {
        bool flag = true;
        if (string.Equals(sourceKey.Name, destinationKey.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(propertyName1, propertyName2, StringComparison.OrdinalIgnoreCase))
          flag = false;
        this.CopyProperty(sourceKey, destinationKey, propertyName1, propertyName2, false);
        if (flag)
          sourceKey.DeleteValue(propertyName1);
        this.WriteWrappedPropertyObject(destinationKey.GetValue(propertyName2), destinationProperty, destinationKey.Name);
      }
      catch (IOException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) sourceKey.Name));
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) sourceKey.Name));
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) sourceKey.Name));
      }
    }

    private string NormalizePath(string path)
    {
      string path1 = path;
      if (!string.IsNullOrEmpty(path))
      {
        path1 = path.Replace('/', '\\');
        if (this.HasRelativePathTokens(path))
          path1 = this.NormalizeRelativePath(path1, (string) null);
      }
      RegistryProvider.tracer.WriteLine("result = {0}", (object) path1);
      return path1;
    }

    private bool HasRelativePathTokens(string path) => path.IndexOf("\\", StringComparison.OrdinalIgnoreCase) == 0 || path.Contains("\\.\\") || (path.Contains("\\..\\") || path.EndsWith("\\..", StringComparison.OrdinalIgnoreCase)) || (path.EndsWith("\\.", StringComparison.OrdinalIgnoreCase) || path.StartsWith("..\\", StringComparison.OrdinalIgnoreCase) || path.StartsWith(".\\", StringComparison.OrdinalIgnoreCase)) || path.StartsWith("~", StringComparison.OrdinalIgnoreCase);

    private void GetFilteredRegistryKeyProperties(
      string path,
      Collection<string> propertyNames,
      bool getAll,
      bool writeAccess,
      out IRegistryWrapper key,
      out Collection<string> filteredCollection)
    {
      bool flag = false;
      if (string.IsNullOrEmpty(path))
        throw RegistryProvider.tracer.NewArgumentException(nameof (path));
      filteredCollection = new Collection<string>();
      key = this.GetRegkeyForPathWriteIfError(path, writeAccess);
      if (key == null)
        return;
      if (propertyNames == null)
        propertyNames = new Collection<string>();
      if (propertyNames.Count == 0 && getAll)
      {
        propertyNames.Add("*");
        flag = true;
      }
      string[] strArray = new string[0];
      string[] valueNames;
      try
      {
        valueNames = key.GetValueNames();
      }
      catch (IOException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.ReadError, (object) path));
        return;
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        return;
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        return;
      }
      foreach (string propertyName in propertyNames)
      {
        WildcardPattern wildcardPattern = new WildcardPattern(propertyName, WildcardOptions.IgnoreCase);
        bool hadAMatch = false;
        foreach (string str1 in valueNames)
        {
          string str2 = str1;
          if (string.IsNullOrEmpty(str1) && !string.IsNullOrEmpty(propertyName))
            str2 = this.GetLocalizedDefaultToken();
          if (flag || !this.Context.SuppressWildcardExpansion && wildcardPattern.IsMatch(str2) || this.Context.SuppressWildcardExpansion && string.Equals(str2, propertyName, StringComparison.OrdinalIgnoreCase))
          {
            if (string.IsNullOrEmpty(str2))
              this.GetLocalizedDefaultToken();
            hadAMatch = true;
            filteredCollection.Add(str1);
          }
        }
        this.WriteErrorIfPerfectMatchNotFound(hadAMatch, path, propertyName);
      }
    }

    private void WriteErrorIfPerfectMatchNotFound(
      bool hadAMatch,
      string path,
      string requestedValueName)
    {
      if (hadAMatch || WildcardPattern.ContainsWildcardCharacters(requestedValueName))
        return;
      Exception exception = (Exception) new PSArgumentException(string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "PropertyNotAtPath"), (object) requestedValueName, (object) path), (Exception) null);
      this.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, (object) requestedValueName));
    }

    private object ResetRegistryKeyValue(IRegistryWrapper key, string valueName)
    {
      RegistryValueKind valueKind = key.GetValueKind(valueName);
      object obj = (object) null;
      switch (valueKind)
      {
        case RegistryValueKind.Unknown:
        case RegistryValueKind.Binary:
          obj = (object) new byte[0];
          break;
        case RegistryValueKind.String:
        case RegistryValueKind.ExpandString:
          obj = (object) "";
          break;
        case RegistryValueKind.DWord:
          obj = (object) 0;
          break;
        case RegistryValueKind.MultiString:
          obj = (object) new string[0];
          break;
        case RegistryValueKind.QWord:
          obj = (object) 0L;
          break;
      }
      try
      {
        key.SetValue(valueName, obj, valueKind);
      }
      catch (IOException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.WriteError, (object) valueName));
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) valueName));
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) valueName));
      }
      return obj;
    }

    private bool IsHiveContainer(string path)
    {
      bool flag = false;
      if (path == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
      if (string.IsNullOrEmpty(path) || string.Compare(path, "\\", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(path, "/", StringComparison.OrdinalIgnoreCase) == 0)
        flag = true;
      RegistryProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    private bool CheckOperationNotAllowedOnHiveContainer(string path)
    {
      if (!this.IsHiveContainer(path))
        return true;
      this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "ContainerInvalidOperationTemplate")), "InvalidContainer", ErrorCategory.InvalidArgument, (object) path));
      return false;
    }

    private bool CheckOperationNotAllowedOnHiveContainer(string sourcePath, string destinationPath)
    {
      if (this.IsHiveContainer(sourcePath))
      {
        this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "SourceContainerInvalidOperationTemplate")), "InvalidContainer", ErrorCategory.InvalidArgument, (object) sourcePath));
        return false;
      }
      if (!this.IsHiveContainer(destinationPath))
        return true;
      this.WriteError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "DestinationContainerInvalidOperationTemplate")), "InvalidContainer", ErrorCategory.InvalidArgument, (object) destinationPath));
      return false;
    }

    private IRegistryWrapper GetHiveRoot(string path)
    {
      if (string.IsNullOrEmpty(path))
        throw RegistryProvider.tracer.NewArgumentException(nameof (path));
      if (this.TransactionAvailable())
      {
        for (int index = 0; index < RegistryProvider.wellKnownHivesTx.Length; ++index)
        {
          if (string.Equals(path, RegistryProvider.hiveNames[index], StringComparison.OrdinalIgnoreCase) || string.Equals(path, RegistryProvider.hiveShortNames[index], StringComparison.OrdinalIgnoreCase))
          {
            using (this.CurrentPSTransaction)
              return (IRegistryWrapper) new TransactedRegistryWrapper(RegistryProvider.wellKnownHivesTx[index], (CmdletProvider) this);
          }
        }
      }
      else
      {
        for (int index = 0; index < RegistryProvider.wellKnownHives.Length; ++index)
        {
          if (string.Equals(path, RegistryProvider.hiveNames[index], StringComparison.OrdinalIgnoreCase) || string.Equals(path, RegistryProvider.hiveShortNames[index], StringComparison.OrdinalIgnoreCase))
            return (IRegistryWrapper) new RegistryWrapper(RegistryProvider.wellKnownHives[index]);
        }
      }
      return (IRegistryWrapper) null;
    }

    private bool CreateIntermediateKeys(string path)
    {
      bool flag = false;
      if (string.IsNullOrEmpty(path))
        throw RegistryProvider.tracer.NewArgumentException(nameof (path));
      try
      {
        path = this.NormalizePath(path);
        int length = path.IndexOf("\\", StringComparison.Ordinal);
        if (length == 0)
        {
          path = path.Substring(1);
          length = path.IndexOf("\\", StringComparison.Ordinal);
        }
        if (length == -1)
          return true;
        string path1 = path.Substring(0, length);
        string subkey = path.Substring(length + 1);
        IRegistryWrapper hiveRoot = this.GetHiveRoot(path1);
        if (subkey.Length == 0 || hiveRoot == null)
          throw RegistryProvider.tracer.NewArgumentException(nameof (path));
        (hiveRoot.CreateSubKey(subkey) ?? throw RegistryProvider.tracer.NewArgumentException(nameof (path))).Close();
        return true;
      }
      catch (ArgumentException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.OpenError, (object) path));
        return flag;
      }
      catch (IOException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.OpenError, (object) path));
        return flag;
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        return flag;
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        return flag;
      }
    }

    private IRegistryWrapper GetRegkeyForPathWriteIfError(
      string path,
      bool writeAccess)
    {
      IRegistryWrapper registryWrapper = (IRegistryWrapper) null;
      try
      {
        registryWrapper = this.GetRegkeyForPath(path, writeAccess);
        if (registryWrapper == null)
        {
          ArgumentException argumentException = new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "KeyDoesNotExist"));
          this.WriteError(new ErrorRecord((Exception) argumentException, argumentException.GetType().FullName, ErrorCategory.InvalidArgument, (object) path));
          return registryWrapper;
        }
      }
      catch (ArgumentException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.OpenError, (object) path));
        return registryWrapper;
      }
      catch (IOException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.OpenError, (object) path));
        return registryWrapper;
      }
      catch (SecurityException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        return registryWrapper;
      }
      catch (UnauthorizedAccessException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
        return registryWrapper;
      }
      return registryWrapper;
    }

    private IRegistryWrapper GetRegkeyForPath(string path, bool writeAccess)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "KeyDoesNotExist"));
      if (this.Stopping)
        return (IRegistryWrapper) null;
      RegistryProvider.tracer.WriteLine("writeAccess = {0}", (object) writeAccess);
      int length = path.IndexOf("\\", StringComparison.Ordinal);
      if (length == 0)
      {
        path = path.Substring(1);
        length = path.IndexOf("\\", StringComparison.Ordinal);
      }
      IRegistryWrapper registryWrapper1;
      if (length == -1)
      {
        registryWrapper1 = this.GetHiveRoot(path);
      }
      else
      {
        string path1 = path.Substring(0, length);
        string name = path.Substring(length + 1);
        IRegistryWrapper hiveRoot = this.GetHiveRoot(path1);
        if (name.Length == 0 || hiveRoot == null)
        {
          registryWrapper1 = hiveRoot;
        }
        else
        {
          registryWrapper1 = hiveRoot.OpenSubKey(name, writeAccess);
          if (registryWrapper1 == null)
          {
            IRegistryWrapper registryWrapper2 = hiveRoot;
            while (!string.IsNullOrEmpty(name))
            {
              bool flag = false;
              foreach (string subKeyName in registryWrapper2.GetSubKeyNames())
              {
                string str = this.NormalizePath(subKeyName);
                if (name.Equals(str, StringComparison.OrdinalIgnoreCase) || name.StartsWith(str + (object) '/', StringComparison.OrdinalIgnoreCase) || name.StartsWith(str + (object) '\\', StringComparison.OrdinalIgnoreCase))
                {
                  IRegistryWrapper registryWrapper3 = registryWrapper2.OpenSubKey(subKeyName, writeAccess);
                  registryWrapper2.Close();
                  registryWrapper2 = registryWrapper3;
                  flag = true;
                  name = !name.Equals(str, StringComparison.OrdinalIgnoreCase) ? name.Substring((str + (object) '\\').Length) : "";
                  break;
                }
              }
              if (!flag)
                return (IRegistryWrapper) null;
            }
            return registryWrapper2;
          }
        }
      }
      return registryWrapper1;
    }

    private void SetRegistryValue(
      IRegistryWrapper key,
      string propertyName,
      object value,
      RegistryValueKind kind,
      string path)
    {
      this.SetRegistryValue(key, propertyName, value, kind, path, true);
    }

    private void SetRegistryValue(
      IRegistryWrapper key,
      string propertyName,
      object value,
      RegistryValueKind kind,
      string path,
      bool writeResult)
    {
      string propertyName1 = this.GetPropertyName(propertyName);
      RegistryValueKind kind1 = RegistryValueKind.Unknown;
      if (kind == RegistryValueKind.Unknown)
        kind1 = RegistryProvider.GetValueKindForProperty(key, propertyName1);
      if (kind1 != RegistryValueKind.Unknown)
      {
        try
        {
          value = RegistryProvider.ConvertValueToKind(value, kind1);
          kind = kind1;
        }
        catch (InvalidCastException ex)
        {
          kind1 = RegistryValueKind.Unknown;
        }
      }
      if (kind1 == RegistryValueKind.Unknown)
      {
        if (kind == RegistryValueKind.Unknown)
          kind = value == null ? RegistryValueKind.String : RegistryProvider.GetValueKindFromObject(value);
        value = RegistryProvider.ConvertValueToKind(value, kind);
      }
      key.SetValue(propertyName1, value, kind);
      if (!writeResult)
        return;
      this.WriteWrappedPropertyObject(key.GetValue(propertyName1), propertyName, path);
    }

    private void WriteWrappedPropertyObject(object value, string propertyName, string path)
    {
      PSObject psObject = new PSObject();
      string name = propertyName;
      if (string.IsNullOrEmpty(propertyName))
        name = this.GetLocalizedDefaultToken();
      psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty(name, value));
      this.WritePropertyObject((object) psObject, path);
    }

    private static object ConvertValueToKind(object value, RegistryValueKind kind)
    {
      switch (kind)
      {
        case RegistryValueKind.String:
          value = value != null ? (object) (string) LanguagePrimitives.ConvertTo(value, typeof (string), (IFormatProvider) Thread.CurrentThread.CurrentCulture) : (object) "";
          break;
        case RegistryValueKind.ExpandString:
          value = value != null ? (object) (string) LanguagePrimitives.ConvertTo(value, typeof (string), (IFormatProvider) Thread.CurrentThread.CurrentCulture) : (object) "";
          break;
        case RegistryValueKind.Binary:
          value = value != null ? (object) (byte[]) LanguagePrimitives.ConvertTo(value, typeof (byte[]), (IFormatProvider) Thread.CurrentThread.CurrentCulture) : (object) new byte[0];
          break;
        case RegistryValueKind.DWord:
          value = (object) (value != null ? (int) LanguagePrimitives.ConvertTo(value, typeof (int), (IFormatProvider) Thread.CurrentThread.CurrentCulture) : 0);
          break;
        case RegistryValueKind.MultiString:
          value = value != null ? (object) (string[]) LanguagePrimitives.ConvertTo(value, typeof (string[]), (IFormatProvider) Thread.CurrentThread.CurrentCulture) : (object) new string[0];
          break;
        case RegistryValueKind.QWord:
          value = (object) (value != null ? (long) LanguagePrimitives.ConvertTo(value, typeof (long), (IFormatProvider) Thread.CurrentThread.CurrentCulture) : 0L);
          break;
      }
      return value;
    }

    private static RegistryValueKind GetValueKindFromObject(object value)
    {
      if (value == null)
        throw RegistryProvider.tracer.NewArgumentNullException(nameof (value));
      RegistryValueKind registryValueKind = RegistryValueKind.Unknown;
      Type type = value.GetType();
      if (type == typeof (byte[]))
        registryValueKind = RegistryValueKind.Binary;
      else if (type == typeof (int))
        registryValueKind = RegistryValueKind.DWord;
      if (type == typeof (string))
        registryValueKind = RegistryValueKind.String;
      if (type == typeof (string[]))
        registryValueKind = RegistryValueKind.MultiString;
      if (type == typeof (long))
        registryValueKind = RegistryValueKind.QWord;
      return registryValueKind;
    }

    private static RegistryValueKind GetValueKindForProperty(
      IRegistryWrapper key,
      string valueName)
    {
      try
      {
        return key.GetValueKind(valueName);
      }
      catch (ArgumentException ex)
      {
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
      return RegistryValueKind.Unknown;
    }

    private static object ReadExistingKeyValue(IRegistryWrapper key, string valueName)
    {
      try
      {
        return key.GetValue(valueName, (object) null, RegistryValueOptions.DoNotExpandEnvironmentNames);
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
      return (object) null;
    }

    private void WriteRegistryItemObject(IRegistryWrapper key, string path)
    {
      if (key == null)
        return;
      path = path.Replace('/', '\\');
      path = RegistryProvider.EscapeSpecialChars(path);
      PSObject psObject = PSObject.AsPSObject(key.RegistryKey);
      string[] valueNames = key.GetValueNames();
      for (int index = 0; index < valueNames.Length; ++index)
      {
        if (string.IsNullOrEmpty(valueNames[index]))
        {
          valueNames[index] = this.GetLocalizedDefaultToken();
          break;
        }
      }
      psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Property", (object) valueNames));
      this.WriteItemObject((object) psObject, path, true);
    }

    private bool ParseKind(string type, out RegistryValueKind kind)
    {
      kind = RegistryValueKind.Unknown;
      if (string.IsNullOrEmpty(type))
        return true;
      bool flag = true;
      Exception innerException = (Exception) null;
      try
      {
        kind = (RegistryValueKind) Enum.Parse(typeof (RegistryValueKind), type, true);
      }
      catch (InvalidCastException ex)
      {
        innerException = (Exception) ex;
      }
      catch (ArgumentException ex)
      {
        innerException = (Exception) ex;
      }
      if (innerException != null)
      {
        flag = false;
        Exception exception = (Exception) new ArgumentException(string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "TypeParameterBindingFailure"), (object) type, (object) typeof (RegistryValueKind).FullName), innerException);
        this.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, (object) type));
      }
      RegistryProvider.tracer.WriteLine("result = {0}", (object) kind);
      return flag;
    }

    private string GetLocalizedDefaultToken() => "(default)";

    private string GetPropertyName(string userEnteredPropertyName)
    {
      string str = userEnteredPropertyName;
      if (!string.IsNullOrEmpty(userEnteredPropertyName) && string.Compare(userEnteredPropertyName, this.GetLocalizedDefaultToken(), true, this.Host.CurrentCulture) == 0)
        str = (string) null;
      RegistryProvider.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    public void GetSecurityDescriptor(string path, AccessControlSections sections)
    {
      using (RegistryProvider.tracer.TraceMethod(path, new object[0]))
      {
        if (string.IsNullOrEmpty(path))
          throw RegistryProvider.tracer.NewArgumentNullException(nameof (path));
        if ((sections & ~AccessControlSections.All) != AccessControlSections.None)
          throw RegistryProvider.tracer.NewArgumentException(nameof (sections));
        path = this.NormalizePath(path);
        IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
        if (pathWriteIfError == null)
          return;
        ObjectSecurity accessControl;
        try
        {
          accessControl = pathWriteIfError.GetAccessControl(sections);
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
          return;
        }
        this.WriteSecurityDescriptorObject(accessControl, path);
      }
    }

    public void SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor)
    {
      using (RegistryProvider.tracer.TraceMethod(path, new object[0]))
      {
        if (string.IsNullOrEmpty(path))
          throw RegistryProvider.tracer.NewArgumentException(nameof (path));
        if (securityDescriptor == null)
          throw RegistryProvider.tracer.NewArgumentNullException(nameof (securityDescriptor));
        path = this.NormalizePath(path);
        ObjectSecurity securityDescriptor1;
        if (this.TransactionAvailable())
        {
          securityDescriptor1 = (ObjectSecurity) (securityDescriptor as TransactedRegistrySecurity);
          if (securityDescriptor1 == null)
            throw RegistryProvider.tracer.NewArgumentException(nameof (securityDescriptor));
        }
        else
        {
          securityDescriptor1 = (ObjectSecurity) (securityDescriptor as RegistrySecurity);
          if (securityDescriptor1 == null)
            throw RegistryProvider.tracer.NewArgumentException(nameof (securityDescriptor));
        }
        IRegistryWrapper pathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
        if (pathWriteIfError == null)
          return;
        try
        {
          pathWriteIfError.SetAccessControl(securityDescriptor1);
        }
        catch (SecurityException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
          return;
        }
        catch (UnauthorizedAccessException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.PermissionDenied, (object) path));
          return;
        }
        this.WriteSecurityDescriptorObject(securityDescriptor1, path);
      }
    }

    public ObjectSecurity NewSecurityDescriptorFromPath(
      string path,
      AccessControlSections sections)
    {
      using (RegistryProvider.tracer.TraceMethod())
        return this.TransactionAvailable() ? (ObjectSecurity) new TransactedRegistrySecurity() : (ObjectSecurity) new RegistrySecurity();
    }

    public ObjectSecurity NewSecurityDescriptorOfType(
      string type,
      AccessControlSections sections)
    {
      using (RegistryProvider.tracer.TraceMethod())
        return this.TransactionAvailable() ? (ObjectSecurity) new TransactedRegistrySecurity() : (ObjectSecurity) new RegistrySecurity();
    }
  }
}
