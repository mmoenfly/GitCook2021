// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HelpProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace System.Management.Automation
{
  internal abstract class HelpProvider
  {
    private HelpSystem _helpSystem;
    [TraceSource("HelpProvider", "HelpProvider")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (HelpProvider), nameof (HelpProvider));

    internal HelpProvider(HelpSystem helpSystem)
    {
      using (HelpProvider.tracer.TraceConstructor((object) this))
        this._helpSystem = helpSystem;
    }

    internal HelpSystem HelpSystem => this._helpSystem;

    internal abstract string Name { get; }

    internal abstract HelpCategory HelpCategory { get; }

    internal abstract IEnumerable<HelpInfo> ExactMatchHelp(
      HelpRequest helpRequest);

    internal abstract IEnumerable<HelpInfo> SearchHelp(
      HelpRequest helpRequest,
      bool searchOnlyContent);

    internal virtual IEnumerable<HelpInfo> ProcessForwardedHelp(
      HelpInfo helpInfo,
      HelpRequest helpRequest)
    {
      using (HelpProvider.tracer.TraceMethod())
        yield return helpInfo;
    }

    internal virtual void Reset()
    {
      using (HelpProvider.tracer.TraceMethod())
        ;
    }

    internal void ReportHelpFileError(Exception exception, string target, string helpFile) => this.HelpSystem.LastErrors.Add(new ErrorRecord(exception, "LoadHelpFileForTargetFailed", ErrorCategory.OpenError, (object) null)
    {
      ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "LoadHelpFileForTargetFailed", new object[3]
      {
        (object) target,
        (object) helpFile,
        (object) exception.Message
      })
    });

    internal string GetDefaultShellSearchPath()
    {
      string shellId = this.HelpSystem.ExecutionContext.ShellID;
      string pathFromRegistry = CommandDiscovery.GetShellPathFromRegistry(shellId);
      string directoryName;
      if (pathFromRegistry == null)
      {
        directoryName = Path.GetDirectoryName(PsUtils.GetMainModule(Process.GetCurrentProcess()).FileName);
        this.HelpSystem.LastErrors.Add(new ErrorRecord((Exception) HelpProvider.tracer.NewArgumentException("DefaultSearchPath", "HelpErrors", "RegistryPathNotFound", (object) Utils.GetRegistryConfigurationPath(shellId), (object) "\\Path", (object) directoryName), "LoadHelpFileForTargetFailed", ErrorCategory.OpenError, (object) null));
      }
      else
      {
        directoryName = Path.GetDirectoryName(pathFromRegistry);
        if (!Directory.Exists(directoryName))
        {
          directoryName = Path.GetDirectoryName(PsUtils.GetMainModule(Process.GetCurrentProcess()).FileName);
          this.HelpSystem.LastErrors.Add(new ErrorRecord((Exception) HelpProvider.tracer.NewArgumentException("DefaultSearchPath", "HelpErrors", "RegistryPathNotFound", (object) Utils.GetRegistryConfigurationPath(shellId), (object) "\\Path", (object) "\\Path", (object) directoryName), "LoadHelpFileForTargetFailed", ErrorCategory.OpenError, (object) null));
        }
      }
      return directoryName;
    }

    internal bool AreSnapInsSupported() => this._helpSystem.ExecutionContext.RunspaceConfiguration is RunspaceConfigForSingleShell;

    internal Collection<string> GetSearchPaths()
    {
      Collection<string> searchPaths = this.HelpSystem.GetSearchPaths();
      searchPaths.Add(this.GetDefaultShellSearchPath());
      return searchPaths;
    }
  }
}
