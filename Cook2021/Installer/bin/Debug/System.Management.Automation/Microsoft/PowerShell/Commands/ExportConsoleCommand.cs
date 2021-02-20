// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ExportConsoleCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Export", "Console", SupportsShouldProcess = true)]
  public sealed class ExportConsoleCommand : ConsoleCmdletsBase
  {
    private string fileName;
    private bool force;
    private bool noclobber;

    [Parameter(Mandatory = false, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [Alias(new string[] {"PSPath"})]
    public string Path
    {
      get
      {
        using (ConsoleCmdletsBase.tracer.TraceProperty())
          return this.fileName;
      }
      set
      {
        using (ConsoleCmdletsBase.tracer.TraceProperty())
          this.fileName = value;
      }
    }

    [Parameter]
    public SwitchParameter Force
    {
      get
      {
        using (ConsoleCmdletsBase.tracer.TraceProperty())
          return (SwitchParameter) this.force;
      }
      set
      {
        using (ConsoleCmdletsBase.tracer.TraceProperty())
          this.force = (bool) value;
      }
    }

    [Parameter]
    public SwitchParameter NoClobber
    {
      get
      {
        using (ConsoleCmdletsBase.tracer.TraceProperty())
          return (SwitchParameter) this.noclobber;
      }
      set
      {
        using (ConsoleCmdletsBase.tracer.TraceProperty())
          this.noclobber = (bool) value;
      }
    }

    protected override void ProcessRecord()
    {
      using (ConsoleCmdletsBase.tracer.TraceMethod())
      {
        string str1 = this.GetFileName();
        if (string.IsNullOrEmpty(str1))
          str1 = this.PromptUserForFile();
        if (string.IsNullOrEmpty(str1))
        {
          PSArgumentException argumentException = ConsoleCmdletsBase.tracer.NewArgumentException("file", "ConsoleInfoErrorStrings", "FileNameNotResolved");
          this.ThrowError((object) str1, "FileNameNotResolved", (Exception) argumentException, ErrorCategory.InvalidArgument);
        }
        if (WildcardPattern.ContainsWildcardCharacters(str1))
          this.ThrowError((object) str1, "WildCardNotSupported", (Exception) ConsoleCmdletsBase.tracer.NewInvalidOperationException("ConsoleInfoErrorStrings", "ConsoleFileWildCardsNotSupported", (object) str1), ErrorCategory.InvalidOperation);
        string str2 = this.ResolveProviderAndPath(str1);
        if (string.IsNullOrEmpty(str2))
          return;
        if (!str2.EndsWith(".psc1", StringComparison.OrdinalIgnoreCase))
          str2 += ".psc1";
        if (!this.ShouldProcess(this.Path))
          return;
        if (File.Exists(str2))
        {
          if ((bool) this.NoClobber)
            this.ThrowTerminatingError(new ErrorRecord((Exception) new UnauthorizedAccessException(ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "FileExistsNoClobber", (object) str2, (object) "NoClobber")), "NoClobber", ErrorCategory.ResourceExists, (object) str2));
          if ((File.GetAttributes(str2) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
          {
            if ((bool) this.Force)
              this.RemoveFileThrowIfError(str2);
            else
              this.ThrowError((object) str1, "ConsoleFileReadOnly", (Exception) ConsoleCmdletsBase.tracer.NewArgumentException(str1, "ConsoleInfoErrorStrings", "ConsoleFileReadOnly", (object) str2), ErrorCategory.InvalidArgument);
          }
        }
        try
        {
          this.Runspace.SaveAsConsoleFile(str2);
        }
        catch (PSArgumentException ex)
        {
          this.ThrowError((object) str2, "PathNotAbsolute", (Exception) ex, ErrorCategory.InvalidArgument);
        }
        catch (PSArgumentNullException ex)
        {
          this.ThrowError((object) str2, "PathNull", (Exception) ex, ErrorCategory.InvalidArgument);
        }
        catch (ArgumentException ex)
        {
          this.ThrowError((object) str2, "InvalidCharacetersInPath", (Exception) ex, ErrorCategory.InvalidArgument);
        }
        Exception innerException = (Exception) null;
        try
        {
          this.Context.EngineSessionState.SetConsoleVariable();
        }
        catch (ArgumentNullException ex)
        {
          innerException = (Exception) ex;
        }
        catch (ArgumentOutOfRangeException ex)
        {
          innerException = (Exception) ex;
        }
        catch (ArgumentException ex)
        {
          innerException = (Exception) ex;
        }
        catch (SessionStateUnauthorizedAccessException ex)
        {
          innerException = (Exception) ex;
        }
        catch (SessionStateOverflowException ex)
        {
          innerException = (Exception) ex;
        }
        catch (ProviderNotFoundException ex)
        {
          innerException = (Exception) ex;
        }
        catch (System.Management.Automation.DriveNotFoundException ex)
        {
          innerException = (Exception) ex;
        }
        catch (NotSupportedException ex)
        {
          innerException = (Exception) ex;
        }
        catch (ProviderInvocationException ex)
        {
          innerException = (Exception) ex;
        }
        if (innerException != null)
          throw ConsoleCmdletsBase.tracer.NewInvalidOperationException(innerException, "ConsoleInfoErrorStrings", "ConsoleVariableCannotBeSet", (object) str2);
      }
    }

    private void RemoveFileThrowIfError(string destination)
    {
      FileInfo fileInfo = new FileInfo(destination);
      if (fileInfo == null)
        return;
      Exception innerException = (Exception) null;
      try
      {
        fileInfo.Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden);
        fileInfo.Delete();
      }
      catch (FileNotFoundException ex)
      {
        innerException = (Exception) ex;
      }
      catch (DirectoryNotFoundException ex)
      {
        innerException = (Exception) ex;
      }
      catch (UnauthorizedAccessException ex)
      {
        innerException = (Exception) ex;
      }
      catch (SecurityException ex)
      {
        innerException = (Exception) ex;
      }
      catch (ArgumentNullException ex)
      {
        innerException = (Exception) ex;
      }
      catch (ArgumentException ex)
      {
        innerException = (Exception) ex;
      }
      catch (PathTooLongException ex)
      {
        innerException = (Exception) ex;
      }
      catch (NotSupportedException ex)
      {
        innerException = (Exception) ex;
      }
      catch (IOException ex)
      {
        innerException = (Exception) ex;
      }
      if (innerException != null)
        throw ConsoleCmdletsBase.tracer.NewInvalidOperationException(innerException, "ConsoleInfoErrorStrings", "ExportConsoleCannotDeleteFile", (object) fileInfo);
    }

    private string ResolveProviderAndPath(string path)
    {
      CmdletProviderContext currentCommandContext = new CmdletProviderContext((Cmdlet) this);
      PathInfo pathInfo = this.ResolvePath(path, true, currentCommandContext);
      if (pathInfo == null)
        return (string) null;
      if (pathInfo.Provider.ImplementingType == typeof (FileSystemProvider))
        return pathInfo.Path;
      throw ConsoleCmdletsBase.tracer.NewInvalidOperationException("ConsoleInfoErrorStrings", "ProviderNotSupported", (object) pathInfo.Provider.Name);
    }

    private PathInfo ResolvePath(
      string pathToResolve,
      bool allowNonexistingPaths,
      CmdletProviderContext currentCommandContext)
    {
      using (ConsoleCmdletsBase.tracer.TraceMethod())
      {
        Collection<PathInfo> collection = new Collection<PathInfo>();
        try
        {
          foreach (PathInfo pathInfo in this.SessionState.Path.GetResolvedPSPathFromPSPath(pathToResolve, currentCommandContext))
            collection.Add(pathInfo);
        }
        catch (PSNotSupportedException ex)
        {
          this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
        }
        catch (System.Management.Automation.DriveNotFoundException ex)
        {
          this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
        }
        catch (ProviderNotFoundException ex)
        {
          this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
        }
        catch (ItemNotFoundException ex)
        {
          if (allowNonexistingPaths)
          {
            ProviderInfo provider = (ProviderInfo) null;
            PSDriveInfo drive = (PSDriveInfo) null;
            string providerPathFromPsPath = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(pathToResolve, currentCommandContext, out provider, out drive);
            PathInfo pathInfo = new PathInfo(drive, provider, providerPathFromPsPath, this.SessionState);
            collection.Add(pathInfo);
          }
          else
            this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
        }
        if (collection.Count == 1)
          return collection[0];
        if (collection.Count <= 1)
          return (PathInfo) null;
        this.WriteError(new ErrorRecord((Exception) ConsoleCmdletsBase.tracer.NewNotSupportedException(), "NotSupported", ErrorCategory.NotImplemented, (object) collection));
        return (PathInfo) null;
      }
    }

    private string GetFileName()
    {
      if (!string.IsNullOrEmpty(this.fileName))
        return this.fileName;
      PSVariable psVariable = this.Context.SessionState.PSVariable.Get("ConsoleFileName");
      if (psVariable == null)
        return string.Empty;
      if (!(psVariable.Value is string baseObject) && psVariable.Value is PSObject psObject && psObject.BaseObject is string)
        baseObject = psObject.BaseObject as string;
      return baseObject != null ? baseObject : throw ConsoleCmdletsBase.tracer.NewArgumentException("fileName", "ConsoleInfoErrorStrings", "ConsoleCannotbeConvertedToString");
    }

    private string PromptUserForFile()
    {
      if (!this.ShouldContinue(ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "PromptForExportConsole"), (string) null))
        return string.Empty;
      Dictionary<string, PSObject> dictionary = this.PSHostInternal.UI.Prompt(ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "FileNameCaptionForExportConsole", (object) "export-console"), ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "FileNamePromptMessage"), new Collection<FieldDescription>()
      {
        new FieldDescription("Name")
      });
      return dictionary != null && dictionary["Name"] != null ? dictionary["Name"].BaseObject as string : string.Empty;
    }
  }
}
