// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.PSSnapInCommandBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  public abstract class PSSnapInCommandBase : PSCmdlet, IDisposable
  {
    internal const string resBaseName = "MshSnapInCmdletResources";
    [TraceSource("PSSnapInCmdlet", "PSSnapIn related cmdlet")]
    internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer("PSSnapInCmdlet", "PSSnapIn related cmdlet");
    private bool _disposed;
    private bool _shouldGetAll;
    private RegistryStringResourceIndirect resourceReader;

    public void Dispose()
    {
      using (PSSnapInCommandBase.tracer.TraceDispose((object) this))
      {
        if (!this._disposed)
        {
          if (this.resourceReader != null)
          {
            this.resourceReader.Dispose();
            this.resourceReader = (RegistryStringResourceIndirect) null;
          }
          GC.SuppressFinalize((object) this);
        }
        this._disposed = true;
      }
    }

    protected override void EndProcessing()
    {
      if (this.resourceReader == null)
        return;
      this.resourceReader.Dispose();
      this.resourceReader = (RegistryStringResourceIndirect) null;
    }

    internal RunspaceConfigForSingleShell Runspace => !(this.Context.RunspaceConfiguration is RunspaceConfigForSingleShell runspaceConfiguration) ? (RunspaceConfigForSingleShell) null : runspaceConfiguration;

    internal void WriteNonTerminatingError(
      object targetObject,
      string errorId,
      Exception innerException,
      ErrorCategory category)
    {
      this.WriteError(new ErrorRecord(innerException, errorId, category, targetObject));
    }

    internal Collection<string> SearchListForPattern(
      Collection<PSSnapInInfo> searchList,
      string pattern)
    {
      Collection<string> collection = new Collection<string>();
      if (searchList == null)
        return collection;
      WildcardPattern wildcardPattern = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
      foreach (PSSnapInInfo search in searchList)
      {
        if (wildcardPattern.IsMatch(search.Name))
          collection.Add(search.Name);
      }
      return collection;
    }

    internal static PSSnapInInfo IsSnapInLoaded(
      Collection<PSSnapInInfo> loadedSnapins,
      PSSnapInInfo psSnapInInfo)
    {
      if (loadedSnapins == null)
        return (PSSnapInInfo) null;
      foreach (PSSnapInInfo loadedSnapin in loadedSnapins)
      {
        string assemblyName = loadedSnapin.AssemblyName;
        if (!string.IsNullOrEmpty(assemblyName) && string.Equals(assemblyName, psSnapInInfo.AssemblyName, StringComparison.OrdinalIgnoreCase))
          return loadedSnapin;
      }
      return (PSSnapInInfo) null;
    }

    protected internal Collection<PSSnapInInfo> GetSnapIns(string pattern)
    {
      if (this.Runspace != null)
        return pattern != null ? this.Runspace.ConsoleInfo.GetPSSnapIn(pattern, this._shouldGetAll) : this.Runspace.ConsoleInfo.PSSnapIns;
      WildcardPattern wildcardPattern = (WildcardPattern) null;
      if (!string.IsNullOrEmpty(pattern))
      {
        if (!WildcardPattern.ContainsWildcardCharacters(pattern))
          PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(pattern);
        wildcardPattern = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
      }
      Collection<PSSnapInInfo> collection = new Collection<PSSnapInInfo>();
      if (this._shouldGetAll)
      {
        foreach (PSSnapInInfo psSnapInInfo in PSSnapInReader.ReadAll())
        {
          if (wildcardPattern == null || wildcardPattern.IsMatch(psSnapInInfo.Name))
            collection.Add(psSnapInInfo);
        }
      }
      else
      {
        List<CmdletInfo> cmdlets = this.InvokeCommand.GetCmdlets();
        Dictionary<PSSnapInInfo, bool> dictionary = new Dictionary<PSSnapInInfo, bool>();
        foreach (CmdletInfo cmdletInfo in cmdlets)
        {
          PSSnapInInfo psSnapIn = cmdletInfo.PSSnapIn;
          if (psSnapIn != null && !dictionary.ContainsKey(psSnapIn))
            dictionary.Add(psSnapIn, true);
        }
        foreach (PSSnapInInfo key in dictionary.Keys)
        {
          if (wildcardPattern == null || wildcardPattern.IsMatch(key.Name))
            collection.Add(key);
        }
      }
      return collection;
    }

    protected internal bool ShouldGetAll
    {
      get => this._shouldGetAll;
      set => this._shouldGetAll = value;
    }

    internal RegistryStringResourceIndirect ResourceReader
    {
      get
      {
        if (this.resourceReader == null)
          this.resourceReader = RegistryStringResourceIndirect.GetResourceIndirectReader();
        return this.resourceReader;
      }
    }
  }
}
