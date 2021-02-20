// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.NewModuleManifestCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("New", "ModuleManifest", SupportsShouldProcess = true)]
  [OutputType(new Type[] {typeof (string)})]
  public sealed class NewModuleManifestCommand : PSCmdlet
  {
    private string _path;
    private string[] _nestedModules = new string[0];
    private Guid _guid = Guid.NewGuid();
    private string _author;
    private string _companyName = "";
    private string _copyright;
    private string _moduleToProcess;
    private Version _moduleVersion = new Version(1, 0);
    private string _description;
    private ProcessorArchitecture? _processorArchitecture = new ProcessorArchitecture?();
    private Version _powerShellVersion;
    private Version _ClrVersion;
    private Version _DotNetFrameworkVersion;
    private string _PowerShellHostName;
    private Version _PowerShellHostVersion;
    private object[] _requiredModules;
    private string[] _types = new string[0];
    private string[] _formats = new string[0];
    private string[] _scripts = new string[0];
    private string[] _requiredAssemblies = new string[0];
    private string[] _miscFiles = new string[0];
    private object[] _moduleList = (object[]) new string[0];
    private string[] _exportedFunctions = new string[1]
    {
      "*"
    };
    private string[] _exportedAliases = new string[1]{ "*" };
    private string[] _exportedVariables = new string[1]
    {
      "*"
    };
    private string[] _exportedCmdlets = new string[1]{ "*" };
    private object _privateData;
    private bool _passThru;

    [Parameter(Mandatory = true, Position = 0)]
    public string Path
    {
      get => this._path;
      set => this._path = value;
    }

    [Parameter(Mandatory = true)]
    [AllowEmptyCollection]
    public string[] NestedModules
    {
      get => this._nestedModules;
      set => this._nestedModules = value;
    }

    [Parameter]
    public Guid Guid
    {
      get => this._guid;
      set => this._guid = value;
    }

    [Parameter(Mandatory = true)]
    [AllowEmptyString]
    public string Author
    {
      get => this._author;
      set => this._author = value;
    }

    [AllowEmptyString]
    [Parameter(Mandatory = true)]
    public string CompanyName
    {
      get => this._companyName;
      set => this._companyName = value;
    }

    [Parameter(Mandatory = true)]
    [AllowEmptyString]
    public string Copyright
    {
      get => this._copyright;
      set => this._copyright = value;
    }

    [Parameter(Mandatory = true)]
    [AllowEmptyString]
    public string ModuleToProcess
    {
      get => this._moduleToProcess;
      set => this._moduleToProcess = value;
    }

    [ValidateNotNull]
    [Parameter]
    public Version ModuleVersion
    {
      get => this._moduleVersion;
      set => this._moduleVersion = value;
    }

    [Parameter(Mandatory = true)]
    [AllowEmptyString]
    public string Description
    {
      get => this._description;
      set => this._description = value;
    }

    [Parameter]
    public ProcessorArchitecture ProcessorArchitecture
    {
      get => !this._processorArchitecture.HasValue ? ProcessorArchitecture.None : this._processorArchitecture.Value;
      set => this._processorArchitecture = new ProcessorArchitecture?(value);
    }

    [Parameter]
    public Version PowerShellVersion
    {
      get => this._powerShellVersion;
      set => this._powerShellVersion = value;
    }

    [Parameter]
    public Version ClrVersion
    {
      get => this._ClrVersion;
      set => this._ClrVersion = value;
    }

    [Parameter]
    public Version DotNetFrameworkVersion
    {
      get => this._DotNetFrameworkVersion;
      set => this._DotNetFrameworkVersion = value;
    }

    [Parameter]
    public string PowerShellHostName
    {
      get => this._PowerShellHostName;
      set => this._PowerShellHostName = value;
    }

    [Parameter]
    public Version PowerShellHostVersion
    {
      get => this._PowerShellHostVersion;
      set => this._PowerShellHostVersion = value;
    }

    [Parameter]
    [ArgumentTypeConverter(new Type[] {typeof (ModuleSpecification[])})]
    public object[] RequiredModules
    {
      get => this._requiredModules;
      set => this._requiredModules = value;
    }

    [AllowEmptyCollection]
    [Parameter(Mandatory = true)]
    public string[] TypesToProcess
    {
      get => this._types;
      set => this._types = value;
    }

    [AllowEmptyCollection]
    [Parameter(Mandatory = true)]
    public string[] FormatsToProcess
    {
      get => this._formats;
      set => this._formats = value;
    }

    [AllowEmptyCollection]
    [Parameter]
    public string[] ScriptsToProcess
    {
      get => this._scripts;
      set => this._scripts = value;
    }

    [Parameter(Mandatory = true)]
    [AllowEmptyCollection]
    public string[] RequiredAssemblies
    {
      get => this._requiredAssemblies;
      set => this._requiredAssemblies = value;
    }

    [AllowEmptyCollection]
    [Parameter(Mandatory = true)]
    public string[] FileList
    {
      get => this._miscFiles;
      set => this._miscFiles = value;
    }

    [AllowEmptyCollection]
    [ArgumentTypeConverter(new Type[] {typeof (ModuleSpecification[])})]
    [Parameter]
    public object[] ModuleList
    {
      get => this._moduleList;
      set => this._moduleList = value;
    }

    [Parameter]
    [AllowEmptyCollection]
    public string[] FunctionsToExport
    {
      get => this._exportedFunctions;
      set => this._exportedFunctions = value;
    }

    [AllowEmptyCollection]
    [Parameter]
    public string[] AliasesToExport
    {
      get => this._exportedAliases;
      set => this._exportedAliases = value;
    }

    [Parameter]
    [AllowEmptyCollection]
    public string[] VariablesToExport
    {
      get => this._exportedVariables;
      set => this._exportedVariables = value;
    }

    [AllowEmptyCollection]
    [Parameter]
    public string[] CmdletsToExport
    {
      get => this._exportedCmdlets;
      set => this._exportedCmdlets = value;
    }

    [Parameter(Mandatory = false)]
    [AllowNull]
    public object PrivateData
    {
      get => this._privateData;
      set => this._privateData = value;
    }

    [Parameter]
    public SwitchParameter PassThru
    {
      get => (SwitchParameter) this._passThru;
      set => this._passThru = (bool) value;
    }

    private string quoteName(object name) => name == null ? "''" : "'" + name.ToString().Replace("'", "''") + "'";

    private string quoteNames(IEnumerable names, StreamWriter streamWriter)
    {
      if (names == null)
        return "@()";
      StringBuilder stringBuilder = new StringBuilder();
      int num = 15;
      bool flag = true;
      foreach (string name in names)
      {
        if (!string.IsNullOrEmpty(name))
        {
          if (flag)
            flag = false;
          else
            stringBuilder.Append(", ");
          string str = this.quoteName((object) name);
          num += str.Length;
          if (num > 80)
          {
            stringBuilder.Append(streamWriter.NewLine);
            stringBuilder.Append("               ");
            num = 15 + str.Length;
          }
          stringBuilder.Append(str);
        }
      }
      return stringBuilder.Length == 0 ? "@()" : stringBuilder.ToString();
    }

    private string quoteModules(IEnumerable moduleSpecs, StreamWriter streamWriter)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("@(");
      if (moduleSpecs != null)
      {
        bool flag = true;
        foreach (object moduleSpec in moduleSpecs)
        {
          if (moduleSpec != null)
          {
            ModuleSpecification moduleSpecification = (ModuleSpecification) LanguagePrimitives.ConvertTo(moduleSpec, typeof (ModuleSpecification), (IFormatProvider) CultureInfo.InvariantCulture);
            if (!flag)
            {
              stringBuilder.Append(", ");
              stringBuilder.Append(streamWriter.NewLine);
              stringBuilder.Append("               ");
            }
            flag = false;
            if (!moduleSpecification.Guid.HasValue && moduleSpecification.Version == (Version) null)
            {
              stringBuilder.Append(this.quoteName((object) moduleSpecification.Name));
            }
            else
            {
              stringBuilder.Append("@{");
              stringBuilder.Append("ModuleName = ");
              stringBuilder.Append(this.quoteName((object) moduleSpecification.Name));
              stringBuilder.Append("; ");
              if (moduleSpecification.Guid.HasValue)
              {
                stringBuilder.Append("GUID = ");
                stringBuilder.Append(this.quoteName((object) moduleSpecification.Guid.ToString()));
                stringBuilder.Append("; ");
              }
              if (moduleSpecification.Version != (Version) null)
              {
                stringBuilder.Append("ModuleVersion = ");
                stringBuilder.Append(this.quoteName((object) moduleSpecification.Version.ToString()));
                stringBuilder.Append("; ");
              }
              stringBuilder.Append("}");
            }
          }
        }
      }
      stringBuilder.Append(")");
      return stringBuilder.ToString();
    }

    private string quoteFiles(IEnumerable names, StreamWriter streamWriter)
    {
      List<string> stringList = new List<string>();
      if (names != null)
      {
        foreach (string name in names)
        {
          if (!string.IsNullOrEmpty(name))
          {
            foreach (string str in this.TryResolveFilePath(name))
              stringList.Add(str);
          }
        }
      }
      return this.quoteNames((IEnumerable) stringList, streamWriter);
    }

    private List<string> TryResolveFilePath(string filePath)
    {
      List<string> stringList = new List<string>();
      ProviderInfo provider = (ProviderInfo) null;
      SessionState sessionState = this.Context.SessionState;
      try
      {
        Collection<string> providerPathFromPsPath = sessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out provider);
        if (!provider.NameEquals(this.Context.ProviderNames.FileSystem) || providerPathFromPsPath == null || providerPathFromPsPath.Count < 1)
        {
          stringList.Add(filePath);
          return stringList;
        }
        foreach (string path in providerPathFromPsPath)
        {
          string str = this.SessionState.Path.NormalizeRelativePath(path, this.SessionState.Path.CurrentLocation.ProviderPath);
          if (str.StartsWith(".\\", StringComparison.OrdinalIgnoreCase) || str.StartsWith("./", StringComparison.OrdinalIgnoreCase))
            str = str.Substring(2);
          stringList.Add(str);
        }
      }
      catch (ItemNotFoundException ex)
      {
        stringList.Add(filePath);
      }
      return stringList;
    }

    private string ManifestFragment(string key, string value, StreamWriter streamWriter)
    {
      string str = ResourceManagerCache.FormatResourceString("Modules", key);
      string newLine = streamWriter.NewLine;
      return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "# {0}{1}{2:19} = {3}{4}{5}", (object) str, (object) newLine, (object) key, (object) value, (object) newLine, (object) newLine);
    }

    private string ManifestComment(string insert, StreamWriter streamWriter)
    {
      if (!string.IsNullOrEmpty(insert))
        insert = " " + insert;
      return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "#{0}{1}", (object) insert, (object) streamWriter.NewLine);
    }

    protected override void ProcessRecord()
    {
      ProviderInfo provider = (ProviderInfo) null;
      string providerPathFromPsPath = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(this._path, out provider, out PSDriveInfo _);
      if (!provider.NameEquals(this.Context.ProviderNames.FileSystem) || !providerPathFromPsPath.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
        this.ThrowTerminatingError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("Modules", "InvalidModuleManifestPath", (object) this._path)), "Modules_InvalidModuleManifestPath", ErrorCategory.InvalidArgument, (object) this._path));
      string action = ResourceManagerCache.FormatResourceString("Modules", "CreatingModuleManifestFile", (object) providerPathFromPsPath);
      if (!this.ShouldProcess(providerPathFromPsPath, action))
        return;
      if (string.IsNullOrEmpty(this._author))
        this._author = Environment.UserName;
      if (string.IsNullOrEmpty(this._companyName))
        this._companyName = ResourceManagerCache.GetResourceString("Modules", "DefaultCompanyName");
      if (string.IsNullOrEmpty(this._copyright))
        this._copyright = ResourceManagerCache.FormatResourceString("Modules", "DefaultCopyrightMessage", (object) DateTime.Now.Year, (object) this._author);
      StreamWriter streamWriter;
      PathUtils.MasterStreamOpen((PSCmdlet) this, providerPathFromPsPath, "unicode", false, false, false, false, out FileStream _, out streamWriter, out FileInfo _);
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.ManifestComment("", streamWriter));
      stringBuilder.Append(this.ManifestComment(ResourceManagerCache.FormatResourceString("Modules", "ManifestHeaderLine1", (object) System.IO.Path.GetFileNameWithoutExtension(providerPathFromPsPath)), streamWriter));
      stringBuilder.Append(this.ManifestComment("", streamWriter));
      stringBuilder.Append(this.ManifestComment(ResourceManagerCache.FormatResourceString("Modules", "ManifestHeaderLine2", (object) this._author), streamWriter));
      stringBuilder.Append(this.ManifestComment("", streamWriter));
      stringBuilder.Append(this.ManifestComment(ResourceManagerCache.FormatResourceString("Modules", "ManifestHeaderLine3", (object) DateTime.Now.ToShortDateString()), streamWriter));
      stringBuilder.Append(this.ManifestComment("", streamWriter));
      stringBuilder.Append(streamWriter.NewLine);
      stringBuilder.Append("@{");
      stringBuilder.Append(streamWriter.NewLine);
      stringBuilder.Append(streamWriter.NewLine);
      if (this._moduleToProcess == null)
        this._moduleToProcess = string.Empty;
      stringBuilder.Append(this.ManifestFragment("ModuleToProcess", this.quoteName((object) this._moduleToProcess), streamWriter));
      stringBuilder.Append(this.ManifestFragment("ModuleVersion", this.quoteName((object) this._moduleVersion.ToString()), streamWriter));
      stringBuilder.Append(this.ManifestFragment("GUID", this.quoteName((object) this._guid.ToString()), streamWriter));
      stringBuilder.Append(this.ManifestFragment("Author", this.quoteName((object) this._author), streamWriter));
      stringBuilder.Append(this.ManifestFragment("CompanyName", this.quoteName((object) this._companyName), streamWriter));
      stringBuilder.Append(this.ManifestFragment("Copyright", this.quoteName((object) this._copyright), streamWriter));
      stringBuilder.Append(this.ManifestFragment("Description", this.quoteName((object) this._description), streamWriter));
      stringBuilder.Append(this.ManifestFragment("PowerShellVersion", this.quoteName((object) this._powerShellVersion), streamWriter));
      stringBuilder.Append(this.ManifestFragment("PowerShellHostName", this.quoteName((object) this._PowerShellHostName), streamWriter));
      stringBuilder.Append(this.ManifestFragment("PowerShellHostVersion", this.quoteName((object) this._PowerShellHostVersion), streamWriter));
      stringBuilder.Append(this.ManifestFragment("DotNetFrameworkVersion", this.quoteName((object) this._DotNetFrameworkVersion), streamWriter));
      stringBuilder.Append(this.ManifestFragment("CLRVersion", this.quoteName((object) this._ClrVersion), streamWriter));
      stringBuilder.Append(this.ManifestFragment("ProcessorArchitecture", this.quoteName((object) this._processorArchitecture), streamWriter));
      stringBuilder.Append(this.ManifestFragment("RequiredModules", this.quoteModules((IEnumerable) this._requiredModules, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("RequiredAssemblies", this.quoteFiles((IEnumerable) this._requiredAssemblies, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("ScriptsToProcess", this.quoteFiles((IEnumerable) this._scripts, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("TypesToProcess", this.quoteFiles((IEnumerable) this._types, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("FormatsToProcess", this.quoteFiles((IEnumerable) this._formats, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("NestedModules", this.quoteFiles((IEnumerable) this._nestedModules, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("FunctionsToExport", this.quoteNames((IEnumerable) this._exportedFunctions, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("CmdletsToExport", this.quoteNames((IEnumerable) this._exportedCmdlets, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("VariablesToExport", this.quoteNames((IEnumerable) this._exportedVariables, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("AliasesToExport", this.quoteNames((IEnumerable) this._exportedAliases, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("ModuleList", this.quoteModules((IEnumerable) this._moduleList, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("FileList", this.quoteFiles((IEnumerable) this._miscFiles, streamWriter), streamWriter));
      stringBuilder.Append(this.ManifestFragment("PrivateData", this.quoteName((object) (string) LanguagePrimitives.ConvertTo(this._privateData, typeof (string), (IFormatProvider) CultureInfo.InvariantCulture)), streamWriter));
      stringBuilder.Append("}");
      stringBuilder.Append(streamWriter.NewLine);
      stringBuilder.Append(streamWriter.NewLine);
      string str = stringBuilder.ToString();
      if (this._passThru)
        this.WriteObject((object) str);
      streamWriter.Write(str);
      streamWriter.Close();
    }
  }
}
